using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Scrapshift.Editor;
namespace Scrapshift.Tests
{
    public sealed class GameplayParityTests
    {
        RunRules Run()
        {
            var run=new RunRules(77);
            run.Configure(AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset"));
            run.Start();return run;
        }
        [Test] public void FirstXpThresholdIsSixteenAndEveryLevelAddsOneMaxHp()
        {
            var r=Run();float hp=r.MaxHealth;
            Assert.AreEqual(0,r.Level);Assert.AreEqual(16,r.NextLevel);
            r.Collect(15);Assert.AreEqual(0,r.PendingLevels);
            r.Collect(1);Assert.AreEqual(1,r.Level);Assert.AreEqual(0,r.XP);
            Assert.AreEqual(25,r.NextLevel);Assert.AreEqual(hp+1,r.MaxHealth);
            r.Collect(25);Assert.AreEqual(2,r.Level);Assert.AreEqual(36,r.NextLevel);
            Assert.AreEqual(hp+2,r.MaxHealth);Assert.AreEqual(2,r.PendingLevels);
        }
        [TestCase(1,0,.5,1)] [TestCase(2,0,.059,2)] [TestCase(2,0,.061,1)]
        [TestCase(4,0,.019,3)] [TestCase(4,0,.021,2)]
        [TestCase(8,0,.0022,4)] [TestCase(8,0,.0024,3)]
        [TestCase(2,100,.119,2)] [TestCase(2,-100,.031,1)]
        [TestCase(100,100,.081,3)] [TestCase(100,100,.251,2)] [TestCase(100,100,.601,1)]
        public void RarityUsesSingleDescendingRollWithLuckAndCaps(int level,float luck,double roll,int tier)
            =>Assert.AreEqual(tier,ReferenceRarity.Roll(level,luck,roll));
        [TestCase(5,2)] [TestCase(10,3)] [TestCase(15,3)] [TestCase(20,3)] [TestCase(25,4)]
        public void MilestonesOverrideRandomRarity(int level,int tier)
        { Assert.AreEqual(tier,ReferenceRarity.Upgrade(level,100,0));Assert.AreEqual(tier,ReferenceRarity.Upgrade(level,-100,.99)); }
        [Test] public void UpgradeRerollAvoidsPreviousStatsAndChargesExactlyOnce()
        {
            var r=Run();r.Collect(100);r.Tick(r.Remaining);
            var before=r.Offers.Select(o=>o.Kind).ToArray();int money=r.Scrap,cost=r.RerollPrice;
            r.Reroll();Assert.AreEqual(money-cost,r.Scrap);
            Assert.IsFalse(r.Offers.Any(o=>before.Contains(o.Kind)));
            Assert.AreEqual(4,r.Offers.Select(o=>o.Kind).Distinct().Count());
        }
        [Test] public void NeutralBossAndSummonBirthsAreNotStarvedByNormalBacklog()
        {
            var q=new ReferenceBirthQueue<int>();var actual=new List<int>();
            for(int i=0;i<120;i++)q.Push(i);
            q.Push(500,BirthLane.Tree);q.Push(600,BirthLane.Boss);q.Push(700,BirthLane.Summon);
            q.Tick(.049f,actual.Add);Assert.IsEmpty(actual);
            q.Tick(.001f,actual.Add);CollectionAssert.AreEqual(new[]{500,600,700,119,118},actual);
            actual.Clear();q.Tick(.05f,actual.Add);CollectionAssert.AreEqual(new[]{117},actual);
        }
        [TestCase(100,1)] [TestCase(110,1)] [TestCase(119,1)] [TestCase(120,2)] [TestCase(300,2)]
        public void QueueAccelerationStartsAt120(int count,int batch)=>Assert.AreEqual(batch,ReferenceBirthQueue<int>.RegularBatch(count));
        [Test] public void ChargeCooldownStartsAfterDashWithoutExtraRecovery()
        {
            var b=EnemyBehavior.For("charger");var a=new EnemyAttack(b,()=>40f/60);
            a.Step(40f/60,Vector2.right,true);Assert.AreEqual(AttackPhase.Windup,a.Phase);
            Assert.IsTrue(a.Step(.4f,Vector2.left,true));Assert.AreEqual(Vector2.right,a.Direction);
            a.Step(.75f,Vector2.left,true);Assert.AreEqual(AttackPhase.Approach,a.Phase);
            a.Step(.65f,Vector2.left,true);Assert.AreEqual(AttackPhase.Approach,a.Phase);
            a.Step(.017f,Vector2.left,true);Assert.AreEqual(AttackPhase.Windup,a.Phase);
        }
        [Test] public void ProfilesRetainDistinctMovementAndProjectileRules()
        {
            Assert.AreEqual(EnemyMovement.Destination,EnemyBehavior.For("looter").movement);
            Assert.AreEqual(EnemyMovement.Destination,EnemyBehavior.For("bruiser").movement);
            Assert.AreEqual(EnemyMovement.Offset,EnemyBehavior.For("spawner").movement);
            Assert.AreEqual(2,EnemyBehavior.For("junkie").minRange);
            Assert.AreEqual(6,EnemyBehavior.For("spitter").projectileSpeed);
            Assert.AreEqual(.75f,EnemyBehavior.For("spitter").projectileGrowth);
            Assert.IsFalse(EnemyBehavior.For("buffer").canBoost);
            Assert.IsFalse(EnemyBehavior.For("evil_mob").canBoost);
            Assert.IsTrue(EnemyBehavior.For("predator").charge);
            Assert.IsTrue(EnemyBehavior.For("predator",true).ranged);
            Assert.AreEqual(.5f,EnemyBehavior.For("predator").active);
        }
        [Test] public void PursuerGainsDiscreteAdditiveSpeedWithTenBoostLimit()
        {
            Assert.AreEqual(0,EnemyBehavior.PursuerBonus(.99f));
            Assert.AreEqual(.45f,EnemyBehavior.PursuerBonus(1));
            Assert.AreEqual(4.5f,EnemyBehavior.PursuerBonus(100));
        }
    }
}

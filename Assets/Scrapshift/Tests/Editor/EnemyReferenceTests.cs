using System.Linq;
using NUnit.Framework;
using UnityEngine;
namespace Scrapshift.Tests
{
    public sealed class EnemyReferenceTests
    {
        ExtendedEncounterData Data=>JsonUtility.FromJson<ExtendedEncounterData>(Resources.Load<TextAsset>("extended-encounters").text);
        EncounterEntry Enemy(string id)=>Data.enemies.Single(x=>x.id==id);
        [Test] public void EverySpawnableEnemyHasSourceCollisionGeometry()
        {
            foreach(var e in Data.enemies)
            {
                var c=CombatGeometry.Enemy(e.id);
                Assert.Greater(c.body,0,e.id);Assert.Greater(c.hurt,0,e.id);Assert.Greater(c.contact,0,e.id);
            }
            Assert.AreEqual(.400125f,CombatGeometry.Enemy("mad_slasher").hurt,.00001f);
            Assert.AreEqual(.13f,CombatGeometry.Enemy("tentacle").body,.00001f);
        }
        [Test] public void SourceRosterIncludesOrdinaryAdditionalElitesAndBothFinalBosses()
        {
            Assert.AreEqual(38,Data.enemies.Length);Assert.AreEqual(8,Data.enemies.Count(x=>x.role=="Elite"));Assert.AreEqual(2,Data.enemies.Count(x=>x.role=="Boss"));
            foreach(var enemy in Data.enemies)foreach(var state in enemy.states)
            {
                var a=state.attack;Assert.GreaterOrEqual(a.windup,0,enemy.id);Assert.GreaterOrEqual(a.tail,0,enemy.id);
                if(a.enabled&&!a.charge&&!a.spawn)Assert.DoesNotThrow(()=>EnemyProjectileView.Profile(a.projectileScene),enemy.id);
            }
        }
        [TestCase("spitter",.8f,.2f)] [TestCase("junkie",.4f,.1f)] [TestCase("slasher",.4f,0)] [TestCase("slasher_egg",1.6f,.4f)]
        public void ReleaseAndRecoveryComeFromAnimationEvents(string id,float release,float tail)
        {var a=Enemy(id).states[0].attack;Assert.AreEqual(release,a.windup,.0001f);Assert.AreEqual(tail,a.tail,.0001f);}
        [Test] public void ChargerUsesOffsetTargetAndFiveFrameCooldownVariation()
        {var a=Enemy("charger").states[0].attack;Assert.AreEqual(1,a.chargeTarget);Assert.AreEqual(5f/60,a.jitter,.0001f);Assert.AreEqual(.75f,a.active);}
        [TestCase(8,.5f,1)] [TestCase(11,.75f,.75f)] [TestCase(17,1,1)]
        public void EarlyBossScalingPreservesSourceBranches(int wave,float hp,float damage)
        {Assert.AreEqual(hp,EnemyReferenceRules.BossHealthFactor(wave));Assert.AreEqual(damage,EnemyReferenceRules.BossDamageFactor(wave));}
        [Test] public void ReleaseTimedChargeRecalculatesOnceAndThenLocks()
        {
            var a=new EnemyAttack(new EnemyBehavior{charge=true,windup=.2f,active=.5f,source=new EncounterAttack{chargeTiming=1}},()=>0);
            a.Step(.01f,Vector2.right,true,()=>Vector2.right);a.Step(.2f,Vector2.up,true,()=>Vector2.up);
            Assert.AreEqual(Vector2.up,a.Direction);a.Step(.1f,Vector2.left,true,()=>Vector2.left);Assert.AreEqual(Vector2.up,a.Direction);
        }
        [Test] public void LongCooldownAppliesAfterConfiguredShotCount()
        {
            var a=new EnemyAttack(new EnemyBehavior{ranged=true,windup=.1f,active=0,source=new EncounterAttack{longEvery=2,longCooldown=2}},()=>0);
            for(int i=0;i<2;i++){a.Step(.01f,Vector2.right,true);Assert.IsTrue(a.Step(.1f,Vector2.right,true));}
            a.Step(1,Vector2.right,true);Assert.AreEqual(AttackPhase.Approach,a.Phase);
            a.Step(1,Vector2.right,true);Assert.AreEqual(AttackPhase.Windup,a.Phase);
        }
        [Test] public void ArrowWarningsAreRestrictedToOriginalSlashUsers()
        {
            var users=Data.enemies.Where(e=>e.states.Any(s=>s.attack.projectile.Contains("slash"))||e.extra.Any(a=>a.projectile.Contains("slash"))).Select(e=>e.id).ToArray();
            CollectionAssert.AreEquivalent(new[]{"slasher","mad_slasher","tentacle","butcher","croc","mantis","mom"},users);
            foreach(var e in Data.enemies)foreach(var s in e.states)if(s.attack.charge)Assert.IsFalse(s.attack.projectile.Contains("slash"));
        }
        [Test] public void DelayedSlashFramesAndForwardVariantStayDistinct()
        {
            var a=EnemyProjectileView.Profile("projectiles/slash/slash_projectile.tscn");var b=EnemyProjectileView.Profile("projectiles/slash/slash_projectile_2.tscn");
            Assert.AreEqual(0,a.FrameIndex(.479f));Assert.AreEqual(1,a.FrameIndex(.48f));Assert.AreEqual(.63f,a.stop,.0001f);
            Assert.AreEqual(0,a.offset[0]);Assert.AreEqual(165,b.offset[0]);Assert.AreEqual(a.frames.Length,b.frames.Length);
        }
    }
}

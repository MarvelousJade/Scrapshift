using NUnit.Framework;
using UnityEngine;

namespace Scrapshift.Tests
{
    public sealed class SupportRemainingParityTests
    {
        [SetUp] public void Setup()=>ArenaGeometry.SetMapSize(0);
        [TearDown] public void Cleanup()=>ArenaGeometry.SetMapSize(0);

        [TestCase(2.1f,-.12f,true)]
        [TestCase(2.25f,-.12f,false)]
        [TestCase(0,2.13f,false)]
        [TestCase(0,-2.3f,true)]
        public void MothZoneUsesTheOffsetPlayerBodyRatherThanPlayerOrigin(float x,float y,bool expected)
            =>Assert.AreEqual(expected,SupportBehavior.InsideMothZone(new Vector2(x,y),Vector2.zero,2));

        [Test] public void ScaledMothZoneRetainsTheUnscaledPlayerBodyRadius()
        {
            Assert.IsTrue(SupportBehavior.InsideMothZone(new Vector2(1.2f,-.12f),Vector2.zero,1));
            Assert.IsFalse(SupportBehavior.InsideMothZone(new Vector2(1.25f,-.12f),Vector2.zero,1));
        }
        [Test] public void OverlappingMothZoneEntriesAndExitsRetainAdditiveRegeneration()
        {
            var run=new RunRules(17);run.Start();run.GainWeaponStat("hp_regeneration",1);run.Hurt(8);
            var moths=new[]{new Vector2(-2.1f,.12f),new Vector2(2.1f,.12f)};
            foreach(var moth in moths)if(SupportBehavior.InsideMothZone(Vector2.zero,moth,2))run.HealingDrones++;
            Assert.AreEqual(2,run.HealingDrones);Assert.AreEqual(3,run.RegenerationHealMultiplier);
            float before=run.Health;run.Tick(5);Assert.AreEqual(before+3,run.Health);
            run.HealingDrones=SupportBehavior.InsideMothZone(Vector2.right,moths[0],2)?1:0;
            if(SupportBehavior.InsideMothZone(Vector2.right,moths[1],2))run.HealingDrones++;
            Assert.AreEqual(1,run.HealingDrones);Assert.AreEqual(2,run.RegenerationHealMultiplier);
            run.HealingDrones=0;Assert.AreEqual(1,run.RegenerationHealMultiplier);
        }
        [Test] public void MultipleMothsEnableDoubleLifeStealInsteadOfMultiplyingItAgain()
        {
            var run=new RunRules(17);run.Start();run.GainWeaponStat("lifesteal",100);run.Hurt(8);run.HealingDrones=3;
            float before=run.Health;Assert.IsTrue(run.TryLifeSteal(0));Assert.AreEqual(before+2,run.Health);
            Assert.IsFalse(run.TryLifeSteal(0),"The shared life-steal cooldown still applies.");
        }
        [Test] public void BotMineUsesTheOriginalSquareCenterInTheMapInterior()
        {
            var origin=new Vector2(2,-1);
            Assert.AreEqual(origin,SupportBehavior.BotMineSpawnCenter(origin));
        }
        [TestCase(1,1)][TestCase(-1,1)][TestCase(1,-1)][TestCase(-1,-1)]
        public void BotMineClampsTheEntireSamplingSquareBeforeRandomization(int xSign,int ySign)
        {
            var half=ArenaGeometry.HalfSize;
            var center=SupportBehavior.BotMineSpawnCenter(new Vector2(xSign*half.x,ySign*half.y));
            Assert.That(center.x,Is.EqualTo(xSign*(half.x-1.14f)).Within(.00001f));
            Assert.That(center.y,Is.EqualTo(ySign*(half.y-1.14f)).Within(.00001f));
            foreach(float x in new[]{-.5f,.5f})foreach(float y in new[]{-.5f,.5f})
            {
                var sample=center+new Vector2(x,y);
                Assert.That(Mathf.Abs(sample.x),Is.LessThanOrEqualTo(half.x-.64f+.00001f));
                Assert.That(Mathf.Abs(sample.y),Is.LessThanOrEqualTo(half.y-.64f+.00001f));
            }
        }
        [Test] public void LootwormKeepsItsLiveTargetWhenAnotherMovesCloser()
        {
            var cache=new SupportBehavior.TargetCache<object>();var original=new object();var closer=new object();object nearest=original;int selections=0;
            System.Func<object> select=()=>{selections++;return nearest;};
            Assert.AreSame(original,cache.Resolve(select,_=>true));
            nearest=closer;
            Assert.AreSame(original,cache.Resolve(select,_=>true),"Moving Gold or the pet must not continuously change its target.");
            Assert.AreEqual(1,selections);
        }
        [Test] public void LootwormGoldSpawnIncludingMergedOrZeroBatchInvalidatesSelection()
        {
            var cache=new SupportBehavior.TargetCache<object>();var original=new object();var nearest=new object();
            Assert.AreSame(original,cache.Resolve(()=>original,_=>true));
            // main.spawn_gold emits its signal even if no new Gold object was added.
            cache.Invalidate();Assert.AreSame(nearest,cache.Resolve(()=>nearest,_=>true));
            cache.Invalidate();Assert.AreSame(original,cache.Resolve(()=>original,_=>true));
        }
        [Test] public void LootwormNeutralSpawnCanReplaceItsGoldTargetWithATree()
        {
            var cache=new SupportBehavior.TargetCache<object>();var gold=new object();var tree=new object();
            Assert.AreSame(gold,cache.Resolve(()=>gold,_=>true));
            cache.Invalidate();Assert.AreSame(tree,cache.Resolve(()=>tree,_=>true));
        }
        [Test] public void LootwormReselectsWhenItsTargetIsCollectedEatenOrKilled()
        {
            var cache=new SupportBehavior.TargetCache<object>();var first=new object();var second=new object();
            var live=new System.Collections.Generic.HashSet<object>{first,second};
            Assert.AreSame(first,cache.Resolve(()=>first,live.Contains));
            live.Remove(first);
            Assert.AreSame(second,cache.Resolve(()=>second,live.Contains));
            live.Remove(second);
            Assert.IsNull(cache.Resolve(()=>null,live.Contains));
        }
        [Test] public void LootwormEmptyWorldUsesAnIdleSentinelUntilASpawnEvent()
        {
            var cache=new SupportBehavior.TargetCache<object>();int selections=0;object spawned=null;
            System.Func<object> select=()=>{selections++;return spawned;};
            Assert.IsNull(cache.Resolve(select,_=>true));
            for(int i=0;i<60;i++)Assert.IsNull(cache.Resolve(select,_=>true));
            Assert.AreEqual(1,selections,"The reference uses self as an idle target rather than polling every physics tick.");
            spawned=new object();Assert.IsNull(cache.Resolve(select,_=>true));
            cache.Invalidate();Assert.AreSame(spawned,cache.Resolve(select,_=>true));Assert.AreEqual(2,selections);
        }
        [Test] public void MultipleLootwormsHaveIndependentTargetsAndNewWaveStartsFresh()
        {
            var first=new SupportBehavior.TargetCache<object>();var second=new SupportBehavior.TargetCache<object>();
            var gold=new object();var tree=new object();
            Assert.AreSame(gold,first.Resolve(()=>gold,_=>true));Assert.AreSame(gold,second.Resolve(()=>gold,_=>true),"Lootworms do not reserve targets exclusively.");
            first.Invalidate();Assert.AreSame(tree,first.Resolve(()=>tree,_=>true));Assert.AreSame(gold,second.Resolve(()=>tree,_=>true));
            var nextWave=new SupportBehavior.TargetCache<object>();Assert.AreSame(tree,nextWave.Resolve(()=>tree,_=>true));
        }
    }
}

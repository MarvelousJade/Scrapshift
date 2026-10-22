using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Scrapshift.Editor;

namespace Scrapshift.Tests
{
    public sealed class ItemProcTargetTests
    {
        const BindingFlags Hidden=BindingFlags.Instance|BindingFlags.NonPublic;
        ArenaGame game;GameObject owner;UnityEngine.Random.State savedRandom;
        object Get(string key)=>typeof(ArenaGame).GetField(key,Hidden).GetValue(game);
        object Call(string method,params object[] args)=>typeof(ArenaGame).GetMethod(method,Hidden).Invoke(game,args);
        static void Field(object obj,string key,object value)=>obj.GetType().GetField(key).SetValue(obj,value);
        Action<float> Capture()=>(Action<float>)Call("CaptureImplantDamageTarget");
        [SetUp] public void SetUp()
        {
            savedRandom=UnityEngine.Random.state;owner=new GameObject("Proc target acceptance");game=owner.AddComponent<ArenaGame>();
            game.catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
            game.Rules.Configure(game.catalog);
            var lich=game.catalog.data.characters.Single(c=>c.id=="lich");
            game.Rules.Start(lich,StarterChoices.Available(game.catalog,lich).First());
            game.Rules.Inventory.Clear();game.Rules.CaptureRandomDamageTarget=Capture;
            typeof(ArenaGame).GetField("sound",Hidden).SetValue(game,owner.AddComponent<GameAudio>());
            game.sparkPrefab=new GameObject("Proc target spark");game.sparkPrefab.transform.SetParent(owner.transform);game.sparkPrefab.AddComponent<SpriteRenderer>();
        }
        [TearDown] public void TearDown()
        {
            foreach(string list in new[]{"sparks","impacts"})foreach(var effect in (IEnumerable)Get(list))
            {var obj=(GameObject)effect.GetType().GetField("obj").GetValue(effect);if(obj!=null)UnityEngine.Object.DestroyImmediate(obj);}
            UnityEngine.Object.DestroyImmediate(owner);UnityEngine.Random.state=savedRandom;
        }
        object Enemy(float hp)
        {
            var enemy=Activator.CreateInstance(typeof(ArenaGame).GetNestedType("Enemy",BindingFlags.NonPublic),true);
            Field(enemy,"hp",hp);Field(enemy,"maxHp",hp);Field(enemy,"type",new EnemyEntry{id="proc-target",role="Enemy"});
            Field(enemy,"renderers",Array.Empty<Renderer>());
            ((IList)Get("enemies")).Add(enemy);return enemy;
        }
        sealed class NoRolls:System.Random
        {public override double NextDouble(){Assert.Fail("A rejected target must not consume a proc probability roll.");return 0;}}
        [Test] public void ZeroHpPendingVictimAbortsLichBeforeProbabilityRoll()
        {
            Enemy(0);typeof(RunRules).GetField("random",Hidden).SetValue(game.Rules,new NoRolls());
            int events=0;game.Rules.RandomDamageRequested+=_=>events++;
            typeof(RunRules).GetProperty("Health").GetSetMethod(true).Invoke(game.Rules,new object[]{9f});
            game.Rules.Heal(1);
            Assert.AreEqual(10,game.Rules.Health);Assert.AreEqual(0,events);
            Assert.AreEqual(0,Get("implantDamage").GetType().GetProperty("Count").GetValue(Get("implantDamage")));
        }

        [Test] public void MixedPendingAndLiveCandidatesCanAbortInsteadOfGuaranteeingLiveTarget()
        {
            Enemy(0);Enemy(1000);int aborts=0,hits=0;
            for(int seed=0;seed<64;seed++)
            {UnityEngine.Random.InitState(seed);if(Capture()==null)aborts++;else hits++;}
            Assert.Greater(aborts,0);Assert.Greater(hits,0);Assert.AreEqual(64,aborts+hits);
        }

        [Test] public void LiveOnlyCandidateAlwaysReservesAndZeroHpOnlyNeverReserves()
        {
            var enemy=Enemy(1000);
            for(int seed=0;seed<64;seed++){UnityEngine.Random.InitState(seed);Assert.NotNull(Capture());}
            Field(enemy,"hp",0f);
            for(int seed=0;seed<64;seed++){UnityEngine.Random.InitState(seed);Assert.IsNull(Capture());}
        }

        [TestCase(false)][TestCase(true)]
        public void CapturedTargetCannotBeReplacedIfItDiesOrLeavesBeforeInvocation(bool removed)
        {
            var selected=Enemy(1000);var submit=Capture();Assert.NotNull(submit);
            var other=Enemy(1000);
            if(removed)((IList)Get("enemies")).Remove(selected);else Field(selected,"hp",0f);
            submit(10);
            Call("ResolveImplantDamage");
            Assert.AreEqual(1000f,other.GetType().GetField("hp").GetValue(other));
            Assert.AreEqual(0,Get("implantDamage").GetType().GetProperty("Count").GetValue(Get("implantDamage")));
        }

        [TestCase(0,10,90)][TestCase(10,0,89)]
        public void QueuedDamageRetainsTheVulnerabilityAtItsSourceCallback(float before,float after,float expectedHealth)
        {
            // Original Main.handle_stat_damages applies Unit defense math at
            // the healing callback, before later elemental hits/timer expiry.
            var target=Enemy(100);Field(target,"iceClock",before>0?3f:0f);Field(target,"icePercent",before);
            var submit=Capture();Assert.NotNull(submit);submit(10);
            Field(target,"iceClock",after>0?3f:0f);Field(target,"icePercent",after);
            Call("ResolveImplantDamage");
            Assert.AreEqual(expectedHealth,target.GetType().GetField("hp").GetValue(target));
            Assert.AreEqual(after,target.GetType().GetField("icePercent").GetValue(target),"Dispatch must not mutate the enemy's current status to simulate its captured context.");
        }
    }
}

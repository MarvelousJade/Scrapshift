using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Scrapshift.Editor;

namespace Scrapshift.Tests
{
    public sealed class CorpseMembershipFollowupTests
    {
        const BindingFlags Hidden=BindingFlags.Instance|BindingFlags.NonPublic;
        ArenaGame game;GameObject owner;UnityEngine.Random.State savedRandom;
        object Get(string name)=>typeof(ArenaGame).GetField(name,Hidden).GetValue(game);
        object Call(string name,params object[] args)=>typeof(ArenaGame).GetMethod(name,Hidden).Invoke(game,args);
        static void Field(object obj,string key,object value)=>obj.GetType().GetField(key).SetValue(obj,value);
        sealed class NoProcRolls:System.Random
        {
            public override double NextDouble(){Assert.Fail("Source Main selects a zero-HP victim and aborts before Cyberball's probability roll.");return 0;}
        }

        [SetUp] public void Setup()
        {
            savedRandom=UnityEngine.Random.state;owner=new GameObject("Dying member callback acceptance");game=owner.AddComponent<ArenaGame>();
            game.catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
            game.Rules.Configure(game.catalog);
            var character=game.catalog.data.characters.Single(c=>c.id=="well_rounded");
            game.Rules.Start(character,StarterChoices.Available(game.catalog,character).First());game.Rules.Inventory.Clear();
            game.Rules.GrantItem(game.catalog.data.items.Single(i=>i.id=="cyberball"));
            game.Rules.CaptureRandomDamageTarget=()=> (Action<float>)Call("CaptureImplantDamageTarget");
            typeof(ArenaGame).GetField("sound",Hidden).SetValue(game,owner.AddComponent<GameAudio>());
            game.sparkPrefab=new GameObject("Dying member spark");game.sparkPrefab.transform.SetParent(owner.transform);game.sparkPrefab.AddComponent<SpriteRenderer>();
        }

        [TearDown] public void Cleanup()
        {
            foreach(string list in new[]{"sparks","impacts"})foreach(var effect in (IEnumerable)Get(list))
            {
                var obj=(GameObject)effect.GetType().GetField("obj").GetValue(effect);
                if(obj!=null)UnityEngine.Object.DestroyImmediate(obj);
            }
            UnityEngine.Object.DestroyImmediate(owner);UnityEngine.Random.state=savedRandom;
        }

        [TestCase(1)][TestCase(3)]
        public void ActualDeathCallbacksRetainDyingMembersAndAbortCyberballBeforeRolling(int count)
        {
            // The original Main target is constructed before its spawner child;
            // the measured signal dispatch preserves corpse membership in both
            // fresh and reconnected source cases. Evil Mob's zero stored gold
            // avoids unrelated instantiated pickup-scene dependencies here.
            var active=(IList)Get("enemies");
            for(int i=0;i<count;i++)
            {
                var enemy=Activator.CreateInstance(typeof(ArenaGame).GetNestedType("Enemy",BindingFlags.NonPublic),true);
                Field(enemy,"hp",1f);Field(enemy,"maxHp",1f);Field(enemy,"pos",Vector2.up);
                Field(enemy,"type",new EnemyEntry{id="evil_mob",role="Enemy"});Field(enemy,"renderers",Array.Empty<Renderer>());
                active.Add(enemy);
                Call("Damage",i,1f,-100f,0f,Vector2.zero,null,true,true,false);
            }
            Assert.AreEqual(count,active.Count);Assert.AreEqual(0,game.Rules.Kills);
            typeof(RunRules).GetField("random",Hidden).SetValue(game.Rules,new NoProcRolls());
            int captures=0;
            game.Rules.CaptureRandomDamageTarget=()=>
            {
                captures++;
                Assert.Greater(active.Count,0,"Main's death callback must still observe its own dying member; an empty pool would also abort but has different source membership.");
                foreach(var member in active)Assert.AreEqual(0f,member.GetType().GetField("hp").GetValue(member));
                return (Action<float>)Call("CaptureImplantDamageTarget");
            };
            int procs=0;game.Rules.RandomDamageRequested+=_=>procs++;
            Call("ResolveEnemyDeaths");Call("ResolveEnemyDeaths");
            Assert.AreEqual(count,captures);Assert.AreEqual(0,procs);Assert.AreEqual(count,game.Rules.Kills);
            Assert.AreEqual(0,active.Count);Assert.AreEqual(count,((IList)Get("corpses")).Count);
        }
    }
}

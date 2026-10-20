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
    public sealed class DeathDispatchFollowupTests
    {
        const BindingFlags Hidden=BindingFlags.Instance|BindingFlags.NonPublic;
        ArenaGame game;GameObject owner;UnityEngine.Random.State savedRandom;
        object Get(string key)=>typeof(ArenaGame).GetField(key,Hidden).GetValue(game);
        object Call(string method,params object[] args)=>typeof(ArenaGame).GetMethod(method,Hidden).Invoke(game,args);
        static void Field(object obj,string key,object value)=>obj.GetType().GetField(key).SetValue(obj,value);
        static T Field<T>(object obj,string key)=>(T)obj.GetType().GetField(key).GetValue(obj);
        [SetUp] public void Setup()
        {
            savedRandom=UnityEngine.Random.state;owner=new GameObject("Death dispatch source acceptance");game=owner.AddComponent<ArenaGame>();
            game.catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
            game.Rules.Configure(game.catalog);
            var lich=game.catalog.data.characters.Single(c=>c.id=="lich");
            game.Rules.Start(lich,StarterChoices.Available(game.catalog,lich).First());game.Rules.Inventory.Clear();
            game.Rules.CaptureRandomDamageTarget=()=> (Action<float>)Call("CaptureImplantDamageTarget");
            // ArenaGame.Start installs the same hook in an ordinary player.
            // These Editor tests bind it explicitly rather than invoking Start.
            game.Rules.BeforeDeathCleanup=()=>Call("ResolveCombatCallbacks");
            typeof(ArenaGame).GetField("sound",Hidden).SetValue(game,owner.AddComponent<GameAudio>());
            game.sparkPrefab=new GameObject("Death dispatch spark");game.sparkPrefab.transform.SetParent(owner.transform);game.sparkPrefab.AddComponent<SpriteRenderer>();
            game.Rules.GainWeaponStat("lifesteal",-1000);
            typeof(RunRules).GetProperty("Health").GetSetMethod(true).Invoke(game.Rules,new object[]{9f});
        }
        [TearDown] public void Cleanup()
        {
            foreach(string list in new[]{"sparks","impacts"})foreach(var effect in (IEnumerable)Get(list))
            {var obj=Field<GameObject>(effect,"obj");if(obj!=null)UnityEngine.Object.DestroyImmediate(obj);}
            UnityEngine.Object.DestroyImmediate(owner);UnityEngine.Random.state=savedRandom;
        }
        object Enemy(float hp)
        {
            var enemy=Activator.CreateInstance(typeof(ArenaGame).GetNestedType("Enemy",BindingFlags.NonPublic),true);
            Field(enemy,"hp",hp);Field(enemy,"maxHp",hp);Field(enemy,"pos",Vector2.up);
            Field(enemy,"type",new EnemyEntry{id="death-dispatch-target",role="Enemy"});Field(enemy,"renderers",Array.Empty<Renderer>());
            ((IList)Get("enemies")).Add(enemy);return enemy;
        }
        sealed class PickupRolls:System.Random
        {
            int used;
            public override double NextDouble()
            {
                Assert.Less(used,3,"After Monkey healing makes Lich kill the only target, Elephant must abort before its chance roll.");
                return used++==0?.9:0;
            }
            public int Used=>used;
        }
        [Test] public void MonkeyLichLethalPickupLeavesElephantNoEligibleTargetBeforeItsRoll()
        {
            game.Rules.GrantItem(game.catalog.data.items.Single(i=>i.id=="cute_monkey"));
            game.Rules.GrantItem(game.catalog.data.items.Single(i=>i.id=="baby_elephant"));
            var target=Enemy(10);var rolls=new PickupRolls();
            typeof(RunRules).GetField("random",Hidden).SetValue(game.Rules,rolls);
            var events=new List<float>();game.Rules.RandomDamageRequested+=events.Add;
            game.Rules.Collect(1);
            Assert.AreEqual(3,rolls.Used);Assert.AreEqual(0,Field<float>(target,"hp"));
            CollectionAssert.AreEqual(new[]{10f},events,"Monkey healing invokes Lich before the same pickup's Elephant effect, which sees zero HP and stops.");
        }
        [Test] public void LichDamageChangesHealthBeforeAnUnrelatedLaterWeaponHit()
        {
            var target=Enemy(100);game.Rules.Heal(1);
            Assert.AreEqual(90,Field<float>(target,"hp"),"Main.handle_stat_damages synchronously applies the Lich hit at the healing callback.");
            Call("Damage",0,7f,-100f,0f,Vector2.up,null,true,false,false);
            Assert.AreEqual(83,Field<float>(target,"hp"));
        }
        [Test] public void EarlierEnemyDeathDispatchesBeforeLaterPlayerLethalCleanup()
        {
            var target=Enemy(1);Field<EnemyEntry>(target,"type").id="evil_mob";
            Call("Damage",0,1f,-100f,0f,Vector2.zero,null,true,false,false);
            Assert.AreEqual(0,game.Rules.Kills);
            bool dispatchedWhilePlaying=false;
            game.Rules.BeforeDeathCleanup=()=>
            {
                Call("ResolveCombatCallbacks");
                dispatchedWhilePlaying=game.Rules.Phase==RunPhase.Playing&&game.Rules.Kills==1;
            };
            game.Rules.Hurt(10000);
            Assert.IsTrue(dispatchedWhilePlaying,"Earlier Unit death requests precede Player's later deferred die and cleanup.");
            Assert.AreEqual(RunPhase.Lost,game.Rules.Phase);Assert.AreEqual(1,game.Rules.Kills);
            Assert.AreEqual(0,((IList)Get("enemies")).Count);Assert.AreEqual(1,((IList)Get("corpses")).Count);
        }
        [Test] public void PlayerLethalCleanupRejectsALaterEnemyHitWithoutAwardingAKill()
        {
            var target=Enemy(1);Field<EnemyEntry>(target,"type").id="evil_mob";
            game.Rules.Hurt(10000);Assert.AreEqual(RunPhase.Lost,game.Rules.Phase);
            Call("Damage",0,1f,-100f,0f,Vector2.zero,null,true,false,false);
            Call("ResolveCombatCallbacks");
            Assert.AreEqual(1,Field<float>(target,"hp"));Assert.AreEqual(0,game.Rules.Kills);
            Assert.AreEqual(0,((IList)Get("corpses")).Count);
        }
        [Test] public void BurnAchievementCounterPrecedesDeferredBurningItemStatGains()
        {
            game.Rules.GrantItem(game.catalog.data.items.Single(i=>i.id=="will_o_the_wisp"));
            float elemental=game.Rules.Stat("elemental_damage");
            for(int i=0;i<30;i++)
            {
                var target=Enemy(1);Field<EnemyEntry>(target,"type").id="evil_mob";
                Call("Damage",i,1f,-100f,0f,Vector2.zero,null,true,true,false);
            }
            Assert.AreEqual(30,typeof(RunRules).GetField("burningKills",Hidden).GetValue(game.Rules),"BurningTimer advances this achievement immediately after Unit.take_damage makes death pending.");
            Assert.AreEqual(elemental,game.Rules.Stat("elemental_damage"),"Main has not dispatched the item's burning-death stat gain yet.");
            Call("ResolveCombatCallbacks");
            Assert.AreEqual(elemental+game.Rules.Character.Gain("elemental_damage"),game.Rules.Stat("elemental_damage"));
            Assert.AreEqual(30,game.Rules.Kills);
        }
        [TestCase(false)][TestCase(true)]
        public void DeferredDeathUpdatesLinkedEnemyOrTreeStatsBeforeTheNextCensus(bool tree)
        {
            string key;float per;
            if(tree)
            {
                var cryptid=game.catalog.data.characters.Single(c=>c.id=="cryptid");
                game.Rules.Start(cryptid,StarterChoices.Available(game.catalog,cryptid).First());game.Rules.Inventory.Clear();
                key="hp_regeneration";per=3;game.Rules.LivingTrees=0;
            }
            else
            {
                game.Rules.GrantItem(game.catalog.data.items.Single(i=>i.id=="community_support"));
                key="attack_speed";per=1;game.Rules.LivingEnemies=0;
            }
            float baseline=game.Rules.Stat(key);
            if(tree)game.Rules.LivingTrees=1;else game.Rules.LivingEnemies=1;
            Assert.AreEqual(baseline+per,game.Rules.Stat(key));
            var target=Enemy(1);var type=Field<EnemyEntry>(target,"type");type.id="evil_mob";type.role=tree?"Tree":"Enemy";
            Call("Damage",0,1f,-100f,0f,Vector2.zero,null,true,false,false);
            Assert.AreEqual(baseline+per,game.Rules.Stat(key),"The source census changes at deferred Main dispatch, not at lethal HP mutation.");
            Call("ResolveCombatCallbacks");Call("ResolveCombatCallbacks");
            Assert.AreEqual(baseline,game.Rules.Stat(key));
            Assert.AreEqual(0,tree?game.Rules.LivingTrees:game.Rules.LivingEnemies);
        }
        [Test] public void GiantBeltReadsHealthAfterTheEarlierLichHit()
        {
            game.Rules.GrantItem(game.catalog.data.items.Single(i=>i.id=="giant_belt"));
            game.Rules.GainWeaponStat("crit_chance",1000);
            var target=Enemy(100);game.Rules.Heal(1);
            bool critical=(bool)Call("Damage",0,10f,0f,0f,Vector2.up,new Equipment{critMultiplier=2},true,false,false);
            Assert.IsTrue(critical);Assert.AreEqual(61,Field<float>(target,"hp"),"Original 100 - Lich10 - (critical20 + floor(90 * .10)) = 61.");
        }
        [Test] public void LethalLichPreemptsTheLaterWeaponAndKeepsAStablePendingMember()
        {
            var target=Enemy(10);Field<EnemyEntry>(target,"type").id="evil_mob";game.Rules.Heal(1);
            Assert.AreEqual(0,Field<float>(target,"hp"));
            Assert.IsTrue(((IList)Get("enemies")).Contains(target),"Original victim remains a member until deferred die; indexed weapon/enemy traversal must remain stable.");
            Call("Damage",0,50f,-100f,0f,Vector2.up,null,true,false,false);
            Assert.AreEqual(0,Field<float>(target,"hp"),"Unit.take_damage rejects the later hit once its death is pending.");
            Assert.AreEqual(0,game.Rules.Kills,"Main.on_enemy_died has not run before the deferred death boundary.");
            Call("ResolveEnemyDeaths");Call("ResolveEnemyDeaths");
            Assert.AreEqual(1,game.Rules.Kills);Assert.AreEqual(1,((IList)Get("corpses")).Count);
            Assert.IsFalse(((IList)Get("enemies")).Contains(target),"The explicit boundary finalizes exactly once. A zero-value Evil Mob avoids unrelated loot-scene dependencies.");
        }
    }
}

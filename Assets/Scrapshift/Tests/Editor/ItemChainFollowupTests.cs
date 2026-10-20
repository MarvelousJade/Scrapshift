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
    public sealed class ItemChainFollowupTests
    {
        const BindingFlags Hidden=BindingFlags.Instance|BindingFlags.NonPublic;
        ArenaGame game;
        readonly List<GameObject> objects=new List<GameObject>();
        UnityEngine.Random.State previousRandom;
        object Get(string name)=>typeof(ArenaGame).GetField(name,Hidden).GetValue(game);
        void Set(string name,object value)=>typeof(ArenaGame).GetField(name,Hidden).SetValue(game,value);
        object Call(string name,params object[] args)=>typeof(ArenaGame).GetMethod(name,Hidden).Invoke(game,args);
        static void Field(object target,string name,object value)=>target.GetType().GetField(name).SetValue(target,value);
        static T Field<T>(object target,string name)=>(T)target.GetType().GetField(name).GetValue(target);
        GameObject Object(string name){var obj=new GameObject(name);objects.Add(obj);return obj;}
        [SetUp] public void Setup()
        {
            previousRandom=UnityEngine.Random.state;
            game=Object("Item chain fixture").AddComponent<ArenaGame>();
            game.catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
            game.Rules.Configure(game.catalog);game.Rules.Start();
            Set("sound",game.gameObject.AddComponent<GameAudio>());
            game.sparkPrefab=Object("Item chain spark");game.sparkPrefab.AddComponent<SpriteRenderer>();
            game.Rules.GainWeaponStat("crit_chance",-1000);
        }
        [TearDown] public void Cleanup()
        {
            foreach(string list in new[]{"sparks","impacts","pickups","shots"})foreach(var effect in (IEnumerable)Get(list))
            {var obj=Field<GameObject>(effect,"obj");if(obj!=null)UnityEngine.Object.DestroyImmediate(obj);}
            foreach(var obj in objects)if(obj!=null)UnityEngine.Object.DestroyImmediate(obj);
            objects.Clear();UnityEngine.Random.state=previousRandom;
        }
        void Add(string id,int copies)
        {
            var item=game.catalog.data.items.Single(entry=>entry.id==id);
            // available records default unlock eligibility, not whether an
            // earned reward can ever enter a run. These are imported earnable
            // definitions; this fixture stages the post-unlock loadout directly.
            if(item.maxCount>0)Assert.LessOrEqual(copies,item.maxCount,"Fixture must respect the imported cap.");
            for(int i=0;i<copies;i++)game.Rules.GrantItem(item);
        }
        object Target()
        {
            var target=Activator.CreateInstance(typeof(ArenaGame).GetNestedType("Enemy",BindingFlags.NonPublic),true);
            Field(target,"hp",1000f);Field(target,"maxHp",1000f);Field(target,"pos",Vector2.zero);
            Field(target,"type",new EnemyEntry{id="chain-fixture",role="Enemy"});
            Field(target,"renderers",Array.Empty<Renderer>());((IList)Get("enemies")).Add(target);return target;
        }
        [TestCase("rip_and_tear","explode_on_death",5,false)]
        [TestCase("rip_and_tear","explode_on_death",5,true)]
        [TestCase("spicy_sauce","explode_on_consumable",2,false)]
        [TestCase("spicy_sauce","explode_on_consumable",2,true)]
        public void PlainItemExplosionsDoNotAcquireSausageBurnOrGreekFire(string item,string trigger,int copies,bool greekFire)
        {
            // Original handle_explode_effect forwards first_stats.burning_data
            // unchanged for ItemExplodingEffect. Only ItemExplodingAndBurnEffect
            // initializes burning. These two imported families are plain effects.
            Add(item,copies);Add("scared_sausage",4);if(greekFire)Add("greek_fire",1);
            Assert.IsTrue(game.Rules.TryItemExplosion(trigger,0,out _,out float damage));
            var target=Target();Call("QueueImplantExplosions",trigger,Vector2.zero);Call("ResolveImplantAttacks");
            Assert.AreEqual(1000-damage,Field<float>(target,"hp"),"A non-burning item explosion must not activate Greek Fire.");
            Assert.AreEqual(0,Field<int>(target,"burnTicks"),"A plain item explosion does not inherit global weapon burning.");
        }
        [Test] public void OrdinarySpecialProjectileStillReceivesSausageBurn()
        {
            // init_ranged_stats(..., true) DOES initialize burning for Beard.
            Add("baby_with_a_beard",1);Add("scared_sausage",4);
            var stats=game.Rules.Effects("projectiles_on_death").Single().projectile.AsEquipment();
            var target=Target();Call("Damage",0,1f,0f,0f,Vector2.up,stats,true,false,false);
            Assert.AreEqual(3,Field<int>(target,"burnTicks"));
            Assert.AreEqual(4,Field<float>(target,"burnDamage"));
        }
        [TestCase("rip_and_tear","explode_on_death",5)]
        [TestCase("spicy_sauce","explode_on_consumable",2)]
        public void PlainItemExplosionHasNoWeaponLifeStealCallback(string item,string trigger,int copies)
        {
            // These item explosions have PlayerExplosion as their hitbox owner;
            // handle_explode_effect never connects a weapon's hit callback.
            Add(item,copies);game.Rules.GainWeaponStat("lifesteal",100);game.Rules.Hurt(3);
            float before=game.Rules.Health;Target();
            Call("QueueImplantExplosions",trigger,Vector2.zero);Call("ResolveImplantAttacks");
            Assert.AreEqual(before,game.Rules.Health,"An item explosion must not heal or trigger Lich through weapon life steal.");
            Assert.IsTrue(game.Rules.TryLifeSteal(0),"The item explosion must not consume the weapon life-steal timer.");
        }
        [TestCase(14,false)]
        [TestCase(15,true)]
        public void FireworksCountsDirectExplosionKillsBeforeDeferredDeathsAndIgnoresZeroHpCandidates(int living,bool expected)
        {
            var definitions=JsonUtility.FromJson<UnlockCatalog>(Resources.Load<TextAsset>("base-unlocks").text).challenges;
            game.Rules.Progress=new Progression(definitions);Add("rip_and_tear",5);
            for(int i=0;i<living;i++)Field(Target(),"hp",1f);
            Field(Target(),"hp",0f); // Existing zero-HP candidate is not this explosion's kill.
            Call("QueueImplantExplosions","explode_on_death",Vector2.zero);Call("ResolveImplantAttacks");
            Assert.AreEqual(0,game.Rules.Kills,"Global die callbacks have not run yet.");
            Assert.AreEqual(living,((IEnumerable)Get("pendingEnemyDeaths")).Cast<object>().Count());
            Assert.AreEqual(expected,game.Rules.Progress.Completed("chal_fireworks"),"The source explosion's immediate killed_something count owns this unlock.");
        }
    }
}

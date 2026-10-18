using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Scrapshift.Editor;

namespace Scrapshift.Tests
{
    public sealed class EnemyRemainingParityTests
    {
        const BindingFlags Hidden=BindingFlags.Instance|BindingFlags.NonPublic;
        ArenaGame game;
        IList Enemies=>(IList)typeof(ArenaGame).GetField("enemies",Hidden).GetValue(game);
        [SetUp] public void Setup()
        {
            game=new GameObject("Enemy remaining parity test").AddComponent<ArenaGame>();
            game.catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
            game.Rules.Configure(game.catalog);game.Rules.Start();
        }
        [TearDown] public void Cleanup()=>UnityEngine.Object.DestroyImmediate(game.gameObject);
        object Enemy(Vector2 position,float health)
        {
            var enemy=Activator.CreateInstance(typeof(ArenaGame).GetNestedType("Enemy",BindingFlags.NonPublic),true);
            Set(enemy,"type",new EnemyEntry{id="baby_alien",role="Chaser"});Set(enemy,"pos",position);Set(enemy,"hp",health);Set(enemy,"maxHp",200f);Enemies.Add(enemy);return enemy;
        }
        static void Set(object obj,string field,object value)=>obj.GetType().GetField(field).SetValue(obj,value);
        static float HP(object enemy)=>(float)enemy.GetType().GetField("hp").GetValue(enemy);
        void Heal(object healer)=>typeof(ArenaGame).GetMethod("HealNearbyEnemies",Hidden).Invoke(game,new[]{healer});

        [Test] public void EnemyHealerZoneDoesNotHealPlayerOnExcludedCollisionLayer()
        {
            // healer.tscn BoostZone mask 4; player.tscn body layer 2.
            var healer=Enemy(Vector2.zero,200);game.Rules.Hurt(3);float before=game.Rules.Health;
            Heal(healer);Assert.AreEqual(before,game.Rules.Health);
        }
        [Test] public void EnemyHealingIsAnEntryEffectAndRequiresExitToHealAgain()
        {
            var healer=Enemy(Vector2.zero,200);var ally=Enemy(Vector2.right,20);
            Heal(healer);Assert.AreEqual(120,HP(ally));
            Set(ally,"hp",50f);Heal(healer);Assert.AreEqual(50,HP(ally));
            Set(ally,"pos",Vector2.right*5);Heal(healer);Assert.AreEqual(50,HP(ally));
            Set(ally,"pos",Vector2.right);Heal(healer);Assert.AreEqual(150,HP(ally));
        }
        [TestCase(5,-50,3)]
        [TestCase(3,50,5)]
        [TestCase(5,50,8)]
        [TestCase(5,-100,1)]
        [TestCase(5,0,5)]
        public void BossSummonsRoundHalfAwayFromZeroAndKeepOneMinimum(int count,float modifier,int expected)
        {
            // spawning_attack_behavior.gd: max(1, round(nb_to_spawn * (1 + effect/100))).
            Assert.AreEqual(expected,EnemyReferenceRules.SummonCount(count,modifier));
        }
    }
}

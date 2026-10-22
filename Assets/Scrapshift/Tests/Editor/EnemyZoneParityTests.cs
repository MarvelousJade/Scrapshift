using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Scrapshift.Editor;

namespace Scrapshift.Tests
{
    public sealed class EnemyZoneParityTests
    {
        const BindingFlags Hidden=BindingFlags.Instance|BindingFlags.NonPublic;
        ArenaGame game;
        IList Enemies=>(IList)typeof(ArenaGame).GetField("enemies",Hidden).GetValue(game);
        [SetUp] public void Setup()
        {
            game=new GameObject("Enemy body-zone parity test").AddComponent<ArenaGame>();
            game.catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
            game.Rules.Configure(game.catalog);game.Rules.Start();
        }
        [TearDown] public void Cleanup()=>UnityEngine.Object.DestroyImmediate(game.gameObject);
        static void Set(object obj,string field,object value)=>obj.GetType().GetField(field).SetValue(obj,value);
        static float Get(object obj,string field)=>(float)obj.GetType().GetField(field).GetValue(obj);
        object Enemy(string id,Vector2 position,float health=20)
        {
            var enemy=Activator.CreateInstance(typeof(ArenaGame).GetNestedType("Enemy",BindingFlags.NonPublic),true);
            Set(enemy,"type",new EnemyEntry{id=id,role=id=="tree"?"Tree":"Chaser"});Set(enemy,"pos",position);Set(enemy,"hp",health);Set(enemy,"maxHp",400f);Enemies.Add(enemy);return enemy;
        }
        void Heal(object healer)=>typeof(ArenaGame).GetMethod("HealNearbyEnemies",Hidden).Invoke(game,new[]{healer});

        [Test] public void BodyOverlapsHealerBeyondTheOldOriginOnlyRadius()
        {
            var healer=Enemy("healer",Vector2.zero,400);var ally=Enemy("baby_alien",new Vector2(2.1f,-.03f));
            Heal(healer);Assert.AreEqual(120,Get(ally,"hp"));
        }
        [Test] public void BodyMustFullyExitBeforeReentryHealsAgain()
        {
            var healer=Enemy("healer",Vector2.zero,400);var ally=Enemy("baby_alien",new Vector2(2.1f,-.03f));
            Heal(healer);Set(ally,"hp",50f);
            Set(ally,"pos",new Vector2(2.2f,-.03f));Heal(healer);Assert.AreEqual(50,Get(ally,"hp"),"Its origin left the zone, but its body is still inside.");
            Set(ally,"pos",new Vector2(2.23f,-.03f));Heal(healer);Assert.AreEqual(50,Get(ally,"hp"));
            Set(ally,"pos",new Vector2(2.2f,-.03f));Heal(healer);Assert.AreEqual(150,Get(ally,"hp"));
        }
        [Test] public void DifferentEnemyBodyOffsetsChangeTheActualOverlap()
        {
            var healer=Enemy("healer",Vector2.zero,400);
            var butcher=Enemy("butcher",Vector2.up*2.65f);var baby=Enemy("baby_alien",Vector2.up*2.65f);
            Heal(healer);Assert.AreEqual(120,Get(butcher,"hp"));Assert.AreEqual(20,Get(baby,"hp"));
            Assert.AreEqual(new Vector2(0,-.88f),CombatGeometry.BodyOffset("butcher"));
            Assert.AreEqual(new Vector2(0,-.24f),CombatGeometry.OriginOffset("healer"));
        }
        [Test] public void HealerUsesBodyRadiusRatherThanLargerHurtOrContactRadius()
        {
            var healer=Enemy("healer",Vector2.zero,400);var ally=Enemy("baby_alien",new Vector2(2.24f,-.03f));
            Heal(healer);Assert.AreEqual(20,Get(ally,"hp"),"2.00639+.22 is outside, although the .32 hurt circle overlaps.");
        }
        [Test] public void NeutralTreesAreExcludedByTheSourceBodyCollisionMask()
        {
            var healer=Enemy("healer",Vector2.zero,400);var tree=Enemy("tree",Vector2.zero);
            Heal(healer);Assert.AreEqual(20,Get(tree,"hp"));Assert.AreEqual(1,CombatGeometry.BodyLayer("tree"));
        }
        [Test] public void FullHealthEntryIsRememberedWithoutAFalseHealFlash()
        {
            var healer=Enemy("healer",Vector2.zero,400);var ally=Enemy("baby_alien",Vector2.right,400);
            Heal(healer);Assert.AreEqual(0,Get(ally,"supportFlash"));
            Set(ally,"hp",50f);Heal(healer);Assert.AreEqual(50,Get(ally,"hp"),"Taking damage inside does not synthesize another body entry.");
            Set(ally,"pos",Vector2.right*4);Heal(healer);Set(ally,"pos",Vector2.right);Heal(healer);
            Assert.AreEqual(150,Get(ally,"hp"));Assert.AreEqual(.2f,Get(ally,"supportFlash"));
        }
        [Test] public void MultipleHealersTrackTheSameBodyIndependently()
        {
            var first=Enemy("healer",Vector2.zero,400);var second=Enemy("healer",Vector2.left*.5f,400);var ally=Enemy("baby_alien",Vector2.right);
            Heal(first);Heal(second);Assert.AreEqual(220,Get(ally,"hp"));
            Set(ally,"hp",50f);Heal(first);Heal(second);Assert.AreEqual(50,Get(ally,"hp"));
        }
        [Test] public void DeadAndUnmaterializedBodiesCannotEnterTheHealZone()
        {
            var healer=Enemy("healer",Vector2.zero,400);var dead=Enemy("baby_alien",Vector2.right,0);var spawning=Enemy("baby_alien",Vector2.left);
            var marker=new GameObject("Spawn marker");Set(spawning,"marker",marker);
            try{Heal(healer);Assert.AreEqual(0,Get(dead,"hp"));Assert.AreEqual(20,Get(spawning,"hp"));}
            finally{UnityEngine.Object.DestroyImmediate(marker);}
        }
    }
}

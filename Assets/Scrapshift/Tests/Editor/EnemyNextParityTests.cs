using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Scrapshift.Editor;

namespace Scrapshift.Tests
{
    public sealed class EnemyNextParityTests
    {
        const BindingFlags Hidden=BindingFlags.Instance|BindingFlags.NonPublic;
        ArenaGame game;
        IList Enemies=>(IList)typeof(ArenaGame).GetField("enemies",Hidden).GetValue(game);
        [SetUp] public void Setup()
        {
            game=new GameObject("Buffer geometry parity").AddComponent<ArenaGame>();
            game.catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
            game.Rules.Configure(game.catalog);game.Rules.Start();
        }
        [TearDown] public void Cleanup(){UnityEngine.Object.DestroyImmediate(game.gameObject);ArenaGeometry.SetMapSize(0);}
        static void Set(object obj,string field,object value)=>obj.GetType().GetField(field).SetValue(obj,value);
        static float Get(object obj,string field)=>(float)obj.GetType().GetField(field).GetValue(obj);
        object Enemy(string id,Vector2 position,bool canBoost=true)
        {
            var enemy=Activator.CreateInstance(typeof(ArenaGame).GetNestedType("Enemy",BindingFlags.NonPublic),true);
            Set(enemy,"type",new EnemyEntry{id=id,role=id=="tree"?"Tree":"Chaser",health=19,speed=1.42f});
            Set(enemy,"pos",position);Set(enemy,"hp",5f);Set(enemy,"maxHp",23f);Set(enemy,"speed",1.42f);
            Set(enemy,"behavior",new EnemyBehavior{canBoost=canBoost});Enemies.Add(enemy);return enemy;
        }
        void Boost(object buffer,float damage=25)=>typeof(ArenaGame).GetMethod("BoostNearbyEntity",Hidden).Invoke(game,new[]{buffer,(object)damage});
        [TestCase("buffer")]
        [TestCase("corrupted_buffer")]
        public void ActualBufferUsesBodyOverlapBeyondTheOldArtOriginOnlyRadius(string id)
        {
            var buffer=Enemy(id,Vector2.zero,false);var ally=Enemy("baby_alien",new Vector2(3.2f,-.03f));
            Boost(buffer);Assert.IsTrue((bool)ally.GetType().GetField("boosted").GetValue(ally));
        }
        [TestCase("buffer")]
        [TestCase("corrupted_buffer")]
        public void ActualBufferCannotBoostTheNearbyPlayerOnAnExcludedBodyLayer(string id)
        {
            var buffer=Enemy(id,Vector2.zero,false);game.Rules.Hurt(3);float hp=game.Rules.Health;
            Boost(buffer);Assert.AreEqual(hp,game.Rules.Health);Assert.AreEqual(10,game.Rules.MaxHealth);
            Assert.AreEqual(0,game.Rules.Stat("speed"));
        }
        [Test] public void BufferUsesPhysicalBodyRadiusOffsetsAndLayerRatherThanTheLargerHurtbox()
        {
            Assert.IsFalse(CombatGeometry.BufferZoneOverlaps("buffer",Vector2.zero,"baby_alien",new Vector2(3.32f,-.03f)));
            Assert.IsTrue(CombatGeometry.BufferZoneOverlaps("buffer",Vector2.zero,"butcher",Vector2.up*3.9f));
            Assert.IsFalse(CombatGeometry.BufferZoneOverlaps("buffer",Vector2.zero,"baby_alien",Vector2.up*3.9f));
            Assert.IsFalse(CombatGeometry.BufferZoneOverlaps("buffer",Vector2.zero,"tree",Vector2.zero));
        }
        [TestCase(25)]
        [TestCase(30)]
        public void ActualBoostRecalculatesRawHealthThenRoundsAndDoesNotCompoundAnExistingBoost(float damage)
        {
            var buffer=Enemy("buffer",Vector2.zero,false);var ally=Enemy("baby_alien",Vector2.right);
            game.Rules.GainWeaponStat("enemy_health",20);Boost(buffer,damage);
            Assert.AreEqual(57,Get(ally,"maxHp"),"round(19 * 1.2 * 2.5), not 23 * 2.5.");
            Assert.AreEqual(57,Get(ally,"hp"));Assert.AreEqual(2.13f,Get(ally,"speed"),.0001f);
            Assert.AreEqual(damage,Get(ally,"boostDamage"));
            Set(ally,"hp",20f);Boost(buffer,damage);Assert.AreEqual(20,Get(ally,"hp"));Assert.AreEqual(2.13f,Get(ally,"speed"),.0001f);
        }
        [Test] public void DeadUnmaterializedAndUnboostableBodiesRemainIneligible()
        {
            var buffer=Enemy("buffer",Vector2.zero,false);var dead=Enemy("baby_alien",Vector2.right);Set(dead,"hp",0f);
            var disabled=Enemy("baby_alien",Vector2.left,false);var spawning=Enemy("baby_alien",Vector2.up);
            var marker=new GameObject("Buffer spawn marker");Set(spawning,"marker",marker);
            try{Boost(buffer);Assert.AreEqual(0,Get(dead,"hp"));Assert.AreEqual(5,Get(disabled,"hp"));Assert.AreEqual(5,Get(spawning,"hp"));}
            finally{UnityEngine.Object.DestroyImmediate(marker);}
        }
        [Test] public void ChargeAccessibilityBonusTruncatesPixelsInsteadOfRoundingOrKeepingFractions()
        {
            Assert.AreEqual(5.62f,EnemyReferenceRules.AdditiveSpeedBonus(7.5f,.75f),.00001f);
            Assert.AreEqual(5.63f,EnemyReferenceRules.ScaledSpeed(7.5f,.75f),.00001f,"Current-stat rounding is a different source operation.");
        }
        [Test] public void PursuerAccessibilityBonusTruncatesEachIncrementBeforeAccumulation()
        {
            float increment=EnemyReferenceRules.AdditiveSpeedBonus(.45f,.5f);
            Assert.AreEqual(.22f,increment,.00001f);Assert.AreEqual(2.2f,10*increment,.00001f);
        }
        [Test] public void SourceRandomPositionKeepsItsDefaultEdgeMarginAndHasNoPlayerExclusion()
        {
            ArenaGeometry.SetMapSize(0);
            Assert.That(SpawnPlacement.SourceRandomPosition(0,0),Is.EqualTo(new Vector2(-9.6f,7.04f)).Using(Vector2Comparer.Instance));
            Assert.That(SpawnPlacement.SourceRandomPosition(1,1),Is.EqualTo(new Vector2(9.6f,-7.04f)).Using(Vector2Comparer.Instance));
            Assert.AreEqual(Vector2.zero,SpawnPlacement.SourceRandomPosition(.5f,.5f),"The center remains a valid source sample even with a player there.");
        }
        [Test] public void SourceAreaClampsTheCenterBeforeSamplingRatherThanClampingEachResult()
        {
            ArenaGeometry.SetMapSize(0);
            Vector2 edge=SpawnPlacement.Bounds;
            Assert.That(SpawnPlacement.SourceAreaPosition(edge,2,0,0),Is.EqualTo(new Vector2(7.6f,7.04f)).Using(Vector2Comparer.Instance));
            Assert.That(SpawnPlacement.SourceAreaPosition(edge,2,1,1),Is.EqualTo(new Vector2(9.6f,5.04f)).Using(Vector2Comparer.Instance));
            Assert.That(SpawnPlacement.SourceAreaPosition(Vector2.zero,2,.25f,.75f),Is.EqualTo(new Vector2(-.5f,-.5f)).Using(Vector2Comparer.Instance));
        }
        sealed class Vector2Comparer:System.Collections.Generic.IEqualityComparer<Vector2>
        {
            public static readonly Vector2Comparer Instance=new Vector2Comparer();
            public bool Equals(Vector2 a,Vector2 b)=>(a-b).sqrMagnitude<.000001f;
            public int GetHashCode(Vector2 value)=>value.GetHashCode();
        }
    }
}

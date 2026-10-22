using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Scrapshift.Editor;

namespace Scrapshift.Tests
{
    public sealed class BeamRemainingParityTests
    {
        const BindingFlags Hidden=BindingFlags.Instance|BindingFlags.NonPublic;
        ArenaGame game;
        UnityEngine.Random.State previousRandom;
        readonly List<GameObject> objects=new List<GameObject>();
        ProjectileReference Profile=>WeaponReference.Projectile(game.catalog.Weapon("particle_accelerator",3));
        object Get(string name)=>typeof(ArenaGame).GetField(name,Hidden).GetValue(game);
        object Call(string name,params object[] args)=>typeof(ArenaGame).GetMethod(name,Hidden).Invoke(game,args);
        static void Set(object obj,string name,object value)=>obj.GetType().GetField(name).SetValue(obj,value);
        static T Read<T>(object obj,string name)=>(T)obj.GetType().GetField(name).GetValue(obj);
        GameObject Object(string name)
        {var obj=new GameObject(name);objects.Add(obj);return obj;}
        [SetUp] public void Setup()
        {
            previousRandom=UnityEngine.Random.state;
            game=Object("Beam parity fixture").AddComponent<ArenaGame>();
            game.catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
            game.Rules.Configure(game.catalog);game.Rules.Start();
            typeof(ArenaGame).GetField("sound",Hidden).SetValue(game,game.gameObject.AddComponent<GameAudio>());
            game.sparkPrefab=Object("Beam parity spark");game.sparkPrefab.AddComponent<SpriteRenderer>();
        }
        [TearDown] public void Cleanup()
        {
            foreach(string list in new[]{"sparks","impacts"})foreach(var effect in (IEnumerable)Get(list))
            {var obj=Read<GameObject>(effect,"obj");if(obj!=null)UnityEngine.Object.DestroyImmediate(obj);}
            foreach(var obj in objects)if(obj!=null)UnityEngine.Object.DestroyImmediate(obj);
            objects.Clear();UnityEngine.Random.state=previousRandom;
        }
        [TestCase(.025f,.30624977f)]
        [TestCase(.05f,.612499375f)]
        [TestCase(.075f,.88749930f)]
        [TestCase(.098f,1.00007110f)]
        [TestCase(.125f,.86015828f)]
        [TestCase(.15f,.556255f)]
        [TestCase(.199f,.00520350f)]
        public void ScaleUsesSourceCubicKeysAndDuplicatedBoundaryNeighbors(float age,float expected)
        {
            Assert.That(ReferenceBeam.Thickness(age),Is.EqualTo(expected).Within(.000002f));
            Assert.AreEqual(.1f,ReferenceBeam.Thickness(0));Assert.AreEqual(1,ReferenceBeam.Thickness(.1f));
            Assert.AreEqual(.00001f,ReferenceBeam.Thickness(.2f));
        }
        float Hit(Vector2 point,float radius,float age=.1f,float angle=0)
            =>ReferenceBeam.HitFraction(Profile,2,1,age,Vector2.zero,angle,point,radius);
        [Test] public void RectangleHasFlatEndsAndCorrectRoundedCircleCornerContact()
        {
            // Source x interval [0,4], y interval [-.23,.22] at peak scale.
            Assert.IsTrue(float.IsPositiveInfinity(Hit(new Vector2(-.1f,0),.05f)));
            Assert.IsTrue(float.IsPositiveInfinity(Hit(new Vector2(4.1f,0),.05f)));
            Assert.IsTrue(float.IsPositiveInfinity(Hit(new Vector2(-.04f,.26f),.05f)));
            Assert.IsFalse(float.IsPositiveInfinity(Hit(new Vector2(-.03f,.25f),.05f)));
            Assert.IsFalse(float.IsPositiveInfinity(Hit(new Vector2(2,.21f),.01f)));
        }
        [Test] public void GrowthShrinkAndStopChangeTheActualHitWindow()
        {
            var point=new Vector2(2,.14f);
            Assert.IsTrue(float.IsPositiveInfinity(Hit(point,.01f,0)));
            Assert.IsFalse(float.IsPositiveInfinity(Hit(point,.01f,.1f)));
            Assert.IsTrue(float.IsPositiveInfinity(Hit(point,.01f,.175f)));
            Assert.IsTrue(float.IsPositiveInfinity(Hit(Vector2.right,.1f,.2f)));
            Assert.IsTrue(float.IsPositiveInfinity(Hit(Vector2.right,.1f,.3f)));
            Assert.IsFalse(float.IsPositiveInfinity(Hit(new Vector2(-.14f,2),.01f,.1f,90)));
        }
        [Test] public void IntervalQueryIncludesTheCubicPeakButCannotReviveAnExpiredBeam()
        {
            Assert.Greater(ReferenceBeam.MaximumThickness(.08f,.12f),1,"Source cubic overshoot must not be clamped.");
            var point=new Vector2(2,.22f);
            Assert.IsTrue(float.IsPositiveInfinity(Hit(point,.001f,.08f)));
            Assert.IsTrue(float.IsPositiveInfinity(Hit(point,.001f,.12f)));
            Assert.IsFalse(float.IsPositiveInfinity(ReferenceBeam.IntervalHitFraction(Profile,2,1,.08f,.12f,Vector2.zero,0,point,.001f)));
            Assert.IsTrue(float.IsPositiveInfinity(ReferenceBeam.IntervalHitFraction(Profile,2,1,.2f,.3f,Vector2.zero,0,point,.1f)));
        }
        object Shot()
        {
            var shot=Activator.CreateInstance(typeof(ArenaGame).GetNestedType("Shot",BindingFlags.NonPublic),true);
            var obj=Object("Beam parity projectile");obj.AddComponent<SpriteRenderer>();
            Set(shot,"obj",obj);Set(shot,"weapon",game.catalog.Weapon("particle_accelerator",3));
            Set(shot,"velocity",Vector2.right*10);Set(shot,"damage",3f);Set(shot,"pierce",99);
            Call("ConfigureReferenceShot",shot,false);((IList)Get("shots")).Add(shot);return shot;
        }
        object Enemy(Vector2 center)
        {
            var enemy=Activator.CreateInstance(typeof(ArenaGame).GetNestedType("Enemy",BindingFlags.NonPublic),true);
            Set(enemy,"type",game.catalog.Enemy("baby_alien"));
            Set(enemy,"pos",center-CombatGeometry.HurtOffset("baby_alien"));Set(enemy,"hp",100f);Set(enemy,"maxHp",100f);
            Set(enemy,"renderers",Array.Empty<Renderer>());((IList)Get("enemies")).Add(enemy);return enemy;
        }
        [Test] public void ActualWeaponUpdateWaitsForGrowthAndHitsEachTargetOnlyOnce()
        {
            Shot();float radius=CombatGeometry.Enemy("baby_alien").hurt;
            var enemy=Enemy(new Vector2(2,radius+.12f));
            Call("UpdateWeapons",.01f);Assert.AreEqual(100,Read<float>(enemy,"hp"));
            Call("UpdateWeapons",.09f);Assert.AreEqual(97,Read<float>(enemy,"hp"));
            Call("UpdateWeapons",.01f);Assert.AreEqual(97,Read<float>(enemy,"hp"));
        }
        [Test] public void ActualCollisionKeepsShotRangeAndRendererUsesTheSameCubicScale()
        {
            var shot=Shot();var view=Read<WeaponProjectileView>(shot,"projectileView");
            float range=view.CapturedRange;Assert.AreEqual(1,view.BeamWidth);
            view.Pose(.05f);var sprite=view.transform.Find("Reference projectile");
            Assert.That(sprite.localScale.y,Is.EqualTo(.612499375f).Within(.000002f));
            Assert.That(sprite.localPosition.y,Is.EqualTo(-.005f*.612499375f).Within(.000001f));
            Assert.That(view.transform.Find("Beam start").localScale.y,Is.EqualTo(sprite.localScale.y));
            game.Rules.GainWeaponStat("range",500);Assert.Greater(game.Rules.WeaponRange(Read<Equipment>(shot,"weapon")),range);
            float radius=CombatGeometry.Enemy("baby_alien").hurt;
            var enemy=Enemy(new Vector2(range+2+radius+.1f,0));Set(shot,"age",.1f);
            Assert.IsTrue(float.IsPositiveInfinity((float)Call("ReferenceShotHitFraction",shot,enemy,Vector2.zero,.1f)),"A later range bonus cannot stretch an already emitted beam.");
        }
    }
}

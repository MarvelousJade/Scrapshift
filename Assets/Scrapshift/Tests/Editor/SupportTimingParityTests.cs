using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Scrapshift.Editor;

namespace Scrapshift.Tests
{
    public sealed class SupportTimingParityTests
    {
        const BindingFlags Hidden=BindingFlags.Instance|BindingFlags.NonPublic;
        ArenaGame game;GameObject turretView;UnityEngine.Random.State randomState;
        IList List(string field)=>(IList)typeof(ArenaGame).GetField(field,Hidden).GetValue(game);
        static void Set(object obj,string field,object value)=>obj.GetType().GetField(field).SetValue(obj,value);
        static T Get<T>(object obj,string field)=>(T)obj.GetType().GetField(field).GetValue(obj);
        object Call(string method,params object[] args)=>typeof(ArenaGame).GetMethod(method,Hidden).Invoke(game,args);
        [SetUp] public void Setup()
        {
            randomState=UnityEngine.Random.state;
            game=new GameObject("Support animation callback checks").AddComponent<ArenaGame>();
            game.catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
            game.Rules.Configure(game.catalog);game.Rules.Start();
            typeof(ArenaGame).GetField("sound",Hidden).SetValue(game,game.gameObject.AddComponent<GameAudio>());
        }
        [TearDown] public void Cleanup()
        {
            foreach(object shot in List("shots"))UnityEngine.Object.DestroyImmediate(Get<GameObject>(shot,"obj"));
            if(turretView!=null)UnityEngine.Object.DestroyImmediate(turretView);
            UnityEngine.Object.DestroyImmediate(game.gameObject);UnityEngine.Random.state=randomState;
        }
        object Enemy(Vector2 rawOrigin)
        {
            var e=Activator.CreateInstance(typeof(ArenaGame).GetNestedType("Enemy",BindingFlags.NonPublic),true);
            Set(e,"type",new EnemyEntry{id="baby_alien",role="Chaser"});
            Set(e,"pos",rawOrigin-CombatGeometry.OriginOffset("baby_alien"));Set(e,"hp",100f);List("enemies").Add(e);return e;
        }
        object Turret()
        {
            var turret=Activator.CreateInstance(typeof(ArenaGame).GetNestedType("StarterTurret",BindingFlags.NonPublic),true);
            var spec=new StructureSpec{kind="turret",stats=new ProjectileStats{range=3,interval=1,damage=1,scaling=Array.Empty<StatEffect>()}};
            turretView=new GameObject("Turret body");turretView.AddComponent<GroundShadow>();
            Set(turret,"obj",turretView);Set(turret,"spec",spec);Set(turret,"stats",spec.Equipment);Set(turret,"baseScale",1f);return turret;
        }
        [Test] public void TurretDoesNotStartWindupWithoutAnEligibleTarget()
        {
            var turret=Turret();Call("TickStructure",turret,.01f);
            Assert.IsFalse(Get<bool>(turret,"firing"));Assert.AreEqual(0,List("shots").Count);
        }
        [Test] public void TargetLossDuringWindupStillEmitsTheFirstShotToTheRight()
        {
            var turret=Turret();Enemy(Vector2.up);
            Call("TickStructure",turret,.01f);Assert.IsTrue(Get<bool>(turret,"firing"));
            List("enemies").Clear();Call("TickStructure",turret,.201f);
            Assert.AreEqual(1,List("shots").Count,"turret.gd shoot() does not return after invalid-target cooldown handling.");
            var velocity=Get<Vector2>(List("shots")[0],"velocity");Assert.That(velocity.x,Is.GreaterThan(0));Assert.That(velocity.y,Is.EqualTo(0).Within(.0001));
        }
        [Test] public void InvalidTargetCallbackRetainsThePreviousFiringDirection()
        {
            var turret=Turret();Set(turret,"angle",Mathf.PI/2);Set(turret,"firing",true);Set(turret,"clock",0f);
            Call("TickStructure",turret,.01f);
            var velocity=Get<Vector2>(List("shots")[0],"velocity");Assert.That(velocity.y,Is.GreaterThan(0));Assert.That(velocity.x,Is.EqualTo(0).Within(.0001));
        }
        [Test] public void TurretWaitsForTheAnimationShootKeyBeforeEmitting()
        {
            var turret=Turret();Enemy(Vector2.right);
            Call("TickStructure",turret,.01f);Call("TickStructure",turret,.1f);
            Assert.AreEqual(0,List("shots").Count);Call("TickStructure",turret,.101f);Assert.AreEqual(1,List("shots").Count);
        }
        [Test] public void SourceNearestTargetStrictlyExcludesTheIdenticalRawOrigin()
        {
            var colocated=Enemy(Vector2.zero);var candidate=Enemy(Vector2.right);
            Assert.AreSame(candidate,Call("StructureTarget",Vector2.zero,3f,true));
            List("enemies").Remove(candidate);Assert.IsNull(Call("StructureTarget",Vector2.zero,3f,true));
        }
        [Test] public void BodyOverlapKeepsAnOutsideCenterInTheTargetPoolWithoutStartingWindup()
        {
            var turret=Turret();Vector2 origin=SupportBehavior.StructureOriginOffset("turret");
            var e=Enemy(origin+Vector2.right*3.1f);
            Assert.AreSame(e,Call("StructureTarget",origin,3f,true));
            Call("TickStructure",turret,.01f);Assert.IsFalse(Get<bool>(turret,"firing"));Assert.AreEqual(0,List("shots").Count);
        }
        [Test] public void StartedAnimationCanAimAtABodyWhoseCenterLeftFiringReach()
        {
            var turret=Turret();Vector2 origin=SupportBehavior.StructureOriginOffset("turret");Enemy(origin+Vector2.right*3.1f);
            Set(turret,"firing",true);Set(turret,"angle",Mathf.PI/2);Call("TickStructure",turret,.01f);
            var velocity=Get<Vector2>(List("shots")[0],"velocity");Assert.That(velocity.x,Is.GreaterThan(0));Assert.That(velocity.y,Is.EqualTo(0).Within(.0001));
        }
        [Test] public void SourceIntegerPixelDistanceRetainsTheFirstTargetWhenSquaredDistancesTruncateEqual()
        {
            var first=Enemy(Vector2.right*1.00001f);Enemy(Vector2.right);
            Assert.AreSame(first,Call("StructureTarget",Vector2.zero,3f,true));
        }
        [TestCase(3.009f,true)][TestCase(3.011f,false)]
        public void SourceFiringGateTruncatesDistanceToIntegerPixels(float distance,bool expected)
            =>Assert.AreEqual(expected,SupportBehavior.StructureFiringReach(Vector2.right*distance,3));
        [Test] public void SubpixelSquaredDistanceTruncatesToZeroAndIsExcluded()
        {
            Enemy(Vector2.right*.005f);Assert.IsNull(Call("StructureTarget",Vector2.zero,3f,true));
        }
    }
}

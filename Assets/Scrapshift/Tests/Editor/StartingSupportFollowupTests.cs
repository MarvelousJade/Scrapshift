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
    public sealed class StartingSupportFollowupTests
    {
        const BindingFlags Hidden=BindingFlags.Instance|BindingFlags.NonPublic;
        ArenaGame game;object spawner;UnityEngine.Random.State randomState;
        IList Mines=>(IList)typeof(ArenaGame).GetField("mines",Hidden).GetValue(game);
        object Call(string method,params object[] args)=>typeof(ArenaGame).GetMethod(method,Hidden).Invoke(game,args);
        static void Set(object obj,string name,object value)=>obj.GetType().GetField(name).SetValue(obj,value);
        static T Get<T>(object obj,string name)=>(T)obj.GetType().GetField(name).GetValue(obj);
        [SetUp] public void Setup()
        {
            randomState=UnityEngine.Random.state;game=new GameObject("Starting support source check").AddComponent<ArenaGame>();
            game.catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
            game.Rules.Configure(game.catalog);game.Rules.Start();
            var spec=game.catalog.data.items.Single(i=>i.id=="landmines").conditionalEffects.Single(e=>e.customKey=="structure").structure;
            spawner=Activator.CreateInstance(typeof(ArenaGame).GetNestedType("StarterTurret",BindingFlags.NonPublic),true);
            Set(spawner,"stats",spec.Equipment);Set(spawner,"trait",new WeaponTrait{radius=spec.explosionRadius});
        }
        [TearDown] public void Cleanup()
        {
            foreach(var mine in Mines)UnityEngine.Object.DestroyImmediate(Get<GameObject>(mine,"obj"));
            UnityEngine.Object.DestroyImmediate(game.gameObject);UnityEngine.Random.state=randomState;
        }
        [Test] public void TimedUngroupedMinesUseSourceGlobalInsetAndRollOrientation()
        {
            typeof(ArenaGame).GetField("playerPos",Hidden).SetValue(game,new Vector2(4,2));
            game.Rules.GainWeaponStat("group_structures",1);
            bool outsideGroup=false;
            for(int seed=0;seed<64;seed++)
            {
                UnityEngine.Random.InitState(seed);var saved=UnityEngine.Random.state;
                var expected=SpawnPlacement.SourceRandomPosition(UnityEngine.Random.value,UnityEngine.Random.value,1.6f);
                UnityEngine.Random.state=saved;Call("SpawnMine",spawner,null,null);
                var actual=Get<Vector2>(Mines[Mines.Count-1],"pos");
                Assert.That(actual.x,Is.EqualTo(expected.x).Within(.00001f));Assert.That(actual.y,Is.EqualTo(expected.y).Within(.00001f));
                var bound=SpawnPlacement.SourceRandomPosition(1,0,1.6f);
                Assert.LessOrEqual(Mathf.Abs(actual.x),Mathf.Abs(bound.x)+.00001f);Assert.LessOrEqual(Mathf.Abs(actual.y),Mathf.Abs(bound.y)+.00001f);
                outsideGroup|=Vector2.Distance(actual,new Vector2(-4,-2))>3;
            }
            Assert.IsTrue(outsideGroup,"group_structures does not group a landmine's source StructureEffect.");
        }
        [Test] public void InitialMineUsesTheAlreadySelectedDeploymentPointExactly()
        {
            var expected=new Vector2(-2.25f,1.75f);
            Call("SpawnMineAt",spawner,null,expected);
            Assert.AreEqual(expected,Get<Vector2>(Mines[0],"pos"),"An already selected source spawn point must not be replaced or jittered again.");
        }
        [Test] public void ExplicitPetLocalMineAreaKeepsItsSeparateRoute()
        {
            var center=new Vector2(2,1);UnityEngine.Random.InitState(717);var saved=UnityEngine.Random.state;
            var expected=RunRules.ClampPosition(center+new Vector2(UnityEngine.Random.Range(-.5f,.5f),UnityEngine.Random.Range(-.5f,.5f)));
            UnityEngine.Random.state=saved;Call("SpawnMine",spawner,null,(Vector2?)center);
            Assert.AreEqual(expected,Get<Vector2>(Mines[0],"pos"));
        }
    }
}

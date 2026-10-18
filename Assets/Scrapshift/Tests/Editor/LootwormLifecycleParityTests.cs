using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Scrapshift.Editor;

namespace Scrapshift.Tests
{
    public sealed class LootwormLifecycleParityTests
    {
        const BindingFlags Hidden=BindingFlags.Instance|BindingFlags.NonPublic;
        ArenaGame game;
        UnityEngine.Random.State randomState;
        IList Pickups=>(IList)typeof(ArenaGame).GetField("pickups",Hidden).GetValue(game);
        [SetUp] public void Setup()
        {
            randomState=UnityEngine.Random.state;
            game=new GameObject("Lootworm lifecycle parity test").AddComponent<ArenaGame>();
            game.catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
            game.Rules.Configure(game.catalog);game.Rules.Start();
        }
        [TearDown] public void Cleanup()
        {
            foreach(object pickup in Pickups)
                UnityEngine.Object.DestroyImmediate((GameObject)pickup.GetType().GetField("obj").GetValue(pickup));
            UnityEngine.Object.DestroyImmediate(game.gameObject);UnityEngine.Random.state=randomState;
        }
        SupportBehavior.TargetCache<object> AddCachedPet(string kind,object target)
        {
            var pet=Activator.CreateInstance(typeof(ArenaGame).GetNestedType("Pet",BindingFlags.NonPublic),true);
            pet.GetType().GetField("spec").SetValue(pet,new PetSpec{kind=kind});
            var cache=(SupportBehavior.TargetCache<object>)pet.GetType().GetField("lootwormTarget").GetValue(pet);
            cache.Resolve(()=>target,_=>true);
            ((IList)typeof(ArenaGame).GetField("pets",Hidden).GetValue(game)).Add(pet);return cache;
        }
        void Drop(int value,bool fruit=false)=>typeof(ArenaGame).GetMethod("Drop",Hidden).Invoke(game,new object[]{Vector2.zero,value,fruit});

        [TestCase(0)] [TestCase(1)]
        public void MaterialSpawnEventInvalidatesEveryLootwormEvenForAnEmptyBatch(int count)
        {
            var old=new object();var next=new object();
            var first=AddCachedPet("lootworm",old);var second=AddCachedPet("lootworm",old);var other=AddCachedPet("catling_gun",old);
            Drop(count);
            Assert.AreSame(next,first.Resolve(()=>next,_=>true));Assert.AreSame(next,second.Resolve(()=>next,_=>true));
            Assert.AreSame(old,other.Resolve(()=>next,_=>true));
        }
        [Test] public void MergeOnlyMaterialBatchStillInvalidatesTheCachedTarget()
        {
            Drop(50);Assert.AreEqual(50,Pickups.Count);
            var cache=AddCachedPet("lootworm",new object());var next=new object();
            Drop(1);Assert.AreEqual(50,Pickups.Count);
            Assert.AreSame(next,cache.Resolve(()=>next,_=>true));
        }
        [Test] public void FruitSpawnDoesNotEmitTheGoldTargetInvalidation()
        {
            var original=new object();var cache=AddCachedPet("lootworm",original);
            Drop(1,true);Assert.AreEqual(1,Pickups.Count);
            Assert.AreSame(original,cache.Resolve(()=>new object(),_=>true));
        }
    }
}

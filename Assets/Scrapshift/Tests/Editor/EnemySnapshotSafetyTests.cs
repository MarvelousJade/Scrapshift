using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Scrapshift.Tests
{
    public sealed class EnemySnapshotSafetyTests
    {
        const BindingFlags Hidden=BindingFlags.Instance|BindingFlags.NonPublic;
        ArenaGame game;GameObject owner;
        object Get(string name)=>typeof(ArenaGame).GetField(name,Hidden).GetValue(game);
        object Call(string name,params object[] args)=>typeof(ArenaGame).GetMethod(name,Hidden).Invoke(game,args);
        IList Enemies=>(IList)Get("enemies");
        [SetUp] public void Setup()
        {
            owner=new GameObject("Enemy traversal safety");game=owner.AddComponent<ArenaGame>();
            game.catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>("Assets/Scrapshift/Content/Definitions/ReferenceCatalog.asset");
            game.Rules.Configure(game.catalog);
        }
        [TearDown] public void Cleanup()=>UnityEngine.Object.DestroyImmediate(owner);
        object Enemy(float hp=0)
        {
            var enemy=Activator.CreateInstance(typeof(ArenaGame).GetNestedType("Enemy",BindingFlags.NonPublic),true);
            enemy.GetType().GetField("hp").SetValue(enemy,hp);Enemies.Add(enemy);return enemy;
        }
        [Test] public void SnapshotPreservesIdentitiesAcrossRemovalAndDoesNotIncludeLaterSpawns()
        {
            object first=Enemy(),second=Enemy(),third=Enemy();
            var snapshot=(IList)Call("RentEnemySnapshot");
            Enemies.RemoveAt(0);Enemies.Remove(third);object later=Enemy();
            CollectionAssert.AreEqual(new[]{first,second,third},snapshot);
            Assert.AreSame(third,snapshot[snapshot.Count-1],"Traversal reads the original last identity first, even after removal.");
            Assert.False(snapshot.Contains(later));
            Call("ReturnEnemySnapshot",snapshot);Assert.AreEqual(0,snapshot.Count,"Released buffers must not retain dead entities.");
        }
        [Test] public void NestedSnapshotCannotOverwriteAnActiveOuterTraversal()
        {
            object original=Enemy();var outer=(IList)Call("RentEnemySnapshot");
            Enemies.Clear();object nested=Enemy();var inner=(IList)Call("RentEnemySnapshot");
            Assert.AreNotSame(outer,inner);CollectionAssert.AreEqual(new[]{original},outer);CollectionAssert.AreEqual(new[]{nested},inner);
            Call("ReturnEnemySnapshot",inner);Assert.AreSame(original,outer[0]);
            Call("ReturnEnemySnapshot",outer);
            var reused=(IList)Call("RentEnemySnapshot");Assert.AreSame(outer,reused);CollectionAssert.AreEqual(new[]{nested},reused);
            Call("ReturnEnemySnapshot",reused);
        }
        [Test] public void NonPlayingEarlyReturnReleasesItsSnapshot()
        {
            Enemy();game.Rules.ReturnToTitle();Call("UpdateEnemies",1f/60);
            var pool=(IEnumerable)Get("enemySnapshotPool");int count=0;
            foreach(IList snapshot in pool){count++;Assert.AreEqual(0,snapshot.Count);}
            Assert.AreEqual(1,count);
        }
        [Test] public void FailedEnemyUpdateStillReleasesItsSnapshot()
        {
            game.Rules.Start();Enemy(1); // Incomplete enemy intentionally fails after entering the traversal.
            Assert.Throws<TargetInvocationException>(()=>Call("UpdateEnemies",1f/60));
            var pool=(IEnumerable)Get("enemySnapshotPool");int count=0;
            foreach(IList snapshot in pool){count++;Assert.AreEqual(0,snapshot.Count);}
            Assert.AreEqual(1,count,"Exception cleanup must release the buffer just as terminal-phase returns do.");
        }
    }
}

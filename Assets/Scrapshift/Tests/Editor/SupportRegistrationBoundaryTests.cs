using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Scrapshift.Editor;

namespace Scrapshift.Tests
{
    public sealed class SupportRegistrationBoundaryTests
    {
        ReferenceCatalog catalog;
        [SetUp] public void Setup()=>catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
        ItemEntry Item(string id)=>catalog.data.items.Single(i=>i.id==id);

        [Test] public void TurretUnlockCountsRegistrationsBeforeDeploymentAndExcludesOtherStructures()
        {
            var definitions=JsonUtility.FromJson<UnlockCatalog>(Resources.Load<TextAsset>("base-unlocks").text).challenges;
            var progress=new Progression(definitions);var run=new RunRules(2022){Progress=progress};run.Configure(catalog);run.Start();run.Inventory.Clear();
            foreach(string id in new[]{"pocket_factory","garden","landmines","wandering_bot"})run.Items.Add(Item(id));
            run.Inventory.Add(catalog.Weapon("screwdriver",1));run.Inventory.Add(catalog.Weapon("pruner",1));
            Assert.AreEqual(0,run.RegisteredTurretCount);
            run.CheckStructureDeploymentUnlock();Assert.IsFalse(progress.Completed("chal_turrets"));
            for(int i=0;i<4;i++)run.Items.Add(Item("turret"));
            run.CheckStructureDeploymentUnlock();Assert.IsFalse(progress.Completed("chal_turrets"));
            run.Inventory.Add(catalog.Weapon("wrench",1));
            Assert.AreEqual(5,run.RegisteredTurretCount);
            run.CheckStructureDeploymentUnlock();Assert.IsTrue(progress.Completed("chal_turrets"));
        }

        [Test] public void SpawnAreaUsesRegisteredEffectsAndClampsTheAreaBeforeSampling()
        {
            var run=new RunRules(2023);run.Configure(catalog);run.Start();run.Inventory.Clear();
            run.Items.Add(Item("pocket_factory"));run.Items.Add(Item("bot_o_mine"));
            Assert.AreEqual(2,run.OwnedStructures);Assert.AreEqual(4,SupportBehavior.GroupedSpawnSpan(run.RegisteredStructureCount),.0001f);
            run.Inventory.Add(catalog.Weapon("wrench",1));
            Assert.AreEqual(4.1f,SupportBehavior.GroupedSpawnSpan(run.RegisteredStructureCount),.0001f);
            Assert.AreEqual(6,SupportBehavior.GroupedSpawnSpan(50),.0001f);
            var center=SpawnPlacement.SourceAreaPosition(ArenaGeometry.HalfSize,2,.5f,.5f);
            Assert.AreEqual(ArenaGeometry.HalfSize.x-.64f,center.x+1,.0001f);
            Assert.AreEqual(ArenaGeometry.HalfSize.y-.64f,center.y+1,.0001f);
            var ungrouped=SpawnPlacement.SourceRandomPosition(1,0,1.6f);
            Assert.AreEqual(ArenaGeometry.HalfSize.x-1.6f,ungrouped.x,.0001f);
            Assert.IsFalse(SupportBehavior.CanGroupStructure("landmine"));
            Assert.IsTrue(SupportBehavior.CanGroupStructure("garden"));
            Assert.IsTrue(SupportBehavior.CanGroupStructure("wandering_bot")); // source player-area override takes precedence
        }
    }
}

using System.Linq;
using NUnit.Framework;
using UnityEditor;
using Scrapshift.Editor;

namespace Scrapshift.Tests
{
    public sealed class WeaponStructureRegistrationTests
    {
        ReferenceCatalog catalog;
        [SetUp] public void Setup()=>catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");

        [Test] public void TechnomageCountsSelectedWrenchAlongsideTwoGrantedTurrets()
        {
            // Source add_weapon applies Wrench's TurretEffect; StructureEffect
            // registers once. get_nb_structures is 3, so the link grants +6.
            var character=catalog.data.characters.Single(c=>c.id=="technomage");
            var run=new RunRules(2020);run.Configure(catalog);run.Start(character,"wrench",30);
            Assert.AreEqual(2,run.Items.Count(i=>i.id=="turret"));
            Assert.AreEqual(1,run.Inventory.Count);
            Assert.AreEqual(3,run.RegisteredStructureCount);
            Assert.AreEqual(3,run.OwnedStructures);
            Assert.AreEqual(6,run.Stat("elemental_damage"),.0001f);

            run.Inventory.Clear();
            Assert.AreEqual(2,run.RegisteredStructureCount);
            Assert.AreEqual(4,run.Stat("elemental_damage"),.0001f);
            run.Inventory.Add(catalog.Weapon("screwdriver",1));
            Assert.AreEqual(3,run.RegisteredStructureCount);
            Assert.AreEqual(6,run.Stat("elemental_damage"),.0001f);
            run.Inventory.Add(catalog.Weapon("pruner",1));
            Assert.AreEqual(4,run.RegisteredStructureCount);
            Assert.AreEqual(8,run.Stat("elemental_damage"),.0001f);
        }

        [Test] public void PocketFactoryAndStructurePetsDoNotEnterRegisteredEffectCount()
        {
            var run=new RunRules(2021);run.Configure(catalog);run.Start();
            run.Inventory.Clear();
            run.Items.Add(catalog.data.items.Single(i=>i.id=="pocket_factory"));
            run.Items.Add(catalog.data.items.Single(i=>i.id=="bot_o_mine"));
            Assert.AreEqual(0,run.RegisteredStructureCount);
            Assert.AreEqual(2,run.OwnedStructures);
            run.Inventory.Add(catalog.Weapon("wrench",1));
            Assert.AreEqual(1,run.RegisteredStructureCount);
            Assert.AreEqual(3,run.OwnedStructures);
        }
    }
}

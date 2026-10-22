using System.Linq;
using NUnit.Framework;
using UnityEditor;
using Scrapshift.Editor;

namespace Scrapshift.Tests
{
    public sealed class CharacterRemainingParityTests
    {
        ReferenceCatalog catalog;
        [SetUp] public void Setup()=>catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
        RunRules Streamer(string weapon)
        {
            var run=new RunRules(42);run.Configure(catalog);
            run.Start(catalog.data.characters.Single(c=>c.id=="streamer"),weapon);
            return run;
        }
        ItemEntry Item(string id)=>catalog.data.items.Single(i=>i.id==id);

        [TestCase("wrench")]
        [TestCase("screwdriver")]
        [TestCase("pruner")]
        public void StreamerCountsRegisteredStartingWeaponStructures(string weapon)
        {
            var run=Streamer(weapon);
            Assert.AreEqual(1,run.Inventory.Count);
            Assert.AreEqual(1,run.OwnedStructures);
            Assert.AreEqual(2,run.Stat("armor"),"add_weapon applies its StructureEffect to the registered structure array.");
        }

        [Test] public void StreamerCountsRealStructureItemsFactoryAndStructurePetButNotOtherPets()
        {
            var run=Streamer("wrench");
            foreach(string id in new[]{"turret","pocket_factory","bot_o_mine"})
                run.Items.Add(Item(id));
            Assert.AreEqual(4,run.OwnedStructures);
            Assert.AreEqual(8,run.Stat("armor"));
            run.Items.Add(Item("catling_gun"));run.Items.Add(Item("bonk_dog"));
            Assert.AreEqual(4,run.OwnedStructures);
            Assert.AreEqual(8,run.Stat("armor"),"Ordinary pets are not structure pets.");
        }

        [Test] public void CombiningOrRemovingStructureWeaponUpdatesStreamerArmor()
        {
            var run=Streamer("wrench");
            run.Items.Add(Item("turret"));
            run.Inventory.Add(catalog.Weapon("screwdriver",1));
            run.Inventory.Add(catalog.Weapon("pruner",1));
            Assert.AreEqual(4,run.OwnedStructures);
            Assert.AreEqual(8,run.Stat("armor"));
            run.Inventory.Clear();run.Inventory.Add(catalog.Weapon("wrench",4));
            Assert.AreEqual(2,run.OwnedStructures);
            Assert.AreEqual(4,run.Stat("armor"));
        }

        [Test] public void DuplicateRegisteredStructuresEachContributeAndRemovalUpdatesLinkedArmor()
        {
            var run=Streamer("pistol");var turret=Item("turret");
            run.Items.Add(turret);run.Items.Add(turret);
            Assert.AreEqual(2,run.OwnedStructures);Assert.AreEqual(4,run.Stat("armor"));
            run.Items.Remove(turret);
            Assert.AreEqual(1,run.OwnedStructures);Assert.AreEqual(2,run.Stat("armor"));
        }
    }
}

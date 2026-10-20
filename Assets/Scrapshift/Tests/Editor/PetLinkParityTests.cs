using System.Linq;
using NUnit.Framework;
using UnityEditor;
using Scrapshift.Editor;

namespace Scrapshift.Tests
{
    public sealed class PetLinkParityTests
    {
        ReferenceCatalog catalog;

        [SetUp] public void SetUp()=>catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");

        [Test] public void BeastMasterSpeedCountsRegisteredPetShieldAndPetStructureEffects()
        {
            // run_data.gd:get_nb_pets counts pet effect entries, the
            // jellyshield_count scalar, and structures whose is_pet is true.
            // Beast Master's speed link is +2 per such entry.
            var character=catalog.data.characters.Single(c=>c.id=="beast_master");
            var run=new RunRules(1702);run.Configure(catalog);run.Start(character);
            Assert.AreEqual(5,run.Items.Count);
            Assert.AreEqual(10,run.Stat("speed"),.0001f);

            // Live pet count is a rendering/combat state, not this stat source.
            run.PetCount=99;
            Assert.AreEqual(10,run.Stat("speed"),.0001f);

            var dog=catalog.data.items.Single(i=>i.id=="bonk_dog");
            Assert.IsTrue(run.RemoveItem(dog));
            Assert.AreEqual(8,run.Stat("speed"),.0001f);
            run.GrantItem(dog);
            Assert.AreEqual(10,run.Stat("speed"),.0001f);

            var shield=catalog.data.items.Single(i=>i.id=="jellyshield");
            run.GrantItem(shield);
            Assert.AreEqual(12,run.Stat("speed"),.0001f);

            var wandering=catalog.data.items.Single(i=>i.id=="wandering_bot");
            Assert.IsTrue(wandering.conditionalEffects.Single(e=>e.customKey=="structure").structure.pet);
            run.GrantItem(wandering);
            Assert.AreEqual(14,run.Stat("speed"),.0001f);
            Assert.IsTrue(run.RemoveItem(shield));
            Assert.AreEqual(12,run.Stat("speed"),.0001f);
            Assert.IsTrue(run.RemoveItem(wandering));
            Assert.AreEqual(10,run.Stat("speed"),.0001f);
        }
    }
}

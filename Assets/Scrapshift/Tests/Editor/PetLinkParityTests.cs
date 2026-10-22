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
            // Source grants Lootworm and selects one of four pet items.
            // The no-selection model default is Bonk Dog.
            CollectionAssert.AreEquivalent(new[]{"lootworm","bonk_dog"},run.Items.Select(i=>i.id).ToArray());
            Assert.AreEqual(4,run.Stat("speed"),.0001f);

            // Live pet count is a rendering/combat state, not this stat source.
            run.PetCount=99;
            Assert.AreEqual(4,run.Stat("speed"),.0001f);

            var dog=catalog.data.items.Single(i=>i.id=="bonk_dog");
            Assert.IsTrue(run.RemoveItem(dog));
            Assert.AreEqual(2,run.Stat("speed"),.0001f);
            run.GrantItem(dog);
            Assert.AreEqual(4,run.Stat("speed"),.0001f);

            var shield=catalog.data.items.Single(i=>i.id=="jellyshield");
            run.GrantItem(shield);
            Assert.AreEqual(6,run.Stat("speed"),.0001f);

            var wandering=catalog.data.items.Single(i=>i.id=="wandering_bot");
            Assert.IsTrue(wandering.conditionalEffects.Single(e=>e.customKey=="structure").structure.pet);
            run.GrantItem(wandering);
            Assert.AreEqual(8,run.Stat("speed"),.0001f);
            Assert.IsTrue(run.RemoveItem(shield));
            Assert.AreEqual(6,run.Stat("speed"),.0001f);
            Assert.IsTrue(run.RemoveItem(wandering));
            Assert.AreEqual(4,run.Stat("speed"),.0001f);
        }
    }
}

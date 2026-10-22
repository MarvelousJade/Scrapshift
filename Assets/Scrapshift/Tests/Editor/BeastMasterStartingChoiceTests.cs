using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using Scrapshift.Editor;

namespace Scrapshift.Tests
{
    public sealed class BeastMasterStartingChoiceTests
    {
        ReferenceCatalog catalog;
        [SetUp] public void SetUp()=>catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");

        [Test] public void BeastMasterChoosesOnePetInAdditionToGrantedLootworm()
        {
            // beast_master_data.tres offers four starting_items; effect_4.tres
            // separately grants Lootworm via the starting_item effect.
            var character=catalog.data.characters.Single(c=>c.id=="beast_master");
            CollectionAssert.AreEqual(new[]{"lootworm"},character.startingItems);
            CollectionAssert.AreEqual(new[]{"bonk_dog","catling_gun","blazemander","bot_o_mine"},character.startingItemChoices);
            foreach(var selected in character.startingItemChoices)
            {
                var run=new RunRules(1710);run.Configure(catalog);run.Start(character,null,0,selected);
                CollectionAssert.AreEquivalent(new[]{"lootworm",selected},run.Items.Select(i=>i.id).ToArray(),selected);
                Assert.IsEmpty(run.Inventory,selected);
                Assert.AreEqual(4,run.Stat("speed"),.0001f,selected);
            }
        }

        [Test] public void BeastMasterDefaultAndInvalidChoiceDoNotGrantEveryOption()
        {
            var character=catalog.data.characters.Single(c=>c.id=="beast_master");
            var run=new RunRules(1711);run.Configure(catalog);run.Start(character);
            CollectionAssert.AreEquivalent(new[]{"lootworm","bonk_dog"},run.Items.Select(i=>i.id).ToArray());
            Assert.Throws<ArgumentException>(()=>run.Start(character,null,0,"jellyshield"));
        }

        [Test] public void AvailableStartingItemsFilterUnlocksAndSortBySourceTier()
        {
            // weapon_selection.gd marks locked entries through ProgressData;
            // ItemService.get_ordered_starting_items sorts eligible items by tier.
            var coffee=catalog.data.items.Single(i=>i.id=="coffee");
            var shield=catalog.data.items.Single(i=>i.id=="jellyshield");
            bool coffeeAvailable=coffee.available,shieldAvailable=shield.available;
            try
            {
                var profile=new CharacterProfile{startingItemChoices=new[]{"jellyshield","coffee"}};
                coffee.available=true;shield.available=false;
                CollectionAssert.AreEqual(new[]{"coffee"},StarterChoices.AvailableItems(catalog,profile).Select(i=>i.id).ToArray());
                shield.available=true;
                CollectionAssert.AreEqual(new[]{"coffee","jellyshield"},StarterChoices.AvailableItems(catalog,profile).Select(i=>i.id).ToArray());
            }
            finally{coffee.available=coffeeAvailable;shield.available=shieldAvailable;}
        }
    }
}

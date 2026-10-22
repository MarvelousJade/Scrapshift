using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Scrapshift.Editor;

namespace Scrapshift.Tests
{
    public sealed class EconomyRemainingParityTests
    {
        ReferenceCatalog catalog;
        UnlockDefinition[] definitions;
        [SetUp] public void Setup()
        {
            catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
            definitions=JsonUtility.FromJson<UnlockCatalog>(Resources.Load<TextAsset>("base-unlocks").text).challenges;
            RunRules.ConfigureShopItems(catalog.data.items);
        }
        [TearDown] public void RestoreShopCatalog()=>RunRules.ConfigureShopItems(catalog.data.items);

        [TestCase("knight",1,.4f,0)]
        [TestCase("knight",101,20.6f,20)]
        [TestCase("vagabond",1,1f,0)]
        [TestCase("vagabond",41,21.5f,20)]
        public void HarvestGrowthUsesFractionalEffectiveStatBeforeCeiling(string character,int baseHarvest,float afterGrowth,int income)
        {
            // main.manage_harvesting passes effective harvesting to integer
            // gold/XP APIs; _on_HarvestingTimer_timeout separately ceils growth.
            var profile=catalog.data.characters.Single(c=>c.id==character);
            var run=new RunRules(42);run.Configure(catalog);
            run.Start(profile,StarterChoices.Available(catalog,profile)[0]);
            run.GainWeaponStat("harvesting",baseHarvest);
            run.Tick(run.Remaining);
            Assert.AreEqual(income,run.Scrap,"Currency is truncated only at the credit boundary.");
            Assert.AreEqual(afterGrowth,run.Stat("harvesting"),.0001f,"Fractional stat gains still grow harvesting.");
        }

        [TestCase("chal_advanced_technology","ranged_damage")]
        [TestCase("chal_magic_and_machinery","elemental_damage")]
        public void WeaponStructuresCountAsRegisteredChallengeEffectsButFactoryDoesNot(string challenge,string stat)
        {
            var progress=new Progression(definitions);
            var run=new RunRules(42){Progress=progress};run.Configure(catalog);run.Start();
            run.Inventory.Clear();
            foreach(string id in new[]{"wrench","screwdriver"})run.Inventory.Add(catalog.Weapon(id,1));
            run.Items.Add(catalog.data.items.Single(i=>i.id=="pocket_factory"));
            run.GainWeaponStat(stat,10);
            Assert.AreEqual(2,run.RegisteredStructureCount);
            Assert.AreEqual(3,run.OwnedStructures,"Pocket Factory adds to owned structures, but not registered effects.");
            progress.CheckStats(run);
            Assert.IsFalse(progress.Completed(challenge));
            run.Inventory.Add(catalog.Weapon("pruner",1));
            Assert.AreEqual(3,run.RegisteredStructureCount);
            Assert.AreEqual(4,run.OwnedStructures);
            progress.CheckStats(run);Assert.IsTrue(progress.Completed(challenge));
        }

        [Test] public void StructurePetsDoNotCountAsStructureEffectChallengeEntries()
        {
            var progress=new Progression(definitions);
            var run=new RunRules(42){Progress=progress};run.Configure(catalog);run.Start();
            var pet=new ItemEntry{id="fixture_structure_pet",effects=Array.Empty<StatEffect>(),conditionalEffects=new[]{
                new ConditionalEffect{key="",customKey="pet",value=3,pet=new PetSpec{structure=true}}
            }};
            run.Items.Add(pet);run.GainWeaponStat("ranged_damage",10);run.GainWeaponStat("elemental_damage",10);
            Assert.AreEqual(1,run.OwnedStructures,"The reference counts the registered structure pet entry, not its multiplicity.");
            progress.CheckStats(run);
            Assert.IsFalse(progress.Completed("chal_advanced_technology"));
            Assert.IsFalse(progress.Completed("chal_magic_and_machinery"));
        }

        [Test] public void OneMultiSpawnEffectIsStillOneChallengeRegistration()
        {
            var progress=new Progression(definitions);
            var run=new RunRules(42){Progress=progress};run.Configure(catalog);run.Start();
            run.Items.Add(new ItemEntry{id="fixture_multi_structure",effects=Array.Empty<StatEffect>(),conditionalEffects=new[]{
                new ConditionalEffect{key="",customKey="structure",value=3}
            }});
            run.GainWeaponStat("ranged_damage",10);
            Assert.AreEqual(1,run.OwnedStructures,"The reference counts one registered effect entry, not its multiplicity.");
            progress.CheckStats(run);
            Assert.IsFalse(progress.Completed("chal_advanced_technology"),"StructureEffect.apply appends once per effect, not per value.");
        }

        [Test] public void CharacterBannedItemRemainsInBackupWithoutRestoringExcludedOffers()
        {
            // item_service._get_rand_item_for_wave removes character bans from
            // the preferred pool; only explicit run bans also affect backup.
            var banned=new ItemEntry{id="coffee",tier=1,available=true,maxCount=-1};
            var excluded=new ItemEntry{id="cake",tier=1,available=true,maxCount=-1};
            RunRules.ConfigureShopItems(new[]{banned,excluded});
            var character=new CharacterProfile{bannedItems=new[]{"item_coffee"}};
            CollectionAssert.AreEqual(new[]{banned},ShopSelection.ItemPool(new[]{banned,excluded},character,1,false,
                new[]{excluded},Array.Empty<ItemEntry>()));
        }

        [Test] public void PreferredTagFailureMayUseCharacterBannedItemInBackup()
        {
            var banned=new ItemEntry{id="coffee",tier=1,available=true,maxCount=-1,tags=new[]{"stat_attack_speed"}};
            var ordinary=new ItemEntry{id="cake",tier=1,available=true,maxCount=-1,tags=new[]{"stat_max_hp"}};
            RunRules.ConfigureShopItems(new[]{banned,ordinary});
            var character=new CharacterProfile{bannedItems=new[]{"item_coffee"},wantedTags=new[]{"stat_luck"}};
            CollectionAssert.AreEqual(new[]{banned,ordinary},ShopSelection.ItemPool(new[]{banned,ordinary},character,1,true,
                Array.Empty<ItemEntry>(),Array.Empty<ItemEntry>()));
        }

        [Test] public void CharacterBanStillAppliesWhenPreferredPoolHasAnEligibleItem()
        {
            var banned=new ItemEntry{id="coffee",tier=1,available=true,maxCount=-1};
            var ordinary=new ItemEntry{id="cake",tier=1,available=true,maxCount=-1};
            RunRules.ConfigureShopItems(new[]{banned,ordinary});
            var character=new CharacterProfile{bannedItems=new[]{"item_coffee"}};
            CollectionAssert.AreEqual(new[]{ordinary},ShopSelection.ItemPool(new[]{banned,ordinary},character,1,false,
                Array.Empty<ItemEntry>(),Array.Empty<ItemEntry>()));
        }
    }
}

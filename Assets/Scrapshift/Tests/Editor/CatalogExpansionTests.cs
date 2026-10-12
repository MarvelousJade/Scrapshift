using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Scrapshift.Editor;
namespace Scrapshift.Tests
{
    public sealed class CatalogExpansionTests
    {
        ReferenceCatalog catalog;
        [SetUp] public void Setup()=>catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
        RunRules Run(){var r=new RunRules(5);r.Configure(catalog);r.Start();return r;}
        [Test] public void BaseWeaponsHaveRealTiersArtAndExplicitRuntimeEligibility()
        {
            Assert.AreEqual(62,catalog.data.equipment.Select(w=>w.id).Distinct().Count());
            Assert.AreEqual(205,catalog.data.equipment.Length);
            Assert.IsNull(catalog.Weapon("rocket_launcher",1));Assert.IsNotNull(catalog.Weapon("rocket_launcher",2));
            Assert.IsNull(catalog.Weapon("chain_gun",3));Assert.IsNotNull(catalog.Weapon("chain_gun",4));
            foreach(var w in catalog.data.equipment)
            {
                Assert.NotNull(w.image,w.id);Assert.IsFalse(AssetDatabase.GetAssetPath(w.image).Contains("/Reference/"));
                Assert.IsTrue(w.available== (w.sourceAvailable&&w.implemented),w.id);
                Assert.IsNotEmpty(w.sounds,w.id);Assert.IsTrue(w.sounds.All(c=>c!=null),w.id);
                if(w.tier<4)Assert.NotNull(catalog.Weapon(w.id,w.tier+1),w.id);
                Assert.IsFalse(float.IsNaN(Run().WeaponDamage(w)),w.id);
            }
        }
        [Test] public void UnfinishedBehaviorsCannotAppearInShop()
        {
            var r=Run();Assert.IsTrue(r.CanEquip(catalog.Weapon("pruner",1)));Assert.IsTrue(r.CanEquip(catalog.Weapon("rocket_launcher",2)));
            r.Collect(10000);r.Tick(r.Remaining);while(r.Phase==RunPhase.Upgrade)r.Choose(0);
            for(int n=0;n<100;n++){Assert.IsTrue(r.Shop.Where(o=>o?.weapon!=null).All(o=>o.weapon.implemented));r.Reroll();}
        }
        [Test] public void DiscountsAndRecyclingUseUnroundedReferencePriceAndClamp()
        {
            var r=Run();r.GainWeaponStat("items_price",-10);Assert.AreEqual(10,r.Price(10));
            r.GainWeaponStat("recycling_gains",100);Assert.AreEqual(r.Price(10),r.RecycleValue(10));
            r.GainWeaponStat("items_price",-200);Assert.AreEqual(1,r.Price(10));
        }
        [Test] public void PickupRangeAndFractionalXpUseReferenceUnits()
        {
            var r=Run();Assert.AreEqual(1.5f,r.PickupRadius);r.GainWeaponStat("pickup_range",50);Assert.AreEqual(2.25f,r.PickupRadius);
            r.GainWeaponStat("pickup_range",-1000);Assert.AreEqual(.3f,r.PickupRadius);
            r.GainWeaponStat("xp_gain",25);for(int i=0;i<4;i++)r.Collect(1);Assert.AreEqual(5,r.XP);Assert.AreEqual(4,r.Scrap);
        }
        [Test] public void CrateGoldDoesNotGrantXpAndHealingUsesConsumableModifier()
        {
            var r=Run();r.Hurt(8);r.GainWeaponStat("consumable_heal",2);r.GainWeaponStat("item_box_gold",15);
            r.CollectConsumable(ConsumableKind.Crate);Assert.AreEqual(7,r.Health);Assert.AreEqual(15,r.Scrap);Assert.AreEqual(0,r.XP);Assert.AreEqual(1,r.PendingCrates);
        }
        [Test] public void CrownAddsToFivePercentHarvestGrowth()
        {
            var r=Run();r.GainWeaponStat("harvesting",20);r.GainWeaponStat("harvesting_growth",8);r.Tick(r.Remaining);Assert.AreEqual(23,r.Stat("harvesting"));
        }
        [Test] public void ImplantNamesDoNotModifySourceDataAndPenaltyColorsAreCorrect()
        {
            var text=new InspectionText(catalog.uiText);var item=catalog.data.items.Single(i=>i.id=="coupon");
            Assert.AreEqual("Merchant Credential Chip",text.ItemName(item));StringAssert.Contains("%",text.ItemDescription(item));
            Assert.IsTrue(SpecialItemEffects.IsPenalty("enemy_health",5));Assert.IsFalse(SpecialItemEffects.IsPenalty("items_price",-5));
            Assert.AreEqual(Color.white,PresentationPolish.BodyTint);
        }
    }
}

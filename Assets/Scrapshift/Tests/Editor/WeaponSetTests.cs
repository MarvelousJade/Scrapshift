using System.Linq;
using NUnit.Framework;
using UnityEditor;
using Scrapshift.Editor;

namespace Scrapshift.Tests
{
    public sealed class WeaponSetTests
    {
        ReferenceCatalog catalog;
        [SetUp] public void Setup()=>catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
        RunRules Run(bool shop=false)
        {
            var run=new RunRules(42);run.Configure(catalog);run.Start();
            if(shop) { run.Collect(1000);run.Tick(run.Remaining);while(run.Phase==RunPhase.Upgrade)run.Choose(0); }
            return run;
        }
        [Test] public void EveryShippedWeaponTierHasItsReferenceMembership()
        {
            Assert.AreEqual(120,catalog.data.equipment.Length);
            foreach(var weapon in catalog.data.equipment)
                Assert.AreEqual(weapon.setIds[0],WeaponSets.For(weapon).Id);
            Assert.IsNull(WeaponSets.For(new Equipment {id="future_unknown"}));
        }
        [TestCase(0,0,0)] [TestCase(1,0,0)] [TestCase(2,10,3)] [TestCase(3,20,6)]
        [TestCase(4,30,9)] [TestCase(5,40,12)] [TestCase(6,50,15)] [TestCase(7,50,15)]
        public void ThresholdsAreTotalsAndCapAtSix(int count,int range,int other)
        {
            Assert.AreEqual(range,WeaponSets.Gun.Bonus(count));
            Assert.AreEqual(other,WeaponSets.Precise.Bonus(count));
            Assert.AreEqual(other,WeaponSets.Primitive.Bonus(count));
        }
        [Test] public void DuplicateWeaponsCountSeparatelyWithoutTierWeight()
        {
            var r=Run();r.Inventory.Clear();
            foreach(int tier in new[]{1,1,2,3,4,4})r.Inventory.Add(catalog.Weapon("knife",tier));
            Assert.AreEqual(6,r.SetCount(WeaponSets.Precise));Assert.AreEqual(15,r.Stat("crit_chance"));
            Assert.AreEqual(.15f,r.CritChance,.00001f);
        }
        [Test] public void MixedSetsApplyGlobalRangeCriticalChanceAndHealth()
        {
            var r=Run();r.Inventory.Clear();
            foreach(string id in new[]{"pistol","smg","knife","knife","spear","spear"})r.Inventory.Add(catalog.Weapon(id,1));
            Assert.AreEqual(10,r.Stat("range"));Assert.AreEqual(3,r.Stat("crit_chance"));Assert.AreEqual(13,r.MaxHealth);
            var pistol=catalog.Weapon("pistol",1);var knife=catalog.Weapon("knife",1);
            Assert.AreEqual(pistol.range+.1f,r.WeaponRange(pistol),.00001f);
            Assert.AreEqual(knife.range+.05f,r.WeaponRange(knife),.00001f);
        }
        [Test] public void PurchasingThenCombiningAndRecyclingUpdatesCountsImmediately()
        {
            var r=Run(true);r.Inventory.Clear();r.Inventory.Add(catalog.Weapon("pistol",1));
            r.Shop[0]=new ShopOffer {weapon=catalog.Weapon("pistol",1)};
            Assert.IsTrue(r.Buy(0));Assert.AreEqual(10,r.Stat("range"));
            r.Combine(0);Assert.AreEqual(1,r.Inventory.Count);Assert.AreEqual(2,r.Inventory[0].tier);
            Assert.AreEqual(0,r.Stat("range"));
            r.Recycle(0);Assert.AreEqual(0,r.SetCount(WeaponSets.Gun));
        }
        [Test] public void FullSlotMatchingPurchaseRetainsSetCount()
        {
            var r=Run(true);r.Inventory.Clear();
            for(int i=0;i<6;i++)r.Inventory.Add(catalog.Weapon("spear",1));
            float hp=r.MaxHealth;
            r.Shop[0]=new ShopOffer {weapon=catalog.Weapon("spear",1)};
            Assert.IsTrue(r.Buy(0));Assert.AreEqual(6,r.Inventory.Count);Assert.AreEqual(hp,r.MaxHealth);
            Assert.IsTrue(r.Inventory.Any(w=>w.tier==2));
        }
        [Test] public void ItemAndLevelStatsNeverBakeInTemporarySetBonuses()
        {
            var r=Run(true);r.Inventory.Clear();float baseline=r.MaxHealth;
            r.Inventory.Add(catalog.Weapon("spear",1));r.Inventory.Add(catalog.Weapon("spear",1));
            r.Shop[0]=new ShopOffer {item=new ItemEntry {value=0,effects=new[]{new StatEffect {key="stat_max_hp",value=7}}}};
            Assert.IsTrue(r.Buy(0));Assert.AreEqual(baseline+10,r.MaxHealth);
            r.Recycle(0);Assert.AreEqual(baseline+7,r.MaxHealth);
            r.NextWave();r.Inventory.Add(catalog.Weapon("spear",1));
            r.Collect(r.NextLevel);Assert.AreEqual(baseline+11,r.MaxHealth);
            r.Inventory.Clear();Assert.AreEqual(baseline+8,r.MaxHealth);
        }
        [Test] public void PrimitiveLossClampsHealthAndRestartClearsBonuses()
        {
            var r=Run(true);r.Inventory.Clear();float baseline=r.MaxHealth;
            r.Inventory.Add(catalog.Weapon("spear",1));r.Inventory.Add(catalog.Weapon("spear",1));
            r.NextWave();Assert.AreEqual(baseline+3,r.Health);
            r.Tick(r.Remaining);while(r.Phase==RunPhase.Upgrade)r.Choose(0);
            float beforeCombine=r.MaxHealth,beforeHealth=r.Health;
            r.Combine(0);Assert.AreEqual(UnityEngine.Mathf.Min(beforeHealth,beforeCombine-3),r.Health);Assert.AreEqual(beforeCombine-3,r.MaxHealth);
            r.Start();Assert.AreEqual(10,r.MaxHealth);Assert.AreEqual(0,r.Stat("range"));
            Assert.AreEqual(0,r.Stat("crit_chance"));Assert.AreEqual(1,r.SetCount(WeaponSets.Gun));
        }
    }
}

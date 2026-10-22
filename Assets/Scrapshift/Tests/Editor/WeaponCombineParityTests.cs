using System.Linq;
using NUnit.Framework;
using UnityEditor;
using Scrapshift.Editor;

namespace Scrapshift.Tests
{
    public sealed class WeaponCombineParityTests
    {
        ReferenceCatalog catalog;
        RunRules run;
        [SetUp] public void Setup()
        {
            catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
            run=new RunRules(42);run.Configure(catalog);run.Start();run.Collect(500);run.Tick(run.Remaining);
            while(run.Phase==RunPhase.Upgrade)run.Choose(0);
            Assert.AreEqual(RunPhase.Shop,run.Phase);run.Inventory.Clear();
        }
        void Equip(params string[] ids){foreach(string id in ids)run.Inventory.Add(catalog.Weapon(id,1));}
        string[] Order()=>run.Inventory.Select(w=>w.id+":"+w.tier).ToArray();
        [Test] public void ManualCombineAppendsUpgradeAndPreservesOtherWeaponOrder()
        {
            Equip("pistol","knife","pistol","smg");run.Combine(0);
            CollectionAssert.AreEqual(new[]{"knife:1","smg:1","pistol:2"},Order());
        }
        [Test] public void SelectingLaterDuplicateStillRemovesFirstTwoMatchingCopiesLikeSource()
        {
            Equip("pistol","knife","pistol","smg","pistol");run.Combine(4);
            CollectionAssert.AreEqual(new[]{"knife:1","smg:1","pistol:1","pistol:2"},Order());
        }
        [Test] public void FullInventoryPurchaseAppendsUpgradeAfterUnrelatedWeapons()
        {
            Equip("pistol","knife","smg","laser_gun","spear","double_barrel_shotgun");
            run.Shop[0]=new ShopOffer{weapon=catalog.Weapon("pistol",1)};Assert.IsTrue(run.Buy(0));
            CollectionAssert.AreEqual(new[]{"knife:1","smg:1","laser_gun:1","spear:1","double_barrel_shotgun:1","pistol:2"},Order());
        }
        [Test] public void AutoCombineWithTwoExistingCopiesKeepsThePurchasedCopyAtTheEndBeforeUpgrade()
        {
            Equip("pistol","knife","pistol","smg","spear","double_barrel_shotgun");
            run.Shop[0]=new ShopOffer{weapon=catalog.Weapon("pistol",1)};Assert.IsTrue(run.Buy(0));
            CollectionAssert.AreEqual(new[]{"knife:1","smg:1","spear:1","double_barrel_shotgun:1","pistol:1","pistol:2"},Order());
        }
        [Test] public void BuyingIntoAnAvailableSlotDoesNotCombineOrReorder()
        {
            Equip("pistol","knife");run.Shop[0]=new ShopOffer{weapon=catalog.Weapon("pistol",1)};Assert.IsTrue(run.Buy(0));
            CollectionAssert.AreEqual(new[]{"pistol:1","knife:1","pistol:1"},Order());
        }
    }
}

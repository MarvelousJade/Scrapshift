using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Scrapshift.Editor;

namespace Scrapshift.Tests
{
    public sealed class EconomyNativeParityTests
    {
        ReferenceCatalog catalog;
        Progression progress;
        RunUnlockPool pool;

        [SetUp] public void Setup()
        {
            catalog=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset"));
            progress=new Progression(JsonUtility.FromJson<UnlockCatalog>(Resources.Load<TextAsset>("base-unlocks").text).challenges);
            pool=new RunUnlockPool();pool.Refresh(catalog,progress);
        }
        [TearDown] public void Teardown()=>UnityEngine.Object.DestroyImmediate(catalog);

        [Test] public void MidRunUnlockNotInCurrentItemPoolButImmediateCharacterRewardIsVisible()
        {
            var fruit=catalog.data.items.Single(i=>i.id=="fruit_basket");
            var chunky=catalog.data.characters.Single(c=>c.id=="chunky");
            Assert.IsFalse(fruit.available);Assert.IsFalse(chunky.available);
            pool.BeginRun(catalog,progress);
            progress.Complete("chal_fruit_basket");progress.Complete("chal_rookie");
            pool.Refresh(catalog,progress);
            Assert.IsTrue(progress.Unlocked("item_fruit_basket"));
            Assert.IsFalse(fruit.available);
            Assert.IsFalse(RunRules.IsShopItem("fruit_basket"));
            Assert.IsFalse(ShopSelection.ItemPool(catalog.data.items,null,2,false,Array.Empty<ItemEntry>(),Array.Empty<ItemEntry>()).Contains(fruit));
            Assert.IsTrue(chunky.available,"The just-unlocked character is visible in the menu and achievement UI immediately.");
        }

        [Test] public void NextRunRebuildsItemAndWeaponEligibilityFromCompletedRewards()
        {
            var fruit=catalog.data.items.Single(i=>i.id=="fruit_basket");
            var fist=catalog.data.equipment.First(w=>w.id=="power_fist");
            Assert.IsFalse(fruit.available);Assert.IsFalse(fist.available);
            pool.BeginRun(catalog,progress);
            progress.Complete("chal_fruit_basket");progress.Complete("chal_brawler");pool.Refresh(catalog,progress);
            Assert.IsFalse(fruit.available);Assert.IsFalse(fist.available);
            pool.BeginRun(catalog,progress);
            Assert.IsTrue(fruit.available);Assert.IsTrue(fist.available);
            Assert.IsTrue(RunRules.IsShopItem("fruit_basket"));
            Assert.Contains(fruit,ShopSelection.ItemPool(catalog.data.items,null,2,false,Array.Empty<ItemEntry>(),Array.Empty<ItemEntry>()));
        }

        [Test] public void LeavingRunUpdatesNextMenuWithoutReenteringGame()
        {
            var fruit=catalog.data.items.Single(i=>i.id=="fruit_basket");
            pool.BeginRun(catalog,progress);progress.Complete("chal_fruit_basket");pool.Refresh(catalog,progress);
            Assert.IsFalse(fruit.available);
            pool.EndRun(catalog,progress);
            Assert.IsTrue(fruit.available);Assert.IsFalse(pool.Frozen);
        }
    }
}

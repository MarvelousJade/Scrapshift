using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Scrapshift.Editor;

namespace Scrapshift.Tests
{
    public sealed class ShopUnlockFollowupParityTests
    {
        ReferenceCatalog catalog;
        UnlockDefinition[] definitions;
        string directory,save;
        [SetUp] public void Setup()
        {
            catalog=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset"));
            definitions=JsonUtility.FromJson<UnlockCatalog>(Resources.Load<TextAsset>("base-unlocks").text).challenges;
            directory=Path.Combine(Path.GetTempPath(),"ScrapshiftShopUnlockFollowup",Guid.NewGuid().ToString("N"));
            save=Path.Combine(directory,"progress.json");
        }
        [TearDown] public void Cleanup()
        {
            var baseline=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
            RunRules.ConfigureShopItems(baseline.data.items);
            UnityEngine.Object.DestroyImmediate(catalog);
            foreach(string name in new[]{"progress.json","progress.json.tmp","progress.json.bak"})
            {string file=Path.Combine(directory,name);if(File.Exists(file))File.Delete(file);}
            if(Directory.Exists(directory))Directory.Delete(directory,false);
        }
        RunRules DemonShop(float effectiveHealth)
        {
            var profile=catalog.data.characters.Single(c=>c.id=="demon");
            var run=new RunRules(771);run.Configure(catalog);run.Start(profile,StarterChoices.Available(catalog,profile)[0]);
            run.Tick(run.Remaining);while(run.Phase==RunPhase.Upgrade)run.Choose(0);
            run.GainWeaponStat("max_hp",effectiveHealth-run.Stat("max_hp"));
            run.Shop[0]=new ShopOffer{item=new ItemEntry{id="followup-one-hp",value=1,effects=Array.Empty<StatEffect>(),conditionalEffects=Array.Empty<ConditionalEffect>()},rolledWave=run.Wave};
            return run;
        }
        // RunData.get_player_currency returns get_stat(max_hp) as int;
        // combat LiveStats can still clamp the player's effective max to one.
        [TestCase(0,0,false)] [TestCase(-1,-1,false)]
        [TestCase(.9f,0,false)] [TestCase(1.1f,1,true)]
        public void DemonAffordabilityUsesRawTruncatedCurrency(float health,int currency,bool canBuy)
        {
            var run=DemonShop(health);Assert.AreEqual(currency,run.ShopBalance);
            Assert.AreEqual(1,run.Price(run.Shop[0]));Assert.AreEqual(canBuy,run.CanBuy(0));
            Assert.GreaterOrEqual(run.MaxHealth,1,"Combat health minimum is separate from shop currency.");
        }
        [Test] public void SpendingLastShopHpDoesNotCreateAnInfiniteOneHpBudget()
        {
            var run=DemonShop(1);var offer=run.Shop[0];Assert.IsTrue(run.Buy(0));
            Assert.AreEqual(0,run.ShopBalance);Assert.AreEqual(1,run.MaxHealth);
            run.Shop[0]=offer;Assert.IsFalse(run.CanBuy(0));Assert.IsFalse(run.Buy(0));
            Assert.AreEqual(0,run.Stat("max_hp"));
        }
        [Test] public void ZeroCurrencyCannotBuyAnItemEvenWhenItsEffectWouldAddHp()
        {
            var run=DemonShop(0);run.Shop[0].item.effects=new[]{new StatEffect{key="stat_max_hp",value=3}};
            Assert.IsFalse(run.Buy(0));Assert.AreEqual(0,run.Stat("max_hp"));
        }
        [Test] public void SavedPartialLifetimeProgressCompletesAtALaterWaveTimeoutOnce()
        {
            var first=new Progression(definitions,save);first.Count("kills",100);first.Save();
            var second=new Progression(definitions,save);Assert.AreEqual(100,second.Total("kills"));
            second.Count("kills",200);Assert.IsFalse(second.Completed("chal_survivor_1"));
            int toasts=0;second.ChallengeCompleted+=d=>{if(d.id=="chal_survivor_1")toasts++;};
            second.CheckCountedChallenges();second.CheckCountedChallenges();Assert.AreEqual(1,toasts);
            var third=new Progression(definitions,save);Assert.AreEqual(300,third.Total("kills"));
            Assert.IsTrue(third.Unlocked("character_old"));
            third.ChallengeCompleted+=d=>toasts++;third.Complete("chal_survivor_1");Assert.AreEqual(1,toasts);
        }
        [Test] public void MidRunItemRewardRemainsFrozenThenSurvivesReloadIntoNextRunPool()
        {
            var first=new Progression(definitions,save);var pool=new RunUnlockPool();pool.BeginRun(catalog,first);
            var fruit=catalog.data.items.Single(i=>i.id=="fruit_basket");Assert.IsFalse(fruit.available);
            first.Count("full_health_fruit",100);pool.Refresh(catalog,first);
            Assert.IsTrue(first.Unlocked("item_fruit_basket"));Assert.IsFalse(fruit.available);
            Assert.IsFalse(RunRules.IsShopItem("fruit_basket"));
            var reloaded=new Progression(definitions,save);pool.BeginRun(catalog,reloaded);
            Assert.IsTrue(fruit.available);Assert.IsTrue(RunRules.IsShopItem("fruit_basket"));
        }
        [Test] public void CharacterWinWeaponRewardIsFrozenUntilNextRunAndPersistsAcrossReload()
        {
            var first=new Progression(definitions,save);var pool=new RunUnlockPool();pool.BeginRun(catalog,first);
            var weapons=catalog.data.equipment.Where(w=>w.id=="power_fist").ToArray();Assert.IsNotEmpty(weapons);
            Assert.IsTrue(weapons.All(w=>!w.available));
            first.Finish(true,"brawler",0,false);pool.Refresh(catalog,first);
            Assert.IsTrue(first.Unlocked("weapon_power_fist"));Assert.IsTrue(weapons.All(w=>!w.available));
            var reloaded=new Progression(definitions,save);pool.BeginRun(catalog,reloaded);
            Assert.IsTrue(weapons.All(w=>w.available));Assert.IsTrue(reloaded.Completed("chal_brawler"));
            Assert.AreEqual(0,reloaded.BestDifficulty("brawler"));
        }
        [Test] public void RetryKeepsGlobalUnlocksAndLifetimeCountersButDoesNotRefreshTheActiveRunPool()
        {
            var progress=new Progression(definitions);var pool=new RunUnlockPool();pool.BeginRun(catalog,progress);
            var run=new RunRules(771){Progress=progress};run.Configure(catalog);run.Start();run.CaptureWaveRetryState();
            progress.Count("kills",7);progress.Count("full_health_fruit",100);
            run.Hurt(10000);run.AdvanceDeathCleanup(.5f);
            Assert.AreEqual(RunPhase.Lost,run.Phase);
            progress.Finish(false,"well_rounded",0,false);pool.Refresh(catalog,progress);
            pool.RefreshAfterRun(catalog,progress,run.Phase,false);
            Assert.IsTrue(progress.Unlocked("character_chunky"));
            var fruit=catalog.data.items.Single(i=>i.id=="fruit_basket");Assert.IsFalse(fruit.available);
            Assert.IsTrue(run.RetryWave());Assert.AreSame(progress,run.Progress);
            Assert.AreEqual(7,progress.Total("kills"));Assert.IsTrue(progress.Completed("chal_fruit_basket"));
            Assert.IsTrue(catalog.data.characters.Single(c=>c.id=="chunky").available);
            Assert.IsTrue(pool.Frozen);Assert.IsFalse(fruit.available);
            pool.BeginRun(catalog,progress);Assert.IsTrue(fruit.available);
        }
        [TestCase(RunPhase.Lost,false,false)] [TestCase(RunPhase.Lost,true,true)]
        [TestCase(RunPhase.Won,false,true)] [TestCase(RunPhase.Title,false,true)]
        [TestCase(RunPhase.Shop,false,false)]
        public void RuntimePoolLifecycleOnlyThawsAtACompletedRunBoundary(RunPhase phase,bool confirmed,bool shouldThaw)
        {
            var progress=new Progression(definitions);var pool=new RunUnlockPool();pool.BeginRun(catalog,progress);
            progress.Complete("chal_fruit_basket");pool.RefreshAfterRun(catalog,progress,phase,confirmed);
            Assert.AreEqual(!shouldThaw,pool.Frozen);
            Assert.AreEqual(shouldThaw,catalog.data.items.Single(i=>i.id=="fruit_basket").available);
        }
    }
}

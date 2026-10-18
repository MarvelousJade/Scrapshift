using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Scrapshift.Editor;

namespace Scrapshift.Tests
{
    public sealed class ShopUnlockNextParityTests
    {
        ReferenceCatalog catalog;
        UnlockDefinition[] definitions;
        [SetUp] public void Setup()
        {
            catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
            definitions=JsonUtility.FromJson<UnlockCatalog>(Resources.Load<TextAsset>("base-unlocks").text).challenges;
        }
        RunRules Run(Progression progress){var run=new RunRules(19);run.Configure(catalog);run.Progress=progress;run.Start();return run;}
        [TestCase("kills","chal_survivor_1")]
        [TestCase("materials","chal_gatherer_1")]
        [TestCase("trees","chal_lumberjack")]
        [TestCase("evil_mob","chal_evil_hat")]
        public void LifetimeThresholdsWaitForTheWaveTimeoutBeforeUnlocking(string counter,string challenge)
        {
            var progress=new Progression(definitions);int needed=(int)definitions.Single(d=>d.id==challenge).value;
            progress.Count(counter,needed);Assert.IsFalse(progress.Completed(challenge));
            Assert.AreEqual(needed,progress.Total(counter));progress.CheckCountedChallenges();
            Assert.IsTrue(progress.Completed(challenge));
            int count=progress.Data.completed.Count;progress.CheckCountedChallenges();Assert.AreEqual(count,progress.Data.completed.Count);
        }
        [Test] public void ActualWaveEndChecksLifetimeCountersAndProducesOneUnlockNotification()
        {
            var progress=new Progression(definitions);var run=Run(progress);int notifications=0;
            progress.ChallengeCompleted+=d=>{if(d.id=="chal_survivor_1")notifications++;};
            for(int i=0;i<300;i++)run.RecordEnemy("baby_alien",false,1,false);
            Assert.IsFalse(progress.Completed("chal_survivor_1"));run.RecordWaveEnd(0,0);run.RecordWaveEnd(0,0);
            Assert.IsTrue(progress.Unlocked("character_old"));Assert.AreEqual(1,notifications);
        }
        [Test] public void DeathDoesNotPrematurelyRunTheDeferredWaveTimeoutChecks()
        {
            var progress=new Progression(definitions);progress.Count("kills",300);progress.Finish(false,"well_rounded",0,false);
            Assert.IsTrue(progress.Completed("chal_rookie"));Assert.IsFalse(progress.Completed("chal_survivor_1"));
            progress.CheckCountedChallenges();Assert.IsTrue(progress.Completed("chal_survivor_1"));
        }
        [Test] public void FullHealthFruitAchievementRemainsImmediateAndAccumulatesAcrossRuns()
        {
            var progress=new Progression(definitions);int threshold=(int)definitions.Single(d=>d.id=="chal_fruit_basket").value;
            progress.Count("full_health_fruit",threshold-1);Assert.IsFalse(progress.Completed("chal_fruit_basket"));
            progress.Finish(false,"well_rounded",0,false,false);progress.Count("full_health_fruit");
            Assert.IsTrue(progress.Completed("chal_fruit_basket"));
        }
        [Test] public void VictoryChecksCountersWhenTheFinalBossPathBypassesItsOrdinaryWaveSweep()
        {
            var progress=new Progression(definitions);progress.Count("kills",300);int notifications=0;
            progress.ChallengeCompleted+=d=>{if(d.id=="chal_survivor_1")notifications++;};
            progress.Finish(true,"well_rounded",0,false);progress.CheckCountedChallenges();
            Assert.IsTrue(progress.Completed("chal_survivor_1"));Assert.AreEqual(1,notifications);
        }
        [Test] public void CleanupFruitCannotUnlockFruitBasketButRetainsPermanentExtraStomachGrowth()
        {
            var progress=new Progression(definitions);var run=Run(progress);
            int threshold=(int)definitions.Single(d=>d.id=="chal_fruit_basket").value;
            progress.Count("full_health_fruit",threshold-1);run.GrantItem(catalog.data.items.Single(i=>i.id=="extra_stomach"));
            float maximum=run.MaxHealth;run.CollectConsumable(ConsumableKind.Fruit,true);
            Assert.AreEqual(maximum+1,run.MaxHealth);Assert.AreEqual(threshold-1,progress.Total("full_health_fruit"));
            Assert.IsFalse(progress.Completed("chal_fruit_basket"));
            run.CollectConsumable(ConsumableKind.Fruit);Assert.IsTrue(progress.Completed("chal_fruit_basket"));
        }
        [Test] public void StatAchievementsUseUncappedHealthAndCompanionsUseIntrinsicWeaponScaling()
        {
            var progress=new Progression(definitions);var run=Run(progress);
            run.GrantItem(catalog.data.items.Single(i=>i.id=="handcuffs"));run.GainWeaponStat("max_hp",100);
            run.Items.Add(catalog.data.items.Single(i=>i.id=="nail"));run.Inventory.Clear();
            for(int i=0;i<6;i++)run.Inventory.Add(catalog.Weapon("pistol",1));
            progress.CheckStats(run);
            Assert.AreEqual(10,run.MaxHealth);Assert.IsTrue(progress.Completed("chal_robust"));
            Assert.IsTrue(progress.Completed("chal_catling_gun"));
            Assert.IsFalse(progress.Completed("chal_bot_o_mine"),"Nail does not rewrite the raw WeaponData stats read by RunData.check_pet_chal.");
        }
        [Test] public void HourglassRequiresStrictlyLessThanFiveSecondsDuringTheUnderlyingWave()
        {
            var run=Run(new Progression(definitions));Assert.IsFalse(run.CanUnlockHourglassOnReturn);
            run.Tick(15);Assert.AreEqual(5,run.Remaining);Assert.IsFalse(run.CanUnlockHourglassOnReturn);
            run.Tick(.01f);Assert.IsTrue(run.CanUnlockHourglassOnReturn);
            run.Pause();Assert.IsTrue(run.CanUnlockHourglassOnReturn);run.Pause();
            run.Tick(run.Remaining);while(run.Phase==RunPhase.Upgrade)run.Choose(0);
            Assert.IsFalse(run.CanUnlockHourglassOnReturn);run.Pause();Assert.IsFalse(run.CanUnlockHourglassOnReturn);
        }
        [Test] public void HourglassUsesRemainingTimeEvenAfterTheFifthWave()
        {
            var run=Run(new Progression(definitions));run.SetShowcaseWave(8);run.Tick(run.Remaining-4);
            Assert.IsTrue(run.CanUnlockHourglassOnReturn);
            Assert.That(new Progression(definitions).Requirement("item_hourglass"),Does.Contain("seconds"));
        }
        [Test] public void ArmsDealerKeepsFractionalDiscountedWeaponBaseUntilFinalPriceConversion()
        {
            var run=new RunRules(21);run.Configure(catalog);var arms=catalog.data.characters.Single(c=>c.id=="arms_dealer");
            run.Start(arms,StarterChoices.Available(catalog,arms).First());
            Assert.AreEqual(-95,run.Stat("weapons_price"));
            Assert.AreEqual(5,run.Price(new ShopOffer{weapon=new Equipment{value=50,tier=1},rolledWave=2}),"2.5 discounted base + 2 + .5 inflation.");
            Assert.AreEqual(7,run.Price(new ShopOffer{weapon=catalog.Weapon("knife",4),rolledWave=2}),"Actual 91-base knife retains 4.55 before inflation.");
        }
        [Test] public void OrdinaryReferenceStartReceivesThirtyMaterialsWithoutPickupExperience()
        {
            var run=new RunRules(42);run.Configure(catalog);
            var character=catalog.data.characters.Single(c=>c.id=="well_rounded");
            run.Start(character,"pistol",RunRules.ReferenceStartingMaterials);
            Assert.AreEqual(30,run.Scrap);Assert.AreEqual(0,run.XP);Assert.AreEqual(1,run.Inventory.Count);
        }
        [Test] public void NeutralFixturesCanExplicitlyChooseAnEmptyStartingPurse()
        {
            var run=new RunRules(42);run.Configure(catalog);run.Start(startingMaterials:0);
            Assert.AreEqual(0,run.Scrap);Assert.AreEqual(0,run.XP);
        }
    }
}

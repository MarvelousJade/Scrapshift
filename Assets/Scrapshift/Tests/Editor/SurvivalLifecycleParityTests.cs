using System;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Scrapshift.Editor;

namespace Scrapshift.Tests
{
    public sealed class SurvivalLifecycleParityTests
    {
        ReferenceCatalog catalog;
        [SetUp] public void Setup()=>catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
        RunRules Run(){var run=new RunRules(771);run.Configure(catalog);run.Start(startingMaterials:100);return run;}
        [TestCase(15,4,2)] [TestCase(-15,4,6)] [TestCase(0,4,4)] [TestCase(1000,4,1)]
        public void IncomingDamageUsesSourceArmorCoefficientAndMinimumOne(float armor,float incoming,float expected)
        {var run=Run();run.GainWeaponStat("max_hp",100);float before=run.Health;run.GainWeaponStat("armor",armor);run.Hurt(incoming);Assert.AreEqual(before-expected,run.Health);}
        [Test] public void DodgePrecedesHitProtectionAndDoesNotConsumeItsCharge()
        {
            var run=Run();run.GainWeaponStat("dodge",100);run.GainWeaponStat("dodge_cap",100);
            // Protection is initialized by the next ordinary wave.
            run.GainWeaponStat("hit_protection",1);run.Tick(run.Remaining);run.NextWave();
            run.Hurt(9);Assert.IsTrue(run.LastHitDodged);Assert.AreEqual(1,run.HitProtection);Assert.AreEqual(run.MaxHealth,run.Health);
            run.GainWeaponStat("dodge",-100);run.Hurt(9);Assert.IsTrue(run.LastHitProtected);Assert.AreEqual(0,run.HitProtection);Assert.AreEqual(run.MaxHealth,run.Health);
            run.Hurt(2);Assert.AreEqual(run.MaxHealth-2,run.Health);
        }
        [Test] public void TortureBlocksOrdinaryHealingDuringCombatButAllowsCleanupHealing()
        {
            var run=Run();run.GainWeaponStat("torture",2);run.Hurt(6);run.Heal(3);Assert.AreEqual(4,run.Health);
            run.BeginWaveSettlement();run.Heal(3);Assert.AreEqual(7,run.Health);
            run.GainWeaponStat("no_heal",1);run.Heal(3);Assert.AreEqual(7,run.Health);
        }
        [Test] public void LethalDamageDoesNotHealOrSettleHarvestingAndHasAnAcknowledgmentDelay()
        {
            var run=Run();run.GainWeaponStat("harvesting",20);run.GainWeaponStat("hp_regeneration",1000);run.Hurt(1000);
            Assert.AreEqual(RunPhase.Lost,run.Phase);Assert.AreEqual(0,run.Health);Assert.IsTrue(run.DeathCleanupPending);
            run.Heal(100);run.Tick(0);Assert.AreEqual(0,run.Health);Assert.AreEqual(100,run.Scrap);Assert.AreEqual(0,run.XP);Assert.AreEqual(20,run.Stat("harvesting"));
            run.AdvanceDeathCleanup(.49f);Assert.IsTrue(run.DeathCleanupPending);run.AdvanceDeathCleanup(.02f);Assert.IsFalse(run.DeathCleanupPending);
        }
        [Test] public void DeathCleanupCanPauseButAcknowledgmentCannot()
        {
            var run=Run();run.Hurt(1000);run.AdvanceDeathCleanup(.1f);run.Pause();Assert.AreEqual(RunPhase.Paused,run.Phase);
            run.AdvanceDeathCleanup(1);Assert.AreEqual(.1f,run.DeathCleanupAge,.00001);run.Pause();Assert.AreEqual(RunPhase.Lost,run.Phase);
            run.AdvanceDeathCleanup(.41f);run.Pause();Assert.AreEqual(RunPhase.Lost,run.Phase);
        }
        [Test] public void RetryWaitsForCleanupAndRestoresCheckpointRatherThanStartingNewRun()
        {
            var run=Run();run.SetShowcaseWave(8);run.GainWeaponStat("max_hp",6);run.GainWeaponStat("range",17);run.CaptureWaveRetryState();
            int originalWeapons=run.Inventory.Count;float originalHealth=run.Health;
            run.Collect(27);run.GainWeaponStat("range",100);run.Inventory.Clear();run.Hurt(1000);
            Assert.IsFalse(run.CanRetryWave);Assert.IsFalse(run.RetryWave());run.AdvanceDeathCleanup(.51f);
            Assert.IsTrue(run.RetryWave());Assert.AreEqual(RunPhase.Playing,run.Phase);Assert.AreEqual(8,run.Wave);Assert.AreEqual(RunRules.Durations[7],run.Remaining);
            Assert.AreEqual(100,run.Scrap);Assert.AreEqual(0,run.XP);Assert.AreEqual(0,run.Level);Assert.AreEqual(17,run.Stat("range"));
            Assert.AreEqual(originalHealth,run.Health);Assert.AreEqual(originalWeapons,run.Inventory.Count);Assert.AreEqual(1,run.Retries);
        }
        [Test] public void RetryPreservesDuplicateItemsAndCopiesLockedOffersWithoutAliasing()
        {
            var run=Run();var coffee=Array.Find(catalog.data.items,item=>item.id=="coffee");run.GrantItem(coffee);run.GrantItem(coffee);
            run.Shop[0]=new ShopOffer{item=coffee,locked=true,rolledWave=3};run.CaptureWaveRetryState();
            run.RemoveItem(coffee);run.Shop[0].locked=false;run.Shop[0].rolledWave=19;run.Hurt(1000);run.AdvanceDeathCleanup(.51f);Assert.IsTrue(run.RetryWave());
            Assert.AreEqual(2,run.Items.Count);Assert.AreSame(coffee,run.Items[0]);Assert.AreSame(coffee,run.Items[1]);Assert.IsTrue(run.Shop[0].locked);Assert.AreEqual(3,run.Shop[0].rolledWave);
        }
        [Test] public void RetryRestoresInitialBankAndDoesNotRepeatStartWavePercentageIncome()
        {
            var run=Run();run.GainWeaponStat("gain_pct_gold_start_wave",50);run.BankMaterials(7);run.CaptureWaveRetryState();
            run.BankMaterials(13);run.Collect(9);run.Hurt(1000);run.AdvanceDeathCleanup(.51f);Assert.IsTrue(run.RetryWave());
            Assert.AreEqual(7,run.Banked);Assert.AreEqual(100,run.Scrap);
        }
        [Test] public void SubsequentRetriesIncrementAndAWholeNewRunResetsTheCheckpoint()
        {
            var run=Run();for(int attempt=1;attempt<=2;attempt++){run.CaptureWaveRetryState();run.Hurt(1000);run.AdvanceDeathCleanup(.51f);Assert.IsTrue(run.RetryWave());Assert.AreEqual(attempt,run.Retries);}
            run.Start();Assert.AreEqual(0,run.Retries);run.Hurt(1000);run.AdvanceDeathCleanup(.51f);Assert.IsFalse(run.CanRetryWave);
        }
        [Test] public void AbandonCannotUseRetryEvenWhenACheckpointExists()
        {var run=Run();run.CaptureWaveRetryState();run.Pause();run.Abandon();run.AdvanceDeathCleanup(1);Assert.IsFalse(run.CanRetryWave);Assert.IsFalse(run.RetryWave());}
        [Test] public void RetryKeepsGlobalProgressAndEventSubscriptionsLive()
        {
            var run=Run();var progress=new Progression(new[]{new UnlockDefinition{id="chal_rookie",name="Rookie",reward="rookie_reward"}});run.Progress=progress;run.CaptureWaveRetryState();progress.Complete("chal_rookie");
            int calls=0;run.RandomDamageRequested+=_=>calls++;run.Hurt(1000);run.AdvanceDeathCleanup(.51f);Assert.IsTrue(run.RetryWave());
            Assert.AreSame(progress,run.Progress);Assert.IsTrue(progress.Completed("chal_rookie"));
            run.Items.Add(new ItemEntry{id="retry-callback",effects=Array.Empty<StatEffect>(),conditionalEffects=new[]{new ConditionalEffect{customKey="dmg_when_heal",key="stat_max_hp",value=100,chance=1}}});
            run.Hurt(2);run.Heal(1);Assert.AreEqual(1,calls);
        }
    }
}

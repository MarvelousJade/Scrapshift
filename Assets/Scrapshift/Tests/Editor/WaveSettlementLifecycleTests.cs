using System;
using NUnit.Framework;
using UnityEditor;
using Scrapshift.Editor;

namespace Scrapshift.Tests
{
    public sealed class WaveSettlementLifecycleTests
    {
        RunRules Run(int wave=20)
        {
            var catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
            var run=new RunRules(771);run.Configure(catalog);run.Start(startingMaterials:50);run.SetShowcaseWave(wave);return run;
        }
        [Test] public void SettlementCreditsBeforeCleanupAndGrowthUsesResetEffectiveHarvesting()
        {
            var run=Run();run.GainWeaponStat("harvesting",20);
            run.Items.Add(new ItemEntry{effects=Array.Empty<StatEffect>(),conditionalEffects=new[]{
                new ConditionalEffect{key="stat_harvesting",value=20,customKey="temp_stats_per_interval",interval=1},
                new ConditionalEffect{key="stat_harvesting",value=5,customKey="stats_end_of_wave"}}});
            run.Tick(1);Assert.AreEqual(40,run.Stat("harvesting"));
            Assert.IsTrue(run.BeginWaveSettlement());
            Assert.AreEqual(95,run.Scrap,"End-wave permanent +5 applies before the 45-material income snapshot.");
            Assert.AreEqual(20,run.WaveSettlementReceiptWave);Assert.AreEqual(50,run.MaterialsBeforeSettlement);
            Assert.AreEqual(95,run.MaterialsAfterSettlement);Assert.AreEqual(45,run.HarvestingAtSettlement);
            Assert.AreEqual(25,run.Stat("harvesting"),"Temporary +20 resets after income, before growth.");
            Assert.AreEqual(RunPhase.Playing,run.Phase);Assert.AreEqual(0,run.Remaining);
            run.AdvanceWaveSettlement(.65f);Assert.AreEqual(25,run.Stat("harvesting"));
            run.AdvanceWaveSettlement(.011f);Assert.AreEqual(27,run.Stat("harvesting"));
            run.AdvanceWaveSettlement(1);Assert.AreEqual(27,run.Stat("harvesting"));
            Assert.IsFalse(run.BeginWaveSettlement());Assert.AreEqual(95,run.Scrap);
        }
        [TestCase(19,2f)] [TestCase(20,4f)]
        public void NoRewardsOrVictoryBeforeSourceCleanupTimer(int wave,float duration)
        {
            var run=Run(wave);run.BeginWaveSettlement();Assert.AreEqual(duration,run.WaveSettlementDuration);
            run.AdvanceWaveSettlement(duration-.01f);Assert.IsFalse(run.CompleteWaveSettlement());
            Assert.AreEqual(RunPhase.Playing,run.Phase);
            run.AdvanceWaveSettlement(.011f);Assert.IsTrue(run.CompleteWaveSettlement());
            Assert.AreEqual(wave==20?RunPhase.Won:RunPhase.Shop,run.Phase);
            Assert.IsFalse(run.CompleteWaveSettlement());
        }
        [Test] public void GrowthRecomputesEffectiveStatAfterCleanupChanges()
        {
            var run=Run();run.GainWeaponStat("harvesting",20);run.BeginWaveSettlement();
            run.GainWeaponStat("harvesting",20);run.AdvanceWaveSettlement(.66f);
            Assert.AreEqual(42,run.Stat("harvesting"));Assert.AreEqual(70,run.Scrap);
        }
        [Test] public void NoGrowthTimerWasScheduledWhenInitialIncomeHarvestingWasZero()
        {
            var run=Run();run.BeginWaveSettlement();run.GainWeaponStat("harvesting",20);
            run.AdvanceWaveSettlement(4);Assert.AreEqual(20,run.Stat("harvesting"));
        }
        [Test] public void OrdinaryTickCannotAdvanceCombatOrSettleTwiceWhileCleanupPending()
        {
            var run=Run();run.GainWeaponStat("harvesting",15);run.BeginWaveSettlement();run.Tick(100);
            Assert.AreEqual(RunPhase.Playing,run.Phase);Assert.AreEqual(0,run.WaveSettlementAge);
            Assert.AreEqual(65,run.Scrap);Assert.AreEqual(15,run.Stat("harvesting"));
            run.AdvanceWaveSettlement(4);run.CompleteWaveSettlement();run.Tick(100);
            Assert.AreEqual(65,run.Scrap);Assert.AreEqual(16,run.Stat("harvesting"));
        }
        [Test] public void PauseFreezesSettlementStages()
        {
            var run=Run();run.GainWeaponStat("harvesting",15);run.BeginWaveSettlement();run.Pause();
            run.AdvanceWaveSettlement(10);Assert.IsFalse(run.CompleteWaveSettlement());Assert.AreEqual(0,run.WaveSettlementAge);
            run.Pause();run.AdvanceWaveSettlement(4);Assert.IsTrue(run.CompleteWaveSettlement());
        }
        [Test] public void NextWaveAndRestartResetLifecycleState()
        {
            var run=Run(19);run.BeginWaveSettlement();run.AdvanceWaveSettlement(2);run.CompleteWaveSettlement();run.NextWave();
            Assert.IsFalse(run.WaveSettlementPending);Assert.AreEqual(0,run.WaveSettlementAge);
            run.BeginWaveSettlement();run.Start(startingMaterials:50);
            Assert.IsFalse(run.WaveSettlementPending);Assert.AreEqual(0,run.WaveSettlementAge);Assert.AreEqual(20,run.Remaining);
            Assert.AreEqual(0,run.WaveSettlementReceiptWave);
        }
        [Test] public void SceneFreeTickAdapterFastForwardsTheSameLifecycle()
        {
            var run=Run();run.GainWeaponStat("harvesting",15);run.Tick(run.Remaining);
            Assert.AreEqual(RunPhase.Won,run.Phase);Assert.AreEqual(65,run.Scrap);
            Assert.AreEqual(16,run.Stat("harvesting"));Assert.IsFalse(run.WaveSettlementPending);
            Assert.AreEqual(4,run.WaveSettlementAge);
        }
    }
}

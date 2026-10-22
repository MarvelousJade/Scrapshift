using NUnit.Framework;
using UnityEditor;
using Scrapshift.Editor;

namespace Scrapshift.Tests
{
    public sealed class FinalWaveSettlementTests
    {
        RunRules Run(int wave=20)
        {
            var catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
            var run=new RunRules(771);run.Configure(catalog);run.Start(startingMaterials:50);run.SetShowcaseWave(wave);return run;
        }
        // main._on_WaveTimer_timeout calls manage_harvesting before cleanup
        // determines Won; its gold/XP credit truncates the effective income.
        [TestCase(15,15,16)]
        [TestCase(0,0,0)]
        [TestCase(-5,-5,-5)]
        [TestCase(-100,-100,-100)]
        public void FinalTimerSettlesHarvestingOnceWithoutOpeningRewards(int harvesting,int income,int grown)
        {
            var run=Run();run.GainWeaponStat("harvesting",harvesting);
            int before=run.Scrap;run.Tick(run.Remaining);
            Assert.AreEqual(RunPhase.Won,run.Phase);
            Assert.AreEqual(System.Math.Max(0,before+income),run.Scrap);
            Assert.AreEqual(grown,run.Stat("harvesting"));
            int settled=run.Scrap,level=run.Level;run.Tick(100);run.DefeatBoss();
            Assert.AreEqual(settled,run.Scrap);Assert.AreEqual(level,run.Level);
            Assert.AreEqual(grown,run.Stat("harvesting"));
        }
        [Test] public void FinalHarvestingAwardsExperienceButDoesNotOpenUpgradeScreen()
        {
            var run=Run();run.GainWeaponStat("harvesting",100);run.Tick(run.Remaining);
            Assert.Greater(run.Level,0);Assert.Greater(run.PendingLevels,0);
            Assert.AreEqual(RunPhase.Won,run.Phase);
        }
        [Test] public void LastBossShortensTimerThenUsesTheSameSettlementBoundary()
        {
            var run=Run();run.GainWeaponStat("harvesting",15);int before=run.Scrap;
            run.DefeatBoss();Assert.AreEqual(RunPhase.Playing,run.Phase);Assert.AreEqual(.1f,run.Remaining,.00001f);
            run.Tick(.05f);Assert.AreEqual(RunPhase.Playing,run.Phase);Assert.AreEqual(before,run.Scrap);
            run.Tick(.051f);Assert.AreEqual(RunPhase.Won,run.Phase);Assert.AreEqual(before+15,run.Scrap);
            run.Tick(1);Assert.AreEqual(before+15,run.Scrap);
        }
        [Test] public void BossDefeatOutsideFinalWaveDoesNotEndOrShortenTheWave()
        {
            var run=Run(10);float before=run.Remaining;run.DefeatBoss();
            Assert.AreEqual(RunPhase.Playing,run.Phase);Assert.AreEqual(before,run.Remaining);
        }
        [Test] public void FinalIncomeIncludesLiveEnemyAndTreeBonusesBeforeTruncation()
        {
            var run=Run();run.GainWeaponStat("harvesting",1);run.GainWeaponStat("pacifist",100);
            run.GainWeaponStat("cryptid",2);run.LivingEnemies=3;run.RemovedEnemies=2;run.LivingTrees=2;
            int before=run.Scrap;run.Tick(run.Remaining);Assert.AreEqual(before+10,run.Scrap);
            Assert.AreEqual(RunPhase.Won,run.Phase);
        }
        [Test] public void OrdinaryWaveKeepsExistingSettlementAndRewardsFlow()
        {
            var run=Run(19);run.GainWeaponStat("harvesting",15);int before=run.Scrap;
            run.Tick(run.Remaining);Assert.AreEqual(before+15,run.Scrap);
            Assert.AreEqual(16,run.Stat("harvesting"));Assert.AreNotEqual(RunPhase.Won,run.Phase);
        }
    }
}

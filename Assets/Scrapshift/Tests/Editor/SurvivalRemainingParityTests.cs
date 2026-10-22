using NUnit.Framework;
using UnityEditor;
using Scrapshift.Editor;

namespace Scrapshift.Tests
{
    public sealed class SurvivalRemainingParityTests
    {
        RunRules Run()
        {
            var catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
            var run=new RunRules(771);run.Configure(catalog);run.Start(startingMaterials:100);return run;
        }
        [Test] public void PeriodicLethalSelfDamageUsesTheSameDelayedCleanupAsAnEnemyHit()
        {
            var run=Run();run.GainWeaponStat("lose_hp_per_second",20);run.GainWeaponStat("harvesting",15);run.CaptureWaveRetryState();
            run.Tick(1);
            Assert.AreEqual(RunPhase.Lost,run.Phase);Assert.AreEqual(0,run.Health);Assert.IsTrue(run.DeathCleanupPending);
            Assert.AreEqual(0,run.DeathCleanupAge);Assert.IsFalse(run.CanRetryWave);
            Assert.AreEqual(100,run.Scrap);Assert.AreEqual(0,run.XP);Assert.AreEqual(15,run.Stat("harvesting"));
            run.Pause();run.Tick(1);Assert.AreEqual(RunPhase.Paused,run.Phase);Assert.AreEqual(0,run.DeathCleanupAge);run.Pause();
            run.AdvanceDeathCleanup(.5f);Assert.IsTrue(run.CanRetryWave);Assert.IsTrue(run.RetryWave());Assert.AreEqual(10,run.Health);
        }
        [Test] public void PeriodicSelfDamageConsumesProtectionBeforeOneHitDeath()
        {
            var run=Run();run.GainWeaponStat("hit_protection",1);run.Tick(run.Remaining);run.NextWave();
            run.GainWeaponStat("die_in_one_hit",1);run.GainWeaponStat("lose_hp_per_second",1);
            run.Tick(1);Assert.AreEqual(RunPhase.Playing,run.Phase);Assert.IsTrue(run.LastHitProtected);Assert.AreEqual(0,run.HitProtection);
            run.Tick(1);Assert.AreEqual(RunPhase.Lost,run.Phase);Assert.IsTrue(run.DeathCleanupPending);
        }
    }
}

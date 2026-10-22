using System;
using NUnit.Framework;
using UnityEditor;
using Scrapshift.Editor;

namespace Scrapshift.Tests
{
    public sealed class SurvivalCharacterBoundaryTests
    {
        ReferenceCatalog catalog;
        [SetUp] public void Setup()=>catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
        RunRules Character(string id)
        {
            var character=Array.Find(catalog.data.characters,c=>c.id==id);Assert.NotNull(character,id);
            var run=new RunRules(71);run.Configure(catalog);run.Start(character,StarterChoices.Available(catalog,character)[0]);return run;
        }
        [Test] public void ImportedSickPeriodicDamageCanCauseNormalDelayedDefeat()
        {
            var run=Character("sick");Assert.AreEqual(1,run.Stat("lose_hp_per_second"));
            run.GainWeaponStat("dodge",-1000);run.GainWeaponStat("armor",-run.Stat("armor"));
            run.Hurt(run.Health-1);Assert.AreEqual(1,run.Health);run.Tick(1);
            Assert.AreEqual(0,run.Health);Assert.AreEqual(RunPhase.Lost,run.Phase);Assert.IsTrue(run.DeathCleanupPending);
        }
        [Test] public void ImportedWoundedDiesFromOneUnblockedHitAndRetryRestoresItsAbilities()
        {
            var run=Character("wounded");Assert.Greater(run.Stat("die_in_one_hit"),0);run.CaptureWaveRetryState();
            run.GainWeaponStat("dodge",-1000);run.Hurt(1);Assert.IsTrue(run.LastHitProtected);Assert.AreEqual(RunPhase.Playing,run.Phase);
            run.Hurt(1);Assert.AreEqual(0,run.Health);Assert.IsTrue(run.DeathCleanupPending);
            run.AdvanceDeathCleanup(.5f);Assert.IsTrue(run.RetryWave());Assert.Greater(run.Stat("die_in_one_hit"),0);
            Assert.AreEqual(run.MaxHealth,run.Health);Assert.AreEqual("wounded",run.Character.id);
        }
        [Test] public void ImportedGolemCannotHealFromOrdinaryTortureOrCleanupHealing()
        {
            var run=Character("golem");Assert.Greater(run.Stat("no_heal"),0);run.GainWeaponStat("dodge",-1000);run.Hurt(3);float hp=run.Health;
            run.Heal(10);run.Heal(10,true);Assert.AreEqual(hp,run.Health);
            run.BeginWaveSettlement();run.Heal(10);Assert.AreEqual(hp,run.Health);
        }
    }
}

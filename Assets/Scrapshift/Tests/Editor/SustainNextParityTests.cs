using System.Linq;
using NUnit.Framework;
using UnityEditor;
using Scrapshift.Editor;

namespace Scrapshift.Tests
{
    public sealed class SustainNextParityTests
    {
        ReferenceCatalog catalog;
        [SetUp] public void Setup()=>catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
        RunRules Run(){var run=new RunRules(77);run.Configure(catalog);run.Start();return run;}
        [TestCase(1.9f,5)]
        [TestCase(10.9f,1)]
        [TestCase(.9f,99)]
        [TestCase(-.9f,99)]
        public void RegenerationPeriodCastsTheEffectiveStatBeforeApplyingTheSourceFormula(float stat,double expected)
            =>Assert.AreEqual(expected,RunRules.RegenerationInterval(stat),.000001);
        [Test] public void ExternalFullHealingDoesNotEraseTheRunningRegenerationTimer()
        {
            var run=Run();run.GainWeaponStat("hp_regeneration",1);run.Hurt(4);run.Tick(4);
            run.Heal(100);run.Tick(.5f);run.Hurt(2);float before=run.Health;
            run.Tick(.49f);Assert.AreEqual(before,run.Health);
            run.Tick(.01f);Assert.AreEqual(before+1,run.Health);
        }
        [Test] public void ARegenerationTimeoutAtFullHealthStopsTheTimerUntilNewDamage()
        {
            var run=Run();run.GainWeaponStat("hp_regeneration",1);run.Hurt(4);run.Tick(4);
            run.Heal(100);run.Tick(1);run.Hurt(2);float before=run.Health;
            run.Tick(4.9f);Assert.AreEqual(before,run.Health);
            run.Tick(.1f);Assert.AreEqual(before+1,run.Health);
        }
        [Test] public void DamageStartsTheTimerEvenWhenAnotherEffectHealsItBeforeTheFirstTick()
        {
            var run=Run();run.GainWeaponStat("hp_regeneration",1);run.Hurt(1);run.Heal(100);
            run.Tick(4.9f);run.Hurt(2);float before=run.Health;run.Tick(.1f);
            Assert.AreEqual(before+1,run.Health);
        }
        [Test] public void DisablingRegenerationStopsItsTimerAndReenablingStartsFresh()
        {
            var run=Run();run.GainWeaponStat("hp_regeneration",1);run.Hurt(4);run.Tick(4);
            run.GainWeaponStat("hp_regeneration",-1);run.Tick(.1f);
            run.GainWeaponStat("hp_regeneration",1);float before=run.Health;
            run.Tick(4.9f);Assert.AreEqual(before,run.Health);run.Tick(.1f);Assert.AreEqual(before+1,run.Health);
        }
        [Test] public void MultipleTimerLoopsSnapshotPotionThresholdAndEmitOneHealingEvent()
        {
            var run=Run();run.Items.Add(catalog.data.items.Single(i=>i.id=="potion"));
            run.GainWeaponStat("hp_regeneration",1000);run.Hurt(6);
            run.Items.Add(new ItemEntry{id="sustain-heal-event",effects=System.Array.Empty<StatEffect>(),conditionalEffects=new[]{new ConditionalEffect{customKey="dmg_when_heal",key="stat_max_hp",value=100,chance=1}}});
            int callbacks=0;run.RandomDamageRequested+=_=>callbacks++;
            run.Tick(.03f); // Two FixedTimer loops inside one physics-sized update.
            Assert.AreEqual(8,run.Health,"Both loops use the below-half-health multiplier sampled before the batch.");
            Assert.AreEqual(1,callbacks,"One source on_healing_effect event must cause one Lich-style callback.");
        }
        [Test] public void MothAndPotionShareTheSameAdditiveBonusArray()
        {
            // doc_moth_effect_1.tres and potion_effect_1.tres both append hp_regen_bonus.
            var run=Run();run.Items.Add(catalog.data.items.Single(i=>i.id=="potion"));run.HealingDrones=1;
            run.GainWeaponStat("hp_regeneration",1);run.Hurt(6);
            Assert.AreEqual(3,run.RegenerationHealMultiplier);run.Tick(5);Assert.AreEqual(7,run.Health);
            Assert.AreEqual(2,run.RegenerationHealMultiplier,"Only Potion's conditional entry expires above its HP threshold.");
        }
        [Test] public void TortureOverridesNegativeRegenerationButStillHonorsNoHeal()
        {
            var run=Run();run.GainWeaponStat("torture",2);run.GainWeaponStat("hp_regeneration",-10);run.Hurt(6);
            run.Heal(3);Assert.AreEqual(4,run.Health);run.Tick(1);Assert.AreEqual(6,run.Health);
            run.GainWeaponStat("no_heal",1);run.Tick(1);Assert.AreEqual(6,run.Health);
            run.GainWeaponStat("no_heal",-1);run.Tick(1);Assert.AreEqual(8,run.Health);
        }
        [Test] public void FullHealthLifeStealStillConsumesItsSharedCooldown()
        {
            var run=Run();run.GainWeaponStat("lifesteal",100);Assert.IsFalse(run.TryLifeSteal(0));
            run.Hurt(2);Assert.IsFalse(run.TryLifeSteal(0));run.Tick(.1f);Assert.IsTrue(run.TryLifeSteal(0));
            Assert.AreEqual(9,run.Health);
        }
        [Test] public void MaximumHealthGrowthBypassesHealingRestrictionsAndHealingCallbacks()
        {
            var run=Run();run.Hurt(4);run.GainWeaponStat("no_heal",1);run.GainWeaponStat("torture",2);
            run.Items.Add(new ItemEntry{id="hp-callback",effects=System.Array.Empty<StatEffect>(),conditionalEffects=new[]{new ConditionalEffect{customKey="dmg_when_heal",key="stat_max_hp",value=100,chance=1}}});
            int callbacks=0;run.RandomDamageRequested+=_=>callbacks++;
            run.GainWeaponStat("max_hp",5);
            Assert.AreEqual(15,run.MaxHealth);Assert.AreEqual(11,run.Health);Assert.AreEqual(0,callbacks);
        }
        [Test] public void MaximumHealthReductionRetainsCurrentHealthUntilDamageOrExplicitHealing()
        {
            var run=Run();run.GainWeaponStat("max_hp",-5);
            Assert.AreEqual(5,run.MaxHealth);Assert.AreEqual(10,run.Health);
            run.Hurt(2);Assert.AreEqual(8,run.Health);
            run.Heal(1);Assert.AreEqual(5,run.Health,"Source on_healing_effect permits a negative actual_value above Max HP.");
        }
        [Test] public void DirtyStatBatchUsesItsNetMaximumInsteadOfTransientRises()
        {
            var run=Run();run.Hurt(4);
            run.GrantItem(new ItemEntry{id="net-hp-batch",effects=new[]{new StatEffect{key="max_hp",value=8},new StatEffect{key="max_hp",value=-6}},conditionalEffects=System.Array.Empty<ConditionalEffect>()});
            Assert.AreEqual(12,run.MaxHealth);Assert.AreEqual(8,run.Health);
        }
        [Test] public void RunningRegenerationDoesNotReduceHealthAboveTheNewMaximum()
        {
            var run=Run();run.GainWeaponStat("hp_regeneration",1);run.Hurt(1);run.Tick(4);
            run.GainWeaponStat("max_hp",-5);Assert.AreEqual(9,run.Health);
            run.Tick(1);Assert.AreEqual(9,run.Health,"on_health_regen clamps its request to zero before on_healing_effect.");
        }
        [Test] public void PaddingAndLevelUpsIncreaseCurrentHealthByTheirCombinedEffectiveMaximum()
        {
            var run=Run();run.GrantItem(catalog.data.items.Single(i=>i.id=="padding"));run.Hurt(4);
            float before=run.Health,maximum=run.MaxHealth;run.Collect(100);
            Assert.Greater(run.MaxHealth,maximum);Assert.AreEqual(before+run.MaxHealth-maximum,run.Health);
        }
        [Test] public void HealthCapSuppressesGrowthUntilTheCapItemIsRemoved()
        {
            var run=Run();var cuffs=catalog.data.items.Single(i=>i.id=="handcuffs");run.GrantItem(cuffs);run.Hurt(4);
            float before=run.Health,maximum=run.MaxHealth;run.GainWeaponStat("max_hp",10);
            Assert.AreEqual(maximum,run.MaxHealth);Assert.AreEqual(before,run.Health);
            run.RemoveItem(cuffs);Assert.AreEqual(before+run.MaxHealth-maximum,run.Health);
        }
        [Test] public void TemporaryMaximumHealthTracksGrowthButDoesNotClampWhenMovementEnds()
        {
            var run=Run();run.Hurt(4);
            run.GrantItem(new ItemEntry{id="moving-hp",effects=System.Array.Empty<StatEffect>(),conditionalEffects=new[]{new ConditionalEffect{customKey="temp_stats_while_moving",key="max_hp",value=5}}});
            run.SetMoving(true);Assert.AreEqual(15,run.MaxHealth);Assert.AreEqual(11,run.Health);
            run.SetMoving(false);Assert.AreEqual(10,run.MaxHealth);Assert.AreEqual(11,run.Health);
            run.Tick(20);while(run.Phase==RunPhase.Upgrade)run.Choose(0);
            Assert.AreEqual(RunPhase.Shop,run.Phase);Assert.AreEqual(run.MaxHealth,run.Health);
            run.NextWave();Assert.AreEqual(run.MaxHealth,run.Health);
        }
        [Test] public void RestartAndWaveStartPercentageDoNotAccumulateInitializationDeltas()
        {
            var run=Run();run.GainWeaponStat("max_hp",30);Assert.AreEqual(40,run.Health);
            run.Start();Assert.AreEqual(10,run.Health);
            run.GainWeaponStat("hp_start_wave",-50);run.Tick(20);while(run.Phase==RunPhase.Upgrade)run.Choose(0);
            run.GainWeaponStat("max_hp",10);Assert.AreEqual(20,run.Health);
            run.NextWave();Assert.AreEqual(10,run.Health);run.Tick(.01f);Assert.AreEqual(10,run.Health);
        }
    }
}

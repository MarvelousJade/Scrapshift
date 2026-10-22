using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using Scrapshift.Editor;

namespace Scrapshift.Tests
{
    public sealed class ItemsCombinedParityTests
    {
        ReferenceCatalog catalog;

        [SetUp] public void SetUp() => catalog = AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root + "/Definitions/ReferenceCatalog.asset");

        RunRules Run()
        {
            var run = new RunRules(1701);
            run.Configure(catalog);
            run.Start();
            return run;
        }

        ItemEntry Item(string id) => catalog.data.items.Single(item => item.id == id);

        [Test] public void StackedCooldownItemsAndPenaltyShareHalfSecondBurnTimer()
        {
            // unit.tscn BurningTimer = 0.5 s; unit.gd:init sums both modifiers
            // before multiplying the base timer, with a 0.1 s floor.
            var run = Run();
            var surgery = Item("eyes_surgery");
            var heart = Item("frozen_heart");
            Assert.AreEqual(.5f, run.BurningInterval, .0001f);
            run.GrantItem(surgery);
            run.GrantItem(surgery);
            Assert.AreEqual(.3f, run.BurningInterval, .0001f);
            run.GrantItem(heart);
            Assert.AreEqual(.8f, run.BurningInterval, .0001f);
            Assert.IsTrue(run.RemoveItem(surgery));
            Assert.AreEqual(.9f, run.BurningInterval, .0001f);
            Assert.IsTrue(run.RemoveItem(heart));
            Assert.AreEqual(.4f, run.BurningInterval, .0001f);
        }

        [Test] public void ScaredSausageCopiesMergeGlobalBurnButKeepNativeBurnTiming()
        {
            // BurningData.merge sums chance/damage and takes the longest duration;
            // WeaponService.init_burning_data keeps a native burn's own chance,
            // duration and scaling, adding only the merged global base damage.
            var run = Run();
            var sausage = Item("scared_sausage");
            run.GrantItem(sausage);
            run.GrantItem(sausage);
            run.GainWeaponStat("elemental_damage", 10);
            var plain = new Equipment();
            var globalBurn = run.CaptureBurn(plain);
            Assert.AreEqual(.5f, globalBurn.chance, .0001f);
            Assert.AreEqual(3, globalBurn.ticks);
            Assert.AreEqual(12, globalBurn.damage);
            var native = new Equipment { traits = new[] {
                new WeaponTrait { key = "effect_burning", burnChance = .3f, burnDamage = 4, burnTicks = 2, burnScale = .5f, burnStat = "elemental_damage" }
            } };
            var mixed = run.CaptureBurn(native);
            Assert.AreEqual(.3f, mixed.chance, .0001f);
            Assert.AreEqual(2, mixed.ticks);
            Assert.AreEqual(11, mixed.damage);
            Assert.IsTrue(run.RemoveItem(sausage));
            Assert.AreEqual(.25f, run.CaptureBurn(plain).chance, .0001f);
            Assert.AreEqual(10, run.CaptureBurn(native).damage);
        }

        [Test] public void MaximumHealthChangeReevaluatesConditionalRegenAndMissingHealthLink()
        {
            // main.gd:check_half_health_stats compares current health to current
            // maximum; player.gd:on_health_regen checks each Potion threshold.
            // linked_stats.gd computes the missing-health source anew.
            var run = Run();
            run.Items.Add(new ItemEntry { effects = Array.Empty<StatEffect>(), conditionalEffects = new[] {
                new ConditionalEffect { customKey = "stats_below_half_health", key = "stat_armor", value = 4 },
                new ConditionalEffect { customKey = "stat_links", key = "stat_percent_damage", stat = "percent_player_missing_health", value = 1, per = 1 }
            } });
            run.GrantItem(Item("potion"));
            run.Hurt(6);
            Assert.AreEqual(4, run.Health);
            Assert.AreEqual(4, run.Stat("armor"));
            Assert.AreEqual(60, run.Stat("percent_damage"));
            Assert.AreEqual(2, run.RegenerationHealMultiplier);
            run.GainWeaponStat("max_hp", -4);
            Assert.AreEqual(6, run.MaxHealth);
            Assert.AreEqual(4, run.Health);
            Assert.AreEqual(0, run.Stat("armor"));
            Assert.AreEqual(33, run.Stat("percent_damage"));
            Assert.AreEqual(1, run.RegenerationHealMultiplier);
        }

        [Test] public void MixedStatLinksReadBaseStatsWithoutFeedingEachOther()
        {
            // linked_stats.gd:reset_player reads RunData.get_stat plus TempStats;
            // linked contributions are collected in a separate LinkedStats pass.
            var run = Run();
            run.GainWeaponStat("armor", 3);
            run.GainWeaponStat("speed", 5);
            float baseArmor = run.Stat("armor"), baseSpeed = run.Stat("speed");
            run.Items.Add(new ItemEntry { effects = Array.Empty<StatEffect>(), conditionalEffects = new[] {
                new ConditionalEffect { customKey = "stat_links", key = "stat_armor", stat = "stat_speed", value = 1, per = 1 },
                new ConditionalEffect { customKey = "stat_links", key = "stat_speed", stat = "stat_armor", value = 1, per = 1 }
            } });
            Assert.AreEqual(baseArmor + baseSpeed, run.Stat("armor"));
            Assert.AreEqual(baseSpeed + baseArmor, run.Stat("speed"));
            run.GainWeaponStat("speed", 2);
            Assert.AreEqual(baseArmor + baseSpeed + 2, run.Stat("armor"));
            Assert.AreEqual(baseSpeed + 2 + baseArmor, run.Stat("speed"));
        }

        [Test] public void PotionBoostedRegenerationTriggersOneCombinedHealingAttack()
        {
            // player.gd:on_health_regen groups the Potion-boosted healing event;
            // main.gd:handle_stat_damages sums both damage entries for one enemy.
            var run = Run();
            run.GrantItem(Item("potion"));
            run.Items.Add(new ItemEntry { effects = Array.Empty<StatEffect>(), conditionalEffects = new[] {
                new ConditionalEffect { customKey = "dmg_when_heal", key = "stat_max_hp", value = 100, chance = 1 },
                new ConditionalEffect { customKey = "dmg_when_heal", key = "stat_max_hp", value = 100, chance = 1 }
            } });
            int requests = 0;
            float damage = 0;
            run.RandomDamageRequested += value => { requests++; damage = value; };
            run.Hurt(6);
            run.Tick(2);
            Assert.AreEqual(4, run.Health);
            Assert.AreEqual(0, requests);
            run.Tick(1);
            Assert.AreEqual(6, run.Health);
            Assert.AreEqual(1, requests);
            Assert.AreEqual(20, damage);
        }
    }
}

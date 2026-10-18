# Registered base item effect and stack coverage

Date: 2026-10-15, America/Toronto. Branch: `codex/gameplay-parity`.

This report covers source assignments for all items registered by the supplied base `singletons/item_service.tscn`. It checks dispatch and stacking semantics against runtime code; it does not establish exact native behavior for every item or replace full-run comparison.

## Inventory and method

- Source: `E:/UnityProjects/brotato-(3.x)` (read-only).
- 209 registered resources / 209 distinct identities; 500 resolved source effect assignments.
- 209 runtime catalog identities; 419 scalar rows and 81 conditional rows = 500 rows.
- All source identities occur in the runtime catalog. Every item has the same total effect-row count as its resolved source resource.
- 60 scalar keys (including one intentional empty spent-item effect); 35 conditional trigger types. Every nonempty registered scalar has a runtime consumer and every conditional type has an executable handler listed below.
- Source resource inventory was resolved afresh with `scripts/analyze_reference.py:resource`; no reference code was executed.
- Registry SHA-256: `1d4f2255c696ebcebda202a8954dd583ba0f8418668a37681e75665c0a79195e`.

## Corrected during this pass

| Behavior | Source evidence | Correction |
|---|---|---|
| Rip and Tear / Spicy Sauce duplicates | `singletons/run_data.gd:1667` `handle_explode_effect` | Sum chances once, then sum every entry’s separately calculated damage into one explosion. Preserve the first effect’s radius and hit properties. |
| Nail / Frozen Heart with item explosions | `singletons/weapon_service.gd:281` `get_explosion_damage` | Include added weapon scaling for explosions; retain the special-projectile exclusion for Alien Eyes and Baby with a Beard. |
| Current-health critical bonus | `entities/units/enemies/enemy.gd:288` | Truncate each bonus separately before summing. Giant Belt itself has shop maxCount 1; duplicate-effect test checks the general effect-array rule. |
| Self-damage with Tardigrade | `entities/units/player/player.gd:300` and `:844` | Blood Donation / Bloody Hand bypass dodge and armor but consume protection. Blocked ticks do not grant on-hit stats or reset no-hit bonuses. Successful ticks respect one-hit-death characters. |
| Multiple Adrenaline effects with Lich | `entities/units/player/player.gd:455` | Sum successful healing entries and heal once, producing one healing-trigger attack. |
| Riposte damage rounding | `entities/units/player/player.gd:440`, `singletons/weapon_service.gd:493` | Truncate base stat damage then round each entry’s percent-scaled result; apply minimum 1. |
| Medicine progress lifetime | `entities/units/player/player.gd:790` | Reset tracked healing on each wave start. Healing amount semantics are integrated separately by the root agent. |
| Removing an item before its queued effect runs | `effects/items/extra_enemy_effect.gd:28`, `items/global/effect.gd:unapply`, `main.gd:1447` | Cancel one removed Bait's pending group and subtract a removed next-wave stat entry only while it remains queued. Preserve a bonus already applied during the active wave. |

## Meaningful stack checks

`Assets/Scrapshift/Tests/Editor/ItemStackParityTests.cs` contains 13 focused regression cases: summed explosion chance, guaranteed two-copy Sauce proc, separately rounded explosion damage, explosion added scaling, separately truncated current-HP bonuses, independent burning-kill caps and wave reset, distinct item link identities and removal, duplicate interval reset-on-hit state, self-damage protection, aggregated dodge healing, Riposte rounding, queued next-wave-stat cancellation, and queued Bait cancellation. The last two include removal after queue consumption to prevent phantom negative effects.

The existing `TriggeredImplantTests`, `ConditionalEffectTests`, `LinkedImplantTests`, `WaveImplantTests`, `ItemLifecycleTests`, `ImplantAttackTests`, and `StructureImplantTests` cover additional named examples. The latest combined `remaining-parity-current-source.xml` passed **503 tests, 0 failed, 0 skipped in 81.3536114 seconds**. Its copied `artifacts/gameplay-parity-native-2026-10-15/editmode-final.xml` was inspected. Counts include all 13 item-stack and 12 special-shop cases, plus 13 controls, 14 unlock and 24 material cases. The original XML is under `E:/Codex Projects/Job Search Assistant/tmp/ScrapshiftParity/artifacts/`.

`artifacts/gameplay-parity-native-2026-10-15/current-source-manifest.json` was inspected: all 444 listed C#/JSON/assembly and C# metadata files match the isolated clone by SHA-256. This checkpoint includes optional real-time campaign/controller logging and fresh in-memory fixture options/progression; fixtures do not read or write the normal user profile.

`SpecialShopParityTests.cs` contains 12 generated cases (11 methods, one with two `TestCase` attributes). Three cases added after the historical 467-test result cover a paid reroll with a guaranteed stat loss and a zero-chance effect, duplicate purchase with one remaining target slot, and unlimited duplicate purchase consuming all source copies. The theft queue test uses the real regular Bait group; the separate elite-template test checks selected-elite materialization. All passed in the 503-test checkpoint.

The special-shop keys `increase_tier_on_reroll`, `gain_stats_on_reroll`, `duplicate_item`, `item_steals` and theft-consequence effects have no registered base item assignments in this supplied source. Their fixtures are synthetic inputs exercising generic source hooks. They are not evidence of supplied DLC item integration or additional base catalog identities.

## Remaining mismatches and limits

- No missing executable dispatch key was found among the 500 registered base assignments after these corrections. This is a dispatch result, not proof that all branch paths behave identically.
- Removal/unapply integration: `LootRules.RemoveItem` restores unlimited caps, removes scalar bonuses, and calls `WaveImplants.CancelQueuedItemEffects` for pending next-wave stats and Bait groups. Normal base shop interaction does not remove those items, but generic duplicate-source consumption can expose this edge case. Both queue-cancellation regression cases passed in the 503-test checkpoint.
- Support effects have nested scene behavior beyond the top-level 500 assignments. Multiplicity dispatch was traced, but complete native movement, collision, aiming, random timing, Doc Moth area enter/exit and all companion combinations were not replayed during this audit.
- `enemy_percent_damage_taken` is presently specialized to the registered Ice Cube assignment (Elemental damage, three seconds, one stack, one proc). Generic multi-source vulnerability stacking from DLC or unregistered weapons is not implemented by that specialized representation; no additional registered base-item assignment requires it.
- Source linked-stat support includes `duplicate_item` and `free_weapon_slots`; none of the 13 registered base-item links uses either. Their absence from `ConditionalEffects.LinkSource` remains a scope limitation if future content registers them.
- Beyond-wave-20 Endless health-percent attenuation is absent from Giant Belt / Greek Fire calculations. Current run rules end at wave 20; do not claim Endless parity.
- Native timing, visuals, sounds, seeded RNG sequences, and a complete ordinary run against the original remain separate checks. Scene-free effect assertions cannot establish them.
- The 503-test checkpoint includes pickup motion, Evil Mob / Lootworm collector ownership, contact-entry consumption and exit/reentry, damage-triggered consumable attraction, and continued attraction after health becomes full. Normal-time full campaigns have passed wave 15 and remain in progress; no completed corrected-run claim is made here. An XboxOne controller was detected, but verification awaits the user's physical button/stick sequence. Scene-free assertions and device detection do not establish ordinary-run balance or controller compatibility.
- The preceding full suite passed 502/503. Its one failed assertion required exact decimal tangency in the pickup collision fixture; the corrected fixture checks points safely inside and outside contact because the source physics does not specify exact decimal tangency. Runtime code was unchanged for that correction, and no arithmetic cause was established by the failed assertion.

## Conditional dispatch coverage

| Trigger | Assigned rows | Runtime entry point |
|---|---:|---|
| `alien_eyes` | 1 | `ImplantAttacks.TickImplantAttacks` |
| `burning_enemy_hp_percent_damage` | 1 | `CharacterMechanics.BurnCurrentHealthDamage` |
| `consumable_stats_while_max` | 1 | `TriggeredImplants.ConsumableFullHealthTriggers` |
| `dmg_on_dodge` | 1 | `ImplantAttacks.RetaliationTriggers` |
| `dmg_when_death` | 1 | `WaveImplants.RandomDamageTrigger` |
| `dmg_when_pickup_gold` | 1 | `WaveImplants.RandomDamageTrigger` |
| `enemy_percent_damage_taken` | 1 | `ImplantAttacks.TryVulnerability` |
| `explode_on_consumable` | 1 | `ImplantAttacks.TryItemExplosion` |
| `explode_on_death` | 1 | `ImplantAttacks.TryItemExplosion` |
| `extra_enemies_next_wave` | 1 | `LootRules.ApplyItem / CharacterMechanics.BeginCharacterWave / ExtendedEncounters` |
| `gain_random_primary_stats_on_go_to_next_wave` | 1 | `CharacterMechanics.BeginCharacterWave` |
| `gain_stat_for_equipped_item_with_stat` | 1 | `TriggeredImplants.OnItemStatAdded` |
| `gain_stat_for_killed_enemies_while_burning` | 1 | `WaveImplants.KillEffects` |
| `giant_crit_damage` | 1 | `TriggeredImplants.CriticalCurrentHealthDamage` |
| `global_burn` | 1 | `ItemLifecycle.RollBurn` |
| `gold_on_crit_kill` | 1 | `TriggeredImplants.CriticalKillMaterials` |
| `heal_on_dodge` | 1 | `TriggeredImplants.DodgeTriggers` |
| `hp_regen_bonus` | 1 | `TriggeredImplants.RegenerationHealMultiplier` |
| `minimum_weapon_cooldowns` | 1 | `TriggeredImplants.MinimumWeaponCooldown` |
| `pet` | 9 | `CyberneticPets.ResetPets / PetBehaviors / PetCombat; jellyshield uses scalar count` |
| `projectiles_on_death` | 1 | `ImplantAttacks.EnemyImplantDeath` |
| `remove_speed` | 1 | `TriggeredImplants.ItemSpeedReduction` |
| `stat_cap` | 2 | `LootRules.ApplyItem / RunRules cap fields` |
| `stat_links` | 13 | `ConditionalEffects.LinkedStat / LinkSource` |
| `stats_end_of_wave` | 8 | `ConditionalEffects.PermanentTrigger via RunRules` |
| `stats_next_wave` | 4 | `WaveImplants.QueueNextWaveEffects / ApplyNextWaveEffects` |
| `stats_on_level_up` | 2 | `ConditionalEffects.PermanentTrigger via RunRules` |
| `structure` | 9 | `StarterWeaponEffects.ResetStarterEffects / ItemStructures.TickStructure` |
| `structures_cooldown_reduction` | 1 | `TriggeredImplants.StructureAttackSpeed` |
| `temp_stats_on_hit` | 1 | `ConditionalEffects.DamageTriggers` |
| `temp_stats_per_interval` | 2 | `ConditionalEffects.TickConditionalEffects` |
| `temp_stats_while_not_moving` | 3 | `ConditionalEffects.ConditionalStat / CharacterMechanics.TickCharacter` |
| `unique_weapon_effects` | 3 | `ConditionalEffects.ConditionalStat` |
| `upgrade_random_weapon` | 1 | `WaveImplants.UpgradeWeaponImplants` |
| `weapon_scaling_stats` | 2 | `TriggeredImplants.AddedWeaponScaling` |

## Scalar consumption coverage

Primary `stat_*` values are summed on acquisition by `LootRules.ApplyItem`, read by `RunRules.Stat`, and combined with character, equipment, conditional and linked modifiers. Secondary values feed the following systems; duplicate scalar entries sum except source replacement semantics such as Ghost Outfit’s capped, unique Dodge cap.

| Scalar family | Runtime consumers |
|---|---|
| Primary combat, HP and movement stats | `RunRules`, `CharacterProfile`, `SurvivalRules`, `ReferenceCombat` |
| XP, shop prices, recycling, free rerolls, harvesting, wave income | `RunRules`, `ItemLifecycle`, `LootRules` |
| Projectile bounce/piercing/knockback, explosion and burning modifiers | `ReferenceCombat`, `StarterWeaponEffects`, `WeaponReference`, `ImplantAttacks`, `ItemLifecycle` |
| Enemy count, health, speed, elites, trees, extra loot aliens | `ArenaGame`, `ExtendedEncounters`, `SpawnDirector`, `ItemLifecycle`, `CharacterMechanics` |
| Gold-drop amount, pickup attraction, pickup heal/double, crate gold, fruit drops | `ReferenceCombat`, `RunRules`, `ReferenceLoot`, `LootRules`, `ItemLifecycle` |
| Hit protection, starting HP, Torture, self-damage, dodge cap | `RunRules`, `SurvivalRules`, `ItemLifecycle`, `ConditionalEffects` |
| Structure speed/crit, Pocket Factory, jellyshield, fog, Hourglass | `TriggeredImplants`, `ItemStructures`, `CyberneticPets`, `ExtendedEncounters`, `CharacterMechanics` |

## Every registered item

This table records source assignment count, allowed maximum copies (`-1` means unlimited), and runtime effect keys. A listed handler means the effect is dispatched, not that this individual item received a native playtest.

| Item ID | Source rows | Max copies | Runtime scalar / conditional keys |
|---|---:|---:|---|
| `acid` | 3 | -1 | `stat_max_hp`, `stat_dodge`, `knockback` |
| `adrenaline` | 2 | 1 | `stat_dodge`, `@heal_on_dodge` |
| `alien_baby` | 2 | -1 | `stat_max_hp`, `enemy_health` |
| `alien_eyes` | 1 | -1 | `@alien_eyes` |
| `alien_magic` | 3 | -1 | `stat_max_hp`, `stat_hp_regeneration`, `stat_luck` |
| `alien_tongue` | 2 | -1 | `pickup_range`, `knockback` |
| `alien_worm` | 3 | -1 | `stat_max_hp`, `stat_hp_regeneration`, `consumable_heal` |
| `alloy` | 6 | -1 | `stat_melee_damage`, `stat_ranged_damage`, `stat_elemental_damage`, `stat_engineering`, `stat_crit_chance`, `stat_dodge` |
| `anvil` | 1 | 1 | `@upgrade_random_weapon` |
| `baby_elephant` | 1 | -1 | `@dmg_when_pickup_gold` |
| `baby_gecko` | 2 | 4 | `stat_range`, `instant_gold_attracting` |
| `baby_with_a_beard` | 2 | -1 | `stat_range`, `@projectiles_on_death` |
| `bag` | 2 | 3 | `item_box_gold`, `stat_speed` |
| `bait` | 2 | -1 | `stat_percent_damage`, `@extra_enemies_next_wave` |
| `ball_and_chain` | 5 | 1 | `stat_percent_damage`, `stat_armor`, `knockback`, `stat_speed`, `@minimum_weapon_cooldowns` |
| `bandana` | 2 | -1 | `piercing`, `stat_percent_damage` |
| `banner` | 3 | -1 | `stat_range`, `stat_attack_speed`, `knockback` |
| `barricade` | 3 | -1 | `knockback`, `stat_speed`, `@temp_stats_while_not_moving` |
| `bat` | 2 | -1 | `stat_lifesteal`, `stat_harvesting` |
| `bean_teacher` | 2 | -1 | `xp_gain`, `stat_lifesteal` |
| `beanie` | 2 | -1 | `stat_speed`, `stat_range` |
| `big_arms` | 5 | -1 | `stat_melee_damage`, `stat_ranged_damage`, `knockback`, `stat_attack_speed`, `stat_speed` |
| `black_belt` | 4 | -1 | `xp_gain`, `stat_melee_damage`, `knockback`, `stat_luck` |
| `blazemander` | 1 | -1 | `@pet` |
| `blindfold` | 3 | -1 | `stat_crit_chance`, `stat_dodge`, `stat_range` |
| `blood_donation` | 2 | -1 | `stat_harvesting`, `lose_hp_per_second` |
| `blood_leech` | 3 | -1 | `stat_lifesteal`, `stat_hp_regeneration`, `stat_harvesting` |
| `bloody_hand` | 3 | 1 | `stat_lifesteal`, `lose_hp_per_second`, `@stat_links` |
| `boiling_water` | 2 | -1 | `stat_elemental_damage`, `stat_max_hp` |
| `bonk_dog` | 1 | -1 | `@pet` |
| `book` | 3 | -1 | `stat_engineering`, `stat_elemental_damage`, `stat_luck` |
| `bot_o_mine` | 1 | -1 | `@pet` |
| `bowler_hat` | 4 | -1 | `stat_luck`, `stat_harvesting`, `stat_attack_speed`, `stat_crit_chance` |
| `boxing_glove` | 2 | -1 | `stat_melee_damage`, `knockback` |
| `broken_hourglass` | 2 | 0 | `(empty spent effect)`, `hp_start_next_wave` |
| `broken_mouth` | 2 | -1 | `stat_max_hp`, `stat_hp_regeneration` |
| `butterfly` | 2 | -1 | `stat_lifesteal`, `stat_elemental_damage` |
| `cake` | 2 | -1 | `stat_max_hp`, `stat_percent_damage` |
| `campfire` | 4 | -1 | `stat_elemental_damage`, `stat_hp_regeneration`, `stat_speed`, `fog_visibility` |
| `candle` | 5 | 1 | `stat_elemental_damage`, `stat_hp_regeneration`, `number_of_enemies`, `stat_percent_damage`, `fog_visibility` |
| `candy_bag` | 2 | 1 | `extra_elite_next_wave_chance`, `@gain_random_primary_stats_on_go_to_next_wave` |
| `cape` | 5 | -1 | `stat_lifesteal`, `stat_dodge`, `stat_melee_damage`, `stat_ranged_damage`, `stat_elemental_damage` |
| `catling_gun` | 1 | -1 | `@pet` |
| `celery_tea` | 3 | -1 | `@stats_end_of_wave`, `@stats_next_wave`, `@stats_next_wave` |
| `chameleon` | 3 | -1 | `stat_dodge`, `stat_percent_damage`, `@temp_stats_while_not_moving` |
| `charcoal` | 3 | -1 | `stat_elemental_damage`, `stat_melee_damage`, `stat_harvesting` |
| `claw_tree` | 3 | -1 | `stat_melee_damage`, `stat_crit_chance`, `stat_max_hp` |
| `clockwork_wasp` | 2 | -1 | `structure_attack_speed`, `stat_speed` |
| `clover` | 3 | -1 | `stat_luck`, `stat_dodge`, `stat_lifesteal` |
| `coffee` | 2 | -1 | `stat_attack_speed`, `stat_percent_damage` |
| `cog` | 3 | -1 | `stat_engineering`, `knockback`, `stat_percent_damage` |
| `coil` | 2 | 3 | `knockback`, `@stat_links` |
| `community_support` | 2 | 1 | `stat_armor`, `@stat_links` |
| `compass` | 3 | -1 | `stat_speed`, `stat_engineering`, `stat_crit_chance` |
| `coupon` | 1 | 5 | `items_price` |
| `crown` | 1 | 1 | `harvesting_growth` |
| `cute_monkey` | 2 | 13 | `heal_when_pickup_gold`, `stat_ranged_damage` |
| `cyberball` | 1 | -1 | `@dmg_when_death` |
| `cyclops_worm` | 2 | -1 | `stat_percent_damage`, `stat_range` |
| `dangerous_bunny` | 1 | 3 | `free_rerolls` |
| `decomposing_flesh` | 2 | -1 | `@stats_on_level_up`, `@stats_on_level_up` |
| `defective_steroids` | 3 | -1 | `stat_max_hp`, `stat_melee_damage`, `stat_attack_speed` |
| `diploma` | 3 | -1 | `stat_engineering`, `xp_gain`, `stat_max_hp` |
| `doc_moth` | 1 | 4 | `@pet` |
| `duct_tape` | 3 | -1 | `stat_armor`, `stat_engineering`, `stat_max_hp` |
| `dynamite` | 1 | -1 | `explosion_damage` |
| `energy_bracelet` | 3 | -1 | `stat_crit_chance`, `stat_elemental_damage`, `stat_ranged_damage` |
| `estys_couch` | 3 | 1 | `stat_max_hp`, `stat_speed`, `@stat_links` |
| `evil_hat` | 3 | 1 | `gold_drops`, `stat_speed`, `stat_dodge` |
| `exoskeleton` | 6 | -1 | `stat_armor`, `stat_crit_chance`, `stat_engineering`, `stat_speed`, `stat_hp_regeneration`, `stat_lifesteal` |
| `explosive_shells` | 3 | -1 | `explosion_damage`, `explosion_size`, `stat_percent_damage` |
| `extra_stomach` | 1 | 1 | `@consumable_stats_while_max` |
| `eyes_surgery` | 3 | 2 | `burning_cooldown_reduction`, `stat_elemental_damage`, `stat_range` |
| `fairy` | 2 | 1 | `@stat_links`, `@stat_links` |
| `fertilizer` | 2 | -1 | `stat_harvesting`, `stat_melee_damage` |
| `fin` | 3 | -1 | `stat_speed`, `stat_lifesteal`, `stat_luck` |
| `focus` | 2 | -1 | `stat_percent_damage`, `@unique_weapon_effects` |
| `fresh_meat` | 2 | -1 | `stat_lifesteal`, `stat_hp_regeneration` |
| `fried_rice` | 1 | 1 | `@stat_links` |
| `frozen_heart` | 4 | 1 | `stat_elemental_damage`, `stat_crit_chance`, `burning_cooldown_reduction`, `@weapon_scaling_stats` |
| `fruit_basket` | 2 | 4 | `enemy_fruit_drops`, `stat_hp_regeneration` |
| `fuel_tank` | 3 | -1 | `stat_elemental_damage`, `stat_melee_damage`, `stat_ranged_damage` |
| `gambling_token` | 2 | -1 | `stat_dodge`, `stat_armor` |
| `garden` | 1 | -1 | `@structure` |
| `gentle_alien` | 3 | 10 | `stat_max_hp`, `stat_percent_damage`, `number_of_enemies` |
| `ghost_outfit` | 4 | 1 | `dodge_cap`, `stat_dodge`, `stat_speed`, `stat_armor` |
| `giant_belt` | 1 | 1 | `@giant_crit_damage` |
| `glass_cannon` | 2 | -1 | `stat_percent_damage`, `stat_armor` |
| `glasses` | 1 | -1 | `stat_range` |
| `gnome` | 4 | -1 | `stat_melee_damage`, `stat_elemental_damage`, `stat_range`, `pickup_range` |
| `goat_skull` | 2 | -1 | `stat_melee_damage`, `stat_crit_chance` |
| `greek_fire` | 2 | 1 | `fog_visibility`, `@burning_enemy_hp_percent_damage` |
| `grinds_magical_leaf` | 3 | -1 | `@stats_end_of_wave`, `@stats_end_of_wave`, `@stats_end_of_wave` |
| `gummy_berserker` | 3 | -1 | `stat_attack_speed`, `stat_range`, `stat_armor` |
| `handcuffs` | 4 | 1 | `stat_melee_damage`, `stat_ranged_damage`, `stat_elemental_damage`, `@stat_cap` |
| `head_injury` | 2 | -1 | `stat_percent_damage`, `stat_range` |
| `heavy_bullets` | 5 | -1 | `stat_ranged_damage`, `stat_percent_damage`, `stat_range`, `stat_attack_speed`, `stat_crit_chance` |
| `hedgehog` | 3 | -1 | `stat_melee_damage`, `stat_ranged_damage`, `stat_hp_regeneration` |
| `helmet` | 2 | -1 | `stat_armor`, `stat_speed` |
| `honey` | 5 | -1 | `stat_ranged_damage`, `explosion_damage`, `explosion_size`, `stat_speed`, `stat_dodge` |
| `hourglass` | 2 | 1 | `item_hourglass`, `hp_start_next_wave` |
| `hunting_trophy` | 1 | 3 | `@gold_on_crit_kill` |
| `ice_cube` | 1 | 1 | `@enemy_percent_damage_taken` |
| `improved_tools` | 2 | 1 | `stat_attack_speed`, `@structures_cooldown_reduction` |
| `injection` | 2 | -1 | `stat_percent_damage`, `stat_max_hp` |
| `insanity` | 2 | -1 | `stat_crit_chance`, `stat_percent_damage` |
| `jelly` | 1 | -1 | `@unique_weapon_effects` |
| `jellyshield` | 2 | 3 | `jellyshield_count`, `@pet` |
| `jetpack` | 4 | -1 | `stat_speed`, `stat_dodge`, `stat_max_hp`, `stat_armor` |
| `landmines` | 1 | -1 | `@structure` |
| `leather_vest` | 3 | -1 | `stat_armor`, `stat_dodge`, `stat_max_hp` |
| `lemonade` | 1 | -1 | `consumable_heal` |
| `lens` | 2 | -1 | `stat_ranged_damage`, `stat_range` |
| `little_frog` | 3 | -1 | `pickup_range`, `stat_harvesting`, `stat_dodge` |
| `little_muscley_dude` | 3 | -1 | `stat_melee_damage`, `stat_max_hp`, `stat_range` |
| `lootworm` | 1 | 1 | `@pet` |
| `lost_duck` | 2 | -1 | `stat_luck`, `stat_elemental_damage` |
| `lucky_charm` | 3 | -1 | `stat_luck`, `stat_melee_damage`, `stat_ranged_damage` |
| `lucky_coin` | 2 | 1 | `stat_armor`, `@stat_links` |
| `lumberjack_shirt` | 1 | 1 | `one_shot_trees` |
| `lure` | 2 | -1 | `stat_hp_regeneration`, `extra_loot_aliens_next_wave` |
| `mammoth` | 5 | -1 | `stat_melee_damage`, `stat_hp_regeneration`, `knockback`, `stat_percent_damage`, `stat_speed` |
| `mastery` | 2 | -1 | `stat_melee_damage`, `stat_ranged_damage` |
| `medal` | 5 | -1 | `stat_max_hp`, `stat_percent_damage`, `stat_armor`, `stat_speed`, `stat_crit_chance` |
| `medikit` | 3 | -1 | `stat_hp_regeneration`, `stat_luck`, `@temp_stats_per_interval` |
| `metal_detector` | 4 | 20 | `chance_double_gold`, `stat_luck`, `stat_engineering`, `stat_percent_damage` |
| `metal_plate` | 2 | -1 | `stat_armor`, `stat_percent_damage` |
| `missile` | 2 | -1 | `stat_percent_damage`, `stat_attack_speed` |
| `mouse` | 3 | 5 | `stat_lifesteal`, `number_of_enemies`, `stat_harvesting` |
| `mushroom` | 2 | -1 | `stat_hp_regeneration`, `stat_luck` |
| `mutation` | 3 | -1 | `stat_ranged_damage`, `stat_elemental_damage`, `knockback` |
| `nail` | 3 | 1 | `stat_engineering`, `stat_ranged_damage`, `@weapon_scaling_stats` |
| `night_goggles` | 4 | -1 | `stat_crit_chance`, `stat_range`, `stat_max_hp`, `stat_armor` |
| `octopus` | 4 | -1 | `stat_max_hp`, `stat_hp_regeneration`, `stat_lifesteal`, `stat_crit_chance` |
| `padding` | 2 | 1 | `stat_max_hp`, `@stat_links` |
| `panda` | 3 | -1 | `stat_max_hp`, `stat_luck`, `stat_percent_damage` |
| `peaceful_bee` | 4 | -1 | `stat_dodge`, `stat_harvesting`, `stat_melee_damage`, `stat_ranged_damage` |
| `peacock` | 3 | -1 | `xp_gain`, `@stats_next_wave`, `@stats_next_wave` |
| `pencil` | 1 | -1 | `stat_engineering` |
| `piggy_bank` | 1 | 1 | `gain_pct_gold_start_wave` |
| `pile_of_books` | 3 | 1 | `structures_can_crit`, `stat_crit_chance`, `stat_engineering` |
| `plant` | 2 | -1 | `stat_hp_regeneration`, `stat_lifesteal` |
| `plastic_explosive` | 1 | -1 | `explosion_size` |
| `pocket_factory` | 2 | 1 | `stat_engineering`, `tree_turrets` |
| `poisonous_tonic` | 4 | -1 | `stat_attack_speed`, `stat_crit_chance`, `stat_range`, `stat_hp_regeneration` |
| `potato` | 9 | -1 | `stat_max_hp`, `stat_hp_regeneration`, `stat_lifesteal`, `stat_percent_damage`, `stat_attack_speed`, `stat_speed`, `stat_dodge`, `stat_armor`, `stat_luck` |
| `potion` | 2 | 1 | `stat_hp_regeneration`, `@hp_regen_bonus` |
| `power_generator` | 2 | 1 | `stat_percent_damage`, `@stat_links` |
| `propeller_hat` | 2 | -1 | `stat_luck`, `stat_percent_damage` |
| `pumpkin` | 2 | -1 | `piercing_damage`, `stat_percent_damage` |
| `ratzilla` | 1 | -1 | `@pet` |
| `recycling_machine` | 1 | 1 | `recycling_gains` |
| `reinforced_steel` | 3 | -1 | `stat_ranged_damage`, `stat_engineering`, `stat_speed` |
| `retromations_hoodie` | 2 | 1 | `stat_range`, `@stat_links` |
| `ricochet` | 2 | -1 | `bounce`, `stat_percent_damage` |
| `rip_and_tear` | 2 | 5 | `stat_crit_chance`, `@explode_on_death` |
| `riposte` | 2 | -1 | `stat_melee_damage`, `@dmg_on_dodge` |
| `ritual` | 3 | -1 | `stat_percent_damage`, `stat_lifesteal`, `stat_engineering` |
| `robot_arm` | 3 | -1 | `@stats_end_of_wave`, `@stats_end_of_wave`, `@stats_end_of_wave` |
| `sad_tomato` | 2 | 1 | `stat_hp_regeneration`, `hp_start_wave` |
| `scapegoat` | 1 | 1 | `@pet` |
| `scar` | 2 | -1 | `xp_gain`, `stat_range` |
| `scared_sausage` | 1 | 4 | `@global_burn` |
| `scope` | 3 | -1 | `stat_ranged_damage`, `stat_range`, `stat_attack_speed` |
| `shackles` | 4 | 1 | `stat_hp_regeneration`, `stat_engineering`, `stat_range`, `@stat_cap` |
| `shady_potion` | 2 | -1 | `stat_luck`, `stat_hp_regeneration` |
| `sharp_bullet` | 4 | 1 | `piercing`, `piercing_damage`, `stat_percent_damage`, `knockback` |
| `shmoop` | 4 | -1 | `stat_max_hp`, `stat_hp_regeneration`, `stat_melee_damage`, `stat_ranged_damage` |
| `sifds_relic` | 2 | 1 | `stat_armor`, `instant_gold_attracting` |
| `silver_bullet` | 1 | -1 | `damage_against_bosses` |
| `small_magazine` | 3 | -1 | `stat_ranged_damage`, `stat_attack_speed`, `stat_percent_damage` |
| `snail` | 2 | 1 | `enemy_speed`, `stat_speed` |
| `snake` | 2 | -1 | `burning_spread`, `stat_max_hp` |
| `snowball` | 1 | 1 | `@gain_stat_for_equipped_item_with_stat` |
| `spicy_sauce` | 2 | 2 | `stat_max_hp`, `@explode_on_consumable` |
| `spider` | 4 | -1 | `stat_percent_damage`, `stat_dodge`, `stat_harvesting`, `@unique_weapon_effects` |
| `statue` | 2 | -1 | `stat_speed`, `@temp_stats_while_not_moving` |
| `stone_skin` | 2 | 1 | `stat_attack_speed`, `@stat_links` |
| `strange_book` | 3 | 1 | `stat_melee_damage`, `stat_ranged_damage`, `@stat_links` |
| `sunglasses` | 2 | -1 | `stat_crit_chance`, `stat_armor` |
| `tardigrade` | 1 | -1 | `hit_protection` |
| `tentacle` | 2 | 5 | `stat_crit_chance`, `heal_on_crit_kill` |
| `terrified_onion` | 2 | -1 | `stat_speed`, `stat_luck` |
| `toolbox` | 2 | -1 | `stat_engineering`, `stat_attack_speed` |
| `torture` | 2 | 1 | `stat_max_hp`, `torture` |
| `toxic_sludge` | 2 | -1 | `stat_elemental_damage`, `stat_dodge` |
| `tractor` | 2 | -1 | `stat_harvesting`, `stat_percent_damage` |
| `tree` | 1 | -1 | `trees` |
| `triangle_of_power` | 3 | -1 | `stat_percent_damage`, `stat_armor`, `@temp_stats_on_hit` |
| `turret` | 1 | -1 | `@structure` |
| `turret_flame` | 1 | -1 | `@structure` |
| `turret_healing` | 1 | -1 | `@structure` |
| `turret_laser` | 1 | -1 | `@structure` |
| `turret_rocket` | 1 | -1 | `@structure` |
| `tyler` | 1 | -1 | `@structure` |
| `ugly_tooth` | 2 | 1 | `stat_speed`, `@remove_speed` |
| `vigilante_ring` | 1 | -1 | `@stats_end_of_wave` |
| `wandering_bot` | 1 | -1 | `@structure` |
| `warrior_helmet` | 3 | -1 | `stat_armor`, `stat_max_hp`, `stat_speed` |
| `weird_food` | 2 | -1 | `consumable_heal`, `stat_dodge` |
| `weird_ghost` | 2 | -1 | `stat_max_hp`, `hp_start_next_wave` |
| `wheat` | 4 | -1 | `stat_melee_damage`, `stat_ranged_damage`, `stat_harvesting`, `stat_elemental_damage` |
| `wheelbarrow` | 2 | -1 | `stat_harvesting`, `stat_armor` |
| `whetstone` | 2 | -1 | `stat_lifesteal`, `knockback` |
| `white_flag` | 2 | 1 | `stat_harvesting`, `number_of_enemies` |
| `will_o_the_wisp` | 2 | -1 | `stat_attack_speed`, `@gain_stat_for_killed_enemies_while_burning` |
| `wings` | 3 | -1 | `stat_speed`, `stat_range`, `stat_elemental_damage` |
| `wisdom` | 2 | 1 | `stat_percent_damage`, `@temp_stats_per_interval` |
| `wolf_helmet` | 3 | -1 | `stat_elemental_damage`, `stat_luck`, `stat_engineering` |

# Gameplay parity audit — October 11, 2026

Target: the supplied local Brotato Godot source at `E:/UnityProjects/Brotato`, base difficulty, single player, 20 normal waves. This is a Unity reimplementation with original Scrapshift art, six weapon families and eight shop items. It is not a claim of complete Brotato equivalence. Endless mode is outside this pass.

Comparison completed before implementation. Scene properties must be read by node: the legacy flat importer overwrites a boss's root attack fields with its later state fields.

| System | Reference evidence (relative to supplied source) | Observed Scrapshift mismatch / planned correction |
| --- | --- | --- |
| Run length | `singletons/run_data.gd`, `nb_of_waves = 20` | Keep 20, optional reference endless not implemented. |
| XP | `singletons/player_run_data.gd`, `current_level = 0`; `run_data.gd`, `get_xp_needed` / `get_next_level_xp_needed` | Start at 0; thresholds 16, 25, 36. Old start at 1 skipped the first threshold. |
| Level reward | `main.gd`, `on_levelled_up` | Add the missing automatic +1 Max HP per level, independently of the chosen upgrade. |
| Rarity | `singletons/item_service.gd`, `reset_tiers_data`, `get_tier_from_wave`, `get_upgrade_data` | One descending cumulative roll; uncommon +6% per wave after wave 1, rare +2% after wave 3, legendary +0.23% after wave 7; caps 60/25/8%. Luck modifies growth before caps. Fixed level 5/10/15/20 milestones. |
| Upgrade reroll | `item_service.gd`, `get_upgrades`; `ui/menus/ingame/upgrades_ui_player_container.gd` | Avoid previous upgrade IDs; reroll price correctly remains wave-based. Keep reduced stat pool explicit. |
| Harvest | `main.gd`, `manage_harvesting` | Negative harvest removes currency, never XP. Positive harvest grants both and grows by ceil(5%). |
| Charge | `entities/units/enemies/attack_behaviors/charging_attack_behavior.gd` and per-enemy scenes | Lock direction during preparation; remove invented recovery slow-down; per-enemy range, cooldown, preparation and dash duration. Random cooldown +/-10 frames except Predator. |
| Ranged | `attack_behaviors/shooting_attack_behavior.gd`, `shoot.tres`, spitter/junkie scenes | Aim at release; continue movement during windup; use projectile speed/damage rather than contact stats. Junkie minimum range and animation speed differ. |
| Movement | `entities/units/movement_behaviors/*`, looter/bruiser/spawner/pursuer scenes/scripts | Retained random destination, fixed offset around moving player, and additive pursuer boosts replace timer wandering/fleeing/continuous percentage acceleration. |
| Buffer | `buffer/buffer.tscn`, `buffer.gd` | Initial 0.25s; subsequent 4s; random eligible target in 309.04px zone; exclude other buffers, bosses and evil mobs. |
| Placement | `global/entity_spawner.gd`, `singletons/zone_service.gd`, `utils.gd` | Resample while relaxing clearance by 5px to 25px; use 64px edge margin; preserve shared square formation. Local conversion is 100 source pixels per Unity unit. Arena remains compact. |
| Queue and cap | `global/entity_spawner.gd`, `_physics_process`, `spawn`, `on_group_spawn_timing_reached` | Separate LIFO queues; drain every 3 physics frames (0.05s at 60Hz), two normal births at queue size >=120. Cull active regular enemies above cap when a group starts, not every birth including trees/bosses. |
| Predator | `predator/predator.tscn`, `boss/boss.gd`, `boss_state.gd`, rotating projectile/pivot scripts | Root charge differs from phase-two attack. Switch at <=50% HP or 45s, faster rotating projectile arm, 16 random-direction shots. Old version always charged and fired uniform rings. |

Implementation and validation results will be appended after tests. Existing synthetic campaign checks use an invulnerable player and supplied equipment; they do not prove balance or a human playthrough. User opened the project in Unity during this pass: validate in a separate copy, without closing their Editor or touching unsaved scenes.

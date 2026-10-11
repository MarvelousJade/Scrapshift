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

The corrections above are implemented. Existing synthetic campaign checks use an invulnerable player and supplied equipment; they do not prove balance or a human playthrough. User opened the project in Unity during this pass: validate in a separate copy, without closing their Editor or touching unsaved scenes.


## Implemented details and additional findings

- Base health now uses the reference's unmodified 10 HP; no reference character passive is assumed. Level 0 and first threshold 16 are intentional source values. Each earned level adds 1 Max HP before its separate upgrade choice.
- The first two shops offer two weapons and two items, counting locked weapons. Later shops retain the one-weapon guarantee through wave 5 and 35% random weapon chance. Owned-weapon and shared-set preferences use the source's declining early-wave bonuses; these preferences do not implement weapon-set stat bonuses.
- Newly rolled shop offers record their wave. A locked offer keeps that price on later waves. Current and previous offers are excluded when the reduced pool allows it. Missing high-tier item pools fall back to a lower tier, an explicit small-catalog adaptation.
- Slasher now creates one stationary, delayed slash near the player: 0.48s warning, damage window through 0.60s, removal at 0.63s. It replaces the incorrect three-bullet fan. The changing hit rectangle is approximated using the source animation keyframes and our player radius; it is not pixel-identical collision geometry.
- Evil mobs target and collect unclaimed materials, evolve at 10/20, slow down per stage, and return the stage-scaled collected value. Their attraction motion and visual evolution remain original approximations. Predator's nine projectiles extend at 1 unit/s to the source radii on one rotating arm.
- The first audit's `predator/`, `boss/` and `buffer/` shorthand paths are under `entities/units/enemies/`. All source was inspected read-only. No third-party code or new media was imported during this pass.

## Exactness boundary

The original art, compact arena, Unity collision/separation model, feedback effects and variable-step combat loop remain. Random draws use Unity/System.Random rather than Godot's generator, so the same seed is not a cross-engine replay guarantee. The 20-wave source schedule is retained, but these differences affect encounter feel.

The selected scope remains six weapon families, eight offered items and eight upgrade stats. Full weapon-set stat bonuses, character passives/selection, all upgrade types, crate rewards, other bosses, higher difficulties and endless mode are absent. Shop probabilities operate on that smaller catalog. Reference audio is unchanged. Slasher's retained entry is not in the base-difficulty campaign schedule, so the campaign does not validate its runtime hit window. This is closer source-based behavior, not complete game equivalence.

User's open Editor was preserved. A separate Unity project compiled and ran automated tests/builds; the Windows player supplied rendered evidence. The initial sandboxed CLI attempt could not initialize host licensing; retrying the same isolated copy with host permission succeeded. No user's Editor process was terminated.

## Validation outcome

83 EditMode tests passed. The final Windows build succeeded with zero errors/warnings, and AssetScope found no issues among 198 assets / 1,337 reference usages. The final automated campaign completed all 20 waves and observed the boss transition. Normal-speed first-wave smoke reached the shop with 1 HP; it warrants personal difficulty testing. See `verification.md` and its linked artifact paths for exact counters, fixture overrides, screenshots and archive digest.

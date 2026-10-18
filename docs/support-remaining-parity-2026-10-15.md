# Remaining support behavior pass — October 15, 2026

Scope: supplied base single-player reference at `E:/UnityProjects/brotato-(3.x)`, inspected read-only. Codex implemented two bounded spatial corrections and a Lootworm target-cache correction requiring two shared spawn hooks from root. This pass preserves the R6 footstep/audio correction that the user confirmed removed the reported sound; it changes no audio code, settings, recordings, processing, mix or music. No Unity processes were launched by this agent.

## Corrected differences

### Doc Moth counts overlap with the player's body

Reference evidence:

- `entities/units/pet/doc_moth/doc_moth.tscn`: `BoostZone` uses a `CircleShape2D` with radius 200 and emits `body_entered`/`body_exited`.
- `doc_moth.gd`, `update_data`: scales the zone by the effect's `boost_zone_scale`. The ordinary item assigns scale 1.
- `entities/units/player/player.tscn`: the player's physical `Collision` uses radius 24 and position `(0, -12)` relative to the source root.
- `doc_moth.gd`, `_on_BoostZone_body_entered` and `_on_BoostZone_body_exited`: apply/unapply the healing effects per body overlap.

Scrapshift previously compared the two origins with the zone radius alone. `SupportBehavior.InsideMothZone` now includes the 0.24-unit player body radius and adapted relative offset. The project uses body-centered coordinates: the player body center is `playerPos + down * 0.12` (`CombatGeometry.PlayerHurtOffset` uses that same center), while the moth's source-origin zone is `mothPos + down * 0.24`. Subtracting those centers gives `playerPos + up * 0.12 - mothPos`; the helper's `up` term is a **relative-vector simplification**, not a claim that the player's physical body center itself is above its art origin. `CyberneticPets.TickPets` uses that predicate for each moth's contribution and existing enter/exit feedback. The zone-view position adaptation remains an existing visual limitation; this focused patch does not claim a native visual check of its registration.

Stacking inspection prevented an incorrect change: `doc_moth_effect_1.tres` adds regeneration bonuses with a matching 100% condition, so two occupied zones yield three HP per ordinary regeneration tick. In contrast, `doc_moth_effect_2.tres` sets `double_lifesteal_bonus`; `singletons/run_data.gd`, `manage_life_steal`, explicitly treats that as a boolean and chooses either one or two HP. It does not choose three HP for two moths. Existing regeneration/life-steal formulas were preserved; `SurvivalRules.cs` was not edited. The reference effect's REPLACE storage and restoration order deserve a separate full event-lifecycle test before making exact claims about every overlapping-zone exit sequence.

### Bot O' Mine samples a whole square inside the map margin

Reference evidence:

- `entities/units/pet/bot_o_mine/bot_o_mine.gd`, `spawn_landmines`: calls `get_spawn_pos_in_area(global_position, 100)`.
- `global/entity_spawner.gd:473`, `get_spawn_pos_in_area`: delegates area sampling to `ZoneService.get_rand_pos_in_area`.
- `singletons/zone_service.gd:123`, `get_rand_pos_in_area`: clamps the area's **center** by the edge margin plus half its width, then independently samples each coordinate inside the square.
- `singletons/utils.gd`: `EDGE_MAP_DIST = 64`. At the supported map sizes, the edge-distance cap does not reduce this value.

Scrapshift's existing `SpawnMine` already sampled a one-unit-wide square around its supplied position. Its previous final-position clamp let Bot O' Mine mines land against the map boundary and compressed out-of-bounds samples onto that boundary. The fix clamps the supplied square center to `halfSize - (0.64 + 0.5)` before the existing random sampling. Interior placement is unchanged. `SupportBehavior.BotMineSpawnCenter` is called only by Bot O' Mine; ordinary weapon/item mine spawners retain their existing paths.

## Validation prepared for integration

The initial spatial/healing portion of `SupportRemainingParityTests.cs` contains 12 cases:

- Four player-body overlap/offset cases and one scaled-zone case, including positions whose origins are outside the zone but whose bodies overlap it.
- A two-zone enter/exit scenario exercising the real `RunRules` regeneration multiplier and health tick.
- A multiple-moth life-steal case preserving the reference's boolean double-heal behavior and existing shared cooldown.
- One unchanged-interior mine-center case and four corner cases verifying all sample-square corners remain 0.64 units inside the map boundary.

These tests have been authored, not executed by this agent. Root owns combined Unity validation and native review. Pure geometry and rules tests do not establish animation, audio or hardware playtesting.

## Lootworm target-cache correction

Lootworm's `lootworm.gd` confirms Gold-only attraction/collection, a per-pickup doubling roll, and tree attacks. The current material collection pass already uses separate attract/contact entry tracking; it does not continuously steal attracted drops on every frame. The current implementation doubles the collected material value on a successful roll. The source also doubles `gold.boosted`, which is a separate presentation/accounting field not modeled on Scrapshift's pickup object; this pass does not silently reinterpret it as another material doubling.

`entities/units/target_behavior/lootworm_target_behavior.gd` caches a selected tree or Gold object and reconnects target-removal signals. Its `update_target` chooses the nearest living tree before considering Gold, without excluding already-attracted Gold. It invalidates selection in `on_gold_spawned`, `on_neutral_spawned`, `on_gold_picked_up_by_player` and `on_dead_tree`. When nothing exists, it sets the target to self; `pet.gd` only calls `update_target` when the target is null. Thus an idle Lootworm must not continuously poll every tick, and moving nearer to a different target must not by itself change its selection.

The runtime now uses a per-pet `SupportBehavior.TargetCache<object>`. It preserves its selected Enemy/Pickup reference while valid, follows the selected object's current position, and uses an initialized null sentinel when idle. Removal or death of the **selected** target invalidates it through the real collection/enemy-list and health checks at the next pet tick; an unrelated object's removal does not. Each pet has an independent cache, and destroying/recreating pets during wave cleanup creates fresh caches without old references. Multiple Lootworms may select the same Gold or tree, matching the source's lack of exclusive reservation for this pet.

Two shared `ReferenceCombat.cs` hooks were requested from root for serial integration:

1. At the end of the non-fruit `Drop` method, call `InvalidateLootwormTargets()` after the material loop, including zero-value and cap-merge-only batches. Source `main.gd`, `spawn_gold`, emits `gold_spawned` after its loop even when it added no new Gold objects. Fruit-only drops skip this path.
2. When a tree's spawn marker materializes in `UpdateEnemies`, call the same invalidation method. This corresponds to `entity_spawner.gd` emitting `neutral_respawned` when it adds the neutral to the active list.

These hooks are required for the complete behavior and must be verified in the integrated checkout. No additional death/pickup hook is needed: the source clears the selected target and selects again on a subsequent physics tick, which the cache's validity check reproduces. Existing Gold-only collection, attraction/contact entry tracking and per-pickup doubling behavior remain intact.

Six additional tests exercise the actual runtime-used cache's lifecycle: retaining a live target despite another moving closer; spawn invalidation including batches with no added object; neutral-spawn reselection; selected target removal; idle sentinel wake-up; independent pet caches and fresh wave instances. Together with the earlier spatial/healing tests, this file now contains 18 cases. These tests have not been executed by this agent; root integrates the hooks and runs Unity validation.

## Remaining limits

Pet cooldown and targeting paths were inspected, including cat left/right half-cooldown staggering, Bot O' Mine's initial mine interval and follow distance, and Lootworm tree priority. This inspection does not certify every pet's timing, collision, target ownership or multiplicity. No claim of complete support parity is made.

## Changed files

- `Assets/Scrapshift/Runtime/SupportBehavior.cs`: body-zone predicate, clamped mine-area center and per-pet target cache.
- `Assets/Scrapshift/Runtime/CyberneticPets.cs`: spatial call-site changes, cached Lootworm selection and a spawn invalidation method. Earlier material-collection changes already present in this dirty file were preserved.
- `Assets/Scrapshift/Tests/Editor/SupportRemainingParityTests.cs` and its import metadata.
- This document.

`StructureSpec.cs`, `SurvivalRules.cs`, shared rules/material files and existing audio assets were not changed in this pass.

## R7 integrated validation - completed

Full suite passed 594/594, zero failed/skipped, 72.8708967 seconds. Native built-player support fixture passed 27/27 and exited zero. R7 build succeeded with zero errors/warnings. Evidence: artifacts/gameplay-parity-continuation-2026-10-15/tests.xml, build-manifest.json and native-support/pets.json. These are automated/native fixture results, separate from human full-run playtesting. Further beam/zone geometry comparisons continue separately after the R7 checkpoint.


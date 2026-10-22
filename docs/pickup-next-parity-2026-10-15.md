# Pickup/drop parity continuation — October 15, 2026

Codex inspected the supplied read-only reference at `E:/UnityProjects/brotato-(3.x)` and implemented consumable eligibility/profile corrections, source drop scatter, material-batch probability and instant-attraction owner routing. The R8 run's differing pickup totals were a reason to investigate, not proof of a particular formula defect. No Unity or validation-clone processes were launched by this agent. Accepted audio and the preceding collection/ownership/cache fixes remain intact. At the user's request, the whole-suite/full-run gate remains pending while the remaining audit continues.

## Consumable eligibility and enemy-specific probabilities

Reference `main.gd`, `spawn_loot`, calls `spawn_consumables` only when `unit.stats.can_drop_consumables` is true. `singletons/item_service.gd`, `get_consumable_to_drop`, then applies the base consumable chance and its fallback fruit effect. Therefore an ineligible creature must not receive that fallback either.

`ReferenceLoot.CanDropConsumables` now excludes the supplied disabled profiles: Junkie, Dire Junkie, Evil Mob, Slasher Egg, Infected Slasher Egg and Corrupted Tree. Neutral Tree remains eligible. Root integrated the same eligibility predicate before the `enemy_fruit_drops` fallback; returning `None` from the ordinary roll alone could not enforce this rule.

The former default `.01/.01` rates also missed these supplied profiles:

| Source enemy | Base consumable chance | Crate chance before Luck/count/modifiers |
| --- | --- | --- |
| Healer | .03 | .03 |
| Mad Slasher | .03 | .03 |
| Horned Spitter | .03 | .10 |
| Tentacle | .02 | .02 |

Their `entities/units/enemies/<id>/<id>_stats.tres` files provide the values. Existing rates for ordinary aliens, trees and the earlier covered enemies were retained.

Reference stages preserved:

1. Base chance is multiplied by `1 + Luck/100`, capped above at 1. Disabled profiles exit before any roll.
2. `always_drop_consumables` bypasses that base roll. Supplied elites/bosses and Looter have this flag and item chance 1.
3. Item chance receives Luck, division by `1 + crates_spawned_this_wave`, then the crate-chance modifier.
4. In the normal run through wave 20, an always-drop profile with item chance at least 1 overrides the resulting item chance to 1. Boss-class drops become legendary crates; Looter drops an ordinary crate. This override follows the modifiers even if Luck or crate chance is negative.

Known supplied Boss subclasses are recognized when no explicit role was supplied, preserving existing callers while matching their actual source class. This pass does not certify endless-mode consumable scaling beyond wave 20 or hypothetical unknown enemy profiles.

## Scatter uses the supplied spread rather than a fixed small square

Reference `main.gd`, `spawn_consumables`, samples a square's side length from 50 through `100 + unit.stats.gold_spread` pixels. `spawn_loot` computes material spread as `clamp((value - 1) * 25, unit.stats.gold_spread, 200)` before calling `spawn_gold`; that function samples the same kind of square. Tree and Looter have base spread 75 pixels; Evil Mob has 125. The source `ZoneService.get_rand_pos_in_area` clamps the sampled square's center so its destination remains inside the map.

`ScrapMotion.CreateDrop` previously always sampled a 50–100 pixel side regardless of source spread. It now accepts an optional **world-unit** spread and samples side lengths `.5 .. 1 + spread`, preserving the existing square-center clamp. Existing callers that omit spread retain their old behavior. `LootPresentation.DropConsumable` forwards its optional spread to this method.

`ReferenceLoot.BaseGoldSpread` and `MaterialSpread` expose source values/formulas for the root-owned death/material path to pass through. The material formula uses the **unrounded** amount from `main.get_gold_value` before the fractional object-count roll and before any banked bonus is reserved for individual spawned pickups. Using an integer batch size is only an approximation and was explicitly rejected during this investigation. Root integrated `DropEnemyLoot`, retaining a separate unrounded `MaterialDropAmount` and rounded object count, and preserved the original three-argument `Drop` wrapper for its other callers.

Banked bonuses, capped pile merging and the `gold_spawned`-equivalent Lootworm invalidation hook are separate behaviors. This pass does not move or duplicate those operations.

## Material batches obey wave and horde probability

The previous ordinary death path always created its material batch. Source `main.gd`, `spawn_loot`, first resolves consumables, then computes a separate material-batch chance:

- Waves 1–4: 1.
- Later waves: `max(0.5, 1 - wave * 0.015)`.
- Horde waves multiply that result by 0.65 **after** the floor.
- `always_drop_consumables` overrides the material chance to 1, independently of whether consumable drops themselves are enabled. Evil Mob therefore has guaranteed materials but no consumables. Looter and Boss subclasses also retain their guarantees.
- Neutral Tree bypasses the enemy probability roll.

For example, an ordinary enemy's wave-20 chance is .7, or .455 on a horde wave. `ReferenceLoot.MaterialBatchChance` now supplies these rules. Root integrated the gate in the real death path **after** consumable resolution. The new gate may explain some prior economy differences, but no numerical full-run attribution has been measured here.

Reference `Utils.get_chance_success` explicitly rejects zero chance and otherwise uses `randf() <= chance`. `ReferenceLoot.Roll` now preserves that positive-boundary behavior, including an endpoint roll of 1 for guaranteed crates. The material fractional-count rule's matching positive-boundary correction was requested from the rules owner separately.

## Instant material attraction prefers Lootworm

Source `main.gd`, `spawn_gold`, routes a successful `instant_gold_attracting` roll to `_entity_spawner.lootworms[player]` when that companion exists, and otherwise to the player. The entity spawner stores the last spawned Lootworm for that player. Base Baby Gecko/Sifd's Relic effects can therefore combine with Lootworm collection and its doubling roll.

Previously every successful instant-attraction roll set Scrapshift's player magnet flag. `LootPresentation.RouteInstantMaterialCollector` now converts that already-rolled flag to external Lootworm ownership when a live companion exists; it introduces no extra random roll and preserves the no-worm player fallback. Root calls it once after constructing a material pickup. Ordinary non-instant drops retain their existing area-entry ownership rules. `UpdatePickups` already steps external collectors, and Lootworm's ordinary contact path already performs its per-pickup doubling/collection; neither path was replaced.

## Source evidence and regression checks

`artifacts/gameplay-parity-native-2026-10-15/pickup-next-audit/reference-drop-profiles.json` records 40 supplied stat resources, their eligibility/rates/tier/spread fields, SHA-256 hashes and the relevant logic-source hashes. The original project remains untouched.

New `PickupNextParityTests.cs` contains 55 cases covering:

- Six ineligible creature profiles, including extreme Luck.
- Seven rate/rarity boundaries for the previously missed enemy profiles.
- Separate Luck, crate-count and crate-modifier stages.
- Four Boss subclasses plus Looter/Tree guarantee distinctions.
- Five spread floor/cap/fractional-amount examples.
- Seeded scatter into the extended source square while respecting map bounds.
- The real `ArenaGame.DropConsumable` path forwarding spread into actual pickup motion.
- Eleven wave/horde/guarantee material-chance cases and the source positive/zero chance boundary.
- Five real `DropEnemyLoot` cases covering disabled fruit fallback, the horde gate, guaranteed neutral materials with source spread, and consumables surviving a rejected material batch.
- Three real death-path instant-attraction cases: last live Lootworm, no-worm player fallback and ordinary non-instant ownership.
- Nine coordinate adapter cases exercising actual player attraction/contact entry, source-root movement for all three collector kinds, Evil Mob contact exclusion and actual enemy drop origin.

These tests were authored but not executed by this agent. The shared root plumbing was inspected after integration; actual execution remains pending. Passing catalog counts or differing full-run RNG outcomes do not substitute for those checks.

## Inspected behavior left intact

The source Item pushback threshold is 20 pixels, followed by ten 60-Hz idle ticks. Items become monitorable during that idle interval, while attraction motion waits until it ends. Current `ScrapMotion` retains that distinction, the 500-pixel initial attraction speed, 20-pixel per-tick acceleration and dead-collector release/reset behavior. Existing rotated pickup collision, contact/attraction entry tracking and ownership rules were preserved. This inspection is scoped source comparison, not a new claim of native movement verification.

The supplied `get_gold_value_modifier` hook has only its base zero-returning implementation; no overrides were found. The `scale_materials_with_distance` runtime hook has no assignment in the supplied base item resources. Current base material modifiers and the minimum half-base amount were compared without claiming support for unregistered/DLC effects. Source end-wave optimized banking can apply additional common pickup boosts, but the supplied default `optimize_end_waves` is false; the ordinary banking path does not justify applying those boosts indiscriminately.

## Lifecycle audit and remaining coordination

The source `Unit.can_drop_loot` resets to true. The only false assignment found in the supplied scripts is performance-cap removal in `entity_spawner.gd`; Scrapshift's cap-removal path already removes those units without producing loot. No summon or egg spawn argument disables loot globally. Egg hatching is different: `slasher_egg.gd` calls `die()` while the value-1 egg remains loot-eligible. The earlier direct hatch removal therefore omitted a possible material batch; root integrated loot/death callbacks in its `ExtendedEncounters` branch. Global death progress and indexed player credit are separate source behaviors and must not be conflated when integrating that hook.

A cross-system coordinate question was investigated independently by the sustain agent and confirmed against the existing body-centered combat convention. The coordinated correction now uses:

| Actor/operation | Offset from adapted body center |
| --- | --- |
| Player attraction area and magnet destination | `(0, -.24)` |
| Player contact pickup circle | `(0, 0)` |
| Lootworm attraction area and destination | `(0, -.24)` |
| Lootworm contact pickup circle | `(0, -.14)` |
| Evil Mob attraction area and destination | Its imported `OriginOffset`, currently `(0, -.24)` |
| Evil Mob contact pickup circle | Imported origin plus `(0, .10)`, currently `(0, -.14)` |
| Enemy material and consumable spawn | Imported per-enemy source origin |

The player scene puts its animation/body center and pickup circle at raw `(0,-24)`, while its attract area and actor origin are raw `(0,0)`. Lootworm likewise has Animation `(0,-24)`, attract area at raw zero and pickup collision at raw `(0,-10)`. Evil Mob uses the same raw attract/contact node offsets, with its source-root/body adapter supplied by the enemy geometry profile. The previous player contact point was 0.24 above the body, and its attract/destination points omitted the downward source-origin adjustment.

`ArenaGame.UpdatePickups` and `AttractNearbyConsumables` now consistently use the player origin for attraction and retarget-distance comparison, and the body center for contact. Shared motion uses the relevant external collector's source origin. `EnemySpecials.EatMaterials` and Lootworm's attraction/contact calls use the corresponding offsets. Lootworm's cached target lifecycle is unchanged; nearest-target distance and movement endpoints are expressed in source-origin coordinates, then transformed back into its body-center position. No origin assumption was applied to pet types that cannot collect materials.

The existing material-collection fixture now supplies an actual Evil Mob type and positions its horizontal-motion sample relative to that source origin. Scatter tests compare destination minus initial spawn position so an origin translation cannot falsely prove extended spread. These changes coordinate the audited pickup paths; they do not claim that every unrelated world subsystem has been validated under the same convention.

## Files owned in this pass

- `Runtime/ReferenceLoot.cs`
- `Runtime/ScrapMotion.cs`
- `Runtime/LootPresentation.cs`
- New `Tests/Editor/PickupNextParityTests.cs` and metadata
- This report and the source-profile artifact

Root owns shared edits in `ReferenceCombat.cs`, material amount rules and integration tests. No audio code, SoundBank, sound assets, music, UI or shared portfolio documents were edited by this agent.

Additional coordinate files authorized during this continuation: `ArenaGame.cs` (pickup methods only), `EnemySpecials.cs` (Evil Mob pickup centers only), `SupportBehavior.cs` (Lootworm origin/contact offsets), `CyberneticPets.cs` (Lootworm pickup and target-coordinate adapters), `CombatGeometry.cs` (player-origin offset), and the bounded fixture adaptation in `MaterialCollectionParityTests.cs`.

## Root integration result

After integration and fixture corrections, the focused selection passed **213/213**, zero failed/skipped. It includes this track's cases and directly affected older regressions. The actual XML and 496-input source manifest are in `artifacts/gameplay-parity-follow-up-2026-10-15`. Earlier statements about unexecuted agent-authored checks or a pending rerun describe the preceding stage. The whole suite, new player build and full campaign remain deferred; these focused results do not establish exact parity.

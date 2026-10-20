# Material discrepancy follow-up — October 20, 2026

## Saved evidence rechecked

The [reproducible auditor](../scripts/material_reconciliation_audit.py) reads the original saved JSON without changing it or launching either game. Its [receipt](../artifacts/item-material-followup-2026-10-20/material-saved-trace-audit.json) independently preserves both observations:

| Original-engine first wave | Timer callback | After cleanup |
| --- | --- | --- |
| Historical full campaign | 25 kills, 5 pickup callbacks, 19 ground units, bank 0 | 25 kills, 5 pickups, ground 0, bank 19 |
| Instrumented one-wave probe | 28 kills, 8 pickup callbacks, 20 ground units, bank 0 | 28 kills, 8 pickups, ground 0, bank 20 |

The historical residual is **still one unit after cleanup**, not merely an incomplete player pickup callback. The later probe balances all 28 actual requested, registered and picked units; its peak requested-but-not-registered count is one. Neither observation establishes the cause of the other.

The auditor preserves the historical kill/unit hypothesis only as a diagnostic residual. It is **not a valid invariant even for this first wave**, because the zone-wide Evil Mob group can introduce zero-value kills or stolen materials. Achievement `materials_collected` counts pickup callbacks, not material value; subsequent waves also have banks, modifiers, trees and multi-unit piles. Applying `kills = pickups + bank + ground` to every wave would manufacture false discrepancies.

To reproduce this read-only analysis from the repository root:

```powershell
python scripts/material_reconciliation_audit.py artifacts/parity-remaining-2026-10-20/original-full-headless/trace.json artifacts/headless-completion-2026-10-20/material-probe-attempt-1/trace.json --output artifacts/item-material-followup-2026-10-20/material-saved-trace-audit.json
```

## Source findings

Paths below are relative to the supplied read-only `E:/UnityProjects/brotato-(3.x)` project.

- `main.gd:502–576`: counted enemy deaths pass the non-cleanup/player-kill condition, call `spawn_loot`, then increment `enemies_killed`. A counted kill does not universally prove a material request.
- `main.gd:624–648`: a no-loot enemy returns immediately. Otherwise ordinary wave 1 has chance 1; the horde modifier can change that in a horde wave. `singletons/utils.gd:397–403` uses `randf() <= chance`, so ordinary chance 1 does not have a rare random failure.
- `zones/zone_1/001/wave_1.tres`, `group_1.tres`, `unit_1.tres`: the selected first wave is 20 seconds, enemy cap 100, one non-horde group of 4–5 baby aliens beginning at second 1 and repeating every 3 seconds. `baby_alien_stats.tres` gives material base value 1. The base schedule permits at most 35 baby aliens during the wave; it cannot alone reach the performance-removal threshold. The historical fixture selects zone index 0, Danger 0 and seed 771; no per-enemy log establishes that every counted unit followed the expected base path.
- Historical timer/shop snapshots report no trees, items, Harvesting or starting bonus bank. The first-wave spendable currency equals the five pickup callbacks. Ordinary bank growth, item modifiers and Harvesting do not account for the recorded extra kill under these conditions.
- **Global enemy exception found:** `zones/wave_manager.gd` adds zone-wide groups. `zones/zone_1/000_all/group_3.tres` schedules one `evil_mob` at second 10 with a 3% unit spawn chance. `entities/units/enemies/evil_mob/evil_mob.gd:60` overrides its nominal resource value with `int(gold_count * (evolution + 1))`, so a killed, unevolved Evil Mob that consumed nothing produces **zero material while counting as an enemy kill**. Its actual pickup callback also removes materials via `gold.pickup(-1)` during combat; `Main.on_gold_picked_up` does not credit those to cleanup bank unless `_cleaning_up` is true. Therefore 25 kills / 24 accounted units is consistent with valid source behavior. The historical trace does not record enemy identities, Evil Mob kills or theft, so this is a source-supported explanation, **not an assigned historical cause**. The base baby-alien schedule alone cannot close the ledger.
- `main.gd:735–772`: a newly instantiated material is added with `call_deferred`, then execution yields for `ready` **before** appending to `_active_golds`. Pooled instances take a different, synchronous path. This is a real non-atomic registration window.
- `main.gd:1001–1048`: cleanup marks the room as cleaning up, then processes only currently registered `_active_golds`. Normal cleanup assigns their attraction target; optimized cleanup sums their values immediately. A request still suspended outside that list is not included by these loops at that instant.
- `main.gd:827–895`: pickup removes the registered node and credits the player or cleanup bank; it is not correct to count a pre-credit diagnostic snapshot as final accounting. The historical shop snapshot already occurs after this ordinary cleanup interval.

The original campaign stderr contains known startup/log-file assertions and shutdown diagnostics. It contains no saved error stack naming `spawn_gold` or `on_gold_picked_up`; absence of such a stack does not reconstruct the missing per-event history.

## Scrapshift Evil Mob source audit

No production mismatch was found in the consumed-material accounting inspected here:

- `Assets/Scrapshift/Runtime/EnemySpecials.cs:77–94` transfers the consumed pile's actual `value` into `enemy.gold`, removes that pile and advances at most one evolution per pickup at thresholds 10/20. Source `evil_mob.gd:28–53` uses the same actual pile value and one conditional evolution step. A single 25-unit pile advances from evolution 0 to 1; it does not jump directly to 2. A later pickup can advance it again.
- `Assets/Scrapshift/Runtime/ReferenceCombat.cs:578` overrides nominal catalog value with `enemy.gold * (enemy.evolution + 1)` before the existing material modifiers and rounding pipeline. This matches source `EvilMob.get_stats_value()`. The zero-consumed case therefore produces zero, not the resource's nominal eight units.
- Unity's consumption path directly removes the pickup; it does not call the player's collection or bank methods. This matches source `gold.pickup(-1)` while cleanup is false.
- Existing `MaterialCollectionParityTests.EnemyCollectorAcquiresAtSourceRadiusAndUsesSharedMotion` and `PickupNextParityTests.EvilMobContactUsesItsAdaptedOffsetRatherThanAnAboveBodyCircle` cover attraction/motion and contact geometry. They do **not** individually establish the complete consumed-value/evolution/death return sequence; this audit is source coverage, not a new executed pipeline test.

Useful bounded value cases for a future paired engine receipt are `(consumed, evolution) -> base payout`: `(0,0)->0`, `(1,0)->1`, `(9,0)->9`, `(10,1)->20`, `(19,1)->38`, `(20,2)->60`, `(25,1)->50`. These expected values come from the source getter; production code was not changed to force a historical aggregate to balance.

## Executed cleanup-boundary experiment

The [four-case summary](../artifacts/item-material-followup-2026-10-20/material-boundary-attempt-1/summary.json) and each case's trace were inspected. All four isolated Godot processes completed with exit 0. They use the actual `Main.spawn_gold` and `Main.clean_up_room` methods; the fixture deliberately invokes cleanup synchronously before a fresh material's deferred registration can finish. The scene's end-wave timer is then stopped to inspect the node and accounting before scene replacement.

The [preparation script](../scripts/prepare_material_cleanup_boundary.py) creates a separate clone and four launch specifications. The [fixture](../scripts/material_cleanup_boundary_reference.gd) performs these paired staged operations:

1. With material pool empty and registered list empty, invoke the genuine `Main.spawn_gold(1, position, 0)`. Before advancing the scene tree, record coroutine suspension, `_active_golds`, `_cleaning_up`, bank and node attraction target.
2. Invoke genuine `clean_up_room()` synchronously before the deferred add is flushed. Advance idle/physics frames, record registration and whether the new node receives the bag target, then inspect again after three seconds with the scene transition held.
3. Repeat with one pooled material (no new-instance yield) as the control, and repeat both normal and optimized end-wave settings. No RNG draws should be added by logging.
4. Preserve the original source, previous probe and historical evidence; record each case separately.

| Case | Suspended before cleanup | Registered before cleanup | Bank after three seconds | Remaining registered node |
| --- | --- | ---: | ---: | --- |
| Fresh / normal | Yes | 0 | 0 | One visible unit; no attraction target |
| Pooled / normal | No | 1 | 1 | None |
| Fresh / optimized | Yes | 0 | 0 | One visible unit; no attraction target |
| Pooled / optimized | No | 1 | 1 | One hidden node, already credited |

These results **demonstrate the forced source registration/cleanup race**: a fresh node registered only after cleanup's list processing, received no cleanup attraction, and remained uncredited after three seconds. Normal pooled cleanup attracted and credited the unit. Optimized pooled cleanup credited immediately and hid the retained node; its value must not also be counted as spendable ground value.

This experiment did not advance the fresh case through scene replacement and does not claim an observed final destroyed/lost node. It also did not reproduce the natural seed-771 history. **The historical 25/24 cause remains unknown.** A zero-value Evil Mob kill or material theft is independently valid reference behavior and invalidates kill-to-material equality even in wave 1; the historical trace lacks the per-enemy and per-material events needed to distinguish those cases from a boundary race.

No Scrapshift payout fix is justified by these results. The current Evil Mob source audit matches consumption and payout behavior, and no corresponding Scrapshift coroutine/cleanup defect was established. This investigation establishes no desktop, UI, controller, visual, audio or human-balance coverage.

## Attribution and interview rehearsal

Codex performed the saved-trace and source analysis under the user's direction; the user did not personally execute these steps in this record. Explain the difference between a callback count and unit value, then explain why an intermediate deferred-registration gap is insufficient to explain a residual that persists after cleanup. A useful engineering decision here is to preserve the unresolved observation and design a discriminating experiment before changing currency logic.

Engine diagnostic limitation: all four cases exited0 and produced the expected boundary traces, but logs retain source DebugService log-open assertions (error7), startup CrashReporter diagnostics and shutdown resource warnings. No other SCRIPT ERROR text was present in the four captured stderr files. See `material-boundary-attempt-1/diagnostics.json`; do not describe these as clean engine logs.

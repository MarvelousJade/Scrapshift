# Incoming damage and sustain audit — 2026-10-15

Source is the supplied read-only project at `E:/UnityProjects/brotato-(3.x)`. This pass preserves the R8 controller, audio, artwork and collision metadata changes.

## Corrected regeneration behavior

1. **External healing does not reset an already running regeneration timer.** `player.gd:742-790` heals without stopping that timer. `on_health_regen:702-724` stops it only when its own timeout observes full HP. A fruit or life-steal fill followed by damage before that timeout must retain progress. Conversely, a timeout at full HP stops the timer and a later hit starts a fresh interval.
2. **The interval uses an integer regeneration stat.** `player.gd:602-603` casts the effective stat to int before `run_data.gd:get_hp_regeneration_timer:1442-1454`. Thus 1.9 uses the 1-stat interval, not a continuously interpolated interval. The source's nonpositive fallback is 99 seconds; ordinary regeneration remains disabled when the effective stat is nonpositive.
3. **Multiple timer loops are one healing event.** `FixedTimer.try_loop` returns a loop count. `player.on_health_regen` samples the HP-threshold bonuses once, multiplies by that count, and calls `on_healing_effect` once. The old per-loop healing could change Potion's multiplier halfway through a batch and emit extra Lich-style healing callbacks.

`SurvivalRules.cs` now tracks whether regeneration is running, preserves the timer across external full healing, stops/restarts it at the source boundaries and batches timer-loop healing. The coordinating agent adds the corresponding `RunRules` hooks: reset survival on run/wave initialization, start regeneration after actual unblocked damage, and remove generic full-heal timer reset.

Torture uses the same lifecycle with a one-second interval and its own healing amount. It is **not** an always-running timer at full HP: `check_hp_regen:587-594` starts a stopped timer only when HP is missing, and `on_health_regen` stops it at full HP. `no_heal` continues to block Torture healing.

## Checked and retained

- Dodge precedes hit protection (`player.get_damage_value:300-312`), and negative armor uses the same coefficient as Scrapshift. The base single-player invulnerability duration is the existing .2–.4 second formula (`player.get_iframes:574-583`). No `CombatGeometry.cs` change was needed.
- Life steal shares a .1-second cooldown (`player.tscn:LifestealTimer`). `player.on_lifesteal_effect:735-739` starts it before attempting healing, including when healing is blocked or HP is full. Existing cooldown behavior was retained.
- A proposed multiplicative Doc Moth × Potion change was **rejected** after inspecting the resources. Both `doc_moth_effect_1.tres` and `potion_effect_1.tres` add entries to the same `hp_regen_bonus` array. `_hp_regen_val` remains 1 in the supplied source. The existing additive moth/Potion multiplier is correct and has a regression test.

## Corrected current HP after effective Max HP changes

`player.update_player_stats:349-368` adds positive Max HP increases directly to current HP, bypassing ordinary healing restrictions and the `healed` signal. Its `LiveStats.copy(..., false)` does not clamp current HP on a Max HP decrease. Dirty stat updates reach this path through RunData → Main → StatsManager, including temporary/linked stat changes. `RunRules` now observes the final effective maximum at health reads and physics boundaries, adds only positive live deltas directly, and retains HP on reductions. `Add` no longer clamps after each individual stat mutation. This preserves combined item mutations, linked Padding, caps and temporary effects without healing callbacks or no_heal/Torture restrictions. Initialization suppresses reconciliation until starting effects and the wave-start HP percentage have been applied. Outside a wave, `RunData.get_player_current_health:302-305` reports Max HP; Scrapshift now does likewise in shop/rewards, and the next wave initializes HP/start-wave percentage separately.

An important source edge is preserved: ordinary `on_healing_effect:742` takes `min(request, max-current)` without a lower clamp, so a later external healing request can reduce above-Max HP back to Max without a healed signal. By contrast, `on_health_regen:717-720` clamps its request to zero first; this pass explicitly preserves above-Max HP during regeneration. Self-damage also starts regeneration after real unprotected HP loss.

The snapshot implementation observes a completed dirty-stat batch when health is needed rather than reproducing Godot's separate StatsManager physics-node queue. Mutations with an intervening health observation are separate snapshots; native frame-order validation remains separate from these deterministic rule tests.

## Verification

`SustainNextParityTests.cs` adds 20 cases covering integer intervals, external healing, full-health timeout/restart, hit-and-heal before the first tick, regeneration disable/re-enable, threshold snapshot and one healing callback, additive moth/Potion behavior, Torture/no-heal, full-health life-steal cooldown, direct Max HP gains without healing callbacks, decrease/ordinary-heal behavior, net item batches, above-Max regeneration, actual Padding/Handcuffs, temporary movement bonuses, restart and next-wave percentages. The existing `LootSurvivalTests` full-reset assertion was corrected to the source timing and its nonpositive interval assertion now uses 99 seconds.

Unity execution is performed by the coordinating agent. These authored checks are not a claim of passed results until the test report is read. No reference runtime, Unity instance or validation clone was launched by this subtask; no user settings or saves were changed. Real-time visual/audio feedback and cross-engine scheduling remain separate native validation concerns.

## Root integration result

After integration and fixture corrections, the focused selection passed **213/213**, zero failed/skipped. It includes this track's cases and directly affected older regressions. The actual XML and 496-input source manifest are in `artifacts/gameplay-parity-follow-up-2026-10-15`. Earlier statements about unexecuted agent-authored checks or a pending rerun describe the preceding stage. The whole suite, new player build and full campaign remain deferred; these focused results do not establish exact parity.

# Remaining survival parity - October 20, 2026

## Scope and evidence

Baseline: `8246340`, following the historical R25 checkpoint. This track inspects the original read-only source, broadens survival boundaries and defines focused/native acceptance. Codex performs this investigation and implementation under the user's direction. New checks are pending root integration; previous suite/native results do not establish the new cases.

## Periodic lethal self-damage

Confirmed mismatch: `ConditionalEffects.TickConditionalEffects` directly set `Phase=Lost` after `lose_hp_per_second` depleted HP, bypassing the delayed death lifecycle used by hostile hits. The original `player.gd` periodic self-damage uses `take_damage`, whose lethal branch calls `die`; `main._on_player_died` then enters the same half-second cleanup and acknowledgment path. The item-effects agent changes only this death transition to `BeginDeathCleanup()`.

`SurvivalRemainingParityTests` checks lethal periodic damage enters pending cleanup, grants no harvesting/currency/XP, permits cleanup pause and restores a captured checkpoint after acknowledgment. A second case combines hit protection, one-hit death and periodic self-damage: protection absorbs the first tick, the second enters the normal death lifecycle.

Native acceptance: stage a non-invulnerable run with no hostile entities, add a periodic self-damage value above HP, then let actual Update run past the one-second boundary. Require Lost with DeathCleanupPending, actor/attack removal, an approximately 0.5-second acknowledgment delay, dead fruit preserved and a working Retry after that delay. This must be a fresh fixture result, not inferred from the earlier hostile-projectile death probe.

## Direct loss transition inventory

A source search finds only three assignments to Lost: the centralized `BeginDeathCleanup`, periodic self-damage (corrected above), and intentional `Abandon`. Abandon is a separate player-confirmed end-run action and must remain non-retryable; it is not a lethal-contact cleanup path.

## Remaining investigations in this track

- Actual imported character combinations with one-hit death, self-damage and No Heal.
- Optional optimized cleanup: the source applies `apply_common_gold_pickup_effects` once per material pile before banking; default cleanup banks raw value. This separate option is not exposed in the current Unity settings.
- Unsuppressed global unlock notifications and loss/retry UI order. The prior native fixture explicitly cleared pending full-screen modals; it did not validate the natural Rookie-unlock case.

No Unity or native player is launched by this track; the root integrates and runs validation. This document will record fresh evidence and unresolved limits as it arrives.

## Periodic damage focused result

`artifacts/parity-remaining-2026-10-20/self-damage-tests.xml`: 16/16 Passed, zero failures/skips, exit0, 0.8398811 seconds. Includes both new periodic-death cases and the14 prior survival lifecycle cases. Native acceptance above remains pending the new review build.

## Optimized cleanup implementation and acceptance

Source `main.gd:1031-1042` runs `RunData.apply_common_gold_pickup_effects` once for each material pile. That helper (`run_data.gd:898-904`) applies only the chance-double-gold boost; `Utils.get_chance_success` returns false without rolling for zero chance and otherwise tests `randf() <= chance`. It does not apply increase-material-value, XP, player healing or pickup-trigger attacks. Optimized cleanup immediately hides/banks materials, while default cleanup animates raw piles into the bank.

This track adds `BankCleanupMaterials(value, optimized)` and the optimized death-sweep branch in `DeathLifecycle.cs`. The root integrates the matching normal-wave branch and the setting, which defaults false. Five focused `OptimizedCleanupParityTests` cases cover off/zero/guaranteed boost, no unrelated pickup effects, dead-state/no-harvesting behavior and multiple piles. Execution evidence is recorded separately when available.

## Unsuppressed loss/reward UI acceptance

Source `main._on_EndWaveTimer_timeout:1126` shows retry/OK and returns before challenge processing. Consequently a naturally queued Rookie unlock must not replace the loss acknowledgment. The root defers the Lost reward modal until the run has been ended. Global completion and pending rewards must survive retry.

The expanded `--survival-review` fixture now attaches actual progression before lethal contact and never clears its unlock queue. It requires natural Rookie completion plus a pending reward at ACK, confirms actual virtual-A Retry with the queue preserved, then causes a second death through actual Update's periodic self-damage. With retry disabled, actual A selects OK; only then may the reward appear. Actual A on Continue must dismiss it and reveal the summary. Two fresh captures, `unlock-after-end.png` and `end-summary.png`, join the earlier six. The fixture still uses isolated profile/IgnoreFocus and staged combat stats; it does not establish hardware play or an ordinary full campaign.

## Fresh R26 native evidence - October 20

Root built and launched R26; this reviewer independently read the JSON/process/log artifacts and visually inspected all eight fresh survival screenshots under `artifacts/parity-remaining-2026-10-20/survival-r26-attempt-1/`. `survival-review.json` is Passed, process exit is 0 (approximately 9.57 seconds), and no case-insensitive error/exception/warning/assert matches appear in its player log. The build summary reports Succeeded, zero errors/warnings and 401,755,514 bytes. This is focused native evidence, not a full campaign.

The hostile-contact/regeneration/cleanup values remain HP 8, 9 and 7. Cleanup pause preserves age `0.016665604` seconds; acknowledgment occurs at `0.500493944`. Death retains 100 materials, zero XP, five banked raw materials and the fruit at the dead player's location. Actual virtual-A retry succeeds and records retry count 1.

The new unsuppressed cases all pass: `unlockModalsSuppressed=false`, natural Rookie reward pending at acknowledgment, reward queue preserved after retry, periodic self-damage enters pending death cleanup, reward shown only after OK ends the run, and actual A dismisses Continue to reveal the summary. The first three screenshots show the expected 8/10, 9/10 and 7/10 HUD states. `defeat-ack.png` and `defeat-retry.png` show the loss buttons while the bottom-left Bulkhead unlock toast remains visible; it does not replace acknowledgment. `retried.png` shows the living player at 10/10 with the toast still present. `unlock-after-end.png` shows the full Bulkhead reward and Continue only after ending the run. `end-summary.png` shows Run Lost, wave 1 / Danger 0, inventory/stats and Restart/New Run/Main Menu after dismissal. No additional presentation defect was observed in these scoped captures.

The separate optimized native fixture also Passed/exit 0; detailed numeric results and its two reviewed captures are recorded in `docs/optimized-cleanup-parity-2026-10-20.md`. Its model-staged lethal trigger and guaranteed boost are explicitly separate from natural random gameplay.

The earlier investigation list above is historical: optimized cleanup and the unsuppressed loss/reward order are now implemented and covered by these focused checks. Imported Sick/Wounded/Golem boundary tests are added; execution must be attributed to the actual integrated test result when the root records it. Remaining limits include a full ordinary non-invulnerable campaign, broader character/effect combinations, stochastic boost distributions and physical Xbox/ordinary OS-focus verification. No new human playthrough is claimed.

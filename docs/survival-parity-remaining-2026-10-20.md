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

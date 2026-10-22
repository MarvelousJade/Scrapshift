# Actual Player cleanup ordering — October 20

## Correction to the R35 interpretation

R35's Unit FIFO probe established scheduling for `Unit.take_damage`. It did not establish Player scheduling. Original Player **overrides** `take_damage` and does not delegate that implementation to Unit. At `entities/units/player/player.gd:524`, the override directly calls `die(Utils.default_die_args)` before returning.

Consequently, the R35 inference that an earlier pending enemy death must finalize before a later player-lethal hit is unsupported and contradicts the Player override. R35's `BeforeDeathCleanup` drain can award an earlier enemy kill which the original suppresses when Player death cleans the room synchronously first. The older Unit-labelled FIFO rows remain valid Unit evidence; their labels must not be presented as actual Player execution.

## Source call chain

1. `player.gd:412–546` performs damage and calls Player `die` directly at zero health.
2. Player `die` delegates to Unit/Entity `die`. Entity marks `dead` and emits `died` synchronously, before Player's later wearable/timer/pickup cleanup.
3. Main `_on_player_died` (`main.gd:467`) detects the absence of live players and calls `clean_up_room`.
4. Main sets `_cleaning_up = true` and calls EntitySpawner cleanup (`main.gd:999–1063`).
5. EntitySpawner `clean_up_room` (`global/entity_spawner.gd:585`) calls each enemy's `die` directly with `cleaning_up = true`.
6. Main's enemy callback observes `_cleaning_up`, suppressing loot and progression counts (`main.gd:506`). The already queued player-owned enemy death later reaches Unit `die`, which returns because the enemy is already dead (`unit.gd:520`).

An enemy death which already dispatched in an earlier event-loop flush remains a credited kill. The boundary is dispatch before Player death, rather than lethal HP mutation before Player death.

## Finite reference fixture

`scripts/player_cleanup_reference.gd` is an isolated autoload with two modes. It must be copied into a reference clone; the supplied original remains read-only.

- `minimal`: invokes **actual Player.take_damage** with supplied Timer nodes. Flash and consumable attraction are suppressed, and the terminal die body is a recording sink. This directly distinguishes synchronous Player dispatch from Unit's deferred dispatch; it does not test Main cleanup.
- `full`: loads the actual Main scene, actual Player and an actual Baby Alien spawned through EntitySpawner. Natural process/physics/timers are frozen after initialization. Actual damage, died signals and Main/Spawner cleanup remain unchanged. Three cases stage enemy-lethal then player-lethal in one stack, the reverse order, and enemy death flushed before the player-lethal hit. Snapshots retain HP, pending/dead flags, cleanup flags, kill count, ground material value, bank and death arguments.

Environment variables:

```
SCRAPSHIFT_PLAYER_CLEANUP_OUTPUT=<fresh absolute reference.json path>
SCRAPSHIFT_PLAYER_CLEANUP_MODE=minimal|full
SCRAPSHIFT_PLAYER_CLEANUP_CASE=enemy_first|player_first|enemy_flushed
```

Use a fresh clone/profile per full-scene case. The fixture initializes its profile below the output directory and installs a 45-second watchdog. Full mode pauses the tree before scene entry, then freezes processing recursively. The autoload remains active for staging and deferred snapshots. Receipt validation writes `Passed`/`Failed` and returns exit 1 on missing acceptance conditions.

## Preserved first attempts

Root executed the probes under `artifacts/native-followup-2026-10-20/`:

- `source-cleanup-player`: invalid minimal receipt, despite process exit 0. The fixture incorrectly assigned a Node Timer into Player's typed `FixedTimer` regeneration field, producing a nil `is_stopped` error. Health reached zero but the method aborted before die; this receipt is not passing evidence. The helper now supplies the correct timer types and fails validation if the actual method does not return a damage array and call die exactly once synchronously.
- `source-cleanup-enemy_first`: actual Main cleanup preempts the pending enemy death, leaving zero kills/materials before and after deferred flush. One enemy died signal has `cleaning_up = true`.
- `source-cleanup-player_first`: actual Player death immediately cleans the enemy; the later direct enemy damage produces no credited kill/materials or second died signal.
- `source-cleanup-enemy_flushed`: allowing the enemy death to dispatch first gives one kill and one ground material before player death. Cleanup retains that prior credit.

The three full-scene first attempts retained movement errors caused by inherited campaign instrumentation querying a missing `/root/ParityFixture` before freezing. They support the observed cleanup call-chain investigation but are not clean fixture acceptance. They are preserved alongside the corrected attempts below. The minimal timer correction was a fixture defect, not a gameplay defect.

## Corrected source evidence

Inspected `reference.json`, `process.json` and `runner-stderr.log` in each corrected folder below. All four receipts have **status Passed, empty validation errors and process exit 0**. None has a `SCRIPT ERROR`. The inherited startup CrashReporter/mod-folder and shutdown resource/leak diagnostics remain in the raw logs; this is not a zero-engine-errors claim.

| Folder under `artifacts/native-followup-2026-10-20/` | Actual observed result |
| --- | --- |
| `source-cleanup-player-attempt-2` | Actual Player damage returns `[1, 1, false]`, HP is zero, `dead` is true and the die sink ran once **before return**. |
| `source-cleanup-enemy_first-attempt-2` | Enemy reaches zero HP/pending death first. Synchronous Player death then sets Main/Spawner cleaning; enemy cleanup emits one died signal. Kill count, ground material and bank remain **0/0/0**, including after queued callbacks flush. |
| `source-cleanup-player_first-attempt-2` | Player cleanup happens first; later direct enemy damage creates no second died signal or credited rewards. Kill count, ground material and bank remain **0/0/0**. |
| `source-cleanup-enemy_flushed-attempt-2` | Enemy death dispatched before Player damage gives **one kill and one ground material** while Player lives and Main is not cleaning. Subsequent Player cleanup retains that earlier credit. |

The full-scene cases execute actual Player/Enemy/Main damage, death signals and cleanup. Natural Update/physics/timers are intentionally frozen; event order and health are staged. This closes those finite source scene cases, including the distinction between pending and already dispatched enemy deaths. It does not establish every natural collision sequence or complete end-of-run acknowledgment timing.

## Corrected Scrapshift contract

The R35 pre-cleanup drain is removed. Player lethal damage enters Lost synchronously. A pending enemy finalized afterward may transfer to corpse state, but cannot award kill credit, loot or item death effects during terminal cleanup. A later ordinary hit is rejected. Enemy deaths already finalized while Playing keep their credit. The focused test `SynchronousPlayerDeathPreemptsAnEarlierPendingEnemyDeath` now requires zero kills; it replaces the R35 test which required the opposite result.

Source `debe1cf` / R35 is therefore superseded for this specific behavior. Its other passing receipts remain historical evidence within their stated scope; they do not prove this corrected behavior or universal parity. Current Unity integration/build results belong in the consolidated checkpoint once inspected.

## Bug and interview note material

**Bug record:** R35 flushed pending enemy deaths before a lethal Player transition. Actual source Player overrides Unit damage and dies synchronously, so original room cleanup suppresses pending enemy rewards. Investigation moved from an inherited Unit timing probe to actual Player and full Main scenes. Removed the pre-cleanup drain and reversed the erroneous acceptance expectation; preserve an already-dispatched death as a control. Corrected source receipts pass all four bounded cases; current Unity integration evidence is tracked separately.

**Interview explanation:** The project encountered a false parity result because an automated test encoded a mistaken inheritance assumption. A passing Unit-level test did not describe Player's override. Codex inspected the override, built a fixture using actual scene callbacks, preserved the failed fixture attempts, and changed the test contract after the source contradicted it. The engineering lesson is to trace the runtime dispatch path and include a control which distinguishes an event being queued from its callback having completed. The user should reproduce and explain that trace before presenting it as their own debugging work; code and assistant-run investigation remain attributed to Codex.

## Remaining limits and attribution

This is a source-backed finding authored by Codex while investigating the user's requested remaining death timing. The full-scene fixture stages event order and health; even passing it would not establish ordinary collision scheduling, human survival balance, physical controller behavior, visual death animation or exhaustive combined effects. Keep this correction visible in bug and interview notes: following an inherited method without checking the subclass override produced a false acceptance expectation, which passing tests then reinforced.

# Corpse candidate membership — October 20, 2026

Scope: source inspection and windowless callback probes. The supplied original is read-only. No desktop, physical input, rendering, or human playtesting is part of this investigation.

## Corrected investigation

The first source reading inferred that death signals followed connection insertion order. That inference was wrong for the supplied Godot runtime. The initial 512-row probe showed Main's narrowed callback executing before spawner removal for both fresh and reused connection sequences: all rows saw two candidates including the zero-HP victim. Both cohorts produced 25 live-target hits and 231 no-hit outcomes using the imported Cyberball chance. These are source probe receipts, not identical Unity random-sequence claims.

The initial recorder was an autoload object constructed before the spawner. Its result disproved the insertion-order assumption but did not, by itself, establish the actual Main target's ordering. The revised probe uses a Main subclass as the callback target, records actual object IDs, and tests both target construction orders. The actual main.tscn creates root Main (line 131) before its EntitySpawner child (line 189). The reverse creation order is a diagnostic control, not the normal scene layout.

The revised probe completed with exit zero and 1,024 rows in `artifacts/death-item-followup-2026-10-20/corpse-source-attempt-2/reference.json`. Its four cohorts were:

| Target construction / connections | Rows | Corpse present during Main | Candidates during Main | Live-target Cyberball hits |
| --- | --- | --- | --- | --- |
| Main before spawner / fresh | 256 | 256 | 2 | 25 |
| Main before spawner / reconnected | 256 | 256 | 2 | 25 |
| Spawner before Main / fresh control | 256 | 0 | 1 | 61 |
| Spawner before Main / reconnected control | 256 | 0 | 1 | 61 |

Every row finished with one active enemy after the complete signal. Main's ID was 14019 versus spawner 14022 in the scene-like construction order; the control used Main 14560 versus spawner 14557. These results establish the ordering for these actual targets in the supplied runtime, without asserting a universal Godot signal-order contract. Fresh/reconnected registration did not alter the measured result.

No pooled-only gameplay change is justified by the first receipt. An unhooked pool metadata helper and its tests were removed before integration when the receipt contradicted that design. Preserving a failed hypothesis is useful evidence; implementing it would introduce a false lifecycle distinction.

## Source facts

- entities/units/unit/unit.gd:262 rejects a later hit when _pending_die is set. The lethal path schedules die using call_deferred, then sets _pending_die (line 352). The victim remains in the spawner candidate arrays during this interval.
- entities/entity.gd:69 marks dead and emits died synchronously. global/entity_spawner.gd:453 removes an ordinary enemy and invalidates its combined enemy cache. Main.handle_stat_damages (main.gd:1555) samples that array and returns immediately when the selected enemy has zero HP.
- Fresh EntitySpawner.spawn_entity connects the spawner's death callback before emitting enemy_spawned (global/entity_spawner.gd:378–381). Main connects its death callback in response (main.gd:1461–1462). **Registration order alone does not establish dispatch order.**
- Entity.death_animation_finished (entities/entity.gd:82) disconnects the spawner callback but leaves Main connected; pooled spawn reconnects the spawner. The initial runtime receipt found no resulting fresh/reused membership distinction.
- Both egg hatching scripts call normal die; performance removal calls normal die with loot disabled. Their death-triggered item callbacks need the same measured membership ordering.

## Finite probe

scripts/corpse_membership_reference.gd stages source spawn connections and emits the actual Godot signal. Its recorder derives from Main and delegates to the actual handle_stat_damages. It executes the real spawner removal/cache methods and imported Cyberball data for 256 seeds under each of two creation orders and two connection lifecycles, totalling 1,024 rows.

Between lifecycle stages it calls actual Entity.death_animation_finished, with an inert AnimationPlayer and replaced final pool insertion sink. The final enemy damage sink is replaced by a recording method. The full Main death handler, actual spawning, animation scheduling, actual Main pool insertion, and Unit.take_damage remain outside this probe.

The integrated finalizer retains the dying zero-HP member through Main-equivalent callbacks, then removes it. `CorpseMembershipFollowupTests` adds two focused real-Arena cases: one and three pending Evil Mob victims resolve their actual deferred finalizers without drawing a Cyberball probability roll, because every sampled candidate is already at zero HP. They assert membership during every callback, exactly-once kill/corpse counts, and removal after callbacks. Both passed in the 27/27 focused run (`focused-postfix-2.xml`, zero failures/skips) and the 1,271/1,271 integrated suite (`suite-attempt-2.xml`, zero failures/skips). Those XML results were read, not inferred from a successful process launch.

`HeadlessDeathTimingReview.cs` provides the opt-in packaged check `--headless-death-timing-review <output-folder>` under batchmode/nographics. It first verifies that actual ArenaGame.Start installed both the target-capture and pre-defeat cleanup hooks. It uses real spawned Evil Mob NPCs with staged HP and no stored gold, disables ordinary Arena Update, and calls the actual damage/heal/deferred-boundary paths.

## R35 packaged acceptance

R35 was built from source commit `debe1cf`. The actual packaged death-timing report at `artifacts/death-item-followup-2026-10-20/death-player/headless-death-timing.json` is Passed with eight passed receipts; its process receipt reports exit zero. Both production startup-hook checks and fixture isolation are true.

| Receipt | Observed result |
| --- | --- |
| Immediate Lich | Enemy HP 100 → 90 before deferred dispatch |
| Giant Belt critical after Lich | Enemy HP 61 |
| Lethal Lich before dispatch | HP 0, one active identity, zero kills/corpses |
| Repeated deferred dispatch | One kill/corpse, no active identity |
| Enemy lethal request before player lethal hit | Lost, one enemy kill/corpse |
| Player lethal hit before later enemy damage | Lost, enemy HP 1, zero kills/corpses |
| One dying Cyberball member | One capture, zero proc rolls, one kill/corpse after removal |
| Three dying Cyberball members | Three captures, zero proc rolls, three kills/corpses after removal |

The companion R35 proc report (`proc-player/headless-proc-target.json`) also Passed/exit zero: 192 candidate rows, zero-HP-only 0/64 captures, live-only 64/64, mixed 29/64. Rejected targets skipped probability rolls; removed/dead reservations did not redirect. Both synchronous vulnerability-order controls matched the source expectations: final HP 89 and 88.

These are packaged callback checks, not ordinary Update/physics scheduling or a fresh campaign. The original signal probe remains a finite source callback fixture with recording sinks; the Unity player uses actual spawned NPCs with staged HP. No interactive controls, focus, UI, audio listening, human balance, exhaustive loadout orderings, or complete original death scene was verified here.

## Interview rehearsal

The project caught an incorrect source-reading assumption by executing a focused callback probe before integrating a proposed fix. The useful explanation is to distinguish registration order from actual runtime dispatch and to use a target-creation-order control. This investigation and probe were authored by Codex agents under the user's parity direction. No independent user debugging experience is claimed.

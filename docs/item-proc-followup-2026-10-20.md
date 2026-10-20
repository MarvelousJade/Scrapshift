# Item proc interaction follow-up — 2026-10-20

## Scope and status

This follow-up adds eight finite, source-derived EditMode cases in `Assets/Scrapshift/Tests/Editor/ItemProcInteractionFollowupTests.cs`. The root agent runs the focused checks and records their actual result below. This agent did not launch Unity, Godot, a player, or desktop controls. These checks supplement the earlier 53-case dynamic oracle; they are not a new original-process differential oracle.

All acquisitions use actual imported items and obey their caps. Adrenaline is capped at one, Tentacle at five, and Hunting Trophy at three. An initial draft proposed multiple Adrenalines; source/catalog inspection rejected that fixture before execution. No production mismatch was established by that invalid proposal.

## Source-derived acceptance cases

Reference root: `E:/UnityProjects/brotato-(3.x)` (read-only).

| Cases | Legal mixture | Required result and source |
| --- | --- | --- |
| 2 | Lich + Adrenaline | A successful dodge heal while missing one HP heals one, then requests one Lich attack; at full HP it requests none. `entities/units/player/player.gd:460–470` aggregates successful Adrenaline healing; `740–753` clamps actual healing and only emits `healed` when positive. |
| 2 | Lich + Cute Monkey + 2 Baby Elephants | Monkey healing emits a Lich request before a separately aggregated Elephant request. Full HP suppresses only Lich. At Luck 8 / effective Damage +25%, each Elephant entry is `round(floor(8 × .25) × 1.25) = 3`, so two successes request 6. `main.gd:839–854`, `1552–1614`. |
| 1 | Lich + Torture + Cute Monkey + 5 Tentacles + 2 Baby Elephants + 2 Cyberballs | Torture blocks both healing sources and therefore Lich attacks. Independent pickup and death procs still request damage. `player.gd:777–794`, `main.gd:852–854`, `main.gd:527–529`. |
| 2 | Lich + 3 Tentacles | The summed 60% chance uses one inclusive roll; success heals exactly one HP and emits one Lich request. Exactly .6 succeeds, .600001 fails. `entities/units/unit/unit.gd:373–374`, `singletons/utils.gd:397–404`. |
| 1 | Lich + 3 Hunting Trophies + 5 Tentacles + Cute Monkey + 2 Baby Elephants + 2 Cyberballs | Trophy direct currency triggers no pickup XP, bank consumption, Monkey healing, or Elephant attacks. A subsequent successful Tentacle heal and two Cyberball successes retain separate heal/death requests. `unit.gd:367–383`, `main.gd:527–529`, `main.gd:839–854`. |

The tests inject bounded random outcomes to exercise exact source boundaries. Expected values derive from the original callbacks, not from calls to the same Unity helper under test. Staging health and ordinary stat contexts is explicit. This does not validate natural random distributions, target selection, damage application to enemies, or scene callback timing.

## Additional ordering audit

The original `player.take_damage` resolves Riposte retaliation before Adrenaline healing and temporary dodge stats. Unity currently calculates healing/temporary stats before retaliation. The only imported `temp_stats_on_dodge` carrier adds Attack Speed; Riposte scales Melee Damage. Therefore this inspection did not establish a direct legal stat-scaling discrepancy. No speculative ordering or random-stream change was made.

The original `main.handle_stat_damages` chooses a target before effect rolls and applies damage synchronously. Unity rolls in `RunRules.RandomDamageTrigger` and queues damage until `ArenaGame.ResolveImplantDamage`, which then chooses an eligible target. Further tracing established a concrete reachable candidate difference: `unit.gd:350–353` sets HP to zero and defers `die`; `global/entity_spawner.gd:455–456` removes that enemy only when the deferred death signal arrives. During a critical kill's Tentacle healing, Lich can therefore choose that zero-HP pending victim and abort (`main.gd:1567–1571`). Unity's later resolver filters it out, guaranteeing a remaining live target instead. This changes target availability, beyond a different sequence of random numbers.

`scripts/item_proc_target_reference.gd` executes a bounded original callback probe: 64 seeds each with zero-HP only, live-only, and mixed zero-HP/live candidate arrays. It calls the actual imported Lich effect through the original `Main.handle_stat_damages` and `EntitySpawner.get_all_enemies`; only the final enemy damage sink is a declared stub. Root execution exited 0: zero-HP-only produced no hits in all 64 cases, live-only hit in all 64 cases, and the mixed cohort produced 38 aborts / 26 hits. See `artifacts/item-material-followup-2026-10-20/proc-source-attempt-1/reference.json` and `process.json`. This does not execute original critical-hit/deferred-death scheduling itself; that reachability is established by the source trace above.

## Confirmed target-reservation fix

`RunRules.RandomDamageTrigger` now asks the scene to capture a target before rolling item entries. The scene includes a currently pending zero-HP enemy in the candidate pool, aborts when that candidate is selected, and queues successful damage against the captured enemy. The later resolver never silently redirects that request to another enemy if its original target dies or leaves the list. Model-only uses retain the existing event contract without requiring scene state.

The queue remains to protect indexed enemy/projectile traversal. This focused fix does not rewrite original-versus-Unity death scheduling. In particular, source damage is synchronous, while Unity applies queued damage later; Unity also removes dead enemies earlier in some paths than the original deferred death callback. Multiple pickups or unrelated weapon hits within one simulation step may therefore still differ in timing, corpse candidate availability, or kill attribution. Do not label the change full recursive proc/death parity. The demonstrated pending-critical-victim retargeting defect and stale-target redirection are addressed separately from those remaining boundaries.

`ItemProcTargetTests` adds five focused scene-object cases: zero-HP target rejection before proc RNG, mixed candidate selection, 64-seed live/dead controls, and dead/removed captured targets never redirecting damage. They manipulate actual `ArenaGame` private enemy lists in EditMode, not a running physics scene. Root focused execution passed23/23 cases, zero failures/skips, exit0,0.74044 seconds (`focused-target-attempt-1.xml`), including all five target regressions.

`HeadlessProcTargetReview.cs` additionally provides opt-in built-player acceptance with `--headless-proc-target-review <output-directory>` under `-batchmode -nographics`. It waits for ordinary scene initialization and verifies that `ArenaGame.Start` installed the production hook before disabling the update component. Its three 64-seed cohorts use manually staged nonrendered enemy records. It checks zero-HP rejection before probability rolls and captured dead/removed targets never redirecting a queued request. It deliberately never applies live damage to these bare records; it checks target reservation/dispatch guards, not enemy physics or a complete critical-hit chain. R32 built-player review Passed/exit0 with192 candidate rows and all startup/roll/stale-target guard flags true (`artifacts/item-material-followup-2026-10-20/proc-r32-attempt-1/headless-proc-target.json`). Build succeeded with zero errors and one fixture-only CS0252 reference-comparison warning.

## Validation

Root focused attempt 1 passed seven of these eight mixture cases. The Torture fixture staged raw health before its +15 Max HP acquisition was reconciled, so the next ordinary health read correctly credited that acquisition (2 became 17). The staging helper now observes acquired Max HP before setting the missing-HP context. This is a fixture correction, not a production healing defect. All eight mixture cases then passed in `artifacts/item-material-followup-2026-10-20/focused-attempt-2.xml`; preserve the failed attempt 1 XML. Target-reservation regressions are a later check and are not included in that count.

## Interview/supporting note

This is assistant-run investigation and assistant-authored regression coverage. A useful explanation to rehearse is why distinct event types matter: material pickup, direct currency payout, healing, and enemy death can each carry different effects. Another concrete lesson is to inspect item caps before designing combined-effect tests; an impossible fixture can appear to expose a gameplay issue. The user should trace and reproduce the example before describing it as their own debugging work.

Root focused attempt2:18/18 Passed, zero failures/skips, process exit0,0.9692116 seconds. Receipt: `artifacts/item-material-followup-2026-10-20/focused-attempt-2.xml`.

Integrated suite after the target fix:1249/1249 Passed, zero failures/skips, exit0,87.9211516 seconds (`suite-attempt-1.xml`). A later source audit closed the unavailable Goblet recursion hypothesis but identified a separate queued vulnerability-timing case for follow-up; this checkpoint does not claim that case fixed.

## Confirmed queued vulnerability timing difference

Final source helper attempt3 uses actual Main.handle_stat_damages, Unit.get_damage_value and imported Ice Cube behavior with a declared narrow damage/lifecycle sink. Lich deals10 first, then a1-damage elemental hit applies10% vulnerability: final89 HP. The actual R33 baseline player instead ended88 HP. Its Passed status with vulnerabilityBaseline=true means the intentional mismatch was reproduced, not parity.

Queued requests now capture the target vulnerability percentage with target identity. A differently named damage core accepts that context; the existing reflected11-argument DamageWithWeaponIndex remains a forwarding wrapper. Ordinary damage uses live vulnerability. No temporary mutation of enemy status is used.

The reverse expiry case preserves11 healing-proc damage after vulnerability expires; with the preceding1-damage elemental hit it ends88 HP. Native contexts explicitly stage HP, timer expiry and remove life steal to isolate the intended heal. The new two Arena-object regressions cover vulnerability appearing and expiring before dispatch.

Final focused receipt:25/25 Passed, zero failures/skips, exit0,0.756643 seconds. Integrated suite:1251/1251 Passed, zero failures/skips, exit0,73.4048644 seconds. Final packaged R34 Passed/exit0:192 target candidate rows, all startup/roll/stale-target flags true, forward89 HP and reverse-expiry88 HP. Build succeeded with zero errors/warnings,401807181 bytes;616 source inputs and159 player files hash-verified. Receipts: artifacts/item-material-followup-2026-10-20.

No reachable Goblet/heal_on_kill carrier was found in the provided source resources/imported catalog; the proposed nested healing chain is unavailable here. Broader same-step lethal preemption and deferred-corpse membership remain unverified. The targeting and vulnerability fixes do not claim identical full death scheduling or every compound interaction.

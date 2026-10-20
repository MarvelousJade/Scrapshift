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

`scripts/item_proc_target_reference.gd` prepares a bounded original callback probe: 64 seeds each with zero-HP only, live-only, and mixed zero-HP/live candidate arrays. It calls the actual imported Lich effect through the original `Main.handle_stat_damages` and `EntitySpawner.get_all_enemies`; only the final enemy damage sink is a declared stub. Root execution is pending. This does not execute original critical-hit/deferred-death scheduling itself; that reachability is established by the source trace above. The new model tests do not claim equivalence of recursive kills, target availability, or random-stream alignment.

## Validation

Root focused attempt 1 passed seven of these eight cases. The Torture fixture staged raw health before its +15 Max HP acquisition was reconciled, so the next ordinary health read correctly credited that acquisition (2 became 17). The staging helper now observes acquired Max HP before setting the missing-HP context. This is a fixture correction, not a production healing defect. Retest pending; preserve the failed XML in `artifacts/item-material-followup-2026-10-20/focused-attempt-1.xml`.

## Interview/supporting note

This is assistant-run investigation and assistant-authored regression coverage. A useful explanation to rehearse is why distinct event types matter: material pickup, direct currency payout, healing, and enemy death can each carry different effects. Another concrete lesson is to inspect item caps before designing combined-effect tests; an impossible fixture can appear to expose a gameplay issue. The user should trace and reproduce the example before describing it as their own debugging work.

Root focused attempt2:18/18 Passed, zero failures/skips, process exit0,0.9692116 seconds. Receipt: `artifacts/item-material-followup-2026-10-20/focused-attempt-2.xml`.

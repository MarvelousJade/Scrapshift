# Headless unlock producer acceptance — October 20, 2026

Status: **R30 headless execution Passed, exit0**. This adds callback-level producer acceptance to the R29 baseline; historical native evidence remains separately attributed.

## Scope and execution

`Assets/Scrapshift/Runtime/HeadlessUnlockProducerReview.cs` self-launches with `--headless-unlocks-review <fresh-folder>` in `-batchmode -nographics`, with in-memory fixture settings selected by the explicit headless flag. It rejects a visible/non-batch launch. No screenshots, OS input, controller simulation, browser or desktop control are used. The game component is disabled while the fixture synchronously invokes production scene callbacks with staged clocks. **These are real runtime callbacks, not automatically scheduled Update or pickup physics.**

The finite inventory is all **115** imported IDs: **109 newly staged callback scenarios**, plus six separately attributed historical native producers. Each new target gets a fresh clone of the catalog and a unique actual JSON profile. The target reward is explicitly locked in that clone. Each scenario checks a lower/nonqualifying boundary, completion, one notification, disk save/reload, a separate frozen active eligibility snapshot, then eligibility in a fresh snapshot. The separate pool test establishes persistence/eligibility semantics; it is not an organically continuing human run.

Character starters use the first imported eligible weapon or starting item, then remove combat weapons. Starting currency is deliberately neutral0. Generic character and danger cases stage the final wave and invoke `BeginWaveSweep`, `TickWaveSweep`, and `TickProgression`. They test dispatch through scene settlement and Finish. They are **not 50 natural 20-wave character campaigns**. Difficulty6 stages nine distinct prior Danger5 records and exercises the tenth win through scene finish. Lifetime cases stage only the lower lifetime counter directly in ProgressSave; the final increment is a real scene `Damage` death or `CollectPickup`, followed by ordinary cleanup callback checking. Their earlier hundreds/thousands of events are not reenacted.

Acquisition cases use imported items and production GrantItem/Buy/Recycle handlers. Weapon offers, funding and inventory resets are staged; these do not verify shop UI selection. Tree/elite targets are spawned from imported definitions, their spawn warning is explicitly cleared, and HP/age/position is staged before actual lethal Damage or scene census. Cleanup has staged elapsed time and ground drops. Medicine exercises Hurt and Heal, not natural regeneration balance. Raw stat cases use GainWeaponStat then the scene's periodic progression callback; they do not claim naturally acquired builds.

## Read-only reference anchors

- `singletons/challenge_service.gd:633` checks lifetime kills/materials/trees/evil counters at timeout; `:649` checks raw stat thresholds; `:666` requires the damage stat and registered structure count; `:674` requires both regeneration and lifesteal.
- `main.gd:1223-1232` checks counted challenges, exactly1 HP and living neutral/tree count at wave timeout; `:1280-1286` sums materials left on the ground.
- `entities/units/enemies/boss/boss.gd:67-75` performs Giant Slayer only for a non-cleanup death and uses **age <=15**.
- `entities/units/player/player.gd:790-792` checks actual accumulated healing against200 for the wave.
- `singletons/run_data.gd:864-876` completes Student on level20 and Fast Learner on level10 only while wave<6.
- `ui/menus/pages/menu_confirm.gd:27-28` records a return with remaining time strictly below5; `main_menu.gd:386-388` converts that flag into the reward. Scrapshift dispatches at its corresponding ReturnToSelection callback; persistence staging differs from reference's scene flag, with the same user-facing eligibility boundary.

## Finite inventory

All IDs retain the separately executed 115-definition model matrix and persistence coverage. R30 executed all109 new callback scenarios successfully; the report and all109 saved target profiles were independently read. The six historical IDs keep the exact scope of their existing evidence in `docs/unlock-world-events-2026-10-20.md` and survival documentation.

| ID | Imported boundary | Callback evidence family |
| --- | --- | --- |
| `unlock_difficulty_1` | 1  | Staged final-wave cleanup / danger or system finish |
| `unlock_difficulty_2` | 2  | Staged final-wave cleanup / danger or system finish |
| `unlock_difficulty_3` | 3  | Staged final-wave cleanup / danger or system finish |
| `unlock_difficulty_4` | 4  | Staged final-wave cleanup / danger or system finish |
| `unlock_difficulty_5` | 5  | Staged final-wave cleanup / danger or system finish |
| `unlock_difficulty_6` | 6  | Staged final-wave cleanup / danger or system finish |
| `chal_difficulty_0` | 0  | Staged final-wave cleanup / danger or system finish |
| `chal_difficulty_1` | 1  | Staged final-wave cleanup / danger or system finish |
| `chal_difficulty_2` | 2  | Staged final-wave cleanup / danger or system finish |
| `chal_difficulty_3` | 3  | Staged final-wave cleanup / danger or system finish |
| `chal_difficulty_4` | 4  | Staged final-wave cleanup / danger or system finish |
| `chal_difficulty_5` | 5  | Staged final-wave cleanup / danger or system finish |
| `chal_difficulty_6` | 6  | Staged final-wave cleanup / danger or system finish |
| `chal_survivor_1` | 300  | Lifetime lower-counter staging / final death or pickup / cleanup |
| `chal_survivor_2` | 2000  | Lifetime lower-counter staging / final death or pickup / cleanup |
| `chal_survivor_3` | 5000  | Lifetime lower-counter staging / final death or pickup / cleanup |
| `chal_survivor_4` | 10000  | Lifetime lower-counter staging / final death or pickup / cleanup |
| `chal_survivor_5` | 20000  | Lifetime lower-counter staging / final death or pickup / cleanup |
| `chal_gatherer_1` | 300  | Lifetime lower-counter staging / final death or pickup / cleanup |
| `chal_gatherer_2` | 2000  | Lifetime lower-counter staging / final death or pickup / cleanup |
| `chal_gatherer_3` | 5000  | Lifetime lower-counter staging / final death or pickup / cleanup |
| `chal_gatherer_4` | 10000  | Lifetime lower-counter staging / final death or pickup / cleanup |
| `chal_gatherer_5` | 20000  | Lifetime lower-counter staging / final death or pickup / cleanup |
| `chal_rookie` | 50  | Historical native producer (R26 / survival) |
| `chal_dying` | -5 hp_regeneration | GainWeaponStat / scene progression sweep |
| `chal_agriculture` | 200 harvesting | GainWeaponStat / scene progression sweep |
| `chal_hallucination` | 60 dodge | GainWeaponStat / scene progression sweep |
| `chal_fast` | 50 speed | GainWeaponStat / scene progression sweep |
| `chal_hoarder` | 3000  | CollectPickup / material and XP producer |
| `chal_turrets` | 5  | Imported acquisition, purchase or recycle / registered-effect callbacks |
| `chal_lumberjack` | 50  | Lifetime lower-counter staging / final death or pickup / cleanup |
| `chal_medicine` | 200  | Rule healing or compound stats / scene progression |
| `chal_perfect_vision` | 300 range | GainWeaponStat / scene progression sweep |
| `chal_fireworks` | 15  | Historical native producer (R26 / survival) |
| `chal_recycling` | 12  | Imported acquisition, purchase or recycle / registered-effect callbacks |
| `chal_slow` | -20 speed | GainWeaponStat / scene progression sweep |
| `chal_hungry` | 20  | Historical native producer (R26 / survival) |
| `chal_advanced_technology` | 10 ranged_damage | Imported acquisition, purchase or recycle / registered-effect callbacks |
| `chal_giant_slayer` | 15  | Activated scene target / real Damage lethal callback |
| `chal_robust` | 100 max_hp | GainWeaponStat / scene progression sweep |
| `chal_baited` | 2  | Imported acquisition, purchase or recycle / registered-effect callbacks |
| `chal_forest` | 10  | Scene census / damage or ground drops / cleanup |
| `chal_bourgeoisie` | 3  | Imported acquisition, purchase or recycle / registered-effect callbacks |
| `chal_student` | 20  | CollectPickup / material and XP producer |
| `chal_reckless` | 1  | Scene census / damage or ground drops / cleanup |
| `chal_scavenger` | 10  | Imported acquisition, purchase or recycle / registered-effect callbacks |
| `chal_fast_learner` | 10  | CollectPickup / material and XP producer |
| `chal_experimentation` | 6  | Imported acquisition, purchase or recycle / registered-effect callbacks |
| `chal_magic_and_machinery` | 10 elemental_damage | Imported acquisition, purchase or recycle / registered-effect callbacks |
| `chal_blood_drinker` | 40 lifesteal | GainWeaponStat / scene progression sweep |
| `chal_well_rounded` | 1  | Staged final-wave scene cleanup / character finish |
| `chal_brawler` | 1  | Staged final-wave scene cleanup / character finish |
| `chal_crazy` | 1  | Staged final-wave scene cleanup / character finish |
| `chal_ranger` | 1  | Staged final-wave scene cleanup / character finish |
| `chal_mage` | 1  | Staged final-wave scene cleanup / character finish |
| `chal_chunky` | 1  | Staged final-wave scene cleanup / character finish |
| `chal_old` | 1  | Staged final-wave scene cleanup / character finish |
| `chal_lucky` | 1  | Staged final-wave scene cleanup / character finish |
| `chal_mutant` | 1  | Staged final-wave scene cleanup / character finish |
| `chal_generalist` | 1  | Staged final-wave scene cleanup / character finish |
| `chal_loud` | 1  | Staged final-wave scene cleanup / character finish |
| `chal_multitasker` | 1  | Staged final-wave scene cleanup / character finish |
| `chal_wildling` | 1  | Staged final-wave scene cleanup / character finish |
| `chal_pacifist` | 1  | Staged final-wave scene cleanup / character finish |
| `chal_gladiator` | 1  | Staged final-wave scene cleanup / character finish |
| `chal_saver` | 1  | Staged final-wave scene cleanup / character finish |
| `chal_sick` | 1  | Staged final-wave scene cleanup / character finish |
| `chal_farmer` | 1  | Staged final-wave scene cleanup / character finish |
| `chal_ghost` | 1  | Staged final-wave scene cleanup / character finish |
| `chal_speedy` | 1  | Staged final-wave scene cleanup / character finish |
| `chal_entrepreneur` | 1  | Staged final-wave scene cleanup / character finish |
| `chal_engineer` | 1  | Staged final-wave scene cleanup / character finish |
| `chal_explorer` | 1  | Staged final-wave scene cleanup / character finish |
| `chal_doctor` | 1  | Staged final-wave scene cleanup / character finish |
| `chal_hunter` | 1  | Staged final-wave scene cleanup / character finish |
| `chal_artificer` | 1  | Staged final-wave scene cleanup / character finish |
| `chal_arms_dealer` | 1  | Staged final-wave scene cleanup / character finish |
| `chal_streamer` | 1  | Staged final-wave scene cleanup / character finish |
| `chal_cyborg` | 1  | Staged final-wave scene cleanup / character finish |
| `chal_glutton` | 1  | Staged final-wave scene cleanup / character finish |
| `chal_jack` | 1  | Staged final-wave scene cleanup / character finish |
| `chal_lich` | 1  | Staged final-wave scene cleanup / character finish |
| `chal_apprentice` | 1  | Staged final-wave scene cleanup / character finish |
| `chal_cryptid` | 1  | Staged final-wave scene cleanup / character finish |
| `chal_fisherman` | 1  | Staged final-wave scene cleanup / character finish |
| `chal_golem` | 1  | Staged final-wave scene cleanup / character finish |
| `chal_king` | 1  | Staged final-wave scene cleanup / character finish |
| `chal_renegade` | 1  | Staged final-wave scene cleanup / character finish |
| `chal_one_arm` | 1  | Staged final-wave scene cleanup / character finish |
| `chal_bull` | 1  | Staged final-wave scene cleanup / character finish |
| `chal_soldier` | 1  | Staged final-wave scene cleanup / character finish |
| `chal_masochist` | 1  | Staged final-wave scene cleanup / character finish |
| `chal_knight` | 1  | Staged final-wave scene cleanup / character finish |
| `chal_demon` | 1  | Staged final-wave scene cleanup / character finish |
| `chal_baby` | 1  | Staged final-wave scene cleanup / character finish |
| `chal_vagabond` | 1  | Staged final-wave scene cleanup / character finish |
| `chal_technomage` | 1  | Staged final-wave scene cleanup / character finish |
| `chal_vampire` | 1  | Staged final-wave scene cleanup / character finish |
| `chal_evil_hat` | 5  | Lifetime lower-counter staging / final death or pickup / cleanup |
| `chal_hourglass` | 5  | ReturnToSelection callback at 5 / 4.99 seconds |
| `chal_candy_bag` | 1  | Shop lock carry / final scene finish / clean next run |
| `chal_fruit_basket` | 100  | Historical native producer (R26 / survival) |
| `chal_will_o_the_wisp` | 30  | Historical native producer (R26 / survival) |
| `chal_ghost_outfit` | 70 dodge | GainWeaponStat / scene progression sweep |
| `chal_vorpal_sword` | 100 crit_chance | GainWeaponStat / scene progression sweep |
| `chal_bonk_dog` | 6  | Imported acquisition, purchase or recycle / registered-effect callbacks |
| `chal_catling_gun` | 6  | Imported acquisition, purchase or recycle / registered-effect callbacks |
| `chal_bot_o_mine` | 6  | Imported acquisition, purchase or recycle / registered-effect callbacks |
| `chal_blazemander` | 6  | Imported acquisition, purchase or recycle / registered-effect callbacks |
| `chal_doc_moth` | 10  | Rule healing or compound stats / scene progression |
| `chal_lootworm` | 20  | Scene census / damage or ground drops / cleanup |
| `chal_jellyshield` | 3  | Historical native producer (R26 / survival) |
| `chal_beast_master` | 1  | Staged final-wave scene cleanup / character finish |
| `chal_paws_n_claws` | 3  | Imported acquisition, purchase or recycle / registered-effect callbacks |
| `chal_wounded` | 1  | Staged final-wave scene cleanup / character finish |

## Executed R30 result and independent review

`artifacts/headless-completion-2026-10-20/unlocks-r30-attempt-1/headless-unlocks.json` reports **Passed**, empty error, **109 unique rows**. Every row has all six checks true: `below`, `completed`, `once`, `reloaded`, `frozen`, and `nextRun` — **654 true assertion fields**. `headless:true` and `capturesVerified:false` are explicit. `process.json` independently records process exit **0**; the run lasted approximately9.76 seconds. This duration is possible because callbacks and elapsed clocks are staged, not because109 full playthroughs occurred.

`independent-review.json` records a second read of the report and **all109 actual saved JSON profiles**: every target ID is present in its corresponding save; the109 unique new IDs plus six historical IDs exactly equal all115 imported definitions, with no overlap or omissions. The report SHA-256 is `2850e8b92a8489517b4d150aa9f22032b7ed23fe0aca1e69a90583fc3aef44fc`. This independent review verifies the recorded inventory and saves; the single-notification and lower-boundary checks are enforced by the fixture and recorded in its report.

The source audit found **no direct Progression.Complete, Check or Count shortcuts** in the fixture. Its direct counter writes are declared lower lifetime-counter staging and nine prior Danger5 records. Shop funds are staged through the Scrap setter; weapons/offers, marker activation, HP and target age are staged as described above. Real producer paths include lethal Damage callbacks, CollectPickup, imported acquisition/Buy/Recycle, Hurt/Heal, scene cleanup and TickProgression. The snapshot checks instantiate a separate eligibility pool rather than pretending to reproduce a subsequent human-controlled scene. Character/system rewards are expected to expose immediately; item/weapon rewards remain frozen until a new pool snapshot.

No new gameplay mismatch was found in these109 focused scenarios. This is a verification unit, not evidence that all possible input sequences, organic thresholds or timing interleavings match the original.

## What this can close and what remains

The passing109-row receipt closes dedicated staged producer/persistence/next-run acceptance for those109 definitions at callback level. It cannot establish threshold naturally reached during play, automated Update scheduling for each case, visual reward disclosure/popups, current picker/menu/controller paths, or human survival/balance. Those interactive gates remain deferred under the user's no-computer-control instruction. None of the cases calls Progression.Complete/Check/Count to manufacture the target completion; direct lower-counter/history staging is declared above.

Codex authored the fixture and source investigation under the user's scope. It is interview rehearsal evidence, not a claim that the user independently performed these debugging or test steps.


R31 repeat: `artifacts/headless-completion-2026-10-20/unlocks-r31-attempt-1/headless-unlocks.json` Passed/exit0,109 rows and654 true fields. Independent review reread all109 saved profiles and verified the same target set as R30. This repeat uses the packaged Golem correction; original staging and presentation limits still apply.

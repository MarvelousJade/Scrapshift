# Remaining runtime, UI and hardware acceptance - October 20, 2026

This is a finite follow-up checklist, not an assertion of remaining bugs or a new authorization for computer control. **The user revoked computer control. Continue source/headless work only; visible UI/input/hardware checks below wait for explicit authorization.** Do not launch them from this document.

## Evidence boundary

- R29 built with zero errors/warnings; 596 source inputs and 159 player files were recorded by the coordinator. Its full headless run Passed/Won20/exit0 with18/18 comparator checks; see the [final checkpoint](gameplay-final-checkpoint-2026-10-20.md). Its ordinary-damage run Passed/exit0 with natural defeat in wave8 after7 completed waves; see the survival report.
- R28 completed the frozen earlier invulnerable full campaign and unchanged 18/18 comparison. It predates the Beast Master selectable-start correction and Wrench/support correction. It is not a current R29 campaign result.
- R27 completed 22 shop action/modal checks with virtual input and eight reviewed captures. R26 completed 23 checks for five unlock event producers and four reviewed captures. R25/R26 survival evidence includes Rookie death/retry behavior. These are bounded staged scenarios, not a human full campaign or current physical-controller acceptance.
- All 115 imported unlock definitions have producer/save/reload/next-run model scenarios. Six IDs have the dedicated native producer evidence summarized below; 109 still lack equivalent individual native threshold acceptance in this inventory.

## Completed headless evidence

| ID | Check | Success criterion |
| --- | --- | --- |
| H1 - complete | R29 full run compared with preserved original | Passed/Won20/exit0,19 shops,18/18 unchanged checks; final96-material credit and40/40 debits reconcile. Exact hashes and receipts are in the final checkpoint. No UI claim. |
| H2 - complete | R29 ordinary-damage run and source counterpart read | R29 Passed/exit0, natural defeat wave8 after7 completions; source lost wave9 after8 completions. Cleanup gate and actual receipts are documented in [survival comparison](headless-survival-comparison-2026-10-20.md); no damage-formula equality or20-wave claim. |

These completed runs are preserved. Do not repeat them without a changed-input risk. A failing invariant becomes a separate source-backed fix with focused checks and its own commit before repeating only the affected validation.

## Priority 1: changed UI and actual controls, after authorization

| ID | Check | Success criterion |
| --- | --- | --- |
| U1 | Beast Master selectable item UI, all four choices | Each visible choice starts the selected companion plus Lootworm only: two items, no weapons, source-derived +4 Speed. Preview, selected highlight, A/click activation and started inventory agree. Selection is not an automatic grant of all four choices. |
| U2 | Corrected Wrench/support registration and unlock presentation | The weapon effect registers once; spawning actors adds no registrations. The fifth qualifying registered turret effect triggers `chal_turrets`; four does not. No acceptance based only on fifth live turret. |
| U3 | Weapon action modal on current build | Only Combine/Recycle/Cancel controls register (two without Combine); background price/reroll/stat/sort/inventory controls cannot activate. Cancel preserves exact offers, weapons and currency. Retain R27 evidence but collect fresh current-build results for changed code. |
| U4 | One physical Xbox navigation/combat/intermission circuit with ordinary focus settings | Left stick/D-pad navigate visible controls; A activates highlighted control, B returns, Menu/Start pauses/resumes correct state. Combat movement/right-stick aiming work without menu-switch cues. Shop lock/reroll, weapon popup, crate choice, upgrade and defeat/retry confirmation work once per intended action. No fixture IgnoreFocus override. Record hardware connection/model and observed failures. |
| U5 | Keyboard/mouse and focus lifecycle | Movement, aiming, click/confirm, pause/resume and returning focus preserve state; hover/selection and activation agree. No background action behind modals. Do not infer source keyboard defaults from the supplied Godot project's blank key codes. |

No new screenshots, physical input, audible claims or UI results are established by a headless run. The earlier client-versus-desktop coordinate suspicion remains unconfirmed; record same-frame root Go/offer and scroll inventory rectangles/window origin before calling it a geometry bug.

## Priority 2: finish imported special-shop alternatives

R27 already covers actual purchase/lock/unlock/free+paid reroll/inspect/cancel/combine/recycle/crate take+disposal/Hourglass Go, plus Arms Dealer/Fisherman/Anvil entry and a forced Candy encounter branch. Retest those only when changed inputs require it. The following finite additions close the explicit gaps without expanding uncarried synthetic effects:

| ID | Additional acceptance | Success criterion |
| --- | --- | --- |
| S1 | Character price modifiers | Stage real identical offers at the same wave for Mutant, Saver, Entrepreneur, Glutton and Baby; compare displayed price and actual debit to the source integer formula. Verify Arms Dealer weapon modifier and Fisherman specific bait price separately. Keep item/weapon/specific modifiers distinct. |
| S2 | Current Demon health purchase boundary | Raw HP budget1 pays cost1 once; raw0 rejects the next offer even if it grants HP; combat minimum remains separate. Material header/reroll remain material-priced; purchase uses heart; raw HP stat0. Extend raw negative/fractional boundaries from model to rendered current UI only if needed. |
| S3 | Full weapon inventory purchase alternatives | At capacity, matching eligible duplicate purchase spends once and combines correctly; unmatched/unusable weapon cannot purchase. Cancelling inspection spends nothing. Maximum-tier copies never form an unsupported tier. |
| S4 | Free/paid reroll and all-sold boundaries | Buying an additional Dangerous Bunny makes its additional free roll usable immediately; exhausted free rolls resume correct paid count. All-sold bonus and four locked offers follow source eligibility/consumption order; rejected actions leave currency/offers unchanged. |
| S5 | Lock persistence into next shop | Carry a real locked offer across Go/wave/shop; source lock preference and retained price apply. Temporary lock/unlock does not mark used-lock state; carrying an active lock out does. Show exact offer identity/value and current UsedLocks receipt. |
| S6 | Acquisition/removal and random target boundaries | Coupon/Recycling Machine/Anvil acquisition/removal affects the next applicable operation exactly once. For Anvil, every selectable eligible target belongs to the source pool; no-target fallback is +2 Armor. Do not infer target probability from a single run. |
| S7 | Hourglass and Candy natural conditions | Hourglass cannot be locked, repeats expected wave and replaces itself once. Candy uses its real 10% chance over an explicitly bounded seed matrix; successful branch adds one eligible encounter and failed branch none. R27's forced100% branch proves consequence only. |

For each staged native action, record before/after currency, item/weapon identity/tier, offer lock/price state, actual input path and report/process result. A visible current-build UI sample is needed for presentation; source/headless model receipts alone can verify only logic. Generalized stealing/duplication/tier-reroll/stat-reroll have no imported carrier and are not added as naturally acquired content requirements.

## Priority 3: complete the remaining unlock producer families

Dedicated native producer evidence currently covers Hungry, Fruit Basket, Jellyshield, Will-o-the-Wisp, Fireworks and Rookie. The following **26 specific + 83 generic = 109** definitions remain a bounded inventory. They can use headless runtime producer fixtures where no UI is required; UI-specific actions wait for authorization.

| Family | Count | IDs or category | Success criteria beyond existing model cases |
| --- | ---: | --- | --- |
| Registrations, healing, timed/lifetime kills | 5 | turrets, lumberjack, medicine, giant_slayer, evil_hat | Fifth registered structure effect after four; actual tree/evil deaths checked at cleanup; actual healing reaches200 within one wave; actual elite/boss death below/equal/above15 seconds. Weapon-created/live actor multiplicity does not count as registered effects. |
| Cleanup snapshots | 3 | forest, reckless, lootworm | Actual wave cleanup reads qualifying living-tree count, HP exactly1 and ground-material value; adjacent nonqualifying boundary does not unlock. |
| Shop/inventory producers | 6 | hoarder, recycling, baited, bourgeoisie, scavenger, experimentation | Actual material/acquisition/recycle producers reach the definition threshold, with one below case. In particular twelve recycles and two bait acquisitions are distinct from R27's one recycle/bait offer. |
| XP and wave boundary | 2 | student, fast_learner | Actual XP/level producer reaches definition level; Fast Learner qualifies before wave6 and not after the deadline. |
| Structure/scaling/stat/compound | 8 | advanced_technology, magic_and_machinery, bonk_dog, catling_gun, bot_o_mine, blazemander, doc_moth, paws_n_claws | Actual acquisition triggers registered-effect/stat requirements; six raw-scaling weapons qualify their respective companion; all five earned prerequisites complete compound reward once, without directly completing prerequisite challenges. |
| Menu/run eligibility | 2 | hourglass, candy_bag | Actual return-to-selection <5 seconds qualifies, at5 does not; actual no-lock victory qualifies and carrying a locked offer out disqualifies. Using these items is not earning their rewards. |
| Character victories | 50 | Every imported character-win definition | Correct character victory completes its target; loss/wrong character does not. Assert target producer, saved reward and next-run eligibility per ID. |
| Raw-stat boundaries | 10 | Imported plain-stat definitions | Actual stat-producing acquisition/level/effect callback reaches exact signed threshold; adjacent nonqualifying value does not. Respect raw/stat-gain semantics. |
| Danger victories | 7 | Danger0 through6 | Actual matching danger victory dispatches exact target; loss/nonmatching danger does not establish it. |
| Difficulty systems | 6 | Prior-danger gates plus Nightmare | Prior-tier victory opens correct gate; Nightmare counts ten distinct Danger5 character records, not ten repeated wins with one character. |
| Lifetime thresholds | 10 | Survivor/Gatherer families | Actual kill/material producers reach threshold, deferred cleanup check completes once; partial progress survives save/reload and later run. |

ID shorthand in the first six rows means `chal_<id>`; exact imported IDs remain in `artifacts/shop-unlocks-followup-2026-10-17/unlock-dispatch-coverage.json`.

Every new native producer case must also record: exactly one target notification; real isolated profile save/reload; character visibility at the appropriate menu; current-run item/weapon pool frozen across immediate completion and retry; fresh-run reward eligibility. Use actual producer callbacks, not direct `Complete(target)`/counter staging while calling it world-event verification. Threshold setup may be explicitly staged if the final event and its boundary are exercised; label that limitation.

## Stop criteria and separate release work

Close each row only with evidence scoped to its success criterion. New confirmed mismatches become focused fixes; passing rows do not require repeated broad campaigns without a changed-input risk. Do not call all115 native-verified until every definition has a mapped producer case. Do not call exact universal gameplay parity from these finite checks.

Browser saves/audio/focus/fullscreen, crowded-wave graphics performance, release/licensing and exhaustive arbitrary item combinations remain separate work; they are not silently certified by this acceptance list. The coordinator maintains bug/interview/status notes with the same evidence boundaries and commits each logical validated change separately.

# Remaining runtime, UI and hardware acceptance - October 20, 2026

This is a finite follow-up checklist, not an assertion of remaining bugs or a new authorization for computer control. **The user revoked computer control. Continue source/headless work only; visible UI/input/hardware checks below wait for explicit authorization.** Do not launch them from this document.

## Evidence boundary

- R29 built with zero errors/warnings; 596 source inputs and 159 player files were recorded by the coordinator. Its full headless run Passed/Won20/exit0 with18/18 comparator checks; see the [final checkpoint](gameplay-final-checkpoint-2026-10-20.md). Its ordinary-damage run Passed/exit0 with natural defeat in wave8 after7 completed waves; see the survival report.
- R28 completed the frozen earlier invulnerable full campaign and unchanged 18/18 comparison. It predates the Beast Master selectable-start correction and Wrench/support correction. It is not a current R29 campaign result.
- R27 completed 22 shop action/modal checks with virtual input and eight reviewed captures. R26 completed 23 checks for five unlock event producers and four reviewed captures. R25/R26 survival evidence includes Rookie death/retry behavior. These are bounded staged scenarios, not a human full campaign or current physical-controller acceptance.
- All 115 imported unlock definitions retain producer/save/reload/next-run model scenarios. R30 adds **109/109 staged scene-callback scenarios Passed/exit0**, complementing six historical native producers. This closes the mapped callback inventory, not 109 organic Update/physics/UI playthroughs; see [headless unlock acceptance](headless-unlocks-2026-10-20.md).
- R30 special-shop logic passed **14/14 action receipts**, covering S1–S7 below; current presentation/input remains deferred. The dynamic item oracle matched the original on **53 cases /397 snapshots /6,352 scalar comparisons, zero differences**; see [shop acceptance](headless-shops-2026-10-20.md) and [dynamic item evidence](headless-items-2026-10-20.md). Arbitrary larger/timed interactions remain outside that finite matrix.

## Completed headless evidence

| ID | Check | Success criterion |
| --- | --- | --- |
| H1 - complete | R29 full run compared with preserved original | Passed/Won20/exit0,19 shops,18/18 unchanged checks; final96-material credit and40/40 debits reconcile. Exact hashes and receipts are in the final checkpoint. No UI claim. |
| H2 - complete | R29 ordinary-damage run and source counterpart read | R29 Passed/exit0, natural defeat wave8 after7 completions; source lost wave9 after8 completions. Cleanup gate and actual receipts are documented in [survival comparison](headless-survival-comparison-2026-10-20.md); no damage-formula equality or20-wave claim. |
| H3 - complete at callback level | R30 unlock inventory |109/109 target rows, all 654 assertion fields true, actual exit 0; all 109 JSON saves independently checked. Six historical native IDs complete the115-ID mapped inventory. Eligibility uses separate synthetic snapshots, not actual subsequent scenes. |
| H4 - logic complete / UI pending | R30 imported special-shop alternatives |14/14 receipts Passed/exit0 across12 grouped scenarios; S1–S7 logic is covered, current displayed/input behavior is not. |
| H5 - finite comparison complete | Dynamic item oracle |53 cases,397 snapshots,6,352 scalar comparisons match the original, zero differences. Callback calculations, not scene timer scheduling or arbitrary combinations. |

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

## Priority 2: special-shop logic covered; visible acceptance deferred

R27 already covers actual purchase/lock/unlock/free+paid reroll/inspect/cancel/combine/recycle/crate take+disposal/Hourglass Go, plus Arms Dealer/Fisherman/Anvil entry and a forced Candy encounter branch. Retest those only when changed inputs require it. R30 now covers the following **S1–S7 logic**, with 14 passing receipts in the [shop report](headless-shops-2026-10-20.md). The table retains the visible acceptance targets for when desktop control is allowed; displayed prices, input and actor physics are not certified by those receipts:

| ID | Additional acceptance | Success criterion |
| --- | --- | --- |
| S1 — logic covered | Character price modifiers | Stage real identical offers at the same wave for Mutant, Saver, Entrepreneur, Glutton and Baby; compare displayed price and actual debit to the source integer formula. Verify Arms Dealer weapon modifier and Fisherman specific bait price separately. Keep item/weapon/specific modifiers distinct. |
| S2 — logic covered | Current Demon health purchase boundary | Raw HP budget1 pays cost1 once; raw0 rejects the next offer even if it grants HP; combat minimum remains separate. Material header/reroll remain material-priced; purchase uses heart; raw HP stat0. Extend raw negative/fractional boundaries from model to rendered current UI only if needed. |
| S3 — logic covered | Full weapon inventory purchase alternatives | At capacity, matching eligible duplicate purchase spends once and combines correctly; unmatched/unusable weapon cannot purchase. Cancelling inspection spends nothing. Maximum-tier copies never form an unsupported tier. |
| S4 — logic covered | Free/paid reroll and all-sold boundaries | Buying an additional Dangerous Bunny makes its additional free roll usable immediately; exhausted free rolls resume correct paid count. All-sold bonus and four locked offers follow source eligibility/consumption order; rejected actions leave currency/offers unchanged. |
| S5 — logic covered | Lock persistence into next shop | Carry a real locked offer across Go/wave/shop; source lock preference and retained price apply. Temporary lock/unlock does not mark used-lock state; carrying an active lock out does. Show exact offer identity/value and current UsedLocks receipt. |
| S6 — logic covered | Acquisition/removal and random target boundaries | Coupon/Recycling Machine/Anvil acquisition/removal affects the next applicable operation exactly once. For Anvil, every selectable eligible target belongs to the source pool; no-target fallback is +2 Armor. Do not infer target probability from a single run. |
| S7 — logic covered | Hourglass and Candy natural conditions | Hourglass cannot be locked, repeats expected wave and replaces itself once. Candy uses its real 10% chance over an explicitly bounded seed matrix; successful branch adds one eligible encounter and failed branch none. R27's forced100% branch proves consequence only. |

For each staged native action, record before/after currency, item/weapon identity/tier, offer lock/price state, actual input path and report/process result. A visible current-build UI sample is needed for presentation; source/headless model receipts alone can verify only logic. Generalized stealing/duplication/tier-reroll/stat-reroll have no imported carrier and are not added as naturally acquired content requirements.

## Priority 3: callback unlock inventory covered; organic/UI acceptance deferred

Historical native producer evidence covers Hungry, Fruit Basket, Jellyshield, Will-o-the-Wisp, Fireworks and Rookie. R30 adds the remaining **26 specific +83 generic =109** definitions through synchronously invoked production scene callbacks. [The 109-row report](../artifacts/headless-completion-2026-10-20/unlocks-r30-attempt-1/headless-unlocks.json) Passed, with all six flags true per row: lower/nonqualifying boundary, completion, one notification, actual JSON reload, frozen active eligibility and fresh-pool eligibility. [Independent review](../artifacts/headless-completion-2026-10-20/unlocks-r30-attempt-1/independent-review.json) read all 109 saves and confirmed that109 plus historical 6 exactly equals the115 imported IDs.

| Family | Count | R30 callback-level acceptance |
| --- | ---: | --- |
| Registrations, healing, timed/lifetime kills |5| Four/five registered turret effects;199/200 healing through Hurt/Heal; staged target ages15.001/15 through lethal Damage; final tree/evil death after declared lower lifetime-counter staging and cleanup checking. |
| Cleanup snapshots |3| Scene tree census, HP2/1 and ground materials19/20 reach production cleanup callbacks. Spawn activation, positions and elapsed cleanup clocks are staged. |
| Shop/inventory producers |6| Material pickups,12 imported weapon purchases/recycles, two bait acquisitions, tier-IV copies and distinct item/weapon acquisition callbacks, with nonqualifying lower cases. Offers and funds are staged. |
| XP and wave boundary |2| CollectPickup reaches Student20; Fast Learner rejects level 10 on wave 6 and accepts level 10 on wave 5. Pickup callbacks are invoked; contact physics is not tested here. |
| Structure/scaling/stat/compound |8| Imported registrations and raw-scaling purchases; compound regeneration/lifesteal boundaries; all five prerequisite rewards earned through their producer callbacks, not direct Complete calls. |
| Menu/run eligibility |2| ReturnToSelection at5 versus4.99 seconds; carrying a shop lock into a win rejects Candy Bag, then staged no-lock victory accepts. Current visible menu/input remains pending. |
| Character victories |50| Per-character final-wave scene settlement and TickProgression dispatch, using eligible starters and neutral0 currency. These are not50 natural campaigns or every wrong-character/loss permutation. |
| Raw-stat boundaries |10| GainWeaponStat and scene progression callback check adjacent/exact signed thresholds. These stats are staged, not naturally acquired builds. |
| Danger victories |7| Per-danger staged final-wave settlement and finish dispatch; not seven natural danger campaigns. |
| Difficulty systems |6| Final scene finish opens the prior-tier gate; Nightmare stages nine distinct prior Danger 5 records and exercises the tenth through finish. |
| Lifetime thresholds |10| Lower lifetime counters are directly staged; a final real death or CollectPickup callback plus cleanup triggers completion. Earlier thousands of events are not reenacted. |

The runtime game component is disabled while these callbacks are invoked synchronously: this is **not automatic Update scheduling, pickup collision delivery, organic survival or UI acceptance**. Persistence is actual disk JSON save/reload; eligibility assertions use a **separate synthetic active/fresh pool snapshot**, not an actual human-controlled next-run scene. No Progression.Complete/Check/Count shortcut manufactures a target completion. Full IDs, source anchors and exact limits are in [headless unlock acceptance](headless-unlocks-2026-10-20.md).

Current unlock popup rendering, reward disclosure, visible character availability, retry input and organically delivered events remain deferred. Retain historical R26 Update/physics evidence separately; do not relabel the 109 callback cases as equivalent native world-event playthroughs.

## Stop criteria and separate release work

Close each row only with evidence scoped to its success criterion. New confirmed mismatches become focused fixes; passing rows do not require repeated broad campaigns without a changed-input risk. All 115 now have a mapped producer case with differing evidence levels; do not call them115 organic native-verified playthroughs. Do not call exact universal gameplay parity from these finite checks.

Browser saves/audio/focus/fullscreen, crowded-wave graphics performance, release/licensing and exhaustive arbitrary item combinations remain separate work; they are not silently certified by this acceptance list. The coordinator maintains bug/interview/status notes with the same evidence boundaries and commits each logical validated change separately.

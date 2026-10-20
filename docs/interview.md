## October20 - acquisition context and legal stack fixtures

The follow-up source comparison matched416 acquisition/removal cases and4576 snapshots at intermission. Initial standing-stat differences came from omitted callbacks in an injected mid-wave fixture; no production fix was inferred.18 focused lifecycle/proc tests passed after source-backed setup corrections for level-up HP and delayed Max HP reconciliation. Explain why legal item caps, lifecycle context and unrelated level-up gains must be controlled before calling a mismatch. Codex authored and ran this evidence under user direction. See [acquisition evidence](item-acquisition-order-followup-2026-10-20.md).

## October20 - reconciling asynchronous event traces

A bounded source-engine material probe balances28 deaths,28 requested/registered units and8 collected plus20 banked. Intermediate callbacks can run before deferred registration or currency credit, so compare complete lifecycle points before changing gameplay formulas. The older25/24 observation remains unexplained. Codex prepared and analyzed this probe; it is an evidence-quality rehearsal, not a user-discovered payout fix. See [material probe](material-accounting-probe-2026-10-20.md).

## October20 - derived stats can recreate cleared state

Golem's low-health bonus exposed a lifecycle issue: clearing stored temporary stats did not prevent a computed conditional from adding the bonus again at HP0. Codex traced the original death/end-run reset chain, limited the conditional to nonterminal phases, and added defeat/victory regressions while preserving paused-combat behavior. The dynamic source comparison passed53 cases/397 snapshots;26 focused item/shop cases passed. See [item evidence](headless-items-2026-10-20.md). Explain the difference between stored state and derived state, and why a reset alone was insufficient. The user directed this parity work; Codex investigated, implemented and ran these checks. Reproduce the example before presenting personal debugging claims.

## October20 - staged producers and real persistence

The remaining109 unlock definitions now have bounded runtime callback scenarios, lower-bound checks, exactly-once notifications and real isolated JSON save/reload receipts. Pool checks use separate active/fresh snapshots; clocks, threshold setup and final-wave state are staged. This is stronger than calling Complete directly, but does not reproduce109 natural playthroughs or UI interactions. See [unlock evidence](headless-unlocks-2026-10-20.md). Discuss how to isolate saves and distinguish a producer callback from its scheduling and presentation.

## October20 - validate the setup before changing economy rules

The expanded shop price check failed for Entrepreneur because the fixture supplied money before Start, and the character correctly discarded that starting purse. Codex followed the source startup modifier and moved staged funding after Start without generating pickup XP. The corrected12 shop groups and14 headless player receipts passed. Explain why a failed check can indicate an invalid setup rather than a gameplay defect, and why logic receipts do not prove displayed prices or mouse/controller behavior. Codex performed this investigation; reproduce the case before claiming personal execution.

## October20 - final integrated evidence

Use [R29 final checkpoint](gameplay-final-checkpoint-2026-10-20.md) for the current rehearsal numbers:1,200 passing tests,10,500 character/item static cases and18/18 bounded checks after a current20-wave campaign. Explain why these are different evidence types. The original and R29 have different random purchases/stacks; each debit and settlement must reconcile internally. Do not claim deterministic cross-engine outcomes, a human survival victory or fresh controller acceptance. Codex ran these checks; reproduce and understand the source chain before presenting personal execution.

## October20 - follow the caller before deciding what a count means

The original character matrix exposed Technomage at +6 Elemental Damage versus Unity's +4. A prior test had incorrectly asserted that Wrench was excluded. Following `RunData.add_weapon` into `StructureEffect.apply` established that a weapon registers its effect before spawning anything. Distinguish registered structures, broad character-owned counts, turret-class registrations and live actors. Each has different consumers. The fix updates links, achievements and placement without counting every spawned actor as a registration. Codex authored the investigation and corrections; reproduce the caller chain and explain the earlier mistaken test before presenting this story.

## October20 - compare runs without overstating the result

The user paused desktop access, so Codex added explicit headless campaign modes rather than treating missing screenshots as visual evidence. The ordinary-damage mode accepts natural defeat only after cleanup and excludes the failed wave from completion. Original and Unity runs lost on different waves, with different random upgrades/items; health metrics also sample different boundaries. Explain why those totals cannot identify a damage-formula bug. Use source-derived controlled checks for causality and the full campaign for integration coverage. See [survival evidence](headless-survival-comparison-2026-10-20.md).

## October20 - preserve selection semantics in imported data

The character oracle exposed a schema mistake: Beast Master's selectable starting pets had been merged with automatic character grants. Separating `startingItemChoices` from `startingItems` preserves what the reference data means. Trace the selected item through the picker, preview and `RunRules.Start`, then explain why explicit effect grants happen afterward. The runtime stores selection IDs before refreshing unlock eligibility so list changes cannot change the chosen item by index. Focused tests cover every pet alternative, default/invalid input and pet-linked stats; source/Unity comparison checks static outcomes. Native picker/controller verification is pending. Codex investigated and implemented this change; reproduce it before presenting it as personally performed debugging.

## October20 - modal shop interaction

Explain how a locally valid availability assignment can break an enclosing modal. GUI.enabled must combine parent interactivity with local affordability rather than overwrite it. Codex fixed this and verified actual UI actions; the initial fixture failure was a scroll-view lookup error and was retained separately. A possible navigation-coordinate issue remains unconfirmed and requires a same-frame origin receipt. See [shop report](shop-parity-remaining-2026-10-20.md). Further desktop checks are deferred at the user's request.

## October20 - acknowledge loss before unlock rewards

Full-screen unlock rewards could intercept the default loss acknowledgment. Defer that modal until the player selects OK/End run, preserving the global reward queue across Retry. R26 actual Update/virtual-A checks passed without suppressing Rookie rewards; all8 fresh captures reviewed. Actual Sick, Wounded and Golem boundary cases also passed. See [survival report](survival-parity-remaining-2026-10-20.md). Codex performed implementation and validation.

## October20 - registered pets versus live actors

Beast Master stat links incorrectly followed live actor counts. The source counts registered pet effects, scalar shields and pet structures instead. Exclude empty presentation-only PetSpec entries to avoid counting Jellyshield twice.10 selected checks passed; see [item report](items-parity-remaining-2026-10-20.md). Codex performed this investigation and validation.

## October20 - character gains on linked stats

Trace a derived stat through its inherited source accessor: the link formula itself was correct, but its output bypassed character gain. Real catalog tests also exposed incorrect test assumptions about Coffee penalties and distinct-item removal. Codex corrected the test expectations against source rather than changing correct behavior to satisfy them.16 selected checks passed; native validation is pending. See [item interaction report](items-parity-remaining-2026-10-20.md).

## October20 - optimized cleanup option

The source option was missing. Added entry-time per-pile common material boosting and banking, without XP/healing/pickup effects; default behavior remains raw banking.21 focused checks Passed; native validation pending. See [optimized cleanup report](optimized-cleanup-parity-2026-10-20.md).

## October20 - periodic damage death transition

A shared state enum is insufficient when entering a state requires cleanup. Enemy hits entered BeginDeathCleanup, but periodic self-damage assigned Lost directly. Codex traced the source take_damage/die chain and corrected this alternate entry, then ran16 focused checks. Explain the missing transition side effects and why Abandon stays separate; native evidence is still pending. See [remaining survival report](survival-parity-remaining-2026-10-20.md).

Current verified and unchecked coverage: [gameplay verification status](gameplay-verification-status.md).

## Current rehearsal - item, survival and unlock lifecycle corrections (R25)

Explain how a passing count/cap test can miss a combined effect: burning had correct payloads but used a 1s base instead of the source Unit timer's 0.5s. Trace ItemLifecycle.BurningInterval through combined-stat tests and actual 60 Hz native burn traces. Forced burn application does not test proc probability.

Trace RunRules.Hurt -> BeginDeathCleanup -> TickDeathSweep -> retry/acknowledgment. A lethal hit must stop later attacks and pickups in the same tick. Half-second cleanup banks raw material without XP/harvesting; consumables cannot heal a dead player. Optional retry restores an explicit wave-start checkpoint while global achievements and RNG remain live. Explain the tradeoff: new persistent run fields require deliberate checkpoint coverage; copying all objects would copy state that must stay live.

Explain why combat minimum HP differs from spendable raw Max HP. Model tests caught repeated last-HP purchases; native screenshot review exposed incorrect currency icons/header/stat values. For unlocks, global completion differs from current-run eligibility: dying can complete Rookie while retry retains its frozen item pool. All 115 definitions have producer/save-reload/next-run tests; counter-adapter cases do not prove native collision producers.

Evidence: 1181/1181 full suite, three native R25 fixtures,12 fresh captures, zero build errors/warnings. See [priority report](gameplay-priority-followup-2026-10-17.md). Codex investigated, implemented and tested under the user's direction. Reproduce these traces before claiming personal hands-on debugging. Exhaustive item pairs and human survival balance remain unverified; no new 20-wave result is claimed.

## Historical completed checkpoint - October17, R23

The complete suite passed **1031/1031**, zero failures/skips, exit0. R23 built with zero errors/warnings at `Builds/GameplayParity20261017R23/LaunchGame.cmd`;550 recorded source/import/settings inputs and160 player files were verified again after the run. Native virtual navigation passed17 snapshots and final settlement passed both routes, exit0. The final normal-speed scripted20-wave campaign completed Won/exit0,20 waves/19 shops, and the comparison with the completed supplied-original run passed **18/18 bounded checks**. Its final receipt credited109 materials (1227 to1336), then Harvesting grew109 to115. See [complete evidence](full-verification-2026-10-17.md).

The verification phase fixed landing clocks/transforms, body/raw spawn and camera geometry, authored muzzle offsets, final income/XP and cleanup ordering. The crowded-wave follow-up removed repeated allocation work in weapon-set calculations, immutable launch classification and collision-resource lookups. In this final scripted run, combat-window wall excess was-.714 seconds across1050.200 active seconds, versus R21's170.537-second excess. Small negative differences reflect trace callback timing; these were separate evolving runs, not identical workloads or FPS benchmarks. The controlled synthetic profiler comparison is [documented separately](late-wave-allocation-investigation-2026-10-17.md).

All confirmed differences found in this phase are corrected and validated within the recorded scope. This is not proof of every character/item combination, danger level, human survival balance, ordinary focus behavior or new physical-controller testing. The campaigns used neutral stats, six tier-II weapons, scripted movement, invulnerability and mute. The virtual-controller fixture has an explicit fixture-only IgnoreFocus policy; normal gameplay policy is unchanged. Failed/interrupted attempts and R19's final-income failure remain preserved. Older checkpoints below are dated history, superseded by this completed R23 checkpoint.

Codex performed the investigations, implementation, builds and scripted tests; the user supplied direction/reference and earlier physical Xbox feedback. Interview stories remain rehearsal material until reproduced and understood.

## Rehearsal: diagnosing crowded-wave slowdown (October17)

The project completed its campaign but its late-wave simulation fell behind wall time. Codex captured a separate Development CPU profile, then added subsystem markers and isolated microbenchmarks. The first evidence showed Update/GC cost rather than rendering. Repeated weapon-set queries created LINQ objects; settings repeatedly copied immutable process arguments; quadratic crowd pairs repeatedly allocated geometry-search predicates.

The correction uses loops for set calculations, caches launch classification and indexes the immutable geometry resource. It deliberately avoids caching mutable combat stat results: health, materials, movement, live counts and inventory can change them. Tests compare old/new set calculations across the catalog and mixed inventories, and check all38 authored geometry records. Two agent reviews checked identity, duplicate rows, float accumulation, fallbacks and live settings.

The synthetic profile's allocation markers fell99.27% per simulation tick. This is diagnostic evidence with short evolving captures and profiler overhead, not a normal FPS claim. A per-thread Mono allocation API returned0 despite collections; it was labeled unavailable instead of cited as zero allocation. The final R23 scripted campaign also completed20/Won/exit0: combat-window wall excess was-.714s across1050.200 active seconds, compared with R21+170.537s. Different evolving workloads and trace callback timing limit that comparison. See [investigation and artifacts](late-wave-allocation-investigation-2026-10-17.md).

Rehearse: trace one crowd-pair lookup; explain why static resource lookup can be cached but live stat results cannot; explain why normalizing by simulation ticks matters; show a regression that preserves multiset membership or float summation. This was Codex-run work under the user's direction. Reproduce and understand it before describing personal implementation/debugging ownership.

## Historical validation checkpoint - October 17, R21

The complete suite passed **1018/1018**, zero failures or skips. The supplied original and current R21 each completed a fresh normal-time scripted20-wave run with19 shops, Won and process exit0. The expanded R21 comparison passed **18/18 bounded checks**, including the corrected final34-material payout. Both native settlement routes passed with payout atcleanup start, growth at.6667s and victory after4.0167 simulated seconds. See [current evidence and limits](full-verification-2026-10-17.md).

Confirmed corrections cover landing clocks/movement/transforms, body/raw spawn and camera geometry, authored numeric muzzle offsets for84 ranged profiles, final-wave income/XP, the last-boss.1-second delay and source cleanup ordering. Failed/interrupted/historical attempts remain preserved; R19's17/18 result exposed the final-income defect rather than being rewritten as a pass.

Remaining observed issue: R21 combat windows took170.537 seconds more wall time than active simulation. The cause is not established. A separate Development build captured baseline and crowded CPU profiles. The synthetic crowded fixture allocated about 2.4 GB inside 16 ArenaGame.Update calls; GC and Update dominate while rendering is small. This narrows the investigation but does not yet identify the inner cause or measure ordinary R21 FPS. See `artifacts/late-wave-profile-2026-10-17/allocation-comparison.json`. The verified R21 playable build stays frozen. Do not claim exact overall or performance parity. These campaigns used a neutral six-weapon loadout, scripted movement, invulnerability and mute; human survival, every item-effect combination and new physical-controller testing are separate evidence.

Allocation follow-up: measured weapon-set/settings and quadratic crowd geometry lookup allocations have been corrected. The new complete suite passed1031/1031 (attempt8), and synthetic allocation markers fell99.27% per simulation tick; this is not ordinary-game FPS. R22 navigation exposed focus-sensitive synthetic input resets, so its opt-in fixture is being isolated before the next full-run timing check. See [profiling evidence and limits](late-wave-allocation-investigation-2026-10-17.md).

Codex performed the investigations, code/tests/builds and scripted validation. The user supplied direction, reference and earlier hands-on Xbox feedback. Interview examples are rehearsal material until reproduced and understood, not independently performed user debugging stories.

## Rehearsal: one settlement across two clocks

Trace `RunRules.BeginWaveSettlement`, `AdvanceWaveSettlement` and `CompleteWaveSettlement` from `RunFeedback`. Income/XP is a transaction at cleanup start; harvesting growth is a separate0.66-second event; completion waits for the source2/4-second timer. Reset temporary effects between the credit and growth, and sample the growth stat at its actual callback time. This prevents a temporary bonus from incorrectly growing permanent harvesting.

Explain why standalone `Tick` consumers can fast-forward these stages while native gameplay must advance them with the actual cleanup clock. The scene-free adapter preserves simple rule tests; lifecycle tests and the native fixture verify the ordering that those atomic tests cannot show. The full-run comparator now uses a receipt captured at the transaction boundary, so moving income earlier does not make a post-credit timer snapshot falsely look unpaid.

This is Codex-assisted implementation and investigation. Reproduce a positive/negative payout, pause the cleanup clock and trace one test before claiming personal debugging ownership.

## Rehearsal: landing animation and projectile coordinates

This is an AI-assisted project investigation, not a confirmed independently performed debugging story. The user observed behavior and supplied the reference; Codex investigated and implemented these corrections. Reproduce and explain the following before presenting personal ownership.

1. Open `CombatGeometry.PlayerBodyStart` and explain why the logical body is .24 units above the raw origin. Follow the camera's conversion back to that raw origin.
2. Trace `ArenaGame`'s combat step into `AdvanceArrivalPresentation`. Explain why an animation may re-position a view but must not tick a weapon cooldown a second time.
3. Evaluate `ArrivalWeaponMotion.Offset(.45f)`: the original weapon parent and the registered body share that translation. Explain how the parent's visual rotation differs from the projectile's local-aim velocity in the supplied implementation.
4. Follow `WeaponReference.MuzzlePosition` into the numeric profile. Pistol's authored neutral muzzle is .56 units forward. The gameplay position must remain stable if Scrapshift's own illustration changes size; the original artwork is not needed to express that rule.
5. Inspect `WeaponMuzzleParityTests`, the native arrival report and the full-run report separately. Explain what a source-derived number, a native Update check and an invulnerable20-wave integration run each establish.

Tradeoff: retaining separate logical, origin and presentation coordinates adds explicit conversions, but prevents illustration changes or accessibility animation settings from moving collision/projectile rules. Limits remain: matching these transforms does not reproduce every detail of Godot's physics solver or prove human survival balance.

Fixture lesson: the first arrival check selected the valid melee default when it needed a ranged weapon; a later screenshot request was overwritten in the same frame. Those setup/capture corrections are documented separately from production landing and muzzle fixes. Tests should expose the intended behavior with explicit setup, and evidence capture should wait until the normal Update has posed the staged state.

## Latest native checkpoint — R14, October 17

R14 is the latest unpacked build (`Builds/GameplayParity20261017R14/LaunchGame.cmd`), zero build errors/warnings. Only the opt-in campaign driver changed from R13: it now uses the normal60Hz combat clock. Its normal-speed one-wave preflight Passed, exit0, reached Shop after20.0165 simulated seconds. Both fresh captures were inspected. It did not run a full campaign.

Carried input-matched evidence:172 focused regressions passed; R13 support43/43, economy and virtual navigation17 snapshots passed; R12 enemy53/eight slash probes and combat passed. Requested basic Xbox controls were confirmed by the user and logged separately. Unlock-pool timing and support projectile behavior fixes remain intact. No whole suite or full20-wave comparison ran. See [latest evidence and attribution](native-gameplay-r14-2026-10-17.md). Earlier R13 notes saying the campaign driver still needs a cadence correction are superseded by R14.

## Latest native checkpoint — R13, October 17

Fixed active-run unlock-pool refresh and support projectile animation/identity/offset/current-age presentation. R12 selected regressions passed172/172 (zero failures/skips); R13 only changes two native fixture files, with identical production/test inputs. R13 build succeeded with zero errors/warnings. Actual R13 native economy Passed, support43/43 Passed, virtual controller navigation Passed17 snapshots, all process exits0. R12 combat and enemy53/eight-slash-probe passes carry unchanged relevant source. The user accepted requested basic physical Xbox controls; B-not-opening-wave-pause is closed as reference behavior. No new broad hardware support claim.

The whole suite and full campaign have not run. Before a new campaign comparison, its older variable-step fixture path needs adaptation/checking against current60Hz normal gameplay. See [current native checkpoint](native-gameplay-r13-2026-10-17.md) for manifests, preserved initial failures, reviewed screenshots and limits. Older pending statements below are historical checkpoints. Codex performed implementation/testing; the user supplied direction/reference and actual basic Xbox feedback.

## October 17 native continuation — source fixes awaiting current build

The accepted B-button behavior is documented separately. New source tracing found a real unlock-pool timing difference and support-projectile frame/registration differences; fixes are integrated for the next review build. A separate combat fixture needed 60 Hz stepping rather than a single .1-second jump. R11's crate/reward/shop fixture passed, but its older support passes do not validate the new corrections. Focused and native validation is pending; the whole suite and full campaign remain held. See [active ownership and evidence](native-remaining-checks-2026-10-17.md).

Interview distinction: a fixture failure can expose a stale sampling assumption, while source tracing can separately establish a production bug. Codex authored the investigation/fixes; the user supplied reference/direction and confirmed requested basic Xbox controls. Do not claim a complete reference replica or an independently user-authored debugging story.

## Controller clarification resolved — October 17

The user clarified that the reported issue was B doing nothing during an active wave, and accepted the explanation. This matches the supplied original: `project.godot:2536/2539` maps B to ui_cancel; `2585/2588` maps Start to ui_pause; `main.gd:417-435` opens pause from ui_pause release, and `ui/menus/ingame/pause_menu.gd:32-53` handles cancel while paused. Scrapshift's `CombatInput.cs:96-97` and `ArenaGame.cs:170` use the same distinction. No gameplay change is required for this report.

The requested basic physical Xbox checks are accepted from the user's confirmation and recorded movement/aim/menu/pause actions. This is scoped acceptance, not verification of every shortcut/device combination. Native gameplay follow-up continues before the full run; see the current dated native check reports. Earlier pending-menu statements below are superseded.

## Physical Xbox follow-up — October 17, superseding the idle snapshot

The user first answered that all requested controls worked, then reported that B returns but a menu could not be opened (exact menu/button clarification pending). Do not mark overall hardware acceptance complete. Refreshed observer evidence contains218 samples,144 significant controller-state samples and206 XInput-owned samples; both sticks span -1 to1. A/B, D-pad down/right and Menu/Start events are present. The log records Playing→Paused three times on Start release and Paused→Playing twice on B release, plus controller-correlated movement/aim changes. These observations support the performed actions but do not dismiss the user's latest menu problem. Other shortcuts and unexercised directional buttons are not newly accepted. Full run remains held. See the refreshed `physical-controller/analysis.json` and pending clarification.

## October 17 latest rehearsal — trustworthy native evidence (R11)

R11 is now built. R10 virtual native navigation passed (17 snapshots), weapons211 and enemies53 passed; R11 support39 passed in both hidden and visible executions. Only the support fixture changed from R10 to R11; carried results are explicitly tied to manifests. The earlier524 focused tests are a separate source checkpoint, not a newly rerun whole suite. Physical Xbox actions and full-run comparison remain pending. This supersedes the earlier current-build/support/repaired-reference-menu pending statements below; original keyboard defaults remain broken, and repaired-copy movement remains unverified.

Explain the difference between an implementation bug and an incorrect test expectation: source permits both independent Cat guns to target the same direction, while Scapegoat's pre-increment check delays rising until the following tick. Explain why a hidden player can run logic without generating useful UI screenshots. Show fresh Cat shadow captures, then distinguish virtual Gamepad navigation from actual Xbox input. Codex performed these investigations and fixture repairs; the user supplied direction/reference and earlier hands-on feedback. Reproduce and trace the example before claiming personal debugging experience. See [native checkpoint](native-gameplay-r11-2026-10-17.md).

## October 17 rehearsal — native evidence changes an implementation

Problem: passing rule fixtures still hid a contact-model difference. Codex compared actual Godot projectile scenes at60Hz and found a reachable speed-cap grazing miss; Scrapshift sweeps had counted it as a hit. The correction uses fixed combat cadence and sampled current projectile/melee/beam geometry, with ordinary spawn overlaps and delayed activation. Keep recoil-completion audio independent of contact sampling. Compare ordinary30-material startup with explicit neutral fixtures; explain why DebugService naming alone cannot justify ignoring its unconditional source assignment. The user reported keyboard failure; Codex traced35 blank key events and prepared an isolated copy. These are AI-assisted examples, not independently user-authored debugging stories. Reproduce a fixture and explain its limits before presenting personal experience. Final selected validation: **524/524 passed**, zero failures/skips, Unity exit zero; all516 recorded inputs still match the isolated clone. Evidence: `gameplay-parity-final-checks-2026-10-17.md` and `artifacts/gameplay-parity-final-checks-2026-10-17/focused-summary.json`. No whole suite or full campaign ran. R9 is intermediate and predates final discrete-contact/starting-currency corrections. A current build, production support captures, physical Xbox verification and repaired-reference keyboard verification remain pending. Computer Use stopped when the user pressed physical Escape; no further UI input was sent.

## October 17 rehearsal — event timing and fixture lifecycle

Explain why in-flight shots retain burn data while melee reads refreshed stats. Trace four Sausages adding four base burn damage before Wisdom rounding (12→13), the fixture startup needed to register extended enemies, and Dagger double-precision chance boundaries. Read `Runtime/ItemLifecycle.cs`, `StarterWeaponEffects.cs`, `ReferenceCombat.cs`, and the weapon/enemy integration tests. Discuss explicit empty snapshots and per-projectile persistent overlap entries. Codex inspected, implemented and tested these examples; the user should reproduce and explain them before presenting a personal debugging story. See `gameplay-parity-systems-2026-10-17.md`. Evidence: `artifacts/gameplay-parity-systems-2026-10-17/focused-summary.json`, both XML/log attempts, input manifest and catalog comparison. The same selected checks passed 434/434 on attempt 2, zero failures/skips, Unity exit zero. All 508 recorded input hashes still match the isolated clone after execution. No whole suite, new build, full campaign or physical-controller retest was run. R8 remains the last delivered gameplay build and predates these changes.

## October 17 rehearsal: preserving event-time state

Problem: recomputing burn at impact let a Wisdom tick change an already-fired shot. Codex traced source HitboxArgs and added explicit fire-time data, including an empty payload; melee stays live because its source stats refresh. Explain why projectile and melee lifetimes differ, then trace a Wisdom regression and the root damage pipeline. The user has not confirmed personally reproducing this example. Focused execution pending; see `gameplay-parity-systems-2026-10-17.md`.

## Active systems continuation — October 15, 2026

Codex is continuing parallel source audits and fixes under the user's instruction to finish remaining differences before full tests. This pass traces callback timing and ownership: per-weapon Ghost counters, direct currency versus physical pickups, explosion replacement versus extra damage, wave-end versus immediate achievement checks, and pet/enemy targeting geometry. Implementation and focused checks are pending for this new batch; the earlier 213-case result applies only to the preceding source snapshot. Keep Codex's implementation/testing separate from the user's requirements and physical feedback. See [systems audit](gameplay-parity-systems-audit-2026-10-15.md).

## Latest checkpoint: source fixes and focused verification (October 15, 2026)

Codex integrated the parallel follow-up gameplay fixes. The focused regression selection passed 213/213 with zero failed/skipped after correcting source-derived fixture assumptions and an EditMode cleanup issue. It is not the whole suite or a full-run comparison; those remain deferred under the user's latest testing-order instruction. R8 remains the last delivered build.

Interview note: matching catalog data did not ensure runtime parity. Further tracing exposed a missing wave/horde material-drop gate, different integer rounding stages, burn callback ordering, uncapped stat scaling under health caps, and raw-origin/body-center pickup adapters. Codex authored the implementation and automated checks; the user supplied direction, the reference and physical controller/audio feedback. Do not claim the user wrote these fixes, that a new human full run was accepted, or that focused checks prove every difference is gone. See [follow-up evidence](gameplay-parity-next-audit-2026-10-15.md).

## Current follow-up: audit and fixes in progress (October 15, 2026)

The user directed Codex to finish remaining gameplay fixes before another full test suite or full-run comparison. Codex's parallel source audit identified additional regeneration, Max HP, loot-probability/spread, damage-order, charge-cadence and item-trigger differences. Implementation and focused regression cases are in progress; they have not been validated by a new combined run. R8's earlier results apply to R8 only.

Interview attribution: the user supplied requirements, reference project and physical controller/audio feedback. Codex performed the source comparison and code/test changes. Do not present source inspection or scripted campaigns as the user's human playtesting or claim exact parity is proven. See [current audit](gameplay-parity-next-audit-2026-10-15.md).

# Interview preparation

## Latest checkpoint: R8 gameplay corrections

R8 passed620/620 integrated tests (zero failed/skipped),205/205 native weapon/tier checks and53/53 native enemy/state checks; its Windows build succeeded with zero errors/warnings. R7's prior27/27 support checks cover the preceding support pass. The fresh R8 normal-timing campaign passed/Won after20 timer ends and19shops, with31purchases; every debit and recorded stack sum reconciled. It used scripted movement, supplied equipment and invulnerability, so it does not establish ordinary human survival balance. Its original-game comparison is documented separately. See gameplay-parity-continuation-2026-10-15.md and full-run-r8-comparison-2026-10-15.md.

Further useful explanations: an animated beam needs its scene rectangle, offsets, captured range and interpolation curve to agree with its rendered thickness; enemy healing depends on body shape and collision mask rather than artwork origins; Streamer's structure link counts eligible registered effects, not Wrench deployments. The Greek Fire hypothesis was rejected by tracing its damage-event argument. Codex authored these fixes/checks under the user's direction; the user physically confirmed R6's sound repair. No independent user implementation or human full-run acceptance is implied.

## Latest continuation: tracing resources into actual behavior

The user requested further parallel parity fixes after physically confirming the R6 movement sound was gone. Codex divided the audit into support, combat and economy/progression tracks, then integrated enemy fixes and regression checks. See [continuation evidence](gameplay-parity-continuation-2026-10-15.md).

A useful interview explanation is: "The project encountered gameplay differences even though its catalog values matched. We traced the reference resource, callback and runtime consumer together. For example, an enemy healer script contained a player-healing branch, but its scene collision mask excluded the player. Copying the branch without its scene eligibility had added behavior that the original could not execute. Another issue came from truncating fractional Harvesting before growth for characters with reduced stat gains."

Be ready to explain:

- Why a scene collision mask can make otherwise valid script code unreachable.
- Why ceiling after a fractional character multiplier differs from truncating first; trace Knight's base Harvesting 1 -> effective 0.2 -> base growth 1 -> effective 0.4.
- Why a burn without a weapon hitbox must not consume the shared life-steal cooldown, and why the source resets old knockback on that damage event.
- How Lootworm caches an object rather than a position, when spawn/removal events invalidate it, and why an empty-world sentinel avoids polling or stale targets.
- Why first inspecting REPLACE/boolean effect semantics prevented an incorrect change to overlapping Doc Moth life steal.

R7 validation passed 594/594 integrated tests and 27/27 built-player support checks; the build succeeded with zero errors/warnings. The initial 592/594 result exposed two malformed synthetic effect fixtures, repaired by initializing their empty-string keys. Both reports are preserved. The catalog audit passed 4,980 checked fields; those resource checks do not by themselves prove special-effect execution. See the continuation report for frozen input/build hashes and remaining geometry comparisons. The user supplied direction and physical audio feedback; these implementation and automated checks were performed by Codex. Do not present this as independently handwritten debugging or a completed human full-run playtest. Reproduce and trace an example before using a first-person interview account.

Use these notes to understand the project. First-person answers must match the contribution record and your actual hands-on work.

## Current parity example — October 15, 2026

### Follow-up audio defect: import settings changed quiet recordings

The user physically tried R5 and reported that the movement sound still resembled button switching. The saved feedback interval contains 129 Playing samples, zero combat menu-focus cue requests and 36 footsteps (`artifacts/gameplay-parity-native-2026-10-15/footstep-audio-audit/r5-user-feedback-summary.json`, inspected). These counters identify the dispatched cue to investigate; they are not listening evidence or a reason to dismiss the user's description.

Codex compared the six supplied stereo Step recordings with their actual contextual WAVs. Every processed mono WAV peaked near 0.92, while raw stereo source peaks ranged approximately 0.129–0.486. The source importers combined forced mono with normalization, boosting quiet footsteps before contextual processing. The fix preserves stereo and disables normalization for Step, makes the exception persistent in the audio builder, and rebakes only those six contextual edits. Hash inspection recorded six changed and **266 unchanged** contextual WAVs; `CyberSoundLibrary.asset`, the 0.18 bank gain, 0.1 pitch variation, corrected cadence and music were preserved. See `controller-movement-audio-2026-10-15.md` for measurements and scope.

The inspected isolated-project `artifacts/footstep-audio-final.xml` passed **23/23 focused audio, contextual-processing and footstep timing checks**, zero failures/skips, in **5.0103641 seconds**. Its suite describes 549 discoverable cases but executes only 23; it is **not a 549-test full-suite result**. The 541-case result below is the historical full runtime checkpoint before this audio-only change. R6 built with zero errors/warnings. The user physically retested and confirmed "the sound is gone". This closes the reported movement sound symptom; it is not full gameplay acceptance.

Interview rehearsal: trace source clip -> AudioImporter settings -> contextual bake -> bank mapping -> runtime cue. Explain why preserving a clip identity does not guarantee preserving its energy, why a persistent importer policy matters, and why unchanged neighboring asset hashes provide useful scope evidence. The user reported the unresolved sound; Codex measured the pipeline and authored the fix and tests. Reproduce the measurements before presenting the investigation as personal engineering work.

The game has grown well beyond the small prototype described in some historical answers below. For a current testing discussion, use the parallel gameplay parity pass: the user requested exact behavior against a supplied read-only Brotato project; Codex divided source audits among agents and integrated their changes in one isolated Unity validation copy. The checkpoint before the Input System migration passed 503/503 EditMode tests, with the catalog audit matching 4,980/4,980 fields. The migration checkpoint passed 519/519 tests; the subsequent shortcut full suite passed 526/526, followed by 46/46 affected cases for the final lock correction. The subsequent physical Xbox check recorded sticks, buttons and player movement, but exposed a highlight/confirmation mismatch and rapid UI-like movement audio. The follow-up passed 541/541 tests and the visible native virtual-input fixture; physical acceptance of R5 remains pending. Native input also found a settings navigation bug that the earlier suite missed. Controlled 20-wave source and Unity campaigns exposed material spawn/collection differences and prompted additional fixes. See `gameplay-parity-controls-stacks-2026-10-15.md` and `full-run-comparison-2026-10-15.md` for exact checkpoints. These are assistant-run checks. Complete behavior equivalence and a normal human full-run playthrough remain open.

### Focus repair validation — R5

The integrated follow-up passed **541/541 EditMode cases**, zero failures/skips, in **69.4470763 seconds** (`artifacts/gameplay-parity-native-2026-10-15/controller-focus-final.xml`, inspected). The native **visible** virtual-Gamepad fixture then passed the actual player `Update`/`OnGUI` path: A entered character, weapon and difficulty selection without preliminary directional input, started Rookie with one weapon, moved the player, paused with Start and resumed with A. During its movement interval it recorded **zero menu cues and three footsteps**. `focus-native-visible/navigation-check.json` and its character/paused PNGs record the result; the integrating agent visually inspected the highlighted character and white Resume control. These are rendered integration checks using injected virtual input, not another physical Xbox test or an audio listening test.

The earlier hidden-window fixture failed because `OnGUI` never registered the controls. Its failed report remains under `focus-native`; it is not counted as a pass. This is a useful test-design tradeoff: a regression involving actual immediate-mode UI needs that UI to execute visibly, whereas pure focus helpers can run in EditMode.

The R5 build at `Builds/GameplayParity20261015R5` replaces R4: zero build errors/warnings, 401,081,415 bytes, runtime SHA-256 `E612D8992DEC50CCF16AFBCEDA2EEA10A7EE201B6C45F5654D183AE748CB94AB` (inspected). After the full suite, an obsolete lookup in the optional diagnostic helper was replaced to remove its warning; ordinary gameplay code was unchanged by that small follow-up. The integrating agent closed R4 and launched isolated R5 for the user. **Physical acceptance of the repaired focus, aiming/navigation and resulting audio remains pending.**

### Physical follow-up: a correct device reading can still activate the wrong control

The user's R4 test went beyond detecting the device: `artifacts/gameplay-parity-native-2026-10-15/controller-r4-retry/input.jsonl` records physical Xbox stick/button events and changes in player position. The user reported that A returned from character selection while the character appeared highlighted, and that moving the stick first changed the action. They also heard rapid sounds resembling button switching during combat movement. These observations led to the R5 repair and validation above; they do not establish that every controller path works.

Codex traced the menu problem to UI lifecycle and focus ownership: an outgoing screen could retain registrations and Back could become the active default while the selected portrait still looked highlighted. The controls repair addresses stale registrations, preferred selected/default controls, an unambiguous actionable highlight, and mouse/controller handover. The opt-in native GUI fixture exercised confirmation with A without a preliminary navigation movement. It uses virtual device input in the real rendered UI and remains labeled separately from the user's physical test. The integrated suite and native fixture passed as recorded above; renewed hardware acceptance remains pending.

For the audio complaint, source inspection found that Scrapshift emitted a footstep immediately, then every fixed 0.28 seconds; the supplied reference's movement animation first contacts at 0.1 seconds, then every 0.3 animation seconds, scaled with movement speed. Codex corrected that timing while preserving the approved Step recordings and mix. Cue counters are intended to distinguish focus cues from footsteps during combat. The reported sound has not yet been heard by Codex or accepted by the user after the repair; timing evidence alone cannot identify or resolve an audible complaint. See `controller-movement-audio-2026-10-15.md`.

The user also clarified that the first character should start with one weapon. A fresh audit followed the reference's real character -> weapon -> difficulty callbacks and checked all 50 imported and serialized profiles: 42 start with one selection, six add an explicit weapon grant, and Bull/Beast Master start weaponless. All six grant identities and tiers match. See `starting-weapons-recheck-2026-10-15.md`; no universal second weapon was restored.

Rehearse the distinction between **selected content**, **current actionable focus**, and **last input device**. Explain why tests of directional navigation can miss the simpler sequence “open a screen, press A immediately.” Attribute the physical observations and clarification to the user, and the source investigation, code and assistant-run checks to Codex. Reproduce and explain the example before presenting it as personal debugging work.

Rehearse by tracing `WeaponReference.DamageAfterHit` through the projectile hit path and its test. Explain why integer storage and calculation order matter, then reproduce the test before using it as a personal debugging story. The old “24 tests,” six-weapon, small-item-pool and missing-weapon-set statements below describe earlier versions; verify current facts before using them in an interview.

## Controller bug: checking the configuration Unity actually loads

The user physically tried an Xbox controller and reported that A/B buttons responded while sticks and D-pad did not. Earlier tests exercised pure helpers with supplied vectors, so they could verify movement normalization and menu navigation without proving Unity had registered the actual input axes. Investigation found malformed legacy `ProjectSettings/InputManager.asset` entries: text resembling axis definitions sat outside Unity's deserialized `m_Axes` array. The buttons working did not establish that analog bindings worked.

Codex added six cases to `Assets/Scrapshift/Tests/Editor/ControlsParityTests.cs`, using `AssetDatabase.LoadAllAssetsAtPath` and `SerializedObject` to inspect the real settings object. They check that each named movement, aim and D-pad axis occurs exactly once with the expected joystick type, hardware axis, inversion and settings. All six now pass after restoring canonical `InputManager.asset` formatting. This addresses the actual configuration boundary that pure arithmetic helpers missed; legacy-axis validity alone does not validate the newer controller path.

The user then chose Unity's newer Input System for controller support, preserving keyboard and mouse behavior. Codex implemented the migration with `com.unity.inputsystem` 1.20.1 and Unity's Both input handling setting. `CombatInput` reads `Gamepad` controls, applies the reference deadzone once to unprocessed stick values, uses actual button press/release state, and handles ordered device events so an unchanged held stick cannot override a later mouse/keyboard event. Removed, disconnected or disabled devices clear controller vectors; keyboard/mouse paths remain available.

Five `InputSystemDeviceTests` cases derive from Unity's `InputTestFixture` and queue real state events into virtual devices. The inspected `input-system-fixture.xml` records **5/5 passed**, zero failures/skips: sticks/D-pad/deadzone, buttons, ordered mouse/gamepad events, aim history/keyboard takeover, and disabled-device cleanup. The final migration suite, `input-system-final.xml`, passed **519/519**, zero failures/skips, in **70.0788143 seconds**, including 19 control cases and these five device cases. This validates device-layout/event/runtime-read integration in a fixture. At that migration checkpoint, physical movement, aiming, navigation, A/B and device switching still required retesting; the later R4 physical feedback is recorded above.

Controller review moved from R2 to `Builds/GameplayParity20261015R3` on `codex/gameplay-parity`. R3 built with zero errors/warnings and was launched with an isolated profile; the modern backend detected an Xbox controller, but no physical input sequence has yet been recorded. It predates the completed X/Y/shoulder-shortcut source work; R4 now replaces it after the validation described below. Distinguish successful build, device detection and actual control verification.

Codex subsequently completed source-based shortcuts: X theft/lock, Y loot recycling or a default 0.5-second reroll hold with progress and a functional hold option, shoulder/trigger tab switching, and per-axis menu navigation. The full shortcut suite passed **526/526**, zero failures/skips, in **74.2703369 seconds** (`controller-shortcuts-final.xml`). A later shared lock-eligibility correction for Hourglass/Broken Hourglass and positive `disable_item_locking` passed **46/46 affected cases** in **0.8029584 seconds** (`controller-locks-final.xml`). Keep those evidence scopes separate; there is no recorded 529-test full-suite result. R4 built from frozen source with zero errors/warnings and replaces R3. Its isolated launch detected the modern Xbox device. A native mouse/keyboard check showed readable Gameplay options, toggled the hold-reroll setting On -> Off -> On, restored it and returned to title with Escape; `r4-controller-options.png` records the visible setting. At that checkpoint the build was left at title awaiting testing; the later physical R4 feedback is recorded above. Device detection and a mouse click alone did not establish controller behavior.

The primary campaign evidence now consists of completed normal-time runs in both games: 20 timer ends and 19 shops each, with 30 reference purchases and 45 Unity purchases. See `full-run-comparison-2026-10-15.md` for unequal progression, Lootworm acquisition evidence and frozen runtime hashes. Both runs were invulnerable and scripted. Their completion demonstrates transition integration; it does not establish equal survival balance or validate the later controller binary. The earlier accelerated reference run with 38 purchases remains historical evidence.

Rehearse these questions: What did the helper tests establish? Why can valid-looking YAML fail to register the intended serialized property? How does inspecting `SerializedObject` improve the test? What does a simulated device state establish, and what remains for hardware testing? Trace the named-axis regression and then the newer device-state path when available; reproduce them before presenting the investigation as personal engineering work.

Attribution: the user performed the physical controller check, reported the failure and selected the migration direction. Codex diagnosed the configuration problem, authored the code and tests, and is performing automated validation. A truthful interview explanation is: “A physical controller check on the project exposed a configuration gap that helper tests missed. I can explain the difference between input logic, Unity's loaded settings and hardware verification.” This describes understanding; it does not claim the user independently wrote or executed the assistant's investigation.

## Latest art/animation example

For a later testing example, use `gameplay-parity-controls-stacks-2026-10-15.md`, `full-run-comparison-2026-10-15.md` and the native observations. A passing catalog/suite did not reveal that arrows skipped every settings row. Native input exposed the defect; the fix changed navigation rectangles while preserving the drawn controls, with a source-derived regression. The full-run comparison also exposed progression differences and led to inspecting material spawn/collection order. Explain why a completed accelerated campaign does not establish equal balance, RNG or controller support. Codex performed this investigation; reproduce the example before presenting it as your own debugging work.

The user identified that presentation lacked character animation, gun firing motion and varied material shapes visible in the screenshots. The project now separates motion state, weapon aiming/firing, and pickup motion. That separation lets pause stop gameplay animation while keeping menu behavior independent, and lets reduced motion suppress bounce without removing the pickup itself.

For a concrete debugging explanation, study ART-001: a generated atlas did not follow its promised grid exactly, so one weapon's slice included a neighboring fragment. The fix changed the import boundary, not gameplay coordinates, and was checked in the actual player. QA-004 is a useful testing discussion: an inherited user preference suppressed the exact effect the fixture was meant to observe. The fix isolated fixture state without overwriting the user's preference.

For AssetScope: point to serialized frame arrays on `ScavengerAnimated.prefab` and sprite references on `ReferenceCatalog.asset`. Six texture atlases support many sprite subassets; testing that the scene has no historical reference-PNG dependency catches incomplete migration. This is a concrete creator-workflow example rather than a claim about Unity's internal tooling.

Explain that Codex generated the art/code and ran these checks under your direction. Reproduce and understand the example before describing it as your own debugging experience.

The user's stride feedback is an especially useful example of why a green test suite is insufficient: six observed frame indices do not establish a convincing gait. ART-002/003 distinguish a frame-slot count from pose quality and movement direction. The battery correction likewise came from actual interpretation of the visual symbol, not a failing arithmetic test. Discuss how these observations changed the assets and acceptance criteria.

## Why build this?

"I wanted a small Unity game that I could finish and use alongside my AssetScope Editor tool. Arena survival gave me a clear gameplay loop and real prefab, material and configuration dependencies. It connects the experience of making a game with the problem of helping creators inspect their projects."

## Why keep it small?

One arena and a bounded content catalog allow attention to responsiveness, readability, testable behavior and a usable build. Adding more enemies or maps before the loop is reliable would dilute that focus.

## How does it relate to the role?

Discuss C# state transitions, Unity assets and serialization, Editor-only package separation, reproducible validation, and how an actual project reveals authoring problems. Do not claim the game implements Unity's internal multi-context authoring or Graph Toolkit.

## What was difficult? How did you fix a bug?

Three actual investigations are in `bugs.md`: an invalid package dependency blocked import; the runtime smoke check falsely treated a screenshot request as visual success; and code inspection identified an aspect-ratio mismatch between camera and HUD. Use the one you have personally rehearsed and understand.

The strongest testing example is QA-002. A report said Passed, but the player log showed screenshot capture failure. A second run produced a black image. The lesson is that a passing process or a file on disk is weaker evidence than the actual requested output. The repair separates gameplay counters from rendered inspection and requires actual screenshot evidence. Initial invalid reports are retained for provenance rather than quietly relabeled as success.

If asked for a personal debugging story before you reproduce it yourself: "The project encountered this failure during development. The automated investigation found X; I can explain the reproduction and fix." Do not say you personally performed steps recorded as assistant-run checks.

## How did you test it?

24 EditMode cases passed: rules, collision geometry, content/AssetScope integration and aspect-ratio handling. The Windows player built with zero reported build warnings/errors. Real title/gameplay/pause rendering and keyboard/mouse controls were inspected using desktop automation. Initial hidden screenshot attempts failed visual validation and are recorded separately. See `verification.md` for the final exact evidence and remaining gaps. Automated coverage does not establish that the game is balanced or fun.

## What would you improve next?

Rebinding/controller support, player feedback-driven balance, performance measurements before pooling or architectural expansion, and broader integration checks against later AssetScope versions.

## Hands-on rehearsal checklist

- Play a full run and describe one concrete balance or readability issue.
- Trace a weapon definition to its effect prefab and shared material.
- Explain how pause and upgrades stop simulation without corrupting a wave.
- Read one real regression test and predict what happens if its fix is removed.
- Make a small tuning change, explain it, test it and commit it.
- Practice a two-minute demonstration: game -> asset workflow -> evidence.

## How would you explain the architecture?

"RunRules owns the progression state without scene references. ArenaGame reads input and coordinates combat and presentation. Enemy and weapon definitions are Unity assets referencing prefabs and shared materials. The Editor assembly contains content generation and AssetScope integration; the runtime assembly does not reference UnityEditor."

Point to `architecture.md`. Explain the limitations too: the coordinator is larger than ideal and would be split before expanding the roster.

## How do you prevent bugs around pause and upgrades?

Advance simulation only in Playing state. A level-up and a wave-clear intermission use the same upgrade screen but have different continuation behavior. Tests verify that an in-wave upgrade preserves remaining time and only a wave intermission advances the wave. Invalid or duplicate choice events cannot apply another upgrade after the state returns to Playing.

## What combat problem did you anticipate?

Fast projectiles can move from one side of an enemy to the other between frames. Checking only their current position can miss the collision. The game measures distance to the whole movement segment instead. Tests include a target between endpoints, a target beyond the segment, and a stationary segment. This is a preventative design and tested edge case, not a claim of an observed tunneling incident.

## Why ScriptableObjects?

They let you tune enemy and weapon values as assets rather than scattering constants through combat code. Their prefab and material references are also inspectable in AssetScope. The current small game still has some positional weapon assumptions; expanding it should introduce named references and broader configuration validation.

## How did you decide what to cut?

The original brief emphasized small, polished, Unity experience and AssetScope compatibility. Multiplayer, a large roster, a shop economy and procedural worlds would add work without directly proving that goal. Three recognizable weapons, three enemy types and a complete short loop are enough to show the authoring/testing workflow.

## How would you optimize it?

Measure first with Unity Profiler on a repeatable scenario. Investigate Instantiate/Destroy and nested collision searches if they dominate a frame. Pool transient objects or introduce spatial partitioning only with evidence. The project currently bounds entity counts; it has not established a production performance benchmark. Do not quote an invented FPS improvement.

## How did references influence the game?

Use `art-references.md` and `combat-references.md`. Discuss movement-centered combat and readable choices from Brotato, compact mechanical silhouettes from Into the Breach, and abstract weapon effects from Nova Drift. Public community mod source informed questions about hit processing, bounded history and effective stats. No claim is made to possess the complete official Brotato source.

## How does this demonstrate attention to users?

The controls and objective are visible before play. Combat stops while reading upgrade choices. A health bar and clear timer communicate status. There are sound and reduced-motion controls. The camera and HUD share an aspect ratio so resizing cannot hide reachable parts of the arena. These are design decisions; usability-study success is not claimed.

## How did you use coding tools?

Answer honestly if asked. The user's direction covered concept, scope, references and documentation; implementation and initial automated investigation in this session used a coding assistant. Explain the code you understand, the outputs you reviewed and the hands-on changes you actually made. Do not equate directing generation with independently typing every line. The role explicitly welcomes AI tools, so clear verification and ownership boundaries are useful discussion points.

## Tell me about collaboration or user feedback

This is currently a directed portfolio project, not evidence of a professional team or user study. Use another verified experience for a teamwork question unless you have a real collaboration example here. Record the first playtest feedback in `contributions.md`, then connect the observed issue to a concrete change and verification.

## Two-minute demo order

1. Start a shift and explain movement, auto-fire and scrap collection.
2. Pause; show the options and explain why simulation is stopped.
3. In Unity, inspect a weapon definition and trace its effect prefab/material in AssetScope.
4. Show one rule test and the real bug log, explaining what each proves and does not prove.
5. End with one specific improvement based on your own playtest.

## How did you respond to negative feedback?

The user explicitly said the original art, UI and upgrade presentation were poor and that the combat lacked effects. The revision used the supplied local reference to identify specific missing signals: character silhouettes, hit confirmation, threat telegraphs and readable stat choices. It then replaced the presentation, expanded feedback, added randomized tiers/rerolls, and rechecked the build and actual screens. This demonstrates iteration from concrete critique; user approval of the redesign is still pending.

The latest suite has 27 tests. DEV-004 is an additional real debugging example: native Unity object construction during deserialization, caught by scene tests despite a nominally successful build. Explain both the lifecycle fix and the stricter build gate. See journal and verification for the newest evidence; earlier examples above describe the first baseline.

## How did you use your Editor tool in a real project?

The game now embeds AssetScope 0.3.0. It scans 72 actual game assets and 1,202 reference usages with saved-scene/property analysis enabled. A regression test locates the Fire_01 audio clip reference in the sound bank, navigates to its source and verifies the selected bank. This links creator tooling to an actual authoring task: find which sound-bank entry uses a clip and go to the owning asset to change it. It is stronger evidence than only showing synthetic demo assets, but remains a small project, not a large-project benchmark.

## Why change the audio architecture?

The user reported that the original sound was poor. Four sine tones did not distinguish enough gameplay events. The revision separates authored cue data from playback, uses multiple original variants, and limits concurrent sounds while reserving voices for important feedback. Explain the distinction between numerical/sample-playback verification and subjective listening; the latter still needs personal evaluation. AUDIO-006 records the actual Unity importer API failure and fix.

## Reference rebuild — current interview examples

These replace the earlier five-wave/three-weapon scope descriptions above.

**Why did the project grow?** The user specifically rejected the simple enemy and upgrade model and asked for a source-informed reconstruction. The response was to analyze wave resources and behavior scripts first, commit the study, then implement the encounter schedule and separate XP/reward/shop states. The bounded catalog keeps the expansion explainable: six weapon families, 24 items, eight stat upgrades, base difficulty only.

**What was a difficult bug?** REF-001: the initial scheduler's shared random sequence depended on frame size because it processed all missed repeats of each group together. Processing chronological seconds fixed the ordering. The regression compares all 20 waves at 60 Hz against a single large step with the same seed. Explain why seeded randomness alone does not ensure determinism.

**How did you test beyond unit tests?** State tests verify atomic purchases, six-slot limits, duplicate combinations, paid rerolls and preserved locks. Actual player captures caught clipped upgrade titles and white birth warnings that the rules tests could not catch. A hidden-window capture failed and was explicitly rejected. The rendered fixture grants health/equipment for coverage and is not evidence of a fair normal run.

**How did you avoid misleading portfolio claims?** Record the supplied reference paths/hashes, distinguish third-party sprites/audio from original work, and explain that Codex implemented/tested the revision under Shaoyu's direction. Describe personal code understanding and hands-on changes only after actually doing them. No fabricated production incident, performance improvement or user approval.

**How does AssetScope help here?** Inspect ReferenceCatalog, follow an enemy sprite or weapon audio reference, and navigate back to the serialized record that uses it. This demonstrates the completed tool against the real game catalog. Use the latest scan totals in verification rather than the older 72-asset baseline.


### Enemy and spawn iteration

**How did you use a reference without guessing?** The team compared the supplied Godot scheduling/placement code and attack animation timings with the Unity implementation, then wrote an evidence-to-change document before editing. The user retained twenty waves and asked for fewer items and a closer UI. Assistant-generated implementation and automated checks are identified in contributions; this is not a claim of personally handwritten code.

**A concrete bug example:** a safety retry moved a tightly grouped enemy anywhere in the arena. That met a distance check but violated the formation's purpose. The fix keeps the group's square or edge constraint during every retry and chooses the safest candidate within it when full clearance is impossible. Tests cover both ordinary and impossible requests; visual review covers what those placements look like.

**What tradeoff did you make?** Explicit attack states and locked direction make a windup readable and dodgeable. Shooter aim lock, a short recovery, and visible direction lines are deliberate adaptations for the compact arena, not claims of exact commercial-game parity. Twenty schedules remain, while eight offered items keep the shop easier to understand.


## Source-parity revision — current answers supersede earlier adaptations

**How did you decide what to change?** Compare the supplied source's scene nodes and runtime methods with the Unity behavior, write a discrepancy table, then implement and test the differences. A visual resemblance or passing tests against our previous implementation did not establish parity. See `gameplay-parity-audit.md`.

**What was the hardest bug to reason about?** A flat scene importer silently merged root and phase-two boss properties. The fix made attack profiles explicit per node/state. Explain how a data-shape error became a gameplay error, and why a compiler or missing-reference scan would not detect it.

**What did tests catch or prove?** Exact XP and rarity boundaries, charge direction commitment and cooldown, paid non-repeating upgrade choices, separate spawn lane order, the 119/120 backlog boundary, and locked-price persistence. A separate accelerated Windows campaign verifies all twenty intermissions and the boss phase transition. The fixture is invulnerable and automatically shops; it does not prove fairness or fun.

**What remains a tradeoff?** Keeping original art, a compact arena and a reduced catalog makes a manageable portfolio project, but also means this is not an exact Brotato clone. Character passives, additional weapon sets, all stats/items and endless mode are absent; Gun, Precise and Primitive were added in the next pass. The source-based rules are documented separately from our adaptations.

**Who did what?** Shaoyu requested the direction, supplied the reference, retained original art, requested incremental commits/documentation and opened the project in Unity. Codex performed source comparison, implementation and automated validation in this pass. Before interviewing, personally trace these code paths and play the project; do not claim these were personally handwritten changes or production incidents.

## Weapon-set discussion example

**Why derive the bonus instead of storing it?** The loadout has at most six slots, so recalculating its contribution keeps one source of truth and avoids stale bonuses after buying, combining or recycling. Permanent item/level stats remain separate. A higher-tier weapon still counts once; two duplicates count twice. The active threshold is a total, not a sum of all earlier thresholds.

**What did verification teach you?** One new test failed because advancing the wave also earned a Harvesting level and +1 HP. The test was corrected to isolate the combine delta. The actual rendered fixture then showed two spears giving +3 Max HP and combining them lowering HP from 30 to 27. Explain the difference between a faulty test assumption and a demonstrated game defect.

**How do you make a rule understandable?** Show weapon membership, equipped count, current effect and all five thresholds together. Inactive sets explicitly say they need two weapons. Unit tests verify the arithmetic; rendered inspection checks readability. These are Codex-authored changes directed by the user; personally trace and play them before using this example in an interview.

## Loot-loop example — state ordering and honest evidence

**How did you prioritize missing features?** The comparison found a missing everyday decision: pursue a loot enemy, collect its crate, then keep the item or recycle it. That changes the run more directly than adding many catalog entries. Survival formulas also changed how existing builds behaved, so those were corrected in the same iteration.

**What integration problem did you solve?** A crate collected during end-of-wave cleanup must enter the queue before choosing the next state. Clearing pickups or opening the shop first loses that ordering. Trace `FinishWavePickups`, `Tick`, `OpenRewards` and `ResolveCrate`: collect, resolve crates, resolve levels, then shop. The targeted fixture leaves a crate offscreen to exercise the cleanup path.

**How did you validate it?** 116 rule/integration tests, a real rendered take/recycle fixture and a current twenty-wave accelerated run. Distinguish those from balance: the separate normal-speed smoke died in wave one. Its old Passed label only measured combat/capture coverage; reporting the actual loss is more accurate than claiming a successful run.

**What remains?** Character/starter selection, broader stats/items, weapon-specific behavior and normal-run balancing. The crate pool is intentionally small. These are assistant-authored changes directed by the user, not personally handwritten production incidents; personally trace and play the implementation before discussing it as experience.

## Simplifying animation after user feedback

**Why simplify the art?** The user wanted springy body movement rather than visible limb cycles. Inspecting the reference showed separate body/feet sprites animated with position and scale. One body plus tiny foot instances removes inconsistent generated stride poses, keeps equipment independent and makes tuning straightforward.

**What did visual review catch that tests missed?** The first gallery showed a clipped enemy nose and a fragment on its neighbor. The imported grid was valid data but wrong for those pixels. Per-sprite alpha bounds corrected it; a border test then caught remaining tree contamination in another sprite. Explain both the visual observation and the actual failed regression before the final 120/120 pass.

**What remains subjective?** Tests establish timing, bounds and state behavior; they do not establish that the spring feels right. The user still needs to play it. Codex generated/implemented these changes under the user's direction; describe that contribution accurately.


## Modular appearance example: separation of data and presentation

Problem: a character's eyes and equipment were baked into one image; inventory icons pasted onto it could not cleanly change identity. Decision: use one blank body with data-driven eye, mouth, hair and item layers, following the inspected source's slot/priority model. Three cosmetic looks demonstrate reuse while keeping stats separate. Both runtime and shop share the resolver to avoid inconsistent previews.

Concrete bug example: the old wardrobe used item count as its refresh condition. A same-count replacement would keep stale art. The cache now compares item identity/order and preset identity; a regression replaces a hat without changing count and verifies the displayed sprite. Sprite border tests also caught two actual atlas cuts (126/127 initial passes); measured transparent separators resolved them. A generated enemy draft with a translucent backdrop was rejected after alpha inspection.

Tradeoff: explicit offsets suit this small catalog, while a larger roster may justify standardized authoring canvases and an appearance editor. Cosmetic variants currently have no character-specific passives. Use this as a documented project example; Codex authored the implementation/checks, and candidate explanation or manual validation should be described truthfully.


## Visual scaling and transform ownership

**How was a visual complaint investigated?** Compare actual reference sprite bounds and scene transforms before guessing scale. The first-wave enemy had an extra .72 multiplier, giving roughly half the player's width. A separate presentation policy makes the new player/enemy ratio explicit while leaving collision balance for a deliberate follow-up.

**Why were shadows separate objects?** A child renderer inherits the actor's squash and rotation. A world-space oval can follow the ground position without distorting. An owner component handles activation and deletion; serialized mesh/material dependencies remain visible to AssetScope. A new test first failed for an Editor-created owner's disable lifecycle; enabling Editor lifecycle callbacks fixed that case. Tests and staged screenshots support these implementation claims, not player acceptance or full-run balance. Codex authored this work under the user's direction.


## Diagnosing a unit-system mismatch

A useful example from the live-reference pass: correct-looking individual sprites still produced the wrong game feel because source movement and weapon ranges used 100 pixels per Unity unit, while arena dimensions and image sizes were chosen independently. Comparing the running game with its source revealed the mismatch. Shared world geometry, measured sprite dimensions and a bounded follow camera address traversal, scale and spawn density together. Automated tests check map edges and camera bounds; staged gameplay checks actual rendering with one and six weapons. See live-reference-comparison.md for facts and limits. Codex implemented and verified this work under user direction; reproduce the reasoning before presenting it as personal engineering experience.


## Debugging combat feel through separate contracts

Use combat-feel-comparison.md as the evidence trail. A single enemy radius had been reused for crowd spacing, bullet hits and outgoing contact, despite the reference defining separate shapes. Knockback also crossed an inconsistent conversion boundary (/10 in import, then *3 at runtime). Explicit profiles and force rules made both problems testable. Travel-ordered swept collisions protect piercing damage order regardless of enemy-list order; an actual runtime fixture verifies100 damage to the near target and50 to the far one. Frame-rate comparisons check push at20/30/60/120Hz. Explain why this does not prove human feel or exact Godot physics parity. Codex authored/verified these changes under user direction; rehearse and reproduce the reasoning before making personal implementation claims.


A second concrete debugging story: hidden Unity runs completed simulation but could not capture screenshots. Existing checks counted capture requests and reported Passed. Comparing log failures, file timestamps and old art in a stale screenshot exposed the false rendering claim. Fresh-file/signature checks and a missing/invalid/stale regression now separate gameplay assertions from rendering evidence. The final reruns passed; normal-speed scripted survival still failed and is reported separately.


## Presentation comparison and validation example

Study the stale-scene incident in yard-and-run-presentation.md. A migration marker described work that had happened earlier, but synchronization had replaced the actual scene. The runtime fixture checked animation state and fresh screenshots, yet initially missed the absent canopy. Inspection caught it; the fix checked the real serialized references, guarded migration skipping with those checks and verified the instantiated object at runtime. Discuss the difference between a completed-step marker and evidence that the current output is correct.

The visual comparison also illustrates scope control: source-grounded mounts/timings, original art, a small supported selection catalog, and historical adaptations. The former timer hold during arrival was removed in the October17 gameplay correction; current combat continues during landing. Do not describe this as a full reproduction of Brotato or a completed human playthrough. Shaoyu directed the changes; Codex implemented, debugged and verified them. Reproduce and understand the example before claiming personal debugging ownership.


## Separating presentation data from gameplay offers

The stat display reused the upgrade-offer list as its icon registry. That worked for eight upgradeable stats but left other visible stats blank. A complete presentation lookup now gives every visible primary stat a serialized icon, while upgrade drafting remains a separate concern. Explain how the AssetScope scan and asset-completeness test cover references without claiming safe deletion or runtime completeness.

Weapon recoil supplied a second example: matching the numerical cooldown alone did not match cadence because the reference suspends cooldown during its shooting tween. The fix accounts for animation duration, attack-speed scaling and frame-boundary overshoot, with tests plus rendered six-weapon combat. Codex performed implementation; study and reproduce the reasoning before presenting it as personal debugging work.

## Further debugging evidence: hit audio and mirrored previews

See [player-hit-and-leg-correction.md](player-hit-and-leg-correction.md) for a concrete wrong-asset-mapping diagnosis, a GUI transform-order bug found visually, regression choices, evidence limits and collaborator attribution. Use the documented facts when preparing an answer; do not describe agent-run checks as personally executed tests.

## Data boundaries and verifying the actual display

The four-character pass offers two further examples: replacing cosmetic-only choices with shared gameplay/preview rules, and fixing a recursive Unity serialization schema by separating leaf projectile stats from weapons with effects. A requested ultrawide capture was actually clamped to 1920x1080, so the report records the real dimensions rather than claiming a test that did not occur. See fullscreen-and-starter-characters.md for source mapping, real failures and attribution. Do not present agent-run checks as personally executed work.


## Inspection UI: data filtering, coordinate spaces and evidence quality

See [inspection-and-weapon-selection.md](inspection-and-weapon-selection.md). Useful questions include why eligibility should be filtered before rendering/Random selection, why nested scrolling breaks naive hit testing, how current-value descriptions avoid stale UI, and why a fresh screenshot can still capture the wrong logical state. The document records actual failures and tests rather than inventing a debugging story. Codex performed the implementation/checks; study and reproduce the reasoning before claiming personal execution.

## Catalog and unlock integration discussion

Use full-base-implementation.md, bugs.md and verification.md as evidence. Explain why catalog identity, display alias, availability and runtime behavior are separate. Imported rows alone do not implement event-driven implants: companions, death chains, next-wave effects and progression need runtime hooks. Unlock persistence needs idempotence and recovery; fixtures must not alter the player's save.

Concrete debugging example: visual weapon-edge lines led to alpha-border regression checks, exposing overlapping atlas cells. Shared clipping geometry fixes both world/UI rendering without repainting art. Tests then caught zero-sized sprites caused by Unity Rect serialization; explicit data-transfer fields fixed the schema. Scroll inspection exposed a separate coordinate-space mismatch.

The user supplied direction, screenshots, play feedback and defect reports; Codex performed the recorded implementation/source comparisons/automated checks. Study and reproduce the reasoning before claiming personal execution. Do not claim frame-perfect parity, a human-balanced campaign or 50 bespoke portraits.


## Evidence-backed comparison: final boss attack bug

A visible difference led me to inspect the source animation rather than only compare damage numbers. Our stationary enemy shots were ordinary three-second damage orbs; the reference pillar enables its hitbox for just 0.14 seconds after a warning. I separated presentation lifetime from collision timing, corrected the source diameter/square-scatter semantics, and wrote boundary tests plus a runtime check that verifies health stays unchanged during warning and drops during the active phase. I also followed a speed-reset function into its implementation: its argument was a percentage modifier, not a speed. This prevented a plausible but incorrect port. The source study and our own procedural art are explicitly distinguished in presentation-and-final-bosses.md.

## Further source-reading examples after R7

Streamer exposed another distinction between deployed actors and registered effects: its armor link uses RunData.get_nb_structures (effect entries, Pocket Factory and structure pets), while the structure achievements use only the effect array. Historical correction: Wrench registers a StructureEffect through add_weapon; its effect qualifies both registration counts, while live multiplicity does not. See support-registration-parity-2026-10-20.md. Codex corrected the broader helper and added real-character tests. A Greek Fire hypothesis was rejected because is_burning is a damage-event argument, distinct from the enemy's _is_burning member. Following the caller prevented an incorrect trigger expansion. Animated beam and enemy-healer geometry fixes are being integrated separately from the passed R7 checkpoint; do not claim their new tests passed until the next results are recorded.

Final R8 comparison:14/14 saved consistency checks passed against the frozen original run, with all30original and31R8purchase debits reconciled. Both completed20 timer ends/19shops. Source-based interpretation identifies XP items (Scar/Peacock), Crown growth and enemy-count/loadout changes as concrete confounders. No formula defect was inferred from aggregate differences alone. The end capture shows an unlock modal, so Won is established by trace/report and exit0. This is an example of keeping evidence boundaries clear, not a claim of a user-played run.

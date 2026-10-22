## 2026-10-21 - Merged main Web release verified

Optimization merged into local main8fd3186 after gameplay merge992e4d2. Final nonreview Web build:0errors/warnings/exit0,73,477,798bytes; ZIP73,347,695bytes,CRC/per-fileSHA readback Passed.2629 source inputs and all5 built/tested/packaged files match;464audio/23texture imports restore. Actual release Rookie/Pistol1-weapon start, movement/pause/Resume/Escape, focus pause,1920x1080fullscreen,42%settings afterreload and console[] passed. Current-source build baseline shrinks5.35%; historical older-content ZIP is not that baseline. Prior builds preserved; no push/upload/newfull20/browserhardware/total-RAM claim. See docs/web-release.md.

## 2026-10-21 - Guard Web atlas imports and restore settings

A compression attempt would resize an oversized wearable atlas, so the invariant guard rejected it and restored settings before building. The accepted candidate compresses only two compatible weapon sheets;21 audited textures retain their original import. All23 texture and464 audio records restore with unchanged source hashes. Packed atlas data falls9MiB; no total-RAM claim. LTO enlarged Wasm and has no valid speed-gain proof, so it stays opt-in. See docs/web-build-experiment-2026-10-21.md; final release package acceptance is separate.

## 2026-10-21 - Web crowd fixture loadout corrected

The diagnostic crowd replaced six inventory weapons without rebuilding their scene views. Knife updates repeatedly threw and interrupted physics. Added SyncEquipment in the review-only command; ordinary game transitions already call it. Earlier Web crowd FPS samples are invalid. Fresh normal-exception-settings sample:1293 frames,7.738 ms mean,11 ms p95,minimum200 enemies/six weapons,zero console warnings/errors. Full-stack diagnostic timing is separate. See docs/web-crowd-fixture-2026-10-21.md.

## 2026-10-21 - Native victory diagnostic capture fixed

The manual ReadPixels coroutine returned blank captures outside a drawing frame. ScreenCapture now schedules the capture after rendering. R2 produced five inspected nonblank frames; a fresh staged Won/summary fixture exited 0. Earlier blank attempts remain invalid visual evidence. See docs/crowded-wave-optimization-2026-10-21.md. This does not add a new full campaign or hardware claim.

## October21 - integration and crowded combat optimization

Main presentation/support/audio work was combined with verified R40 gameplay.
Integration corrected objectless blood-emitter assumptions in logic fixtures,
Entrepreneur staged funding after wave setup, accumulating victory foot offsets,
and queued letter cues surviving return to title. Failed first suite45/1269 is
retained; focused208/208 then complete1309/1309 passed.

Pooled enemy identity snapshots, invariant collision lookups and lazy stat-key
construction preserve traversal/death semantics. Two-run200-enemy synthetic CPU
mean20.012 to12.002ms (40.0% lower), exact gameplay checksums. UI disabled; no
ordinary-game FPS or retained-RAM claim. Native victory state passed, but its
ReadPixels captures were blank and are not visual acceptance. See
crowded-wave-optimization-2026-10-21.md and main-integration-2026-10-21.md.

## October21 - physical Xbox core retest passed

The user operated the Xbox controller and confirmed all requested core controls worked. R40 logs268 samples/136 Playing, physical navigation/movement/aim and Menu pause/resume, with zero combat menu-focus cue requests. No virtual device or code fix was needed. Ranged projectile firing and other controller shortcuts were not independently exercised. [Hardware evidence](controller-hardware-2026-10-21.md).

## October21 - controlled campaign policy and completed receipts

The unfiltered R36 comparison legitimately lost to two Blood Donation copies: both engines' debug invulnerability blocks contacts, not periodic HP costs. Preserve that Failed objective rather than altering damage rules. Added an opt-in acquisition policy, disabled in survival, to skip positive periodic self-damage offers and recycle such crate rewards. Both fresh matched-policy runs completed20 waves/19 shops/exit0.12 focused and1286 integrated checks passed. Completed Python runner receipts omit the older native status field; summary/comparison now accept that schema while requiring actual exitCode. First metadata-only comparator failure is preserved; final18/18 passed. [Policy and limits](campaign-safe-loadout-policy-2026-10-20.md).

## October20 - scaled scroll navigation bounds disagreed with pixels

Native R37 traces and screenshots established a47/31-pixel offset at80% canvas scale between registered scroll controls and their rendered bounds. Actual D-pad/A confirmation still worked in that sampled case; no lost activation was claimed. Capture the parent origin/axes before BeginScrollView and retain that pass's returned scroll snapshot in a nested context stack. R38 bothscale probes Passed: maximum independent-coordinate error0.0153 pixels at80%, zero at100%; immediate/settled activation stayed correct. All8 captures were inspected. Updated shop/navigation fixtures and suite1274/1274 Passed. [Evidence and limits](scroll-navigation-native-followup-2026-10-20.md).

## October20 - clipped companion selection headings

Native R36 screenshots showed all four two-line companion titles clipped by a fixed47-pixel text rectangle. Increased the title area to68 pixels with a measured font-size fallback, preserving the icon/detail layout. All13 R37 native captures were inspected; titles are complete, start/retry flows still pass. A large support shadow in an earlier capture was an unticked fixture artifact, corrected separately from gameplay. [Native review](starting-support-native-followup-2026-10-20.md).

## October20 - mine placement discarded the selected source point

Initial StructureEffect deployment selected a source position, then SpawnMine ignored it and sampled the entire map again. Timed global mines also missed the source 160-pixel edge inset. Preserve the initial point through SpawnMineAt; use SourceRandomPosition with1.6 world-unit inset for timed ungrouped mines, retaining the explicit pet-local path. Three focused cases passed across64 seeds and direct/local controls. R36 native four-choice start/retry and support registration/placement checks passed; separate screenshot review found clipped selection titles and an unticked fixture-only shadow, tracked in the follow-up. [Evidence](starting-support-native-followup-2026-10-20.md).

## October20 - Player override preempts pending enemy rewards

R35 incorrectly inferred Player scheduling from Unit.take_damage and drained pending enemy deaths before loss cleanup. Actual Player.take_damage overrides that method and calls die synchronously. Four corrected original probes Passed/exit0; actual Main/Player/Enemy scenes suppress pending kill/loot in both same-stack orders and retain one kill/material when enemy death already flushed. Removed the pre-cleanup drain; corrected focused checks passed. See [source receipts and correction](player-cleanup-followup-2026-10-20.md). Earlier Unit FIFO receipts remain valid for Unit, not Player. Initial fixture timer/instrumentation errors are preserved separately.

## October20 - immediate damage and deferred death callback ownership

Confirmed source differences: stat-triggered damage waited too long; dying candidates were removed before Cyberball callbacks; plain Rip & Tear/Spicy Sauce explosions inherited weapon burning and life steal. Apply HP immediately, defer finalization/removal, drain older enemy requests before player cleanup, and give plain explosions their original empty burning payload and callback context. Explosion unlocks count immediate lethal transitions. The integrated 27 focused checks passed; final suite and packaged acceptance are recorded separately in [integration evidence](death-item-integration-2026-10-20.md). Source runtime probes rejected the initial pool-only ordering hypothesis before integration. Broader Player/Main cleanup and arbitrary loadouts remain bounded coverage.

## October20 - queued proc vulnerability changed after request

An original Lich heal followed by an Ice Cube elemental hit leaves89 enemy HP. The unfixed R33 player left88: its delayed Lich damage picked up the later vulnerability. Capture the request-time percentage with the enemy and apply that override only to queued proc damage; preserve ordinary hit processing and the reflected damage signature.25 focused cases passed, including the reverse expiry boundary. Final integrated/player checks follow in [completion evidence](item-material-followup-2026-10-20.md).

## October20 - proc targets selected too late

The source selects a random enemy before proc probability rolls and can abort on a zero-HP enemy awaiting deferred removal. Scrapshift instead selected a live enemy when draining its damage queue, guaranteeing a replacement target. A192-case original callback probe confirmed both hit and abort outcomes in mixed pools. The fix captures the target before rolling and preserves it through queued dispatch; dead/removed captured targets no longer redirect damage.23 focused checks passed. The queue still preserves traversal safety; full synchronous/deferred death timing is not claimed. See [proc evidence](item-proc-followup-2026-10-20.md).

## October20 - material ledger assumptions corrected

The historical25-kill/24-unit observation is not proof of a payout defect: the original zone-wide Evil Mob can count as a zero-material kill or steal drops. Current Scrapshift consumed-value/evolution payout logic matches the inspected source. Four forced source cleanup-boundary cases also demonstrate that fresh deferred drops can miss attraction/banking while pooled controls bank normally. That injected source race does not assign the historical cause or justify changing Scrapshift payouts. See [follow-up evidence](material-reconciliation-followup-2026-10-20.md).

## October20 - reference material accounting investigation

A new instrumented original-engine wave reconciles28 deaths with8 pickups plus20 cleanup-banked units. Deferred registration can leave intermediate event snapshots one unit behind. This did not reproduce or explain the historical25-kill/24-unit trace; no payout defect or fix is claimed. See [probe evidence](material-accounting-probe-2026-10-20.md).

## October20 - Golem terminal stat cleanup

Golem's computed below-half-health bonus could reappear after defeat or low-HP victory, even after temporary stats were cleared. The original clears these temporary bonuses on death/end-run. Exclude Lost/Won when evaluating this conditional; combat and paused low-HP bonuses remain valid. The source-derived dynamic oracle passed53 cases/397 snapshots with zero differences, and26 focused item/shop tests passed, including two terminal regressions. See [item evidence](headless-items-2026-10-20.md). This is source/model evidence; no new desktop playtest is claimed.

## October20 - additional headless shop boundaries

All12 source-derived shop groups and14 built-player headless receipts passed, including rawHP0 material rerolls, inventory capacity rejection, both lock preferences, acquire/remove effects, Anvil eligibility and Candy's natural chance/consequence. No production shop mismatch was established. Retained failures were fixture defects: a C# scope collision and funding Entrepreneur before its legitimate purse-discard effect. See [shop evidence](headless-shops-2026-10-20.md). Display/input acceptance is still separate.

## October20 - weapon structure registration and deployment consumers

Wrench, Screwdriver and Pruner were omitted from registered structure counts. This undercounted character links and two unlock conditions. Following `add_weapon` into `StructureEffect.apply` disproved the earlier exclusion assumption; corrected regressions retain that distinction from live actors. Group placement now uses the correct registered count and source area rules; mines cannot group. The five-turret unlock now runs from registered effects at wave setup, preventing Pocket Factory live turrets from bypassing it. See [source chain and evidence](support-registration-parity-2026-10-20.md).

## October20 - starting item choices versus automatic grants

Beast Master incorrectly received all four selectable pets plus Lootworm. The original selection screen adds one chosen item, then its character effect grants Lootworm. The import/catalog now distinguish selectable choices from automatic grants; selection, preview and run startup carry the chosen item ID. All four choices yield two items, zero weapons and +4 Speed. Focused tests passed **16/16**, and the corrected 30-case source/Unity stat preflight has zero mismatches. Interactive selection remains deferred under the user's desktop-control hold. See [starting-choice report](starting-item-choices-2026-10-20.md).

## October20 - modal shop interaction

The weapon action popup disabled ShopUI, but nested eligibility assignments re-enabled its background buttons. Preserve the inherited enabled state. R27 passed22 native action/modal assertions, including exactly3/2 popup controls and unchanged inventory/offers/currency on Cancel. See [shop report](shop-parity-remaining-2026-10-20.md). Further desktop checks are deferred at the user's request.

## October20 - acknowledge loss before unlock rewards

Full-screen unlock rewards could intercept the default loss acknowledgment. Defer that modal until the player selects OK/End run, preserving the global reward queue across Retry. R26 actual Update/virtual-A checks passed without suppressing Rookie rewards; all8 fresh captures reviewed. Actual Sick, Wounded and Golem boundary cases also passed. See [survival report](survival-parity-remaining-2026-10-20.md). Codex performed implementation and validation.

## October20 - registered pets versus live actors

Beast Master stat links incorrectly followed live actor counts. The source counts registered pet effects, scalar shields and pet structures instead. Exclude empty presentation-only PetSpec entries to avoid counting Jellyshield twice.10 selected checks passed; see [item report](items-parity-remaining-2026-10-20.md). Codex performed this investigation and validation.

## October20 - character gains on linked stats

Linked-stat bonuses omitted target character gain modifiers. Source TempStats applies the gain after accumulating truncated links; Renegade now gains +2 effective Damage per distinct common item rather than +10. Actual Coffee direct penalties and acquire/remove order are included in the corrected regression. See [item interaction report](items-parity-remaining-2026-10-20.md).

## October20 - optimized cleanup option

The source option was missing. Added entry-time per-pile common material boosting and banking, without XP/healing/pickup effects; default behavior remains raw banking.21 focused checks Passed; both R26 native normal/lethal cleanup routes Passed/exit0 with two fresh captures reviewed. See [optimized cleanup report](optimized-cleanup-parity-2026-10-20.md).

## October20 - periodic damage death transition

Periodic lethal item damage bypassed BeginDeathCleanup and set Lost directly. Route it through the shared death lifecycle. Sixteen focused tests passed; the R26 survival fixture also exercised periodic lethal damage through actual cleanup/acknowledgment, with eight captures reviewed across its scenarios. Intentional Abandon remains separate. See [remaining survival report](survival-parity-remaining-2026-10-20.md).

Current verified and unchecked coverage: [gameplay verification status](gameplay-verification-status.md).

## Current priority follow-up - October17, R25

The three requested tracks were audited and their confirmed differences fixed: combined burning cadence, Torture cleanup healing, lethal-frame/defeat cleanup and optional retry, raw health-shop currency/presentation, and unlock-pool lifetime across retry. Full suite **1181/1181 Passed**, zero failures/skips, exit0; Windows build zero errors/warnings. Three R25 native fixtures Passed/exit0 with 12 fresh captures reviewed; 568 source/settings/import inputs and 160 player files verified. Launch: `Builds/GameplayParity20261017R25/LaunchGame.cmd`.

All 115 imported unlock definitions have executed producer/save-reload/next-run eligibility scenarios, plus one coverage guard. These are automated checks, not 115 native playthroughs. Item combinations remain representative rather than exhaustive; native survival uses staged real projectile contacts and virtual-A retry, not a human survival campaign. Special shops include a source audit of every imported carrier; uncarried synthetic effects remain labeled. R23's invulnerable 20-wave comparison is historical and was not rerun or relabeled. See [priority report](gameplay-priority-followup-2026-10-17.md) and its detailed track reports.

## Historical completed checkpoint - October17, R23

The complete suite passed **1031/1031**, zero failures/skips, exit0. R23 built with zero errors/warnings at `Builds/GameplayParity20261017R23/LaunchGame.cmd`;550 recorded source/import/settings inputs and160 player files were verified again after the run. Native virtual navigation passed17 snapshots and final settlement passed both routes, exit0. The final normal-speed scripted20-wave campaign completed Won/exit0,20 waves/19 shops, and the comparison with the completed supplied-original run passed **18/18 bounded checks**. Its final receipt credited109 materials (1227 to1336), then Harvesting grew109 to115. See [complete evidence](full-verification-2026-10-17.md).

The verification phase fixed landing clocks/transforms, body/raw spawn and camera geometry, authored muzzle offsets, final income/XP and cleanup ordering. The crowded-wave follow-up removed repeated allocation work in weapon-set calculations, immutable launch classification and collision-resource lookups. In this final scripted run, combat-window wall excess was-.714 seconds across1050.200 active seconds, versus R21's170.537-second excess. Small negative differences reflect trace callback timing; these were separate evolving runs, not identical workloads or FPS benchmarks. The controlled synthetic profiler comparison is [documented separately](late-wave-allocation-investigation-2026-10-17.md).

All confirmed differences found in this phase are corrected and validated within the recorded scope. This is not proof of every character/item combination, danger level, human survival balance, ordinary focus behavior or new physical-controller testing. The campaigns used neutral stats, six tier-II weapons, scripted movement, invulnerability and mute. The virtual-controller fixture has an explicit fixture-only IgnoreFocus policy; normal gameplay policy is unchanged. Failed/interrupted attempts and R19's final-income failure remain preserved. Older checkpoints below are dated history, superseded by this completed R23 checkpoint.

Codex performed the investigations, implementation, builds and scripted tests; the user supplied direction/reference and earlier physical Xbox feedback. Interview stories remain rehearsal material until reproduced and understood.

## Historical validation checkpoint - October 17, R21

The complete suite passed **1018/1018**, zero failures or skips. The supplied original and current R21 each completed a fresh normal-time scripted20-wave run with19 shops, Won and process exit0. The expanded R21 comparison passed **18/18 bounded checks**, including the corrected final34-material payout. Both native settlement routes passed with payout atcleanup start, growth at.6667s and victory after4.0167 simulated seconds. See [current evidence and limits](full-verification-2026-10-17.md).

Confirmed corrections cover landing clocks/movement/transforms, body/raw spawn and camera geometry, authored numeric muzzle offsets for84 ranged profiles, final-wave income/XP, the last-boss.1-second delay and source cleanup ordering. Failed/interrupted/historical attempts remain preserved; R19's17/18 result exposed the final-income defect rather than being rewritten as a pass.

Remaining observed issue: R21 combat windows took170.537 seconds more wall time than active simulation. The cause is not established. A separate Development build captured baseline and crowded CPU profiles. The synthetic crowded fixture allocated about 2.4 GB inside 16 ArenaGame.Update calls; GC and Update dominate while rendering is small. This narrows the investigation but does not yet identify the inner cause or measure ordinary R21 FPS. See `artifacts/late-wave-profile-2026-10-17/allocation-comparison.json`. The verified R21 playable build stays frozen. Do not claim exact overall or performance parity. These campaigns used a neutral six-weapon loadout, scripted movement, invulnerability and mute; human survival, every item-effect combination and new physical-controller testing are separate evidence.

Allocation follow-up: measured weapon-set/settings and quadratic crowd geometry lookup allocations have been corrected. The new complete suite passed1031/1031 (attempt8), and synthetic allocation markers fell99.27% per simulation tick; this is not ordinary-game FPS. R22 navigation exposed focus-sensitive synthetic input resets, so its opt-in fixture is being isolated before the next full-run timing check. See [profiling evidence and limits](late-wave-allocation-investigation-2026-10-17.md).

Codex performed the investigations, code/tests/builds and scripted validation. The user supplied direction, reference and earlier hands-on Xbox feedback. Interview examples are rehearsal material until reproduced and understood, not independently performed user debugging stories.

## Confirmed gameplay fixes in this verification phase

| Difference | Source-backed correction | Evidence |
| --- | --- | --- |
| First-wave landing held combat or weapon clocks / attacks | Continue live combat once per tick; animate body/weapon parents separately, retain local projectile aim | `ArrivalGameplayParityTests`, `ArrivalNativeReview`, R19 native report/captures |
| Body spawn, camera target and wall limits used inconsistent origins | Separate body and raw player origin by .24 units; follow raw camera target; clamp the offset collision circle | `PlayerSpawnOriginTests`, `CombatGeometry`, R19 native centered-start and later-wave checks |
| Projectile origin depended on replacement weapon art | Import authored numeric Sprite/Muzzle offsets for84 ranged profiles; compose them through aim/landing transforms | `WeaponMuzzleParityTests`, numeric provenance report, native measured muzzle error0 |
| Final-wave payout and cleanup lifecycle differed | Credit income/XP before cleanup; reset temporary stats; grow at .66s; complete after2/4s; final boss starts .1s timer | `FinalWaveSettlementTests`, `WaveSettlementLifecycleTests`, R21 native timing and full18/18 comparison |
| Crowded updates repeatedly allocate lookup work | Replace weapon-set LINQ, cache immutable launch classification, index immutable collision data; preserve live stat/settings behavior | `WeaponSetAllocationParityTests`, `GeometryLookupParityTests`, complete suite1031, synthetic per-tick CPU/GC comparison; R23 full20 timing recorded separately with no reproduced late-wave drift |
| Virtual navigation can reset synthetic held buttons when desktop focus changes | Give only the opt-in fixture IgnoreFocus; preserve normal gameplay policy | R22 failed attempts preserved; R23 native17 snapshots Passed, exit0; excludes physical/focus-policy verification |

These production fixes are distinct from corrected validation issues: two stale initial suite assertions; a ranged fixture accidentally selecting the valid melee default; queued screenshots replacing one another; and a staged later-wave capture preceding the normal pose Update. All failed/partial attempts remain in the dated artifact folders. R19 ordinary/controller launchers also now use their own log folders instead of inherited R16 paths; this does not change gameplay.

The complete suite covers49 added landing/origin/muzzle cases within1000 passing checks. Detailed full-run results and remaining scope limits belong in [the current verification report](full-verification-2026-10-17.md).

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

## October 17 current checkpoint — R11 native checks

Latest build is `Builds/GameplayParity20261017R11/LaunchGame.cmd` (zero build errors/warnings). R10 native results: virtual navigation Passed/17 snapshots, weapons211 checks, enemies53 checks. R11 support39/39 passed in hidden and visible executions. Initial R10 support37/39 is retained: incorrect opposite-direction Cat and immediate Scapegoat-rise assertions were repaired from source evidence. Staged Cat shadow placement was also corrected and fresh PNGs inspected. These were fixture corrections, not new production pet bugs.

Physical Xbox actions remain unchecked: detected device, nine sparse observer rows, no meaningful controller actions as of this inspection. Still captures do not close projectile scale/motion/audio or crowd/contact-order comparison. Whole suite and full campaign stay held. See [current evidence](native-gameplay-r11-2026-10-17.md). Older build/pending statements below are historical checkpoints.

## October 17 remaining gameplay pass — final focused result

Fixed keyboard/D-pad merge, horizontal menu priority, Cancel release, ordinary-target bounce eligibility, turret windup/target-pool rules, discrete60Hz contact sampling, deferred delayed activation, melee sound event timing and ordinary30-material starts. Native Godot source-scene evidence exposed the reachable flame grazing mismatch that swept collision introduced. The reference keyboard problem is separately diagnosed:35 blank keyboard events; an isolated review copy has temporary bindings, original untouched. Final selected validation: **524/524 passed**, zero failures/skips, Unity exit zero; all516 recorded inputs still match the isolated clone. Evidence: `gameplay-parity-final-checks-2026-10-17.md` and `artifacts/gameplay-parity-final-checks-2026-10-17/focused-summary.json`. No whole suite or full campaign ran. R9 is intermediate and predates final discrete-contact/starting-currency corrections. A current build, production support captures, physical Xbox verification and repaired-reference keyboard verification remain pending. Computer Use stopped when the user pressed physical Escape; no further UI input was sent.

## October 17 gameplay continuation — focused checks passed

Integrated fire-time burn snapshots, weapon-slot callbacks, support collision/lifetime profiles, explosion windows, shop/unlock timing, raw-origin enemy behavior and Predator persistent contacts/accessibility speed. Attempt 1 passed 427/434: one fixture missed extended-enemy startup, three burn expectations omitted additive Sausage base damage, and three Dagger boundaries exposed inconsistent chance precision. These were repaired against source evidence. See `gameplay-parity-systems-2026-10-17.md`. Evidence: `artifacts/gameplay-parity-systems-2026-10-17/focused-summary.json`, both XML/log attempts, input manifest and catalog comparison. The same selected checks passed 434/434 on attempt 2, zero failures/skips, Unity exit zero. All 508 recorded input hashes still match the isolated clone after execution. No whole suite, new build, full campaign or physical-controller retest was run. R8 remains the last delivered gameplay build and predates these changes.

## October 17 gameplay integration — checks pending

Integrated projectile burn snapshots, support projectile profiles and explosion/contact callbacks. Added source-derived Predator persistent-contact and accessibility-speed fixes; Wandering Bot slow uses finalized speed. See `gameplay-parity-systems-2026-10-17.md`. New focused results are pending; prior 213 checks predate these changes.

## Active systems continuation — October 15, 2026

The user requested continuing agents and fixing remaining gameplay differences before full tests. New source-backed findings include achievement contact/timing rules, fractional weapon prices, Ghost instance counters, direct critical-kill currency, explosive-hit callback ordering, Taser slow duplication, pet targeting/hit geometry and enemy additive-speed handling. Fixes are in progress; the preceding 213 passing focused checks do not validate this new batch. The whole suite and another campaign remain held. See [systems audit](gameplay-parity-systems-audit-2026-10-15.md).

## Latest follow-up result: 213 focused checks passed (October 15, 2026)

Integrated the source-backed follow-up fixes and corrected the nine issues found by the initial targeted selection. The same selection now passes 213/213, zero failed/skipped. This includes regeneration/Max HP, uncapped HP damage scaling, loot probability/scatter/coordinates/Lootworm routing, burn callbacks, item stacks/triggers, damage order and egg-hatch callbacks. The generic range-extended charge hook is explicitly labeled unassigned in the supplied base scenes.

Per the user's instruction, the whole suite and another full campaign remain deferred. No new player build or native acceptance is claimed; R8 remains the delivered build. Current source evidence, first failed attempt, successful focused XML and 496 synchronized input hashes are recorded in [the follow-up audit](gameplay-parity-next-audit-2026-10-15.md).

## Current follow-up: source fixes before combined validation (October 15, 2026)

The user asked to finish the remaining gameplay differences before running the whole suite or another full campaign. The follow-up is in progress: regeneration lifecycle, Max HP changes, material drop probability/spread/consumable eligibility, boss damage order, Ice Cube rounding, range-extended charge cadence, item-trigger rounding, Anvil order and burn initialization. Fixes and regression cases are being integrated; no new full-suite/build/campaign result is claimed. R8 below remains the last delivered validated build.

Track current source findings and implementation status in [the follow-up audit](gameplay-parity-next-audit-2026-10-15.md). Preserve the accepted movement-audio fix. These new findings supersede any reading of the R8 checkpoint as proof that no gameplay differences remain.

# Completed R8 gameplay validation - October 15, 2026

Latest review: Builds/GameplayParity20261015R8/LaunchControllerTest.cmd (isolated profile) or LaunchGame.cmd (normal profile). The agents' corrections cover economy/unlocks/shop fallback, burn and aiming rules, support zones/mines/Lootworm, enemy healer/summon rules, animated beam collision and Streamer structure-linked Armor. Source-backed issue entries and causes follow below.

Verified final checkpoint:620/620 integrated tests;205/205 native weapon/tier checks;53/53 native enemy/state checks; build zero errors/warnings. Fresh normal-timing R8 campaign Passed/Won after20timer ends and19shops, exit0. Frozen-original comparison passed14/14 consistency checks: all30original and31R8purchase debits reconcile; all recorded R8stacks reconcile; ground-material cap50 held. All272processed audio clips retain the accepted bank; the user-confirmed movement sound fix remains closed.

See docs/gameplay-parity-continuation-2026-10-15.md and docs/full-run-r8-comparison-2026-10-15.md for exact input/build hashes, source findings, rejected hypotheses, captures and limits. The R8 end capture shows a queued Drifter unlock; Won is recorded in the trace/report. Different acquired gear/progression is analyzed with actual source effects rather than assumed to be RNG alone. These scripted invulnerable checks do not establish ordinary human survival balance or exhaustive gameplay equivalence. Earlier in-progress/checkpoint statements below are historical.
# R8 verified fixes and campaign in progress - October 15, 2026

R8 includes the eleven R7 correction groups below plus these source-backed repairs:

| ID | Difference | Correction and regression evidence |
| --- | --- | --- |
| BEAM-REMAIN-01 | Particle Accelerator used a constant capsule with rounded collision ends | Source animated rectangle, offsets, captured range and cubic growth/shrink curve; 12 cases plus native weapon checks. docs/beam-remaining-parity-2026-10-15.md |
| ZONE-REMAIN-01 | Enemy healer compared actor origins rather than body overlaps; healed neutral trees and flashed full-health entrants | Imported body centers/radii/layers, source mask and entry occupancy; eight actual-method cases. docs/enemy-zone-parity-2026-10-15.md |
| CHARACTER-REMAIN-01 | Streamer gained Armor from Wrench/Screwdriver/Pruner deployments and numeric effect multiplicity | Count registered structure/pet entries plus Pocket Factory as the source does; six real-character cases. docs/character-remaining-parity-2026-10-15.md |

Combined620/620 tests passed; native205/205 weapon and53/53 enemy/state checks passed; R8built0errors/warnings. Accepted audio is unchanged. A proposed Greek Fire trigger expansion was rejected after tracing the damage-event argument; existing trigger scope was correct. Fresh normal-timing campaign and comparison are in progress. See docs/gameplay-parity-continuation-2026-10-15.md for current evidence and limits; earlier checkpoints below remain historical.
# Verified R7 gameplay checkpoint - October 15, 2026

The eleven correction groups below passed 594/594 integrated tests (zero failed/skipped, 72.8708967 seconds). R7 built with zero errors/warnings; its native support fixture passed 27/27 and exited zero. The initial two malformed synthetic test fixtures are corrected and their failure evidence is retained. The audio bank remains byte-identical to accepted R6.

Current review: Builds/GameplayParity20261015R7/LaunchControllerTest.cmd (isolated profile) or LaunchGame.cmd (ordinary profile). Detailed evidence and remaining geometry work: docs/gameplay-parity-continuation-2026-10-15.md. The next agent pass is checking animated beam collision, enemy healing-zone body geometry and character-specific rules; R7 does not contain those later changes until separately validated. No complete human full-run equivalence is claimed.
# Remaining gameplay fixes - October 15, 2026 continuation

Source-backed fixes are integrated; combined Unity validation is in progress. See docs/gameplay-parity-continuation-2026-10-15.md for the current evidence index.

| ID | Observed source/runtime mismatch | Cause and fix |
| --- | --- | --- |
| ECON-REMAIN-01 | Knight/Vagabond fractional Harvesting could fail to grow | Premature integer cast; retain effective float through growth, cast combined income at credit boundary. |
| UNLOCK-REMAIN-01 | Structure unlocks could count weapon actors, pets and Pocket Factory | Achievement used broad OwnedStructures; count registered structure-effect entries as the source does. |
| SHOP-REMAIN-01 | Exhausted preferred item pool could restore excluded offers instead of eligible backup | Character bans incorrectly changed backup; apply them only to the preferred pool. |
| COMBAT-REMAIN-01 | Burning could trigger life steal, weapon knockback or high-stat critical hits | Shared hit path treated burn as a weapon hit; explicitly apply source no-hitbox behavior. |
| COMBAT-REMAIN-02 | Automatic melee swing stayed at its launch angle | Sampled pose ignored tracked direction; use current target direction while preserving manual freeze and initial knockback vector. |
| COMBAT-REMAIN-03 | Automatic tracking dropped the attack's angular spread | Retain one captured spread throughout the attack; preserve last angle if its target disappears. |
| SUPPORT-REMAIN-01 | Moth zone overlap ignored player-body radius/offset | Use source circle overlap; preserve correct additive regeneration and boolean double-life-steal. |
| SUPPORT-REMAIN-02 | Bot mine samples compressed onto arena borders | Clamp the full sampling square center before randomization. |
| SUPPORT-REMAIN-03 | Lootworm switched to nearer targets every frame | Cache the selected object/idle sentinel; invalidate on material/tree spawns or selected-target removal; fresh cache per pet/wave. |
| ENEMY-REMAIN-01 | Enemy healer incorrectly healed the player | Source scene mask selects enemy bodies only; remove unreachable player-proximity effect. |
| ENEMY-REMAIN-02 | Boss summon counts differed at half-integer boundaries | Use source half-away rounding instead of Unity half-to-even, retaining the one-summon minimum. |

These are defects found by source comparison, not new user-reported incidents. Runtime regressions and source paths are in the four detailed remaining-parity reports. Complete physics/RNG/timing equivalence and an ordinary human full-run comparison remain open; historical full-run results and R6 sound acceptance remain separately documented below.
# Current bug documentation checkpoint - October 15, 2026 (R6)

User confirmation: "Okay, the sound is gone." The movement/button-like sound report is CLOSED for the user's R6 walking retest. Unity forced-mono normalization amplified quiet Step recordings; preserve stereo and disable normalization, then rebake only the six footsteps. All other 266 contextual WAVs stayed unchanged. Focused audio tests: 23/23 passed; R6 build: zero errors/warnings. This is actual user listening acceptance of this symptom, not acceptance of every controller function or complete gameplay parity.

Known issue and evidence index (older checkpoint text below is historical):

| Area | Recorded status / detailed report |
| --- | --- |
| Movement sounded like rapid button switching | Fixed; user confirmed gone in R6. docs/controller-movement-audio-2026-10-15.md |
| Xbox sticks/D-pad unresponsive; A selected Back despite highlighted character | Input System migration and focus lifecycle fixes implemented; R5 full suite 541/541 and visible virtual-input fixture passed. Remaining physical aiming/navigation checks are separate. docs/gameplay-parity-controls-stacks-2026-10-15.md |
| Controls, unlock conditions, special shop transactions | Source-backed corrections and regression evidence: docs/gameplay-parity-controls-stacks-2026-10-15.md |
| Item effects, duplicate stacks, queued effects and removal | Corrections and remaining support/Endless coverage limits: docs/item-stack-coverage-2026-10-15.md |
| Starting weapon count | Rechecked: first character correctly starts with one; explicit extra-weapon and weaponless exceptions retained. docs/starting-weapons-recheck-2026-10-15.md |
| Full-run collection/progression differences | Both scripted normal-time 20-wave runs completed; unequal collection/progression remains documented, without claiming exact balance. docs/full-run-comparison-2026-10-15.md |
| Combat, enemies, economy and remaining exactness boundaries | docs/gameplay-exactness-2026-10-15.md and docs/gameplay-parity-parallel-workplan.md; later dated reports above supersede earlier checkpoints. |

This index covers known reports and findings; it does not establish that no undiscovered bugs remain.
# R5 controller focus and movement feedback - October 15, 2026

User physical R4 testing confirmed actual left-stick/horizontalD-pad input and gameplay, but exposed A targeting Back while a character remained highlighted. R5 fixes screen/default focus lifecycle, mouse/controller handover, a single visible activation target and duplicate/blocked focus cues. Footsteps now use source .1s initial contact/.3 animation-second speed-scaled timing with approved clips/mix unchanged. Full checkpoint541/541 passed,0failed/skipped,69.4470763s. Visible native virtual-Gamepad test passed actual A-only selection->Playing, movement with0menu/3footstep requests, Startpause/Aresume and one Rookie weapon. Hidden-window attempt did not register GUI; visible acceptance is recorded separately from physical input. Build R5 succeeded401081415bytes0errors/warnings; optional helper object-lookup warning fixed afterward without ordinary gameplay changes. Evidence: artifacts/gameplay-parity-native-2026-10-15/controller-r5-build-manifest.json, focus-native-visible/navigation-check.json and observations.md. User R5 hardware acceptance remains pending.

# Xbox controller configuration and input migration - October 15, 2026

User-reported hardware failure: A/B worked, sticks/D-pad did not. Legacy axis records were outside m_Axes and required canonical Unity YAML formatting; six tests now inspect actual deserialized settings. Controller runtime now uses Unity Input System 1.20.1 with direct Gamepad controls and ordered device events, preserving legacy keyboard/mouse under Both. Five device tests use the official InputTestFixture; ordinary EditMode timing could update button values without gameplay frame edges. Final integrated checkpoint: 519/519 passed. Native R3 detects Xbox Controller [XInputControllerWindows]; physical input acceptance is pending. Codex authored the migration/tests; the user supplied the physical failure report and selected the Input System direction.

# October 15 gameplay investigation updates

- INPUT-20261015: Native keyboard arrows skipped settings rows and jumped to Back. Spatial navigation registered only the distant right-side controls. The fix registers complete rows, initializes active-tab focus and scrolls focused rows. See ControlsParityTests and artifacts/gameplay-parity-native-2026-10-15/observations.md.
- LOOT-20261015: Full-run outcome differences prompted source inspection. Ground materials now cap at 50 and merge overflow; bank bonuses attach at spawn; achievement materials count collected objects. MaterialCollectionParityTests exercises the component and transaction boundaries. These fixes alone do not explain the entire observed income gap. See docs/full-run-comparison-2026-10-15.md.

Historical log follows, preserved in its original encoding.
# Bug and investigation log

Record observed behavior, expected behavior, reproduction conditions, cause, fix and regression evidence. Deliberate test fixtures must be labeled; they are not accidental production bugs. Development blockers are distinguished from gameplay defects.

## ART-002 — Walking technically had frames but looked like two repeated poses

**User report:** The walk did not work properly and looked like two frames or excessively fast playback.

**Cause:** The first generated sheet had four slots but very similar repeated contact/passing poses. The loop ran at nine frames/second. Counting different Sprite objects did not validate meaningful pose differences.

**Fix:** Generate a dedicated six-pose alternating-leg cycle and play it at seven frames/second, aligning its scale/baseline with idle. Full cycle changes from about 0.44 to 0.86 seconds. Keep the original sheet as provenance.

## ART-003 — Stride appeared backward

**User report:** The revised character looked as though it walked backward.

**Investigation:** Rendered left movement correctly mirrored the face; facing flags alone were not the issue. The generated contact poses mixed foot orientations, including a forward boot pointing opposite the authored rightward direction.

**Fix:** Correct the walk artwork so both boots point right in the authored cycle, with alternating planted/recovery legs, then mirror the whole sprite for left movement. Active asset: `hero-walk-v2.png`. Inspect both direction captures and the cycle itself; a frame-count assertion cannot prove perceptual animation quality.

## UX-001 — Battery currency drop implied a healing pickup

**User report:** The green battery looked as if it should recover health, although it only granted materials.

**Cause:** A plus symbol on the generated currency sprite conflicted with the repair-capsule affordance.

**Fix:** Remove `equipment_09` from the material pool. Keep five geometric scrap shapes and the separate red/ivory health capsule. The content test explicitly excludes the battery and checks the health sprite.

## ART-001 — Neighboring weapon fragment leaked into the SMG

**Observed:** The first rendered walk/idle captures showed a tiny black fragment beside the SMG. The generated sheet did not exactly follow its requested equal-column spacing.

**Cause:** The SMG import region ended at source x=510, including the leading edge of the shotgun. A prompt specifying a grid is not evidence of a pixel-perfect grid.

**Fix:** Move the shared boundary to x=502, between the actual silhouettes, and reimport through a versioned migration. Preserve original image pixels and sprite names.

**Verification:** Rebuilt and reran all 45 tests; inspected corrected runtime captures. Initial screenshot retained locally under ignored `artifacts/raw/original-art-first/`.

## QA-004 — Saved reduced-motion preference invalidated the art fixture

**Observed:** Initial art fixture returned Failed: all four idle/walk frames, six shapes, facing, muzzle, recoil, settling and magnet were seen, but bounce/scatter was false.

**Cause:** The fixture inherited the saved reduced-motion preference, which intentionally suppresses bounce. This was a fixture setup issue, not a broken normal-game preference.

**Fix:** Explicitly disable reduced motion in the opt-in fixture's memory only; never write PlayerPrefs. Force the staged drop sprites to span all six variants so coverage is deterministic. Ordinary play still honors the saved preference.

**Evidence:** Initial failed JSON retained locally in `artifacts/raw/original-art-first/art-review.json`; corrected report is `artifacts/original-art-review/art-review.json`.

## UI-005 — Atlas icons required source-rectangle rendering

**Found by code inspection:** Existing IMGUI icon code drew the full texture. This worked for single-sprite PNGs but would show the complete atlas after the art migration.

**Fix:** Aspect-fit each sprite rectangle and draw its normalized UV region. This is an integration defect found before the first atlas UI capture, not a claimed user-reported incident.

**Verification:** Inspected `artifacts/original-art-ui/shop.png` and `upgrade.png`, plus serialized-subasset and scene-dependency tests.

## DEV-001 - Invalid input dependency stopped project import

**Observed:** First authorized Unity import exited with code 1: `com.unity.modules.inputlegacy@1.0.0 cannot be found` in `artifacts/raw/generate.log`.

**Cause:** The manifest confused Unity's `InputLegacyModule` assembly name with an installable built-in package. Inspection of this Editor's BuiltInPackages directory confirmed no such package exists.

**Fix:** Remove the nonexistent dependency; use the Editor's built-in legacy input assembly. No input-system migration is implied.

**Verification:** Next Unity import and content generation exited 0. Runtime and Editor assemblies compiled, the scene and assets were generated, and AssetScope validation completed. This is a project-setup defect, not a production incident.

## ENV-001 - Sandboxed Unity could not reach licensing normally

The first sandboxed batch process stalled during license initialization and logged system-information access errors. It was stopped; the same project was launched with approved elevated tool execution and reached package resolution. Existing Unity projects and license settings were not changed. This is an environment limitation, not a gameplay bug.

## QA-002 - Smoke reported success without a usable screenshot

**Observed:** The first 24-second Windows run recorded 29 kills and 64 shots, but Unity logged `Failed to capture screen shot` in batch mode. The smoke report still said Passed. Raw initial evidence is preserved in `artifacts/raw/initial-smoke.json` and `initial-player.log`; its overall pass label is invalid and is not final visual verification.

**Cause:** The pass condition checked that a screenshot was requested, not that a file was produced. A second hidden-window run produced an entirely black PNG, which visual inspection rejected. File existence alone is also insufficient proof of correct rendering.

**Fix:** Require the screenshot file for runtime smoke, and independently inspect actual pixels before claiming visual success. Use an interactive preview for presentation review because hidden-window capture did not produce usable pixels on this host.

**Status:** Corrected code compiled and built. The visible preview successfully saved a real rendered PNG; title, combat, pause and upgrade screens were independently inspected. Invalid hidden-run artifacts are retained under ignored `artifacts/raw/`. No post-fix hidden screenshot run is claimed successful. This is a real verification defect, not a game-balance bug.

## UI-003 - Narrow windows would crop the arena

**Found by code inspection:** HUD coordinates were fitted to 1440x900 while the orthographic camera filled any window aspect. A square window would show less horizontal world space than the movement bounds allow.

**Fix:** A shared 16:10 presentation aspect now letterboxes the camera consistently with the HUD. Added regression cases for 1440x900, 1920x1080 and 900x900. No claim of a pre-fix manual reproduction; this issue was identified from the projection math.

**Verification:** The expanded 24-case EditMode suite passed, including all three aspect tests; default-size rendering inspected in the actual player. Square/widescreen behavior is mathematically tested, not yet manually resized in the player.

## DEV-004 - Hit-flash graphics object created during scene deserialization

- **Observed:** First presentation-v2 build said Succeeded but reported one error. EditMode validation passed 26/27; opening the scene emitted `CreateImpl is not allowed to be called from a MonoBehaviour constructor (or instance field initializer)`.
- **Cause:** A new MaterialPropertyBlock in an ArenaGame field initializer invoked Unity's native graphics API during deserialization.
- **Fix:** Create the block lazily when gameplay first flashes an enemy. Also fail the build wrapper when `totalErrors != 0`, even if the build result says Succeeded.
- **Verification:** The same scene/content test now passes; all 27 tests pass and the rebuilt Windows player reports zero errors/warnings.
- **Evidence:** `artifacts/raw/v2-before-fix-tests.xml`, `artifacts/raw/v2-before-fix-build.log`; current `artifacts/editmode.xml` and `artifacts/build.txt`.
- **Attribution:** Assistant implemented, reproduced and fixed this issue. Candidate can rehearse the lifecycle explanation; no personal debugging claim is implied.

## UI-005 - Upgrade feedback wrapped into two crowded lines

Observed during desktop play after selecting Field Lunch: UPGRADED! used the same 120-pixel label rectangle as a two-digit damage number. Expanded long feedback labels to 260 pixels while keeping ordinary damage numbers compact. This is a direct rendered finding, not a fabricated unit-test failure.

## AUDIO-006 - Audio preload setting moved in Unity 6

First audio build failed compilation with CS0619: `AudioImporter.preloadAudioData` is obsolete and moved to per-platform sample settings. Changed the importer to set `defaultSampleSettings.preloadAudioData` with PCM/decompress-on-load settings. Subsequent build succeeded with zero errors/warnings. Original compiler output is retained in `artifacts/raw/audio-import-api-failure.log`. Assistant-observed and fixed; not claimed as the candidate's personal debugging work.

## REF-001 — Spawn randomness depended on frame size

- Found during review of the initial group scheduler: catching up all repeats of one group before another changes shared RNG call order on a long frame.
- Fix: process elapsed integer seconds chronologically and then groups in stable order, using a double accumulator. A test compares every wave at 60 Hz against one large tick with the same seed.
- Validation: all 20 schedules produce exactly equal counts; included in the 33 passing EditMode tests.

## REF-002 — Hidden player could not capture its frame

- First new Windows smoke run reached the shop with 25 kills and 18 shots, but returned Failed because Unity reported `Failed to capture screen shot` while launched hidden.
- This is a failed visual verification run, not a successful screenshot. Gameplay counters alone are insufficient.
- Next action: run the interactive preview with a rendered window and verify saved pixels before marking the runtime check passed.

## REF-003 — Upgrade headings clipped and birth warnings lost their color

- Actual rendered review, not a unit-test assertion, exposed two defects: 30-point multi-line headings exceeded a 62-pixel label, and the source warning texture was white because its source-engine tint was not encoded in the PNG.
- Fix: use reference upgrade icons, give titles a 96-pixel centered region, and explicitly tint the SpriteRenderer red. Copy source upgrade-tier values instead of multiplying every stat linearly. Clear damage floaters on intermission so frozen combat numbers cannot overlap menus.
- Evidence: independently inspected corrected `artifacts/reference-review/upgrade.png` and `combat.png`. Long HP Regeneration / Attack Speed labels fit; warning Xs are red. The tier regression verifies +16% tier-four damage and +9% tier-four critical chance.

REF-002 resolution: the rendered player generated all three valid review images. The hidden run remains a recorded failure; it was not relabeled as a pass.


## COMBAT-001 — Spawn groups lost their spatial and timing contract

**Found by comparing code with the supplied reference:** safe-position retries could replace a clustered/edge position with an unrelated arena-wide position, and clustered units ignored extra player distance. The callback created separate anchors for different unit types in the same group. Queue length 100 silently discarded additional births. An initial chance miss incorrectly canceled all future repeats; interval reduction was applied one repeat late, and scheduling from rounded check time accumulated drift.

**Fix:** one anchor per emitted group; bounded square/edge-preserving safe sampling for every unit; queue drains faster above its threshold without discarding births; initial chance applies only to the initial appearance; subsequent repeats reduce their interval before advancing the original schedule time.

**Evidence:** explicit 1,4,7,9,10-second accelerating-schedule test, failed-initial-chance repeat test, 250 edge placements and 100 clustered samples, plus all-wave frame-size stability. These were source-comparison defects, not claimed prior human reproductions.

## COMBAT-002 — Ranged hits ignored weapon-specific knockback

**Found by code inspection:** melee passed the weapon's knockback into Damage, but ranged Shot did not store it and always used Damage's default of 1. This made SMG crowd control stronger than its displayed zero knockback and erased differences between weapon families.

**Fix:** carry knockback in each ranged Shot and pass it to Damage. The shop displays the same value used by the runtime. Runtime combat fixtures exercise ranged damage; no pre-fix human comparison is claimed.

## UI-007 — Shop background leaked the combat HUD

**Observed:** the first captured redesigned shop showed faint health/XP text and wave/timer labels behind its header. Its translucent background drew after the combat HUD.

**Fix:** route Shop and Upgrade before the combat HUD in OnGUI; Upgrade draws its own compact HUD once. Hide the zero countdown during reward screens. Inspect the replacement screenshots after rebuilding.


## PARITY-001 — Correct-looking attacks used the wrong timing and targeting

Source inspection found an invented 0.18-second charge recovery, shooter aim locked at preparation, ranged enemies frozen while shooting, and shared projectile values that ignored scene overrides. Per-enemy profiles now distinguish movement, preparation, animation tail, range, dash length, spread and projectile stats. Charges retain their committed direction; ranged aim follows the player until release. The regression tests use source timing boundaries. Earlier COMBAT-001/iteration claims about safest-candidate placement and intentional ranged aim locking are historical and superseded by this parity pass.

## PARITY-002 — Spawn queue and cap changed encounters

The old FIFO shared queue delayed trees/bosses behind enemies, sped up at the wrong backlog threshold, and capped warnings/trees/bosses together on every birth. Source uses LIFO lanes, a 0.05-second cadence, two normal spawns at 120 queued entries, and group-boundary culling of active regular enemies. The new queue test checks exact order and both sides of the 119/120 boundary. Safe placement now relaxes clearance by 5 source pixels per retry instead of searching for the farthest candidate.

## PARITY-003 — Flat scene parsing mixed boss states

The old importer combined properties from every scene node, allowing phase-two attack settings to overwrite Predator's root charge. Explicit node-based profiles now separate these attacks. The runtime phase switch occurs at <=50% HP or 45 elapsed active seconds, checked each second. The campaign reports observed boss phase changes; this is an actual integration path, not just a profile assertion.

## PARITY-004 — Progression and shop rules diverged from the reference

Starting at level 1 skipped the 16-XP threshold; automatic +1 Max HP per level was absent; rarity used unrelated independent rolls; upgrade rerolls repeated previous stats; negative harvest was discarded. These now follow the inspected rules. The first two shops previously could offer more than two weapons; locks preserved the object but let its price rise with the next wave. Offers now retain the wave used for pricing. Boundary and transaction tests cover these defects. Base HP is deliberately corrected from 20 to the source default 10; this is a balance change requiring user playtesting, not a measured improvement in difficulty.

## SET-001 — Temporary bonuses could contaminate permanent stats

Integration hazard found during implementation: `Add` formerly read `Stat` before storing a new value. Once `Stat` includes equipped-set effects, using that total would permanently bake those effects into an item or level reward. Separate `PermanentStat` from the derived total and mutate only the former. Regression tests apply permanent effects while a set is active, then remove the set and verify only the permanent values remain. This was caught during implementation, not reported as an observed user incident.

## SET-002 — A test confused level HP with a combine bonus

The first set suite passed 97/98. A test captured baseline Max HP before advancing a wave; positive Harvesting then earned a level and its automatic +1 HP. Its expected post-combine value was stale. Capture Max HP immediately before combining and assert the exact 3-HP loss instead. Final suite: 98/98. Independently rendered combine evidence shows 30 to 27 HP; the subsequent earned level produces 28. This was a test-assumption failure, not a product fix.

## LOOT-001 — Generic healing drops removed enemy rewards

Source comparison found that every successful consumable drop became a repair pickup. Trees never offered crate decisions, loot aliens lacked their guaranteed crate, and the generic path could drop consumables from spawned junkies despite their source prohibition. Explicit per-enemy profiles now perform separate consumable and crate rolls, apply luck and dilution by crates spawned, and honor guarantees/exclusions. Tests cover probability boundaries; the rendered fixture kills a looter through the actual damage/drop path and verifies its crate.

## LOOT-002 — Wave cleanup would discard queued rewards

When integrating crates, the previous transition cleared all pickups after selecting the next phase. Collecting crates after that point would either fail the Playing guard or leave the already-chosen Upgrade/Shop phase out of sync. Cleanup now collects harmless consumables before `RunRules.Tick` chooses the reward phase, then processes crates before levels. The fixture leaves a crate far from the player and verifies it appears in the two-crate queue at wave end. This was an integration hazard identified before final validation, not a claimed user incident.

## HEAL-001 — Continuous regeneration and unlimited life steal changed build strength

The old runtime healed `regen / 5` HP each second and allowed every successful hit to steal HP. Source uses discrete one-HP ticks with a nonlinear interval and a shared 0.1-second life-steal cooldown. New tests verify exact intervals, no early fractional healing, reset at full HP and pause semantics. Existing content/art is unchanged.

## VERIFY-002 — A runtime smoke pass does not mean a wave was survived

The latest normal-speed smoke reported Passed but ended Lost after 15.30 simulated seconds (17 kills, no upgrade). Inspection confirmed that its existing pass condition only requires shots, kills, enemies and screenshot output. Retained the raw report and explicitly classified it as combat coverage with a failed wave clear in verification. This result is not evidence of acceptable balance and was not overwritten by repeated attempts.

## ART-004 — Generated atlas cells clipped a nose and leaked neighboring pixels

Observed in the actual Windows enemy gallery: the horned charger's nose stopped at a vertical edge, and its tip appeared beside the golden junkie. Uniform 256-pixel columns cut across generated bodies that did not strictly follow the requested grid. Measured connected alpha bounds and authored per-sprite import rectangles instead of editing the PNG pixels.

The added transparent-border regression then failed on the spitter's top border (alpha 0.235 versus the 0.12 threshold): padding picked up the adjacent tree. A shared transparent column at x1280 separates the two final import regions. Final suite 120/120 and the corrected gallery both pass. Initial gallery: `artifacts/spring-gallery/`; corrected gallery: `artifacts/spring-gallery-final/`; failed test XML is retained under ignored raw artifacts. This was an observed visual defect and subsequent test failure, not a fabricated interview example.


## ART-005 - Same-count wardrobe replacements and generated atlas boundaries

Observed code defect: wardrobe invalidation compared only inventory count. New identity/order/preset comparisons refresh same-count replacements; covered defaults restore through the shared resolver. Regression coverage is in AppearanceTests.

Actual validation failures: first modular pass 126/127 (wing cut, alpha .172549), second 126/127 (glasses cut, alpha .207843). Evidence: ignored artifacts/raw/appearance-first-tests.xml and appearance-second-tests.xml. Final atlas imports use per-cell measured transparent separators and border assertions on all six sheets. An enemy spacing draft also had no fully transparent background pixels and was rejected before import. See docs/modular-appearance.md for reproduction and generation provenance.


## October 11 - relative scale and shadow ownership

User-reported: player too large, first-wave enemies too small, thin leg edges and inconsistent shadows. Baby art was additionally scaled to .72 while player stayed at 1.0; relative bare-body width was about half. Set visual-only factors to .85 / 1.4 and separated enemy presentation from collision radii. Replaced an unnormalized atlas-shadow crop that inherited squash/rotation with a flat world-space circle renderer owned by the actor. The first new EditMode regression failed on disable propagation (128/129); ExecuteAlways restores lifecycle callbacks for Editor-created instances. A regression also checks oval bounds under squash/rotation and destruction cleanup. See verification.md for final evidence.


## October 11 � world dimensions inconsistent with imported movement/range

Observed mismatch: whole shallow arena visible at once in Unity, whereas the running reference has a taller map and clamped camera following. Root cause: source movement and ranges were converted at 100px/unit while a 23x11.4 floor and ad-hoc visual scales survived from the prototype. Centralized map/camera geometry, applied 20.48x15.36 bounds to movement/spawns/scatter and calibrated original sprites by reference dimensions. Normal-camera edge traversal and source-grounded regression checks cover the fix. Contact radii remain an explicit adaptation; this is not full collision parity.


## October 11 - combat geometry and partial conversions

Code-observed defects: bullets were still culled at the old y=5.25 map edge; one role-based radius conflated crowd spacing, target area and contact; pistol knockback imported at /10 was multiplied by3 instead of10; reverse-list projectile traversal could spend full piercing damage on the farther target first. The impact shape inherited a23px local offset which the concrete bullet/laser scene cancels; this was caught during source review before final verification. Runtime now uses separate measured radii and lower-body centers, map-based cleanup, corrected force and60Hz-equivalent damping, and earliest swept-circle entry. Regression and controlled runtime evidence are in CombatFeelTests and artifacts/combat-feel-review. No failing automated test was required to discover these code/source mismatches; do not invent one. Existing melee and rigid-body response approximations remain documented.


### Rendering checks could pass without screenshots

Actual validation failure during the combat pass: hidden player launches logged3/5/6 screenshot failures for combat, enemy and campaign fixtures, while their gameplay assertions returned Passed. The combat directory also contained older PNGs, so existence alone would have been misleading. Preserved counts in artifacts/combat-capture-recovery/hidden-results.json. Added shared CaptureVerification checks for a PNG signature, minimum bytes and modification time after run start; all three report formats now require capturesVerified. A regression rejects missing, invalid and stale captures. Reran with a rendering window; final reports and inspected images are the evidence, not the earlier hidden-run Passed fields.


## October 11 - weapon presentation and stale migrated scene

Source-observed: idle weapon directions fanned radially and all weapon sprite pivots used .24 units. Reference weapons rest horizontally toward facing; knife/spear use different sprite offsets. Fixed independently of combat stats. Ending UI also incorrectly switched to a distant menu camera; now it retains the death scene and shows the attacker.

Actual validation defect: source synchronization restored an older Deck.unity while YardPresentationV1.txt remained. A subsequent build rendered straight rails and no parachute, even though the fixture reported successful arrival pose/flow checks. A fresh screenshot proved that valid PNGs and numeric animation state did not guarantee the intended asset was present. Migration now checks saved scene objects/references, the EditMode suite checks the authored scene, and runtime checks require authoredScenePresent and canopyInstantiated. Existing generation setup repaired the scene before the new regression executed; no failing test run is claimed. Final verification uses the repaired scene, matching source/validation bytes and inspected runtime images.

Two fixture-only presentation issues were also corrected: saved reduced motion suppressed the tested movement; frozen birth age made the staged enemy look tiny. Production scale was not altered to hide a fixture setup error.


## October 11 - incomplete stat icons and animation counted as free time

The stat sidebar resolved images through eight upgrade definitions, but displayed thirteen stats. Five had no icon source. A complete stat presentation lookup now covers all sixteen reference primary rows with existing art plus eight original code glyphs. Serialized completeness is tested independently of the offered upgrades.

Source inspection also found that shooting animation suspends the reference cooldown. Unity counted down both simultaneously and used one recoil shape for all guns, producing a cadence/feel mismatch. WeaponMotion now separates those intervals and covers frame-boundary overshoot, family timings and attack-speed scaling. No failing old test is invented: this was a source-comparison discovery.

Visual verification found the new common Ok button overlapped the victory caption after moving it to match the loss dialog. Moved the victory caption below the button and rerendered. A fixture edit briefly declared four unused local variables instead of including the new UI-phase booleans in its pass condition; build warnings exposed it. Corrected the conjunctions and produced a zero-warning build.

## October 11 - wrong player-hit sound and mismatched leg preview

The Hurt import used zombie_voice_attack_grunt_01, explaining the user's rejected vocal cue. The reference player scene actually declares bullet_impact_body_flesh_05 through 08. Reused the exact four existing Impact clips, corrected first-time wiring and added an idempotent bank migration plus a serialized-reference test. Removed the extra damage border and player disappearance, which the inspected reference damage path does not use. Refined the player impact from a closed star into separate tapered streaks after inspecting the source frame and our render.

The silver limb sprite was mounted horizontally. Corrected resting angles and the saved prefab; subsequent renders exposed GUI helpers mirroring after rotation. An explicit transform now mirrors in sprite space first, matching SpriteRenderer. A regression verifies bilateral preview symmetry in addition to world-facing checks. One stale hurtOverlay reset caused an initial compiler error; removed it before the successful build. See player-hit-and-leg-correction.md for source paths and verification boundaries.

## October 11 - fullscreen viewport and starter profiles

User-observed black side bars came from the fixed-aspect camera viewport. Full-window camera rendering, aspect-aware following, extended background bounds and shared UI coordinate mapping remove pillarboxing while preserving sprite/UI proportions. Native borderless fullscreen is now the default, with a persisted F11/Options toggle.

Actual test failures: the first expanded suite passed 163/166; two wardrobe tests assumed the old bearded-character index and one assumed seven art sheets. Tests now locate the beard by feature and expect the new original atlas. The added atlas-border check then caught a shield clipped by a regular row boundary; the first gutter correction still left the preceding sprite's last column at alpha .145. Centering slice boundaries inside the transparent gutters fixed both sides without rewriting the bitmap.

Build-warning inspection exposed a recursive Equipment -> WeaponTrait -> Equipment schema that exceeded Unity serialization depth 10. A non-recursive ProjectileStats leaf removes the cycle. The builder now preserves warning/error messages alongside the count. Source inspection also caught missing-health lifesteal using base HP and ignoring set bonuses; a regression now covers Sharp Tooth after Primitive HP increases. See fullscreen-and-starter-characters.md and verification.md for final evidence and limits.

The final player-location review exposed a harness path bug: Unity resolved relative screenshot paths against the Data directory, unlike Directory/File operations. All nine captures failed, and CaptureVerification correctly rejected stale prior PNGs. Normalize the requested review path with Path.GetFullPath at entry; the same relative-path invocation then passed with fresh images. This is a fixture correction after the 167-test pass, verified with the actual rendered player.


## October 11 - selection eligibility and inspection descriptions

Weapon selection incorrectly reused locked character-tile presentation for weapon choices; source weapon selection explicitly hides locked entries. Filtering now uses profile eligibility and availability before constructing the grid or Random pool. Inspection work also exposed an empty Wrench scaling list (170/171 initial tests), scroll-view hover-coordinate mismatch, deferred screenshot/state advancement, and missing percentage units on serialized stat effects. See inspection-and-weapon-selection.md for causes, fixes and final evidence.

## October 11 - expansion migration and parachute mesh

The first expansion migration dropped original wearables outside the eight-item shop whitelist. Preserve all authored images/appearances before merging source data (22 supported original definitions). A rendered canopy showed jagged recesses despite passing bounds tests: center-fan triangles bridged concave edges. Replaced them with a vertical strip and re-rendered. Tooltip translucency also let underlying stats show through; use opaque backgrounds. Full account and regression evidence: item-expansion-and-design-parity.md.

## October 11 - run feedback and rewards

The ranged pre-shot line was a custom telegraph, now removed. Aiming previously acquired only attack-range targets; detection now extends beyond firing distance using the reference thresholds. Instant wave cleanup concealed stored-material collection; presentation now runs before the reward transition. Tree markers used enemy red; original green strokes avoid multiplying red sprite pixels.

## October 12 - wrong collection destination

Wave-end pickups targeted the bag at HUD y=166 instead of the money icon at y=122.5. Corrected the endpoint, added arrival pulse/rising amounts and removed the forced disappearance timeout. User identified the visual mismatch; Codex corrected and validated it.

## October 12 - full catalog and screenshot-driven UI

Actual failures: scrolling shop weapon inspection used the wrong GUI coordinate space; explicit viewport coordinates fixed the missing focus target. Atlas-border checks found neighboring artwork in multiple irregular weapon cells. Connected-component-derived bounds and shared UI/world clipping remove fragments without repainting the PNG. Imported sprites rejected OverrideGeometry, requiring separate runtime sprites. Unity Rect JSON serialization then produced zero-size imports; explicit pixel-region fields fixed the weapon-size test and rendered mesh errors. Failed runs are superseded by final verification evidence.

Reference audits also corrected Vorpal reward identity, double-applied Fisherman Bait damage, and Nightmare requiring ten distinct Danger-5 wins. Pacifist now counts population-capped enemies and rounds before the horde divisor; 21 enemies in the regression distinguishes this from flooring.

The final geometry test also caught a coordinate-contract error: OverrideGeometry accepts pixel coordinates relative to the sprite rectangle, not pivot-adjusted world units. Corrected against https://docs.unity3d.com/ScriptReference/Sprite.OverrideGeometry.html and retained the vertex-count regression.

Runtime geometry verification was moved into the built-player fixture because EditMode disallows the native override call. EditMode still checks mask bounds/retained pixels. Visual inspection then exposed subpixel strip loss in IMGUI; render each masked icon at native resolution into a cached render target before scaling it once. This is a rendering correction, not a replacement of the original artwork.

Final visual review found the level-up Stats panel covered the pending-level arrows. Offset that panel down on Upgrade only, matching the supplied screenshot while preserving the shop alignment.

The first completed 20-wave campaign reported Failed despite Won/20 waves/47 purchases: boss-phase telemetry counted Predator only, but the new reference boss selection chose Invoker. Added phase-transition accounting to extended bosses and reran the campaign; the original failed report is retained separately.


## October 12 - boss hazard and shop presentation fixes

Invoker hazards damaged immediately for three seconds because all stationary projectiles shared a generic orb lifetime. A dedicated timed pillar now gates collision to 0.54–0.68 seconds, uses a 0.23 radius, and stops by 1.04 seconds or on contact. The spread field was treated as a radius and disk; source uses square width or circle diameter. Added boundary tests and actual built-player health/contact assertions. Final mutation speed is +150%, confirmed by following reset_speed_stat through EntityService; it is not an absolute 150 px/s.

Shop money icons reduced the available number width and exposed IMGUI numeric wrapping. The first screenshot showed a clipped price; disabling wrapping and allowing text-measurement padding fixed it. Fixture-only mouse hover suppression prevents an unrelated real cursor from covering staged reference comparisons.


## October 12 - upgrade inspection expectation

Removing the level-up inventory made its previous forced weapon-inspection test obsolete. Removed that target and retained weapon inspection checks on pause/shop. Added character inspection as a separate shop capture. Character portraits are presentation entries only; they do not enter the item collection or change rolls/stats.

UI follow-up: displaced grey Frame shadows made tiles look doubled/rough. Removed the shared offset shadow, retaining rounded dark borders. Pause menu lacked a complete inventory/timeline layout; now uses sorted tiles with a character entry and bottom dot progress. Codex compilation initially referenced a nonexistent weapons field; fixed to CatalogData.equipment.

Colored card edge follow-up: Frame supplied a black outer perimeter before a colored inner border was added, leaving an unwanted black lip. Replaced the stacked construction with TierCard, one colored perimeter and a uniform inset fill.

Rounded inventory focus: Inspect drew four white rectangular border strips over the rounded tile fill, creating square white corners. Replaced them with a rounded border-only draw (radius 8, width 2). The inspection fixture now applies forced focus to this outline too, so screenshot validation exercises the previously missed layer.

White inventory seam follow-up: the previous rounded outline fix was incomplete. Frame still painted a dark full-size base behind an inset white fill, and Inspect added another perimeter. Focused frames now draw one solid rounded white surface, and inventory inspection no longer overlays another border. Keyboard focus uses the same tile state. Added a pause-character reproduction capture.

## QA-REMAIN-01 - Synthetic effect fixtures omitted serialized empty keys

The initial continuation suite passed 592/594; two new structure-unlock fixtures threw NullReferenceException in ConditionalStat because their constructed ConditionalEffect objects omitted key. Real imported entries contain an empty-string key for these effects. Initialize the two synthetic keys to empty strings; runtime code is unchanged by this fixture repair. Initial XML/log are preserved under artifacts/gameplay-parity-continuation-2026-10-15/initial-tests.*. The full rerun is in progress.


## October 13 - item pool and reroll parity (source-audit defect)

**Trigger and expected behavior:** Shaoyu asked for shop, crate and level-reward randomization to match the supplied extracted game. Source review found that the default random item pool must reject zero-maximum-count items, a new shop excludes only locked prior offers, a reroll excludes the offers it replaces, and an empty rarity tier does not fall back to a lower tier. This was a code/source mismatch; no manual run is claimed to have displayed the rare bad item.

**Cause:** `ConfigureShopItems` accepted `broken_hourglass` because it checked availability but not `maxCount`; item selection had a lower-tier fallback; shop exclusion and random draw order differed from the reference. The original allows `broken_hourglass` through a separate transformation, not ordinary shop/crate rolls.

**Fix:** Filter zero-count items, keep the selected tier, distinguish fresh-shop from reroll exclusions, and align the shop/crate draw sequence. The level reward rarity formula already matched; only its draw order changed. See `item-randomization-parity.md` and commit `d6cd68c`.

**Verification and limit:** The isolated item branch passed 330 Unity EditMode tests, including eligibility and reroll cases. This establishes the implemented selection rules for the imported pool, not identical individual offers from equal numeric seeds or full DLC/unlock parity.

## October 13 - XP level label sat at the right of the bar (UI defect)

**User report:** The laptop game's level text appeared at the right of the XP bar rather than centered like the health text.

**Cause and fix:** `GameUI` drew `LV.#` in a narrow x=195, width=87 rectangle with right alignment. Commit `574f935` uses the XP bar's full x=24, width=262 rectangle and the shared centered style.

**Verification and limit:** The Windows build succeeded with zero errors and warnings. A separate screenshot assertion or post-fix user acceptance was not recorded for this alignment change.

## October 13 - auto melee swing stopped tracking its target (combat defect)

**User report:** A melee weapon sometimes missed when Shaoyu moved during its attack. Some misses are expected: the original damages only when the animated hitbox overlaps an enemy, and ordinary weapons start attacks while the player is stationary. The report prompted a source comparison; it did not by itself prove every miss was a bug.

**Cause:** The supplied `weapons/weapon.gd` keeps rotating an auto-aimed weapon toward its live target while shooting, and its melee behavior moves a timed collision hitbox. Scrapshift's `BeginMeleeAttack` stored the initial angle and `TickMeleeAttack` reused it for the whole swing. Movement could therefore leave the hitbox pointed at the target's old direction. Manual aim is intentionally fixed during a swing in the reference.

**Fix:** Commit `137bbd4` updates the auto-aimed swing angle from the current weapon mount and live target each frame, preserves the spread chosen when the attack began, and stops tracking after the target leaves detection range. It leaves manual aim and the collision-based hit rule intact.

**Verification and limit:** 331/331 Unity EditMode tests passed, the Windows build had zero errors/warnings, and the built-player fixture passed 205/205 stationary weapon attack checks. The new movement-angle regression checks direction and preserved spread. No human playthrough or proof that every moving-target miss disappeared is claimed. Shaoyu reported the symptom; Codex compared the source, implemented and ran these checks.

## October 13 - missing red enemy hit burst (visual defect)

**User report:** Enemy deaths lacked the red exploding effect visible in the supplied original game.

**Source diagnosis:** `global/effects_manager.gd` emits `hit_particles.tscn` on every positive hit to an enemy with `generate_blood`, including a fatal hit. That scene uses eight dark red `particle_1.png` puffs, a half-second lifetime, outward motion and no gravity. Trees set `generate_blood = false`. The separate `entity.tscn` death animation shrinks, rotates and darkens the body; Scrapshift already had that part. Its generic coral sparks were the visible mismatch.

**Fix:** Import the supplied original particle image and emit its dark red puffs from the hurt sprite's edge on enemy hits, including kills. Exclude trees, honor the visual-effects setting and reuse the existing particle cap and cleanup path. The fatal hit shows the burst alongside the existing corpse animation.

**Verification and limit:** Unity Windows build succeeded with zero errors and warnings. The built-player combat-feel fixture passed, counted eight blood particles on the fatal hit and saved a fresh `enemy-death.png` capture, which was inspected. The fixture uses a staged one-HP enemy and explicit time advances; it does not establish frame-perfect Godot particle trajectories or human play acceptance. The first runtime launch hit a Direct3D 12 device-removed crash, so the successful fixture used Direct3D 11 and an absolute capture folder.
## October 13 - rocket impact lacked the source smoke burst (visual and timing defect)

**User report:** Shaoyu asked whether the rocket launcher contact explosion used the same effect as the original game, then requested the source appearance, duration, particle rate, radius, and damage area.

**Cause:** The existing `ReferenceWeaponBehavior` used the source flash sprite, sound clips, and approximate area, but destroyed the whole effect after 0.05 seconds. The original `explosion.tscn` removes only the flash and hitbox at 0.05 seconds, emits smoke, and keeps the node alive until 1 second. The collision's 3-pixel downward offset also needed to scale with the effect size.

**Fix:** Imported the original `particle_3.png`; added 48 dark smoke particles for rocket tiers with the source emission radius, offset, speed, rotation, fade, and 0.75-second lifetime. The flash and damage window remain 0.05 seconds, and the effect cleans up at 1 second. The radius is 1.76808 world units before explosion-size bonuses. A floating-point threshold tolerance prevents the smoke from slipping one frame late at exactly 0.05 seconds. See `rocket-explosion-parity.md`.

**Verification and limit:** Windows build succeeded with zero errors and warnings. A Direct3D 11 built-player fixture passed flash, area, smoke count/radius/lifetime, and cleanup checks. Visible screenshots were inspected for the flash and smoke sequence. Unity and Godot random particle motion and rasterization are not asserted to be pixel-identical.
## October 13 - other explosions and Baby With A Beard projectile parity

**User report:** After the rocket fix, Shaoyu asked whether other weapons and items use explosions, how piercing works, and which item fires a projectile when an enemy dies.

**Cause:** The rocket smoke fix covered only its three tiers. Twenty-one weapon/tier profiles reference the source explosion, but the other eighteen had no source smoke count in Scrapshift. Rip and Tear, Spicy Sauce, Bull, Glutton, mines, Rocket Turret, and Bonk Dog still used generic impact bursts. Item stacks were rolled separately instead of the source's combined chance and damage. Baby With A Beard already spawned a death projectile, but its default bullet scene was absent from the imported projectile selection.

**Fix:** Import source smoke counts, radii, sound levels, and flesh-explosion art/audio for all weapon and item paths; use source-style overlap for item, structure, and pet explosions. Aggregate item stacks into one roll and one effect. Resolve special-spawn projectiles without a declared scene to the source default bullet. Preserve the existing travel-ordered piercing and bounce sequence.

**Verification and limit:** Windows build passed with zero errors/warnings. Direct3D 11 built-player checks passed 21 weapon profiles, Shredder and Nuclear smoke, red Rip and Tear smoke, Baby's bullet scene and two-target piercing (100 then 50), and source structure metadata. Existing rocket and full starter/item/deployable reviews passed. Captures were inspected. See explosion-and-piercing-parity.md. Pixel-identical particle motion is not claimed.
## October 13 - projectile count, bounce position, and post-hit damage parity

**User report:** Shaoyu asked whether extra projectiles, bouncing, and piercing damage buffs or reductions behave like the supplied original game.

**Cause:** The main gun path already added the projectile stat and spread, and bounce already took priority over piercing. Its minimum-one-shot clamp differed from the source when a base count is zero or modifiers take it to zero. Turrets and ranged pets did not apply the added projectile count or spread. Bounce redirects used the end of a frame's travel rather than the collision point and could select a boss or neutral tree, while the source spawner selects ordinary enemies. Damage retained after a bounce or pierce remained fractional in Unity; the source hitbox stores an integer and truncates the reduced amount.

**Fix:** Use the source count/spread rule for held ranged weapons, turrets, and ranged pets. Move a bouncing shot to the collision position and restrict its random target pool to ordinary enemies. Truncate positive post-hit damage after clamping reduction, retaining at least one. Preserve bounce-before-pierce, ignored prior targets, and the Particle Accelerator no-bounce gate.

**Verification and limit:** The Windows build passed with zero errors/warnings, nine focused Unity EditMode tests passed, and a Direct3D 11 built-player fixture fired three shots with +2 projectiles, bounced for 11 then 5 damage, and pierced for 11 then 7 with Pumpkin's +15 bonus. Full starter/deployable and companion reviews passed. See projectile-rules-parity.md. Random trajectories and crowded-wave target choices are not asserted to be deterministic.

## October 13 - showcase helpers appeared instantly in fixed rows

**User report:** The Auto Turret demo started with turrets at fixed points, and pets appeared immediately. Shaoyu asked for wave-start spawning like the supplied game and stronger item-based Ranger, Engineer, and Druid builds.

**Cause:** `BeginLoadoutShowcase` called `StartWave`, which deployed all structures and pets immediately, then overwrote their positions with hardcoded rows. The reference `entity_spawner.gd` queues them from the one-second `StructureTimer` and releases one structure and one pet every three physics frames.

**Fix:** Showcase wave start now queues owned structures and pets, waits about 1.05 seconds, then releases one of each every 0.05 seconds using ordinary random placement. The Ranger receives real Bandana, Ricochet, Pumpkin, and attack-speed items. The Engineer and Druid receive additional helper items plus attack-speed, Engineering, and structure-crit buffs. No projectile-count stat is injected. Seashell is a DLC item outside the supplied base-game catalog and was excluded after Shaoyu clarified the desired item effects.

**Verification and limit:** 363/363 EditMode tests passed, including item-derived stat checks and queue cadence boundaries. The Windows build succeeded; the first run had one unrelated deprecated-API warning in the performance review, which was corrected before the final build. The timing is a 60 Hz approximation of the source timer and queue; see `showcase-loadouts.md` for the exact scope.
**Built-player check:** Hidden headless player runs logged the Engineer queue draining to 11 turrets/gardens plus 3 pets, and the Druid queue draining to 1 structure plus 12 pets, both at 1.61 seconds. The twelfth Engineer structure request is a landmine spawner, tracked separately from visible turrets. The final build had zero errors and warnings. Visual placement was not inspected in this headless check.

## October 13 - wave countdown warning missing

**User report:** The final five-second wave counter should turn red and play the original clock sound.

**Cause:** The Unity HUD always rendered its normal timer color, and the sound bank had no wave-tick cue. The source `wave_timer.gd` starts a one-second tick when less than six seconds remain; its first timeout changes the label to the configurable negative color and plays `clock_tick_01.wav`.

**Fix:** Color displayed seconds 5 through 1 with the configured negative color. Add the byte-identical stereo clock WAV as a dedicated cue, playing once per displayed-second crossing with the source ±0.1 pitch variation. Keep its notification voice audible without ducking combat.

**Verification and limit:** The Windows build succeeded with zero errors and warnings. All 364 EditMode tests passed. A hidden Direct3D 11 built-player mixer check passed all 22 cues, including the clock tick. A visual screenshot at exactly five seconds was not captured; see `wave-countdown-parity.md` for source and timing details.

## October 13 - unlock popup used the install sound

**User report:** Shaoyu asked for the original unlock sound, including unlocks earned on a lost run.

**Cause:** Scrapshift played its generic install cue when an achievement toast was drawn. The end-of-run reward card had no matching sound. The original challenge_completed_ui.gd uses a dedicated popup clip and a separate popout whoosh.

**Fix:** Import the two original clips byte for byte. Play the popup when a challenge completes, including Rookie on the first loss, and play the whoosh when its toast expires. Queued challenges announce each popup. Keep the end-run reward card silent so a reward is not announced twice.

**Verification and limit:** See unlock-sound-parity.md for the source trace and hashes. The cue and queue tests check imported clips, gain, pitch, and expiry; the full build and runtime checks are recorded with the integration build. Scrapshift's 2.6-second toast duration and end-run card layout still differ from the source.
## October 14 - end-of-run inventory overlapped unlock review

**User report:** The loss summary needed to show newly unlocked rewards clearly.

**Cause:** The inventory panel occupied the horizontal space reserved for the Progress and unlock review area. The presentation review also assumed the starting SMG was the only inventory entry, which no longer held after item integration.

**Fix:** Narrow and reposition the inventory panel, accept an SMG anywhere in the starting inventory during review, and clear the review's newly unlocked state before restart. The integrated Windows build succeeded with zero errors and warnings; 387 EditMode tests passed.


## October 14 - Web release exposed local showcase mode

**User report:** The itch.io Web build should contain the normal game without the photo/video demo mode.

**Cause:** The WebGL build excluded diagnostic review controls but still compiled the title screen showcase buttons and F5-F10 shortcuts from the desktop presentation code.

**Fix:** Compile those buttons, shortcuts, and showcase command-line entry points only for non-Web builds. Keep the desktop showcase for capture work. Rebuild the WebGL release from main and replace the upload ZIP.

**Verification and limit:** Final WebGL release build succeeded with zero errors and warnings. The upload ZIP has index.html at its root. Browser UI inspection and itch.io iframe testing remain open.

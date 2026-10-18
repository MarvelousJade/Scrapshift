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

## October 17 — latest native build and support fixture repair

Built R10, ran native virtual navigation, weapon and enemy checks, and retained a support37/39 failure. Source tracing corrected unsupported Cat direction and Scapegoat immediate-rise assertions. Rebuilt R11; support39 passed both hidden and visible runs, exit zero. Fixed only staged Cat shadow placement and inspected fresh captures. Returned observer-only R10 Xbox window to foreground; no meaningful hardware actions recorded yet. Updated bug, verification and interview notes. Full campaign and whole suite remain held. See [evidence and attribution](native-gameplay-r11-2026-10-17.md).

## October 17 native reference follow-up

Parallel controls/support/contact audits were integrated. Native source-scene observations prompted a discrete-collision correction after514 passing checks;522 then passed, followed by524 after source-backed currency tests. R9 built with zero errors/warnings and visible virtual navigation passed; hidden navigation lacked UI focus/paint. Final-source native review remains unfinished after Computer Use stopped. Final selected validation: **524/524 passed**, zero failures/skips, Unity exit zero; all516 recorded inputs still match the isolated clone. Evidence: `gameplay-parity-final-checks-2026-10-17.md` and `artifacts/gameplay-parity-final-checks-2026-10-17/focused-summary.json`. No whole suite or full campaign ran. R9 is intermediate and predates final discrete-contact/starting-currency corrections. A current build, production support captures, physical Xbox verification and repaired-reference keyboard verification remain pending. Computer Use stopped when the user pressed physical Escape; no further UI input was sent.

## October 17 — integrate parallel parity fixes

Codex integrated disjoint support/weapon/enemy work and held the full suite/build/campaign. Rejected hypotheses included Predator growth (correct scene overrides), ordinary Bot mine scaling (source explicitly pet), and missing base features inferred from unregistered DLC hooks. First attempt427/434 led to evidence-backed fixture and chance-precision repairs. See `gameplay-parity-systems-2026-10-17.md`. Evidence: `artifacts/gameplay-parity-systems-2026-10-17/focused-summary.json`, both XML/log attempts, input manifest and catalog comparison. The same selected checks passed 434/434 on attempt 2, zero failures/skips, Unity exit zero. All 508 recorded input hashes still match the isolated clone after execution. No whole suite, new build, full campaign or physical-controller retest was run. R8 remains the last delivered gameplay build and predates these changes.

## October 15: finish source fixes before the full validation run

The user requested holding the whole suite and another full campaign until remaining source-audit differences are fixed. Parallel follow-up tracks identified and integrated regeneration/Max HP lifecycle, uncapped HP scaling, loot probability/coordinates/routing, item rounding/stack triggers and burn callback differences. Root added damage-order, range-extended charge and egg-hatch callback fixes. New checks are authored; execution status is tracked in docs/gameplay-parity-next-audit-2026-10-15.md. Accepted movement audio and R8 delivery are preserved. R8 results do not validate these newer changes.

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
# R6 movement audio accepted - October 15, 2026

The user confirmed: "Okay, the sound is gone." The six-footstep import/rebake correction passed 23 focused tests and built with zero errors/warnings. Physical listening acceptance now closes this reported symptom. Other controller checks and complete gameplay parity remain separate. See docs/bugs.md for the current known-issue index and docs/controller-movement-audio-2026-10-15.md for cause and evidence. Earlier checkpoints below are historical.

# R5 controller focus and movement feedback - October 15, 2026

User physical R4 testing confirmed actual left-stick/horizontalD-pad input and gameplay, but exposed A targeting Back while a character remained highlighted. R5 fixes screen/default focus lifecycle, mouse/controller handover, a single visible activation target and duplicate/blocked focus cues. Footsteps now use source .1s initial contact/.3 animation-second speed-scaled timing with approved clips/mix unchanged. Full checkpoint541/541 passed,0failed/skipped,69.4470763s. Visible native virtual-Gamepad test passed actual A-only selection->Playing, movement with0menu/3footstep requests, Startpause/Aresume and one Rookie weapon. Hidden-window attempt did not register GUI; visible acceptance is recorded separately from physical input. Build R5 succeeded401081415bytes0errors/warnings; optional helper object-lookup warning fixed afterward without ordinary gameplay changes. Evidence: artifacts/gameplay-parity-native-2026-10-15/controller-r5-build-manifest.json, focus-native-visible/navigation-check.json and observations.md. User R5 hardware acceptance remains pending.

# Final controller review R4 - October 15, 2026

Builds/GameplayParity20261015R4 is built and open using isolated in-memory settings/progression. Build succeeded with zero errors/warnings,401072035bytes. Runtime SHA2562F2C8956165804502E946CC75EF89B818F1B4B631C8F2782A6EB65D4E24C218E. Controller input uses Unity Input System1.20.1; X/Y/shoulder/trigger shortcuts and shared nonlockable-item restrictions are included. Full suite526/526 passed74.2703369s before the final bounded lock guard; latest affected controls/input/specialshop suite46/46 passed.8029584s after that guard. R4native mouse verified readable hold-reroll option On/Off/On and Escape title. Xbox device detected; physical input acceptance is pending. Evidence: artifacts/gameplay-parity-native-2026-10-15/controller-r4-build-manifest.json and observations.md. Earlier checkpoints below remain historical.

# Input System controller migration - October 15, 2026

Unity Input System 1.20.1 now handles controller input with Both input backends enabled. The final isolated suite passed 519/519, zero failed/skipped, in 70.0788143 seconds. Evidence: artifacts/gameplay-parity-native-2026-10-15/input-system-final.xml. This includes five official InputTestFixture device-event cases, six actual serialized-axis checks and the five later weapon-combine cases. Physical Xbox sticks/D-pad/aim/pause acceptance remains pending in the replacement R3 review. The earlier 503-suite and full-run binary are historical checkpoints, separately identified in docs/full-run-comparison-2026-10-15.md.

# Final gameplay suite - October 15, 2026

The isolated Unity suite passed 503/503, zero failed/skipped, in 68.5824993 seconds. Result: E:/Codex Projects/Job Search Assistant/tmp/ScrapshiftParity/artifacts/remaining-parity-release-final.xml. This includes the final collector ownership, collision and area-entry rules. The prior 502/503 run failed an exact-tangent fixture expectation; the revised fixture checks clearly inside/outside the source shape boundary. No arbitrary runtime tolerance was added. The current build and full-run evidence are documented in docs/gameplay-parity-controls-stacks-2026-10-15.md and docs/full-run-comparison-2026-10-15.md. Earlier checkpoint headers below are historical.
# Gameplay parity validation checkpoint - October 15, 2026

Current branch: codex/gameplay-parity. The isolated Unity 6000.6.5f1 suite passed 489/489, zero failed/skipped, 68.314614 seconds: tmp/ScrapshiftParity/artifacts/remaining-parity-motion-final.xml in the parent workspace. It includes controls, unlocks, stack/removal rules, special shop hooks, material cap/banking and pickup motion. Later collector ownership/geometry changes require separate validation; see docs/gameplay-parity-controls-stacks-2026-10-15.md for final scope. Fresh catalog audit: 4980/4980 values agree.

Native keyboard/mouse testing observed a settings-row navigation defect, cursor-following movement, pause/resume and a first-death unlock. Source and Unity controlled 20-wave fixtures are separate from a normal human playthrough. See docs/full-run-comparison-2026-10-15.md and artifacts/gameplay-parity-native-2026-10-15/observations.md. No hardware-controller, exact-parity, performance or WebGL claim follows from these checks.

Historical checkpoints below are preserved in their original encoding.
## 2026-10-15 - Parallel gameplay parity audit

The user requested exact gameplay behavior against the supplied Brotato project and asked to split testing so agents could work concurrently. Codex assigned disjoint economy, combat, enemy, item and support source audits, then integrated their rule fixes and a real Nightmare projectile setting. The combined isolated EditMode run found one float precision boundary (426/427); the focused retest and final full suite passed 1/1 and 427/427. The imported catalog audit passed 4,980/4,980. See gameplay-parity-parallel-workplan.md and gameplay-exactness-2026-10-15.md. Native visual timing, controller operation and human full-run parity are still open.

# Development journal

## October 11, 2026 — Original art, animation and wearable items

Inspected the twelve supplied reference screenshots. Generated original scavenger/enemy/gear/item/stat sheets and committed them before integration, with full prompts. Added separate idle/walk animation, ranged recoil and muzzle frames, five final green pickup shapes with scatter/bounce/magnet, and serialized atlas references. Retained the requested reference audio.

Runtime inspection caught a neighboring weapon fragment in the SMG import rectangle and inherited reduced-motion state in the art fixture. Corrected both and retained the first failed evidence locally. Reduced large legacy particles that obscured the new muzzle effects.

User feedback then identified repeated walking poses, backward-looking feet, the need for worn item visuals, and misleading battery loot. Added a dedicated six-pose seven-fps stride and corrected its rightward boot orientation, implemented appearance slots for all 24 items plus a shop preview, and excluded the battery from the currency pool. Decorative arms remain independent of six floating weapons. These are actual iterations, not a claim that the first generated sheet was production-ready.

Final measurements and remaining limits are recorded in verification. Source/art, integration, visual fixes, feedback changes and verification are separate Git milestones.

Closeout: 47 tests passed; AssetScope inspected 197 assets and 1,337 reference usages with zero findings. Final art fixture saw six walk frames, five material shapes, five worn items and six weapons. Normal first-wave smoke reached Shop with 24 kills and eight HP. The user chose to move on from further gait investigation; rear-view movement and full-run acceptance are explicitly left open.

Dates use America/Toronto. Entries record actual work as it occurs; commit hashes provide the sequence.

## 2026-10-10 - Foundation

**Request:** Build a small polished Brotato-inspired Unity game that works with AssetScope. The user requested strong interview documentation, real bug records and incremental Git commits.

**Inspected:** Installed Unity 6000.6.5f1 and Windows build support; AssetScope README, package manifest, scanner/index API and demo builder. Its current package is embedded separately to avoid changing the other active project.

**Implemented:** Project/package manifests, assembly boundaries, weapon/enemy data definitions and scene-independent run rules. Five one-minute waves, three upgrade choices and explicit title/playing/upgrade/paused/win/loss states.

**Not yet verified:** Unity compilation, gameplay, rendering, tests or player build. These are upcoming milestones, not completed evidence.

## 2026-10-10 - Combat and authored content

Implemented the three weapons, three enemy definitions, bounded arena, scrap collection, menu/HUD, pause/sound/reduced-motion settings, original robot mesh prefabs and deck artwork. The content generator creates normal serialized Unity assets and preserves existing content on repeated calls.

The first import exposed invalid input-package configuration (DEV-001). After removing it, Unity compilation/content generation completed with exit code 0 and AssetScope scanned the generated content successfully. No Windows gameplay or visual quality result is claimed yet. Those checks follow in the next milestone.

The user additionally requested real visual references and Brotato code research. Art sources and what they influenced are in `art-references.md`; code research is being kept separate from claims about the original game's implementation.

## 2026-10-10 - Automated rules and content checks

Added and ran 21 EditMode cases; all passed. Checks cover the five-wave timeline, stopped simulation during pause/upgrades, invalid/repeated selections, lethal damage precedence, restart reset, bounded movement, swept projectile collision, real serialized weapon references, shared material reverse references, complete scene configuration, prefab content and repeat-generation GUID preservation.

Evidence: `artifacts/editmode.xml` and `artifacts/raw/tests.log`. This proves the specified checks, not complete gameplay balance or manual input quality. Windows build and rendered smoke follow separately.

## 2026-10-10 - Player verification and real fixes

The Windows build succeeded with zero reported build errors or warnings. A 24-second scripted run exercised attacks, enemy deaths, pickups, damage and an upgrade, but its claimed screenshot success was invalid. Batch mode saved no image; a hidden window saved a black image. Both failed visual verification. QA-002 records the defective check and the corrected evidence boundary.

Code inspection also found that a resizable square window could crop world space while keeping the HUD fitted. Added a common aspect-ratio calculation and three regression cases. The resulting 24-case suite passed, and the player rebuilt without reported warnings/errors.

Opened the player as an interactive preview. Title art and layout were inspected. Desktop automation verified Return starts the run, a D tap moves the bot, Escape pauses/resumes, a mouse click changes reduced-motion state, the 1 key installs orbit blades, and a mouse click installs the pulse core. Observed automatic combat, scrap collection, damage, an in-wave level-up and readable upgrade UI. These were assistant-operated checks, not claimed user playtests.

The first wave completed at 90 kills. Choosing Field Repair changed hull from 52/100 to 107/115 and advanced to wave two at 60 seconds. The preview was left waiting at a later in-wave upgrade choice. Reduced motion remained enabled from the control check and can be changed in the pause menu.

## 2026-10-10 - Delivery baseline

Prepared a Windows ZIP and recorded executable/archive SHA-256 hashes. Added architecture/tradeoffs, source research, AssetScope walkthrough, verified-results matrix, a personal playtest worksheet and common interview questions. Kept invalid screenshot evidence in ignored raw artifacts; committed selected real interactive screenshots. No public repository push, release publication or external upload was performed.

Incremental history reflects real milestones: foundation -> combat/content -> tests -> observed fixes -> documentation/evidence. No backdated commits, fabricated debugging incidents or synthetic claim of a human-authored history.

## 2026-10-10 - Responding to the art and game-feel critique

The user rejected the placeholder character/enemy art, UI, upgrades and weak effects, and supplied `E:/UnityProjects/Brotato` as a local reference. Inspected its potato sprite, unit hit/knockback/flash processing, range-keeping movement behavior, and upgrade-container tier/reroll wiring. The directory contains a GDRE export; this is user-provided reference material, not evidence of an official open-source release. No source or art was copied into Scrapshift.

Replaced the navy robot presentation with original outlined scavenger/creature mesh prefabs and an olive yard. Added a visible pistol, charger windup/dash, brute knockback resistance, hit flashes, floating damage, crits, hit-stop, directional knockback, colored muzzle/hit/death bursts and damage-border feedback. Rebuilt the HUD and menu hierarchy with larger type, chunky frames and rarity-coded upgrade cards. Separated presentation and feedback into partial-class files; this improves file organization but is not an architectural decoupling claim.

The first validation found DEV-004: MaterialPropertyBlock creation in a MonoBehaviour field initializer logged a native-construction error during scene loading. The test suite rejected it even though BuildPipeline called the build successful. Lazy runtime initialization fixes the lifecycle violation; the build gate now also rejects nonzero reported errors. After the fix, 27 tests passed and the build reported zero errors/warnings. Retained the rejected test XML and build log under ignored raw artifacts.

Art and behavior were committed as separate actual milestones. Final pistol/spawn-marker polish, interactive verification and packaging follow those commits.

Final v2 delivery: 27 tests passed; 35 assets scanned with no prefab missing scripts; Windows build zero reported errors/warnings. Visible scripted combat passed and the generated screenshot was independently inspected as real game content. Updated Windows ZIP and hashes. Kept full-run balance and subjective audio evaluation open. Saved before-fix evidence for the lifecycle issue and actual upgrade-card screenshots; no personal user playtest is claimed.

## 2026-10-10 - Reference-informed sound bank and current AssetScope integration

User clarified that the next critique was sound design. Replaced four synthesized runtime beeps with 36 authored PCM variants across 15 cues and a bounded mixer. Added dedicated damage, critical, death, blade, pulse, charger, reroll and reward sounds; cooldowns, non-repeating variants, stereo placement, pickup chains, reserved notification voices, ducking, and saved volume. Kept character art unchanged after explaining that its red strip is a scarf.

The user also asked whether the finished tool could be used in this project. Read the companion chat and current package; upgraded the embedded AssetScope copy from 0.1.0 to 0.3.0 at b7aac0df08a31f7ae951792c34e440163ef42d34. Enabled saved-scene/property inspection in the game scan. Verified 72 assets, 1,202 reference usages, no findings, and a real sound-bank source-navigation test. All 30 game tests passed. The original tool project was not changed.

Compilation caught AUDIO-006, a Unity 6 importer API move. Corrected it and retained the failed log. Audio source assets and tool upgrade were committed as separate milestones before the final integration/evidence commits.

Final audio validation: all 15 cue categories produced sampled Unity mixer output and mute suppressed playback. The 24-second gameplay check completed with 28 kills, 67 shots and no logged error; this particular run did not reach an upgrade. Generated listening sampler and refreshed Windows ZIP. Subjective listening approval is still pending.

## Crash Zone reconstruction — systems milestone

Analyzed the user-supplied reference before changing runtime. Added a provenance-tracked catalog with 20 wave schedules, 18 base-difficulty enemy entries, 24 equipment tier entries (six families), and 24 stat items. Replaced interrupted combat leveling with queued rewards and a separate shop. Added six concurrent weapons, duplicate combinations, recycling, paid rerolls, persistent locks, enemy projectiles, charge tells and visual corpses. Integrated sprites/audio as serialized catalog dependencies visible to AssetScope. First build succeeded with zero warnings/errors; 33 EditMode tests passed. Initial hidden-window screenshot failed and remains a separate verification issue. Assistant implementation; Shaoyu provided design direction and reference assets.

## Reference reconstruction — visual correction milestone

Rendered screenshots caught a white birth warning and clipped upgrade titles. Corrected the explicit tint and layout, imported the eight actual stat icons/tier tables, and added a final-boss completion test. Separated the original hidden-window failure from subsequent rendered evidence. Refined crowd separation and cleared frozen combat text at intermission. Added an opt-in, clearly labeled review fixture for combat, shop and upgrade rendering; it grants equipment/currency/health and does not count as a normal playthrough. Source tests now pass 35 cases. Further measured runtime results are recorded in verification.

## Final input and output evidence

Actual mouse/keyboard shop check verified the displayed Coffee tradeoff, purchase-at-capacity combination, paid R reroll with a preserved lock, recycling refund and Enter transition. Recorded exact observed values in verification. Normal first-wave smoke passed separately without health overrides, and isolated audio emitted all 15 cues with mute suppression. AssetScope inspected 183 assets and 1,388 serialized reference usages with zero findings. Full-run balance and the user's subjective assessment remain open.


## Enemy spawning and UI reference pass

The user asked for better enemies/spawning/combat and a closer interface, briefly narrowed to five waves, then chose twenty again. Retained the twenty-wave schedule and reduced ordinary shop items to eight. Committed the supplied-reference comparison before implementation, then spawn/attack-state fixes separately from UI and evidence.

Source comparison found formation-breaking retries, dropped queued births, repeat scheduling differences and discarded ranged knockback. Added bounded formation-preserving placement and independently testable attack states. Actual UI captures exposed faint combat HUD bleed-through behind the shop; corrected draw routing and recaptured. Initial player-launch argument quoting failed before the fixture started; corrected path quoting, then all final player fixtures completed.

Measured closeout: 53/53 tests; Windows build zero warnings/errors; AssetScope 198 assets, 1,337 usages, zero findings. Normal first wave reached Shop. Staged enemy and UI fixtures passed. Accelerated invulnerable integration traversed all twenty waves to Won and made 44 affordable shop purchases. It does not prove a normal human can finish the game or approve its feel. Audio assets and deferred upward walking art remain unchanged.


## October 11 — Source parity, rather than assuming resemblance proves correctness

Compared the supplied scene nodes, movement/attack scripts, spawner, XP and item service before editing; committed the audit first (`e1d9bdf`). Corrected progression (`eaaece9`), enemy behavior/spawn queues (`3770d0b`), and shop selection/pricing (`c927682`) in separate increments. The source revealed bugs in our previous intentional adaptations, including locked ranged aim and extra recovery, so those earlier design notes are explicitly superseded. Original art/media files were retained.

The user opened the project in Unity during the task. Validation ran in a separate copy instead of closing their Editor or trying to batch-open the same project. The first restricted launch could not access host licensing; the host-permitted launch succeeded. Validation scope and measured final results are recorded in `verification.md`, with source-comparison defects in `bugs.md`. No complete human playthrough or subjective sound review is implied.

## Weapon sets — source comparison, rules, UI, evidence

The user requested Brotato-style weapon-set bonuses for our existing weapons, visible UI, continued documentation and incremental Git commits. Inspected counting/application code, every included weapon tier's membership, bonus resources and synergy-panel rendering before implementation. Committed that audit as `d125e52`.

`92c8bc6` adds derived Gun/Precise/Primitive bonuses, shared combat/display range, transaction health clamping and 15 regression cases. Separating permanent stats prevents temporary effects from becoming permanent. The first run's sole failure was a test's stale HP expectation after Harvesting earned a level; recorded and corrected it.

`03a11fe` adds current set counts, bonuses, membership labels, threshold details and a dedicated rendered fixture. Final checks: 98 tests passed, Windows build zero warnings/errors, AssetScope zero findings, all 569 validation inputs byte-equal. Independently inspected three active sets, Primitive threshold details, 30-to-27 HP after combining and the following upgrade screen. The fixture forces hover state; manual input and normal balance are not established. Original content assets remain unchanged. Evidence/documentation form the final commit; no twenty-wave rerun or user acceptance is claimed for this revision.

## Loot and survival — largest remaining loop gaps

User requested another source comparison, prioritized missing gameplay, existing art, documentation and incremental commits. Ranked crates/survival ahead of expanding character/item breadth. `fbf98c3` records the comparison before code. `264c6f7` implements crate reward rules and survival timers with eighteen regression cases. `b257eea` integrates source-based drops, pickup/cleanup, take-or-recycle UI, a rendered fixture and existing campaign/review compatibility.

Both Unity test runs passed 116 cases; the final build reported zero errors/warnings. The dedicated loot fixture passed all assertions, and the accelerated current-build campaign reached Won after twenty waves. The normal smoke died in wave one despite its combat-coverage Passed label; kept that evidence and documented the limitation. Original content assets are byte-unchanged. Final reports, screenshots, interview notes and verified package are recorded separately; no human playtest is claimed.

## Body-only art and spring movement

User asked for no enemy hands/legs, no player hands, tiny feet and a slower idle/faster movement spring, and asked to see Brotato's sprites. Inspected and displayed actual supplied body/foot/enemy PNGs plus source animation resources. Committed the comparison as `e634814`, then edited two original atlases with built-in imagegen. Implemented a child-body spring and separate tiny feet, with wardrobe following the body and weapons independent.

Initial 119 tests passed, but rendered review revealed sprite clipping from the atlas grid. Added a border regression; its first run caught adjacent tree pixels in the spitter region (119/120). Corrected source rectangles and reran: 120/120, clean Windows build, final enemy/player/outfit captures and AssetScope zero findings. Committed implementation as `7f2219b`. Earlier bitmaps remain, and final documentation records source-inspired motion separately from exact parity and user acceptance.


## October 11 - modular player and clean visual direction

Compared supplied appearance resource, display-priority and player composition code first (audit commit 69aa925). Replaced baked-in character identity with a shared shell, swappable eyes/hair/beard, tiny pointed legs and 24 separate wearable details. User compared a clean preview and explicitly selected removal of weathering across items/enemies and all player shell/leg panel lines. Implemented clean art and appearance composition in 2aac926.

Initial 126/127 test runs exposed wing/glasses atlas cuts; alpha inspection also rejected an enemy draft with a translucent backdrop. Final import uses measured per-cell transparent separators, verified by border regression. Final 127/127 tests, clean build, AssetScope 211 assets/1395 usages/zero findings, and 625 byte-equal validation inputs. Rendered three looks/outfits, all 18 enemies and staged six-weapon combat; exact evidence/limits in verification.md. Preserved the open Editor and unrelated PackageManagerSettings.asset.


## October 11 - wider outward-curved legs

User clarified the desired leg spacing/curve after rejecting the first pointy-leg arrangement. Compared supplied legs.png and mirrored leg scenes. Generated one curved original leg, mirrored left/right, increased separation from .26 to .62, raised attachment to meet the lower body corners, and reduced walk swing. Kept shell/faces/items/enemies intact. Build and all 127 tests pass; appearance fixture standing/right/left/shop passed and screenshots inspected. New asset prompt and hash are documented in modular-appearance.md and wide-leg-prompt.txt.


## October 11 - outlines, proportions and shadows

Compared bare body alpha bounds and scene transforms in the supplied Brotato reference, then implemented a smaller gameplay player, larger first-wave enemy, bolder curved-leg sprite and owned world-space oval shadows. Visual scaling is separated from collision rules. Commit 0c58968 includes the new serialized dependencies and rendering fixture. Initial shadow lifecycle regression failed in EditMode (128/129); ExecuteAlways fixed callback delivery and the final suite passed 129/129. Build and AssetScope checks pass; staged scale and combat images were inspected. Exact results and scope limits are in verification.md; prompt/provenance is in outlined-leg-prompt.txt and modular-appearance.md. No full-run balance or manual acceptance claim.


## October 11 � running Godot reference and a consistent world scale

Inspected the recovered Godot 3.6.2 reference in the running title/selection screens and Danger 0 first wave. Short observation runs ended in death; no successful playthrough is claimed. Compared source map/camera data, body and weapon texture bounds, attachment slots and spawn placement with our existing Unity rendering. Found that movement/range used 100 reference pixels per unit but the arena and visual sizes used unrelated dimensions. Added the reference-sized tall arena, bounded follow camera, per-enemy/weapon scale calibration and count-specific weapon mounts. Original PNGs are unchanged. Documentation separates exact measurements from pickup/pivot/UI adaptations. See live-reference-comparison.md and verification.md.


## October 11 - combat collision, push and hit feedback

Compared a live stationary Danger 0 pistol sample and the Godot enemy/unit/player/projectile scenes. The source separates physical spacing, incoming hits and outgoing contact; our shared radius did not. Corrected all enemy profiles and lower-body offsets, fixed the knockback import/runtime conversion and damping, added source-style slowdown after contact, adjusted protection timing, and resolved projectiles in travel order across the full restored map. Added original procedural impact stars/player flashes and removed global hit-stop. See combat-feel-comparison.md for exact evidence, adaptations and test scope. No artwork was replaced.


Combat verification concluded with 143/143 tests, clean Windows build, zero AssetScope findings, controlled combat and attack fixtures, and scripted 20-wave integration. Hidden screenshot failures exposed a validation bug; a second implementation commit requires fresh PNG evidence. Final visible renders passed. The normal-speed scripted smoke died in wave 1 at 16.728 seconds and remains a failed survival check, despite its legacy activity-based Passed field. See verification.md for exact metrics and package hash.


## October 11 - curved yard, weapon rests and run presentation

User requested closer matching of the map boundary, ground details, weapon placement, thicker legs, menus and parachute entrance. Compared the live paused reference (left intact) and Godot attachment/landing/menu resources. Implemented original native yard/canopy geometry, horizontal weapon rests with family-specific pivots, 45% wider legs, a character/weapon/start flow, a 1.2-second first-wave arrival, and loss/win plus inventory summary screens. Code and source comparison are in fdfd5ff and yard-and-run-presentation.md.

Rendering exposed a bright jagged rim, inherited reduced-motion settings, a staged enemy frozen at birth scale, and a validation sync restoring an old scene under a new migration marker. Fixed the visuals/fixture and made migration skipping conditional on actual floor/parachute references. The saved-scene regression and runtime presence checks cover the latter. Final 148/148 tests, clean build, AssetScope 225 assets/1841 usages/zero findings, and thirteen verified captures. No new PNG art, full-run balance claim or user acceptance.


## October 11 - complete UI pass, silver legs and recoil

Inspected live title, selection, landing, loss and summary screens plus local source UI/weapon resources. Removed Enter and movement legends, rebuilt selection interaction and pause/end inventories, added all primary stat icons, replaced only the requested silver rounded limb art, revised the native canopy and corrected body-only landing squash. Kept original characters/enemies/items/weapons and small gameplay catalog. Ranged recoil/cooldown is a separate incremental commit (0525286). See ui-and-motion-comparison.md for scope and deliberate differences; verification.md for results. The older player at a wave-three upgrade is preserved; the refreshed build is separate.

## October 11 - outward silver leg stance

Corrected the horizontal silver limb mounting with a mirrored 110-degree resting angle and a slightly higher attachment. Existing limb pixels, width and outlines are unchanged. Kept the swing relative to the resting pose, corrected both facing directions, migrated the saved prefab and shared the shop/selection portrait renderer. Render inspection exposed a separate GUI mirror/rotation order error; explicit matrix composition now matches the world sprites. The final regression suite passes 158/158, including facing, mirror-order, dimensions and reduced-motion checks. Staged captures are in artifacts/hit-leg-review; source/evidence details are in player-hit-and-leg-correction.md.

## October 11 - player-hit feedback and audio correction

Followed the reference player scene and damage signal through sound, flash, particles, floating text and camera shake. Replaced the wrongly selected zombie voice with the four player impact samples, preserved source-style flash/protection timing, removed invented border/blinking, and authored open-center impact streaks with timed red fragments. No character/enemy/item bitmap changed. Verification includes source audio hashes, serialized bank assertions, actual mixer output, staged contact/recovery captures and the final 158-test suite. The updated player is Builds/HitRefresh/Scrapshift.exe; older build folders are retained. This is source/render verification, not user listening approval or a human survival playthrough.

## October 11 - four gameplay profiles and fullscreen

Implemented the user's clarified roster: Random, four playable reference-derived profiles under new names, 46 locked slots. Imported full starting-choice lists and numerical weapon data, generated one original weapon atlas, connected character modifiers and extra starting weapons, and added fullscreen controls. Replaced cosmetic-only selection with gameplay previews. Preserved original body/enemy art and reference audio provenance. Controlled runtime checks cover all available starter families and core effects; full-run balance remains unverified. See fullscreen-and-starter-characters.md for source mapping, debugging history and remaining adaptations.


## October 11 - reference typography and inspectable equipment

Compared source selection, stat popup and weapon-description code. Hid locked weapon choices, retained character locks, imported the reference Anybody font and selected English templates, and added pointer/keyboard inspection across pause and all reward/shop screens. Original art remains. Corrected scroll hit targets, empty turret scaling, percent formatting and capture sequencing. Final 172-test/build/render/native-input evidence is in verification.md; scope and interview discussion are in inspection-and-weapon-selection.md.

## October 11 - expanded items and reference presentation

Expanded the obtainable pool from 8 to 64, preserving original art and documenting the 127 unsupported special-effect items. Compared live character/weapon UI and source parachute keys, moved set inspection beside weapons, corrected character modifier rows and bonus-weapon tray, and rebuilt the canopy. Debugging exposed appearance loss during migration, an incorrect health-test assumption and concave mesh triangulation. See item-expansion-and-design-parity.md and verification.md.

## October 11 - run feedback and rewards

Implemented two-weapon starts for all four profiles, pre-range aiming, green tree warnings, removed ranged warning lines, animated material banking and exposed pending rewards/harvesting. Reworked weapon actions and crate cards. See run-feedback-and-rewards.md.

## October 12 - completed catalog and progression

Completed the last 16 items, all 50 character profiles and saved challenge rewards, using the supplied Godot resources/scripts. Implemented companion actions, wave replay and difficulty encounters. User screenshots guided full-width shop/upgrade cards, inline icons, tier/buff colors, sorting, two-row weapons and Harvesting-only secondary HUD. Retained cream mascot and cybernetic presentation. Actual debugging and final evidence are in bugs.md and verification.md.


## October 12 - measured presentation and final boss correction

Audited both final bosses and all 62 weapon families from the supplied source. Replaced Invoker persistent orbs with animation-timed pillars, corrected diameter-based scatter, its 24-pillar ring and +150% speed mutation. Checked Predator charge/volley and restored body-relative projectile placement. Removed extra tier stripes/rims, rounded cards, added shop currency icons and special-effect bullets, replaced pale ground polygons with solid stones, and restored spawn-warning audio. Visual QA caught price wrapping; fixed before final capture. See presentation-and-final-bosses.md.


## October 12 - upgrade and inventory feedback

Applied the supplied screenshots: common upgrade borders, dark reroll buttons with F and currency, removal of level-up inventory, face/arrow and wooden-crate rewards, dedicated white inventory hover/audio, centered currency, plain Items heading, separator and an inspectable character tile. Shared character description formatting and enlarged inline effect icons/bullets. See upgrade-and-inventory-ui.md.

October 12 follow-up: matched upgrade/reward, shop inventory, pause and HUD presentation against supplied screenshots. Removed offset card shadows; added character ability inspection, distinct inventory focus audio, a pause Codex and dot timeline. See upgrade-and-inventory-ui.md for decisions and verification limits.

October 12 styling follow-up: removed the second black edge beneath tier borders, darkened menus and icon tiles, capped pause inventory at six columns, widened pause controls, and increased both HUD outlines from four to six units. Rebuilt and captured the normal runtime UI for visual review.

October 12: fixed square white inventory hover corners without changing sprites or tile dimensions. Shared inspection outline now follows the rounded tile for pointer and keyboard focus.

October 12: user screenshot revealed a thin dark seam remained inside rounded selected tiles. Removed the dark base and duplicate border for white focus surfaces; added pause-character coverage. No running Scrapshift process was available to confirm the user executable path.

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

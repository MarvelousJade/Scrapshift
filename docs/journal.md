# Development journal

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

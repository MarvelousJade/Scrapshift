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

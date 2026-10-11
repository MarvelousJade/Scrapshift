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

# Bug and investigation log

Record observed behavior, expected behavior, reproduction conditions, cause, fix and regression evidence. Deliberate test fixtures must be labeled; they are not accidental production bugs. Development blockers are distinguished from gameplay defects.

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

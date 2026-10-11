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

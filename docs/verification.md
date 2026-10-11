# Verification record

## Latest results - October 11 combat feel and capture verification

Implementation `b2404ce`; capture-validation correction `f7adcbf`. Source/live comparison, adaptations and remaining gaps are in combat-feel-comparison.md. Original PNG artwork is unchanged.

- **143/143 EditMode tests passed**, zero failed/skipped, 2.161 seconds. Eleven added cases cover collision profiles/offsets, knockback force and frame-rate equivalence, contact suppression/slowdown, damage-dependent protection, swept-hit ordering, map-edge projectile retention and rejection of missing/invalid/stale screenshots.
- **Windows build succeeded**, zero reported errors/warnings, 144,172,980 reported bytes. **AssetScope: 217 assets, 1,398 usages, zero findings.** All **665** Assets/Packages/ProjectSettings files match isolated validation byte-for-byte, digest `3d13494ccbf2bf873a313c70f72115f3c63006bffa02b38c4bb1bd36a7542257`. Unsaved Editor state is outside this comparison. No Editor was closed.
- **Controlled combat runtime passed**, all nine gameplay assertions plus fresh captures: near/far piercing damage 100/50,1.05426 units of actual enemy displacement over an explicit0.1s step, no contact at x=.40, contact at x=.35, contact slowdown, second-hit protection and enemy/player white flash. Protection for 1 HP lost out of 10 was .266667s. Three fresh PNGs verified; final knockback render inspected. Fixed enemies/HP/shots and stepped simulation make this integration/rendering evidence, not a playthrough.
- **Enemy attack fixture passed**, fresh captures verified: spawn warnings, charge/ranged preparation, committed dash and hostile projectile;6 charge releases,1 ranged release,4 kills after supplied six-weapon loadout. Final combat capture inspected. Player health was restored by the fixture.
- **Accelerated twenty-wave integration passed**, Won, 20 completed waves, 48 purchases, peak 107 enemies, 823 charge releases,1,069 ranged attacks and one boss phase change, with 16 enemy IDs observed (not all 18). All six fresh captures verified; wave20 inspected. The fixture uses supplied tier-two gear, invulnerability and scripted movement; 23.628 wall seconds is not normal completion time or a performance benchmark.
- **Normal-speed scripted smoke exercised combat but failed survival:** Lost at 16.728 simulated seconds in wave 1,20 kills,14 shots, peak9 enemies,0 HP and no upgrades. Final fresh gameplay image inspected (8/10 HP at capture). Its legacy Passed field only indicates combat activity plus a screenshot; do not call it a first-wave clear. No health override was used. Ordinary human difficulty/contact-feel acceptance remains open.
- **Actual validation defect corrected:** hidden launches produced 3/5/6 screenshot failures while gameplay-only reports said Passed. Prior PNGs in an output folder were not proof of a new render. CaptureVerification now rejects missing, invalid and stale images; final three fixture reports require capturesVerified=true. Evidence is retained in artifacts/combat-capture-recovery/hidden-results.json. Final player logs contain only the existing D3D12 info-queue diagnostic0x80004002 among case-insensitive Error/Exception/Failed matches, with no screenshot failures.
- **Package verified:** 155 entries,49,639,685 bytes, CRC and every-file comparison passed. SHA-256 `e70f10c24006331a3b3156df38b04e13470a91f56d3bdc2eecbfcec8e312409f`.

Reproduce with scripts/verify.ps1 and scripts/build.ps1 in the isolated project, then rendered player flags `--combat-feel-review`, `--enemy-review`, `--campaign-check` and `--smoke`, each followed by its output folder. Rendering needs a game window; a hidden launch is not a valid screenshot run. Raw logs remain under ignored artifacts/raw; current reports/images are in combat-feel-review, combat-enemy-review, combat-campaign and combat-smoke. Pre-existing unrelated settings and earlier untracked presentation evidence remain untouched.


## Latest results - October 11 live-reference world and scale comparison

Implementation commit `04b5f37`. Compared short live first-wave samples and inspected the Godot 3 source backup; see live-reference-comparison.md for measured values and adaptation limits. No original PNG artwork changed.

- **132/132 EditMode tests passed**, zero failed/skipped, 2.152 seconds. Coverage includes full-height map bounds and spawn coverage, camera follow/clamping, and weapon family/tier dimensions and mounts.
- **Windows build succeeded**, zero reported errors/warnings, 144,162,740 reported bytes. **AssetScope: 217 assets, 1,398 usages, zero findings.** All **657** Assets/Packages/ProjectSettings input files match isolated validation byte-for-byte, digest `63eb61b3d427c191868ea39925e25eb48e5a2849feb9403125301bd4572ad1e2`. Unsaved Editor state is outside this comparison.
- **Rendered world review passed:** four-direction travel reached x 10.24 and y 7.68; camera travel 2.58/4.21 includes shake; 115 shots, peak 13 enemies, four captures. Inspected single-weapon, six-weapon/outfit and map-edge renders. The fixture restores health and supplies equipment; this is not purchase or balance validation.
- **Accelerated 20-wave integration passed:** Won, 20 completed waves, 50 purchases, peak 102 enemies, 1,061 charge releases, 1,290 ranged attacks, one boss phase change, 17 observed enemy IDs. Slasher was not selected in this run. The fixture uses eight 50ms simulation steps per rendered frame, supplied six tier-two weapons, scripted movement, seeded spawning and invulnerability. Its 23.803 wall seconds is neither normal completion time nor a performance benchmark.
- Player logs retain the known D3D12 info-queue diagnostic `0x80004002`; no new exceptions observed. Ordinary human balance/contact-feel acceptance remains outstanding. Collision radii are unchanged by this visual pass.
- **Package verified:** 155 entries, 49,635,354 bytes, CRC and every-file comparison passed. SHA-256 `4dbe7417d8a287e2ccca93c1b87406138efe8c9c6193e7bc71e09a15f71a9745`.

Evidence: artifacts/world-review, artifacts/world-campaign, artifacts/editmode.xml, artifacts/build.txt, artifacts/assetscope-report.json, artifacts/validation-copy.json and artifacts/package.json. Reproduce with scripts/verify.ps1 and scripts/build.ps1 in the isolated validation project, then player flags `--world-review <folder>` and `--campaign-check <folder>`. Main Editor remained open. Pre-existing unrelated settings and earlier untracked presentation evidence remain uncommitted.

## Latest results - October 11 proportions, outlines and shadows

129/129 EditMode tests passed (2.221s), including actor-size, leg-version and shadow ownership regressions. Initial run: 128/129, Editor disable lifecycle failure; retained in artifacts/scale-shadow-review/initial-editmode.xml and explained in modular-appearance.md. One retry encountered Unity's project-open guard after the build; no process was killed, and the later run passed.

Windows build succeeded with zero reported errors/warnings, 144,157,956 bytes. AssetScope: 216 assets, 1398 usages, zero findings. All 647 Assets/Packages/ProjectSettings input files match isolated validation, digest `ef4a1b4b0c813b0b88faff7da3e1c2d560c7b0e73b640f4020b0d1223ed1d978`. Code/art commit `0c58968`. Unity-generated metadata contains its usual trailing spaces; source changes were reviewed separately.

Inspected artifacts/scale-shadow-review/gameplay-scale.png and outline-and-shadow.png: actual player/basic-enemy renderers, neutral width ratio 1.187, normal camera 7.5 and detail camera 2.8. Staged art combat also passed (33 shots, five material shapes, six weapons, five visible item attachments; recoil, muzzle flash and both movement directions observed). Inspected walk-right.png. The art fixture supplies equipment and restores health; it is not normal-run balance evidence. Player logs retain only the known D3D12 info-queue diagnostic among Error/Exception/Failed matches. The main Editor was not closed by this work.

Package: 49,633,564 bytes, 155 entries, CRC and every-file comparison passed. SHA-256 `2e84e742407c686fa34d1be8d812ed3ac1efd6b3f74034c2049443c336f8ed5e`. Hitboxes are unchanged; larger enemy-silhouette contact feel still needs user playtesting.


## Latest results - October 11 outward-curved leg correction

127/127 tests passed (2.017s); Windows build succeeded, zero errors/warnings, 144,148,516 reported bytes. AssetScope: 213 assets, 1395 usages, zero findings. All 631 input files match isolated validation. Inspected standing, left/right walking and title/shop images from artifacts/wide-leg-review; the fixture passed. Only the existing D3D12 info-queue diagnostic appeared in the player log. This is visual verification, not a new combat/balance run.

The new player-leg-v1 sprite is mirrored and spaced at +/-0.31. Seven original PNGs are active. Code/art commit bd9f92f. Package: 49,575,500 bytes, 155 entries, CRC and every-file comparison passed; SHA-256 `2093aec6407ee668eadd1440acfd69a3ca84aefc69e0b1c084b8f8cd44a71416`.


## Latest results - October 11 clean modular appearance

- **127/127 EditMode tests passed**, zero failed/skipped, 1.831s. Seven added tests cover priority/duplicate precedence, item override/restoration, same-count replacement, preset refresh/mirroring, cosmetic/stat separation, leg motion and alpha borders. Initial failures are retained and explained in modular-appearance.md.
- **Windows build succeeded**, zero errors/warnings, Unity 6000.6.5f1, 137,856,260 reported bytes. **625 input files match** the isolated copy byte-for-byte; digest `63c7288d54f61ef11968a568ec8025ec948fa197b48815adec3e24168c88a74f`. The user's Editor remained open; unsaved state is outside this comparison.
- **AssetScope: 211 assets, 1,395 usages, zero findings** for saved content. Six clean original PNG dependencies; historical textured/reference sprites are not active scene PNGs. This verifies serialized inspection coverage, not arbitrary runtime completeness.
- **Appearance rendering passed**: three shared-body looks, three supplied outfits, left/right spring motion, title LOOK selection and shop composition. World/resolver agreement and face mirroring are true. Inspected final looks-and-items, title-veteran and shop-layers captures in artifacts/clean-appearance-review.
- **All 18 enemy renderers inspected** in artifacts/clean-enemy-gallery; fixed grid, presentation animation only. Not an AI/balance test.
- **Staged combat art fixture passed**: 39 shots, five scrap shapes, six weapons, five visible item layers, left/right movement, muzzle flashes, recoil, scatter, settling and magnet attraction. One body sprite per state, idle spring range .15 and walk range .30. Inspected outfit screenshot; artifacts/clean-art-review/art-review.json. This fixture supplies equipment/items and heals the player.
- Player logs retain the known D3D12 info-queue startup diagnostic `0x80004002`; no other Error/Exception/Failed matches. All players exited successfully. No new twenty-wave balance run or manual input acceptance is claimed.
- **Package verified**: 155 entries, 49,356,996 bytes, CRC passed and every archived file matches the build. SHA-256 `00b933ff2982e92eda8d16801024fdc2d9b6c283030ca5ab8aa17ca977ee339b`.

Reproduce: scripts/build.ps1 then scripts/verify.ps1 in the isolated project; compare with scripts/verify_validation_copy.py. Player flags: --appearance-review, --spring-gallery, --art-review, each followed by an output folder. Generation prompts and final asset hashes are in docs/clean-art-prompts.json, clean-art-repair-prompts.json and clean-atlas-regions.json. The pre-existing PackageManagerSettings.asset remains uncommitted.


## Latest — October 11 body-only spring animation

- **120/120 EditMode tests passed**, zero failed/skipped, 1.158s test duration. Source pose landmarks, slower idle/faster walk, bounded alternating feet, pause, reduced motion, serialized body/foot references and sprite-border integrity are covered. Old six-frame assertions were replaced with the new requirement. The added border check caught one real atlas defect before passing; `artifacts/raw/spring-border-first-tests.xml` retains the ignored failed report.
- **Windows build succeeded, zero errors/warnings**, Unity 6000.6.5f1, 131,521,308 reported bytes. Validation ran in the isolated copy without closing the user's Editor. **595 files match byte-for-byte**; `artifacts/validation-copy.json`. Only the generated catalog/prefab/scene, atlas metadata and migration marker were brought back from that copy; unsaved Editor state is outside validation.
- **AssetScope: 201 assets, 1,355 usages, zero findings**, saved scenes/properties inspected, cache disabled. Five PNG sheets are active scene dependencies; older art remains for history. All eighteen enemy entries reference the new body-only sheet; player body/feet and wardrobe references are serialized. Weapons, pickup art and supplied audio are unchanged.
- **Final art fixture passed:** one shared body sprite for idle/walk, measured horizontal spring ranges 0.15 / 0.30, both facings, five wearable attachments, six floating weapons, 37 shots; muzzle/recoil, five scrap shapes, scatter/settle/magnet observed. `artifacts/spring-art-final/art-review.json`. This grants equipment/items/health and uses scripted movement with a close-up camera. Frame counts no longer claim the visual quality of a gait.
- **All-enemy gallery inspected:** `artifacts/spring-gallery-final/` uses eighteen actual enemy renderers at fixed positions and the real presentation function; no AI combat or balance is exercised. Compared the initial clipped gallery with corrected silhouettes. Player idle/walk/outfit and final shop preview were also visually inspected; no user acceptance is claimed.
- Both final player logs contain only the known D3D12 info-queue diagnostic among Error/Exception/Failed matches; rendering completes. Raw logs stay ignored. The new atlas-border failure was corrected; neither its report nor the earlier clipped screenshots are called final passes.
- **Package verified:** 155 entries, 48,301,015 bytes, CRC passed and every file matched the Windows folder. SHA-256 `c6b4a1a5a5450e229d01197b02e4738af42dcd1db5b0aabf05ae351e1ac404b2`.

Reproduce with `scripts/build.ps1` (runs the versioned migration), `scripts/verify.ps1`, then `--art-review <folder>` and `--spring-gallery <folder>` on the rendered Windows player. Reduced-motion behavior was unit-tested, not manually accepted. Smooth interpolation/enemy cadence and simple feet are adaptations; no rear-view gait is supplied. The earlier twenty-wave fixture and first-wave smoke loss belong to the prior gameplay build, not a new balance claim. Pre-existing `ProjectSettings/PackageManagerSettings.asset` remains uncommitted.

## Previous — October 11 loot and survival loop

- **116/116 EditMode tests passed**, zero failed/skipped, 1.050s test duration; `artifacts/editmode.xml`. Eighteen added cases cover source drop probabilities, luck/dilution/guarantees, take/recycle ordering and effects, restart/loss, regeneration timing, pause, full-health reset, life-steal cooldown and attraction. Both test runs passed.
- **Windows build succeeded, zero errors/warnings**, Unity 6000.6.5f1, 137,850,956 reported bytes. All **581 files** in Assets, Packages and ProjectSettings match the isolated validation project byte-for-byte; `artifacts/validation-copy.json`. The user's Editor was not closed; unsaved state is not covered.
- **AssetScope: 198 assets, 1,337 usages, zero findings**, saved scenes/properties inspected, cache disabled. All original content assets are unchanged from `344d3f5`. Runtime procedural crate geometry uses the existing serialized sprite material; serialized scans do not prove arbitrary runtime references.
- **Rendered loot integration passed**, all seven assertions true; `artifacts/loot-review/loot-review.json`. Actual looter death generated a crate, pickup healed, wave cleanup collected the offscreen crate, crates preceded upgrades, take added an item, recycle paid the displayed refund, and the shop was reached. Independently inspected ground-crate, take-or-recycle and upgrade-after-crates screenshots. The fixture supplies a looter, scripted lethal damage, a second crate, XP and invulnerability; actions are scripted, not manual input or normal balance.
- **Current 20-wave integration passed:** Won, 20 completed waves, 54 affordable purchases, peak 80 enemies including births, 765 charge releases, 700 ranged releases, one boss phase change. `artifacts/loot-campaign/`. This uses supplied six tier-two weapons, scripted movement, invulnerability and accelerated simulation; 23.22 wall seconds is not a normal completion time or performance benchmark. The reward handler now resolves crates before upgrades. The separate loot fixture establishes actual crate-phase coverage; the campaign report does not count opened crates.
- **Normal-speed smoke: combat exercised, wave-clear failed.** The run ended Lost at 15.30 simulated seconds, 17 kills, 13 shots, peak eight enemies, zero upgrades and zero HP. `artifacts/loot-smoke/runtime-smoke.json`. Its legacy Passed field requires shots/kills/enemies and a capture, not survival; do not describe it as a successful first-wave clear. The gameplay screenshot was independently inspected (2/10 HP before death). No health override was used. Human balance testing remains necessary.
- All three player logs contain the existing D3D12 info-queue startup diagnostic (`0x80004002`); no other Error/Exception/Failed matches. Rendering and shutdown completed. Raw logs stay ignored under `artifacts/raw/`.
- **Package verified:** 155 entries, 51,762,882 bytes, CRC passed and every archive file matched the Windows build. SHA-256 `05ed54df1f56ec3b4af15e8b258fdd2362939e868ff3f4b924f84cbb4df12bd1`; `artifacts/package.json`.

Reproduce: `scripts/verify.ps1`, `scripts/build.ps1`; rendered player flags `--loot-review <folder>`, `--campaign-check <folder>`, `--smoke <folder>`. Crate take/recycle mouse and keyboard interaction has not been manually exercised. The reduced eight-item reward pool falls back at missing tiers; full legendary/special crate effects, character selection, broader stats/items, difficulties and endless remain absent. No complete normal human run or acceptance is claimed. Pre-existing `ProjectSettings/PackageManagerSettings.asset` stays uncommitted.

## Previous — October 11 weapon-set bonuses

- **98/98 EditMode tests passed**, zero failed/skipped, 0.905s test duration; `artifacts/editmode.xml`. Fifteen added cases exercise membership, count thresholds, transactions, permanent-stat separation and HP clamping. The first 97/98 run exposed a test assumption about Harvesting/level HP, corrected before the final run; see `bugs.md`.
- **Windows build succeeded, zero errors/warnings**, Unity 6000.6.5f1, 137,842,252 reported bytes; `artifacts/build.txt`. The isolated validation project matches all **569 files** in Assets, Packages and ProjectSettings byte-for-byte; `artifacts/validation-copy.json`. Unsaved Editor state is outside this comparison.
- **AssetScope: 198 assets, 1,337 usages, zero findings**, cache disabled, saved scenes/properties inspected. Content assets, including original art and existing audio, are unchanged from `38b44c4`.
- **Rendered weapon-set fixture passed:** 113 shots, 55 kills, 53 melee hits; combat/shop/upgrade capture flags and both set assertions passed. `artifacts/weapon-set-review/review.json`. Independently inspected shop, threshold, after-combine and upgrade screenshots. Two weapons in each set activate all three bonuses; combining spears removes exactly 3 Max HP (30 to 27) and leaves the other sets active. The following earned level correctly adds 1 HP separately.
- Fixture scope: supplied tier-two equipment, wave-nine start, restored health, currency, scripted combine and forced threshold panel. It validates rendered output and runtime integration, not manual hover/input, normal balance or user acceptance. Reproduce with `Scrapshift.exe --reference-review <folder> --weapon-set-review`.
- Player log contains one graphics startup diagnostic: `d3d12: failed to query info queue interface (0x80004002)`. Rendering and fixture completion succeeded; no other Error/Exception/Failed matches. Raw log: ignored `artifacts/raw/weapon-set-review.log`.
- **Package verified:** 155 entries, 51,759,570 bytes, CRC passed and every archived file matches the Windows build. SHA-256 `ecbd329420b10a35cbad1dccb2f64543c410c168c19c3569fe2c3492f00bedc2`; `artifacts/package.json`.

Current limits: three sets represented by our six weapon families; no all-weapons-count character modifier or additional sets. The reduced item/upgrade catalog, compact arena, different physics/RNG, missing characters/crates/endless and approximate slash/material geometry remain. The twenty-wave campaign evidence below belongs to the previous build; it was not rerun for this revision. No complete normal human run or new subjective audio review is claimed. The pre-existing `ProjectSettings/PackageManagerSettings.asset` remains outside our commits.

## Previous — October 11 gameplay parity correction

- **83/83 EditMode tests passed**, zero failed/skipped, 1.019s reported test duration. `artifacts/editmode.xml`. Tests now assert source-based XP/rarity boundaries, milestone rewards, charge and shooter timing, queue ordering/119–120 threshold, negative harvest, non-repeating upgrades and locked prices.
- **Windows build succeeded, zero errors/warnings**, Unity 6000.6.5f1, 137,838,668 reported build bytes. Tests/build ran in `E:/Codex Projects/Job Search Assistant/tmp/ScrapshiftParity` while the user's Editor remained open. `scripts/verify_validation_copy.py` confirmed all **565 files** in Assets, Packages and ProjectSettings matched this checkout byte-for-byte; `artifacts/validation-copy.json`. Unsaved Editor state is outside this check.
- **AssetScope: 198 assets, 1,337 usages, zero findings**, saved scenes/properties included, cache disabled. All `Assets/Scrapshift/Content` files remain unchanged from pre-pass commit `f185c85`; original sprites and existing audio are retained.
- **Final 20-wave integration passed:** Won, 20 completed waves, 48 affordable purchases, peak 63 enemies including births, 718 charge releases, 518 ranged releases, one observed Predator phase change. Report/screenshots: `artifacts/parity-campaign-final/`. This fixture uses scripted movement, six supplied tier-two weapons and invulnerability. Its 23.13-second wall time is accelerated test execution, not game completion time or a performance benchmark. The source-equivalent active-enemy culling threshold may temporarily be exceeded between group events.
- **Normal-speed first-wave smoke passed:** reached Shop, 24 kills, 18 shots, one chosen upgrade, peak 11 enemies, 1 HP remaining. `artifacts/parity-smoke/runtime-smoke.json`. No health override; it is scripted movement, not a user playtest. Independently inspected the saved gameplay image. Ending with 1 HP does not establish comfortable difficulty.
- **Combat presentation fixture passed:** warning, charge windup, dash, ranged windup and hostile projectile observed; five charges, two ranged releases and three kills. `artifacts/parity-enemies/`. Captured before the final shop-only corrections; combat code is unchanged. Charge/projectile images independently inspected.
- **Final rendered UI fixture passed:** gameplay, shop and upgrade captures; 135 shots, 39 kills, 25 melee hits. Independently inspected `artifacts/parity-ui-final/shop.png` and `upgrade.png`: original icons, readable multi-line titles, corrected COMMON/UNCOMMON/RARE labels and effect text. This staged fixture supplies equipment, health and currency.
- No Error/Exception/Failed match in the four player logs for combat, final campaign, normal smoke and final UI. Raw logs remain under ignored `artifacts/raw/`.
- **Package verified:** 155 entries, 51,757,988 bytes; CRC check and every archived file matched the Windows build. SHA-256 `a76dca7a8782e652155fed877f696015ff40df0c0619ef9752e33228312f4bc6`; `artifacts/package.json`.

Limits: a reduced catalog, compact arena, different physics/RNG, missing set bonuses/characters/crates/endless, and approximate material attraction/slash geometry remain. Slasher is not scheduled in the base campaign and its actual runtime hit window was not exercised by these fixtures. No complete normal human run, subjective sound review or user acceptance is claimed. The user-created `ProjectSettings/PackageManagerSettings.asset` was left outside our commits.

## Previous enemy, spawn and UI pass

- **53/53 EditMode tests passed**, zero failed/skipped. Six new cases cover locked attack direction, windup/recovery/release timing, accelerated repeat intervals, bounded edge/cluster clearance, and the retained twenty-wave progression with an eight-item shop pool. Existing all-wave frame-size stability, economy, art and AssetScope tests remain green.
- Windows build **Succeeded, zero errors/warnings**, Unity 6000.6.5f1; `artifacts/build.txt`.
- AssetScope: **198 assets, 1,337 serialized usages, zero findings**, saved scenes/properties inspected and cache disabled. Runtime-created direction lines/projectiles are outside serialized dependency discovery; their material comes from the catalog.
- `artifacts/enemy-review/enemy-review.json`: **Passed**; spawn warning, charge/ranged preparation, recovery and hostile bullets observed; six charge releases, one ranged release, three kills. Fixed enemies, restored health, initially unarmed player and later injected equipment; not normal balance. Inspected charge, ranged and projectile screenshots.
- `artifacts/combat-ui/review.json`: **Passed**, 154 shots, 47 kills and 35 melee hits; actual combat, shop and upgrade screenshots inspected. Staged wave-nine showcase with supplied equipment/currency and restored health. The new shop no longer leaks the combat HUD; stat effects and calculated weapon values fit the cards.
- `artifacts/combat-smoke/runtime-smoke.json`: **Passed**, normal first wave reached Shop with 25 kills, 18 shots, peak eleven enemies and six HP remaining. Scripted normal-speed movement, no health override. Actual gameplay screenshot inspected.
- `artifacts/campaign-check/campaign-check.json`: **Passed**, twenty waves reached Won, 44 affordable purchases, peak 63 enemies, 568 charge releases, 468 ranged releases and sixteen distinct enemy ids observed. Five sampled wave images and final win capture saved; late-wave images inspected. This accelerates actual simulation with invulnerability, a supplied six-weapon loadout and mute. It validates integration through the finale, **not normal-run balance, human completion or a performance benchmark**. Cumulative shots/kills/times are explicitly per-wave snapshots in the report.
- No Exception/Error/Failed match in the four final runtime logs. Supplied audio assets unchanged; isolated audio/mixer validation was not rerun in this pass.
- Package checksum/CRC and comparison against every current Windows-build file are recorded in `artifacts/package.json`.

Reproduce with `scripts/verify.ps1`, `scripts/build.ps1`, then visible player flags `--enemy-review <folder>`, `--reference-review <folder>`, `--smoke <folder>` and `--campaign-check <folder>`. The fixtures do not alter stored user preferences. `scripts/package.py` rebuilds and verifies the ZIP.

Remaining limits: no complete normal twenty-wave human playthrough or user acceptance of this combat/UI revision. The compact arena, aim-locked shooter windup, direction cues, 0.18-second recovery and adapted final boss are intentional departures; exact Brotato parity is not claimed. Eight item types are offered; all 24 historical catalog records retain their sprite/wardrobe mappings. Upward player movement still uses front-facing art as previously deferred by the user. Older results below are historical.

## Previous original-art iteration — October 11, 2026

- **47/47 EditMode tests passed**, zero failed/skipped. New coverage includes six-pose timing, stable facing across idle transitions, reward conservation at the pickup cap, scatter/settle/magnet behavior, wearable-slot selection, exclusion of the misleading battery, and serialized atlas references.
- Windows build **Succeeded, zero reported warnings/errors**, Unity 6000.6.5f1. `artifacts/build.txt`.
- AssetScope: **197 content assets, 1,337 serialized usages, zero findings**, saved-scene/property inspection enabled and cache disabled. Six active generated PNG sheets are scene dependencies; historical reference PNGs are not.
- Art fixture: four idle frames, six walk frames, both horizontal facings, five material shapes, recoil/muzzle/scatter/settle/magnet states, five worn items and six floating weapons observed. `artifacts/original-art-review/art-review.json`; inspected direction and outfit/shop captures. This is staged with injected equipment/items, restored health and a following close-up camera. It does not test real purchasing or normal-run balance.
- Normal first-wave smoke: **Passed**, 24 kills, 18 shots, peak nine enemies, eight HP remaining, reached Shop. Scripted normal-speed movement, no health/invulnerability override. `artifacts/original-art-smoke/`.
- Combat/shop/upgrade presentation fixture passed; captures in `artifacts/original-art-ui/`. This is a staged showcase, not a complete playthrough. No runtime exception/error was found in the three final player logs.
- Audio files/mixer remain unchanged from the previous verified reference-audio pass; sound-bank integration tests passed in the current suite. The isolated mixer test was not repeated for this art-only change.
- Package with `python scripts/package.py`: CRC validation plus byte comparison of every archived file against the Windows build; result/hash in `artifacts/package.json`.

### Interpretation and remaining limits

User feedback rejected earlier gait quality despite passing frame-count checks. Those counters prove state coverage, not convincing animation. The sheet was revised and slowed to seven fps, then corrected for inconsistent boot direction. The user elected to move on without identifying the movement direction involved. Left/right art is mirrored correctly; **upward movement still uses front-facing art**, with no dedicated rear-view cycle. Visual acceptance and a complete human 20-wave playthrough remain open.

All 24 items have appearance mappings, with newest-per-slot visibility and all owned stat effects retained. Five attachments were visually checked together; every outfit combination has not been manually reviewed. The wardrobe uses sprite attachments, not skeletal clothing. Historical records below describe prior versions.

### Reproduce the current art checks

Run `scripts/verify.ps1` and `scripts/build.ps1`. Launch the player visibly with `--art-review <folder>`, `--reference-review <folder>`, or `--smoke <folder>`. The art fixture disables reduced motion only in its process, leaving saved preferences unchanged. Captured frame sequences are local/ignored; selected evidence is tracked.

## Previous reference reconstruction — October 10, 2026

- **35/35 EditMode tests pass**, with zero failed/skipped. Covers all 20 scheduled wave durations, frame-size-independent group counts, reward queuing, shop costs, lock persistence, atomic purchases, full inventory, duplicate combinations, recycling, every included item effect, reference upgrade tiers, boss completion, imported media, and AssetScope navigation.
- Windows build **Succeeded, zero reported errors/warnings**, using Unity 6000.6.5f1. Source and selected evidence are tracked; binary output is under `Builds/Windows`.
- AssetScope 0.3.0: **183 content assets, 1,388 serialized reference usages, zero findings**. Saved scenes and properties inspected, cache disabled. This validates serialized references in its documented scope, not all runtime behavior.
- Normal first-wave smoke: **Passed**, 23 kills, 18 shots, peak 10 enemies, 11 HP remaining, reached Shop. Scripted movement with no health/invulnerability override. Independently inspected the actual `artifacts/reference-smoke/gameplay.png`; no runtime exception reported. This is a first-wave check, not a complete normal run.
- Rendered showcase: combat, four-offer shop and four-choice upgrades captured and inspected. The fixture starts at wave 9 with six tier-two weapons, supplies currency and restores health; it is explicitly not a balance test. Initial white warning/clipped heading defects were corrected. See `artifacts/reference-review/`.
- Audio: **15/15 cues emitted nonzero mixer output**, mute suppression passed, two peak simultaneous voices, sampled peak about **0.173**. `artifacts/reference-audio/audio-runtime.json`. This does not establish subjective listening quality or certify continuous peak behavior under a dense fight.

### Reproduce the new evidence

Use `./scripts/verify.ps1` and `./scripts/build.ps1`. Launch the player visibly for screenshot checks; hidden windows on this host can fail to render a capture. Optional flags: `--smoke <folder>` for the normal first wave; `--reference-review <folder>` for staged combat/shop/upgrades; add `--review-interactive` to stop the staged fixture in the shop for input inspection; `--audio-check <folder>` for isolated mixer checks. Run audio checks without another instance of this game.

### Current limitations

No complete 20-wave human playthrough, user acceptance, performance benchmark, controller/rebinding or non-Windows verification. Only base difficulty is recreated; roster, arena geometry, contact/ranged details and one boss are bounded adaptations. No claim of complete Brotato parity. Imported art/audio are third-party reference assets, with source hashes in the manifest. See `docs/reference-rebuild.md` for exclusions.

The older records below document earlier versions. Their five-wave, three-weapon, all-original-audio and lower test-count descriptions are superseded by this section.


Verified October 10, 2026 (America/Toronto), Unity 6000.6.5f1, Windows on this machine. This is a portfolio prototype baseline, not a shipped production certification.

## Original v1 results (historical)

| Layer | Actual result | Evidence / limits |
| --- | --- | --- |
| Unity project compilation | Passed | Content generation exited 0 after DEV-001 fix |
| EditMode checks | **24 passed, 0 failed, 0 skipped** | `artifacts/editmode.xml`; 18 rule/geometry/aspect cases and six content-integration cases |
| AssetScope | **33 content assets scanned; 0 missing-script findings** | `artifacts/assetscope-report.json`; scope is direct dependencies and prefab missing scripts, not complete serialized-reference or scene validation |
| Windows build | **Succeeded, 0 reported errors, 0 reported warnings** | `artifacts/build.txt`; player output `Builds/Windows/` |
| Scripted runtime | Combat, kills, health, pickups and an upgrade observed | Initial 24-second run: 29 kills, 64 shots, peak nine enemies, one upgrade, 76 hull. Its overall pass label was rejected because screenshot validation failed; raw evidence remains in `artifacts/raw/` |
| Actual interactive rendering | Title, combat, pause, upgrade and wave-clear layouts inspected | `artifacts/preview/`; real rendered images, unlike rejected hidden-window captures |
| Input/control check | Return, D movement tap, Escape pause/resume, mouse reduced-motion toggle, keyboard orbit upgrade and mouse pulse upgrade verified | Assistant-operated desktop automation, not claimed user playtesting |
| Wave transition | Wave 1 completed, combat cleared, repair chosen, wave 2 began at 60 seconds | Observed 90 kills; hull changed from 52/100 to 107/115 after repair plus wave recovery |

## Reproduce

Close any Unity Editor using this game project before batch commands. The independently opened AssetScope project can remain open. From Scrapshift:

```powershell
./scripts/verify.ps1
./scripts/build.ps1
```

Open `Builds/Windows/Scrapshift.exe` for the interactive game. No Steam account, network service or paid asset pack is needed to play this Windows build.

An optional `--smoke <absolute-output-folder>` flag drives 24 seconds of scripted movement and writes counters and a screenshot request. On this host, hidden/batch capture produced missing or black images. A runtime report is not accepted as visual evidence without independent image inspection. The final visual baseline uses the actual interactive build.

`--preview-capture <absolute-output-folder>` saves one title screenshot shortly after launch and leaves the game interactive. It does not drive player input.

## Original v1 visual observations (superseded)

At 1440x900, title copy and start button fit without overlap; the robot silhouette is readable; the dark deck stays behind mint projectiles, coral enemies and gold scrap. HUD labels remain legible, upgrade descriptions fit their cards, and keyboard shortcuts are visible. The current simple style is intentionally geometric. Initial title overlay dims the showcase bot more than the gameplay art; that is a potential art iteration, not a missing texture.

## Remaining limits

- A complete five-wave human playtest and final-wave balance have not been verified. The rules suite covers all wave transitions, but that is different from surviving the full game.
- No controller/rebinding, save progression, localization, screen-reader navigation, performance benchmark or non-Windows player validation.
- Sound now uses an original layered PCM effects bank. Subjective audio quality still needs a user listening check.
- Default-size rendering was inspected. Other aspect ratios have three mathematical regression cases, not a full manual display matrix.
- The smoke screenshot check now requires file existence, but rejecting black/incorrect images still requires the separate visual check. This boundary is explicit.
- Tests check swept collision and progression, but are not exhaustive coverage of every runtime weapon/enemy interaction.
- AssetScope is now the embedded 0.3.0 snapshot, integration-tested on the game content; see the latest results below.

## Evidence provenance

Raw logs and rejected runs are local under ignored `artifacts/raw/`. XML/JSON summaries and selected real screenshots are committed. `docs/contributions.md` records who performed which kind of work. Never claim an unperformed personal test, production incident or user-study outcome.

## Presentation v2 verification

- 27 EditMode tests pass, including unique randomized offers, the one-reroll limit, exclusion of unlocked weapons and tier-to-damage agreement. Content tests include the serialized pistol reference.
- Windows build succeeds with zero reported errors/warnings. AssetScope scans 35 content assets with no missing-script findings. The extra assets are the version marker and pistol prefab; existing prefab/material GUIDs are preserved by in-place art migration.
- Real desktop inspection: new title/character art; gameplay enemies, projectile feedback and damage numbers; large readable upgrade cards; R changes offers and shows REROLL USED; key 1 selects the displayed Field Lunch and changes health from 64/100 to 104/115. A later draft visibly offered Rare Scavenger (+1.30 pickup range), Epic Heavy Rounds (+45% damage), and Pulse Core.
- Screenshots under `artifacts/preview-v2/` are assistant-captured from the actual player. The upgrade screenshots precede the final pistol/spawn-marker polish; those changes do not alter the cards.
- The desktop check also revealed UPGRADED! wrapping in a damage-number-sized rectangle. Widened the rectangle only for longer labels before final packaging.
- Full-run balance, subjective sound quality and user acceptance remain unverified. No automatic test can certify that the redesign is fun or attractive.

Final v2 runtime check: a visible 24-second scripted run completed with 28 kills, 65 shots, peak 10 enemies, one upgrade, one orbit hit and 100 health. No exception/error was found in its player log. Independently opened `artifacts/smoke-v2/gameplay.png` and verified actual rendered pixels: outlined pistol and creatures, coral particle debris, orange 24! critical damage and white ordinary damage text, spawn ring and HUD. This run's visual evidence is valid, unlike the rejected original hidden captures. It does not exercise the later-wave charger/brute balance or pulse weapon.

## Audio and AssetScope 0.3.0 revision

- **30/30 game tests pass**, including the real sound-bank reference navigation integration.
- **72 assets / 1,202 serialized reference usages / zero findings**, with saved-scene and property inspection enabled. Embedded tool source: `b7aac0df08a31f7ae951792c34e440163ef42d34`.
- Windows build **Succeeded, zero errors and warnings**. Updated build/archive hashes are in `artifacts/build-hashes.json`.
- **36 generated samples / 15 cue types** pass numerical peak/RMS/end-point checks. Source generation is reproducible with `scripts/generate_audio.py`; no reference WAV was copied.
- Actual runtime audio check passed: all 15 isolated cues produced nonzero sampled Unity mixer output; mute suppressed playback. Sampled peak was 0.201. This is not a claim to have listened, a continuous mix clipping test, or a busy-wave benchmark. See `artifacts/audio/audio-runtime.json` and the offline listening sampler.
- The audio-enabled 24-second scripted gameplay check passed with 28 kills, 67 shots, peak ten enemies and 100 health. It collected scrap but reached no upgrade in this run. No exception/error was found in the player log. Its screenshot was independently opened and verified as real game rendering; upgrade/reward audio was covered by the separate isolated cue check.

### Actual shop input check

Assistant-operated Computer Use on the rendered final Windows player, using the opt-in staged shop fixture:

- Coffee purchase: materials 536 -> 489; attack speed 5 -> 15; damage 0 -> -2; item count 0 -> 1.
- Matching tier-two SMG at six slots: materials 489 -> 406; the existing SMG became tier three; inventory stayed at six.
- Keyboard R: materials 406 -> 397; reroll price 9 -> 12; the locked tier-three shotgun persisted while other offers changed.
- Selected and recycled the tier-two pistol: materials 397 -> 409; inventory 6 -> 5; previously blocked shotgun purchase became available.
- Enter started wave 10 at 60 seconds and restored health to 23/23 with the modified loadout.

This verifies actual GUI input in a staged run, not a full human balance playthrough. The staged windows were closed after inspection.

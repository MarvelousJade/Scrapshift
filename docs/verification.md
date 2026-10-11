# Verification record

## Current reference reconstruction — October 10, 2026

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

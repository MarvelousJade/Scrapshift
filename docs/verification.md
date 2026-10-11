# Verification record

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
- Sound is implemented with synthesized tones; subjective audio quality has not been independently assessed.
- Default-size rendering was inspected. Other aspect ratios have three mathematical regression cases, not a full manual display matrix.
- The smoke screenshot check now requires file existence, but rejecting black/incorrect images still requires the separate visual check. This boundary is explicit.
- Tests check swept collision and progression, but are not exhaustive coverage of every runtime weapon/enemy interaction.
- AssetScope is the embedded 0.1.0 snapshot, not the concurrently developed newer version.

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

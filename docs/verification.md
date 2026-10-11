# Verification record

Verified October 10, 2026 (America/Toronto), Unity 6000.6.5f1, Windows on this machine. This is a portfolio prototype baseline, not a shipped production certification.

## Results

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

## Visual observations

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

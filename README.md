# Scrapshift

A Unity arena-survival study built around the supplied Brotato Crash Zone reference: 20 scheduled waves, warned enemy groups, six equipped weapons, stat upgrades and a separate item/weapon shop. Six original AI-generated art sheets supply the animated scavenger, enemies, weapons, pickups and UI icons. The supplied reference sound effects remain.

## Why this exists

The Unity Editor developer role asks for C#, Unity project experience, automated testing and attention to creators' workflows. AssetScope is the companion Editor tool. This playable project supplies real scene, prefab, material, sprite, audio and data references for inspecting dependencies and navigating to their owners.

The user requested close reference analysis before implementation. See [the source study](docs/reference-rebuild.md), [original art and full prompts](docs/original-art.md), and [historical media provenance](artifacts/reference-media-provenance.json). Reference audio is third-party work supplied by the user. Historical borrowed sprite files remain in the repo but are no longer game-scene dependencies. This is not a claim of official source access or complete game parity.

## Play and develop

Unity **6000.6.5f1**, Windows. Open this directory as a Unity project, then `Assets/Scrapshift/Content/Scenes/Deck.unity`. **Scrapshift > Generate Game Content** applies versioned content migrations while preserving existing asset GUIDs.

- WASD / arrows: move. Weapons attack automatically.
- Escape: pause/resume. M: mute. Pause menu: volume and reduced motion.
- Collect materials for both spending and XP. Levels queue until the wave ends.
- 1–4 or mouse: choose a stat reward. R: paid reroll.
- Shop: buy items/weapons, lock offers, select weapon slots to combine or recycle. Six slots maximum; buying a matching tier at capacity combines it.
- Enter or GO: start the next wave. Enter also starts/restarts a run.

Run `Builds/Windows/Scrapshift.exe`; keep its entire Windows folder together. Archive: `Builds/Scrapshift-Windows.zip`.

Run `./scripts/verify.ps1` and `./scripts/build.ps1` with this project's Editor closed. If it is open, copy `Assets`, `Packages`, `ProjectSettings` and `scripts` to a separate validation project and run those scripts there. Do not start a second Editor on the same project folder. The October 11 checks used `E:/Codex Projects/Job Search Assistant/tmp/ScrapshiftParity`; compare source hashes before attributing its results to this checkout. Binaries/raw logs are ignored; source, Unity metadata, test reports and selected visual evidence are tracked.

## Scope

20 base-difficulty schedules; 18 enemy entries, including trees and a final boss; six weapon families in four tiers; eight offered stat-tradeoff items (24 retained catalog entries); eight upgrade stats using reference tier values. Predator now switches from charging to random projectile bursts at half health or 45 seconds and carries its rotating projectile arm. Higher difficulties, alternate bosses, all characters/items, weapon-set stat bonuses, endless mode, DLC and full balance parity are not implemented. The reference culling threshold is 100 active regular enemies at group boundaries; pending births, trees and bosses are separate. The arena and collision model remain adapted to this presentation.

## Interview evidence

- [Purpose and scope](docs/design.md)
- [Reference analysis](docs/reference-rebuild.md)
- [Enemy, spawn and UI comparison](docs/enemy-spawn-iteration.md)
- [Latest gameplay parity audit and remaining differences](docs/gameplay-parity-audit.md)
- [Generated art, animation and pickup design](docs/original-art.md)
- [Development journal](docs/journal.md)
- [Observed bugs and fixes](docs/bugs.md)
- [Interview rehearsal](docs/interview.md)
- [Contributions](docs/contributions.md)
- [Architecture](docs/architecture.md)
- [AssetScope demonstration](docs/assetscope-demo.md)
- [Verification and limits](docs/verification.md)
- [Hands-on playtest](docs/playtest.md)

The latest measured results are in the verification record. A scripted presentation fixture is explicitly separate from a normal run and from the user's own playtest.

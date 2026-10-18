# Scrapshift

A Unity arena-survival study built around the supplied Brotato Crash Zone reference: 20 scheduled waves, warned enemy groups, six equipped weapons, stat upgrades and a separate item/weapon shop. Original AI-generated sheets provide player parts, wearable details, body-only enemies, weapons, pickups and UI icons. The cream human-like mascot uses cybernetic implants, swappable eyes/hair/beard and rounded silver tiny legs. A slow idle spring and faster movement spring animate the body. The supplied reference sound effects remain.

## Why this exists

I started Scrapshift because I enjoy Brotato and wanted to improve my ability to make games in Unity and practice C#. Rebuilding the gameplay with my own characters and visual direction gives me a concrete way to learn, experiment and improve through a playable project.

AssetScope is the companion Editor tool. Scrapshift also supplies real scene, prefab, material, sprite, audio and data references for inspecting dependencies and navigating to their owners.

The user requested close reference analysis before implementation. See [the source study](docs/reference-rebuild.md), [original art and full prompts](docs/original-art.md), and [historical media provenance](artifacts/reference-media-provenance.json). Reference audio is third-party work supplied by the user. Historical borrowed sprite files remain in the repo but are no longer game-scene dependencies. This is not a claim of official source access or complete game parity.

## Play and develop

Unity **6000.6.5f1**, Windows. Open this directory as a Unity project, then `Assets/Scrapshift/Content/Scenes/Deck.unity`. **Scrapshift > Generate Game Content** applies versioned content migrations while preserving existing asset GUIDs.

- Start: Random or Rookie / Pugilist / Hothead / Sharpshot, then an eligible starting weapon. Characters have distinct reference modifiers and a fixed weapon plus your chosen weapon. Remaining character slots are locked.
- WASD / arrows: move. Weapons attack automatically.
- Escape: pause/resume. M: mute. Pause menu: Gameplay, Audio, Video and Controls tabs.
- Collect materials for both spending and XP. Levels queue until the wave ends.
- Crates heal 3 HP and queue a free item at wave end. Take with Enter or recycle with X; remaining crates are collected before level rewards.
- 1–4 or mouse: choose a stat reward. R: paid reroll.
- Shop: buy items/weapons, lock offers, select weapon slots to combine or recycle. Six slots maximum; buying a matching tier at capacity combines it.
- Weapon sets: duplicates count; tiers count equally. Gun adds Range, Precise adds Critical Chance, Primitive adds Max HP. Hover the set rows for all thresholds.
- Enter or GO: start the next wave. Enter also starts/restarts a run.

For screenshots and video, choose **Martial Monk**, **Cyberpunk Ranger**, **Auto Turret**, or **Druid Pets** on the title screen. Every showcase starts at wave 19 with six weapons, visible worn items, 18 nearby enemies, a continuing crowd of up to 36 enemies, and an invulnerable player. The normal game HUD is shown without a showcase bar; the wave 19 timer loops while the showcase continues. F6/F7/F9/F10 switch builds, F5 resets the current build, and F8 hides or restores the HUD. The showcases do not award progression.

The showcase Windows player is in `Builds/Showcase` (keep the full folder together). Source tests and the build passed, but repeated local Direct3D 12 runs reported a GPU device error during capture checks; runtime graphics stability remains unresolved on this machine.

Run `Builds/ConditionalImplants/Scrapshift.exe`; keep its entire player folder together. Archive: `Builds/Scrapshift-Windows.zip`.

Run `./scripts/verify.ps1` and `./scripts/build.ps1` with this project's Editor closed. If it is open, copy `Assets`, `Packages`, `ProjectSettings` and `scripts` to a separate validation project and run those scripts there. Do not start a second Editor on the same project folder. The October 11 checks used `E:/Codex Projects/Job Search Assistant/tmp/ScrapshiftParity`; compare source hashes before attributing its results to this checkout. Binaries/raw logs are ignored; source, Unity metadata, test reports and selected visual evidence are tracked.

## Scope

20 base-difficulty schedules; 18 enemy entries, including trees and a final boss. All 62 base weapon families / 205 tier resources are registered; 43 families / 148 tiers are currently obtainable. 119 of 209 base item definitions are implemented, 98 obtainable under the source unlock flags. Fifteen weapon sets and four gameplay profiles are supported. Cybernetic aliases preserve reference gameplay values and IDs. No DLC content.

90 items and six explicitly gated weapon families remain unfinished; the locked Rail Gun also lacks its special behavior. Incomplete shop pools cannot reproduce full-catalog probabilities. Difficulty progression, full character unlocks and human balance acceptance remain outside the current verified scope. See [conditional implants and special weapons](docs/conditional-implants.md) for this pass's reference comparison and limitations.

## Interview evidence

- [Purpose and scope](docs/design.md)
- [Reference analysis](docs/reference-rebuild.md)
- [Enemy, spawn and UI comparison](docs/enemy-spawn-iteration.md)
- [Remaining gaps, loot and survival comparison](docs/loot-sustain-audit.md)
- [Weapon-set comparison and implementation](docs/weapon-sets.md)
- [Latest gameplay parity audit and remaining differences](docs/gameplay-parity-audit.md)
- [Body-only sprite and spring-animation comparison](docs/spring-animation.md)
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

Latest appearance design and reference comparison: [modular appearance](docs/modular-appearance.md).


Latest visual pass: bolder curved-leg outlines, a 15% smaller gameplay player, larger basic enemies and simple oval ground shadows that stay flat during spring animation. Details and source comparison: [modular appearance](docs/modular-appearance.md); validation: [verification](docs/verification.md).

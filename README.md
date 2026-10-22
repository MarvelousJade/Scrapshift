# Scrapshift

<p align="center">
  <a href="https://youtu.be/vxglIsqjHaU">
    <img src="https://img.youtube.com/vi/vxglIsqjHaU/hqdefault.jpg" alt="Watch Scrapshift gameplay on YouTube" width="480">
  </a>
  <br>
  <a href="https://marvelousjade.itch.io/scrap-shift?password=scrapshift">
    <img src="https://img.shields.io/badge/Play_Demo-itch.io-FA5C5C?style=for-the-badge&amp;logo=itchdotio&amp;logoColor=white" alt="Play Demo on itch.io">
  </a>
</p>

A cyberpunk arena-survival roguelite built in **Unity and C#**, inspired by [Brotato](https://store.steampowered.com/app/1942280/Brotato/). Fight through 20 waves, turn collected materials into upgrades, and assemble a build of weapons, cybernetic items and companions.

I started Scrapshift because I enjoy Brotato and wanted to improve my Unity development skills and practice C#. It grew into a portfolio project exploring gameplay systems, asset tooling and automated validation, with its own characters and visual identity.

## Gameplay highlights

- **Move, dodge and survive.** Weapons fire automatically so you can concentrate on positioning, with optional manual aiming for more control over targeting.
- **Choose a different playstyle.** 50 character profiles offer distinct abilities, stat tradeoffs and starting equipment, including companion-focused and weaponless starts.
- **Build a multiweapon loadout.** Explore 62 weapon families, combine matching weapons into stronger tiers, and use weapon-set bonuses. Standard characters have six weapon slots; character rules can change capacity.
- **Turn loot into decisions.** Materials provide both XP and spending currency. Choose stat upgrades, then buy, reroll, lock or recycle offers between waves.
- **Experiment with synergies.** A catalog of 209 items supports stacked effects, stat scaling, turrets and companions. Unlocks and character restrictions shape what each run can offer.
- **Survive 20 escalating waves.** Face scheduled enemy groups, elites and bosses, balancing immediate survival against investment in later waves.
- **See your build evolve.** Cybernetic equipment appears through modular wearable layers, while achievements and persistent unlocks open more choices for later runs.

## Game and UI design

The design alternates busy combat with deliberate build decisions. Automatic attacks let players focus on positioning; wave-end rewards and shops create space to compare upgrades and plan the next wave.

- **Visual identity:** illustrated cybernetic characters, clean silhouettes, bold outlines and distinct equipment icons. Modular wearables make acquired items visible on the character.
- **Menus and character selection:** an illustrated title screen leads into character portraits, ability descriptions, records and run options, with terrain providing a sense of place.
- **Combat HUD and feedback:** prominent health and XP bars, currency and wave information keep run status readable. Pickup particles, attack telegraphs and contextual sounds communicate events during combat.
- **Shop and rewards:** offer cards, rarity colors, stat icons and tooltips support comparison. Separate price, reroll and lock controls sit alongside sortable inventory and weapon capacity information.
- **Pause, Codex and settings:** pause combines run controls with inventory and wave progress; the Codex presents unlock requirements and rewards. Settings group gameplay, audio, video and controls.

Explore the implementation in [GameUI](Assets/Scrapshift/Runtime/GameUI.cs), [ShopUI](Assets/Scrapshift/Runtime/ShopUI.cs) and [CodexUI](Assets/Scrapshift/Runtime/CodexUI.cs), or read about the [wearable design system](docs/modular-appearance.md).

## Engineering highlights

- **Gameplay rules separated from presentation.** [RunRules](Assets/Scrapshift/Runtime/RunRules.cs) manages inventory, economy and run transitions; [SpawnDirector](Assets/Scrapshift/Runtime/SpawnDirector.cs) processes scheduled enemy groups. This supports focused tests alongside playable integration checks.
- **Reference-driven debugging.** Source comparisons and isolated fixtures cover starting equipment, stacked effects, shop transactions and defeat/retry sequencing. See the [architecture](docs/architecture.md) and [bug investigations](docs/bugs.md).
- **Measured performance work.** Profiling identified allocations in repeated combat lookups. Targeted changes reduced allocations in the synthetic per-tick diagnostic by 99.27%; this is not an FPS measurement. [Investigation and evidence](docs/late-wave-allocation-investigation-2026-10-17.md).
- **Crowded combat CPU work.** A controlled 200-enemy Windows benchmark reduced mean simulation CPU time from 20.012 to 12.002 ms (40.0%), with matching gameplay checksums. This excludes UI and is not ordinary-game FPS or a total-RAM claim. [Measurements and limits](docs/crowded-wave-optimization-2026-10-21.md).
- **Editor tooling integration.** The embedded AssetScope package traces serialized asset references to their owners, using the game's scenes, catalog and audio as practical examples. [Reproducible walkthrough](docs/assetscope-demo.md).

## Run locally

Requires **Unity 6000.6.5f1**. Open this repository as a Unity project, open `Assets/Scrapshift/Content/Scenes/Deck.unity`, then enter Play mode.

Move with **WASD / arrow keys**, use the **mouse** for menus, and press **Escape** to pause. Weapons attack automatically by default.

For this workspace's integrated Windows review build, launch `Builds/Optimization20261021R2/LaunchGame.cmd`. Keep the full player folder together; binaries are not included in the source checkout. R40 remains the separately documented full campaign/hardware checkpoint.

With the project's Editor closed, run:

```powershell
./scripts/verify.ps1
./scripts/build.ps1
```

## Validation

Latest integrated suite: **1,309/1,309 tests passed**, plus **40/40** focused snapshot/death/item/victory checks. **R40** remains the latest full campaign and physical Xbox checkpoint. Native software/visual checks cover companion starts, retry, shops and scaled navigation. See the [verification checklist](docs/gameplay-verification-status.md) for coverage and remaining limits.

The [fresh R40 comparison](docs/full-run-r40-comparison-2026-10-21.md) completed **20 waves in both engines**, with **18/18 checks passing**. It uses normal timing, scripted movement, contact immunity and an explicit matched acquisition policy; it is separate from human survival balance and physical controller verification.

That campaign used scripted movement and contact immunity. It does not establish human survival balance or exhaustive gameplay parity. The user separately confirmed R40's core physical Xbox controls; additional buttons, reconnect and transport cases retain their limits. Browser acceptance is documented separately. See the [hardware check](docs/controller-hardware-2026-10-21.md), [Web release](docs/web-release.md) and [coverage checklist](docs/gameplay-verification-status.md).

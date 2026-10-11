# AssetScope in Scrapshift

Embedded version: **0.3.0**, copied from companion commit `b7aac0df08a31f7ae951792c34e440163ef42d34`. The companion project was not modified. This supersedes the original 0.1.0 snapshot.

## Use it in this game

1. Open Scrapshift in Unity 6000.6.5f1.
2. Open **Window > AssetScope > Asset Search & Validation**.
3. Set Folder to `Assets/Scrapshift/Content`. Expand scan options, enable **Inspect serialized properties** and **Inspect saved scenes**, then **Scan folder**.
4. Search `GameSounds t:SoundBank`, select the bank, and inspect its serialized reference locations. A `variants` array entry targets `Fire_01.wav`. Click **Open source** to select the bank owning that reference; the message includes the array/property path.
5. Select `Deck.unity` and locate the ArenaGame `soundBank` reference. Source navigation selects its owning game component in the saved scene. Keep the Inspector unlocked so it follows selection.
6. Search `Ink t:Material` for shared prefab impact, or `RivetGun t:WeaponDefinition` for a weapon -> effect prefab -> mesh/material dependency chain. Enable transitive dependencies when useful.
7. Export the scan, or use **Scrapshift > Export AssetScope Scan** for the game's saved-scene/property validation preset.

## Verified integration

30 game EditMode tests pass. A game-specific test scans actual content, locates the Fire_01 sound reference, calls source navigation, and verifies Unity selects the GameSounds bank. It also verifies the scene's soundBank property is present in the analysis. The build-time scan completed on 72 assets with 1,202 serialized reference usages and zero findings.

The companion tool's own 66-test result is recorded in its repository. That is separate from the 30 game tests run here; the complete companion UI suite was not rerun in this host.

## Interpretation and limits

The package inspects supported saved prefab/scene components, ScriptableObjects and Materials, including missing scripts/references. It does not discover runtime-only loads, inspect unsaved scene changes, establish that zero-referenced assets are safe to delete, or prove gameplay correctness. Source navigation selects the owning object/component; automatic Inspector field highlighting is not implemented.

Use the companion project's isolated broken fixtures to demonstrate findings. Do not damage the playable game or present deliberate test fixtures as discovered production incidents.

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

Historical integration evidence below records the earlier 53-game-test checkpoint and its asset scan. It is not the current suite total or a fresh scan. Current gameplay validation is in `gameplay-parity-controls-stacks-2026-10-15.md`. On October 15 the menu declarations were rechecked in `Packages/com.shaoyu.assetscope/Editor/AssetScopeWindow.cs` and `Assets/Scrapshift/Editor/ContentBuilder.cs`; the actions above still exist. No new AssetScope scan or companion test total is claimed by the gameplay pass.

53 game EditMode tests pass. A game-specific test scans actual content, locates the Fire_01 sound reference, calls source navigation, and verifies Unity selects the GameSounds bank. It also verifies the scene's soundBank property is present in the analysis. The latest build-time scan completed on 198 assets with 1,337 serialized reference usages and zero findings.

For the original-art iteration, inspect the `idleFrames` and `moveFrames` arrays on `ScavengerAnimated.prefab`, and the catalog's `scrapShapes` / `muzzleFrames` arrays. These are real serialized Sprite subasset references into six texture sheets. The scene-dependency test confirms no historical reference PNG remains in the active scene graph.

The companion tool's own 66-test result is recorded in its repository. That is separate from the 53 game tests run here; the complete companion UI suite was not rerun in this host.

## Interpretation and limits

The package inspects supported saved prefab/scene components, ScriptableObjects and Materials, including missing scripts/references. It does not discover runtime-only loads, inspect unsaved scene changes, establish that zero-referenced assets are safe to delete, or prove gameplay correctness. Source navigation selects the owning object/component; automatic Inspector field highlighting is not implemented.

Use the companion project's isolated broken fixtures to demonstrate findings. Do not damage the playable game or present deliberate test fixtures as discovered production incidents.

## Reference catalog walkthrough

For the latest build, select `Content/Definitions/ReferenceCatalog.asset` in AssetScope. It contains real Sprite references for enemies, items and upgrade stats, and AudioClip arrays for each equipped weapon family. Trace SMG or shotgun sound clips back to their equipment entries; trace the catalog to Deck.unity. The prior sound-bank and shared-material navigation examples still apply. `reference-media-provenance.json` documents the supplied file origins separately from Unity dependency analysis.

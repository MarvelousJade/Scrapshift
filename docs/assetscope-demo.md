# AssetScope demonstration

## Setup

Open Scrapshift in Unity 6000.6.5f1. AssetScope is included as a local embedded Editor-only package, independently of the other active AssetScope project. Use **Window > AssetScope > Asset Search & Validation**.

Set the folder to `Assets/Scrapshift/Content`, then select **Scan folder**.

## Three useful workflows

1. **Shared material impact:** Search `Ink t:Material`. Its reverse references include the scavenger and enemy prefabs. Explain that changing a shared material affects multiple creature designs.
2. **Weapon content chain:** Search `RivetGun t:WeaponDefinition`. Inspect the effect prefab and accent-material references. Select the referenced prefab and follow it to the shared mesh/material. The current snapshot shows direct dependencies, not the richer future field-level workflow.
3. **Pre-handoff validation:** Run **Scrapshift > Export AssetScope Scan**. Read `artifacts/assetscope-report.json`. The verified normal game content contains 35 scanned assets and no missing-script findings. The scene is indexed for dependencies, not opened by this version's missing-script validator.

## Important interpretation

A clean missing-script scan does not prove that every serialized reference is valid, an asset is unused, gameplay is correct, or deletion is safe. The game's integration tests separately check expected weapon/prefab/material assignments. Keep these claims distinct in an interview.

## Demonstrating a failure safely

Use AssetScope's own isolated demo fixtures in its companion project, or a separate disposable test folder. Do not damage Scrapshift's playable content to manufacture a bug story. A deliberately broken fixture is a test fixture, not a discovered production incident.

## Updating the tool

The snapshot came from AssetScope commit `b3f060e9874bb7cb94235de532ad9f219c422c9c`. To upgrade: inspect the new package, replace the embedded package in a dedicated commit, run Scrapshift's content integration tests and the new tool's relevant tests, inspect the UI, and update this guide to its actual supported scope. Do not run two copies with the same package name in one Unity project.

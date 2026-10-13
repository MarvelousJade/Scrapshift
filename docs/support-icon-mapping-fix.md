# Support icon mismatch

October 13, 2026. User reported that Assault Companion Socket displayed a turret while spawning a rat, and many Codex entries shared that turret icon.

The item is the cybernetic name for `ratzilla`; it grants the rat companion. The screenshot cannot establish whether it was bought or taken from a crate. Its icon was incorrect, not evidence of a different item being granted.

## Cause and fix

Unity's serialized catalog materializes unused nested `StructureSpec` fields as empty objects. `SupportArt.Apply` checked only for non-null structure data, then resolved its empty kind through a fallback to the first turret. This overrode pet art and unrelated conditional-item art across the shop, inventory, rewards and Codex.

The mapping now requires the active `customKey` (`structure` or `pet`) and a recognized kind. Ordinary items retain their authored sprites; pet cards use the matching companion and structures use their matching structure. The existing shield special case remains. Gameplay values, acquisition and companion behavior are unchanged.

## Regression coverage

Added a test against a cloned, serialized production catalog, rather than hand-constructed objects. It verifies every support item's sprite identity, explicitly checks `ratzilla`, and verifies all unrelated items keep their original image. The earlier tests checked that the atlas had distinct sprites, but missed the application step on Unity-deserialized data.

301/301 EditMode tests pass. Reports and built-player captures are in `artifacts/icon-mapping-fix`. Updated executable: `Builds/IconMappingFix/Scrapshift.exe`.

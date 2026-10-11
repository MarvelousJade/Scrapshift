# Yard, weapon placement and run presentation

October 11, 2026. User direction: softer curved arena edges with a little ground detail; weapons arranged like Brotato; thicker legs; matching start/end compositions and the parachute entrance. Keep Scrapshift artwork.

## Reference inspection

Read the supplied Godot 3 project at `E:/UnityProjects/brotato-(3.x)` and inspected the live, paused wave-three window. The user's paused run was left intact. Earlier live title, character/weapon selection and loss-screen observations are recorded in `live-reference-comparison.md`.

| Reference evidence | Implementation decision |
| --- | --- |
| Paused gameplay: irregular dark earth boundary, small scattered ground marks, weapons surrounding the body | Replace straight rails with a curved, gently uneven outline and sparse stones, grass and buried scrap. Preserve the physical map bounds. |
| `entities/units/player/weapons_container.gd`: all one-to-six attachment calls use zero idle angle; `weapons/weapon.gd` mirrors idle direction with player facing | Remove the radial fan at rest. Retain measured count-specific slots; all idle weapons face the same horizontal direction. |
| Weapon scenes: ranged sprites offset 24px from pivot, knife 32px, spear 48px | Use per-family pivot offsets at 100px/unit. Rear slots render behind the body; front/side slots render in front. |
| Loaded `weapon.gd` resolves `Sprite/Attach`; inherited node has zero position | Do not blindly apply the separately serialized, mismatched `Attach parent="."` overrides as active offsets. This decompiled backup contains inconsistent inherited node paths. |
| `player.tscn`, animation resource 13: 1.2s landing; high-left descent, hover, quick final drop and stretch/squash | Implement explicit arrival poses, with carried weapon positions and a shadow that appears on touchdown. |
| `parachute/parachute.tscn`: canopy stretch at .8s, flatten at .9s, disappear at 1s; `main.gd` calls landing only on wave one | Original cream canopy mesh and four ropes collapse before the final drop. Landing occurs on a new run/restart, not between every wave. |
| Start screens: large brand, hero and vertical buttons; character then weapon cards | New title, cosmetic character selection, six starting weapons and a standard-run confirmation. |
| Lost screen: oversized RUN LOST, attacker, confirmation; end-run scene has stats, equipment and progression | Preserve the death camera, show actual attacker and image, then stats/loadout/items/wave progress with Restart, New Run and Main Menu. Victory has its own RUN WON presentation. |

## Original art and adaptations

No existing PNG was edited, replaced or copied from the reference. The yard and parachute use native generated meshes/materials. The existing outlined leg is widened 45% without changing its height or left/right separation; runtime and menu portraits share that ratio. Modular faces and wearables are retained.

This is a close presentation study, not pixel-identical reproduction. Our branding, typography, color palette, weapon silhouettes, portraits and decorative ground marks remain original. The landing interpolates source landmarks with simpler curves. Reduced motion keeps the character at ground level and suppresses squash/sway. For a fair opening, our simulation, enemy spawning and wave timer wait for arrival to finish; the reference starts its timer independently, so this is a deliberate adaptation. Character choices remain cosmetic, and there is one supported difficulty. The run setup does not pretend to offer the original's full character/difficulty/unlock system.

The opaque parachute meshes are serialized through `ArenaGame.parachutePrefab`, so AssetScope can traverse the prefab, mesh and material dependencies. The migration checks both its marker and the saved scene's floor/parachute references before skipping work. In-place regeneration of its generated mesh/material assets preserves their GUIDs.

## Bugs found and fixes

- Source comparison exposed radial idle angles and a generic .24-unit weapon pivot. Corrected facing behavior and melee-specific offsets, independently of weapon damage/range.
- The old end screen reset the camera to menu framing and discarded the location where the player died. Won/lost now keep the gameplay camera and equipment visible beneath the overlay.
- First render showed an overly bright rim with jagged corners. Reduced corner perturbation and darkened the rim/outside ground; lowered debris contrast to keep pickups readable.
- First arrival capture inherited the saved reduced-motion preference. The fixture now explicitly enables full motion without writing PlayerPrefs. Its initial checks passed but could not establish descent/squash visually; only the rerun is retained as final evidence.
- The full-motion edge fixture froze an enemy at the beginning of its birth scale animation, making it misleadingly tiny. The staged enemy now advances to a settled active age before capture. No gameplay enemy scale was changed to fix the fixture.
- A later source-to-validation sync restored the older scene while leaving the migration marker. Fresh screenshots then showed the old straight rails and no canopy, despite passing pose/transition assertions. The migration now checks the saved scene as well as the marker; an EditMode regression and runtime `authoredScenePresent`/`canopyInstantiated` assertions cover the missing content. The content-generation setup repaired the fixture before the new test executed: no red regression run is claimed. Final source and validation copies include the regenerated scene and match byte for byte.

## Verification scope

`RunPresentationTests` checks facing/layering, landing pose boundaries, loss-to-title-to-new-run reset, and non-accumulating leg width over repeated ticks. `--presentation-review <folder>` exercises actual menu methods, selected starting equipment, arrival gating, restart reset, return to title and accelerated twenty-wave victory. It supplies a six-weapon loadout and a fixed enemy, freezes normal AI, and produces thirteen fresh PNGs. PNG signature/time checks prevent stale captures from passing.

See `artifacts/presentation-review/presentation.json`, the adjacent rendered screenshots and the latest entry in `verification.md`. This does not establish a normal twenty-wave survival run, subjective feel acceptance, or that every UI control has received human input testing.

# Running reference comparison — October 11, 2026

User asked to compare the running Brotato game with Scrapshift, including movement, enemy spawning, arena dimensions, character/enemy/weapon/item scale and shadows, while retaining our art.

## Evidence and limits

Reference: `E:/UnityProjects/brotato-(3.x)`, Godot 3.6.2, displayed game version 1.1.15.4. Codex inspected the live title, Well-Rounded character selection, pistol selection, Danger 0 and first-wave gameplay through the computer-use tool. The first-wave samples showed a small bare character, enemies slightly wider than that body, a pistol mounted near the lower body, flat ground shadows, and green materials considerably more readable than our previous tiny drops. These were short stationary/brief-input runs, which ended in death; they are not a successful gameplay playtest. Exact speed and animation timing comparisons come from source inspection, not frame-timing measurements of those screenshots.

The source backup was previously repaired for startup (two Godot 4 array names and a null-language fallback). No reference gameplay rules or reference artwork were modified for this comparison. The previous Unity render in `artifacts/scale-smoke/gameplay.png` showed the entire shallow arena at once. New rendered evidence is in `artifacts/world-review/`.

## Findings and changes

| Area | Reference evidence | Unity change |
|---|---|---|
| World size | zone_1_data.tres: 32 x 24; utils.gd: TILE_SIZE 64 | 20.48 x 15.36 world units at the existing 100 px/unit convention. Previous floor was 23 x 11.4, with even smaller spawn bounds. |
| Camera | my_camera.gd: single-player follows target, minimum zoom 1; main.gd adds 96px borders and 192px above | Gameplay half-height 5.4, clamped follow camera, matching source vertical field of view. Our existing 16:10 UI viewport remains, versus the reference's 16:9, so horizontal field of view differs. Menus retain their authored framing. |
| Movement | player_stats.tres: speed 450; player_movement_behavior.gd uses normalized directional input | Keep 4.5 world units/sec and diagonal normalization. The newly consistent character size makes movement/body-size and travel/map-size ratios closer to the source. Existing one-second idle / 0.6-second walking spring remains. |
| Player | potato.png visible width 59px | Root scale .59 (native body about 1 unit wide), including original face, clothing and curved legs. Title/shop portraits keep intentional magnification. |
| Enemies | Per-enemy source textures, no visual scale override on inspected enemy Sprite nodes | Normalize each of all 18 original sprites to measured source width. Baby .73, bruiser 1.20, horned bruiser 1.40, predator 1.98 world units, etc. Preserve original aspect ratios and anatomy, rather than importing the source silhouettes. Measurements and source paths are in live-reference-metrics.json. |
| Weapons | Six source weapon textures plus player.tscn count-specific attachment positions | Normalize our six families to measured lengths (.64–1.62 units), maintain tiers, use separate one-to-six attachment layouts, and offset the aiming sprite from its mount. The .24 aiming extension is an adaptation based on the reference pistol; full per-weapon pivot parity is not claimed. |
| Materials and consumables | Live reference material silhouettes are readable; source material textures include glow padding | Normalize original green material shapes to .28–.36 visible maximum dimension. Capsule .48; crate approximately .85 wide. These are visual adaptations, not copying glow-texture canvas dimensions. Worn items inherit the corrected player scale; inventory/shop icons retain their UI sizes. |
| Shadows | Live flat dark ovals; player.tscn alpha .392157, vertical flattening .3 | Keep the existing clean circle geometry, opacity and grounded ownership. Corrected actor scales now size these shadows consistently. Our enemy shadows are explicit geometry; source textures may include baked shading. |
| Spawns | entity_spawner.gd places groups in the whole zone, with 64px edge inset and player-clearance retries | Preserve imported 20-wave timings/counts, warning delay, queue rules and clearance. Use the complete restored map bounds for random, edge, boss and retry positions. The larger vertical area no longer compresses encounters into the old shallow rectangle. |

## Implementation and verification design

`ArenaGeometry` centralizes map bounds and camera limits. Movement, pickup scatter and spawn placement share those bounds. An idempotent Editor migration resizes the authored floor/rails and redistributes existing dust marks. The artwork itself is unchanged: no generated or borrowed raster assets in this pass.

`ActorPresentation` separates visual dimensions from collision radii. Original body/weapon aspect ratios remain intact, and all equipment tiers use the same family scale. Existing contact/separation/projectile radii are unchanged; precise collision-shape parity and ordinary twenty-wave balance remain outside this pass.

Regression coverage checks north/south spawn coverage, map clamping, camera following/clamping including oversized viewports, weapon tier consistency and separate mounts. `--world-review` exercises actual first-wave simulation and normal gameplay camera while moving in four directions, then injects six weapons and three visible items. It restores HP and supplies gear, so it is rendering/integration evidence, not progression or balance evidence. Older enlarged sprite galleries explicitly retain their old camera framing.

For interviews: the actionable discovery was a unit-system mismatch. Movement/range data had already been divided by 100, while map dimensions and sprite sizes were independently chosen. Correcting a single sprite ratio could not fix traversal, camera framing or encounter density. The solution uses one world convention and distinguishes measured source values from deliberate art/UI adaptations. Codex performed the inspection, implementation and automated checks under the user's direction; candidate understanding and human gameplay acceptance should not be overstated.

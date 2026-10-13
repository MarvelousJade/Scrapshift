# Support motion correction and playable showcase

The local Brotato reference at `E:/UnityProjects/brotato-(3.x)` was inspected directly.

- `entities/structures/turret/turret.gd`, `turret.tscn` and `turret_shoot_animation.tres`: the ordinary turret sprite stays upright. Target direction controls projectiles. The 0.3-second firing animation squashes and lifts the whole visual, firing at 0.2 seconds. Removed our rotating/flipping head and applied the existing reference recoil to the connected assembly.
- `entities/units/pet/catling_gun/catling_gun.tscn`: movement has a 2 Hz body bounce, 1.0–1.05 horizontal scale, 1.0–0.95 vertical scale and a small +/-1.8 degree body sway. There are no separate walking-leg tracks. Removed the added dangling legs from our cat and added body sway; retained our artwork. This does not reproduce its separately animated ears, tail and two gun pivots.
- `effects/items/pet_effect.gd` and `global/entity_spawner.gd`: each owned pet effect spawns its quantity as separate actors. Scrapshift does this too. Full behavioral parity is not established: reference target reservation can spread same-kind pets across enemies, and Catling has separate left/right targeting and cooldowns. Scrapshift currently uses nearest targets and an aggregate firing interval. Nearby players/non-cat pets boost Catling in both implementations; cats do not boost other cats.

## Playable local preview

Launch `Scrapshift.exe --support-showcase`. WASD/arrows move, Escape pauses, F5 resets. Nine pet types, eight structures and a mine spawner; renewable enemies; player invulnerability and no wave end. No normal save writes (command-line fixture isolation). No player weapons so support attacks are easier to see. Only actual pet/structure effects enter the inventory: Unity materializes unused nested effect objects, so testing non-null objects alone incorrectly included unrelated items.

## Item icon audit

The repeated wrench/stat icons are real placeholders, not evidence of an old build. `Editor/AppearanceBuilder.cs` supplies a 24-item authored sheet. `Editor/InspectionBuilder.cs:EnsureInspectionUI` gives expanded items their first stat icon, falling back to Engineering. `SupportArt.Apply` separately fixes recognized support effects. The previous support mapping repair did not create unique artwork for every expanded item. That remaining artwork task is not completed by this motion correction.

Validation: all 302 EditMode tests passed, including upright turret recoil and cat body attachment/reduced-motion checks. Native build and screenshot results are recorded alongside the local preview artifacts. This is a presentation correction, not a claim of complete gameplay parity.

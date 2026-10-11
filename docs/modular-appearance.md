# Modular player appearance: reference audit

October 11, 2026. User direction: one simple robot body, interchangeable eyes/hair/beard, wearable embellishments and tiny pointed legs, without hands or shoes.

## Supplied source inspected before implementation

- `E:/UnityProjects/Brotato/items/global/item_appearance_data.gd`: appearance resources carry position, display priority, depth, and character-appearance flag. Eyes, mouth, forehead, hat, neck, torso, back and other positions are independent.
- `singletons/run_data.gd:add_item_displayed` (1294): duplicate owned item IDs do not add a second appearance; a new occupied-slot appearance replaces the old only when its priority is greater or equal. Position OTHER is unrestricted. Sorting uses depth. This differs from our old latest-item-only rule.
- `entities/units/player/player.gd:apply_items_effects` (316): sprites are attached to the animated body node; sufficiently negative depth draws behind it.
- `items/characters/old/old_eyes_appearance.tres`: eyes slot, priority 0, character appearance. `items/all/glasses/glasses_appearance.tres`: same slot, priority 2. An item can replace character eyes.
- `items/all/beanie/beanie_appearance.tres`: hat slot, priority 0, depth 1000.
- `entities/units/player/legs.png` and `leg_l.tscn`: two little tapered leg shapes, cropped from a two-column texture; no shoe silhouette. Prior spring audit covers movement timing.

## Planned implementation and limits

Keep our original illustrated robot palette, simplify the base shell and move facial identity out of its pixels. Three cosmetic presets reuse body/legs and swap eye/mouth/hair layers. Items get separate wearable art, while existing shop icons and stat effects remain independent. Use priority per slot, later equal-priority replacement, duplicate item suppression, and depth order. Shared resolution drives world and shop previews. Recompute when item identity or preset changes, including same-count replacements.

This is a small appearance system, not Brotato character passives/unlocks or a claim of complete appearance parity. Our explicit offsets replace the reference's aligned transparent canvases. Inventory removal will re-resolve remaining layers, intentionally restoring covered base features. Source inspected locally; no claim that supplied game code is officially open source.

Codex authored this audit and planned implementation in response to Shaoyu's visual direction. Validation and actual problems will be recorded after implementation.

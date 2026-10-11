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

## Implemented direction

The user compared the weathered and clean parts previews, then explicitly selected the clean style for all items and enemies and asked to remove every panel line from the player's body and legs. Active sheets are now player-parts-v2, wearables-v2, enemies-v5, items-v2, equipment-v2 and stats-v2. Outer outlines and broad shading remain; the blank shell and tiny pointed legs have no interior panel seams. Historical PNGs remain inactive.

Three cosmetic looks (Rookie, Veteran, Punk) share one shell and two instances of one pointed leg sprite. The title's LOOK buttons switch eyes/mouth/hair without changing starting stats. Character identity lives in serialized PlayerLook layers; item appearance is separate from its shop icon. All 24 catalog entries have original wearable embellishments; the existing shop still offers its eight-item subset. Six floating weapons remain independent of the body. No hands were added.

AppearanceResolver resolves defaults then unique item IDs, honors priority, replaces later equal-priority layers, and sorts by drawing depth. World renderers and the shop call the same resolver. Renderers are pooled per character and cached by preset, item identity/order and material, avoiding a new list every frame. Covered base features reappear when the covering item is removed. Item effects and inventory ownership are untouched.

Mapping is intentionally adapted to our art: coffee produces alert eyes, cake a cream moustache, fertilizer a sprout tuft, and bat wings draw behind the body. Reference priorities are retained where they map to our features (helmet 1 versus beanie 0; glasses/blindfold 2; cyclops eye 1). This is not a claim that every original appearance position, passive, unlock or purchase rule was copied.

## Actual issues encountered

- The previous wardrobe refreshed only when item count changed. Replacing one item at the same count left stale visuals. The new cache checks item references/order and look identity; a regression test replaces a hat and switches presets without changing count.
- Initial weathered wearable atlas slices clipped a wing edge: first run 126/127 passed, border alpha 0.172549. Moving that boundary exposed a separate glasses edge (second run 126/127, alpha 0.207843). Final clean sheets use measured transparent separators, sometimes different for each row/cell. The audit reads pixels but never rewrites images. Border tests cover all six final atlases.
- Clean enemy attempt v3 lacked a vertical separator between drill and junkie. A spacing edit (v4) unexpectedly contained a translucent background (minimum alpha 8, zero fully transparent pixels). It was rejected; imagegen background extraction produced v5 with 58.8% fully transparent pixels. Rejected sheets are not imported. Generation prompts and final hashes are retained.
- First rendered gallery labels crossed the characters. The fixture labels were moved below the sprites before final captures.

Built-in imagegen produced/edited raster assets. Unity sprite import slices preserve generated pixels; code animates and composites the parts. The clean preview with seams remains under docs/art-options as the user's decision reference; it is not the active player asset. Exact prompts: appearance-generation-prompts.json, art-options/clean-preview-prompt.txt, clean-art-prompts.json and clean-art-repair-prompts.json. Final PNG hashes and rectangles: clean-atlas-regions.json.

## Final evidence

127/127 tests; build errors/warnings 0; AssetScope 211 assets, 1395 usages, 0 findings. All 625 input files match isolated validation. Appearance, enemy gallery and combat art fixtures completed; selected screenshots inspected. See verification.md for exact limits/package hash.


## Leg silhouette correction

User rejected the narrow straight leg arrangement and clarified that the distance between the legs should be wider, with an outward curve like the reference. Reinspected supplied player/legs.png and leg_l/leg_r scenes. New original player-leg-v1 is one clean ivory curved appendage, mirrored for the opposite side. The body and facial sprites are unchanged.

Leg centers move from +/-0.13 to +/-0.31 world units (0.26 to 0.62 total separation), with the attachment raised from -0.62 to -0.52 so the wider legs join the curved lower body. Leg height is 0.22; lateral walking travel is reduced from 0.08 to 0.035 and rotation from 28 degrees plus a fixed tilt to 12 degrees around the new curved silhouette. World and shop share these dimensions; the shop explicitly mirrors the left sprite. Existing reduced-motion, facing, loop and atlas-boundary checks were updated for the new stance.

Built-in imagegen supplied the new leg PNG, using our player parts for style and the provided source legs only as silhouette reference. Exact prompt: wide-leg-prompt.txt. PNG SHA-256: ec9cdb2b9733259bbb25439d0c427d5d6c5b564e7151013fa42cdf834c595c84

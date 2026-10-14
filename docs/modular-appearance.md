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


## October 11 - proportions, outlines and flat ground shadows

User feedback: the player's body dominated the first-wave enemy, the curved legs needed a clearer outline, and ground shadows should resemble the simple shapes in Brotato.

Reference inspected before implementation: supplied `E:/UnityProjects/Brotato/entities/units/player/player.tscn`, `potato.png`, `enemies/enemy.tscn`, `enemies/baby_alien/baby_alien.tscn` and its texture. The player's Shadow node uses the potato texture, black tint alpha 0.392157 and vertical scale -0.3. The baby scene has no extra visual shrink. Native nonzero-alpha body bounds are 59x69 pixels for potato and 73x86 for baby: about 1.24 width/height ratio before animation. These are bare sprite measurements, not an assertion of every character's appearance or camera parity. Our enemy shadows intentionally generalize the player's simple flattened shape; the inspected enemy scene does not declare the same shadow node.

Implementation: player gameplay root scale 1 -> 0.85, retaining the same body/face/item/leg proportions and outward stance. Baby-alien visual scale 0.72 -> 1.4, giving neutral imported widths 0.853 (player) and 1.012 (baby), ratio 1.187, versus roughly 0.52 before. Other enemy size classes stay as authored. Visual scale now has its own ActorPresentation policy; contact, separation and projectile hit radii are unchanged. This deliberately isolates the requested presentation correction from balance; collision feel at the larger silhouette still needs user playtesting.

The v2 leg is an imagegen edit of our v1 curved leg, with a much thicker charcoal contour, clean ivory surface, no interior seams and the same tiny mirrored layout. The sprite is shared by world/title/shop. Active original PNG count remains seven. SHA-256: `420238ad41b30cb65aa940e9fb0ad8ce916619954e7cbe3401716b02e793531c`. Built-in image_gen was used with transparent background; exact prompt in outlined-leg-prompt.txt. PNG pixels were not edited by scripts; Unity import trims transparent padding.

Shadows use the existing unit-circle mesh, a dedicated transparent material, a 0.3 height/width ratio and alpha 0.392157. Diameter derives from each sprite's width; position follows its ground anchor. A GroundShadow component owns a separate world-space renderer, so body squash/dash rotation cannot shear the ellipse. It follows activation, fades/shrinks with a corpse and is destroyed with the actor. Serialized catalog mesh/material references keep AssetScope inspection possible. Title art remains an intentionally enlarged presentation; this change targets gameplay world scale.

Actual regression found: initial test run was 128/129. The EditMode shadow activation check failed because normal MonoBehaviour lifecycle callbacks did not run for the Editor-created object. ExecuteAlways allows the component's existing activation/cleanup callbacks to work for Editor instances too; it has no autonomous update or asset-writing behavior. Initial XML is retained in artifacts/scale-shadow-review/initial-editmode.xml. Final results are recorded in verification.md.

The new --scale-review fixture captures normal camera size 7.5, then a detail view at 2.8. It uses actual actor renderers and idle/walk springs but fixed positions and no combat/AI, so it is visual evidence rather than a balance test.

## October 13 — Scrapshift art for reference appearance rules

The appearance rules are sourced from the supplied game's metadata; the visible artwork is Scrapshift's own. The initial reference-PNG binding was corrected after user feedback. Source PNGs now live only in `artifacts/appearance-parity/source-art`, outside Unity's Assets and Resources, and are excluded from the playable build.

178 eligible items retain their source slot, priority, depth, duplicate and removal rules. Existing fitted wearables use our `wearables-v2` art. Other items use their distinct custom shop artwork with anchors sized for our body. Multiple layers remain independent. Skin-slot items attach their own designs to our shell rather than substituting the original game's potato graphics. Character features use our authored player parts and accessory art while preserving source eligibility and blockers; shared facial archetypes are intentional, not 50 newly authored portraits.

World icon-derived overlays use the existing geometry masks to exclude neighboring atlas fragments. This runtime mesh path is skipped for non-playing Editor inspections because Unity rejects geometry overrides there. Native review captures verify the actual masked renderers, layering and left/right mirroring. Appearance provenance tests reject any original-game appearance bitmap in scene dependencies or visible layer bindings.

## October 13 — regenerated fitted wearables

After the user rejected complete shop icons pasted onto the body, the built-in imagegen tool generated dedicated transparent parts for all 185 appearance layers (178 eligible items). Nine atlas PNGs include eight initial sheets and a targeted redraw preserving the Piercing Stabilizer and Gyroscopic Crown designs. Exact prompts and PNG hashes are in artifacts/fitted-appearance-review. The input references were Scrapshift's own player parts and item art. PNG pixels are unchanged; scripts only inspect alpha and produce sprite slicing and isolation masks.

Each part has calibrated body-relative dimensions and anchors. Caps clear the eyes, mouth guards are centered, collars curve around the lower shell, and paired accessories have separate art. Full-body skin items replace the base shell; removing them restores it. Skin exclusivity, priority, acquisition, duplicate, removal and layer depth rules remain sourced from the reference. Portraits stretch fitted masks to the same dimensions used by world renderers, rather than applying inventory-icon aspect fitting.

The native review fixture captures representative outfits facing both directions and all 178 eligible items. The new art intentionally adapts the visual designs to our anatomy; this does not claim pixel-identical placement to the original game's character.

## October 13 — four shared-template corrections

The beanie, leather vest, glasses and coffee now use separate full 1254-square transparent images in `OriginalArt/TemplateWearables`, generated from our player and inventory references. All four use 627 pixels per unit, layer scale 1, zero layer offset and no nonuniform stretch. The generator did not reliably obey requested pixel coordinates, so each complete canvas has an explicitly authored sprite pivot registering it to the same body origin. These pivots are import metadata; PNG pixels and transparent margins are preserved. This is registration of full artwork, not alpha-bound auto-fitting at runtime.

`TemplateWearableBuilder` applies only these four overrides after the existing appearance catalog generation. Other item artwork and positions are retained pending individual visual review. The portrait renderer now accounts for the sprite pivot as SpriteRenderer does. The full-frame sprites inherit the body animation and mirror with it. Coffee occupies the above-head slot in the inspected original resource; the earlier statement that the original coffee appearance changes eyes was incorrect.

`artifacts/shared-template-review` shows the four individually, combined, at smaller scale and mirrored during a walking pose. Native checks passed and 364 EditMode regression tests passed. Those checks verify registration, layering, removal and rendering behavior; they do not establish that every other item fits visually. `Builds/TemplateWearables/LaunchGame.cmd` is the new playable output. Exact generation/refinement prompts are linked in the review.

### October 13 — remaining wearable registration pass

Reviewed all six native item-gallery pages. Added an explicit body-origin registration table for the remaining 181 atlas layers (174 items); the four approved TemplateWearables PNGs and their bindings are unchanged. Made 64 per-piece fit corrections, including cyclops/elemental eyes, hats, horns, and side attachments. Existing good fits and source artwork remain intact; no original-game raster artwork was imported.

Atlas custom pivots now express the character-body origin, including pivots outside cropped rectangles. Item-layer offsets are zero, and world/portrait rendering uses the same registration. Existing authored width/height ratios remain where appropriate; this does not claim that every atlas sprite is uniformly scaled. Slot priorities, source depths, exclusivity and acquisition logic are unchanged. `scripts/register_remaining_wearables.py` preserves the correction table independently of alpha-bound audits. `scripts/sync_template_wearables.py --all-wearables` copies only appearance fields and texture import metadata from the validation clone.

Validation: 365/365 EditMode tests pass, including all 181 registered centers/dimensions and the four unchanged template canvases. Native runtime fixture passed (178 eligible items, 50 characters, 23 visible layers, mirrored correctly). Visually reviewed all 178 individual item appearances and twelve selected combinations in both facing directions; not every possible combination was visually tested. Fresh native captures, before/after gallery, and build links: `artifacts/remaining-wearables-review/review.html`. Unpacked Windows build: `Builds/RegisteredWearables/LaunchGame.cmd`.

### October 13 — Blind Optics correction

Replaced the horizontally compressed atlas bandana with a dedicated transparent front-facing red eye wrap (`TemplateWearables/blindfold.png`). Generated with the built-in imagegen tool using our player and wearable sheets, with exact prompt saved under `artifacts/blind-optics-review/prompt.txt`. Preserved PNG pixels; registration uses full 1254-square canvas, 860 PPU, body origin (627,690) in top-down pixels, offset zero and uniform scale. The previous four approved wearables remain unchanged. Atlas registrations now cover 180 layers and full-canvas bindings cover five.

Native review checks solo, hat/jacket/coffee combinations, small size and mirrored walking pose. The Ranger's priority-3 eyes intentionally block the priority-2 item appearance; the review documents this exception. Native runtime checks passed. Build: `Builds/BlindOptics/LaunchGame.cmd`. Preview: `artifacts/blind-optics-review/review.html`.

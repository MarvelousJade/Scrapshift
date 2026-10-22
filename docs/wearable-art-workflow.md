# Generating item art for the character layer system

This guide describes the working Scrapshift appearance pipeline, checked against the importers and portrait renderer on 2026-10-15. Use it with the preferences in `../AGENTS.md` and the implementation history in `modular-appearance.md`.

## 1. Define the two different assets

An inventory icon and an equipped appearance serve different purposes:

| Asset | What to draw | Where it is bound |
| --- | --- | --- |
| Inventory/shop icon | The complete recognizable item, composed for a square tile | `ItemEntry.image` |
| Character appearance | Only the part worn on, behind or above our character, composed to fit its anatomy | Each `ItemEntry.appearances` layer's `image` |

Keep their visual identity consistent, but do not bind the whole shop icon as a wearable. A jacket icon may show a standalone jacket; its appearance must wrap the lower egg body and leave the face clear. A beanie must sit on the crown. Coffee belongs above the head. Blind Optics must cover the eyes.

First inspect the item's current ID, own icon and appearance rules. Some items intentionally have no wearable, particularly deployed structures/companions. Some have multiple independent pieces. Do not invent a visible appearance for every item or collapse a multilayer item into one arbitrary foreground image.

## 2. Prepare references and generation instructions

Use the image-generation skill and the built-in image tool for raster creation/edits. Follow its current instructions. This guide itself does not require generating anything.

Useful project references:

- `../Assets/Scrapshift/Content/OriginalArt/player-parts-v2.png`: our cream egg body and face style. The upper-left cell is the body; don't ask the generator to copy the whole sheet into the output.
- The item's actual Scrapshift icon or existing dedicated wearable, located through its current catalog binding.
- `../Assets/Scrapshift/Content/OriginalArt/TemplateWearables`: examples of the approved full-canvas approach.

Inspect local reference images before passing them to the tool. Clearly identify which image provides body proportions and which provides the item's design. Use our own images as generation inputs. Inspect the original game's metadata for rules; don't use original-game appearance PNGs as the visible output.

Request one wearable piece per output, actual transparency, generous transparent margins, a consistent front-facing view, clean flat colors, bold smooth near-black outlines and minimal shading. Exclude the body, eyes, mouth, feet, background, checkerboard, guides, labels, shadows, grain and unrelated objects. For an eye-covering item, require an opaque region covering both eye locations. For open glasses, explicitly decide whether their lenses replace the eyes visually or allow them to remain visible; the slot rules must support the intended result.

For a layered object, specify which piece belongs behind the body and which belongs in front. Generate and bind these as separate layers where needed. A slot name is an exclusivity rule, not a positioning instruction that the renderer automatically understands.

### Reusable prompt template

Replace every bracketed field before submitting:

```text
Create ONE production 2D wearable sprite for Scrapshift on a genuinely
transparent square canvas. Reference 1 supplies our character's cream egg
proportions and clean black-outline cartoon style. Reference 2 supplies
the visual identity of [item ID and item name].

Draw ONLY [specific worn piece], front-facing, fitted to an invisible
egg body. Do not draw the character, face, eyes, mouth, legs, background,
guides, text, checkerboard or ground shadow. Preserve generous transparent
margins around the wearable. No grain, rust or heavy texture.

For layout guidance on a 1024-square canvas, imagine an invisible body
centered at (512,512), about 514 pixels wide and 620 pixels high, with eye
centers near (433,456) and (591,456), and mouth near (512,625).
Draw [shape, dimensions, opening, curvature and attachment point].
Keep [face openings or other required clear regions]. The art should
[wrap the lower body / sit on the crown / cover the eyes / attach at the
side / sit above the head]. Include only [necessary details].
```

These coordinates are composition guidance, not an import contract. A generator may return another canvas size or place the art elsewhere. Inspect and measure the actual PNG; do not assume it obeyed the prompt. Avoid fixing a wrong silhouette by squashing it. For example, Blind Optics needed a flat eye wrap, not a hollow bandana cylinder compressed into a thin strip.

Save the exact final prompt, reference paths, generated source path and selected output in the dated review folder. Copy the selected output into the project. Preserve its alpha and original pixels; retain the original generated file. When refining an asset, keep the approved version available until the new fit is verified.

## 3. Register a full-canvas wearable to the body

Prefer a dedicated full transparent canvas for new or substantially corrected pieces. Preserve the margins: cropping and then automatically centering the opaque object changes its relationship to the body.

`../Assets/Scrapshift/Editor/TemplateWearableBuilder.cs` currently applies the dedicated overrides after the atlas appearances are generated. It imports a single sprite with alpha, no mipmaps, uncompressed texture, bilinear filtering and Full Rect mesh. The current approved files are 1254 × 1254; the importer explicitly validates this size. A new image size requires a deliberate importer/test change, not silent resizing of every approved asset.

Current approved registrations:

| Item ID | Pixels per world unit | Body origin in PNG, top-down pixels |
| --- | ---: | --- |
| `beanie` | 627 | (627, 627) |
| `leather_vest` | 627 | (627, 718.5) |
| `glasses` | 627 | (627, 669) |
| `coffee` | 627 | (627, 699.5) |
| `blindfold` | 860 | (627, 690) |

These are registrations for these exact files. They are not universal pivots for all hats, glasses or coffee-like art.

Let the actual canvas size be `(W,H)` and the authored body origin in top-down pixels be `(Bx,By)`. Unity's normalized sprite pivot is:

```text
pivot = (Bx / W, 1 - By / H)
```

Select pixels-per-unit from the desired worn size, then set the body origin so a clear landmark lands correctly. For example, with pixels-per-unit `P`, a landmark at image pixel `(fx,fy)` that should appear at body-relative world coordinate `(cx,cy)` gives:

```text
Bx = fx - cx * P
By = fy + cy * P
```

Y is inverted because image coordinates here run downward and Unity world Y runs upward. Check several landmarks: a single correctly placed center does not prove a jacket wraps properly or a hat matches the crown. Alpha bounds alone do not identify the attachment point.

For this full-canvas path use `layer.offset = (0,0)`, `layer.scale = 1` and `layer.stretch = (1,1)`. Tune the authored pivot and uniform pixels-per-unit, preserving the artwork's proportions. Regenerate the shape if it still does not fit. Record the chosen registration and why it was selected.

The current importer uses parallel ID/origin/PPU arrays and assumes each listed item has exactly one appearance. Extend all corresponding data together. Do not push a multilayer item through `.Single()`; support each piece explicitly with stable item/layer identity.

## 4. Existing atlas path

Existing dedicated atlas pieces live in `OriginalArt/FittedWearables`. Keep good fits rather than regenerating them indiscriminately.

- `manifest.json` identifies each source rectangle and its stable `itemID_layerIndex` key.
- `registrations.json` stores each remaining piece's explicit world width, height and center relative to the body.
- `../scripts/register_remaining_wearables.py` authors that registration table. Change its correction data when changing a fit so a later regeneration preserves the adjustment.
- `../scripts/audit_fitted_wearables.py` analyzes source alpha and writes slicing/masking metadata. It is tied to the existing batches; inspect it before rerunning or extending it. It is not a universal auto-fit tool for a new sheet.
- `../Assets/Scrapshift/Resources/fitted-wearable-masks.json` isolates the pieces from nearby atlas art. The mask must agree with the source sprite rectangle.

`AppearanceReferenceBuilder.FittedAppearances` imports the custom pivot as:

```text
pivot = (0.5 - centerX / width, 0.5 - centerY / height)
```

This places the body origin in the cropped sprite's coordinates; it can be outside the rectangle, representing transparent padding without duplicating a large texture. The layer then has zero offset. The importer also applies the authored width/height fit. Some existing atlas pieces have nonuniform fitting; do not claim the whole atlas uses uniform scale, and do not use this as an excuse to compress a new unsuitable drawing.

When moving another item from atlas to a dedicated canvas, update the atlas registration exclusion, template binding, relevant expected counts, metadata synchronization and tests together. The present 180 registered atlas layers plus five dedicated canvases are a historical baseline. The importer still creates 185 atlas definitions before the five template overrides; don't remove counts without understanding that generation order.

## 5. Bind art without changing gameplay or selection rules

Keep the item's stable ID and each layer's source identity, slot, priority, depth and character-appearance flag. Preserve character-specific blockers and duplicate/removal behavior. The existing importer maps depth into rendering order; don't reorder accessories globally just to hide a bad fit.

Set the appearance layer's image, not the inventory image. Keep the primary/legacy appearance and compatibility wear fields in agreement using the existing builder. Put the change in content generation so the next Generate Game Content run does not overwrite a manual catalog edit.

`CharacterWardrobe` attaches resolved sprites under `SpriteFrameAnimator.BodyRoot`, making the wearables follow body squash/bounce and face mirroring. Skin layers replace the base shell; removing them restores it. Keep the renderer's existing depth/ordering behavior and source-driven eligibility.

UI portraits must respect the same sprite pivot. `ShopUI.DrawAppearance` compensates for the difference between image center and pivot when drawing the full image rectangle. Test all other portrait paths that consume these layers. Don't introduce another independently tuned UI offset to compensate for a world registration error.

## 6. Validate in the actual player

Generate content and run relevant appearance tests in the appropriate Unity project, using the isolated validation clone if the working Editor is open. Check current project/build state before copying assets. Do not overwrite unrelated gameplay fields or another ongoing validation run.

Review the changed piece:

1. Alone on the normal body, showing that its opening/strap/contact point actually fits.
2. With relevant approved hats, eye pieces, clothing and above-head items; inspect occlusion and slot priority.
3. On applicable character faces and full-body appearances. Label intentional blockers: Ranger's higher-priority eyes can legitimately suppress Blind Optics.
4. Facing both directions and during body squash/bounce. Look for floating gaps, detached knots, unexpected flips and clipped edges.
5. At actual gameplay size as well as a larger inspection view.
6. In world rendering and the relevant UI portraits/tooltips.
7. After acquisition, duplicate acquisition, replacement and removal; test Hide item appearance according to its existing semantics.

Check alpha/atlas isolation, correct catalog binding and body registration with tests. Passing those tests does not establish visual fit. Read the native logs/reports and inspect the captured images before claiming success.

The existing native appearance fixture accepts `--appearance-parity-review <output-folder>`, with optional `--template-wearable-review`, `--remaining-wearable-review` or `--blind-optics-review` presets. Check `../Assets/Scrapshift/Runtime/AppearanceParityReview.cs` before using or extending the fixture. A fixture is separate from an ordinary gameplay run.

For validated catalog synchronization, inspect `../scripts/sync_template_wearables.py` before running it. Its default ID list is explicitly maintained; `--all-wearables` includes the atlas item IDs. It copies appearance fields/import metadata rather than replacing the whole catalog. Update its scope when adding a new template item and confirm that the clone contains this task's validated assets.

Deliver a dated review with native screenshots, art/prompt links, branch, exact build/launcher path, test results and any limits. The screenshot must come from the build being shared. Preserve previously approved assets unless the user asks to change them.

## Worked examples

- Approved first four: `../artifacts/shared-template-review/review.html` and its saved prompts.
- Remaining atlas registrations: `../artifacts/remaining-wearables-review/review.html`.
- Blind Optics redesign, exact prompt and native checks: `../artifacts/blind-optics-review/review.html`.

These are fitting examples and historical evidence; check whether a newer build or code change supersedes their implementation details.

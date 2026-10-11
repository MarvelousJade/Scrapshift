# Original art and animation pass — October 11, 2026

## Current spring revision

The user replaced the multi-frame gait direction with a simpler body-only design. The active set is now five sheets: `hero-spring-v1.png`, `enemies-v2.png`, and the existing equipment/items/stats sheets. All eighteen enemy bodies have no hands or legs; the player has no arms/hands and uses one small separate foot sprite twice. Slow idle and faster walking squash/stretch are code-driven. Worn items follow the body and weapons float independently. See `spring-animation.md` and `spring-art-provenance.json` for source comparison, generation briefs, slicing corrections and verification.

The remaining sections below describe the previous frame-animation iteration; their four/six-frame claims and decorative-arm description are historical.

## Reference study and decision

Reviewed all twelve user-supplied screenshots in `E:/UnityProjects/Brotato/References`. The strongest gameplay references are `Brotato_o9JlhVG5Sb.png` (green drops, varied silhouettes and orientations, red spawn marks) and `Brotato_RCniq1VEZX.png` (separate floating weapons, short yellow firing cues, dark character outlines). `Brotato_j2seABbqdB.png` and `Brotato_1I4vrFxXQE.png` guide readable shop/upgrade icons. Screenshots establish appearance, not animation timing; timing below is our own design.

The user requested generated art while retaining the supplied audio. Six active transparent PNG sheets now supply the game, generated using the built-in image_gen tool. The initial five-sheet prompts are in `generated-art-prompts.json`; walk revisions are in `walk-revision-prompt.txt` and `walk-direction-prompt.txt`. Assets are saved in `Assets/Scrapshift/Content/OriginalArt/`; the active walk sheet is `hero-walk-v2.png`. Their pixels are preserved; Unity sprite import rectangles select individual frames/icons. The ivory scavenger and mechanical enemies are AI-generated original designs. Historical sprites remain for provenance but are not dependencies of the scene. Audio remains the supplied reference audio.

## Implementation targets

- Four idle/blink frames and six walking frames, independent of weapon aiming; stable facing while stationary.
- Separate ranged recoil and melee thrust, three-frame muzzle flash, projectiles emitted from the muzzle.
- Five green scrap shapes with random scale/orientation, outward scatter, a short bounce, settled resting pose, and accelerating collection magnet.
- Preserve currency when splitting enemy drops or reaching the pickup cap.
- Slice atlas icons correctly in both world rendering and IMGUI cards; preserve serialized sprite references for AssetScope.

## Attribution

The user supplied direction, screenshots and visual feedback on stride timing, backward-looking feet, wearable items and confusing battery loot. Codex generated the art and implemented the integration. This is not hand-drawn candidate artwork. Enemy/economy analysis remains separately attributed in `reference-rebuild.md`. Measured runtime checks and limits are in `verification.md`.

## Implemented behavior

The scavenger uses four idle/blink frames on a three-second cycle and six distinct walk frames at seven frames per second. Motion-state transitions reset the frame clock; facing follows horizontal input and stays stable when stopped. The simulation drives animation, so pause and hit-stop freeze its frame. The title uses the same character and idle frames. The former coral scarf/bar is absent.

Ranged weapons kick backward over 25 ms and recover over 135 ms, with a three-frame muzzle flash lasting about 66 ms. Melee weapons thrust forward instead. Weapon art points right at rest; aiming rotates it, with a vertical flip for leftward aim. Tracers use the generated tapering effect and start at the barrel tip. Reduced motion lowers recoil and suppresses pickup bounce/spin, while retaining readable character frame animation.

Each enemy reward becomes up to six physical pieces; the final piece carries any remaining value. Five generated green shapes vary in angle and size, scatter outward, bounce with gravity, settle with damping, and accelerate toward the player after entering pickup radius. At the 240-pickup limit, material value is banked. Repair capsules have a separate red/ivory sprite and heal three HP.

Unity stores each sprite as an atlas subasset. `OriginalArtBuilder` imports validated rectangles and trims transparent margins without altering PNG pixels. Its versioned migration binds all 18 enemy entries, 24 weapon tiers, 24 items, eight upgrade stats, warning, muzzle frames and drops. Shop/upgrade cards draw the sprite UV rectangle, not the entire atlas. AssetScope can inspect the serialized catalog and animation-frame references.

## User-directed corrections

The original four-frame walking row looked like two repeated poses. A separate six-pose cycle reduced playback from nine to seven frames per second (full cycle approximately 0.86 seconds instead of 0.44). The next user observation identified backward-looking steps: the sheet mixed rightward and leftward boot orientation. The final `hero-walk-v2.png` corrects the rightward stride and uses the existing horizontal mirror for left movement. Frame-count tests establish coverage, not whether a gait looks natural; actual rendered inspection and user feedback remain necessary.

Weapons float independently of the decorative arms. All 24 retained catalog items have wearable mappings (eight are offered in the current shop): head, eyes, body, back, crest, badge, mouth and belt/shoulder attachments. The newest item in a shared appearance slot is visible; every owned item's stat effect remains active. A shop preview shows the current outfit. These are small sprite attachments, not a skeletal clothing/deformation system.

The green battery carried a plus sign that suggested healing. It was removed from the active material pool at the user's request. Five shapes remain: washer, nut, bent fragment, triangular shard and square chip. Only the red/ivory repair capsule heals. The unused battery art remains in the source sheet for provenance.

The user chose to move on after the backward-walk report without confirming which direction caused it. The mixed horizontal foot orientation has been corrected; upward movement still reuses front-facing art. A dedicated rear-view cycle is a known presentation limitation, not claimed complete.


## Current clean modular art - October 11

Supersedes the weathered spring sheets above. User selected clean surfaces after seeing a smooth preview and requested removal of all player body/leg panel seams. Active: player-parts-v2, wearables-v2, enemies-v5, items-v2, equipment-v2, stats-v2. Six original generated atlases retain bold outlines and meaningful item/enemy construction detail, with no gray rust/scuff texture. Player shell and pointed leg are intentionally unmarked. Twenty-four dedicated wearables are separate from twenty-four item icons. Three cosmetic presets share the same body. See modular-appearance.md, clean-atlas-regions.json (hashes/rectangles), and clean-art-prompts.json plus clean-art-repair-prompts.json. Built-in imagegen performed raster edits; Unity imports/animates the parts. No source PNG pixels were manually altered.

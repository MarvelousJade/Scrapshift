# Support actors, manual aiming, and reference audit — 2026-10-13

## Problem and result

Deployed turrets and pets reused weapon sprites: a wrench on the ground could be mistaken for a dropped weapon. Eighteen original transparent sprites now distinguish nine structures and nine companions. The same sprites identify their associated inventory items. Weapons keep their existing art. Landmines also have a distinct sprite, a one-time arming click, tint, and compression.

The full-inventory shop replaced its numeric price with `Slots full` inside a narrow price button, clipping the message. Prices remain numeric now; the capacity explanation appears above the button.

Manual aim while pressed previously depended on the separate always-on option, and a firing weapon could still resolve its direction from an enemy target. Mouse-press mode now works independently and firing retains the cursor direction. This is manual **aiming with automatic cooldown firing**, as in the reference, not one shot per click.

## Reference comparison

Read-only source: `E:/UnityProjects/brotato-(3.x)`, especially `singletons/item_service.gd`, `singletons/utils.gd`, and `entities/structures/landmine/landmine.gd`.

- A player or enemy entering a mine arms it. Leaving it triggers the explosion. Our enter/exit sequence already followed this; this pass improves its feedback. Player proximity is therefore a valid trigger. Friendly mine explosions damage enemies.
- Standard shop reroll base cost is `floor(wave * .75) + max(1, floor(wave * .4)) * (paidRerolls + 1)`. Wave 6 starts at 6, then 8, 10, 12. Free rerolls are consumed first. Added the previously omitted `reroll_price` percentage multiplier and ceiling, with a minimum 10% multiplier.
- Ordinary item price before modifiers is `baseValue + wave + baseValue * wave * .1`, floored after price modifiers and clamped to at least 1. Locked offers retain their original wave price. Weapon-specific and health-shop rules are separate.
- The early shops guarantee two weapons in waves 1–2 and one in waves 3–5. Same-weapon, weapon-set and wanted-tag weighting and positive/negative Luck rarity formulas were compared to the source. Added the missing unique-weapon-character exclusion for lower-tier duplicates and final-tier duplicates.

## Evidence and limits

`python scripts/verify_catalog_parity.py` freshly parses the supplied reference resources. It passed **4,918 comparisons**, with **50 characters, 62 weapon identities (205 tier records), and all 209 registered base items**. No missing registered items or differences in the audited fields.

Audited fields include character starting choices/tags/base stats, weapon primary stats/scaling, and item tier/value/tags/caps/scalar effects. This is not proof that every special effect, interaction, timing rule, or RNG sequence is identical. Our generators do not produce Godot-identical seeded shop sequences. Endless inflation and all unusual shop fallback cases are not certified. The player's requested two starting weapons remain an intentional difference.

Current menu controls are connected to gameplay/audio/display settings: volumes and focus muting; fullscreen, map, shake/effects/damage and FPS; mouse movement, both aim modes, focus pause, boss HP, lock retention and item appearance; enemy health/damage/speed, description size, projectile/pet opacity, darkening, effect icons, reduced motion and text colors. This pass fixes aiming, rather than claiming every option has been manually operated on hardware. Controller/co-op/endless parity is not established.

Validation: Windows build succeeded with zero errors/warnings; **292/292 EditMode tests passed**. The music-duration test initially failed because it hard-coded the previous 61-second loop; updated for the deliberate new 105-second composition. The isolated validation project matches Assets/Packages/ProjectSettings byte for byte. Built-player staged UI and companion reports are in `artifacts/support-polish-review` and `artifacts/support-pets-review`; these fixtures are not a human survival playthrough.

## Music

`NeonSalvage.wav` is now an original 146 BPM industrial EDM/metal synth loop: distorted synthesized power-chord chugs, sub bass, four-on-floor kicks, fast hats, minor-key riffs, fills and breakdowns. The 64-bar stereo loop lasts 105.205 seconds, normalized to .82 peak (RMS .168). `scripts/compose_music.py` reproduces it. No commercial song or reference melody was sampled. Automated audio validation checks stereo samples, duration and headroom; subjective listening approval remains with the player.

## Art provenance

Tool: built-in `image_gen.imagegen`, transparent background. Saved game asset: `Assets/Scrapshift/Resources/SupportAtlas.png`. `SupportArt.cs` slices 6×3 cells and crops transparent margins at runtime. The atlas is readable, uncompressed and has no mipmaps, preserving alpha edges. No original-game sprite was copied.

Prompt: Create ONE production game sprite atlas on a transparent background, exactly 6 columns by 3 rows of equal square cells, 18 individual sprites, no text, no labels, no grid lines. Each sprite centered in its cell with generous transparent padding, never touching another cell. Cohesive clean 2D cartoon cyberpunk survivor game art: bold smooth black outlines, simple flat silver/cream bodies with colored lights, restrained two-tone shading, no texture/no grain. Three-quarter top-down view, small friendly deployable machines and robotic pet mascots, distinct silhouettes readable at 50 pixels. Row 1 left to right: (1) basic silver tripod bullet turret with short black barrel and cyan light (2) copper flamethrower tripod turret with orange canister (3) stout missile launcher turret with two red rocket tubes (4) slim cyan laser turret on anchored base (5) white medical healing dispenser on pedestal with green plus (6) low round orange landmine with warning light. Row 2 left to right: (1) small hydroponic garden planter with green sprouts (2) purple electrical defensive drone on pedestal (3) round teal hovering slow-field robot (4) friendly chunky silver robotic dog with impact hammer tail (5) cream robotic cat with two small shoulder gatling barrels (6) compact tracked mine-layer companion robot carrying orange mines. Row 3 left to right: (1) orange robotic salamander with little flame exhaust tail (2) purple cybernetic mouse companion with small claws (3) white and mint medical robot moth with green medical plus, spread wings (4) segmented gold and teal salvage worm robot with magnet mouth (5) sturdy cream cybernetic goat with shield plates and short horns (6) cyan jellyfish shield companion drone with translucent dome and short mechanical tendrils. All art original, not copied from any game. Atlas cell order is critical. No weapons alone: each is a complete machine or pet.

## Interview notes

The useful distinction is data parity versus behavior parity. A source-driven audit prevents silent catalog drift, while runtime tests catch control and rendering defects that matching numbers cannot. The mine report was a presentation problem rather than a missing trigger; the manual-aim report exposed two actual input/target-resolution bugs. Explicitly recording those differences avoids claiming exact equivalence from a passing data comparison.


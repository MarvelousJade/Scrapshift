# Player hit feedback and outward-curving legs

## Request and diagnosis

Shaoyu reported that the silver legs had the wrong orientation and disliked the player damage sound (described as "ha ha"). The current code confirmed both issues: the elongated limb sprite had zero resting rotation, making it look like a horizontal runner, and the reference importer mapped Hurt to `resources/sounds/zombie_voice_attack_grunt_01.wav` instead of the player's actual sounds. This was an incorrect source mapping, not a Unity audio playback failure.

## Reference inspected

Read the user-supplied `E:/UnityProjects/brotato-(3.x)` project, without modifying it:

- `entities/units/player/player.tscn`: four hurt samples, `bullet_impact_body_flesh_05.wav` through `_08.wav`.
- `entities/units/player/player.gd`, `take_damage`: white flash on positive damage, player sound with pitch variation 0.2, damage signal, and 0.2-0.4 second protection based on damage/max HP. This path does not make the player blink invisible.
- `entities/units/unit/unit.gd`, `unit.tscn`, `resources/shaders/flash.gdshader`: white material replacement for 0.1 second.
- `global/effects_manager.gd` and `visual_effects/hit_effect/hit_effect.tscn`: randomly rotated hit effect; three frames at 20 FPS (0.15 seconds). Player stats enable blood particles.
- `particles/hit_particles.tscn`: eight particles (engine default), dark red, 0.5 second lifetime, full-circle spread, no gravity, 600 pixels/second with 50% randomization.
- `global/screenshaker.gd`: player damage shakes the camera by 5 source pixels for 0.1 second.
- `visual_effects/floating_text/floating_text_manager.gd`: player damage displays a negative red number.

## Implementation

Keep the existing silver limb PNG unchanged. Mount it at mirrored 110-degree local angles so each convex edge faces outward and the rounded tip points down/out. Move the attachment slightly upward to keep it tucked into the body. Movement adds the existing small swing to that resting angle; facing left preserves the world-space outward curve. Reduced motion stops the swing without flattening the stance. Portraits use the same resting angle with inverted screen Y, and the shop now shares the portrait routine. Update the saved prefab as well as runtime poses, checking actual rotation/position before skipping migration.

Hurt now reuses the already imported four `Impact_01..04` files; SHA-256 comparisons confirmed that each equals the corresponding reference player sound. Correct both the initial import wiring and existing-bank migration so regeneration cannot restore the zombie voice. Use pitch variation +/-0.2 and retain our reserved damage audio voices, gain, cooldown and combat ducking. The old unused grunt file is retained as historical imported material; it is no longer referenced by the Hurt bank entry.

Remove the invented red inset screen border and whole-player invulnerability blinking. Keep the existing 0.1-second white flash, red damage number, short camera shake and source-derived protection timing. Comparing the rendered closed star to `frame0000.png` exposed excessive coverage over the body. Replace the player-only star with four separate tapered white streaks and an open center, authored as native line geometry. Randomize its orientation; give its eight red fragments the source lifetime, emission radius and speed range. Enemy impact geometry is unchanged.

## Scope and evidence

This is a source-driven correction with rendered Unity verification. No new reference-game live hit capture or subjective audio listening approval is claimed. Our impact streaks, fragment geometry, character/wardrobe flash, sound mixing and camera offset function remain Unity adaptations using our art; this is not pixel-identical reproduction. Damage text still uses our general floating-text animation. Reduced motion continues to suppress camera shake.

The first compile caught one obsolete `hurtOverlay` reset after removing the overlay; removed it and rebuilt. Render inspection exposed a preview-specific transform bug: successive GUI pivot helpers mirrored after rotating, so the left leg did not mirror the right like the world sprite. Replaced them with an explicit sprite-space mirror followed by rotation, shared through `PortraitLegTransform`, and added a symmetry regression. Also tilted the ends outward before final validation. These are actual findings, not invented red-test stories.

Automated coverage checks the serialized four-sound mapping, orientation across left/right facing, stable leg dimensions, flash expiry, effect cleanup and second-hit immunity. Rendered fixtures supply contacts/gear or freeze AI; they do not establish normal-run balance. Final counts and artifact paths are recorded in verification.md.

## Interview discussion prompts

- **Why compare source as well as appearance?** A plausible filename led to the wrong sound. Following `player.tscn` into `take_damage` identified the actual asset dependency and timing.
- **How was the bug prevented from returning?** Fix the importer and existing-asset migration, then assert the saved sound bank references all four correct clips.
- **What made the visual fix tricky?** World Y and GUI Y point opposite ways; facing also mirrors the limbs. Verify actual transforms in both directions and share preview code rather than fixing one screen.
- **What did tests miss?** An earlier mathematically valid orientation still looked too straight. Render inspection motivated the final outward tilt.

Attribution: Shaoyu supplied the direction, reference project and visual/audio feedback. Codex inspected source, implemented these changes and ran the documented checks. These notes are evidence for explaining the collaboration, not a claim that Shaoyu personally wrote or manually tested every line.

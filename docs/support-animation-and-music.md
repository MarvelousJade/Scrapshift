# Softer music and animated support actors — 2026-10-13

Music now stays silent through launch, character choice and weapon choice. `StartSelectedRun` calls `GameMusic.StartOnce` when Start is confirmed. Subsequent waves/restarts do not rewind playback. The update recovery path is gated until this first start. Pause still lowers music volume; the setting slider still controls volume.

The revised original 146 BPM EDM loop removes hard guitar distortion and master saturation. Chords now use five softly weighted harmonics instead of eighteen, lower filtering, and slower attacks. Lead FM brightness, harsh noise hats and crash levels were reduced. Bass and rhythmic energy remain. Peak .72, RMS .1496, 105.205 seconds stereo. Audio-file tests are objective checks, not a claim of listening approval.

## Animation reference and adaptation

Inspected the supplied Godot 3.x `entities/structures/turret/turret_shoot_animation.tres`, `turret.gd`, and pet `.tscn`/`.gd` resources. A compact extraction of the pet root tracks is saved in `artifacts/support-animation-reference.json`.

Turret reference keys at 0/.1/.2/.3 seconds are (1.1,.9), (.8,1.2), (1.2,.8), (1,1). Its shot occurs at .2 seconds. The new visual timeline uses these scale keys, intermediate easing exponents, and the corresponding position changes around the resting offset. It runs at the existing attack-speed multiplier and completes during recovery. Static shadows use the unanimated anchor and base scale.

Pets now face movement direction and use a slower idle pulse / faster moving pulse. The 0.5-second movement cycle uses the source root squash strengths: .05 for dog/salamander, .03 for the mine bot, .20 for the rat and .15 for the goat. Added attack/eating pulses, worm sway, a moth flutter impression, shield pulsing, and flattened goat incapacitation. The dog's existing jump retains its ground shadow. Reduced Motion suppresses decorative animation.

This adapts the reference's motion to our complete single-sprite artwork. The source has separate animated heads, ears, tails, wings and legs; this pass does not reproduce that multi-part rig or claim every pose is identical. Gameplay movement and damage values were not changed.

## Surface cleanup

Replaced `Assets/Scrapshift/Resources/SupportAtlas.png` with a simpler original image edit: removed dirty patches, scratches, rivets and unnecessary seams, retaining strong outlines, broad color fills and readable silhouettes. Used built-in `image_gen.imagegen` with the previous atlas as reference and transparency enabled. The atlas order stays six columns by three rows.

Edit prompt: Preserve the exact six-column by three-row layout, all eighteen subjects, order, directions, silhouettes, identifying colors and transparent spacing. Remove rust, gray dirty patches, wear, scratches, chipped paint, mottling, grain, speckles, little seams, rivets and decorative panel lines. Use broad uninterrupted flat color areas with one simple shadow shape per part, bold black contours and only essential part separation. Simplify plants and moth wings. No text, grid, new objects or cross-cell spill.

## Validation

Windows build and EditMode regression results are saved with the release. `--support-motion-review` checks silent music before Start, playback after Start, no rewind on repeated Start, changing turret poses and grounded shadows, then captures a motion sequence. Companion behavior checks exercise all nine types separately. These deterministic fixtures do not constitute human listening or a full survival playthrough.

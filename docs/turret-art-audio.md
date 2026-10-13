# Turret artwork and support audio

The local original `entities/structures/turret/turret.tscn` uses one upright texture and a recoil animation. Its script aims the spawned projectiles. It does not rotate a separate gun head. Scrapshift retains the user's requested visible head tracking, now with side-view gun artwork and a round rear joint over the fixed tripod. This is a deliberate presentation difference.

Catling's extra mesh-based paw stepping is disabled. The complete body sprite keeps its attached legs, body bounce and sway, and independently rotating guns.

## Original audio

`Resources/original-support-audio.json` maps original turret and pet weapon-stat sound arrays and decibel settings. Original WAV files are copied without processing from the local reference into `Resources/OriginalSupportAudio`. Runtime events use these for all turret firing variants, Catling and Bot-o-mine shots, Blazemander fire, Bonk Dog attacks, Catling boost entry, moth zone entry/exit, Jellyshield hits, Scapegoat death/revival and its continuous revival loop, and mine presses. Empty source shooting arrays remain silent. SFX volume, mute and bounded voice handling still apply.

This does not establish complete audio parity: original end-of-wave petting interactions are not implemented, and the existing general explosion/mixing system remains. Purring and bark clips are retained in the mapping for their corresponding future interactions, not substituted for firing.

## Artwork provenance

Built-in imagegen, reference-guided generation. Asset: `Assets/Scrapshift/Resources/TurretHeadsV2.png`. Reference: existing `SupportParts.png`. The generated sheet is sliced at runtime; circular joint positions are authored separately from the bounds.

Prompt: Create a production game sprite atlas with exactly five separate turret upper gun assemblies, arranged in one horizontal row. True transparent background. Use the reference clean cream/black cartoon style. Strict flat side view aimed right, no perspective, legs, pedestal or underside post. Large round rear pivot centered vertically, straight barrel extending right. Variants: basic cyan-detail cannon; orange flame nozzle; red twin rocket launcher; cyan laser; green-cross healing emitter. Clean solid colors, thick black outlines, minimal cel shading, no grime, shadows, text or guides. Transparent gutters. No pets.

## Verification

27 support tests passed, including loading the original firing clips and confirming Catling uses its undeformed body sprite. Native review checks combat behavior and captures each turret variant facing right, up, left and down. Audio mappings and triggers were inspected; no claim of an auditory side-by-side comparison is made.


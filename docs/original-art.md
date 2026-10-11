# Original art and animation pass — October 11, 2026

## Reference study and decision

Reviewed all twelve user-supplied screenshots in `E:/UnityProjects/Brotato/References`. The strongest gameplay references are `Brotato_o9JlhVG5Sb.png` (green drops, varied silhouettes and orientations, red spawn marks) and `Brotato_RCniq1VEZX.png` (separate floating weapons, short yellow firing cues, dark character outlines). `Brotato_j2seABbqdB.png` and `Brotato_1I4vrFxXQE.png` guide readable shop/upgrade icons. Screenshots establish appearance, not animation timing; timing below is our own design.

The user requested generated art while retaining the supplied audio. Five new transparent PNG sheets were generated using the built-in image_gen tool. Full prompts are in `generated-art-prompts.json`. Their pixels are preserved; Unity sprite import rectangles select individual frames/icons. The new ivory scavenger and mechanical enemies are AI-generated original designs. Historical reference sprites remain in the repository for provenance but must not be dependencies of the game scene after migration. Audio remains the previously integrated user-supplied reference audio.

## Implementation targets

- Four idle/blink frames and four walking frames, independent of weapon aiming; stable facing while stationary.
- Separate ranged recoil and melee thrust, three-frame muzzle flash, projectiles emitted from the muzzle.
- Six green scrap shapes with random scale/orientation, outward scatter, a short bounce, settled resting pose, and accelerating collection magnet.
- Preserve currency when splitting enemy drops or reaching the pickup cap.
- Slice atlas icons correctly in both world rendering and IMGUI cards; preserve serialized sprite references for AssetScope.

## Attribution

The user supplied direction and screenshots. Codex generated the art and implemented the integration. This is not hand-drawn candidate artwork. The enemy/economy analysis from the previous iteration remains separately attributed in `reference-rebuild.md`. Runtime fixtures and limitations will be recorded after verification.

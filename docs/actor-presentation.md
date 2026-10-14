# Actor proportions, outlines and ground shadows

October 11, 2026. User feedback: legs need stronger outlines, the player looks too large, and first-wave enemies too small. Preserve the approved wide curved stance and clean original robot art.

## Reference evidence

Inspected the supplied Godot 3-format scenes before the user converted the reference folder to Godot 4.7 during this work. `entities/units/player/player.tscn` uses a black body-texture shadow with alpha 0.392157, scale (1, -0.3), and offset (0,38) under its Animation node. `enemy.tscn` assigns the baby-alien texture; the baby scene has no additional visual scale override. We did not find an equivalent enemy-shadow node in these inspected scenes; uniform enemy ovals are our adaptation, not a claim of exact source parity.

Read-only PNG alpha bounds: potato 59 x 69 visible pixels, baby alien 73 x 86. Their neutral bare-body width ratio is approximately 1.24. This is a source-art comparison, not measurement of a running Godot build. The original project was not executed for this pass.

## Implementation

- Gameplay player scale is 0.85, including its facial layers and curved legs. Title and shop portraits retain their intentional display magnification.
- Basic enemy visual scale changes from 0.72 to 1.4. Other enemy scales remain as before. `ActorPresentation` separates visual sizes from the existing collision/separation radius calculation, so this pass does not silently rebalance contact, projectile or pickup rules.
- Enemy death presentation uses the same visual scale, including evolution, avoiding a size snap on death.
- A new bold-outline leg sprite is shared by world, title and shop. Its shape is still a tiny pointed outward curve; there are no added feet or body panel lines.
- Ground shadows use the existing unit circle mesh and a transparent flat-black material, with a 0.3 height/width ratio and reference-inspired 0.392157 opacity. Their width comes from actor dimensions instead of an atlas crop's arbitrary transparent margins.
- Each actor owns a separate shadow object. Explicit logical-position placement keeps the oval anchored during squash, emergence and dash rotation; actor enable/disable and destruction manage its visibility and lifetime. Corpse shadows shrink and fade with the corpse.
- Mesh and material are serialized in ReferenceCatalog so the build and AssetScope can trace them. No runtime shader lookup is needed.

## Art provenance

Built-in ImageGen edited our original `player-leg-v1.png`, preserving the curved silhouette and adding a bolder charcoal contour. Output is `Assets/Scrapshift/Content/OriginalArt/player-leg-v2.png`; source PNG is copied unchanged. Prompt is in `outlined-leg-prompt.txt`. Original generated output: `C:/Users/FanSh/.codex/generated_images/01a1284b-4f69-7eb3-a342-b7a2b6b94bb9/exec-4ec710a3-6d5f-4165-9304-6cefb382d411.png`.

## Verification scope

`--scale-review` stages the actual player and first-wave enemy at normal gameplay camera size, then zooms in for outline/shadow inspection. It exercises idle and moving presentation, without AI or combat. Editor regression coverage checks the relative sprite sizes, active leg version, shadow independence from nonuniform scale/rotation, visibility, cleanup and serialized assets. See verification.md for completed results.

The first build attempt was rejected by Unity's project-open guard. Process inspection subsequently found only Unity Hub and no Editor; the lock file was absent. No process was killed and no user's Editor state was discarded. A retry followed after that check.

Interview framing: the bug came from multiplying two unrelated presentation scales and reusing a sprite crop as ground geometry. The fix isolates presentation from combat rules and gives shadows explicit ownership and placement. The tradeoff is that larger enemy art retains its earlier, more forgiving hitbox; full gameplay feel and balance require player evaluation.

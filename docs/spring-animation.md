# Body-only spring animation — October 11, 2026

User direction: enemies have no hands or legs; the player has no hands and only tiny feet. Idle springs slowly; moving springs faster. Retain our original salvage identities and floating weapons.

## Reference inspected before implementation

The supplied Brotato files are individual parts, not a baked multi-frame gait atlas:

- `E:/UnityProjects/Brotato/entities/units/player/potato.png`: body.
- `entities/units/player/legs.png`: small separate feet.
- `entities/units/enemies/baby_alien/baby_alien.png`: individual enemy body.
- `player_idle.tres`: one-second loop, horizontal scale 1 to 1.15, vertical scale 1 to 0.85, five-pixel vertical movement.
- `player_move.tres`: 0.6-second loop with two squash/stretch pulses. At 0 / 0.1 / 0.2 / 0.3 seconds, scale is (1,1), (1.2,0.8), (0.9,1.1), (1,1). Tiny legs move and rotate separately. The body's position uses -24 / -14 / -29 / -24 source pixels. Reimplement these cues in Unity rather than copy the source scene.

Two built-in imagegen edits use our prior original PNGs as edit targets: `enemies-v2.png` removes limbs across all eighteen archetypes; `hero-spring-v1.png` contains an armless body plus one small foot to reuse. Existing weapon, item, pickup and audio assets remain. Old character/enemy atlases stay for history. No PNG pixel processing is used; Unity import rectangles select and trim the generated parts while preserving their alpha.

Implementation and measured checks will be recorded after integration. A single front/three-quarter body mirrored horizontally is deliberate; no separate rear-facing walk cycle is claimed.

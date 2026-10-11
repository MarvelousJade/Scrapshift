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

Implementation and measured checks are recorded below. A single front/three-quarter body mirrored horizontally is deliberate; no separate rear-facing walk cycle is claimed.

## Implementation and verification

`SpringMotion` samples a one-second idle cycle and two walking pulses in 0.6 seconds. Source pose landmarks are retained; smooth interpolation, a simplified alternating tiny-foot motion and scaled enemy cadence are deliberate adaptations. `SpriteFrameAnimator` animates a child body, leaving the gameplay root and tiny feet outside body squash. Worn item renderers attach to the body; weapons remain independent. Reduced motion returns neutral body scale/offset and stationary feet; pause/hit-stop stop animation time. Trees remain stationary props. Existing collision positions, combat rules and death shrinking remain unchanged.

`SpringArtBuilder` preserves the player prefab identity and binds all eighteen new enemy sprites. Original PNGs and earlier atlases remain intact. The new active art has one body pose, not repeated fake walk frames. Up/down still uses the same body; leftward travel mirrors it horizontally.

The first rendered enemy gallery exposed a clipped horned-charger nose and stray pieces in adjacent cells. A uniform 6x3 grid did not match the generated silhouettes. Import rectangles now follow measured alpha bounds; the spitter and tree share a narrow transparent-column boundary. The new border test initially failed on spitter contamination (119/120), then passed after that correction. See `bugs.md` for the actual failure, not a hypothetical bug story.

Final checks: **120/120 tests pass**; Windows build succeeds with zero warnings/errors; all **595 validation files** match the checkout. AssetScope: **201 assets, 1,355 usages, zero findings**. The final staged art fixture observed idle scale range 0.15, walking scale range 0.30, both facings, tiny feet, five item attachments, six floating weapons, 37 shots and existing pickup/recoil effects. `artifacts/spring-art-final/` and `artifacts/spring-gallery-final/` contain actual rendered evidence. Independently inspected corrected full enemy silhouettes and player/outfit rendering. The fixtures supply equipment/health and do not establish balance or subjective animation acceptance.

Commits: `e634814` comparison, `7f2219b` sprites/animation/import regression. Documentation and final evidence are a separate commit. The prior twenty-wave run and first-wave loss are historical; this visual revision did not rerun the complete campaign.

# Particle Accelerator beam collision — 2026-10-15

This bounded pass replaces the remaining constant-width capsule query for the supplied base game's Particle Accelerator. It supersedes the beam limitation recorded in `combat-remaining-parity-2026-10-15.md`. Existing combat, support, controller and audio changes are preserved.

## Source reconstruction

Read-only source: `E:/UnityProjects/brotato-(3.x)/projectiles/bullet_particle_accelerator/particle_accelerator_projectile.tscn` and its adjacent `.gd` script.

- The shape is a rectangle with half extents `(50,22.5)` source pixels and collision offset `(50,.5)`. `shoot()` moves Contents +100 pixels and Hitbox −100 pixels, cancelling those translations. Its horizontal scale is `max(1,(weapon_range+200)/100)` because the supplied center texture is 100×33 pixels.
- In Unity units, a range of 2 produces a rectangle covering x=0…4 and, at scale 1, y=−.23….22. Circle contact is tested against this rotated rectangle; there are no large rounded end caps.
- Contents and both cap containers use cubic scale keys `(1,.1)`, `(1,1)` and `(1,.00001)` at source times 0/.2/.4. Playback speed 2 makes the real duration .2 seconds and the central key .1 seconds. Stop disables the hitbox. The source explicitly disables offscreen destruction for this projectile.
- The non-looping interpolation duplicates its first/last neighbors. This was verified against official [Godot 3.5 Animation interpolation](https://github.com/godotengine/godot/blob/3.5/scene/resources/animation.cpp#L1613-L1630) and [Vector2 cubic interpolation](https://github.com/godotengine/godot/blob/3.5/core/math/vector2.cpp#L145-L160). The tiny near-peak overshoot is preserved. The curve is not clamped to one.

## Runtime changes

- `WeaponReference.cs`: shared cubic beam scale, analytic maximum scale over a time interval, rectangle/circle contact and beam visual pose. Middle/cap sprites now share the same curve and the source 33-pixel texture's half-pixel vertical registration.
- `ReferenceWeaponBehavior.cs`: the actual projectile collision query dispatches beams to their animated rectangle and keeps normal projectile sweeps intact. Range comes from `WeaponProjectileView`'s captured emission range, so a conditional range bonus cannot stretch a beam already on screen.
- `ReferenceCombat.cs`: passes the previous/current shot age into the beam query and respects the source beam's offscreen exemption. Existing support lifecycle hooks remain present.

The rectangle grows and shrinks about a fixed line, so its shapes are nested. Taking the exact cubic maximum over the crossed frame interval prevents a variable Unity frame from missing a target covered between its endpoints. This retains the existing continuous-hit approach used for fast projectiles and melee; it is an explicit scheduling adaptation rather than a claim that Unity and Godot generate identical physics callbacks. Simultaneous beam overlaps retain Scrapshift's deterministic ordering along the beam for piercing; Godot's overlap signal order is not reproduced.

## Verification

`BeamRemainingParityTests.cs` adds 12 cases:

- Seven independently calculated scale samples, including overshoot and the end of the animation.
- Flat endpoints, rectangle corners and rotated contact.
- Growth, shrink and inactive-at-stop timing.
- A frame spanning the cubic peak, plus rejection of an already expired interval.
- Actual `ArenaGame.UpdateWeapons` waiting for growth and damaging each target once.
- The real collision callback retaining captured range and the real renderer/caps sharing the source scale and vertical registration.

The coordinating agent runs Unity validation and the combined build. No Unity or reference runtime was launched by this subtask, no source assets were edited, and no audio was replaced. Native beam appearance/hit timing in the combined build remains a separate check; the authored test cases are not a claim of a normal human playthrough or total gameplay parity.

## R8 integrated checkpoint

Combined suite620/620 passed,0failed/skipped,74.3181245s. R8build succeeded0errors/warnings. Native weapon205/205 and enemy/state53/53 checks passed with exit0; beam/healer captures visually inspected. Artifacts: gameplay-parity-geometry-2026-10-15. Fresh normal-timing campaign is in progress; do not treat fixture checks as human full-run equivalence. See gameplay-parity-continuation-2026-10-15.md for exact paths, hashes and scope.


## R8 normal-timing campaign completed

Fresh final-build campaign Passed/Won, exit0:20timers19shops31purchases18upgrades6crate takes, normal60Hz pickup physics,1092.05wall seconds. All31debits and all recorded stack totals reconcile,0matched player exceptions. End capture shows queuedDrifterunlock; Won comes from report/trace. Comparison against the frozen supplied-original run is in full-run-r8-comparison-2026-10-15.md; scripted invulnerable fixtures do not establish human survival parity.


# Support follow-up: animation callbacks and native evidence — 2026-10-17

## Scope and status

Finite follow-up after the combined 434 focused checks: turret windup callbacks, target eligibility, and the ability of the native support fixture to exercise actual production projectiles. Read-only source: `E:/UnityProjects/brotato-(3.x)`. No Unity session, build, automated test execution, native capture, audio listening or full run was performed by this agent. Root owns integrated focused validation and native review.

## Confirmed corrections

1. **Target disappears during turret windup.** `entities/structures/turret/turret.gd:shoot` resets shooting/cooldown on an invalid target but then continues to its sound and pellet loop. `_next_proj_rotation` is retained, initially zero. Scrapshift previously returned without emitting. `ItemStructures.TickStructure` now retains the last rotation in the existing non-orbiting turret angle field, initializes it to zero, and emits after windup even when the target vanished. Acquiring a target is still required to begin windup. Garden continues its specialized fruit callback. This is a source-observed mismatch, not a player-reported bug.
2. **Range body pool and integer distance.** The source Range stores overlapping bodies (mask5, Tyler mask4), while the later firing gate compares raw-root distance. Scrapshift previously discarded bodies as soon as their roots exceeded reach; it now tests actual body radii/offsets separately, so an already-started callback can aim at a body still overlapping the area after its root left firing reach. The helper’s inferred integer pixel-squared distance and `Utils.is_between` integer pixel-distance boundary are retained. Player/healing acquisition uses the player body radius and independent raw root as well. This does not model an extra physics-frame delay in body enter/exit events.
3. **Identical-origin target.** `singletons/utils.gd:get_nearest_no_max` uses a strict `distance_squared > min_distance_squared`; with the registered zero minimum it excludes an exactly coincident raw origin. `StructureTarget` now follows that boundary. Detection includes source body-layer eligibility and separate body-pool/raw-root gates. No claim of identical Godot physics contact ordering is made.

`SupportTimingParityTests` contains eleven source-derived cases: no target/no windup; target removed during windup still emits rightward on the first shot; retained previous direction; the 0.2-second animation shoot key; strict zero-distance exclusion; body-overlap outside firing reach; callback retargeting outside reach; equal integer pixel-squared ties; two integer-distance firing boundaries; and subpixel exclusion. These are initially pending execution. Existing 434-pass evidence predates this follow-up.

## Native fixture changes

`PetReview` still exposes nine companion cases, Cat gun poses/boost views, turret aim views, and multiple-support checks. Earlier turret aim shots are explicitly static artwork samples. Added six separate `*-live-projectile.png` stages for ordinary, flame, rocket, laser, healing and Tyler turrets. Each uses `TickStructure`, captures actual `StarterProjectile` output/profile, removes the stationary damage target and advances `UpdateWeapons` by 0.04 seconds before the screenshot. The JSON adds two checks per kind for source-profile binding and survival through that initial update.

This fixture is deterministic, muted and uses stationary targets. It can demonstrate the actual emitted projectile and its initial presentation, but does not establish human audiovisual acceptance, identical random trajectories, hardware controls or full survival-run behavior. Screenshots and JSON must be read from the freshly built fixture before claiming any check passed. The own-art muzzle/body attachment, all shot directions, animated projectiles in motion and audible roles still require native inspection. No reference art, music or approved audio waveforms were changed; companion leg animation remains omitted.

## References

- `entities/structures/turret/turret.gd`: `shoot`, `_next_proj_rotation`, `_on_AnimationPlayer_animation_finished`.
- `entities/structures/turret/turret_shoot_animation.tres`: length0.3s, shoot callback0.2s.
- `singletons/utils.gd`: `get_nearest_no_max` strict minimum-distance comparison.
- Runtime: `Assets/Scrapshift/Runtime/ItemStructures.cs`, `PetReview.cs`.
- Focused checks: `Assets/Scrapshift/Tests/Editor/SupportTimingParityTests.cs`.

## Interview/attribution note

The user directed concurrent parity investigation and supplied the reference. Codex inspected the source, authored the correction and added the checks/fixture. Useful rehearsal: trace an animation key into its callback and explain why a lost target does not automatically cancel a shot. The existing fixture once displayed a projectile sprite without exercising its emitter; the new stage makes that evidence boundary explicit. Do not present this as independently authored or personally reproduced by the user until confirmed.

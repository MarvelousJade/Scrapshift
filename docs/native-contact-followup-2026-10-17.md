# Native contact follow-up â€” 2026-10-17

## Scope and attribution

Codex inspected the supplied source and ran a bounded native Godot **3.6.2** fixture at **60 physics ticks/s**, time scale **1**, with rendering hidden and audio muted. The original project and the user's open Godot editor were not changed. The isolated project is `E:/Codex Projects/Job Search Assistant/tmp/BrotatoContactParity`.

This fixture instantiates the actual supplied player, delayed lightning and flame projectile scenes and scripts. Its receiver is the actual `Hurtbox` subtree detached from `baby_alien.tscn` before the enemy enters the tree; the receiver logs Area2D entry. Enemy AI, the production Unit damage callback, pooling, invulnerability, hit effects, and campaign progression are **not** exercised. These are source-scene physics observations, not human playtesting or a full-run comparison.

## Observations

| Case | Native observation |
| --- | --- |
| Ordinary projectile, 600 px/s, stationary target | First contact at fixture step 6 |
| Delayed lightning, 600 px/s, delay explicitly set to 2 source frames | First contact at step 8 |
| Ordinary projectile, catalog speed 3000 px/s | Contact at step 2 |
| Delayed lightning, 3000 px/s, same forced delay | Contact at step 2; travel reaches target after enable |
| Target moves away before delayed projectile enables | No entry |
| Same initially overlapping target with ordinary projectile | Entry recorded before the first fixture movement tick |
| Flame at 6000 px/s, centerline target | Entry |
| Flame at 6000 px/s, grazing target | No entry despite continuous path intersecting the combined circles |
| Ordinary projectile at 36000 px/s across a stationary target | No entry; deliberately outside registered/capped speeds, diagnostic only |

The nine cases recorded six contact cases and three no-contact cases. These counts classify observations; they are **not nine independent regression tests**.

## Concrete physics difference to reconcile

The source flame scene inherits a **44 px** circular hitbox centered at local **(24,4)**. The supplied baby alien hurtbox has **32 px** radius. At 6000 px/s the projectile moves approximately **100 px per physics tick**. A target center at **(174,174)** and projectile root starting at **(0,100)** misses the endpoint circles sampled by native physics, while a continuous sweep at y=104 comes within **70 px**, less than the combined **76 px** radius. The centerline control target at **(174,104)** does register.

This is a reachable speed ceiling for range-scaled projectiles: `singletons/weapon_service.gd:115` clamps speed to **50â€“6000 px/s**. The supplied Flamethrower base speed is 800 and enables range scaling, so Range 1950 reaches the cap. It is an extreme stat case, but the 36000 px/s diagnostic must not be presented as the playable mismatch. An implementation that sweeps the whole segment can therefore register grazing hits the original's Area2D sampling misses. Root integration owns any Scrapshift correction and focused validation.

## Evidence and limitations

- `artifacts/gameplay-parity-contact-native-2026-10-17/results.json`: per-tick positions, disabled state and entry events from the final nine-case run.
- `source-manifest.json`: all **14** relevant original and cloned scripts/scenes are byte-identical.
- `reference-source/`: saved relevant supplied source findings.
- `contact_fixture.gd`, `.tscn`, `project.godot`: exact fixture configuration.
- `attempt4-stdout.log` / `attempt4-stderr.log`: final native logs. Earlier attempts are preserved.
- Initial autoload logging reports failure to create the generic `user://logs` directory, and shutdown reports autoload resource leaks. ProgressData save paths are redirected inside the isolated clone; the contact fixture completes and writes all nine cases. Do not label the run error-free or treat its exit as proof of a clean project startup.
- Delayed lightning's ordinary random delay is 0â€“3 source frames; this fixture deliberately fixes it at 2 for repeatability. The original script defers collision enable/disable; the receiver observes that real lifecycle.
- Native game damage ordering, moving enemy AI, Godot rigidbody separation, UI/controller hardware, support visuals/audio, and complete campaigns remain outside this fixture.

## Follow-up: isolated deferred activation boundary

A second bounded fixture uses stationary initial overlap and **matching hitbox geometry**. The ordinary control's Hitbox offset is explicitly overridden to (-20,-1), matching delayed lightning's source offset; the delayed scene/script itself is unchanged. The same real baby-alien Hurtbox subtree receives the entries. The ordinary control registers at step 0. Delayed initial countdowns 0, 1 and 2 register at steps 1, 1 and 2 respectively.

| Forced delay | First physics callback | After that callback, before deferred calls | After deferred collision update | First entry |
| --- | --- | --- | --- | --- |
| 0 frames | disabled; countdown becomes -1 | still disabled | enabled | step 1 |
| 1 frame | disabled; countdown becomes 0 | still disabled | enabled | step 1 |
| 2 frames | disabled; countdown becomes 1 | still disabled | still disabled | step 2, after second callback reaches 0 and deferred enable |

`step` is incremented in the fixture parent's physics callback before the projectile's callback. Area entry arrives during the physics synchronization before the next parent increment. Consequently “entry step 1” describes a collision observed after the first movement callback and its deferred enable, not a synchronous hit inside that callback. An implementation needs an initial disabled/pending state even for randomly selected delay zero; merely testing `remainingDelay > 0` permits an earlier contact. The callback that reaches zero still sees the collision disabled until deferred updates.

The original 600 px/s cases at steps 6 and 8 have **different source offsets** (ordinary combined offset (23,0), delayed combined offset (3,-1)); their two-step difference must not be attributed entirely to the forced delay. The stationary matching-geometry follow-up isolates activation timing.

Evidence: `delay-results.json`, `delay_fixture.gd`, `contact_observer.gd`, `delay_fixture.tscn`, `delay-project.godot`, and `delay-stdout.log` / `delay-stderr.log`. This remains a custom receiver fixture, with the same startup/shutdown warnings and scope limitations above.

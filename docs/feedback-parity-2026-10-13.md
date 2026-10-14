# Camera shake and enemy blood spray parity

The October 13 follow-up compared the supplied Godot 3.6 game at `E:/UnityProjects/brotato-(3.x)` with Scrapshift's showcase implementation. The source version is 1.1.15.4. Source files are `global/screenshaker.gd`, `global/effects_manager.gd`, `particles/hit_particles.tscn`, and weapon `*_stats.tres` files. The imported red particle image has the same SHA-256 hash as the source image.

## Changes

- Camera shake now accepts a request only when both intensity and duration exceed the current shake. For each active frame, camera offset uses independent random values from zero to the source intensity on each axis. Player damage requests 5 px for 0.1 seconds; enemy damage requests `min(damage / 3, 3)` px for 0.1 seconds.
- Normal enemy hits no longer produce separate coral or amber sparks. The existing hit star, damage numbers, sounds, and death animation remain.
- Enemy blood uses the original red sprite and color, bilinear filtering, a 15 px sphere emission area, full-circle spread, 300-600 px/s initial velocity, 0.5-0.625 initial scale, linear size falloff, and a 0.5 second lifetime. A positive hit emits blood even when it kills the enemy; trees do not emit blood.
- The 205 imported weapon profiles now carry the source `effect_scale`. Blood emits `max(1, floor(8 * effect_scale))` particles. Minigun and Gatling Laser hits emit one; a full-scale hit emits eight. The active-emitter cap rejects a complete burst rather than truncating particles during crowded waves.

## Verification and limits

All 357 Unity EditMode tests passed. They check camera retrigger and offset, particle count, radius, spread, speed, scale, lifetime, and representative rapid-fire weapon profiles. The headless Windows build succeeded with zero errors and zero warnings.

After the user confirmed the graphics card was working, the updated Windows player launched successfully. The Cyberpunk Ranger showcase ran in wave 19 with visible enemy blood. A second, visible Direct3D 11 run of the controlled combat-feel fixture passed, reported eight particles on the fatal enemy hit, and saved nonblank `piercing-impact.png` and `enemy-death.png` captures under `artifacts/feedback-parity-visual-visible-20261013/`. The death image was inspected: dark red puffs are visible and the former separate generic hit sparks are absent. A hidden-window run also passed its logic checks but produced black screenshots; those images are not visual evidence. The source and port use different renderers and player/enemy art, so pixel identity and frame-by-frame camera motion against a simultaneously captured Godot run remain unverified.

# Rocket launcher explosion parity

The reference is the supplied extracted Brotato source under `E:/UnityProjects/Brotato`. The three rocket launcher tiers use `weapons/ranged/rocket_launcher/rocket_launcher_effect.tres`, `projectiles/explosion.tscn`, `projectiles/player_explosion.gd`, and `particles/explosion_smoke.tscn`.

| Behavior | Source | Scrapshift |
| --- | --- | --- |
| Trigger | 100% chance on a rocket hit | The `effect_explode` trait is rolled on hit. |
| Damage area | Circle radius 147.34 pixels, offset 3 pixels down, scaled by the effect size of 1.2 and the explosion-size stat | Radius 1.76808 world units before stat bonuses, offset 0.036 units down; each enemy can be hit once. |
| Flash | Original 256 pixel sprite, scale 1.36719, tint `(0.788235, 0.788235, 0.788235, 0.392157)` | Same sprite and values; removed at 0.05 seconds. |
| Sound | Four source explosion clips, -10 dB, 0.2 variation | Existing imported clips and sound setting retained. |
| Smoke | Original `particle_3.png`, emitted once at 0.05 seconds, 48 particles (`40 * 1.2`) | Same source image and count; disabled by the Visual effects option. |
| Smoke geometry and motion | 128 pixel emission radius, 24 pixel downward offset, 30 pixels/second initial speed with 0.5 random variation, 720 degrees/second angular speed with 0.73 random variation | Scaled to world units and effect size, with a circular Unity particle emitter and matching speed, spin, random rotation and linear size fade. |
| Smoke color and duration | Dirt arena outline color `(0.470588, 0.403922, 0.345098)`, 0.75 second particle lifetime | Same tint and lifetime; the explosion object is cleaned up at 1 second. |

The source uses Godot CPUParticles2D; Scrapshift uses Unity ParticleSystem. The implementation matches authored values and visible sequence, but random particle positions and rasterization are engine-dependent and are not a pixel-identical rendering claim.

Verify with a Windows build and `Scrapshift.exe -force-d3d11 --rocket-explosion-review <absolute-output-folder>`. The report checks the damage reach, flash state, 48 emitted particles, smoke radius and lifetime, and cleanup. The fixture also captures `rocket-flash.png`, `rocket-smoke.png`, and `rocket-smoke-mid.png` for visual inspection.

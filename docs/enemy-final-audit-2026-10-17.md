## October 17 root validation result

See `gameplay-parity-systems-2026-10-17.md`. Evidence: `artifacts/gameplay-parity-systems-2026-10-17/focused-summary.json`, both XML/log attempts, input manifest and catalog comparison. The same selected checks passed 434/434 on attempt 2, zero failures/skips, Unity exit zero. All 508 recorded input hashes still match the isolated clone after execution. No whole suite, new build, full campaign or physical-controller retest was run. R8 remains the last delivered gameplay build and predates these changes. Earlier pending statements below reflect the handoff before root execution.

# Enemy integration audit — October 17, 2026

Status: source inspection and new regression fixtures; execution pending root integration. No Unity instance, build, full suite or campaign was launched by this audit. The supplied project at `E:/UnityProjects/brotato-(3.x)` was read only.

## Confirmed integration behavior

- `enemy.gd:update_target` visits targetable pets before players and accepts only strictly smaller distance. Its accepted distance is stored in an integer measured in squared source pixels. Scrapshift's `SelectEnemyTarget` preserves that truncation, uses raw origins, and retains target identity for the quarter-second refresh while following its current position. Target death triggers immediate reselection.
- `global/entity_spawner.gd:on_group_spawn_timing_reached` disables loot and then calls ordinary `die()` when removing enemies for performance. Death counters and global callbacks still occur. `RemoveEnemyForPerformance` preserves those callbacks without creating a loot batch.
- `entities/units/enemies/spawner/spawner.gd` and `bloated_spawner/bloated_spawner.gd` emit three and five children respectively in a 200-pixel area around the parent's raw origin. The child body's art offset must be removed when storing its position, so its recovered raw origin remains in that area. `SpawnEnemyChildren` implements that conversion.
- `singletons/zone_service.gd:get_rand_pos_in_area` first clamps the area center with edge plus half-area margin, then samples the square. Source-specific summon and wandering paths use the corresponding sampler. This is separate from custom fixture positioning.

## Predator rotating arm findings

Reference sources:

- `entities/units/enemies/predator/predator.tscn`: nine persistent enemy projectiles, raw pivot `(0,-32)` pixels, target radii 250/300/350/600/650/700/950/1000/1050 pixels; every child explicitly sets damage growth **1.15**, overriding the script default of 1.0.
- `entities/units/enemies/corrupted_tree/pivot.gd`: rotation speed multiplies accessibility speed.
- `projectiles/bullet_enemy/enemy_projectile_rotating.gd`: radial extension also multiplies accessibility speed; `destroy_on_hit=false` keeps the projectile after a contact.
- `entities/units/player/player.gd:check_chal_jellyshield`: a hitbox whose parent is any `Projectile` counts, including this rotating subclass. Counting is not conditioned on health loss.
- `entities/units/pet/scapegoat/scapegoat.gd` and `jellyshield/jellyshield.gd`: each invokes the hitbox callback on entry. The rotating projectile's override retains the orb. Scapegoat takes one damage and disables its hurtbox for 0.6 seconds; Jellyshield plays its hit response.

Root integration now includes raw pivot conversion, accessibility speed, persistent player entry tracking, projectile contact counting and nonconsuming pet contact handling. The previous damage-growth hypothesis was rejected after inspecting the scene overrides; **no growth correction was needed**. The earlier proposed slow-recovery change from 3 to 5 world units per second was also rejected: source `60 * delta * 5` pixels is 3 world units per second.

The initial spawn marker was reported to root separately: its initial position used the art/body center while relocated markers used the raw origin. Root corrected the initial warning to the raw-origin adapter too.

## Focused fixtures authored

`Assets/Scrapshift/Tests/Editor/EnemyIntegrationParityTests.cs` adds **12 cases** covering:

- Raw-origin target ties, quarter-second identity caching with live positions, immediate dead-target replacement, and integer squared-distance truncation.
- Performance-removal counters/callbacks with no pickups.
- Both spawner child counts and raw-area conversion.
- Predator hurtbox/phase eligibility, independent per-orb entry, continuous overlap suppression, hurtbox reentry, raw pivot, and accessibility-scaled radial overshoot.

These are assistant-authored integration checks, not evidence of user controller testing, native sound review or complete gameplay parity. Root owns execution and the shared bug/interview updates.

### First focused execution and fixture repair

Root's first selected run recorded 427/434 passing cases in `artifacts/gameplay-parity-systems-2026-10-17/focused-attempt-1.xml`. This class had one failure: the Bloated Spawner child test reached a missing `dire_junkie` catalog entry. The reference scene explicitly assigns `dire_junkie.tscn`, so the expected child identity was correct. Production `ProgressionRuntime.InitializeProgression` calls `InitializeEncounters`, which appends the extended enemy roster; the fixture had skipped that startup step. The fixture now clones the catalog and invokes `InitializeEncounters` before starting rules. Cloning isolates the registration from the shared asset. No production fallback or catalog regeneration was needed. The same focused selection must be rerun; this repair is not a passing result.

## Bounded unchecked areas

- No enemy scene/resource in the supplied roster explicitly registers charging `target=2` (`RAND_POINT`). Its generic code path exists, but it is not a confirmed active-roster discrepancy; do not invent a fix from that unused branch.
- Godot's radial step checks `< target` before adding the frame increment, which can overshoot a radius by one physics step. Root replaced the analytic radius clamp with per-orb accumulated distances and the same pre-step comparison; the new boundary fixture checks that the overshoot is retained and stops increasing.
- Godot physics contact scheduling, crowd response and frame ordering are not proven identical by helper fixtures. Native enemy/state checks and a fresh full run remain deferred until the confirmed source fixes are integrated.

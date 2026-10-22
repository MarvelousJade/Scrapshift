## October 17 root validation result

See `gameplay-parity-systems-2026-10-17.md`. Evidence: `artifacts/gameplay-parity-systems-2026-10-17/focused-summary.json`, both XML/log attempts, input manifest and catalog comparison. The same selected checks passed 434/434 on attempt 2, zero failures/skips, Unity exit zero. All 508 recorded input hashes still match the isolated clone after execution. No whole suite, new build, full campaign or physical-controller retest was run. R8 remains the last delivered gameplay build and predates these changes. Earlier pending statements below reflect the handoff before root execution.

# Enemy movement and Buffer continuation — October 15, 2026

Read-only reference: `E:/UnityProjects/brotato-(3.x)`. No Unity process, build, full suite or campaign was launched by this subtask.

## State mappings checked and retained

`Boss._current_state` begins at -1. This does **not** mean imported state zero is off by one: `scripts/import_encounters.py:84-87` prepends the default AttackBehavior/MovementBehavior as synthetic state zero, then appends source States children. Source state N is imported state N+1.

| Enemy | Source callback | Imported index and multiplier |
| --- | --- | --- |
| Monk | `monk.gd:8-13`: states 0/1/2 reset speed -50/+25/-50% | 1/2/3 use .5/1.25/.5 |
| Gargoyle | `gargoyle.gd:7-10`: states 0/1 reset speed -25/-40% | 1/2 use .75/.6 |
| Invoker | `invoker.gd:7-8`: state 1 resets speed +150% | 2 uses 2.5 |

These three source stats have zero speed randomization. Existing state speed mappings are correct and were preserved. `Unit.reset_speed_stat:112-120` and `EntityService.get_final_enemy_speed:64-75` multiply the separate state/boost percent factor with effect/accessibility/danger factors, then round the final pixel speed. Existing `EnemyStatSpeed` follows that order. Evil Mob's override (`evil_mob.gd:56-58`) deliberately returns raw `stats.speed`, ignoring normal randomized/modifier speed; its evolution adds a separate negative bonus. That override was retained.

## Integer additive speed differs from final stat rounding

`Unit.bonus_speed` is an int. `charging_attack_behavior.gd:117` assigns `charge_speed * accessibility.speed`; fractional source pixels truncate. `pursuer.gd:56` similarly increments that int on **each** boost, using the configured 45-pixel increment (`pursuer.tscn:78`). These operations must not retain fractional pixels or use round-half-away-from-zero.

Added `EnemyReferenceRules.AdditiveSpeedBonus` for this boundary. Root owns its production call wiring: charge 750 pixels at .75 accessibility becomes 562 pixels (5.62 world units); Pursuer at .5 accessibility gains 22 pixels per boost, totaling 220 after ten boosts. Final current-stat rounding remains separate: the corresponding stat-speed operation rounds 562.5 to 563 pixels.

The proposed slow-decay increase from 3 to 5 world units/second was rejected after checking `unit.gd:158-161`: negative decaying speed recovers at `60 * delta * 5` **pixels**, or 300 pixels/second = 3 world units/second. Root restores 3. This is a unit conversion correction, not a source change.

## Actual Buffer corrections

Both `buffer.tscn` and `corrupted_buffer.tscn` configure BoostZone `collision_mask = 4`, with a CircleShape2D radius of 309.04 pixels at the source root. Their scripts' Player and Structure branches cannot be reached by those scene masks. The actual `BoostNearbyEntity` adapter now includes only eligible enemy bodies, using layer metadata, body circle radius and imported source-root/body offsets. It excludes dead, spawning, already boosted and unboostable enemies. Root-centered visual distances and larger hurtboxes no longer substitute for physical-body overlap. No artwork transform is used as a logical collision scale.

Source normal Buffer grants +150% HP, +25% damage, +50% speed to one target; corrupted Buffer grants the same HP/speed and +30% damage to two targets. Existing count/damage import and raw-speed multiplication were retained. `Enemy.boost:311-318` calls `reset_health_stat`, which recalculates from raw base HP with modifiers before final rounding. The adapter now does likewise rather than multiplying an already-rounded maximum. Example: raw HP 19 with +20% enemy health gives 23 before boost, but boosted HP is `round(19 * 1.2 * 2.5) = 57`, not 57.5. Source reset also fills current HP to that new maximum, which is retained.

The public `RunRules.EnemyBoost` helper preserves source Player.boost arithmetic for generic direct fixtures, but the supplied Buffer scenes cannot activate it. The two earlier direct player-boost cases were renamed/labeled accordingly; their result is not evidence of normal Buffer-to-player gameplay. Existing generic structure boost helpers likewise remain separate from this scene-mask eligibility.

## Verification

New `EnemyNextParityTests` contains twelve authored cases: both Buffer scenes' real method paths beyond the former origin-only range, both scenes excluding a nearby player, physical radius/offset/layer boundaries, normal/corrupted damage scalar and raw-HP rounding, no repeat boost, dead/spawning/unboostable guards, charge truncation, per-increment Pursuer truncation and the two raw point samplers below. Root handles focused Unity validation and actual movement call wiring. Dynamic Pursuer shape growth, asynchronous Area2D entry ordering and ordinary native play remain distinct from this bounded base metadata/rule evidence.

## Raw coordinates and source point distributions

Root's adapters consistently represent an enemy source root as `enemy.pos + OriginOffset(id)` and the player source root as `playerPos + down*.24`. QueueGroup, encounter summons and split-spawner births subtract the **child's** origin offset after generating the raw spawn point. No additional production `Birth` push lacking that conversion was found. Ordinary Spawn calls in explicit visual fixtures intentionally receive adapted centers.

`ZoneService.get_rand_pos:104-111` defaults to a 64-pixel edge inset (`Utils.EDGE_MAP_DIST`), with no rejection near the player. `get_rand_pos_in_area:123-131` clamps the **center** inside that inset plus half the square's side, then samples the full square. Added pure `SpawnPlacement.SourceRandomPosition` and `SourceAreaPosition` for those distributions. Input rolls keep source ordering, including the Y flip: a zero Y roll samples the world's top edge. Tests establish the base-map endpoints, valid center sample and outward-facing area clamping. Root owns production wiring to random destinations and summon paths. Imported negative summon spread (-.01 from source sentinel -1) means no local scatter and must bypass the area helper.

Charge RAND_POINT uses its parent's full minimum/maximum bounds, so it is intentionally separate from ZoneService's inset sampler. Source target-forward logic reads the actual target's movement, but no imported attack registers that mode; this generic distinction was reported without broadening the patch. The target cache's .25-second update and pets-first ordering were checked; the source additionally stores each accepted nearest squared **pixel** distance in an int, which root handles separately from these samplers.

Source contact callbacks are queued through `Unit._on_Hurtbox_area_entered:610-611` and its deferred handler does not recheck disabled hurtbox state. Root handles a shared contact-batch eligibility snapshot across enemy and projectile contacts; damage invulnerability remains a separate guard. Initial/normal shooting cooldown order and charge cooldown running concurrently with the dash were checked without finding an additional confirmed cooldown correction in this subtask.

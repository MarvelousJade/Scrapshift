# Projectile count, bounce, and piercing

Compared with the supplied extracted Brotato source at `E:\UnityProjects\Brotato`: `singletons/weapon_service.gd`, `weapons/shooting_behaviors/ranged_weapon_shooting_behavior.gd`, `projectiles/player_projectile.gd`, `overlap/hitbox.gd`, and `global/entity_spawner.gd`. See also [Godot 3.6's integer conversion](https://docs.godotengine.org/en/3.6/classes/class_int.html): assigning a positive fractional value to an integer removes its fractional part.

## Added projectiles

`WeaponService._set_common_ranged_stats` adds the player's `projectiles` effect to a positive base `nb_projectiles` and adds 0.1 radians of spread per extra projectile. A base count of zero remains zero. `RangedWeaponShootingBehavior.shoot` fires that many independently randomized directions in the resulting spread interval. Renegade's starting effect adds two projectiles, so a one-shot pistol fires three shots and its spread increases by 0.2 radians. The same common stat initialization applies to ranged structures and pets. Scrapshift now uses the count and spread rule for held ranged weapons, turrets, and ranged pets. Special item spawns such as Baby With A Beard use their explicit spawn counts; their source path calls `manage_special_spawn_projectile` for each spawn.

## Bounce

On a hit, the original projectile ignores the enemy it just struck. A remaining bounce takes priority over piercing. It selects another random ordinary living enemy from `EntitySpawner.enemies`, redirects from the contact position toward that enemy, preserves speed, clears knockback, extends range, and consumes a bounce. Bosses and neutral trees are outside that target pool. If no target exists, it chooses a random direction. It reduces the projectile's integer hitbox damage by its capped bounce damage reduction, with a minimum of one. Particle Accelerator tiers III-IV forbid bouncing even if the player has bounce bonuses. Scrapshift already had the priority, speed, range, knockback, and `canBounce` gate; it now redirects at the contact point and uses the source target pool.

## Piercing and damage modifiers

When no bounce remains, a piercing value of one allows one more enemy hit. Each pierced enemy is ignored thereafter. Damage for the *next* hit is reduced by the weapon's base `piercing_dmg_reduction` minus `piercing_damage / 100`, clamped to a reduction from zero to one. The integer hitbox truncates fractional results and retains at least one damage. The same calculation uses `bounce_dmg_reduction` and `bounce_damage` for bounces. Critical hit effects can grant an extra pierce or bounce before this decision.

For a weapon with the common 50% piercing loss and 11 current damage: Pumpkin's +15 piercing damage changes the loss to 35%, so the next hit receives 7; Sharp Bullet's -20 changes it to 70%, so the next hit receives 3. Those modifiers affect damage retained after a pierce; they do not add a pierce by themselves. Bandana's +1 piercing lets an ordinary shot hit a second enemy.

## Verification

The Direct3D 11 Windows build succeeded with zero errors and warnings. The built-player `--projectile-parity-review` fired three shots from a pistol with +2 projectiles, checked spread, bounced an 11-damage shot from its contact point into a second enemy for 5, and pierced two targets for 11 then 7 with +15 piercing damage. Both screenshots were inspected. All nine focused Unity EditMode tests passed, as did the full starter/deployable and companion reviews (27 companion cases). Unit checks cover zero-base projectiles, negative added count, source spread, and integer damage reductions. The fixture uses controlled stationary enemies; random target selection in a crowded wave remains stochastic.

# Item and status parity audit — October 13, 2026

Reference: the supplied local Brotato Godot 3 source at `E:/UnityProjects/brotato-(3.x)`, especially `singletons/weapon_service.gd`, `singletons/entity_service.gd`, `entities/units/unit/unit.gd`, `entities/units/enemies/boss/boss.gd`, and the registered item resources. Scope: the base game, single player. The branch began with unrelated uncommitted enemy and UI changes; this audit preserves them.

## Coverage

- `scripts/verify_catalog_parity.py` compared 4,918 imported fields against fresh source resources: all 209 registered items, 205 weapon tiers, and 50 characters. There were zero value differences. The check includes item tiers, prices, tags, limits, and ordinary scalar effects; it does not execute special effects.
- The 209 item definitions contain 60 distinct immediate scalar keys and 35 distinct conditional effect families. The runtime applies ordinary item effects on acquisition, evaluates movement, health, inventory, and linked conditions while a run changes, and dispatches timed, hit, dodge, kill, level, and wave events. Existing EditMode tests exercise the item catalog and these hooks.
- Traced stat consumers through weapon damage, cooldown, range, critical chance, lifesteal, knockback, projectile count/pierce/bounce, burn, explosions, structures and pets; enemy health, speed, damage, spawn counts and debuffs; and HP, regeneration, dodge, pickups, shop prices and rerolls. The focused regressions in `ItemStatusParityTests` cover the discrepancies below.

## Corrections

| Status path | Source rule | Correction |
| --- | --- | --- |
| Alien Baby `enemy_health` to bosses and elites | `EntityService.get_final_enemy_health` applies the same item factor to every enemy before `Boss.init` applies its wave health adjustment. | Use one `EnemyHealthFactor` for every enemy role. A +10 status now gives +10% to boss and elite base health, instead of +1%. |
| Snail `enemy_speed` | `Unit.reset_speed_stat` passes `1 + enemy_speed / 100` through without a 1% floor. | Permit a total -100 status to reduce enemy speed to zero. |
| Enemy damage status | `EntityService.get_final_enemy_damage` floors the item factor at 1%. | Use the 1% floor in `EnemyDamageFactor`. |
| Character Max HP gain | `RunData.get_stat` multiplies the starting 10 HP as well as later bonuses, and `get_player_max_health` stores an integer. | Include base HP inside the gain calculation and truncate Max HP. Linked stat sources use the same base and gain order. |
| Weapon scaling and Damage status | `WeaponService.apply_scaling_stats_to_damage` truncates the scaled base to an integer; the final percentage product is rounded, with minimum damage 1. | Match both integer boundaries for ordinary and item-spawned weapon projectiles. Explosion Damage remains additive with ordinary Damage before the final round. |

## Verification and limits

An isolated copy passed 348/348 EditMode tests and built a Windows player with zero errors and warnings after the final Max HP correction. The copy was necessary because Unity reported another Editor instance using the working checkout. The data check passed 4,918/4,918 fields.

This checks imported values exhaustively and verifies the named behavior changes with tests. It does not establish identical random sequences, collision frames, enemy targeting, companion movement, or complete frame-by-frame behavior across engines. The user's existing two-weapon start and Scrapshift presentation also remain different from the source. Other pre-existing changes in the checkout were included in the isolated test/build copy but are outside this status audit.

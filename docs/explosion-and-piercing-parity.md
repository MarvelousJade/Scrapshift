# Explosions, piercing, and projectiles on enemy death

Source comparison: the supplied extracted Brotato project at `E:\UnityProjects\Brotato`. The relevant paths are `projectiles/explosion.tscn`, `projectiles/explosion_smoke.tscn`, `projectiles/flesh_explosion/flesh_explosion.tscn`, `projectiles/player_projectile.gd`, `singletons/weapon_service.gd`, `singletons/run_data.gd`, and `main.gd`. Scrapshift uses a Unity implementation of those rules and imported source art/audio.

## Which weapons explode?

The base roster contains 21 exploding weapon and tier combinations. Their explosion traits, including chance, size, damage, and source smoke count, are imported into `weapon-reference.json` and the equipment catalog. All use the base explosion flash and smoke scene, with tier-specific size and smoke count:

| Weapon | Exploding tiers | Source smoke particles |
| --- | --- | --- |
| Rocket Launcher | II-IV | 48 |
| Shredder | I-II, III-IV | 8, 9 |
| Plasma Sledgehammer | III, IV | 10, 13 |
| Power Fist | III, IV | 10, 25 |
| Nuclear Launcher | III, IV | 80, 90 |
| Plank | I-IV | 8 |
| Fireball | II-IV | 10 |
| Dextroyer | IV | 31 |

The source flash and damage window last 0.05 seconds. Smoke starts when they close, lives 0.75 seconds, and the explosion object cleans up after 1 second. Smoke amount comes from the source effect scale times base amount, rounded as Godot rounds positive half values. Explosion Size changes visual and damage radius, while Explosion Damage changes damage. It does not multiply smoke count. Fireball's source explosion sound is disabled.

## Which items and companions explode?

| Trigger | Effect | Source smoke particles |
| --- | --- | ---: |
| Rip and Tear, enemy death | Red flesh explosion, separate sprite and sound | 1 |
| Spicy Sauce, consumable pickup | Base explosion | 25 |
| Bull, taking damage | Base explosion | 20 |
| Glutton, consumable pickup | Base explosion | 40 |
| Landmines or Bot O Mine's mine, on detonation | Base explosion | 70 |
| Rocket Turret projectile, on enemy contact | Base explosion | 20 |
| Bonk Dog, on impact | Base explosion | 60 |

The item's explosion hit area is a circle with the source's downward collision offset and enemy hurt radius included. The same overlap rule now applies to the mine, rocket turret, and pet explosion. When multiple copies of one exploding item are owned, `RunData.handle_explode_effect` sums their chances and damage, rolls once, and creates one explosion using the first effect's appearance. The position varies by up to 10 source pixels on each axis. Scrapshift follows that sequence. A normal enemy death blood burst is a separate hit effect, not a Rip and Tear explosion.

## Piercing and the death projectile

In `player_projectile.gd`, a hit adds that enemy to an ignored list. If bounce remains, bounce happens first. Otherwise the projectile stops when piercing is exhausted; if a pierce remains, it consumes one and reduces damage for the next enemy, with a minimum of 1. Scrapshift resolves collisions in travel order, ignores already hit enemies, and applies this sequence. A pierce value of 1 allows two enemy hits. The built-player check dealt 100 to the near target and 50 to the far target from one bullet.

The remembered item is **Baby With A Beard**. `main.gd` spawns one projectile per killed enemy in a random direction (more copies add projectiles). Its source ranged stats use the default bullet scene, 1 base damage plus 100% Ranged Damage, 30 world units per second, and player piercing bonuses. Scrapshift already spawned the shot and excluded the dead enemy from its hits. The special-spawn projectile now resolves the source's default bullet art and animation as well.

## Verification

The Direct3D 11 Windows build completed with zero errors and warnings. The built-player `--explosion-parity-review` passed its 21-profile inventory, Shredder (9) and Nuclear Launcher (80) smoke emission, stacked Rip and Tear red smoke, Rocket Turret and mine metadata, Baby With A Beard bullet scene, and two-target piercing checks. The existing rocket fixture passed flash, hit area, 48-particle smoke, and cleanup. Captures in the review folder were inspected. The checks reproduce source values and event order; random particle positions and final pixels may differ between Godot and Unity.

# Shop presentation and final boss audit

Request: remove decorative tier strips, use reference tier signals, round cards, add money icons, replace pale ground outlines, measure weapons, restore spawn-warning audio, and correct the final bosses. Follow-up: bullet special weapon effects such as burning damage.

## Reference and decisions

Read the supplied `E:/UnityProjects/brotato-(3.x)` project directly. Compared the user's three shop/upgrade screenshots with built-player captures. No original game artwork was imported in this pass. The cream mascot, weapon sprites and cybernetic item presentation remain.

- `singletons/progress_data.gd` / `item_service.gd`: white, blue (#5ABEFF), purple (#AD5AFF), red (#FF3B3B); weapon II/III/IV suffixes, dark tier backgrounds and tinted inventory tiles. Removed our extra top stripes and colored weapon rims. Black tile outlines and white interaction feedback remain. Rounded the shared card renderer, stats background and icon backplates.
- Shop currency is larger, prices and rerolls have currency icons, and category text follows the measured title height. Special weapon traits now start with a bullet; primary item modifiers retain their stat icons.
- Removed runtime pale outlined machine plates. Their positions now contain solid irregular stones with muted facets; existing grass tufts remain. No increase in clutter count.
- `entities/birth/entity_birth.gd`: plays `birth_begin_sound.wav` when a crosshair appears. Imported that user-authorized reference sound and attached a new appended enum cue, preserving serialized IDs. The local mixer still applies gain, voice limits and a short cooldown; this is not a claim of identical audio mixing.
- Removed the artificial charge-direction line; enemies retain anticipation tint/squash.

## Weapon scale measurement

`scripts/measure_weapon_presentation.py` reads each base-family scene's sprite texture, alpha bounds and local scale, and exports numeric metrics only. All 62 families are covered. The reference body alpha width is 59 pixels; the project uses 100 reference pixels per world unit.

Pistol is 65 px, fist 44 px, knife 77 px, spear 162 px. The previous fallback made almost every weapon 65 px. The new per-family dimensions reduce small weapons and restore deliberately longer weapons. A universal reduction would make several weapons less accurate. Sprite styling/aspect differs; these are visible bounding-extent comparisons, not identical silhouettes or attack hitboxes.

Evidence: `artifacts/presentation-boss-review/weapon-measurements.json`. Tests verify all catalog tiers use the family dimension. Weapon damage, reach and attachment positions are unchanged by this presentation adjustment.

## Final bosses: what was wrong

Invoker previously spawned immediately damaging stationary plasma orbs lasting three seconds. The source uses a pillar animation:

| Property | Reference and corrected behavior |
|---|---|
| Warning | Harmless until 0.54 seconds |
| Damage window | 0.54 inclusive to 0.68 exclusive |
| Collision | 23 px radius, center offset 4 px upward |
| Fade/removal | Shrinks after 0.68; removed at 1.04 |
| Contact | EnemyProjectile stops on hit |
| Initial pattern | 10 pillars in a 500 px square; first on the target |
| First mutation | 30 pillars in an 800 px square; first on the target |
| Final mutation | 24 equally spaced pillars on a 300 px radius ring |
| Mutation triggers | 75% HP or 30 s; 40% HP or 60 s, checked once a second |
| Final movement | Random destination, +150% base speed (2.5×), not a fixed speed of 150 |

Sources: `invoker.tscn`, `invoker.gd`, `boss.gd`, `unit.gd`, `entity_service.gd`, `shooting_attack_behavior.gd`, `pillar_projectile.tscn`, `enemy_projectile.gd`. The spawn-spread field is a diameter, and non-border scatter is square, not a disk. Also removed the extra artificial 0.1 s ranged recovery tail. The source's full angular spread is divided across the projectile count rather than applied as the spacing between every projectile.

Predator already had its 0.8 s preparation, 0.5 s charge, +750 px/s charge speed, 50% HP/45 s mutation, nine expanding orbit projectiles, 1.2 rad/s pivot and 25% mutation acceleration. Audited these against `predator.tscn`, `predator.gd`, `pivot.gd` and `enemy_projectile_rotating.gd`. Corrected the pivot and bullet origin relative to our body-centered art, and prevented the parent's spring animation from scaling/tilting orbit projectiles. Invoker now shares the matching boss collision offsets instead of ordinary-enemy offsets.

Pillars use original procedural cybernetic warning/energy visuals. This pass matches the audited pattern and timing; it does not claim pixel-identical effects or complete engine-level combat parity.

## Debugging and verification

The first render exposed prices wrapping inside their buttons once the icon reduced text width. Disabled wrapping and added measurement padding. Review captures also picked up an unrelated live mouse hover, obscuring the shop; the deterministic fixture now uses its explicitly selected inspection targets only.

The expanded built-player fixture checks harmless warning versus actual health loss, projectile consumption on contact, the 24-pillar ring and final speed, plus Predator's real charge release, phase change, nine orbit objects and 16-projectile volley. It captures warning, active, fade, ring, Predator and four-tier shop screens. These are controlled runtime checks, not a human survival/balance playthrough.

Final results and build paths are recorded in `verification.md`. Earlier catalog/progression verification remains separate; this pass does not add or remove content.

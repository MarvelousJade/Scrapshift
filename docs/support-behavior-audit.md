# Support behavior audit — 2026-10-13

Inspected the local reference project at `E:/UnityProjects/brotato-(3.x)`, including active scene resources, scripts, collision masks, and animation tracks. Values use 100 reference pixels per world unit. Scrapshift keeps its own artwork and sound assets.

## Behaviors implemented and checked

| Support | Reference behavior used |
| --- | --- |
| Bonk Dog | Same-kind target reservations; nearest enemy within 600 px, global fallback; periodic contact attack; half-second exponential leap toward the highest-max-HP eligible enemy; landing explosion; retry after 40% cooldown when no jump target exists. |
| Catling | Random-map wandering; stops within 150 px of the player or another non-cat pet; two independently aimed guns with alternating half-interval timers; proximity halves both timers; cats do not boost other cats. Visible paws are integrated into the new body, with separate shoulder-mounted guns and recoil. |
| Bot-o-mine | Follows the player, stopping within 100 px; ranged attack and engineering cooldown; mines placed within a 100 px square around the bot. |
| Blazemander | Prefers non-burning targets within 500 px; same-kind target reservation with shared elite targets; approximately 82 px fire trigger radius; six radial projectiles; 0.15-second contact-attack preparation. |
| Ratzilla | Same-kind target reservations; does not target bosses/elites; periodic contact hitbox, max-HP damage scaling. |
| Doc Moth | Patrols a circle with radius 80% of map half-height; visible 200 px healing zone; overlapping zones add regeneration healing bonuses; life-steal doubling remains a single effect. Wings retain attached hinges. |
| Lootworm | Active scene seeks the nearest tree first, then materials. Claims materials within 200 px and accelerates them toward itself; collects within 60 px; doubling chance and tree damage remain active. |
| Scapegoat | Random-map wandering; attracts nearby enemies; takes one damage per hit with 0.6-second invulnerability; downed goats stop blocking; nearby revival preserves progress and restores the wave-scaled maximum health. Health display follows the boss-health-bar setting. |
| Jellyshield | Equal angular spacing for duplicates, 100 px orbit, one radian/second; hostile-projectile blocking and hit pose. |
| Basic/flame/rocket/laser/healing turrets | Upright connected assembly; 0.3-second squash/recoil track, shot at 0.2 seconds, target-directed projectiles, randomized 70–130% cooldown and burst reload handling. Flame direct damage is fixed at one; burning carries the main damage. Healing turret targets the player. |
| Tyler / wandering bot / garden | Enemy-only Tyler targeting; wandering bot orbit and contact slow; garden fruit production. |
| Mines | Player/enemy pressure contact, pressed state on entry, detonation on exit; critical chance and knockback included in explosion. Reference mask 6 excludes pets. |
| Pocket Factory | Destroyed trees deploy functioning turrets. |

## Evidence

- Pet scripts and scenes: `entities/units/pet/{bonk_dog,catling_gun,bot_o_mine,blazemander,ratzilla,doc_moth,lootworm,scapegoat,jellyshield}/`.
- Target selection: `entities/units/target_behavior/closest_enemy_target_behavior.gd`, `closest_non_burning_enemy_target_behavior.gd`, `lootworm_target_behavior.gd`; `singletons/pet_service.gd`.
- Movement: random-position, follow-target, and map-patrol movement scripts referenced by those scenes.
- Spawn grouping: `singletons/pet_service.gd`, `global/entity_spawner.gd`.
- Structures: `entities/structures/turret/turret.gd`, its shoot animation resource, variant scenes, and `entities/structures/landmine/landmine.gd` / `.tscn`.
- Enemy decoy targeting: `entities/units/enemies/enemy.gd`.

## Verification and limits

304 EditMode tests pass. Native `--pet-review` checks all nine pets plus duplicate targeting, cat proximity/alternating guns, moth stacking, Lootworm priority, decoy revival, ten structure cases, and player-triggered mines (27 checks). Close-up captures and machine-readable results are in `artifacts/support-parity/`.

This is verified combat behavior alignment, not a claim of complete frame-for-frame equivalence. Artwork proportions, limb geometry, particles, and sounds remain Scrapshift adaptations. Reference end-of-wave petting/purring sequences, co-op-specific interactions, and exhaustive combinations of every modifier are not covered by these checks. Ordinary turrets intentionally do not rotate their upper body: their reference script aims the projectile while the whole assembly recoils upright.

Run `Builds/SupportParity/LaunchSupportPreview.cmd` for the playable all-support scene. It uses isolated fixture progression, gives the player no weapons and invulnerability, and keeps the wave running. WASD/arrows move, Esc pauses, F5 resets.

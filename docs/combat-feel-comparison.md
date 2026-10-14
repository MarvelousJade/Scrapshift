# Combat feel comparison - October 11, 2026

The user requested a comparison of enemy movement, hitboxes, knockback and damage effects against the running Brotato reference, preserving Scrapshift's artwork.

## Evidence

Reference folder: `E:/UnityProjects/brotato-(3.x)`, running in Godot 3.6.2, displayed version 1.1.15.4. Codex started Well-Rounded, pistol, Danger 0 through the game UI. A stationary first-wave sample showed converging baby aliens, close contact pressure, red damage numbers and a prominent white outlined impact burst. At one captured instant the player had 9/15 HP with 12 seconds remaining; the sample ended in death with nine seconds remaining. This establishes visual/contact observations, not a successful playthrough, input-latency benchmark or frame-timed measurement of knockback. Exact rules below come from the supplied source. Well-Rounded has 15 starting HP and a +5% speed bonus; our default starts with 10 HP. These samples are not a controlled balance comparison.

No reference gameplay code, settings or artwork was modified for this pass. No reference raster image or sound was added to Unity. Existing original sprites, wardrobe, legs, enemy designs and sound assets remain in use. Impact stars are new procedural geometry using the existing serialized sprite material.

## Biggest differences and fixes

| Area | Reference evidence | Previous Unity behavior | Change |
|---|---|---|---|
| Collision purpose | `enemy.tscn` separates physical Collision, Hurtbox and Hitbox; `baby_alien.tscn` overrides physical radius to 22px while inherited hurt/contact radii are 32/16.0312px | A single role-based radius served crowd spacing, incoming shots and contact | Explicit profiles for all 18 enemies. Baby spacing/hurt/contact radii are .22/.32/.160312 units. Tree's .486621 hurt radius is inherited through `neutral.tscn`; trees have no contact damage. |
| Player contact | `player.tscn`: Hurtbox radius21, net position(0,-12), neutral body center(0,-24) | .30 player contact radius and body-centered checks | Radius .21 and body-relative downward offset .12. Enemy hurt/contact centers use inherited scene offsets relative to a neutral -24px animation origin; boss centers sit .72 below its body center. These circles remain independent of visual spring/squash. |
| Projectile collision | `projectile.tscn`: radius44 and Collision.x23; concrete bullet/laser scenes offset Hitbox.x-23, cancelling that lead | .10 bullet margin; reversed list traversal could hit a farther enemy first | Radius .44, body-relative enemy hurt centers, swept circle entry ordered by travel distance. Concrete projectile centers have no forward lead; their negligible 1px lateral offset is omitted. Piercing applies the half-damage reduction after each ordered hit. |
| Edge shots and range | `player_projectile.gd` adds100px to automatic-fire range; projectiles leave the padded zone | Old ±5.25 y cutoff deleted bullets in the restored arena; lifetime used only targeting range | Shot lifetime includes +1 unit, movement caps at remaining lifetime, and cleanup uses map bounds plus1 unit. Muzzle now uses the actual rendered weapon scale/position. Target acquisition range is unchanged. Padding is a symmetric adaptation, not exact camera-rect parity. |
| Knockback force | `unit.gd`: .2 attack direction + .8 player-to-enemy direction; force times100px, reduced by resistance | Imported force already divided by 10, then multiplied by3; pistol4.5 instead of15 units/sec | Restore conversion factor10 and direction blend. Source pre-step .9 damping at60Hz is integrated across variable frame durations. An unresisted aligned pistol hit travels .225 on its first60Hz step and approaches2.25 units of push, excluding normal enemy movement. |
| Contact during push | `enemy.gd`: disable contact while push velocity exceeds movement velocity | Disabled only above a fixed1 unit/sec | Compare push against the enemy's current movement speed, including charge and temporary slow. Heavy/resistant enemies retain their resistance. |
| Enemy recovery after contact | `enemy.gd` `_on_hit_something`, `unit.gd` `add_decaying_speed`: negative speed modifier, recovering300px/sec | Enemies kept full pursuit speed after contact | Add the source-style contact slowdown, including its .7/.45/.9 guards, recovering3 units/sec. This creates a brief chance to move away after a hit. Existing follow, keep-range, destination and committed-charge behavior is retained. Crowd separation now uses physical radii. |
| Player protection | `player.gd` `get_iframes`: clamp((damage/maxHP)*.4/.15,.2,.4) outside endless | Fixed .6s after damage, .15 after dodge | Protection lasts .2-.4s based on actual HP lost; a dodge receives .2s. Do not describe this as a pure difficulty reduction: tighter contact areas and stronger push are paired with shorter protection. |
| Hit readability | `unit.tscn` FlashTimer .1s; `hit_effect.tscn` three frames at20fps; `screenshaker.gd` .1s shake | .08 enemy flash, no full player flash, sparks but no distinct impact star, .025 global critical/hurt pause | .1 white flashes including player appearance layers, .15s outlined impact stars, and source-scaled shake capped .03 (.05 for player damage), with no added global hit-stop. The October 13 pass replaced the camera's symmetric oscillation with the source's positive random offset and stronger-hit retrigger rule. It removed generic coral/amber sparks from normal enemy hits while retaining the original hit star, sounds, text and death shrink/spin. Reduced-motion disables camera shake and star expansion. |

## Implementation and design boundaries

`CombatGeometry` contains reviewable collision profiles, force/decay, contact and protection rules. `ReferenceCombat` uses them for movement, damage and ordered projectile sweeps. `CombatFeedback` owns impact lifetime and uses the existing `_Flash` shader property; cleanup clears effects and flash state. Cached renderer-list storage avoids a fresh array for each player-flash update.

Collision centers are adapted from the reference's neutral pose to our body-centered coordinate system. Original sprites have different silhouettes/aspect ratios, so this is not a pixel-identical collision port. Physical crowd response remains our soft separation instead of Godot rigid-body collision/mass/slide response. Enemy root movement targets also remain body-centered. Existing melee attacks still apply a broad instant segment rather than the reference's timed moving melee hitbox. Enemy speed randomization, attack timing, resistance and wave composition were already imported and remain in place. Broader weapon stat accuracy, full armor rounding, all effects/items and normal twenty-wave balance are not covered by this pass.

The minimum death push uses force15 for low/zero-force weapons, matching the source minimum before base resistance; our existing shrink/spin remains an adaptation. The October 13 pass also imports weapon-specific effect scales for enemy blood particles. No subjective claim of identical feel is made.

## Validation and reproduction

New `CombatFeelTests` cover all-profile availability, distinct collision sizes, lower-body offsets, force/resistance/direction, equivalence at20/30/60/120Hz, speed-based contact enablement, slowdown, protection limits, ordered swept intersections/grazes and north/south projectile lifetime bounds.

`--combat-feel-review <folder>` supplies fixed baby aliens with 1000 HP and controlled shots. It checks survival of a projectile above the old y cutoff,100/50 damage order for piercing, actual renderer flash, knockback displacement, safe versus damaging contact distance, post-contact slowing, second-hit blocking and player appearance flash. It advances simulation explicitly to capture short-lived effects reproducibly. It is an integration/rendering fixture, not normal gameplay.

`--enemy-review <folder>` exercises warnings, committed charge, ranged preparation/projectiles and then supplied six-weapon combat. `--campaign-check <folder>` uses invulnerability, supplied gear and accelerated scripted movement to exercise twenty waves and purchases. Exact final results are recorded in `verification.md`; neither fixture establishes ordinary difficulty, performance benchmarking or human acceptance.

For manual comparison, circle around a small group with one pistol; check grazes above/below the player, pressure just after contact, pistol versus SMG push, charged-enemy contact during weak push, and shooting toward either vertical map edge. Compare ordinary and reduced-motion modes. These human checks remain open.

## Interview discussion

The core debugging lesson is that a visible sprite, a collision body and a damage area are different contracts. A single convenient radius coupled crowd behavior to damage and targeting. Another defect came from a partial unit conversion: the importer divided knockback by 10, while runtime used an arbitrary multiplier 3. Reading both sides of that boundary revealed why tuning enemy speed alone could not fix the weak push.

Codex performed source/UI inspection, implementation and automated verification under Shaoyu's direction. Shaoyu requested the comparison and preservation of original art. These notes document the actual work and its limitations; they are not evidence that the candidate independently wrote or personally tested every feature. No invented test failure or user acceptance should be added to an interview story.


### Capture validation correction

The first final-build launches were hidden, so Unity reported screenshot failures even though gameplay assertions passed. Older files in one output directory exposed why checking capture requests alone is insufficient. The three rendered fixtures now require fresh PNG files from the current run. The regression covers missing files, invalid data and stale screenshots. Recovery evidence is retained in artifacts/combat-capture-recovery/hidden-results.json; final visible-window runs supersede those rendering claims.

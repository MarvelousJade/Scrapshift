# Full base-game implementation work log

Scope confirmed by the user: all 209 registered base items, all 62 weapon families, all 50 base characters and their actual unlock behavior. Five characters begin unlocked, matching this supplied reference (including Mage). Keep our cream mascot, cybernetic aliases and original artwork. No DLC injection. Imported records alone do not count as implemented behavior.

## Source inventory

`scripts/audit_full_base.py` archives the 50 registered characters and 116 registered challenge records into `artifacts/base-roster-unlocks.json`, with resource hashes and resolved effect definitions. The existing item archive covers all 209 item identities. This inventory includes the newer base-game pets and characters in the supplied backup; it does not infer the roster from an older online count.

## Original plan (historical; superseded by the completion entry below)

- Special weapons: implementing Screwdriver mine deployment, Pruner gardens, Particle Accelerator's engineering-scaled decaying slow, Vorpal Sword's 1/2/3% execution chance, Thunder Sword's delayed slowing projectiles, Torch burning spread, and Rail Gun's damage-growth/reset timer.
- Remaining 90 items: requires linked stats/caps, pickup/damage/dodge/death events, single-wave effects, structures, pets and economy/spawn hooks. They remain excluded until their complete effect set is supported.
- Remaining 46 character profiles: requires general starting loadouts, restrictions and weapon capacities, linked stats, special economy/combat rules, roster presentation, persistence and challenge event tracking. The five-starting-character decision supersedes the earlier four-character request.
- Unlock challenges that depend on difficulty require the corresponding difficulty rules, not a fake completed flag. Source challenge IDs, thresholds and reward IDs must be preserved independently of display aliases.

## Weapon source notes

Compared `railgun.gd`, unit damage/burning/decaying-speed code, `burning_particles.gd`, delayed taser projectile code, structure spawn scheduling, landmine body-exit explosions and turret/garden code.

Rail Gun adds flat base damage before scaling and percentage Damage, restarts its whole-second count on damage (but does not restart the Timer's fractional phase), and clears between waves. Particle slowing has diminishing returns above 45% accumulated slowdown, rejects more slowdown above 70%, and limits a single addition to 90% of base speed. Source pixels convert at 100 pixels per Unity unit. Regression tests caught a floating-point floor error at the 90% boundary; the calculation now truncates in pixel space. Vorpal checks before death resolution, without adding a boss immunity absent from the supplied unit code.

Burn spread is a chain: transfer the remaining count minus one to one unburned nearby enemy, then consume the originating enemy's spread. Reapplying burn preserves the stronger damage and longer remaining duration rather than replacing them with a weaker hit. Thunder Sword's separate slow overlap applies immediately; its damaging hitbox has a random 0–3-frame delay. Gardens use tier intervals 15/14/12/10 seconds with the source turret cooldown jitter. Mines spawn at wave start and then at 12/9/6/3 seconds, press on contact and explode on exit. Structures scale with Engineering without ordinary player percentage Damage.

Weapon-stage validation: Windows build clean; 206/206 EditMode tests passed. Rendered fixture passed all 160 currently obtainable tiers plus garden fruit, mine exit detonation, burn spread, engineering slow, delayed thunder projectiles and forced Vorpal execution. The first slow fixture used a stationary zero-speed target; this was corrected to a real positive speed, matching the source percentage calculation. This is Codex-assisted engineering work; automated fixtures do not establish human balance or total parity.

## First item group

Added 16 items using source stat links, HP/speed purchase-time caps, self-damage and burn spread: Blood Donation, Bloody Hand, Coil, Community Support, Esty's Couch, Fairy, Fried Rice, Handcuffs, Lucky Coin, Padding, Power Generator, Retromation's Hoodie, Shackles, Snake, Stone Skin and Strange Book. Cybernetic aliases are in the name map. Links exclude other linked values to prevent circular amplification, use signed divisors and truncate after multiplication; Fairy counts distinct item IDs, not stacks.

Current stage: 135/209 items (110 source-unlocked), all seven special-weapon mechanics implemented. 74 items and the full roster/unlock integration remain in progress. The combined build passes 213/213 tests with zero build errors/warnings. This is an incremental checkpoint, not completion of the full requested scope.

## Pickup, survival and economy item group

Added 21 more complete item records with cybernetic names, reaching 156/209 items (127 initially available; 53 still missing). Compared the reference player damage/healing code, common material pickup effects, wave-start HP/interest, shop free-reroll accounting, unit burning interval and group spawn count logic.

Damage shields consume only after a failed dodge and refill each wave. Torture replaces regeneration with five HP each second and disables other healing. Weird Ghost's starting-HP override is consumed once; Sad Tomato applies every wave. Pickup duplication includes the material's banked bonus and XP, while pickup healing rolls once per pickup. Interest uses unspent materials when the next wave begins. Newly purchased free-reroll implants grant their roll immediately; free rolls do not raise the paid price. Spawn modifiers apply before fractional spawn rolls, leave loot aliens unchanged and add trees per tree group. Material-drop reductions retain the reference's half-value floor.

Validation: isolated Windows build succeeded with zero errors/warnings; 224/224 EditMode tests passed after fixes. A regression test exposed float-versus-double rounding at the critical-heal probability threshold, fixed by evaluating the percentage as a double. A second test had incorrectly expected an additive +100% material modifier to leave the value unchanged; corrected its expected result. No human balance or full-roster parity claim is made.

## Five initial characters and combat-triggered implants

Implemented Mage as Arc Weaver, the fifth initially unlocked profile. Its eight starting choices, +25% elemental gains, zero melee/ranged gains, half Engineering gains and starting Snake/Scared Sausage effects come from the supplied character resource. The shared cream body and existing face/beard layers remain our artwork. The user's earlier two-starting-weapon decision is preserved, including the additional default Pistol; that is an intentional deviation from the reference for characters without a bonus starting weapon.

Global burning merges base damage and chance across implants, retains the longest duration and applies elemental scaling once. Native-burning weapons keep their native chance/duration/scaling and gain the global base damage. Added Scared Sausage and thirteen more combat-triggered items: Adrenaline, Potion, Ugly Tooth, Hunting Trophy, Extra Stomach, Giant Belt, Nail, Improved Tools, Frozen Heart, Ball and Chain, Snowball, Clockwork Wasp and Pile of Books. Reference comparisons included effect storage, enemy damage/slow hooks, player consumable/regen hooks, weapon cooldown/scaling and structure crit eligibility. Snowball responds to new positive item stat effects, not old inventory or ordinary level/stat gains. Full-health consumable gains reset their per-wave limits; critical HP damage uses current HP and the source boss reduction. Permanent tooth slowdown is separate from recovering knockback/contact slowdown.

Current checkpoint: 170/209 base items, 138 initially available, 39 missing. Five playable starting profiles; the other 45 profiles and persistent unlock integration remain unfinished. Windows build: zero errors/warnings, 234/234 EditMode tests passed. The five-character rendered fixture passed at the Mage stage (226 tests), including all 160 available weapon tiers, and its selection screenshot was visually inspected. Later trigger changes have unit/build coverage but have not yet been rerun through that rendered fixture.

Two older tests assumed exactly one bearded preset and exactly four starting profiles. They were updated to test layer replacement and the five-profile lists without imposing those obsolete constraints. This is a staged implementation log, not a claim that the full requested roster is complete.

## One-wave and death-triggered implants

Added Anvil, Peacock, Celery Tea, Will-o'-the-Wisp, Baby Elephant and Cyberball: 176/209 items, 141 initially available, 33 missing. One-wave stats are queued on acquisition, consumed at the next wave start and cleared at its end. Anvil upgrades a randomly chosen eligible weapon on entering the next shop, falling back to its exact Armor gain when no upgrade exists. Burning kills count victims already burning as well as lethal burn ticks, with the reference's per-wave reward limit. Random damage stacks are rounded individually, aggregated and resolved iteratively against live enemies to avoid recursive list mutation during death chains.

Compared shop-entry effects, player/run wave-reset logic and main-scene stat-damage/death handling. Windows build clean; 239/239 EditMode tests passed. Tests cover single-use expiry, repeat purchase, persistent versus temporary XP, Anvil fallback, burning-kill thresholds/reset and damage rounding. Full character roster/unlocks and the remaining 33 items are still in progress.

## Deployable implant group

Added nine deployables: Landmines, Turret, Incendiary/Medical/Laser/Explosive Turrets, Tyler, Wandering Bot and Garden. Catalog: 185/209 items, 150 initially available, 24 missing. The shared structure definition also upgrades Wrench tiers to their full source structure data. Compared turret windup/animation completion, random cooldowns, projectile pellets/piercing/accuracy, Engineering burn scaling, medical projectile collision, garden production and wandering-bot orbit/slow entry behavior. Ordinary player percentage damage and lifesteal no longer leak into structure damage. Their presentation reuses our existing generated weapon sprites as deployable glyphs; it is not a new bespoke structure-art set.

Corrected wave-one initialization so spawn modifiers are read after the selected profile starts. The first tooltip test exposed Unity's conversion of a null inline burn record into an empty record; code now checks whether it actually contains burn ticks. An older turret fixture expected immediate fire, so it now advances through the source's 0.2-second firing animation. The hidden-window fixture exercised all nine mechanics successfully but Unity could not capture screenshots in that mode; it is not accepted as rendered evidence. Windows build clean; 242/242 EditMode tests passed before the fixture timing adjustment.

The visible rerun passed with fresh captures, five starter profiles, all 160 available weapon tiers and all nine deployable assertions. Accepted report: `artifacts/deployable-implants-review-visible/starter-review.json`. Hidden-run failure evidence is retained separately. Source text in RunPresentation was normalized from Windows-1252 to UTF-8 after the patch tool detected an invalid byte; the displayed middle dot is preserved.

## Attack implant group

Added Alien Eyes, Baby with a Beard, Rip and Tear, Spicy Sauce, Ice Cube and Riposte with our cybernetic display names. Coverage is 191/209 items (154 initially available), leaving 18 items and the full character roster/unlock integration unfinished. Compared projectile effect resources, special-spawn scaling in weapon_service.gd, consumable explosion placement in main.gd, dodge retaliation and the enemy elemental damage-taken effect.

Radial optics fire six projectiles per copy every three seconds. Death projectiles exclude their originating victim. Death/consumable explosions roll independently per copy and resolve through a queue to avoid recursive enemy-list mutation. Riposte hits the actual attacker after a successful dodge. Elemental vulnerability affects the triggering hit, lasts three seconds and cannot refresh on the same enemy. Special projectiles do not inherit Nail/Frozen Heart added weapon scaling; normal weapons still do.

Validation: Windows build succeeded with zero errors/warnings; 245/245 EditMode tests passed. Visible runtime fixture passed all six attack implants, nine deployables, 160 available weapon tiers and five starter profiles. Accepted evidence: artifacts/attack-implants-review/starter-review.json. Fixed a compile error in the new fixture's HurtPlayer call before running it. The first build attempt used the wrong script root and failed licensing in the restricted environment; accepted builds used the isolated validation copy with normal licensing access. This remains an incremental implementation checkpoint, not a claim of complete base-game parity.
A follow-up scaling test caught a formula distinction: explosion Damage is added to ordinary percentage Damage, not multiplied separately. Corrected these implant explosions; final suite is 246/246 passing with a clean rebuilt Windows player.


## Tree conversion and next-wave loot spawns

Added Pocket Factory and Lure, reaching 193/209 items (155 initially available, 16 missing). Pocket Factory creates the normal Engineering turret in the reference two-unit square near a destroyed tree, with its center clamped so the area stays inside the map. Lure consumes its two additional loot aliens on the next wave start. Each alien uses the base Crash Zone loot group and a random spawn time from five seconds to ten seconds before the wave ends. Enemy-count modifiers do not affect loot aliens.

Source comparison: main.gd neutral-death handler; entity_spawner.gd / zone_service.gd area placement; zone_1/000_loot group and unit resources; wave_manager.gd extra-loot scheduling. This exposed a scheduler edge case: repeating=-1 means spawn once with no repeats, while our old left=repeats+1 skipped that group entirely. The scheduler now clamps the number of repeats before adding the initial spawn. Bait requires the additional Lamprey enemy behavior and is still excluded rather than importing a nonfunctional effect.

Windows build succeeded with zero errors/warnings; 248/248 tests passed. Tests verify Lure consumption/re-purchase, its spawn window, count, and immunity to enemy-count scaling. The visible fixture passed the tree-to-turret conversion and actual turret damage plus all prior combat assertions: artifacts/wave-implants-review/starter-review.json. The full roster and saved unlock progression are still outstanding. Five starting profiles are implemented, as explicitly confirmed by the user; the other 45 visible locks are currently placeholders.


## October 12 completion: full catalog, roster and saved unlocks

The final 16 records are now implemented: Bait, Blazemander, Bonk Dog, Bot-o-Mine, Broken Hourglass, Campfire, Candle, Candy Bag, Catling Gun, Doc Moth, Greek Fire, Hourglass, Jellyshield, Lootworm, Ratzilla and Scapegoat. All **209 registered base items**, **62 weapon families / 205 tier records** and **50 character profiles** are included. Fresh saves expose five characters, 161 items and 46 weapon families / 160 tiers; the rest use real source challenge rewards. No DLC injection. This supersedes every smaller checkpoint count above.

Nine companions have movement, attacks and secondary actions: jumping impact, accelerated twin guns, mines, contact/radial burning, HP-scaled attacks, healing amplification, material/tree collection, a revivable decoy and orbiting projectile shields. Hourglass consumes itself, repeats the current wave and grants Broken Hourglass's next-wave HP override. Candy Bag grants eight random primary-stat points and can add an elite. Bait queues Lamprey groups. Greek Fire uses current enemy HP with the reference boss reduction. Vision implants change Nightmare fog visibility.

Character import now resolves every profile's effects, gain multipliers, starting eligibility, bonus loadouts, weapon capacity, tier limits, wanted item tags and banned items. Runtime hooks cover health-funded shops, stationary attacks/income, half-wave stat conversion, random stat gains, unusual weapon-set counting, weapon-slot growth and the other source effect families. The user's requested default weapon plus selected weapon is retained when the profile allows it; Bull remains unarmed and single-slot characters retain one weapon. Existing cream-body cosmetic layers are reused across the roster; 50 profiles do not mean 50 bespoke portraits.

Progression imports 115 usable registered challenges (the source's additional ban-system reward is not a normal item/weapon/character reward). Counters, stat thresholds, per-wave conditions, character wins and difficulty wins award real resources and persist atomically with backup recovery. Nightmare requires ten distinct Danger-5 character wins. Run fixtures use memory-only progression. Save incompatibility/corruption is preserved rather than silently replaced. Explosive multi-kill tracking includes mines, structures and companion explosions. Pacifist income includes enemies removed by the population cap, rounds before halving on horde waves.

Source comparisons: registered resources in item_service.tscn and challenge_service.tscn; player_run_data/run_data effect handling; main.gd wave-end, damage, healing and challenge hooks; entity_spawner.gd population cap; wave_manager/zone_service source wave selection; companion scenes/scripts; weapon_service scaling; character resource restrictions. Imported 20 full wave definitions, five horde groups, elite/final-boss selection and 12 Nightmare projectile-event patterns so related item/unlock rules have actual encounters.

### Screenshot-driven presentation

The supplied three screenshots guided the full-width four-card shop, bottom-left item inventory, bottom-right three-column/two-row weapon inventory, item sorting, gold type labels, tier colors, inline scaling/stat icons, green bonuses and red penalties. Level-up cards use the same icon/signed-value/white-label hierarchy. Weapon actions attach set descriptions and show Combine only when available. Harvesting is the only secondary currency-area readout; stored-material banking remains gameplay, with its bag hidden. Walking smoke uses our procedural lobed puffs with source-style emission, rotation and shrink timing.

Weapon atlas boundary tests found neighboring artwork crossing multiple nominal grid cells. The fix derives connected-component clipping geometry from our existing PNG and imports complete sprite bounds; it does not repaint assets. UI and world renderers share those masks. A Unity JSON mismatch initially deserialized Rect values to zero; explicit serialized pixel-region fields fixed it. See bugs.md and the final verification entry for accepted evidence, rather than earlier failed runs.

### Practical limits

This is a completed catalog/character/unlock implementation, not a claim of frame-perfect equivalence to the entire original game. Extended elite attack timing, enemy contact geometry and some companion movement remain Unity adaptations. Some base weapon damage calculations still retain fractional values where the source rounds to integers; full combat formula parity is not asserted. Many implants use existing stat glyphs and companions use existing weapon art. The requested two-weapon default differs from the source on ordinary characters. Automated deterministic checks do not establish a balanced human 20-wave survival run, native input acceptance or identical random outcomes across engines. The new UI preserves our artwork and cybernetic names, so it is a structural reference match rather than pixel-identical screenshots.

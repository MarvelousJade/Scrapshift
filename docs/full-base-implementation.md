# Full base-game implementation work log

Scope confirmed by the user: all 209 registered base items, all 62 weapon families, all 50 base characters and their actual unlock behavior. Five characters begin unlocked, matching this supplied reference (including Mage). Keep our cream mascot, cybernetic aliases and original artwork. No DLC injection. Imported records alone do not count as implemented behavior.

## Source inventory

`scripts/audit_full_base.py` archives the 50 registered characters and 116 registered challenge records into `artifacts/base-roster-unlocks.json`, with resource hashes and resolved effect definitions. The existing item archive covers all 209 item identities. This inventory includes the newer base-game pets and characters in the supplied backup; it does not infer the roster from an older online count.

## Work in progress

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

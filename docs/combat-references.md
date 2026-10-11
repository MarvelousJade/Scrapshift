# Brotato gameplay and code research

Research date: October 10, 2026. An official public release of the full Brotato source was not verified. Public community mod code was inspected; it is not described as the complete original implementation. Scrapshift does not vendor or translate that code.

## Sources actually inspected

- [Brotato Steam page](https://store.steampowered.com/app/1942280/Brotato/): automatic weapons, short enemy waves, materials and between-wave purchases are the high-level gameplay reference.
- [BrotatoMods](https://github.com/BrotatoMods): community repository index, including effects and utility libraries.
- [DarkTwinge BalanceMod](https://github.com/DarkTwinge/Brotato-BalanceMod): mod author's balance rationale, tradeoffs among damage/defense/economy, and clearer descriptions.
- [BalanceMod weapon hit extension](https://github.com/DarkTwinge/Brotato-BalanceMod/blob/master/extensions/weapons/weapon.gd): read the public GDScript extension. It records damage per weapon, guards missing hitboxes, tracks bounded per-attack history, and changes an effect's first-wave behavior.
- [BalanceMod weapon stat service extension](https://github.com/DarkTwinge/Brotato-BalanceMod/blob/master/extensions/singletons/weapon_service.gd): read the extension's treatment of contextual stat bonuses. The source illustrates why a displayed base stat and an effective combat stat can differ.
- [BFX effects library](https://github.com/BrotatoMods/Brotato-BFX): read its documented effects interface, including invulnerability duration and on-hit effects. Its documentation is a mod API reference, not proof of Scrapshift behavior.

## Decisions taken from the research

The following are design conclusions, not assertions that Brotato uses Scrapshift's implementation:

1. **Keep the movement decision central.** Weapons target automatically, leaving attention for positioning and collecting scrap.
2. **Make the rhythm readable.** Five fixed one-minute waves, with a clear intermission, are a deliberately smaller format than the reference game.
3. **Show exact upgrade effects.** Explicit damage, fire-rate, repair and pickup-range values avoid vague "better weapon" descriptions. One place owns run stat modifiers.
4. **Separate damage events from visual feedback.** A projectile is consumed on a hit; orbit blades use a cooldown; the pulse has one damage event and a separate expanding visual. This helps prevent repeated damage from every rendered frame.
5. **Give recovery space.** Contact damage starts an invulnerability window; a brief spawn telegraph avoids immediate edge damage.
6. **Limit state growth.** Active enemies, pickups and transient sparks have explicit caps. These are safeguards, not measured performance claims.

## Deliberate simplifications

No six-weapon inventory, rarity tiers, shop rerolls, character roster, elaborate stat stacking or exact Brotato cooldown formulas. The first two weapon-option choices unlock orbit and pulse, then that option increases damage. These simple choices favor a comprehensible five-minute project; they are not a faithful simulation of Brotato balance.

## Interview discussion

Explain one observed reference mechanic, why it fits this project's scope, how it was implemented differently in Unity, and what evidence validates the result. Say "I studied community mod code" if asked about source research; do not say "I obtained the original source" or claim the mod's fixes as bugs personally fixed in Scrapshift.

## User-supplied local implementation reference

Read `E:/UnityProjects/Brotato/entities/units/unit/unit.gd`, `entities/units/movement_behaviors/stay_in_range_from_player_movement_behavior.gd`, and `ui/menus/ingame/upgrades_ui_player_container.gd`. Observed separate damage/crit/knockback/flash handling, a range-based movement behavior, and tier-colored offers with reroll state. These are observations of the supplied extracted project, not a claim to have found an official public source release.

Our independent Unity adaptation uses bounded floating text, shader property-block flashes, capped particles, seven upgrade kinds, three unique offers and one free reroll. Enemy scope is chaser, telegraphed charger and slow knockback-resistant brute; range-keeping enemies were studied but not added. No Godot implementation was copied.

# Weapon-set comparison — October 11, 2026

Source inspected before implementation: the supplied local `E:/UnityProjects/Brotato` project. No art or audio replacement is needed.

`singletons/run_data.gd:update_sets` removes previous set effects, counts each equipped weapon's memberships, then applies only the bonus indexed by `count - 2`, capped at the final entry. Duplicates count individually; tier does not multiply contribution. This is independent of `get_unique_weapon_ids`. Add/remove/combine operations refresh sets. The special character/item effect that makes every weapon count for all owned sets is outside our current roster.

All four tiers of our six families were checked against their `weapons/*/*/*/*_data.tres` set references:

| Set | Our members | 2 / 3 / 4 / 5 / 6 equipped weapons |
| --- | --- | --- |
| Gun | Pistol, SMG, Double Barrel Shotgun, Laser Gun | +10 / +20 / +30 / +40 / +50 Range |
| Precise | Knife | +3 / +6 / +9 / +12 / +15% Critical Chance |
| Primitive | Spear | +3 / +6 / +9 / +12 / +15 Max HP |

Values come from `items/sets/{gun,precise,primitive}/*/set_*_effect_1.tres` and the corresponding `*_set_data.tres` resources. These are total bonuses at the current count, not cumulative sums of earlier tiers. One weapon contributes to the count but grants zero bonus. Six copies of one family qualify.

`ui/menus/shop/synergy_panel.gd` shows all five thresholds and highlights exactly the current count. Our UI will expose membership on weapon cards, current set counts/bonuses on shop and upgrade screens, and threshold details on hover. Stats and combat must share the same totals. `singletons/weapon_service.gd` applies full Range to ranged weapons and half to melee weapons; our existing 100-pixels-per-unit conversion is retained.

Implementation plan: derive set bonuses from the equipped list instead of repeatedly adding them into permanent stats. Keep item/upgrade stats separate to prevent bonuses becoming permanent when an item is purchased or a level is earned. A manual combine lowers the count; a matching purchase at six slots upgrades one existing weapon without changing the count. Recycling removes the contribution; restarting clears it. Primitive reductions clamp current HP and the next wave fills the resulting maximum as before.

Validation results and implementation commits will be recorded after the checks run. This is a scoped implementation for the three sets represented by our weapons, not support for every source-game set or character modifier.

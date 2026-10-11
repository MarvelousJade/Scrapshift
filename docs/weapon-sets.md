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

`ui/menus/shop/synergy_panel.gd` shows all five thresholds and highlights exactly the current count. Our UI exposes membership on weapon cards, current set counts/bonuses on shop and upgrade screens, and threshold details on hover. Stats and combat must share the same totals. `singletons/weapon_service.gd` applies full Range to ranged weapons and half to melee weapons; our existing 100-pixels-per-unit conversion is retained.

Implementation: derive set bonuses from the equipped list instead of repeatedly adding them into permanent stats. Keep item/upgrade stats separate to prevent bonuses becoming permanent when an item is purchased or a level is earned. A manual combine lowers the count; a matching purchase at six slots upgrades one existing weapon without changing the count. Recycling removes the contribution; restarting clears it. Primitive reductions clamp current HP and the next wave fills the resulting maximum as before.

This is a scoped implementation for the three sets represented by our weapons, not support for every source-game set or character modifier.

## Implementation and evidence

`WeaponSets.cs` centralizes explicit membership and the three total-bonus curves. `RunRules.Stat` adds the current equipped-list contribution to permanent stats. Stat mutations use `PermanentStat`, so an item purchase or level cannot accidentally store a temporary bonus. Combat and shop descriptions share `WeaponRange`; global Range applies fully to ranged weapons and at half strength to melee weapons. Combining/recycling clamps HP to the resulting maximum.

Shop and upgrade screens show each set's count and current bonus. Weapon cards expose membership; hovering a membership or set row shows all five thresholds with only the active threshold highlighted. Inactive sets say "Needs 2 weapons." Existing sprites, audio and content assets are unchanged.

15 added test cases bring the suite to **98/98 passing**. They cover all 24 weapon tiers, counts 0 through 7, duplicates, mixed sets, full-inventory matching purchases, combining/recycling, permanent-stat separation, HP clamping and restart. The first run passed 97/98: a test's pre-wave HP assumption missed Harvesting earning a level. It now compares HP immediately before and after combining; no product defect was inferred from that test failure.

The Windows build succeeded with zero warnings/errors. The rendered fixture (`artifacts/weapon-set-review/`) shows Gun 2/6, Precise 2/6 and Primitive 2/6 active together. Combining the spears changes Primitive to 1/6 and Max HP from 30 to 27; other sets remain active. Shop, threshold, after-combine and upgrade screenshots were independently inspected. The fixture forces the hovered set and scripts the combine; it does not prove manual pointer interaction or normal-run balance. AssetScope remains at 198 assets / 1,337 usages / zero findings.

Incremental commits: `d125e52` reference comparison, `92c8bc6` rules and regression tests, `03a11fe` UI and rendered fixture. Final evidence is recorded separately. See `verification.md` for current reports and remaining limits.

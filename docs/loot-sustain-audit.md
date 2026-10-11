# Remaining gameplay comparison — October 11, 2026

Compared the current `344d3f5` implementation with the supplied local `E:/UnityProjects/Brotato` source before editing. Original art and the 20-wave campaign remain the baseline.

## Priority by effect on an ordinary run

1. **Loot and reward decisions:** no crates exist; trees always heal, and loot aliens do not guarantee their item reward. This removes a reason to pursue enemies and a whole take-or-recycle decision before upgrades. Implement first.
2. **Survival rules:** regeneration currently heals continuously at stat/5 per second; life steal has no cooldown; healing consumables magnetize even at full HP. These alter the value of builds and how players position around healing. Correct in this pass.
3. **Build variety and run setup:** one starting character/weapon, eight offered items and eight upgrade stats. Character passives, starter selection and more supported stat choices are the next substantial content/system work. Keep the small catalog for this pass rather than claim a full roster.
4. **Encounter and long-run breadth:** base difficulty only, one final boss, no selectable difficulties, endless mode or co-op. Arena size, collision and RNG still differ. These need separate balancing work after the core loop is complete.
5. **Weapon-specific effects:** global stat support does not establish exact per-weapon scaling, critical multipliers or projectile behavior. Audit each supported family separately before a future combat pass.

## Source contracts for this pass

- `main.gd:spawn_consumables`: base item chance is multiplied by `(1 + luck/100)` then divided by `(1 + crates already spawned this wave)`. Guaranteed drops with item chance 1 bypass dilution in the normal campaign. The counter tracks spawned crates, not collected ones.
- `singletons/item_service.gd:get_consumable_to_drop`: consumable success uses the unit's base chance times luck; a second roll chooses the item box. `entities/units/neutral/tree_stats.tres`: base 1, item 0.2. Ordinary enemy stats generally use 0.01 / 0.01; `looter_stats.tres` and `predator_stats.tres` guarantee a consumable and item box. Predator yields a legendary box, though final-wave rewards have no following shop in our normal run.
- `items/consumables/item_box/item_box_data.tres` shares the fruit's **3 HP** healing effect and queues processing until wave end. `main.gd:clean_up_room` attracts remaining harmless consumables at wave end. Materials are banked separately.
- `ui/menus/ingame/upgrades_ui.gd:show_options` processes consumables **before** queued levels. The item is generated once when opened (`item_service.gd:process_item_box`); the player takes it or recycles it. Ordinary boxes use wave/luck rarity, not guaranteed uncommon items. Recycling uses 25% of current-wave price, floored, minimum 1 (source `get_recycling_value`).
- `player.gd:_on_ItemAttractArea_area_entered`: healing consumables attract when HP is missing; direct contact can still collect at full health. Our existing scatter/settle art motion stays.
- `run_data.gd:get_hp_regeneration_timer`: one HP every `5 / (1 + (regen - 1)/2.25)` seconds for positive regeneration. Stop/reset at full health; no negative-regen damage. `player.gd:on_lifesteal_effect` plus `player.tscn:LifestealTimer`: successful life steal heals one HP with a shared **0.1-second cooldown**.

Implementation boundaries: crate rewards use our existing eight-item offered pool with the existing lower-tier fallback where a tier has no items. No character-specific loot modifiers, cursed/extra crates, legendary reward catalog or endless scaling is claimed. A small procedural salvage-box outline will distinguish crates while reusing the existing original sprites and palette; no art/audio asset replacement.

## Implemented and measured

`ReferenceLoot` records profiles for all 18 supported enemies. The audit also found that `junkie_stats.tres` and `evil_mob/evil_mob.tres` prohibit consumable drops; the previous generic probability path ignored that. Spitters use 0.03 consumable / 0.1 crate chance, chasers/fins 0.02 / 0.03, bruisers/horned bruisers/pursuers/slashers 0.03 / 0.03; other supported regular enemies use 0.01 / 0.01.

`LootRules` queues crates, generates one item when opening each, prevents shop/reroll actions in the loot phase, and applies the same item effects as buying. The new screen shows the item, tradeoffs, rarity, remaining count, materials, stats, wardrobe and weapon sets. Enter takes, X recycles. A successful normal wave collects remaining harmless consumables before selecting its reward phase; banked materials remain separate. Crates never disappear because the material pickup limit was reached. Loss and restart cannot claim old rewards.

`SurvivalRules` owns the regeneration accumulator and shared life-steal cooldown. Pause freezes both; full health clears regeneration progress and the next wave resets both clocks. Full-health consumables remain on the ground unless directly touched or already attracted; missing health enables their attraction. Existing original repair and material sprites remain, and crates use an added procedural salvage-box outline.

**116/116 tests passed** (18 added cases), Windows build succeeded with zero warnings/errors, AssetScope scanned 198 assets / 1,337 usages with zero findings. All 581 validation input files match the checkout. The rendered loot fixture verified a looter's guaranteed crate, +3 HP, offscreen crate cleanup, crates before upgrades, item effects, recycling and reaching the shop. Ground, reward and post-crate upgrade screenshots were independently inspected.

The current accelerated campaign reached Won after all 20 waves: 54 purchases, peak 80 enemies, 765 charge releases, 700 ranged releases and one boss phase change. It uses supplied equipment and invulnerability. A separate normal-speed smoke exercised combat but **died at 15.30 simulated seconds with 17 kills and no upgrade**. Its legacy report says Passed for combat/capture coverage only; it did not pass a wave-clear or balance criterion. No human acceptance is claimed.

Incremental commits: `fbf98c3` comparison, `264c6f7` rules/tests, `b257eea` runtime/UI/fixture. Verification and interview documentation are committed separately. Original `Assets/Scrapshift/Content` files are unchanged from `344d3f5`.

## Next priorities

Character/starter selection and passives, more supported upgrade stats/items, and a weapon-by-weapon audit remain. Crate rarity uses the reduced eight-item pool and lower-tier fallback; a full legendary reward catalog is absent. Current source-like rules do not establish normal-run balance in our smaller arena. Difficulty/elite schedules, more bosses, endless, co-op and special item effects remain outside this revision.

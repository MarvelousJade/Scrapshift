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

Validation and actual outcomes will be appended after implementation.

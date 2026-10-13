# Item randomization against the supplied Brotato reference

Reference inspected: `E:/UnityProjects/Brotato/singletons/item_service.gd`,
`singletons/item_service_get_shop_items_args.gd`,
`ui/menus/shop/base_shop.gd`, and
`ui/menus/ingame/upgrades_ui_player_container.gd`.
This is the user-supplied extracted game project.

## Shop

The reference fills four offers. Waves 1 and 2 require two weapons; waves 3
through 5 require one. Later slots choose a weapon with 35% probability.
Each offer gets a wave-and-luck rarity roll. Weapon rolls prefer an owned
weapon or its set with the reference's early-wave bonuses. Item rolls use a
5% wanted-tag chance, or 10% with the boosted-tag effect. Previously shown
offers are excluded on a reroll; a new shop starts with only its locked
offers in that exclusion list. Bought items and locked item offers count
toward an item's maximum quantity.

The imported catalog has 209 item identities and 161 marked unlocked by
default. The original `init_unlocked_pool` also rejects `max_nb == 0` and
items with `can_be_looted == false`. `broken_hourglass` is the one unlocked
record with those flags, so the fresh random item pool contains 160 items.
It is still obtainable by the Hourglass transformation, which does not use
the random shop pool. The roll now stays in its chosen rarity tier and never
replaces an empty tier with a lower one.

## Level rewards

`get_upgrades` draws four upgrade options. Each option rolls rarity from
the player level and luck; levels 5, 10, 15, and 20 force tiers 2, 3, 3,
and 3 respectively, and later multiples of five force tier 4. Options
already shown in the current set or the previous reroll are rejected.
The Unity implementation's 16 stat tracks have entries at every tier, so
sampling distinct tracks gives the same option distribution for this pool.
The rarity formula and milestone tiers already match the reference.

## Crates

Crate contents are rolled when each crate reward opens, using the current
wave and luck. Ordinary crates use the same eligible item tiers and
character restrictions as the shop. Legendary crates force tier 4. Owned
items and locked item offers count toward maximum quantities; other shop
offers do not. A crate item is fixed once shown and is then taken or
recycled. The zero-maximum-count correction also removes
`broken_hourglass` from legendary crates.

The game uses `System.Random` and the reference uses Godot's RNG, so equal
numeric seeds do not imply identical offer sequences. The comparison here
is the selection rules and candidate probabilities for the imported
default-unlocked pool. Character unlock progression, DLC pools, and
special runtime modifications such as the reference's Axolotl redraw are
outside this implementation.

Validation: the isolated branch passed all 330 Unity EditMode tests after
the eligible-pool count assertion was corrected. The other enemy work was
validated separately in its own checkout.

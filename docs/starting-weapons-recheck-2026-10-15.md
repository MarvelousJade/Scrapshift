# Starting weapon recheck — October 15, 2026

## User observation and clarification

During physical controller testing, the user questioned the one-weapon start, remembering two weapons for many characters. Their subsequent clarification confirmed that the first character should start with one weapon and that the current character-specific rule is correct. Preserve that rule.

Codex independently re-inspected the supplied read-only source at `E:/UnityProjects/brotato-(3.x)` and the current imported Scrapshift data. No runtime code changed in this recheck.

## Exact source path through a new run

1. `ui/menus/run/character_selection.gd:211` completes character selection. It calls `RunData.add_character` and advances to weapon selection when weapon slots or starting item choices exist. Characters without either skip to difficulty selection after granting their explicit starting effects.
2. `singletons/run_data.gd:906` implements `add_character`: it stores the character and calls `add_item` to apply character effects. This does not automatically add a default weapon.
3. `ui/menus/run/weapon_selection.gd:117` builds the available choices from the character's `starting_weapons` and `starting_items` arrays. **`starting_weapons` is the choice list**, not a list of weapons already owned.
4. `weapon_selection.gd:145` completes selection: line 151 adds the selected weapon once, with `is_selection=true`; line 155 calls `RunData.add_starting_items_and_weapons()`.
5. `singletons/run_data.gd:1618` implements that grant operation. Its lines 1629–1635 add each weapon explicitly registered under the `starting_weapon` effect, respecting the effect quantity. `items/global/effect.gd:62` registers key/value effects; it does not spawn another selected weapon.
6. `ui/menus/run/difficulty_selection/difficulty_selection.gd:83` applies difficulty and enters the game. It does not add another weapon. `entities/units/player/player.gd:315` instantiates exactly the weapons in `RunData.get_player_weapons_ref`.
7. Restart follows the same sequence: `RunData.reset(true)` saves the selected weapon, clears player data, reapplies the character, adds the selected weapon once, then grants explicit starting effects (`run_data.gd:504–531`).

## All 50 registered characters

The `characters` array in `singletons/item_service.tscn:725` registers 50 characters. Following each resource's referenced effects gives **42 one-weapon characters, six two-weapon characters, and two weaponless characters**. These counts describe ordinary fresh starts in the supplied base catalog, before buying anything.

### Two weapons: one selection plus one grant

| Reference character | Scrapshift character | Explicit additional weapon | Tier | Current profile and serialized catalog |
|---|---|---|---|---|
| Brawler | Pugilist | Fist | I | Matches |
| Crazy | Hothead | Knife | I | Matches |
| Ranger | Sharpshot | Pistol | I | Matches |
| Wildling | Feral | Stick | I | Matches |
| Engineer | Fabricator | Wrench | I | Matches |
| Cyborg | Augment | Minigun | III | Matches |

Grant resources are `items/characters/brawler/brawler_effect_2.tres`, `crazy/crazy_effect_3.tres`, `ranger/ranger_effect_3.tres`, `wildling/wildling_effect_2.tres`, `engineer/engineer_effect_2.tres`, and `cyborg/cyborg_effect_0b.tres`. Each has `custom_key = "starting_weapon"`, `storage_method = 1`, and `value = 1`.

### No weapons

| Reference character | Scrapshift character | Source and current state |
|---|---|---|
| Bull | Reactor | Empty weapon choices; `bull_effect_5.tres` replaces weapon slots with zero; no grant |
| Beast Master | Drone Shepherd | Empty weapon choices; `beast_master_effect_2.tres` replaces weapon slots with zero; no grant |

### One selected weapon: the other 42

`well_rounded`, `mage`, `chunky`, `old`, `lucky`, `mutant`, `generalist`, `loud`, `multitasker`, `pacifist`, `gladiator`, `saver`, `sick`, `farmer`, `ghost`, `speedy`, `entrepreneur`, `explorer`, `doctor`, `hunter`, `artificer`, `arms_dealer`, `streamer`, `glutton`, `jack`, `lich`, `apprentice`, `cryptid`, `fisherman`, `golem`, `king`, `renegade`, `one_arm`, `soldier`, `masochist`, `knight`, `demon`, `baby`, `vagabond`, `technomage`, `vampire`, `wounded`.

In particular, Well-Rounded / Rookie has 18 starting choices and no explicit weapon grant. Selecting one produces one weapon. Mage / Arc Weaver additionally receives starting **items**, not another weapon.

## Scrapshift checks and limits

- Inspected `Assets/Scrapshift/Content/Reference/starter-profiles.json`: all six grant identities/tiers and both zero-slot profiles agree with the supplied source.
- Independently split and checked all 50 serialized profile blocks in `Assets/Scrapshift/Content/Definitions/ReferenceCatalog.asset`. Every profile's `bonusWeapon`, `bonusTier`, and `weaponSlots` matches the JSON. This catches stale serialized data rather than relying only on the importer input.
- Inspected `Assets/Scrapshift/Runtime/RunRules.cs:129`: `Start` clears inventory, adds the selected valid weapon, then adds the character's explicit `bonusWeapon`. The zero-slot branch does not add either.
- Existing `CharacterProfileTests.EveryUnlockedChoiceStartsWithSelectedWeaponAndReferenceBonus` exercises all available choices; `CharacterGrantedWeaponsAndUnarmedCharactersFollowReference` explicitly covers Rookie, Pugilist, Cyborg, Bull and Beast Master. This recheck inspected their assertions; it did not launch a new test suite or claim a new pass total.
- This is source and serialized-data evidence, not a new native playtest of all 50 characters. No universal extra weapon should be restored.

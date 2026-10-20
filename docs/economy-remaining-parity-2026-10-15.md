## October20 correction to historical structure claims

The statement below that Wrench/Screwdriver/Pruner effects never register is superseded. Source `RunData.add_weapon` applies `StructureEffect`, so those weapons count as registered structures before actors spawn. Broad character links, direct structure challenges and live actors still have different counting contracts. See [corrected caller chain and verified tests](support-registration-parity-2026-10-20.md). Historical test totals do not validate the old assumption.

# Remaining economy and progression audit — October 15, 2026

Branch: `codex/gameplay-parity`. Reference: `E:/UnityProjects/brotato-(3.x)`, inspected read-only. Codex performed this source audit and authored the bounded fixes below under the user's request to continue parallel parity work. Existing dirty changes and the accepted R6 audio were preserved. The confirmed one-selected-weapon starting rule and character exceptions are unchanged.

## Three confirmed differences corrected

### Fractional Harvesting must grow before integer credit conversion

Reference `main.gd:1289`, `manage_harvesting`, reads `Utils.get_stat(stat_harvesting)` as a float and combines it with other income before calling integer `RunData.add_gold`/`add_xp`. Separately, `_on_HarvestingTimer_timeout` at `main.gd:1521` computes `ceil(effective_harvesting * harvesting_growth / 100)` and adds that integer to the base stat. `RunData.get_stat` at `singletons/run_data.gd:1383` applies the character's gain multiplier when reading it.

Scrapshift previously cast effective Harvesting to an integer before both income and growth. This suppresses growth below one effective Harvesting and can cross a ceiling boundary at larger fractional values. These are reachable with real base characters: Knight gains only 20% Harvesting; Vagabond gains 50%.

`RunRules.Tick` now retains the effective float through growth calculation and casts only the combined income at the credit boundary. Examples covered by the new tests:

| Character | Base Harvesting before end | Effective before | Materials credited | Effective after growth |
|---|---:|---:|---:|---:|
| Knight | 1 | 0.2 | 0 | 0.4 |
| Knight | 101 | 20.2 | 20 | 20.6 |
| Vagabond | 1 | 0.5 | 0 | 1.0 |
| Vagabond | 41 | 20.5 | 20 | 21.5 |

The existing 5% base growth and item/character growth modifiers remain in the same calculation. The change does not add endless mode.

### Structure achievements count registered effects, not all structure actors

Reference `singletons/challenge_service.gd:666`, `_check_struct_challenges`, requires the relevant damage stat and `RunData.get_player_effect(Keys.structures_hash).size() >= 3`. `effects/items/structure_effect.gd:21`, `apply`, appends one entry per effect. It does not use the broader `RunData.get_nb_structures` helper.

Scrapshift's `Progression.CheckStats` previously used `OwnedStructures`, which also includes Wrench/Screwdriver/Pruner weapon actors, Pocket Factory and structure pets. That could unlock Advanced Technology/Cyborg or Magic and Machinery/Technomage without three registered structure effects.

Those two achievement checks now use `run.Effects("structure").Count()`. Broader gameplay structure counts were left intact. Tests cover both damage-stat achievements, the two-versus-three registration boundary after equipping all three structure weapons and Pocket Factory, a structure pet, and one synthetic effect with multiplicity three. The synthetic fixtures explicitly test effect-array registration semantics; they do not introduce new catalog content.

### Character item bans must not alter the fallback backup

Reference `singletons/item_service.gd:301`, `_get_rand_item_for_wave`, constructs a preferred pool and backup. Its explicit **run** banned-item list, excluded offers and reached maximum counts affect backup eligibility. Character `banned_items` and `banned_item_groups` are applied as preference filters to the preferred pool. If that pool empties, the source may return a character-banned item from backup rather than reintroducing excluded offers from the full rarity pool.

`ShopSelection.ItemPool`, located in `Runtime/ItemDesign.cs`, previously removed character-banned items from backup. It now leaves those items in backup and applies character bans to the preferred pool. Excluded offers and maximum-count rules retain their previous backup treatment. Tests verify an exhausted preferred pool without restoring an excluded offer, a failed wanted-tag preference falling back to both otherwise eligible backup items, and ordinary character-ban filtering while an eligible preferred item remains.

## Files and validation handoff

- Runtime changes: `Assets/Scrapshift/Runtime/RunRules.cs`, `ItemDesign.cs`, `Progression.cs`.
- New `Assets/Scrapshift/Tests/Editor/EconomyRemainingParityTests.cs` contains **11 executable cases**: four fractional Harvesting, four achievement-registration cases (including the parameterized damage pair), and three item-pool fallback cases.
- No Unity process was launched by this audit agent. Integration compilation and test execution are the root agent's next step; **no new pass total is claimed here**.
- `ProgressionRuntime.cs` required no change in this bounded pass. No catalog or audio asset was changed.

## Inspected behavior and remaining limits

The inspected base non-endless price, recycling, reroll and rarity formulas retain the earlier source-backed implementation, including weapon-base integer conversion, separate discount/inflation stages, paid reroll escalation, negative-Luck scaling and guaranteed upgrade milestones. This bounded audit does not assert exhaustive equality for all economy interactions.

The source contains a special 50% one-redraw branch for Axolotl after a successful preferred-pool selection. No Axolotl resource or registration exists in the supplied base item tree or the imported 209-item catalog. This unassigned generic hook was documented rather than adding a synthetic item or changing actual base-item draws.

The source has an explicit player-selected run-ban list in addition to character bans. Scrapshift's current `ItemPool` has no run-ban-list argument; this correction preserves the available character-ban behavior and does not claim full ban-mode support. Source maximum-count/fallback behavior, DLC hooks, endless progression and every possible item combination were not reimplemented in this bounded pass.

The previous full scripted runs completed with different offered/acquired items and movement histories. Their outcome gap is not itself evidence that a particular economy formula is wrong. The three fixes above instead have direct source branches and focused boundary cases. Human full-run equivalence remains a separate acceptance step.

## R7 integrated validation - completed

Full suite passed 594/594, zero failed/skipped, 72.8708967 seconds. Native built-player support fixture passed 27/27 and exited zero. R7 build succeeded with zero errors/warnings. Evidence: artifacts/gameplay-parity-continuation-2026-10-15/tests.xml, build-manifest.json and native-support/pets.json. These are automated/native fixture results, separate from human full-run playtesting. Further beam/zone geometry comparisons continue separately after the R7 checkpoint.


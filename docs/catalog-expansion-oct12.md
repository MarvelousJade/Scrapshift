# Base catalog expansion — October 12, 2026

## Data versus implementation

Registered resources in the supplied Godot 3 project are the boundary; DLC folders are not included. The weapon importer now reads the `weapons` array in `singletons/item_service.tscn`, producing all 62 families and 205 actual tier records. It does not fabricate tier-1 versions of high-tier-only weapons. Prices, base damage, cooldown frames, crits, scaling, classes and unlock flags come from those resources. All 209 registered item definitions and their resolved effects are archived separately in `artifacts/base-item-resources.json` for continued implementation.

Runtime items increase from 74 to 106; 91 are eligible on the fresh reference profile. Added scalar secondary effects: knockback, pickup range, consumable healing, XP gain, shop prices, harvesting growth, recycling gains, enemy health/speed, explosion size/damage, piercing damage, bounce, piercing and crate materials. The original character tag preference, tier roll, repeat exclusions, caps and restricted-item filters continue to select from the expanded pool. The remaining 103 item definitions are not runtime items, so overall shop probabilities still differ from the complete reference game.

Forty weapon families are currently obtainable. Ten newly imported families remain explicitly unavailable pending their special behavior: screwdriver, pruner, drill, scythe, jousting lance, Excalibur, particle accelerator, vorpal sword, thunder sword and torch. These gates are separate from the source unlock flags. Other source-locked weapons remain locked. The older locked Rail Gun's no-hit bonus also still needs implementation; serialized presence is not proof of every mechanic. No claim of complete combat parity is made.

## Mechanics and defects

- Pickup radius corrected from 1.3 to the source's 1.5 units, multiplied by pickup-range percentage and clamped to 0.3.
- Fractional XP multipliers carry their remainder between pickups; currency is unaffected by XP gain.
- Discounts apply before integer truncation. Recycling uses the source's clamped 25% plus modifier rate.
- Crate material bonuses grant currency without XP; healing uses the consumable modifier. Crown adds to the baseline 5% harvesting growth.
- Explosive ranged/melee weapon keys now use the existing explosion handler. Ghost weapon gains display their actual stat rather than always saying Attack Speed.
- An explosion can remove targets from a surrounding hit loop; stale indices are rejected before dereference.
- New weapons exposed a WAV-only audio assumption: Fireball uses MP3. Import supports both and forces mono.
- Tests assuming a tier-1 version of every weapon were corrected to compare real family tiers. Original-art dependency count increases by one.

## Theme and assets

All 106 runtime items have cybernetic display names, separate from stable reference IDs; positive and negative numeric effects remain visible. Existing character/enemy/weapon/item PNG files are preserved. New 32-sprite weapon atlas: `Assets/Scrapshift/Content/OriginalArt/base-weapons-v1.png`, generated with built-in image_gen using our existing sheet as style reference. Final prompt and provenance: `artifacts/base-weapons-art-prompt.txt`. Original alpha and pixels retained; Unity slices at measured gutters. Imported source audio is third-party reference audio for this local study, not original authorship.

Existing item artwork is retained; generic stat glyphs are used where no bespoke item sprite exists. The implant naming pass does not imply 106 unique new implant illustrations. Native input, exact visual timing, all enemy/structure interactions and a human balance run remain distinct from deterministic fixtures.

## Validation

Seven new EditMode tests cover actual tier existence and eligibility gates, prices/recycling, fractional XP/pickup radius, crate currency/healing, Crown growth and theme separation. Existing item tests compare all imported records against extracted values and buy every eligible item. Runtime weapon fixture exercises every eligible tier, not just tier 1. Final results are recorded in `docs/verification.md` after execution.

Visual QA additionally fixed long weapon names splitting mid-word, neighboring sprite slivers in the atlas slice, and discount/enemy-stat benefit colors in shop cards. Final 39-frame inspection and 187-test rerun pass; 20-wave fixture completes.

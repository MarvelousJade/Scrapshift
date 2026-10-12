# Conditional implants and weapon behaviors â€” October 12

This pass translates more of the supplied Brotato base-game data into independent Unity rules while preserving the cream mascot and our artwork. Cybernetic names are presentation aliases; reference IDs, values, tiers, prices, tags, item limits and source unlock flags remain unchanged. No DLC resources are added.

## Source comparison

Read `entities/units/player/player.gd`, its Timer nodes in `player.tscn`, `effects/items/temp_stats_per_interval_effect.gd`, `items/global/effect.gd`, `singletons/run_data.gd` and `main.gd` in the supplied Godot 3 backup. Resource hashes and extracted records are in `artifacts/item-design-audit.json` and `artifacts/starter-provenance.json`.

- Standing bonuses follow zero movement input immediately. Pushing against a wall still counts as movement. Pause retains the current displayed bonus and freezes timers.
- Unique-weapon effects count distinct `weapon_id` families, regardless of tier. Additional-weapon effects count every weapon, including the weapon supplying the effect.
- Interval effects accumulate on the global whole-second wave clock. A reset-on-hit effect removes only its accrued bonus; taking damage does not restart that clock. None of this pass's imported interval effects use reset-on-hit, but the source flag is retained and tested.
- Damage-triggered bonuses are temporary, and activate after damage rather than purchase or a dodge. Scythe loses 3 HP every second, bypassing armor and dodge, and gains 3% damage per damage event for the wave. Self-damage can kill.
- Wave-end gains are permanent and occur before harvesting. Level-up gains accompany the reference's normal +1 Max HP. Temporary gains clear at wave end; inventory-dependent effects update when gear changes.

## Added content

13 items: Anchor Plating (Barricade), Adaptive Dermis (Chameleon), Necrotic Vessel (Decomposing Flesh), Targeting Cortex (Focus), Regenerative Organ (Grind's Magical Leaf), Modular Tissue (Jelly), Repair Gland (Medikit), Growing Servo Arm (Robot Arm), Distributed Reflex Mesh (Spider), Stationary Overclock (Statue), Volatile Power Gland (Triangle of Power), Adaptive Combat Chip (Vigilante Ring), Learning Cortex (Wisdom).

Drill gains +10% attack speed every 5 seconds for the wave; Scythe implements self-damage and damage-triggered growth; Jousting Lance uses its tier-specific speed bonus and standing damage penalty; Excalibur applies -2 Armor per held weapon. Existing damage, scaling, cooldown, sound and artwork data are retained. Excalibur remains source-locked.

119/209 item definitions are now implemented; 98 are fresh-profile obtainable. All 62 base weapon families / 205 tier resources are imported, with 43 families / 148 tiers obtainable. Six weapon families remain explicitly gated: Screwdriver, Pruner, Particle Accelerator, Vorpal Sword, Thunder Sword and Torch. The legacy source-locked Rail Gun's no-hit effect is also unfinished. 90 items remain missing; the incomplete shop pool still cannot reproduce full-catalog probabilities. New implant illustrations are generic existing stat glyphs, not bespoke art.

## Engineering and bug record

The previous scalar-only schema would treat a conditional weapon stat as always active. Added an explicit conditional schema and excluded custom-key traits from ordinary equipment stats. Kept source values separate from timing and trigger dispatch, with scene-independent tests. Import eligibility requires all of an item's effects to be supported.

Two legacy archived records flattened Decomposing Flesh and Jelly into immediate stats. The new import separates their level-up and distinct-weapon triggers; the audit records these as structural differences rather than silently erasing them.

Self-damage introduces a loss path inside the rule tick. The tick now stops on death so a lethal last-second hit cannot be overwritten by Shop or Won. Survival and timed effects are stepped together at second boundaries, with double-precision time accumulation; tests compare one large tick with 300 small ticks. Shop cards now scroll long implant descriptions rather than omitting trigger text or overlapping the buy button.

Initial verification: Windows build succeeded with zero errors/warnings; 198/198 EditMode tests passed, zero skipped. Eleven new tests cover reference edge cases and importer tests compare all conditional metadata. Rendered player validation is recorded separately after the final build. Tests are evidence for these cases, not proof of complete parity or human balance. Implementation and investigation were Codex-assisted; this document does not claim unaided professional work.

Rendered review found a separate migration bug: `InspectionBuilder` stored a tiered display name, then `WeaponName` reused that name as its fallback and appended another suffix on every build. Names such as “Drill IV IV IV” were visible. The fallback now derives from the stable ID, with a regression test that repeats migration four times. Two atlas slice boundaries also included a neighboring weapon's pixels; narrowed the Scythe and Jousting Lance sprite rectangles without editing any PNG.

Final validation: 199/199 tests, clean Windows build, 41 fresh rendered UI cases, all 148 obtainable weapon tiers dealt damage, and the automated 20-wave campaign finished Won with 47 purchases and one boss phase change. AssetScope checked 311 assets with zero findings. All 918 validation inputs match byte-for-byte. See `verification.md` for evidence, package hash and fixture limitations.

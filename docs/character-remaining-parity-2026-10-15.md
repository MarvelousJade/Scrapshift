## October20 correction to historical structure claims

The statement below that Wrench/Screwdriver/Pruner effects never register is superseded. Source `RunData.add_weapon` applies `StructureEffect`, so those weapons count as registered structures before actors spawn. Broad character links, direct structure challenges and live actors still have different counting contracts. See [corrected caller chain and verified tests](support-registration-parity-2026-10-20.md). Historical test totals do not validate the old assumption.

# Character mechanics recheck — October 15, 2026

Branch: `codex/gameplay-parity`. Supplied reference inspected read-only at `E:/UnityProjects/brotato-(3.x)`. Codex performed the investigation and authored the focused fix and tests under the user's continuing parallel parity request. Accepted art/audio and starting-weapon rules were preserved.

## Confirmed mismatch: Streamer structure-linked armor

`items/characters/streamer/streamer_data.tres` grants an armor stat link: +2 Armor per structure. `singletons/linked_stats.gd:36`, `reset_player`, resolves a `structure` link through `RunData.get_nb_structures`. That function at `singletons/run_data.gd:1519` counts:

1. Entries in the registered structures-effect array.
2. Copies of Pocket Factory.
3. Entries in the pet-effect array whose `is_structure` flag is true.

It does **not** inspect equipped Wrenches, Screwdrivers or Pruners, nor count the actors deployed by those weapons. Registered effects and pets count once per entry, independently of an effect's numeric value.

Scrapshift's `RunRules.OwnedStructures` previously added equipped structure-producing weapons and summed effect values. Streamer / Broadcaster therefore received +2 Armor immediately when starting with a Wrench, Screwdriver or Pruner, without acquiring an eligible structure item. Additional weapon copies/tiers could also alter that linked Armor incorrectly.

`Assets/Scrapshift/Runtime/CharacterMechanics.cs` now follows the reference count: structure effect entry count + Pocket Factory copies + structure-pet entry count. The weapon actor term was removed. The link in `ConditionalEffects.LinkSource` already uses `OwnedStructures`, so it needs no separate change. Existing structure deployment layout also uses this helper and now receives the corrected source count.

The distinction from the earlier achievement repair remains necessary: Advanced Technology and Magic and Machinery inspect **only** registered structures-effect entries. They deliberately exclude Pocket Factory and structure pets even though Streamer's broader structure stat link includes them. The old economy audit's statement that the broader count includes weapon actors describes the pre-recheck implementation and is superseded here.

## Source-derived checks

New `Assets/Scrapshift/Tests/Editor/CharacterRemainingParityTests.cs` contains six executable cases:

- Three actual Streamer starts, separately selecting Wrench, Screwdriver and Pruner: one equipped weapon, zero eligible structures, zero linked Armor.
- Actual Turret + Pocket Factory + Bot-o-Mine count three and grant six Armor; adding Catling Gun and Bonk Dog does not increase it.
- Replacing three structure weapons with a tier-IV Wrench leaves a real turret's two Armor unchanged.
- Two registered turret copies count separately; removing one updates the link from four Armor to two.

Three assertions in the preceding `EconomyRemainingParityTests.cs` were reconciled with the newly corrected source count: the weapon-plus-factory setup counts one, and synthetic multi-value structure/pet effects count one entry each. Their challenge eligibility checks remain unchanged.

`git diff --check` reports no whitespace errors for the edited tracked files. This audit agent did **not** launch Unity or execute the new tests. Integrated compilation, suite results and the next review build are the root agent's validation gate; no new pass result is claimed here. The reported earlier R7 594-case suite and 27-case native support check predate this change.

## Rejected candidate and limits

The audit initially questioned Greek Fire on normal hits against an already burning enemy. Inspection of the complete function signature corrected that hypothesis: `enemy.gd:271` receives an `is_burning` **damage-event argument**, defaulting to false, distinct from the unit's `_is_burning` status field. `unit.gd:303` forwards `args.is_burning`, and the burn timer sets that argument. The normal-hit broadening was therefore rejected; no such change belongs in this fix.

Cyborg conversion, stationary income, full-health consumable triggers and wave-start health/material effects were inspected as candidates. This bounded pass does not assert exhaustive equality for those paths and makes no unsupported edits to them. `TriggeredImplants.cs` and `ItemLifecycle.cs` required no change for the confirmed Streamer defect.

The new checks establish deterministic structure counting and linked-stat behavior. They do not establish a complete human playthrough, audio perception, every character/item interaction or overall gameplay parity.

## R8 integrated checkpoint

Combined suite620/620 passed,0failed/skipped,74.3181245s. R8build succeeded0errors/warnings. Native weapon205/205 and enemy/state53/53 checks passed with exit0; beam/healer captures visually inspected. Artifacts: gameplay-parity-geometry-2026-10-15. Fresh normal-timing campaign is in progress; do not treat fixture checks as human full-run equivalence. See gameplay-parity-continuation-2026-10-15.md for exact paths, hashes and scope.


## R8 normal-timing campaign completed

Fresh final-build campaign Passed/Won, exit0:20timers19shops31purchases18upgrades6crate takes, normal60Hz pickup physics,1092.05wall seconds. All31debits and all recorded stack totals reconcile,0matched player exceptions. End capture shows queuedDrifterunlock; Won comes from report/trace. Comparison against the frozen supplied-original run is in full-run-r8-comparison-2026-10-15.md; scripted invulnerable fixtures do not establish human survival parity.


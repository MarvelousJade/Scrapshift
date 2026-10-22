# Registered support effects - October 20, 2026

## Confirmed defects and source chain

`RunData.add_weapon` applies weapon effects (`singletons/run_data.gd:982-990`). `StructureEffect.apply` registers each effect once (`effects/items/structure_effect.gd:21-22`). Thus Wrench, Screwdriver and Pruner belong to the registered structure array, even before their actors spawn. The prior assumption in `CharacterRemainingParityTests`, `EconomyRemainingParityTests` and dated notes that weapon structures never register was wrong and is superseded here. Deployed actor multiplicity is still a separate concept.

- `RegisteredStructureCount` includes item/character and weapon registrations. `OwnedStructures` additionally includes Pocket Factory and structure pets, matching `get_nb_structures`. Technomage's selected Wrench plus two granted Turrets now gives three structures and +6 Elemental Damage, rather than +4. Streamer and both relevant structure unlocks use the corresponding source count.
- Pet links also include weapon structure effects flagged as pets; imported Wrench/Screwdriver/Pruner effects currently are not pets.
- Grouped support spawn span uses only registered structure entries (`entity_spawner.gd:539-563`). Pocket Factory and structure-pet extras must not enlarge it. Pets and structures use separate group centers. Shared `SpawnPlacement.SourceAreaPosition` clamps the area center before sampling; ungrouped structures use the source160-pixel edge inset.
- Landmine effects cannot be grouped. Wandering Bot has a two-unit player-area override even though its source effect permits grouping. The override takes precedence.
- `chal_turrets` counts registered turret effects at wave setup (`entity_spawner.gd:512-528`), including Wrench but excluding gardens, mines and Wandering Bot. Removed the per-live-actor check, which could incorrectly grant this reward from Pocket Factory's tree-spawned turrets. Its source default maximum turret count is effectively unbounded (`Utils.LARGE_NUMBER`); no imported cap modifier is asserted here.

## Validation

The initial focused integrated structure set passed **22/22**, zero failures/skips, exit0 (`weapon-structure-tests.xml`). The final support/dispatch focused set passed5/5, exit0, and the complete suite then passed1,200/1,200 with no failures/skips, exit0 (suite-attempt-4.xml,69.7817621 seconds), including the final position refinements. The corrected character oracle matches **10,500/10,500** static16-stat vectors with zero export errors or mismatches, including the Technomage baseline. Its first run is preserved and triaged separately in the oracle report.

These are source/model/headless checks. Actual current native positions, actor rendering, turret-unlock presentation and physical controls remain pending while desktop control is paused. R28's completed20-wave comparison predates this correction and is not relabelled as a current-build campaign.

Codex agents investigated, implemented and tested these changes under the user's parity direction. The user did not independently perform these automated investigations.

## Preserved complete-suite failure

Suite attempt3 passed1,199/1,200 but failed the older `UnlockDispatch_chal_turrets` scenario because it created five live actors and expected a reward. That scenario encoded the disproved assumption. It now explicitly verifies that five live actors do not qualify, four registered effects remain below threshold, and the fifth registered effect dispatches through the production deployment helper. This is a fixture correction following the production source correction; the failed XML is retained. The focused rerun passed5/5, followed by complete attempt4 passing1,200/1,200; the earlier failure remains historical evidence.

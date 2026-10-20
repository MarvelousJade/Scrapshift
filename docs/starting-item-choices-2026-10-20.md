# Starting item choices - October 20, 2026

The source `ui/menus/run/weapon_selection.gd:121-155` adds exactly one selected weapon or starting item, then `RunData.add_starting_items_and_weapons` grants explicit character effects. Beast Master's four pet alternatives were incorrectly imported as automatic grants. The corrected schema has four `startingItemChoices` and only Lootworm in `startingItems`.

`RunRules.Start` validates and applies one choice before automatic grants. The picker includes available item alternatives, previews their effects and retains the selected ID through fresh-run unlock-pool refresh. The weaponless Bull still skips the empty picker. Character descriptions label alternatives as choices.

The targeted catalog helper updates choices/automatic grants for all50 characters, but effect rows only for Beast Master. The reviewed YAML diff preserves unrelated curated Ranger fields. Helper attempt1 failed because of an invalid new meta GUID; attempt2 revealed that unrelated field drift; attempt3 exited0 with the restricted diff. Neither earlier candidate was copied into the root catalog.

## Verified and pending

- `starting-choice-tests.xml`: **16/16 Passed**, no failures/skips, exit0,0.7459803 seconds. Includes every selected pet, default/invalid choices, availability/tier ordering, pet-linked stats and existing starter checks.
- Corrected original/Unity30-case character preflight: zero exporter errors or static16-stat differences. Beast Master selected Bonk Dog plus Lootworm, zero weapons, Speed4 on both sides.
- `suite-attempt-2.xml`: **1196/1196 Passed**, no failures/skips, exit0,86.2744431 seconds after the production selection-ID preservation edit.
- Static oracle evidence does not verify the visible picker, controller navigation, animation or audio. Those checks are deferred until the user permits desktop control again. Broader character/item findings belong to the separate oracle report.

Codex performed investigation, implementation and headless testing. The user supplied reference/direction and requested the desktop-control hold.

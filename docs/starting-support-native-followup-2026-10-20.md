# Starting items and support deployment follow-up — October 20, 2026

## Source expectations

- Beast Master's `items/characters/beast_master/beast_master_data.tres` offers four `starting_items`; its separate `beast_master_effect_4.tres` grants Lootworm. Choosing one starting pet must produce that pet plus Lootworm, with no weapon. Choice availability follows unlock eligibility. Preview, starting inventory, actors, and wave retry must agree.
- The turret achievement counts registered turret StructureEffects, including Wrench. Landmines, gardens, Wandering Bot, and Pocket Factory's future tree turrets do not substitute for the five registered turrets. The relevant checks are source-backed in `SupportRegistrationBoundaryTests` and production `RegisteredTurretCount`.
- `global/entity_spawner.gd:541–568` gives structures and pets separate group centers. The square span is `min(600,400+registered_structure_count*10)` source pixels. An eligible grouped structure uses its structure group; pets use their own group. A structure's explicit player-area override takes precedence. Wandering Bot has that override.
- Both `items/all/landmines/landmines_effect_1.tres` and the four Screwdriver StructureEffects set `can_be_grouped=false` and `spawn_around_player=-1`. They use the global map sampler with an edge inset of `Utils.EDGE_MAP_DIST*2.5` (160 source pixels). Bot-O-Mine explicitly requests a local 100-pixel square in its own script and is a separate route.

## Confirmed mine placement difference and fix

`DeployStructure` selected the correct global landmine point, then discarded it by calling `SpawnMine` without a position. `SpawnMine` sampled again through `RandomPosition`, which used the entire map bounds and a player-distance retry. Timed item/Screwdriver mine deployments used the same incorrect sampler.

Initial deployment now passes the already selected point to `SpawnMineAt`. Timed global deployments use `SpawnPlacement.SourceRandomPosition` with the source's 1.6-world-unit inset. The existing explicit local pet route retains its position/jitter behavior. The existing three-argument `SpawnMine` interface remains available for its callers.

`StartingSupportFollowupTests` contains three focused cases: 64 seeded global placements with the source roll orientation/inset, an exact preselected initial point, and preservation of the explicit local pet route. All three passed alongside nine death-timing cases in `artifacts/native-followup-2026-10-20/focused-attempt-3.xml`: 12/12 Passed, zero failures/skips. This focused result does not establish broad gameplay parity.

## Native fixture

Run the unpacked player with:

```text
Scrapshift.exe --controller-check <isolated-controller-output> --starting-support-review <review-output>
```

This fixture requires a visible native player. It does not support batchmode. It creates a software Gamepad inside the process, uses fixture-only IgnoreFocus, and queues software A events. It sends no operating-system input and does not disable physical devices. This is not physical Xbox acceptance or ordinary focus behavior.

The fixture clones the catalog, stages a fresh in-memory progression profile with Beast Master and its four starting pets unlocked, and stages the character/page and logical focus. Actual Update/OnGUI then selects the focused item, confirms difficulty to start the run, and activates Retry. This verifies activation routes after focus staging; D-pad movement to those targets is separate coverage.

For each of four choices it records:

1. The focused item and actual preview contain only the selected pet and Lootworm.
2. Software A confirms that item through OnGUI.
3. Software A on difficulty starts the actual selected run; inventory and spawned companion kinds match.
4. After a staged lethal hit and ordinary defeat cleanup, software A on the actual Retry control restores those items and companions.

The separate support checks explicitly invoke native deployment callbacks. They test registered turret counts 0, 4, and 5 around the unlock threshold, then run 64 placement seeds with staged, separated group/player centers. They inspect actual spawned turret, mine, Wandering Bot, and pet positions. They are callback evidence, not ordinary scheduler or spawn-distribution evidence.

The report is `<review-output>/starting-support.json`. Capture names are `choice-<item>.png`, `run-<item>.png`, `retry-<item>.png` for the four items, plus `support-placement.png`: 13 captures total.

## R36 actual acceptance and visual findings

`artifacts/native-followup-2026-10-20/starting-support/starting-support.json` is Passed and the process receipt reports exit zero. All four choices passed preview, actual OnGUI confirmation/start/retry, exact starting/retried item sets, and spawned companion-kind checks. The three registered-turret threshold guards are true. All 64 placement rows completed; 56 mines lie outside the staged structure group, while the global inset, Wandering Bot player-area override, and separate pet-group checks passed.

An independent agent opened every original 1280×720 capture through `view_image`. The file hashes and per-image observations are recorded in `starting-support/visual-review-r36.json`. The four run captures show the selected companion plus Lootworm, no weapon, and a readable HUD. All four retry captures show zero HP, RUN LOST, and the highlighted Retry wave control. This supports the finite route and capture observations; it is not physical input, D-pad targeting, human play, or movement/audio acceptance.

The review also found two issues that the passing JSON did not cover:

1. **Production title clipping.** Every choice card's two-line companion name lost the bottom of its second line. The 47-pixel title region was too short. The fix uses the existing space from y=174 to y=242 (68 pixels), leaving four pixels before the icon at y=246, and reduces the existing heading font only when its measured wrapped height exceeds that region. No other selection layout was changed.
2. **Fixture-only shadow presentation.** The support-placement capture showed an oversized pet shadow and a player shadow left at its prior position. The fixture disables Arena Update and stages positions directly; it had skipped the ordinary normalization in `TickPets` and player shadow placement. The fixture now calls `TickPets(0)` and places the player shadow before capture. The four ordinary run captures had normal companion shadows, so no ordinary gameplay shadow defect was established.

These visual corrections target R37. Fresh captures from that exact build must be inspected before they are marked visually accepted. R36's mechanical acceptance remains preserved; its old images are not relabelled as fixed.

## Existing fixture map

| Flag | Report | Existing coverage and limit |
| --- | --- | --- |
| `--pet-review <folder>` | `pets.json` | Broad native pet and structure behavior checks, rendered aim/projectile captures; does not cover the four-item picker route. |
| `--support-motion-review <folder>` | `checks.json` | Support pose, body/shadow, aim and music lifecycle receipts/captures; does not establish starting-item selection or registration thresholds. |
| `--headless-unlocks-review <folder>` | `headless-unlocks.json` | Staged producer and persistence cases; separate from visible OnGUI selection and visual inspection. |

## Interview rehearsal and attribution

The useful debugging story is a discarded intermediate result: the initial mine's correct source-based location was computed, then a lower-level helper replaced it with a second random location. The fix preserves the selected point and keeps global versus explicit local spawn contexts distinct. The user requested parity and supplied the original; Codex agents inspected the source and authored this fix and fixtures. Hands-on reproduction and explanation remain rehearsal tasks unless confirmed by the user. No independent user debugging or physical-controller test is claimed.

# R23 full-run economy comparison - October 17, 2026

Generated from saved traces by `scripts/compare_r14_campaign.py`. The supplied-reference normal-time run at the input path and the Scrapshift run are separate scripted fixtures. The script records their exact file hashes below.

Bounded checks: **18/18 passed**. This result does not establish exact gameplay parity.

## Evidence

JSON: `artifacts\full-verification-2026-10-17\economy-comparison-r23.json`.

| Input | SHA-256 |
| --- | --- |
| `referenceTrace`: `E:\Codex Projects\Job Search Assistant\work\repos\Scrapshift\artifacts\full-verification-2026-10-17\original-attempt-2\trace.json` | `3de61684e5ad50854b6f89831cf420ebaf2df8044f4809dc91bd87526b9d4a71` |
| `referenceSummary`: `E:\Codex Projects\Job Search Assistant\work\repos\Scrapshift\artifacts\full-verification-2026-10-17\original-attempt-2\summary.json` | `7aabd262ef87eb7e3dffd04fb710a4693604f495cce2cb269e8baebb640f236f` |
| `referenceSourceManifest`: `E:\Codex Projects\Job Search Assistant\work\repos\Scrapshift\artifacts\full-verification-2026-10-17\original-attempt-2\source-manifest.json` | `5c3c5243db66f76eee8ddc0f57e37e61f37c692e32877a8e98925e835775cabc` |
| `unityTrace`: `E:\Codex Projects\Job Search Assistant\work\repos\Scrapshift\artifacts\full-verification-2026-10-17\unity-attempt-4\campaign-trace.json` | `869d64ac3ec44033979d8e9d3a3eae28edcd7c37e9d32e0c51e18232a0b01f2f` |
| `unitySummary`: `E:\Codex Projects\Job Search Assistant\work\repos\Scrapshift\artifacts\full-verification-2026-10-17\unity-attempt-4\campaign-check.json` | `6ede6338c7a987bb919581852dde573b70cccc574eae4c2692c203a1dc2d3422` |
| `unityProcess`: `E:\Codex Projects\Job Search Assistant\work\repos\Scrapshift\artifacts\full-verification-2026-10-17\unity-attempt-4\process.json` | `41d91abe900605b0da1193ab037ddc8ac38736e5fc4ab869ba8a53bd241fb8dc` |
| `buildManifest`: `E:\Codex Projects\Job Search Assistant\work\repos\Scrapshift\artifacts\gameplay-parity-r23-2026-10-17\build-manifest.json` | `f49e92346b175d0dc28ac899737b8edbb25fbc98b29d6fe70a5a3c5d21d15321` |
| `sourceManifest`: `E:\Codex Projects\Job Search Assistant\work\repos\Scrapshift\artifacts\gameplay-parity-r23-2026-10-17\source-manifest.json` | `62bb6a3623ffc2b3653cc6109eb96c3cecf9379ac97af58d275410ff01301394` |
| `itemCatalog`: `E:\Codex Projects\Job Search Assistant\work\repos\Scrapshift\Assets\Scrapshift\Content\Reference\item-design.json` | `aa1bd9687c8c012ba8df064a9ae76c5da243834cadf18abf0607b9e2478fa311` |

## Bounded checks

| Check | Result |
| --- | --- |
| referenceWonAndExitedZero | pass |
| unityFinalPositiveHarvestCredited | pass |
| unityProcessExitedZero | pass |
| unityWon | pass |
| referenceTwentyTimersNineteenShops | pass |
| unityTwentyTimersNineteenShops | pass |
| referenceFixed60HzDurations | pass |
| unityFixed60HzDurations | pass |
| referencePurchaseDebits | pass |
| unityPurchaseDebits | pass |
| referenceFinalStackSum | pass |
| unityEverySnapshotStackSum | pass |
| unityEverySnapshotStackCaps | pass |
| referenceFinalStackCaps | pass |
| unityFreshRunUnlockEligibility | pass |
| sourceCopyMatches | pass |
| referenceManifestPresent | pass |
| buildManifestPresent | pass |

Purchases: reference 43 (43 debit checks); Scrapshift 37 (37 debit checks). Different prices and purchase counts are not required to equal across random runs.
Stacks: reference final 58/58; Scrapshift final 45/45. Scrapshift inconsistent snapshots: 0.
Positive catalog stack-cap violations: reference final 0; Scrapshift snapshots 0. These neutral fixtures have no character-specific cap modifiers.
Initial-catalog locked acquisitions in the fresh Scrapshift run: 0; unknown acquisitions: 0.

## Observed run totals

| Metric | Reference | Scrapshift |
| --- | ---: | ---: |
| materials | 1596 | 1336 |
| bankedMaterials | 714 | 91 |
| level | 21 | 20 |
| itemCount | 58 | 45 |
| enemyKills | 4867 | 4554 |
| treeKills | 42 | 50 |
| materialPickups | 1854 | 2009 |
| harvesting | 78 | 115.0 |
| luck | 68 | 4.0 |

## Wave-end divergence timeline

Snapshots use each fixture's wave-timer callback, not an identical settlement phase: the original fixture connects after Main's existing timer handler, whereas Unity records before Rules.Tick settlement. Gold/stat comparisons at this boundary may therefore include original harvesting already paid. Equal base loadouts do not reproduce cross-engine spawn placement, target order or physics. First-wave kill/drop differences still precede shop purchases.

| Wave | Enemy kills ref / Unity | Pickups ref / Unity | Materials ref / Unity | Items ref / Unity |
| --- | --- | --- | --- | --- |
| 1 | 28 / 27 | 8 / 5 | 8 / 5 | 0 / 0 |
| 2 | 65 / 68 | 16 / 21 | 20 / 32 | 0 / 0 |
| 3 | 125 / 133 | 34 / 41 | 33 / 32 | 1 / 1 |
| 4 | 202 / 210 | 72 / 69 | 83 / 59 | 2 / 2 |
| 5 | 337 / 341 | 135 / 102 | 167 / 79 | 3 / 3 |
| 6 | 504 / 498 | 194 / 143 | 140 / 100 | 6 / 5 |
| 7 | 721 / 701 | 335 / 206 | 261 / 159 | 9 / 8 |
| 8 | 883 / 847 | 414 / 231 | 289 / 97 | 13 / 9 |
| 9 | 1322 / 1260 | 520 / 386 | 261 / 355 | 16 / 11 |
| 10 | 1682 / 1572 | 641 / 486 | 295 / 436 | 21 / 16 |
| 11 | 1916 / 1802 | 740 / 631 | 480 / 544 | 23 / 19 |
| 12 | 2217 / 2081 | 859 / 769 | 458 / 644 | 26 / 23 |
| 13 | 2474 / 2356 | 952 / 879 | 583 / 665 | 30 / 25 |
| 14 | 2707 / 2594 | 1047 / 985 | 652 / 798 | 34 / 28 |
| 15 | 3141 / 3018 | 1138 / 1161 | 652 / 808 | 37 / 31 |
| 16 | 3484 / 3306 | 1219 / 1351 | 746 / 946 | 40 / 33 |
| 17 | 3708 / 3512 | 1445 / 1568 | 879 / 1294 | 44 / 37 |
| 18 | 4048 / 3808 | 1595 / 1727 | 1083 / 1498 | 50 / 38 |
| 19 | 4590 / 4319 | 1733 / 1874 | 1356 / 1346 | 54 / 43 |
| 20 | 4867 / 4554 | 1854 / 2009 | 1492 / 1336 | 58 / 45 |

## Final-wave settlement

Recorded final-wave harvesting at credit: 109.0; minimum base credit 109; observed credit 109, measured by explicit settlement receipt. New traces carry a settlement receipt because timer snapshots now follow credit. Legacy R19 retains its failing timer-to-win measurement. This is a neutral-fixture lower bound, not an exact all-effect payout formula.

## Wall-clock timing limitation

The duration checks above measure active simulated combat time. They do not establish wall-clock or performance parity. Start-to-start wall intervals also include transitions and any elapsed time the driver did not consume. The driver uses `Time.deltaTime`, which is subject to Unity's maximum-delta setting. There is no frame-profiler or complete window-intervention trace to attribute longer intervals. Earlier R19 captures had differing resolution; that historical observation does not establish the cause of longer intervals in this campaign.

| Wave | Wall seconds until next wave start | Active combat seconds |
| --- | ---: | ---: |
| 1 | 22.225 | 20.01653289794922 |
| 2 | 27.068 | 25.016284942626953 |
| 3 | 32.001 | 30.018733978271484 |
| 4 | 37.038 | 35.024147033691406 |
| 5 | 42.051 | 40.015106201171875 |
| 6 | 47.041 | 45.00567626953125 |
| 7 | 52.013 | 50.00445556640625 |
| 8 | 57.027 | 55.00325012207031 |
| 9 | 62.096 | 60.00201416015625 |
| 10 | 62.004 | 60.00201416015625 |
| 11 | 62.038 | 60.00201416015625 |
| 12 | 62.024 | 60.00201416015625 |
| 13 | 62.031 | 60.00201416015625 |
| 14 | 62.046 | 60.00201416015625 |
| 15 | 62.041 | 60.00201416015625 |
| 16 | 62.040 | 60.00201416015625 |
| 17 | 61.993 | 60.00201416015625 |
| 18 | 62.076 | 60.00201416015625 |
| 19 | 62.022 | 60.00201416015625 |

### Combat versus cleanup wall intervals

Measured directly from wave_start, wave_timer_end and the next wave_start (or final finished). Positive combat excess is observed wall time beyond active simulation; its cause is not established. Cleanup/choice time is measured separately, so shop outcomes are not used to explain combat-window stalls.

| Wave | Active seconds | Combat wall seconds | Combat wall excess | Cleanup/choices wall seconds |
| --- | ---: | ---: | ---: | ---: |
| 1 | 20.017 | 20.179 | 0.163 | 2.046 |
| 2 | 25.016 | 24.999 | -0.017 | 2.069 |
| 3 | 30.019 | 29.960 | -0.058 | 2.040 |
| 4 | 35.024 | 34.994 | -0.030 | 2.044 |
| 5 | 40.015 | 39.990 | -0.025 | 2.061 |
| 6 | 45.006 | 44.970 | -0.036 | 2.071 |
| 7 | 50.004 | 49.969 | -0.035 | 2.044 |
| 8 | 55.003 | 54.984 | -0.019 | 2.042 |
| 9 | 60.002 | 59.989 | -0.013 | 2.107 |
| 10 | 60.002 | 59.931 | -0.071 | 2.073 |
| 11 | 60.002 | 59.958 | -0.044 | 2.081 |
| 12 | 60.002 | 59.959 | -0.043 | 2.066 |
| 13 | 60.002 | 59.952 | -0.050 | 2.078 |
| 14 | 60.002 | 59.966 | -0.036 | 2.080 |
| 15 | 60.002 | 59.958 | -0.044 | 2.083 |
| 16 | 60.002 | 59.949 | -0.053 | 2.090 |
| 17 | 60.002 | 59.933 | -0.069 | 2.060 |
| 18 | 60.002 | 59.982 | -0.021 | 2.094 |
| 19 | 60.002 | 59.938 | -0.064 | 2.084 |
| 20 | 90.074 | 89.924 | -0.149 | 4.021 |

### First-wave material accounting

The first wave diverged before either run acquired an item or upgrade. The original killed 28 enemies and no trees: 8 material pickups + 20 remaining ground value = 28. Scrapshift killed 27 enemies and one tree: 5 pickups + 25 ground value = 30. The supplied `entities/units/neutral/tree_stats.tres` sets tree value to 3; `RunRules.MaterialDropAmount` preserves base value with zero drop modifiers. Thus 27 + 3 = 30 reconciles this observed difference. This rules out an obvious missing-material accounting error in these snapshots; it does not establish matching spawn placement, targeting or pickup trajectories.

## First acquisition differences

Different source and Unity random sequences, offers, movement and upgrades prevent a causal conclusion from an aggregate difference. This list identifies follow-up items, not automatically bugs.

| Item | Reference first seen | Scrapshift first seen |
| --- | --- | --- |
| alien_tongue | not acquired | wave 8 (purchase) |
| baby_elephant | wave 4 (purchase) | not acquired |
| baby_gecko | wave 3 (purchase) | wave 17 (purchase) |
| bag | wave 8 (crate_take) | wave 8 (purchase) |
| bait | wave 13 (purchase) | not acquired |
| banner | not acquired | wave 14 (purchase) |
| bat | wave 6 (purchase) | not acquired |
| black_belt | wave 12 (crate_take) | not acquired |
| blood_leech | wave 16 (purchase) | not acquired |
| book | not acquired | wave 5 (purchase) |
| boxing_glove | wave 9 (crate_take) | not acquired |
| broken_mouth | wave 12 (purchase) | not acquired |
| campfire | not acquired | wave 18 (purchase) |
| charcoal | not acquired | wave 6 (purchase) |
| clockwork_wasp | not acquired | wave 11 (purchase) |
| cog | not acquired | wave 16 (crate_take) |
| coil | not acquired | wave 10 (purchase) |
| cute_monkey | not acquired | wave 6 (purchase) |
| cyclops_worm | not acquired | wave 14 (purchase) |
| dangerous_bunny | not acquired | wave 4 (purchase) |
| duct_tape | wave 7 (purchase) | wave 3 (purchase) |
| dynamite | wave 14 (crate_take) | wave 11 (purchase) |
| extra_stomach | wave 17 (crate_take) | not acquired |
| fertilizer | wave 12 (crate_take) | not acquired |
| fresh_meat | not acquired | wave 2 (purchase) |
| frozen_heart | wave 8 (purchase) | not acquired |
| garden | not acquired | wave 11 (crate_take) |
| gentle_alien | wave 6 (purchase) | not acquired |
| goat_skull | wave 7 (crate_take) | not acquired |
| head_injury | not acquired | wave 12 (purchase) |
| helmet | wave 5 (purchase) | not acquired |
| honey | not acquired | wave 12 (purchase) |
| ice_cube | wave 15 (purchase) | not acquired |
| landmines | not acquired | wave 18 (purchase) |
| lemonade | wave 7 (purchase) | wave 9 (crate_take) |
| little_frog | wave 19 (purchase) | not acquired |
| little_muscley_dude | wave 9 (purchase) | not acquired |
| lumberjack_shirt | not acquired | wave 9 (purchase) |
| metal_detector | wave 11 (purchase) | not acquired |
| missile | wave 13 (crate_take) | not acquired |
| mutation | wave 17 (purchase) | not acquired |
| peaceful_bee | wave 6 (purchase) | wave 6 (crate_take) |
| pencil | not acquired | wave 18 (purchase) |
| piggy_bank | wave 10 (purchase) | not acquired |
| pile_of_books | wave 11 (purchase) | not acquired |
| plant | wave 13 (purchase) | not acquired |
| pocket_factory | wave 5 (purchase) | wave 7 (purchase) |
| propeller_hat | wave 15 (purchase) | not acquired |
| pumpkin | wave 12 (purchase) | not acquired |
| recycling_machine | wave 8 (purchase) | wave 9 (purchase) |
| reinforced_steel | wave 15 (crate_take) | not acquired |
| scope | wave 10 (crate_take) | not acquired |
| shady_potion | wave 9 (purchase) | not acquired |
| sharp_bullet | wave 17 (purchase) | not acquired |
| small_magazine | wave 14 (purchase) | not acquired |
| snake | wave 19 (crate_take) | wave 13 (crate_take) |
| snowball | not acquired | wave 14 (purchase) |
| statue | wave 14 (purchase) | wave 13 (purchase) |
| tardigrade | wave 13 (purchase) | not acquired |
| terrified_onion | wave 2 (purchase) | not acquired |
| tractor | not acquired | wave 18 (purchase) |
| tree | not acquired | wave 9 (crate_take) |
| triangle_of_power | wave 9 (crate_take) | not acquired |
| turret_flame | wave 17 (purchase) | not acquired |
| turret_healing | not acquired | wave 15 (purchase) |
| turret_laser | not acquired | wave 19 (purchase) |
| ugly_tooth | wave 9 (purchase) | wave 16 (purchase) |
| weird_food | wave 7 (purchase) | wave 5 (purchase) |
| weird_ghost | wave 5 (crate_take) | wave 10 (purchase) |

## Interpretation

Both runs use a neutral scripted six-weapon loadout, Danger 0, invulnerability, mute, scripted movement and fresh profiles at normal time. The supplied-reference trace at `artifacts\full-verification-2026-10-17\original-attempt-2\trace.json` records 1 fixture pause intervention(s). Its source manifest is an explicit hashed input. The manifest hashes bind the saved evidence, not an independent visual audit.

The unlock-eligibility check applies only to real item IDs acquired by purchase or crate in this fresh run. It checks the imported catalog's run-start availability. It does not sample every shop/crate candidate, establish every unlock trigger, or validate subsequent character-specific runs. Item stack sums do not prove each effect formula. Screenshots and physical controls need separate review. No full-run totals are treated as equality requirements.

## Current R23 completion and validation history

This full R23 campaign completed Passed/Won, 20 waves/19 shops, native exit 0. The actual wave 20 receipt credits materials 1227 to 1336: 109 materials at harvesting 109, followed by growth to 115. Final inventory has 45 item copies across 36 distinct stacks; all recorded stack sums/caps and purchase debits passed. Complete-suite attempt 9 passed 1031/1031, zero failures/skips/inconclusive, exit 0, 58.5371699 seconds. Native virtual-navigation 17 snapshots and staged settlement 2 routes passed separately; they are not human physical-controller acceptance.

R23 combat windows totaled 1049.485831 wall seconds for 1050.199951 active seconds (signed excess -0.714120), with 43.335596 seconds of separate cleanup/choice intervals. Player-summary wall duration 1093.956055 seconds. The earlier R21 combat-window excess +170.536661 seconds was not reproduced after the allocation corrections in this new run. Random acquisitions/combat totals differ, so these numbers are not a controlled identical-workload FPS benchmark or proof of all-hardware performance parity.

Historical R19 JSON remains 17/18 at `economy-comparison.json`; R21 remains 18/18 at `economy-comparison-r21.json`. Prior reports/notes are preserved as `economy-comparison-r19-history.md` and `economy-comparison-r21-history.md`. The current result is separately hashed in `economy-comparison-r23.json`; no earlier run was relabeled.

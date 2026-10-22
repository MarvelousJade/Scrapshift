# R29 headless full-run economy comparison - October 17, 2026

Generated from saved traces by `scripts/compare_r14_campaign.py`. The supplied-reference normal-time run at the input path and the Scrapshift run are separate scripted fixtures. The script records their exact file hashes below.

Bounded checks: **18/18 passed**. This result does not establish exact gameplay parity.

## Evidence

JSON: `artifacts\parity-remaining-2026-10-20\headless-economy-comparison-r29.json`.

| Input | SHA-256 |
| --- | --- |
| `referenceTrace`: `E:\Codex Projects\Job Search Assistant\work\repos\Scrapshift\artifacts\parity-remaining-2026-10-20\original-full-headless\trace.json` | `eee5a1481fdb279b91cda3d7d226d0b3cbce8775c4097f1c04eac61e24fa7d74` |
| `referenceSummary`: `E:\Codex Projects\Job Search Assistant\work\repos\Scrapshift\artifacts\parity-remaining-2026-10-20\original-full-headless\summary.json` | `fba28a944a2fedc570be8ec0da459cbc57c4f64aa35c1bea3d93142f8ebc52f8` |
| `referenceSourceManifest`: `E:\Codex Projects\Job Search Assistant\work\repos\Scrapshift\artifacts\parity-remaining-2026-10-20\original-full-headless\source-manifest.json` | `f354606517bba1932472ba18972b1c8fae9f5399b68403f3e41f3128f5625de2` |
| `unityTrace`: `E:\Codex Projects\Job Search Assistant\work\repos\Scrapshift\artifacts\parity-remaining-2026-10-20\unity-full-headless-r29\campaign-trace.json` | `dac1521165c528e3a33d6e222f08cd6a31bb27cd99fe417a43162a6f9a05b948` |
| `unitySummary`: `E:\Codex Projects\Job Search Assistant\work\repos\Scrapshift\artifacts\parity-remaining-2026-10-20\unity-full-headless-r29\campaign-check.json` | `ea10f9d234faeeb8e48b290ec786ea2e0fa8a7aa2304b9a480edc83db10f0723` |
| `unityProcess`: `E:\Codex Projects\Job Search Assistant\work\repos\Scrapshift\artifacts\parity-remaining-2026-10-20\unity-full-headless-r29\process.json` | `753087a6475789d336a55b89a18230705fbb3fcd08dde4593252a0f5725227dd` |
| `buildManifest`: `E:\Codex Projects\Job Search Assistant\work\repos\Scrapshift\artifacts\parity-remaining-2026-10-20\build-r29-manifest.json` | `de9c3bf54e1cb84bd31252d52e1aeffb6b0dbc4c2e848932d589f2c5f247a233` |
| `sourceManifest`: `E:\Codex Projects\Job Search Assistant\work\repos\Scrapshift\artifacts\parity-remaining-2026-10-20\build-r29-source-manifest.json` | `e823a10a4a74be6d7dd87e356cc26c72fbe270a723d75ddde3ffe475598d5b1b` |
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

Purchases: reference 31 (31 debit checks); Scrapshift 40 (40 debit checks). Different prices and purchase counts are not required to equal across random runs.
Stacks: reference final 32/32; Scrapshift final 45/45. Scrapshift inconsistent snapshots: 0.
Positive catalog stack-cap violations: reference final 0; Scrapshift snapshots 0. These neutral fixtures have no character-specific cap modifiers.
Initial-catalog locked acquisitions in the fresh Scrapshift run: 0; unknown acquisitions: 0.

## Observed run totals

| Metric | Reference | Scrapshift |
| --- | ---: | ---: |
| materials | 995 | 2036 |
| bankedMaterials | 1045 | 438 |
| level | 18 | 19 |
| itemCount | 32 | 45 |
| enemyKills | 4654 | 4653 |
| treeKills | 48 | 28 |
| materialPickups | 1514 | 1730 |
| harvesting | 51 | 101.0 |
| luck | -20 | 8.0 |

## Wave-end divergence timeline

Snapshots use each fixture's wave-timer callback, not an identical settlement phase: the original fixture connects after Main's existing timer handler, whereas Unity records before Rules.Tick settlement. Gold/stat comparisons at this boundary may therefore include original harvesting already paid. Equal base loadouts do not reproduce cross-engine spawn placement, target order or physics. First-wave kill/drop differences still precede shop purchases.

| Wave | Enemy kills ref / Unity | Pickups ref / Unity | Materials ref / Unity | Items ref / Unity |
| --- | --- | --- | --- | --- |
| 1 | 25 / 27 | 5 / 5 | 5 / 5 | 0 / 0 |
| 2 | 63 / 69 | 19 / 21 | 27 / 33 | 0 / 1 |
| 3 | 126 / 135 | 48 / 43 | 48 / 34 | 1 / 3 |
| 4 | 209 / 211 | 78 / 74 | 68 / 63 | 2 / 4 |
| 5 | 339 / 339 | 111 / 96 | 78 / 44 | 4 / 7 |
| 6 | 497 / 496 | 141 / 117 | 99 / 46 | 5 / 8 |
| 7 | 705 / 701 | 194 / 161 | 155 / 109 | 8 / 9 |
| 8 | 862 / 851 | 251 / 209 | 192 / 136 | 10 / 11 |
| 9 | 1277 / 1255 | 301 / 308 | 170 / 263 | 12 / 14 |
| 10 | 1592 / 1574 | 378 / 381 | 166 / 228 | 15 / 16 |
| 11 | 1824 / 1792 | 475 / 433 | 326 / 189 | 15 / 18 |
| 12 | 2113 / 2076 | 570 / 536 | 462 / 267 | 15 / 21 |
| 13 | 2371 / 2338 | 653 / 649 | 301 / 336 | 18 / 24 |
| 14 | 2626 / 2569 | 747 / 790 | 455 / 358 | 21 / 28 |
| 15 | 3064 / 2972 | 825 / 983 | 384 / 526 | 24 / 31 |
| 16 | 3360 / 3290 | 980 / 1149 | 580 / 544 | 25 / 36 |
| 17 | 3581 / 3528 | 1201 / 1372 | 813 / 987 | 28 / 37 |
| 18 | 3911 / 3841 | 1323 / 1509 | 957 / 1551 | 29 / 38 |
| 19 | 4427 / 4395 | 1435 / 1642 | 1070 / 1991 | 30 / 41 |
| 20 | 4654 / 4653 | 1514 / 1730 | 947 / 2036 | 32 / 45 |

## Final-wave settlement

Recorded final-wave harvesting at credit: 96.0; minimum base credit 96; observed credit 96, measured by explicit settlement receipt. New traces carry a settlement receipt because timer snapshots now follow credit. Legacy R19 retains its failing timer-to-win measurement. This is a neutral-fixture lower bound, not an exact all-effect payout formula.

## Wall-clock timing limitation

The duration checks above measure active simulated combat time. They do not establish wall-clock or performance parity. Start-to-start wall intervals also include transitions and any elapsed time the driver did not consume. The driver uses `Time.deltaTime`, which is subject to Unity's maximum-delta setting. There is no frame-profiler or complete window-intervention trace to attribute longer intervals. Earlier R19 captures had differing resolution; that historical observation does not establish the cause of longer intervals in this campaign.

| Wave | Wall seconds until next wave start | Active combat seconds |
| --- | ---: | ---: |
| 1 | 22.063 | 20.01653289794922 |
| 2 | 27.071 | 25.016284942626953 |
| 3 | 32.004 | 30.018733978271484 |
| 4 | 37.064 | 35.024147033691406 |
| 5 | 41.997 | 40.015106201171875 |
| 6 | 47.037 | 45.00567626953125 |
| 7 | 52.051 | 50.00445556640625 |
| 8 | 57.030 | 55.00325012207031 |
| 9 | 62.040 | 60.00201416015625 |
| 10 | 62.038 | 60.00201416015625 |
| 11 | 62.056 | 60.00201416015625 |
| 12 | 62.002 | 60.00201416015625 |
| 13 | 62.096 | 60.00201416015625 |
| 14 | 61.976 | 60.00201416015625 |
| 15 | 62.073 | 60.00201416015625 |
| 16 | 61.987 | 60.00201416015625 |
| 17 | 62.032 | 60.00201416015625 |
| 18 | 62.078 | 60.00201416015625 |
| 19 | 62.014 | 60.00201416015625 |

### Combat versus cleanup wall intervals

Measured directly from wave_start, wave_timer_end and the next wave_start (or final finished). Positive combat excess is observed wall time beyond active simulation; its cause is not established. Cleanup/choice time is measured separately, so shop outcomes are not used to explain combat-window stalls.

| Wave | Active seconds | Combat wall seconds | Combat wall excess | Cleanup/choices wall seconds |
| --- | ---: | ---: | ---: | ---: |
| 1 | 20.017 | 20.024 | 0.008 | 2.038 |
| 2 | 25.016 | 25.002 | -0.015 | 2.069 |
| 3 | 30.019 | 29.960 | -0.059 | 2.044 |
| 4 | 35.024 | 34.986 | -0.038 | 2.079 |
| 5 | 40.015 | 39.957 | -0.059 | 2.040 |
| 6 | 45.006 | 44.988 | -0.017 | 2.048 |
| 7 | 50.004 | 49.993 | -0.011 | 2.058 |
| 8 | 55.003 | 54.965 | -0.038 | 2.065 |
| 9 | 60.002 | 59.980 | -0.022 | 2.060 |
| 10 | 60.002 | 59.974 | -0.028 | 2.064 |
| 11 | 60.002 | 59.972 | -0.030 | 2.084 |
| 12 | 60.002 | 59.934 | -0.069 | 2.069 |
| 13 | 60.002 | 59.973 | -0.029 | 2.123 |
| 14 | 60.002 | 59.906 | -0.096 | 2.070 |
| 15 | 60.002 | 59.974 | -0.028 | 2.100 |
| 16 | 60.002 | 59.926 | -0.076 | 2.061 |
| 17 | 60.002 | 59.973 | -0.029 | 2.060 |
| 18 | 60.002 | 59.981 | -0.021 | 2.097 |
| 19 | 60.002 | 59.925 | -0.077 | 2.089 |
| 20 | 90.074 | 89.925 | -0.149 | 4.025 |

## First acquisition differences

Different source and Unity random sequences, offers, movement and upgrades prevent a causal conclusion from an aggregate difference. This list identifies follow-up items, not automatically bugs.

| Item | Reference first seen | Scrapshift first seen |
| --- | --- | --- |
| adrenaline | wave 15 (purchase) | wave 10 (purchase) |
| alien_baby | wave 19 (purchase) | not acquired |
| alien_tongue | not acquired | wave 11 (purchase) |
| baby_elephant | not acquired | wave 4 (purchase) |
| bait | not acquired | wave 14 (purchase) |
| banner | not acquired | wave 19 (purchase) |
| black_belt | wave 17 (purchase) | not acquired |
| blindfold | wave 13 (crate_take) | not acquired |
| blood_donation | not acquired | wave 13 (purchase) |
| book | not acquired | wave 7 (purchase) |
| boxing_glove | not acquired | wave 8 (purchase) |
| cake | wave 6 (purchase) | not acquired |
| campfire | not acquired | wave 13 (purchase) |
| coffee | wave 4 (purchase) | not acquired |
| cog | wave 14 (purchase) | not acquired |
| coupon | wave 4 (purchase) | wave 2 (purchase) |
| cute_monkey | wave 13 (purchase) | not acquired |
| cyclops_worm | not acquired | wave 15 (purchase) |
| defective_steroids | not acquired | wave 13 (purchase) |
| energy_bracelet | not acquired | wave 14 (purchase) |
| extra_stomach | wave 12 (purchase) | not acquired |
| eyes_surgery | not acquired | wave 12 (crate_take) |
| gambling_token | not acquired | wave 19 (purchase) |
| gentle_alien | not acquired | wave 15 (crate_take) |
| glasses | not acquired | wave 1 (crate_take) |
| gummy_berserker | wave 7 (purchase) | not acquired |
| helmet | not acquired | wave 12 (purchase) |
| insanity | not acquired | wave 4 (crate_take) |
| landmines | wave 2 (purchase) | not acquired |
| lens | wave 6 (crate_take) | not acquired |
| little_muscley_dude | wave 9 (purchase) | not acquired |
| mastery | not acquired | wave 19 (crate_take) |
| medal | not acquired | wave 17 (crate_take) |
| metal_plate | wave 13 (crate_take) | not acquired |
| mushroom | wave 9 (purchase) | not acquired |
| nail | wave 16 (purchase) | not acquired |
| peaceful_bee | not acquired | wave 6 (purchase) |
| pencil | wave 9 (purchase) | wave 2 (purchase) |
| piggy_bank | not acquired | wave 15 (purchase) |
| plant | wave 7 (purchase) | not acquired |
| power_generator | not acquired | wave 19 (purchase) |
| propeller_hat | not acquired | wave 18 (purchase) |
| reinforced_steel | wave 8 (purchase) | not acquired |
| riposte | wave 12 (purchase) | wave 14 (purchase) |
| scar | wave 19 (purchase) | not acquired |
| shackles | not acquired | wave 9 (purchase) |
| sharp_bullet | wave 3 (purchase) | not acquired |
| small_magazine | wave 16 (crate_take) | not acquired |
| snake | not acquired | wave 7 (purchase) |
| statue | not acquired | wave 12 (purchase) |
| sunglasses | not acquired | wave 15 (purchase) |
| terrified_onion | wave 6 (purchase) | not acquired |
| toxic_sludge | wave 8 (purchase) | wave 11 (crate_take) |
| turret | not acquired | wave 13 (purchase) |
| ugly_tooth | not acquired | wave 3 (purchase) |
| weird_food | not acquired | wave 4 (purchase) |
| weird_ghost | wave 18 (purchase) | wave 10 (purchase) |

## Interpretation

Both runs use a neutral scripted six-weapon loadout, Danger 0, invulnerability, mute, scripted movement and fresh profiles at normal time. The supplied-reference trace at `artifacts\parity-remaining-2026-10-20\original-full-headless\trace.json` records 1 fixture pause intervention(s). Its source manifest is an explicit hashed input. The manifest hashes bind the saved evidence, not an independent visual audit.

The unlock-eligibility check applies only to real item IDs acquired by purchase or crate in this fresh run. It checks the imported catalog's run-start availability. It does not sample every shop/crate candidate, establish every unlock trigger, or validate subsequent character-specific runs. Item stack sums do not prove each effect formula. Screenshots and physical controls need separate review. No full-run totals are treated as equality requirements.

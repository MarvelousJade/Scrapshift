# R40 full-run economy comparison - October 17, 2026

Generated from saved traces by `scripts/compare_r14_campaign.py`. The supplied-reference normal-time run at the input path and the Scrapshift run are separate scripted fixtures. The script records their exact file hashes below.

Bounded checks: **18/18 passed**. This result does not establish exact gameplay parity.

## Evidence

JSON: `artifacts\native-followup-2026-10-20\full-campaign-r40\economy-comparison.json`.

| Input | SHA-256 |
| --- | --- |
| `referenceTrace`: `E:\Codex Projects\Job Search Assistant\work\repos\Scrapshift\artifacts\native-followup-2026-10-20\original-safe-full\trace.json` | `5c6d6ad78d550abfb7569ce0b64538d222cb7cecdc42eced439088c91651d740` |
| `referenceSummary`: `E:\Codex Projects\Job Search Assistant\work\repos\Scrapshift\artifacts\native-followup-2026-10-20\original-safe-full\summary.json` | `5c3a41b49b2edb8aa8eb5b9eb5e489516f0a06dea3102f1d09bff812041560c0` |
| `referenceSourceManifest`: `E:\Codex Projects\Job Search Assistant\work\repos\Scrapshift\artifacts\native-followup-2026-10-20\reference-safe-policy-preparation\clone-after.json` | `f50a8bd5716e2faed6169cd23df9716fa7c5ce749209361dcb588c1c8fd2ca16` |
| `unityTrace`: `E:\Codex Projects\Job Search Assistant\work\repos\Scrapshift\artifacts\native-followup-2026-10-20\full-campaign-r40\campaign-trace.json` | `4193df365ffae53819e81f4be5ac32c94ab41385b809ef57429b0b4891ab850c` |
| `unitySummary`: `E:\Codex Projects\Job Search Assistant\work\repos\Scrapshift\artifacts\native-followup-2026-10-20\full-campaign-r40\campaign-check.json` | `bd09524854164238326b178e8c549654f92ec383df1e39eda78e1dd0994e6951` |
| `unityProcess`: `E:\Codex Projects\Job Search Assistant\work\repos\Scrapshift\artifacts\native-followup-2026-10-20\full-campaign-r40\process.json` | `f2e5ef6f29162e9b2c59d8929d2788ccce725e4c5c7551f7b1f8d24c20c75b53` |
| `buildManifest`: `E:\Codex Projects\Job Search Assistant\work\repos\Scrapshift\artifacts\native-followup-2026-10-20\build-r40-manifest.json` | `e8257bbe86eba566b86b3ee2d61674e4aace590cafdb32589036b470132b0021` |
| `sourceManifest`: `E:\Codex Projects\Job Search Assistant\work\repos\Scrapshift\artifacts\native-followup-2026-10-20\build-r40-source-manifest.json` | `4319d1d020bbf431d087f1647558f54e5e846fb77649b4f96791cfabd263a758` |
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

Purchases: reference 29 (29 debit checks); Scrapshift 36 (36 debit checks). Different prices and purchase counts are not required to equal across random runs.
Stacks: reference final 43/43; Scrapshift final 41/41. Scrapshift inconsistent snapshots: 0.
Positive catalog stack-cap violations: reference final 0; Scrapshift snapshots 0. These neutral fixtures have no character-specific cap modifiers.
Initial-catalog locked acquisitions in the fresh Scrapshift run: 0; unknown acquisitions: 0.

## Observed run totals

| Metric | Reference | Scrapshift |
| --- | ---: | ---: |
| materials | 1472 | 460 |
| bankedMaterials | 1084 | 1309 |
| level | 18 | 18 |
| itemCount | 43 | 41 |
| enemyKills | 4647 | 4931 |
| treeKills | 43 | 37 |
| materialPickups | 1490 | 1558 |
| harvesting | 25 | 66.0 |
| luck | 83 | 19.0 |

## Wave-end divergence timeline

Snapshots use each fixture's wave-timer callback, not an identical settlement phase: the original fixture connects after Main's existing timer handler, whereas Unity records before Rules.Tick settlement. Gold/stat comparisons at this boundary may therefore include original harvesting already paid. Equal base loadouts do not reproduce cross-engine spawn placement, target order or physics. First-wave kill/drop differences still precede shop purchases.

| Wave | Enemy kills ref / Unity | Pickups ref / Unity | Materials ref / Unity | Items ref / Unity |
| --- | --- | --- | --- | --- |
| 1 | 25 / 27 | 8 / 5 | 8 / 5 | 0 / 0 |
| 2 | 67 / 68 | 22 / 23 | 31 / 35 | 0 / 1 |
| 3 | 133 / 130 | 44 / 41 | 67 / 41 | 0 / 2 |
| 4 | 212 / 209 | 69 / 78 | 52 / 85 | 1 / 3 |
| 5 | 342 / 339 | 105 / 108 | 85 / 145 | 2 / 3 |
| 6 | 499 / 493 | 145 / 140 | 94 / 129 | 4 / 6 |
| 7 | 705 / 699 | 187 / 170 | 111 / 106 | 8 / 8 |
| 8 | 857 / 860 | 254 / 252 | 144 / 190 | 11 / 10 |
| 9 | 1267 / 1278 | 308 / 307 | 161 / 131 | 12 / 13 |
| 10 | 1586 / 1614 | 431 / 421 | 316 / 312 | 15 / 14 |
| 11 | 1803 / 1850 | 561 / 523 | 456 / 220 | 18 / 17 |
| 12 | 2097 / 2147 | 681 / 650 | 399 / 332 | 23 / 18 |
| 13 | 2353 / 2438 | 783 / 723 | 534 / 264 | 26 / 23 |
| 14 | 2626 / 2729 | 826 / 804 | 161 / 264 | 29 / 26 |
| 15 | 3036 / 3198 | 889 / 875 | 225 / 266 | 31 / 28 |
| 16 | 3339 / 3538 | 955 / 926 | 220 / 223 | 32 / 30 |
| 17 | 3568 / 3778 | 1106 / 1128 | 442 / 532 | 35 / 32 |
| 18 | 3871 / 4132 | 1245 / 1315 | 783 / 696 | 36 / 34 |
| 19 | 4385 / 4665 | 1390 / 1420 | 1097 / 637 | 38 / 38 |
| 20 | 4647 / 4931 | 1490 / 1558 | 1449 / 460 | 43 / 41 |

## Final-wave settlement

Recorded final-wave harvesting at credit: 58.0; minimum base credit 58; observed credit 58, measured by explicit settlement receipt. New traces carry a settlement receipt because timer snapshots now follow credit. Legacy R19 retains its failing timer-to-win measurement. This is a neutral-fixture lower bound, not an exact all-effect payout formula.

## Wall-clock timing limitation

The duration checks above measure active simulated combat time. They do not establish wall-clock or performance parity. Start-to-start wall intervals also include transitions and any elapsed time the driver did not consume. The driver uses `Time.deltaTime`, which is subject to Unity's maximum-delta setting. There is no frame-profiler or complete window-intervention trace to attribute longer intervals. Earlier R19 captures had differing resolution; that historical observation does not establish the cause of longer intervals in this campaign.

| Wave | Wall seconds until next wave start | Active combat seconds |
| --- | ---: | ---: |
| 1 | 22.124 | 20.01653289794922 |
| 2 | 26.990 | 25.016284942626953 |
| 3 | 32.038 | 30.018733978271484 |
| 4 | 37.025 | 35.024147033691406 |
| 5 | 42.088 | 40.015106201171875 |
| 6 | 47.002 | 45.00567626953125 |
| 7 | 52.036 | 50.00445556640625 |
| 8 | 57.057 | 55.00325012207031 |
| 9 | 62.002 | 60.00201416015625 |
| 10 | 62.093 | 60.00201416015625 |
| 11 | 61.993 | 60.00201416015625 |
| 12 | 62.094 | 60.00201416015625 |
| 13 | 61.983 | 60.00201416015625 |
| 14 | 62.021 | 60.00201416015625 |
| 15 | 62.048 | 60.00201416015625 |
| 16 | 62.021 | 60.00201416015625 |
| 17 | 62.056 | 60.00201416015625 |
| 18 | 62.047 | 60.00201416015625 |
| 19 | 62.021 | 60.00201416015625 |

### Combat versus cleanup wall intervals

Measured directly from wave_start, wave_timer_end and the next wave_start (or final finished). Positive combat excess is observed wall time beyond active simulation; its cause is not established. Cleanup/choice time is measured separately, so shop outcomes are not used to explain combat-window stalls.

| Wave | Active seconds | Combat wall seconds | Combat wall excess | Cleanup/choices wall seconds |
| --- | ---: | ---: | ---: | ---: |
| 1 | 20.017 | 20.046 | 0.030 | 2.078 |
| 2 | 25.016 | 24.949 | -0.067 | 2.041 |
| 3 | 30.019 | 29.994 | -0.025 | 2.044 |
| 4 | 35.024 | 34.981 | -0.043 | 2.044 |
| 5 | 40.015 | 39.993 | -0.022 | 2.096 |
| 6 | 45.006 | 44.950 | -0.056 | 2.053 |
| 7 | 50.004 | 49.964 | -0.041 | 2.072 |
| 8 | 55.003 | 54.974 | -0.030 | 2.083 |
| 9 | 60.002 | 59.939 | -0.063 | 2.063 |
| 10 | 60.002 | 59.971 | -0.031 | 2.123 |
| 11 | 60.002 | 59.918 | -0.084 | 2.075 |
| 12 | 60.002 | 59.965 | -0.037 | 2.129 |
| 13 | 60.002 | 59.891 | -0.111 | 2.092 |
| 14 | 60.002 | 59.945 | -0.057 | 2.076 |
| 15 | 60.002 | 59.964 | -0.039 | 2.084 |
| 16 | 60.002 | 59.948 | -0.054 | 2.072 |
| 17 | 60.002 | 59.964 | -0.038 | 2.092 |
| 18 | 60.002 | 59.942 | -0.060 | 2.105 |
| 19 | 60.002 | 59.920 | -0.082 | 2.100 |
| 20 | 90.074 | 89.916 | -0.158 | 4.026 |

## First acquisition differences

Different source and Unity random sequences, offers, movement and upgrades prevent a causal conclusion from an aggregate difference. This list identifies follow-up items, not automatically bugs.

| Item | Reference first seen | Scrapshift first seen |
| --- | --- | --- |
| adrenaline | not acquired | wave 17 (purchase) |
| alien_baby | wave 14 (crate_take) | not acquired |
| alien_tongue | wave 6 (crate_take) | wave 7 (purchase) |
| alien_worm | wave 19 (crate_take) | not acquired |
| baby_elephant | not acquired | wave 5 (purchase) |
| banner | not acquired | wave 19 (purchase) |
| bat | wave 5 (purchase) | not acquired |
| beanie | not acquired | wave 16 (purchase) |
| book | not acquired | wave 5 (purchase) |
| boxing_glove | not acquired | wave 18 (purchase) |
| cape | wave 12 (crate_take) | not acquired |
| chameleon | not acquired | wave 13 (crate_take) |
| clockwork_wasp | not acquired | wave 13 (purchase) |
| coil | wave 15 (purchase) | not acquired |
| crown | not acquired | wave 18 (purchase) |
| dangerous_bunny | wave 11 (purchase) | not acquired |
| defective_steroids | not acquired | wave 2 (purchase) |
| duct_tape | not acquired | wave 12 (purchase) |
| energy_bracelet | wave 19 (crate_take) | not acquired |
| fertilizer | wave 17 (purchase) | not acquired |
| frozen_heart | wave 13 (purchase) | not acquired |
| fuel_tank | not acquired | wave 18 (crate_take) |
| garden | not acquired | wave 14 (purchase) |
| glasses | not acquired | wave 6 (purchase) |
| gummy_berserker | wave 7 (purchase) | not acquired |
| handcuffs | not acquired | wave 12 (crate_take) |
| hedgehog | wave 7 (crate_take) | not acquired |
| injection | wave 6 (purchase) | not acquired |
| insanity | not acquired | wave 15 (purchase) |
| lemonade | not acquired | wave 7 (purchase) |
| lens | wave 11 (crate_take) | wave 3 (purchase) |
| little_frog | not acquired | wave 16 (purchase) |
| little_muscley_dude | not acquired | wave 12 (crate_take) |
| lost_duck | wave 11 (purchase) | not acquired |
| metal_detector | wave 6 (crate_take) | wave 10 (purchase) |
| mushroom | not acquired | wave 6 (purchase) |
| pencil | not acquired | wave 5 (purchase) |
| piggy_bank | wave 9 (crate_take) | not acquired |
| pile_of_books | wave 10 (crate_take) | not acquired |
| plant | wave 5 (crate_take) | not acquired |
| pocket_factory | wave 13 (crate_take) | not acquired |
| propeller_hat | wave 7 (purchase) | not acquired |
| reinforced_steel | wave 19 (crate_take) | not acquired |
| scar | wave 16 (purchase) | not acquired |
| scared_sausage | wave 18 (purchase) | not acquired |
| shady_potion | wave 3 (purchase) | not acquired |
| snake | wave 18 (purchase) | not acquired |
| sunglasses | not acquired | wave 8 (purchase) |
| terrified_onion | wave 6 (purchase) | not acquired |
| toxic_sludge | wave 10 (purchase) | wave 1 (crate_take) |
| turret_flame | not acquired | wave 10 (purchase) |
| turret_rocket | not acquired | wave 11 (purchase) |
| ugly_tooth | wave 4 (purchase) | wave 13 (purchase) |
| vigilante_ring | wave 11 (purchase) | wave 19 (purchase) |
| wandering_bot | wave 8 (purchase) | not acquired |
| warrior_helmet | wave 19 (crate_take) | not acquired |
| weird_ghost | not acquired | wave 8 (purchase) |
| wheelbarrow | not acquired | wave 12 (crate_take) |

## Interpretation

Both runs use a neutral scripted six-weapon loadout, Danger 0, invulnerability, mute, scripted movement and fresh profiles at normal time. The supplied-reference trace at `artifacts\native-followup-2026-10-20\original-safe-full\trace.json` records 1 fixture pause intervention(s). Its source manifest is an explicit hashed input. The manifest hashes bind the saved evidence, not an independent visual audit.

The unlock-eligibility check applies only to real item IDs acquired by purchase or crate in this fresh run. It checks the imported catalog's run-start availability. It does not sample every shop/crate candidate, establish every unlock trigger, or validate subsequent character-specific runs. Item stack sums do not prove each effect formula. Screenshots and physical controls need separate review. No full-run totals are treated as equality requirements.

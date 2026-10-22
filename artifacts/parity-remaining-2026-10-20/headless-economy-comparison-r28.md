# R28 headless full-run economy comparison - October 17, 2026

Generated from saved traces by `scripts/compare_r14_campaign.py`. The supplied-reference normal-time run at the input path and the Scrapshift run are separate scripted fixtures. The script records their exact file hashes below.

Bounded checks: **18/18 passed**. This result does not establish exact gameplay parity.

## Evidence

JSON: `artifacts\parity-remaining-2026-10-20\headless-economy-comparison-r28.json`.

| Input | SHA-256 |
| --- | --- |
| `referenceTrace`: `E:\Codex Projects\Job Search Assistant\work\repos\Scrapshift\artifacts\parity-remaining-2026-10-20\original-full-headless\trace.json` | `eee5a1481fdb279b91cda3d7d226d0b3cbce8775c4097f1c04eac61e24fa7d74` |
| `referenceSummary`: `E:\Codex Projects\Job Search Assistant\work\repos\Scrapshift\artifacts\parity-remaining-2026-10-20\original-full-headless\summary.json` | `fba28a944a2fedc570be8ec0da459cbc57c4f64aa35c1bea3d93142f8ebc52f8` |
| `referenceSourceManifest`: `E:\Codex Projects\Job Search Assistant\work\repos\Scrapshift\artifacts\parity-remaining-2026-10-20\original-full-headless\source-manifest.json` | `f354606517bba1932472ba18972b1c8fae9f5399b68403f3e41f3128f5625de2` |
| `unityTrace`: `E:\Codex Projects\Job Search Assistant\work\repos\Scrapshift\artifacts\parity-remaining-2026-10-20\unity-full-headless\campaign-trace.json` | `a00e77b56f2afe42b86eb41f31036e236cb038296e5f7f04c491296553d34441` |
| `unitySummary`: `E:\Codex Projects\Job Search Assistant\work\repos\Scrapshift\artifacts\parity-remaining-2026-10-20\unity-full-headless\campaign-check.json` | `a12c26f4fd6006b35ec960854dc62e54dd1802527fb16fa2900ce727950687af` |
| `unityProcess`: `E:\Codex Projects\Job Search Assistant\work\repos\Scrapshift\artifacts\parity-remaining-2026-10-20\unity-full-headless\process.json` | `f7ae1a1de4156c57a01ed1eed283badfb6a66f46339e98a3ca226340b0f60d32` |
| `buildManifest`: `E:\Codex Projects\Job Search Assistant\work\repos\Scrapshift\artifacts\parity-remaining-2026-10-20\build-r28-manifest.json` | `0d3353258c6cf2f7bb9987149a09a4b4038d3f8678b01297bbac088092187ff2` |
| `sourceManifest`: `E:\Codex Projects\Job Search Assistant\work\repos\Scrapshift\artifacts\parity-remaining-2026-10-20\build-r28-source-manifest.json` | `1810692b52f950f05e963bdaf1a9b9a5ee1753e5f0a84566abdbd694ea2eda10` |
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

Purchases: reference 31 (31 debit checks); Scrapshift 31 (31 debit checks). Different prices and purchase counts are not required to equal across random runs.
Stacks: reference final 32/32; Scrapshift final 35/35. Scrapshift inconsistent snapshots: 0.
Positive catalog stack-cap violations: reference final 0; Scrapshift snapshots 0. These neutral fixtures have no character-specific cap modifiers.
Initial-catalog locked acquisitions in the fresh Scrapshift run: 0; unknown acquisitions: 0.

## Observed run totals

| Metric | Reference | Scrapshift |
| --- | ---: | ---: |
| materials | 995 | 353 |
| bankedMaterials | 1045 | 1252 |
| level | 18 | 17 |
| itemCount | 32 | 35 |
| enemyKills | 4654 | 4687 |
| treeKills | 48 | 34 |
| materialPickups | 1514 | 1420 |
| harvesting | 51 | 27.0 |
| luck | -20 | 23.0 |

## Wave-end divergence timeline

Snapshots use each fixture's wave-timer callback, not an identical settlement phase: the original fixture connects after Main's existing timer handler, whereas Unity records before Rules.Tick settlement. Gold/stat comparisons at this boundary may therefore include original harvesting already paid. Equal base loadouts do not reproduce cross-engine spawn placement, target order or physics. First-wave kill/drop differences still precede shop purchases.

| Wave | Enemy kills ref / Unity | Pickups ref / Unity | Materials ref / Unity | Items ref / Unity |
| --- | --- | --- | --- | --- |
| 1 | 25 / 27 | 5 / 5 | 5 / 5 | 0 / 0 |
| 2 | 63 / 68 | 19 / 21 | 27 / 32 | 0 / 0 |
| 3 | 126 / 133 | 48 / 40 | 48 / 42 | 1 / 1 |
| 4 | 209 / 209 | 78 / 69 | 68 / 65 | 2 / 2 |
| 5 | 339 / 336 | 111 / 102 | 78 / 92 | 4 / 4 |
| 6 | 497 / 495 | 141 / 139 | 99 / 97 | 5 / 5 |
| 7 | 705 / 698 | 194 / 183 | 155 / 97 | 8 / 8 |
| 8 | 862 / 852 | 251 / 236 | 192 / 204 | 10 / 9 |
| 9 | 1277 / 1258 | 301 / 287 | 170 / 117 | 12 / 12 |
| 10 | 1592 / 1593 | 378 / 403 | 166 / 256 | 15 / 15 |
| 11 | 1824 / 1825 | 475 / 525 | 326 / 385 | 15 / 17 |
| 12 | 2113 / 2128 | 570 / 605 | 462 / 264 | 15 / 20 |
| 13 | 2371 / 2412 | 653 / 687 | 301 / 183 | 18 / 22 |
| 14 | 2626 / 2683 | 747 / 786 | 455 / 262 | 21 / 23 |
| 15 | 3064 / 3103 | 825 / 828 | 384 / 135 | 24 / 26 |
| 16 | 3360 / 3420 | 980 / 903 | 580 / 171 | 25 / 27 |
| 17 | 3581 / 3622 | 1201 / 1099 | 813 / 416 | 28 / 28 |
| 18 | 3911 / 3919 | 1323 / 1204 | 957 / 445 | 29 / 31 |
| 19 | 4427 / 4430 | 1435 / 1301 | 1070 / 282 | 30 / 34 |
| 20 | 4654 / 4687 | 1514 / 1420 | 947 / 353 | 32 / 35 |

## Final-wave settlement

Recorded final-wave harvesting at credit: 25.0; minimum base credit 25; observed credit 25, measured by explicit settlement receipt. New traces carry a settlement receipt because timer snapshots now follow credit. Legacy R19 retains its failing timer-to-win measurement. This is a neutral-fixture lower bound, not an exact all-effect payout formula.

## Wall-clock timing limitation

The duration checks above measure active simulated combat time. They do not establish wall-clock or performance parity. Start-to-start wall intervals also include transitions and any elapsed time the driver did not consume. The driver uses `Time.deltaTime`, which is subject to Unity's maximum-delta setting. There is no frame-profiler or complete window-intervention trace to attribute longer intervals. Earlier R19 captures had differing resolution; that historical observation does not establish the cause of longer intervals in this campaign.

| Wave | Wall seconds until next wave start | Active combat seconds |
| --- | ---: | ---: |
| 1 | 22.073 | 20.01653289794922 |
| 2 | 27.088 | 25.016284942626953 |
| 3 | 31.981 | 30.018733978271484 |
| 4 | 37.042 | 35.024147033691406 |
| 5 | 42.037 | 40.015106201171875 |
| 6 | 47.073 | 45.00567626953125 |
| 7 | 51.994 | 50.00445556640625 |
| 8 | 57.066 | 55.00325012207031 |
| 9 | 62.024 | 60.00201416015625 |
| 10 | 62.088 | 60.00201416015625 |
| 11 | 61.990 | 60.00201416015625 |
| 12 | 62.029 | 60.00201416015625 |
| 13 | 62.004 | 60.00201416015625 |
| 14 | 62.064 | 60.00201416015625 |
| 15 | 62.001 | 60.00201416015625 |
| 16 | 62.059 | 60.00201416015625 |
| 17 | 62.061 | 60.00201416015625 |
| 18 | 62.053 | 60.00201416015625 |
| 19 | 61.980 | 60.00201416015625 |

### Combat versus cleanup wall intervals

Measured directly from wave_start, wave_timer_end and the next wave_start (or final finished). Positive combat excess is observed wall time beyond active simulation; its cause is not established. Cleanup/choice time is measured separately, so shop outcomes are not used to explain combat-window stalls.

| Wave | Active seconds | Combat wall seconds | Combat wall excess | Cleanup/choices wall seconds |
| --- | ---: | ---: | ---: | ---: |
| 1 | 20.017 | 20.034 | 0.017 | 2.039 |
| 2 | 25.016 | 24.995 | -0.021 | 2.092 |
| 3 | 30.019 | 29.934 | -0.085 | 2.048 |
| 4 | 35.024 | 34.982 | -0.042 | 2.060 |
| 5 | 40.015 | 39.981 | -0.034 | 2.056 |
| 6 | 45.006 | 44.974 | -0.032 | 2.100 |
| 7 | 50.004 | 49.929 | -0.075 | 2.065 |
| 8 | 55.003 | 54.970 | -0.034 | 2.097 |
| 9 | 60.002 | 59.934 | -0.068 | 2.091 |
| 10 | 60.002 | 59.942 | -0.060 | 2.146 |
| 11 | 60.002 | 59.898 | -0.104 | 2.092 |
| 12 | 60.002 | 59.942 | -0.060 | 2.086 |
| 13 | 60.002 | 59.947 | -0.055 | 2.057 |
| 14 | 60.002 | 59.965 | -0.037 | 2.099 |
| 15 | 60.002 | 59.940 | -0.062 | 2.061 |
| 16 | 60.002 | 59.998 | -0.004 | 2.061 |
| 17 | 60.002 | 59.966 | -0.036 | 2.095 |
| 18 | 60.002 | 59.930 | -0.072 | 2.123 |
| 19 | 60.002 | 59.911 | -0.091 | 2.069 |
| 20 | 90.074 | 89.949 | -0.124 | 4.019 |

## First acquisition differences

Different source and Unity random sequences, offers, movement and upgrades prevent a causal conclusion from an aggregate difference. This list identifies follow-up items, not automatically bugs.

| Item | Reference first seen | Scrapshift first seen |
| --- | --- | --- |
| adrenaline | wave 15 (purchase) | not acquired |
| alien_baby | wave 19 (purchase) | not acquired |
| alien_eyes | not acquired | wave 12 (purchase) |
| baby_elephant | not acquired | wave 6 (crate_take) |
| black_belt | wave 17 (purchase) | not acquired |
| blindfold | wave 13 (crate_take) | not acquired |
| boiling_water | not acquired | wave 4 (crate_take) |
| butterfly | not acquired | wave 17 (purchase) |
| cake | wave 6 (purchase) | not acquired |
| clockwork_wasp | not acquired | wave 5 (purchase) |
| coffee | wave 4 (purchase) | not acquired |
| cog | wave 14 (purchase) | not acquired |
| coil | not acquired | wave 14 (purchase) |
| coupon | wave 4 (purchase) | not acquired |
| cute_monkey | wave 13 (purchase) | not acquired |
| defective_steroids | not acquired | wave 9 (purchase) |
| extra_stomach | wave 12 (purchase) | not acquired |
| fresh_meat | not acquired | wave 11 (purchase) |
| fried_rice | not acquired | wave 14 (crate_take) |
| gummy_berserker | wave 7 (purchase) | not acquired |
| head_injury | wave 5 (purchase) | wave 9 (purchase) |
| helmet | not acquired | wave 14 (purchase) |
| landmines | wave 2 (purchase) | not acquired |
| lens | wave 6 (crate_take) | wave 18 (crate_take) |
| little_frog | not acquired | wave 18 (purchase) |
| little_muscley_dude | wave 9 (purchase) | not acquired |
| lost_duck | not acquired | wave 17 (crate_take) |
| metal_plate | wave 13 (crate_take) | wave 10 (purchase) |
| mushroom | wave 9 (purchase) | not acquired |
| mutation | not acquired | wave 3 (purchase) |
| nail | wave 16 (purchase) | not acquired |
| peaceful_bee | not acquired | wave 9 (crate_take) |
| pencil | wave 9 (purchase) | not acquired |
| plant | wave 7 (purchase) | wave 2 (purchase) |
| pocket_factory | not acquired | wave 11 (purchase) |
| propeller_hat | not acquired | wave 6 (purchase) |
| pumpkin | not acquired | wave 7 (crate_take) |
| ratzilla | not acquired | wave 8 (purchase) |
| reinforced_steel | wave 8 (purchase) | not acquired |
| riposte | wave 12 (purchase) | not acquired |
| sad_tomato | not acquired | wave 8 (purchase) |
| scar | wave 19 (purchase) | not acquired |
| scope | not acquired | wave 15 (purchase) |
| shackles | not acquired | wave 18 (purchase) |
| sharp_bullet | wave 3 (purchase) | wave 4 (purchase) |
| shmoop | not acquired | wave 19 (purchase) |
| small_magazine | wave 16 (crate_take) | not acquired |
| terrified_onion | wave 6 (purchase) | wave 13 (purchase) |
| toxic_sludge | wave 8 (purchase) | wave 6 (purchase) |
| turret | not acquired | wave 10 (purchase) |
| ugly_tooth | not acquired | wave 11 (purchase) |
| weird_ghost | wave 18 (purchase) | wave 8 (purchase) |

## Interpretation

Both runs use a neutral scripted six-weapon loadout, Danger 0, invulnerability, mute, scripted movement and fresh profiles at normal time. The supplied-reference trace at `artifacts\parity-remaining-2026-10-20\original-full-headless\trace.json` records 1 fixture pause intervention(s). Its source manifest is an explicit hashed input. The manifest hashes bind the saved evidence, not an independent visual audit.

The unlock-eligibility check applies only to real item IDs acquired by purchase or crate in this fresh run. It checks the imported catalog's run-start availability. It does not sample every shop/crate candidate, establish every unlock trigger, or validate subsequent character-specific runs. Item stack sums do not prove each effect formula. Screenshots and physical controls need separate review. No full-run totals are treated as equality requirements.

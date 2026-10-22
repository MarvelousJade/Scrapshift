# R21 full-run economy comparison - October 17, 2026

Generated from saved traces by `scripts/compare_r14_campaign.py`. The supplied-reference normal-time run at the input path and the Scrapshift run are separate scripted fixtures. The script records their exact file hashes below.

Bounded checks: **18/18 passed**. This result does not establish exact gameplay parity.

## Evidence

JSON: `artifacts\full-verification-2026-10-17\economy-comparison-r21.json`.

| Input | SHA-256 |
| --- | --- |
| `referenceTrace`: `E:\Codex Projects\Job Search Assistant\work\repos\Scrapshift\artifacts\full-verification-2026-10-17\original-attempt-2\trace.json` | `3de61684e5ad50854b6f89831cf420ebaf2df8044f4809dc91bd87526b9d4a71` |
| `referenceSummary`: `E:\Codex Projects\Job Search Assistant\work\repos\Scrapshift\artifacts\full-verification-2026-10-17\original-attempt-2\summary.json` | `7aabd262ef87eb7e3dffd04fb710a4693604f495cce2cb269e8baebb640f236f` |
| `referenceSourceManifest`: `E:\Codex Projects\Job Search Assistant\work\repos\Scrapshift\artifacts\full-verification-2026-10-17\original-attempt-2\source-manifest.json` | `5c3c5243db66f76eee8ddc0f57e37e61f37c692e32877a8e98925e835775cabc` |
| `unityTrace`: `E:\Codex Projects\Job Search Assistant\work\repos\Scrapshift\artifacts\full-verification-2026-10-17\unity-attempt-3\campaign-trace.json` | `605ffcfa5baf9c6b9c32891ba00f6218b53b399a965dbe94349536c3df92adf5` |
| `unitySummary`: `E:\Codex Projects\Job Search Assistant\work\repos\Scrapshift\artifacts\full-verification-2026-10-17\unity-attempt-3\campaign-check.json` | `22e44622bee5532299885b73833e0df6c4b1ecf1e8ce38525d372c862ed351bb` |
| `unityProcess`: `E:\Codex Projects\Job Search Assistant\work\repos\Scrapshift\artifacts\full-verification-2026-10-17\unity-attempt-3\process.json` | `4e8e2550e6ee6b5cdd67dd4454405eacf99d075f8c7effb270e98da68211e4a4` |
| `buildManifest`: `E:\Codex Projects\Job Search Assistant\work\repos\Scrapshift\artifacts\gameplay-parity-r21-2026-10-17\build-manifest.json` | `b22ea38db2524c658392b8c5bb63a0c6ed5e0e5a4b5b8f635e1d9f3ae325be9e` |
| `sourceManifest`: `E:\Codex Projects\Job Search Assistant\work\repos\Scrapshift\artifacts\gameplay-parity-r21-2026-10-17\source-manifest.json` | `7655d330fabd649ef885a7ffb805cf18e4cb4ace1638c5f73b2c1679e428a1dc` |
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
| referenceFixed60 HzDurations | pass |
| unityFixed60 HzDurations | pass |
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

Purchases: reference 43 (43 debit checks); Scrapshift 35 (35 debit checks). Different prices and purchase counts are not required to equal across random runs.
Stacks: reference final 58/58; Scrapshift final 42/42. Scrapshift inconsistent snapshots: 0.
Positive catalog stack-cap violations: reference final 0; Scrapshift snapshots 0. These neutral fixtures have no character-specific cap modifiers.
Initial-catalog locked acquisitions in the fresh Scrapshift run: 0; unknown acquisitions: 0.

## Observed run totals

| Metric | Reference | Scrapshift |
| --- | ---: | ---: |
| materials | 1596 | 1260 |
| bankedMaterials | 714 | 958 |
| level | 21 | 18 |
| itemCount | 58 | 42 |
| enemyKills | 4867 | 4676 |
| treeKills | 42 | 39 |
| materialPickups | 1854 | 1376 |
| harvesting | 78 | 36.0 |
| luck | 68 | 6.0 |

## Wave-end divergence timeline

Snapshots use each fixture's wave-timer callback, not an identical settlement phase: the original fixture connects after Main's existing timer handler, whereas Unity records before Rules.Tick settlement. Gold/stat comparisons at this boundary may therefore include original harvesting already paid. Equal base loadouts do not reproduce cross-engine spawn placement, target order or physics. First-wave kill/drop differences still precede shop purchases.

| Wave | Enemy kills ref / Unity | Pickups ref / Unity | Materials ref / Unity | Items ref / Unity |
| --- | --- | --- | --- | --- |
| 1 | 28 / 27 | 8 / 5 | 8 / 5 | 0 / 0 |
| 2 | 65 / 68 | 16 / 21 | 20 / 32 | 0 / 0 |
| 3 | 125 / 132 | 34 / 40 | 33 / 38 | 1 / 1 |
| 4 | 202 / 211 | 72 / 63 | 83 / 50 | 2 / 2 |
| 5 | 337 / 340 | 135 / 87 | 167 / 59 | 3 / 4 |
| 6 | 504 / 504 | 194 / 106 | 140 / 57 | 6 / 5 |
| 7 | 721 / 716 | 335 / 157 | 261 / 106 | 9 / 7 |
| 8 | 883 / 883 | 414 / 213 | 289 / 145 | 13 / 9 |
| 9 | 1322 / 1328 | 520 / 281 | 261 / 150 | 16 / 11 |
| 10 | 1682 / 1662 | 641 / 375 | 295 / 232 | 21 / 13 |
| 11 | 1916 / 1903 | 740 / 488 | 480 / 281 | 23 / 17 |
| 12 | 2217 / 2202 | 859 / 606 | 458 / 520 | 26 / 18 |
| 13 | 2474 / 2469 | 952 / 710 | 583 / 663 | 30 / 21 |
| 14 | 2707 / 2715 | 1047 / 820 | 652 / 893 | 34 / 23 |
| 15 | 3141 / 3142 | 1138 / 906 | 652 / 933 | 37 / 26 |
| 16 | 3484 / 3445 | 1219 / 1002 | 746 / 1049 | 40 / 28 |
| 17 | 3708 / 3630 | 1445 / 1170 | 879 / 1471 | 44 / 31 |
| 18 | 4048 / 3943 | 1595 / 1282 | 1083 / 1399 | 50 / 35 |
| 19 | 4590 / 4484 | 1733 / 1350 | 1356 / 1263 | 54 / 40 |
| 20 | 4867 / 4676 | 1854 / 1376 | 1492 / 1260 | 58 / 42 |

## Final-wave settlement

Recorded final-wave harvesting at credit: 34.0; minimum base credit 34; observed credit 34, measured by explicit settlement receipt. New traces carry a settlement receipt because timer snapshots now follow credit. Legacy R19 retains its failing timer-to-win measurement. This is a neutral-fixture lower bound, not an exact all-effect payout formula.

## Wall-clock timing limitation

The duration checks above measure active simulated combat time. They do not establish wall-clock or performance parity. Start-to-start wall intervals also include transitions and any elapsed time the driver did not consume. The driver uses `Time.deltaTime`, which is subject to Unity's maximum-delta setting. There is no frame-profiler or complete window-intervention trace to attribute longer intervals. Earlier R19 captures had differing resolution; that historical observation does not establish the cause of longer intervals in this campaign.

| Wave | Wall seconds until next wave start | Active combat seconds |
| --- | ---: | ---: |
| 1 | 22.228 | 20.01653289794922 |
| 2 | 27.047 | 25.016284942626953 |
| 3 | 32.014 | 30.018733978271484 |
| 4 | 37.038 | 35.024147033691406 |
| 5 | 42.027 | 40.015106201171875 |
| 6 | 47.045 | 45.00567626953125 |
| 7 | 52.041 | 50.00445556640625 |
| 8 | 57.018 | 55.00325012207031 |
| 9 | 62.045 | 60.00201416015625 |
| 10 | 62.117 | 60.00201416015625 |
| 11 | 62.028 | 60.00201416015625 |
| 12 | 62.031 | 60.00201416015625 |
| 13 | 64.535 | 60.00201416015625 |
| 14 | 74.252 | 60.00201416015625 |
| 15 | 97.779 | 60.00201416015625 |
| 16 | 78.129 | 60.00201416015625 |
| 17 | 77.457 | 60.00201416015625 |
| 18 | 93.765 | 60.00201416015625 |
| 19 | 102.416 | 60.00201416015625 |

### Combat versus cleanup wall intervals

Measured directly from wave_start, wave_timer_end and the next wave_start (or final finished). Positive combat excess is observed wall time beyond active simulation; its cause is not established. Cleanup/choice time is measured separately, so shop outcomes are not used to explain combat-window stalls.

| Wave | Active seconds | Combat wall seconds | Combat wall excess | Cleanup/choices wall seconds |
| --- | ---: | ---: | ---: | ---: |
| 1 | 20.017 | 20.182 | 0.166 | 2.045 |
| 2 | 25.016 | 24.984 | -0.032 | 2.063 |
| 3 | 30.019 | 29.982 | -0.036 | 2.031 |
| 4 | 35.024 | 35.008 | -0.016 | 2.030 |
| 5 | 40.015 | 39.992 | -0.023 | 2.034 |
| 6 | 45.006 | 45.000 | -0.006 | 2.045 |
| 7 | 50.004 | 49.995 | -0.010 | 2.047 |
| 8 | 55.003 | 54.983 | -0.020 | 2.035 |
| 9 | 60.002 | 59.996 | -0.006 | 2.049 |
| 10 | 60.002 | 60.180 | 0.178 | 1.937 |
| 11 | 60.002 | 59.983 | -0.019 | 2.045 |
| 12 | 60.002 | 60.000 | -0.002 | 2.031 |
| 13 | 60.002 | 62.584 | 2.582 | 1.951 |
| 14 | 60.002 | 72.333 | 12.331 | 1.919 |
| 15 | 60.002 | 95.882 | 35.880 | 1.897 |
| 16 | 60.002 | 76.219 | 16.217 | 1.910 |
| 17 | 60.002 | 75.485 | 15.483 | 1.972 |
| 18 | 60.002 | 91.801 | 31.799 | 1.964 |
| 19 | 60.002 | 100.501 | 40.499 | 1.915 |
| 20 | 90.074 | 105.645 | 15.571 | 3.889 |

### First-wave material accounting

The first wave diverged before either run acquired an item or upgrade. The original killed 28 enemies and no trees: 8 material pickups + 20 remaining ground value = 28. Scrapshift killed 27 enemies and one tree: 5 pickups + 25 ground value = 30. The supplied `entities/units/neutral/tree_stats.tres` sets tree value to 3; `RunRules.MaterialDropAmount` preserves base value with zero drop modifiers. Thus 27 + 3 = 30 reconciles this observed difference. This rules out an obvious missing-material accounting error in these snapshots; it does not establish matching spawn placement, targeting or pickup trajectories.

## First acquisition differences

Different source and Unity random sequences, offers, movement and upgrades prevent a causal conclusion from an aggregate difference. This list identifies follow-up items, not automatically bugs.

| Item | Reference first seen | Scrapshift first seen |
| --- | --- | --- |
| baby_elephant | wave 4 (purchase) | not acquired |
| baby_gecko | wave 3 (purchase) | not acquired |
| bag | wave 8 (crate_take) | not acquired |
| bait | wave 13 (purchase) | not acquired |
| bat | wave 6 (purchase) | not acquired |
| black_belt | wave 12 (crate_take) | wave 14 (crate_take) |
| blindfold | not acquired | wave 18 (crate_take) |
| blood_leech | wave 16 (purchase) | not acquired |
| boxing_glove | wave 9 (crate_take) | wave 17 (purchase) |
| broken_mouth | wave 12 (purchase) | not acquired |
| clover | not acquired | wave 14 (purchase) |
| cute_monkey | not acquired | wave 10 (crate_take) |
| dangerous_bunny | not acquired | wave 19 (purchase) |
| duct_tape | wave 7 (purchase) | wave 2 (purchase) |
| dynamite | wave 14 (crate_take) | not acquired |
| energy_bracelet | not acquired | wave 19 (purchase) |
| extra_stomach | wave 17 (crate_take) | not acquired |
| fertilizer | wave 12 (crate_take) | wave 16 (purchase) |
| frozen_heart | wave 8 (purchase) | not acquired |
| garden | not acquired | wave 6 (crate_take) |
| glasses | not acquired | wave 7 (crate_take) |
| goat_skull | wave 7 (crate_take) | wave 5 (purchase) |
| gummy_berserker | not acquired | wave 18 (purchase) |
| head_injury | not acquired | wave 11 (purchase) |
| helmet | wave 5 (purchase) | not acquired |
| ice_cube | wave 15 (purchase) | wave 4 (crate_take) |
| lemonade | wave 7 (purchase) | not acquired |
| little_frog | wave 19 (purchase) | not acquired |
| little_muscley_dude | wave 9 (purchase) | not acquired |
| lost_duck | not acquired | wave 13 (purchase) |
| mastery | not acquired | wave 18 (purchase) |
| medal | not acquired | wave 18 (purchase) |
| metal_detector | wave 11 (purchase) | not acquired |
| metal_plate | not acquired | wave 17 (purchase) |
| missile | wave 13 (crate_take) | not acquired |
| mushroom | not acquired | wave 3 (purchase) |
| mutation | wave 17 (purchase) | not acquired |
| peaceful_bee | wave 6 (purchase) | wave 10 (purchase) |
| piggy_bank | wave 10 (purchase) | wave 9 (purchase) |
| pile_of_books | wave 11 (purchase) | not acquired |
| plant | wave 13 (purchase) | wave 12 (purchase) |
| plastic_explosive | not acquired | wave 17 (purchase) |
| pocket_factory | wave 5 (purchase) | not acquired |
| propeller_hat | wave 15 (purchase) | not acquired |
| pumpkin | wave 12 (purchase) | wave 7 (purchase) |
| recycling_machine | wave 8 (purchase) | not acquired |
| reinforced_steel | wave 15 (crate_take) | wave 17 (purchase) |
| riposte | not acquired | wave 14 (purchase) |
| scope | wave 10 (crate_take) | not acquired |
| shady_potion | wave 9 (purchase) | not acquired |
| sharp_bullet | wave 17 (purchase) | wave 12 (purchase) |
| small_magazine | wave 14 (purchase) | not acquired |
| snake | wave 19 (crate_take) | not acquired |
| statue | wave 14 (purchase) | not acquired |
| sunglasses | not acquired | wave 10 (purchase) |
| tardigrade | wave 13 (purchase) | not acquired |
| terrified_onion | wave 2 (purchase) | wave 8 (purchase) |
| tree | not acquired | wave 9 (purchase) |
| triangle_of_power | wave 9 (crate_take) | not acquired |
| turret | not acquired | wave 18 (purchase) |
| turret_flame | wave 17 (purchase) | not acquired |
| ugly_tooth | wave 9 (purchase) | wave 16 (purchase) |
| wandering_bot | not acquired | wave 15 (purchase) |
| weird_food | wave 7 (purchase) | not acquired |
| weird_ghost | wave 5 (crate_take) | not acquired |

## Interpretation

Both runs use a neutral scripted six-weapon loadout, Danger 0, invulnerability, mute, scripted movement and fresh profiles at normal time. The supplied-reference trace at `artifacts\full-verification-2026-10-17\original-attempt-2\trace.json` records 1 fixture pause intervention(s). Its source manifest is an explicit hashed input. The manifest hashes bind the saved evidence, not an independent visual audit.

The unlock-eligibility check applies only to real item IDs acquired by purchase or crate in this fresh run. It checks the imported catalog's run-start availability. It does not sample every shop/crate candidate, establish every unlock trigger, or validate subsequent character-specific runs. Item stack sums do not prove each effect formula. Screenshots and physical controls need separate review. No full-run totals are treated as equality requirements.

## Validation history and current correction evidence

R19 completed its20-wave campaign, but the expanded comparator correctly remains 17/18 because final positive harvesting was omitted. That JSON is retained at `artifacts/full-verification-2026-10-17/economy-comparison.json`; previous report/notes are preserved in `economy-comparison-r19-history.md`. R20's focused native checks verified the final payout, followed by source review establishing the pre-cleanup payout and delayed-growth ordering.

R21 explicit Begin/Advance/Complete settlement now credits before cleanup, resets temporary stats, recomputes growth at 0.66 seconds, and waits source 2/4-second cleanup timers. Scene-free Tick is a documented atomic adapter; actual Update advances the stages over time. Complete suite attempt 7 passed **1018/1018**, zero failures/skips/inconclusive,55.3793378 seconds, including all nine lifecycle cases. Strengthened native timer/boss-route checks passed, exit 0: payout age 0, growth age 0.6666667, final cleanup4.01666 simulated seconds,100 to115 materials,15 to16 harvesting,0 to15 XP, no duplicate credit or final choices. Both captures were visually reviewed by the coordinating agent.

The current full R21 campaign separately completed Passed/Won,20 waves/19 shops, exit 0. Actual final receipt: wave 20, materials 1226 to1260, harvesting at credit 34; final harvesting 36. This report's **18/18** result belongs to that new complete R21 campaign. Native boss-route tests call the completion API and do not reenact boss death itself.

## Unresolved combat-window slowdown

Across R21, active combat1050.200 seconds consumed1220.737 wall seconds between wave starts and timer ends: **170.537 seconds excess**. Separate cleanup/choice intervals totaled41.811 seconds. The player summary wall duration was 1263.670 seconds. Largest excesses were wave 19:+40.499 seconds; wave 15:+35.880; wave 18:+31.799; wave 16:+16.217. These are observed within combat windows and cannot be assigned to random purchases. The trace does not contain frame profiling that establishes the cause. The 18 bounded checks do not cover wall-clock/performance parity. Any follow-up development profile is separate evidence.

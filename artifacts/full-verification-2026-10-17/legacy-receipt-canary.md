# R19-legacy-canary full-run economy comparison â€” October 17, 2026

Generated from saved traces by `scripts/compare_r14_campaign.py`. The supplied-reference normal-time run at the input path and the Scrapshift run are separate scripted fixtures. The script records their exact file hashes below.

Bounded checks: **17/18 passed**. This result does not establish exact gameplay parity.

## Evidence

JSON: `artifacts\full-verification-2026-10-17\legacy-receipt-canary.json`.

| Input | SHA-256 |
| --- | --- |
| `referenceTrace`: `E:\Codex Projects\Job Search Assistant\work\repos\Scrapshift\artifacts\full-verification-2026-10-17\original-attempt-2\trace.json` | `3de61684e5ad50854b6f89831cf420ebaf2df8044f4809dc91bd87526b9d4a71` |
| `referenceSummary`: `E:\Codex Projects\Job Search Assistant\work\repos\Scrapshift\artifacts\full-verification-2026-10-17\original-attempt-2\summary.json` | `7aabd262ef87eb7e3dffd04fb710a4693604f495cce2cb269e8baebb640f236f` |
| `referenceSourceManifest`: `E:\Codex Projects\Job Search Assistant\work\repos\Scrapshift\artifacts\full-verification-2026-10-17\original-attempt-2\source-manifest.json` | `5c3c5243db66f76eee8ddc0f57e37e61f37c692e32877a8e98925e835775cabc` |
| `unityTrace`: `E:\Codex Projects\Job Search Assistant\work\repos\Scrapshift\artifacts\full-verification-2026-10-17\unity-attempt-2\campaign-trace.json` | `332ce5690de74d20733fd71fed284e1f55c617b63c2c0d174b3952e172723b7f` |
| `unitySummary`: `E:\Codex Projects\Job Search Assistant\work\repos\Scrapshift\artifacts\full-verification-2026-10-17\unity-attempt-2\campaign-check.json` | `29d1599746a21280bbf2d1bf785e4898b4f22d06f8b4ca85de300ca693a9a6e2` |
| `unityProcess`: `E:\Codex Projects\Job Search Assistant\work\repos\Scrapshift\artifacts\full-verification-2026-10-17\unity-attempt-2\process.json` | `b2b423409c999d8dfc3f5c2f854cf733120165320caebaaf50224f6e808dfdb7` |
| `buildManifest`: `E:\Codex Projects\Job Search Assistant\work\repos\Scrapshift\artifacts\gameplay-parity-r19-2026-10-17\build-manifest.json` | `136d9b19fd6f9f16723f94e4efb73814e4fc366df947ff57a4a5afc839d1d472` |
| `sourceManifest`: `E:\Codex Projects\Job Search Assistant\work\repos\Scrapshift\artifacts\gameplay-parity-r19-2026-10-17\source-manifest.json` | `8be3c32d6f67136bb7329d9774f37b0f1d5b01c2250fcae309987a764e9bd20a` |
| `itemCatalog`: `E:\Codex Projects\Job Search Assistant\work\repos\Scrapshift\Assets\Scrapshift\Content\Reference\item-design.json` | `aa1bd9687c8c012ba8df064a9ae76c5da243834cadf18abf0607b9e2478fa311` |

## Bounded checks

| Check | Result |
| --- | --- |
| referenceWonAndExitedZero | pass |
| unityFinalPositiveHarvestCredited | FAIL |
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
| materials | 1596 | 2057 |
| bankedMaterials | 714 | 2 |
| level | 21 | 23 |
| itemCount | 58 | 45 |
| enemyKills | 4867 | 5074 |
| treeKills | 42 | 39 |
| materialPickups | 1854 | 2502 |
| harvesting | 78 | 15.0 |
| luck | 68 | 55.0 |

## Wave-end divergence timeline

Snapshots use each fixture's wave-timer callback, not an identical settlement phase: the original fixture connects after Main's existing timer handler, whereas Unity records before Rules.Tick settlement. Gold/stat comparisons at this boundary may therefore include original harvesting already paid. Equal base loadouts do not reproduce cross-engine spawn placement, target order or physics. First-wave kill/drop differences still precede shop purchases.

| Wave | Enemy kills ref / Unity | Pickups ref / Unity | Materials ref / Unity | Items ref / Unity |
| --- | --- | --- | --- | --- |
| 1 | 28 / 27 | 8 / 5 | 8 / 5 | 0 / 0 |
| 2 | 65 / 68 | 16 / 21 | 20 / 32 | 0 / 0 |
| 3 | 125 / 133 | 34 / 40 | 33 / 30 | 1 / 1 |
| 4 | 202 / 210 | 72 / 57 | 83 / 42 | 2 / 2 |
| 5 | 337 / 343 | 135 / 126 | 167 / 152 | 3 / 3 |
| 6 | 504 / 490 | 194 / 176 | 140 / 221 | 6 / 4 |
| 7 | 721 / 695 | 335 / 254 | 261 / 329 | 9 / 5 |
| 8 | 883 / 845 | 414 / 319 | 289 / 346 | 13 / 8 |
| 9 | 1322 / 1256 | 520 / 408 | 261 / 276 | 16 / 10 |
| 10 | 1682 / 1597 | 641 / 579 | 295 / 372 | 21 / 13 |
| 11 | 1916 / 1843 | 740 / 737 | 480 / 571 | 23 / 18 |
| 12 | 2217 / 2171 | 859 / 903 | 458 / 578 | 26 / 21 |
| 13 | 2474 / 2488 | 952 / 1065 | 583 / 748 | 30 / 23 |
| 14 | 2707 / 2785 | 1047 / 1229 | 652 / 859 | 34 / 25 |
| 15 | 3141 / 3278 | 1138 / 1400 | 652 / 1080 | 37 / 27 |
| 16 | 3484 / 3621 | 1219 / 1658 | 746 / 1492 | 40 / 32 |
| 17 | 3708 / 3860 | 1445 / 1883 | 879 / 1769 | 44 / 35 |
| 18 | 4048 / 4207 | 1595 / 2079 | 1083 / 1905 | 50 / 39 |
| 19 | 4590 / 4777 | 1733 / 2245 | 1356 / 2095 | 54 / 41 |
| 20 | 4867 / 5074 | 1854 / 2502 | 1492 / 2057 | 58 / 45 |

## Final-wave settlement

Recorded final-wave harvesting at credit: 15.0; minimum base credit 15; observed credit 0, measured by legacy timer-to-win delta. New traces carry a settlement receipt because timer snapshots now follow credit. Legacy R19 retains its failing timer-to-win measurement. This is a neutral-fixture lower bound, not an exact all-effect payout formula.

## Wall-clock timing limitation

The duration checks above measure active simulated combat time. They do not establish wall-clock or performance parity. Start-to-start wall intervals also include transitions and any elapsed time the driver did not consume. The driver uses `Time.deltaTime`, which is subject to Unity's maximum-delta setting. There is no frame-profiler or complete window-intervention trace to attribute longer intervals. Native capture resolution changed during this campaign; that observation does not establish the timing cause.

| Wave | Wall seconds until next wave start | Active combat seconds |
| --- | ---: | ---: |
| 1 | 22.273 | 20.01653289794922 |
| 2 | 26.861 | 25.016284942626953 |
| 3 | 31.810 | 30.018733978271484 |
| 4 | 36.820 | 35.024147033691406 |
| 5 | 41.839 | 40.015106201171875 |
| 6 | 46.869 | 45.00567626953125 |
| 7 | 51.814 | 50.00445556640625 |
| 8 | 56.845 | 55.00325012207031 |
| 9 | 61.824 | 60.00201416015625 |
| 10 | 61.841 | 60.00201416015625 |
| 11 | 61.834 | 60.00201416015625 |
| 12 | 61.825 | 60.00201416015625 |
| 13 | 61.833 | 60.00201416015625 |
| 14 | 61.830 | 60.00201416015625 |
| 15 | 61.886 | 60.00201416015625 |
| 16 | 61.835 | 60.00201416015625 |
| 17 | 65.638 | 60.00201416015625 |
| 18 | 72.412 | 60.00201416015625 |
| 19 | 91.152 | 60.00201416015625 |

### First-wave material accounting

The first wave diverged before either run acquired an item or upgrade. The original killed 28 enemies and no trees: 8 material pickups + 20 remaining ground value = 28. Scrapshift killed 27 enemies and one tree: 5 pickups + 25 ground value = 30. The supplied `entities/units/neutral/tree_stats.tres` sets tree value to 3; `RunRules.MaterialDropAmount` preserves base value with zero drop modifiers. Thus 27 + 3 = 30 reconciles this observed difference. This rules out an obvious missing-material accounting error in these snapshots; it does not establish matching spawn placement, targeting or pickup trajectories.

## First acquisition differences

Different source and Unity random sequences, offers, movement and upgrades prevent a causal conclusion from an aggregate difference. This list identifies follow-up items, not automatically bugs.

| Item | Reference first seen | Scrapshift first seen |
| --- | --- | --- |
| alien_baby | not acquired | wave 13 (purchase) |
| alien_eyes | not acquired | wave 9 (purchase) |
| baby_elephant | wave 4 (purchase) | wave 15 (purchase) |
| baby_gecko | wave 3 (purchase) | wave 4 (purchase) |
| bag | wave 8 (crate_take) | not acquired |
| bait | wave 13 (purchase) | not acquired |
| bandana | not acquired | wave 17 (purchase) |
| bat | wave 6 (purchase) | wave 9 (purchase) |
| bean_teacher | not acquired | wave 10 (crate_take) |
| beanie | not acquired | wave 7 (purchase) |
| black_belt | wave 12 (crate_take) | not acquired |
| blood_leech | wave 16 (purchase) | wave 15 (purchase) |
| boxing_glove | wave 9 (crate_take) | not acquired |
| broken_mouth | wave 12 (purchase) | wave 7 (crate_take) |
| clover | not acquired | wave 13 (crate_take) |
| coil | not acquired | wave 19 (purchase) |
| coupon | not acquired | wave 17 (purchase) |
| cute_monkey | not acquired | wave 2 (purchase) |
| cyberball | not acquired | wave 18 (purchase) |
| duct_tape | wave 7 (purchase) | wave 10 (purchase) |
| dynamite | wave 14 (crate_take) | wave 5 (purchase) |
| extra_stomach | wave 17 (crate_take) | not acquired |
| fertilizer | wave 12 (crate_take) | not acquired |
| frozen_heart | wave 8 (purchase) | wave 17 (crate_take) |
| gambling_token | not acquired | wave 12 (purchase) |
| gentle_alien | wave 6 (purchase) | not acquired |
| goat_skull | wave 7 (crate_take) | not acquired |
| helmet | wave 5 (purchase) | not acquired |
| ice_cube | wave 15 (purchase) | not acquired |
| lemonade | wave 7 (purchase) | not acquired |
| little_muscley_dude | wave 9 (purchase) | not acquired |
| mastery | not acquired | wave 16 (purchase) |
| metal_detector | wave 11 (purchase) | not acquired |
| missile | wave 13 (crate_take) | not acquired |
| mouse | not acquired | wave 11 (crate_take) |
| mutation | wave 17 (purchase) | wave 15 (crate_take) |
| peaceful_bee | wave 6 (purchase) | wave 16 (crate_take) |
| piggy_bank | wave 10 (purchase) | wave 12 (purchase) |
| pile_of_books | wave 11 (purchase) | not acquired |
| plant | wave 13 (purchase) | not acquired |
| pocket_factory | wave 5 (purchase) | not acquired |
| power_generator | not acquired | wave 11 (purchase) |
| propeller_hat | wave 15 (purchase) | wave 6 (purchase) |
| pumpkin | wave 12 (purchase) | wave 14 (purchase) |
| ratzilla | not acquired | wave 10 (purchase) |
| recycling_machine | wave 8 (purchase) | wave 17 (purchase) |
| reinforced_steel | wave 15 (crate_take) | not acquired |
| riposte | not acquired | wave 7 (purchase) |
| scope | wave 10 (crate_take) | not acquired |
| shady_potion | wave 9 (purchase) | wave 10 (crate_take) |
| sharp_bullet | wave 17 (purchase) | not acquired |
| sifds_relic | not acquired | wave 19 (purchase) |
| small_magazine | wave 14 (purchase) | not acquired |
| snake | wave 19 (crate_take) | not acquired |
| statue | wave 14 (purchase) | not acquired |
| sunglasses | not acquired | wave 9 (purchase) |
| tardigrade | wave 13 (purchase) | wave 15 (crate_take) |
| terrified_onion | wave 2 (purchase) | wave 3 (purchase) |
| triangle_of_power | wave 9 (crate_take) | not acquired |
| turret_flame | wave 17 (purchase) | wave 19 (purchase) |
| ugly_tooth | wave 9 (purchase) | not acquired |
| warrior_helmet | not acquired | wave 11 (purchase) |
| weird_food | wave 7 (purchase) | not acquired |
| weird_ghost | wave 5 (crate_take) | not acquired |
| wings | not acquired | wave 18 (purchase) |
| wisdom | not acquired | wave 8 (purchase) |

## Interpretation

Both runs use a neutral scripted six-weapon loadout, Danger 0, invulnerability, mute, scripted movement and fresh profiles at normal time. The supplied-reference trace at `artifacts\full-verification-2026-10-17\original-attempt-2\trace.json` records 1 fixture pause intervention(s). Its source manifest is an explicit hashed input. The manifest hashes bind the saved evidence, not an independent visual audit.

The unlock-eligibility check applies only to real item IDs acquired by purchase or crate in this fresh run. It checks the imported catalog's run-start availability. It does not sample every shop/crate candidate, establish every unlock trigger, or validate subsequent character-specific runs. Item stack sums do not prove each effect formula. Screenshots and physical controls need separate review. No full-run totals are treated as equality requirements.

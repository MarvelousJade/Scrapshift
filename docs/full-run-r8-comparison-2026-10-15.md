# R8 full-run comparison — October 15, 2026

Generated from saved traces by `scripts/compare_r8_campaign.py`. Original reference evidence is reused unchanged; this is a fresh Scrapshift campaign against that frozen original run.

Saved integration checks: **passed** (14/14). A passed integration comparison does not establish exact gameplay parity.

Both reached Won after 20 normal-time wave timers and 19 shops. The tested R8 runtime SHA-256 was independently checked against the delivered DLL: `D0A63A27C53081D9D82DE2F04A90FEB8569389B8EC5AF7AAEFD7DAC5C91A15F8`. Its frozen manifest records the preceding **620/620 EditMode suite**, zero failures/skips, 74.3181245 seconds, and a build with zero errors/warnings. Those suite results and this scripted campaign are separate evidence.

## Evidence and build

Machine-readable result: `artifacts\gameplay-parity-geometry-2026-10-15\realtime\comparison.json`.

| Input | SHA-256 |
|---|---|
| `referenceTrace`: `E:\Codex Projects\Job Search Assistant\work\repos\Scrapshift\artifacts\gameplay-parity-reference-2026-10-15\realtime\trace.json` | `98ac455148dba90b48f89771ea5fe720a8487f95d70f2bc2162fd6799d41a535` |
| `referenceSummary`: `E:\Codex Projects\Job Search Assistant\work\repos\Scrapshift\artifacts\gameplay-parity-reference-2026-10-15\realtime\summary.json` | `d5f4fa85c3543f08b739990f1086e866b0072ce7f75abd4dacb1cacbbd672e1e` |
| `unityTrace`: `E:\Codex Projects\Job Search Assistant\work\repos\Scrapshift\artifacts\gameplay-parity-geometry-2026-10-15\realtime\campaign-trace.json` | `9d0da6893b7d8aef198e988e6b7ceeb25afdfdb9cdf6ca37f71227801dae8df8` |
| `unitySummary`: `E:\Codex Projects\Job Search Assistant\work\repos\Scrapshift\artifacts\gameplay-parity-geometry-2026-10-15\realtime\campaign-check.json` | `21993f6b89a77f30262fd7493edbff5b0aeeaab91499b993c351ad8761258dd2` |
| `buildManifest`: `E:\Codex Projects\Job Search Assistant\work\repos\Scrapshift\artifacts\gameplay-parity-geometry-2026-10-15\build-manifest.json` | `b246db29a71b3d6dbff1df84217504ba32e60b36da9d79aef1c4b31720b49bb3` |

Frozen build manifest:

```json
{
  "branch": "codex/gameplay-parity",
  "buildPath": "E:\\Codex Projects\\Job Search Assistant\\work\\repos\\Scrapshift\\Builds\\GameplayParity20261015R8",
  "result": "Succeeded",
  "errors": 0,
  "warnings": 0,
  "bytes": 401561639,
  "tests": 620,
  "passed": 620,
  "failed": 0,
  "skipped": 0,
  "testDuration": 74.3181245,
  "files": [
    {
      "sha256": "001AEEBFB92718F8B49F0C0E0A03E313C54669552B08E952B2FEE3DDD143683A",
      "path": "Scrapshift.exe",
      "bytes": 667136
    },
    {
      "sha256": "D0A63A27C53081D9D82DE2F04A90FEB8569389B8EC5AF7AAEFD7DAC5C91A15F8",
      "path": "Scrapshift.Runtime.dll",
      "bytes": 521728
    }
  ],
  "assetPayloads": [
    {
      "sha256": "EE8D160B41A3351AD0BADDA99318F2C2D1AC24883D901985BD87167AD16EFA53",
      "path": "globalgamemanagers.assets",
      "bytes": 70400
    },
    {
      "sha256": "75877125745A6DF997ABB67C5BD1B2639A1ACFC2FF935134F998B223DA9C7668",
      "path": "resources.assets",
      "bytes": 70641224
    },
    {
      "sha256": "68A096A2BCB7110E99850689EF7E2799B2D32DDD2876B1FD89AB12F170D06F9F",
      "path": "resources.resource",
      "bytes": 29917920
    },
    {
      "sha256": "661A340DC721EFC05312D81F5D0969E9CB7CBFE0421861DEA3BE543EF6894414",
      "path": "sharedassets0.assets",
      "bytes": 139741896
    },
    {
      "sha256": "724D24BC540B82D3A0B69CC063E429C57FB918FDB0BC3F8D65A41C9971A42565",
      "path": "sharedassets0.resource",
      "bytes": 6407264
    }
  ]
}
```

## Observed completion and progression

| Observation | Original | Scrapshift |
|---|---:|---:|
| Wave timer ends | 20 | 20 |
| Shops | 19 | 19 |
| Purchases | 30 | 31 |
| Upgrades | 17 | 18 |
| Crates taken | 8 | 6 |
| materials | 385 | 344 |
| bankedMaterials | 1832 | 2315 |
| level | 17 | 19 |
| itemCount | 34 | 36 |
| enemyKills | 4968 | 4478 |
| treeKills | 56 | 31 |
| materialPickups | 1218 | 1191 |
| harvesting | 77 | 57.0 |
| luck | 8 | 20.0 |

Purchase debit consistency: **30/30 original**, **31/31 Scrapshift**. Original purchase snapshots precede the callback; the next purchase/shop-exit snapshot supplies the after balance. Unity records explicit before/after values. These checks validate each run's own debits, not equality of differently modified prices.

Final stack sums: original 34/34, Scrapshift 36/36. Unity snapshot stack inconsistencies: 0. Recorded multiplicities do not establish every stacked effect formula.

## Aligned timer-end snapshots

Each cell is original / Scrapshift. Sampling occurs around timer end, before the complete intermission; callback boundaries differ. Pickups count collected objects, not currency value.

| Wave | Enemy kills | Pickup objects | Materials | Bank | Harvesting | Luck |
|---|---|---|---|---|---|---|
| 1 | 25 / 27 | 10 / 4 | 10 / 4 | 0 / 0 | 0 / 0.0 | 0 / 0.0 |
| 2 | 66 / 71 | 27 / 17 | 35 / 24 | 0 / 0 | 0 / 0.0 | 0 / 0.0 |
| 3 | 124 / 130 | 44 / 49 | 36 / 54 | 0 / 0 | 0 / 0.0 | 0 / 0.0 |
| 4 | 207 / 207 | 65 / 80 | 41 / 79 | 0 / 0 | 0 / 5.0 | 0 / 0.0 |
| 5 | 339 / 333 | 99 / 122 | 109 / 96 | 32 / 0 | 0 / 6.0 | 0 / 0.0 |
| 6 | 498 / 498 | 142 / 140 | 99 / 61 | 92 / 67 | 6 / 15.0 | 0 / 0.0 |
| 7 | 704 / 703 | 204 / 170 | 127 / 67 | 134 / 187 | 15 / 16.0 | 0 / 0.0 |
| 8 | 858 / 842 | 259 / 212 | 119 / 143 | 205 / 328 | 16 / 17.0 | 0 / -5.0 |
| 9 | 1275 / 1256 | 323 / 240 | 233 / 208 | 261 / 438 | 17 / 18.0 | 0 / -5.0 |
| 10 | 1605 / 1586 | 397 / 317 | 235 / 348 | 497 / 754 | 22 / 19.0 | 0 / -5.0 |
| 11 | 1839 / 1827 | 459 / 376 | 235 / 267 | 657 / 979 | 24 / 20.0 | 0 / -5.0 |
| 12 | 2126 / 2115 | 515 / 423 | 228 / 408 | 773 / 1140 | 26 / 21.0 | 0 / 15.0 |
| 13 | 2410 / 2364 | 583 / 494 | 220 / 362 | 925 / 1349 | 28 / 23.0 | 0 / 10.0 |
| 14 | 2692 / 2608 | 640 / 537 | 213 / 143 | 1047 / 1479 | 32 / 25.0 | 10 / 10.0 |
| 15 | 3151 / 3074 | 712 / 554 | 219 / 119 | 1150 / 1669 | 37 / 27.0 | 10 / 10.0 |
| 16 | 3491 / 3377 | 747 / 646 | 108 / 358 | 1409 / 2032 | 42 / 29.0 | 8 / 10.0 |
| 17 | 3720 / 3577 | 890 / 854 | 313 / 633 | 1514 / 2029 | 48 / 33.0 | 8 / 10.0 |
| 18 | 4100 / 3849 | 1009 / 974 | 438 / 586 | 1466 / 1989 | 53 / 51.0 | 8 / 10.0 |
| 19 | 4683 / 4242 | 1157 / 1070 | 575 / 499 | 1427 / 2045 | 60 / 54.0 | 8 / 20.0 |
| 20 | 4968 / 4478 | 1218 / 1191 | 317 / 299 | 1604 / 2160 | 68 / 57.0 | 8 / 20.0 |

## Acquisition differences to investigate

First-seen acquisition waves identify recorded inventory differences. They do not quantify an item's causal contribution. Original acquisition callbacks record IDs before applying them; Unity stacks record resulting ownership.

| Item | Original first seen | Scrapshift first seen |
|---|---|---|
| acid | wave 11 (purchase) | not acquired |
| bag | not acquired | wave 4 (purchase) |
| bait | wave 13 (purchase) | not acquired |
| banner | wave 7 (purchase) | not acquired |
| beanie | not acquired | wave 3 (crate_take) |
| blindfold | not acquired | wave 8 (crate_take) |
| blood_leech | not acquired | wave 16 (purchase) |
| boiling_water | not acquired | wave 13 (purchase) |
| boxing_glove | not acquired | wave 2 (purchase) |
| charcoal | wave 5 (purchase) | not acquired |
| claw_tree | not acquired | wave 13 (purchase) |
| clover | not acquired | wave 11 (crate_take) |
| crown | wave 12 (crate_take) | not acquired |
| cute_monkey | wave 19 (purchase) | not acquired |
| cyberball | wave 13 (purchase) | not acquired |
| cyclops_worm | wave 6 (purchase) | not acquired |
| dynamite | wave 16 (crate_take) | wave 13 (purchase) |
| fresh_meat | wave 1 (crate_take) | wave 8 (purchase) |
| gentle_alien | wave 17 (purchase) | wave 14 (purchase) |
| glass_cannon | wave 12 (purchase) | not acquired |
| hedgehog | wave 9 (purchase) | wave 4 (purchase) |
| helmet | wave 5 (crate_take) | not acquired |
| honey | wave 14 (purchase) | not acquired |
| ice_cube | not acquired | wave 19 (purchase) |
| injection | wave 8 (crate_take) | not acquired |
| landmines | wave 18 (purchase) | wave 8 (crate_take) |
| leather_vest | not acquired | wave 10 (purchase) |
| lemonade | wave 8 (purchase) | not acquired |
| lens | not acquired | wave 3 (purchase) |
| little_muscley_dude | wave 18 (purchase) | not acquired |
| mushroom | wave 15 (purchase) | not acquired |
| mutation | wave 15 (purchase) | not acquired |
| peaceful_bee | wave 9 (purchase) | not acquired |
| peacock | not acquired | wave 18 (purchase) |
| pencil | wave 18 (purchase) | not acquired |
| piggy_bank | wave 19 (purchase) | not acquired |
| poisonous_tonic | wave 19 (purchase) | not acquired |
| propeller_hat | not acquired | wave 18 (purchase) |
| recycling_machine | not acquired | wave 19 (purchase) |
| sad_tomato | not acquired | wave 18 (purchase) |
| scar | not acquired | wave 11 (purchase) |
| scared_sausage | wave 3 (purchase) | not acquired |
| scope | not acquired | wave 5 (purchase) |
| snowball | not acquired | wave 13 (purchase) |
| strange_book | not acquired | wave 17 (purchase) |
| sunglasses | wave 16 (purchase) | not acquired |
| terrified_onion | not acquired | wave 7 (purchase) |
| toolbox | wave 15 (crate_take) | not acquired |
| toxic_sludge | wave 2 (purchase) | not acquired |
| turret_flame | not acquired | wave 6 (purchase) |
| weird_food | wave 7 (purchase) | not acquired |
| weird_ghost | wave 6 (purchase) | not acquired |
| wheelbarrow | not acquired | wave 17 (purchase) |
| white_flag | not acquired | wave 16 (purchase) |
| will_o_the_wisp | not acquired | wave 19 (purchase) |

## Scope and remaining interpretation

### Concrete interpretation of this run

- R8 ends with **344 spendable + 2,315 banked = 2,659 materials**, versus **385 + 1,832 = 2,217** in the original. Banked value is reserved future pickup value, not immediately spendable money. Purchase debits total 2,866 in R8 versus 2,704 original. Final spendable money plus purchase debits is 3,210 / 3,089; it is net currency after other gains/losses, not a material-drop total.
- Collected objects are **1,191 R8 versus 1,218 original**, a difference of 27 (about 2.2%). Both traces record a 50-object ground maximum. R8 has more value banked, and pickup counts do not measure pile value, doubling, harvesting or other income. This pair therefore does not establish equal collection physics or identical drop-value throughput.
- Neither run acquires Lootworm. The previous pre-controller Unity campaign did acquire it in shop 10; that older report's Lootworm explanation must **not** be carried over as an explanation for R8. Here, distinct early pickups already occur before any companion: wave 1 records 4 R8 / 10 original pickups despite 27 / 25 enemy kills. Their path/contact history differs, so this remains a collection/path observation without a uniquely identified formula defect.
- R8 reaches level **19 versus 17** despite collecting fewer material objects. Its acquisition trace records Scar in shop 11 (+20% XP), and Peacock in shop 18 (+25% XP permanently, +100% XP next wave). The original acquires neither. Those source effects are confirmed in `items/all/scar/scar_effect_1.tres` and `items/all/peacock/peacock_effect_1.tres` / `_2.tres`. This supports XP modifiers as a concrete contributor; the trace lacks per-source XP accounting to quantify their total contribution.
- Original Harvesting ends at **77 versus R8 57**. The original takes Crown in wave 12; R8 never acquires it. Its supplied `crown_effect_1.tres` adds eight percentage points of Harvesting growth. R8 instead acquires White Flag in shop 16 (+5 Harvesting) and Wheelbarrow in shop 17 (+16). Upgrade histories also differ. These acquisitions explain relevant mechanisms and timing without claiming the full 20-point gap has been decomposed.
- Enemy kills end at **4,478 R8 versus 4,968 original**. Early cumulative kills are close (wave 6: 498 / 498; wave 12: 2,115 / 2,126), then separate more. R8 acquires White Flag in shop 16 (-5% enemy count), while the original never does; Gentle Alien (+5% enemy count) arrives in R8 shop 14 and original shop 17. Final weapon tiers also differ substantially: R8 has five tier-II weapons and one tier-III SMG; the original has a tier-IV Spear and tier-III Laser/Pistol alongside three tier-II weapons. Spawn-count effects, upgrade history and damage capacity are measurable confounders. Kill totals alone cannot identify a spawn-schedule defect or attribute the entire gap to randomness.
- R8 recorded two boss phase changes and 1,050.145 active simulated seconds. Its report wall time is 1,092.050 seconds including fixture processing; this is not a performance benchmark or the same timing boundary as the source fixture's simulated clock.

The follow-up hypotheses above are grounded in acquired IDs, source effects and aligned telemetry. No new deterministic gameplay mismatch was established solely by this pair. A shared deterministic offer/choice replay with per-source income/XP and pickup ownership counters would be needed to isolate the remaining aggregate differences quantitatively.

Both fixtures use neutral character effects, Danger 0, six supplied tier-II weapons, invulnerability, scripted circular movement, first offered upgrades, crate takes and affordable actual shop callbacks. They are muted, run at normal timing and use isolated profiles. They are not ordinary human survival runs.

The original run is frozen evidence, not a newly rerun original. Cross-engine random draw ordering, offers, upgrades, trajectories and resulting gear differ. Outcome differences are observations to investigate; this report neither attributes every gap to randomness nor declares a particular formula wrong from aggregate totals alone. Per-wave telemetry and first acquisition records support narrower follow-up hypotheses.

Source-derived unit checks and the new suite establish their stated boundaries separately. This campaign does not verify audio perception, physical controller behavior, every item interaction or complete reference parity. Wall time is not a performance benchmark. Screenshots require separate visual inspection; file existence does not prove their content.

## R8 capture inspection

The integrating agent inspected the fresh R8 wave5,15,20 and end captures. They show active early/later combat and the Invoker boss wave. The end image displays a queued Drifter unlock; it does not display a victory title. Won/completion is established by the final campaign trace/report and exit0. This is native visual evidence of this scripted run, not human survival acceptance.

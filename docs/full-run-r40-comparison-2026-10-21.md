# R40 full-run independent review - October 21, 2026

## Result and scope

Both saved headless campaigns reached a win after all 20 waves and exited 0. The generated comparator passed all 18 bounded checks, as independently read from `full-campaign-r40/economy-comparison.json`. This is a fresh bounded integration comparison, not proof of exact gameplay parity. No game process, desktop application, original source or existing evidence was modified by this independent review.

Inputs are `artifacts/native-followup-2026-10-20/full-campaign-r40` and `original-safe-full`. The R40 manifests contain 634 source input records (all marked matching) and 159 player file hashes. Source preparation records 3,816 original inputs unchanged; only the isolated clone fixture changed for the safe-loadout acquisition policy.

## Observations

| Metric | Original | R40 |
| --- | ---: | ---: |
| Completed waves / shop entries | 20 / 19 | 20 / 19 |
| Purchases / debit checks passed | 29 / 29 | 36 / 36 |
| Upgrades / crates taken | 18 / 16 | 18 / 6 |
| Final items | 43 | 41 |
| Final level | 18 | 18 |
| Final materials / banked materials | 1472 / 1084 | 460 / 1309 |
| Enemy kills / tree kills | 4647 / 43 | 4931 / 37 |
| Material pickups | 1490 | 1558 |
| Final harvesting / Luck | 25 / 83 | 66 / 19 |
| Finished-event wall seconds | 1146.374 | 1092.828 |

The purchase traces contain no debit failures. Every R40 inventory snapshot sums to its reported item count. The original final debug inventory contains its current 43 items and final weapon IDs; the summary script validated and froze this attempt's log before comparison (43 items, 34 distinct IDs, matching final wave and weapon inventory).

Sampled original Main-scene-to-timer intervals range from -0.182131 to -0.042653 seconds relative to the scheduled duration; R40 timer intervals range from +0.002014 to +0.073608 seconds. Both are inside the existing 0.5-second tolerance for all 20 waves. Original timestamps are fixture samples after Main creation, not a claim of matching exact callback boundaries. Original total simulatedSeconds includes intermission fixture callbacks; R40 simulatedSeconds excludes cleanup and choices, so final cumulative timestamps cannot be equated.

The final R40 explicit settlement receipt credits 58 materials with harvesting 58 at the credit boundary. Post-settlement harvesting is 66. This supports the positive base credit check, not every interacting income formula.

## Safe-loadout policy and immunity

Both runs explicitly record `safe_loadout_policy` and contact hurtbox immunity. The policy skips positive `lose_hp_per_second` effect carriers, and recycles such crates through the original action. It does not change production damage rules. R40 skipped one Blood Donation offer in wave 9; the original skipped zero offers; neither recycled a crate under this policy. The original trace contains one explicit pause intervention in wave 1, resumed by the fixture.

These are neutral Danger 0 runs with six supplied tier-II starting weapons, scripted movement, muted audio, fresh profiles and engine-specific random sequences. They are not ordinary player starts or a survival/balance acceptance test. Original observed net damage 3 and healing 41 versus R40 0 and 66 are sampled net changes, not complete hit/heal telemetry.

## Differences and limits

Aggregate outcomes differ before purchases: at the first wave timer, original enemy kills 25, tree kills 1, pickups 8 and ground material value 20; R40 enemy kills 27, tree kills 1, pickups 5 and ground material value 25. Each snapshot reconciles ordinary base material value (25 + 3 = 8 + 20; 27 + 3 = 5 + 25). This does not identify a material payout defect. Different movement, placement, target order, random offers and subsequent inventory make total equality inappropriate. Neither successful run independently closes every item interaction or establishes identical physics and presentation.

Original stderr retains 7 ERROR records: missing CrashReporter log directory, one focus-on-detached-control error, three `_first != nullptr` conditions, resources still in use and remaining MemoryPool allocations. It also has 80 missing Attach warnings, a missing mods folder warning and an ObjectDB leak warning. No `SCRIPT ERROR` record was found. The R40 player log has no exception/error diagnostics. Exit 0 must not be described as an error-free original engine run.

No screenshots, audible effects, physical controller input, normal human movement/focus, browser behavior, or ordinary-damage survival are validated by these headless campaigns. Physical Xbox acceptance remains deferred by the user's instruction.

## Comparator result and reproduction

Root ran the summary and comparator successfully. First reproduce with `scripts/summarize_original_campaign.py --reference-dir artifacts/native-followup-2026-10-20/original-safe-full`. Its default debug path is this trace's own `save_root/user/log.txt`; it validates wave, item count and weapon IDs, freezes `reference-run-data.log`, and writes `summary.json`. Do not substitute an older summary.

Then use `scripts/compare_r14_campaign.py` with:

- `--unity-dir artifacts/native-followup-2026-10-20/full-campaign-r40`
- `--reference-dir artifacts/native-followup-2026-10-20/original-safe-full`
- `--reference-source-manifest artifacts/native-followup-2026-10-20/reference-safe-policy-preparation/clone-after.json`
- `--build-manifest artifacts/native-followup-2026-10-20/build-r40-manifest.json`
- `--source-manifest artifacts/native-followup-2026-10-20/build-r40-source-manifest.json`
- `--label R40`
- `--json-out artifacts/native-followup-2026-10-20/full-campaign-r40/economy-comparison.json`
- `--report-out docs/full-run-r40-economy-generated-2026-10-21.md`

The reference manifest check only tests a nonempty dictionary; it does not independently establish original preservation or clone equality. Preserve and cite the preparation before/after manifests too. The generated Markdown heading currently hardcodes October 17, 2026; the dated output path and this independent review identify the actual review date. The comparison manifest binds 3,819 isolated clone input hashes; it is not a complete original/clone equality assertion. The original preservation preparation separately verifies 3,816 original source inputs unchanged. This reviewer inspected the actual 18 passing checks and current final debug-inventory validation after root invocation.
### Process receipt compatibility correction

The Python runner records completed processes as `exitCode` and elapsed time, without the older runner's explicit `status: exited` field. The first comparison failed only that metadata gate (17/18); its `economy-comparison-schema-attempt-1` evidence was preserved. The summarizer/comparator now accept absent status or `exited` status while still requiring an exit-code receipt (zero for the comparator). An explicit non-completed status is rejected. Re-running against the same game traces passed 18/18. Gameplay, duration, purchase, stacks, eligibility and outcome checks were unchanged; no campaign was replayed to obtain this result.
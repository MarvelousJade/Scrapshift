# Complete verification — October 17, 2026

Branch: `codex/gameplay-parity`. **Latest R23 complete suite: 1031/1031 passed. R23 build has zero errors/warnings. Native virtual-navigation and staged-settlement checks passed, exit 0. Both R23 and the supplied-original scripted full campaigns completed Won, exit 0, 20 waves/19 shops. Current comparison: 18/18 bounded checks passed.** The R21 combat-window slowdown was not reproduced in the fresh R23 campaign; this is measured scripted-run evidence, not exact overall gameplay or performance parity.

## Complete available suite

All XML/log/summary evidence is retained in `artifacts/full-verification-2026-10-17/`.

- Attempt 1: 949/951 passed, two stale assertions failed, zero skipped, exit 2. Source flame burn truncates engineering damage before structure percentage; lowering Max HP preserves current HP. Tests were corrected against source instead of changing correct behavior.
- Attempt 2: 951/951 passed, zero failures/skips/inconclusive, exit 0, 68.3595964 seconds: 921 Scrapshift and 30 AssetScope checks.
- Attempt 3: 979/979 passed after arrival/origin changes, zero failures/skips/inconclusive, exit 0, 59.7238404 seconds.
- Attempt 4 produced no XML after a Unity licensing interruption; it is not a test failure.
- Attempt 5: 1000/1000 passed, zero failures/skips/inconclusive, exit 0, 54.1840171 seconds. Includes 21 muzzle tests spanning 84 authored profile cases.

- Attempt 6 after the final-settlement correction: **1009/1009 passed**, zero failures/skips/inconclusive, exit 0, 57.9269644 seconds: 979 Scrapshift and 30 AssetScope checks. All nine new settlement cases passed.

- Attempt 7 after staged cleanup lifecycle: **1018/1018 passed**, zero failures/skips/inconclusive, 55.3793378 seconds. Includes nine new lifecycle cases.

- Attempt 8 after allocation corrections: **1031/1031 passed**, zero failures/skips/inconclusive, exit 0, **61.156202 seconds**. Actual XML/summary are retained; this includes behavior-preserving optimization checks.

- Attempt 9 for R23: **1031/1031 passed**, zero failures/skips/inconclusive, exit 0, **58.5371699 seconds**. Actual XML/summary are retained.

These are Editor assembly checks. Native player fixtures and scripted campaigns are separate evidence.

## Native build checkpoints

- R15 build: zero errors/warnings, 401612839 bytes, 524 matching source/import/settings inputs, 160 hashed player files. Its campaign driver resets scripted movement age per wave. Its partial campaign was intentionally stopped after confirming production landing/origin differences.
- R16 build: zero errors/warnings, 401624265 bytes, 534 matching inputs, 160 hashed player files. Arrival fixture failed before gameplay samples because it selected the correct default melee starter although its test required a ranged weapon. Fixture setup was corrected explicitly; no universal second starter was restored.
- R17 build: zero errors/warnings, 401636441 bytes, 536 matching inputs, 160 hashed player files. Native gameplay assertions passed, including exact authored muzzle error 0, but one queued screenshot was missing at validation time. Overall fixture remained Failed.
- R18 build: zero errors/warnings, 401636441 bytes, 536 matching inputs, 160 hashed player files. Native arrival Passed, exit 0, all four captures. Virtual navigation Passed, exit 0, 17 snapshots. A wave-2 capture needed one real Update before the newly created weapon view received its pose.
- R19 review player: `Builds/GameplayParity20261017R19/LaunchGame.cmd`. Zero errors/warnings, 401636953 bytes, 536 matching inputs, 160 hashed player files. R18-to-R19 only changes `ArrivalNativeReview.cs`; production behavior is identical. Native arrival Passed, exit 0, authored muzzle/body-position errors 0, all capture/gameplay flags true, second-wave rendered alignment true. The weapon pivot lies 0.24 from its neutral mount. Virtual-controller fixtures do not establish physical Xbox acceptance or human gameplay.

The production arrival correction preserves simulation and input during landing, matches source raw map-center/body offset and player collision geometry, avoids duplicate weapon-clock advancement, uses authored weapon muzzle data, and does not replay arrival on later waves. Source-backed native assertions are distinct from artistic inspection.

## Fresh original provenance

The original remains read-only at `E:/UnityProjects/brotato-(3.x)`. Its isolated fixture copy is `E:/Codex Projects/Job Search Assistant/tmp/BrotatoFullComparison20261017`. The manifest records 3817 production-source entries and 14 explicit helper/differing files. Fresh host profiles and redirected userdata isolate saves; actual boot traces confirm neutral zero-material startup, six tier-II weapons, fresh progression, seed 771, time scale 1 and 60 Hz physics/render target. Both campaigns use invulnerability and scripted steering; they are controlled integration fixtures, not human survival/balance tests.

Original attempt 1 (`original/`) ended around wave 12, exit 1073807364, without a summary. Termination cause is unknown; runner/tool lifetime is an inference. One early pause was resumed using source PauseMenu.unpause; inspected captures showed no lingering overlay. The partial trace remains evidence.

Original attempt 2 (`original-attempt-2/`) completed Won, exit 0, 20 timers, 19 shops, 43 purchase debits, 58 item copies across 46 distinct stacks and 41 captures. Runner UTC start 2026-10-17T23:27:00.2567098Z, finish 23:46:12.3643358Z. It records one pause intervention and is not a zero-intervention run.

R18 Unity (`unity-final/`) stopped after 12 complete waves without a final report. Process/runner were absent after runtime interruption while its stale process JSON still said running. Cause is unknown; retain this partial trace. R15 Unity (`unity/`) was intentionally interrupted for the landing/origin fix.

## Completed R19 comparison and follow-up

R19 (`unity-attempt-2/`) completed Passed/Won, 20 waves, 19 shops, normal 60 Hz, no preflight, exit 0. Runner UTC start 2026-10-18T00:40:20.1055533Z, finish 00:59:17.7369775Z (October 17 local date). Final active combat time is 1050.199951 seconds.

`economy-comparison.json` and preserved `economy-comparison-r19-history.md` contain the historical R19 evidence/result; the current economy document now reports R23. Its original 17 checks passed; expanded coverage now reports **17/18**, with final positive-harvesting credit failing. Passed checks include: completion/process outcomes, all timers/shops, simulated durations, purchase debits (43 reference, 37 Unity), stack sums/caps, run-start eligibility and manifests. Unity ended with 45 item copies across 40 distinct stacks.

Totals differ: reference/Unity pickups 1854/2502, kills 4867/5074, materials 1596/2057, bank 714/2, harvesting 78/15. Acquisitions, upgrades and weapon tiers differ. The report preserves wave totals and first acquisitions. Before any shop, the original's 28 enemies yielded 8 pickups + 20 ground value; Unity's 27 enemies + one tree worth 3 yielded 5 pickups + 25 ground value. That concrete reconciliation rules out an obvious missing-material error in these first-wave snapshots without proving trajectory/targeting equivalence.

**Confirmed follow-up:** `RunRules.Tick` returned Won before final-wave harvesting, and `DefeatBoss` also bypassed settlement. Source `_on_WaveTimer_timeout` invokes `manage_harvesting()` before cleanup/win-state setup. Original wave-20 materials rose 1492 to 1596; Unity stayed 2057 despite harvesting 15. The original delta may include other end-wave effects and is not labeled pure harvesting. Runtime correction is verified in R20: timer expiry now settles existing income/XP/growth before Won; last-boss death starts a 0.1-second timer instead of immediately setting Won. Nine focused regression cases and the corrected existing boss-timer assertion passed in complete suite attempt 6. R20 build and focused native settlement passed as detailed below. No R20 full 20-wave rerun has been performed. The expanded comparator requires at least the recorded positive base-harvesting credit in this neutral fixture; R19 recorded zero against a minimum of 15.

Simulated duration checks do not prove wall-clock/performance parity. Unity wave-19 start to wave-20 start took 91.152 wall seconds for 60.002 active seconds. Driver Time.deltaTime is subject to maximum-delta settings; captures changed resolution during this run. No profiler or complete intervention log establishes the timing cause.

## R20 correction verified

Historical R20 review launch: `Builds/GameplayParity20261017R20/LaunchGame.cmd`. Build succeeded with zero errors/warnings, 401643261 reported bytes, 540 matched source/import/settings inputs and 160 hashed built files. The complete post-fix suite is attempt 6: **1009/1009 passed**, exit 0, with all nine new settlement cases.

The staged native final-settlement fixture reports **Passed**, empty error, process exit **0**. Both timer expiry and the delayed-boss route use ordinary Update and pickup sweep, then settle exactly once: materials **100 to 115**, harvesting **15 to 16**, XP **0 to 15**, phase **Won**, no final choices, and no duplicate credit after additional Tick. The boss route retains the **0.1-second** delay. Both captures exist and were visually reviewed by the coordinating agent. This fixture calls the boss-completion rule API; it does not reenact boss death itself or play all 20 waves.

- [Post-fix complete-suite summary](<E:/Codex Projects/Job Search Assistant/work/repos/Scrapshift/artifacts/full-verification-2026-10-17/suite-attempt-6-summary.json>)
- [Native settlement report](<E:/Codex Projects/Job Search Assistant/work/repos/Scrapshift/artifacts/gameplay-parity-r20-2026-10-17/settlement/final-settlement.json>) and [process exit](<E:/Codex Projects/Job Search Assistant/work/repos/Scrapshift/artifacts/gameplay-parity-r20-2026-10-17/settlement/process.json>)
- [Timer-route capture](<E:/Codex Projects/Job Search Assistant/work/repos/Scrapshift/artifacts/gameplay-parity-r20-2026-10-17/settlement/final-timer.png>) and [boss-route capture](<E:/Codex Projects/Job Search Assistant/work/repos/Scrapshift/artifacts/gameplay-parity-r20-2026-10-17/settlement/final-boss.png>)

The R19 full-run comparator remains **17/18** because it correctly describes the historical pre-fix run. Do not relabel that evidence as passing or combine it with the R20 fixture to imply a new complete R20 playthrough. The isolated production change affects final settlement and last-boss timer routing; focused native checks cover both changed paths, while pre-final-wave behavior remains unchanged by this fix.

## R21 cleanup-order correction and full campaign verified

Further source review established that end-wave income is credited before enemy removal/pickup cleanup, temporary stats reset before the separate 0.66-second harvesting-growth callback, and the ordinary/won cleanup timers are 2/4 seconds. R20's native fixture established final payout but did not establish this intermediate order.

The revised runtime uses an explicit lifecycle: `BeginWaveSettlement` credits endstats/conversion/income/XP once before cleanup and resets temporary stats; `AdvanceWaveSettlement` applies scheduled growth at 0.66 seconds using the current effective stat; `CompleteWaveSettlement` permits rewards/Won only after the 2/4-second timer. Public scene-free `Tick` fast-forwards these same stages for model consumers. Real ArenaGame Update advances them over time. RecordWaveEnd challenge sampling remains before settlement. Start/NextWave reset lifecycle state.

All nine new lifecycle cases passed in suite attempt 7 (1018/1018). The strengthened native fixture passed; the fresh full campaign completed Won/exit 0 with 20 waves/19 shops. R21 comparison output is `artifacts/full-verification-2026-10-17/unity-attempt-3/`. The new complete full-run comparison is labeled R21; R19/R20 results remain separate from the historical R21 evidence.

R21 build: zero errors/warnings, **401646845** reported bytes, **542** matching source/import/settings inputs, **160** hashed player files. Native report `artifacts/gameplay-parity-r21-2026-10-17/settlement/final-settlement.json` says Passed, empty error, process exit 0. Both timer/boss routes observed payout at cleanup age **0**, growth at **0.6666667** seconds, and Won after **4.01666** simulated cleanup seconds (60 Hz sampling). Route wall durations were 4.034/4.143 seconds. Both routes credited materials 100 to 115, XP 0 to 15, grew harvesting 15 to 16 once, and opened no final choices. Both captures were visually reviewed by the coordinating agent.

The new R21 full campaign started at UTC `2026-10-18T01:14:38.2919843Z` and exited 0 at `2026-10-18T01:35:46.0781795Z` (October 17 local date). Its report says Passed/Won, 20 complete waves/19 shops, normal 60 Hz, no preflight. The final settlement receipt records harvesting 34, materials 1226 to 1260 (credit 34), and final harvesting 36 after delayed growth. These are separate from the staged native timing fixture.

### Fresh R21 comparison invocation

Completed with exit 0 after reading `unity-attempt-3/campaign-check.json` and exited-zero process evidence. Preserve `economy-comparison.json` as the historical R19 failure; the historical R21 result was written separately:

```powershell
& 'C:/Users/FanSh/.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe' scripts/compare_r14_campaign.py --reference-dir artifacts/full-verification-2026-10-17/original-attempt-2 --unity-dir artifacts/full-verification-2026-10-17/unity-attempt-3 --label R21 --build-manifest artifacts/gameplay-parity-r21-2026-10-17/build-manifest.json --source-manifest artifacts/gameplay-parity-r21-2026-10-17/source-manifest.json --json-out artifacts/full-verification-2026-10-17/economy-comparison-r21.json --report-out docs/full-run-economy-review-2026-10-17.md
```

New campaign events include an exact settlement receipt (wave, materials before/after credit and harvesting at credit). This makes the final-income check independent of the timer callback's phase. Old R19 traces have no receipt and retain the failing legacy delta check; the separate `legacy-receipt-canary.json` confirms 17/18 without changing old traces/results.

## Final R21 saved-trace result and limitations

Historical R21 comparator: **18/18 passed**, exit 0, `artifacts/full-verification-2026-10-17/economy-comparison-r21.json`. Its report/notes are archived in `economy-comparison-r21-history.md`; the current economy report now describes R23. The historical R19 JSON remains 17/18; its report/notes were preserved in `economy-comparison-r19-history.md` before generating the current report. No historical run was relabeled.

R21 had 35 purchases,7 crates,17 upgrades and42 item copies across35 distinct stacks. Every recorded debit, stack sum/cap, initially locked acquisition check, active-duration check and manifest check passed. The final explicit receipt proves34 materials were credited at the settlement boundary. Reference totals and acquisition/upgrade paths differ; the report retains their per-wave timelines and first-acquisition differences. First-wave material accounting still reconciles exactly to each run's kills plus the tree's source value3.

**Historical measured timing issue:** R21 active combat totaled 1050.200 seconds, while start-to-timer combat windows totaled 1220.737 wall seconds: **170.537 seconds excess**. Cleanup/choice intervals were measured separately at 41.811 seconds; they do not explain that combat-window excess. The player summary wall duration was 1263.670 seconds. Worst combat windows: wave 19,100.501 wall/60.002 active; wave 15,95.882/60.002; wave 18,91.801/60.002; wave 16,76.219/60.002. The report records all intervals. The fixed-step driver's bounded catch-up and delta-time settings can limit advancement, but no frame profile from this run establishes why those frames slowed. Do not attribute this to shop randomness, claim performance parity, or turn these timings into a benchmark claim. Any separate development profile must retain its own build/scenario provenance.

## R22 allocation correction and R23 current checkpoint

R22 frozen build `Builds/GameplayParity20261017R22/LaunchGame.cmd` succeeded with zero errors/warnings,401679774 reported bytes. Its suite attempt 8 passed 1031/1031, exit 0, 61.156202 seconds. The correction removes allocation-heavy weapon-set membership/bonus queries, caches immutable process launch flags while preserving live settings, and removes allocation-heavy combat-geometry lookups. Independent review and oracle tests cover membership/identity, bonus-row selection, clamping, summation and live settings. R22 synthetic navigation failures are preserved; fixture focus resets invalidated held synthetic inputs.

Current review player: **`Builds/GameplayParity20261017R23/LaunchGame.cmd`**. R23 changes only opt-in `ControllerNavigationCheck` focus handling relative to R22 production gameplay: virtual input uses fixture-only IgnoreFocus. It does not establish physical-controller or ordinary-focus behavior. Build succeeded with **zero errors/warnings**, **401680286** reported bytes, **550** matching source/import/settings inputs, **160** hashed player files. Suite attempt 9 passed **1031/1031**, zero failures/skips/inconclusive, exit 0, 58.5371699 seconds.

R23 native navigation: `artifacts/gameplay-parity-r23-2026-10-17/navigation/navigation-check.json` has passed=true, empty error, **17 snapshots**, process exit 0. It injects virtual gamepad events through actual Update/OnGUI and stages intermission rewards; it is not human/physical-controller or ordinary-focus acceptance. R23 staged settlement: `settlement/final-settlement.json` reports **Passed**, process exit 0, both timer/delayed-boss routes. Character/combat/pause captures were visually reviewed by the coordinating agent.

## Completed fresh R23 full campaign and final comparison

Output: `artifacts/full-verification-2026-10-17/unity-attempt-4/`. Actual campaign report says **Passed/Won**, normal 60Hz, no preflight, **20 complete waves and 19 shops**. Process JSON records exit 0, UTC start `2026-10-18T02:06:41.8179839Z` and finish `2026-10-18T02:24:59.9887827Z` (October 17 local date).

The current comparator completed exit 0: **18/18 bounded checks passed**. Result: `artifacts/full-verification-2026-10-17/economy-comparison-r23.json`; current report: `docs/full-run-economy-review-2026-10-17.md`. It verified both completions/process outcomes, 20 timers/19 shops, active durations, 43 reference/37 Unity purchase debits, stack sums/caps, fresh-run unlock eligibility and input manifests.

Actual final settlement receipt: wave 20, materials **1227 to 1336**, credited **109**, harvesting at credit **109**. Delayed growth produced final harvesting **115**. The run ended with **45 item copies across 36distinct stacks**,37 purchases,19 upgrades,9 crates, level 20,4554 enemy kills,50 tree kills,2009 material pickups,91 banked materials and Luck 4. Reference acquisitions/upgrades/totals differ; the report retains their timelines and first-acquisition differences rather than equating random totals.

First-wave kill/tree/material accounting remained the concrete reconciled baseline: original 28 enemies yielded 8 pickups + 20 ground value; Unity 27 enemies + one tree worth 3 yielded 5 pickups + 25 ground. No debit, stack-cap/sum or initially locked-acquisition anomaly was detected in the full current trace.

### Measured timing after allocation corrections

R23 active combat totaled **1050.199951 seconds**. Start-to-timer combat wall windows totaled **1049.485831 seconds**, a signed excess of **-0.714120 seconds**; separate cleanup/choice windows totaled **43.335596 seconds**. Player-summary wall duration: **1093.956055 seconds**. The earlier R21 combat-window excess was +170.536661 seconds; that large slowdown was not reproduced in this new R23 scripted campaign. These runs have different random acquisitions/combat totals, so this is not an identical-workload FPS benchmark or proof of all-hardware performance parity. Separate development-profile evidence retains its own scenario/build provenance.

Historical `economy-comparison.json` remains R19's 17/18 failure. R21's 18/18 JSON and archived report remain unchanged. R22 synthetic focus failures remain preserved. The current R23 result does not rewrite those historical attempts.

Reproduce the completed comparison with:

```powershell
& 'C:/Users/FanSh/.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe' scripts/compare_r14_campaign.py --reference-dir artifacts/full-verification-2026-10-17/original-attempt-2 --unity-dir artifacts/full-verification-2026-10-17/unity-attempt-4 --label R23 --build-manifest artifacts/gameplay-parity-r23-2026-10-17/build-manifest.json --source-manifest artifacts/gameplay-parity-r23-2026-10-17/source-manifest.json --json-out artifacts/full-verification-2026-10-17/economy-comparison-r23.json --report-out docs/full-run-economy-review-2026-10-17.md
```

## Attribution and rehearsal

The user supplied requirements, reference and physical Xbox feedback. Codex and its agents performed source investigation, implementation, tests, builds and scripted checks. Keep stale assertions, fixture defects and production differences distinct. Trace a source formula and reproduce its regression before presenting it as personal hands-on debugging. These invulnerable scripted campaigns are not the user's human playthrough or evidence of exact overall parity.

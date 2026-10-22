# Headless full-campaign comparison - October 20, 2026

**Completed: both headless runs Passed/Won, 20 waves, process exit0; unchanged comparator 18/18 checks passed.** This is evidence for the frozen R28 player, before the later Beast Master selectable-start correction and pending Wrench correction. It does not validate those changes or claim the current checkout has completed this campaign. This pass uses no visible player/window control and creates no screenshot/UI evidence.

## Inputs and preparation

- Original output: `artifacts/parity-remaining-2026-10-20/original-full-headless`.
- Unity output: `artifacts/parity-remaining-2026-10-20/unity-full-headless`.
- Unity player: `Builds/GameplayParity20261020R28/Scrapshift.exe`. Its runner spec records `-batchmode -nographics --campaign-headless --campaign-realtime`.
- Original engine: supplied Godot 3.6.2 binary, cloned project `E:/Codex Projects/Job Search Assistant/tmp/BrotatoParity20261020`, explicit `--no-window`; runner spec records isolated APPDATA/LOCALAPPDATA/OUTPUT and `SURVIVAL=0`.
- Headless reference preparation: `artifacts/parity-remaining-2026-10-20/headless-reference-preparation/preparation.json` and frozen fixture/project snapshots. This changes only the isolated fixture/project from the earlier prepared clone. The original remains read-only.
- Reference source manifest: `original-full-headless/source-manifest.json`. Eleven selected original/clone runtime pairs are hashed; only profile-routing `singletons/progress_data.gd` differs among those selected pairs. Fixture/project hashes match the frozen preparation. Four original files have matching preparation-before/preparation-after/current hashes. This is selected-source provenance, not a complete source-tree equality audit.
- R28 build/source manifests: `build-r28-manifest.json`, `build-r28-source-manifest.json`. Build summary reports 0 errors/warnings, 401757562 bytes; all 586 source/settings/import rows report matches. This binds the R28 build, not subsequent source changes.

The two invulnerable comparison fixtures use neutral character effects, six supplied tier-II weapons, Danger0, scripted movement and fresh isolated progression. They are not human survival campaigns or deterministic cross-engine RNG reproductions. Normal 60Hz simulation checks do not establish graphics performance, audio or ordinary focus/controller behavior.

## Completion and diagnostics

The original trace records `finished`, wave20, `status=won`, `run_won=true`. Its process independently exited0 at 2026-10-20 12:27:24.8791106 UTC, starting 12:07:54.6543718 UTC. The current-attempt summarizer validated the final source debug inventory against the trace: 32 copies, 28 distinct item IDs, and matching final weapons.

Unity records `Passed`, `Won`, completedWaves20, shops19, realtime=true, pickupPhysicsRate60, headless=true and capturesVerified=false. Its process independently exited0 at 12:31:24.5884019 UTC, starting 12:13:07.5523153 UTC. No image freshness or pixel claim is inferred from either run.

The original stderr contains 35 script assertion messages for missing mod-loader log-file paths (error7), with `_rotate_log_file` / `_write_to_log_file` callsites, plus CrashReporter missing-engine-log diagnostics. The summary also retains two `!is_inside_tree` conditions, three `_first != nullptr` conditions, shutdown resource/MemoryPool notices, 80 vanished Attach-node warnings and an ObjectDB leak warning. One scripted startup pause intervention is explicitly retained in the trace. These are not removed or described as an error-free original execution; a successful completion does not prove every diagnostic harmless.

## Actual bounded comparison result

Evidence: `artifacts/parity-remaining-2026-10-20/headless-economy-comparison-r28.json` and `.md`. The JSON hashes all nine comparison inputs. All 18 original checks pass without changing `scripts/compare_r14_campaign.py`:

- Both wins and process exits; ordered 20 timer ends and 19 shops; both original 60Hz duration checks have no failures.
- Original **31/31** purchase debit checks and Unity **31/31** purchase debit checks pass.
- Original final stack sum **32** matches the actual source debug log; Unity final **35 copies / 32 distinct IDs**, with **zero** inconsistent stack snapshots.
- No positive catalog stack-cap violations in either checked scope, no initially locked/unknown Unity acquisitions, all 586 source-copy rows match, and both source/build manifests are present. Manifest presence is not itself a full source-tree equality claim.
- Final Unity settlement receipt is wave20, materials **328 -> 353**, harvesting at credit **25**, observed credit **25**. Finished harvesting is **27** after growth. This satisfies the existing positive base-income lower bound; it does not prove every possible income modifier.

| Final observation | Original | Unity R28 |
| --- | ---: | ---: |
| Materials | 995 | 353 |
| Banked materials | 1045 | 1252 |
| Level | 18 | 17 |
| Item copies | 32 | 35 |
| Enemy kills | 4654 | 4687 |
| Tree kills | 48 | 34 |
| Material pickups | 1514 | 1420 |
| Harvesting | 51 | 27 |
| Luck | -20 | 23 |

The original took 18 upgrades and 4 crates; Unity took 16 upgrades and 7 crates. Acquisitions/targets/spawn placement differ across engines; aggregate totals are not asserted equal. The first-wave accounting observation below is retained rather than explaining every difference as shop RNG.

## Timing scope

Unity active combat totals **1050.199951** seconds. Its summed start-to-timer combat wall intervals are **1049.121845** seconds (wall minus active **-1.078106**); separately measured cleanup/choices total **43.555876** seconds. The player's reported overall wall duration is **1093.803223** seconds. These are event-boundary observations from one headless scripted run, not FPS, rendering performance, identical-workload performance parity or a human survival measurement. The original and Unity engines also sample different callbacks; finite boundary/accumulator offsets are retained.

## Early divergence before shop acquisition

First-wave Unity snapshot: 27 enemy kills, 1 tree kill, 5 material pickups and 25 remaining ground value. At base tree value3, `27 + 3 = 5 + 25 = 30` reconciles that snapshot.

First-wave original snapshot: 25 enemy kills, no tree kills, 5 material pickups and 19 remaining ground value. The following shop snapshot banks19, so `5 + 19 = 24` leaves one material unmatched to the aggregate kill count. No item was yet acquired. Source has no-loot performance-removal deaths (`global/entity_spawner.gd:164` sets can_drop_loot=false) and deferred gold registration (`main.gd:spawn_gold` yields until a newly added gold node is ready). The trace lacks per-kill/drop/removal receipts, so neither explanation is established for this one-unit difference. It is an unresolved accounting observation, not a confirmed missing-material bug or a shop-RNG explanation.

## Comparison procedure

Keep `scripts/compare_r14_campaign.py` unchanged. It has 18 saved-trace/economy checks and no screenshot/capturesVerified requirement. Headless mode therefore needs no weakened check. `--campaign-realtime` remains required for its existing 60Hz/pickup-physics checks.

The coordinator executed `scripts/summarize_original_campaign.py --reference-dir artifacts/parity-remaining-2026-10-20/original-full-headless`. It read that attempt's actual debug log through boot save_root, checked final item/weapon inventory against the final trace, retained diagnostics and pause interventions, and required completed process evidence. No previous run's stacks were substituted.

The completed comparison used these arguments:

```text
scripts/compare_r14_campaign.py
  --reference-dir artifacts/parity-remaining-2026-10-20/original-full-headless
  --unity-dir artifacts/parity-remaining-2026-10-20/unity-full-headless
  --build-manifest artifacts/parity-remaining-2026-10-20/build-r28-manifest.json
  --source-manifest artifacts/parity-remaining-2026-10-20/build-r28-source-manifest.json
  --label "R28 headless"
  --json-out artifacts/parity-remaining-2026-10-20/headless-economy-comparison-r28.json
  --report-out artifacts/parity-remaining-2026-10-20/headless-economy-comparison-r28.md
```

The new output paths preserve prior R23/R25/native evidence. The actual 18/18 result establishes these bounded invariants only, with the unresolved reference first-wave observation and diagnostics above. Beast Master startup and the pending Wrench correction require their own subsequent evidence. No image/UI/hardware or universal gameplay-parity claim follows from this pass.

The user directed the scope and revoked computer control for this phase. Codex performed code/evidence inspection; the coordinating agent launched the authorized headless fixtures. No user hands-on playtest is implied.

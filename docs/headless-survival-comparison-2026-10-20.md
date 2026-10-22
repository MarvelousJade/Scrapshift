# Bounded headless survival comparison - October 20, 2026

## Scope

Following the user's instruction to stop computer control, these runs use Godot `--no-window` and Unity batch/headless mode. This reviewer read artifacts only; no desktop input, screenshot inspection, process launch or commit was performed for this review. Both fixtures explicitly report captures unverified/false. Earlier native screenshots are separate historical evidence.

Evidence folders under `artifacts/parity-remaining-2026-10-20/`:

- `original-survival-headless`: original clone report, `trace.json`, runner specification, process result, stdout/stderr and isolated userdata.
- `unity-survival-headless`: current Unity report, `campaign-trace.json`, runner specification, process result and logs.

Both use nominal seed 771, Danger 0, no character effects, scripted movement and the same six tier-II weapons: Pistol, SMG, Double Barrel Shotgun, Laser Gun, Knife and Spear. They use fresh progression and no continuously injected invulnerability. Normal damage immunity after a hit remains part of gameplay. These supplied loadouts are deliberately staged and do not represent an ordinary one-weapon start. The engines' random generators, roll ordering, movement units and accumulated random shop choices are not synchronized.

## Actual results

| Observation | Original Godot clone | Unity |
| --- | --- | --- |
| Fixture result / process | Passed / exit 0 | Passed / exit 0 |
| Natural outcome | Lost during wave 9 | Lost during wave 7 |
| Completed waves | 8 | 6 |
| Final HP / Max HP | 0 / 21 | 0 / 17 |
| Level / item count | 9 / 16 | 7 / 9 |
| Spendable materials | 122 | 96 |
| Final harvesting / Luck | 25 / -7 | 16 / 20 |
| Net damage / healing counters | 110 / 100 | 23 / 13 |
| Lowest sampled HP | 0 | 0 |
| Trace final elapsed wall time | 399.093 seconds | 229.465 seconds |

Passed means the explicitly bounded survival acceptance passed: genuine observed HP loss followed by natural defeat and the delayed acknowledgment state. **Neither is a 20-wave completion.** Each trace excludes its failed wave from the completed-wave count. These reports establish damage, healing observations, survival across successive waves, random purchases and a real loss path without injected invulnerability; they do not prove equal difficulty or universal parity.

### Sampling and timing limits

Godot samples current HP in the fixture's physics callback and retains the last observation across scene changes. Its positive deltas can therefore include new-wave restoration and Max HP changes. Unity sums HP deltas around each simulation step, including cleanup executed through that step but excluding shop-only transitions. Both are net changes, so damage and healing within the same sample can cancel. The 110/100 and 23/13 totals are not directly comparable counts of damage/healing events.

The original fixture finishes only after `Main._is_wave_failed`, `_end_wave_timer_timedout` and a visible retry/OK node. Its recorded age since the first physics observation of HP zero is 0.431 seconds; that observation can occur after the lethal callback, so it does not measure the source timer's complete half-second duration. Unity records `deathCleanupAge=0.500002861` at acknowledgment. The source half-second timer is independently established by source inspection and focused native checks, rather than treating these differently placed observations as a timing defect.

The original trace records one explicit fixture pause intervention near startup (wave 1, wall 4.172 seconds): it resumed an unexpected pause. This is disclosed fixture interference, not human control or evidence of correct ordinary focus behavior.

Final material banks are sampled at different points while cleanup can still animate remaining piles. The original acknowledgment trace has bank 235 and 310 ground material value; Unity's finished trace has bank 150 with 20 ground value, while its report one second later records bank 170. These observations must not be compared as equal settlement snapshots or interpreted as lost currency.

## Supported differences and conclusions

The observed outcomes differ (wave 9 versus wave 7), and so do accumulated levels, inventory size, Luck, harvesting and material collection. Those are confirmed run observations. Random shop/loadout histories and measurement contexts differ, so this pair alone identifies **no new causal gameplay implementation defect**. A specific mismatch would require a controlled matching encounter/loadout/event or source-derived regression, not changing difficulty until two unrelated random runs end on the same wave.

The complete invulnerable 20-wave comparison is a separate run and is not represented by these survival results. Human play, physical Xbox input, ordinary OS focus, visuals/audio and browser behavior remain outside this headless evidence.

## Reference diagnostics and preparation hardening

The original stderr is not clean: 35 startup ModLoader assertions report a missing log parent, one CrashReporter error reports no engine-log directory, one warning reports no unpacked mods folder, 36 warnings mention vanished Attach nodes, and shutdown reports three self-list conditions, one leaked-instance warning, resources still in use and MemoryPool allocations. The fixture nevertheless reaches its explicit finished record and process exit 0. These diagnostics are preserved and must not be described as zero runtime errors. The missing log directory is a clone harness setup issue; the remaining warnings/exit diagnostics are not attributed to a gameplay mismatch by this review.

`prepare_headless_campaign_reference.py` now creates the inherited redirected ModLoader logs directory before future launches. The script was edited only; preparation was not rerun while other campaigns use the clone, and no running clone fixture was changed. The original read-only project's hashes recorded during preparation remain preserved. Unity's survival player log and runner stderr contain no error/exception/warning/assert matches in this attempt; that narrow scan is not a general warning-free project claim.

Codex performed this artifact analysis under the user's parity direction. These are automated scripted observations, not user-performed debugging or a human campaign.

## Fresh R29 bounded survival observation

After support-registration corrections in commit `1ff6bb1`, the root ran the fresh R29 headless survival fixture. This reviewer read `unity-survival-headless-r29/campaign-check.json`, `campaign-trace.json`, `process.json`, `player.log` and runner stderr under the same evidence root. No desktop interaction, process launch or new source audit was performed by this reviewer.

- Report **Passed**, process **exit 0**, natural **Lost during wave 8** after **7 completed waves**. Trace `wave_completed` records are exactly 1 through 7; the failed eighth wave is not counted. This is not a 20-wave completion.
- `headless=true`, `survival=true`, `invulnerabilityInjected=false`, `capturesVerified=false`; normal timing and 60 Hz pickup physics are recorded.
- Lowest sampled HP is zero; summed simulation-step net damage/healing are **45 / 32**. Final trace HP/Max HP is **0 / 21**. These are net observations, not individual hit/heal counters or proof of any particular damage formula.
- Death acknowledgment age is **0.500002027 seconds**. The final trace occurs at approximately **301.642 seconds** and the report at **302.659 seconds**. The process exits normally at October 20, 12:51:03 UTC.
- Seven shops, nine purchases, seven upgrades and three crates; final level 8 with 12 distinct item copies, 147 spendable materials, harvesting 23 and Luck -1. Inventory consists of Beanie, Blood Leech, Boiling Water, Book, Broken Mouth, Claw Tree, Fertilizer, Insanity, Missile, Scared Sausage, Terrified Onion and Ugly Tooth (one each).
- Final report bank is 279. The earlier acknowledgment trace has bank 199 with 80 raw material value still animating on the ground; the subsequent difference matches continued cleanup collection. These are differently timed snapshots.
- Neither the current player log nor runner stderr has case-insensitive error/exception/warning/assert matches. The R29 build summary reports Succeeded, zero errors/warnings and 401,761,114 bytes. Integrated `suite-attempt-4.xml` records 1200/1200 Passed; earlier attempt 3's failed result remains historical and is not hidden by the passing retry.

R29 lasts seven completed waves versus R28's six and the source run's eight. Those are observed outcomes from different random acquisition histories and fixture executions, not a measurement of the registration correction's survival benefit. In particular, neither the aggregate damage totals nor the wave counts isolate a damage formula, target selection, support behavior or balance cause. A new causal mismatch requires controlled source/event evidence. The separate R29 full headless comparison remains independently tracked by the root; these bounded survival results do not replace it. No hardware, ordinary focus, visuals/audio, human campaign or browser claims follow from this run.

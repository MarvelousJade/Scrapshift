# Latest native checkpoint — R14, October 17, 2026

Branch: `codex/gameplay-parity`. Latest unpacked review: `Builds/GameplayParity20261017R14/LaunchGame.cmd`; its controller launcher isolates settings/progression. This replaces R13 as the latest build. The supplied original remains read-only.

## Completed native gate

The unlock-pool and support projectile corrections, native economy/item-stack/removal checks, support sequences, enemy slash probes and accepted basic Xbox controls are documented in [the detailed R13 checkpoint](native-gameplay-r13-2026-10-17.md). Current bounded evidence:

- **172/172 focused regressions**, zero failed/skipped, Unity exit0 (R12).
- **43 support cases passed**, zero failures, exit0; four production sprite probes and four sampled projectile sequences,63 fresh PNGs (R13). Eight fresh captures were inspected; twelve earlier R12 sequence captures were also inspected.
- **Native economy passed**, exit0; five fresh R13 captures inspected, including one Coupon and no Peacock after removal. Locked price14→14, free/paid reroll0→1; special shop hooks are explicitly synthetic.
- **Virtual native controller navigation passed**,17 snapshots, exit0 (R13). Physical basic controls were separately confirmed by the user and logged in R10. Start opens pause and B returns/resumes, matching the original.
- **53 enemy cases and eight slash probes passed**, exit0 (R12); all eight new slash captures inspected. Native combat impact/contact/recovery and the earlier loot path also passed in their separately identified builds.

These are carried results with recorded input continuity, not claims that every check was rerun in R14. Only `CampaignCheck.cs`, an opt-in test driver, changed between R13 and R14. R13 itself differed from R12 only in two fixture files. See the per-build `source-comparison.json` and manifests.

## Campaign-driver cadence correction and one-wave check

Source review found that the older campaign fixture directly called `Simulate` with variable rendered delta, bypassing the game's current fixed 60 Hz accumulator. R14 uses `ReferencePhysicsClock.Consume` and a1/60-second simulation step for normal-speed campaign fixtures. The accelerated fixture now uses24 such steps per rendered frame and remains separately labeled. Ordinary gameplay already used this clock; the correction is to the test driver.

An opt-in `--campaign-preflight` stops after wave1 and its shop, rather than running the full campaign. Actual R14 launch used this flag with `--campaign-realtime`:

- `campaign-check.json`: **Passed**, `preflight=true`, `realtime=true`, phaseShop, one completed wave and one shop, normal60Hz pickup physics.
- Process exit0. Active simulation20.0165329 seconds; wall time23.3965588 seconds. The small floating/tick boundary is reported, not rounded into a claim of exact timer identity.
- Final3 materials,27 banked,28 total kills (27 recorded enemies), zero purchases/upgrades/crates in this one-wave sample. Do not claim this preflight exercised purchases; the dedicated economy fixture did.
- Root opened both fresh captures, `wave-01.png` and `end.png`: combat/HUD/six-weapon fixture loadout and the actual wave1 shop were visible.

Evidence: `artifacts/gameplay-parity-r14-2026-10-17/one-wave-preflight/campaign-check.json`, its trace, screenshots, `preflight-process.json` and `preflight.log`. Neutral fixture startup deliberately uses zero materials and supplied six tier-II weapons; ordinary selected-character runs use30 materials and their character-specific starting loadout.

## Build and limits

R14 build succeeded, **zero errors/warnings**,401612839 reported bytes. All524 recorded root/clone source/settings/import hashes matched after build; these are inputs, not524 new tests. The build manifest hashes160 files. Normal play is not claimed to be running merely because the build succeeded.

**No whole suite or full20-wave campaign was run in this continuation.** The one-wave preflight closes the identified test-driver cadence issue; it is not a full comparison with the original. A future full run must use the current driver and retain engine-specific RNG/acquisition differences and test setup limits. Human survival, simultaneous engine ordering, audible equivalence and current WebGL behavior are not established by these focused fixtures.

The user supplied requirements, reference and real Xbox feedback. Codex implemented and tested the changes. Interview rehearsal: explain why a passing production regression selection cannot validate a test driver's independent simulation path; trace the common60Hz clock, read the preflight flag/report and distinguish one-wave evidence from a full campaign. Reproduce and understand the example before presenting it as personal debugging experience.

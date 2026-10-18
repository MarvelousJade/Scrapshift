## Latest native checkpoint — R14, October 17

R14 is the latest unpacked build (`Builds/GameplayParity20261017R14/LaunchGame.cmd`), zero build errors/warnings. Only the opt-in campaign driver changed from R13: it now uses the normal60Hz combat clock. Its normal-speed one-wave preflight Passed, exit0, reached Shop after20.0165 simulated seconds. Both fresh captures were inspected. It did not run a full campaign.

Carried input-matched evidence:172 focused regressions passed; R13 support43/43, economy and virtual navigation17 snapshots passed; R12 enemy53/eight slash probes and combat passed. Requested basic Xbox controls were confirmed by the user and logged separately. Unlock-pool timing and support projectile behavior fixes remain intact. No whole suite or full20-wave comparison ran. See [latest evidence and attribution](native-gameplay-r14-2026-10-17.md). Earlier R13 notes saying the campaign driver still needs a cadence correction are superseded by R14.

# Native gameplay checkpoint — R13, October 17, 2026

Branch: `codex/gameplay-parity`. Latest unpacked review: `Builds/GameplayParity20261017R13/LaunchGame.cmd`. The controller launcher uses an isolated in-memory profile. The supplied original remains read-only.

## Corrections

- Item/weapon unlock eligibility is now frozen for the active run and refreshed for the next run, matching source `ItemService.init_unlocked_pool` at `RunData.reset`. Achievement notifications and unlocked character availability remain immediate. The native case completes Fruit Basket during play, verifies its current-pool exclusion, then verifies next-run eligibility.
- Support projectiles now pose at their current age, use source-specific sprite offsets, progress ordinary bullets beyond their initial flash, use Tyler's lightning frames and distinguish orange Blazemander flames from blue turret flames. All textures were already shipped in this project; no new reference artwork was imported.
- The combat presentation fixture now advances fast shots in six 60 Hz steps instead of one .1-second jump. This is a fixture correction, preserving discrete gameplay contact sampling.
- The user confirmed the requested basic Xbox controls and accepted B not opening pause during a wave as reference behavior. Start opens pause; B returns/resumes. Physical observer data records navigation, movement, aiming, three Start pause transitions and two B resume transitions. Optional shop shortcuts/disconnect/device combinations remain outside that physical acceptance scope.

## Evidence and provenance

R12 passed **172 selected EditMode checks**, zero failed/skipped, Unity exit zero. R12 and R13 builds both succeeded with **zero errors/warnings**. R13's recorded build size is 401612327 bytes. All **524 recorded source/settings/import inputs** matched the isolated validation clone after building; this number is an input count, not a test count. Build manifests hash all160 files.

Only `EconomyNativeReview.cs` and `StructureReview.cs` differ between R12 and R13. These are fixture changes: capture pause inventory after removal, and isolate the sprite-presentation probe from retained combat targets while logging each condition. Production behavior and focused test inputs are identical; the 172 results are carried R12 evidence, not a falsely relabeled R13 test execution.

| Check | Actual evidence |
| --- | --- |
| R12 combat presentation | Passed, exit0; near/far piercing damage100/50, push distance1.0542579, contact/flash/recovery checks true; root inspected three captures |
| R12 enemies | Passed53 existing cases plus eight slash probes, exit0; all eight new captures inspected by the enemy agent |
| R11 loot path | Passed seven drop/pickup/cleanup/reward/shop checks, exit0; staged damage/XP, not normal survival |
| R13 economy | Passed all report booleans, exit0; two Coupons purchased, one retained after removal, locked price14→14, free/paid reroll0→1; all five fresh captures inspected |
| R13 support | Passed43, zero failures, exit0; four production sprite probes and four 60 Hz sequences included;63 fresh captures |
| R13 virtual controller navigation | Passed,17 snapshots, exit0; actual player input/menu lifecycle using synthetic Gamepad, including new run-start unlock lifecycle; not repeated physical hardware input |

R12's first expanded support execution is preserved as Failed:42 passed, one failed (`structure: turret`). Four time-sampled sequences passed, and the printed turret age/texture/rounded offset were correct, but the old diagnostic did not identify which conjunct failed. Its sprite probe shared randomly placed targets with the preceding behavior case. R13 isolates that presentation-only probe and records live membership, exact offset, age, texture, hit count and remaining life. All four probes passed: live/age/texture/offset true, zero hit count; turret retained .09333334 seconds of life, with offset(.00999999,-.00999999). Do not retroactively claim a particular collision was proved by the incomplete R12 log.

The native economy case uses real rule acquisition/removal and real screen renderers with staged resources/offers. Special-shop effects are separately labeled synthetic because the supplied base catalog does not assign those keys. The after-removal pause capture visibly shows one Coupon and no Peacock. Hidden modifier/pool arithmetic remains report assertions, not screenshot proof. See `native-economy-next-checks-2026-10-17.md`.

Support sequences sample the same emitted shots at0/1/2/3/6 physics ticks. Twelve R12 samples were visually inspected: flash-to-elongated bullet, cyan lightning, orange pet flames and blue turret flames. The original turret's initial flash also overlaps its body; own barrel silhouette is not a demonstrated gameplay mismatch. Preserve the source projectile origin. See `native-support-next-checks-2026-10-17.md`.

Enemy slash probes are independently staged production attacks at .2/.48/.54/.63 seconds for standard and forward variants. They establish warning/no hit, active hit and expiry/removal with matching frames; they are not one uninterrupted video. See `native-enemy-review-2026-10-17.md`.

## Full-run boundary and remaining limits

No whole suite or full campaign was run in this continuation. The next full-run phase must first adapt/check `CampaignCheckTick` against the current fixed 60 Hz combat cadence: its older direct `Simulate(delta)` path bypasses the ordinary Update accumulator. Do not call an older variable-step campaign result validation of the current normal-play cadence. This fixture issue does not change the native checks above, which explicitly step production contacts or run the actual input loop.

These focused results close bounded source-derived bugs and native probes. They do not establish universal physics/contact ordering, cross-engine RNG equality, audible equivalence, human survival balance or new WebGL readiness. Whole-run comparison and human playtesting remain distinct evidence.

## Interview and attribution

The user supplied requirements, original project and actual Xbox feedback. Codex inspected source, implemented fixes, authored/repaired fixtures, built players and ran automated/native reviews. Explain why unlock notifications can be immediate while the random pool is fixed for a run; why the current sprite frame differs from a previous-age capture; and how test-world isolation prevents an unrelated target from determining a presentation assertion. Reproduce and trace these examples before claiming personal debugging experience. Do not present synthetic setup or AI-assisted work as independently user-authored playtesting.

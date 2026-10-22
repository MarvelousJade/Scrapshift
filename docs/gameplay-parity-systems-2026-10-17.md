# Focused gameplay continuation  -  October 17, 2026

Branch: `codex/gameplay-parity`. Supplied original inspected read-only at `E:/UnityProjects/brotato-(3.x)`.

## Implemented and covered by focused validation

- Integrated fire-time burn payloads through ordinary/support projectiles, direct contacts and native/support explosions. Melee uses current contact data.
- Integrated Ghost slot counters and ordered critical income/on-hit callbacks; common explosions retain their active window and cumulative kill count.
- Bound support shots to source collision/lifetime profiles without replacing approved art or audio. Healing shots can hit Scapegoat, matching the supplied script.
- Completed source-derived shop/unlock timing, fractional pricing, cleanup-fruit and incoming projectile-contact changes from the preceding source pass.
- Corrected finalized enemy-speed use, raw-origin targeting/spawning, Buffer body eligibility, and performance-cull death callbacks without loot.
- Corrected initial spawn warnings to raw origins, and Predator arm accessibility speed for rotation/extension and persistent contact accounting. Track entries independently per orb and reset eligibility when the hurtbox disables; support contacts do not destroy these persistent projectiles.
- Corrected Wandering Bot slow to use current modified enemy speed. Kept Bot mine pet scaling: its source effect explicitly sets `is_pet=true`.

## Rejected hypotheses

Predator child scenes override damage growth to 1.15, so their existing growth is retained. Enemy slow recovery remains 3 world units/second. Source Dwarf/Unstoppable Force hooks and random charge-point target mode have no registered resources in the supplied base roster; they do not establish missing playable base behavior. Turret boostClock has no production writer and source Buffer cannot reach turrets through its mask.

## Validation status

Attempt 1 ran 434 selected checks: 427 passed and seven failed. One fixture missed extended-enemy initialization; three burn fixtures omitted four Sausages’ additive base damage; three Dagger boundary cases exposed inconsistent chance precision. The fixture setup/expectations and Dagger precision were corrected using source evidence. Attempt 2 passed **434/434**, zero failures/skips, Unity exit zero. Post-run SHA-256 checks confirmed all **508** recorded inputs still match root and clone. Generated catalog contents match after line-ending normalization; it was not overwritten. The preceding 213 passing checks predate this batch. No whole suite, new player build or full campaign comparison has been launched. Selected checks ran after the agents froze their files. Evidence in `artifacts/gameplay-parity-systems-2026-10-17/`: `focused-attempt-2.xml`, `focused-attempt-2.log`, `focused-summary.json`, `source-manifest.json`, and `catalog-comparison.json`. Attempt 1 XML/log remain available.

Remaining verification includes live contact/timing behavior, native support muzzle alignment, physical Xbox retest and an eventual full-run comparison. Passing selected fixtures cannot establish exact parity or release readiness.

## Evidence / attribution

See `weapon-next-audit-2026-10-15.md`, `support-next-audit-2026-10-15.md`, `shop-unlock-next-audit-2026-10-15.md`, `enemy-movement-buffer-next-audit-2026-10-15.md`, and `enemy-final-audit-2026-10-17.md` for source references and owned checks. The user provided requirements and prior physical feedback; Codex inspected sources, authored changes/tests and runs validation. These are not independently user-authored debugging stories.

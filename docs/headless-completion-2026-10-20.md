# Remaining headless acceptance — October 20, 2026

Branch: `codex/gameplay-parity`. Latest unpacked Windows review: `Builds/GameplayParity20261020R31/LaunchGame.cmd`. Keep the complete player folder together. Nothing is claimed to be visibly running; desktop control remains paused at the user's request.

## Fixed

Golem's below-half-health bonuses were recomputed after temporary-stat cleanup in Lost/Won. The original clears them at death/end-run. The terminal-phase guard now removes that invalid bonus while retaining valid low-HP combat/pause behavior. Source anchors, two regressions and terminal-fixture limits are in [item evidence](headless-items-2026-10-20.md).

## Executed evidence

| Check | Result | Scope |
| --- | --- | --- |
| Focused tests |26/26 Passed, exit0 |14 item/proc and12 shop cases; `focused-attempt-4.xml` |
| Integrated suite |1,226/1,226 Passed, zero failures/skips, exit0;68.7548356 seconds |`suite-attempt-2.xml`; pre-fix1,224 result retained separately |
| Dynamic original/Unity comparison |53 cases,397 snapshots,6,352 values; zero exporter errors/differences |Bounded timer/movement/HP/consumable/lifecycle callbacks. Source terminal fixtures invoke reset operations, not a full death scene. |
| Packaged R31 shops |14 receipts Passed, exit0 |Prices/debits, HP budget, capacity, locks, free/paid rerolls, acquisition/removal, Anvil, Hourglass and natural Candy conditions/consequence. No GUI/physics claims. |
| Packaged R31 unlocks |109 unique scenarios Passed,654 true flags, exit0 |Production callbacks with staged setup/clocks; independently reread109 actual JSON profiles. Active/fresh pool checks use separate synthetic snapshots. With historical6 native IDs, all115 definitions have mapped producer evidence. |
| Original material probe |28 deaths =8 pickups +20 banked; exit0 |One instrumented source wave. Older25/24 discrepancy was not reproduced and remains unexplained. No payout fix. |
| Packaging |Succeeded, zero build errors/warnings;401797798 bytes |608 source/settings/import inputs and159 player files hash-verified; catalog equal after line-ending normalization. |

Receipts are under `artifacts/headless-completion-2026-10-20`: `dynamic-items-attempt-5`, `shops-r31-attempt-1`, `unlocks-r31-attempt-1`, `material-probe-attempt-1`, build manifests and `r31-input-verification.json`. Failed fixture attempts remain preserved and explained in the area documents.

## Campaigns and remaining limits

R29's successful bounded20-wave comparison and ordinary-damage natural defeat are preserved in the [earlier checkpoint](gameplay-final-checkpoint-2026-10-20.md). They are not relabelled as R31 campaigns. The only new gameplay change affects Golem terminal stats; the neutral campaign character does not carry this conditional. R31 therefore received affected source/model checks and packaged callback regressions without repeating the unaffected broad campaign.

The finite additional headless shop/unlock inventory is complete at the stated callback level. Arbitrary item combinations/orders, every real-time interleaving and every character/loadout are not exhaustively proven. No known mismatch remains in these executed cases; universal exact parity remains unproven.

Hands-on gates remain: current starting-item picker, support placement/presentation, ordinary keyboard/mouse focus, physical Xbox navigation/combat/modal transitions, readable prices/rewards and audio. Human survival/balance, organic unlock thresholds and current browser saves/audio/focus/fullscreen/performance also remain. See [verification status](gameplay-verification-status.md) and [acceptance checklist](remaining-native-acceptance-2026-10-20.md).

## Commits and attribution

- `a5469c9`: additional shop acceptance and isolated fixture settings.
- `a56bfb4`: Golem terminal correction and dynamic item comparison.
- `269a8d2`: remaining unlock producer/persistence acceptance.
- `d038414`: bounded material-accounting investigation.

The user directed scope and supplied the original reference. Codex agents inspected source, authored the correction/fixtures, ran CLI checks and analyzed receipts. These are assistant-run checks, not user-authored debugging or fresh user playtesting. Bug, interview and contribution notes retain this distinction.

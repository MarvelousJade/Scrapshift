# Material accounting probe — October 20, 2026

## Result and remaining question

The isolated original-engine probe reconciled **28 counted enemy deaths = 28 requested units = 28 registered units = 8 player pickups + 20 cleanup-banked units**. No material remained on the ground at completion. The [accounting receipt](../artifacts/headless-completion-2026-10-20/material-probe-attempt-1/accounting.json) reports `Passed`; the [process receipt](../artifacts/headless-completion-2026-10-20/material-probe-attempt-1/process.json) records exit 0.

**The older observation of 25 kills against 24 accounted units was not reproduced and remains unexplained.** This new wave had 28 kills and a different event/RNG history. It neither resolves that historical discrepancy nor justifies changing Scrapshift payouts. No gameplay payout fix is implied.

## Bounded evidence

| Observation | Recorded result |
| --- | ---: |
| Counted enemy deaths / loot attempts | 28 / 28 |
| Requested / registered material units | 28 / 28 |
| Player pickup callbacks (`player_index=0`) | 8 |
| Cleanup pickup callbacks (`player_index=-1`) | 20 |
| Ground value at wave-timer callback | 20 |
| Final ground value / bonus bank | 0 / 20 |
| Peak requested but not yet registered units | 1 |
| Performance-removal callbacks | 0 |

The [trace](../artifacts/headless-completion-2026-10-20/material-probe-attempt-1/trace.json) independently contains 28 each of `material_loot_attempt`, `material_requested`, `material_enemy_counted`, `material_registered` and `material_picked`. All pickup values were 1. Its SHA-256 is `fde76560f74ff1ad658d232a99f24336916df3b148e9ddd90fce7b629ca5d87c`, matching the accounting receipt.

At the timer callback, the trace recorded 28 kills, 8 pickups, 20 ground value, 0 bank and 8 spendable materials. At completion it recorded the same 28 kills / 8 pickups / 8 spendable materials, with ground 0 and bank 20. Cleanup bank is a future-drop bonus, separate from immediate spendable currency.

## Instrumentation and isolation

The [preparation receipt](../artifacts/headless-completion-2026-10-20/material-probe-attempt-1/preparation.json) identifies a dedicated `tmp/BrotatoMaterialProbe20261020` clone based on the preserved comparison clone. The supplied original and R29 evidence were not edited. Paths/settings/save destinations were redirected into the probe's isolated folders. The [runner specification](../artifacts/headless-completion-2026-10-20/material-probe-attempt-1/runner-spec.json) used Godot 3.6.2 with `--no-window`; no desktop or interactive input was controlled.

Preserved snapshots show diagnostic additions in:

- [main.gd](../artifacts/headless-completion-2026-10-20/material-probe-attempt-1/main.gd.snapshot): loot attempts, the existing computed spawn count, material-cap boosts, registration in `_active_golds`, pickup callbacks and the existing enemy-count location.
- [entity_spawner.gd](../artifacts/headless-completion-2026-10-20/material-probe-attempt-1/entity_spawner.gd.snapshot): performance-removal notification before disabling loot and removing an enemy.
- [parity_fixture.gd](../artifacts/headless-completion-2026-10-20/material-probe-attempt-1/parity_fixture.gd.snapshot): forwards active diagnostic events into the trace and stops after the first wave's cleanup.

The diagnostic callbacks add no RNG draws and do not replace the source loot or pickup operations. They do add execution/logging overhead, so this is not an identical timing replay or performance benchmark.

## Timing and coverage limits

The source creates new material nodes through deferred `add_child`, waits for `ready`, then registers them in `_active_golds`. A callback can therefore observe one requested unit before registration; this probe's peak was 1. The enemy-count diagnostic also precedes incrementing the aggregate kill statistic, and the pickup diagnostic precedes removing the node and crediting currency/bank. Individual trace rows are event-boundary snapshots, not atomic end-of-frame balances. Final reconciliation is required. This timing observation is **not proof of the cause** of the older missing-unit observation.

This was one configured 20-second wave with six tier II weapons, neutral character effects, Danger 0, scripted movement and injected invulnerability. The timer callback was observed at 20.111225 simulated seconds; completion at 23.067753 included cleanup. One unexpected-pause intervention is explicitly recorded. The source log includes startup/settings/scene and shutdown leak/resource diagnostics, so exit 0 and the reconciled accounting must not be described as an error-free engine log. Zero performance removals does not validate the material behavior of that unexercised branch.

No screenshots, normal focus/input, audio, survival balance, multiwave accounting or exact historical reproduction are established. The remaining historical discrepancy requires its original event history or a reproducing run before assigning a cause.

## Attribution and interview use

The user requested the remaining feasible headless checks and withheld computer control. Codex prepared/ran the isolated probe and inspected its receipts; this document is assistant-authored analysis of that evidence. Do not present it as a payout bug personally found or fixed by the user. A useful rehearsal is to explain why deferred object registration and pre-credit callbacks can make an intermediate snapshot incomplete, then show how final conservation distinguishes that possibility from an actual lost payout.

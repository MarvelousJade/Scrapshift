# Item and material follow-up — October 20, 2026

Branch: `codex/gameplay-parity`. All work in this pass used agents, source inspection, CLI or windowless fixtures. The supplied original remained read-only; no desktop input or visible game control was used.

## Confirmed differences

1. **Proc target chosen too late.** The original chooses before probability rolls and can waste a hit on a zero-HP enemy awaiting removal. Scrapshift selected a live replacement when draining its queue. Target identity is now captured before rolling and retained through dispatch. R32's packaged192-row target review passed;23 focused tests and the1,249-test intermediate suite passed.
2. **Queued hits received later vulnerability.** Original Lich damage followed by an Ice Cube hit leaves89 HP; the unfixed R33 player left88 because the queued Lich hit used the newly applied vulnerability. Queued hits now retain the vulnerability percentage present at their request. Ordinary hit behavior and the existing reflected damage signature are preserved. The25 focused tests passed, including vulnerability appearing and expiring before dispatch. Final integrated suite:1,251/1,251 Passed, zero failures/skips, exit0,73.4048644 seconds (`suite-attempt-2.xml`). Packaged acceptance is recorded below.

The source probe invokes actual Main target selection, Unit defense math and imported Ice Cube behavior. Its damage sink/lifecycle wrapper is deliberately narrow; it is not an original critical-hit physics playthrough. R33's `Passed` baseline status means it successfully reproduced88 HP, **not** that it matched the original. See [proc investigation](item-proc-followup-2026-10-20.md).

## Additional checks completed

| Area | Evidence |
| --- | --- |
| Acquisition/removal order |416 cases /4,576 snapshots /73,216 primary-stat values, zero differences. Snowball with every208 other imported items under two characters; direct-grant context includes unavailable definitions. [Scope](item-acquisition-order-followup-2026-10-20.md). |
| Legal combined effects |18 focused lifecycle/proc cases Passed: Snowball order, duplicate wave gains across characters, damage/protection/dodge and mixed healing/direct-currency/pickup/death triggers. Fixture corrections are preserved. |
| Source target selection |192 cases; dead-only64 aborts, live-only64 hits, mixed38 aborts/26 hits. No identical cross-engine RNG-sequence claim. |
| Historical material trace |25 kills /24 accounted units remains causally unassigned. Kill-to-material equality is invalid: zone-wide Evil Mobs can count as zero-value kills or consume materials. Scrapshift's inspected consumption/evolution/payout path matches the source. |
| Cleanup boundary |Four injected source cases completed/exit0. Fresh deferred drops missed cleanup attraction/banking; pooled controls banked normally. Scene replacement was held, so this does not prove a final destroyed drop or the historical cause. Source diagnostic assertions are retained. [Evidence](material-reconciliation-followup-2026-10-20.md). |

No Scrapshift payout change was justified by the material receipts.

## Remaining limits

The queue still protects indexed list traversal. Lethal preemption by unrelated same-step hits and the complete lifetime of deferred corpses have not been compared exhaustively. Captured target/vulnerability checks do not establish identical death scheduling, kill attribution or every compound item sequence. Arbitrary larger loadouts and all real-time interleavings remain unproven.

Hands-on keyboard/mouse, Xbox, focus, UI presentation, audio and browser acceptance remain deferred. R29's earlier full campaign is historical evidence; it is not relabelled as a campaign on these new combat rules.

The user directed scope and supplied the reference. Codex agents authored the investigation, fixes, fixtures and checks. Interview notes are rehearsal material; no independent user debugging or new human playtesting is claimed.

## Final R34 acceptance

`Builds/GameplayParity20261020R34/LaunchGame.cmd` is the current unpacked review. Build Succeeded, zero errors/warnings,401807181 bytes.616 source/settings/import inputs and159 player files were hash-verified. The final headless player review Passed/exit0 with192 candidate rows and all guard flags true; forward vulnerability health89 and reverse-expiry health88 match expectations. Full suite1251/1251 and focused25/25 Passed. Receipts are in `artifacts/item-material-followup-2026-10-20`. This is not a fresh full20-wave campaign or interactive review.

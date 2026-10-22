# Item acquisition order comparison — October 20, 2026

The finite source/Unity comparison passed **416 cases / 4,576 snapshots / 73,216 primary-stat values**, with zero exporter errors or differences. Two characters (Well-Rounded and Mage) each exercise Snowball with every other208 imported item definitions. There are160 available and48 unavailable companion definitions in this matrix; direct grants of unavailable content are model coverage, not natural shop eligibility.

Each case enters intermission, then adds Snowball, acquires/removes/reacquires the companion item, removes Snowball, acquires the companion without it, and reacquires Snowball before another removal/acquisition cycle. At most one copy of either item is held. The source uses genuine RunData add/remove operations; Unity uses GrantItem/RemoveItem. All16 primary stats are compared after each operation. This tests order, nonretroactivity and persistence of earned gains; it does not validate prices, input, all secondary effects, organic progression or arbitrary larger loadouts.

The generator is `scripts/headless_acquisition_order_cases.py`, using the existing dynamic source and Unity exporters. Receipts are in `artifacts/item-material-followup-2026-10-20/acquisition-attempt-2`. Both processes exited0.

## Preserved fixture correction

Attempt1 injected acquisitions into a stationary in-wave Player without delivering another movement callback. It produced36 differences involving only Barricade, Chameleon and Statue in the two character contexts. Unity computes their standing bonus on demand; the unready source fixture had not received the standing-input callback for the newly added effect. Ordinary item acquisition/removal occurs at intermission. The corrected matrix enters that shared context before acquisition; no production standing-stat change was made to satisfy an artificial sequence. Attempt1 remains preserved.

## Related focused tests and attribution

The new lifecycle and proc suites passed18/18 focused cases with exit0 (`focused-attempt-2.xml`), including legal stacks, wave growth, damage/healing combinations and acquisition order. The initial3 failures were corrected fixture assumptions about Doctor level-up HP and reconciling Torture Max HP before staging health; they are not production defects. See [lifecycle](item-lifecycle-followup-2026-10-20.md) and [proc checks](item-proc-followup-2026-10-20.md).

Codex prepared and ran these checks under the user's no-desktop-control instruction. Interview rehearsal: trace a permanent gain separately from its trigger item, explain why removal does not roll back an already earned gain, and explain how an omitted callback can create a false comparison failure. These are assistant-run checks, not user playtesting or independent user debugging.

# Dynamic item checks without desktop control — October 20, 2026

## Scope and evidence boundary

This pass adds a finite state comparison against the supplied original source.
It does not use keyboard, mouse, controller, browser, screenshots, or a visible
game window. The original project remains read-only; its oracle script is copied
only into an isolated clone by the integration owner.

The source oracle invokes actual `Player` timer, movement and consumable callbacks
and `Main.check_half_health_stats`. It supplies the narrow dependencies needed
by these methods without running their scenes' `_ready` methods. The Unity
exporter uses `RunRules`. This tests callback state calculations; it does not
establish scene wiring, wall-clock timer scheduling, collision delivery, audio,
rendering, physical input, or ordinary survival balance.

## Finite inventory

The shared generator is `scripts/headless_dynamic_items_cases.py`; the original
exporter is `scripts/headless_dynamic_items_reference.gd`; the Unity exporter is
`Assets/Scrapshift/Editor/HeadlessDynamicItemsOracle.cs`. Comparison requires equal
case sets, equal snapshot counts, and all 16 primary stats at every snapshot.

Final generated matrix: **53 cases / 397 snapshots / 6,352 scalar comparisons**.

| Family | Cases and state boundaries |
|---|---|
| Five-second temporary gains | Wisdom and Medikit separately, duplicate copies and mixed copies; seconds 0, 1, 4, 5, 9, 10, 14 and 15; Well-Rounded, Doctor and Renegade gain contexts |
| Standing bonuses and links | Barricade, Chameleon and Statue individually and together, duplicate mixture, plus Hoodie linked-stat interaction; repeated standing/moving/no-change inputs in three character contexts |
| Full-health consumable gains | Extra Stomach with one, two and four forced copies; 12 pre-healing full/injured contacts for each; per-entry cap and eligible HP checked by source callback |
| Moving bonuses | Streamer alone, with standing items and with Hoodie; five alternating input states |
| Half-health thresholds | Golem alone, with Acid, and with standing/attack-speed items; exact floor/ceil thresholds, below threshold, return to full; includes even and odd effective maximum HP |
| Ordinary acquisition/removal lifecycle | Doctor with Wisdom, Medikit or Barricade: timed/standing activation, wave end, remove in shop, reacquire, next wave, fresh five-second activation. Source uses actual add/remove effects, TempStats cleanup and Main's Harvesting growth callback; Unity uses public wave/shop transitions |
| Terminal Golem states | Low HP followed by lethal defeat or staged final-wave victory; source executes the actual TempStats reset operations used by Player.die/end_run (not full scene methods); Unity uses real model damage and ordinary final-wave settlement |

Some duplicate fixtures deliberately exceed ordinary catalog acquisition caps
(Wisdom and Extra Stomach are capped at one). They check effect-entry handling
under forced loadouts and must not be described as legal normal shop purchases.
Earlier cap/eligibility tests remain the evidence for ordinary acquisition limits.

## Controlled proc checks

`HeadlessDynamicProcTests` adds 14 focused NUnit cases:

- All eight success/failure combinations for **three legal Hunting Trophy
  copies**, using the source's inclusive 33% boundary. Each entry rolls
  independently; payouts are direct currency with unchanged XP, bank and HP.
- A noncritical kill consumes no trophy random draws and pays nothing.
- Adrenaline rolls just below, exactly at and above 50%, with Lich healing
  damage counted separately. The original explicitly uses a strict `<` for
  this effect. Successful healing emits one Lich attack.
- Golem retains half-health bonuses while combat is paused, clears them on
  lethal defeat, and clears them after the ordinary 20th-wave settlement. The
  model victory test advances all 20 waves through public run transitions.

The test replaces only a constructed test run's `Random` field after setup.
Production RNG APIs and normal player profiles are unchanged. The values are
source-derived boundary cases, not a statistical claim that Unity and Godot
produce identical random sequences.

## Source checks and rejected mismatch hypotheses

- `player.gd:1084–1102` advances a shared integer second counter; matching
  interval entries accrue together. Removing timed accrued bonuses on ordinary
  damage would be wrong unless the source effect explicitly resets on hit.
- `player.gd:215–268` uses movement **input**, so pushing against a boundary
  still counts as moving. Repeated identical input must not accumulate standing
  bonuses.
- `main.gd:1622–1641` compares health with floating-point `max / 2.0`; an odd
  maximum must not use an integer half threshold.
- `player.gd:918–938` samples Extra Stomach eligibility before consumable healing
  and keeps a cap counter per effect entry; `RunData.reset_wave_caches` resets
  those counters.
- `unit.gd:367–373` routes Hunting Trophy through `Utils.get_chance_success`,
  which accepts the positive probability endpoint. Adrenaline instead uses
  `randf() < chance` in `player.gd:460–466`; unifying these would introduce a
  boundary mismatch.
- `main.handle_stat_damages` rejects `randf() >= chance`, so the existing strict
  comparison for Lich/Cyberball random-damage triggers is correct. It should
  not be changed to the trophy's inclusive comparison.

## Confirmed mismatch: Golem bonuses recreated after terminal reset

The follow-up source audit confirmed one production issue after the first
matching 51-case matrix. `Player.die` calls `TempStats.reset_player` at line 650;
`ui/menus/run/end_run.gd:14–15` resets temporary/linked stats before populating
the final stats panel. Scrapshift cleared its temporary-stat dictionary but
then computed Golem's below-half-health bonus again from HP 0 in `Lost`, or
remaining low HP in `Won`. That could leave +40 Attack Speed and +20 Speed in
the final stat readout.

`ConditionalEffects.ConditionalStat` now excludes `Lost` and `Won` for this
one conditional family. Combat pause and existing nonterminal behavior are
preserved. Two source-reset oracle cases and two public-flow NUnit cases cover
the correction. This is a source-confirmed terminal-state bug, not a new
claim about active combat or an observation attributed to the user.

## Execution status

Focused attempt three passed **24/24 tests** in **1.3977808 seconds**; all
**12 `HeadlessDynamicProcTests` cases passed**. Receipt:
`artifacts/headless-completion-2026-10-20/focused-attempt-3.xml`.
The earlier failed starter fixture is preserved in attempt two.

Before the terminal-state fix, dynamic attempt four compared **51/51 cases, 393 snapshots and 6,288 scalar
values with zero export errors and zero differences**. Source runner exited
zero. Read receipts:

- `artifacts/headless-completion-2026-10-20/dynamic-items-attempt-4/comparison.json`
- `artifacts/headless-completion-2026-10-20/dynamic-items-attempt-4/process.json`

For attempt four, Unity's unchanged attempt-three output was reused after correcting only the
source fixture's omitted Harvesting callback. The final comparison records the
hashes of its case file and both result files. That initial matrix did not
include terminal states; the subsequent Golem source finding extends it.

Expanded **attempt five passed 53/53 cases, 397 snapshots and 6,352 scalar
values, with zero export errors and zero differences**. Its source process
exited zero. Receipts:

- `artifacts/headless-completion-2026-10-20/dynamic-items-attempt-5/comparison.json`
- `artifacts/headless-completion-2026-10-20/dynamic-items-attempt-5/process.json`

The two terminal source samples execute the exact temporary-stat reset
operations from the source death/end-screen paths. They do not execute the
full original player death scene or end-screen `_ready` UI method. The Unity
samples use real model damage and final-wave settlement. Scene wiring and
visual final-stat acceptance remain interactive checks. The new Golem NUnit
cases require their fresh integration XML before being marked passed.

The first source attempt exposed a fixture using the abbreviated `hoodie`
instead of the actual imported/reference ID `retromations_hoodie`. The case
generator now uses the full ID. Doctor's restricted starter is explicitly
`medical_gun` on both sides. These fixture errors are not production bugs.
The source exporter now explicitly validates character/item/weapon
IDs and has a 60-second watchdog so an unexpected callback error cannot leave
the probe running indefinitely. Preserve the failed partial output and log;
it is not gameplay mismatch evidence.

Attempt three exported all 51 cases but found 15 Harvesting differences only
in the lifecycle cases: the source fixture omitted the real Harvesting timer
callback. Doctor grants 5 Harvesting (the global default is zero). The fixture
now invokes `Main._on_HarvestingTimer_timeout` after temporary-stat cleanup,
matching the actual source sequence. Preserve this failed comparison as a
fixture-context correction, not a production gameplay defect.

## Interview rehearsal and attribution

The user requested exact behavior and the restriction against computer control.
Codex investigated the supplied source and authored these fixtures. A useful
rehearsal is to explain why a positive 33% trophy boundary is inclusive while
the 50% Adrenaline boundary is strict, then trace one test to its source method.
Do not present the assistant-run source investigation as independently performed
personal work, or a callback oracle as physical playtesting.

Another rehearsal is to explain how a rule computed from HP 0 can recreate a
bonus after a dictionary reset. Trace the reset in the original, the terminal
phase guard in Scrapshift, and the test that preserves the valid paused-combat
bonus while removing the invalid final-state bonus. Codex performed this source
investigation and authored the implementation/tests; the user directed parity
and the test constraints.

Final focused receipt: `artifacts/headless-completion-2026-10-20/focused-attempt-4.xml` reports26/26 Passed, zero failures/skips, exit0,1.4245843 seconds:14 item/proc cases and12 shop cases.

Integrated final receipt: `artifacts/headless-completion-2026-10-20/suite-attempt-2.xml` reports1226/1226 Passed, zero failures/skips, process exit0,68.7548356 seconds. This includes the Golem fix and all14 new item/proc regressions.

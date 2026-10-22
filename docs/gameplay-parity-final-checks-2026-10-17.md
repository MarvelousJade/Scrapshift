## Latest native checkpoint — R14, October 17

R14 is the latest unpacked build (`Builds/GameplayParity20261017R14/LaunchGame.cmd`), zero build errors/warnings. Only the opt-in campaign driver changed from R13: it now uses the normal60Hz combat clock. Its normal-speed one-wave preflight Passed, exit0, reached Shop after20.0165 simulated seconds. Both fresh captures were inspected. It did not run a full campaign.

Carried input-matched evidence:172 focused regressions passed; R13 support43/43, economy and virtual navigation17 snapshots passed; R12 enemy53/eight slash probes and combat passed. Requested basic Xbox controls were confirmed by the user and logged separately. Unlock-pool timing and support projectile behavior fixes remain intact. No whole suite or full20-wave comparison ran. See [latest evidence and attribution](native-gameplay-r14-2026-10-17.md). Earlier R13 notes saying the campaign driver still needs a cadence correction are superseded by R14.

## Latest native checkpoint — R13, October 17

Fixed active-run unlock-pool refresh and support projectile animation/identity/offset/current-age presentation. R12 selected regressions passed172/172 (zero failures/skips); R13 only changes two native fixture files, with identical production/test inputs. R13 build succeeded with zero errors/warnings. Actual R13 native economy Passed, support43/43 Passed, virtual controller navigation Passed17 snapshots, all process exits0. R12 combat and enemy53/eight-slash-probe passes carry unchanged relevant source. The user accepted requested basic physical Xbox controls; B-not-opening-wave-pause is closed as reference behavior. No new broad hardware support claim.

The whole suite and full campaign have not run. Before a new campaign comparison, its older variable-step fixture path needs adaptation/checking against current60Hz normal gameplay. See [current native checkpoint](native-gameplay-r13-2026-10-17.md) for manifests, preserved initial failures, reviewed screenshots and limits. Older pending statements below are historical checkpoints. Codex performed implementation/testing; the user supplied direction/reference and actual basic Xbox feedback.

## Superseding native checkpoint — R11, October 17

A current build and native fixtures have now run; the earlier stop/pending descriptions below record the preceding checkpoint. R11 build has zero errors/warnings. R10 virtual native navigation Passed/17 snapshots, weapons211 and enemies53 passed; R11 support39 passed. Ordinary native startup shows30 materials. Repaired-copy keyboard menu/start/pause actions were observed after the user resumed Computer Use; movement remains unverified. Physical Xbox actions and full-run comparison remain pending. See [current native report](native-gameplay-r11-2026-10-17.md) for exact build input continuity, fixture repairs, captures and limits.

# Remaining native gameplay checks - October 17, 2026

Branch: `codex/gameplay-parity`. Supplied original remains read-only at `E:/UnityProjects/brotato-(3.x)`.

## New findings and corrections

- Controls: combine opposing keyboard/D-pad actions before selecting analog movement; use horizontal stick navigation priority and controller Back on release. The active-device aiming path was already filtered; the extra helper is preventive and the logger now reflects that ownership.
- Turrets: starting windup and its later shoot callback have different eligibility. A lost target does not cancel an already-started shot; body overlap builds the target pool, raw integer pixel distance gates new windup, and selected squared distances truncate as in the supplied source.
- Bounces and Lightning: random ordinary-enemy targeting excludes neutral trees, elites and bosses and uses raw origins.
- Initial timing work clipped delayed sweeps; actual native source-scene evidence then showed that the sweep itself can invent grazing hits at the reachable speed cap. The next correction uses a 60 Hz combat clock and discrete contact poses for projectile, slow-zone, melee and support contact checks, with delayed collision activation accounted for separately. This correction passed the final selected524-case run. Melee audio remains an event at recoil completion even when no sampled hitbox pose overlaps.

## Actual evidence and current stop point

- Source continuation passed **514/514 selected EditMode checks**, prior to the discrete-contact correction. It was not a whole suite.
- R9 review build succeeded: zero errors/warnings. Its visible native virtual-controller navigation fixture passed (process exit zero), including immediate A selection, combat movement and pause/resume. The hidden-window attempt failed before its first page because UI focus/paint was unavailable; both are retained. These are in-process Gamepad events, not physical Xbox acceptance.
- Root opened the user's supplied Godot project and used mouse selection to start Well-Rounded/Pistol/Danger0. The visible start had one weapon, 15 HP and a 20-second first wave. Idle play lost at nine seconds remaining; no movement or full run was tested. Start/idle-loss screenshots are retained. Root stopped that debugging session and left the editor open.
- User's latest correction: only mouse clicks work in that original run; keyboard, including Ctrl, does not. Source has35 blank keyboard bindings. Original files were not repaired or edited. The original game itself wrote its normal local runtime save/checkpoint while being played; this was not an isolated original-game profile.
- The separate Godot contact fixture uses actual supplied projectile scenes and the detached Baby Alien Hurtbox with a custom receiver. Nine movement cases plus explicit delay-boundary observations expose discrete geometry and deferred activation. They do not exercise production enemy AI or damage ordering. See `native-contact-followup-2026-10-17.md`; startup logging/shutdown resource diagnostics are retained.

## Final selected validation

The final selection passed **524/524**, zero failures/skips; Unity exited zero. All **516** recorded source/settings/import hashes still match the isolated clone after execution. See `artifacts/gameplay-parity-final-checks-2026-10-17/focused-attempt-3.xml`, its log, `focused-summary.json`, `source-manifest.json` and `catalog-comparison.json`. Earlier514- and522-case runs are retained and predate the final currency change.

The ordinary selected-character start and preview now receive **30 materials**, matching `DebugService.starting_gold` and unconditional `RunData.set_player_count` assignment in the supplied source. Neutral rule fixtures can explicitly start at0. Actual native capture of this final purse is still pending; R9 predates it.

The isolated repaired reference copy is `E:/Codex Projects/Job Search Assistant/tmp/BrotatoKeyboardReview/LaunchKeyboardReview.cmd`. Temporary WASD/arrows/Enter/Escape/P bindings preserve all26 joypad events; the movement script is exact original, campaign fixture removed, and saves are redirected. It was launched, but native keyboard behavior was not observed before the user pressed physical Escape and stopped Computer Use. The window was left alone; no further UI input or launch followed that stop.

## Pending before a full run

Build a new review containing the final524-checked source, replacing intermediate R9; execute current support projectile/native control fixtures; inspect rendered captures and collect physical Xbox feedback where available. A whole suite and complete campaign remain held. Browser release evidence is historical October13, and does not validate the new source on WebGL.

## Attribution and rehearsal

The user supplied the task, reference project and direct keyboard/controller observations. Codex performed source inspection, runtime integration, fixture authoring and automated checks. Explain the difference between a mathematical sweep and a discrete physics contact, why event-time burn data remains separate, and why a passing virtual controller fixture does not prove hardware behavior. Do not present these as independently user-authored debugging stories.

The 60Hz clock separates rendering/input polling from combat simulation. Catch-up is capped at8 ticks after a long stall; that guard is not a claim of measured equivalence under engine overload. Simultaneous overlap ordering, source deferred callbacks and crowd response remain native comparison limits.

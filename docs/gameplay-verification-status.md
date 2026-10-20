# Gameplay verification status - October 20, 2026

Current Windows review: **R29**, branch `codex/gameplay-parity`, source checkpoint `1ff6bb1`. This checklist separates static comparisons, model tests, headless campaigns, native fixtures and human input. It does not claim universal reference parity. The user paused desktop control; no new interactive checks are authorized for this phase.

## Verified

| Area | Evidence | What it establishes |
| --- | --- | --- |
| Integrated regression suite | `artifacts/parity-remaining-2026-10-20/suite-attempt-4.xml` | **1,200/1,200 Passed**, zero failures/skips, process exit0,69.7817621 seconds. Includes corrected source registration and all115 unlock scenarios plus coverage guard. Earlier attempt3's stale turret fixture failure is preserved. |
| Static item combinations | [Item-pair oracle](item-pair-oracle-2026-10-20.md) | **22,155/22,155** baseline/single/distinct-pair/double-copy vectors match the original, zero exporter errors.209 imported items,16 primary stats, Well-Rounded/tier-I Pistol, one fixed acquisition order, full-health intermission. |
| Character/item static composition | [Character oracle](character-item-oracle-2026-10-20.md) | **10,500/10,500** vectors match, zero exporter errors:50 characters by baseline plus209 single items, first eligible starter and30 materials, no live Player TempStats. |
| Starting item semantics | [Starting-choice correction](starting-item-choices-2026-10-20.md) | Beast Master selects one of four pets and receives automatic Lootworm; each alternative has two items, zero weapons and Speed4.16 focused checks plus latest complete suite. |
| Character gains and support registration | [Item audit](items-parity-remaining-2026-10-20.md), [support correction](support-registration-parity-2026-10-20.md) | Linked output uses character gain; pet links count registrations; Wrench/Screwdriver/Pruner register structures. Corrected stat links, unlocks and support placement rules pass focused/full checks. Native placement remains pending. |
| Survival/death lifecycle | [Survival correction](survival-parity-remaining-2026-10-20.md) | R26 actual hostile contact, healing, periodic lethal damage,0.5s cleanup, unsuppressed unlock queue, virtual-A retry/OK/Continue;8 captures reviewed. Sick/Wounded/Golem boundaries pass model checks. |
| Optional optimized cleanup | [Cleanup evidence](optimized-cleanup-parity-2026-10-20.md) | R26 normal and lethal banking use entry-time per-pile common doubling without XP/healing/pickup effects; both native routes Passed/exit0,2 captures reviewed. |
| Shop alternatives/modal controls | [Shop evidence](shop-parity-remaining-2026-10-20.md) | R27 passed22 native checks, exit0;8 captures reviewed. Purchase/recycle/reroll/lock/special actions and exclusive weapon-modal Cancel are sampled; source/model coverage is broader. |
| Every imported unlock definition | Current suite and [unlock evidence](unlock-world-events-2026-10-20.md) |115 producer/save-reload/next-run scenarios plus guard pass. Corrected turret scenario rejects five live actors and checks registered threshold. These are not115 native playthroughs. |
| Actual world-event unlocks | R26 world-event fixture |23 native assertions cover100 fruit contacts,3 hostile projectiles,30 burn deaths and15 explosion kills, plus persistence/pools. Five target IDs; Rookie has separate native evidence.4 captures reviewed. |
| Ordinary-damage campaign | [Survival comparison](headless-survival-comparison-2026-10-20.md) | R29 Passed/exit0, natural defeat in wave8 after7 completed waves, no injected invulnerability; cleanup age0.500002 seconds. Original lost in wave9 after8 completed. Different random loadouts and sampling prevent a difficulty/formula equality claim. |
| Latest full campaign | [R29 checkpoint](gameplay-final-checkpoint-2026-10-20.md) | Both engines Won/20 waves/19 shops/exit0; unchanged18-check comparator Passed against current R29. All31 reference and40 R29 debits reconcile; final96-material credit verified. Headless, scripted, neutral six-weapon loadouts, invulnerability/mute; no UI/human claim. |
| Windows packaging/provenance | R29 build summary/manifests and `r29-input-verification.json` | Zero build errors/warnings,401761114 build bytes;596 source/settings/import inputs and159 packaged files hash-verified, catalog equal after line-ending normalization. Build success is not an interactive playtest. |

R29's full and ordinary-damage headless campaigns are complete; see the [final checkpoint](gameplay-final-checkpoint-2026-10-20.md). The read-only original's fresh20-wave reference remains frozen. Original campaign diagnostics and one scripted startup pause intervention are retained, not labelled error-free execution.

## Remaining or partially checked

| Priority | Gap | Required acceptance |
| --- | --- | --- |
| 1 | Latest native starting-item picker and support placement/unlock presentation | When desktop access resumes: select each Beast Master pet with keyboard/mouse and Xbox; verify preview/run/retry identity; observe grouped mines, Wandering Bot/player area, independent pet groups and registered turret threshold. |
| 1 | Fresh physical Xbox and ordinary focus | Earlier user-confirmed Xbox controls are historical. New device detection is not hardware verification. Check sticks, D-pad, A/B, aiming, Start pause/resume and focus on R29. |
| 1 | Dynamic item interactions beyond sampled families | Static matrices do not close timed/proc effects, movement/health thresholds, reverse acquisition/removal orders, larger combinations or item pairs under every character. Use controlled source-derived cases. |
| 1 | Every native special-shop alternative and unlock producer | Source/model carrier coverage is broader than native UI/event evidence.109 definitions lack dedicated native threshold acceptance in the current inventory; see the [finite family inventory](unlock-world-events-2026-10-20.md). |
| 1 | Human survival/balance and encounter breadth | Scripted ordinary-damage runs reach natural defeat; invulnerable20-wave runs check integration. Neither proves human survival balance or every character/danger/elite/boss/loadout. |
| 2 | Scroll-view controller target coordinates | Provisional observation only: record same-frame root/window/viewport coordinates before deciding whether a defect exists. No speculative transform change was made. |
| 2 | Original first-wave material accounting observation | One aggregate reference kill lacks a reconciled material in the sampled trace. No-loot removal/deferred registration are possible source paths, but the trace does not identify the cause. This is unresolved evidence, not a confirmed payout bug. |
| Separate release work | Current browser saves/audio/focus/fullscreen/performance | Windows/headless results do not validate WebGL. Browser checks are deferred with desktop control; publication remains separate. |

Confirmed source defects found in this pass have corrections and focused/model evidence. Latest native presentation remains unverified where noted. Generalized steal/duplicate/tier-reroll/stat-reroll handlers have synthetic tests but no imported carriers; they are not represented as naturally acquired gameplay.

## Review and attribution

Launch later: `Builds/GameplayParity20261020R29/LaunchGame.cmd` (unpacked local Windows build). No visible game is claimed to be open now. Keep its player folder together.

The user set scope, supplied reference and earlier physical-controller feedback. Codex agents performed source investigation, implementation, automated tests, scripted campaigns and historical capture review. Interview notes are rehearsal material until reproduced and understood; these checks do not establish independent user debugging or a fresh user-played campaign.

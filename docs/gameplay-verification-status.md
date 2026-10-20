# Gameplay verification status - October 20, 2026

Current Windows review: **R35**, branch `codex/gameplay-parity`, code commit `debe1cf`. Latest full20 comparison remains **R29** (source `1ff6bb1`). R35 fixes synchronous HP/deferred death timing and plain item explosion ownership. Source/model, packaged callbacks, accelerated integration, presentation and human evidence remain distinct; universal parity is not claimed. Desktop access is allowed again. Physical Xbox testing is deferred at the user's request; this pass used no desktop input. See [current integration](death-item-integration-2026-10-20.md).

## Verified

| Area | Evidence | What it establishes |
| --- | --- | --- |
| Integrated regression suite | `artifacts/death-item-followup-2026-10-20/suite-attempt-2.xml` | **1,271/1,271 Passed**, zero failures/skips, exit0,64.1649722 seconds. Focused27/27 Passed. One historical global-kill timing assertion was corrected; failed attempt retained. |
| Acquisition/removal sequences | [Order comparison](item-acquisition-order-followup-2026-10-20.md) |416 cases /4576 snapshots /73216 primary-stat values matched, zero errors/differences. Snowball with208 other imported items under two characters; includes48 unavailable definitions as direct-grant model coverage. |
| Proc target and defense timing | [R35 integration](death-item-integration-2026-10-20.md) |192 packaged candidate rows Passed/exit0. Synchronous Lich/Wand sequences retain89/88 HP controls. Eight packaged death rows verify61-HP Giant Belt, pending membership, exact-once finalization and both player/enemy lethal orders. Original probe uses narrower Unit FIFO sinks, not full Player/Main cleanup. |
| Plain item explosion interactions | [R35 item-chain evidence](item-chain-followup-2026-10-20.md) |70 original payload cases, four packaged real-NPC burn/Greek Fire/life-steal cases,14/15 Fireworks boundaries with actual saved profiles, and Beard positive control Passed. No explosion travel/collision or arbitrary-loadout claim. |
| Dynamic item callbacks and terminal stats | [Dynamic item evidence](headless-items-2026-10-20.md) |53 cases/397 snapshots/6,352 scalar values match source callbacks with zero differences. Duplicate/mixed sampled timed families, movement/HP/consumable thresholds and acquisition/removal lifecycles. Fixed Golem below-half bonuses leaking into Lost/Won stats. Source terminal fixture invokes exact reset operations, not a full death scene. |
| Additional special shops | [Headless shop evidence](headless-shops-2026-10-20.md) |12 source-derived groups and14 R30 and R31 player receipts Passed/exit0. Price/debit, rawHP, capacity, reroll, locks, acquire/remove, Anvil pools and natural Candy chance/consequence. No new production mismatch; no current rendered/input acceptance. |
| Remaining unlock callback producers | [Headless unlock evidence](headless-unlocks-2026-10-20.md) |109 unique staged runtime scenarios Passed/exit0 on R30 and R31;654 true fields and109 independently read actual saved JSON profiles. With historical6, all115 mapped. Threshold context and clocks staged; active/fresh pool checks are separate synthetic snapshots, not continued organic scenes. |
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
| R35 actual simulation/loss integration | [R35 integration](death-item-integration-2026-10-20.md) |Accelerated headless ordinary-damage run Passed/exit0:8 waves completed, natural loss in wave9, net damage/healing142/115, cleanup age0.506142s. No injected invulnerability. One-wave smoke also Passed but took zero damage. Different timing/loadouts preclude difficulty comparison. |
| Latest full campaign | [R29 checkpoint](gameplay-final-checkpoint-2026-10-20.md) | Both engines Won/20 waves/19 shops/exit0; unchanged18-check comparator Passed against current R29. All31 reference and40 R29 debits reconcile; final96-material credit verified. Headless, scripted, neutral six-weapon loadouts, invulnerability/mute; no UI/human claim. |
| Windows packaging/provenance | `artifacts/death-item-followup-2026-10-20/r35-input-verification.json` |R35 zero build errors/warnings,401824411 bytes;626 source/settings/import inputs and159 player files hash-verified after execution; normalized catalog equality verified. No interactive playtest implied. |

R29's full and ordinary-damage headless campaigns are complete; see the [final checkpoint](gameplay-final-checkpoint-2026-10-20.md). The read-only original's fresh20-wave reference remains frozen. Original campaign diagnostics and one scripted startup pause intervention are retained, not labelled error-free execution.

## Remaining or partially checked

| Priority | Gap | Required acceptance |
| --- | --- | --- |
| 1 | Latest native starting-item picker and support placement/unlock presentation | Pending interactive acceptance: select each Beast Master pet with keyboard/mouse and Xbox; verify preview/run/retry identity; observe grouped mines, Wandering Bot/player area, independent pet groups and registered turret threshold. |
| Deferred | Fresh physical Xbox and ordinary focus | Earlier user-confirmed Xbox controls remain historical. User asks to skip fresh hardware testing if it needs their input. Virtual input cannot verify physical sticks/buttons; ordinary OS focus remains unchecked on R35. |
| 1 | Broader compound timing and arbitrary item interactions | Immediate HP, finite lethal-order and corpse-membership cases now pass source/Editor/packaged checks. Full original Player/Main cleanup and arbitrary larger combinations/orders remain unverified; these finite matrices are not exhaustive. |
| 1 | Every native special-shop alternative and unlock producer | Source/model carrier coverage is broader than native UI/event evidence.All115 now have mapped producer evidence:109 staged callback scenarios plus6 historical native producers. Actual Update/physics, organic thresholds and current rendered/UI routes are not established per definition; see the [finite family inventory](unlock-world-events-2026-10-20.md). |
| 1 | Human survival/balance and encounter breadth | Scripted ordinary-damage runs reach natural defeat; invulnerable20-wave runs check integration. Neither proves human survival balance or every character/danger/elite/boss/loadout. |
| 2 | Scroll-view controller target coordinates | Provisional observation only: record same-frame root/window/viewport coordinates before deciding whether a defect exists. No speculative transform change was made. |
| 2 | Historical original material cause unassigned | Kill-to-material equality is invalid because global Evil Mobs can yield zero or steal drops; inspected Scrapshift path matches. Four injected source cleanup-boundary cases demonstrate a deferred-registration race, not the old trace cause. No confirmed Scrapshift payout defect. See [follow-up](material-reconciliation-followup-2026-10-20.md). |
| Separate release work | Current browser saves/audio/focus/fullscreen/performance | Windows/headless results do not validate WebGL. Current browser checks remain separate release work; publication remains separate. |

Confirmed source defects found in this pass have corrections and focused/model evidence. Latest native presentation remains unverified where noted. Generalized steal/duplicate/tier-reroll/stat-reroll handlers have synthetic tests but no imported carriers; they are not represented as naturally acquired gameplay.

## Review and attribution

Launch later: `Builds/GameplayParity20261020R35/LaunchGame.cmd` (unpacked local Windows build). No visible game is claimed to be open now. Keep its player folder together.

The user set scope, supplied reference and earlier physical-controller feedback. Codex agents performed source investigation, implementation, automated tests, scripted campaigns and historical capture review. Interview notes are rehearsal material until reproduced and understood; these checks do not establish independent user debugging or a fresh user-played campaign.

## Preserved R31 headless completion checkpoint

See [R31 completion report](headless-completion-2026-10-20.md). The finite remaining callback/shop/unlock inventory has executed receipts. R29 campaigns are preserved rather than relabelled as R31: the only gameplay change is terminal Golem stats, and the neutral campaign character has no such conditional. No new full20 run or current presentation/hardware acceptance is claimed.

## Latest item/material checkpoint

See [R34 follow-up](item-material-followup-2026-10-20.md). R31/R29 receipts above retain their original scope; R29 campaigns were not rerun on the new combat rules. Material counter assumptions were corrected without changing payouts. No desktop input was used.

## Latest death/item checkpoint

See [R35 integration](death-item-integration-2026-10-20.md). Six packaged processes Passed/exit0, including109 unlock producer/save/reload/next-run scenarios and the accelerated natural-defeat run. R29 full20 comparison remains historical; no fresh hardware, UI, visual or audible acceptance follows from these receipts.

## October 17 continuation ownership and integration

Current branch remains `codex/gameplay-parity`. This continuation reused disjoint runtime ownership: support agent owned pet/structure/profile files and SupportNextParityTests; weapon agent owned weapon callbacks/burn rules and WeaponNextParityTests; enemy agent owned EnemyIntegrationParityTests/report only; root owned ArenaGame, ReferenceCombat, EnemySpecials, validation and shared docs. Agents performed source checks without launching Unity. Root integrates the combined change and runs selected checks once frozen.

See `gameplay-parity-systems-2026-10-17.md` for current evidence. Historical suite/build totals below predate this continuation. No full suite or full-run comparison is part of the October 17 validation slice.

# Parallel gameplay parity workplan — October 15, 2026

Target: the supplied read-only `E:/UnityProjects/brotato-(3.x)` source, single-player normal runs, against Scrapshift on `codex/gameplay-parity`. Follow `AGENTS.md`. The existing checkout has unrelated uncommitted work; preserve it. A passing catalog audit or EditMode suite does not establish full gameplay parity.

## Current continuation after R6 acceptance

The user confirmed the movement/button-like sound is gone in R6, then requested continued agent fixes with bug and interview documentation kept current. Preserve the accepted audio import correction. The following ownership supersedes the older track table below for this continuation:

| Track | Current ownership | Report |
| --- | --- | --- |
| Support actors | `SupportBehavior.cs`, `StructureSpec.cs`, `CyberneticPets.cs`; bounded `SurvivalRules.cs` edits only if source proves necessary; new support tests | `support-remaining-parity-2026-10-15.md` |
| Combat | `ReferenceCombat.cs`, `ReferenceWeaponBehavior.cs`, `WeaponReference.cs`; new combat tests | `combat-remaining-parity-2026-10-15.md` |
| Economy/progression | `RunRules.cs`, `ItemDesign.cs`, `Progression.cs`, `ProgressionRuntime.cs`; new economy tests | `economy-remaining-parity-2026-10-15.md` |
| Integration/enemies | Root owns `ExtendedEncounters.cs`, `EnemyReferenceBehavior.cs`, shared hooks, validation/build and shared documentation | `enemy-remaining-parity-2026-10-15.md` |

All findings require actual reference script/resource inspection. Candidate mismatches may prove correct: Doc Moth regeneration stacks additively while life-steal doubling is a boolean in the supplied source. Do not change correct behavior to satisfy an earlier hypothesis. Tests and native results remain pending until their outputs are inspected.

## Earlier independent tasks

| Track | Owned Unity files | Reference questions and deliverable |
| --- | --- | --- |
| A — economy and progression | `RunRules.cs`, `ItemDesign.cs`, `LootRules.cs`, `CharacterMechanics.cs`, `UpgradeOffer.cs` and matching tests | Starting loadouts, XP and levels, shop pools and weights, prices, rerolls, locked offers, crates and rewards. Compare formulas and edge cases with `singletons/run_data.gd` and `singletons/item_service.gd`. |
| B — combat and weapons | `ReferenceCombat.cs`, `ReferenceWeaponBehavior.cs`, `WeaponReference.cs`, `StarterWeaponEffects.cs`, `CombatGeometry.cs` and matching tests | Damage order and rounding, critical hits, attack timing, targeting, projectile paths, pierce, bounce, statuses and weapon-specific mechanics. Inspect the corresponding weapon resources and player/projectile scripts. |
| C — enemies and waves | `EnemyReferenceBehavior.cs`, `EnemyAttack.cs`, `EnemyBehavior.cs`, `SpawnDirector.cs`, `WaveSelection.cs`, `ExtendedEncounters.cs`, encounter-specific files and matching tests | Wave schedule, placement, enemy movement and attacks, elites and bosses, telegraphs, and end-of-wave behavior. Compare the relevant enemy scenes/scripts and spawn services. |

Each agent should edit only its owned files. If a fix crosses a boundary, record the exact change needed and let the integrator apply it after the parallel pass. In particular, only the integrator edits `GameUI.cs`, `ArenaGame.cs`, `GameSettings.cs`, shared docs and `AGENTS.md`. Agents must not launch Unity in the main checkout or the shared validation clone.

## Test slices for the active tracks

| Track | Deterministic checks | Native comparison after integration |
| --- | --- | --- |
| A | Zero/one/two starting weapons according to character effects; level threshold boundaries; locked offer price across waves; free and paid reroll ordering; empty shop; full inventory; rarity and item pool exclusions; crate choice and recycle values. | Complete selection, wave, reward, shop and next-wave sequence with visible currency, XP and inventory. |
| B | Same source-derived damage case with and without critical/armor; cooldown and manual target selection; melee contact window; projectile penetration/bounce/return; on-hit status order and stacking. | Record representative melee, ranged, area and unusual weapons at normal speed, including visible projectile and impact behavior. |
| C | Enemy group eligibility for each wave and danger level; queue/cap boundaries; spawn clearance; attack preparation/dash/recovery timing; elite/boss phase transitions and attack-specific telegraphs. | Play early, milestone, elite and boss waves at normal speed and compare timings, counts and attacks. |

For random systems, verify pool composition, weights and statistical distributions with fixed local seeds. Equal numeric seeds across Godot and Unity do not guarantee the same run because their generators and draw sequences differ.

## Ready follow-on tasks

October 15 status: the item/support/control/unlock tracks below have received parallel source audits and implementation. The latest complete suite checkpoint is 503/503 passing, including collector ownership/geometry and entry rules; native controls and controlled source/Unity runs are documented in `gameplay-parity-controls-stacks-2026-10-15.md` and `full-run-comparison-2026-10-15.md`. This table remains an ownership/reference checklist, not a claim that every native behavior or input device was verified.

These can run as separate agent tasks when a slot opens. Give each agent a unique set of files before any edits.

| Track | Suggested ownership | Minimum comparison |
| --- | --- | --- |
| D — items and characters | `SpecialItemEffects.cs`, `ConditionalEffects.cs`, `TriggeredImplants.cs`, `CharacterProfile.cs`, `ReferenceCatalog.cs`, related tests | Per-character modifiers, item triggers, stacking, caps, eligibility and unlock conditions. Catalog field equality is only the starting point. |
| E — supports and pickups | `SupportBehavior.cs`, `StructureSpec.cs`, `CyberneticPets.cs`, `ReferenceLoot.cs`, `SurvivalRules.cs`, related tests | Each pet/turret/mine/consumable: spawn count, movement, target choice, range, cooldown, collision, healing and drops. Include Catling, Ratzilla and landmine entry/exit behavior. |
| F — controls and run flow | `CombatInput.cs`, `GameSettings.cs`, `GameUI.cs`, `CharacterSelection.cs`, `ShopUI.cs`, related tests | Start/weapon selection, manual aiming versus firing, pause/resume in every run phase, settings effects, keyboard/mouse and actual controller navigation. Requires native play checks in addition to source tests. |
| G — unlocks and records | `CodexUI.cs`, `AchievementToast.cs`, progression/save code, related tests | Achievement triggers, reward disclosure, unlock eligibility, record persistence and its effect on available character/item pools. |

## Required handoff from each agent

1. Give each finding a reference file and function or resource, the corresponding Scrapshift file and function, and the observed difference. Identify behavior that already matches as well as changed behavior.
2. Add a focused test for any deterministic rule changed. Include boundary cases and the expected values derived from the reference. Do not add tests that simply restate a hard-coded implementation table.
3. Report every file changed, tests added, and unresolved cross-track dependencies. Distinguish inspected source, automated results, native playtesting and assumptions.
4. Do not claim complete parity for the track without exercising runtime behavior. Engine physics, RNG draw order, timing and input may need a native comparison even when formulas match.

## Integration gate (single owner)

After each agent finishes, review its diff against its assigned files and the dirty baseline. Apply cross-track corrections serially. Synchronize only intended changed source/test files into `E:/Codex Projects/Job Search Assistant/tmp/ScrapshiftParity` and verify hashes. Run the complete Unity EditMode suite, inspect its XML and log, run the catalog audit, then build a Windows player from that exact synchronized copy. Use native play sessions to compare loadout, combat, wave transitions, shop/rewards, pause/options and controller operation. Record the build path and evidence. Any untested or differing mechanic stays open in the parity report.

# Gameplay verification status — October 17, 2026

Current Windows checkpoint: **R25**, branch `codex/gameplay-parity`. This records what the evidence establishes; it does not claim universal reference parity.

## Verified

| Area | Evidence | What it establishes |
| --- | --- | --- |
| Integrated regression suite | `artifacts/gameplay-priority-followup-2026-10-17/suite-attempt-1.xml` | **1,181/1,181 passed**, no failures or skips; process exit 0. Includes the earlier regression tests and the new cases below. |
| Item combinations | Combined-effect tests and R25 `items-attempt-2` | Representative acquire/remove, linked stats, healing triggers and burn combinations. Actual native simulation measured global, mixed-cooldown and weapon-native burn payloads/cadence. Forced burn application does not test hit/proc probability. |
| Ordinary survival | Survival lifecycle tests and R25 `survival-attempt-2` | Real hostile projectile contact, armor, hit immunity, regeneration, Torture cleanup healing, lethal contact and stopping pickups on the lethal frame. No continuous invulnerability in this survival probe. |
| Defeat and retry | Same survival fixture plus checkpoint tests | Half-second acknowledgment delay, paused cleanup clock, raw-material banking without XP/harvesting, dead-player fruit preserved, default OK and optional Retry/End run UI. Actual virtual A confirmed retry and restored the wave-start state. |
| Special shops | Imported-carrier source audit, existing economy tests and new HP boundary tests | Reviewed all 6 item and 8 character carriers across 12 relevant shop effect families, including prices, free rerolls, guarantees, recycling, Anvil, Hourglass and Candy Bag. The model checks cover alternative actions; native coverage is narrower. |
| Health shop presentation | R25 `shop-attempt-2`, fresh screenshot review | Last-HP purchase leaves raw budget 0; another purchase is rejected. Material header/reroll icons, heart purchase prices and raw Max HP stat display are correct in the staged Demon shop. |
| Every imported unlock definition | `unlocks-attempt-1.xml`, dispatch coverage inventory | **115 definition scenarios plus one inventory guard passed**. Each uses a production trigger/producer, save/reload and next-run reward eligibility. Some world events enter through rule counter adapters rather than native collisions. |
| Unlock lifecycle across runs | New model tests and R25 shop/unlock fixture | Global Rookie/item rewards and lifetime progress survive death/retry; current-run item eligibility remains frozen; a fresh run admits the saved reward. The visible next-run offer is staged, not an RNG outcome. |
| Windows build and provenance | Build attempt 3 summary and `final-verification.json` | Zero build errors/warnings; 568 intended source/settings/import inputs match the isolated clone; 160 player files verified. Three native fixtures passed/exit 0, with 12 fresh screenshots reviewed. |

The source-backed defects found in this pass are fixed within these checks. No additional confirmed mismatch is recorded as unresolved in these three tracks. An unchecked case is still a coverage gap, not proof that it matches.

## Still missing or only partly checked

| Priority | Gap | Next useful check |
| --- | --- | --- |
| 1 | Exhaustive item pairs, larger combinations, character modifiers and acquisition/removal orders | Expand by effect interaction families, especially shared triggers, threshold crossings and competing cooldowns. Current combined examples do not cover every loadout. |
| 1 | Ordinary survival across a complete campaign | Play/compare a full non-invulnerable run with damage, healing, lethal effects and defeat/retry choices. The R25 native probe is staged; no human survival campaign is established. |
| 1 | Every special-shop choice through native UI over subsequent runs | Exercise real purchase/recycle/reroll/lock/cancel alternatives for imported carriers, including randomized target choices and encounter consequences. Source/model coverage is broader than native coverage. |
| 1 | Every unlock's world-event producer in native play | Verify actual collision, burning, explosion, consumable and other event producers. All 115 dispatch/persistence cases passed, but they are not 115 native playthroughs. |
| 2 | Broader survival boundaries | Expand character/lethal-effect combinations, optional optimized-cleanup material boosts and interaction with unsuppressed unlock modals. Builder-only conversion has no imported character carrier and is not claimed as playable coverage. |
| 2 | Fresh full 20-wave comparison on R25 | R23 completed the earlier bounded comparison, but R25 has not run a new campaign. R23 used neutral stats, six tier-II weapons, scripted movement, invulnerability and mute. Its 18/18 comparison checks are historical evidence, not survival validation. |
| 2 | Physical controller and ordinary focus behavior on R25 | Earlier user-confirmed Xbox checks remain historical. R25 retry used virtual input with fixture-only IgnoreFocus; it does not revalidate physical hardware or ordinary focus handling. |
| Separate release work | Current browser build, crowded-wave performance and release readiness | Earlier browser evidence predates these fixes. Rebuild and recheck browser saves/audio/focus/fullscreen and representative performance before treating R25 as web-ready. Publication and third-party licensing are separate. |

Generalized steal/duplicate/tier-reroll/stat-reroll handlers have synthetic tests but no imported item/character carriers. They are not presented as naturally acquired content. Full parity across every character, danger, enemy encounter and loadout remains unproven.

## Evidence and attribution

- [Priority implementation report](gameplay-priority-followup-2026-10-17.md)
- [Item combinations](items-combinations-followup-2026-10-17.md), [survival](survival-followup-2026-10-17.md), [shops and unlocks](shop-unlocks-followup-2026-10-17.md)
- [Earlier R23 full comparison](full-verification-2026-10-17.md)
- Current launch: `Builds/GameplayParity20261017R25/LaunchGame.cmd` (local unpacked build; binaries are not part of the source commit).

The user set the priorities. Codex performed the source investigation, implementation and automated/native fixture checks. These results do not establish independent user debugging or a newly completed human playthrough. Bug and interview notes link this same status to avoid stronger claims.

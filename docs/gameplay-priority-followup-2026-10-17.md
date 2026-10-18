# Priority gameplay follow-up - October17, 2026

Baseline: committed `affceae`, branch `codex/gameplay-parity`, verified R23 player. The user prioritizes these three coverage areas; passing R23 integration checks are separate historical evidence.

| Track | Owner/files | Acceptance |
| --- | --- | --- |
| Item combinations and stacks | Items agent: ConditionalEffects, SpecialItemEffects, TriggeredImplants, ItemLifecycle, ImplantAttacks and unique combination tests | Compare actual trigger formulas, additive/boolean stacking, caps, reset and acquire/remove interactions with supplied source; test combined behavior, not only inventory counts |
| Survival | Combat agent: SurvivalRules, native survival fixture and unique lifecycle tests; root shared damage/loss integration | Ordinary damage/protection/armor, healing/regen/lifesteal, lethal hit and source defeat/cleanup flow; native probes without continuous invulnerability |
| Shop and unlocks | Economy agent: ShopSpecialEffects, Progression, ProgressionRuntime and unique tests | Special-shop effects and purchase/recycle/reroll/lock/steal/health/cancel alternatives; challenge triggers/save reload/next-run availability |

Root owns RunRules, ArenaGame, UI, shared docs, integration and all Unity/native process launches. Agents inspected the original read-only and recorded exact sources before fixing behavior. Completed results and limitations are below; the consolidated [verification and missing-work checklist](gameplay-verification-status.md) is the current coverage summary.

## Confirmed corrections

- Burn interval: restored the source Unit BurningTimer base0.5s and additive cooldown modifiers. Combined global/native burn damage and duration retain their distinct rules.
- Torture and cleanup: ordinary healing is permitted during wave cleanup, retaining dead/no-heal restrictions.
- Defeat lifecycle: added the source0.5s cleanup/confirmation gate, raw-material banking without XP/harvesting, preserved uncollected consumables and stopped later combat/pickups on the lethal frame. Pausing freezes cleanup.
- Optional retry: added the source gameplay option and explicit wave-start checkpoint. Retry restores run state without resetting global achievements or rolling back RNG; a fresh run resets retry history.
- Health shop: use raw truncated max-HP budget, separate from the combat minimum. Zero budget blocks repeated purchases. The header still shows materials for rerolls; purchase buttons use the HP icon and the stat panel shows raw max HP.
- Unlock snapshot: dying no longer refreshes current-run eligibility before the retry/end choice. Global progress persists while retry retains the frozen pool; a new run refreshes it.

## Evidence and scope

Focused checks passed45/45; the additional complete unlock-dispatch inventory passed116/116, including all115 imported definitions and a coverage guard. Every definition uses a production trigger/producer, save/reload and next-run reward eligibility. These are deterministic model/Editor checks, not115 native playthroughs.

R24 native item, survival and shop fixtures each Passed/exit0. Items measured three combined burn sequences; survival used real projectile damage, regeneration, lethal contact, pause/acknowledgment and virtual-A retry without continuous invulnerability; shop checked last-HP spending and saved unlocks across retry/new run. Fresh screenshot review caught the health-shop icon/header/stat mismatch after the first model checks passed. The final R25 build completed with zero errors/warnings (401713034 bytes); full suite1181/1181 Passed, zero failures/skips, exit0. All three native fixtures repeated on R25 Passed/exit0, with12 fresh captures reviewed. The corrected Demon screen shows17 materials, material reroll currency, heart purchase prices and raw Max HP0; another HP purchase is disabled. Final568-input and160-player-file hash checks passed; generated catalog equality was retained after line-ending normalization. Launch `Builds/GameplayParity20261017R25/LaunchGame.cmd`; receipt: `artifacts/gameplay-priority-followup-2026-10-17/final-verification.json`.

Detailed source/evidence/limits: [items](items-combinations-followup-2026-10-17.md), [survival](survival-followup-2026-10-17.md), [shops and unlocks](shop-unlocks-followup-2026-10-17.md). Artifacts: `artifacts/gameplay-priority-followup-2026-10-17`. R23's20-wave campaign remains historical evidence; it is not relabeled as a survival run. Exhaustive pairwise item coverage and human survival balancing remain unverified. Codex performed implementation/testing under the user's direction; no independent user debugging is implied.

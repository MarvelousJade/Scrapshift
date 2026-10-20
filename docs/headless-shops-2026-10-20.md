# Headless special-shop acceptance — October 20, 2026

## Scope and evidence

The user authorized remaining source/code/headless checks and withheld desktop control. This pass inspects the supplied original read-only and exercises finite production `RunRules` actions with explicitly staged offers/inventory. It does not establish current displayed prices, hover/highlight geometry, popup input exclusion, physical controls, audible feedback or human gameplay.

`HeadlessShopAcceptance` contains 12 grouped scenarios. The EditMode wrapper and opt-in built-player runner use the same scenarios, allowing a finite action receipt to be checked with both the Editor catalog and the built catalog. Repeating those scenarios in a player is deployment evidence, not an independent second implementation of the rules. The runtime runner requires `-batchmode -nographics --headless-shops-review <output>` and marks captures unverified.

The player repeats the lock-transition scenario with both keep-lock preferences through isolated in-memory overrides and restores the prior setting. It records 14 receipts across the 12 scenario groups, including a separate actual Candy encounter-request path.

## Executed results

- [Focused attempt3](../artifacts/headless-completion-2026-10-20/focused-attempt-3.xml): **24/24 Passed**, zero failed/skipped, exit0 reported by the coordinator; duration 1.3977808 s. This includes **12 shop groups plus 12 separate proc tests**, not 24 shop cases.
- **R30 headless player:** [14/14 receipts Passed](../artifacts/headless-completion-2026-10-20/shops-r30-attempt-1/headless-shops-review.json), [process exit0](../artifacts/headless-completion-2026-10-20/shops-r30-attempt-1/process.json), `headless:true`, `isolated:true`, `capturesVerified:false`.
- Both keep-lock logic branches passed. Anvil's 96 seeded choices selected Pistol 31, Knife 31 and Wrench 34 times; max-tier exclusion, empty/max-only +2 Armor fallback, removal and first action on the next shop entry passed. These counts establish bounded target coverage, not probability equivalence.
- Imported Candy 10% yielded **26 successes/230 failures across 256 seeded cases**, with deterministic replay and removal checks. The separate production encounter path observed one failure and one success in two natural trials; the success emitted exactly one eligible `butcher` callback. It did not instantiate or move an enemy.

**No confirmed production shop mismatch was found in these finite scenarios.** The S1–S7 logic described below is checked; current UI/input and actor-physics acceptance remain pending. No broad campaign was required for these diagnostic-only additions.

## Source-derived acceptance

Reference root: `E:/UnityProjects/brotato-(3.x)`.

| Acceptance | Reference evidence | Finite scenario |
| --- | --- | --- |
| S1 — separate price modifiers | `singletons/item_service.gd:726` keeps weapon-discounted base fractional, adds inflation, applies item/specific factor, then truncates. Character resources supply their own factors. | Same wave5 coffee/pistol: Mutant52/30, Saver52/30, Entrepreneur26/15, Glutton43/25, Baby28/16. Source coffee base20 and pistol base10. Actual item and weapon debits; Arms Dealer tierIV Knife wave2 cost7; Fisherman Bait minimum1 while ordinary coffee35. |
| S2 — raw HP budget | `singletons/run_data.gd` currency helpers; `item_service.gd` integer cost, Demon resource `hp_shop`. | Raw1 pays cost1 once; raw0 rejects a staged HP-granting item. Purchasing leaves material balance unchanged; a subsequent paid reroll at rawHP0 debits materials only. Combat health minimum remains separate. Existing fractional/negative cases remain in `ShopUnlockFollowupParityTests`. |
| S3 — capacity | `ui/menus/shop/base_shop.gd` purchase/combine paths and `shop_item.gd` eligibility. | Full inventory matching duplicate spends once, combines into one tierII and preserves count; unmatched/max-tier and Ranger-ineligible melee copies reject without spending or removing the offer. Inspection cancellation remains interactive evidence. |
| S4 — rerolls | `base_shop.gd:406-487` rejects four locks before payment; consumes sold-out bonus before item freebies; increments paid count only on paid rolls. `:536-543` adds newly acquired Bunny charges immediately. | Two separately purchased Bunnies preserve prior paid index; sold-out bonus then Bunny then cost1/next2; four locked offers preserve free token, balance, offers and reroll count; an unaffordable paid attempt preserves the same receipt. |
| S5 — retained lock | `base_shop.gd:117-131,324-329,360` carries locked item identity and rolled wave, setting used-lock only on leaving with an active lock. | Lock then unlock does not mark used-lock; active locked offer survives actual Go/wave/shop transitions with same identity/price. Post-entry lock state follows the current keep-lock preference. |
| S6 — acquire/remove | `item_service.gd:726,764`; Coupon/Recycling Machine resources; `base_shop.gd:62-77` source Anvil candidates require upgrade and tier below maximum. | Coupon wave5 coffee35→33→35. Recycling Machine wave5 pistol recycle5→12→5 and one actual credit. Anvil96 seeds select only eligible PistolI/KnifeII/WrenchIII, retain max-tier SMG, preserve weapon count, stop after removal; max-only and empty pools yield+2 Armor; purchasing Anvil in an existing shop acts first at the next shop entry. This is bounded target coverage, not probability equality. |
| S7 — natural conditions | `base_shop.gd:334-349`; Hourglass resources disallow locking and replace on removal; `items/all/candy_bag/candy_bag_effect_1.tres` has10% next-elite chance. | Hourglass wave7→7→8, exactly one broken replacement; both intact/broken forms cannot lock. Candy256 seeds use imported10%, deterministic replay, both natural outcomes, and removal blocking subsequent branches. The player also runs actual `AddWaveEncounters` into an empty director and checks one eligible single-elite emission for success and none for failure, stopping after both natural outcomes within128 trials. This is request/schedule acceptance without actor spawning or physics. |

## Remaining interactive boundaries

- Verify displayed price/currency icons against the actual input debit in the current UI.
- Verify cancelled inspection and modal controls prevent background actions in the current build (preserve R27 prior evidence).
- Check both keep-lock preferences in the current visible build. The headless player checks both logic branches through in-memory overrides; visible controls and persistence are still pending.
- Check Candy's emitted encounter placement and targeting visibly. Headless request/director callback receipts do not prove actor spawning, physics, presentation or matching cross-engine random sequences.

## Interview and attribution notes

The user requested parallel parity work, meaningful commits and no computer control. Codex inspected source and authored the action scenarios. These are preventive boundary checks unless a failing scenario demonstrates a source-backed mismatch. Do not present them as bugs the user independently found or fixed. A useful rehearsal topic is why a free reroll must not advance the paid-price index, and why a sold-out bonus must be consumed before a purchased Bunny charge. Trace one receipt through `RunRules.Reroll` and the source `base_shop.gd` before explaining that implementation personally.

## Fixture corrections retained in evidence

- First compile attempt found a C# local-name collision in the new scenario harness; no checks ran. The harness was corrected.
- The first executed price scenario rejected Entrepreneur's purchase because its real wave-start effect discarded the initially staged purse. The test now funds the staged shop after starting the character through the immediate material-credit API, avoiding pickup XP and extra upgrades. This is a fixture setup correction, not a production economy bug. Preserve the failed XML alongside the corrected result.

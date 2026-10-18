## October 17 root validation result

See `gameplay-parity-systems-2026-10-17.md`. Evidence: `artifacts/gameplay-parity-systems-2026-10-17/focused-summary.json`, both XML/log attempts, input manifest and catalog comparison. The same selected checks passed 434/434 on attempt 2, zero failures/skips, Unity exit zero. All 508 recorded input hashes still match the isolated clone after execution. No whole suite, new build, full campaign or physical-controller retest was run. R8 remains the last delivered gameplay build and predates these changes. Earlier pending statements below reflect the handoff before root execution.

# Shop and unlock continuation — October 15, 2026

Inspected supplied read-only source at `E:/UnityProjects/brotato-(3.x)`. This pass preserves the preceding 213/213 focused checkpoint; the changes below require their own focused validation. No Unity, build, full suite or full campaign was launched by this agent.

## Confirmed corrections

### Discounted weapon prices preserve their fractional base

`singletons/item_service.gd:727` declares `var value_after_weapon_price = base_value`, an untyped Variant, then assigns the fractional weapon discount at line 732. The final price expression alone is cast to int at line 749. Scrapshift incorrectly cast the adjusted base before inflation. `RunRules.ReferencePrice` now retains that fraction through the final cast. Actual Arms Dealer uses -95% weapon price (`items/characters/arms_dealer/arms_dealer_effect_1.tres`). At wave 2 a base-50 weapon costs 5, and the actual tier-IV Knife with base 91 costs 7. This also corrects recycling's use of the weapon price. The prior base-11/+25%/wave-6 regression is corrected from 26/6 to price 28/recycle 7; ordinary item recycling remains 5.

Locked offers retain their original rolled wave, specific-item discounts remain additive with item price, and all prices retain the minimum of 1. No new discount or reroll formula was invented.

### Lifetime counted challenges wait for wave timeout

`ProgressData.increment_stat:1294` only increments the counter. `main._on_WaveTimer_timeout:1223` invokes `ChallengeService.check_counted_challenges:633-646` for enemy kills, collected material objects, trees and evil enemies. Scrapshift previously unlocked these immediately inside `Progression.Count`.

`Count` now accumulates these counters without completing their achievements. New `CheckCountedChallenges` runs through `RecordWaveEnd`. A successful `Finish` also invokes this idempotently, because Scrapshift's immediate final-boss adapter can bypass its ordinary wave sweep; losses do not invoke it. Thresholds remain cumulative across runs and completion/notifications remain unique.

Fruit Basket is different: `player.gd:944-955` checks its threshold immediately after a full-health fruit contact, only outside cleanup. Its immediate check is retained. Root adds the cleanup flag to the actual pickup path; the new regression proves cleanup fruit cannot unlock Fruit Basket while preserving Extra Stomach's permanent growth, whose source callback is outside that cleanup guard.

### Hourglass uses remaining seconds, not wave number

`ui/menus/pages/menu_confirm.gd:24-28` requires an active wave and strictly less than the challenge's 5 seconds remaining, then sets a flag. `main_menu.gd:386-388` completes the challenge on returning to the menu. Scrapshift had tested `Wave < 5` and described that incorrect requirement.

`CanUnlockHourglassOnReturn` now expresses the source condition, including an underlying paused combat phase and excluding a paused shop/reward phase. Root wires the actual return-to-menu action to this predicate. The requirement text now states less than five seconds remaining during a wave. Tests cover the strict boundary, pause ownership, shop rejection and eligibility on wave 8.

### Projectile-contact achievement counts contacts before damage outcome

`player.take_damage:416` calls `check_chal_jellyshield` before dodge, hit protection and the healing branch. `check_chal_jellyshield:546-551` checks for a Projectile parent, not HP loss. Healing turret bullets inherit `player_projectile.tscn` and `PlayerProjectile extends Projectile`, use Hitbox layer 16, and overlap the player's Hurtbox mask 20. Medical Gun's ordinary weapon bullets are not healing turret shots.

Root owns the actual projectile wiring/regressions: count eligible hostile and healing contacts independent of HP loss, preserve the source's disabled-hurtbox window, and avoid destroying passing projectiles during that disabled window. Source `disable_hurtbox` is deferred, so simultaneous contacts during the same physics batch require a stable starting hurtbox state; this is separate from the later damage invulnerability guard.

## Other audited paths and retained behavior

| Path | Source evidence / conclusion |
| --- | --- |
| Free/paid reroll stat rolls | `base_shop.gd:401-428`: rolls run after payment, including a zero price. Existing implementation retains this. |
| Tier token consumption | `base_shop.gd:371-395`: first matching effect/source token only, when new offers can be generated. Existing guards prevent all-locked consumption. |
| Duplicate purchase / charm | `base_shop.gd:582-604`: extra copies consume sources and reserve capacity for the normal bought copy. Existing implementation and focused tests cover target caps. |
| Theft and consequences | `base_shop.gd:547-578`, `item_service.gd:988-994`: consumes a charge, rolls enemy-group and caught consequences before zeroing the purchase price; tier conversion and displayed-price factor retained. |
| Lock eligibility and Candy Bag | `base_shop.gd:227-235,323-329`: X theft then eligible locking, and only surviving locked offers at shop departure disqualify the win. Existing source-backed guards retained. |
| Character prices | Source resources for Arms Dealer, Fisherman, Saver, Entrepreneur, Mutant, Glutton and Baby inspected. Fractional Arms Dealer base was the confirmed remaining formula difference. Fisherman bait-specific -100 and normal item-price modifiers remain in existing rules. |
| Arms Dealer destruction | Existing shop-entry weapon clearing retained; no new character start weapon behavior introduced. |
| Stat achievements | `challenge_service.gd:649-684` reads uncapped effective stats. Existing `CheckStats` correctly uses `Stat`, including HP under Handcuffs. |
| Structure achievements | Source counts registered structures-effect entries. Existing explicit structure counting retained, rather than counting all support actors. |
| Companion weapon challenges | `run_data.gd:1026-1047` examines raw weapon stats scaling. Nail/Frozen Heart do not rewrite that raw catalog field; existing intrinsic-scaling check retained. |
| Lootworm | `main.gd:1280-1286` sums ground gold values; current callers already sum pickup values, not object count. |
| Material lifetime counter | `main.gd:891` increments once per collected gold object, regardless of its merged value; current `Collect` retains that distinction. Harvest income does not add collected-object progress. |
| Burn / projectile wave counters | Existing wave-start reset retained. Source Wisp records kills by burn ticks (`unit.gd:698-700`), not every ordinary kill of a burning enemy. |
| Victory and difficulty | Character reward, actual won danger, global next danger and ten distinct Danger-5 character records reviewed; no additional confirmed base difference changed here. |

The supplied `.tres` resource scan contains no registrations for `item_steals`, `duplicate_item`, `increase_tier_on_reroll` or `gain_stats_on_reroll`, despite generic scripts exposing these paths. The imported JSON content likewise contains no such registrations. Existing generic fixtures demonstrate their implemented hooks; they do not establish that a supplied base character/item can activate them. This audit does not claim unsupplied DLC content is implemented or verified.

## Verification boundary

`ShopUnlockNextParityTests` adds 13 source-derived cases: four deferred lifetime counters, actual wave-end notification, loss versus victory eligibility, immediate fruit progress, cleanup fruit versus permanent growth, uncapped HP/intrinsic scaling, two Hourglass boundary/phase cases and Arms Dealer pricing. Bounded existing expectations in `FullBaseTests` and `SupportParityTests` were updated to the corrected source behavior. `git diff --check` passes for owned changes. Focused Unity execution remains with root; source inspection and authored tests do not establish full native or ordinary-run parity.

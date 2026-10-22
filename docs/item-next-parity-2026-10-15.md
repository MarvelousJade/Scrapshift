# Item and wave trigger recheck — October 15, 2026

Branch: `codex/gameplay-parity`. Supplied source inspected read-only at `E:/UnityProjects/brotato-(3.x)`. Codex authored this audit, runtime fixes and tests under the user's continued parity request. Existing R8 corrections, accepted artwork and audio remain intact. This agent launched no Unity process, suite or campaign; the user requested delaying the full suite until the remaining audit work is ready.

## Confirmed fixes

### Random stat damage rounds halfway values away from zero

Source `main.gd:1556`, `handle_stat_damages`, floors `max(1, source_stat * effect_percent)`, then calls Godot `round` after applying the player's damage percentage, then sums successful effect entries into one hit. `WaveImplants.RandomDamageTrigger` already preserved that order but used Unity `Mathf.Round`, whose halfway behavior can differ.

The handler now uses the existing `EnemyReferenceRules.SourceRound` helper. With Lich's real 100%-Max-HP healing trigger at 10 Max HP, **effective** +5% Damage gives **11** damage rather than 10, and effective +25% gives **13** rather than 12. Lich's `lich_effect_3.tres` reduces damage-stat gains by 50%, so those effective modifiers require +10 / +50 base Damage modifiers respectively. The tests divide their requested effective percentage by that real character gain and assert the resulting stat before triggering healing. Actual Cyberball entries on neutral rules with Luck 8 and +25% Damage each produce `round(floor(8 * .25) * 1.25) = 3`; two successful entries sum to one request of 6.

Five Lich cases cover positive/negative halfway percentages and zero final damage; two Cyberball cases exercise actual item chance with one/two copies using a fixed seed. These tests observe handler requests, not enemy selection or actual target mitigation.

The same source function reads `Utils.get_stat`, including **uncapped** Max HP. The previous special case used usable `MaxHealth`, losing Lich damage after Handcuffs capped health. That special case was removed: Lich with a usable cap of 10 and an underlying Max HP stat of 20 now requests 20 damage when ordinary healing triggers it. One real Lich/Handcuffs case checks this. Handcuffs' `handcuffs_effect_4.tres` captures the current Max HP cap; Torture instead grants +15 Max HP and disables ordinary healing, so it was an incorrect initial fixture choice. Related weapon/explosion Max HP scaling was reported to root for the shared combat paths.

### Anvil appends its upgrade after removing one matching weapon

Source `ui/menus/shop/base_shop.gd:62` applies Anvil on shop entry. `_combine_weapon` at line 636 uses `is_upgrade=true` to remove one copy via `RunData.remove_weapon`; it then appends the upgraded weapon via `RunData.add_weapon` at line 693. Normal combining removes two; Anvil removes one.

`WaveImplants.UpgradeWeaponImplants` previously replaced the chosen inventory index. It now removes the first weapon with the selected ID/tier and appends the upgraded weapon, following source ordering. A test with `[Pistol I, Knife IV, SMG IV]` expects `[Knife IV, SMG IV, Pistol II]`. Another checks that an all-maxed inventory retains its order and receives the existing +2 Armor fallback.

### Burn damage has two separate integer stages

Source `singletons/weapon_service.gd:290`, `init_burning_data`, combines native/global base burn and stat scaling. `apply_scaling_stats_to_damage` at line 489 clamps and truncates that scaled base to an integer. `apply_damage_bonus` at line 493 then rounds the percentage-adjusted integer, again enforcing a minimum of one. Native structure burning uses `apply_structure_damage_bonus` at line 498 instead of the ordinary player Damage modifier.

`ItemLifecycle.RollBurn` previously returned the unrounded product. The later combat damage path truncated it, which loses halfway results and applies the minimum at the wrong stage for negative scaling. It now truncates/clamps the scaled base first, applies the appropriate percentage, then uses source rounding and the final minimum.

Seven cases cover native Wand burning, a negatively scaled base, -100% ordinary Damage, Mage's fractional elemental gain, global Scared Sausage burning and native flame-turret Engineering scaling. Examples:

- Wand I: base burn 3 with +50% Damage becomes **5**.
- Wand I: base burn 3 with -5 Elemental Damage and +50% Damage clamps the scaled base to 1, then becomes **2**.
- Mage: Wand base 3 + starting Sausage 1 + effective Elemental 1.25 truncates to 5; +10% Damage yields **6**.
- Flame Turret: base burn 5 + one Engineering * .33 truncates to **5**, unaffected by +100% ordinary Damage.

Duration, global/native selection and spread are not replaced by this arithmetic correction. The source `Utils.get_chance_success` uses an inclusive positive chance boundary: `RollBurn` now accepts `roll <= chance` when chance is positive. One additional case checks certain chance at roll one and no burning at zero chance/roll zero; the Sausage case checks exactly .25 and just above it. The historical `CharacterProfileTests` .25 expectation was corrected with root authorization.

### Ugly Tooth preserves integer speed assignment and scaled maximum speed

`entities/units/unit/live_stats.gd` declares `speed` as an integer. `enemy.gd:254–256` subtracts a percentage of `max_stats.speed` from `current_stats.speed`, storing the result back into that integer after every hit. `entity_service.gd:64` has already applied enemy, accessibility and danger speed factors when that maximum was initialized.

`TriggeredImplants.ItemSpeedReduction` previously accumulated fractional world-unit reductions. It now converts the supplied maximum/current speeds to reference pixels, performs the subtraction and integer assignment, and returns the resulting reduction in world units. The root combat hook supplies final scaled maximum speed rather than the unscaled catalog value. One Ugly Tooth on 142 pixels/s reduces it to 134, a reduction of eight pixels/s. Two copies reduce it to 127, then 112 on the next hit, with the existing source cap checked before each subtraction.

Five cases cover one/two/three copies, consecutive integer assignments with cap overshoot, and a doubled maximum-speed modifier. Source `Unit.get_move_speed:222` adds `decaying_bonus_speed` separately from `current_stats.speed`; contact/temporary slows therefore must not be included in the permanent Tooth helper's current-speed calculation.

The audit also traced burn reset ordering for root's integration: `hurt_area_entered_deferred:550` calls the enemy Tooth hook before `apply_burning:578`; a fresh burn calls `set_burning(true)`, which calls `reset_speed_stat:725` and clears the permanent Tooth reduction, including the initial hit's reduction. Refreshing an existing burn merges it without that reset. Burn expiration calls `stop_burning` -> `set_burning(false)` and resets speed again. Separate decaying contact slow is not cleared by that speed-stat reset. Root owns the corresponding `ApplyBurn`/expiration hooks and their integrated checks.

The complete callback trace also corrects a narrower application-hit case: immediately after `apply_burning`, source sets the hit's `args.is_burning=true` before `take_damage`. That successful application hit qualifies for Greek Fire and a burning-kill event, even when it kills immediately. An ordinary hit against an already-burning target whose application roll fails does not acquire that argument merely from the status. Root owns moving burn application before damage and passing this event flag. Source `take_damage` checks any native crit-on-burning effect after the new status is set, but no resource assignment for that generic effect was found in the supplied registered base weapons/items, so no trait was invented.

## Shared pickup hook

At the integrating agent's request, added `public float MaterialDropAmount(int baseValue, bool tree=false)` in `ItemLifecycle.cs`. It exposes the existing material modifier and half-base floor **before** stochastic integer rounding. The pickup track needs this unrounded amount for source-style scattered materials. Pickup expansion and collection integration are owned by the other track/root.

The pickup source audit also confirmed that a **positive** fractional remainder succeeds when `roll <= fraction`, while integer amounts do not request an additional pickup at roll zero. `MaterialDropValue` now uses that exact boundary, guarding `fraction > 0`. Two tests check .75 just below/at/above the boundary, the existing .5 lower floor, and exact integer amount at zero roll.

## Critical-kill chance boundaries

Source `unit.gd:367–374` uses `Utils.get_chance_success` for each Hunting Trophy material entry and the summed Tentacle healing chance. Tentacle's handler previously rejected an exact positive chance boundary, and Trophy's used a strict comparison. Both now follow positive-inclusive comparison with a zero guard. Explicit strict source comparisons were retained for Adrenaline's `randf() < chance` and stat-damage entries' `randf() >= chance` rejection; the fix does not globally change every chance rule.

Five Tentacle cases cover just below/at/above its actual 20% chance, zero chance at roll zero, and five stacked Tentacles at 100% chance/roll one. Root authorized the corresponding old `ItemLifecycleTests` boundary expectation correction. Hunting Trophy retains independent per-copy rolls and material grants; no chance was replaced by a combined roll.

## Source inspection coverage

| Runtime area | Supplied source inspected | Finding / current scope |
|---|---|---|
| Wave-start health, one-time health, income | `main.gd:1404–1440`, `player_run_data.gd` initial defaults | Existing base modifiers are nonpositive health percentages; next-wave override/reset and ordinary positive/negative income branches inspected. No additional confirmed base mismatch here. |
| Next-wave temporary stat queues/removal | `items/global/effect.gd` KEY_VALUE apply/unapply, `main.gd` wave-start effects, Peacock resources | Existing sum, pending cancellation and consumed-queue behavior inspected; active temporary bonuses remain separate. Existing queue-removal tests provide the earlier boundary coverage. |
| Free shop rerolls | `ui/menus/shop/base_shop.gd` initialization and `_update_free_rerolls` paths | Existing free/bonus/paid accounting and increases on purchase inspected; no new change in this track. |
| Dodge healing and temporary buffs | `entities/units/player/player.gd:460–470` | Source sums successful healing entries into one heal before temporary dodge stat entries; current ordering retained. |
| Full-health consumable gains | `player.gd:915–935`, Extra Stomach resources | Pre-pickup full-health check and separate per-entry counters/caps inspected; existing tests cover duplicates, same-stat entries, injured pickups and wave resets. |
| Positive item-stat gains / Snowball | `run_data.gd:1244–1279`, `_apply_gain_stat_for_equipped_item_with_stat_effects:2014` | Current item-only positive scalar dispatch inspected; Snowball itself has no positive scalar effect, avoiding a self-grant. No new change. |
| Hunting Trophy / Tentacle critical kills | `unit.gd:367–374`, corresponding item resources | Fixed inclusive positive chance boundaries and zero guards; target eligibility belongs to combat. |
| Ugly Tooth slow | `enemy.gd:249–256`, `run_data.gd:1415–1418`, `unit/live_stats.gd` | Source sums slow magnitude, takes maximum cap and checks cap before subtraction; fixed integer assignment and final-scaled-speed use. Burn reset callback order traced for root. |
| Giant Belt / Greek Fire | `enemy.gd:271–307` | Giant Belt truncates each entry; Greek Fire sums percentages then truncates on a burn **event**. Existing arithmetic retained; rejected event/status hypothesis is not revived. |
| Added weapon scaling | `weapon_service.gd:620–638` | Source adds coefficients to native scaling; existing additive damage contribution retained. Weapon-specific/projectile paths are owned by combat. |
| Structure speed and minimum cooldown | `weapon_service.gd:553–590`, Improved Tools / Ball and Chain resources | Existing frame truncation and negative-speed branches inspected. Source uses the final minimum-cooldown entry; the registered base catalog has the one Ball and Chain value, so no speculative generic change was made. |
| Burning-kill stat caps | `main.gd:563–575`, `run_data.gd:243–253` | Source accepts burn-event kills or already-burning victims, per-entry counts and wave reset; existing `KillEffects` interface and cap behavior retained. Combat supplies the event/status predicate. |
| Random damage / Anvil / burn arithmetic / material remainder boundary | Functions detailed above | Corrected halfway rounding, uncapped source stat, upgrade ordering, two burn integer stages and source chance/remainder boundaries. Source callback/target integration remains root's gate. |

## Rejected healing-composition hypothesis

A temporary proposed Doc Moth/Potion multiplication was explicitly reverted after the sustain track inspected the actual resources. Doc Moth contributes to the same `hp_regen_bonus` effect array as Potion; `Player._hp_regen_val` remains one. The original additive `RegenerationHealMultiplier` expression is restored. The sustain track owns further regeneration loop/rounding checks. No multiplication change should be integrated from this pass.

## Validation and remaining limits

`Assets/Scrapshift/Tests/Editor/ItemNextParityTests.cs` contains **30 executable cases**: eight stat-damage cases, two Anvil cases, eight burn cases, two unrounded-material/boundary cases, five Tooth cases and five Tentacle cases. They have not been executed by this agent. Existing suite totals, the R8 native campaign and R8 screenshots predate these changes and must remain labeled as earlier evidence. Root owns focused/integrated validation and any new build.

Root's first **focused selection** executed 213 cases: 204 passed and nine failed (`tmp/ScrapshiftParity/artifacts/follow-up-focused.xml`, inspected). It was not a full-suite result. Six failures belonged to this track's fixtures: four Lich halfway cases had supplied base modifiers while asserting effective modifiers, the uncapped-health case had incorrectly used Torture as an HP cap, and an old material-drop test still expected the former exclusive .5 boundary. These were repaired from the actual Lich gain and Handcuffs resources and the confirmed inclusive material boundary; no runtime fix was reverted to satisfy them. Three other failures were outside this track. A rerun is pending, so the corrected fixtures are not claimed to pass yet.

One source stateful-removal quirk remains outside the confirmed reachable base flows: `BurningData.merge` keeps the maximum duration, but `BurningData.remove` subtracts duration. Removing one of two Scared Sausage effects therefore leaves zero global duration despite another copy; reconstructing the remaining catalog effects gives three. No normal supplied base flow that removes Scared Sausage was found (item selling is not present; generic duplicate-item removal hooks have no registered base assignment). The quirk was reported to root rather than silently introducing speculative state bookkeeping or claiming removal parity.

This is coverage of the inspected owned handlers, not proof of all possible trigger combinations or complete parity. Source code inspection does not establish audio perception, physical controls, target selection, survival balance, DLC/endless modes or an ordinary human full run. No shared interview/bug documents, reference project, Unity validation clone or audio assets were edited by this track.

## Root integration result

After integration and fixture corrections, the focused selection passed **213/213**, zero failed/skipped. It includes this track's cases and directly affected older regressions. The actual XML and 496-input source manifest are in `artifacts/gameplay-parity-follow-up-2026-10-15`. Earlier statements about unexecuted agent-authored checks or a pending rerun describe the preceding stage. The whole suite, new player build and full campaign remain deferred; these focused results do not establish exact parity.

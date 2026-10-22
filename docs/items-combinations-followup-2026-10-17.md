# Item combination parity follow-up — 2026-10-17

## Confirmed correction

The supplied reference's `entities/units/unit/unit.tscn:51-52` sets `BurningTimer.wait_time = 0.5`. Its `unit.gd:67-71` sums all players' burning cooldown increase and reduction, multiplies that **half-second** timer by `1 + increase - reduction`, and floors the result at 0.1 second. Scrapshift's `ItemLifecycle.cs` had the same modifier and floor but used a one-second base. Every ordinary burn tick therefore occurred twice as slowly, including with Eyes Surgery and Frozen Heart together. `ItemLifecycle.cs` now uses the reference base timer. The old `.8` expectation for a 20% reduction in `ItemLifecycleTests.cs` was a stale assertion for the wrong runtime value; it is corrected to `.4`.

`ItemsCombinedParityTests.cs` now exercises two Eyes Surgery copies (`item-design.json` grants 20% reduction each), a Frozen Heart (−100% reduction), then removal of one copy and the Heart. Expected periods are 0.5 → 0.3 → 0.8 → 0.9 → 0.4 seconds. This covers additive positive and negative modifiers, the timer multiplier, and acquisition/removal recalculation.

The same test class checks two Scared Sausage copies with a native-burning weapon. The supplied `scared_sausage_burning_data.tres` defines 25% chance, 1 damage, three ticks and 100% Elemental Damage scaling per copy. `effects/burning_data.gd:24-28` sums chance and base damage while taking the longest duration. `singletons/weapon_service.gd:291-329` keeps a native weapon's chance, duration and scaling, adding only global base damage. The test checks global 50% / 12 damage / three ticks versus native 30% / 11 damage / two ticks at 10 Elemental Damage, then one-copy removal. Existing Scrapshift code already matched these values; this mixed-effect check guards it against regressions.

Two more mixed tests exercise health and links. `main.gd:1622-1642` compares current HP to the current maximum for below-half effects; `player.gd:702-713` tests each regeneration bonus threshold against the same ratio. `linked_stats.gd:20-74` computes its missing-health source from current/max HP and reads ordinary and temporary stats before writing linked contributions. The test crosses 4/10 HP to 4/6 HP by lowering maximum HP, checking the below-half armor, Potion multiplier and missing-health linked damage together. A separate reciprocal armor/speed link test checks that linked output does not feed the other link, then updates when the base speed changes.

`player.gd:702-721` issues one regeneration heal for a timer batch. Its healing callback emits one `healed` signal, and `main.gd:1552-1595` sums successful damage-on-heal entries for one enemy hit. A new test combines Potion's below-half multiplier with two Lich-style entries: one 2 HP regeneration event must request one 20-damage hit at 10 Max HP. This tests both additive copies and a single trigger from combined healing.

## Other inspected combinations

- `singletons/run_data.gd:303-309` collapses duplicate Alien Eyes and on-death projectile counts into the first payload on deserialize; current native behavior uses the first Alien Eyes projectile template and summed count. All catalog copies share the same template, so no source-backed production mismatch was established here.
- `effect_behaviors/enemies/percent_damage_taken_enemy_effect_behavior.gd:62-126` groups vulnerability by source ID with a proc limit. The only current item is Ice Cube (`maxCount: 1` in Scrapshift's catalog, `max_procs = 1` in the supplied `.tres`), so the current single-application path is consistent for obtainable runs. It does not prove general arbitrary mixed-source handling.
- `singletons/run_data.gd:1412-1421` sums remove-speed magnitudes and takes the maximum cap; `entities/units/enemies/enemy.gd:254-256` applies that aggregate per hit. Scrapshift's aggregate follows this order.
- `entities/units/player/player.gd:918-930` tracks full-health consumable gains per effect entry and permits the last grant to exceed a listed maximum. Scrapshift's per-entry counter has the same cap-before-grant behavior.
- `effects/burning_data.gd:24-35` has an unusual removal rule: merging two Scared Sausages takes the maximum three-tick duration, but removing one subtracts three and leaves zero. `RunData.remove_item` does call the item's `unapply` method. Ordinary shop removal call sites in `base_shop.gd` consume Hourglass, Goldfish or Duplicator; its discard action removes weapons. The arbitrary Sausage removal path is available from the debug menu, but an ordinary run appears unable to trigger it. No special state tracking was added for that debug-only edge.

## Coverage matrix

| Effect combination | Source rule checked | Regression coverage | Remaining limit |
| --- | --- | --- | --- |
| Global and native burn, duplicate Sausage, cooldown increase/reduction | `burning_data.gd`, `weapon_service.gd`, `unit.gd`, `unit.tscn` | New `ItemsCombinedParityTests` burn tests | Native enemy tick cadence still needs a fresh player fixture. |
| Temporary interval bonuses and reset on hit | `player.gd:1085-1103`, damage callback | Existing `ConditionalEffectTests.ResetOnHitClearsOnlyAccruedTimedBonusAndKeepsGlobalClock` and `ItemStackParityTests.DuplicateIntervalBonusesResetTogetherOnlyForResetOnHitEffects` | No exhaustive interval pairwise grid. |
| HP thresholds, max HP changes and missing-health source | `main.gd:1622-1642`, `player.gd:702-713`, `linked_stats.gd` | New combined threshold test; existing `TriggeredImplantTests.MissingHealthLinksRoundTheSourcePercentBeforeApplyingScaling` | Native threshold display/update timing remains unobserved. |
| Reciprocal stat links and distinct item counts | `linked_stats.gd:20-74` | New reciprocal-link test; existing `ItemStackParityTests.DuplicateStatLinksTruncateEachCopyAndCountDistinctItemIdentities` | Uncommon link sources are outside these cases. |
| Per-wave consumable caps and duplicate copies | `player.gd:918-930`, `run_data.gd:588-600` | Existing `TriggeredImplantTests` full-health cap and duplicate-copy tests, plus wave reset test | Mid-wave item mutation is not a normal shop path. |
| Healing-triggered damage and regeneration modifiers | `player.gd:702-721`, `main.gd:1552-1595`, Potion threshold | Existing `ItemStackParityTests.DuplicateDodgeHealingEmitsOneHealingAttack`, existing `TriggeredImplantTests.RegenBonusStopsAtHalfHealthAndDoesNotBoostTorture`, new combined threshold and Potion/Lich damage tests | Survival track owns end-of-run healing integration; no new shared healing edits here. |
| Additive versus boolean/capped duplicates | `run_data.gd` effect storage; item effect scripts | Existing `ItemStackParityTests` explosion, Giant Belt, retaliation and weapon cap tests; new burn combination test | This is representative source-derived coverage, not exhaustive pairwise parity. |

## Verification state

Final R25 validation supersedes the R24 review below. The full EditMode XML records **1,181/1,181 passed**, zero failed, skipped or inconclusive (`suite-attempt-1.xml`); the test runner log ends with exit 0. The R25 player build summary (`build-attempt-3-summary.txt`) records Succeeded, **0 errors and 0 warnings**. The final item native fixture in `items-attempt-2/items-combined.json` reports Passed, and its process record confirms exit 0.

I opened all three fresh R25 screenshots (`items-global.png`, `items-mixed-cooldown.png`, `items-native.png`) and checked the JSON and process/log records. They show the staged Wave 1 player and enemy with visible floating 12, 22 and 11 damage numbers respectively. R25 measured the same tick sequences as R24: global 0.5000/1.0000/1.5000 s at 12 each, mixed cooldown 0.8167/1.6167/2.4167 s at 22 each, and native 0.5000/1.0000 s at 11 each. The mixed ticks are one 60 Hz step after each 0.8 s boundary, within the recorded fixture tolerance. Screenshots show visible rendering at capture time; only the JSON records cadence and HP deltas. The fixture still manually applies burn to a stationary high-HP enemy with empty spawns and cleared weapons, so this does not establish ordinary hit probability, full-run balance or human play behavior.

Root's earlier R24 focused EditMode run passed **45/45**, zero failed/skipped/inconclusive, exit 0. The result and source provenance are in `artifacts/gameplay-priority-followup-2026-10-17/focused-attempt-1-summary.json` and its source manifest. That was a focused batch before the final R25 suite above.

The opt-in R24 native item fixture (`items-attempt-1/items-combined.json`) passed with an in-memory profile and process exit 0 (`process.json`). It stages an empty spawn director, removes the player's weapons and manually applies each payload to a stationary 10,000 HP enemy, then lets normal `ArenaGame.Update` advance the 60 Hz combat simulation:

| Staged case | Payload | Measured tick times from application | HP change |
| --- | --- | --- | --- |
| Two Scared Sausages | 50% global chance, 12 damage, three ticks; 0.5 s interval | 0.5000, 1.0000, 1.5000 s | 10,000 → 9,964; 12 each |
| Two Eyes Surgery plus Frozen Heart and Sausages | 50% global chance, 22 damage, three ticks; 0.8 s interval | 0.8167, 1.6167, 2.4167 s | 10,000 → 9,934; 22 each |
| Native burn plus two Sausages | Native 30% chance, 11 damage, two ticks; 0.5 s interval | 0.5000, 1.0000 s | 10,000 → 9,978; 11 each |

All three R24 PNGs were opened: `items-global.png`, `items-mixed-cooldown.png`, `items-native.png`. Each visibly shows Wave 1, a player and the staged enemy with the corresponding 12, 22 or 11 floating damage number. The images corroborate that the native scene rendered those cases; the JSON's frame timestamps and HP deltas establish cadence. The mixed 0.8 s case lands one 60 Hz step after each mathematical boundary, within the fixture's two-step tolerance. The manual burn application, frozen spawns and cleared weapons mean this review does not verify chance rolls, actual weapon hits, ordinary wave balance, survival, or a human playthrough. The final full-suite/build result is recorded above.

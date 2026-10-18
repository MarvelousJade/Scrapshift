# Shop and cross-run unlock follow-up - October 17, 2026

Baseline: `affceae`, branch `codex/gameplay-parity`. Current three-track follow-up validated in R25: whole suite 1181/1181 Passed and focused native checks below; no new full campaign is claimed here. Original source `E:/UnityProjects/brotato-(3.x)` remains read-only. Source hashes: `artifacts/shop-unlocks-followup-2026-10-17/source-findings.json`.

## Confirmed differences and correction

1. **Demon currency used the combat HP floor.** Source `run_data.gd:get_player_currency` returns `get_stat(stat_max_hp) as int`. The previous Scrapshift `ShopBalance`/`CanBuy` used `MaxHealth`, floored to 1 for combat. Spending effective shop HP to zero therefore left a false one-HP budget and allowed another cost-one purchase. The root-owned correction uses raw truncated `Stat(max_hp)` for ShopBalance and that balance for CanBuy. Combat minimum HP remains a separate calculation. The initial R24 ShopUI rendered ShopBalance as the header, but source inspection requires the header to remain material gold, HP purchase prices to use a heart icon, and the stat row to show raw HP 0 rather than combat floor 1. Source anchors are `ui/menus/shop/base_shop.gd:154` and `:835` (material-gold header), `shop_item.gd:138` (HP price icon), and `stat_container.gd:update` (raw stat). Those root-owned UI corrections are verified in the R25 native captures below.
2. **Death prematurely thawed the current run's unlock snapshot.** Source `RunData.reset` calls `ItemService.init_unlocked_pool`; retry restores the current run instead of starting a fresh eligibility snapshot. The old TickProgression ended its frozen pool whenever Phase became Lost, which would make a reward unlocked during that run eligible after retry. `RunUnlockPool.RefreshAfterRun` now waits for confirmed end on Lost, while Title/Won still end the run. Rookie completion/global save remain live at death. Retry retains the same Progression instance, lifetime counters and completed rewards; a genuinely new run refreshes item/weapon eligibility.

## Imported special-shop inventory

The imported `item-design.json` and `starter-profiles.json` contain **6 item carriers, 8 character carriers and 12 relevant effect families**. This is an audit inventory, not 12 new implementation changes.

| Family | Imported carriers | Source/runtime route reviewed |
| --- | --- | --- |
| `items_price` | Coupon; Mutant, Saver, Entrepreneur, Glutton, Baby | ItemService.get_value / RunRules.ReferencePrice |
| `weapons_price` | Arms Dealer | get_value preserves fractional discounted base / ReferencePrice |
| `specific_items_price` | Fisherman bait | first matching source-specific price / ReferencePrice |
| `guaranteed_shop_items` | Fisherman bait | ItemService shop args / RollShop guarantee queue |
| `recycling_gains` | Recycling Machine, Entrepreneur | ItemService.get_recycling_value / RecycleValue |
| `free_rerolls` | Dangerous Bunny | base_shop initial/additional free rolls / BeginShopRerolls + UpdateShopRerolls |
| `minimum_weapons_in_shop` | Arms Dealer, Baby | source early-guarantee/minimum rules / RollShop |
| `destroy_weapons` | Arms Dealer | base_shop entry before Anvil / OpenShop |
| `upgrade_random_weapon` | Anvil | base_shop entry eligible random weapon or armor / UpgradeWeaponImplants |
| `hp_shop` | Demon | raw max-HP currency, ceil price conversion / ShopBalance + CanBuy |
| `item_hourglass` | Hourglass | base_shop next-wave reset/replacement / PrepareNextWave |
| `extra_elite_next_wave_chance` | Candy Bag | base_shop next-wave encounter request / PrepareNextWave + encounter integration |

Price growth, reroll escalation/free-roll consumption, locked-offer price retention, full weapon capacity/forced combining, ordinary purchase/recycle, crate take/disposal and lock eligibility were traced through existing source/runtime handlers. Existing tests remain relevant: ReferenceRulesTests, ItemLifecycleTests, WaveImplantTests, SpecialShopParityTests, EconomyRemainingParityTests and EconomyNativeParityTests. This source read found the Demon boundary above; it does not replace native UI/interaction checks.

The generalized handlers for `item_steals`, steal-spawn effects, `duplicate_item`, `increase_tier_on_reroll`, and `gain_stats_on_reroll` were compared with base_shop and ItemService source. **No imported item/character carries these effects in the current catalogs.** Existing SpecialShopParityTests exercise synthetic definitions; that evidence is separate from imported-content acquisition/playtesting. `lock_current_weapons` is also uncarried; source prevents combine/recycle/forced combine when active, but expanding that unimported mechanic is outside this focused correction.

## Unlock coverage and persistence

The imported catalog has 115 definitions, including 50 character-win rewards, 10 plain stat-threshold definitions, 7 danger-win definitions and 6 difficulty-system definitions. Existing source/runtime paths were reviewed for deferred lifetime kill/material/tree/evil counters, direct stat/inventory requirements, win/danger rewards, lock-free win, Rookie death and late-wave menu return. The source marks used-lock state when leaving a shop with an active locked offer; temporary lock/unlock alone does not disqualify the win. Hourglass return also accepts the source cleanup interval while wave_in_progress remains true until end-wave timeout, so no unsupported cleanup-only restriction was added.

New `ShopUnlockFollowupParityTests` has **15 focused cases**:

- Four Demon raw/truncated balances, exact last-HP spend and repeat rejection, and rejecting a positive-HP item when its price cannot be paid first.
- Saving partial lifetime progress, reloading, completing on a later wave-timeout check and avoiding duplicate unlock notifications.
- Mid-run item reward frozen from the current pool, saved/reloaded and eligible next run.
- An actual character-win reward (Brawler/Power Fist), all tiers frozen during that run and available after reload/new-run refresh.
- Retry preserving global Rookie/item unlocks and lifetime progress while retaining the current run's frozen item eligibility.
- Five cases exercising the runtime pool-boundary hook across Lost pending/confirmed, Won, Title and Shop.

This does **not** claim every one of the 115 individual trigger scenarios has been played. Per-wave healing, burning, projectile-hit and consumable trigger mechanics are covered by the separate item/survival tracks; retry serialization is implemented by that track. The coordinating agent executed all 15 follow-up cases: 15/15 Passed within focused-attempt-1 (45/45 overall), exit 0. Native evidence is documented separately below.

## Every imported unlock: source dispatch and executed producer coverage

`artifacts/shop-unlocks-followup-2026-10-17/unlock-dispatch-coverage.json` maps every imported definition to its actual runtime dispatcher. No missing matching dispatcher, unresolved reward mapping or mismatched dynamic character-win ID was found. This is a source result, not proof that every trigger was previously executed.

| Definition category | Count | Producer exercised by new cases |
| --- | ---: | --- |
| Character-win rewards | 50 | Finish loss does not unlock; qualifying character win does |
| Plain stat thresholds | 10 | Nonqualifying raw stat then exact positive/negative threshold via CheckStats |
| Danger-win rewards | 7 | Loss does not unlock; exact danger victory does |
| Difficulty systems | 6 | Prior-danger win; Nightmare after ten distinct Danger-5 characters |
| Lifetime thresholds | 10 | Survivor/Gatherer below threshold, reach threshold, wait for RecordWaveEnd |
| Specific triggers | 32 | Production rule/Progression callbacks; includes actual DeployStructure and ReturnToSelection |
| **Total definitions** | **115** | **115 parameterized producer/persistence cases** |

New `UnlockDispatchParityTests.cs` adds those 115 scenarios plus one inventory guard, **116/116 tests Passed**, zero failures/skips, process exit 0, 6.064891 seconds (`artifacts/gameplay-priority-followup-2026-10-17/unlocks-attempt-1.xml`). Each scenario avoids Complete(target), invokes a production producer or rule counter adapter, checks exactly one target notification, saves/reloads the real Progression representation, and checks the reward in a new run's catalog pool. The compound Beast Master case earns the four scaling companion prerequisites via CheckStats and the final Lootworm prerequisite via RecordWaveEnd. Turrets invokes real DeployStructure in an EditMode object fixture; Hourglass invokes real ReturnToSelection. Native frame/input/contact behavior remains separate.

Before these additions only 23 imported IDs appeared literally in existing test sources; that text search was not branch coverage, and shared dynamic dispatch tests covered more than the literal count. The executed per-ID scenarios close that deterministic producer/dispatch/persistence test inventory gap. They do not imply 115 native human playthroughs or establish every world-event producer (for example actual burn/explosion/contact behavior) from counter-adapter tests alone.

## Native check guidance

In an isolated Demon shop, stage raw effective max HP 1 and a cost-one offer. After purchase the raw HP stat should display 0, a second cost-one HP offer must be disabled/rejected, and the combat max-health floor can remain 1. The header must retain material gold, with HP costs identified by a heart icon. Also stage raw 0/negative/fractional values to distinguish affordability from the live HP floor. For retry, unlock a real item mid-wave, die with retry enabled, verify Rookie/global progress is retained but that item is still absent from this run's eligible pool; a fresh run should include it. The completed R24/R25 native subset is distinguished below; fractional/negative affordability boundaries are automated rule tests.

## Opt-in native fixture prepared

`Assets/Scrapshift/Runtime/ShopUnlockRuntimeReview.cs` supports `--shop-unlock-review <folder> --controller-check <folder/input>`. It uses a 25-second internal timeout, writes `shop-unlock-review.json`, captures `demon-zero-shop.png`, `retry-frozen-run.png` and `next-run-unlocked-shop.png`, and exits 0 only when all assertions/captures pass.

It stages the imported Demon and real Bag (wave-one price 1 HP), checks raw currency 1 to 0 and blocked repeat purchase, then renders the actual ShopUI. It creates a unique `fixture-progress-<guid>.json` under the output folder, stages the Fruit Basket threshold through Progression.Count (not actual fruit contact), and saves/reloads that real item reward and lifetime count, lets actual Update perform death/Rookie/pool handling, calls real RetryCurrentWave, verifies the global progression object and frozen pool survive, and verifies a fresh run admits the unlocked item. The final visible offer is deliberately staged; it does not prove that an RNG draw produced it. Its setup uses the scene-free Tick adapter for shop transitions and does not constitute a full run or player-driven controls test.

R24 native attempt1 (`artifacts/gameplay-priority-followup-2026-10-17/shop-attempt-1/`) reports **Passed**, empty error, process exit **0**. Actual assertions: real Bag price1HP, first purchase true, raw currency1 to0, combat maxHP1, second purchase rejected, one Bag, saved/reloaded reward/counters, immediate Rookie character visibility at death, pool still frozen at death and retry, retry count1, lifetime kills7 preserved, and fresh-run item eligibility true. All three PNGs exist and were reviewed by the coordinating agent.

That model/dispatch pass did not establish visual parity. The R24 Demon capture exposed a source UI mismatch: the shop header showed HP currency instead of material gold, the price needed a heart icon, and the HP stat row displayed the combat minimum instead of raw0. Root corrected these renderers; the R25 rerun verifies the correction below. Preserve attempt1 as evidence rather than relabeling its images. The native Fruit Basket threshold is staged through Progression.Count; it does not prove the actual fruit-contact trigger, which is exercised separately at the rule level in the 115-definition matrix.

## Final R25 verification

`artifacts/gameplay-priority-followup-2026-10-17/shop-attempt-2/shop-unlock-review.json` reports **Passed** with an empty error. `process.json` independently records **exit 0**, from 2026-10-18 03:30:49.4366942 UTC to 03:30:55.4597700 UTC. The player log identifies `Builds/GameplayParity20261017R25`; no fixture exception appears and runner stderr is empty. Every boolean assertion is true: isolated profile, real cost-one Bag purchase, rejected second purchase, real save/reload, mid-run frozen pool, immediate Rookie visibility, frozen eligibility through death/retry, retained progression and fresh-run reward eligibility. Actual numbers remain raw HP 1 -> 0, combat max HP 1, one Bag, one retry and seven lifetime kills.

Both this agent and the coordinating agent inspected all three R25 captures:

- `shop-attempt-2/demon-zero-shop.png`: header shows **17 green materials**, reroll costs **1 green material**, all four purchase buttons show **1 heart** and are disabled at raw HP **0**; the Max HP stat row displays **0**.
- `shop-attempt-2/retry-frozen-run.png`: resumed Wave 1 gameplay, 15/15 live HP, and the newly unlocked Nutrient Recovery Filter popup. The JSON assertions establish that current-run pool eligibility remains frozen; a screenshot cannot establish that internal pool state.
- `shop-attempt-2/next-run-unlocked-shop.png`: the fresh-run shop renders the unlocked Nutrient Recovery Filter and its effects with material pricing. Its offer was explicitly staged after the eligibility assertion, not selected by a random shop roll.

The R25 build summary (`build-attempt-3-summary.txt`) reports **Succeeded, 0 errors, 0 warnings**, 401713034 bytes. The complete suite XML (`suite-attempt-1.xml`) reports **1181/1181 Passed, 0 failed, 0 skipped**, duration **67.9761842 seconds**; the coordinating process reports exit 0. The separate per-ID unlock dispatch run remains 116/116 Passed. These results validate the integrated fixes and documented staged native scenarios; they do not constitute 115 native trigger playthroughs, physical-controller retesting or a new 20-wave comparison.

## Attribution

The user directed the parity scope. Codex agents performed this source investigation and generated the tests/correction. The raw HP-currency issue is a source-backed boundary defect; the retry pool issue follows from integrating a new supported retry lifecycle with the existing snapshot. No independent user debugging or hands-on validation is implied.

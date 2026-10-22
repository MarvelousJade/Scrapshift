# Remaining shop and unlock native checks - October 20, 2026

Baseline: `8246340`, branch `codex/gameplay-parity`. Original `E:/UnityProjects/brotato-(3.x)` is read-only. This follows the R25 source/model/native distinctions in `docs/gameplay-verification-status.md` and preserves its historical results.

## Unit 1: imported shop alternatives through runtime UI

Prepared `Assets/Scrapshift/Runtime/ShopAlternativesRuntimeReview.cs` and its import metadata. Run only in an isolated player with:

`--shop-alternatives-review <output-folder> --controller-check <input-folder>`

The fixture has a 70-second internal deadline, catches nested coroutine failures, writes `shop-alternatives.json`, and exits 0 only if all recorded checks pass. It stages characters, inventory, offers, materials and wave deadlines. Virtual A/Y events then traverse actual Update/OnGUI. It does not invoke Buy/Recycle/Combine/ResolveCrate directly for the UI assertions. Fixture-only IgnoreFocus avoids synthetic input reset; physical controls and ordinary focus behavior remain separate.

Source-derived acceptance:

| Scenario | Acceptance | Read-only source |
| --- | --- | --- |
| Coupon purchase | Wave-one discounted integer price; UI purchase acquires real Bag and spends exactly that price | `item_service.gd:get_value`, `base_shop.gd:buy_item` |
| Lock / free / paid reroll / unlock | Actual lock button; same locked offer and price retained; Dangerous Bunny's free roll spends zero; first paid wave-one roll costs 1; actual unlock button | `base_shop.gd:fill_shop_items`, `_on_RerollButton_pressed`, `set_reroll_button_price` |
| Weapon inspect/cancel | Popup opens; only its 3 combine/recycle/cancel controls register (2 without combine), no background shop/stat/inventory controls; Cancel preserves weapon inventory, exact offer objects and currency | Source popup manager's modal behavior; `base_shop.gd:_on_item_cancel_button_pressed` |
| Combine/recycle | Two real tier-I Sticks become one tier-II; UI recycling uses integer discounted value and 60% multiplier with Coupon/Recycling Machine | `base_shop.gd:_combine_weapon`, `_on_item_discard_button_pressed`; `item_service.gd:get_recycling_value` |
| Crate take/disposal | Item found Take adds exact displayed reward; Recycle consumes second crate and credits exact recycle value | Original Item found handlers; actual runtime LootUI |
| Arms Dealer | Shop entry destroys starting weapons and offers the imported minimum weapon count | `base_shop.gd:_ready` before upgrades; ItemService shop selection |
| Fisherman | Imported guarantee produces actual bait offer | ItemService guaranteed-items argument and specific-price handler |
| Anvil | One of two eligible real weapons upgraded; separate no-upgrade-target case gives +2 Armor | `base_shop.gd:_ready` |
| Hourglass | UI Go repeats wave 1 and replaces real Hourglass with Broken Hourglass | `base_shop.gd:_on_GoButton_pressed` |
| Candy Bag | Catalog chance is 10%; explicitly forced 100% branch via fixture stat boost produces an Elite through actual Go/StartWave/director/birth queue | `base_shop.gd:_on_GoButton_pressed`, source elite group start at second 1 |

Prepared captures: `coupon-free-paid-lock.png`, `weapon-combine-actions.png`, `recycled-inventory.png`, `crate-disposal-actions.png`, `arms-dealer-shop.png`, `fisherman-guaranteed-bait.png`, `hourglass-repeated-wave.png`, `candy-extra-elite.png`.

R26 attempt 1 (`artifacts/parity-remaining-2026-10-20/shop-r26-attempt-1`) is preserved as **Failed, exit 1** after seven successful price/purchase/lock/reroll checks. Its first weapon inspection failed because the fixture looked up a scroll-view tile with the root offer/canvas coordinate transform. The fixture now selects the actual repaint-registered inventory control immediately before Go, according to ShopUI registration order, and records logical and registered rectangles in JSON for diagnosis. This changes only the fixture. The failed lookup alone does not establish a gameplay coordinate defect. The coupon/free/paid/lock capture was inspected; remaining actions and popup modal acceptance await the rebuilt rerun.

This source audit found one modal defect: GameUI disabled the underlying ShopUI while weapon actions were open, but ShopUI's local eligibility assignments re-enabled the background and registered its controls. The coordinator corrected root-owned ShopUI to retain the inherited enabled state and combine it with each local eligibility check. R27 verifies exclusive modal navigation and unchanged offers/currency on Cancel. The coordinator owns Unity/build/native execution and commits this logical fix with its fixture; the world-event unlock unit remains separate.

## R27 executed result

`artifacts/parity-remaining-2026-10-20/shop-r27-attempt-1/shop-alternatives.json` reports **Passed, 22/22 recorded checks true**, empty error. `process.json` independently records **exit 0**, 2026-10-20 11:58:56.6319868 UTC to 11:59:11.4843769 UTC (14.8523901 seconds). The player log identifies R27, shows no fixture exception, and runner stderr is empty.

Actual currency checks: discounted Bag price **16**, purchase **2008 -> 1992**, free reroll **1992 -> 1992**, paid reroll **1992 -> 1991**, recycling combined Stick **1991 -> 2004**, crate disposal **2008 -> 2014**. Exact locked offer/price retention, cancel preservation, combine result, entry effects, Hourglass replacement/repeat, and forced Candy encounter checks all passed. The popup registered exactly its three combine/recycle/cancel controls, or two when combine was unavailable; background shop/stat/inventory controls were excluded.

This agent inspected all eight PNGs: coupon/reroll/lock presentation; darkened shop with selected first Stick and Combine/Recycle/Cancel popup; empty recycled weapon inventory and 2004 materials; actual Item found Take/Recycle buttons; Arms Dealer empty inventory; Fisherman guaranteed bait at price 1; Hourglass repeated Wave 1; Candy transition to Wave 2. The Candy capture frames the player, not the distant Elite; actual native enemy-list assertion establishes that the encounter producer created an Elite. These screenshots and scripted actions do not establish natural RNG/acquisition or human playtesting.

### Coordinate diagnosis remains provisional

Successful activation does not establish every directional-navigation transition. R27 records the first weapon's logical rect `(870,672,82,80)` and registered rect `(933.6,671.6,65.6,64)` in all three mappings. The client-area screenshot at .8 scale places the tile near `(760,537.6,65.6,64)`, but GUIUtility.GUIToScreenPoint may include desktop/window origin. R27 did not record a same-frame root Go/offer rect or desktop/window origin, so comparing these two coordinate systems does **not** establish a gameplay defect. The earlier description of a confirmed coordinate discrepancy is withdrawn.

R26 derived its expected root transform from the rendered offer anchor, which should normally carry the window origin, but its failed lookup did not log that expected screen point or actual controls. Stale registrations or coordinate-origin behavior therefore remain possible fixture explanations. A future diagnostic should record root Go, root offer anchor and weapon rect in the same frame before drawing a conclusion. The user subsequently revoked computer control: no further native/UI process or input is authorized now. The R27 action/modal result stands; no coordinate runtime change is made or claimed on this evidence.

## Remaining scope

The prior 115 unlock producer/save/reload/next-run scenarios remain model evidence, including adapters for some world events. The separate native producer fixture now passed 23 checks for actual fruit pickup, hostile projectile contacts, burn deaths and explosion deaths, including isolated persistence/next-run eligibility. See `docs/unlock-world-events-2026-10-20.md` for actual results and staging limits.

This new shop fixture does not establish every character modifier, every RNG outcome, natural unlock acquisition, every naturally generated crate reward, or human survival balance. Generalized steal/duplicate/tier-reroll/stat-reroll have no imported carrier and remain separately labelled synthetic coverage. R25's Demon raw-HP/presentation and retry-unlock-pool evidence remains historical until rerun against the integrated current build.

## Attribution

The user set the parity scope and requested separate commits for logical changes. Codex authored this fixture and source-derived acceptance list. No new human playtesting or independently performed user debugging is implied.

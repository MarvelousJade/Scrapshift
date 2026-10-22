# Native survival, shop and world-unlock review — October 20

## Evidence reviewed independently

Both runner specifications identify `Builds/GameplayParity20261020R36/Scrapshift.exe`, windowed at 1280 × 720 with isolated `--controller-check` profiles. Root ran the fixtures. A separate Codex agent inspected both reports/process receipts and **all 16 saved PNGs individually using `view_image`**. No new game process or desktop interaction was used for this review.

Evidence roots are under `artifacts/native-followup-2026-10-20/`:

| Fixture | Inspected result | Capture review |
| --- | --- | --- |
| `survival-native/survival-review.json` | Passed; empty error; process exit 0 in 8.496145 seconds | All eight PNGs inspected |
| `shop-alternatives/shop-alternatives.json` | Passed; empty error; all **22 checks** passed; process exit 0 in 14.213306 seconds | All eight PNGs inspected |

Each folder now has `independent-visual-review.json` with SHA-256 hashes for its report, process receipt, runner specification and every inspected PNG. Those hashes identify the exact artifacts reviewed. Screenshots support visible state; the runtime report supplies transition, timing and callback assertions.

## Survival: production contact and staged progression

The survival fixture uses actual Update and production hostile projectile contact with initial invulnerability zero. Positions/damage and Armor/Regeneration are supplied by the fixture. The report records:

- A staged 4-damage projectile through 15 Armor leaves **8 HP**; another contact during invulnerability does not reduce it further.
- Ordinary Update regeneration restores **9 HP**. Torture blocks the direct combat heal, while a real cleanup fruit restores **7 HP** through the cleanup exception.
- Lethal hostile contact enters pending death cleanup and clears enemies, projectiles and supports.
- A **direct model Pause call**, rather than OS focus or physical pause input, freezes cleanup age at 0.016667999 seconds. Acknowledgment appears after 0.504274 seconds.
- Death retains 100 materials and zero XP, banks five raw materials, grants no harvesting income, and leaves fruit uncollected.
- **Virtual A through actual Update/OnGUI** confirms Retry, restores wave 1 with 10 HP/checkpoint economy and increments retries to one.
- The naturally produced Rookie/Bulkhead unlock remains queued through acknowledgment and retry. A later periodic lethal event uses the same cleanup path; actual virtual A acknowledges the loss and dismisses Continue after the run ends.

`IgnoreFocus` is a fixture setting that protects synthetic button events. It does not validate operating-system focus behavior. Incoming projectile contact and regeneration are native Update evidence; directly supplied modifiers, direct Pause and forced periodic damage remain staged inputs.

### Survival capture observations

| Capture | Visible observation |
| --- | --- |
| `damaged.png` | 8/10 HP, 100 materials, wave 1 and red -2 feedback |
| `regenerated.png` | 9/10 HP and green +1 HP feedback |
| `torture-cleanup.png` | 7/10 HP, +3 HP feedback and zero wave timer |
| `defeat-ack.png` | Dimmed Run Lost, Baby Alien icon/name, OK and bottom-left Bulkhead unlock toast |
| `defeat-retry.png` | Retries: 0, Retry wave and End run, with unlock toast retained |
| `retried.png` | Wave 1 combat, timer 20, 10/10 HP, 100 materials, retained unlock toast |
| `unlock-after-end.png` | Bulkhead reward panel with readable abilities and highlighted Continue |
| `end-summary.png` | Run Lost summary, Salvage Yard/wave 1/Danger 0, zero enemies defeated, character inventory and Restart/New Run/Main Menu |

These captures are consistent with the report. They do not by themselves prove a timer, state transition or input event happened.

## Shop: actual UI events and direct entry effects

Synthetic Gamepad A/Y events traverse real Update/OnGUI. Focus positions, inventories, offers, materials and wave deadlines are staged. The fixture disables physical Gamepad devices for isolation; this is not Xbox hardware evidence.

**UI paths verified:** purchase, lock/unlock, free and paid rerolls, retained locked offer/price, inventory inspect, exclusive weapon-popup navigation, Cancel without state change, Combine, Recycle, crate Take/Recycle, and Go for Hourglass/Candy. The weapon popup registers only Combine, Recycle and Cancel, excluding background controls.

**Directly staged production entry paths:** Arms Dealer destroys the starting inventory and offers weapons; Fisherman's shop includes its guaranteed bait; Anvil upgrades one eligible weapon or gives +2 Armor without an eligible target. These checks exercise production entry/model behavior, rather than human purchases or naturally acquired builds.

**Candy limit:** the imported effect is asserted to be 10%, then the fixture explicitly raises it to 100% for successful-branch coverage. Virtual Go and actual StartWave/director/birth queue produce an Elite. The screenshot shows spawn warnings; actor identity comes from the runtime assertion. This native run does not measure the natural probability distribution.

### Shop capture observations

| Capture | Visible observation |
| --- | --- |
| `coupon-free-paid-lock.png` | Shop currency 1991, reroll price 2, four priced offers, first Lock selection, item inventory and Weapons (1/6) |
| `weapon-combine-actions.png` | Dimmed shop; Stick popup with selected Combine, Recycle (+6), Cancel and readable set tooltip |
| `recycled-inventory.png` | Currency 2004, Weapons (0/6), popup closed and reroll selected |
| `crate-disposal-actions.png` | Item found card, selected Take and Recycle (+7), readable effect icons and no bottom inventory strip |
| `arms-dealer-shop.png` | Weapons (0/6), Hand/Wrench offers, character inventory and free reroll |
| `fisherman-guaranteed-bait.png` | Threat Beacon Implant (imported bait), price 1, charging-enemy description and Fisherman portrait |
| `hourglass-repeated-wave.png` | Wave 1, timer 20, player with knife at 1/15 HP; replacement/repeat facts are report assertions |
| `candy-extra-elite.png` | Wave 2, timer 24, player at 15/15 HP and red spawn warnings; the image alone does not identify the Elite |

The inspected snapshots show readable corresponding controls and state. No additional gameplay mismatch was established in this bounded review.

## R37 world-event unlock follow-up

Independently inspected `world-unlocks-r37/unlock-world-events.json`, its process/runner receipts, the actual saved profile and all **four PNGs** using `view_image`. The runner identifies `Builds/GameplayParity20261020R37/Scrapshift.exe` at 1280 × 720. Result: **Passed, 23/23 checks, process exit 0** in 8.029234 seconds. `world-unlocks-r37/independent-visual-review.json` records hashes and individual observations, bringing this document's capture review to 20 images across the three fixtures.

| Event producer | Actual bounded evidence | Staging and limits |
| --- | --- | --- |
| Fruit pickups | 100 actual full-health contacts; Hungry absent at 19 and completed at 20; Fruit Basket absent at 99 and completed at 100 | Fruit objects are placed at the player; no lifetime-counter injection. Full HP and invulnerability are staged. |
| Hostile projectiles | Jellyshield absent after two real contacts and completed on the third through actual Update | Bullet position/damage are staged; invulnerability is reset before each contact. No physical firing/input. |
| Burn deaths | Will-o'-the-Wisp absent after 29 deaths and completed at 30 through actual burn timers and deferred death processing | Torch is imported, but `TryApplyWeaponBurn` is called directly on supplied 1-HP targets. This verifies the downstream producer, not natural Torch firing. |
| Explosion deaths | A single explosion killing 14 does not unlock Fireworks; a **distinct** explosion killing 15 does | Rocket Launcher explosion metadata is imported; explosion creation, damage and clustered 1-HP targets are staged. Report total **29 explosion kills = 14 + 15**, not one 29-kill blast. |

The fixture uses production callbacks without direct challenge Record/Count/Complete shortcuts. Five targets are saved/reloaded and each emits exactly one target completion notification. The saved JSON contains `chal_hungry`, `chal_fruit_basket`, `chal_jellyshield`, `chal_will_o_the_wisp` and `chal_fireworks`, plus full-health fruit count 100 and total enemy kills 59 (30 burn + 29 explosion).

Fruit Basket, Jellyshield and Will-o'-the-Wisp item rewards remain excluded from each active frozen eligibility snapshot, then become available through an **actual fresh StartSelectedRun**. Fireworks' character reward becomes visible through its achievement callback. Final saved outcomes survive another fresh-run transition. Those entries use direct fixture calls to start runs; no menu navigation or human cross-run play is claimed.

### World-event capture observations

- `fruit-contact-unlocks.png`: full HP, pickup feedback and a readable Devourer achievement toast.
- `hostile-contact-unlock.png`: 12/15 HP, -1 hit feedback and a spawn warning; the existing Devourer toast remains visible.
- `burn-death-unlock.png`: distant target cluster, damage numbers and material drops, with the player at full HP.
- `explosion-death-unlock.png`: visible explosion circle, clustered target damage and material drops.

The four snapshots do not display five separate reward panels or independently count contacts/kills. Thresholds, persistence and eligibility assertions come from the inspected runtime/profile evidence. This is native Update coverage for five staged event producers, not 115 native playthroughs or exhaustive unlock-path coverage. No additional mismatch was established by this review.

## Remaining limits and interview use

Physical controller checks are skipped. OS focus, manual keyboard/mouse behavior and ordinary navigation outside the staged focus positions remain separate work. These fixtures do not close human survival balance, natural acquisition, arbitrary item interactions, full-campaign comparison, audio listening, or WebGL/browser acceptance.

The user authorized continued verification. Codex authored/executed the fixtures and performed this independent artifact review; none of that is recorded as the user's personal playtesting. For interview preparation, this evidence demonstrates separating a real UI activation from direct model calls, recording a forced probability branch explicitly, and checking the rendered result alongside machine assertions. The user should reproduce a chosen path and explain its limits before presenting it as their own hands-on example.

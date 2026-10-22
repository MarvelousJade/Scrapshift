# Native unlock event producers - October 20, 2026

Baseline: `8246340`. Original source remains read-only. This is a separate logical validation unit from the shop alternatives fixture.

Prepared `Assets/Scrapshift/Runtime/UnlockWorldEventsRuntimeReview.cs` and metadata. Invocation: `--unlock-world-events-review <folder> --controller-check <input-folder>`. It has a 55-second internal deadline, writes `unlock-world-events.json`, catches nested coroutine errors and exits 0 only when every recorded assertion passes. This was prepared by an agent and later executed by the coordinator as R26, as recorded below.

## Source-derived acceptance

| Imported challenge | Actual runtime producer exercised | Boundaries / persistence |
| --- | --- | --- |
| Hungry | DropConsumable objects -> actual pickup physics/contact -> CollectPickup -> CollectConsumable | 19 below / 20 qualifies; real save/reload, single notification |
| Fruit Basket | Same native fruit contacts at full HP, no Count shortcut | 99 below / 100 qualifies; active item pool frozen, fresh StartSelectedRun admits item; real save/reload |
| Jellyshield | EnemyBullet objects staged at real player hurtbox -> actual projectile Update/contact | 2 below / 3 qualifies; each contact explicitly resets hit immunity; active item pool frozen / next run admits item; real save/reload |
| Will-o-the-Wisp | Imported Torch TryApplyWeaponBurn -> actual burn timer -> Damage/death | 29 below / 30 burn kills in same wave qualifies; active pool frozen / next run admits item; real save/reload |
| Fireworks | Imported Rocket Launcher explosion metadata -> actual timed explosion overlaps/deaths | Separate 14-kill explosion fails; one 15-kill explosion qualifies; character becomes visible immediately; real save/reload and final next run retain all outcomes |

Read-only source anchors: `entities/units/player/player.gd:check_chal_jellyshield` counts projectile contact before damage eligibility; `player.gd` consumable handler increments this-run consumables and full-health fruit lifetime progress. `entities/units/unit/unit.gd:698` records lethal burn ticks. `projectiles/player_explosion.gd:_on_Hitbox_killed_something` counts kills within one explosion and completes at 15. `singletons/run_data.gd:245` resets the wave burning challenge counter.

## Staging and limits

The fixture never calls challenge Record/Count/Complete adapters directly. It creates real fruit/bullet objects and lets actual Update resolve them. Torch application is invoked with a real imported weapon; initial weapon contact/firing is not tested. Explosion creation is explicitly invoked with imported metadata; projectile travel, initial rocket hit, proc probability and natural enemy clustering are not tested. Enemy positions/HP and idle encounter director are staged. Invulnerability protects the fruit/burn/explosion fixture; projectile cases reset immunity and take ordinary one-damage contacts. This is event-dispatch/persistence evidence, not a human survival run.

Each scenario completes through a fresh unique artifact-path Progression profile. Each target checks one notification and a new Progression instance reloading the real JSON. Fruit, Jellyshield and Will-o-the-Wisp check frozen current-run item eligibility and eligibility after actual new-run lifecycle. Hungry/Fireworks check saved character rewards; Fireworks also checks immediate character visibility. Five representative native producers do not close every one of the 115 imported unlock world-event paths.

## R26 executed result

`artifacts/parity-remaining-2026-10-20/unlocks-r26-attempt-1/unlock-world-events.json` reports **Passed, 23/23 recorded checks true**, empty error. Actual totals: **100 fruit contacts, 3 hostile projectile contacts, 30 timed burn deaths, 29 explosion deaths** (separate 14-kill and 15-kill explosions). All five target save/reload and exactly-once notification checks passed; current-run freeze and fresh-run item eligibility checks passed. The actual fixture save contains all five completed challenge IDs.

`process.json` independently records exit **0**, from 2026-10-20 11:54:21.4813356 UTC to 11:54:29.8514905 UTC. The player log identifies R26, has no fixture exception, and runner stderr is empty. R26 build summary reports zero errors/warnings, 401755514 bytes. These are focused native fixture results, not a newly executed whole suite or full campaign.

This agent inspected all four actual PNGs: `fruit-contact-unlocks.png` shows full-health fruit pickup particles and the Hungry/Devourer popup; `hostile-contact-unlock.png` shows 12/15 HP and hit numbers after three contacts; `burn-death-unlock.png` shows burn tick damage/death drops; `explosion-death-unlock.png` shows the explosion and overlapping damage numbers. Queued toasts still display the earlier Devourer message in several captures; the JSON/save assertions establish the distinct target completions. Screenshots alone do not establish counter thresholds or persistence.

No gameplay defect was found in these five world-event paths. No direct challenge adapters were used to produce their completions. The source and staging limits above remain applicable; the other 110 definitions have prior producer/model evidence, not this native-event evidence.

## Current mapped inventory: R30 callbacks plus historical native producers

All 115 imported definitions retain producer/save/reload/next-run model cases. R26 adds the five dedicated native producers above; Rookie has separate R25/R26 survival death/retry evidence. R30 now adds **109/109 staged production scene-callback scenarios Passed, actual exit 0**. The 109 unique new IDs plus six historical IDs exactly equal all 115 imported definitions, without overlap or omissions. See the [full per-ID inventory and source anchors](headless-unlocks-2026-10-20.md).

The actual [R30 report](../artifacts/headless-completion-2026-10-20/unlocks-r30-attempt-1/headless-unlocks.json) has all 654 assertion fields true:109 lower/nonqualifying boundaries, completions, exactly-once notifications, reloads, active-pool eligibility checks and fresh-pool eligibility checks. [Independent review](../artifacts/headless-completion-2026-10-20/unlocks-r30-attempt-1/independent-review.json) independently read the report and all 109 saved JSON profiles; every saved target is present. This is actual JSON persistence. The pool assertions instantiate separate **synthetic active/fresh eligibility snapshots**, not an organically continuing run or actual subsequent menu scene.

| Family | IDs | R30 staged callback coverage |
| --- | --- | --- |
| Registrations, timed targets, healing and lifetime kills | `chal_turrets`, `chal_lumberjack`, `chal_medicine`, `chal_giant_slayer`, `chal_evil_hat` | Registered turret4/5, Hurt/Heal199/200, activated lethal target age15.001/15, declared lower lifetime counters plus final actual tree/evil Damage callback and cleanup. |
| Cleanup state | `chal_forest`, `chal_reckless`, `chal_lootworm` | Scene tree census9/10, HP2/1 and ground material19/20 through cleanup callbacks; cleanup clock and actor positions are staged. |
| Shop/acquisition inventory | `chal_hoarder`, `chal_recycling`, `chal_baited`, `chal_bourgeoisie`, `chal_scavenger`, `chal_experimentation` | Material pickup and imported acquisition/Buy/Recycle handlers reach their thresholds with lower cases. Shop offers and funds are staged; no UI input is claimed. |
| Level and wave boundary | `chal_student`, `chal_fast_learner` | CollectPickup/XP reaches level 20; level 10 excluded on wave 6 and accepted on wave 5. Callback delivery is explicit, not pickup physics. |
| Registered structures, raw scaling and compound unlock | `chal_advanced_technology`, `chal_magic_and_machinery`, `chal_bonk_dog`, `chal_catling_gun`, `chal_bot_o_mine`, `chal_blazemander`, `chal_doc_moth`, `chal_paws_n_claws` | Imported registrations, raw-scaling weapon purchases, compound stat thresholds and five producer-earned prerequisites. No direct Complete shortcut. |
| Menu/run completion eligibility | `chal_hourglass`, `chal_candy_bag` | ReturnToSelection5/4.99 seconds and locked/no-lock staged final scene finishes. Current visible menu and actual input remain pending. |

The other83 definitions share dispatch families:50 character wins,10 signed raw-stat thresholds,7 danger wins,6 difficulty-system unlocks and10 lifetime kill/material thresholds. Character/danger wins stage the final wave and settlement callbacks; they are not83 full playthroughs. Nightmare explicitly stages nine prior distinct Danger 5 records before the tenth finish. Lifetime lower counters are staged; only the final event is reenacted. Stats are staged through GainWeaponStat before the scene progression callback.

The ArenaGame component is disabled for R30 while the fixture synchronously invokes scene callbacks. Thus R30 is **not organic Update scheduling, physics, human balance, current UI or hardware verification**. R26 remains separate actual Update/contact/timer evidence for its five IDs; its native next-run transitions are not replaced by R30's synthetic pool checks. No new gameplay mismatch was found in the 109 finite scenarios. Current popup/disclosure rendering, visible availability and interactive retry remain deferred until the user allows desktop control.

R30 also closes S1–S7 special-shop **logic** with [14 passing receipts](headless-shops-2026-10-20.md), including 12 grouped rule scenarios, both keep-lock preferences and a Candy encounter request. Current price display, input, modal presentation and Candy actor physics remain pending. The [dynamic item oracle](headless-items-2026-10-20.md) separately matched **53 cases /397 snapshots /6,352 scalar comparisons, zero differences**; broader arbitrary item combinations, scheduling and organic survival are still outside that finite matrix.

The October20 correction makes `chal_turrets` depend on registered structure effects. Earlier fifth-live-turret acceptance is withdrawn. Use four/five qualifying registrations; live body multiplicity does not establish the condition. The updated [remaining acceptance checklist](remaining-native-acceptance-2026-10-20.md) separates completed callback logic from deferred UI/hardware gates. R29's full20 and ordinary-damage results remain historical campaign evidence, not individual unlock threshold playthroughs.

The user requested the parity scope and separate meaningful commits; Codex authored the investigation and checks. No new user hands-on validation is implied.

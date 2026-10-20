# Native unlock event producers - October 20, 2026

Baseline: `8246340`. Original source remains read-only. This is a separate logical validation unit from the shop alternatives fixture.

Prepared `Assets/Scrapshift/Runtime/UnlockWorldEventsRuntimeReview.cs` and metadata. Invocation: `--unlock-world-events-review <folder> --controller-check <input-folder>`. It has a 55-second internal deadline, writes `unlock-world-events.json`, catches nested coroutine errors and exits 0 only when every recorded assertion passes. No Unity/native process was launched by this agent.

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

## Narrow remaining native acceptance inventory

All 115 imported definitions retain producer/save/reload/next-run model cases. R26 adds dedicated native producer acceptance for five IDs above. Rookie also has earlier native death/retry evidence (R25, expanded by the survival track). This is six specifically checked unlock producers, with **109 definitions lacking equivalent dedicated native threshold acceptance in this inventory**. Incidental completions during campaigns are not counted as a separate threshold/persistence scenario.

The remaining 26 specific definitions fall into six targeted families:

| Family | IDs | Required producer evidence still missing here |
| --- | --- | --- |
| Registered effects, timed lethal targets, healing and lifetime kills | `chal_turrets`, `chal_lumberjack`, `chal_medicine`, `chal_giant_slayer`, `chal_evil_hat` | Fifth registered structure-effect entry (not fifth live/deployed turret), tree/evil lifetime threshold checked at cleanup, 200 healing in one wave, and elite/boss death at/beyond 15 seconds |
| Cleanup state | `chal_forest`, `chal_reckless`, `chal_lootworm` | Actual wave-end living trees, exactly one HP, and uncollected ground-material threshold |
| Shop/acquisition inventory | `chal_hoarder`, `chal_recycling`, `chal_baited`, `chal_bourgeoisie`, `chal_scavenger`, `chal_experimentation` | Material threshold, twelfth actual recycle, second bait, required tier-IV weapons/distinct tier-I items/distinct weapons through acquisition callbacks |
| Level and wave boundary | `chal_student`, `chal_fast_learner` | Actual pickup/XP level threshold and Level10 before wave6 |
| Registered structures, raw scaling and compound unlock | `chal_advanced_technology`, `chal_magic_and_machinery`, `chal_bonk_dog`, `chal_catling_gun`, `chal_bot_o_mine`, `chal_blazemander`, `chal_doc_moth`, `chal_paws_n_claws` | Three registered structure effects plus relevant damage stat; six raw-scaling weapons per companion; regeneration/lifesteal thresholds; actual five-prerequisite compound completion |
| Menu/run completion eligibility | `chal_hourglass`, `chal_candy_bag` | Actual return-to-selection with <5 seconds left, and win after no active locked offer was carried out of a shop |

The other **83** definitions share generic dispatch families: 50 character wins, 10 plain-stat thresholds, 7 danger wins, 6 difficulty-system unlocks, and 10 lifetime kill/material thresholds. Their individual 115-matrix model scenarios do not constitute 83 native playthroughs. A future headless simulation can exercise producer/persistence assertions without verifying UI or human controls.

R27's native recycle action is one recycle, not the twelve-recycle challenge. Its guaranteed bait offer is not the two-bait unlock trigger. Using Hourglass to repeat a wave is different from earning its late-wave-return reward. Candy Bag's forced encounter branch is different from its no-locks-win unlock. These distinctions prevent the shop action fixture from silently closing those trigger gaps.

The October20 source correction makes `chal_turrets` depend on registered structure effects. An earlier checklist incorrectly called for the fifth deployed turret; that acceptance is withdrawn. Use acquisition/removal of the fifth qualifying registration, with four below threshold and weapon-created/live-body multiplicity excluded. The finite follow-up gates are in `docs/remaining-native-acceptance-2026-10-20.md`. R29 headless evidence is complete:20-wave Won/exit0 with18/18 bounded comparison checks, and ordinary-damage natural defeat in wave8 after7 completions. These do not close the individual native producer cases above; see the final checkpoint.

The user requested the parity scope and separate commits; Codex authored these checks. No new user hands-on validation is implied.

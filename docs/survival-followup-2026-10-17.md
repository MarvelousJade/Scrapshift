# Survival, lethal damage and retry follow-up - October 17, 2026

## Scope and evidence status

The preceding R23 full campaign was invulnerable, so it did not establish ordinary survival, lethal damage or defeat transitions. This follow-up starts from the parent's `affceae` checkpoint. Codex inspected the supplied read-only reference source and implemented source-backed corrections with separate model/native acceptance fixtures. **The focused selection passed 45/45, the complete integrated suite passed 1181/1181, and the final R25 staged native survival review Passed with exit code 0.** The parent launched the tests/player; this reviewer independently read their artifacts and inspected all six native screenshots. The R25 build succeeded with zero errors and warnings; its fresh survival captures were reviewed below.

The user requested exact gameplay behavior and parallel follow-up work. Their direction and earlier hands-on Xbox feedback are distinct from Codex's source investigation, code and staged checks. This document does not imply a human survival playthrough or independently user-authored debugging.

## Reference findings

| Behavior | Supplied source evidence | Result |
| --- | --- | --- |
| Incoming hit ordering | `entities/units/player/player.gd:300-314`, `412-549` | Dead guard, healing-hit handling, invulnerability check, then dodge -> hit protection -> armor. Positive actual damage triggers on-hit effects, starts regeneration and may cause death. |
| Armor | `singletons/run_data.gd:1429`, player `get_damage_value` | Integer Armor uses `10/(10+abs(armor)/1.5)`; negative Armor reflects the reduction above one. Ordinary armored damage rounds and has minimum one. Existing rules formula matches. |
| Invulnerability | player `get_iframes:574`, `take_damage:437-439` | Actual damage/Max HP maps to 0.2-0.4 seconds for the normal configured run. Dodge/protection also start minimum iframes. Existing `CombatGeometry.Invulnerability` matches this normal-run rule. |
| Regeneration | player `check_hp_regen`, `on_health_regen`, `global/fixed_timer.gd`, RunData `get_hp_regeneration_timer:1442` | Effective stat is cast to int before the interval formula. Timer loops batch into one healing event, threshold bonuses sample before healing, external full healing does not immediately reset a running timer. Existing SurvivalRules plus prior sustain tests cover these rules. |
| Life Steal | RunData `manage_life_steal:1375`, player `on_lifesteal_effect:734`, `player.tscn:1381` | Successful chance triggers share a 0.1-second timer, including when full HP prevents a positive heal. Existing shared cooldown matches. |
| Torture and cleanup healing | player `heal:777-790` | Dead and No Heal block healing. Torture blocks ordinary healing during combat, but explicitly allows it while `cleaning_up`. Unity lacked this cleanup exception. |
| Lethal cleanup | `main.gd:_on_player_died:466`, `clean_up_room:946`, `_on_EndWaveTimer_timeout:1117` | Loss sound and cleanup start immediately; the loss timer is 0.5 seconds. This path does not invoke `_on_WaveTimer_timeout`, harvesting or end-of-wave stat effects. |
| Lost-wave acknowledgment | `main.gd:1126-1130`, `ui/menus/ingame/retry_wave.gd` | After cleanup timeout, RetryWave is shown and pause is disabled. Default `retry_wave=false` shows an OK button that goes to the end-run scene. It is not an automatic summary transition. |
| Optional retry | retry-wave script, RunData `on_wave_start:237`, `get_state:1727`, `reset_to_start_wave_state:1769`, `resume_from_state:1785` | When the option is enabled, retry restores start-wave run/player data and increments retries; it does not start a new run or reset global progression/RNG. This option/path was missing from Scrapshift. |

### Death pickup and pause details

`ProgressData.settings.optimize_end_waves` defaults false. Ordinary cleanup changes remaining material collision layers and attracts them to the gold bag. `main.gd:on_gold_picked_up:895` banks their **raw value** when no player collected them; it does not grant spendable currency, XP or normal player pickup effects. There are no living players to attract fruit/crates. Optional optimized cleanup instead applies the common material-boost chance before banking; that separate option must not be silently used as the default.

`main._check_for_pause` has no cleanup guard. PauseMenu stays enabled during the half-second loss cleanup, so pausing freezes that timer. The timeout disables pause before showing acknowledgment. Bonus-gold conversion effects (for example Builder) remain a separate integration concern from raw banking; this review does not claim an exhaustive conversion matrix.

### Lethal hit and temporary-stat ordering

The source applies `temp_stats_on_hit` inside `player.take_damage` before calling `die`. `player.die` resets the player's TempStats before the later `took_damage` signal is emitted. The inspected signal listeners render effects/shake/floating text or reset Railgun/Builder no-hit counters; they do not re-add on-hit temporary player stats after death. Therefore model `DamageTriggers()` followed by lethal cleanup's conditional-stat reset preserves this ordering. A lethal hit must not retain that temporary on-hit bonus in the end inventory.

### Lethal-frame pickup exclusion

The original `player.die` immediately disables attraction and pickup-area monitoring. Unity previously continued its remaining simulation subsystems after lethal contact, which could remove a fruit at the dead player even though `Heal` rejected it. The parent adds phase guards after damage-capable subsystems and before ordinary pickup processing. The native death fixture places its fruit directly at the player alongside the lethal projectile; it must remain uncollected through cleanup and acknowledgment. This is a same-frame regression check, not merely a distant-fruit check.

## Implemented changes and file ownership

The parent owns shared integration in `RunRules.cs`, `DeathLifecycle.cs`, `ArenaGame.cs`, `RunFeedback.cs`, settings and end-screen flow. The agreed implementation preserves immediate `RunPhase.Lost` for model API compatibility, introduces a timed death-cleanup pending state, clears actors/attacks, banks raw materials without currency/XP, leaves consumables uncollected, permits pause during pending cleanup and delays acknowledgment. The parent adds Torture's `WaveSettlementPending` healing exception while retaining the dead/No Heal guards. R24 and final R25 native execution below validate the integrated survival path alongside the complete suite and fresh build.

This track adds:

- `Runtime/RetryWaveState.cs`: an explicit checkpoint of player/run state, not runtime reflection or serialized delegates. It copies permanent/temporary/queued stats, inventory/item copies, locked offers, currency/XP/level/health, relevant model counters and pending shop-spawn groups. Restore retains the same catalog, Progression, event subscriptions and RNG. It resets combat-only clocks, protection flags and cleanup state, and restores start-wave HP/materials without applying start-wave percentage income again.
- `Tests/Editor/SurvivalLifecycleParityTests.cs`: 14 declared source-derived cases covering armor, dodge/protection ordering, Torture cleanup/No Heal, lethal no-income behavior, cleanup pause, retry delay and state restoration, duplicate item/locked-offer ownership, bank restoration, repeated retry counts, abandonment, new-run reset and live progression/callbacks.
- `Runtime/SurvivalRuntimeReview.cs`: opt-in native fixture using actual hostile projectile contact and ordinary Update, followed by regeneration, a real cleanup fruit, lethal cleanup, raw material banking and retry confirmed by actual virtual A input/OnGUI.

`SurvivalRules.cs` was inspected but unchanged: no newly confirmed mismatch was found in its normal regeneration/lifesteal formulas or shared timer behavior. Item-effect crossovers are coordinated with the separate item track.

### Retry integration contract

`ArenaGame.StartWave` calls `Rules.CaptureWaveRetryState()` before consuming encounter queues or creating actors. Unity has already applied its once-per-wave model effects by then, so the checkpoint stores their resulting state rather than reapplying them on retry. `RunRules.Start` calls `ResetWaveRetry()` for genuinely new runs. `RetryCurrentWave` clears combat, restores the checkpoint, resets presentation/physics clocks and raw spawn origin, then starts the same wave. It does not call `StartSelectedRun` or `RunUnlockPool.BeginRun`.

The original checkpoints PlayersData and encounter/locked-offer state before actor creation; unlocked global achievements remain live. This explicit Unity checkpoint must be maintained if new persistent run fields are added. Focused restoration checks do not prove every character/effect combination; the parent must integrate the parallel shop/unlock regression and review queued special encounters.

## Native acceptance command

Launch the fresh integrated player with:

```text
--survival-review <fresh-output-folder> --controller-check <isolated-profile-folder>
```

The fixture has a 20-second bound and writes `survival-review.json` plus six captures: `damaged`, `regenerated`, `torture-cleanup`, `defeat-ack`, `defeat-retry`, `retried`.

It stages neutral rules with 100 materials, disables gamepad/mouse movement, mutes sound and starts with invulnerability zero. Production hostile projectiles deal four damage into 15 Armor (two HP lost), and an additional contact during iframes must not damage again. Ordinary Update must heal with 10 Regeneration. A real fruit during cleanup must heal through Torture. A lethal projectile must clear actors, permit a paused cleanup, wait at least half a second, bank five raw materials without currency/XP/harvesting, leave the dead player's fruit uncollected, then restore the same-wave checkpoint through retry.

The fixture stages projectile birth/damage and some rule/cleanup triggers. It uses fixture-only IgnoreFocus for synthetic input and clears pending full-screen unlock modals only in its in-memory profile to expose the acknowledgment/retry buttons; it is not human playtesting, physical-controller verification, random encounter balance or a complete survival campaign. Capture existence is checked in code; actual screenshots must be inspected after execution before making presentation claims.

## Observed validation - R24

Evidence root: `artifacts/gameplay-priority-followup-2026-10-17/`.

- `focused-attempt-1-summary.json` and its XML: 45/45 passed, zero failed/skipped, exit 0. This is the combined focused selection, including this track's 14 declared cases; it is not the complete project suite. The subsequent same-frame pickup guard is exercised by the fresh native build below.
- `survival-attempt-1/runner-spec.json`: actual player `Builds/GameplayParity20261017R24/Scrapshift.exe`, isolated controller-check input folder, windowed 1280x720. `process.json` records normal exit 0 after approximately 6.66 seconds, finishing October 18 at 03:24:12 UTC (October 17 Toronto).
- `survival-review.json`: Passed; every reported acceptance boolean true. Real hostile contact reduced HP to 8, iframes blocked the extra contact, ordinary regeneration raised it to 9, and cleanup fruit healed through Torture to 7.
- Lethal contact cleared actors; cleanup age stayed exactly `0.0166691039` while paused and acknowledgment appeared at `0.500457466` seconds. Spendable materials stayed 100, XP stayed 0, five raw materials banked, harvesting stayed unchanged, and the fruit placed at the dead player remained uncollected.
- The optional retry was activated through virtual A and actual Update/OnGUI. It restored wave 1, 10 HP, 100 materials, zero XP/bank and retry count 1. This is synthetic input using the fixture's explicit IgnoreFocus behavior, not physical Xbox verification.
- The native `player.log` contains no case-insensitive `error`, `exception`, `warning` or `assert` matches. This scoped log scan does not establish a warning-free project or all runtime paths.

All six PNGs were visually inspected from that exact attempt. `damaged.png` shows 8/10 HP and a -2 hit; `regenerated.png` shows 9/10 and +1 HP; `torture-cleanup.png` shows 7/10 and +3 HP. `defeat-ack.png` shows RUN LOST, Baby Alien attribution and the default OK button. `defeat-retry.png` shows Retry wave / End run with Retries: 0. `retried.png` returns to the living player in wave 1 with 10/10 HP and 100 materials. Screenshots corroborate presentation; JSON and actual frame-driven checks provide the timing/state evidence.

Remaining limits: no human non-invulnerable campaign, exhaustive character/lethal-effect combination matrix, physical-controller retest, optional optimized cleanup material-boost matrix or Builder-only conversion coverage. The local fixture suppresses pending full-screen unlock modals in its in-memory profile to expose loss/retry controls. These checks support the corrected survival lifecycle and do not prove universal gameplay parity.

## Final integrated verification - R25

The later final checkpoint supersedes R24 for the current build while preserving its historical evidence above.

- `suite-attempt-1.xml`: complete suite Passed, **1181/1181**, zero failed/inconclusive/skipped, approximately 67.98 seconds. The parent records test-process exit 0.
- `build-attempt-3-summary.txt`: Succeeded, **zero errors and warnings**, 401,713,034 bytes. The final native runner uses `Builds/GameplayParity20261017R25/Scrapshift.exe`; `build-manifest-r25.json` preserves the build's file hashes.
- `survival-attempt-2/survival-review.json`: Passed, every reported acceptance flag true, HP values 8 / 9 / 7, materials 100, XP 0, banked 5, retries 1. Pause held cleanup age at `0.0166714005` seconds and acknowledgment occurred at `0.500011384` seconds.
- `survival-attempt-2/process.json`: exited 0, approximately 6.59 seconds, finishing October 18 at 03:30:49 UTC (October 17 Toronto). Its native log has no case-insensitive error/exception/warning/assert matches.

This reviewer independently inspected all six fresh R25 PNGs. They show the same expected damage/regeneration/cleanup HP states, default OK acknowledgment with Baby Alien attribution, optional Retry wave / End run buttons, and restored living wave-1 state with 10/10 HP and 100 materials. The actual virtual-A retry and preservation of fruit at the dead player's location passed again. No additional presentation defect was observed in these six scoped captures. The synthetic-input, in-memory modal suppression, human-survival and exhaustive-combination limits above still apply; this is not a new full campaign or physical-controller retest.

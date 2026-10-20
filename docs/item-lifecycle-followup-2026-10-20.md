# Item lifecycle follow-up — October 20, 2026

## Scope

Source-derived, scene-free checks for imported item acquisition order, removal, mixed permanent wave growth, and combined on-hit effects. These add finite interaction coverage after R31's 53-case dynamic oracle. They do not establish arbitrary item-combination parity, natural physics scheduling, UI interaction, or human play balance.

No desktop control was used. The supplied Brotato project remains read-only. Codex performed the investigation and authored these tests; this is not a claim that the user independently performed the debugging.

## Reference findings and acceptance cases

- `singletons/run_data.gd:1242–1293`: `apply_item_effects` calls `_apply_gain_stat_for_equipped_item_with_stat_effects` only after an actual positive stat change from an item. `unapply_item_effects` invokes effect unapply without issuing that hook. `run_data.gd:2014` credits the reward permanently. **Two cases** acquire Snowball before/after Boiling Water, remove both, acquire Water while Snowball is absent, then reacquire Snowball and another Water. Only positive acquisitions while Snowball is present earn its bonus; removal preserves earned bonuses. Snowball remains within its imported one-copy cap.
- `main.gd:1235–1275`: the wave timer applies `stats_end_of_wave` before temporary-stat reset. Leaf grants +3 Max HP/+1 regeneration/+1 lifesteal; Robot Arm grants +3 melee/+3 engineering/−1 Max HP; Vigilante Ring grants +3 damage. **Six cases** equip two legal copies of each, reverse acquisition order, and exercise neutral/Doctor/Renegade stat gains. Wave one accrues two sets; removal of one copy leaves one set for wave two; removal of all copies preserves prior permanent gains and stops growth on wave three.
- `entities/units/player/player.gd:493–503`: actual HP damage applies every temporary on-hit entry, whereas dodge and hit protection return before these entries. **One case** combines Masochist's +5 damage with two legal Triangle of Power copies' −2 each. It checks +1 net per actual damage event, wave reset, and no trigger from protected/dodged hits.
- `effects/items/temp_stats_per_interval_effect.gd` groups interval values by stat/interval/reset flag. The only imported **item** families with this trigger are Wisdom and Medikit, and both have `reset_on_hit=false`. **One catalog case** verifies that boundary. Existing artificial reset-on-hit tests cover a generic rule, not a currently playable reset-on-hit item family. Weapon timer behaviors are outside this catalog assertion.

## Validation

`Assets/Scrapshift/Tests/Editor/ItemLifecycleFollowupTests.cs` contains ten NUnit cases. Initial focused run (`artifacts/item-material-followup-2026-10-20/focused-attempt-1.xml`) passed eight and failed both Doctor acquisition orders at wave three. This was a fixture omission: Doctor's real Harvesting grows 5 → 6 → 7, and its accumulated income crosses the first XP threshold. Ordinary level-up grants +1 Max HP independently of the removed growth items (`main.gd:911`; Unity `RunRules.GainXP`). The corrected check preserves that character behavior, accounts for level-up HP, and compares before choosing an unrelated upgrade. No production change is warranted for that finding. Root agent records the corrected result after execution.

## Remaining limits

- These checks call public `RunRules` APIs; no native scene, physics callback, sound, controller or UI assertion is made.
- Negative/zero item-stat fields, other item sources, and Snowball's character gain interactions are being exercised by the separate differential source oracle owned by the root agent.
- Temporary interval effects, consumable caps and burn-kill counters still require explicitly scoped acquisition/removal timing cases before broader claims. The imported single-copy Extra Stomach cap does not authorize testing impossible ordinary duplicate stacks as normal gameplay.
- Exhaustive arbitrary triples, every ordering across every wave, and natural-run balance remain open-ended coverage limits rather than a finite proof of exact parity.

## Interview discussion

Explain why removal has two distinct responsibilities: remove the item's future trigger, and retain permanent gains already earned. Contrast that with temporary wave stats, which reset at settlement. A useful rehearsal is tracing one Snowball acquisition through `ApplyItem`/`OnItemStatAdded`, then tracing removal through `RemoveItem`, and explaining why a passing data-catalog audit alone cannot verify these lifetimes. User direction defined exact-reference behavior; the code/test investigation here was assistant-run and should be attributed accordingly.

Root focused attempt2:18/18 Passed, zero failures/skips, process exit0,0.9692116 seconds. Receipt: `artifacts/item-material-followup-2026-10-20/focused-attempt-2.xml`.

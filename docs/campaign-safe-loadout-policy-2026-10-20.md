# Controlled campaign acquisition policy — October 20

## Preserved full-run observation

`artifacts/native-followup-2026-10-20/full-campaign/campaign-check.json` records R36's normal-timing comparison ending Lost after 12 completed waves, during wave 13. It reports 273 net damage, 251 net healing and two Blood Donation copies. The report is Failed because its non-survival acceptance requires all 20 waves; the trace remains preserved. This observation is not evidence of a gameplay payout or damage defect.

The old report's unqualified word “invulnerability” was too broad. Supplied original `Player._ready` (`player.gd:114–115`) checks `RunData.invulnerable` and disables the contact hurtbox. Original `Player.take_damage` has no such global gate. `_on_LoseHealthTimer_timeout` (`player.gd:845–850`) explicitly bypasses invincibility and calls damage directly. Blood Donation supplies `lose_hp_per_second = 1` and +40 Harvesting per copy. Contact immunity therefore does not protect either game's campaign from an acquired periodic HP cost.

Scrapshift correctly applies periodic self-damage separately in `ConditionalEffects.cs`, bypassing armor/dodge/contact invulnerability while respecting hit protection. No RunRules, HP, periodic-damage or ordinary gameplay behavior is changed for this follow-up.

## Explicit opt-in controlled policy

`--campaign-safe-loadout` changes only CampaignCheck's scripted acquisition choices, and only when `--campaign-survival` is absent. Default campaign acquisition and survival acquisition remain unchanged.

- Skip a shop offer whose item flat/conditional effects or weapon traits contain a **positive** `lose_hp_per_second` effect, including its `stat_`-prefixed form.
- Recycle such a crate reward through the existing `ResolveCrate(false)` path.
- Detect effect semantics rather than hardcoded item IDs. Ordinary negative modifiers and zero/negative HP-loss values remain allowed.
- Preserve offered objects, catalog values, acquired items, current HP and damage rules. No item removal, health restoration or blanket immunity is added.

Trace events identify `purchase_skipped_policy` and `crate_recycle_policy`. The final report identifies the opt-in policy, filtered definition IDs, skipped/recycled counts and `damageImmunityScope = contact_hurtbox_only`. Its scope explicitly says the historical original campaign did not filter acquisition this way.

This is a controlled wave/timer/combat integration policy. A completed run under it is not an identical purchase-history comparison with the historical original, natural survival evidence or proof of universal gameplay parity. Keep the preserved unfiltered loss alongside any completed controlled run.

## Focused checks

`CampaignLoadoutPolicyTests` contains 12 cases covering explicit opt-in and survival exclusion, actual imported Blood Donation, positive-effect semantics and unrelated penalties, conditional/weapon carriers, unchanged effect objects, and ordinary Blood Donation continuing to grant its modifier and deal one HP each second. Focused12/12 Passed (`campaign-policy-focused.xml`), and the integrated R40 suite passed1286/1286 with zero failures/skips. Fresh R40 and original runs with the matched opt-in policy both completed20 waves/19 shops, exit0. The18-check comparator passed after accepting the completed Python runner receipt schema; its first metadata-only failure is preserved. See [fresh comparison](full-run-r40-comparison-2026-10-21.md).

## Optional matched original policy

A fresh isolated original campaign can apply the same decision rule without modifying the supplied reference:

1. Inspect each `ItemParentData.effects` entry for key `lose_hp_per_second`/`stat_lose_hp_per_second` and positive value before the scripted shop purchase.
2. Skip that offer in the existing fixture loop before `container.on_shop_item_buy_button_pressed(item)`.
3. For a crate, call original `upgrades_ui._on_discard_button_pressed(item, 0)` instead of its Take callback when filtered; both emit the existing consumable selection result.
4. Log the policy and each decision, leaving original Player damage and RunData contact invulnerability unchanged.

That would match the acquisition rule across new controlled runs; engine-specific RNG still gives different offers/purchases. Historical original traces retain their prior policy. No reference adaptation or process launch was performed by this agent.

## Attribution and interview note

The user requested the remaining parity checks. Codex's source audit corrected an initial proposal to block all damage: the actual reference debug option only prevents contacts. The chosen opt-in policy makes a test experiment's acquisition choices explicit while preserving production behavior and failed evidence. The transferable lesson is to distinguish a failed test objective from a gameplay defect, and to document a changed experiment rather than silently broadening invulnerability. The user should reproduce and explain this distinction before presenting it as personal debugging work.

## Executed matched reference policy

`prepare_safe_campaign_reference.py` changed only the isolated clone's `parity_fixture.gd`; all3816 supplied-original inputs stayed unchanged. Before/after fixture bytes and source manifests are saved in `reference-safe-policy-preparation`. Both fresh runs used the same declared rule. Unity skipped one Blood Donation offer; source skipped none, and neither recycled a filtered crate. Random acquisition histories still differ. Actual callbacks, damage and catalog definitions were not replaced.

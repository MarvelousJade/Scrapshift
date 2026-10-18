# Full-run gameplay comparison — October 15, 2026

Both the supplied original and Scrapshift completed a **normal-time 20-wave integration run**, including all 20 timer ends and 19 shops, and reached Won. The original process exited 0; Scrapshift's final check reports Passed/Won. These were invulnerable scripted runs with supplied gear. They establish working full-run transitions and concrete behavior evidence; they do not prove every gameplay rule is identical or ordinary survival balance.

## Exact evidence and build

The primary comparison uses [original realtime trace](../artifacts/gameplay-parity-reference-2026-10-15/realtime/trace.json), [original summary](../artifacts/gameplay-parity-reference-2026-10-15/realtime/summary.json), [original source/clone manifest](../artifacts/gameplay-parity-reference-2026-10-15/realtime/source-manifest.json), [Scrapshift realtime trace](../artifacts/gameplay-parity-unity-2026-10-15/realtime/campaign-trace.json), [Scrapshift report](../artifacts/gameplay-parity-unity-2026-10-15/realtime/campaign-check.json), [frozen build manifest](../artifacts/gameplay-parity-unity-2026-10-15/realtime/build-manifest.json), and [machine-readable comparison with input hashes](../artifacts/gameplay-parity-unity-2026-10-15/comparison.json).

Branch: `codex/gameplay-parity`. The tested realtime runtime DLL SHA-256 is `CDB71FBA2194FC45C1E413CABB4E866549A42415FEE2FF8A8B29549A700FA09E`; executable SHA-256 is `001AEEBFB92718F8B49F0C0E0A03E313C54669552B08E952B2FEE3DDD143683A`. Later controller diagnostics, the final deterministic combine-order correction and the newer Input System migration are separate changes and were not in that running binary.

Both start at level 0 with 0 materials, no items and neutral character effects, Danger 0 and the same six tier-II weapons: pistol, SMG, double barrel shotgun, laser gun, knife and spear. The reference needs a Well-Rounded identity for UI but omits its character effects; Unity uses neutral rules. Both use invulnerability, circular scripted movement, first offered upgrades, crate takes and affordable real shop purchase callbacks. Both are muted.

The original runs at Engine time scale 1 and physics 60 Hz, confirmed in every snapshot; Unity uses ordinary frame delta capped at 50ms, a 60fps render target and 60 Hz item physics. The original profile directory was new, with no saves before launch; its empty logs directory was precreated, and boot counters confirm 0 enemies, trees, pickups, bank, materials, items, harvesting and Luck. Original supplied source and the user's independent game window were not edited or operated. The source manifest records 3816 original/copied source-resource hashes, twelve previously documented clone-only path/startup/movement differences and the added fixture hash.

## Observed results

| Observation | Original | Scrapshift |
| --- | --- | --- |
| Wave timer ends / shops | 20 / 19 | 20 / 19 |
| Purchases / upgrades / crates | 30 / 17 / 8 | 45 / 23 / 11 |
| Final materials / banked value | 385 / 1832 | 2479 / 8 |
| Final level | 17 | 24 |
| Item entries / unique IDs | 34 / 31 | 52 / 43 |
| Enemy kills / tree kills | 4968 / 56 | 4639 / 46 |
| Collected material objects | 1218 | 3641 |
| Harvesting / Luck | 77 / 8 | 31 / -9 |

Every recorded purchase debited its displayed price: **30/30 original and 45/45 Scrapshift**, with no mismatches. Original purchase events precede the callback, so the next purchase/shop-exit snapshot supplies the resulting balance; Unity records explicit before/after balances. This verifies internal debit consistency, not equal offer selection or price modifiers across differently equipped runs.

Final inventory stack sums match item entries (34 original, 52 Unity). Every Unity snapshot's stack sum also matches its item count. The complete stack records are in comparison.json. These records establish acquired multiplicities; dedicated effect tests establish formula behavior, refresh rules and stack caps.

Final original weapons: smg 2, double_barrel_shotgun 2, knife 2, laser_gun 3, spear 4, pistol 3. Final Unity weapons: pistol 3, smg 3, double_barrel_shotgun 3, laser_gun 2, knife 2, spear 3. Both keep six weapons and acquire real tier upgrades; different random offers produce different weapon builds.

## Progression gap and item effects

The comparison does **not** attribute every outcome difference to randomness. Source-backed material differences were found and corrected before this realtime run: a 50-object ground cap with overflow merged into random existing piles; bank bonuses reserved when new pickups spawn; Gatherer counting picked-up objects; and source pickup timing, collision geometry and collector ownership. Both runs now record a maximum of 50 ground material objects.

Through wave 9 the material traces are close: source / Unity timer-end money is 233 / 225, bank 261 / 231, and enemy kills 1275 / 1221. Unity's shop-10 purchase snapshot then records **one Lootworm**, bought for 62; the original acquisition trace never records Lootworm. Between waves 10 and 11, Unity's collected-object counter rises from 399 to 673 while the original rises from 397 to 459. By wave 12, the counts are 971 / 515 for Unity / original, with Unity ground value 34 at wave 11 versus original 222. The inventory change and collection telemetry support Lootworm as a contributor to the later income/XP gap.

The supplied `items/all/lootworm/lootworm_effect_0.tres` sets a 10% doubling chance; `lootworm.update_data` applies that effect over its scene default. Unity's serialized PetSpec also uses 0.1 and applies it before crediting a pet pickup. Actual successful double events were not counted individually, so the trace does not quantify their exact contribution. Gear, upgrades, damage and later acquisition histories also differ.

Final unspent resources demonstrate why bank and spendable money must be separated: original has 385 spendable + 1832 banked = 2217; Unity has 2479 + 8 = 2487. Unity also bought more gear. Purchase debits plus final spendable money give net currency after other gains/losses of 3089 original and 5722 Unity; these are not pure material-drop totals.

## Aligned timer-end telemetry

Each cell gives original / Unity values. Enemy kills exclude trees; pickup counts are objects rather than currency value. Timer-end events precede harvesting/intermission completion, though engine callback ordering and sampling still differ.

| Wave | Enemy kills | Material objects collected | Spendable money | Banked value | Harvesting | Luck |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | 25 / 27 | 10 / 3 | 10 / 3 | 0 / 0 | 0 / 0 | 0 / 0 |
| 2 | 66 / 68 | 27 / 12 | 35 / 18 | 0 / 0 | 0 / 0 | 0 / 0 |
| 3 | 124 / 130 | 44 / 36 | 36 / 63 | 0 / 0 | 0 / 0 | 0 / 0 |
| 4 | 207 / 209 | 65 / 69 | 41 / 107 | 0 / 0 | 0 / 0 | 0 / 0 |
| 5 | 339 / 333 | 99 / 113 | 109 / 102 | 32 / 1 | 0 / 5 | 0 / 5 |
| 6 | 498 / 489 | 142 / 155 | 99 / 131 | 92 / 45 | 6 / 6 | 0 / 4 |
| 7 | 704 / 684 | 204 / 221 | 127 / 136 | 134 / 90 | 15 / 15 | 0 / 4 |
| 8 | 858 / 830 | 259 / 267 | 119 / 117 | 205 / 178 | 16 / 16 | 0 / 4 |
| 9 | 1275 / 1221 | 323 / 348 | 233 / 225 | 261 / 231 | 17 / 15 | 0 / 4 |
| 10 | 1605 / 1535 | 397 / 399 | 235 / 348 | 497 / 507 | 22 / 16 | 0 / 4 |
| 11 | 1839 / 1772 | 459 / 673 | 235 / 769 | 657 / 548 | 24 / 17 | 0 / 4 |
| 12 | 2126 / 2054 | 515 / 971 | 228 / 1101 | 773 / 243 | 26 / 18 | 0 / 4 |
| 13 | 2410 / 2323 | 583 / 1234 | 220 / 1537 | 925 / 27 | 28 / 19 | 0 / 4 |
| 14 | 2692 / 2595 | 640 / 1491 | 213 / 1720 | 1047 / 0 | 32 / 20 | 10 / 4 |
| 15 | 3151 / 3047 | 712 / 1844 | 219 / 1771 | 1150 / 0 | 37 / 21 | 10 / 4 |
| 16 | 3491 / 3356 | 747 / 2188 | 108 / 2003 | 1409 / 0 | 42 / 23 | 8 / 4 |
| 17 | 3720 / 3563 | 890 / 2522 | 313 / 2066 | 1514 / 0 | 48 / 25 | 8 / 4 |
| 18 | 4100 / 3898 | 1009 / 2836 | 438 / 2147 | 1466 / 0 | 53 / 27 | 8 / 4 |
| 19 | 4683 / 4377 | 1157 / 3304 | 575 / 2552 | 1427 / 0 | 60 / 29 | 8 / 4 |
| 20 | 4968 / 4639 | 1218 / 3641 | 317 / 2479 | 1604 / 0 | 68 / 31 | 8 / -9 |

All 20 configured durations match the original `zones/zone_1/NNN/wave_N.tres` values exactly: 20, 25, 30, 35, 40, 45, 50, 55; eleven 60-second waves; then 90 seconds, totaling 1050 active seconds. Observed intervals are preserved in comparison.json. Source scene-start sampling and Unity's active-combat clock have different boundaries. Total wall times (1145.2s source, 1089.1s Unity) include different intermission/callback processing and are not performance benchmarks.

## Last discrepancy found by comparison

Reconciling weapon inventories exposed another concrete difference: source `base_shop.gd` removes both combining copies using `RunData.remove_weapon` and appends the upgraded weapon using `RunData.add_weapon`. The tested Unity binary replaced a previous index, changing weapon order and mount placement. Current source now appends the incoming purchase, removes the first two equal copies and appends the upgrade; manual combining follows the same source order. Five new `WeaponCombineParityTests` cover this deterministic behavior. The parent task verifies those tests and the resulting delivery build separately; the completed realtime traces remain tied to their frozen hash above.

## Pickup acceptance rules

`items/global/item.gd` defines pushback toward a sampled square destination, followed by a 10/60-second idle interval. Pickups become monitorable during idle, allowing contact and attraction assignment, while movement toward the collector waits until idle finishes. Attraction starts at 500 reference pixels/s and gains 20 pixels per physics tick without a speed cap. Normal runtime uses 60 Hz fixed item substeps; the historical accelerated fixture explicitly used 240 Hz.

Collision uses the source 16-pixel half-extent box, rotated for gold and unrotated for consumables, with logical bonus scale 1.25 and merge growth +0.05 capped at 2, independent of Scrapshift artwork scale. Empty pet/Evil circles use [Godot 3.6's documented 10-pixel default](https://docs.godotengine.org/en/3.6/classes/class_circleshape2d.html), yielding 100-pixel attraction / 30-pixel pickup after scene scaling. Player circles are explicitly 150 / 32 pixels. Scene offsets also participate.

Per-collector entry tracking preserves source assignment, takeover and contact events. A blocked entry cannot retry merely because ownership or distance changes while overlapping; leaving and re-entering permits another attempt. Player takeover uses the source's unusual player-to-item versus player-to-collector distance comparison. Dead collectors release ownership/reset speed; player takeover retains speed. Actual damage explicitly rechecks nearby healing consumables; already attracted consumables keep following after full health. These rules have focused regressions in `MaterialCollectionParityTests`.

## Native captures, diagnostics and limits

Both realtime end images were visually inspected. The [original image](../artifacts/gameplay-parity-reference-2026-10-15/realtime/end.png) reads Run Won — Crash Zone — Danger 0, with level 17, Luck 8 and harvesting 77 matching telemetry. The [Unity image](../artifacts/gameplay-parity-unity-2026-10-15/realtime/end.png) shows a queued Drifter unlock card over wave 20. Its victory is established by the completed trace/report, not a visible victory title in that image.

The original completed with zero SCRIPT ERROR entries. Preserved diagnostics include two startup CrashReporter path errors, one not-inside-tree condition, inherited Attach warnings and exit resource cleanup diagnostics; it was not warning-free. The user's original game source and save profile remain separate from this clone's evidence.

These runs do not validate ordinary survival damage balance, human aiming, physical controller bindings, audio listening, every item formula/stack cap or identical random outcomes. Controller evidence, including the newer Input System migration, is a separate review performed after this full run. Engine physics, seeded draw ordering, choices and builds differ, so this pair remains evidence for specified rules and full-run integration rather than a blanket exact-parity claim.

## Historical accelerated evidence

Earlier [reference fixture report](reference-full-run-2026-10-15.md), original `corrected` reference telemetry and Unity `final` accelerated traces are preserved. They are superseded as the primary timing comparison by this realtime pair. Their time scale 8 / physics 240 overrides changed some per-tick counters and cannot establish ordinary timing parity. Their original results remain under `historical_accelerated_comparison` in comparison.json.

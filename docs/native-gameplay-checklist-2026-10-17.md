# Targeted native gameplay checks — October 17, 2026

Branch: `codex/gameplay-parity`. This checklist was prepared from current runtime fixtures and supplied read-only source. It is not a test result. The parent owns the exact new build path, hashes, launches and results. No build, Unity execution, UI control or full run was performed by this checklist agent.

## Existing built-player entry points

All three flags below take an output folder. Use the exact freshly built executable selected by the parent; do not reuse R8 to validate current source. These modes mute audio, stage actors and exit with a machine-readable report. Their screenshots require inspection separately from their assertions.

| Entry point | Useful evidence now | What it cannot establish |
| --- | --- | --- |
| `--pet-review <output-folder>` | Nine companion smoke cases, target reservations, regeneration stacks, production emitter/profile checks; six actual structure-projectile captures, four-direction turret and Cat aiming captures, Cat boost captures. Report: `pets.json`. | Full survival behavior, physical controls, audible cues, exact movement timing. Several cases directly tick component methods; they do not prove the production accumulator. |
| `--weapon-fidelity-review <output-folder>` | Each registered weapon/tier can attack stationary targets; staged melee poses and projectile art. Report: `weapons.json`. | The smoke loop now preserves its four-second duration as 240 direct **60 Hz** steps. It additionally runs six `UpdateWeapons` native contact probes, reported in `native-contact.json` and folded into the main result. Direct component calls still do not validate the production accumulator. Count actual report entries. |
| `--enemy-fidelity-review <output-folder>` | Each imported enemy/state advances 720 direct 60 Hz steps, finite movement, attack payload counts and charge phase transitions; staged warnings/projectiles. Report: `report.json`. | Player damage protection is forced to 999 seconds. This does not prove real incoming damage, the production accumulator or exact crowd/contact ordering. |

Do not combine review flags in one process: each starts a separate review state and may quit the application.

## Fixture repairs implemented before the next build

The following changes are fixture-only and require the parent to rebuild; no passing result is claimed here.

1. `PetReview.cs`, Jellyshield smoke case: its old call crosses from one unit left to one unit right and expects a block. Confirmed discrete endpoint semantics mean that both endpoints can be outside the shield. The fixture now asserts both an outside crossing is unblocked and an inside endpoint is blocked. This changes fixture geometry, not the source-derived collision rule.
2. `StructureReview.cs`, landmine case: exit now queues the common explosion, then the old assertion immediately checks damage. The fixture now advances `UpdateWeapons(1f/60)` before asserting. Source `landmine.gd:_on_Area2D_body_exited` defers explosion and `explosion.tscn` supplies the active hitbox window. A queued explosion has not yet dealt damage.

## Smallest contact comparison to add or perform in a targeted fixture

`RuntimeNativeContactReview.cs` now runs six targeted built-player cases at the end of `--weapon-fidelity-review`: flame-cap center/grazing, delayed zero/one/two frames and beam growth/one-hit behavior. It calls actual production `UpdateWeapons` with source-derived profiles and native 1000-HP Baby actors, and writes `native-contact.json`. Build/results remain pending. No new launch flag is needed. Currency, melee timing and simultaneous ordering still need separate evidence; focused Editor results are not native results.

| Case | Source-derived acceptance | Current evidence / gap |
| --- | --- | --- |
| Reachable flame-cap grazing | Source flame radius 44 px at `(24,4)`, Baby Hurtbox radius 32 px; horizontal flame starting `(0,100)` at 6000 px/s, target center `(174,174)`: no contact at 60 Hz. At the two nearest samples, x=124/224 and y=104; both distances exceed 76. | Native Godot source scenes observed no contact. New Unity focused geometry case matches it. The new native probe stages both center and grazing geometry. |
| Initial ordinary overlap | An ordinary enabled projectile can report its spawn overlap before its first movement; do not replace this with a swept path. | Native source overlap at initial event flush; Unity focused spawn-snapshot case. The initial ordinary spawn snapshot remains covered by focused checks, not a dedicated new native assertion. |
| Delayed zero/one/two frames | Source delayed lightning begins disabled, including zero delay. With stationary overlapping actual hit/hurt circles, native zero/one-frame delay enters at first deferred flush; two-frame delay at second. | Native source fixture and real `UpdateWeapons` focused cases. The new native probes assert initial pending rejection and after-movement flush contact. This does not claim identical engine callback phase. |
| Current beam scale | Particle Accelerator's `Contents/Hitbox` inherits the current animated cubic scale. No contact may be invented solely because an intermediate peak between physics samples would overlap. Expired beams cannot hit. | Focused actual-contact cases plus existing cubic/render checks. The new built-player probe checks initial narrow miss, later growth hit, and no second hit. No dedicated native Godot beam timing comparison yet. |
| Fast melee | Current physics pose decides overlap; do not replay 240 Hz intermediate poses. Recoil-completion sound event still occurs once even if the attack's active pose is missed by contact sampling. | Focused finished-pose/event-flag case. Existing melee screenshots are staged, and audio is muted. Listening and precise native tween/physics ordering remain separate. |
| Source callback target pool | Bounce/lightning select an ordinary enemy other than the struck unit, excluding Tree, Boss and Elite pools. Aim toward raw unit origin. | Focused helper eligibility; root owns ordinary bounce integration. Stationary weapon smoke alone is insufficient. |

The native source collision fixture uses actual source scenes and a custom receiver: see `artifacts/gameplay-parity-contact-native-2026-10-17/summary.json`, `results.json`, `delay-results.json` and captured source hashes. Its six observed contact cases and three observed no-contact cases are classified observations, not a human playtest or a production damage run.

## Currency checks without a campaign

Use an isolated staged shop/kill scenario with explicit starting state, rather than waiting through an entire run.

- **Critical weapon income:** force a guaranteed tier-four Drill critical kill. `unit.gd` grants weapon `gold_on_crit_kill` immediately through `RunData.add_gold`; verify currency rises before collecting deferred drops, XP does not change from that grant, and no extra material actor is spawned for it. Keep Hunting Trophy/Tentacle ordering distinct if adding those items.
- **Demon shop:** stage a health-priced offer, record its displayed price, base Max HP and effective Max HP, purchase once, then verify source `RunData.remove_currency` applies the displayed debit to base HP before the character's stat-gain modifier. Root's current focused currency audit is the authority for exact numeric setup; do not guess a nominal effective-HP subtraction.
- **Arms Dealer fractional weapon price:** stage a base-price-50 weapon at wave two with the source character modifier; the source keeps adjusted base as float until the final truncation, producing the independently recorded price 5 rather than 4. Confirm displayed price and actual debit agree.

`ReferenceReview` can display a staged shop but advances several waves through rule calls and does not assert these currency relationships. Its screenshot is not native currency validation. None of the three primary fixture modes above supplies these precise currency checks today.

## Another stale fixture left outside this pass

`CombatFeelReview.cs` still advances one piercing probe by `.1` seconds and expects targets along the skipped path to be hit in travel order. That assertion was authored for the superseded swept-contact model and must be repaired before using that optional mode. The parent is not running that mode in this native pass; this is a fixture limitation, not a newly observed production bug.

## Completion boundary

Before the eventual full run, record results against the exact new executable, inspect the production support captures for attached weapons/muzzle alignment, and retain any source/native timing mismatch as open. Physical Xbox confirmation, audible feedback, Godot deferred-event ordering, melee/beam native timing and crowd physics cannot be closed by the existing stationary fixtures alone. Full campaign and whole-suite status must remain separate.

The user supplied direction and the original project. Codex inspected current fixtures/source and authored this checklist; this is not evidence that the user personally executed these checks.

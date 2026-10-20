# Death timing and item chains — integration, October 20, 2026

Branch: `codex/gameplay-parity`. This pass follows R34's captured-target and vulnerability fixes. It checks when damage happens, when a death becomes final, and which effects an item explosion may activate. Agents used source inspection and isolated windowless fixtures; the supplied original remains read-only.

## Confirmed fixes

| Difference | Integrated behavior | Distinguishing evidence |
| --- | --- | --- |
| Lich/Elephant/Cyberball damage waited until the end of a combat step | Apply the selected hit synchronously, while deferring enemy death finalization | Lethal Lich immediately leaves 0 HP and rejects a subsequent weapon hit. Lich before Giant Belt leaves 61 HP, rather than the delayed path's 60. |
| Synchronous damage could invalidate indexed traversal if it also removed enemies immediately | Keep the pending victim in the candidate list until an explicit safe death boundary; reject additional hits at 0 HP | Pending/finalized checks require exactly one corpse, kill and finalization despite a repeated boundary drain. |
| Removing the dying enemy before Main-equivalent callbacks changed Cyberball's target pool | Retain its zero-HP candidate during those callbacks, then remove it | Actual Godot signal dispatch with scene-like Main/spawner construction sees the corpse in both fresh and reconnected cases. |
| Plain Rip & Tear/Spicy Sauce explosions inherited Sausage weapon burning | Supply their original empty burning payload | Four legal Sausages must not add burning or activate Greek Fire on these plain item explosions. Beard projectile burning remains enabled. |
| Plain item explosions ran the weapon/projectile life-steal callback | Use the explosion callback context, restored with `finally` | Missing player HP and the life-steal cooldown remain unchanged. Beard's projectile callback remains eligible to heal. |
| Deferring global kill totals would break per-explosion Fireworks counting | Count that explosion's immediate living-to-lethal transitions, excluding already-zero-HP candidates | 14/15 direct-kill boundaries remain false/true before the global death queue is flushed. |

The death finalizer follows the supplied Main callback order: Cyberball damage, Beard projectiles/explosions, burning-item stat gains, then loot and progression. The Will-o'-the-Wisp achievement counter remains immediate on a lethal burning hit; the item's burn-kill stat gain remains deferred. Those are separate events.

Two additional cleanup boundary cases check enemy-lethal-before-player-lethal and the reverse order. `BeforeDeathCleanup` drains older enemy requests while the run is still Playing; player death first prevents later enemy hit rewards. Their complete source Player/Main cleanup equivalence is still bounded by the narrower source probe described below.

## Source receipts and a rejected hypothesis

Receipts are preserved under `artifacts/death-item-followup-2026-10-20`.

- **Death timing:** `death-source-attempt-2/reference.json` records actual Main/Enemy/Unit damage calls. Lethal Lich sets pending death synchronously, rejects the later weapon hit, and invokes the deferred death sink exactly once. The Giant Belt sequence ends at 61 HP. Two additional Unit cases preserve both enqueue orders through deferred dispatch. The probe suppresses rendered flash and replaces the final `die` body with a counting sink; it does not execute complete Player/Main cleanup. Process exit: 0.
- **Corpse membership:** `corpse-source-attempt-2/reference.json` contains 1,024 rows: 256 seeds for each of two target-construction orders and two connection lifecycles. Scene-like Main-before-spawner construction retains the zero-HP corpse in all fresh and reconnected callbacks. Both cohorts have 25 live-target Cyberball hits. Reversed construction is a diagnostic control: both cohorts exclude the corpse and have 61 live-target hits. All rows finish with one active enemy. Process exit: 0.
- **Explosion payloads:** `chain-source-attempt-1/reference.json` Passed with 70 non-burning payloads across legal Rip & Tear copies 1–5 or Sauce copies 1–2, Sausages 0–4, and Greek Fire absent/present. It executes the original source-extracted `handle_explode_effect`, replacing only its two deferred scene-emission calls with a recorder. It retains original resources, probability, damage calculation and payload assignment, and records the source hash. Process exit: 0.

**The pool-only ordering hypothesis was falsified.** Reading connection insertion order initially suggested fresh and reused enemies would expose different candidates. The first 512-row runtime probe contradicted that prediction. A revised probe used a Main subclass as the actual callback target, recorded object IDs and reversed target construction as a control. It reproduced the fresh/reconnected equality in the scene-like order. The proposed unhooked pool-metadata helper and its tests were removed before integration. These observations describe the supplied runtime and targets; they do not establish a universal Godot signal-order guarantee.

The 70-payload explosion probe does not instantiate explosion hitboxes. The corpse probe uses actual signal dispatch and spawner removal/cache methods, but a recording enemy damage sink and inert animation/pool dependencies. Neither probe is a complete original combat scene or a cross-engine identical-RNG playthrough.

## Validation checkpoint

| Layer | Current result |
| --- | --- |
| Reproduced item baseline | `item-baseline-3.xml`: 7 cases, 1 Passed / 6 Failed. Four burning/Greek Fire and two life-steal differences reproduced; Beard control passed. |
| Earlier fixture corrections | Earlier attempts incorrectly required the default-unlock `available` flag for earnable locked rewards. Corrected to stage the post-unlock loadout and enforce actual stack caps. These are fixture defects, not gameplay bugs. |
| Combined focused checks | `focused-postfix-1.xml`: 25/25 Passed, including nine item-chain, seven death-dispatch, two corpse-membership and seven preserved target-context checks. |
| Added cleanup boundaries | `focused-postfix-2.xml`: **27/27 Passed**, zero failures/skips, exit0, 0.6664488 seconds. Includes both additional cleanup-order cases. |
| Complete test suite | `suite-attempt-2.xml`: **1,271/1,271 Passed**, zero failures/skips, exit0, 64.1649722 seconds. First attempt had one old immediate-global-kill assertion; corrected to check immediate explosion kills and deferred global totals separately. Its failing receipt is preserved. |
| R35 packaged player | Build and execution pending. Do not treat fixture source or a successful compile as player acceptance. |

The opt-in packaged fixtures are `--headless-death-timing-review <folder>` and `--headless-item-chain-review <fresh-folder>`, under `-batchmode -nographics`. They wait for actual ArenaGame initialization and verify its production hooks, then disable normal ArenaGame updates to stage synchronous callback orders using real spawned NPCs. The item fixture also records actual fresh JSON progression profiles for Fireworks and retains a Beard positive control. These are packaged callback checks, not normal collision or movement tests.

## Remaining limits

Arbitrary larger loadouts, all real-time collision/death interleavings, full source Player/Main terminal cleanup, visual death presentation, audible feedback and human survival balance remain separate acceptance work. The old 25-kill/24-unit material trace remains causally unassigned; these death receipts do not identify its historical cause or establish a payout defect. R29's full 20-wave comparison remains historical and must not be relabelled as a campaign on the integrated death rules.

Desktop access is now allowed again under the user's latest direction. Physical Xbox checks are skipped at the user's latest request; no new hardware acceptance is claimed. The headless work documented here did not control the desktop.

## Interview and attribution notes

The engineering lesson is to separate immediate state changes from deferred lifecycle callbacks. Reserving a target was insufficient when later effects read its current HP; applying damage immediately still required stable collection membership until removal was safe. A second lesson is to measure actual callback dispatch before designing a pool-specific fix, and preserve the failed hypothesis as evidence. Finally, sharing damage code does not mean item explosions inherit every weapon callback: payload construction and life-steal ownership require separate inspection.

The user directed parity scope, supplied the reference and reported prior gameplay observations. Codex agents investigated these cases and authored the changes, probes and checks. This document supports rehearsal; it does not claim the user independently performed this debugging or new human playtesting. The user should reproduce and trace a focused case before describing it as hands-on experience.

Further detail: [death dispatch](death-dispatch-followup-2026-10-20.md), [corpse membership](corpse-membership-followup-2026-10-20.md), [item chains](item-chain-followup-2026-10-20.md).

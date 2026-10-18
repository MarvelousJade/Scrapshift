## Latest native checkpoint — R14, October 17

R14 is the latest unpacked build (`Builds/GameplayParity20261017R14/LaunchGame.cmd`), zero build errors/warnings. Only the opt-in campaign driver changed from R13: it now uses the normal60Hz combat clock. Its normal-speed one-wave preflight Passed, exit0, reached Shop after20.0165 simulated seconds. Both fresh captures were inspected. It did not run a full campaign.

Carried input-matched evidence:172 focused regressions passed; R13 support43/43, economy and virtual navigation17 snapshots passed; R12 enemy53/eight slash probes and combat passed. Requested basic Xbox controls were confirmed by the user and logged separately. Unlock-pool timing and support projectile behavior fixes remain intact. No whole suite or full20-wave comparison ran. See [latest evidence and attribution](native-gameplay-r14-2026-10-17.md). Earlier R13 notes saying the campaign driver still needs a cadence correction are superseded by R14.

## Latest native checkpoint — R13, October 17

Fixed active-run unlock-pool refresh and support projectile animation/identity/offset/current-age presentation. R12 selected regressions passed172/172 (zero failures/skips); R13 only changes two native fixture files, with identical production/test inputs. R13 build succeeded with zero errors/warnings. Actual R13 native economy Passed, support43/43 Passed, virtual controller navigation Passed17 snapshots, all process exits0. R12 combat and enemy53/eight-slash-probe passes carry unchanged relevant source. The user accepted requested basic physical Xbox controls; B-not-opening-wave-pause is closed as reference behavior. No new broad hardware support claim.

The whole suite and full campaign have not run. Before a new campaign comparison, its older variable-step fixture path needs adaptation/checking against current60Hz normal gameplay. See [current native checkpoint](native-gameplay-r13-2026-10-17.md) for manifests, preserved initial failures, reviewed screenshots and limits. Older pending statements below are historical checkpoints. Codex performed implementation/testing; the user supplied direction/reference and actual basic Xbox feedback.

# Focused native continuation after R11 — October 17, 2026

The user accepted B doing nothing during combat as reference behavior: Menu/Start opens pause and B returns/resumes. Requested basic Xbox checks are accepted from user feedback and the observer log. No repeated basic hardware test is required. Shortcut/device combinations outside that scope remain separately labeled.

## Parallel ownership

| Owner | Files | Acceptance before full run |
| --- | --- | --- |
| Economy agent | ProgressionRuntime, first StartSelectedRun hook in RunPresentation, EconomyNativeReview, EconomyNativeParityTests | Freeze item/weapon random eligibility until the next run; keep immediate reward notifications/character availability; native purchase/removal/stack/pool/unlock and labeled synthetic shop hooks |
| Support agent | SupportProjectileArt/Behavior, StructureReview, PetReview, SupportProjectileNextTests and one old expected routing assertion | Current-age sprite pose, per-type offsets, bullet/lightning frame progression, orange pet/blue turret flame identity using already shipped textures; production probes and native frame sequences |
| Enemy agent | EnemyFidelityReview and dated report | Preserve 53 state cases, add a bounded native standard/forward slash warning-to-active-to-expiry sequence |
| Root | ArenaGame wiring, ReferenceCombat age hook, CombatFeelReview, build/sync/evidence and shared docs | Integrate once frozen, focused checks only, current player captures and accurate result inspection |

## Findings and initial evidence

- Confirmed unlock difference: source ItemService constructs unlocked random pools at RunData reset. Scrapshift refreshed them during a run, allowing newly unlocked items into later shops/crates. The snapshot correction and focused/native checks are in progress.
- Confirmed support visual behavior differences: ordinary support bullets held their first flash frame; Tyler used generic bullet art; support poses used previous age; all sprite types used a 24-pixel offset. The orange pet flame also shared blue turret routing. Reuse existing approved/shipped textures; no new original art is imported.
- Existing combat-feel fixture failed its piercing/flash/push assertions in R11 because it moved a fast shot in one .1-second jump. This skips both targets under current discrete contact sampling. Only the fixture is changed to six 60 Hz steps; do not restore swept collision to satisfy it. Initial failure and captures are retained at `artifacts/native-remaining-2026-10-17/combat-initial/`.
- R11 native loot fixture passed all seven checks, exit zero, through actual drop, pickup, crate cleanup, take/recycle, upgrade and shop paths. Root inspected Item found and final shop captures. This uses scripted damage/XP/cleanup and is not a human survival run. Evidence: `artifacts/native-remaining-2026-10-17/loot-initial/loot-review.json`.
- Read-only review of nine enemy captures found no confirmed new attack-indicator mismatch. Warning screenshots do not establish active or expiry presentation; a bounded sequence is being added.

No whole suite or full campaign has run in this continuation. R11 passes predate the new production corrections; new validation results belong to the next synchronized build. Codex performed implementation/investigation; the user supplied direction, reference and confirmed basic physical input experience. Interview notes should distinguish faulty fixture assumptions from production mismatches and synthetic setup from ordinary progression.

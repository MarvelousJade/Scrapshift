# Native enemy evidence review — 2026-10-17

Scope: review of R10's native enemy report, selected captures and relevant supplied source, followed by a bounded fixture extension authorized by the root. Only `EnemyFidelityReview.cs` was changed; no production gameplay, metadata, artwork, build, editor state or original project changed. No new native fixture or full run was launched by this reviewer.

## Report and what it proves

`artifacts/gameplay-parity-r10-2026-10-17/enemies/report.json` reports **Passed**, with 53 enemy/state labels and an empty failure list. Its scope explicitly identifies staged captures and excludes identical physics, artwork or random sequences.

`Assets/Scrapshift/Runtime/EnemyFidelityReview.cs:24` advances each state through 720 calls at 1/60 second. This is twelve seconds of logical simulation per state, not twelve seconds of normal wall-clock gameplay. The fixture detects nonfinite movement, exercises charge release, checks projectile counts for applicable primary attacks and renders a staged state. It sets player invulnerability to 999 and does not establish normal damage/balance or complete encounter schedules by itself. Other contact-rule evidence should remain separately attributed.

## Images actually inspected

The following R10 PNGs were opened using the image viewer, not judged from filenames:

| Capture under `artifacts/gameplay-parity-r10-2026-10-17/enemies/` | Observed presentation |
| --- | --- |
| `charger-0.png` | Anticipating red charger, no long arrow attached to a generic charge. |
| `butcher-0.png` | Several long red-outlined, translucent warning arrows. |
| `mantis-1.png` | Charge-state anticipation without a generic arrow overlay. |
| `predator-1.png` | Boss body and a red/white plasma orb; no slash warning applied to this arm attack. |
| `croc-0.png` | Charge-state body without an invented slash arrow. |
| `invoker-1.png` | Multiple red plasma projectiles with readable white cores. |
| `mad_slasher-0.png` | Two intersecting directional slash warnings. |
| `tentacle-0.png` | Two forward-offset slash warnings sharing the actor's origin region. |
| `mom-1.png` | Forward-offset directional slash warning. |

Also opened the supplied `E:/UnityProjects/brotato-(3.x)/projectiles/slash/sprites/slash_0000.png`: it is the warning-arrow frame, with black source pixels that the enemy projectile shader maps to red. No claim is made that these static pictures establish movement, collision, animation timing or sound.

## Source qualification: arrows are not generic charge indicators

- The reference charger scene `E:/UnityProjects/brotato-(3.x)/entities/units/enemies/charger/charger.tscn:5` references `enemy_charge_prep_animation.tres`. That animation's position/scale/color tracks anticipate the charge, while method keys call `start_shoot` and `shoot`. `charging_attack_behavior.gd` moves/locks the actor and applies the charge speed; it does not create slash-warning projectiles.
- Actual slash scenes are assigned in reference `entities/units/enemies/butcher/butcher.tscn:4`, `mantis/mantis.tscn:3`, `mad_slasher/mad_slasher.tscn:6` and `slasher/slasher.tscn:6`. The forward variant is assigned in `croc/croc.tscn:3`, `mom/mom.tscn:3` and `tentacle/tentacle.tscn:4`. An enemy may have both charge and slash states; arrow qualification follows the attack's projectile, not the enemy's name.
- Scrapshift's `EnemyReferenceTests.cs:108` enumerates exactly those seven slash users and checks that charge attacks do not use slash projectiles. This is source/data coverage, distinct from the captures above.
- Reference `entities/units/enemies/predator/predator.tscn:81` positions its rotating projectile pivot at source y=-32; line 84 sets rotation speed 1.2. Its arm is a persistent projectile arrangement, not a slash arrow. The single orb capture is a staged frame, not proof that every arm segment extends/rotates correctly over time.

No confirmed arrow-qualification mismatch was found in the viewed captures and inspected source. Do not add arrows to all charge attacks as a generic correction.

## Warning versus active slash frames

Reference `projectiles/slash/slash_projectile.tscn:79` changes sprite frames at 0, .48, .51, .54, .57, .6 and .63 seconds. Hitbox event/shape tracks at lines 91, 114, 126 and 138 separately define the active collision sequence. The `_2` scene uses the same frames with a forward offset. Scrapshift's `EnemyReferenceTests.cs:116` checks the .48 transition and the distinct 165-pixel forward offset.

R10's enemy capture fixture sets slash presentation to **.2 seconds** (`EnemyFidelityReview.cs:59`), so its all-red warnings do not demonstrate the active slash's white detail. `EnemyProjectileTone.shader` preserves source green/blue channels while mapping black to red, so white pixels remain white; absence of white in the .2-second warning alone is not evidence of a shader defect.

## Bounded fixture extension, authored for R12

The root authorized closing this evidence gap before the integrated native review. `EnemyFidelityReview.cs` now retains the existing 53 cases and adds eight captures, each staged on a separate rendered frame: standard and forward slash at **.2, .48, .54 and .63 seconds**. It creates actual slasher/tentacle production attacks through `FireEncounter`, then isolates one projectile to make the presentation and health-contact assertions unambiguous. This is a labeled fixture, not a natural encounter.

The added `slashSequence` JSON records:

- Source frame indices 0, 1, 3 and 6 at those four times.
- Warning: live renderer matches frame zero; no contact or health loss.
- .48/.54 active stages: actual `UpdateSlashes` hit registration and health loss when the player's hurt circle is placed at the source collision center (-122 or -6.5 pixels along the slash, with the forward variant's +165-pixel offset).
- .63 expiry: removed from the live slash list, no contact/health loss, and the renderer's GameObject must be destroyed by the next rendered-frame check.
- Capture files must exist and be nonempty; original case failures and sequence failures remain visible in the combined report.

The report wording now calls the 720-step state exercise twelve **logical** seconds. This is an evidence extension, not a newly confirmed gameplay bug or a requirement for universal engine equivalence.

## R12 native result: inspected report and all eight frames

Independently read `artifacts/gameplay-parity-r12-2026-10-17/enemies/report.json`: **Passed**, 53 original case labels retained, no failures, and eight `slashSequence` records. All eight PNGs exist and are nonempty (75,450–90,592 bytes). The root's batch proceeded to the support fixture after the enemy player exited; this review's direct result evidence is the saved enemy JSON and images.

| Age | Sprite index | Standard / forward contact result | Native visual observation |
| --- | --- | --- | --- |
| .2 | 0 | Both live; no hit; health 10 → 10; renderer matches source frame. | Long red outline with translucent interior; terrain remains visible through the arrow. |
| .48 | 1 | Both live; hit registered; health 10 → 9; renderer matches source frame. | Short initial slash with visible white detail, red edge/glow and player hit flash. |
| .54 | 3 | Both live; hit registered; health 10 → 9; renderer matches source frame. | Broad active slash with a clear white center and red edge/glow. |
| .63 | 6 boundary | Both removed from live list; no hit; health 10 → 10; GameObject gone by the next rendered-frame check. | Slash warning/attack sprite is absent; player/enemy remain visible. |

Opened and inspected **all eight** R12 captures with the image viewer: `slash-standard-0.png`, `slash-standard-1.png`, `slash-standard-2.png`, `slash-standard-3.png`, `slash-forward-0.png`, `slash-forward-1.png`, `slash-forward-2.png` and `slash-forward-3.png`, all under the R12 `enemies` directory. The forward variant visibly places the slash ahead of the emitter. Native JSON also records the exact frame used, hit/health state and deferred destruction for each variant.

These are separately staged age probes, each creating a fresh real attack and positioning the hurt circle for that age. They are not frames sampled from one uninterrupted slash: direction/camera placement can differ between captures, so those differences must not be interpreted as animated jumps or timing evidence. The source-derived frames, active contacts and expiry now have bounded native coverage; ordinary real-time encounter timing and sound remain separate full-run/reference evidence.

No concrete mismatch was found in the eight images or their recorded contact/renderer assertions. The previously bounded slash warning/active/expiry presentation gap is closed for these two variants.

## Disposition before the full run

**Confirmed new gameplay/presentation mismatches from this review: none.** Existing native state coverage can support proceeding once the root's remaining confirmed fixes are settled. Preserve the distinction between staged simulation/captures and eventual normal-timing original-versus-Scrapshift run evidence. Random acquisition differences do not by themselves establish a gameplay defect.

Attribution: the user requested exact behavior and selective reference-qualified telegraphs. Codex performed this source/image review. Third-party reference source and its images remain source material, not independently authored user work.

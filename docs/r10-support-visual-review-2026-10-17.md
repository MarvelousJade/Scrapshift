# R10 support screenshot inspection — October 17, 2026

Build fixture: `artifacts/gameplay-parity-r10-2026-10-17/support/`. Codex visually opened **19 fresh PNG captures** from the parent-launched R10 player using `view_image`. These are staged native screenshots, not motion observation, listening, controller testing or a full run.

## Visible findings

| Captures inspected | Observation |
| --- | --- |
| `cat-aim-0.png` through `cat-aim-3.png` | Substantial Gatling assembly stays visually connected to the Cat's back/body in right/up/left/down aiming poses. No floating attachment gap is visible in these views. |
| `cat-boost-0.png`, `cat-boost-1.png` | Both captures show bright red eyes and an angry face, distinct from the gold-eyed ordinary aiming captures. They do not establish firing-rate change or animation timing. |
| `turret-aim-1.png`, `turret-aim-3.png`, `healing-aim-1.png`, `rocket-aim-3.png` | Head/barrel assemblies meet their body/pedestal without a visible floating gap. The downward rocket tips remain attached to their launchers. These are pose checks, not proof of rotation about the correct joint during motion. |
| `turret-live-projectile.png`, `flame-live-projectile.png`, `rocket-live-projectile.png`, `laser-live-projectile.png`, `healing-live-projectile.png`, `tyler-live-projectile.png` | Production-emitted sprites are visible. Bullet is yellow/white, laser magenta/white, healing green/white, flame includes a small orange muzzle flame and white projectile disc, rocket is a large angled slug; Tyler shows multiple yellow/white shots around its structure. Colors/shapes are distinguishable. A single time sample does not prove trajectory, muzzle-origin alignment, projectile lifetime or exact reference size/presentation. |
| `ratzilla.png`, `bot_o_mine.png`, `scapegoat.png` | Distinct appropriate companion bodies are visible, with attached robot components; Bot has a separate ground mine below it. Scapegoat has its own visible health bar. No body/attachment gap is apparent at these capture scales. |

## Capture defect requiring follow-up

`cat-aim-1.png`, `cat-aim-2.png` and `cat-aim-3.png` contain very large dark ground-shadow ovals covering substantial portions of the background; `cat-aim-0.png` does not show the same large centered oval. These captures must not be described as clean visual acceptance.

Read-only source inspection suggests a fixture setup cause: `GroundShadow.Initialize` initially places a shadow at scale one, while `SetUpPetCase` then repositions its actor without advancing `TickPets` or explicitly placing that shadow at the pet's actual scale. The Cat aim fixture stages its rig directly. This is a hypothesis to verify, not a demonstrated production shadow defect. No runtime or fixture changes were made during this inspection.

The bright projectile discs occupy a large fraction of the structure height in the live captures. This review records their actual appearance; without side-by-side original captures at equivalent scale it does not classify their size as matching or incorrect. Source mechanical radius checks do not validate visible sprite size.

## Machine report observed during inspection

The initial `support/pets.json` read **Failed**: 37 passed checks, with failures `cat alternate independent guns` and `decoy revival retains progress and grown max HP`. Those failures require source/fixture triage by the parent before reporting successful support validation. Still screenshots cannot settle them. A later rerun must retain this initial evidence and identify its own report/capture folder.

The parent's separate weapon result was reported as 205 weapon/tier checks plus six native contact probes passing; this document does not replace that report or independently rerun it. The support fixture's assertions and these visual observations remain different kinds of evidence.

## Source triage and fixture-only corrections awaiting rerun

The two initial failures were traced to unsupported or stale fixture expectations:

- Source `entities/units/pet/catling_gun/catling_gun.gd` independently chooses each gun's nearest target; both may validly choose the same enemy. It alternates the left/right cooldown handoff, resetting the other gun to half its interval. The old fixture additionally required shots with both positive and negative x velocity, which the source does not guarantee. The corrected fixture checks first/second timer handoff and additional shots, advances by the actual observed remaining right-gun cooldown, and logs both target positions, cooldowns, shot origins and velocities before its assertion.
- Source `entities/units/pet/scapegoat/scapegoat.gd:_physics_process` checks current health against maximum **before** adding the current frame's revival increment. Reaching full HP therefore precedes revival until the next tick. The old extended case asserted alive immediately after filling; the corrected case records that intermediate full-but-dead state, advances one more 60 Hz tick and checks the rise while retaining the grown maximum HP. Diagnostics include partial, held, filled and final states.

The staging shadow hypothesis is also supported by the actual call path: production `TickPets` places each shadow at `p.baseScale`, while the direct aim setup repositioned the actor without doing so after `GroundShadow.Initialize` used scale one. `SetUpPetCase` now explicitly places the shadow using the staged position and actual base scale. Fresh captures are required to verify that this removes the large ovals.

These edits change `PetReview.cs` only; production pet behavior and art remain unchanged. The initial R10 failed report and affected screenshots remain historical evidence. No corrected passing report or replacement capture has yet been read by this inspection agent.

## Remaining visual/timing limits

Not inspected here: continuous aiming/firing motion, leg or body animation, live muzzle-to-projectile transition, audible role-specific cues, actual boost cadence, contact timing, physical controller response, ordinary-run clutter or full-run comparison. No image-generation or asset processing was performed. User requirements directed the inspection; Codex performed it.

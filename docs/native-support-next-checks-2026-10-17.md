# Bounded support projectile follow-up — October 17, 2026

Branch: `codex/gameplay-parity`. Scope: explain the bright discs in the R11 captures, correct confirmed animation/registration identity gaps, and define the next small native evidence set. Supplied reference was read-only. No Unity, build, UI or native process was launched by this audit agent.

## Current native result: R13

The actual `artifacts/gameplay-parity-r13-2026-10-17/support/pets.json` was read: **Passed, 43 passed entries, zero failures**. The parent observed process exit zero and recorded the exact build/evidence hashes separately. Four production presentation probes each report `alive`, `ageOK`, `textureOK` and `offsetOK` true, zero hits and two temporarily excluded staged enemies. Ordinary/laser/healing rendered offsets are approximately `(0.00999999,-0.00999999)`; Tyler approximately `(0.03999996,0.00999999)`, within the authored tolerance. All four stepped production sequence checks passed.

Codex visually opened **eight fresh R13 PNGs**: `sequence-turret-00.png`, `sequence-turret-06.png`, `sequence-tyler-03.png`, `sequence-pet_flame-06.png`, `sequence-flame-06.png`, `laser-live-projectile.png`, `healing-live-projectile.png`, and `cat-aim-2.png`.

- Ordinary turret changes from its initial round flash to a narrow elongated yellow projectile farther right.
- Tyler displays cyan lightning bolts rather than generic yellow discs.
- Blazemander displays orange/yellow flames; the turret displays a distinct cyan/blue flame.
- The later laser frame is elongated magenta/white; healing is a green/white triangular pulse, replacing the formerly stale first-frame discs in the reviewed 0.04-second captures.
- The Cat's left-facing gun remains attached, with a small correctly placed ground shadow and no former large background oval.

This is native sampled-frame and fixture evidence, not uninterrupted motion observation, audio listening or full-run validation. The full run had not started at this review handoff.

### Preserved R12 failure and precise limit

R12's actual report remains **Failed: 42 passed, one `structure: turret` failure**, although all four sequence checks passed. Its earlier combined diagnostic printed correct age, texture name and rounded offset but did not log each condition, so the exact failed condition cannot be established retrospectively. The presentation fixture shared a live randomly located enemy pool with its preceding behavior check. R13 isolates that pool only during the separate presentation probe, restores it afterward, and logs each condition, survival, hit count, lifetime and unrounded offset. R13 passing in isolation supports the fixture repair; it does not prove which omitted R12 condition failed. No production collision rule changed for this repair.

### Initial flash overlap is not a confirmed gameplay mismatch

The reviewed own-art turret's initial flash overlaps its body. The actual supplied `turret.png` and `bullet/frame0000.png` were also opened: the original compact turret's muzzle is `(0,-12)` source pixels and the initial bullet sprite offset is `(1,1)`, placing the roughly 80-pixel visible flash only 11 pixels above its body center. Substantial body overlap is therefore also expected from the original assets/transforms. This observation is **not** a blocking gameplay discrepancy; preserve the source-derived firing origin. Scrapshift's longer barrel silhouette differs by design, and exact artistic fit is separate from mechanical parity.

## What the R11 discs actually establish

They are authored texture content, not demonstrated lighting inflation. Existing `SupportProjectiles/bullet.png` is byte-identical to supplied `projectiles/bullet/frame0000.png`; both are 120×120, with an alpha≥32 visible box 80×80. The later source `frame0001.png` has a much thinner 87×30 box. Source `bullet_projectile.tscn` switches between them at 0.02 seconds.

Current support art selected only the first bullet texture for ordinary turrets/Builder, so the flash remained a disc throughout flight. This is a confirmed animation mismatch, not a reason to shrink the entire projectile. The already shipped `WeaponReference/frame0001_a728a067` provides the missing later frame without importing any new image.

Laser and healing resources are also byte-identical to their supplied first/later frames. Their 0.02-second transitions were already present. However, `ReferenceCombat.UpdateWeapons` posed support sprites **before** advancing age, and `CaptureLiveStructureProjectile` then advanced once by 0.04 seconds. That capture therefore showed first-frame discs at a recorded age of 0.04. The parent moves posing after age advancement. This corrects the one-update visual lag and the staged frame mismatch; source frame dimensions stay unchanged.

## Confirmed identity and registration gaps

| Kind | Supplied sprite center, source pixels | Correct world offset at 100 px/unit | Previous difference |
| --- | --- | --- | --- |
| Bullet / Builder | `(1,1)` | `(0.01,-0.01)` | Universal `(24,0)` sprite registration displaced it by 23 px horizontally and 1 px vertically. |
| Laser / healing | `(1,1)` | `(0.01,-0.01)` | Same universal-registration error. |
| Tyler lightning | `(4,-1)` | `(0.04,0.01)` | Generic bullet image and `(24,0)` registration; no lightning animation. |
| Rocket | `(24,0)` | `(0.24,0)` | Existing center already matches. |
| Blue turret flame / orange pet flame | `(24,0)` | `(0.24,0)` | Center already matches; pet and turret incorrectly shared one animation identity. |

Source paths: `projectiles/projectile.tscn`, `bullet/bullet_projectile.tscn`, `bullet_laser/bullet_laser.tscn`, `bullet_medical/turret_healing/bullet_turret_healing.tscn`, `bullet_lightning/{lightning_projectile,delayed_lightning_projectile}.tscn`, `rocket/rocket_projectile.tscn` and the blue/orange flame scenes. All are under the supplied reference's `projectiles/` directory.

Tyler now uses the existing shipped lightning textures and their source 0/.02/.04/.06/.08-second frame sequence, repeating on the imported 0.1-second loop. No new original sprite was imported. Sprite pivots now preserve each type's actual offset instead of applying 24 pixels universally.

An initial hypothesis that the turret flame resources were orange was rejected after byte comparisons: existing `SupportProjectiles/flame0`, `flame1`, `flame3` and `flame7` match **blue** source frames, not orange. Preserve them. Source `entities/structures/turret/flame/flame_turret_stats.tres` explicitly selects blue flame, while `entities/units/pet/blazemander/blazemander_ranged_weapon_stats.tres:8` selects ordinary orange flame. `PetArtKind` now distinguishes `pet_flame`, reusing the already shipped orange `WeaponReference` frames.

This pass retains baseline projectile effect assets already present in the build. Several are supplied-reference images, as the byte comparisons establish; they must not be described as independently authored Scrapshift art. No new textures, character/companion/turret artwork, audio or import metadata were introduced.

## Implemented files and focused acceptance

- `SupportProjectileArt.cs`: frame progression, distinct lightning/pet-flame identity, and per-type pivots at unchanged 100 PPU.
- `SupportProjectileBehavior.cs`: source-backed `pet_flame` routing.
- `SupportProjectileNextTests.cs` plus metadata: **21 new cases** covering eight type offsets, four ordinary bullet timing samples, six lightning loop samples, distinct pet/turret flame identity, and two later-frame native SpriteRenderer poses.
- Parent-owned `ReferenceCombat.cs`: pose at the newly advanced age.
- Parent-owned existing `SupportNextParityTests.cs`: update the historical `flame` routing expectation to `pet_flame`; preserve the turret's `SupportProjectile_flame0` identity expectation.
- `StructureReview.cs`: four fixture-only production-update probes for ordinary turret, laser, healing and Tyler. Each emits a real structure-configured shot away from staged targets, advances `UpdateWeapons(.04f)`, then checks actual age, an independently named expected texture, and the rendered world center offset. The corresponding existing structure result now requires this check; diagnostics are written into the native log and pet report. This covers the parent's current-age hook rather than calling `Pose` directly.

Tests are authored and pending parent execution. The earlier R11 39 passing native support checks predate these follow-up edits.

## Next bounded native evidence

1. Rebuild and rerun only the existing support fixture. For all six actual production emitters, check that renderer texture at resulting age 0.04 equals the correct current frame and that its sprite bounds center matches the per-type table. Inspect the fresh live captures; ordinary bullet/laser/healing should show their later shapes, Tyler its lightning frame, and flame its appropriate animation frame. This validates the current-age hook rather than a static gallery.
2. Capture a short sequence from emission through at least 0.1 seconds for one ordinary turret, Tyler, and Blazemander/blue turret flame. Include emitter mount and projectile in the same view. A still frame cannot establish origin-to-trajectory alignment or frame progression; color/shape identity can be inspected without claiming exact movement.
3. Keep the outstanding native melee/beam timing comparison separate: current fixture checks are source-derived rules and component stepping, while the actual Godot Area2D/tween/animation phase relationship is not fully established. A small 60 Hz contact probe is sufficient; no campaign is needed to investigate it.

Continuous support aiming, audible cues, physical Xbox actions, simultaneous contact ordering and full-run comparison remain separate. The source comparison closes specific frame/registration discrepancies; it does not establish complete gameplay parity.

## Sequence fixture prepared for the next native run

`PetReview.cs` now adds exactly **20 screenshots**: four actual emitters (ordinary turret, Tyler, orange Blazemander and blue flame turret), each sampled at ticks 0, 1, 2, 3 and 6 of a 60 Hz clock. The same live shots advance in 1/60-second production `UpdateWeapons` steps; emitter bodies remain in view and staged targets are removed after emission, so AI does not alter the sequence. These are sampled frame sequences rather than a video recording.

The four additional native checks require nonempty shots, finite positions, survival through the sample window, independently named source-derived expected textures at each sample, and more than one observed frame identity. JSON diagnostics record capture filename, tick, age, position, actual texture and expected resource for every shot/sample. Tyler's 0.1-second sample intentionally returns to the first loop frame. The parent separately corrects initial pet projectile posing so the zero-time sample displays the proper existing orange effect immediately.

At fixture-authoring time the sequence run and inspection were pending. The subsequent R12/R13 observations and current R13 passing result are recorded above with their exact evidence folders.

The user directed the parity work and provided the original. Codex inspected source/assets, performed byte/dimension comparisons, authored the bounded fixes/tests and documented these limits.

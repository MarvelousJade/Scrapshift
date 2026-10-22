# R11 support screenshot inspection — October 17, 2026

Evidence: `artifacts/gameplay-parity-r11-2026-10-17/support-visible/`. Codex opened **nine fresh native PNG screenshots** with `view_image`: Cat aiming poses 1–3 and all six production projectile captures. No runtime edits, UI actions, motion observation or listening were performed by this reviewer.

## Shadow correction verified in captures

`cat-aim-1.png`, `cat-aim-2.png` and `cat-aim-3.png` now show a small oval under the Cat. The large background-spanning ovals present in the corresponding initial R10 screenshots are absent. This visually verifies the fixture staging correction: placing `GroundShadow` at the staged pet position and its actual base scale.

The Gatling assembly remains visibly connected to the back/body in the up, left and down aiming poses. These views provide attachment evidence only; they do not establish continuous aiming animation or a physically correct rotation joint during motion. The initial R10 screenshots and failed report remain preserved and are described separately in `r10-support-visual-review-2026-10-17.md`.

## Production projectile captures inspected

| Screenshot | Visible observation |
| --- | --- |
| `turret-live-projectile.png` | Connected white/cyan turret and a yellow/white projectile disc ahead of it. |
| `flame-live-projectile.png` | Connected orange firing assembly, small orange muzzle flame and a separate white disc ahead/above it. |
| `rocket-live-projectile.png` | Connected twin launcher and a distinct elongated gray projectile to its right, oriented slightly downward in this sample. |
| `laser-live-projectile.png` | Connected cyan barrel and magenta/white projectile disc. |
| `healing-live-projectile.png` | Attached green medical assembly and green/white projectile disc. |
| `tyler-live-projectile.png` | Distinct purple structure with multiple yellow/white discs distributed around it. |

Sprites and attachment connections are visible in every inspected frame. Single screenshots do not prove muzzle-to-projectile trajectory alignment, projectile speed/lifetime, timing, sound or exact reference size. The bright discs remain large relative to their structures; no equivalent-scale original visual comparison was performed in this review, so size parity remains unverified.

## Machine evidence read separately

Both `support/pets.json` and `support-visible/pets.json` were read: each reports **Passed, 39 passed entries, zero failures**. Their diagnostics explain the initial R10 failures:

- Cat gun clocks switch from `0.4583333 / 0.2291667` to `0.2291667 / 0.4583333`, with shots increasing `1 → 2`. The two logged projectiles have distinct origins but the same velocity. This supports the source-backed replacement of the unsupported opposite-direction assertion.
- Scapegoat revival records partial HP 40, progress held while away, full HP 80 while awaiting the next-tick rise, then alive at HP/max HP 80. This supports the source-backed next-tick fixture correction.

The parent reported the background support process exited zero. This review independently read the JSON contents and inspected the fresh visible images; it did not launch or control the processes.

## Remaining limits

These checks do not close continuous animation, audible cues, physical Xbox input, exact source projectile presentation, simultaneous engine contact ordering, crowd physics or a full-run comparison. R11 corrected fixture setup/expectations; this review does not claim a production pet-logic or art change between R10 and R11. The user directed the work; Codex authored the fixture repairs and performed this inspection.

# R12 support sequence inspection — October 17, 2026

Evidence folder: `artifacts/gameplay-parity-r12-2026-10-17/support/`. Codex opened twelve actual PNGs: `sequence-{turret,tyler,pet_flame,flame}-{00,02,06}.png`, corresponding to emission, two physics ticks and six physics ticks (0.1 seconds). These are stepped native frame samples with the emitter visible, not a video, listening test or full run.

## Visible changes confirmed

| Emitter | Emission / two ticks / six ticks |
| --- | --- |
| Ordinary turret | Initial yellow/white flash overlaps the body; subsequent samples show the narrower elongated yellow projectile farther to the right. The former permanently round bullet is gone. |
| Tyler | Initial cyan lightning cluster separates into cyan zigzag bolts around the structure, with larger radial displacement at six ticks. Yellow generic bullet discs are no longer used. |
| Blazemander | Initial white puffs separate around the visible orange companion and become orange/yellow flame shapes at six ticks. |
| Flame turret | Initial white puff moves to the right and becomes a cyan/blue flame shape at six ticks, visibly distinct from Blazemander's orange flame. |

The sequence supports frame-identity and sampled position progression. It does not establish uninterrupted animation quality, exact source callback phase, collision behavior or audible cues.

## Initial flash overlap: later source clarification

`sequence-turret-00.png` shows the initial flash centered over the turret body while the cyan barrel muzzle is visibly farther right. Subsequent inspection opened the actual original `turret.png` and initial bullet frame: its roughly 80-pixel flash is also centered only 11 pixels above the compact turret body from source muzzle `(0,-12)` plus sprite `(1,1)`. Thus body overlap is not a confirmed gameplay mismatch or blocker. Preserve the exact firing origin. The own-art barrel silhouette and artistic fit are separate from mechanical parity; no production/art correction was made for this observation.

## R12 machine result and fixture follow-up

The actual `pets.json` was read: **Failed, 42 passed entries, one failure `structure: turret`**. All four `production projectile sequence` entries passed. The failed presentation probe reported the expected age 0.04, texture name and rounded offset, but its old diagnostic combined four conditions and omitted shot survival/contact state. The exact failed condition cannot be reconstructed confidently from that text alone.

`StructureReview.cs` was corrected only as a fixture: temporarily remove and restore the staged enemy pool and move/restore the player while advancing the isolated presentation shot. Prior randomly located behavior-test targets must not influence separate sprite acceptance. New diagnostics report each condition (`alive`, `ageOK`, `textureOK`, `offsetOK`), hit count, life, position and unrounded offset components. Parent R13 validation is pending. The R12 failed report and screenshots are preserved; no passing overall support result is claimed for R12.

The parent owns build execution and full source/evidence hashes. Codex authored the fixture isolation and performed this screenshot/report inspection; the user supplied requirements and the original project.

# Cybernetic presentation — October 12, 2026

The player chose a human-like cream mascot with cybernetic upgrades. The body now uses white render tint, preserving the original warm sprite pixels. Existing portraits, limbs and equipment art are preserved. Implant names are a display layer over stable reference item IDs; values, negative tradeoffs, prices, character restrictions and shop weights are unchanged. All 74 current item names are mapped in `artifacts/cybernetic-name-map.json`. Level upgrades use implant names with the original numeric stat descriptions below them.

## Feedback and menus

Harvesting uses the same number style as money. Pending levels have separate unnumbered arrows, with crates placed below their rows. First-character eligible starter weapons fit one row. Footsteps use six supplied reference clips, timed every 0.28 seconds during movement; short expanding ground puffs follow movement. Material attraction was already latched; a contact ring now accompanies the existing contact sound. No extra attraction sound was added. Menu button hover/press sounds use the supplied UI clips. Options expose Gameplay, Audio, Video and Controls tabs, with working reduced-motion, mute, volume and fullscreen settings. These are our supported settings, not every reference option.

Wave cleanup removes enemies/projectiles immediately and preserves trees for intermission. Player and tree idle animation continues during shop/reward screens and freezes while paused. A clamped near-black to violet exterior gradient follows map height. Small circle stones are replaced with outlined machine fragments.

## Bugs found and fixed

- The first validation run passed 179/180 tests: new audio imported as stereo. The migration now forces mono and accepts MP3 in fresh sound-bank creation. Final rerun: 180/180.
- A fixed crate row could overlap multiple pending-level rows; its vertical position now follows the row count.
- The first dust screenshot occurred after the 0.45-second effect expired. The staged capture freezes dust age; normal gameplay still advances it. This is fixture timing, not an extended gameplay lifetime.
- Wave-end cleanup previously removed background trees. Cleanup is now split between enemy removal and next-wave reset.

## Validation and scope

Unity Windows build: zero errors/warnings; 180 EditMode tests pass. Runtime fixture extended from 29 to 35 captures for options, movement dust and intermission trees. It stages screen states and does not substitute for native input or a human balance run. Reference audio provenance is recorded in `artifacts/presentation-audio-provenance.json`; no redistribution license is inferred. New sound timing/gain and dust geometry are adaptations, not a claim of exact source animation timing.

The full base catalog remains an active follow-up. Before this pass it has 30 of 62 weapon families and 74 of 209 registered items; 24 families and 64 items are available under fresh-profile unlock rules. Theme names do not make unsupported effects functional. Keep incomplete effects out of the shop until implemented and tested.

Engineering discussion: stable data identities allow reskinning without changing balance. Serialized import migrations need repeatable tests on both existing and fresh assets. Visual fixtures require timing that actually captures transient effects. This work was implemented with Codex assistance; do not present it as unaided professional production experience.

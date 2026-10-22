# Physical Xbox core controls - October 21, 2026

**Passed for the requested core controls**, R40/source `ede4094`. The user physically operated their Xbox controller and confirmed **"Done—all worked"**. Codex set up the isolated window and inspected the resulting log; no virtual Gamepad was injected. This is fresh hardware evidence, distinct from prior simulated tests.

## Recorded evidence

- Device: `Xbox Controller [XInputControllerWindows] #9`;159 player files match the R40 package manifest.
-268 input snapshots,136 in Playing, all Playing snapshots owned by the physical Xbox.
- Both stick axes and D-pad reached -1/+1 in the session. Menu navigation used left stick and D-pad; A/B were recorded with menu transitions.
- Actual run positions changed with left-stick and D-pad input.83 active right-stick samples matched the resulting manual aim direction within0.000000069 normalized-vector error.
- Menu/Start was recorded with Playing -> Paused -> Playing transitions. The user confirmed both pause and resume.
- Zero menu-focus cue requests during combat;43 footstep requests recorded. This is cue-counter evidence, not an independent listening claim.

Raw immutable input is `artifacts/controller-hardware-2026-10-21/attempt-1/input.snapshot.jsonl`; derived checks, setup, launch, package verification and the exact human confirmation are saved alongside it. The normal logger's live files remain separate from the frozen snapshot. Preferences/progression were isolated in memory; manual aiming was enabled through native Options for the right-stick check.

## Limits

The projectile-shot counter remained zero, so this session does not independently verify ranged firing. No claim is made for X/Y shop shortcuts, shoulders/triggers, stick clicks, reconnect/hot-plug, USB versus Bluetooth, alternative pads or remapping. Hardware axes, resulting movement/aim, core buttons and user-confirmed behavior passed; this is not a blanket claim that every controller action/transport was tested.

## Attribution

The user performed the physical controller test. Codex prepared the observer profile, configured the aiming option, analyzed the recordings and updated the evidence. No production code changed for this acceptance.

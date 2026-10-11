# Interview preparation

Use these notes to understand the project. First-person answers must match the contribution record and your actual hands-on work.

## Why build this?

"I wanted a small Unity game that I could finish and use alongside my AssetScope Editor tool. Arena survival gave me a clear gameplay loop and real prefab, material and configuration dependencies. It connects the experience of making a game with the problem of helping creators inspect their projects."

## Why keep it small?

One arena and a five-minute run allow attention to responsiveness, readability, testable behavior and a usable build. Adding more enemies or maps before the loop is reliable would dilute that focus.

## How does it relate to the role?

Discuss C# state transitions, Unity assets and serialization, Editor-only package separation, reproducible validation, and how an actual project reveals authoring problems. Do not claim the game implements Unity's internal multi-context authoring or Graph Toolkit.

## What was difficult? How did you fix a bug?

Three actual investigations are in `bugs.md`: an invalid package dependency blocked import; the runtime smoke check falsely treated a screenshot request as visual success; and code inspection identified an aspect-ratio mismatch between camera and HUD. Use the one you have personally rehearsed and understand.

The strongest testing example is QA-002. A report said Passed, but the player log showed screenshot capture failure. A second run produced a black image. The lesson is that a passing process or a file on disk is weaker evidence than the actual requested output. The repair separates gameplay counters from rendered inspection and requires actual screenshot evidence. Initial invalid reports are retained for provenance rather than quietly relabeled as success.

If asked for a personal debugging story before you reproduce it yourself: "The project encountered this failure during development. The automated investigation found X; I can explain the reproduction and fix." Do not say you personally performed steps recorded as assistant-run checks.

## How did you test it?

24 EditMode cases passed: rules, collision geometry, content/AssetScope integration and aspect-ratio handling. The Windows player built with zero reported build warnings/errors. Real title/gameplay/pause rendering and keyboard/mouse controls were inspected using desktop automation. Initial hidden screenshot attempts failed visual validation and are recorded separately. See `verification.md` for the final exact evidence and remaining gaps. Automated coverage does not establish that the game is balanced or fun.

## What would you improve next?

Rebinding/controller support, player feedback-driven balance, performance measurements before pooling or architectural expansion, and broader integration checks against later AssetScope versions.

## Hands-on rehearsal checklist

- Play a full run and describe one concrete balance or readability issue.
- Trace a weapon definition to its effect prefab and shared material.
- Explain how pause and upgrades stop simulation without corrupting a wave.
- Read one real regression test and predict what happens if its fix is removed.
- Make a small tuning change, explain it, test it and commit it.
- Practice a two-minute demonstration: game -> asset workflow -> evidence.

## How would you explain the architecture?

"RunRules owns the progression state without scene references. ArenaGame reads input and coordinates combat and presentation. Enemy and weapon definitions are Unity assets referencing prefabs and shared materials. The Editor assembly contains content generation and AssetScope integration; the runtime assembly does not reference UnityEditor."

Point to `architecture.md`. Explain the limitations too: the coordinator is larger than ideal and would be split before expanding the roster.

## How do you prevent bugs around pause and upgrades?

Advance simulation only in Playing state. A level-up and a wave-clear intermission use the same upgrade screen but have different continuation behavior. Tests verify that an in-wave upgrade preserves remaining time and only a wave intermission advances the wave. Invalid or duplicate choice events cannot apply another upgrade after the state returns to Playing.

## What combat problem did you anticipate?

Fast projectiles can move from one side of an enemy to the other between frames. Checking only their current position can miss the collision. The game measures distance to the whole movement segment instead. Tests include a target between endpoints, a target beyond the segment, and a stationary segment. This is a preventative design and tested edge case, not a claim of an observed tunneling incident.

## Why ScriptableObjects?

They let you tune enemy and weapon values as assets rather than scattering constants through combat code. Their prefab and material references are also inspectable in AssetScope. The current small game still has some positional weapon assumptions; expanding it should introduce named references and broader configuration validation.

## How did you decide what to cut?

The original brief emphasized small, polished, Unity experience and AssetScope compatibility. Multiplayer, a large roster, a shop economy and procedural worlds would add work without directly proving that goal. Three recognizable weapons, three enemy types and a complete short loop are enough to show the authoring/testing workflow.

## How would you optimize it?

Measure first with Unity Profiler on a repeatable scenario. Investigate Instantiate/Destroy and nested collision searches if they dominate a frame. Pool transient objects or introduce spatial partitioning only with evidence. The project currently bounds entity counts; it has not established a production performance benchmark. Do not quote an invented FPS improvement.

## How did references influence the game?

Use `art-references.md` and `combat-references.md`. Discuss movement-centered combat and readable choices from Brotato, compact mechanical silhouettes from Into the Breach, and abstract weapon effects from Nova Drift. Public community mod source informed questions about hit processing, bounded history and effective stats. No claim is made to possess the complete official Brotato source.

## How does this demonstrate attention to users?

The controls and objective are visible before play. Combat stops while reading upgrade choices. A health bar and clear timer communicate status. There are sound and reduced-motion controls. The camera and HUD share an aspect ratio so resizing cannot hide reachable parts of the arena. These are design decisions; usability-study success is not claimed.

## How did you use coding tools?

Answer honestly if asked. The user's direction covered concept, scope, references and documentation; implementation and initial automated investigation in this session used a coding assistant. Explain the code you understand, the outputs you reviewed and the hands-on changes you actually made. Do not equate directing generation with independently typing every line. The role explicitly welcomes AI tools, so clear verification and ownership boundaries are useful discussion points.

## Tell me about collaboration or user feedback

This is currently a directed portfolio project, not evidence of a professional team or user study. Use another verified experience for a teamwork question unless you have a real collaboration example here. Record the first playtest feedback in `contributions.md`, then connect the observed issue to a concrete change and verification.

## Two-minute demo order

1. Start a shift and explain movement, auto-fire and scrap collection.
2. Pause; show the options and explain why simulation is stopped.
3. In Unity, inspect a weapon definition and trace its effect prefab/material in AssetScope.
4. Show one rule test and the real bug log, explaining what each proves and does not prove.
5. End with one specific improvement based on your own playtest.

## How did you respond to negative feedback?

The user explicitly said the original art, UI and upgrade presentation were poor and that the combat lacked effects. The revision used the supplied local reference to identify specific missing signals: character silhouettes, hit confirmation, threat telegraphs and readable stat choices. It then replaced the presentation, expanded feedback, added randomized tiers/rerolls, and rechecked the build and actual screens. This demonstrates iteration from concrete critique; user approval of the redesign is still pending.

The latest suite has 27 tests. DEV-004 is an additional real debugging example: native Unity object construction during deserialization, caught by scene tests despite a nominally successful build. Explain both the lifecycle fix and the stricter build gate. See journal and verification for the newest evidence; earlier examples above describe the first baseline.

## How did you use your Editor tool in a real project?

The game now embeds AssetScope 0.3.0. It scans 72 actual game assets and 1,202 reference usages with saved-scene/property analysis enabled. A regression test locates the Fire_01 audio clip reference in the sound bank, navigates to its source and verifies the selected bank. This links creator tooling to an actual authoring task: find which sound-bank entry uses a clip and go to the owning asset to change it. It is stronger evidence than only showing synthetic demo assets, but remains a small project, not a large-project benchmark.

## Why change the audio architecture?

The user reported that the original sound was poor. Four sine tones did not distinguish enough gameplay events. The revision separates authored cue data from playback, uses multiple original variants, and limits concurrent sounds while reserving voices for important feedback. Explain the distinction between numerical/sample-playback verification and subjective listening; the latter still needs personal evaluation. AUDIO-006 records the actual Unity importer API failure and fix.

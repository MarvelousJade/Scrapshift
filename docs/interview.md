# Interview preparation

Use these notes to understand the project. First-person answers must match the contribution record and your actual hands-on work.

## Why build this?

"I wanted a small Unity game that I could finish and use alongside my AssetScope Editor tool. Arena survival gave me a clear gameplay loop and real prefab, material and configuration dependencies. It connects the experience of making a game with the problem of helping creators inspect their projects."

## Why keep it small?

One arena and a five-minute run allow attention to responsiveness, readability, testable behavior and a usable build. Adding more enemies or maps before the loop is reliable would dilute that focus.

## How does it relate to the role?

Discuss C# state transitions, Unity assets and serialization, Editor-only package separation, reproducible validation, and how an actual project reveals authoring problems. Do not claim the game implements Unity's internal multi-context authoring or Graph Toolkit.

## What was difficult? How did you fix a bug?

Pending actual verified investigations in `bugs.md`. Prepare the sequence: observation -> minimal reproduction -> hypothesis -> evidence -> change -> regression check -> remaining limitation. Do not substitute a hypothetical story for an observed bug.

## How did you test it?

Pending execution. Explain the planned layers: scene-independent rules; content and AssetScope integration; player build and runtime smoke; visual inspection; user playtesting. Distinguish automated coverage from whether the game feels fun.

## What would you improve next?

Rebinding/controller support, player feedback-driven balance, performance measurements before pooling or architectural expansion, and broader integration checks against later AssetScope versions.

## Hands-on rehearsal checklist

- Play a full run and describe one concrete balance or readability issue.
- Trace a weapon definition to its effect prefab and shared material.
- Explain how pause and upgrades stop simulation without corrupting a wave.
- Read one real regression test and predict what happens if its fix is removed.
- Make a small tuning change, explain it, test it and commit it.
- Practice a two-minute demonstration: game -> asset workflow -> evidence.

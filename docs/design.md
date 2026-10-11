# Purpose, scope and art direction

## User need

Build a small but polished Unity game for the Unity Editor software developer application, complementing the AssetScope tool already under development. The player should understand the objective and controls immediately, finish a run in five minutes of active gameplay, and make a few meaningful upgrade choices.

## Why this genre

The player controls movement while weapons attack automatically. That makes the first playable loop achievable without complex animation, maps or a large content pipeline. Enemy prefabs, shared materials and ScriptableObject weapon/enemy definitions also create understandable asset relationships for AssetScope demonstrations.

## Explicit boundaries

One arena; three enemy types; three weapons; five waves; a small upgrade menu; keyboard and mouse menus; local Windows build. No multiplayer, accounts, procedural world, meta-progression, commercial launch or promise of perfect balance. Initial sound is synthesized. No outside sprite pack is required.

## Art plan

Original code-native mesh artwork, saved as real Unity assets. A mint repair robot contrasts with coral, amber and purple hostile robots against a navy deck. Layered shapes provide readable silhouettes, visor faces, armor highlights, shadows and weapon accents. Movement bob, particles, pulses and small camera impulses supply motion. Art uses an orthographic camera and unlit materials; it does not require a separate modeling or painting application.

## Architecture decisions

- Separate run rules from the scene: wave transitions, health, upgrade selection and pause behavior can be tested without rendering or input.
- ScriptableObjects contain reusable enemy/weapon definitions, with explicit prefab/material references for authoring and AssetScope.
- A bounded arena and simple distance checks avoid a physics dependency for this small top-down combat loop. This is a scope decision, not a claim that physics is generally undesirable.
- Snapshot AssetScope as an embedded Editor-only package. Source baseline: `b3f060e9874bb7cb94235de532ad9f219c422c9c`. The other chat is independently expanding it; changes are not silently pulled into this game.
- Use the legacy Input API for this limited keyboard prototype; migration to action-based Input System bindings is a follow-up for rebinding or controller support.

## Acceptance targets

Playable title/run/upgrade/pause/end loop; inspectable serialized content; no missing-script findings in normal game content; meaningful rules and integration tests; an actual Windows build; a rendered gameplay inspection; precise limits in the final verification record.

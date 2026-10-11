# Architecture and tradeoffs

## Runtime flow

`ArenaGame.Update` reads input, dispatches state-specific actions, and advances combat only while `RunRules.Phase` is Playing. Paused, Upgrade and end screens leave world simulation stopped. The code does not change global `Time.timeScale`, so UI and inspection remain responsive.

`RunRules` owns wave time, health, scrap thresholds, unlocks and stat modifiers. It has no scene-object references and takes explicit time increments. `WeaponDefinition` and `EnemyDefinition` are serialized ScriptableObjects. `ArenaGame` connects these to instantiated prefabs, distance queries, audio and UI.

## Important invariants

- One hit consumes a rivet projectile.
- Swept segment distance checks the projectile's movement between frames, avoiding endpoint-only misses for fast bullets.
- Orbit damage uses a cooldown rather than damaging every frame a target overlaps.
- A pulse applies damage once at activation; its expanding ring is feedback, not repeated damage.
- Contact invulnerability limits successive hits from overlapping drones.
- Only a completed wave's intermission advances the wave counter. An in-wave level-up resumes its remaining time.
- A choice is accepted only in Upgrade state, preventing repeated installation from a second UI event.
- End states ignore time and damage; restarting resets the run modifiers.

## Unity asset flow

Scene -> ArenaGame -> weapon/enemy definitions -> effect/enemy prefabs -> meshes/materials -> shader.

Some references are direct scene-to-prefab links (player, scrap and sparks). These relationships are deliberately ordinary serialized references, useful to both Unity and AssetScope. Weapon data does not depend on the Editor package.

`Scrapshift.Editor` references `AssetScope.Editor`; `Scrapshift.Runtime` does not. The embedded package's assemblies target Editor only, so the asset browser does not become a player runtime dependency.

## Why native mesh art?

Mesh geometry keeps the content small, editable and free of an external sprite pipeline. Shared materials enforce a consistent palette and create a useful reverse-reference example. The art can later be replaced behind the prefabs. The current style is intentionally geometric, not a substitute for a production character-art pipeline.

## Why these simple implementations?

| Choice | Benefit | Cost / future trigger |
| --- | --- | --- |
| Distance queries and lists | Easy to inspect at a small entity count | Work grows with active shots and enemies; profile before raising caps |
| Instantiate/Destroy with caps | Fewer lifecycle assumptions for the first version | Pool short-lived objects if profiler evidence shows allocation or destruction spikes |
| IMGUI runtime menus | A compact implementation with clear keyboard shortcuts | UI Toolkit/uGUI is a better next step for richer navigation, accessibility and localization |
| Legacy keyboard input | Few dependencies for this keyboard-only prototype | Use action-based input for rebinding, controller support and input testing |
| Embedded AssetScope snapshot | Repeatable integration while the other project changes | Explicitly update and retest later; it is not a live link |
| Five fixed waves | Clear short completion target | Less variety than the reference game; needs real player balance feedback |

No frame-rate, memory-allocation or scalability claim has been measured. Caps are protective limits, not performance benchmarks.

## Known technical debt

`ArenaGame` is a relatively large coordinator. A sensible next split is combat entities, presentation/audio and runtime verification, preserving the tested run rules. Presentation and combat feedback now live in partial-class files; they still share the coordinator state. Upgrade offers are randomized after an introductory draft and some weapon lookup order is positional. This is manageable for three weapons but would warrant named data references and validation before expanding the roster. Enemies now include a telegraphed charger and a knockback-resistant brute; crowd separation is still absent.

## Safe content authoring

The generator is a first-run scaffold: it preserves an existing scene instead of overwriting authored work. Change the saved definitions/prefabs to tune the game after generation. Changing generator constants alone does not silently rebuild existing content. Git tracks `.meta` files to preserve asset GUIDs.

## Audio and Editor integration update

`ArenaGame` dispatches semantic SoundCue events to a separate GameAudio component. The SoundBank ScriptableObject owns gains, variant arrays, cooldowns and pitch range. PCM imports are preloaded for short low-latency effects. Playback uses 16 bounded AudioSources with reserved important-cue slots, immediate mute, short ducking and per-cue cooldowns. A separate opt-in AudioCheck component tests actual sampled mixer output.

The scene references GameSounds -> AudioClip variants; AssetScope 0.3.0 indexes those serialized arrays and can navigate to their owner. The package remains Editor-only. Runtime playback has no AssetScope or UnityEditor dependency.

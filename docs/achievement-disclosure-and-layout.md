# Achievement disclosure and reward layout

October 13, 2026.

The Challenges page previously exposed the reward name, portrait and effects before completion. The user requested the reference's locked behavior and its separate banner/reward layout, plus more transparent Codex grid tiles.

Compared the supplied screenshot and Godot reference files `ui/menus/run/challenge_and_reward_ui.gd` and `ui/menus/ingame/challenge_ui.gd`. The reference hides the entire reward section while locked; its banner shows a lock, `???` and the unlock requirement. Completion reveals the achievement name and a separate reward panel.

Implemented that split in `CodexUI.cs`. Locked rendering returns before any reward fields are drawn. Completed achievements show an alien-themed banner, requirement, Reward heading and centered card with the actual reward portrait/icon, name, category and scrollable rich effects. Uses our existing alien and character art. Ordinary Codex item/weapon details retain their existing discovery rules.

Unselected Codex tiles now use translucent fills instead of an opaque frame beneath the fill. The white selected tile stays opaque. This change is scoped to the Codex; it does not alter shop or inventory tiles.

Added deterministic built-player captures for the same Gatherer 2 challenge in locked and completed states, alongside the existing UI checks. These captures stage completion only in the review fixture. The source disclosure check uses actual progression completion, independently of whether the reward has been acquired.

Review and build results: `artifacts/achievement-layout-review`. Playable folder: `Builds/AchievementLayout`.
`Windows build succeeded with zero errors and warnings. All 82 staged UI captures passed; the locked/completed Gatherer 2 pair was visually inspected. Validation source copy matches byte for byte. These checks cover rendering and staged state, not native controller/mouse navigation.`

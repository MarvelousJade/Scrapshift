# Challenge unlock sound parity

The original `ui/menus/ingame/challenge_completed_ui.gd` connects to `ChallengeService.challenge_completed`. When a challenge popup appears, it plays `ui/sounds/Shield 4.mp3` at -5 dB with random pitch ±0.2. On the popup's hide timer it plays `weapons/melee_sounds/whoosh_swish_small_harsh_03.wav` at the default gain and pitch. The scene file `challenge_completed_ui.tscn` assigns those exact resources. First death completes `chal_rookie` in `main.gd`, so that popup can occur on a lost run. The end-run `Progress` list is a separate display of rewards earned during the run.

Scrapshift previously used its install cue when drawing the achievement toast and played no unlock sound on the end-of-run reward card. It now imports both source clips byte for byte. Challenge completion starts the popup cue, and the toast's 2.6-second expiry starts the popout cue. Queued challenges play the popup sound as each one becomes current. The end-of-run reward card remains a separate review display and does not replay the sound.

Source and imported SHA-256 hashes match: `Shield 4.mp3` / `AchievementPopup_01.mp3`: `3A46749AC7E5175B4F847F50903B8C3E7444831AC457A849C590A7D04C1F42EC`; `whoosh_swish_small_harsh_03.wav` / `AchievementPopout_01.wav`: `C3EE0794CCA2D85614E2F76633497E22D108EA8CBE8A6C1984A207E9539B4365`.

Scrapshift's toast lasts 2.6 seconds, while the source popup's hide timer controls its duration. This change matches the clips, relative gain, pitch variation, and appear/disappear events, not the exact visual timing or end screen layout.

Final integration verification: 387 Unity EditMode tests passed. The Windows player built with zero errors and zero warnings. Its hidden Direct3D 11 mixer check played all 26 cues audibly, including both unlock cues; mute suppression passed. This checks sound output, not a human listening comparison or the exact toast animation timing.

# Menu scene presentation — October 12, 2026

The selection screen previously used enlarged item icons as wallpaper, and the portrait stood on a flat color. It now shows a cached camera view of our actual yard with a closer terrain crop inside the preview card. The capture isolates the Deck art hierarchy, restores layers in a finally block, and releases its render texture when the game component is destroyed. No actors, combat drops or UI icons enter the capture.

The title uses a newly generated original salvage-yard illustration (Resources/Menu/SalvageTitle.png), with empty space for the title and controls. Our existing cream mascot and two weapon sprites animate independently above it. Eight subdued enemy sprites orbit slowly; no reference game artwork was copied. The original generated asset remains in Codex generated_images as well as its committed project copy.

Reference inspected: supplied title and character-selection screenshots; Godot ui/menus/title_screen/title_screen_background/base_title_screen_background.tscn and brotato_idle.tres (1.5-second looping translation and 1.07/0.93 scale extrema), plus the u31 background's independently animated layers. Our .95-second bounce and seven-second occasional jump/spin are an adaptation, not identical extracted tracks. Leg wiggle, oval ground shadow and weapons are separate layers. Reduced Motion freezes all these menu motions without changing the saved preference.

Settings: removed the bottom keyboard/help and resolution text; added the missing black row behind sound-effects volume. Fixed the clipped Run options heading.

Bug discovered in visual review: GUIUtility pivot transforms were applied around unscaled coordinates after the full-screen GUI transform, moving title guns far from the mascot. Local TRS matrices now compose after the canvas matrix, including the legs and enemy rotations.

Validation: 288 EditMode tests passed after the animation/layout changes. Final build (including generated backdrop) succeeds with zero errors and warnings. The screenshot fixture adds title idle/jump, selection idle/spin and Reduced Motion cases to the existing 65 stages. A hidden-window screenshot attempt could not capture rendered frames; it is retained locally as a failed attempt, and rerun visibly for the final review. Screenshot fixtures stage normal rendering with frozen simulation; they do not test controller input. Controller support and music are unchanged in this presentation pass.

Final visible screenshot run: Passed, 70/70 captures verified; title idle/jump, selection idle/spin, Reduced Motion and settings images inspected. No errors or exceptions in the player log. Source and validation copy match across 1,029 files. Windows package CRC and every archived file match the MenuScene build.

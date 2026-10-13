# Maps, title and grounded idle

## Requested presentation

The title is now a single cohesive generated illustration including the logo, mascot, enemies and weapons; Unity overlays only the interactive menu buttons. Final art uses a single Telescopic Optics eye scope (catalog ID scope), Muscle Stimulator cheek detail (defective_steroids), plain cream body, larger correctly oriented guns and small cream oval grip hands. Goggles and chest armor were removed at user request. The earlier complete illustration is retained as an unused asset iteration; SalvageTitleEquipped is the active resource. Title equipment depicts existing catalog items, without granting those items or their stats to a new run.

The character preview now calls SpringMotion.Pose directly in idle mode, matching SpriteFrameAnimator: 1-second body squash/stretch, 1.15/0.85 scale extrema and -0.05 body offset, with fixed feet. Removed the separate jump, spin and leg wiggle. Facial layers follow the body transform. Reduced Motion returns the resting pose.

## Six selectable visual maps

Salvage Yard has smaller, curved grass tufts and quiet pebble groups. Added Rust Flats (warm dirt and metal flakes), Overgrown Relay (moss-colored soil and plants), Cryo Depot (cold ground and small crystals), Ash Foundry (dark ash and slag), and Neon Drain (muted violet floor, small pools and reeds). Each has a deterministic distinct scatter seed. Grass is shorter and uses bent, rounded tips instead of the former straight crown silhouette.

These are six selectable background themes on the same combat arena, not six new campaigns or separate enemy/boss ecosystems. Collision geometry, waves, enemies, item economy and weapon values are unchanged. Details share one mesh per arena; old meshes and cached menu ground textures are released when switching themes.

Reference options inspected: ui/menus/pages/background_button.gd, menu_options.gd/.tscn and singletons/run_data.gd reset_background. The original Video tab exposes Background with Random followed by named backgrounds. Our Video settings use the same choices (Random plus six themes), persist the selection, and immediately refresh the current ground. Random uses a separate System.Random so cosmetic selection does not consume combat randomness, and chooses once per new run. The character-selection Map button shares this preference.

## Grip hands

Small cream oval meshes with dark outlines attach to weapon grips, inherit weapon rotation/recoil and mirror their local vertical offset with SpriteRenderer.flipY. Sorting follows the weapon so rear hands remain behind the player. Fist, Hand, Power Fist and Claw retain their existing art. Generic ranged and melee grip positions are normalized to sprite bounds; special-shaped weapon art may need per-weapon tuning after play feedback.

## Verification and fixes

288 EditMode tests pass. Final build succeeds with no errors/warnings. The visual fixture was expanded to 79 stages for title, grounded idle/compressed poses, Reduced Motion, all six arena themes, both hand-facing directions and the map selector. Source and isolated validation copy are compared byte-for-byte before delivery.

An edit initially hit the Windows default text encoding when inserting a dropdown arrow and truncated the new map file. The build caught it; the file was restored explicitly as UTF-8 with a C# Unicode escape and the build/tests passed. No partial build was packaged.
`nThe final 79-stage capture run passed, including the map dropdown and both grip directions. A fixture-only visibility issue after returning from character selection was corrected by explicitly restoring the player and shadow before the arena captures. Native mouse interaction and persistence across process restarts were not exercised by this frozen screenshot fixture.

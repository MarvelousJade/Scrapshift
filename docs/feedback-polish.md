# Feedback, pause, audio and ground-detail revision — October 12, 2026

## Evidence and decisions

The supplied Brotato shop/upgrade captures show visible gray purchase and Choose surfaces, while menu, lock and reroll buttons stay much darker. Scrapshift now gives just those card actions a solid gray idle fill, retaining the white focus state and unaffordable-price indication.

The reference `ui/menus/ingame/challenge_completed_ui.gd/.tscn` subscribes to completion events, queues notifications, anchors the panel at the bottom left and uses a two-second hide timer plus animations. Scrapshift now sends an event only on first completion and queues a nonblocking 2.6-second notification including transition time. It shows our reward name and art. The notification queue is independent of the end-of-run reward review, and existing saves do not replay old achievements.

## Bugs fixed

- Pause only accepted Playing and always resumed Playing. It now stores the originating phase and accepts combat, shop, upgrades and crate rewards. Resuming restores exactly the same offers, crate, currency and timer. Title/back and final-result navigation retain their own existing controls.
- The crate page passed icon markup to a plain text label, exposing `[[range]]` literally. It now uses the same icon-aware text renderer as shop and inspection cards. The bottom weapons/items strip was removed as requested.
- Pickup feedback was an expanding ring and shared a busy combat voice pool. Materials now emit six small green particles over 0.3 seconds, following `particles/pickup_gold_particles.tscn`. One reserved voice keeps their sounds separate from combat; three voices remain reserved for damage and other notifications. The older-bank migration now raises pickup gain from 0.18 to 0.42.
- Consumables previously reused the material sound. They now select among the reference's three `comedy_bite_chew` clips; crates keep their separate reward sound. Original material pickup mapping in `main.tscn/main.gd` uses the five water-drop samples, rather than a separate wind event. Copied consumption files are hash-verified in the review provenance manifest.
- The small player-attached health bar and its redundant option were removed. The upper-left health/XP HUD remains.

## Music and map

`Neon Salvage` is an original 60.95-second, 126-BPM E-minor electronic loop, authored by the reproducible `scripts/compose_music.py`. It has a synthesized kick/snare/hat groove, pulsing bass, pads and delayed FM plucks, with a quieter middle section. No reference melodies or soundtrack samples were copied. A dedicated looping source fades to lower volume outside combat and has an independent persistent music slider; master volume and focus mute also apply. Audio fixtures mute music so it cannot mask missing SFX in the automated output check. Numerical checks are not subjective listening approval.

Inspected `zones/backgrounds/background_data_1.tres` and its `resources/tiles/tiles_1.png`: small rock clusters, grass and sprouts, muted outlines and scattered spacing. Replaced broad scrap fragments with 245 deterministic small groups using original vector geometry. All details share one mesh, have no colliders, and do not alter enemy pathing or arena bounds. These are a visual interpretation of the reference, not its copied tile sheet.

## Interview explanation

The core lesson is separating state and presentation. Pausing should suspend an existing state, not create a new combat state. Unlock notifications should observe progression without consuming persistent rewards. Shared description markup needs one renderer across all screens. Audio needs bounded, purpose-specific voice allocation so frequent pickups remain audible without displacing critical feedback. Source comparison identified concrete timing, layout and routing differences; targeted regression tests guard the behavioral fixes.

Validation results and current executable are recorded in `verification.md` and `PLAYER_README.txt`. The review page includes full-size staged captures, the original screen references, and playable music/crunch previews.

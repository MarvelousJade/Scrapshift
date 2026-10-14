# Victory sequence

## Original game reference

The supplied Brotato 3.x project sets `RunData.run_won` when the final timed wave ends with a living player (`main.gd::_set_run_states`). `clean_up_room()` plays `ui/sounds/end_wave.wav` immediately and starts a four-second end-wave timer. A living player starts the `won` animation after 0.25 seconds (`entities/units/player/player.gd`). At 1.25 seconds, the player creates `particles/starship_beam.tscn`, whose sound is `starship_beam_won.wav`. The animation calls `run_won_screen` at 1.4 seconds from cleanup; the game then plays `Fear 10.mp3` at -5 dB and types `RUN_WON` with two bubble sounds. At four seconds, `main.gd::_on_EndWaveTimer_timeout()` changes to the end-run scene. That scene displays stats, weapons, progress rewards, items, and Restart / New Run / Main Menu buttons. Its victory title includes the zone and difficulty without repeating wave 20.

## Scrapshift implementation

The final wave keeps the arena visible for four seconds. The player performs a squash and jump, the original blue circle sprite, blue particles, and the original beam sound start at 1.25 seconds, and the original victory and letter sounds play with the `RUN WON` label from 1.4 seconds. The results screen opens automatically after four seconds. The beam sound and circle sprite are copied byte-for-byte from the supplied game and recorded in `artifacts/audio-parity-provenance.json`. `--win-review <folder>` captures the jump, label, beam, and results for visual review.

The player motion and light animation recreate the main beats of the reference; they are not a frame-for-frame port of Godot's seven-second animation or its three particle systems. The results screen retains Scrapshift's existing stats, inventory, and progression presentation, so its detailed contents are not identical to Brotato's end-run scene.

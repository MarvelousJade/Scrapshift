# Audio revision

## Trigger and reference

The user explicitly rejected the sound design and clarified that the request was about audio. The old implementation used four short sine tones; impact and player damage shared a cue, and pulse reused the upgrade sound. A separate question about the red lower-body rectangle was answered: it is a scarf. Character art was left unchanged in this audio pass.

Read the supplied Brotato export's `singletons/sound_manager_parent.gd` and `sound_manager_2d.gd`: 12 pooled players, bounded queue, position and per-play pitch variation. Inspected the local multi-variant pistol/impact/pickup sample organization and WAV metadata. Example reference durations: pistol 1.500 seconds, body impact .424 seconds, level-up 1.967 seconds, buy .865 seconds. These measurements are not a listening evaluation.

## Independent implementation

`scripts/generate_audio.py` authors 36 original mono 44.1kHz PCM samples across 15 cue categories. It never reads reference audio. Gunshots layer a noise crack, low punch and metallic tail; impacts use duller short thuds; criticals add a sharper metallic accent; deaths have a low falling crunch; pickups use brief harmonic clinks. Blade swishes, pulse bursts and charger windups have distinct envelopes. Upgrade, install, reroll, win/loss and wave-clear cues have separate patterns.

`GameSounds.asset` stores clips, gains, cooldowns and pitch variation as inspectable Unity data. The GameAudio component has 12 combat voices and four reserved important-cue voices. It avoids immediate sample repetition, uses mild stereo position/pitch variation, rate-limits impact/pickup spam, steps pickup pitch through short chains, and briefly ducks combat under player-damage/menu rewards. Mute stops active voices immediately. Pause settings include saved effects volume. No music track was added.

## Verification boundaries

The generator checks RMS/peak and zero-ended samples; numerical checks do not prove pleasing sound. `artifacts/audio/sound-sampler.wav` is an offline sequence of the new sounds, not a gameplay recording. `--audio-check <folder>` plays every cue through the actual Unity mixer, samples nonzero output and checks mute suppression. It is not a continuous clipping measurement or a substitute for listening on headphones and speakers.

Game tests check complete playable cue assets, non-repeating selection and actual AssetScope navigation to a bank entry. Runtime and build results are recorded in verification.md. Full busy-wave mixing, device variety and the user's preference remain listening tasks.

## Supplied reference audio revision

The user's later request explicitly permitted use of the supplied game's sounds. Current playback uses reference files for Fire, Impact, Kill, Critical, Pickup, LevelUp, Install, WaveClear, Swing, Charge, Hurt and Reroll. SMG, shotgun and laser use their own equipment sound arrays. Win/Lose and the unused legacy pulse cue retain original clips. Earlier original-audio descriptions are historical. The bounded mixer, cooldowns, non-repeating variants, volume, mute and priority ducking remain in use. The provenance manifest identifies each copied WAV and its hash. Numerical output checks are not a claim that the user likes the mix.

# Audio revision

## Approved v3 cyberpunk treatment

Latest user correction: material pickups bypass the baked library and use the original reference clips. Their existing gain, sample variation and pickup-chain pitch behavior remain. The other v3 sounds, including the separately tuned Alien Eyes cue, are unchanged. Historical pickup edits remain as unused preview/baked assets.

The user approved `artifacts/audio-preview-2026-10-15/tuned-v3`. `CyberSoundBuilder` bakes its four processing profiles into `Content/CyberAudioV3`; `Resources/CyberSoundLibrary.asset` maps source clips to their edited versions. `GameAudio` resolves the selected variant through that library, retaining existing pitch variation, voice limits, gain, cooldowns and ducking. There is no runtime synthesis or first-shot conversion work.

Coverage includes catalog weapon variants, generic hit/hurt/critical/kill feedback, reference weapon impact and explosion overrides, turret/companion weapon shots, melee swings and material pickups. The user subsequently included Victory, Defeat and Alien Eyes in the tuning. Alien Eyes uses the watery pickup profile; Victory/Defeat use a modulated delayed layer derived from their own recordings, retaining their pitch and phrasing. All nine defeat variants are processed. Music, UI, footsteps, consumable chewing and non-shot companion cues retain their existing recordings. The source WAV/MP3 files remain intact; these edits are derivatives of supplied third-party sounds. The generated provenance TSV records every source/profile mapping.

The recipe preserves the source transient and adds the approved 1.45-strength layers, two short reflections, a 0.14-second tail, energy matching and a 0.94 sample-peak ceiling. Unity's imported source channel layout is retained. Build validation rejects missing covered mappings; tests check override coverage, distinct clips, tail lengths, peaks and untuned cue routing and all requested reference-source variations. These checks do not establish subjective busy-combat mix approval.

## October 15, 2026: supplied end-run sounds

The user rejected the synthesized Win/Lose jingles. The active bank now uses the exact `main.tscn` assignments: `Fear 10.mp3` for Win and `Madness 1, 2, 3, 4, 6, 7, 8, 9, 10.mp3` for Lose. `main.gd` plays each at -5 dB with no pitch variation; the Unity cue gain is 0.56234133, before the existing master/effects mix. The ten imported MP3s are byte-identical to the supplied files, recorded in `artifacts/audio-preview-2026-10-15/end-run-provenance.json`. Content generation reapplies these mappings. Old generated files remain historical assets, no longer selected by the bank.

Correction to the historical "unused legacy pulse" statement below: `ImplantAttacks.TickImplantAttacks` uses Pulse for Alien Eyes. Reference `player.gd:on_alien_eyes_timeout` plays `alien_sounds`, assigned to two water-impact clips in `player.tscn`. Its empty weapon shooting-sound array does not mean silence. The user subsequently requested replacement: Pulse now selects both supplied water-impact clips, with gain 1 (0 dB) and pitch variation 0.1 as in the reference call. Both imported WAVs are byte-identical to their sources; see `artifacts/audio-preview-2026-10-15/alien-eyes-provenance.json`. The three-second radial attack logic is unchanged.

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

## Player damage mapping correction - October 11

The earlier reference integration incorrectly mapped Hurt to a zombie attack grunt. The supplied player's scene uses bullet_impact_body_flesh_05..08 instead. Hurt now shares the four exact imported Impact_01..04 files (all four SHA-256 comparisons match), with +/-0.2 pitch variation. Existing-bank migration and initial integration both enforce the corrected mapping; the importer no longer imports the grunt as Hurt. Reserved voices, gain and ducking remain Unity mix adaptations. artifacts/player-hit-audio/audio-runtime.json verifies all 15 cue outputs and mute suppression; this is not subjective listening approval. The local comparison page provides source-pitch previews.


## Context-aware effects (2026-10-15)

The approved weapon-context-v1 previews now anchor the sound-effect treatment. ContextSoundProfile selects a weapon/support family independently of the original source clip. GameAudio takes an optional context profile; WeaponReference and OriginalSupportAudio supply it. This avoids applying a rocket treatment to a pistol that shares its recording.

ContextAudioV1 contains baked, deterministic edits for the catalog weapon variants, bank cues, impact/explosion overrides, support cues, and the companion healing loop. Material Pickup always bypasses processing, even when passed a context override. Music is unchanged. Original reference files, cue gains, cooldowns, variant selection and random pitch remain intact. Existing tuned Victory/Defeat/Alien Eyes retain their reference-based processing.

The four approved preview waveforms are stored in ApprovedContextAudio and reused for their matching source/context pairs, with gain normalization only. Remaining effects use families chosen from the weapon artwork/name and gameplay purpose: laser, magnetic rail, rocket, electricity, flame, blade, heavy blunt, ballistic, rapid fire, bow, organic, ethereal, motor, healing, plasma, ice, explosion, voice, shield, status and burn. UI/footstep/consumption processing is deliberately light. The healing loop keeps its exact frame count and uses integer-cycle modulation.

Regenerate with Scrapshift/Generate approved cyberpunk audio. CyberSoundBuilder writes source/profile provenance and the Resources library. The existing CyberAudioV3 files remain available for comparison; runtime uses the new library. Automated checks verify routing coverage, sample integrity, pickup bypass and loop length; they do not establish subjective listening quality.

Validation: 272 baked source/context pairs, 62 weapon types; all eight audio integration/routing tests passed in the isolated project full-suite run (saved as artifacts/audio-preview-2026-10-15/context-audio-full-suite.xml). All baked WAVs are finite, non-silent and within the peak ceiling. The four canonical preview waveforms match after gain adjustment (correlation > 0.999999). Windows build: Builds/ContextAudioV1/LaunchGame.cmd, zero build errors/warnings. Listening gallery: artifacts/audio-preview-2026-10-15/context-audio-applied/index.html. No audible automated playback or subjective listening validation was performed.

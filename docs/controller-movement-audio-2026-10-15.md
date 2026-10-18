# Controller movement audio investigation — October 15, 2026

## Observation and scope

The user physically tested an Xbox controller in the R4 review build. They reported that movement during combat sounded like rapid menu switching, alongside a mismatch between the highlighted character and the action confirmed by A. This report is actual user feedback. The following is a source and asset audit by Codex; it does not claim that Codex heard the reported sound or that the hardware issue is resolved.

## Asset routing

`Assets/Scrapshift/Content/Scenes/Deck.unity` binds `soundBank` to the GameSounds asset (GUID `49ab37c00c4a78746aa4ec942ef972ee`). In `Content/Definitions/GameSounds.asset`:

| Cue | Source | Gain | Pitch variation | Context profile |
| --- | --- | --- | --- | --- |
| Step (15) | Six `Content/Audio/Step_1.mp3` through `Step_6.mp3` | 0.18 | 0.1 | 215 |
| UiFocus (16) | `Content/Audio/UiFocus_1.wav` | 0.35 | 0 | 216 |
| InventoryFocus (19) | `Content/Audio/InventoryFocus_01.wav` | 0.35 | 0 | 219 |

`CyberSoundLibrary.Profile` and the resource library keep these roles separate. All six Step sources map to their own edits; none map to a UI edit. For example, Step 1 source GUID `307549fdcda7a754e9e615beee716ddb` maps to edited GUID `4582a77e1997ea74e868dd5faee71b99`, whereas UiFocus maps to `07b274cd40965fa46853e90b7114b020` and InventoryFocus to `3c83f9d961c8e284ba562a3c78f88ffb`.

SHA-256 checks confirmed that every original Step MP3 in Scrapshift matches its supplied reference recording byte for byte. This verifies provenance of the inputs, not perceived similarity of the processed outputs. `ContextSoundDesign.Process` uses a brief filtered-noise layer for Step and separate tone layers for UiFocus/InventoryFocus; it retains the source recording. The approved context audio and music should be preserved while investigating event routing.

## Reference cadence and discovered mismatch

Reference root: `E:/UnityProjects/brotato-(3.x)` (read-only).

- `entities/units/player/player.tscn:1169` assigns all six Step recordings. Its external resource 38 points to `player_move.tres`.
- `entities/units/player/player_move.tres` has a 0.6-second looping move animation. Its method track calls `play_step_sound` at 0.1 and 0.4 seconds: a first step after 0.1 seconds and subsequent steps every 0.3 animation seconds.
- `entities/units/player/player.gd:203` resets idle playback speed to 1; line 205 scales movement playback by `get_move_speed() / stats.speed`. `player_stats.tres` supplies base speed 450.
- `player.gd:606–610` selects a Step recording with -6 dB gain and 0.1 pitch variation, unless the debug invisible flag is active.
- The swim move animation also calls at 0.1/0.4 seconds, but uses its separately assigned swim sounds. This investigation does not assert swim/map behavior parity.

At the initial audit, Scrapshift's `PresentationPolish.TickFootsteps` emitted immediately on movement start and then every fixed 0.28 seconds. This is about 7.14% faster than the reference's baseline cadence and does not scale with movement-speed stats. Stopping resets the countdown to zero. This deterministic difference is a valid focused fix regardless of whether it explains the user's particular sound. A source-derived cadence should use a 0.1-second initial delay followed by 0.3-second intervals, with animation time scaled by `Rules.MoveSpeed / 4.5`.

The reference's -6 dB cue gain corresponds to about 0.501 linear amplitude. Scrapshift's existing 0.18 gain is quieter; this audit does not recommend increasing it in response to a noise complaint. Cross-engine voice, master-volume and processing differences also prevent treating that number as an audibility comparison.

## Menu event audit and acceptance check

Before this feedback, `TickMenuNavigation` already returned while `Rules.Phase == Playing`; ordinary combat HUD rendering also does not draw menu buttons. Therefore the source does not establish that controller movement itself intentionally navigates menu buttons during combat. The separate focus-transition defect and UI hover paths require inspection and recording; hearing a repeated sound cannot alone identify its cue.

The controls agent owns diagnostic cue counters in `GameAudio` and `ControllerCheck`, plus focus fixes. Root owns cadence and any asset/mix changes. Source-derived checks for the integrated build:

1. Enter character selection with the mouse, leave the pointer over a different control, and press A without first moving the stick. The visibly focused control must be the one activated. Repeat entering with the controller, then returning with B.
2. Record cue counters before a continuous ten-second combat movement interval. `UiFocus` and `InventoryFocus` requests must remain unchanged; Step requests should advance with movement. Stop for several seconds: Step requests must stop too.
3. Repeat movement via keyboard and controller at the same speed stat. Their footstep timing must agree. At base speed, the source schedule is 0.1, 0.4, 0.7 seconds, etc.; faster/slower movement stats scale animation time.
4. Verify menu navigation still produces one focus cue when focus actually changes and that moving against an edge does not generate repeated focus sounds without a focus change.
5. Obtain another physical controller check and listen to the integrated build before describing the user's audio complaint as resolved. Counter evidence can identify the event class; it does not substitute for judging the resulting sound.

## Focused implementation following the audit

`PresentationPolish.cs` now uses `FootstepTiming` to advance the reference contact schedule in animation time, scaled by `Rules.MoveSpeed / 4.5`. It retains timing phase when speed changes and resets on stopping or `ClearFootDust` (the existing room cleanup path). The reference's `on_room_cleanup` likewise stops smoke and returns its animation to idle. The foot dust timer and approved Step sources, processing, pitch and gain are unchanged.

`Tests/Editor/FootstepTimingTests.cs` adds seven cases covering the initial delay, loop contacts, three speed ratios, phase retention on a speed change, stop/cleanup reset, and frame partitioning. These are source-derived timing checks, not audio listening or controller hardware checks. Integration test results and the exact review build must be recorded separately after execution.

## R5 physical feedback and PCM investigation

The user reported that the button-like movement sound persisted in R5. Root reported the recorded R5 counters as 36 combat footstep requests and zero combat menu-focus requests (46 menu-focus requests occurred in menus). This identifies which movement cue to investigate; it does not invalidate the user's description of the sound.

Codex decoded the actual six MP3 recordings with the installed `C:/Program Files/ShareX/ffmpeg.exe` and compared their sample content with the actual baked profile-215 WAV files. All six baked WAVs are distinct from the UiFocus and InventoryFocus WAVs: aligned sample correlations range from -0.0247 to 0.0569. This rules out identical PCM content under different GUIDs in these audited assets, without claiming that sounds with different PCM cannot be perceived as similar.

The important defect was gain. Every R5 baked Step WAV peaked at approximately 0.92, while the source recordings were much quieter, particularly variants 4–6. The R5 source importer had `forceToMono: 1` and `normalize: 1`; the processed profile then normalized against those imported samples. That preserved the gain increase already introduced during import. The bake's mild filtered-noise layer and 31-Hz amplitude modulation remain in the signal. This profile does **not** use the echo paths found in `CyberSoundBuilder.Process` for profiles below 100.

| Variant | Raw stereo peak | Raw stereo RMS | Baked mono peak | Baked mono RMS |
| --- | --- | --- | --- | --- |
| 1 | 0.4863 | 0.02186 | 0.9200 | 0.03920 |
| 2 | 0.2436 | 0.01922 | 0.9200 | 0.07151 |
| 3 | 0.2641 | 0.02276 | 0.9200 | 0.07553 |
| 4 | 0.1344 | 0.01187 | 0.9200 | 0.07111 |
| 5 | 0.1339 | 0.01068 | 0.9200 | 0.07522 |
| 6 | 0.1288 | 0.01388 | 0.9200 | 0.09272 |

Source and baked durations are 0.6531 and 0.7931 seconds respectively (the process adds a 0.14-second tail). For a separate aligned comparison using FFmpeg's equal-power mono downmix, correlation is 0.9930–0.9963 with zero lag, and baked-to-source fitted gain is 1.53–6.05. Mono downmix conventions differ, so those fitted ratios are diagnostic comparisons rather than a cross-engine loudness measurement. Arithmetic channel averaging produces still lower source peaks. The raw stereo figures above avoid interpreting FFmpeg's mono conversion as the original recording.

Detailed measurements: `artifacts/gameplay-parity-native-2026-10-15/footstep-audio-audit/source-vs-edited.json`. That directory also contains decoded original mono previews at the existing 0.18 bank gain; they are diagnostic exports, not replacement game assets.

Recommended bounded pipeline fix: preserve the original Step channel layout and energy during import (`forceToMono: false`), or disable mono normalization if mono is retained; make the Step exception persistent in `AudioBuilder.EnsureMovementUiAudio`, which currently forces all these cues to mono. Rebuild only the six Step contextual edits so the approved texture remains restrained relative to the supplied recording. Leave all other approved audio, music and gain settings intact. Confirm actual decoded source energy after Unity import and inspect the regenerated WAVs before attributing a reduction to this change. A runtime resolved-clip record and isolated physical listening check are still needed before calling the reported noise resolved.

### Bounded importer and bake correction

Implemented `ContentBuilder.EnsureFootstepImportSettings` and the corresponding Step exception in movement/UI audio generation. The six source MP3 importer metas now preserve stereo and disable normalization. Both a full contextual bake and `CyberSoundBuilder.GenerateFootstepsOnly()` enforce that import policy. The latter uses the same existing processing/writer path but regenerates only the six profile-215 WAVs and writes decoded source/baked channel, peak, RMS and energy measurements to `artifacts/footstep-audio-energy.json` in the validation project. It does not regenerate other contextual edits or change the bank gain/pitch.

`FootstepAudioTests` adds eight cases reading actual Unity importers and decoded audio data: six variant/channel/energy checks, an idempotent guard check that snapshots all other bank cue importers, and the retained bank mix/Pickup routing check. `AudioIntegrationTests` now expects the supplied two channels for Step while retaining the mono requirement for the other cues. These checks require the footstep-only bake before running.

### Executed validation for the R6 correction

The first bake attempt exposed a compile error: Unity's `AudioImporter` has no public `normalize` property. The corrected guard reads and writes the actual serialized `m_Normalize` field, matching Unity's AudioImporter inspector, and fails clearly if that field is absent. The later footstep-only bake succeeded in the isolated validation project.

Actual Unity-decoded measurements after that bake are saved in `artifacts/gameplay-parity-native-2026-10-15/footstep-audio-audit/after-energy.json`:

| Variant | Imported source peak | New edited peak | Source channels | Edited channels |
| --- | --- | --- | --- | --- |
| 1 | 0.49298 | 0.47778 | 2 | 2 |
| 2 | 0.22623 | 0.23315 | 2 | 2 |
| 3 | 0.26407 | 0.27457 | 2 | 2 |
| 4 | 0.13135 | 0.13556 | 2 | 2 |
| 5 | 0.13293 | 0.12857 | 2 | 2 |
| 6 | 0.13461 | 0.12933 | 2 | 2 |

Every edited clip's total sample energy is within 0.007% of its imported source, including the contextual tail. RMS is lower over the longer edited duration; it should not be confused with lost total energy. These are numerical decoding measurements, not a listening assessment. Small differences between Unity's source peaks and FFmpeg's earlier values reflect different decoding/import paths; the new regression compares source and edit within Unity.

The before/after hash comparison in `footstep-audio-audit/changed-clips.json` confirms that exactly the six profile-215 WAVs changed and all other 266 contextual WAVs stayed byte-identical. The CyberSoundLibrary asset hash also remained unchanged. The approved remaining audio and music were not regenerated by this correction.

The focused FootstepAudio, CyberSound, AudioIntegration and FootstepTiming XML was inspected and copied to `artifacts/gameplay-parity-native-2026-10-15/footstep-audio-final.xml`: **23/23 passed**, zero failed/skipped, in 5.0103641 seconds. R6 built with zero errors/warnings and is running from `Builds/GameplayParity20261015R6` with isolated controller observation; its state detects the Xbox Controller. Build and 465-input synchronization manifests are in the same artifact directory. The user's physical listening confirmation remains pending; neither the corrected cadence in R5 nor the counter evidence alone resolved the user's perceived sound complaint.

## User acceptance - R6

The user physically retested R6 and reported: "Okay, the sound is gone." This closes the movement/button-like sound complaint. It does not establish acceptance of every controller binding or exact gameplay parity.


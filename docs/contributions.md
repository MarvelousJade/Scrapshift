# Contribution and evidence boundaries

The user chose the Brotato-inspired direction, requested a small polished Unity game integrated with AssetScope, and required incremental commits and interview documentation. Code, original procedural art setup, automated verification and initial documentation in this build session are implemented with a coding assistant under that direction.

Do not turn assistant-run checks into claims of personal manual playtesting or independently handwritten implementation. This project is portfolio work, not professional production experience. No players, revenue, team metrics, hiring outcome or business impact are claimed.

Add the user's actual hands-on observations and edits below as they occur. A suggested interview answer is a rehearsal aid, not evidence that an event happened.

## Hands-on record

User visual feedback during this iteration: the walk appeared to repeat two poses, then appeared backward; six independent floating weapons do not need gripping hands; purchased items should appear on the body; the green battery incorrectly suggests a health pickup. These observations drove the revisions below. A full normal-run playthrough and final acceptance are not recorded.

## Original art iteration — October 11, 2026

User: supplied twelve reference screenshots, requested generated artwork, retained reference sound, and specified idle/move animation, gun firing animation and varied drops. Assistant: inspected screenshots, prompted five original transparent art sheets using built-in image_gen, implemented sprite imports and Unity animation/loot effects, and performed the recorded tests/captures. Generated art is not candidate hand-drawn artwork. Historical paragraphs below describe earlier versions; the active scene now uses generated sprite art and reference audio. User acceptance and a complete human playthrough remain unrecorded.

Follow-up corrections: the user identified the stride/pose problems and ambiguous battery, and requested visible item equipment. Assistant generated and integrated the revised walk sheet, added wearable slots and shop preview, removed the battery from the active drop pool, and retained the reported test limits. Six sheets are active; earlier revisions remain as history.

## Presentation v2

User: supplied the local Brotato reference and directly identified weak character/enemy design, UI, upgrades and missing combat feedback. Assistant: inspected the supplied material, authored new mesh art and Unity code, introduced randomized upgrade rules and regression checks, investigated DEV-004, and performed automated/desktop validation. User playtest and acceptance of the revised art are not yet recorded.

## Audio and tool-use feedback

The user explicitly criticized the sound and confirmed the request meant sound effects/audio, then asked whether the finished companion tool could be used in the game. Assistant authored and integrated the new sound bank/mixer, updated AssetScope to 0.3.0, ran the integration and mixer-output checks, and documented the importer API issue. The user's comments are direct product feedback; they do not establish a completed five-wave playtest or approval of the new mix.

## Reference reconstruction

User: required analysis before implementation, close reference spawning/enemy types, separate items/weapons/upgrades, matching UI/audio/death feedback, and authorized using supplied media. Assistant: parsed reference resources read-only, implemented Unity systems, selected/copied media with hashes, wrote deterministic economy/scheduling tests and performed rendered review. Supplied sprites/audio are Brotato assets, not assistant or candidate art. Source inspection does not establish official open-source provenance. The original procedural scavenger remains a separate asset. The latest 20-wave version has not yet received a complete user playtest or acceptance.


Current enemy/spawn/UI pass: Shaoyu requested comparison with the Brotato reference, closer combat and UI, a smaller item pool, and ultimately twenty waves after briefly suggesting five. Codex inspected supplied source/screenshots, implemented schedule/placement and attack-state changes, revised the interface, authored tests/fixtures, and ran the recorded checks. No fresh user acceptance or human twenty-wave playthrough is implied.


October 11 parity pass: Shaoyu authorized closer enemy/spawn/upgrade behavior after source comparison, retaining original art, and suggested Unity command-line use while the Editor was open. Codex authored the audit, code, regression tests, incremental commits and validation reports. Tests/builds used an isolated project copy to preserve the user's open Editor. Actual user playtesting/acceptance of this revision is still unrecorded.

## Weapon-set pass

User: requested source comparison, bonuses for our weapons, visible UI, continued documentation and incremental commits. Codex: inspected local reference scripts/resources, implemented the three sets and UI, authored tests/fixture, corrected the test's Harvesting/HP assumption, ran Unity CLI checks in an isolated copy and inspected rendered evidence. Existing original art/content remained unchanged. No user acceptance, manual hover verification or new full normal playthrough is recorded.

## Loot and survival pass

User requested comparison and implementation of the biggest remaining gameplay gaps, keeping original art and incremental documentation/commits. Codex prioritized loot/survival after reading supplied source, implemented rules and UI with a procedural crate indicator, authored eighteen tests and a staged runtime fixture, ran Unity CLI validation and inspected rendered evidence. Codex also retained/documented the normal smoke's first-wave loss. No new art/audio assets were generated or replaced, and no user playtest/acceptance is recorded.

## Spring animation and simplified anatomy

User specified body-only enemies, an armless player with tiny feet and spring cadence, and requested actual reference sprites. Codex inspected/displayed local Brotato sprites and animation resources, edited the existing original art using built-in imagegen, integrated the body/feet hierarchy, inspected runtime output, fixed atlas clipping and documented the real regression failure. Two PNG edits are AI-generated; source pixels were not manually painted. User acceptance of the new motion is not yet recorded.


## Modular character and clean style

Shaoyu requested a shared body with swappable eyes/hair/beard, item embellishments and tiny pointed legs, then selected the clean style and requested removal of player panel lines. Codex compared the supplied appearance resource/priority code, generated/edited original atlases, implemented the resolver and cosmetic selector, added regression tests and rendered fixtures, fixed atlas defects, and documented/committed the work. The user approved the visual direction; no complete manual gameplay acceptance is implied.


Leg correction: Shaoyu rejected the close-set straight legs and specified a wider outward curve. Codex regenerated only the leg sprite, shared world/shop spacing constants, adjusted the existing motion checks, and inspected a rendered stance/movement fixture.


## Live reference calibration

Shaoyu supplied and started the recovered Godot 3 project, then requested direct gameplay comparison while keeping Scrapshift art. Codex inspected the running reference and source, implemented shared world dimensions, follow camera, presentation scaling and weapon mounts, and ran automated/rendered checks. No new raster artwork was generated or copied in this pass. The short reference runs and staged Unity fixtures are not human balance/acceptance playtests.


## Combat feel comparison

Shaoyu requested movement/hitbox/knockback/feedback comparison and retention of original art. Codex inspected live reference combat and inherited source scene/script rules, implemented collision profiles, knockback/contact recovery, ordered projectile hits and original procedural feedback, then ran regression/rendered/integration checks and committed the changes. Automated fixtures supply enemies, gear or invulnerability as documented; ordinary human difficulty/feel acceptance is still open.


## Yard and run presentation

Shaoyu requested curved soft map edges/details, original-like weapon positioning, thicker legs, start/end UI and the parachute entrance. Codex inspected the user's paused reference and source without resuming/resetting that run, implemented native geometry and presentation code, ran automated and rendered checks, operated menu controls, fixed the stale-scene migration issue, documented the evidence and committed locally. Existing raster art was preserved. Menu input checks used Options/Escape, Enter through selection, Punk selection and laser selection; the complete start/end transition checks were staged runtime checks. No full manual playthrough or user visual acceptance is claimed.


## UI and motion follow-through

Shaoyu requested direct UI matching, no key legends, silver rounded legs and a moving/shooting comparison. Codex inspected the running reference and source, generated the replacement limb from existing original art, created missing stat glyphs in code, revised UI flow/panels/recoil/landing, operated checks and committed locally. Eighteen-screen staging and supplied-gear combat are automated evidence, not human acceptance or exact full-game parity.

## Player-hit and leg correction

Shaoyu identified the unwanted leg orientation and vocal damage cue and requested closer reference behavior. Codex inspected the supplied scene/scripts/effect image, corrected mounting and preview transforms, traced the erroneous audio mapping, implemented native hit streaks and recovery feedback, validated the build and documented the findings. Existing character/limb pixels were retained. The fixtures and mixer checks were agent-run; no personal manual playtesting or audio acceptance is attributed to Shaoyu.

## Four-profile/fullscreen pass

Shaoyu requested native fullscreen without side bars and specified Random plus four playable characters, with the remaining slots locked. Codex inspected supplied source data, implemented profiles/selection/weapons/fullscreen, generated the additional weapon atlas, diagnosed test/build failures and ran the recorded checks. No human playthrough or independent manual implementation is attributed to Shaoyu. See fullscreen-and-starter-characters.md.


## Weapon selection and inspection

Shaoyu corrected locked weapon visibility and requested source typography/descriptions plus pause/intermission inspection. Codex compared the supplied source, implemented filtering and shared text/interaction paths, diagnosed the recorded failures, ran tests and staged/native-input checks, and committed locally. Reference font/text are explicitly attributed; existing original sprites are unchanged. No full manual playthrough or human visual acceptance is claimed.

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

# Scrapshift — project instructions and user preferences

Updated: 2026-10-21, America/Toronto. This file records the user's instructions from this project's conversation. Read it before working in this repository. Keep it current when the user makes a new decision.

## Scope and precedence

- This is the Scrapshift Unity game project. The parent workspace's job-search and resume instructions do not apply to game work.
- Follow the user's latest explicit instruction when it changes an earlier preference. Do not revive rejected designs from old screenshots, builds, documentation or assistant suggestions.
- These preferences are requirements, not a claim that every feature is implemented or verified. Check the current source and relevant evidence before describing current behavior.
- Reference means the user's supplied **Brotato** game, screenshots and local reference project. Scrapshift keeps its own characters, cybernetic item identities and artwork while closely following the requested reference behavior and presentation.
- Preserve working features and art the user approved. Make focused changes; don't replace the whole system just because one attachment needs correction.
- Complete authorized work and show concrete results. Resolve routine implementation details without repeatedly asking permission. Ask only for consequential missing information that cannot be established from the source or references.

## Reference inspection and claims

- October 15 latest: the user wants game logic and gameplay behavior to match the supplied original exactly, including character-specific weaponless starts. This supersedes the earlier automatic two-weapon Scrapshift start. The reference adds the selected weapon, then only weapons explicitly granted by character effects; some characters have no weapon. Treat catalog equality and passing fixtures as partial evidence; keep a source-backed mismatch list and correct behavior before claiming parity.
- October 15 controller retest: the user confirmed the first character should start with one weapon. Keep character-specific extra starting weapons and weaponless exceptions; do not restore a universal second weapon. The user reported selected highlights disagreeing with controller activation (A returning to Back), plus rapid UI-like sounds while moving. Verify actual menu lifecycle and distinguish menu cues from footstep cues in native evidence.
- Inspect the relevant reference implementation, resources, animations or footage before asserting how the original behaves. The user expects independent inspection where tools permit it.
- Supplied reference source has been available at `E:/UnityProjects/brotato-(3.x)`; some historical notes use `E:/UnityProjects/Brotato`. Check which path exists. Treat the reference as read-only.
- Save relevant reference screenshots or source findings alongside review evidence. Use source inspection and native captures where available; don't pretend static screenshots establish movement, timing or sound.
- Distinguish matching catalog numbers, implemented behavior, automated tests, native visual fixtures and actual playtesting. A passing data audit does not prove exact gameplay parity.
- Verify character modifiers, weapon values, stats including Luck, item effects, unlock eligibility, shop pools, weighting, rarity and price growth. Do not describe a simple uniform shop randomizer as equivalent to the reference.
- When a reference fact is uncertain, investigate and state the limit. In particular, do not repeat unsupported claims that all turrets rotate, that none rotate, or that every projectile is interchangeable.

## Artwork and icon identity

- Use **Scrapshift's own art** for visible characters, items, weapons, pets, turrets and wearable overlays. Original-game visuals are references for behavior, shape and layout, not replacement player or wearable sprites.
- The user approved the cleaned icon set. Keep clean flat colors, clear silhouettes, strong black outlines and restrained shading. Remove grain, rusty patches and unnecessary surface clutter from icons, pets and turrets.
- Avoid heavy grainy/drop shadows on cards and icons. Match the reference's restrained dark edge/shadow treatment. Eliminate stray black lines at the bottom or edges of colored cards and selected icons.
- Every inventory item must resolve to its own appropriate icon. Do not silently use a stat icon, wrench, turret or another item's sprite as a fallback. Audit the entire catalog when fixing a recurring mapping problem, including the shop, pause inventory, rewards, tooltips and Codex.
- Piercing Stabilizer was specifically reported as having the wrong/generic art. Companion items must depict the actual companion, not a turret; this includes the Assault Companion/Ratzilla mismatch.
- Preserve transparency and prevent neighboring atlas cells from leaking into an icon or world sprite. Check actual imported assets and rendered output, not only filenames.
- Crate drops and their UI icons should use an original cyberpunk design, replacing the old disliked crate.

## Shop, rewards and HUD

- Hovering item or weapon inventory icons should turn the tile background white, with a hover sound distinct from the price-button hover sound.
- White hover/selection backgrounds must retain rounded corners and clean edges. Avoid square white overlays behind a rounded tile.
- Choice and purchase-price buttons should be visible medium/dark gray like the reference, not nearly invisible black.
- Price buttons show a numeric price plus currency icon. Keep capacity messages such as "Slots full" outside the narrow price button; never clip explanatory text into it.
- Align currency amounts vertically with their currency icons, including the shop header, prices and reroll controls.
- Remove the parenthesized **Items** count. Retain weapon capacity information such as `Weapons (2/6)` where the reference uses it.
- Include thin muted green separators beside inventory headings, the sort selector and sort-direction control. Inventory sorting must function.
- Include the current character in the item inventory, with a tooltip describing that character's abilities.
- Effect-leading stat icons must be large and readable. No missing icons, raw `[[range]]` tokens or tiny symbols in item, character, elemental or weapon descriptions. Inspect all description renderers, including the Item found screen.
- On **Item found**, remove the bottom Weapons/Items inventory strip.
- Health and experience bars should have matching width/height, a slightly larger/taller reference-like footprint, and especially thick black outlines. Move the money and harvesting rows down enough to leave a visible gap below the XP bar.
- Remove the small health bar beneath the player character; use the main HUD health bar.
- Match reference item-price and reroll escalation, including free rerolls, discounts/modifiers and locked-offer price retention. Verify rather than guess the formulas.

## Pause screen

- Pause should be available throughout a run, including shop, upgrades and reward screens, not only active combat. Resume the correct underlying state.
- Match the reference's broad, dark left-side buttons and spacing: Resume, Restart, End the run, Codex, Options, Return to Main Menu, with real corresponding behavior.
- Keep the scene behind the pause screen appropriately dark. Inventory tile backgrounds should also be dark with restrained transparency.
- Use the reference-like weapons/items arrangement and six items per row in the pause inventory; avoid oversized opaque central panels.
- Put wave progress along the bottom: completed yellow dots, upcoming light dots, a character marker at the current wave and a boss marker at the end. Show milestone numbers and the applicable danger/wave/run information.

## Options and controls

- October21 physical Xbox core acceptance: the user operated R40's physical Xbox and confirmed "Done—all worked" for left-stick/D-pad menus, A/B, movement, right-stick aiming and Menu pause/resume.268 snapshots/136 Playing and83 active aim samples support this; no virtual Gamepad was injected. Manual aiming enabled in isolated profile.159 package hashes match. Projectile counter stayed0; no independent ranged-firing or X/Y/shoulder/trigger/reconnect/transport/remapping claim. See `docs/controller-hardware-2026-10-21.md`; this supersedes deferral for these core checks.


- October 17 reference input observation: the user confirmed only mouse clicks work in the original Godot run; keyboard movement, Enter/Escape and Ctrl do not. The supplied project contains blank keyboard key codes (see `docs/reference-input-diagnosis-2026-10-17.md`). Preserve the original read-only; use isolated reference fixtures and distinguish their temporary bindings from recovered original defaults.

- October 15 latest: the user requested Unity's newer Input System for controller input while preserving working keyboard and mouse controls. Physical Xbox testing confirmed A/B button responses but exposed nonfunctional sticks and D-pad caused by malformed legacy input settings. Migration to the newer system is underway; device detection, helper tests and simulated input do not establish hardware verification. Recheck actual movement, aiming, menu navigation, confirmation and cancellation on the physical controller before claiming support.
- Inspect and implement the reference options relevant to the game as it exists. Visible options must actually affect the corresponding system; don't add decorative nonfunctional toggles.
- Settings labels and values should be as readable and as large as the menu options. Keep long labels inside their rows.
- All settings rows, including **Sound effects volume**, need the consistent dark strip behind them.
- Remove bottom explanatory/help text and resolution/F11 footer text from the settings/menu screens, as requested. Keep necessary setting labels and controls.
- Put map-related preferences in settings now that multiple maps exist. Do not put a map-selection control back onto character selection.
- Manual aiming/attack-related options must work as specified by the reference. Explain the distinction between manual aiming with cooldown-based automatic firing and one-click-per-shot behavior when relevant; don't claim one while implementing the other.
- The user asked about controller support. Inspect and report its real state and test actual bindings/navigation before claiming support. A screenshot fixture or mouse/keyboard test does not prove controller compatibility.
- Keep reduced-motion, volume, display and other existing preferences functional during presentation changes.

## Main menu and character selection

- Use a clean, simple illustrated title composition with a similar broad format to the reference, built from our own artwork. The user requested a generated whole background image rather than a sparse black scene.
- Main-menu hero: two substantial weapons, correctly oriented, with simple fingerless body-colored hand shapes around their handles. Decorations should correspond to actual in-game items.
- Remove the hero's goggles and body plate from title art; use a single eye scope instead, per the later request. This is a title-art preference, not removal of the wearable goggles item.
- Animate the title character with the game's gentle squash/bounce, not an unrelated floating motion. The user later asked for **slower** title-character animation.
- Background enemies/bushes should subtly rock clockwise then counterclockwise, returning back and forth. Avoid a constant full orbit or busy distracting motion when matching that request.
- Character selection uses a restrained, enlarged view of map terrain behind the UI. The character's portrait card should look like the character is standing in a particular place on the map. Avoid repeated item/stat icon wallpaper or an overly dense miniature-map background.
- Portrait body and legs should remain attached and move together. Preserve the requested gentle idle movement and occasional jump/spin where applicable; don't let the legs remain visually disconnected from the animated body.
- Match the reference portrait/card proportions. Use a much wider character grid with fewer rows, close to the supplied three-row reference arrangement.
- Character selection should have the character panel, a **Records** panel and **Run options**. Show real records where available and appropriate empty-state information otherwise. Remove the former map selector from this screen. Don't imply unsupported run modes are functional.
- Fit the start/menu screen to the user's display while preserving aspect ratio. Avoid stretching the scene or artwork to fill mismatched resolutions.

## Maps and pickup feedback

- Remove the previously disliked large/awkward stones. The later terrain request permits small reference-like ground rocks, grass tufts and sparse debris: don't interpret the earlier removal as a permanent ban on all tiny terrain details.
- Grass should resemble the simple, small reference tufts; avoid the disliked oversized/angular grass. Match the reference's unobtrusive distribution and density.
- The user requested five additional map themes. Preserve existing map variety and applicable settings; verify actual counts before reporting completion.
- Material/consumable pickups need readable effects and sound. Replace the plain green circle impression with reference-like green splash/particles from the player and a soft wind/whoosh where appropriate.
- Consumable pickup should have the requested eating/crunching sound. Keep it distinct from ordinary material pickup feedback.

## Audio and music

- Latest: user approved the weapon-context-v1 previews and requested the same context-sensitive approach across game sound effects. Route weapon and support edits by their role as well as source clip, so shared recordings do not force unrelated weapons to sound identical. Cover UI/status/impact/footstep/consume effects too, with restrained treatment appropriate to their purpose. Keep existing music and the explicit original material Pickup exception. Preserve the four approved preview waveforms as canonical examples.

- Latest pickup exception: the user requested the original sound when collecting a drop. Material Pickup must bypass cyberpunk processing and retain its supplied reference variants. Keep the other approved tuned sounds, including Alien Eyes, Victory and Defeat.
- October 15 latest: the user approved the `tuned-v3` sci-fi/cyberpunk preview style and requested applying it to the game audio, then explicitly included Victory, Defeat and Alien Eyes in the tuning. Keep their supplied Brotato recordings as the source, with all variants, but process them too. Preserve existing music. Do not revert to the rejected synthesized jingles or earlier weaker tuning.
- October 15: replace the rejected synthesized Victory/Defeat jingles with the supplied Brotato `main.tscn` assignments: victory `Fear 10.mp3`, defeat all nine assigned `Madness` variants. Keep zero pitch variation and the reference -5 dB cue gain. Existing preview labels saying "Original" mean the current game baseline, not necessarily reference recordings.
- Pulse is still used by Alien Eyes in Scrapshift. The supplied reference player uses `alien_sounds` (`bullet_impact_water_01.wav` and `_02.wav`); do not infer silence from the empty weapon `shooting_sounds` list. Latest decision: replace the synthesized Pulse with both supplied Alien Eyes clips, using the reference 0 dB cue gain and 0.1 pitch variation, and rebuild the game.
- Latest music preference: **energetic EDM with softer synths**. Earlier EDM/metal requests were superseded by rejection of harsh/distorted music.
- Remove or soften the repetitive high-pitched solo "dadada" passage. Avoid piercing lead tones and harsh metal distortion while retaining an energetic battle feel.
- Latest timing: music begins when the player finishes choosing a weapon and clicks the **first Start** to begin the run. Do not start it merely by opening the title/selection screen. Avoid restarting the track at every wave or shop transition.
- The user explicitly requested supplied original-game sound effects for pickups, support actors and weapon impacts where appropriate. Inspect the actual reference sound assignments and use the available supplied effects when authorized and technically available; don't substitute one generic shot/impact sound for everything.
- Do not claim reference effects are our original audio or imply commercial redistribution rights have been established. Track third-party sound provenance; release/licensing status is separate from the user's requested local implementation.
- Listen or obtain actual audio evidence where supported. Do not claim to have heard the result based only on sample-rate, duration or waveform checks.

## Turrets, pets and spawned entities

- "Pets / companions" was the user's clarification of "pad/path." Treat these requests as game companions, not ChatGPT desktop pets or healing pads unless explicitly identified.
- Turrets, each pet and other spawned objects need distinct suitable art and matching inventory icons, not ground-placed wrench/weapon sprites.
- Match each reference entity's spawning, multiplicity, following/movement, targeting, range, cooldown, projectiles, special effects and sounds. Include mice/Ratzilla and other spawned actors rather than checking only the cat.
- Turret art must support the intended base/head relationship. Where aiming is implemented, separate the stationary base and rotating firing assembly and rotate around the actual joint, not the sprite center or lower body.
- Catling must look like a cat with a substantial Gatling gun mounted on its back. The gun needs a firmly attached back platform/base and separately aiming barrels, with no visible floating gap. The weapon should be larger and readable.
- Keep all support bodies and attachments visually connected. After changing the cat's rig, check the other pets as well; don't break their rendering or animation.
- **Latest decision: omit pet leg movement.** Earlier alternating walking-leg requests were superseded by the explicit instruction to remove it. This does not authorize removing player leg animation. Preserve appropriate body movement/flight and reference mechanics.
- Check the Catling proximity boost against the reference: faster firing when applicable, a noticeable special boost animation, and **red glowing eyes** as requested.
- Different turret types need their own projectile effects and sounds as appropriate: bullets, flame, rockets, laser and healing behavior should not all look or sound like an ordinary gun.
- Landmine interaction should be checked against the reference's player/enemy entry/exit activation behavior; the user specifically asked whether stepping on it works.
- Provide an easy-to-launch local support test level/build with the player and all pets/turrets when demonstrating their movement or behavior. Distinguish this test scene from normal gameplay/progression.

## Weapons and enemies

- Match individual weapon behaviors and projectile presentation to the reference, including distinctive impacts and impact sounds. Matching damage values alone is insufficient.
- Audit enemy spawn logic and behavior, including added enemies, elites and bosses, against the relevant reference schedules and scripts.
- Attack/charge telegraphs should have the requested long-arrow treatment: light/white detail, red outline and translucent red interior.
- Use those indicators **only for the attacks that use them in the reference**, not every attack or every enemy. Inspect which attacks qualify before adding them.

## Achievements, unlocks and Codex

- Inspect the reference achievement, Codex and unlock systems and implement their functional relationships, not only their screens.
- Before an achievement is unlocked, disclose how to unlock it; do not reveal the complete reward effects/details prematurely.
- Match the reference right panel: achievement/alien banner at the top with name and requirement, Reward heading below, then a reward card with the correct icon, name, type/character information and effects when unlocked.
- Use unique appropriate achievement icons inspired by the reference's organization. Do not fill achievements with the same reward/turret placeholder.
- Achievement/Codex grid tile backgrounds should be more transparent, per the user's explicit clarification.
- Show an achievement-unlocked popup at the bottom left when an unlock occurs.
- Fix icon mappings consistently across locked/unlocked views, reward cards, inventory and Codex.

## Wearable appearance system — keep and maintain

- Keep the overlay system. The user's earlier suggestion to remove it was superseded by approving the corrected approach and requesting the other items be updated.
- Follow reference eligibility, slots, priority, depth, duplicates, removal and character-specific blockers using **our own artwork**. This includes cat-related appearances; do not import original-game character appearance PNGs into the playable build.
- Use dedicated fitted wearable pieces. Do not paste whole inventory/shop icons in front of the character or automatically stretch every accessory into a generic slot rectangle.
- Register each piece to the body origin. World sprites and portraits must agree on pivot, position, size, layering, mirroring and animation. Inherit the body's squash/bounce while keeping attachments connected.
- Preserve the approved full-canvas beanie, jacket, goggles and coffee corrections. Beanie sits on the crown, jacket wraps the lower body, goggles align with the eyes, and coffee occupies the above-head slot. The prior assistant claim that the reference coffee changes eyes was incorrect.
- **Blind Optics** has a dedicated red opaque eye wrap, curved around the face with a side knot, replacing the compressed bandana. Keep its corrected full-canvas artwork and uniform scaling.
- Some characters legitimately block item appearances with higher-priority features. Do not remove these rules merely to make every item visible. Explain intentional exceptions in previews; Ranger's priority-3 eyes block priority-2 Blind Optics.
- Preserve pieces that already fit. Review awkward items individually and regenerate art when registration alone cannot correct the silhouette. The remaining atlas pieces are not all uniformly scaled; do not claim otherwise.
- Review each changed item alone, with relevant combinations, at gameplay size and facing both directions. Visual inspection is required; registration tests cannot establish artistic fit.
- Historical baseline from the last appearance pass: 178 eligible items / 185 layers; five full-canvas wearables and 180 registered atlas layers. These are dated counts, not permanent limits if the catalog grows.
- Relevant sources: `Assets/Scrapshift/Editor/TemplateWearableBuilder.cs`, `AppearanceReferenceBuilder.cs`, `Assets/Scrapshift/Content/OriginalArt/TemplateWearables`, `FittedWearables/registrations.json`, `scripts/register_remaining_wearables.py`, `scripts/sync_template_wearables.py`, and `docs/modular-appearance.md`.

## Generating item images for the layer system

- Before creating or refitting item art, read `docs/wearable-art-workflow.md`. It documents the working generation, registration, import, binding and native-review process, including reusable prompt guidance and the approved template values.
- Generate the inventory icon and the worn part for their respective roles. Keep our item's visual identity, but draw a dedicated attachment that fits our body rather than pasting the complete shop icon onto it.
- Request true transparency and an invisible body template with clear attachment landmarks. Preserve the output's pixels/margins and save the exact prompt and reference paths. Measure the actual generated result; requested pixel positions and output sizes are not guaranteed.
- Register new dedicated canvases through an authored pivot and uniform pixels-per-unit; preserve zero layer offset, scale one and stretch one. Existing atlas pieces use their explicit registration table. Regenerate a wrong silhouette instead of crushing it into place.
- Keep item/layer IDs, slot priorities, depths, character blockers and compatibility bindings consistent. Extend generation scripts and validation/sync scope when adding a new dedicated item, especially for multilayer items.
- Verify the native character and UI portraits alone, in combinations, at gameplay size, mirrored and animated, with acquisition/removal checks. Tests alone do not prove that the image fits.

## Builds, previews and working practices

- October21 next phase: user explicitly requests merging gameplay work to main, a new optimization branch, then merging validated optimization back to main and updating the web build. Priority confirmed: crowded-wave FPS and memory first, then web loading/build size. Measure before/after and preserve gameplay parity; commit meaningful changes with evidence. Updating the local web build is authorized; publication/account upload remains subject to the separate release instructions.

- October21 integrated optimization/release checkpoint: gameplay merge992e4d2, optimization branch merged into main8fd3186. Full suite1309/1309 and focused40/40 Passed; deterministic200-enemy simulation CPU20.012->12.002ms with matching checksums (UI disabled, not normal FPS/total-RAM proof). Native R2 capture helper fixed; five inspected victory images and fresh staged Won/summary exit0,159 package files. Final nonreview Web from8fd3186:0errors/warnings/exit0,2629 inputs/five built-tested-packaged files hash-match, ZIP readback Passed. Build73,477,798bytes,ZIP73,347,695;5.35% build reduction vs current-source77,628,486 baseline, not older different-content releases. Two weapon sheets reduce packed texture payload9MiB;21 other candidates skipped to preserve dimensions.464audio/23texture imports restored. Current Chromium ordinary pistol controls/pause/focus/fullscreen1920x1080/42%save-reload/console[] Passed. Corrected diagnostic200+enemy/six-weapon crowd1293frames/7.738ms mean/11ms p95, no concurrent build; old Web FPS samples invalid due missing melee-view initialization. Full-stack tracing is diagnostic-only. Raw147MB audio receipts stay local; committed per-path hash summaries preserve proof. See docs/web-release.md and docs/web-crowd-fixture-2026-10-21.md. No upload/newfull20/currentbrowserhardware/all-browser/subjectiveaudio/total-RAM claim; R40 retains full20 and physical Xbox scope.


- October21 current checkpoint: R40 (`Builds/GameplayParity20261020R40/LaunchGame.cmd`), source `ede4094`; suite1286/1286 Passed, focused cleanup/mine12/12 and policy12/12. Actual Player override enters cleanup synchronously: R35 pre-cleanup drain was wrong and is superseded. Corrected mine placement, clipped companion titles and scaled scroll bounds; R37/R38 native receipts cover four start/retry choices,64 placement seeds,22 shop checks,23 world-unlock checks and17 virtual-navigation stages.66 capture hashes reviewed/verified. Fresh source and R40 normal-timing matched-policy campaigns both Won/20/19shops/exit0;18/18 comparison passed. Debug immunity is contact-only: unfiltered R36 lost legitimately with BloodDonation2; matched opt-in acquisition policy excludes periodic HP-cost carriers without changing damage.634 source inputs/159 player files/3816 original inputs/3819 clone inputs hash-verified; build0 errors/warnings. Core physical Xbox subsequently verified on October21; finite matrices do not establish exhaustive interactions/human balance/WebGL. See `docs/gameplay-verification-status.md` and `docs/full-run-r40-comparison-2026-10-21.md`.


- October20 death/item checkpoint: R35 (`Builds/GameplayParity20261020R35/LaunchGame.cmd`), source `debe1cf`; focused27/27 and suite1271/1271 Passed/exit0. Synchronous stat-proc HP with deferred enemy finalization, dying Cyberball membership, pre-defeat drain, plain explosion empty burning/no weapon life steal, and direct explosion kill counters corrected. Six packaged processes Passed/exit0:8 death rows,4 explosion cases/Fireworks14-15/Beard,192 target rows,109 unlock scenarios, one-wave smoke and accelerated natural defeat after8 completed waves. Source4 death receipts/1024 corpse rows/70 payloads; pool-only ordering hypothesis rejected. Build0 errors/warnings;626 inputs/159 player files hash-verified. Broader original Player/Main cleanup, arbitrary interactions, current presentation/hardware and normal-speed balance remain bounded or unchecked. R29 full20 remains historical. See `docs/death-item-integration-2026-10-20.md`.


- October20 item/material follow-up: R34 (`Builds/GameplayParity20261020R34/LaunchGame.cmd`), code `ac6bffb`; suite1251/1251 and focused25/25 Passed/exit0;416 acquisition sequences/4576 snapshots match. Target capture and request-time vulnerability fixed; packaged192-row target review plus forward89/reverse88 HP checks Passed. Build0 errors/warnings;616 inputs/159 files verified. Historical25-kill/24-unit cause remains unknown; Evil Mob zero-value/theft makes kill=material invalid. Four forced original cleanup-boundary cases completed; no Scrapshift payout defect established. See `docs/item-material-followup-2026-10-20.md`. No desktop control or fresh full20 campaign; broader lethal-preemption/corpse timing and arbitrary combinations remain unchecked.
- October20 preserved campaign checkpoint: R29 (`Builds/GameplayParity20261020R29/LaunchGame.cmd`), source `1ff6bb1`, full suite1200/1200 Passed/exit0; corrected static character/item oracle10500/10500 and recorded neutral item-pair oracle22155/22155 match. Current R29 full20 Won/exit0 and unchanged18/18 comparison against fresh original; ordinary-damage run lost wave8 after7 completed, Passed/exit0. Build0 errors/warnings;596 source inputs/159 player files hash-verified after campaigns. See `docs/gameplay-final-checkpoint-2026-10-20.md`. Do not claim current UI/hardware/browser verification or exhaustive timed combinations. Wrench/Screwdriver/Pruner DO register StructureEffect through add_weapon; older exclusion claims are superseded.


- October20 latest authorization: computer/desktop access is allowed again. The user asks to leave physical controller testing for now if it requires their input. Automated virtual input can verify software handling, but cannot establish physical Xbox acceptance; do not ask for button presses in this pass. Preserve earlier hardware evidence as historical.

- October20: finish remaining parity checks and fix confirmed mismatches, then refresh full comparison evidence. Commit each meaningful change separately with related tests/docs; do not make a commit for every small supporting edit.

- October17 documentation: `docs/gameplay-verification-status.md` is the consolidated verified/partial/missing checklist. Keep source/model/native/human coverage separate and update it after new evidence.

- October17 priority follow-up: R25 (`Builds/GameplayParity20261017R25/LaunchGame.cmd`) fixes combined burning timing, cleanup healing, defeat/retry lifecycle, raw HP-shop currency/presentation and retry unlock-pool lifetime. Full suite1181/1181 Passed; all115 unlock definitions have producer/save-reload/next-run cases plus one guard. Three R25 native fixtures Passed/exit0;12 captures reviewed,568 source inputs/160 player files verified. See `docs/gameplay-priority-followup-2026-10-17.md`. Staged checks do not establish exhaustive item pairs or human survival balance; R23 full20 remains historical.

- October17 follow-up priority: the user selected three areas to fix first: combined item effects/stacks; ordinary damage/healing/lethal hits/defeat; special shop alternatives and unlock triggers/persistence across subsequent runs. Use disjoint agent ownership, source-derived focused tests and native checks for confirmed fixes before another broad campaign. R23 and commit affceae are the preserved baseline; previous verification does not close these broader cases.

- October17 latest authorization: the user now requests the complete test suite and a fresh full20-wave comparison against the supplied original, with fixes for confirmed differences and updated bug/interview notes. The earlier hold on whole-suite/full-run testing is superseded for this phase. Use isolated profiles/clones and preserve prior evidence.

- October 15 latest testing order: finish the remaining source audits and gameplay fixes before running the whole test suite or another full-run comparison. Focused checks may support individual fixes. Keep bug documentation and interview notes updated, and distinguish implemented fixes from validation still pending.
- The user requested dividing gameplay parity testing into separate subtasks for concurrent agents. Give each agent disjoint file ownership and source-derived acceptance checks; integrate and validate the combined result once. See `docs/gameplay-parity-parallel-workplan.md`.
- The user requested a separate branch for appearance work. The last verified branch was `codex/appearance-parity`; check current Git state rather than switching blindly. Preserve unrelated/uncommitted work.
- October 15: the user explicitly requested a new branch for the full gameplay-parity pass. `codex/gameplay-parity` was created from the current checkout; existing uncommitted appearance and audio work was preserved and remains in the worktree. Check current Git state before further branch operations.
- Verify the project Unity version from `ProjectSettings/ProjectVersion.txt` (6000.6.5f1 when this file was created).
- Do not launch a second Unity Editor on an already-open project. Use the established isolated validation clone when needed: `E:/Codex Projects/Job Search Assistant/tmp/ScrapshiftParity`. Verify the relevant source/asset copies before attributing its results to this checkout.
- Build/test within the authorized scope. Synchronize only validated intended asset fields and import metadata; avoid copying a generated catalog over unrelated gameplay changes.
- The user has repeatedly encountered stale previews and wrong-build confusion. Give every review a clear purpose, actual build path and branch. Refresh its screenshots from the exact build being shared. Identify what changed and which previous review it replaces.
- Show actual native character screenshots for wearable corrections. A mockup or HTML image gallery is not proof that the playable build uses the same assets.
- Provide a direct local Windows launch path and keep the full player folder together. The user questioned receiving a compressed file; prefer an unpacked runnable build for local testing. ZIPs can be additional distribution artifacts, clearly labeled.
- Do not claim a game is running just because its build succeeded. Do not claim tests passed without reading results. Old test totals in documents are historical evidence, not current results.
- Label visual fixtures, automated tests and ordinary gameplay separately. Report remaining limitations without presenting every system as an exact replica.
- Never bypass a tool's security block using another route. In an earlier session the local browser preview route was blocked; do not assume permission to work around it. Local file links and approved native fixture captures remain useful ways to deliver evidence.
- This file records preferences; it does not request new scheduled tasks, autonomous goals, commits, releases, public uploads or unrelated work.

## Web release direction

- The user wants an itch.io release and asked for a web version. Check Web Build Support for the project's actual Unity version before suggesting installation.
- The user explicitly requested adapting/testing browser saves, audio, fullscreen and browser controls, plus reducing loading size and checking crowded-wave performance. Keep these as release requirements; inspect `docs/web-release.md` and current evidence for completion status.
- Verify browser audio startup restrictions, save persistence and control/focus behavior in a real web build. A Windows build does not validate WebGL.
- Prepare a reviewable build/package before any publication step. Do not treat discussion of itch.io as authorization to publish to an account.

## Interview preparation and portfolio documentation

- October 20 README direction: keep the README a clear, concise portfolio showcase focused on the game, engineering highlights, local setup and linked evidence. Keep detailed development history in supporting documents and preserve accurate attribution and validation limits.
- October 20 README follow-up: also showcase game design and UI design, including menus, character selection, combat HUD, shop/rewards, pause, Codex and settings. Remove the attribution section from the README; retain the supporting contribution/provenance records.
- October 20 feature-writing direction: use Brotato's official online feature presentation as a structural reference for concise, player-facing Scrapshift highlights. Adapt the wording to verified Scrapshift features and its own identity rather than copying claims about Brotato-only features.

- October 17 project motivation: the user enjoys Brotato and built Scrapshift to improve their Unity game development ability and practice C#. Present this as the README's reason for the project rather than a job requirement; the AssetScope and portfolio connections are secondary uses.
- Scrapshift also serves as a Unity/C# portfolio project and a practical demonstration alongside the AssetScope Editor tool. Keep interview preparation useful as the game evolves, not frozen at the original small prototype.
- After meaningful implementation, debugging, architecture, performance or validation work, update the relevant interview/supporting documents with concrete evidence. Small cosmetic edits do not require a new interview story unless they demonstrate a useful decision or lesson.
- Maintain `docs/interview.md` for clear explanations and rehearsal questions; `docs/contributions.md` for accurate attribution; and `docs/assetscope-demo.md` for a reproducible demonstration of the game-to-Editor workflow. Keep related details in `docs/architecture.md`, `docs/bugs.md`, `docs/verification.md` and `docs/journal.md` where appropriate rather than duplicating inconsistent accounts.
- For an interview-worthy change, record the problem or user observation, relevant constraints, investigation, decision and tradeoffs, implementation, validation and remaining limitations. Link actual source files, tests, screenshots or reports. Distinguish an observed bug from a preventive edge-case test.
- Clearly separate the user's direction, design choices, feedback and personally performed work from Codex-generated code/art, assistant-run investigation/testing, and third-party reference material. Do not invent personal experience or rewrite AI-assisted work as independently authored by the user.
- First-person interview answers must match `docs/contributions.md` and the user's confirmed hands-on experience. Encourage the user to reproduce, trace and explain an example before presenting it as their own debugging story. Where appropriate say that the project encountered an issue and describe what the user understands, rather than claiming they personally executed every step.
- Explain technical choices in plain language, with enough Unity/C# detail to withstand follow-up questions: serialization and asset references, Editor/runtime separation, state transitions, data-driven rules, rendering/animation, debugging, automated tests and measured performance. Avoid inflated claims about Unity internals or systems this project does not implement.
- Keep a short demonstrable walkthrough: launch the verified build, show a relevant behavior, trace its asset/configuration references in AssetScope, and show supporting validation. Verify current asset paths and menu actions before using the walkthrough. Deliberately broken fixtures must be labeled and kept separate from the playable game.
- Reconcile stale interview claims after substantial project changes. Old roster counts, prototype scope, test totals, unsupported-feature statements and build paths must not be presented as current. Date historical evidence; verify the current facts rather than replacing them with guessed numbers.
- Never invent FPS improvements, performance benchmarks, ownership, complete reference parity, controller support, WebGL readiness or commercial-release rights. Keep game tests, AssetScope tests, scripted visual fixtures and real user playtesting distinct.
- Keep rehearsal actionable: explain a real tradeoff, trace a definition to runtime behavior, understand a regression test, reproduce a bug/fix, and make and validate a small change. Document hands-on completion only when the user confirms it or there is direct evidence.
- If tailoring preparation to a specific interview or job application, read the actual role requirements and applicable workspace interview instructions then. Do not invent a new employer or role, or apply unrelated resume rules to ordinary game development.

## Evidence map and historical reviews

- October17 completed full-validation phase: R23 at `Builds/GameplayParity20261017R23/LaunchGame.cmd`, zero build errors/warnings; complete suite1031/1031 Passed (attempt9), native virtual navigation17 snapshots and settlement2 routes Passed, all exit0. R23 completed fresh normal-time20/Won/exit0 with19 shops; supplied-original attempt2 also completed20/Won/exit0. Expanded comparison18/18 Passed; final receipt109 materials credited1227 to1336, Harvesting109 to115. Postrun source550/player160 hashes match. Combat wall excess-.714s across1050.200 active seconds versus R21+170.537s; separate evolving runs, not identical workloads/FPS proof. Synthetic allocation reduction99.27% per tick is separate diagnostic evidence. Preserve all earlier failures/interrupted traces. R23 virtual fixture alone uses IgnoreFocus; ordinary gameplay focus and physical hardware are separate. See current full-verification/profiling docs; do not claim exhaustive overall parity.


- October17 historical native checkpoint: R14 at `Builds/GameplayParity20261017R14/LaunchGame.cmd`, zero build errors/warnings. R14 only changes the opt-in campaign driver from R13 to use the normal60Hz clock; its normal-speed ONE-WAVE preflight Passed, exit0. Carried unchanged-input evidence: selected172/172 regressions; R13 economy/support43/virtual navigation17 Passed; R12 combat/enemy53/eight slash probes Passed. User accepted requested basic Xbox controls, including source-matching Start pause/B return behavior. Fixed active-run unlock pools and support projectile frames/identity/offset/current age. Full suite and full20-wave campaign have not run. See `docs/native-gameplay-r14-2026-10-17.md` for hashes, preserved failures, captures and limits. Reference remains read-only.

Use the relevant documents as evidence, checking dates and later corrections rather than treating every old statement as current:

- Appearance: `docs/modular-appearance.md`; `artifacts/shared-template-review`, `artifacts/remaining-wearables-review`, `artifacts/blind-optics-review`.
- Historical October15 gameplay review: `Builds/GameplayParity20261015R8/LaunchControllerTest.cmd`, with isolated in-memory preferences/progression; ordinary play uses its `LaunchGame.cmd`. R8 retains R7's eleven corrections and adds animated beam collision, enemy-healer body/layer eligibility and Streamer's structure-linked Armor count. It passed 620 integrated tests, 205 native weapon/tier checks and 53 native enemy/state checks; build had zero errors/warnings. Its fresh normal-timing scripted campaign Passed/Won after 20 timer ends and 19 shops, exit zero; comparison with the frozen supplied-original run passed 14 consistency checks. Follow `docs/gameplay-parity-continuation-2026-10-15.md` and `docs/full-run-r8-comparison-2026-10-15.md` for hashes, acquisition differences and limits. R6's footstep import guard is preserved; the user confirmed "the sound is gone". Other physical-controller acceptance and human survival parity remain separate. R7 (594 tests, 27 support checks), R5 (541 tests) and Blind Optics (374 tests) are historical checkpoints.
- UI/menu/maps: `docs/upgrade-and-inventory-ui.md`, `docs/menu-fit-and-hud.md`, `docs/menu-scene-presentation.md`, `docs/maps-and-title.md`, `docs/progression-and-options.md`.
- Achievements: `docs/achievement-disclosure-and-layout.md`, `docs/achievement-icons.md`.
- Support actors: `docs/support-behavior-audit.md`, `docs/support-art-and-parity.md`, `docs/support-aim.md`, `docs/turret-art-audio.md`, `docs/cat-back-aim.md`, `docs/cat-boost-projectile-audit.md`, `docs/red-boost-projectiles.md`.
- Gameplay/shop/weapons: `docs/item-status-parity-audit.md`, `docs/item-randomization-parity.md`, `docs/gameplay-parity-audit.md`, `docs/combat-feel-comparison.md`, `docs/weapon-recoil.md`.
- Sound: `docs/audio-design.md`, `docs/support-animation-and-music.md`. Older industrial/metal music notes are superseded by the softer-EDM preference above.
- General verification and release: `docs/verification.md`, `docs/web-release.md`, relevant dated artifact reports.
- Interview and portfolio: `docs/interview.md`, `docs/contributions.md`, `docs/assetscope-demo.md`, supported by `docs/architecture.md`, `docs/bugs.md` and `docs/journal.md`.

When a new user instruction supersedes an entry here, update that entry and preserve only enough history to prevent regression. Do not claim this file replaces the actual conversation or records decisions from other chats that have not been inspected.

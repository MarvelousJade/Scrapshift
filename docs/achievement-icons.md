# Achievement-specific icons

October 13, 2026.

The previous achievement layout used the same alien in every completed banner and the reward art in the grid. The user requested achievement-specific icons following the reference.

## Reference and design

Read `challenges/global/challenge_data.gd` and the supplied `.tres` resources. Each challenge has an icon independent of its reward. Gatherer uses an alien tongue across its series; Survivor uses a scar; character-win challenges use the character who wins, not the rewarded item. This is why the Generalist reward should sit below a Gatherer icon rather than supply that icon itself.

Added 38 original thematic symbols across two generated sheets. Seven companion challenges use our existing corresponding companion art. Character-win challenges render the relevant character portrait. Series share their thematic symbol as in the reference. Thus these are explicit achievement mappings, not 109 separately drawn images. The same mapping is used in completed grid cells, the achievement banner and unlock toast. Reward cards retain their own independent art. Locks conceal achievement icons until completion.

No icon fallback to the first alien or reward is permitted. An unmapped key raises an explicit error; a catalog-wide test checks every definition including difficulty unlock notifications.

## Art provenance

Built-in image generation, transparent backgrounds, original cyberpunk cartoon symbols: broad colors, bold black outlines, no lettering or cell borders. Sheet A is six columns by four rows; B is five by three (the final trophy is unused). Subjects follow the source challenge associations, adapted to our game. The exact ordered mapping is `AchievementArt.Symbols`.

Generated originals retained under `C:/Users/FanSh/.codex/generated_images/01a12d09-527c-71e2-851b-a118bb06309e/`:

- `exec-a9efd910-dd20-4511-b2b8-2818f036dc03.png` → `Resources/AchievementIconsA.png`.
- `exec-bc7a1abb-8820-4add-88b8-d5818ec660de.png` → `Resources/AchievementIconsB.png`.

Some generated icons extend across nominal cell boundaries. `scripts/audit_achievement_art.py` reads connected alpha components and emits padded bounds plus clipping strips. It does not modify image pixels. The existing atlas preview renderer uses these masks to prevent chopped icons or neighboring fragments.

## Validation

Added a production-catalog regression test checking coverage, distinct symbol sprites, Gatherer's family mapping and the character identity for character-win challenges. Build, test results and locked/unlocked rendered captures are saved in `artifacts/achievement-icons-review`. The gameplay rules and unlock requirements are unchanged.
Validation completed: 302/302 EditMode tests passed; Windows build succeeded with zero errors and warnings; all 82 staged UI checks passed. Gatherer completed/locked captures were inspected. Runtime log contains no errors or exceptions. Source and validation copy match across 1,067 files.

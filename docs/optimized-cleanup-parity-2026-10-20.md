# Optimized cleanup parity - October 20, 2026

Source `main.gd:1031-1042` applies common double-material effects once per ground pile and banks the result immediately when Optimize End Waves is enabled. Default cleanup banks raw value. `run_data.gd:898` applies only the common chance multiplier; it does not grant XP, spendable materials, healing or material-pickup triggers.

Added a functional Gameplay option (default Off), a common banking helper and single entry-time processing for both wave and death cleanup. Consumables stay on their existing separate path. Pausing and changing the option during an ongoing cleanup does not retrospectively reprocess ground piles.

Focused `optimized-cleanup-tests.xml`: **21/21 Passed**, no failures/skips, exit0, 0.7435155 seconds; includes five optimized banking cases plus prior survival regressions. Fresh native checks/build are pending. Builder-specific turret conversion has no imported carrier and is not claimed.

Codex inspected and implemented this under the user's parity direction; this is not user-performed debugging or a new human campaign.

## Fresh R26 native result

`artifacts/parity-remaining-2026-10-20/optimized-r26-attempt-1/optimized-cleanup-review.json` is **Passed**, all acceptance flags true; `process.json` records exit 0 after approximately 6.17 seconds. This reviewer read those artifacts and inspected both fresh PNGs. The native log has no case-insensitive error/exception/warning/assert matches. R26 build summary: Succeeded, zero errors/warnings, 401,755,514 bytes.

Both normal and lethal cleanup banked exactly **10** from raw piles of 2 and 3 with guaranteed double materials, despite an unrelated +100% material-value modifier. Spendable materials remained 100 and XP remained zero. Normal fruit healed HP 4 to 7; death left HP zero and the fruit uncollected, with no harvesting settlement. `normal-optimized.png` shows 7/10 HP and +3 HP feedback during cleanup; `death-optimized.png` shows zero HP and the default loss acknowledgment. Banking amounts are established by runtime JSON checks, not inferred from the HUD.

This stages guaranteed boost, pickup positions and model lethal damage while exercising actual Update cleanup/pickup paths. It does not test stochastic distributions, every boost/effect combination, physical input or a human campaign. The earlier pending-native statement is superseded by this fresh result; Builder-specific conversion remains outside imported-carrier coverage.

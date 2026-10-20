# Optimized cleanup parity - October 20, 2026

Source `main.gd:1031-1042` applies common double-material effects once per ground pile and banks the result immediately when Optimize End Waves is enabled. Default cleanup banks raw value. `run_data.gd:898` applies only the common chance multiplier; it does not grant XP, spendable materials, healing or material-pickup triggers.

Added a functional Gameplay option (default Off), a common banking helper and single entry-time processing for both wave and death cleanup. Consumables stay on their existing separate path. Pausing and changing the option during an ongoing cleanup does not retrospectively reprocess ground piles.

Focused `optimized-cleanup-tests.xml`: **21/21 Passed**, no failures/skips, exit0, 0.7435155 seconds; includes five optimized banking cases plus prior survival regressions. Fresh native checks/build are pending. Builder-specific turret conversion has no imported carrier and is not claimed.

Codex inspected and implemented this under the user's parity direction; this is not user-performed debugging or a new human campaign.

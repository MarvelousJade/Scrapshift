"""Read saved original-engine material traces; never launch or modify the game.

The kill/unit residual preserves a first-wave hypothesis, not an invariant:
zone-wide Evil Mobs can steal gold and count as zero-value kills.
Instrumented accounting uses actual unit values, not pickup achievement counts.
"""
import argparse
import collections
import hashlib
import json
from pathlib import Path


def audit(path):
    raw = path.read_bytes()
    rows = json.loads(raw)
    first_wave = [r for r in rows if r.get("wave") == 1]
    timer = next((r for r in first_wave if r.get("event") == "wave_timer_end"), None)
    after = next((r for r in first_wave if r.get("event") in ("finished", "shop_exit")), None)
    if after is None:
        after = next((r for r in reversed(first_wave) if r.get("event") == "scene"), None)
    counts = collections.Counter(r["event"] for r in first_wave)
    instrumented = counts["material_requested"] > 0
    result = {"path": str(path), "sha256": hashlib.sha256(raw).hexdigest(),
              "rows": len(rows), "wave_one_rows": len(first_wave),
              "instrumented": instrumented, "events": dict(sorted(counts.items()))}
    fields = ("simulated_seconds", "enemy_kills", "tree_kills", "material_pickups",
              "gold", "banked_materials", "ground_material_value", "item_count", "harvesting")
    for label, row in (("timer", timer), ("after_cleanup", after)):
        if row:
            result[label] = {k: row[k] for k in fields if k in row}
            # This preserves the historical observation; it is not a universal law.
            result[label]["neutral_unit_hypothesis_residual"] = (
                row["enemy_kills"] - row["material_pickups"]
                - row["ground_material_value"] - row["banked_materials"])
    if instrumented:
        requested = registered = boosted = picked_player = picked_bank = pending_peak = 0
        for row in first_wave:
            kind = row["event"]
            if kind == "material_requested":
                requested += row["count"]
            elif kind == "material_registered":
                registered += row["value"]
            elif kind == "material_cap_boost":
                boosted += row.get("value", 1)
            elif kind == "material_picked":
                if row["player_index"] >= 0:
                    picked_player += row["value"]
                else:
                    picked_bank += row["value"]
            pending_peak = max(pending_peak, requested - registered - boosted)
        ground = after["ground_material_value"] if after else None
        result["unit_accounting"] = {
            "requested": requested, "registered": registered, "cap_boosted": boosted,
            "player_picked_value": picked_player, "cleanup_picked_value": picked_bank,
            "final_ground": ground, "peak_requested_not_registered": pending_peak,
            "performance_removals": counts["material_performance_removal"],
            "balanced": ground is not None and requested == registered + boosted
            and registered + boosted == picked_player + picked_bank + ground}
    result["limit"] = ("Finite first-wave saved-trace analysis. Evil Mob theft/zero-value kills "
                       "invalidate kill-to-unit equality even in wave one. Missing event provenance "
                       "cannot be reconstructed; kill residual alone does not establish lost payouts.")
    return result


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("traces", type=Path, nargs="+")
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    result = {"scope": "Read-only saved first-wave reconciliation", "traces": [audit(p) for p in args.traces]}
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(result, indent=2) + "\n", encoding="utf-8")
    print(json.dumps({"output": str(args.output), "traces": len(result["traces"])}))


if __name__ == "__main__":
    main()

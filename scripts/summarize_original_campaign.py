"""Summarize one original-game attempt from its own trace/process/debug log.

No engine, source project, fixture or profile is modified. Missing final inventory
evidence blocks output rather than substituting a previous run's item stacks.
"""
import argparse
from collections import Counter
import hashlib
import json
from pathlib import Path
import re


def read(path):
    return json.loads(path.read_text(encoding="utf-8-sig"))


def evidence(path):
    return {"path": str(path.resolve()), "sha256": hashlib.sha256(path.read_bytes()).hexdigest(),
            "bytes": path.stat().st_size}


def final_inventory(text):
    blocks = re.findall(r"--Run Data--(.*?)--Run Data end--", text, re.S)
    if not blocks:
        raise ValueError("Current debug log contains no complete Run Data block.")
    block = blocks[-1]
    player = re.search(r"\*\* Player 0\s*\n(.*?)(?=\*\* Player \d|\Z)", block, re.S)
    if player is None:
        raise ValueError("Last current debug block has no Player 0 inventory.")
    body = player.group(1)
    items = re.search(r"^Items: ?(.*)$", body, re.M)
    wave = re.search(r"^Wave: (\d+)\s*$", body, re.M)
    weapons = re.search(r"^Weapons: ?(.*)$", body, re.M)
    if items is None or wave is None or weapons is None:
        raise ValueError("Last current debug block lacks Items, Wave or Weapons.")
    ids = [part.strip() for part in items.group(1).split(",") if part.strip()]
    if any(re.fullmatch(r"item_[a-z0-9_]+", item) is None for item in ids):
        raise ValueError("Unrecognized item identifier in current final inventory.")
    return dict(Counter(ids)), int(wave.group(1)), [v.strip() for v in weapons.group(1).split(",") if v.strip()]


def summarize(folder, debug_log=None):
    trace_path, process_path = folder / "trace.json", folder / "process.json"
    trace, process = read(trace_path), read(process_path)
    if process.get("status") != "exited" or "exitCode" not in process:
        raise ValueError("Current process record does not contain a finished exit result.")
    end = next((row for row in reversed(trace) if row.get("event") == "finished"), None)
    boot = next((row for row in trace if row.get("event") == "boot"), None)
    if end is None or boot is None:
        raise ValueError("Current trace lacks boot/finished events; no complete summary generated.")
    if debug_log is None:
        if not boot.get("save_root"):
            raise ValueError("Trace lacks save_root; pass this attempt's actual --debug-log explicitly.")
        debug_log = Path(boot["save_root"]) / "user/log.txt"
    if not debug_log.is_file():
        raise ValueError("Current source debug log is missing: " + str(debug_log))
    debug_raw = debug_log.read_bytes()
    stacks, debug_wave, debug_weapons = final_inventory(debug_raw.decode("utf-8-sig", errors="replace"))
    if debug_wave != end.get("wave") or sum(stacks.values()) != end.get("item_count"):
        raise ValueError("Current debug inventory does not match final trace wave/item count; investigate before comparison.")
    if Counter(debug_weapons) != Counter(end.get("weapons", [])):
        raise ValueError("Current debug weapons do not match final trace weapons.")
    waves = []
    for timer in (row for row in trace if row.get("event") == "wave_timer_end"):
        start = next((row for row in trace if row.get("event") == "scene" and row.get("wave") == timer["wave"]
                      and row.get("filename") == "res://main.tscn"), None)
        if start is None:
            raise ValueError("Timer has no matching current Main scene: wave " + str(timer["wave"]))
        row = dict(timer)
        row["sampled_simulated_interval"] = timer["simulated_seconds"] - start["simulated_seconds"]
        row["sampled_wall_interval"] = timer["wall_seconds"] - start["wall_seconds"]
        waves.append(row)
    stderr_path = folder / "runner-stderr.log"
    if not stderr_path.is_file():
        raise ValueError("Current runner stderr log is missing: " + str(stderr_path))
    stderr = re.sub(r"\x1b\[[0-9;]*m", "", stderr_path.read_text(encoding="utf-8-sig", errors="replace"))
    script_errors = Counter(re.findall(r"^SCRIPT ERROR: (.*)$", stderr, re.M))
    inputs = {"trace": evidence(trace_path), "process": evidence(process_path),
              "source_debug_log": evidence(debug_log), "stderr": evidence(stderr_path)}
    for name in ("source-manifest.json", "parity_fixture.gd", "project.godot", "runner-spec.json"):
        path = folder / name
        if path.exists():
            inputs[name] = evidence(path)
    # Freeze the actual source debug bytes in this attempt's evidence directory.
    snapshot = folder / "reference-run-data.log"
    if snapshot.exists() and snapshot.read_bytes() != debug_raw:
        raise ValueError("Saved debug snapshot differs from current profile log; preserve both and investigate.")
    snapshot.write_bytes(debug_raw)
    inputs["saved_debug_snapshot"] = evidence(snapshot)
    return {"fixture_result": end, "process_exit_code": process["exitCode"],
            "wave_timer_ends": len(waves), "shop_entries": sum(row.get("event") == "scene" and
             row.get("filename") == "res://ui/menus/shop/shop.tscn" for row in trace),
            "captures": len(list(folder.glob("*.png"))), "waves": waves,
            "final_source_debug_log_item_stacks": stacks,
            "final_debug_inventory_validation": {"wave": debug_wave, "items": sum(stacks.values()),
             "distinct_items": len(stacks), "weapons": debug_weapons, "matches_trace": True},
            "fresh_profile_boot": {key: boot.get(key) for key in ("gold", "level", "item_count", "harvesting", "luck",
             "banked_materials", "enemy_kills", "tree_kills", "material_pickups")},
            "runtime_mode_snapshots": sorted({(row.get("fixture_time_scale"), row.get("fixture_physics_hz")) for row in trace}),
            "fixture_pause_interventions": [row for row in trace if row.get("event") == "fixture_pause_intervention"],
            "final_pause_intervention_count": end.get("fixture_pause_interventions"),
            "diagnostics": {"script_error_count": sum(script_errors.values()), "script_errors": dict(script_errors),
             "errors": dict(Counter(re.findall(r"^ERROR: (.*)$", stderr, re.M))),
             "warnings": dict(Counter(re.findall(r"^WARNING: (.*)$", stderr, re.M)))},
            "inputs": inputs, "summary_script": evidence(Path(__file__)),
            "scope": "This attempt's actual original-engine 60Hz scripted campaign trace/process and final source debug inventory. Intermission timestamps include fixture processing; sampled wave intervals use matching Main scene to timer callback. Neutral supplied loadout/invulnerability/mute; not a human survival, controls/audio or deterministic cross-engine RNG test. Diagnostics and pause interventions are retained."}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--reference-dir", required=True, type=Path)
    parser.add_argument("--debug-log", type=Path)
    parser.add_argument("--output", type=Path)
    args = parser.parse_args()
    try:
        result = summarize(args.reference_dir, args.debug_log)
    except (ValueError, OSError, KeyError) as exc:
        parser.exit(2, "Original summary blocked: " + str(exc) + "\n")
    output = args.output or args.reference_dir / "summary.json"
    output.write_text(json.dumps(result, indent=2), encoding="utf-8")
    print(json.dumps({"summary": str(output), "status": result["fixture_result"].get("status"),
          "exit": result["process_exit_code"], "timers": result["wave_timer_ends"],
          "shops": result["shop_entries"], "stack_count": sum(result["final_source_debug_log_item_stacks"].values()),
          "pause_interventions": result["final_pause_intervention_count"], "diagnostics": result["diagnostics"]}, indent=2))


if __name__ == "__main__":
    main()

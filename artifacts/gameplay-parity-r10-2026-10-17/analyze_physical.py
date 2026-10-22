"""Snapshot the observer-only controller log; do not infer hardware acceptance.

Run with the bundled Python interpreter. The input file is read once, so a live
writer's incomplete trailing record is reported and skipped without polling.
"""
import argparse
import hashlib
import json
import math
from pathlib import Path

BUTTONS = ["A/South", "B/East", "X/West", "Y/North", "LB", "RB", "View/Select",
           "Menu/Start", "Left stick click", "Right stick click", "LT", "RT",
           "D-pad up", "D-pad down", "D-pad left", "D-pad right"]
AXES = ["left_x", "left_y", "right_x", "right_y", "dpad_x", "dpad_y"]


def physical_owner(row):
    return bool(row.get("usingGamepad")) and "XInputControllerWindows" in row.get("lastInputDevice", "")


def length(vector):
    return math.hypot(vector.get("x", 0), vector.get("y", 0))


def delta(a, b):
    return {key: b.get(key, 0) - a.get(key, 0) for key in ("x", "y")}


def analyze(path):
    raw = path.read_bytes()
    rows, malformed = [], []
    for index, line in enumerate(raw.splitlines(), 1):
        if not line.strip():
            continue
        try:
            rows.append(json.loads(line))
        except (ValueError, UnicodeDecodeError):
            malformed.append(index)
    axes = {}
    for index, name in enumerate(AXES):
        values = [float(r.get("axes", [0] * 6)[index]) for r in rows]
        axes[name] = {"min": min(values, default=0), "max": max(values, default=0),
                      "negative_beyond_half": sum(v < -.5 for v in values),
                      "positive_beyond_half": sum(v > .5 for v in values)}
    button_stats = {name: {"pressed_samples": 0, "rising_edges": 0, "falling_edges": 0} for name in BUTTONS}
    transitions, physical_menu, movement, aim, pauses = [], [], [], [], []
    previous_mask = 0
    for index, row in enumerate(rows):
        mask = int(row.get("buttons", 0))
        for bit, name in enumerate(BUTTONS):
            stats = button_stats[name]
            stats["pressed_samples"] += bool(mask & (1 << bit))
            stats["rising_edges"] += bool(mask & (1 << bit) and not previous_mask & (1 << bit))
            stats["falling_edges"] += bool(previous_mask & (1 << bit) and not mask & (1 << bit))
        previous_mask = mask
        if index == 0:
            continue
        prev = rows[index - 1]
        changes = {key: [prev.get(key), row.get(key)] for key in ("phase", "menuPage", "menuFocus", "usingGamepad", "manualAim") if prev.get(key) != row.get(key)}
        if changes:
            record = {"seconds": row.get("seconds"), "changes": changes,
                      "lastInputDevice": row.get("lastInputDevice"), "xinput_owner": physical_owner(row)}
            transitions.append(record)
            if physical_owner(row) and ("menuPage" in changes or "menuFocus" in changes):
                physical_menu.append(record)
        values = row.get("axes", [0] * 6)
        if physical_owner(row) and prev.get("phase") == row.get("phase") == "Playing":
            displacement = length(delta(prev.get("playerPosition", {}), row.get("playerPosition", {})))
            if displacement > .01 and any(abs(values[i]) > .5 for i in (0, 1, 4, 5)):
                movement.append({"seconds": row.get("seconds"), "distance_between_logged_samples": displacement,
                                 "axes": values, "sample_gap_seconds": row.get("seconds", 0) - prev.get("seconds", 0)})
            if row.get("manualAim") and any(abs(values[i]) > .5 for i in (2, 3)) and length(delta(prev.get("aimDirection", {}), row.get("aimDirection", {}))) > .01:
                aim.append({"seconds": row.get("seconds"), "aim": row.get("aimDirection"), "raw_right_stick": values[2:4]})
        if physical_owner(row) and prev.get("phase") != row.get("phase") and "Paused" in (prev.get("phase"), row.get("phase")):
            pauses.append({"seconds": row.get("seconds"), "from": prev.get("phase"), "to": row.get("phase"),
                           "previous_mask": prev.get("buttons"), "current_mask": mask})
    significant = [r for r in rows if r.get("buttons", 0) or any(abs(v) > .5 for v in r.get("axes", []))]
    ownership = [r for r in rows if physical_owner(r)]
    return {
        "input": str(path.resolve()), "input_sha256_at_snapshot": hashlib.sha256(raw).hexdigest(), "bytes_read": len(raw),
        "samples": len(rows), "malformed_or_partial_line_numbers": malformed,
        "first_seconds": rows[0].get("seconds") if rows else None, "last_seconds": rows[-1].get("seconds") if rows else None,
        "devices": sorted({d for r in rows for d in r.get("devices", [])}),
        "provenance": "Expected observer-only R10 physical-controller session, as launched by the root agent. Device names alone cannot prove physical provenance; do not analyze the virtual navigation log as hardware evidence.",
        "virtual_fixture_report": "../navigation/navigation-check.json (synthetic Gamepad; separate evidence)",
        "status": "significant_controller_state_observed_requires_review" if significant else "no_significant_controller_actions_observed",
        "significant_controller_state_samples": len(significant), "xinput_owned_samples": len(ownership),
        "all_samples_isolated_profile": all(r.get("isolatedProfile") for r in rows) if rows else None,
        "axis_coverage": axes, "buttons": button_stats, "transitions": transitions,
        "xinput_correlated_menu_transitions": physical_menu,
        "xinput_correlated_playing_movement": movement,
        "xinput_correlated_manual_aim_changes": aim,
        "xinput_correlated_pause_transitions": pauses,
        "latest_state": rows[-1] if rows else None,
        "limitations": [
            "Detected device or tiny neutral-axis noise is not an action or hardware pass.",
            "Correlated state changes require native observation/user feedback; this analyzer cannot prove a highlighted target matches the user's intent.",
            "Continuous rows are sparse and omitted while unfocused. Missing evidence is unchecked, not a demonstrated failure.",
            "Cue request counters do not establish actual heard audio. Logger does not record offer ownership/lock details.",
            "No overall controller pass is assigned. Native virtual fixture results and physical observations stay separate."
        ]
    }


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--input", type=Path, default=Path(__file__).parent / "physical-controller/input.jsonl")
    parser.add_argument("--output", type=Path, default=Path(__file__).parent / "physical-controller/analysis.json")
    args = parser.parse_args()
    report = analyze(args.input)
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(report, indent=2), encoding="utf-8")
    print(json.dumps({key: report[key] for key in ("status", "samples", "devices", "significant_controller_state_samples", "xinput_owned_samples", "last_seconds")}, indent=2))

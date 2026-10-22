"""Compare exact dynamic snapshots, without screenshot or human-input claims."""
import argparse
import hashlib
import json
import math
from pathlib import Path

STATS = ("max_hp", "hp_regeneration", "lifesteal", "percent_damage", "melee_damage", "ranged_damage", "elemental_damage", "attack_speed", "crit_chance", "engineering", "range", "armor", "dodge", "speed", "luck", "harvesting")


def load(path):
    rows = [json.loads(line) for line in path.read_text(encoding="utf-8-sig").splitlines() if line.strip()]
    result = {row["id"]: row for row in rows}
    if len(rows) != len(result):
        raise ValueError("duplicate IDs in " + str(path))
    return result


def main():
    parser = argparse.ArgumentParser()
    for key in ("cases", "reference", "unity", "output"):
        parser.add_argument("--" + key, type=Path, required=True)
    args = parser.parse_args()
    cases, source, unity = (load(path) for path in (args.cases, args.reference, args.unity))
    if set(cases) != set(source) or set(cases) != set(unity):
        raise ValueError("different case sets")
    errors, mismatches = [], []
    snapshots = 0
    for id_, case in cases.items():
        a, b = source[id_], unity[id_]
        if a.get("error") or b.get("error"):
            errors.append({"id": id_, "source": a.get("error"), "unity": b.get("error")})
            continue
        left = a["stats"]
        right = b["stats"]
        expected = len(case["operations"])
        if len(left) != expected or len(right) != expected * len(STATS) or b["snapshots"] != expected:
            errors.append({"id": id_, "sourceSnapshots": len(left), "unityValues": len(right)})
            continue
        for index, values in enumerate(left):
            if len(values) != len(STATS):
                errors.append({"id": id_, "snapshot": index, "sourceValues": len(values)})
                continue
            snapshots += 1
            for key, x, y in zip(STATS, values, right[index * 16:(index + 1) * 16]):
                if not math.isclose(x, y, abs_tol=1e-4, rel_tol=1e-6):
                    mismatches.append({"id": id_, "snapshot": index, "stat": key, "source": x, "unity": y})
    report = {"caseCount": len(cases), "snapshots": snapshots, "errorCount": len(errors), "mismatchCount": len(mismatches),
              "errors": errors, "mismatches": mismatches,
              "scope": "Original Player timer/movement/pre-healing consumable and Main half-health/Harvesting callbacks vs RunRules; six family item IDs plus Hoodie link context, five characters, deterministic state sequences",
              "limits": "No real-time scheduling, scene lifecycle, live contact, audio, UI, arbitrary proc combinations or hardware verification",
              "inputs": {key: {"path": str(getattr(args, key).resolve()), "sha256": hashlib.sha256(getattr(args, key).read_bytes()).hexdigest()} for key in ("cases", "reference", "unity")}}
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    print(f"{len(cases)} cases, {snapshots} snapshots, {len(errors)} errors, {len(mismatches)} differences")
    raise SystemExit(1 if errors or mismatches else 0)


if __name__ == "__main__":
    main()

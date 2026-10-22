"""Compare static stat vectors exported by the original and Scrapshift.

This detects discrepancies; it does not classify dynamic, timed or random
effects and does not assert that all item behavior is covered.
"""
import argparse
import collections
import hashlib
import json
import math
from pathlib import Path

STATS = ("max_hp", "hp_regeneration", "lifesteal", "percent_damage", "melee_damage", "ranged_damage", "elemental_damage", "attack_speed", "crit_chance", "engineering", "range", "armor", "dodge", "speed", "luck", "harvesting")


def load(path):
    rows = {}
    with path.open(encoding="utf-8") as stream:
        for number, line in enumerate(stream, 1):
            if not line.strip():
                continue
            row = json.loads(line)
            id_ = row["id"]
            if id_ in rows:
                raise ValueError(f"duplicate case {id_} in {path}:{number}")
            rows[id_] = row
    return rows


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--cases", type=Path, required=True)
    parser.add_argument("--reference", type=Path, required=True)
    parser.add_argument("--unity", type=Path, required=True)
    parser.add_argument("--output", type=Path, required=True)
    parser.add_argument("--scope", default="static 16-stat intermission vector, Well-Rounded with tier-I pistol, full health, after zero, one or two acquired imported items; no live Player scene applies movement TempStats")
    args = parser.parse_args()
    cases, reference, unity = (load(path) for path in (args.cases, args.reference, args.unity))
    if set(cases) != set(reference) or set(cases) != set(unity):
        raise ValueError(f"case sets differ: input={len(cases)}, reference={len(reference)}, unity={len(unity)}; missing reference={sorted(set(cases)-set(reference))[:10]}, missing unity={sorted(set(cases)-set(unity))[:10]}")
    mismatches, errors = [], []
    by_stat = collections.Counter()
    for id_ in cases:
        original, port = reference[id_], unity[id_]
        if original.get("error") or port.get("error"):
            errors.append({"id": id_, "reference": original.get("error"), "unity": port.get("error")})
            continue
        left, right = original["stats"], port["stats"]
        if len(left) != len(STATS) or len(right) != len(STATS):
            errors.append({"id": id_, "reference_length": len(left), "unity_length": len(right)})
            continue
        differences = {}
        for key, a, b in zip(STATS, left, right):
            if not math.isclose(float(a), float(b), rel_tol=1e-6, abs_tol=1e-4):
                differences[key] = {"reference": a, "unity": b, "delta": round(b-a, 6)}
                by_stat[key] += 1
        if differences:
            mismatches.append({"id": id_, "differences": differences})
    report = {
        "scope": args.scope,
        "limits": "No timed, random/proc, enemy, combat, pickup, shop, transition, or acquisition-order parity claim; all mismatches require source triage.",
        "caseCount": len(cases), "compared": len(cases)-len(errors),
        "errorCount": len(errors), "mismatchCount": len(mismatches),
        "mismatchByStat": dict(by_stat), "errors": errors[:100],
        "mismatches": mismatches[:100],
        "truncatedErrors": max(0, len(errors)-100),
        "truncatedMismatches": max(0, len(mismatches)-100),
        "inputs": {name: {"path": str(path.resolve()), "sha256": sha(path)} for name, path in (("cases", args.cases), ("reference", args.reference), ("unity", args.unity))},
    }
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    print(f"{len(cases)} cases, {len(errors)} export errors, {len(mismatches)} static-vector mismatches -> {args.output}")
    if errors or mismatches:
        raise SystemExit(1)


if __name__ == "__main__":
    main()

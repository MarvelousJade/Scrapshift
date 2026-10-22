"""Read saved campaign evidence and write a bounded cross-engine comparison.

No game, profile, source asset or trace is modified. A zero exit status means
the saved integration checks passed, never that gameplay is exactly equivalent.
"""
import argparse
from collections import Counter
import hashlib
import json
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
NATIVE = ROOT / "artifacts/gameplay-parity-geometry-2026-10-15/realtime"
REFERENCE = ROOT / "artifacts/gameplay-parity-reference-2026-10-15/realtime"


def read(path):
    return json.loads(path.read_text(encoding="utf-8-sig"))


def digest(path):
    return {"path": str(path.resolve()), "sha256": hashlib.sha256(path.read_bytes()).hexdigest()}


def stack_map(row):
    return {entry["id"]: entry["count"] for entry in row.get("itemStacks", [])}


def normalized(row, original=False):
    if original:
        return {"wave": row.get("wave"), "materials": row.get("gold"), "level": row.get("level"),
                "itemCount": row.get("item_count"), "bankedMaterials": row.get("banked_materials"),
                "enemyKills": row.get("enemy_kills"), "treeKills": row.get("tree_kills"),
                "materialPickups": row.get("material_pickups"), "harvesting": row.get("harvesting"),
                "luck": row.get("luck"), "materialObjects": row.get("ground_material_objects"),
                "groundMaterialValue": row.get("ground_material_value"), "weapons": row.get("weapons")}
    result = {key: row.get(key) for key in ("wave", "materials", "level", "itemCount", "bankedMaterials",
              "enemyKills", "materialPickups", "harvesting", "luck", "materialObjects", "groundMaterialValue", "weapons")}
    result["treeKills"] = row.get("kills", 0) - row.get("enemyKills", 0)
    return result


def purchase_checks(events, original=False):
    checks = []
    for index, row in enumerate(events):
        if row["event"] != "purchase":
            continue
        if original:
            following = next((e for e in events[index + 1:] if e["event"] in ("purchase", "shop_exit")
                              and e["wave"] == row["wave"]), None)
            before, after = row["gold"], following["gold"] if following else None
        else:
            before, after = row["materialsBefore"], row["materials"]
        checks.append({"wave": row["wave"], "id": row.get("id", row.get("detail")),
                       "before": before, "after": after, "price": row["price"],
                       "passed": after is not None and before - after == row["price"]})
    return checks


def first_acquisitions(events, original=False):
    found = {}
    for row in events:
        if original:
            if row["event"] not in ("purchase", "crate_take") or not row.get("id", "").startswith("item_"):
                continue
            ids = [row["id"][5:]]
        else:
            ids = stack_map(row)
        for item in ids:
            found.setdefault(item, {"wave": row["wave"], "event": row["event"]})
    return found


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--unity-dir", type=Path, default=NATIVE)
    parser.add_argument("--reference-dir", type=Path, default=REFERENCE)
    parser.add_argument("--build-manifest", type=Path)
    parser.add_argument("--json-out", type=Path)
    parser.add_argument("--report-out", type=Path, default=ROOT / "docs/full-run-r8-comparison-2026-10-15.md")
    parser.add_argument("--label", default="R8")
    args = parser.parse_args()
    files = {"referenceTrace": args.reference_dir / "trace.json", "referenceSummary": args.reference_dir / "summary.json",
             "unityTrace": args.unity_dir / "campaign-trace.json", "unitySummary": args.unity_dir / "campaign-check.json",
             "buildManifest": args.build_manifest or args.unity_dir / "build-manifest.json"}
    missing = [str(path) for path in files.values() if not path.is_file()]
    if missing:
        parser.exit(2, "Comparison not run: missing evidence:\n" + "\n".join(missing) + "\n")
    ref, summary, unity_doc, check, manifest = (read(files[k]) for k in files)
    unity = unity_doc["events"]
    ref_counts, unity_counts = Counter(r["event"] for r in ref), Counter(r["event"] for r in unity)
    ref_end = next(r for r in reversed(ref) if r["event"] == "finished")
    unity_end = next(r for r in reversed(unity) if r["event"] == "finished")
    ref_final, unity_final = normalized(ref_end, True), normalized(unity_end)
    ref_stacks = {key.removeprefix("item_"): value for key, value in summary["final_source_debug_log_item_stacks"].items()}
    unity_stacks = stack_map(unity_end)
    ref_buys, unity_buys = purchase_checks(ref, True), purchase_checks(unity)
    stack_failures = [{"event": r["event"], "wave": r["wave"], "sum": sum(stack_map(r).values()), "itemCount": r["itemCount"]}
                      for r in unity if sum(stack_map(r).values()) != r["itemCount"]]
    ref_waves = {r["wave"]: normalized(r, True) for r in ref if r["event"] == "wave_timer_end"}
    unity_waves = {r["wave"]: normalized(r) for r in unity if r["event"] == "wave_timer_end"}
    ref_first, unity_first = first_acquisitions(ref, True), first_acquisitions(unity)
    checks = {
        "originalWon": ref_end.get("run_won") is True and ref_end.get("status") == "won",
        "originalExitZero": summary.get("process_exit_code") == 0,
        "unityWon": unity_end.get("phase") == "Won" and check.get("status") == "Passed" and check.get("phase") == "Won",
        "originalTwentyTimersNineteenShops": ref_counts["wave_timer_end"] == 20 and summary["shop_entries"] == 19,
        "unityTwentyTimersNineteenShops": unity_counts["wave_timer_end"] == 20 and unity_counts["shop_enter"] == 19,
        "timerWavesOneToTwenty": sorted(ref_waves) == sorted(unity_waves) == list(range(1, 21)),
        "originalPurchaseDebits": all(r["passed"] for r in ref_buys),
        "unityPurchaseDebits": all(r["passed"] for r in unity_buys),
        "originalFinalStackSum": sum(ref_stacks.values()) == ref_final["itemCount"],
        "unityEverySnapshotStackSum": not stack_failures,
        "originalGroundCap": max(r.get("ground_material_objects", 0) for r in ref) <= 50,
        "unityGroundCap": max(r.get("materialObjects", 0) for r in unity) <= 50,
        "referenceOrdinaryTiming": all(r["fixture_time_scale"] == 1 and r["fixture_physics_hz"] == 60 for r in ref),
        "unityOrdinaryTiming": check.get("realtime") is True and check.get("pickupPhysicsRate") == 60,
    }
    output = {"purpose": "Saved normal-time integration comparison; not complete gameplay parity",
              "label": args.label, "allIntegrationChecksPassed": all(checks.values()), "checks": checks,
              "inputs": {key: digest(path) for key, path in files.items()}, "buildManifest": manifest,
              "eventCounts": {"original": dict(ref_counts), "unity": dict(unity_counts)},
              "final": {"original": ref_final, "unity": unity_final},
              "purchaseDebitChecks": {"original": ref_buys, "unity": unity_buys},
              "finalItemStacks": {"original": ref_stacks, "unity": unity_stacks},
              "unityStackFailures": stack_failures,
              "firstAcquisitions": {"original": ref_first, "unity": unity_first},
              "timerEnds": [{"wave": w, "original": ref_waves.get(w), "unity": unity_waves.get(w)} for w in range(1, 21)]}
    json_out = args.json_out or args.unity_dir / "comparison.json"
    if json_out.resolve() in {p.resolve() for p in files.values()} or args.report_out.resolve() in {p.resolve() for p in files.values()}:
        parser.error("Output paths must not overwrite input evidence.")
    rows = [f"# {args.label} full-run comparison — October 15, 2026", "",
            "Generated from saved traces by `scripts/compare_r8_campaign.py`. Original reference evidence is reused unchanged; this is a fresh Scrapshift campaign against that frozen original run.", "",
            f"Saved integration checks: **{'passed' if all(checks.values()) else 'FAILED'}** ({sum(checks.values())}/{len(checks)}). A passed integration comparison does not establish exact gameplay parity.", "",
            "## Evidence and build", "",
            f"Machine-readable result: `{json_out.relative_to(ROOT) if json_out.is_relative_to(ROOT) else json_out}`.", "",
            "| Input | SHA-256 |", "|---|---|"]
    rows += [f"| `{key}`: `{entry['path']}` | `{entry['sha256']}` |" for key, entry in output["inputs"].items()]
    rows += ["", "Frozen build manifest:", "", "```json", json.dumps(manifest, indent=2), "```", "",
             "## Observed completion and progression", "", "| Observation | Original | Scrapshift |", "|---|---:|---:|"]
    stats = [("Wave timer ends", ref_counts["wave_timer_end"], unity_counts["wave_timer_end"]),
             ("Shops", summary["shop_entries"], unity_counts["shop_enter"]),
             ("Purchases", len(ref_buys), len(unity_buys)), ("Upgrades", ref_counts["upgrade"], unity_counts["upgrade"]),
             ("Crates taken", ref_counts["crate_take"], unity_counts["crate_take"])]
    stats += [(key, ref_final[key], unity_final[key]) for key in ("materials", "bankedMaterials", "level", "itemCount", "enemyKills", "treeKills", "materialPickups", "harvesting", "luck")]
    rows += [f"| {name} | {a} | {b} |" for name, a, b in stats]
    rows += ["", f"Purchase debit consistency: **{sum(r['passed'] for r in ref_buys)}/{len(ref_buys)} original**, **{sum(r['passed'] for r in unity_buys)}/{len(unity_buys)} Scrapshift**. Original purchase snapshots precede the callback; the next purchase/shop-exit snapshot supplies the after balance. Unity records explicit before/after values. These checks validate each run's own debits, not equality of differently modified prices.", "",
             f"Final stack sums: original {sum(ref_stacks.values())}/{ref_final['itemCount']}, Scrapshift {sum(unity_stacks.values())}/{unity_final['itemCount']}. Unity snapshot stack inconsistencies: {len(stack_failures)}. Recorded multiplicities do not establish every stacked effect formula.", "",
             "## Aligned timer-end snapshots", "", "Each cell is original / Scrapshift. Sampling occurs around timer end, before the complete intermission; callback boundaries differ. Pickups count collected objects, not currency value.", "",
             "| Wave | Enemy kills | Pickup objects | Materials | Bank | Harvesting | Luck |", "|---|---|---|---|---|---|---|"]
    for wave in range(1, 21):
        a, b = ref_waves.get(wave, {}), unity_waves.get(wave, {})
        values = [f"{a.get(k, 'missing')} / {b.get(k, 'missing')}" for k in ("enemyKills", "materialPickups", "materials", "bankedMaterials", "harvesting", "luck")]
        rows.append("| " + " | ".join([str(wave)] + values) + " |")
    rows += ["", "## Acquisition differences to investigate", "",
             "First-seen acquisition waves identify recorded inventory differences. They do not quantify an item's causal contribution. Original acquisition callbacks record IDs before applying them; Unity stacks record resulting ownership.", "",
             "| Item | Original first seen | Scrapshift first seen |", "|---|---|---|"]
    differing = [item for item in sorted(set(ref_first) | set(unity_first)) if ref_first.get(item) != unity_first.get(item)]
    for item in differing:
        describe = lambda x: f"wave {x['wave']} ({x['event']})" if x else "not acquired"
        rows.append(f"| {item} | {describe(ref_first.get(item))} | {describe(unity_first.get(item))} |")
    rows += ["", "## Scope and remaining interpretation", "",
             "Both fixtures use neutral character effects, Danger 0, six supplied tier-II weapons, invulnerability, scripted circular movement, first offered upgrades, crate takes and affordable actual shop callbacks. They are muted, run at normal timing and use isolated profiles. They are not ordinary human survival runs.", "",
             "The original run is frozen evidence, not a newly rerun original. Cross-engine random draw ordering, offers, upgrades, trajectories and resulting gear differ. Outcome differences are observations to investigate; this report neither attributes every gap to randomness nor declares a particular formula wrong from aggregate totals alone. Per-wave telemetry and first acquisition records support narrower follow-up hypotheses.", "",
             "Source-derived unit checks and the new suite establish their stated boundaries separately. This campaign does not verify audio perception, physical controller behavior, every item interaction or complete reference parity. Wall time is not a performance benchmark. Screenshots require separate visual inspection; file existence does not prove their content.", ""]
    json_out.parent.mkdir(parents=True, exist_ok=True)
    args.report_out.parent.mkdir(parents=True, exist_ok=True)
    json_out.write_text(json.dumps(output, indent=2) + "\n", encoding="utf-8")
    args.report_out.write_text("\n".join(rows), encoding="utf-8")
    print(json.dumps({"integrationChecksPassed": all(checks.values()), "checks": checks,
                      "json": str(json_out), "report": str(args.report_out)}, indent=2))
    return 0 if all(checks.values()) else 1


if __name__ == "__main__":
    raise SystemExit(main())

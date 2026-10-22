"""Compare a Scrapshift full campaign with a saved supplied-reference run.

Exit 0 means bounded trace invariants passed, not exact gameplay parity. Inputs
are read only; outputs are written only to the selected JSON/report paths.
"""
import argparse
from collections import Counter
import hashlib
import json
import math
from pathlib import Path

from compare_r8_campaign import normalized, purchase_checks, stack_map, first_acquisitions


ROOT = Path(__file__).resolve().parents[1]
REFERENCE = ROOT / "artifacts/gameplay-parity-reference-2026-10-15/realtime"
ITEMS = ROOT / "Assets/Scrapshift/Content/Reference/item-design.json"
DURATIONS = [20, 25, 30, 35, 40, 45, 50, 55, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 90]


def read(path):
    return json.loads(path.read_text(encoding="utf-8-sig"))


def proof(path):
    return {"path": str(path.resolve()), "sha256": hashlib.sha256(path.read_bytes()).hexdigest()}


def numbered(events, kind):
    return [row["wave"] for row in events if row["event"] == kind]


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--unity-dir", required=True, type=Path)
    parser.add_argument("--reference-dir", type=Path, default=REFERENCE)
    parser.add_argument("--reference-source-manifest", type=Path)
    parser.add_argument("--build-manifest", required=True, type=Path)
    parser.add_argument("--source-manifest", required=True, type=Path)
    parser.add_argument("--item-catalog", type=Path, default=ITEMS)
    parser.add_argument("--json-out", type=Path)
    parser.add_argument("--report-out", type=Path, default=ROOT / "docs/full-run-economy-review-2026-10-17.md")
    parser.add_argument("--label", default="R14")
    args = parser.parse_args()
    files = {
        "referenceTrace": args.reference_dir / "trace.json",
        "referenceSummary": args.reference_dir / "summary.json",
        "referenceSourceManifest": args.reference_source_manifest or args.reference_dir / "source-manifest.json",
        "unityTrace": args.unity_dir / "campaign-trace.json",
        "unitySummary": args.unity_dir / "campaign-check.json",
        "unityProcess": args.unity_dir / "process.json",
        "buildManifest": args.build_manifest,
        "sourceManifest": args.source_manifest,
        "itemCatalog": args.item_catalog,
    }
    missing = [str(path) for path in files.values() if not path.is_file()]
    if missing:
        parser.exit(2, "Comparison not run; missing evidence:\n" + "\n".join(missing) + "\n")
    json_out = args.json_out or args.unity_dir / "economy-comparison.json"
    protected = {path.resolve() for path in files.values()}
    if json_out.resolve() in protected or args.report_out.resolve() in protected or json_out.resolve() == args.report_out.resolve():
        parser.error("Output paths must be distinct from one another and from input evidence.")

    ref, ref_summary = read(files["referenceTrace"]), read(files["referenceSummary"])
    reference_source_manifest = read(files["referenceSourceManifest"])
    unity_summary, unity_doc = read(files["unitySummary"]), read(files["unityTrace"])
    unity_process = read(files["unityProcess"])
    unity = unity_doc["events"]
    source_manifest, build_manifest = read(files["sourceManifest"]), read(files["buildManifest"])
    items = {row["id"]: row for row in read(files["itemCatalog"])["items"]}
    ref_counts, unity_counts = Counter(row["event"] for row in ref), Counter(row["event"] for row in unity)
    ref_end = next((row for row in reversed(ref) if row["event"] == "finished"), {})
    unity_end = next((row for row in reversed(unity) if row["event"] == "finished"), {})
    ref_buys, unity_buys = purchase_checks(ref, True), purchase_checks(unity)
    ref_timers = [row for row in ref if row["event"] == "wave_timer_end"]
    unity_timers = [row for row in unity if row["event"] == "wave_timer_end"]
    ref_intervals = {row["wave"]: row.get("sampled_simulated_interval") for row in ref_summary.get("waves", [])}
    unity_intervals = {row["wave"]: row.get("waveSimulatedSeconds") for row in unity_timers}
    ref_duration_failures = [{"wave": wave, "observed": seconds, "expected": DURATIONS[wave - 1]}
                             for wave, seconds in sorted(ref_intervals.items()) if wave not in range(1, 21)
                             or seconds is None or abs(seconds - DURATIONS[wave - 1]) > 0.5]
    unity_duration_failures = [{"wave": wave, "observed": seconds, "expected": DURATIONS[wave - 1]}
                               for wave, seconds in sorted(unity_intervals.items()) if wave not in range(1, 21)
                               or seconds is None or abs(seconds - DURATIONS[wave - 1]) > 0.5]
    stack_failures = [{"event": row["event"], "wave": row["wave"], "sum": sum(stack_map(row).values()),
                       "itemCount": row["itemCount"]} for row in unity
                      if sum(stack_map(row).values()) != row["itemCount"]]
    ref_stacks = {key.removeprefix("item_"): value for key, value in
                  ref_summary["final_source_debug_log_item_stacks"].items()}
    stack_cap_failures = [{"event": row["event"], "wave": row["wave"], "id": item,
                           "count": count, "maxCount": items[item]["maxCount"]}
                          for row in unity for item, count in stack_map(row).items()
                          if item in items and items[item].get("maxCount", -1) > 0
                          and count > items[item]["maxCount"]]
    ref_stack_cap_failures = [{"id": item, "count": count, "maxCount": items[item]["maxCount"]}
                              for item, count in ref_stacks.items()
                              if item in items and items[item].get("maxCount", -1) > 0
                              and count > items[item]["maxCount"]]
    acquisition_rows = [row for row in unity if row["event"] in ("purchase", "crate_take") and row.get("detail") in items]
    locked_acquisitions = [{"wave": row["wave"], "event": row["event"], "id": row["detail"]}
                           for row in acquisition_rows if not items[row["detail"]]["available"]]
    unknown_items = [{"wave": row["wave"], "event": row["event"], "id": row.get("detail")}
                     for row in unity if row["event"] in ("purchase", "crate_take")
                     and row.get("detail") not in items and row.get("detail") not in
                     {weapon["id"] for weapon in row.get("weapons", [])}]
    # An ordinary fresh-profile fixture must not gain a catalog-locked item even
    # if its challenge completes during this same run. Weapons are checked by
    # the item's absence from the catalog and the trace's held-weapon IDs.
    final_timer = next((row for row in reversed(unity_timers) if row["wave"] == 20), {})
    receipt_present = unity_end.get("settlementWave") == 20 and all(key in unity_end for key in
                          ("materialsBeforeSettlement", "materialsAfterSettlement", "harvestingAtSettlement"))
    observed_harvesting = unity_end["harvestingAtSettlement"] if receipt_present else final_timer.get("harvesting", 0)
    minimum_final_harvest = math.floor(max(0, observed_harvesting))
    final_material_credit = (unity_end["materialsAfterSettlement"] - unity_end["materialsBeforeSettlement"]
                             if receipt_present else unity_end.get("materials", 0) - final_timer.get("materials", 0))
    final_harvest_observation = {"harvestingAtCredit": observed_harvesting,
                                 "minimumBaseHarvestCredit": minimum_final_harvest,
                                 "observedMaterialCredit": final_material_credit,
                                 "measurement": "explicit settlement receipt" if receipt_present else "legacy timer-to-win delta",
                                 "scope": "Neutral fixture positive base harvesting credit before cleanup. New traces record the exact credit boundary; legacy R19 retains its timer-to-win evidence. Lower bound, not an exact all-effect income formula."}
    checks = {
        "referenceWonAndExitedZero": ref_end.get("run_won") is True and ref_summary.get("process_exit_code") == 0,
        "unityFinalPositiveHarvestCredited": final_material_credit >= minimum_final_harvest,
        "unityProcessExitedZero": unity_process.get("status") in (None, "exited") and unity_process.get("exitCode") == 0,
        "unityWon": unity_end.get("phase") == "Won" and unity_summary.get("status") == "Passed"
                    and unity_summary.get("phase") == "Won" and not unity_summary.get("preflight", False),
        "referenceTwentyTimersNineteenShops": numbered(ref, "wave_timer_end") == list(range(1, 21))
                    and ref_summary.get("shop_entries") == 19,
        "unityTwentyTimersNineteenShops": numbered(unity, "wave_timer_end") == list(range(1, 21))
                    and numbered(unity, "shop_enter") == list(range(1, 20))
                    and unity_summary.get("completedWaves") == 20 and unity_summary.get("shops") == 19,
        "referenceFixed60HzDurations": not ref_duration_failures and sorted(ref_intervals) == list(range(1, 21))
                    and all(row.get("fixture_time_scale") == 1 and row.get("fixture_physics_hz") == 60 for row in ref),
        "unityFixed60HzDurations": not unity_duration_failures and sorted(unity_intervals) == list(range(1, 21))
                    and unity_summary.get("realtime") is True and unity_summary.get("pickupPhysicsRate") == 60,
        "referencePurchaseDebits": bool(ref_buys) and all(row["passed"] for row in ref_buys),
        "unityPurchaseDebits": bool(unity_buys) and all(row["passed"] for row in unity_buys),
        "referenceFinalStackSum": sum(ref_stacks.values()) == ref_end.get("item_count"),
        "unityEverySnapshotStackSum": not stack_failures,
        "unityEverySnapshotStackCaps": not stack_cap_failures,
        "referenceFinalStackCaps": not ref_stack_cap_failures,
        "unityFreshRunUnlockEligibility": not locked_acquisitions and not unknown_items,
        "sourceCopyMatches": isinstance(source_manifest, list) and bool(source_manifest)
                    and all(row.get("matches") is True for row in source_manifest),
        "referenceManifestPresent": isinstance(reference_source_manifest, dict) and bool(reference_source_manifest),
        "buildManifestPresent": isinstance(build_manifest, (list, dict)) and bool(build_manifest),
    }
    ref_final, unity_final = normalized(ref_end, True), normalized(unity_end)
    reference_wave_totals = {row["wave"]: normalized(row, True) for row in ref_timers}
    unity_wave_totals = {row["wave"]: normalized(row) for row in unity_timers}
    wave_totals = [{"wave": wave, "reference": reference_wave_totals.get(wave),
                    "unity": unity_wave_totals.get(wave)} for wave in range(1, 21)]
    unity_starts = [row for row in unity if row["event"] == "wave_start"]
    unity_wall_intervals = [{"wave": current["wave"],
                             "toNextStartWallSeconds": following["wallSeconds"] - current["wallSeconds"],
                             "activeSeconds": unity_intervals.get(current["wave"])}
                            for current, following in zip(unity_starts, unity_starts[1:])]
    starts_by_wave = {row["wave"]: row for row in unity_starts}
    unity_timing_breakdown = []
    for timer in unity_timers:
        start = starts_by_wave.get(timer["wave"])
        following = starts_by_wave.get(timer["wave"] + 1)
        if start is None:
            continue
        combat_wall = timer["wallSeconds"] - start["wallSeconds"]
        next_boundary = following if following is not None else unity_end
        unity_timing_breakdown.append({"wave": timer["wave"], "activeSeconds": timer.get("waveSimulatedSeconds"),
            "combatWallSeconds": combat_wall,
            "combatWallExcess": combat_wall - timer.get("waveSimulatedSeconds", 0),
            "cleanupAndChoicesWallSeconds": next_boundary.get("wallSeconds", timer["wallSeconds"]) - timer["wallSeconds"],
            "nextBoundary": "next wave start" if following is not None else "finished"})
    first_ref, first_unity = first_acquisitions(ref, True), first_acquisitions(unity)
    differences = {item: {"reference": first_ref.get(item), "unity": first_unity.get(item)}
                   for item in sorted(first_ref.keys() | first_unity.keys())
                   if first_ref.get(item) != first_unity.get(item)}
    output = {
        "scope": "Bounded saved-trace integration comparison; no random-total equality or exact parity claim",
        "label": args.label, "allChecksPassed": all(checks.values()), "checks": checks,
        "inputs": {name: proof(path) for name, path in files.items()},
        "eventCounts": {"reference": dict(ref_counts), "unity": dict(unity_counts)},
        "final": {"reference": ref_final, "unity": unity_final},
        "purchaseDebits": {"reference": ref_buys, "unity": unity_buys},
        "waveTotals": wave_totals,
        "finalHarvestObservation": final_harvest_observation,
        "unityWallIntervals": unity_wall_intervals,
        "unityTimingBreakdown": unity_timing_breakdown,
        "durationFailures": {"reference": ref_duration_failures, "unity": unity_duration_failures},
        "stackFailures": stack_failures,
        "stackCapFailures": {"referenceFinal": ref_stack_cap_failures, "unitySnapshots": stack_cap_failures},
        "lockedAcquisitions": locked_acquisitions,
        "unknownAcquisitions": unknown_items, "firstAcquisitionDifferences": differences,
    }
    rows = [f"# {args.label} full-run economy comparison - October 17, 2026", "",
            "Generated from saved traces by `scripts/compare_r14_campaign.py`. The supplied-reference normal-time run at the input path and the Scrapshift run are separate scripted fixtures. The script records their exact file hashes below.", "",
            f"Bounded checks: **{sum(checks.values())}/{len(checks)} passed**. This result does not establish exact gameplay parity.", "",
            "## Evidence", "", f"JSON: `{json_out}`.", "",
            "| Input | SHA-256 |", "| --- | --- |"]
    rows.extend(f"| `{name}`: `{entry['path']}` | `{entry['sha256']}` |" for name, entry in output["inputs"].items())
    rows += ["", "## Bounded checks", "", "| Check | Result |", "| --- | --- |"]
    rows.extend(f"| {name} | {'pass' if passed else 'FAIL'} |" for name, passed in checks.items())
    rows += ["", f"Purchases: reference {len(ref_buys)} ({sum(row['passed'] for row in ref_buys)} debit checks); Scrapshift {len(unity_buys)} ({sum(row['passed'] for row in unity_buys)} debit checks). Different prices and purchase counts are not required to equal across random runs.",
             f"Stacks: reference final {sum(ref_stacks.values())}/{ref_end.get('item_count')}; Scrapshift final {sum(stack_map(unity_end).values())}/{unity_end.get('itemCount')}. Scrapshift inconsistent snapshots: {len(stack_failures)}.",
             f"Positive catalog stack-cap violations: reference final {len(ref_stack_cap_failures)}; Scrapshift snapshots {len(stack_cap_failures)}. These neutral fixtures have no character-specific cap modifiers.",
             f"Initial-catalog locked acquisitions in the fresh Scrapshift run: {len(locked_acquisitions)}; unknown acquisitions: {len(unknown_items)}.", "",
             "## Observed run totals", "", "| Metric | Reference | Scrapshift |", "| --- | ---: | ---: |"]
    for key in ("materials", "bankedMaterials", "level", "itemCount", "enemyKills", "treeKills", "materialPickups", "harvesting", "luck"):
        rows.append(f"| {key} | {ref_final.get(key)} | {unity_final.get(key)} |")
    rows += ["", "## Wave-end divergence timeline", "",
             "Snapshots use each fixture's wave-timer callback, not an identical settlement phase: the original fixture connects after Main's existing timer handler, whereas Unity records before Rules.Tick settlement. Gold/stat comparisons at this boundary may therefore include original harvesting already paid. Equal base loadouts do not reproduce cross-engine spawn placement, target order or physics. First-wave kill/drop differences still precede shop purchases.", "",
             "| Wave | Enemy kills ref / Unity | Pickups ref / Unity | Materials ref / Unity | Items ref / Unity |", "| --- | --- | --- | --- | --- |"]
    for pair in wave_totals:
        r, u = pair["reference"] or {}, pair["unity"] or {}
        values = [f"{r.get(key)} / {u.get(key)}" for key in ("enemyKills", "materialPickups", "materials", "itemCount")]
        rows.append(f"| {pair['wave']} | " + " | ".join(values) + " |")
    rows += ["", "## Final-wave settlement", "",
             f"Recorded final-wave harvesting at credit: {observed_harvesting}; minimum base credit {minimum_final_harvest}; observed credit {final_material_credit}, measured by {final_harvest_observation['measurement']}. New traces carry a settlement receipt because timer snapshots now follow credit. Legacy R19 retains its failing timer-to-win measurement. This is a neutral-fixture lower bound, not an exact all-effect payout formula."]
    rows += ["", "## Wall-clock timing limitation", "",
             "The duration checks above measure active simulated combat time. They do not establish wall-clock or performance parity. Start-to-start wall intervals also include transitions and any elapsed time the driver did not consume. The driver uses `Time.deltaTime`, which is subject to Unity's maximum-delta setting. There is no frame-profiler or complete window-intervention trace to attribute longer intervals. Earlier R19 captures had differing resolution; that historical observation does not establish the cause of longer intervals in this campaign.", "",
             "| Wave | Wall seconds until next wave start | Active combat seconds |", "| --- | ---: | ---: |"]
    rows.extend(f"| {row['wave']} | {row['toNextStartWallSeconds']:.3f} | {row['activeSeconds']} |" for row in unity_wall_intervals)
    rows += ["", "### Combat versus cleanup wall intervals", "",
             "Measured directly from wave_start, wave_timer_end and the next wave_start (or final finished). Positive combat excess is observed wall time beyond active simulation; its cause is not established. Cleanup/choice time is measured separately, so shop outcomes are not used to explain combat-window stalls.", "",
             "| Wave | Active seconds | Combat wall seconds | Combat wall excess | Cleanup/choices wall seconds |", "| --- | ---: | ---: | ---: | ---: |"]
    rows.extend(f"| {row['wave']} | {row['activeSeconds']:.3f} | {row['combatWallSeconds']:.3f} | {row['combatWallExcess']:.3f} | {row['cleanupAndChoicesWallSeconds']:.3f} |" for row in unity_timing_breakdown)
    first_wave = wave_totals[0]
    first_ref_wave, first_unity_wave = first_wave["reference"] or {}, first_wave["unity"] or {}
    if (first_ref_wave.get("enemyKills"), first_ref_wave.get("treeKills"), first_ref_wave.get("materialPickups"), first_ref_wave.get("groundMaterialValue"),
        first_unity_wave.get("enemyKills"), first_unity_wave.get("treeKills"), first_unity_wave.get("materialPickups"), first_unity_wave.get("groundMaterialValue")) == (28, 0, 8, 20, 27, 1, 5, 25):
        rows += ["", "### First-wave material accounting", "",
                 "The first wave diverged before either run acquired an item or upgrade. The original killed 28 enemies and no trees: 8 material pickups + 20 remaining ground value = 28. Scrapshift killed 27 enemies and one tree: 5 pickups + 25 ground value = 30. The supplied `entities/units/neutral/tree_stats.tres` sets tree value to 3; `RunRules.MaterialDropAmount` preserves base value with zero drop modifiers. Thus 27 + 3 = 30 reconciles this observed difference. This rules out an obvious missing-material accounting error in these snapshots; it does not establish matching spawn placement, targeting or pickup trajectories."]
    rows += ["", "## First acquisition differences", "",
             "Different source and Unity random sequences, offers, movement and upgrades prevent a causal conclusion from an aggregate difference. This list identifies follow-up items, not automatically bugs.", "",
             "| Item | Reference first seen | Scrapshift first seen |", "| --- | --- | --- |"]
    for item, pair in differences.items():
        describe = lambda row: f"wave {row['wave']} ({row['event']})" if row else "not acquired"
        rows.append(f"| {item} | {describe(pair['reference'])} | {describe(pair['unity'])} |")
    rows += ["", "## Interpretation", "",
             f"Both runs use a neutral scripted six-weapon loadout, Danger 0, invulnerability, mute, scripted movement and fresh profiles at normal time. The supplied-reference trace at `{files['referenceTrace']}` records {ref_end.get('fixture_pause_interventions', 0)} fixture pause intervention(s). Its source manifest is an explicit hashed input. The manifest hashes bind the saved evidence, not an independent visual audit.", "",
             "The unlock-eligibility check applies only to real item IDs acquired by purchase or crate in this fresh run. It checks the imported catalog's run-start availability. It does not sample every shop/crate candidate, establish every unlock trigger, or validate subsequent character-specific runs. Item stack sums do not prove each effect formula. Screenshots and physical controls need separate review. No full-run totals are treated as equality requirements.", ""]
    json_out.parent.mkdir(parents=True, exist_ok=True)
    args.report_out.parent.mkdir(parents=True, exist_ok=True)
    json_out.write_text(json.dumps(output, indent=2) + "\n", encoding="utf-8")
    args.report_out.write_text("\n".join(rows), encoding="utf-8")
    print(json.dumps({"allChecksPassed": all(checks.values()), "checks": checks,
                      "json": str(json_out), "report": str(args.report_out)}, indent=2))
    return 0 if all(checks.values()) else 1


if __name__ == "__main__":
    raise SystemExit(main())

"""Compare combat-window wall time from two completed Scrapshift campaigns.

This reports observed timing, not FPS, identical workloads or reference parity.
Shop/reward/cleanup time is excluded using wave_start -> wave_timer_end pairs.
"""
import argparse
import hashlib
import json
from pathlib import Path


def summarize(folder):
    trace = folder / "campaign-trace.json"
    data = json.loads(trace.read_text(encoding="utf-8-sig"))["events"]
    summary = json.loads((folder / "campaign-check.json").read_text(encoding="utf-8-sig"))
    process = json.loads((folder / "process.json").read_text(encoding="utf-8-sig"))
    assert summary["status"] == "Passed" and summary["phase"] == "Won"
    assert process["status"] == "exited" and process["exitCode"] == 0
    starts = {e["wave"]: e for e in data if e["event"] == "wave_start"}
    ends = {e["wave"]: e for e in data if e["event"] == "wave_timer_end"}
    assert sorted(starts) == sorted(ends) == list(range(1, 21))
    rows = []
    for wave in range(1, 21):
        start, end = starts[wave], ends[wave]
        wall = end["wallSeconds"] - start["wallSeconds"]
        active = end["simulatedSeconds"] - start["simulatedSeconds"]
        rows.append(dict(wave=wave, combatWall=wall, activeSeconds=active, combatWallExcess=wall-active))
    return dict(path=str(folder.resolve()), traceSha256=hashlib.sha256(trace.read_bytes()).hexdigest(),
                rows=rows, activeSeconds=sum(r["activeSeconds"] for r in rows),
                combatWall=sum(r["combatWall"] for r in rows),
                combatWallExcess=sum(r["combatWallExcess"] for r in rows))


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--before", type=Path, required=True)
    parser.add_argument("--after", type=Path, required=True)
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    before, after = summarize(args.before), summarize(args.after)
    result = dict(status="Compared", scope=__doc__, before=before, after=after,
                  excessReductionSeconds=before["combatWallExcess"]-after["combatWallExcess"],
                  limits="Separate evolving runs with engine-specific random combat and shop choices; not identical entity/shot counts, a human survival benchmark, FPS or proof that all performance issues are resolved. Trace callback timing can produce small negative per-wave excess.")
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(result, indent=2)+"\n", encoding="utf-8")
    print(json.dumps({k: result[k] for k in ["status", "excessReductionSeconds"]}))


if __name__ == "__main__":
    main()

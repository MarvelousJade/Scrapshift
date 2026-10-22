"""Finite static-stat probes for the supplied Brotato item catalog.

Usage: python scripts/item_pair_cases.py --mode preflight --output CASES.jsonl
       python scripts/item_pair_cases.py --mode all --output CASES.jsonl
"""
import argparse
import itertools
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
DATA = ROOT / "Assets/Scrapshift/Content/Reference/item-design.json"
PREFLIGHT = (
    "coffee", "beanie", "jellyshield", "wandering_bot", "bonk_dog",
    "scared_sausage", "eye_surgery", "frozen_heart", "power_generator",
    "potion", "lich", "triangle_of_power", "peacock", "nail",
)


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--mode", choices=("preflight", "all"), default="preflight")
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    ids = sorted(item["id"] for item in json.loads(DATA.read_text(encoding="utf-8"))["items"])
    assert len(ids) == len(set(ids)) == 209
    if args.mode == "preflight":
        chosen = [id_ for id_ in PREFLIGHT if id_ in ids]
        pairs = list(itertools.combinations(chosen, 2))[:24]
        pairs += [(id_, id_) for id_ in chosen[:6]]
    else:
        chosen = ids
        pairs = itertools.chain(itertools.combinations(ids, 2), ((id_, id_) for id_ in ids))
    args.output.parent.mkdir(parents=True, exist_ok=True)
    count = 0
    with args.output.open("w", encoding="utf-8", newline="\n") as stream:
        stream.write(json.dumps({"id": "baseline", "items": []}, separators=(",", ":")) + "\n")
        count += 1
        for id_ in chosen:
            stream.write(json.dumps({"id": f"single:{id_}", "items": [id_]}, separators=(",", ":")) + "\n")
            count += 1
        for left, right in pairs:
            stream.write(json.dumps({"id": f"{left}+{right}", "items": [left, right]}, separators=(",", ":")) + "\n")
            count += 1
    print(f"{count} cases from {len(ids)} imported item IDs -> {args.output}")


if __name__ == "__main__":
    main()

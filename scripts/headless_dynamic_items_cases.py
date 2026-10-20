"""Finite dynamic state cases shared by the original and Unity exporters."""
import argparse
import json
from pathlib import Path


def cases():
    # These are all imported item IDs in the standing/interval/full-health
    # consumable families, rather than an arbitrary sample of static stats.
    for character in ("well_rounded", "doctor", "renegade"):
        for items in (["wisdom"], ["wisdom"] * 2, ["medikit"], ["medikit"] * 2,
                      ["wisdom", "medikit"], ["wisdom"] * 2 + ["medikit"] * 2):
            yield {"id": character + ":timer:" + "+".join(items), "character": character,
                   "items": items, "operations": [{"kind": "tick", "value": value} for value in (0, 1, 3, 1, 4, 1, 4, 1)]}
        for items in (["barricade"], ["chameleon"], ["statue"],
                      ["barricade", "chameleon", "statue"],
                      ["barricade", "chameleon", "statue"] * 2,
                      ["barricade", "chameleon", "statue", "retromations_hoodie"]):
            yield {"id": character + ":movement:" + "+".join(items), "character": character,
                   "items": items, "operations": [{"kind": "move", "value": value} for value in (0, 1, 1, 0, 0, 1, 0)]}
    for copies in (1, 2, 4):
        for injured in (False, True):
            yield {"id": f"stomach:{copies}:injured={injured}", "character": "well_rounded",
                   "items": ["extra_stomach"] * copies,
                   "operations": [{"kind": "fruit", "value": 1 if injured else 0} for _ in range(12)]}
    for items in ([], ["barricade", "chameleon", "statue"], ["barricade", "retromations_hoodie"]):
        yield {"id": "streamer:movement:" + "+".join(items), "character": "streamer", "items": items,
               "operations": [{"kind": "move", "value": value} for value in (0, 1, 0, 1, 0)]}
    for items in ([], ["acid"], ["barricade", "statue"]):
        yield {"id": "golem:threshold:" + "+".join(items), "character": "golem", "items": items,
               "operations": [{"kind": "health", "value": value} for value in (0, 1, 2, 3, 2, 0)]}
    for item in ("wisdom", "medikit", "barricade"):
        yield {"id": "doctor:lifecycle:" + item, "character": "doctor", "items": [item],
               "operations": [{"kind": "tick", "value": 5}, {"kind": "intermission"},
                              {"kind": "remove", "key": item}, {"kind": "add", "key": item},
                              {"kind": "next_wave"}, {"kind": "tick", "value": 5}]}
    for outcome in ("lost", "won"):
        yield {"id": "golem:terminal:" + outcome, "character": "golem", "items": [],
               "operations": [{"kind": "health", "value": 3}, {"kind": "end_" + outcome}]}


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    rows = list(cases())
    for row in rows:
        row["starter"] = "medical_gun" if row["character"] == "doctor" else "pistol"
    root = Path(__file__).resolve().parents[1]
    profiles = {row["id"]: row for row in json.loads((root / "Assets/Scrapshift/Content/Reference/starter-profiles.json").read_text(encoding="utf-8"))["characters"]}
    item_ids = {row["id"] for row in json.loads((root / "Assets/Scrapshift/Content/Reference/item-design.json").read_text(encoding="utf-8"))["items"]}
    for row in rows:
        if row["character"] not in profiles or row["starter"] not in profiles[row["character"]]["startingWeapons"]:
            raise ValueError("Unsupported character/starter in " + row["id"])
        referenced = row["items"] + [operation["key"] for operation in row["operations"] if "key" in operation]
        if set(referenced) - item_ids:
            raise ValueError("Missing imported item IDs in " + row["id"] + ": " + str(set(referenced) - item_ids))
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text("".join(json.dumps(row, separators=(",", ":")) + "\n" for row in rows), encoding="utf-8")
    print(json.dumps({"cases": len(rows), "snapshots": sum(len(row["operations"]) for row in rows)}))


if __name__ == "__main__":
    main()

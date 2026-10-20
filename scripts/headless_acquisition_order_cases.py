"""Bounded real-item acquisition/removal sequences for the dynamic exporters."""
import argparse
import json
from pathlib import Path


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    root = Path(__file__).resolve().parents[1]
    items = json.loads((root / "Assets/Scrapshift/Content/Reference/item-design.json").read_text())["items"]
    profiles = json.loads((root / "Assets/Scrapshift/Content/Reference/starter-profiles.json").read_text())["characters"]
    starters = {p["id"]: p["startingWeapons"][0] for p in profiles if p["startingWeapons"]}
    cases = []
    # Never hold more than one copy of either item. Removing a trigger item
    # must not erase a permanent gain that was already earned from it.
    for character in ("well_rounded", "mage"):
        for item in items:
            if item["id"] == "snowball":
                continue
            # Ordinary item acquisition/removal happens at intermission. An
            # unready source Player cannot deliver a standing-input callback
            # immediately after an injected in-wave acquisition.
            operations = [{"kind": "intermission"}]
            for kind, key in (("add", "snowball"), ("add", item["id"]),
                              ("remove", item["id"]), ("add", item["id"]),
                              ("remove", "snowball"), ("remove", item["id"]),
                              ("add", item["id"]), ("add", "snowball"),
                              ("remove", item["id"]), ("add", item["id"])):
                operations.append({"kind": kind, "key": key})
            cases.append({"id": character + ":snowball-order:" + item["id"],
                          "character": character, "starter": starters[character],
                          "items": [], "operations": operations})
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text("".join(json.dumps(c, separators=(",", ":")) + "\n" for c in cases))
    print(json.dumps({"cases": len(cases), "snapshots": sum(len(c["operations"]) for c in cases),
                      "availableOtherItems": sum(i["available"] for i in items if i["id"] != "snowball"),
                      "limits": "Direct grants/removals, including unavailable imported items; not shop or crate eligibility"}))


if __name__ == "__main__":
    main()

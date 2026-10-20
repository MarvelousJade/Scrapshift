"""Character baselines and single-item static-stat probes.

Usage: python scripts/character_item_cases.py --mode preflight --output CASES.jsonl
       python scripts/character_item_cases.py --mode all --output CASES.jsonl
"""
import argparse
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
CONTENT = ROOT / "Assets/Scrapshift/Content/Reference"
PREFLIGHT_CHARACTERS = ("well_rounded", "renegade", "beast_master", "golem", "cyborg")
PREFLIGHT_ITEMS = ("coffee", "jellyshield", "wandering_bot", "retromations_hoodie", "power_generator")


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--mode", choices=("preflight", "all"), default="preflight")
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    starter_data = json.loads((CONTENT / "starter-profiles.json").read_text(encoding="utf-8"))
    characters = starter_data["characters"]
    available_weapons = {(weapon["id"], weapon["tier"]) for weapon in starter_data["equipment"] if weapon["available"]}
    items = sorted(item["id"] for item in json.loads((CONTENT / "item-design.json").read_text(encoding="utf-8"))["items"])
    assert len(characters) == 50 and len(items) == 209
    if args.mode == "preflight":
        characters = [c for c in characters if c["id"] in PREFLIGHT_CHARACTERS]
        items = [id_ for id_ in PREFLIGHT_ITEMS if id_ in items]
    characters.sort(key=lambda c: c["id"])
    args.output.parent.mkdir(parents=True, exist_ok=True)
    count = 0
    with args.output.open("w", encoding="utf-8", newline="\n") as stream:
        for character in characters:
            selected = character.get("startingWeapons") or []
            starter = next((id_ for id_ in selected if (id_, character.get("minTier", 1)) in available_weapons), "") if character.get("weaponSlots", 6) > 0 else ""
            selected_item = (character.get("startingItemChoices") or [""])[0]
            common = {"character": character["id"], "starter": starter, "selectedItem": selected_item, "tier": character.get("minTier", 1)}
            for id_, loadout in [("baseline", [])] + [(f"single:{item}", [item]) for item in items]:
                row = {"id": f"{character['id']}:{id_}", **common, "items": loadout}
                stream.write(json.dumps(row, separators=(",", ":")) + "\n")
                count += 1
    print(f"{count} cases for {len(characters)} characters -> {args.output}")


if __name__ == "__main__":
    main()

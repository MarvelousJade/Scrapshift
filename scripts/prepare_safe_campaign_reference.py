"""Add opt-in acquisition filtering to the established isolated original fixture.

Does not launch Godot or change production reference files. Preserve the exact
old fixture and before/after source hashes under a new evidence directory.
"""
import argparse
import hashlib
import json
from pathlib import Path

REPO = Path(__file__).resolve().parents[1]
CLONE = Path("E:/Codex Projects/Job Search Assistant/tmp/BrotatoParity20261020")
ORIGINAL = Path("E:/UnityProjects/brotato-(3.x)")

def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

def inputs(root):
    return {p.relative_to(root).as_posix(): sha(p) for p in sorted(root.rglob("*"))
            if p.is_file() and p.suffix.lower() in {".gd", ".tres", ".tscn", ".godot"}}

def once(text, old, new):
    if text.count(old) != 1:
        raise ValueError("Expected exactly one inherited fixture fragment: " + repr(old[:90]))
    return text.replace(old, new, 1)

HELPERS = '''func safe_loadout_excludes(item):
\tif not safe_loadout or item == null or not item is ItemParentData or item.effects == null:
\t\treturn false
\tfor effect in item.effects:
\t\tif effect is Effect and effect.value > 0 and effect.key in ["lose_hp_per_second", "stat_lose_hp_per_second"]:
\t\t\treturn true
\treturn false

func safe_skip_offer(item):
\tvar token = str(RunData.current_wave) + ":" + str(item.get_instance_id()) + ":" + item.item_data.my_id
\tif not safe_skipped_offers.has(token):
\t\tsafe_skipped_offers[token] = true
\t\tsafe_skipped_offer_count += 1
\t\tevent("purchase_skipped_policy", {"id": item.item_data.my_id, "price": item.value})

'''

def prepare(evidence):
    if CLONE.resolve() == ORIGINAL.resolve():
        raise ValueError("Clone cannot be the original")
    evidence = evidence.resolve()
    evidence.relative_to(REPO.resolve())
    evidence.mkdir(parents=True, exist_ok=False)
    fixture = CLONE / "parity_fixture.gd"
    before = fixture.read_bytes()
    text = fixture.read_text(encoding="utf-8")
    if "MATCHED_SAFE_LOADOUT_POLICY" in text:
        raise ValueError("Already adapted; do not overwrite prior preparation evidence")
    original_before, clone_before = inputs(ORIGINAL), inputs(CLONE)
    (evidence / "parity_fixture.before.gd").write_bytes(before)
    survival = 'var survival = OS.get_environment("SURVIVAL").to_lower() in ["1", "true", "yes"]'
    text = once(text, survival, survival + '''
# MATCHED_SAFE_LOADOUT_POLICY: fixture acquisition choices only; no damage edits.
var safe_loadout = OS.get_environment("CAMPAIGN_SAFE_LOADOUT").to_lower() in ["1", "true", "yes"] and not survival
var safe_skipped_offers = {}
var safe_skipped_offer_count = 0
var safe_recycled_crates = 0
''')
    text = once(text, "func weapon_list():", HELPERS + "func weapon_list():")
    text = once(text, '\trow["headless"] = true', '''\trow["safe_loadout_policy"] = safe_loadout
\trow["damage_immunity_scope"] = "none" if survival else "contact_hurtbox_only"
\trow["skipped_self_damage_offers"] = safe_skipped_offer_count
\trow["recycled_self_damage_crates"] = safe_recycled_crates
\trow["headless"] = true''')
    text = once(text, '\tget_tree().change_scene(MenuData.game_scene)', '''\tif safe_loadout:
\t\tevent("safe_loadout_policy", {"detail": "Opt-in matched acquisition policy skips positive lose_hp_per_second offers and recycles such crates; contact immunity only, periodic damage unchanged. Historical original traces did not filter choices."})
\tget_tree().change_scene(MenuData.game_scene)''')
    text = once(text, '\t\t\t\t\tcrates+=1;event("crate_take",{"id":item.my_id});ui._on_take_button_pressed(item,0)', '''\t\t\t\t\tcrates += 1
\t\t\t\t\tif safe_loadout_excludes(item):
\t\t\t\t\t\tsafe_recycled_crates += 1
\t\t\t\t\t\tevent("crate_recycle_policy", {"id": item.my_id})
\t\t\t\t\t\tui._on_discard_button_pressed(item, 0)
\t\t\t\t\telse:
\t\t\t\t\t\tevent("crate_take", {"id": item.my_id})
\t\t\t\t\t\tui._on_take_button_pressed(item, 0)''')
    text = once(text, '\t\t\tif container._is_delay_active:return', '''\t\t\tif safe_loadout_excludes(item.item_data):
\t\t\t\tsafe_skip_offer(item)
\t\t\t\tcontinue
\t\t\tif container._is_delay_active:return''')
    text = once(text, '\tvar f=File.new();f.open(output+"/campaign-check.json",File.WRITE);', '''\tsummary["safe_loadout_policy"] = safe_loadout
\tsummary["damage_immunity_scope"] = "none" if survival else "contact_hurtbox_only"
\tsummary["skipped_self_damage_offers"] = safe_skipped_offer_count
\tsummary["recycled_self_damage_crates"] = safe_recycled_crates
\tif safe_loadout:
\t\tsummary["scope"] += " Explicit matched safe-loadout acquisition policy: positive periodic self-damage offers skipped and such crates recycled; no production damage changes. Historical original campaign used unfiltered choices."
\tvar f=File.new();f.open(output+"/campaign-check.json",File.WRITE);''')
    fixture.write_text(text, encoding="utf-8")
    original_after, clone_after = inputs(ORIGINAL), inputs(CLONE)
    if original_before != original_after:
        raise RuntimeError("Original source inputs changed during preparation")
    changed = sorted(k for k in set(clone_before) | set(clone_after) if clone_before.get(k) != clone_after.get(k))
    if changed != ["parity_fixture.gd"]:
        raise RuntimeError("Unexpected isolated source input changes: " + repr(changed))
    (evidence / "parity_fixture.after.gd").write_bytes(fixture.read_bytes())
    for name, value in [("original-before", original_before), ("original-after", original_after), ("clone-before", clone_before), ("clone-after", clone_after)]:
        (evidence / (name + ".json")).write_text(json.dumps(value, indent=2), encoding="utf-8")
    report = {"clone": str(CLONE), "original_read_only": str(ORIGINAL), "original_source_input_count": len(original_before),
              "original_source_unchanged": original_before == original_after, "original_has_git_metadata": (ORIGINAL / ".git").exists(),
              "changed_clone_inputs": changed, "before_fixture_sha256": hashlib.sha256(before).hexdigest(), "after_fixture_sha256": sha(fixture),
              "preserved_clone_source_input_count": len(clone_after) - 1, "launch_performed": False,
              "launch_environment": {"OUTPUT": "fresh isolated output directory", "SURVIVAL": "0", "CAMPAIGN_SAFE_LOADOUT": "1"},
              "policy": "Skip positive lose_hp_per_second item/weapon effect carriers, recycle matching crates through original callback; disabled for survival; no damage changes"}
    (evidence / "preparation.json").write_text(json.dumps(report, indent=2), encoding="utf-8")
    print(json.dumps(report, indent=2))

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--evidence", type=Path, default=REPO / "artifacts/native-followup-2026-10-20/reference-safe-policy-preparation")
    prepare(parser.parse_args().evidence)

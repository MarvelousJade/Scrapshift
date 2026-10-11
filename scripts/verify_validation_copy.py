"""Verify an isolated Unity validation copy against this checkout, byte for byte."""
import argparse
import hashlib
import json
from pathlib import Path

parser = argparse.ArgumentParser()
parser.add_argument("copy", type=Path)
args = parser.parse_args()
root = Path(__file__).resolve().parents[1]

def inventory(base):
    return {p.relative_to(base).as_posix(): hashlib.sha256(p.read_bytes()).hexdigest()
            for folder in ("Assets", "Packages", "ProjectSettings")
            for p in (base / folder).rglob("*") if p.is_file()}

original, isolated = inventory(root), inventory(args.copy)
differences = [key for key in sorted(original.keys() | isolated.keys())
               if original.get(key) != isolated.get(key)]
report = {
    "scope": "Assets, Packages and ProjectSettings including Unity metadata; byte comparison, not unsaved Editor state",
    "validationCopy": str(args.copy.resolve()),
    "files": len(original),
    "contentDigest": hashlib.sha256(json.dumps(original, sort_keys=True).encode()).hexdigest(),
    "differences": differences,
    "status": "Passed" if not differences else "Failed",
}
(root / "artifacts/validation-copy.json").write_text(json.dumps(report, indent=2) + "\n")
print(json.dumps(report, indent=2))
raise SystemExit(bool(differences))

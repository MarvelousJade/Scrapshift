"""Package the Windows player and verify every archived file against this build."""
from pathlib import Path
import hashlib
import json
import shutil
import zipfile

ROOT = Path(__file__).resolve().parents[1]
build = ROOT / "Builds/Windows"
archive = ROOT / "Builds/Scrapshift-Windows.zip"
if not (build / "Scrapshift.exe").is_file():
    raise SystemExit("Build the player first.")
shutil.copyfile(ROOT / "docs/PLAYER_README.txt", build / "README.txt")
files = sorted(p for p in build.rglob("*") if p.is_file())
with zipfile.ZipFile(archive, "w", zipfile.ZIP_DEFLATED, compresslevel=6) as bundle:
    for file in files:
        bundle.write(file, file.relative_to(build).as_posix())
with zipfile.ZipFile(archive) as bundle:
    if bundle.testzip() is not None:
        raise SystemExit("Archive CRC validation failed.")
    for file in files:
        if bundle.read(file.relative_to(build).as_posix()) != file.read_bytes():
            raise SystemExit(f"Archive differs from build: {file}")
report = {
    "archive": archive.relative_to(ROOT).as_posix(),
    "bytes": archive.stat().st_size,
    "sha256": hashlib.sha256(archive.read_bytes()).hexdigest(),
    "entries": len(files),
    "crcCheck": "Passed",
    "everyArchivedFileMatchesBuild": True,
}
(ROOT / "artifacts/package.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
print(json.dumps(report, indent=2))

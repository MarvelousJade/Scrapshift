"""Package an existing Web build, with per-file hashes and ZIP readback verification."""
import argparse
import hashlib
import json
from pathlib import Path
import zipfile

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--build', required=True, type=Path)
parser.add_argument('--zip', required=True, type=Path)
parser.add_argument('--receipt', required=True, type=Path)
parser.add_argument('--source-commit', default='')
parser.add_argument('--source-manifest', type=Path)
args = parser.parse_args()
build = args.build.resolve(strict=True)
archive = args.zip.resolve()
receipt_path = args.receipt.resolve()
if not (build / 'index.html').is_file():
    parser.error('Build folder must contain index.html')
if archive.is_relative_to(build):
    parser.error('ZIP must be outside the build folder')
if archive.exists():
    parser.error('Preserve the previous ZIP before creating its replacement')
if receipt_path == archive or receipt_path.is_relative_to(build):
    parser.error('Receipt must be outside the build folder and distinct from the ZIP')
if receipt_path.exists():
    parser.error('Preserve the previous receipt before creating its replacement')
source_manifest = args.source_manifest.resolve(strict=True) if args.source_manifest else None
source_proof = (dict(path=str(source_manifest), sha256=hashlib.sha256(source_manifest.read_bytes()).hexdigest())
                if source_manifest else None)
files = []
archive.parent.mkdir(parents=True, exist_ok=True)
with zipfile.ZipFile(archive, 'x', compression=zipfile.ZIP_DEFLATED, compresslevel=6) as output:
    for path in sorted(build.rglob('*')):
        if not path.is_file():
            continue
        name = path.relative_to(build).as_posix()
        data = path.read_bytes()
        output.writestr(name, data)
        files.append(dict(path=name, bytes=len(data), sha256=hashlib.sha256(data).hexdigest()))
with zipfile.ZipFile(archive) as packaged:
    if packaged.testzip() is not None or 'index.html' not in packaged.namelist():
        raise RuntimeError('ZIP integrity or root index verification failed')
    if set(packaged.namelist()) != {row['path'] for row in files}:
        raise RuntimeError('ZIP entry set differs from build files')
    for row in files:
        if hashlib.sha256(packaged.read(row['path'])).hexdigest() != row['sha256']:
            raise RuntimeError('ZIP readback differs: ' + row['path'])
receipt = dict(build=str(build), zip=str(archive), zipBytes=archive.stat().st_size,
               zipSha256=hashlib.sha256(archive.read_bytes()).hexdigest(),
               buildBytes=sum(row['bytes'] for row in files), zipReadbackPassed=True,
               sourceCommit=args.source_commit, sourceManifest=source_proof, files=files)
receipt_path.parent.mkdir(parents=True, exist_ok=True)
receipt_path.write_text(json.dumps(receipt, indent=2) + '\n', encoding='utf-8')
print(json.dumps({key: value for key, value in receipt.items() if key != 'files'}))

"""Restore large raw evidence from lossless archives; verify existing files without overwriting."""
from pathlib import Path
import gzip,hashlib,json
root=Path(__file__).resolve().parent
for row in json.loads((root/'compressed-evidence.json').read_text(encoding='utf-8')):
 target=root/row['path'];archive=root/row['gzipPath']
 assert hashlib.sha256(archive.read_bytes()).hexdigest()==row['gzipSha256']
 raw=gzip.decompress(archive.read_bytes());assert hashlib.sha256(raw).hexdigest()==row['sha256']
 if target.exists():assert target.read_bytes()==raw
 else:target.parent.mkdir(parents=True,exist_ok=True);target.write_bytes(raw)
print('Large raw evidence restored/verified without overwriting existing data.')

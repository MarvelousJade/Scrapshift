"""Copy intended Unity inputs to the established isolated validation project."""
from pathlib import Path
import hashlib,json,shutil
root=Path(__file__).resolve().parents[1]
# Explicit project rather than a second Editor on the user's working project.
clone=Path('E:/Codex Projects/Job Search Assistant/tmp/ScrapshiftParity')
out=root/'artifacts/optimization-2026-10-21'
out.mkdir(parents=True,exist_ok=True)
records=[]
for folder in ('Assets','Packages','ProjectSettings'):
 for src in sorted((root/folder).rglob('*')):
  if not src.is_file():continue
  rel=src.relative_to(root);dst=clone/rel
  data=src.read_bytes();digest=hashlib.sha256(data).hexdigest()
  if not dst.exists() or hashlib.sha256(dst.read_bytes()).hexdigest()!=digest:
   dst.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(src,dst)
  records.append({'path':rel.as_posix(),'sha256':digest})
(out/'source-manifest.json').write_text(json.dumps(records,indent=2)+'\n')
print(f'Copied/verified {len(records)} inputs in {clone}')

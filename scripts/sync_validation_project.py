"""Copy intended Unity inputs; reject stale or unexpected validation inputs."""
from pathlib import Path
import argparse,hashlib,json,shutil
root=Path(__file__).resolve().parents[1]
# Explicit project rather than a second Editor on the user's working project.
clone=Path('E:/Codex Projects/Job Search Assistant/tmp/ScrapshiftParity')
out=root/'artifacts/optimization-2026-10-21'
out.mkdir(parents=True,exist_ok=True)
parser=argparse.ArgumentParser(description=__doc__)
parser.add_argument('--manifest',default='source-manifest.json',help='Manifest path relative to the optimization artifact folder')
args=parser.parse_args()
manifest=(out/args.manifest).resolve()
if not manifest.is_relative_to(out.resolve()):parser.error('--manifest must remain inside the optimization artifact folder')
manifest.parent.mkdir(parents=True,exist_ok=True)
folders=('Assets','Packages','ProjectSettings')
def digest(path):return hashlib.sha256(path.read_bytes()).hexdigest()
def inputs(project):
 return {p.relative_to(project).as_posix():p for folder in folders
         for p in (project/folder).rglob('*') if p.is_file()}
expected=inputs(root)
records=[]
for rel,src in sorted(expected.items()):
 dst=clone/rel;source_hash=digest(src)
 if not dst.exists() or digest(dst)!=source_hash:
  dst.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(src,dst)
 records.append({'path':rel,'sha256':source_hash})
# No blanket .meta or generated-file exception: Unity imports extra scripts and
# assets too. Quarantine obsolete inputs explicitly outside Assets beforehand.
actual=inputs(clone)
extra=sorted(actual.keys()-expected.keys())
missing=sorted(expected.keys()-actual.keys())
mismatched=[row['path'] for row in records
            if row['path'] in actual and digest(actual[row['path']])!=row['sha256']]
receipt={'status':'Passed' if not(extra or missing or mismatched) else 'Failed',
         'clone':str(clone),'manifest':manifest.relative_to(out.resolve()).as_posix(),'inputs':len(records),'extraFiles':extra,
         'missingFiles':missing,'mismatchedFiles':mismatched}
manifest.write_text(json.dumps(records,indent=2)+'\n')
(out/'source-sync-verification.json').write_text(json.dumps(receipt,indent=2)+'\n')
if receipt['status']!='Passed':
 raise SystemExit('Validation project input verification failed: '+json.dumps(receipt))
print(f'Copied/verified {len(records)} inputs in {clone}; no unexpected inputs')

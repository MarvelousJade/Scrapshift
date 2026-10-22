from pathlib import Path
import json,hashlib,shutil,sys
root=Path.cwd();a=Path(__file__).resolve().parent;clone=Path('E:/Codex Projects/Job Search Assistant/tmp/ScrapshiftParity');name=sys.argv[1]
summary=(clone/'artifacts/controller-build.txt').read_text();assert 'Result: Succeeded' in summary and 'Errors: 0' in summary
(a/('build-'+name.lower()+'-summary.txt')).write_text(summary)
src=clone/'Builds/ControllerCheck';dst=root/('Builds/GameplayParity20261020'+name);assert not dst.exists();shutil.copytree(src,dst)
(dst/'LaunchGame.cmd').write_bytes(b'@echo off\r\ncd /d "%~dp0"\r\nstart "" "%~dp0Scrapshift.exe"\r\n')
files=[]
for p in sorted(dst.rglob('*')):
 if p.is_file():
  rel=p.relative_to(dst);h=hashlib.sha256(p.read_bytes()).hexdigest();files.append(dict(path=rel.as_posix(),sha256=h));assert rel.as_posix()=='LaunchGame.cmd' or h==hashlib.sha256((src/rel).read_bytes()).hexdigest()
(a/('build-'+name.lower()+'-manifest.json')).write_text(json.dumps(files,indent=2)+'\n')
inputs=json.loads((a/'source-manifest.json').read_text())
for row in inputs:
 assert hashlib.sha256((root/row['path']).read_bytes()).hexdigest()==row['sha256'];assert hashlib.sha256((clone/row['path']).read_bytes()).hexdigest()==row['sha256']
(a/('build-'+name.lower()+'-source-manifest.json')).write_text(json.dumps(inputs,indent=2)+'\n');print(summary, len(inputs),len(files),flush=True)

from pathlib import Path
import hashlib,json,shutil
root=Path(__file__).resolve().parents[2]
clone=Path('E:/Codex Projects/Job Search Assistant/tmp/ScrapshiftParity')
target=root/'Builds/Optimization20261021'
shutil.copytree(clone/'Builds/Performance',target,dirs_exist_ok=False)
(target/'LaunchGame.cmd').write_text('@echo off\npushd "%~dp0"\nstart "" "Scrapshift.exe"\npopd\n')
rows=[dict(path=p.relative_to(target).as_posix(),sha256=hashlib.sha256(p.read_bytes()).hexdigest(),bytes=p.stat().st_size)for p in target.rglob('*')if p.is_file()]
Path(__file__).with_name('final-package.json').write_text(json.dumps(dict(path=str(target),files=rows,sourceManifest='source-manifest.json',cloneInputsVerifiedBeforeWebBuild=2626),indent=2)+'\n')
print(len(rows),sum(x['bytes']for x in rows))

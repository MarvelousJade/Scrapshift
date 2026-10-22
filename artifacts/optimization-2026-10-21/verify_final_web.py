"""Bind the tested HTTP copy, release folder, ZIP and prebuild source manifest."""
from pathlib import Path
import hashlib
import json

root = Path(__file__).resolve().parents[2]
evidence = Path(__file__).parent
clone = Path('E:/Codex Projects/Job Search Assistant/tmp/ScrapshiftParity')
def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()
manifest = json.loads((evidence / 'release-source-manifest.json').read_text())
mismatches = [r['path'] for r in manifest if digest(root / r['path']) != r['sha256']]
if mismatches:
    raise RuntimeError('Working source differs from prebuild manifest: ' + str(mismatches))
folders = dict(built=clone / 'Builds/WebRelease20261021', packaged=root / 'Builds/Web',
               browser=root / 'Builds/WebReview20261021/release')
trees = {name: {p.relative_to(folder).as_posix(): digest(p) for p in folder.rglob('*') if p.is_file()}
         for name, folder in folders.items()}
if not trees['built'] == trees['packaged'] == trees['browser']:
    raise RuntimeError('Built/tested/packaged files differ')
package = json.loads((evidence / 'web-final-package.json').read_text())
if trees['packaged'] != {row['path']: row['sha256'] for row in package['files']}:
    raise RuntimeError('Package receipt differs from current release')
if digest(Path(package['zip'])) != package['zipSha256'] or not package['zipReadbackPassed']:
    raise RuntimeError('ZIP differs from its verified readback receipt')
screenshots = ['final-release-title.jpg', 'final-fullscreen.jpg',
               'final-save-after-reload.jpg', 'final-focus-return-paused.jpg']
receipt = dict(status='Passed', sourceCommit=package['sourceCommit'],
               sourceInputs=len(manifest), currentRootInputsMatchPrebuild=True,
               builtTestedPackagedMatch=True, packageFiles=len(trees['built']),
               files=trees['built'], zipSha256=package['zipSha256'],
               screenshots={name: digest(evidence / 'browser' / name) for name in screenshots})
(evidence / 'web-final-verification.json').write_text(json.dumps(receipt, indent=2) + '\n')
print(json.dumps({key: value for key, value in receipt.items() if key not in ('files', 'screenshots')}))

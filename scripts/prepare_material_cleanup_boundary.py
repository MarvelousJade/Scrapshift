"""Prepare a separate source-engine clone and four specs; do not launch games."""
import argparse
import hashlib
import json
import shutil
from pathlib import Path


def main():
    p = argparse.ArgumentParser(description=__doc__)
    p.add_argument('--base', type=Path, required=True)
    p.add_argument('--clone', type=Path, required=True)
    p.add_argument('--output', type=Path, required=True)
    p.add_argument('--engine', type=Path, required=True)
    a = p.parse_args()
    base, clone, out = a.base.resolve(), a.clone.resolve(), a.output.resolve()
    if clone.exists():
        raise SystemExit('Refusing to overwrite existing boundary clone')
    if base == clone or base in clone.parents:
        raise SystemExit('Clone must be separate from base')
    shutil.copytree(base, clone, ignore=shutil.ignore_patterns('.git', 'userdata*', 'host-data*', 'logs'))
    fixture = Path(__file__).with_name('material_cleanup_boundary_reference.gd')
    shutil.copy2(fixture, clone / 'parity_fixture.gd')
    # Redirect previous absolute clone/user paths only inside this new clone.
    old = str(base).replace('\\', '/')
    new = str(clone).replace('\\', '/')
    for path in clone.rglob('*'):
        if path.suffix not in ('.gd', '.godot', '.tscn') or not path.is_file():
            continue
        text = path.read_text(encoding='utf-8')
        updated = text.replace(old, new).replace(str(base), str(clone))
        if text != updated:
            path.write_text(updated, encoding='utf-8')
    out.mkdir(parents=True, exist_ok=True)
    specs = []
    for case in ('fresh_normal', 'pooled_normal', 'fresh_optimized', 'pooled_optimized'):
        case_out = out / case
        case_out.mkdir(exist_ok=True)
        spec = {'executable': str(a.engine.resolve()), 'workingDirectory': str(Path.cwd()),
                'output': str(case_out), 'arguments': ['--path', str(clone), '--no-window', '--resolution', '1280x720'],
                'environment': {'APPDATA': str(case_out/'host-data'/'Roaming'),
                                'LOCALAPPDATA': str(case_out/'host-data'/'Local'),
                                'OUTPUT': str(case_out), 'MATERIAL_CASE': case}}
        target = case_out / 'runner-spec.json'
        target.write_text(json.dumps(spec, indent=2)+'\n', encoding='utf-8')
        specs.append(str(target))
    snapshots = {}
    for relative in ('main.gd', 'global/entity_spawner.gd', 'parity_fixture.gd', 'project.godot'):
        source = clone / relative
        target = out / (relative.replace('/', '_') + '.snapshot')
        shutil.copy2(source, target)
        snapshots[relative] = hashlib.sha256(source.read_bytes()).hexdigest()
    receipt = {'base': str(base), 'clone': str(clone), 'scope': 'Separate diagnostic clone; no game launched',
               'snapshots': snapshots, 'runner_specs': specs}
    (out/'preparation.json').write_text(json.dumps(receipt, indent=2)+'\n', encoding='utf-8')
    print(json.dumps(receipt))


if __name__ == '__main__':
    main()

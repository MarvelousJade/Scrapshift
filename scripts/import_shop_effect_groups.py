"""Read-only import of spawn groups referenced by shop theft effects."""
import json
from pathlib import Path
import analyze_reference as source

source.ROOT = Path('E:/UnityProjects/brotato-(3.x)')
root = Path(__file__).resolve().parents[1]
records = []
for path in sorted((source.ROOT / 'zones').rglob('*.tres')):
    relative = path.relative_to(source.ROOT).as_posix()
    data = source.resource(relative)
    if 'wave_units_data' not in data:
        continue
    units = []
    for unit_path in data['wave_units_data']:
        unit = source.resource(unit_path)
        units.append(dict(id=Path(unit['unit_scene']).stem, min=unit.get('min_number', 1),
                          max=unit.get('max_number', 1), chance=unit.get('spawn_chance', 1),
                          distance=unit.get('additional_min_distance_from_player', 0) / 100))
    group = dict(start=data.get('spawn_timing', 1), repeats=data.get('repeating', 999),
                 interval=data.get('repeating_interval', 3), reduce=data.get('reduce_repeating_interval', 0),
                 minimum=data.get('min_repeating_interval', 1), chance=data.get('spawn_chance', 1),
                 area=data.get('area', -1) / 100, inset=data.get('spawn_dist_away_from_edges', 0) / 100,
                 edge=data.get('spawn_edge_of_map', False), units=units, bucket=0,
                 minDifficulty=data.get('min_difficulty', 0), maxDifficulty=data.get('max_difficulty', 9999),
                 minWave=data.get('min_wave', 0), maxWave=data.get('max_wave', 9999),
                 preventElite=data.get('prevent_if_elite', False), preventHorde=data.get('prevent_if_horde', False),
                 boss=data.get('is_boss', False), loot=data.get('is_loot', False))
    records.append(dict(path=relative, group=group))
output = root / 'Assets/Scrapshift/Resources/shop-effect-spawns.json'
output.write_text(json.dumps(dict(groups=records), indent=2) + '\n', encoding='utf-8')
print(f'{len(records)} source spawn groups imported without changing the reference')

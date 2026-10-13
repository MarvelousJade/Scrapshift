"""Import base wave schedules for all difficulty settings, including real horde groups."""
from pathlib import Path
import json
import analyze_reference as source
source.ROOT=Path('E:/UnityProjects/brotato-(3.x)');root=Path(__file__).resolve().parents[1]
zone=source.resource('zones/zone_1/zone_1_data.tres')
def group(path,bucket=0):
 g=source.resource(path);units=[]
 for up in g.get('wave_units_data',[]):
  u=source.resource(up);units.append(dict(id=Path(u['unit_scene']).stem,min=u.get('min_number',1),max=u.get('max_number',1),chance=u.get('spawn_chance',1),distance=u.get('additional_min_distance_from_player',0)/100))
 return dict(start=g.get('spawn_timing',1),repeats=g.get('repeating',999),interval=g.get('repeating_interval',3),reduce=g.get('reduce_repeating_interval',0),minimum=g.get('min_repeating_interval',1),chance=g.get('spawn_chance',1),area=g.get('area',-1)/100,inset=g.get('spawn_dist_away_from_edges',0)/100,edge=g.get('spawn_edge_of_map',False),bucket=bucket,units=units,
  minDifficulty=g.get('min_difficulty',0),maxDifficulty=g.get('max_difficulty',9999),minWave=g.get('min_wave',0),maxWave=g.get('max_wave',9999),preventElite=g.get('prevent_if_elite',False),preventHorde=g.get('prevent_if_horde',False),boss=g.get('is_boss',False))
waves=[]
for path in zone['waves_data']:
 w=source.resource(path);groups=[group(p) for p in w.get('groups_data',[])+zone['groups_data_in_all_waves']]
 for i,options in enumerate(w.get('conditional_groups_data',[]),1):groups.extend(group(p,i) for p in options)
 waves.append(dict(duration=w.get('wave_duration',60),cap=w.get('max_enemies',100),groups=groups))
(root/'Assets/Scrapshift/Resources/base-waves.json').write_text(json.dumps(dict(waves=waves,hordes=[group(p) for p in zone['horde_groups']]),indent=2)+'\n',encoding='utf8')
print(len(waves),'waves;',len(zone['horde_groups']),'horde groups')

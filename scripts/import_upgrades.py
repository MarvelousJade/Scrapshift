import json
from pathlib import Path
import analyze_reference as source
source.ROOT=Path('E:/UnityProjects/brotato-(3.x)')
root=Path(__file__).resolve().parents[1]
rows=[]
for folder in (source.ROOT/'items/upgrades').iterdir():
 if not folder.is_dir():continue
 tiers=list(folder.glob('[1-4]/*_data.tres'))
 if len(tiers)!=4:continue
 values=[];stat=''
 for p in sorted(tiers):
  d=source.resource(p.relative_to(source.ROOT));e=source.resource(d['effects'][0]);stat=e['key'].removeprefix('stat_');values.append(e['value'])
 if stat not in ['max_hp','hp_regeneration','lifesteal','percent_damage','melee_damage','ranged_damage','elemental_damage','attack_speed','crit_chance','engineering','range','armor','dodge','speed','luck','harvesting']:continue
 rows.append(dict(stat=stat,values=values))
(root/'Assets/Scrapshift/Resources/base-upgrades.json').write_text(json.dumps(dict(upgrades=rows),indent=2)+'\n',encoding='utf8')
print(len(rows),'upgrade tracks')

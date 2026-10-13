import json
from pathlib import Path
import analyze_reference as source
root=Path(__file__).resolve().parents[1]
source.ROOT=Path('E:/UnityProjects/brotato-(3.x)')
archive=json.loads((root/'artifacts/base-roster-unlocks.json').read_text(encoding='utf-8'))
rows=[]
for c in archive['challenges']:
    if not isinstance(c.get('reward'),str):continue
    reward=source.resource(c['reward'])
    rid=('weapon_'+Path(c['reward']).parts[-3]) if c['reward'].startswith('weapons/') else reward['my_id']
    rows.append(dict(id=c['my_id'],name=c['name'],description=c['description'],value=c['value'],stat=c['stat'].removeprefix('stat_'),reward=rid,kind=c['reward_type'],args=c.get('additional_args',[])))
folder=root/'Assets/Scrapshift/Resources';folder.mkdir(exist_ok=True)
(folder/'base-unlocks.json').write_text(json.dumps(dict(challenges=rows),indent=2)+'\n',encoding='utf-8')
print(len(rows),'reward definitions')

"""Count registered base catalog identities, before DLC injection."""
import json,re,sys
from pathlib import Path
import analyze_reference as source
source.ROOT=Path(sys.argv[1] if len(sys.argv)>1 else 'E:/UnityProjects/brotato-(3.x)')
root=Path(__file__).resolve().parents[1]
scene=(source.ROOT/'singletons/item_service.tscn').read_text()
resources={i:path for path,i in re.findall(r'\[ext_resource path="res://([^"]+)" type="Resource" id=(\d+)\]',scene)}
def paths(field):
    line=next(l for l in scene.splitlines() if l.startswith(field+' ='))
    return [resources[i] for i in re.findall(r'ExtResource\( (\d+) \)',line)]
base={p.split('/')[2] for p in paths('weapons') if p.startswith('weapons/')}
items=[dict(path=p,id=source.resource(p)['my_id']) for p in paths('items')]
base_items={i['id'].removeprefix('item_') for i in items}
content=root/'Assets/Scrapshift/Content/Reference'
equipment=json.loads((content/'starter-profiles.json').read_text())['equipment']+json.loads((content/'catalog.json').read_text())['equipment']
present={w['id'] for w in equipment};available={w['id'] for w in equipment if w.get('available',True)}
source_available={w['id'] for w in equipment if w.get('sourceAvailable',w.get('available',True))}
unfinished={w['id'] for w in equipment if not w.get('implemented',True)}
ours=json.loads((content/'item-design.json').read_text())['items']
report=dict(reference='Registered base weapons and unique item my_id values in supplied item_service.tscn, before DLC injection; weapon families, not tiers',baseWeaponFamilies=len(base),importedWeaponFamilies=len(base&present),availableWeaponFamilies=len(base&available),missingWeaponFamilies=sorted(base-present),lockedWeaponFamilies=sorted((base&present)-source_available),unfinishedWeaponFamilies=sorted(unfinished),implementedWeaponFamilies=len(base- unfinished),baseItems=len(base_items),importedItems=len(ours),availableItems=sum(i['available'] for i in ours),missingItems=sorted(base_items-{i['id'] for i in ours}),itemSources=items)
(root/'artifacts/base-catalog-coverage.json').write_text(json.dumps(report,indent=2)+'\n')
print('Missing:',len(report['missingWeaponFamilies']),'weapon families,',len(report['missingItems']),'items')

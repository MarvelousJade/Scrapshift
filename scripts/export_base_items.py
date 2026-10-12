"""Archive registered base item data/effect definitions without importing unsupported mechanics."""
import re,json,hashlib
from pathlib import Path
import analyze_reference as source
source.ROOT=Path('E:/UnityProjects/brotato-(3.x)')
root=Path(__file__).resolve().parents[1]
scene=(source.ROOT/'singletons/item_service.tscn').read_text()
refs={i:p for p,i in re.findall(r'\[ext_resource path="res://([^"]+)" type="Resource" id=(\d+)\]',scene)}
paths=[refs[i] for i in re.findall(r'ExtResource\( (\d+) \)',next(l for l in scene.splitlines() if l.startswith('items =')))]
items=[]
for p in paths:
    d=source.resource(p)
    items.append(dict(path=p,sha256=hashlib.sha256((source.ROOT/p).read_bytes()).hexdigest(),**d,resolvedEffects=[source.resource(e) for e in d.get('effects',[])]))
(root/'artifacts/base-item-resources.json').write_text(json.dumps(items,indent=2)+'\n')
print(len({i['my_id'] for i in items}),'registered item identities archived; archive is not runtime implementation')

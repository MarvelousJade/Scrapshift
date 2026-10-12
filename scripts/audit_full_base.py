"""Read-only dependency inventory for the complete registered base roster and unlocks."""
import json,re,hashlib
from pathlib import Path
import analyze_reference as source
source.ROOT=Path('E:/UnityProjects/brotato-(3.x)')
root=Path(__file__).resolve().parents[1]
def registered(scene,field):
    text=(source.ROOT/scene).read_text()
    refs={i:p for p,i in re.findall(r'\[ext_resource path="res://([^"]+)" type="Resource" id=(\d+)\]',text)}
    line=next(l for l in text.splitlines() if l.startswith(field+' ='))
    return [refs[i] for i in re.findall(r'ExtResource\( (\d+) \)',line)]
def record(path):
    d=source.resource(path)
    return dict(path=path,sha256=hashlib.sha256((source.ROOT/path).read_bytes()).hexdigest(),**d,
        resolvedEffects=[source.resource(e) for e in d.get('effects',[])])
characters=[record(p) for p in registered('singletons/item_service.tscn','characters')]
challenges=[record(p) for p in registered('singletons/challenge_service.tscn','challenges')]
result=dict(scope='Registered resources before DLC injection; archive is not runtime implementation.',characters=characters,challenges=challenges)
(root/'artifacts/base-roster-unlocks.json').write_text(json.dumps(result,indent=2)+'\n')
print(len(characters),'characters;',len(challenges),'registered challenges')

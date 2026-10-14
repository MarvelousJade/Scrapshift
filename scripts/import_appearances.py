"""Import exact appearance eligibility, position, priority and depth from the supplied game."""
from pathlib import Path
import json,re,hashlib,shutil
from PIL import Image
import analyze_reference as src
ROOT=src.ROOT=Path('E:/UnityProjects/brotato-(3.x)');PROJECT=Path(__file__).resolve().parents[1]
OUT=PROJECT/'artifacts/appearance-parity/source-art';OUT.mkdir(parents=True,exist_ok=True)
service=(ROOT/'singletons/item_service.tscn').read_text(encoding='utf-8-sig');refs=re.findall(r'\[ext_resource path="res://([^"]+\.tres)"',service)
paths={}
for path in refs:
 d=src.resource(path)
 if d.get('my_id','').startswith(('item_','character_')):paths[d['my_id']]=path
provenance={};textures={}
def read(path):
 provenance[path]=hashlib.sha256((ROOT/path).read_bytes()).hexdigest();return src.resource(path)
positions=['other','above-head','top-left','top-right','hat','forehead','eyes','nose','mouth','back','neck','torso','pocket-left','pocket-right','skin','tail','accessory-left','accessory-right']
def layer(path):
 d=read(path);p=d.get('sprite');
 if not p:return dict(id=path,texture='',position=d.get('position',0),slot='' if d.get('position',0)==0 else positions[d['position']],priority=d.get('display_priority',0),depth=d.get('depth',1),character=d.get('is_character_appearance',False),width=150,height=150)
 key=Path(p).stem+'_'+hashlib.sha1(p.encode()).hexdigest()[:8]
 shutil.copyfile(ROOT/p,OUT/(key+'.png'));provenance[p]=hashlib.sha256((ROOT/p).read_bytes()).hexdigest()
 im=Image.open(ROOT/p).convert('RGBA');box=im.getchannel('A').getbbox();textures[key]=dict(source=p,width=im.width,height=im.height,alpha=list(box) if box else None)
 pos=d.get('position',0)
 return dict(id=path,texture='AppearanceReference/'+key,position=pos,slot='' if pos==0 else positions[pos],priority=d.get('display_priority',0),depth=d.get('depth',1),character=d.get('is_character_appearance',False),width=im.width,height=im.height)
items=[];chars=[]
for item in json.loads((PROJECT/'Assets/Scrapshift/Content/Reference/item-design.json').read_text())['items']:
 path=paths['item_'+item['id']];d=read(path);items.append(dict(id=item['id'],source=path,layers=[layer(x) for x in d.get('item_appearances',[]) if x]))
for char in json.loads((PROJECT/'Assets/Scrapshift/Content/Reference/starter-profiles.json').read_text())['characters']:
 path=paths['character_'+char['id']];d=read(path);chars.append(dict(id=char['id'],source=path,layers=[layer(x) for x in d.get('item_appearances',[]) if x]))
for p in ['items/global/item_appearance_data.gd','singletons/run_data.gd','entities/units/player/player.gd','global/sorter.gd','ui/menus/ingame/character_panel_ui.gd']:read(p)
body=Image.open(ROOT/'entities/units/player/potato.png').convert('RGBA').getchannel('A').getbbox()
data=dict(items=items,characters=chars,sourceBody=list(body))
(PROJECT/'Assets/Scrapshift/Resources/appearance-reference.json').write_text(json.dumps(data,indent=2)+'\n')
review=PROJECT/'artifacts/appearance-parity';review.mkdir(exist_ok=True)
(review/'source-audit.json').write_text(json.dumps(dict(items=len(items),visibleItems=sum(bool(i['layers']) for i in items),itemLayers=sum(len(i['layers']) for i in items),characters=len(chars),characterLayers=sum(len(c['layers']) for c in chars),noAppearance=[i['id'] for i in items if not i['layers']],textures=textures,files=provenance),indent=2)+'\n')
print(len(items),'items;',sum(bool(i['layers']) for i in items),'with overlays;',len(chars),'characters;',len(textures),'source textures')



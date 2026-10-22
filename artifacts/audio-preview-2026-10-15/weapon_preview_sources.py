from pathlib import Path
import re,json,csv,hashlib
from PIL import Image

BASE=Path(__file__).resolve().parent
ROOT=BASE.parents[1]
OUT=BASE/'weapon-context-v1'
OUT.mkdir(exist_ok=True)
catalog=(ROOT/'Assets/Scrapshift/Content/Definitions/ReferenceCatalog.asset').read_text()
guid_paths={}
for meta in (ROOT/'Assets/Scrapshift').rglob('*.meta'):
    match=re.search(r'^guid: (\w+)',meta.read_text(errors='replace'),re.M)
    if match:guid_paths[match[1]]=meta.with_suffix('')
mapping=list(csv.DictReader((ROOT/'Assets/Scrapshift/Content/CyberAudioV3/provenance.tsv').open(),delimiter='\t'))
result=[]
for wid in ['laser_gun','rail_gun','rocket_launcher','lightning_shiv']:
    block=re.search(r'^    - id: '+wid+r'\n(.*?)(?=^    - id: |\Z)',catalog,re.S|re.M)[1]
    title=re.search(r'^      title: (.+)',block,re.M)[1]
    image=re.search(r'image: \{fileID: (-?\d+), guid: (\w+)',block)
    sprite_id,guid=image.groups();texture=guid_paths[guid];meta=Path(str(texture)+'.meta').read_text()
    sprite=None
    for section in meta.split('    - serializedVersion: ')[1:]:
        if re.search(r'internalID: '+sprite_id+r'\b',section):sprite=section;break
    assert sprite,(wid,sprite_id,texture)
    rect=re.search(r'rect:\s+serializedVersion: \d+\s+x: ([\d.]+)\s+y: ([\d.]+)\s+width: ([\d.]+)\s+height: ([\d.]+)',sprite)
    x,y,w,h=[round(float(v)) for v in rect.groups()]
    im=Image.open(texture);icon=im.crop((x,im.height-y-h,x+w,im.height-y));icon.save(OUT/(wid+'.png'))
    sounds=block.split('      sounds:')[1].split('      setIds:')[0]
    audio_guid=re.search(r'guid: (\w+)',sounds)[1]
    original=guid_paths[audio_guid]
    melee=re.search(r'^      melee: (\d)',block,re.M)[1]=='1'
    profile='3' if melee else '0'
    current=next(r for r in mapping if r['profile']==profile and (ROOT/r['supplied_source_asset']).resolve()==original.resolve())
    result.append(dict(id=wid,title=title,source=str(original),current=str(ROOT/current['derived_v3_asset']),sprite=str(texture),sprite_id=sprite_id,rect=[x,y,w,h],icon=str(OUT/(wid+'.png')),interval=float(re.search(r'^      interval: ([\d.]+)',block,re.M)[1])))
(OUT/'sources.json').write_text(json.dumps(result,indent=2))
print(json.dumps(result,indent=2))

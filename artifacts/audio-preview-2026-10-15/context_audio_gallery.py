"""Review actual baked audio for every weapon, plus other sound-effect families."""
from pathlib import Path
import re, csv, html, os
from PIL import Image

ROOT=Path(__file__).resolve().parents[2]
OUT=Path(__file__).resolve().parent/'context-audio-applied'
OUT.mkdir(exist_ok=True)
catalog=(ROOT/'Assets/Scrapshift/Content/Definitions/ReferenceCatalog.asset').read_text()
profile_code=(ROOT/'Assets/Scrapshift/Runtime/ContextSoundProfile.cs').read_text()
constants={k:int(v) for k,v in re.findall(r'(\w+)=(\d+)',profile_code)}
weapon_map={}
for cases,family in re.findall(r'((?:\s*case "[^"]+":)+)return (\w+);',profile_code.split('public static int Support')[0]):
    for key in re.findall(r'case "([^"]+)"',cases):weapon_map[key]=constants[family]
guids={}
for meta in (ROOT/'Assets/Scrapshift').rglob('*.meta'):
    m=re.search(r'^guid: (\w+)',meta.read_text(errors='replace'),re.M)
    if m:guids[m[1]]=meta.with_suffix('')
def url(path):return Path(os.path.relpath(path,OUT)).as_posix()
def player(path):return f'<audio controls preload="none" src="{html.escape(url(path),quote=True)}"></audio>'
cards=[];seen=set();used=set()
for block in re.split(r'^    - id: ',catalog,flags=re.M)[1:]:
    if '      sounds:' not in block:continue
    key=block.splitlines()[0]
    if key in seen:continue
    seen.add(key)
    title=re.search(r'^      title: (.+)',block,re.M)[1]
    melee=re.search(r'^      melee: (\d)',block,re.M)[1]=='1'
    family=weapon_map.get(key,constants['Blade'] if melee else constants['Ballistic'])
    sounds=block.split('      sounds:')[1].split('      setIds:')[0]
    guid=re.search(r'guid: (\w+)',sounds)[1]
    new=ROOT/f'Assets/Scrapshift/Content/ContextAudioV1/{guid}-{family}.wav'
    old=ROOT/f'Assets/Scrapshift/Content/CyberAudioV3/{guid}-{3 if melee else 0}.wav'
    assert new.exists(),new
    if not old.exists():old=guids[guid]
    used.add(new.name)
    sid,artguid=re.search(r'image: \{fileID: (-?\d+), guid: (\w+)',block).groups()
    tex=guids[artguid]
    for section in Path(str(tex)+'.meta').read_text().split('    - serializedVersion: ')[1:]:
        if re.search(r'internalID: '+sid+r'\b',section):break
    r=re.search(r'rect:\s+serializedVersion: \d+\s+x: ([\d.]+)\s+y: ([\d.]+)\s+width: ([\d.]+)\s+height: ([\d.]+)',section)
    x,y,w,h=[round(float(v)) for v in r.groups()]
    im=Image.open(tex);im.crop((x,im.height-y-h,x+w,im.height-y)).save(OUT/(key+'.png'))
    cards.append(f'<article><img src="{key}.png" alt=""><h2>{html.escape(title)}</h2><div class="pair"><section><label>Previous game sound</label>{player(old)}</section><section><label>Applied contextual sound</label>{player(new)}</section></div></article>')
rows=list(csv.DictReader((ROOT/'Assets/Scrapshift/Content/ContextAudioV1/provenance.tsv').open(),delimiter='\t'))
extras=[]
families={v:k for k,v in constants.items()}
cue_names=['Fire','Impact','Kill','Hurt','Critical','Pickup','LevelUp','Install','Pulse','Swing','Reroll','Win','Lose','WaveClear','Charge','Step','UiFocus','UiPress','Spawn','InventoryFocus','Consume']
for row in rows:
    path=ROOT/row['derived_context_asset'];p=int(row['profile'])
    if path.name in used:continue
    label=families.get(p,cue_names[p-200] if 200<=p<221 else {0:'Fire',1:'Impact',2:'Alien Eyes',3:'Swing',4:'Victory',5:'Defeat'}.get(p,str(p)))
    extras.append(f'<article><h2>{html.escape(label)} · {html.escape(Path(row["supplied_source_asset"]).stem)}</h2>{player(path)}</article>')
(OUT/'index.html').write_text('''<!doctype html><html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Scrapshift applied audio</title><style>body{background:#101921;color:#e5eff5;font:16px system-ui;margin:30px auto;max-width:1000px;padding:0 20px}p{color:#b3c6d2;line-height:1.6}article{background:#1c2b36;padding:20px;border-radius:12px;margin:16px 0;overflow:hidden}h2{font-size:19px}img{width:140px;height:90px;object-fit:contain;float:right}.pair{display:grid;grid-template-columns:1fr 1fr;gap:24px;clear:both}label{display:block;margin:10px 0}audio{width:100%}input,button{padding:12px;border-radius:8px;border:0;margin:6px}input{width:60%}@media(max-width:650px){.pair{grid-template-columns:1fr}}</style><h1>Applied weapon and gameplay sounds</h1><p>Actual baked game clips. The weapon comparisons show one representative variant per weapon; all assigned variants are processed in the game. Playback uses the audio assets directly, without game mixer gain or random pitch. Pickup keeps the original recording; music is unchanged.</p><input placeholder="Filter by weapon or sound name" aria-label="Filter sounds" oninput="document.querySelectorAll('article').forEach(x=>x.hidden=!x.textContent.toLowerCase().includes(this.value.toLowerCase()))"><button onclick="document.querySelectorAll('audio').forEach(a=>{a.pause();a.currentTime=0})">Stop all</button>'''+''.join(cards)+'<h1>Additional variants and gameplay cues</h1>'+''.join(extras)+'''<script>document.querySelectorAll('audio').forEach(a=>a.addEventListener('play',()=>document.querySelectorAll('audio').forEach(b=>{if(a!==b)b.pause()})))</script></html>''',encoding='utf-8')
print(f'{len(cards)} weapons; {len(rows)} baked source/context pairs. {OUT / "index.html"}')

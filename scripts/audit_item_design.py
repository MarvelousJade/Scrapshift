"""Read-only reference extraction for the supported item/character shop rules."""
import json, hashlib, re
from pathlib import Path
import analyze_reference as source
source.ROOT=Path('E:/UnityProjects/brotato-(3.x)')
root=Path(__file__).resolve().parents[1]
ids=['coffee','beanie','glasses','helmet','leather_vest','bat','fertilizer','cake']
records=[]; characters=[]; files=[]; unsupported=[]
supported={'stat_'+k for k in ['max_hp','hp_regeneration','lifesteal','percent_damage','melee_damage','ranged_damage','elemental_damage','attack_speed','crit_chance','engineering','range','armor','dodge','speed','luck','harvesting']}
original_ids=ids.copy()
ids=[]
for path in sorted((source.ROOT/'items/all').glob('*/*_data.tres')):
    d=source.resource(path.relative_to(source.ROOT).as_posix()); effects=[source.resource(e) for e in d.get('effects',[])]
    if effects and all(e.get('key') in supported and isinstance(e.get('value'),(int,float)) and e.get('script')=='items/global/effect.gd' and not e.get('custom_key') for e in effects):ids.append(path.parent.name)
    else:unsupported.append(dict(id=path.parent.name,effects=[e.get('key') for e in effects]))
def read(path):
    files.append(dict(path=path,sha256=hashlib.sha256((source.ROOT/path).read_bytes()).hexdigest()))
    return source.resource(path)
groups=source.ROOT.joinpath('singletons/item_service.gd').read_text()
groups={k:json.loads(v) for k,v in re.findall(r'"([a-z_]+)": (\[[^\n]+\])',groups)}
for id in ids:
    d=read(f'items/all/{id}/{id}_data.tres')
    effects=[read(p) for p in d['effects']]
    records.append(dict(id=id,title=id.replace('_',' ').title(),available=d.get('unlocked_by_default',False),tier=d.get('tier',0)+1,value=d['value'],tags=d.get('tags',[]),maxCount=d.get('max_nb',-1),
        groups=[k for k,v in groups.items() if 'item_'+id in v],
        effects=[dict(key=e['key'],value=e['value']) for e in effects]))
for id in ['well_rounded','brawler','crazy','ranger']:
    d=read(f'items/characters/{id}/{id}_data.tres');rows=[]
    for path in d['effects']:
        e=read(path);kind='stat';key=e['key'].removeprefix('stat_');extra=''
        if 'set_id' in e:kind='class';key=e['stat_displayed_name'].removeprefix('stat_');extra=e['set_id'].removeprefix('set_')
        elif 'stats_modified' in e:kind='gain';key=e['stat_displayed'].removeprefix('stat_')
        elif e.get('custom_key')=='starting_weapon':kind='weapon';key=e['key'].removeprefix('weapon_').removesuffix('_1')
        elif e['key']=='no_melee_weapons':kind='noMelee';key='melee_damage'
        rows.append(dict(kind=kind,key=key,value=e['value'],extra=extra))
    characters.append(dict(id=id,wantedTags=d['wanted_tags'],bannedItems=d['banned_items'],rows=rows))
data=dict(items=records,characters=characters)
(root/'Assets/Scrapshift/Content/Reference/item-design.json').write_text(json.dumps(data,indent=2)+'\n')
original=json.loads((root/'Assets/Scrapshift/Content/Reference/catalog.json').read_text())
differences=[]
for i in records:
    previous=next((a for a in original['items'] if a['id']==i['id']),None)
    if previous is None:continue
    for key in ['tier','value','effects']:
        if previous[key]!=i[key]:differences.append(dict(id=i['id'],field=key,before=previous[key],source=i[key]))
(root/'artifacts/item-design-audit.json').write_text(json.dumps(dict(supportedItems=len(records),items=records,characters=characters,
    initialDataDifferences=differences,sources=files,unsupported=unsupported,availableItems=sum(i['available'] for i in records),scope='All supported flat-stat items in base items/all; exact source data and metadata. Incomplete catalog does not reproduce full-game item distribution.'),indent=2)+'\n')
print(f'{len(records)} supported items audited; {len(differences)} value/tier/effect differences; character tag and effect metadata extracted.')

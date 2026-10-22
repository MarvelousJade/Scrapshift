"""Read-only, fresh Godot-resource comparison. Does not claim simulation equivalence."""
import json, re, hashlib
from pathlib import Path
import analyze_reference as source
root=Path(__file__).resolve().parents[1]
source.ROOT=Path('E:/UnityProjects/brotato-(3.x)')
profiles=json.loads((root/'Assets/Scrapshift/Content/Reference/starter-profiles.json').read_text())
items=json.loads((root/'Assets/Scrapshift/Content/Reference/item-design.json').read_text())['items']
diff=[];checks=0;sources={}
def read(path):
    sources[path]=hashlib.sha256((source.ROOT/path).read_bytes()).hexdigest()
    return source.resource(path)
def check(identity,key,actual,expected):
    global checks
    checks+=1
    same=abs(actual-expected)<1e-6 if isinstance(actual,(int,float)) and isinstance(expected,(int,float)) else actual==expected
    if not same:diff.append(dict(id=identity,field=key,actual=actual,reference=expected))
for c in profiles['characters']:
    d=read(f"items/characters/{c['id']}/{c['id']}_data.tres")
    for key,field in [('wantedTags','wanted_tags'),('bannedUpgrades','banned_upgrades'),('bannedItemGroups','banned_item_groups')]:check(c['id'],key,c.get(key,[]),d.get(field,[]))
    check(c['id'],'startingWeapons',c['startingWeapons'],[Path(p).parts[-3] for p in d.get('starting_weapons',[])])
    check(c['id'],'initiallyUnlocked',c['available'],d.get('unlocked_by_default',False))
    effects=[read(p) for p in d.get('effects',[])]
    granted=[e['key'].removeprefix('weapon_').rsplit('_',1) for e in effects if e.get('custom_key')=='starting_weapon']
    check(c['id'],'grantedWeaponCount',1 if c.get('bonusWeapon') else 0,len(granted))
    if granted:
        check(c['id'],'grantedWeapon',c.get('bonusWeapon',''),granted[0][0])
        check(c['id'],'grantedWeaponTier',c.get('bonusTier',1),int(granted[0][1]))
    for stat in c['stats']:
        matches=[e for e in effects if e.get('key','').removeprefix('stat_')==stat['key'] and not e.get('custom_key') and e.get('script')=='items/global/effect.gd']
        if matches:check(c['id'],'stat:'+stat['key'],stat['value'],sum(e.get('value',0) for e in matches))
weapon_paths={}
for p in (source.ROOT/'weapons').rglob('*_data*.tres'):
    if p.parent.name.isdigit():weapon_paths[(p.parent.parent.name,int(p.parent.name))]=p.relative_to(source.ROOT).as_posix()
fields={'damage':('damage',1,1),'interval':('cooldown',60,60),'range':('max_range',200,100),'pellets':('nb_projectiles',1,1),'piercing':('piercing',0,1),'crit':('crit_chance',0,1),'critMultiplier':('crit_damage',2,1),'knockback':('knockback',0,10),'lifesteal':('lifesteal',0,1),'spread':('projectile_spread',0,1),'projectileSpeed':('projectile_speed',3000,100),'bounce':('bounce',0,1),'pierceLoss':('piercing_dmg_reduction',.5,1),'bounceLoss':('bounce_dmg_reduction',.5,1)}
for w in profiles['equipment']:
    identity=w['id']+':'+str(w['tier']);path=weapon_paths.get((w['id'],w['tier']))
    if path is None:diff.append(dict(id=identity,field='source path',reference='missing'));continue
    d=read(path);stats=read(d['stats']);check(identity,'value',w['value'],d.get('value',10))
    for key,(field,default,scale) in fields.items():check(identity,key,w[key],stats.get(field,default)/scale)
    check(identity,'scaling',w['scaling'],[dict(key=k.removeprefix('stat_'),value=v) for k,v in stats.get('scaling_stats',[])])
scene=(source.ROOT/'singletons/item_service.tscn').read_text()
refs={i:p for p,i in re.findall(r'\[ext_resource path="res://([^"]+)" type="Resource" id=(\d+)\]',scene)}
registered=[read(refs[i]) for i in re.findall(r'ExtResource\( (\d+) \)',next(l for l in scene.splitlines() if l.startswith('items =')))]
lookup={d['my_id'].removeprefix('item_'):d for d in registered}
for item in items:
    d=lookup[item['id']]
    for key,expected in [('tier',d.get('tier',0)+1),('value',d['value']),('tags',d.get('tags',[])),('maxCount',d.get('max_nb',-1))]:check(item['id'],key,item[key],expected)
    effects=[read(p) for p in d.get('effects',[])]
    for effect in item['effects']:
        matches=[e for e in effects if e.get('key')==effect['key'] and not e.get('custom_key') and e.get('script')=='items/global/effect.gd']
        if matches:check(item['id'],'effect:'+effect['key'],effect['value'],sum(e.get('value',0) for e in matches))
report=dict(status='Passed' if not diff else 'Differences',scope='Fresh resource values: character lists/tags/base stats/granted starting weapons, weapon primary stats/scaling, item tier/price/tags/caps/scalar effects. Does not prove special-effect execution or RNG sequence equivalence.',characters=len(profiles['characters']),weaponIdentities=len({w['id'] for w in profiles['equipment']}),weaponTiers=len(profiles['equipment']),items=len(items),registeredItems=len(lookup),missingItems=sorted(set(lookup)-{i['id'] for i in items}),checks=checks,differences=diff,sources=sources)
(root/'artifacts/catalog-parity.json').write_text(json.dumps(report,indent=2)+'\n')
print(json.dumps({k:v for k,v in report.items() if k!='sources'},indent=2))

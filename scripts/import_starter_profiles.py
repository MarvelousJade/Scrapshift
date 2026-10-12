"""Extract the first four character profiles and their complete starting-weapon lists.
Reads the supplied Godot 3 reference. Does not copy its character or weapon art.
"""
import json, hashlib, shutil, re
from pathlib import Path
import analyze_reference as source

ROOT=Path('E:/UnityProjects/brotato-(3.x)')
source.ROOT=ROOT
PROJECT=Path(__file__).resolve().parents[1]
OUT=PROJECT/'Assets/Scrapshift/Content/Reference'
read=source.resource
profiles=[]; weapons={}; sets={}; provenance=[]
renames={'well_rounded':'Rookie','brawler':'Pugilist','crazy':'Hothead','ranger':'Sharpshot'}
descriptions={
 'well_rounded':'+5 Max HP\n+5% Speed\n+8 Harvesting',
 'brawler':'+50% Attack Speed with Unarmed weapons\nStart with 1 Fist\n+15% Dodge\n-50 Range\n-50 Ranged Damage',
 'crazy':'+100 Range with Precise weapons\n+25% Attack Speed\nStart with 1 Knife\n-30% Dodge\n-10 Engineering\n-10 Ranged Damage',
 'ranger':'+50 Range\nStart with 1 Pistol\nRanged Damage gains increased by 50%\nCannot equip melee weapons\nMax HP gains reduced by 25%'}

def stat(key): return key.removeprefix('stat_')
def track(path):
 p=ROOT/path
 provenance.append({'source':path,'sha256':hashlib.sha256(p.read_bytes()).hexdigest()})
def sound(path):
 name='starter_'+Path(path).name
 dest=OUT/name
 if not dest.exists():shutil.copyfile(ROOT/path,dest)
 track(path)
 return name[:-4]
def stats_data(s):
 return dict(damage=s.get('damage',1),interval=s.get('cooldown',60)/60,range=s.get('max_range',200)/100,
   pellets=s.get('nb_projectiles',1),piercing=s.get('piercing',0),spread=s.get('projectile_spread',0),
   crit=s.get('crit_chance',0),critMultiplier=s.get('crit_damage',2),knockback=s.get('knockback',0)/10,
   scaling=[dict(key=stat(k),value=v) for k,v in s.get('scaling_stats',[])],lifesteal=s.get('lifesteal',0),
   projectileSpeed=s.get('projectile_speed',3000)/100,pierceLoss=s.get('piercing_dmg_reduction',.5),
   bounce=s.get('bounce',0),bounceLoss=s.get('bounce_dmg_reduction',.5),
   reloadEvery=s.get('additional_cooldown_every_x_shots',-1),reloadMultiplier=s.get('additional_cooldown_multiplier',1),
   recoil=s.get('recoil',25)/100,recoilHalf=s.get('recoil_duration',.1))
def weapon(path):
 if path in weapons:return
 d=read(path);s=read(d['stats']);track(path);track(d['stats'])
 name=Path(path).parts[-3];tier=int(Path(path).parts[-2])
 result=dict(id=name,tier=tier,title=name.replace('_',' ').title(),value=d.get('value',10),
   melee='/melee/' in path,available=d.get('unlocked_by_default',False),
   setIds=[Path(p).parts[-2] for p in d.get('sets',[])],soundNames=[sound(p) for p in s.get('shooting_sounds',[])],
   **stats_data(s))
 traits=[]
 for path2 in d.get('effects',[]):
  e=read(path2);track(path2)
  trait=dict(key=e.get('key','').lower(),value=e.get('value',0),chance=e.get('chance',1),
   stat=stat(e.get('stat','')),amount=e.get('stat_nb',0),radius=e.get('scale',.75),
   per=e.get('nb_stat_scaled',0))
  if 'weapon_stats' in e:trait['projectile']=stats_data(read(e['weapon_stats']))
  if 'burning_data' in e:
   b=read(e['burning_data']);trait.update(burnDamage=b.get('damage',1),burnTicks=b.get('duration',3),burnInterval=b.get('interval',1),burnChance=b.get('chance',1),burnScaling=b.get('scaling_stats',[]));trait.pop('burnScaling')
   trait['burnScale'] = sum(v for k,v in b.get('scaling_stats',[]) if k=='stat_elemental_damage')
  if 'scene' in e and 'structures/' in e['scene']:
   trait['key']='garden' if '/garden/' in e['scene'] else 'landmine' if '/landmine/' in e['scene'] else 'turret'
   trait['projectile']=stats_data(read(e['stats']))
   trait['spawnInterval']=e.get('spawn_cooldown',-1)
   trait['animationSpeed']=e.get('shooting_animation_speed',1)
   if trait['key']=='landmine':trait['radius']=read(e['effects'][0]).get('scale',1)*1.4734
  if e.get('script')=='effects/weapons/one_shot_on_hit_effect.gd':trait['key']='one_shot_on_hit'
  if e.get('scaling_stat'):trait['stat']=stat(e['scaling_stat'])
  trait['sourceScript']=Path(e.get('script','')).stem
  trait['interval']=e.get('interval',0)
  trait['customKey']=e.get('custom_key','')
  trait['resetOnHit']=e.get('reset_on_hit',False)
  traits.append(trait)
 result['traits']=traits
 result['sourceAvailable']=result['available']
 unsupported=set()
 result['implemented']=name not in unsupported
 result['available']=result['available'] and result['implemented']
 weapons[path]=result
 for p in d.get('sets',[]):
  sid=Path(p).parts[-2]
  if sid not in sets:
   sd=read(p);sets[sid]=dict(id=sid,name=sid.title(),bonuses=[dict(count=n+2,effects=[dict(key=stat(read(e)['key']),value=read(e)['value']) for e in group]) for n,group in enumerate(sd['set_bonuses'])])
 upgrade=d.get('upgrades_into')
 if isinstance(upgrade,str):weapon(upgrade)

for cid,title in renames.items():
 path=f'items/characters/{cid}/{cid}_data.tres';d=read(path);track(path)
 effects=[read(e) for e in d['effects']]
 profile=dict(id=cid,title=title,description=descriptions[cid],startingWeapons=[Path(p).parts[-3] for p in d['starting_weapons']],
   stats=[],gains=[],bonusWeapon='',classSet='',classStat='',classValue=0,noMelee=False,
   bannedItemGroups=d.get('banned_item_groups',[]))
 for e in effects:
  if e.get('custom_key')=='starting_weapon':profile['bonusWeapon']=e['key'].removeprefix('weapon_').removesuffix('_1')
  elif 'stats_modified' in e:profile['gains'] += [dict(key=stat(k),value=e['value']) for k in e['stats_modified']]
  elif 'set_id' in e:profile.update(classSet=e['set_id'].removeprefix('set_'),classStat=e['stat_name'],classValue=e['value'])
  elif e['key']=='no_melee_weapons':profile['noMelee']=True
  elif e['key'].startswith('stat_'):profile['stats'].append(dict(key=stat(e['key']),value=e['value']))
 profiles.append(profile)
 for w in d['starting_weapons']:weapon(w)
# Registered base-game resources only; never infer tiers or include DLC folders.
scene=(ROOT/'singletons/item_service.tscn').read_text()
refs={i:p for p,i in re.findall(r'\[ext_resource path="res://([^"]+)" type="Resource" id=(\d+)\]',scene)}
line=next(l for l in scene.splitlines() if l.startswith('weapons ='))
for resource_id in re.findall(r'ExtResource\( (\d+) \)',line):weapon(refs[resource_id])
data=dict(characters=profiles,equipment=list(weapons.values()),sets=list(sets.values()),rosterSlots=50)
(OUT/'starter-profiles.json').write_text(json.dumps(data,indent=2)+'\n',encoding='utf-8')
(PROJECT/'artifacts/starter-provenance.json').write_text(json.dumps(dict(referenceRoot=str(ROOT),sources=list({p['source']:p for p in provenance}.values())),indent=2)+'\n')
print(f'{len(profiles)} characters; {len(weapons)} weapon tiers; {len(sets)} weapon sets')

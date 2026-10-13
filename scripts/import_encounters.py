"""Export encounter numbers and state definitions; never copy reference media/code."""
import json,re,functools
from pathlib import Path
import analyze_reference as source
ROOT=Path(__file__).resolve().parents[1]
source.ROOT=Path('E:/UnityProjects/brotato-(3.x)')
def value(v,refs):
 v=re.sub(r'ExtResource\(\s*(\d+)\s*\)',lambda m:json.dumps(refs[int(m[1])]),v.strip())
 try:return json.loads(v)
 except ValueError:return v
@functools.lru_cache(None)
def defaults(path):
 if not path or not path.endswith('.gd'):return {}
 s=(source.ROOT/path).read_text(encoding='utf8');d={}
 parent=re.search(r'^extends "res://([^"]+)"',s,re.M)
 if parent:d.update(defaults(parent[1]))
 for m in re.finditer(r'^export\s*\([^\n]*?\)\s*var\s+(\w+)(?:\s*:\s*\w+)?\s*(?:=|:=)\s*([^\n]+)',s,re.M):d[m[1]]=value(m[2],{})
 return d
@functools.lru_cache(None)
def scene(path):
 s=(source.ROOT/path).read_text(encoding='utf8');refs={int(i):p[6:] for p,i in re.findall(r'\[ext_resource path="([^"]+)"[^\]]* id=(\d+)\]',s)};nodes={}
 for m in re.finditer(r'^\[node (.*?)\]\s*(.*?)(?=^\[|\Z)',s,re.M|re.S):
  attrs=m[1];name=re.search(r'name="([^"]+)"',attrs)[1];parent=re.search(r'parent="([^"]+)"',attrs);key='' if not parent else (parent[1]+'/' if parent[1]!='.' else '')+name
  instance=re.search(r'instance=ExtResource\(\s*(\d+)\s*\)',attrs)
  if instance:
   for k,v in scene(refs[int(instance[1])]).items():nodes[(key+'/' if key and k else key)+k]=v.copy()
  fields={k:value(v,refs) for k,v in re.findall(r'^([\w/]+) = (.*)$',m[2],re.M)}
  base=nodes.get(key,{}).copy();script=fields.get('script',base.get('script',''))
  if script:base={**defaults(script),**base}
  base.update(fields);nodes[key]=base
 return nodes
def attack(n):
 script=n.get('script','');charge='charging_attack' in script;spawn='spawning_attack' in script
 return dict(enabled=any(k in script for k in ['shooting','spawning','charging']),charge=charge,spawn=spawn,projectile=Path(n.get('projectile_scene','')).stem,
  cooldown=n.get('cooldown',60)/60,initial=n.get('initial_cooldown',0)/60,jitter=n.get('max_cd_randomization',10)/60,
  windup=.4/max(.01,n.get('attack_anim_speed',1)),active=n.get('charge_duration',.1),chargeSpeed=n.get('charge_speed',500)/100,
  minRange=n.get('min_range',0)/100,maxRange=n.get('max_range',300)/100,
  projectileSpeed=n.get('projectile_speed',500)/100,damage=n.get('damage',1),growth=n.get('damage_increase_each_wave',1),
  count=n.get('nb_to_spawn',1) if spawn else n.get('number_projectiles',1),spread=n.get('projectile_spread',.1),
  spawnSpread=n.get('spawn_in_radius_around_unit',0)/100 if spawn else n.get('projectile_spawn_spread',0)/100,
  onTarget=n.get('spawn_projectiles_on_target',False),borders=n.get('projectile_spawn_only_on_borders',False),
  constant=n.get('constant_spread',False),randomDirection=n.get('random_direction',False),
  away=n.get('shoot_away_from_unit',False),inDirection=n.get('shoot_in_unit_direction',False),
  onTargetOnce=n.get('atleast_one_projectile_on_target',False),deleteOnDeath=n.get('delete_projectile_on_death',False),
  summon=Path(n.get('enemy_to_spawn','')).stem,longEvery=n.get('long_cooldown_every_x_shoots',0),longCooldown=n.get('long_cooldown',0)/60)
def movement(n):
 s=n.get('script','');kind='KeepRange' if 'stay_in_range' in s else 'AroundPlayer' if 'around_player' in s else 'Destination' if 'rand_pos' in s else 'Follow'
 return dict(movement=kind,spacing=n.get('target_range',300)/100,offset=n.get('range_around_player',300)/100)
entries=[]
paths=[source.resource(p.relative_to(source.ROOT))['scene'] for p in (source.ROOT/'entities/units/enemies/elites').glob('*_data.tres')]
paths+=['entities/units/enemies/lamprey/lamprey.tscn','entities/units/enemies/slasher_egg/slasher_egg.tscn','entities/units/enemies/tentacle/tentacle.tscn','entities/units/enemies/invoker/invoker.tscn']
extra_ids=['dire_junkie','bloated_spawner','corrupted_buffer','fly','healer','horned_fly','horned_spitter','infected_slasher_egg','mad_slasher']
paths += [f'entities/units/enemies/{eid}/{eid}.tscn' for eid in extra_ids]
for path in paths:
 n=scene(path);base=n[''];stats=source.resource(base['stats']);eid=Path(path).stem
 states=[dict(hp=1,time=0,attack=attack(n.get('AttackBehavior',{})),**movement(n.get('MovementBehavior',{})))]
 for key,node in n.items():
  if key.startswith('States/') and key.count('/')==1:
   states.append(dict(hp=node.get('hp_start',.5),time=node.get('timer_start',45),attack=attack(n.get(key+'/AttackBehavior',{})),**movement(n.get(key+'/MovementBehavior',{}))))
 art={'lamprey':'charger','slasher_egg':'spawner','tentacle':'slasher','invoker':'predator','butcher':'slasher','colossus':'bruiser','croc':'horned_charger','gargoyle':'spitter','mantis':'slasher','mom':'spawner','monk':'buffer','rhino':'horned_bruiser','dire_junkie':'junkie','bloated_spawner':'spawner','corrupted_buffer':'buffer','fly':'baby_alien','healer':'buffer','horned_fly':'fin_alien','horned_spitter':'spitter','infected_slasher_egg':'spawner','mad_slasher':'slasher'}[eid]
 row=dict(id=eid,art=art,role='Boss' if eid=='invoker' else 'Elite' if path in paths[:8] else 'Summon' if eid in ['slasher_egg','tentacle'] else 'Spawner' if eid=='bloated_spawner' else 'Buffer' if eid=='corrupted_buffer' else 'Healer' if eid=='healer' else 'Chase',
  health=stats.get('health',1),growth=stats.get('health_increase_each_wave',0),speed=stats.get('speed',0)/100,speedRandom=stats.get('speed_randomization',0)/100,
  damage=stats.get('damage',1),damageGrowth=stats.get('damage_increase_each_wave',0),armor=stats.get('armor',0),armorGrowth=stats.get('armor_increase_each_wave',0),
  resistance=stats.get('knockback_resistance',0),value=stats.get('value',1),states=states,
  extra=[attack(v) for k,v in n.items() if k.startswith('ChargingShoot') or k=='SpawningShootingBehavior'])
 entries.append(row)
diff=[]
for i in range(7):
 p=next((source.ROOT/'items/difficulties').glob(f'*/difficulty_{i}.tres'));d=source.resource(p.relative_to(source.ROOT));effects=[source.resource(e) for e in d['effects']]
 diff.append(dict(level=i,effects=[dict(key=e['key'],value=e['value']) for e in effects]))
out=ROOT/'Assets/Scrapshift/Resources/extended-encounters.json'
out.write_text(json.dumps(dict(enemies=entries,difficulties=diff),indent=2)+'\n',encoding='utf8')
print(len(entries),'encounters;',len(diff),'difficulty profiles')


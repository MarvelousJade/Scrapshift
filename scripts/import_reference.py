"""Import selected user-supplied reference data/media; preserve source hashes."""
from analyze_reference import resource, ROOT, PROJECT
from pathlib import Path
import json, shutil, hashlib, re

OUT=PROJECT/'Assets/Scrapshift/Content/Reference'
OUT.mkdir(exist_ok=True)
manifest=[]
def media(source, name=None):
    if not source: return ''
    p=ROOT/source
    dest=OUT/(name or p.name)
    shutil.copyfile(p,dest)
    manifest.append({'source':source,'asset':str(dest.relative_to(PROJECT)).replace('\\','/'),'sha256':hashlib.sha256(p.read_bytes()).hexdigest()})
    return dest.stem

report=json.loads((PROJECT/'artifacts/reference-analysis.json').read_text())
waves=[];used=set()
for wave in report['waves']:
    groups=[]
    for bucket, gs in [(0,wave['groups'])]+[(i+1,g) for i,g in enumerate(wave['conditionalGroups'])]:
        for g in gs:
            if not g.get('min_difficulty',0)<=0<=g.get('max_difficulty',999): continue
            if not g.get('min_wave',0)<=wave['number']<=g.get('max_wave',999): continue
            for u in g['units']: used.add(u['id'])
            groups.append(dict(start=g.get('spawn_timing',1),repeats=g.get('repeating',0),interval=g.get('repeating_interval',3),reduce=g.get('reduce_repeating_interval',0),minimum=g.get('min_repeating_interval',1),chance=g.get('spawn_chance',1),area=g.get('area',-1)/100,edge=g.get('spawn_edge_of_map',False),inset=g.get('spawn_dist_away_from_edges',0)/100,bucket=bucket,units=[dict(id=u['id'],min=u['min'],max=u['max'],chance=u['chance'],distance=u['extraDistance']/100) for u in g['units']]))
    waves.append(dict(duration=wave['duration'],cap=wave['maxEnemies'],groups=groups))
used.update(['slasher','predator'])
enemies=[]
roles={'charger':'Charge','horned_charger':'Charge','bruiser':'Charge','horned_bruiser':'Charge','spitter':'Shooter','horned_spitter':'Shooter','tentacle':'WanderShooter','junkie':'WanderShooter','mad_slasher':'WanderShooter','fly':'Fly','healer':'Healer','buffer':'Buffer','slasher_egg':'Egg','spawner':'Spawner','tree':'Tree','looter':'Looter','evil_mob':'Looter','pursuer':'Pursuer','predator':'Boss'}
for name in sorted(used):
    scene=report['units'].get(name,{}).get('scene',f'entities/units/enemies/{name}/{name}.tscn')
    sd=resource(scene);st=resource(sd.get('stats',{}))
    texture=next(iter((ROOT/Path(scene).parent).glob(name+'.png')),None)
    if texture is None: texture=ROOT/'entities/units/enemies/baby_alien/baby_alien.png'
    enemies.append(dict(id=name,role=roles.get(name,'Chase'),sprite=media(str(texture.relative_to(ROOT)).replace('\\','/')),health=st.get('health',10),growth=st.get('health_increase_each_wave',2),speed=st.get('speed',250)/100,speedRandom=st.get('speed_randomization',0)/100,damage=st.get('damage',1),damageGrowth=st.get('damage_increase_each_wave',.6),value=st.get('value',1),armor=st.get('armor',0),armorGrowth=st.get('armor_increase_each_wave',0),resistance=st.get('knockback_resistance',0),cooldown=sd.get('cooldown',120)/60,range=sd.get('max_range',700)/100,chargeSpeed=sd.get('charge_speed',500)/100,dropChance=st.get('base_drop_chance',.01)))

equipment=[]
for category,name in [('ranged','pistol'),('ranged','smg'),('ranged','double_barrel_shotgun'),('ranged','laser_gun'),('melee','knife'),('melee','spear')]:
    base=f'weapons/{category}/{name}'
    for tier in range(1,5):
        p=next((ROOT/base/str(tier)).glob('*_data.tres'));d=resource(str(p.relative_to(ROOT)).replace('\\','/'));s=resource(d['stats'])
        equipment.append(dict(id=name,tier=tier,title=name.replace('_',' ').title(),icon=media(d['icon']),value=d['value'],melee=category=='melee',damage=s['damage'],interval=s['cooldown']/60,range=s.get('max_range',200)/100,pellets=s.get('nb_projectiles',1),piercing=s.get('piercing',0),spread=s.get('projectile_spread',0),crit=s.get('crit_chance',0),knockback=s.get('knockback',10)/10))
supported={'stat_max_hp','stat_attack_speed','stat_percent_damage','stat_speed','stat_crit_chance','stat_armor','stat_hp_regeneration','stat_harvesting','stat_ranged_damage','stat_melee_damage','stat_dodge','stat_lifesteal','stat_range'}
items=[]
for p in sorted((ROOT/'items/all').glob('*/*_data.tres')):
    d=resource(str(p.relative_to(ROOT)).replace('\\','/')); effects=[resource(e) for e in d.get('effects',[])]
    if not effects or not all(e.get('key') in supported and isinstance(e.get('value'),(int,float)) for e in effects): continue
    if d.get('tier',0)>1 or not d.get('icon'): continue
    name=p.parent.name
    items.append(dict(id=name,title=name.replace('_',' ').title(),tier=d.get('tier',0)+1,icon=media(d['icon']),value=d.get('value',20),effects=[dict(key=e['key'],value=e['value']) for e in effects]))
    if len(items)>=24: break

audio={'Fire':'weapons/ranged/pistol/gun_pistol_shot_*.wav','Impact':'entities/units/unit/hurt_sounds/bullet_impact_body_flesh_*.wav','Kill':'entities/units/unit/hurt_sounds/punch_general_body_impact_*.wav','Critical':'entities/units/unit/crit_sounds/*.wav','Pickup':'items/materials/water_drop_drip_single_*.wav','LevelUp':'resources/sounds/level_up.wav','Install':'ui/sounds/buy.wav','WaveClear':'ui/sounds/end_wave.wav','Swing':'weapons/melee_sounds/whoosh_weapon_knife_swing_*.wav','Charge':'entities/birth/birth_begin_sound.wav','Hurt':'resources/sounds/zombie_voice_attack_grunt_01.wav'}
for cue,pattern in audio.items():
    for i,p in enumerate(sorted(ROOT.glob(pattern))): media(str(p.relative_to(ROOT)).replace('\\','/'),f'{cue}_{i+1:02}.wav')
for name in ['smg','double_barrel_shotgun','laser_gun']:
    for i,p in enumerate(sorted((ROOT/'weapons/ranged'/name).glob('*.wav'))):media(str(p.relative_to(ROOT)).replace('\\','/'),f'{name}_{i+1:02}.wav')
media('entities/birth/entity_birth.png','spawn_warning.png')
data=dict(waves=waves,enemies=enemies,equipment=equipment,items=items)
(OUT/'catalog.json').write_text(json.dumps(data,indent=2)+'\n')
(PROJECT/'artifacts/reference-media-provenance.json').write_text(json.dumps({'origin':'User-supplied Brotato reference, third-party art/audio, imported for this local study build. No original-art claim or redistribution license inferred.','files':list({m['asset']:m for m in manifest}.values())},indent=2)+'\n')
print(f'{len(enemies)} enemies, {len(equipment)} weapon tiers, {len(items)} items, {len(waves)} waves; {len(set(m["asset"] for m in manifest))} media files')

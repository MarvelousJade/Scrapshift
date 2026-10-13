"""Import source scene presentation and omitted weapon mechanics, with inherited node fields."""
import copy, hashlib, json, re, shutil, uuid
from pathlib import Path
import analyze_reference as src
ROOT=src.ROOT=Path('E:/UnityProjects/brotato-(3.x)')
PROJECT=Path(__file__).resolve().parents[1]
OUT=PROJECT/'Assets/Scrapshift/Resources/WeaponReference'
OUT.mkdir(parents=True,exist_ok=True)
provenance={}
def read(path):
    p=ROOT/path;provenance[path]=hashlib.sha256(p.read_bytes()).hexdigest();return p.read_text(encoding='utf-8-sig')
def asset(path):
    key=Path(path).stem+'_'+hashlib.sha1(path.encode()).hexdigest()[:8]
    dest=OUT/(key+Path(path).suffix);shutil.copyfile(ROOT/path,dest)
    provenance[path]=hashlib.sha256((ROOT/path).read_bytes()).hexdigest()
    if dest.suffix=='.png' and not Path(str(dest)+'.meta').exists():
        template=(PROJECT/'Assets/Scrapshift/Resources/PetGatlingUpper.png.meta').read_text()
        Path(str(dest)+'.meta').write_text(re.sub(r'guid: \w+','guid: '+uuid.uuid4().hex,template,count=1))
    if dest.suffix=='.png':
        meta=Path(str(dest)+'.meta');settings=meta.read_text()
        settings=re.sub(r'wrap([UVW]): \d+',r'wrap\1: 1',settings).replace('alphaIsTransparency: 0','alphaIsTransparency: 1')
        meta.write_text(settings)
    return 'WeaponReference/'+key
def fields(body,refs,subs):
    out={}
    for m in re.finditer(r'^([\w/]+) = (.*?)(?=^\w[\w/]* = |\Z)',body,re.M|re.S):
        v=m[2].strip()
        v=re.sub(r'ExtResource\(\s*(\d+)\s*\)',lambda x:json.dumps(refs[int(x[1])]),v)
        v=re.sub(r'SubResource\(\s*(\d+)\s*\)',lambda x:json.dumps(subs[int(x[1])]),v)
        v=re.sub(r'(?:Vector2|PoolRealArray)\(\s*([^)]*)\)',r'[\1]',v)
        v=re.sub(r'NodePath\("([^"]+)"\)',r'"\1"',v)
        try:out[m[1]]=json.loads(v)
        except ValueError:out[m[1]]=v
    return out
cache={}
def scene(path):
    if path in cache:return copy.deepcopy(cache[path])
    text=read(path);refs={int(i):p for p,i in re.findall(r'\[ext_resource path="res://([^"]+)"[^\]]* id=(\d+)\]',text)}
    subs={};nodes={}
    for h,b in re.findall(r'\[([^\]]+)\]\s*(.*?)(?=^\[|\Z)',text,re.M|re.S):
        if h.startswith('sub_resource'):
            sub=fields(b,refs,subs);sub['_type']=re.search(r'type="([^"]+)"',h)[1];subs[int(re.search(r'id=(\d+)',h)[1])]=sub
        elif h.startswith('node '):
            name=re.search(r'name="([^"]+)"',h)[1];parent=re.search(r'parent="([^"]+)"',h);inst=re.search(r'instance=ExtResource\( (\d+) \)',h)
            if parent is None:
                if inst:nodes=scene(refs[int(inst[1])])
                key='.'
            else:key=name if parent[1]=='.' else parent[1]+'/'+name
            nodes.setdefault(key,{}).update(fields(b,refs,subs))
    cache[path]=nodes;return copy.deepcopy(nodes)
projectiles={}
DEFAULT='projectiles/bullet/bullet_projectile.tscn'
def projectile(path):
    path=path or DEFAULT
    if path in projectiles:return path
    n=scene(path);root=n['.'];beam='particle_accelerator' in path
    sprite=n['Contents/Sprite' if beam else 'Sprite'];anim=n.get('AnimationPlayer',{});animation=anim.get('anims/'+anim.get('autoplay',''),{})
    frames=[asset(sprite['texture'])];times=[0]
    for k,v in animation.items():
        if k.endswith('/path') and str(v).endswith('Sprite:texture'):
            keys=animation[k.replace('/path','/keys')];frames=[asset(x) for x in keys['values']];times=keys['times']
    stop=0
    for k,v in animation.items():
        if k.endswith('/type') and v=='method':
            keys=animation[k.replace('/type','/keys')]
            if any(x.get('method')=='stop' for x in keys['values']):stop=keys['times'][-1]
    hit=n.get('Contents/Hitbox' if beam else 'Hitbox',{});col=n.get('Contents/Hitbox/Collision' if beam else 'Hitbox/Collision',{})
    shape=col.get('shape',{});hp=hit.get('position',[0,0]);cp=col.get('position',[0,0]);ext=shape.get('extents',[shape.get('radius',44)]*2)
    result=dict(id=path,frames=frames,times=times,loop=animation.get('loop',False),duration=animation.get('length',0),stop=stop,
        offset=sprite.get('position',[0,0]),scale=sprite.get('scale',[1,1]),spin=1500 if root.get('rotation_speed',0)!=0 else 0,
        radius=shape.get('radius',0)/100,half=[v/100 for v in ext],hitOffset=[(hp[0]+cp[0])/100,-(hp[1]+cp[1])/100],
        slow='taser' in path,delayed='delayed' in path,beam=beam,rail='rail_gun' in path,
        sinusoidal=[v/100 for v in root.get('sinusoidal_motion',[0,0])],sinSpeed=root.get('sinusoidal_motion_speed',[5,5]),
        stopDelay=root.get('stop_delay',0)/60)
    if beam:
        result.update(start=asset(n['StartContainer/StartProjectile']['texture']),end=asset(n['EndContainer/EndProjecitle']['texture']),stop=.2)
    if result['rail']:result['trail']=asset('particles/sprites/particle_28.png')
    if 'fireball_projectile' in path:
        read('particles/burning/torch_burning_particles.tscn')
        result['trail']=asset('particles/sprites/particle_fire_animation.png')
    projectiles[path]=result;return path
text=read('singletons/item_service.tscn');refs={i:p for p,i in re.findall(r'\[ext_resource path="res://([^"]+)" type="Resource" id=(\d+)\]',text)}
line=next(l for l in text.splitlines() if l.startswith('weapons ='));weapons=[]
for i in re.findall(r'ExtResource\( (\d+) \)',line):
    path=refs[i];d=src.resource(path);s=src.resource(d['stats']);read(path);read(d['stats']);wid=Path(path).parts[-3]
    melee='/melee/' in path;nodes=scene(d['scene']);h=nodes.get('Sprite/Hitbox',{});c=nodes.get('Sprite/Hitbox/Collision',{});shape=c.get('shape',{});p=h.get('position',[0,0]);q=c.get('position',[0,0])
    secondary=[];explosions=[]
    for index,effect in enumerate(d.get('effects',[])):
        e=src.resource(effect)
        if 'explosion_scene' in e:
            ex=scene(e['explosion_scene']);explosions.append(dict(index=index,texture=asset(ex['Sprite']['texture']),clips=[asset(v) for v in ex['.'].get('explosion_sounds',[])],soundDb=e.get('sound_db_mod',-10),radius=ex['Hitbox/Collision']['shape'].get('radius',147.34)/100))
        if 'weapon_stats' in e:secondary.append(dict(index=index,scene=projectile(src.resource(e['weapon_stats']).get('projectile_scene'))))
    weapons.append(dict(id=wid,tier=int(Path(path).parts[-2]),scene='' if melee else projectile(s.get('projectile_scene')),secondary=secondary,explosions=explosions,
        attackType=s.get('attack_type',0),alternate=s.get('alternate_attack_type',False),returnDamage=s.get('deal_dmg_on_return',False),
        accuracy=s.get('accuracy',1),soundDb=s.get('sound_db_mod',-5),speedWithRange=s.get('increase_projectile_speed_with_range',False),canBounce=s.get('can_bounce',True),
        half=[v/100 for v in shape.get('extents',[shape.get('radius',32)]*2)],hitOffset=[(p[0]+q[0])/100,-(p[1]+q[1])/100]))
unit=scene('entities/units/unit/unit.tscn')['.']
audio=[dict(id=key,clips=[asset(p) for p in unit[key+'_sounds']]) for key in ['hurt','crit','burn']]
data=dict(weapons=weapons,projectiles=list(projectiles.values()),audio=audio)
(OUT.parent/'weapon-reference.json').write_text(json.dumps(data,indent=2)+'\n')
(PROJECT/'artifacts/weapon-reference-provenance.json').write_text(json.dumps(provenance,indent=2)+'\n')
print(f'{len(weapons)} weapon profiles, {len(projectiles)} projectile scenes')


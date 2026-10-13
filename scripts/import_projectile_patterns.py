"""Read-only export of the twelve registered base-game projectile event patterns."""
import json,re
from pathlib import Path
from import_encounters import scene,source,ROOT

def vec(raw,default=(0,0),scale=1):
    if not isinstance(raw,str):return dict(x=default[0]*scale,y=default[1]*scale)
    n=[float(x.strip()) for x in raw[raw.index('(')+1:raw.index(')')].split(',')]
    return dict(x=n[0]*scale,y=n[1]*scale)
patterns=[]
registered=source.resource('singletons/zone_service.tscn')['bullets_hell']
for path in [source.ROOT/x for x in registered]:
    nodes=scene(str(path.relative_to(source.ROOT)).replace('\\','/'));base=nodes[''];gens=[]
    for key,n in nodes.items():
        if not key:continue
        gens.append(dict(anchor=n.get('anchor_position',0),offset=vec(n.get('offset_anchor_positon'),scale=.01),
            along=n.get('duplication_constant_along',0),copies=n.get('duplications',1),step=vec(n.get('duplication_offset_position'),scale=.01),
            delay=n.get('start_cool_down',0),delayStep=n.get('duplication_offset_start_cd',0),speed=n.get('speed_factor',1),
            rate=n.get('spawn_rate_factor',1),damage=n.get('damage_factor',1),direction=vec(n.get('direction'),(1,0)),
            random=vec(n.get('position_spawn_randomness'),scale=.01),sin=vec(n.get('sinusoidal_motion'),scale=.01),
            sinSpeed=vec(n.get('sinusoidal_motion_speed'),(5,5)),active=n.get('is_active',True)))
    patterns.append(dict(id=path.stem,rate=base.get('spawn_rate',1),speed=base.get('projectile_speed',300)/100,growth=base.get('projectile_damage_increase_each_wave',0),generators=gens))
(ROOT/'Assets/Scrapshift/Resources/projectile-patterns.json').write_text(json.dumps(dict(patterns=patterns),indent=2)+'\n',encoding='utf8')
print(len(patterns),'projectile event patterns')

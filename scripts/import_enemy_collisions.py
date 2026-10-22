from pathlib import Path
helper=Path(__file__).with_name('import_weapon_fidelity.py')
exec(helper.read_text().split("text=read('singletons/item_service.tscn')")[0])
rows=[]
for enemy in json.loads((PROJECT/'Assets/Scrapshift/Resources/extended-encounters.json').read_text())['enemies']:
    eid=enemy['id']; n=scene(f'entities/units/enemies/{eid}/{eid}.tscn')
    center=n['Animation/Sprite'].get('position',[0,0]); center=[center[0],center[1]-24]
    def offset(parent,child):
        a=n.get(parent,{}).get('position',[0,0]);b=n[child].get('position',[0,0])
        return [(a[0]+b[0]-center[0])/100,-(a[1]+b[1]-center[1])/100]
    radii=[n[k]['shape'].get('radius',20)/100 for k in ['Collision','Hurtbox/Collision','Hitbox/Collision']]
    rows.append(dict(id=eid,radii=radii,hurt=offset('Hurtbox','Hurtbox/Collision'),contact=offset('Hitbox','Hitbox/Collision'),
        body=offset('', 'Collision'),origin=[-center[0]/100,center[1]/100],bodyLayer=n['.'].get('collision_layer',1)))
(PROJECT/'Assets/Scrapshift/Resources/enemy-collisions.json').write_text(json.dumps(dict(entries=rows),indent=2)+'\n')
print('Imported',len(rows),'source collision profiles')

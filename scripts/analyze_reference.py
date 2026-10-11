"""Read-only inventory of the user-supplied Godot reference; no engine code execution."""
from pathlib import Path
import re, json

ROOT=Path('E:/UnityProjects/Brotato')
PROJECT=Path(__file__).resolve().parents[1]

def resource(path):
    if isinstance(path,dict): return path
    s=(ROOT/path).read_text(encoding='utf-8-sig')
    refs={int(i):p[6:] for p,i in re.findall(r'\[ext_resource path="([^"]+)"[^\]]* id=(\d+)\]',s)}
    subs={}
    def fields(text):
        result={}
        for m in re.finditer(r'^([\w/]+) = (.*?)(?=^\w[\w/]* = |^\[|\Z)',text,re.M|re.S):
            v=m[2].strip()
            v=re.sub(r'ExtResource\(\s*(\d+)\s*\)',lambda m:json.dumps(refs[int(m[1])]),v)
            v=re.sub(r'SubResource\(\s*(\d+)\s*\)',lambda m:json.dumps(subs.get(int(m[1]),{})),v)
            try: result[m[1]]=json.loads(v)
            except (ValueError,TypeError): result[m[1]]=v
        return result
    for m in re.finditer(r'\[sub_resource[^\]]* id=(\d+)\]\s*(.*?)(?=^\[|\Z)',s,re.M|re.S): subs[int(m[1])]=fields(m[2])
    if '[resource]' in s: return fields(s.split('[resource]',1)[1])
    result=fields(s);result['_references']=list(refs.values());return result

def main():
    zone=resource('zones/zone_1/zone_1_data.tres');units={};waves=[]
    def group(gp):
        gd=resource(gp).copy();result=[]
        for up in gd.get('wave_units_data',[]):
            ud=resource(up);scene=ud.get('unit_scene','');name=Path(scene).stem
            result.append(dict(id=name,min=ud.get('min_number',1),max=ud.get('max_number',1),chance=ud.get('spawn_chance',1),extraDistance=ud.get('additional_min_distance_from_player',0),entityType=ud.get('type',1)))
            if name and name not in units:
                sd=resource(scene);refs=sd['_references'];stats=resource(sd['stats']) if isinstance(sd.get('stats'),str) else {}
                units[name]={'scene':scene,'stats':stats,'scripts':[p for p in refs if p.endswith('.gd')],'textures':[p for p in refs if p.endswith('.png')], 'sceneFields':{k:v for k,v in sd.items() if k in ['cooldown','charge_duration','charge_speed','max_range','min_range','target_range','shoot_angle','nb_projectiles']}}
        gd.pop('script',None);gd.pop('wave_units_data',None);gd['units']=result;gd['source']=gp if isinstance(gp,str) else 'inline';return gd
    for n,path in enumerate(zone['waves_data'],1):
        wd=resource(path)
        groups=[group(g) for g in wd.get('groups_data',[])+zone['groups_data_in_all_waves']]
        choices=[[group(g) for g in bucket] for bucket in wd.get('conditional_groups_data',[])]
        waves.append({'number':n,'duration':wd.get('wave_duration',60),'maxEnemies':wd.get('max_enemies',100),'groups':groups,'conditionalGroups':choices})
    report={'referenceRoot':str(ROOT),'scope':'Crash Zone, base solo difficulty 0; data inventory, not a parity claim','waves':waves,'units':units}
    (PROJECT/'artifacts/reference-analysis.json').write_text(json.dumps(report,indent=2)+'\n')
    print('Durations:',[w['duration'] for w in waves]);print('Units:',len(units))
    for name,u in units.items(): print(name,{k:u['stats'].get(k) for k in ['health','speed','damage','value']},u['scripts'],u['sceneFields'])

if __name__=='__main__': main()

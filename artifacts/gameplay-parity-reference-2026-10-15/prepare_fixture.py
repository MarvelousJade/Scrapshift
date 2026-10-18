from pathlib import Path
p=Path(__file__).parent
out=p/'userdata'; (out/'logs').mkdir(parents=True,exist_ok=True)
changed=[]
for f in p.rglob('*'):
 if f.suffix not in ['.gd','.godot','.tscn','.tres']:continue
 t=f.read_text(encoding='utf-8-sig'); n=t.replace('user://',out.as_posix()+'/')
 if n!=t:f.write_text(n,encoding='utf-8');changed.append(str(f.relative_to(p)))
t=(p/'project.godot').read_text();t=t.replace('run/main_scene="res://pause.tscn"','run/main_scene="res://parity_bootstrap.tscn"');(p/'project.godot').write_text(t)
(p/'parity_bootstrap.tscn').write_text('[gd_scene format=2]\n[node name="ParityBootstrap" type="Node"]\n')
f=p/'entities/units/movement_behaviors/player_movement_behavior.gd';t=f.read_text();t=t.replace('func get_movement() -> Vector2:', 'func get_movement() -> Vector2:\n\tif get_node("/root/ParityFixture").active:\n\t\treturn get_node("/root/ParityFixture").movement()');f.write_text(t)
(p/'redirected-paths.json').write_text(__import__('json').dumps(changed,indent=2))

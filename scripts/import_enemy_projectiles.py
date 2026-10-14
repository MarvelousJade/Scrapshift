"""Import hostile projectile frames and their source animation metadata unchanged."""
from pathlib import Path
import json
helper=Path(__file__).with_name('import_weapon_fidelity.py')
exec(helper.read_text().split("text=read('singletons/item_service.tscn')")[0])
OUT=PROJECT/'Assets/Scrapshift/Resources/EnemyReference';OUT.mkdir(parents=True,exist_ok=True)
original_asset=asset
def asset(path):return original_asset(path).replace('WeaponReference/','EnemyReference/')
for p in ['bullet_enemy/enemy_projectile','slash/slash_projectile','slash/slash_projectile_2','pillar_enemy/pillar_projectile']:
 projectile('projectiles/'+p+'.tscn')
(OUT.parent/'enemy-projectiles.json').write_text(json.dumps(dict(projectiles=list(projectiles.values())),indent=2)+'\n')
(PROJECT/'artifacts/enemy-reference-provenance.json').write_text(json.dumps(provenance,indent=2)+'\n')
print(len(projectiles),'hostile projectile scenes')

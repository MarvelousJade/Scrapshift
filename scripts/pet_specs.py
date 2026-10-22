from pathlib import Path
import analyze_reference as source
from structure_specs import stats_data,structure,smoke_amount

def pet(e):
    scene=source.resource(e['scene']);kind=Path(e['scene']).stem
    unit=source.resource(scene['stats'])
    result=dict(kind=kind,speed=unit['speed']/100,health=unit['health'],healthGrowth=unit.get('health_increase_each_wave',0),
        structure=e.get('is_structure',False),doubleChance=e.get('double_chance',0),revive=e.get('revive_duration',2),zone=e.get('boost_zone_scale',1)*2)
    weapon=e.get('weapon_stats',scene.get('weapon_stats'))
    if weapon:result['stats']=stats_data(source.resource(weapon))
    if 'ranged_weapon_stats' in e:result['secondary']=stats_data(source.resource(e['ranged_weapon_stats']))
    if 'explosion_effect' in e:
        explosion=source.resource(e['explosion_effect']);result['secondary']=stats_data(source.resource(explosion['stats']));result['explosionRadius']=explosion['scale']*1.4734
        result['explosionSmokeAmount']=smoke_amount(explosion);result['explosionSoundDb']=explosion.get('sound_db_mod',-10)
    if 'landmine_effect_stat' in e:
        mine=source.resource(e['landmine_effect_stat']);result['mine']=structure(mine);result['mineInterval']=mine['spawn_cooldown']/60
    if 'burning_data' in e:
        b=source.resource(e['burning_data']);result['burn']=dict(burnDamage=b['damage'],burnTicks=b['duration'],burnChance=b['chance'],burnScale=b['scaling_stats'][0][1],burnStat='elemental_damage')
    return result

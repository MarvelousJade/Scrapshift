"""Convert source structure effects into the shared Unity structure schema; no artwork copied."""
import analyze_reference as source

def stats_data(s):
    return dict(damage=s.get('damage',1),interval=s.get('cooldown',60)/60,range=s.get('max_range',200)/100,
        pellets=s.get('nb_projectiles',1),piercing=s.get('piercing',0),spread=s.get('projectile_spread',0),
        crit=s.get('crit_chance',0),critMultiplier=s.get('crit_damage',2),knockback=s.get('knockback',0)/10,
        scaling=[dict(key=k.removeprefix('stat_'),value=v) for k,v in s.get('scaling_stats',[])],
        lifesteal=s.get('lifesteal',0),projectileSpeed=s.get('projectile_speed',3000)/100,
        pierceLoss=s.get('piercing_dmg_reduction',.5),bounce=s.get('bounce',0),bounceLoss=s.get('bounce_dmg_reduction',.5),
        reloadEvery=s.get('additional_cooldown_every_x_shots',-1),reloadMultiplier=s.get('additional_cooldown_multiplier',1),recoil=s.get('recoil',25)/100,recoilHalf=s.get('recoil_duration',.1))

def structure(e):
    scene=e['scene'];s=source.resource(e['stats'])
    kind=next((k for k in ['landmine','garden','wandering_bot','healing','tyler','flame','rocket','laser'] if '/'+k+'/' in scene),'turret')
    result=dict(kind=kind,stats=stats_data(s),spawnInterval=e.get('spawn_cooldown',-1),animationSpeed=e.get('shooting_animation_speed',1),accuracy=s.get('accuracy',1),explosionRadius=0,
        delayed='delayed_' in s.get('projectile_scene',''),healing=s.get('is_healing',False) and kind!='garden',pet=e.get('is_pet',False))
    for p in e.get('effects',[]):
        effect=source.resource(p)
        if 'burning_data' in effect:
            b=source.resource(effect['burning_data']);scaling=b.get('scaling_stats',[['stat_engineering',.33]])
            result['burn']=dict(key='effect_burning',burnDamage=b['damage'],burnChance=b['chance'],burnTicks=b['duration'],burnScale=scaling[0][1],burnStat=scaling[0][0].removeprefix('stat_'))
        if 'explosion_scene' in effect:result['explosionRadius']=effect.get('scale',1)*1.4734
    return result

"""Export the registered base roster; preserve source IDs and use our presentation names."""
import json
from pathlib import Path
import analyze_reference as source
from structure_specs import stats_data,smoke_amount

ROOT=Path(__file__).resolve().parents[1]
source.ROOT=Path('E:/UnityProjects/brotato-(3.x)')
archive=json.loads((ROOT/'artifacts/base-roster-unlocks.json').read_text(encoding='utf-8'))
path=ROOT/'Assets/Scrapshift/Content/Reference/starter-profiles.json'
data=json.loads(path.read_text(encoding='utf-8'))
aliases='Rookie|Pugilist|Hothead|Sharpshot|Arc Weaver|Bulkhead|Veteran|Probability Hacker|Splicer|Hybrid|Amplifier|Multicore|Feral|Mediator|Duelist|Vaultkeeper|Hemovore|Cultivator|Phasewalker|Courier|Broker|Fabricator|Surveyor|Surgeon|Tracker|Demolitionist|Arms Broker|Broadcaster|Augment|Devourer|Titan Hunter|Revenant|Intern|Forest Phantom|Angler|Monolith|Sovereign|Outlaw|Monolimb|Reactor|Sentinel|Pain Engine|Paladin|Blood Broker|Newborn|Drifter|Circuit Witch|Bloodline|Drone Shepherd|Glassheart'.split('|')
assert len(aliases)==len(archive['characters'])
profiles=[]
for i,c in enumerate(archive['characters']):
    cid=c['my_id'].removeprefix('character_')
    p=dict(id=cid,title=aliases[i],description='',available=c['unlocked_by_default'],
        startingWeapons=[Path(w).parts[-3] for w in c['starting_weapons']],
        wantedTags=c.get('wanted_tags',[]),bannedItems=[x.removeprefix('item_') for x in c.get('banned_items',[])],
        bannedItemGroups=c.get('banned_item_groups',[]),bannedUpgrades=c.get('banned_upgrades',[]),
        stats=[],gains=[],conditionalEffects=[],effectRows=[],startingItems=[],startingItemChoices=[],bonusWeapon='',bonusTier=1,
        classSet='',classStat='',classValue=0,noMelee=False,noRanged=False,weaponSlots=6,minTier=1,maxTier=4,canAttackMoving=True)
    for v in c.get('starting_items',[]):
        item=source.resource(v);p['startingItemChoices'].append(item['my_id'].removeprefix('item_'))
    for e in c['resolvedEffects']:
        key=e['key'];value=e['value'];custom=e.get('custom_key','');script=Path(e['script']).stem
        row=dict(kind='stat',key=key.removeprefix('stat_'),value=value,extra='')
        effect=None
        if custom=='starting_weapon':
            weapon,tier=key.removeprefix('weapon_').rsplit('_',1);p['bonusWeapon']=weapon;p['bonusTier']=int(tier);row.update(kind='weapon',key=weapon)
        elif custom=='starting_item':
            item=key.removeprefix('item_');p['startingItems'] += [item]*int(value);row.update(kind='item',key=item)
        elif 'stats_modified' in e:
            p['gains'] += [dict(key=k.removeprefix('stat_'),value=value) for k in e['stats_modified']]
            row.update(kind='gain',key=e['stat_displayed'].removeprefix('stat_'))
        elif 'set_id' in e:
            p.update(classSet=e['set_id'].removeprefix('set_'),classStat=e['stat_name'],classValue=value)
            row.update(kind='class',key=e['stat_displayed_name'].removeprefix('stat_'),extra=p['classSet'])
        elif script=='gain_stat_for_every_stat_effect':
            effect=dict(key=key,value=value,customKey='stat_links',stat=e['stat_scaled'],per=e['nb_stat_scaled'],permanentOnly=e.get('perm_stats_only',True))
        elif script=='chance_stat_damage_effect':effect=dict(key=key,value=value,customKey=custom,chance=e['chance']/100)
        elif script=='convert_stat_effect':effect=dict(key=key,value=value,customKey=custom,chance=e['pct_converted']/100,stat=e['to_stat'],per=e['to_value'])
        elif script=='item_exploding_effect':effect=dict(key=key,value=value,customKey=key,chance=e['chance'],radius=e['scale']*1.4734,projectile=stats_data(source.resource(e['stats'])),
            smokeAmount=smoke_amount(e),soundDb=e.get('sound_db_mod',-10),fleshExplosion='flesh_explosion' in e.get('explosion_scene',''))
        elif key=='weapon_slot':p['weaponSlots']=int(value if e['storage_method']==2 else 6+value)
        elif key in ['min_weapon_tier','max_weapon_tier']:p['minTier' if key.startswith('min') else 'maxTier']=int(value)+1
        elif key=='no_melee_weapons':p['noMelee']=True;row['kind']='noMelee'
        elif key=='no_ranged_weapons':p['noRanged']=True;row['kind']='noRanged'
        elif key=='can_attack_while_moving':p['canAttackMoving']=bool(value)
        elif custom=='DIE_IN_ONE_HIT':p['stats'].append(dict(key=key,value=value))
        elif custom:effect=dict(key=key,value=value,customKey=custom,per=e.get('value2',0))
        elif key:p['stats'].append(dict(key=key.removeprefix('stat_'),value=value))
        if effect:
            p['conditionalEffects'].append(effect);row.update(kind='effect',extra=str(len(p['conditionalEffects'])-1))
        if key:p['effectRows'].append(row)
    for item in c.get('starting_items',[]):
        p['effectRows'].append(dict(kind='itemChoice',key=source.resource(item)['my_id'].removeprefix('item_'),value=1,extra=''))
    profiles.append(p)
data['characters']=profiles
path.write_text(json.dumps(data,indent=2)+'\n',encoding='utf-8')
print(f'{len(profiles)} base profiles, {sum(p["available"] for p in profiles)} initially unlocked')

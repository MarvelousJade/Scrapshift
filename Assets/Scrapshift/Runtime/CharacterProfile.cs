using System;
using System.Linq;
using UnityEngine;

namespace Scrapshift
{
    public static class StarterChoices
    {
        public static string[] Available(ReferenceCatalog catalog,CharacterProfile profile)=>profile.startingWeapons
            .Select(id=>catalog.Weapon(id,1)).Where(w=>w!=null&&w.available&&profile.Allows(w))
            .OrderBy(w=>w.melee?0:1).Select(w=>w.id).ToArray();
    }
    [Serializable] public sealed class CharacterProfile
    {
        public string[] wantedTags,bannedItems;
        public CharacterEffectRow[] effectRows;
        public string id,title,description,bonusWeapon,classSet,classStat;
        public string[] startingWeapons,bannedItemGroups,startingItems;
        public StatEffect[] stats,gains;
        public float classValue;
        public bool noMelee;
        public string DefaultWeapon=>!string.IsNullOrEmpty(bonusWeapon)?bonusWeapon:"pistol";
        public float Gain(string stat)=>1+(Array.Find(gains,g=>g.key==stat)?.value??0)/100;
        public float WeaponBonus(Equipment w,string stat)=>classStat==stat&&w.HasSet(classSet)?classValue:0;
        public bool Allows(Equipment w)=>w!=null&&(!noMelee||!w.melee);
    }
    [Serializable] public sealed class WeaponTrait
    {
        public string key,stat,sourceScript,customKey,burnStat;
        public StructureSpec structure;
        public float interval;
        public float spawnInterval=-1,animationSpeed=1;
        public bool resetOnHit;
        [NonSerialized] ConditionalEffect conditional;
        public ConditionalEffect Conditional=>conditional??(conditional=new ConditionalEffect{key=key,value=value,customKey=customKey,interval=interval,resetOnHit=resetOnHit});
        public float value,chance=1,amount,radius,per,burnDamage,burnTicks,burnInterval=1,burnChance=1,burnScale;
        public ProjectileStats projectile;
    }
    // Leaf schema: projectile data must not recursively contain weapon traits in Unity serialization.
    [Serializable] public sealed class ProjectileStats
    {
        public float damage,interval,range,spread,crit,knockback,critMultiplier=2,lifesteal,projectileSpeed=30,
            pierceLoss=.5f,bounceLoss=.5f,reloadMultiplier=1,recoil=.25f,recoilHalf=.1f;
        public int pellets=1,piercing,bounce,reloadEvery=-1;
        public StatEffect[] scaling;
        [NonSerialized] Equipment cached;
        public Equipment AsEquipment()=>cached??(cached=new Equipment{damage=damage,interval=interval,range=range,
            spread=spread,crit=crit,knockback=knockback,critMultiplier=critMultiplier,lifesteal=lifesteal,
            projectileSpeed=projectileSpeed,pierceLoss=pierceLoss,bounceLoss=bounceLoss,reloadMultiplier=reloadMultiplier,
            recoil=recoil,recoilHalf=recoilHalf,pellets=pellets,piercing=piercing,bounce=bounce,reloadEvery=reloadEvery,scaling=scaling});
    }
    [Serializable] public sealed class SetBonusEntry { public int count; public StatEffect[] effects; }
    [Serializable] public sealed class SetEntry { public string id,name; public SetBonusEntry[] bonuses; }
    [Serializable] public sealed class StarterData
    {
        public CharacterProfile[] characters;
        public Equipment[] equipment;
        public SetEntry[] sets;
        public int rosterSlots;
    }
    public sealed partial class RunRules
    {
        public CharacterProfile Character {get;private set;}
        public bool CanEquip(Equipment w)=>w!=null&&w.available&&(Character==null||Character.Allows(w));
        public float WeaponAttackSpeed(Equipment w)=>(Stat("attack_speed")+(Character?.WeaponBonus(w,"attack_speed_mod")??0))/100;
        public float WeaponFireRate(Equipment w){float speed=WeaponAttackSpeed(w);return speed>=0?1+speed:1/(1-speed);}
        public float WeaponCooldown(Equipment w)=>Mathf.Max(MinimumWeaponCooldown-WeaponAnimationDuration(w),Mathf.Max(2,Mathf.Floor(Mathf.Max(2,w.interval*60)/WeaponFireRate(w)+.0001f))/60);
        public float WeaponAnimationDuration(Equipment w)=>WeaponMotion.ReferenceDuration(w,WeaponAttackSpeed(w),WeaponRange(w));
        public float WeaponDamage(Equipment w)
        {
            float damage=w.damage+NoHitWeaponBonus(w)+AddedWeaponScaling;
            if(w.scaling!=null)foreach(var s in w.scaling)damage+=(s.key=="max_hp"?MaxHealth:s.key=="levels"?Level:Stat(s.key))*s.value;
            else damage+=Stat(w.melee?"melee_damage":"ranged_damage");
            var stack=w.Trait("effect_weapon_stack");
            if(stack!=null)damage+=stack.value*Mathf.Max(0,Inventory.Count(a=>a.id==w.id)-1);
            return Mathf.Max(1,damage*DamageScale);
        }
        float EquipmentStat(string key)
        {
            float value=0;
            foreach(var w in Inventory)if(w.traits!=null)foreach(var t in w.traits)
                if(string.IsNullOrEmpty(t.customKey)&&(t.key=="stat_"+key||t.key==key))
                    value+=t.per>0?Mathf.Floor((1-Mathf.Clamp01(Health/MaxHealth))*100/t.per)*t.value:t.value;
            return value;
        }
        public void GainWeaponStat(string key,float amount)=>Add(key,amount);
    }
}

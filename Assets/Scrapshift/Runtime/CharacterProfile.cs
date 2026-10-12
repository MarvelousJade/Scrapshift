using System;
using System.Linq;
using UnityEngine;

namespace Scrapshift
{
    [Serializable] public sealed class CharacterProfile
    {
        public string id,title,description,bonusWeapon,classSet,classStat;
        public string[] startingWeapons,bannedItemGroups;
        public StatEffect[] stats,gains;
        public float classValue;
        public bool noMelee;
        public float Gain(string stat)=>1+(Array.Find(gains,g=>g.key==stat)?.value??0)/100;
        public float WeaponBonus(Equipment w,string stat)=>classStat==stat&&w.HasSet(classSet)?classValue:0;
        public bool Allows(Equipment w)=>w!=null&&(!noMelee||!w.melee);
    }
    [Serializable] public sealed class WeaponTrait
    {
        public string key,stat;
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
        public float WeaponFireRate(Equipment w)=>Mathf.Max(.1f,1+(Stat("attack_speed")+(Character?.WeaponBonus(w,"attack_speed_mod")??0))/100);
        public float WeaponDamage(Equipment w)
        {
            float damage=w.damage;
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
                if(t.key=="stat_"+key)
                    value+=t.per>0?Mathf.Floor((1-Mathf.Clamp01(Health/MaxHealth))*100/t.per)*t.value:t.value;
            return value;
        }
        public void GainWeaponStat(string key,float amount)=>Add(key,amount);
    }
}

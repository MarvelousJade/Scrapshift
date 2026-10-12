using System;
using UnityEngine;

namespace Scrapshift
{
    public sealed partial class RunRules
    {
        int freeShopRerolls,knownFreeRerolls;
        public int HitProtection {get;private set;}
        public bool LastHitProtected {get;private set;}
        public float BurningInterval=>Mathf.Max(.1f,1+(Stat("burning_cooldown_increase")-Stat("burning_cooldown_reduction"))/100);
        public float DodgeLimit=>Mathf.Clamp(Mathf.Max(60,Stat("dodge_cap")),0,100)/100;
        void BeginItemWave()
        {
            consumableGains.Clear();
            HitProtection=Math.Max(0,(int)Stat("hit_protection"));LastHitProtected=false;
            float percentage=100+Stat("hp_start_wave");
            float once=PermanentStat("hp_start_next_wave");
            if(once!=0){percentage=100+once;stats["hp_start_next_wave"]=0;}
            Health=Mathf.Max(1,(int)(MaxHealth*Mathf.Clamp01(percentage/100)));
            Scrap=Math.Max(0,Scrap+(int)(Scrap*Stat("gain_pct_gold_start_wave")/100));
        }
        void BeginShopRerolls()
        {freeShopRerolls=knownFreeRerolls=Math.Max(0,(int)Stat("free_rerolls"));}
        void UpdateShopRerolls()
        {
            int total=Math.Max(0,(int)Stat("free_rerolls"));
            if(total>knownFreeRerolls){freeShopRerolls+=total-knownFreeRerolls;knownFreeRerolls=total;}
        }
        public bool TryCriticalKillHeal(bool critical,double roll)
        {
            if(!critical||roll>=Math.Max(0,Math.Min(1,Stat("heal_on_crit_kill")/100.0)))return false;
            float before=Health;Heal(1);return Health>before;
        }
        public int MaterialDropValue(int baseValue,bool tree,double fractionalRoll)
        {
            float modifier=Stat("gold_drops")+Stat(tree?"neutral_gold_drops":"enemy_gold_drops");
            float value=Mathf.Max(baseValue*.5f,baseValue*(1+modifier/100));
            return (int)value+(fractionalRoll<value-(int)value?1:0);
        }
        public float DamageToBoss(float damage)=>Stat("damage_against_bosses")>0?(int)(damage*(1+Stat("damage_against_bosses")/100)):damage;
        public bool RollBurn(Equipment weapon,double roll,out float damage,out int ticks,bool structure=false)
        {
            var burn=weapon?.Trait("effect_burning");float globalDamage=0,globalChance=0,globalScale=0,globalTicks=0;
            foreach(var e in ConditionalEffects())if(e.customKey=="global_burn")
            {globalDamage+=e.value;globalChance+=e.chance;globalScale=Mathf.Max(globalScale,e.per);globalTicks=Mathf.Max(globalTicks,e.interval);}
            // A weapon with native burning keeps its chance/duration/scaling, gaining only global base damage.
            float chance=burn!=null?burn.burnChance:globalChance;
            ticks=(int)(burn!=null?burn.burnTicks:globalTicks);
            damage=Mathf.Max(1,((burn?.burnDamage??0)+globalDamage+Stat(burn!=null&&!string.IsNullOrEmpty(burn.burnStat)?burn.burnStat:"elemental_damage")*(burn!=null?burn.burnScale:globalScale))*(structure&&burn!=null?1:DamageScale));
            return ticks>0&&roll<chance;
        }
    }
}

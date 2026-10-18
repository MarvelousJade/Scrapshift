using System;
using UnityEngine;

namespace Scrapshift
{
    public sealed class BurnPayload
    {
        public float chance,damage;
        public int ticks,spread;
        public string stat;
        public bool Succeeds(double roll)=>ticks>0&&chance>0&&roll<=chance;
    }
    public sealed partial class RunRules
    {
        int freeShopRerolls,knownFreeRerolls;
        public int ExtraLootAliens {get;private set;}
        public int HitProtection {get;private set;}
        public bool LastHitProtected {get;private set;}
        public float BurningInterval=>Mathf.Max(.1f,1+(Stat("burning_cooldown_increase")-Stat("burning_cooldown_reduction"))/100);
        public float DodgeLimit=>Mathf.Clamp(Mathf.Max(60,Stat("dodge_cap")),0,100)/100;
        void BeginItemWave()
        {
            burningKills=projectileHits=0;runHealing=0;BeginCharacterWave();
            ExtraLootAliens=Math.Max(0,(int)PermanentStat("extra_loot_aliens_next_wave"));stats["extra_loot_aliens_next_wave"]=0;
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
            double chance=Math.Max(0,Math.Min(1,Stat("heal_on_crit_kill")/100.0));
            if(!critical||chance<=0||roll>chance)return false;
            float before=Health;Heal(1);return Health>before;
        }
        // RunData.add_gold from a weapon critical kill is immediate currency;
        // it never creates a pickup or awards pickup experience/effects.
        public void GainWeaponCriticalMaterials(int value)
        {if(Phase==RunPhase.Playing&&value>0)Scrap=Math.Max(0,Scrap+value);}
        public float MaterialDropAmount(int baseValue,bool tree=false)
        {
            float modifier=Stat("gold_drops")+Stat(tree?"neutral_gold_drops":"enemy_gold_drops");
            return Mathf.Max(baseValue*.5f,baseValue*(1+modifier/100));
        }
        public int MaterialDropValue(int baseValue,bool tree,double fractionalRoll)
        {
            float value=MaterialDropAmount(baseValue,tree);
            float fraction=value-(int)value;
            return (int)value+(fraction>0&&fractionalRoll<=fraction?1:0);
        }
        public float DamageToBoss(float damage)=>Stat("damage_against_bosses")>0?(int)(damage*(1+Stat("damage_against_bosses")/100)):damage;
        public bool RollBurn(Equipment weapon,double roll,out float damage,out int ticks,bool structure=false)
        {
            var payload=CaptureBurn(weapon,structure);damage=payload.damage;ticks=payload.ticks;
            return payload.Succeeds(roll);
        }
        public BurnPayload CaptureBurn(Equipment weapon,bool structure=false)
        {
            var burn=weapon?.Trait("effect_burning");float globalDamage=0,globalChance=0,globalScale=0,globalTicks=0;
            foreach(var e in ConditionalEffects())if(e.customKey=="global_burn")
            {globalDamage+=e.value;globalChance+=e.chance;globalScale=Mathf.Max(globalScale,e.per);globalTicks=Mathf.Max(globalTicks,e.interval);}
            // A weapon with native burning keeps its chance/duration/scaling, gaining only global base damage.
            float chance=burn!=null?burn.burnChance:globalChance;
            int ticks=(int)(burn!=null?burn.burnTicks:globalTicks);
            string stat=burn!=null&&!string.IsNullOrEmpty(burn.burnStat)?burn.burnStat:"elemental_damage";
            // WeaponService.init_burning_data truncates scaled base damage to
            // an int, then rounds the separately applied percentage modifier.
            int scaled=Math.Max(1,(int)((burn?.burnDamage??0)+globalDamage+Stat(stat)*(burn!=null?burn.burnScale:globalScale)));
            float percent=structure&&burn!=null?Stat("structure_percent_damage"):Stat("percent_damage");
            float damage=Math.Max(1,EnemyReferenceRules.SourceRound(scaled*(1+percent/100)));
            return new BurnPayload{chance=chance,damage=damage,ticks=ticks,spread=(int)Stat("burning_spread"),stat=stat};
        }
    }
}

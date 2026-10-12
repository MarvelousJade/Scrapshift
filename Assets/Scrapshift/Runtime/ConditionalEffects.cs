using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Scrapshift
{
    [Serializable] public sealed class ConditionalEffect
    {
        public string key,customKey;
        public string stat;
        public float per;
        public bool permanentOnly;
        public float value,interval=1;
        public bool resetOnHit;
        public string Description=>ItemEntry.FormatStat(key,value)+Suffix;
        public string Suffix
        {
            get
            {
                switch(customKey)
                {
                    case "temp_stats_while_not_moving":return " while standing still";
                    case "temp_stats_while_moving":return " while moving";
                    case "unique_weapon_effects":return " for each different weapon you have";
                    case "additional_weapon_effects":return " for every weapon you have";
                    case "temp_stats_on_hit":return " when you take damage, until the end of the wave";
                    case "stats_end_of_wave":return " at the end of every wave";
                    case "stats_on_level_up":return " when you level up";
                    case "temp_stats_per_interval":return $" every {interval:0.##} seconds until the end of the wave"+(resetOnHit?" (reset when you take damage)":"");
                    case "stat_links":return $" for every {per:0.##} {ItemEntry.StatName(stat)}";
                    case "stat_cap":return " capped at your current value";
                    default:return "";
                }
            }
        }
    }
    public sealed partial class RunRules
    {
        readonly Dictionary<string,float> temporaryStats=new Dictionary<string,float>();
        readonly Dictionary<string,float> resetOnHitStats=new Dictionary<string,float>();
        double effectElapsed;
        int effectSeconds;
        int noHitSeconds;
        public float NoHitWeaponBonus(Equipment weapon)
        {var t=weapon.Trait("effect_no_hit_boost");return t==null||t.interval<=0?0:(int)(noHitSeconds/t.interval)*t.value;}
        bool moving;
        // Source uses input movement, not displacement: pushing against an edge still counts.
        public void SetMoving(bool value){if(Phase==RunPhase.Playing)moving=value;}
        IEnumerable<ConditionalEffect> ConditionalEffects()
        {
            foreach(var item in Items)if(item.conditionalEffects!=null)foreach(var effect in item.conditionalEffects)yield return effect;
            foreach(var weapon in Inventory)if(weapon.traits!=null)foreach(var trait in weapon.traits)
                if(!string.IsNullOrEmpty(trait.customKey))yield return trait.Conditional;
        }
        float ConditionalStat(string name)
        {
            float result=temporaryStats.TryGetValue(name,out var value)?value:0;
            foreach(var effect in ConditionalEffects())
            {
                if(effect.key.Replace("stat_","")!=name)continue;
                switch(effect.customKey)
                {
                    case "unique_weapon_effects":result+=effect.value*Inventory.Select(w=>w.id).Distinct().Count();break;
                    case "additional_weapon_effects":result+=effect.value*Inventory.Count;break;
                    case "temp_stats_while_not_moving":if((Phase==RunPhase.Playing||Phase==RunPhase.Paused)&&!moving)result+=effect.value;break;
                    case "temp_stats_while_moving":if((Phase==RunPhase.Playing||Phase==RunPhase.Paused)&&moving)result+=effect.value;break;
                }
            }
            return result;
        }
        float LinkedStat(string name)
        {
            float value=0;
            foreach(var effect in ConditionalEffects())if(effect.customKey=="stat_links"&&effect.key.Replace("stat_","")==name&&effect.per!=0)
                value+=(int)(effect.value*LinkSource(effect.stat,effect.permanentOnly)/effect.per);
            return value;
        }
        public int LivingEnemies,LivingTrees,BurningEnemies,StructureCount,PetCount;
        float LinkSource(string source,bool permanent)
        {
            switch(source)
            {
                case "materials":return Scrap;
                case "living_enemy":return LivingEnemies;
                case "living_tree":return LivingTrees;
                case "burning_enemy":return BurningEnemies;
                case "structure":return StructureCount;
                case "pet":return PetCount;
                case "different_item":return Items.Select(i=>i.id).Distinct().Count();
                case "common_item":return Items.Where(i=>i.tier==1).Select(i=>i.id).Distinct().Count();
                case "legendary_item":return Items.Where(i=>i.tier==4).Select(i=>i.id).Distinct().Count();
                case "percent_player_missing_health":return (1-Health/MaxHealth)*100;
            }
            if(source.StartsWith("item_"))return Items.Count(i=>"item_"+i.id==source);
            string key=source.Replace("stat_","");
            // Source stat links intentionally exclude other linked bonuses, avoiding cycles.
            float value=(PermanentStat(key)+WeaponSets.Bonus(Inventory,key)+EquipmentStat(key))*(Character?.Gain(key)??1);
            if(key=="max_hp")value+=10;
            if(!permanent)value+=ConditionalStat(key);
            return value;
        }
        void ResetConditionalEffects()
        {temporaryStats.Clear();resetOnHitStats.Clear();effectElapsed=0;effectSeconds=0;noHitSeconds=0;moving=false;}
        void AddTemporary(string key,float value)
        {key=key.Replace("stat_","");temporaryStats.TryGetValue(key,out var current);temporaryStats[key]=current+value;}
        void PermanentTrigger(string trigger)
        {foreach(var effect in ConditionalEffects())if(effect.customKey==trigger)Add(effect.key.Replace("stat_",""),effect.value);}
        void DamageTriggers()
        {
            noHitSeconds=0;
            foreach(var effect in ConditionalEffects())if(effect.customKey=="temp_stats_on_hit")AddTemporary(effect.key,effect.value);
            foreach(var entry in resetOnHitStats)AddTemporary(entry.Key,-entry.Value);
            resetOnHitStats.Clear();
        }
        void TickConditionalEffects(float seconds)
        {
            double remaining=seconds;
            while(remaining>0&&Phase==RunPhase.Playing)
            {
                double step=Math.Min(remaining,1-effectElapsed);
                TickSurvival((float)step);effectElapsed+=step;remaining-=step;
                if(effectElapsed<1-1e-7)continue;
                effectElapsed=0;effectSeconds++;noHitSeconds++;
                foreach(var effect in ConditionalEffects())
                {
                    if(effect.customKey!="temp_stats_per_interval"||effect.interval<=0||effectSeconds%effect.interval!=0)continue;
                    AddTemporary(effect.key,effect.value);
                    if(effect.resetOnHit){resetOnHitStats.TryGetValue(effect.key,out var amount);resetOnHitStats[effect.key]=amount+effect.value;}
                }
                float loss=Stat("lose_hp_per_second");
                if(loss>0){Health=Math.Max(0,Health-loss);DamageTriggers();if(Health==0)Phase=RunPhase.Lost;}
            }
        }
    }
}

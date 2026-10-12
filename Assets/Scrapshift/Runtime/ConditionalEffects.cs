using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Scrapshift
{
    [Serializable] public sealed class ConditionalEffect
    {
        public string key,customKey;
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
        void ResetConditionalEffects()
        {temporaryStats.Clear();resetOnHitStats.Clear();effectElapsed=0;effectSeconds=0;moving=false;}
        void AddTemporary(string key,float value)
        {key=key.Replace("stat_","");temporaryStats.TryGetValue(key,out var current);temporaryStats[key]=current+value;}
        void PermanentTrigger(string trigger)
        {foreach(var effect in ConditionalEffects())if(effect.customKey==trigger)Add(effect.key.Replace("stat_",""),effect.value);}
        void DamageTriggers()
        {
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
                effectElapsed=0;effectSeconds++;
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

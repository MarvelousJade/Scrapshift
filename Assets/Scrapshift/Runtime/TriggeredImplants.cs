using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Scrapshift
{
    public sealed partial class RunRules
    {
        readonly Dictionary<string,float> consumableGains=new Dictionary<string,float>();
        float EffectTotal(string kind)=>ConditionalEffects().Where(e=>e.customKey==kind).Sum(e=>e.value);
        void DodgeTriggers()
        {foreach(var e in ConditionalEffects())if(e.customKey=="heal_on_dodge"&&random.NextDouble()<e.chance)Heal(e.value);}
        void ConsumableFullHealthTriggers()
        {
            if(Health<MaxHealth)return;
            foreach(var group in ConditionalEffects().Where(e=>e.customKey=="consumable_stats_while_max").GroupBy(e=>e.key))
            {
                consumableGains.TryGetValue(group.Key,out float gained);
                float limit=group.Sum(e=>e.per),amount=group.Sum(e=>e.value);
                if(limit>0&&gained>=limit)continue;
                Add(group.Key.Replace("stat_",""),amount);consumableGains[group.Key]=gained+amount;
            }
        }
        void OnItemStatAdded(string key,float value)
        {
            if(value<=0)return;
            foreach(var e in ConditionalEffects())if(e.customKey=="gain_stat_for_equipped_item_with_stat"&&e.stat==key)Add(e.key.Replace("stat_",""),e.value);
        }
        public void CriticalKillMaterials(bool critical)
        {if(Phase==RunPhase.Playing&&critical)foreach(var e in ConditionalEffects())if(e.customKey=="gold_on_crit_kill"&&random.NextDouble()<e.value/100.0)Scrap++;}
        public float RegenerationHealMultiplier=>1+ConditionalEffects().Where(e=>e.customKey=="hp_regen_bonus"&&Health<MaxHealth*e.per/100).Sum(e=>e.value);
        public float ItemSpeedReduction(float previous,float baseSpeed)
        {
            var effects=ConditionalEffects().Where(e=>e.customKey=="remove_speed").ToArray();
            if(effects.Length==0||previous>=baseSpeed*effects.Max(e=>e.per)/100)return previous;
            // Source checks the cap before subtraction; stacked teeth can overshoot it on the final hit.
            return previous+baseSpeed*effects.Sum(e=>e.value)/100;
        }
        public float CriticalCurrentHealthDamage(float health,bool boss)=>Mathf.Floor(health*EffectTotal("giant_crit_damage")/(boss?1000:100));
        public float AddedWeaponScaling=>ConditionalEffects().Where(e=>e.customKey=="weapon_scaling_stats").Sum(e=>Stat(e.key.Replace("stat_",""))*e.value/100);
        public float MinimumWeaponCooldown=>ConditionalEffects().Where(e=>e.customKey=="minimum_weapon_cooldowns").Select(e=>e.value/60).DefaultIfEmpty(0).Max();
        public float StructureAttackSpeed=>Stat("structure_attack_speed")+ConditionalEffects().Where(e=>e.customKey=="structures_cooldown_reduction").Sum(e=>Stat(e.key.Replace("stat_",""))*e.value/100);
        public float StructureCooldown(float interval)
        {float speed=StructureAttackSpeed/100;return Mathf.Max(2,Mathf.Floor(interval*60*(speed<0?1-speed:1/(1+speed))))/60;}
    }
}

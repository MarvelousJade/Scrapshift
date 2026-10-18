using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Scrapshift
{
    public sealed partial class RunRules
    {
        // The reference stores one counter per effect entry, including duplicate copies of an item.
        readonly Dictionary<int,float> consumableGains=new Dictionary<int,float>();
        void DodgeTriggers()
        {
            float healing=0;
            foreach(var e in ConditionalEffects())
                if(e.customKey=="heal_on_dodge"&&random.NextDouble()<e.chance)healing+=e.value;
            // player.gd heals the successful Adrenaline entries together, emitting
            // one healing event (and therefore one Lich attack) for the dodge.
            if(healing>0)Heal(healing);
            foreach(var e in ConditionalEffects())if(e.customKey=="temp_stats_on_dodge")AddTemporary(e.key,e.value);
        }
        void ConsumableFullHealthTriggers()
        {
            if(Health<MaxHealth)return;
            int effectIndex=0;
            foreach(var effect in ConditionalEffects().Where(e=>e.customKey=="consumable_stats_while_max"))
            {
                consumableGains.TryGetValue(effectIndex,out float gained);
                if(effect.per<=0||gained<effect.per)
                {
                    Add(effect.key.Replace("stat_",""),effect.value);
                    if(effect.per>0)consumableGains[effectIndex]=gained+effect.value;
                }
                effectIndex++;
            }
        }
        void OnItemStatAdded(string key,float value)
        {
            if(value<=0)return;
            foreach(var e in ConditionalEffects())if(e.customKey=="gain_stat_for_equipped_item_with_stat"&&e.stat==key)Add(e.key.Replace("stat_",""),e.value);
        }
        public void CriticalKillMaterials(bool critical)
        {if(Phase==RunPhase.Playing&&critical)foreach(var e in ConditionalEffects())if(e.customKey=="gold_on_crit_kill"&&e.value>0&&random.NextDouble()<=e.value/100.0)Scrap++;}
        public float RegenerationHealMultiplier=>1+HealingDrones+ConditionalEffects().Where(e=>e.customKey=="hp_regen_bonus"&&Health<MaxHealth*e.per/100).Sum(e=>e.value);
        public float ItemSpeedReduction(float previous,float baseSpeed)
        {
            var effects=ConditionalEffects().Where(e=>e.customKey=="remove_speed").ToArray();
            if(effects.Length==0)return previous;
            // LiveStats.speed is an integer in reference pixels. Each hit's
            // subtraction assigns back to that int before the next hit.
            int maximum=EnemyReferenceRules.SourceRound(baseSpeed*100);
            int current=EnemyReferenceRules.SourceRound((baseSpeed-previous)*100);
            if(current<=maximum*(1-effects.Max(e=>e.per)/100))return previous;
            // Source checks the cap before subtraction; stacked teeth can overshoot it on the final hit.
            int remaining=(int)(current-maximum*effects.Sum(e=>e.value)/100);
            return (maximum-remaining)/100f;
        }
        // enemy.gd truncates each Giant Belt entry before adding it to the hit.
        public float CriticalCurrentHealthDamage(float health,bool boss)=>Effects("giant_crit_damage").Sum(e=>Mathf.Floor(health*e.value/(boss?1000:100)));
        public float AddedWeaponScaling=>ConditionalEffects().Where(e=>e.customKey=="weapon_scaling_stats").Sum(e=>Stat(e.key.Replace("stat_",""))*e.value/100);
        public float MinimumWeaponCooldown=>ConditionalEffects().Where(e=>e.customKey=="minimum_weapon_cooldowns").Select(e=>e.value/60).DefaultIfEmpty(0).Max();
        public float StructureAttackSpeed=>Stat("structure_attack_speed")+ConditionalEffects().Where(e=>e.customKey=="structures_cooldown_reduction").Sum(e=>Stat(e.key.Replace("stat_",""))*e.value/100);
        public float StructureCooldown(float interval)
        {float speed=StructureAttackSpeed/100;return Mathf.Max(2,Mathf.Floor(interval*60*(speed<0?1-speed:1/(1+speed))))/60;}
    }
}

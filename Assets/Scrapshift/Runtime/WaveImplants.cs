using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Scrapshift
{
    public sealed partial class RunRules
    {
        readonly Dictionary<string,float> nextWaveStats=new Dictionary<string,float>();
        readonly List<int> burnKillCounts=new List<int>();
        public event Action<float> RandomDamageRequested;
        void QueueNextWaveEffects(ItemEntry item)
        {
            if(item.conditionalEffects==null)return;
            foreach(var e in item.conditionalEffects)if(e.customKey=="stats_next_wave")
            {nextWaveStats.TryGetValue(e.key,out float value);nextWaveStats[e.key]=value+e.value;}
        }
        void CancelQueuedItemEffects(ItemEntry item)
        {
            // Source unapply only changes entries still present in the next-wave
            // queue. It does not remove a temporary bonus already active this wave.
            if(item.id=="bait"&&queuedBaits>0)queuedBaits--;
            foreach(var effect in item.conditionalEffects??Array.Empty<ConditionalEffect>())
                if(effect.customKey=="stats_next_wave"&&nextWaveStats.TryGetValue(effect.key,out float value))
                {
                    value-=effect.value;
                    if(value==0)nextWaveStats.Remove(effect.key);else nextWaveStats[effect.key]=value;
                }
        }
        void ApplyNextWaveEffects()
        {foreach(var e in nextWaveStats)AddTemporary(e.Key,e.Value);nextWaveStats.Clear();burnKillCounts.Clear();}
        void UpgradeWeaponImplants()
        {
            foreach(var e in ConditionalEffects().Where(e=>e.customKey=="upgrade_random_weapon").ToArray())
            {
                var candidates=Inventory.Where(w=>w.tier<MaximumWeaponTier&&catalog.Weapon(w.id,w.tier+1)!=null).ToArray();
                if(candidates.Length==0){Add(e.key.Replace("stat_",""),e.value);continue;}
                var chosen=candidates[random.Next(candidates.Length)];
                // base_shop._combine_weapon(is_upgrade=true) removes the first
                // equal copy through RunData.remove_weapon, then appends its upgrade.
                int index=Inventory.FindIndex(w=>w.id==chosen.id&&w.tier==chosen.tier);
                Inventory.RemoveAt(index);Inventory.Add(catalog.Weapon(chosen.id,chosen.tier+1));
            }
        }
        public void KillEffects(bool burning)
        {
            if(Phase!=RunPhase.Playing)return;
            if(burning)
            {
                int index=0;
                foreach(var e in ConditionalEffects())if(e.customKey=="gain_stat_for_killed_enemies_while_burning")
                {
                    while(burnKillCounts.Count<=index)burnKillCounts.Add(0);
                    int kills=++burnKillCounts[index++];
                    if(e.value>0&&kills%e.value==0&&kills/e.value<=e.limit)Add(e.key.Replace("stat_",""),e.per);
                }
            }
            RandomDamageTrigger("dmg_when_death");
        }
        void RandomDamageTrigger(string trigger)
        {
            float total=0;
            foreach(var e in ConditionalEffects())if(e.customKey==trigger&&random.NextDouble()<e.chance)
                total+=EnemyReferenceRules.SourceRound(Mathf.Floor(Mathf.Max(1,e.value/100*Stat(e.key.Replace("stat_",""))))*DamageScale);
            if(total>0)RandomDamageRequested?.Invoke(total);
        }
    }
}

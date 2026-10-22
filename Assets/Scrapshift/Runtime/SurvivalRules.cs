using System;
using UnityEngine;

namespace Scrapshift
{
    public sealed partial class RunRules
    {
        double regenElapsed,lifeStealCooldown;
        bool regenRunning;
        public static double RegenerationInterval(float stat)
        {int value=(int)stat;return value<=0?99:5.0/(1+(value-1)/2.25);}
        void ResetSurvival(){regenElapsed=lifeStealCooldown=0;regenRunning=false;}
        void BeginRegenerationAfterDamage()
        {if(!regenRunning){regenElapsed=0;regenRunning=true;}}

        void TickSurvival(float dt)
        {
            lifeStealCooldown=Math.Max(0,lifeStealCooldown-dt);
            float regen=Stat("hp_regeneration"),torture=Stat("torture");
            if(torture<=0&&regen<=0){regenElapsed=0;regenRunning=false;return;}
            if(!regenRunning)
            {
                if(Health>=MaxHealth)return;
                BeginRegenerationAfterDamage();
            }
            regenElapsed+=dt;
            double interval=torture>0?1:RegenerationInterval(regen);
            int ticks=(int)Math.Floor((regenElapsed+1e-7)/interval);
            if(ticks>0)
            {
                regenElapsed-=ticks*interval;
                // FixedTimer reports all loops together. Potion's HP threshold is
                // sampled once, and the player emits one healing event for the batch.
                int healing=(int)((torture>0?torture:RegenerationHealMultiplier)*ticks);
                Heal(Math.Max(0,Math.Min(healing,MaxHealth-Health)),torture>0);
                if(Health>=MaxHealth)regenRunning=false;
            }
        }

        public bool TryLifeSteal(double roll,float weaponBonus=0)
        {
            if(Phase!=RunPhase.Playing || lifeStealCooldown>1e-7 || roll>=Mathf.Clamp01(Stat("lifesteal")/100+weaponBonus))return false;
            lifeStealCooldown=.1;
            float before=Health;Heal(HealingDrones>0?2:1);return Health>before;
        }
    }
}

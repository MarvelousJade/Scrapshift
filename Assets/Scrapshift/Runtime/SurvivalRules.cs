using System;
using UnityEngine;

namespace Scrapshift
{
    public sealed partial class RunRules
    {
        double regenElapsed,lifeStealCooldown;
        public static double RegenerationInterval(float stat)=>stat<=0?double.PositiveInfinity:5.0/(1+(stat-1)/2.25);

        void TickSurvival(float dt)
        {
            lifeStealCooldown=Math.Max(0,lifeStealCooldown-dt);
            float regen=Stat("hp_regeneration"),torture=Stat("torture");
            if(torture<=0&&regen<=0 || Health>=MaxHealth){regenElapsed=0;return;}
            regenElapsed+=dt;
            double interval=torture>0?1:RegenerationInterval(regen);
            int ticks=(int)Math.Floor((regenElapsed+1e-7)/interval);
            if(ticks>0){regenElapsed-=ticks*interval;for(int i=0;i<ticks;i++)Heal(torture>0?torture:RegenerationHealMultiplier,torture>0);}
        }

        public bool TryLifeSteal(double roll,float weaponBonus=0)
        {
            if(Phase!=RunPhase.Playing || lifeStealCooldown>1e-7 || roll>=Mathf.Clamp01(Stat("lifesteal")/100+weaponBonus))return false;
            lifeStealCooldown=.1;
            float before=Health;Heal(1);return Health>before;
        }
    }
}

using UnityEngine;
namespace Scrapshift
{
    public static class SpecialWeaponRules
    {
        // Reference decaying speed is measured in pixels; our world uses 100 pixels/unit.
        public static float Slow(float accumulated,float baseSpeed,float amount)
        {
            if(accumulated>baseSpeed*.7f)return accumulated;
            if(accumulated>baseSpeed*.45f)amount/=20;
            return accumulated+(float)System.Math.Floor(System.Math.Min(amount*100.0,baseSpeed*90.0))/100;
        }
        public static bool Execute(Equipment weapon,float roll)=>weapon?.Trait("one_shot_on_hit") is WeaponTrait t&&roll<t.value/100;
        public static float EngineeringSlow(Equipment weapon,float engineering,float baseSpeed)
        {var t=weapon?.Trait("effect_weapon_slow_on_hit");return t==null?0:Mathf.Floor(baseSpeed*Mathf.Max(0,(int)engineering)*t.value)/100;}
        public static float StructureDamage(RunRules rules,Equipment stats)
        {
            float damage=stats.damage;
            if(stats.scaling!=null)foreach(var s in stats.scaling)damage+=rules.Stat(s.key)*s.value;
            // Ordinary percentage Damage does not affect engineering structures.
            return Mathf.Max(1,stats.pet?rules.WeaponDamage(stats,rules.Stat("structure_percent_damage")):damage*(1+rules.Stat("structure_percent_damage")/100));
        }
    }
}

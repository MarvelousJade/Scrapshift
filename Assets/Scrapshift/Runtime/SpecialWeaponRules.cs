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
        public static bool Execute(Equipment weapon,float roll)=>weapon?.Trait("one_shot_on_hit") is WeaponTrait t&&t.value>0&&roll<=t.value/100;
        // GDScript float arithmetic is double precision. Keep the chance and
        // injected roll in that domain rather than rounding only the argument.
        public static bool CriticalMaterials(WeaponTrait trait,bool critical,bool killed,double roll)=>critical&&killed&&trait!=null&&roll<=trait.value/100.0;
        public static bool Explodes(WeaponTrait trait,float roll)=>trait!=null&&trait.chance>0&&roll<=trait.chance;
        public static float InitialCooldown(float cooldownSeconds,int weaponCount,float roll)
        {
            float frames=cooldownSeconds*60,basis=Mathf.Min(frames,180),count=Mathf.Min(weaponCount,6);
            float variation=Mathf.Min(count*basis/5,count*5);
            // init_stats(true) calls get_next_cooldown(true), then reset_cooldown.
            // The first timer has no big reload and cannot exceed its stat timer.
            return Mathf.Min(frames,Mathf.Lerp(Mathf.Max(1,basis-variation),basis+variation,roll))/60;
        }
        public static bool BurnSpreadOverlaps(string sourceId,Vector2 source,string targetId,Vector2 target)
        {
            if((CombatGeometry.BodyLayer(targetId)&4)==0)return false;
            // Unit's BurningParticles is at (0,-24) source pixels; its spread
            // area overlaps physics bodies, rather than their hurtboxes.
            Vector2 center=source+CombatGeometry.OriginOffset(sourceId)+Vector2.up*.24f;
            float radius=1.28f+CombatGeometry.Enemy(targetId).body;
            return (target+CombatGeometry.BodyOffset(targetId)-center).sqrMagnitude<=radius*radius;
        }
        public static float EngineeringSlow(Equipment weapon,float engineering,float baseSpeed)
        {var t=weapon?.Trait("effect_weapon_slow_on_hit");return t==null?0:Mathf.Floor(baseSpeed*Mathf.Max(0,(int)engineering)*t.value)/100;}
        public static float StructureDamage(RunRules rules,Equipment stats,float extraPercent=0)
        {
            float damage=stats.damage;
            if(stats.scaling!=null)foreach(var s in stats.scaling)damage+=rules.Stat(s.key)*s.value;
            // Ordinary percentage Damage does not affect engineering structures.
            return stats.pet?rules.WeaponDamage(stats,rules.Stat("structure_percent_damage")+extraPercent):
                Mathf.Max(1,EnemyReferenceRules.SourceRound(Mathf.Max(1,(int)damage)*(1+(rules.Stat("structure_percent_damage")+extraPercent)/100)));
        }
    }
}

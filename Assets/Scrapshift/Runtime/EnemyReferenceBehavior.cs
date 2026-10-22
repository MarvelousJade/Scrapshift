using UnityEngine;
namespace Scrapshift
{
    public static class EnemyReferenceRules
    {
        // Godot converts base health to an integer before applying run modifiers,
        // then rounds positive final health and damage halves away from zero.
        public static int SourceRound(float value)=>value>=0?Mathf.FloorToInt(value+.5f):Mathf.CeilToInt(value-.5f);
        public static int ScaledHealth(float baseHealth,float factor)=>SourceRound((int)baseHealth*factor);
        public static int ScaledDamage(float baseDamage,float factor)=>SourceRound(baseDamage*factor);
        // SpawningAttackBehavior.shoot: Godot round uses halves away from zero.
        public static int SummonCount(int count,float enemyPercent)=>Mathf.Max(1,SourceRound(count*(1+enemyPercent/100)));
        // Unit.take_damage rounds the critical multiplier before get_damage_value;
        // Unit.get_damage_value then subtracts the enemy's integer Armor.
        public static int CriticalDamage(int incoming,float multiplier)=>SourceRound(incoming*multiplier);
        public static int ArmoredDamage(int incoming,float armor)=>Mathf.Max(1,incoming-SourceRound(armor));
        public static float ScaledSpeed(float baseSpeed,float factor)=>SourceRound(baseSpeed*100*factor)/100f;
        // Unit.bonus_speed is an int; charge assignment and each Pursuer
        // increment truncate the accessibility-scaled pixel amount.
        public static float AdditiveSpeedBonus(float baseWorld,float accessibility)=>(int)(baseWorld*100*accessibility)/100f;
        public static float BossHealthFactor(int wave,bool doubleBoss=false)=>wave<=8?.5f:wave<=12||wave==20&&doubleBoss?.75f:1;
        public static float BossDamageFactor(int wave)=>wave>8&&wave<=12?.75f:1;
        public static float ChargeTargetRadius(EncounterAttack a)=>a.targetSize<0?Mathf.Min(6,a.maxRange/5):a.targetSize;
    }
    public sealed partial class ArenaGame
    {
        Vector2 playerVelocity;
        float EnemyStatSpeed(Enemy e)
        {
            // EvilMob overrides reset_speed_stat and deliberately uses stats.speed.
            if(e.type.id=="evil_mob")return e.type.speed;
            return EnemyReferenceRules.ScaledSpeed(e.speed,GameSettings.Value("enemySpeed")*Rules.EnemySpeedFactor*(Rules.Difficulty>=6?1.1f:1));
        }
        void ResetEnemyMovement(Enemy e)
        {
            e.spacing=e.behavior.spacing+UnityEngine.Random.Range(-e.behavior.spacingRandom,e.behavior.spacingRandom);
            e.moveRange=Mathf.Max(0,e.behavior.offset+UnityEngine.Random.Range(-e.behavior.rangeRandom,e.behavior.rangeRandom));
            e.wander=new Vector2(UnityEngine.Random.Range(-e.moveRange,e.moveRange),UnityEngine.Random.Range(-e.moveRange,e.moveRange));
            if(!e.behavior.allowWithin)e.wander=e.wander.normalized*e.moveRange;
            e.moveUpdate=0;
        }
        Vector2 ChargeTarget(Enemy e,Vector2 target)
        {
            Vector2 origin=EnemyOrigin(e);
            var a=e.behavior.source;if(a==null||a.chargeTarget==0)return target-origin;
            if(a.chargeTarget==2)return RandomPosition(PlayerOrigin,0)-origin;
            float radius=EnemyReferenceRules.ChargeTargetRadius(a);
            Vector2 random=new Vector2(UnityEngine.Random.Range(-radius,radius),UnityEngine.Random.Range(-radius,radius));
            if(a.targetForward&&playerVelocity.sqrMagnitude>.001f)random=(Vector2)(Quaternion.Euler(0,0,UnityEngine.Random.Range(-90f,90f))*playerVelocity.normalized)*UnityEngine.Random.Range(radius/4,radius);
            return target+(target-origin).normalized*radius+random-origin;
        }
    }
}

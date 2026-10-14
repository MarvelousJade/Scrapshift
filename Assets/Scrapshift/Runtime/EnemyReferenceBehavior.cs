using UnityEngine;
namespace Scrapshift
{
    public static class EnemyReferenceRules
    {
        public static float BossHealthFactor(int wave)=>wave<=8?.5f:wave<=12?.75f:1;
        public static float BossDamageFactor(int wave)=>wave>8&&wave<=12?.75f:1;
        public static float ChargeTargetRadius(EncounterAttack a)=>a.targetSize<0?Mathf.Min(6,a.maxRange/5):a.targetSize;
    }
    public sealed partial class ArenaGame
    {
        Vector2 playerVelocity;
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
            var a=e.behavior.source;if(a==null||a.chargeTarget==0)return target-e.pos;
            if(a.chargeTarget==2)return RandomPosition(playerPos,0)-e.pos;
            float radius=EnemyReferenceRules.ChargeTargetRadius(a);
            Vector2 random=new Vector2(UnityEngine.Random.Range(-radius,radius),UnityEngine.Random.Range(-radius,radius));
            if(a.targetForward&&playerVelocity.sqrMagnitude>.001f)random=(Vector2)(Quaternion.Euler(0,0,UnityEngine.Random.Range(-90f,90f))*playerVelocity.normalized)*UnityEngine.Random.Range(radius/4,radius);
            return target+(target-e.pos).normalized*radius+random-e.pos;
        }
    }
}

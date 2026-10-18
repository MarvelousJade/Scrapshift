using System;
using UnityEngine;
namespace Scrapshift
{
    public enum AttackPhase { Approach, Windup, Active }
    public sealed class EnemyAttack
    {
        public AttackPhase Phase { get; private set; }
        public Vector2 Direction { get; private set; }
        public float Progress { get; private set; }
        readonly EnemyBehavior behavior;
        readonly Func<float> nextCooldown;
        float remaining,chargeDistance,chargeCooldown;
        int shots;
        float NextCooldown(){if(behavior.source!=null&&behavior.source.longEvery>0&&shots>=behavior.source.longEvery){shots=0;return behavior.source.longCooldown;}return nextCooldown();}
        public float WindupDuration => behavior.windup;
        public EnemyAttack(EnemyBehavior behavior, Func<float> nextCooldown)
        {
            this.behavior = behavior; this.nextCooldown = nextCooldown;
            remaining = nextCooldown() + behavior.initial;
            chargeCooldown=remaining;
        }
        void Prepare(Vector2 targetDirection,Func<Vector2> chargeTarget)
        {
            var target=behavior.charge&&chargeTarget!=null?chargeTarget():targetDirection;
            chargeDistance=target.magnitude;Direction=target.normalized;
            Phase=AttackPhase.Windup;remaining=WindupDuration;Progress=0;
        }
        // Ranged aim follows the target until release; charges commit at preparation.
        // Source charge cooldown starts at release with the base dash duration.
        // It runs concurrently with any extra duration caused by target range.
        public bool Step(float dt, Vector2 targetDirection, bool inRange,Func<Vector2> chargeTarget=null)
        {
            if (dt <= 0) return false;
            remaining -= dt;
            if(behavior.charge)chargeCooldown-=dt;
            if(behavior.charge&&Phase==AttackPhase.Active&&chargeCooldown<=.00001f&&inRange)
            {Prepare(targetDirection,chargeTarget);return false;}
            if (Phase == AttackPhase.Approach)
            {
                if (remaining <= .00001f && inRange)
                    Prepare(targetDirection,chargeTarget);
                return false;
            }
            if (Phase == AttackPhase.Windup)
            {
                if (!behavior.charge) Direction = targetDirection.normalized;
                Progress = Mathf.Clamp01(1 - remaining / WindupDuration);
            }
            if (remaining > .00001f) return false;
            if (Phase == AttackPhase.Windup)
            {
                float duration=behavior.active;
                if(behavior.charge&&behavior.source?.chargeTiming==1&&chargeTarget!=null){var target=chargeTarget();Direction=target.normalized;chargeDistance=target.magnitude;}
                if(behavior.charge&&behavior.source?.scaleDuration==true)duration+=chargeDistance/150;
                shots++;Phase = AttackPhase.Active; remaining += duration;
                if(behavior.charge)chargeCooldown=behavior.active+NextCooldown();
                if (duration <= 0) { Phase = AttackPhase.Approach; remaining += behavior.charge?chargeCooldown:NextCooldown(); }
                return true;
            }
            Phase = AttackPhase.Approach;
            remaining=behavior.charge?chargeCooldown:remaining+NextCooldown();Progress=0;
            return false;
        }
    }
}

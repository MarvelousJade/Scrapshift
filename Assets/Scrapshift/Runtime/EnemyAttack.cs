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
        float remaining,chargeDistance;
        int shots;
        float NextCooldown(){if(behavior.source!=null&&behavior.source.longEvery>0&&shots>=behavior.source.longEvery){shots=0;return behavior.source.longCooldown;}return nextCooldown();}
        public float WindupDuration => behavior.windup;
        public EnemyAttack(EnemyBehavior behavior, Func<float> nextCooldown)
        {
            this.behavior = behavior; this.nextCooldown = nextCooldown;
            remaining = nextCooldown() + behavior.initial;
        }
        // Ranged aim follows the target until release; charges commit at preparation.
        // Active lasts for the dash or ranged animation tail, then normal cooldown.
        public bool Step(float dt, Vector2 targetDirection, bool inRange,Func<Vector2> chargeTarget=null)
        {
            if (dt <= 0) return false;
            remaining -= dt;
            if (Phase == AttackPhase.Approach)
            {
                if (remaining <= .00001f && inRange)
                { var target=behavior.charge&&chargeTarget!=null?chargeTarget():targetDirection;chargeDistance=target.magnitude;Direction=target.normalized; Phase = AttackPhase.Windup; remaining = WindupDuration; Progress = 0; }
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
                if (duration <= 0) { Phase = AttackPhase.Approach; remaining += NextCooldown(); }
                return true;
            }
            Phase = AttackPhase.Approach; remaining += NextCooldown(); Progress = 0;
            return false;
        }
    }
}

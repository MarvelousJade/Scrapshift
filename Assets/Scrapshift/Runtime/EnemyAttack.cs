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
        float remaining;
        public float WindupDuration => behavior.windup;
        public EnemyAttack(EnemyBehavior behavior, Func<float> nextCooldown)
        {
            this.behavior = behavior; this.nextCooldown = nextCooldown;
            remaining = nextCooldown() + behavior.initial;
        }
        // Ranged aim follows the target until release; charges commit at preparation.
        // Active lasts for the dash or ranged animation tail, then normal cooldown.
        public bool Step(float dt, Vector2 targetDirection, bool inRange)
        {
            if (dt <= 0) return false;
            remaining -= dt;
            if (Phase == AttackPhase.Approach)
            {
                if (remaining <= .00001f && inRange)
                { Direction = targetDirection.normalized; Phase = AttackPhase.Windup; remaining = WindupDuration; Progress = 0; }
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
                Phase = AttackPhase.Active; remaining += behavior.active;
                if (behavior.active <= 0) { Phase = AttackPhase.Approach; remaining += nextCooldown(); }
                return true;
            }
            Phase = AttackPhase.Approach; remaining += nextCooldown(); Progress = 0;
            return false;
        }
    }
}

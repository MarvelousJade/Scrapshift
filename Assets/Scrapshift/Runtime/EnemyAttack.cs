using UnityEngine;

namespace Scrapshift
{
    public enum AttackPhase { Approach, Windup, Active, Recovery }
    public sealed class EnemyAttack
    {
        public AttackPhase Phase { get; private set; }
        public Vector2 Direction { get; private set; }
        public float Progress { get; private set; }
        readonly bool charge;
        readonly float cooldown;
        float remaining;
        public float WindupDuration => charge ? .4f : .8f;
        public EnemyAttack(bool isCharge, float cooldown, float initialDelay)
        { charge = isCharge; this.cooldown = Mathf.Max(.2f, cooldown); remaining = initialDelay; }

        // Direction commits at windup; returns true once on release. No Step means paused.
        public bool Step(float dt, Vector2 targetDirection, bool inRange)
        {
            if (dt <= 0) return false;
            remaining -= dt;
            if (Phase == AttackPhase.Approach)
            {
                if (remaining <= 0 && inRange)
                { Direction = targetDirection.normalized; Phase = AttackPhase.Windup; remaining = WindupDuration; Progress = 0; }
                return false;
            }
            if (Phase == AttackPhase.Windup) Progress = Mathf.Clamp01(1 - remaining / WindupDuration);
            if (remaining > .00001f) return false;
            if (Phase == AttackPhase.Windup)
            { Phase = AttackPhase.Active; remaining = charge ? .75f : .12f; return true; }
            if (Phase == AttackPhase.Active)
            { Phase = AttackPhase.Recovery; remaining = .18f; return false; }
            Phase = AttackPhase.Approach; remaining = cooldown; Progress = 0;
            return false;
        }
    }
}

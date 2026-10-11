namespace Scrapshift
{
    public enum EnemyMovement { Follow, Destination, Offset, KeepRange, Pursuer, Materials, AroundPlayer }
    // Node-specific values from supplied scenes; media references remain unchanged.
    public sealed class EnemyBehavior
    {
        public EnemyMovement movement;
        public bool charge, ranged, canBoost = true;
        public float cooldown = 1, jitter = 10f/60, initial, windup = .4f, active = .75f;
        public float minRange, maxRange = 3, chargeSpeed = 5, spacing, offset = 2;
        public float projectileSpeed = 5, projectileGrowth = 1, spread;
        public static EnemyBehavior For(string id, bool secondPhase = false)
        {
            var b = new EnemyBehavior();
            switch (id)
            {
                case "charger": case "horned_charger":
                    b.charge = true; b.cooldown = 40f/60; break;
                case "bruiser": case "horned_bruiser":
                    b.charge = true; b.maxRange = 5; b.chargeSpeed = 7;
                    if (id == "bruiser") b.movement = EnemyMovement.Destination; break;
                case "spitter":
                    b.ranged = true; b.movement = EnemyMovement.KeepRange; b.spacing = 3;
                    b.cooldown = 2; b.windup = .8f; b.active = .2f; b.maxRange = 7;
                    b.projectileSpeed = 6; b.projectileGrowth = .75f; break;
                case "junkie":
                    b.ranged = true; b.movement = EnemyMovement.Destination;
                    b.cooldown = 2; b.windup = .4f; b.active = .1f; b.minRange = 2;
                    b.maxRange = 9999; b.spread = 1.2f; break;
                case "slasher":
                    b.ranged = true; b.movement = EnemyMovement.Offset; b.offset = 3;
                    b.cooldown = 1.25f; b.initial = .5f; b.active = 0; b.maxRange = 5;
                    b.projectileSpeed = 0; break;
                case "spawner": b.movement = EnemyMovement.Offset; break;
                case "looter": b.movement = EnemyMovement.Destination; break;
                case "evil_mob": b.movement = EnemyMovement.Materials; b.canBoost = false; break;
                case "pursuer": b.movement = EnemyMovement.Pursuer; break;
                case "buffer": b.movement = EnemyMovement.KeepRange; b.spacing = 5; b.canBoost = false; break;
                case "predator":
                    b.canBoost = false; b.jitter = 0; b.charge = !secondPhase; b.ranged = secondPhase;
                    b.cooldown = secondPhase ? 1.5f : 2; b.windup = .8f;
                    b.active = secondPhase ? 0 : .5f; b.maxRange = secondPhase ? 20 : 5;
                    b.chargeSpeed = 7.5f; b.projectileGrowth = 1.15f;
                    b.movement = secondPhase ? EnemyMovement.AroundPlayer : EnemyMovement.Follow;
                    b.offset = 3; if (secondPhase) b.jitter = 10f/60; break;
            }
            return b;
        }
        public static float PursuerBonus(float sinceHit) => UnityEngine.Mathf.Min(10, UnityEngine.Mathf.FloorToInt(sinceHit)) * .45f;
    }
}

using UnityEngine;

namespace Scrapshift
{
    // Godot collision dimensions / 100. Centers are adapted to our body-centered art.
    // Crowd, incoming-hit and outgoing-contact circles serve different purposes.
    public readonly struct EnemyCollision
    {
        public readonly float body, hurt, contact;
        public EnemyCollision(float body, float hurt, float contact)
        { this.body = body; this.hurt = hurt; this.contact = contact; }
    }

    public static class CombatGeometry
    {
        public const float PlayerHurtRadius = .21f;
        public const float BulletRadius = .44f;
        public const float HostileRadius = .170294f, FlashDuration = .1f;
        public static Vector2 PlayerHurtOffset => Vector2.down * .12f;
        // Relative to the neutral body center (Animation at -24 source pixels).
        // Shapes do not inherit the animated visual squash or rotation.
        public static Vector2 HurtOffset(string id)
        {
            switch(id)
            {
                case "bruiser": case "horned_bruiser": case "slasher": return Vector2.down*.4f;
                case "predator": case "invoker": return Vector2.down*.72f;
                case "charger": case "horned_charger": return Vector2.down*.12f;
                case "junkie": case "spawner": return Vector2.down*.16f;
                case "tree": return Vector2.down*.18f;
                default: return Vector2.down*.15f;
            }
        }
        public static Vector2 ContactOffset(string id)
        {
            switch(id)
            {
                case "bruiser": case "horned_bruiser": case "slasher": return new Vector2(-.01f,-.39f);
                case "buffer": return new Vector2(.01f,-.21f);
                case "charger": case "horned_charger": return new Vector2(.01f,-.15f);
                case "chaser": case "fin_alien": return new Vector2(.05f,-.24f);
                case "evil_mob": case "looter": return Vector2.down*.23f;
                case "helmet_alien": return new Vector2(.03f,-.26f);
                case "junkie": return new Vector2(.06f,-.22f);
                case "predator": case "invoker": return Vector2.down*.72f;
                case "pursuer": return new Vector2(-.01f,-.16f);
                case "spawner": return new Vector2(.03f,-.3f);
                case "spitter": return new Vector2(.05f,-.18f);
                default: return Vector2.down*.24f;
            }
        }
        public static EnemyCollision Enemy(string id)
        {
            switch (id)
            {
                case "lamprey": return new EnemyCollision(.190263f,.480104f,.290172f);
                case "butcher":case "croc":case "gargoyle":case "mantis":case "monk":case "rhino": return new EnemyCollision(.24f,.4f,.4f);
                case "colossus": return new EnemyCollision(.4f,.48f,.48f);
                case "mom":return new EnemyCollision(.4f,.64f,.56f);
                case "invoker":return new EnemyCollision(.4f,.4f,.4f);
                case "slasher_egg":case "tentacle":return new EnemyCollision(.24f,.4f,.24f);
                case "baby_alien": return new EnemyCollision(.22f, .32f, .160312f);
                case "bruiser": return new EnemyCollision(.162788f, .400125f, .270185f);
                case "horned_bruiser": return new EnemyCollision(.162788f, .400125f, .27f);
                case "buffer": return new EnemyCollision(.08544f, .32f, .22f);
                case "charger": case "horned_charger": return new EnemyCollision(.243311f, .480104f, .25f);
                case "chaser": case "fin_alien": return new EnemyCollision(.190263f, .32f, .16f);
                case "evil_mob": case "looter": return new EnemyCollision(.162788f, .32f, .210238f);
                case "helmet_alien": return new EnemyCollision(.22f, .32f, .21f);
                case "junkie": return new EnemyCollision(.13f, .32f, .2f);
                case "predator": return new EnemyCollision(.4f, .4f, .4f);
                case "pursuer": return new EnemyCollision(.140357f, .643817f, .26f);
                case "slasher": return new EnemyCollision(.162788f, .400125f, .31f);
                case "spawner": return new EnemyCollision(.323883f, .485077f, .34f);
                case "spitter": return new EnemyCollision(.14f, .32f, .22f);
                case "tree": return new EnemyCollision(.24f, .486621f, 0);
                default: throw new System.ArgumentException("Unknown enemy collision profile: " + id);
            }
        }

        // Import stores weapon knockback / 10; source applies 100 pixels per unit of force.
        public static Vector2 Knockback(Vector2 attackDirection, Vector2 awayFromPlayer, float importedAmount, float resistance)
            => (.2f * attackDirection.normalized + .8f * awayFromPlayer.normalized)
                * (importedAmount * 10) * (1 - Mathf.Clamp01(resistance));

        // Integrate the source's pre-movement 0.9 damping at 60 Hz without render-rate dependence.
        public static Vector2 StepKnockback(ref Vector2 velocity, float dt)
        {
            float decay = Mathf.Pow(.9f, Mathf.Max(0, dt) * 60);
            Vector2 distance = velocity * (.15f * (1 - decay));
            velocity *= decay;
            return distance;
        }
        public static bool ContactEnabled(Vector2 knockback, float moveSpeed)
            => knockback.sqrMagnitude <= moveSpeed * moveSpeed;

        public static float ContactSlow(float currentSlow, float baseSpeed)
        {
            if (currentSlow > baseSpeed * .7f) return currentSlow;
            float added = Mathf.Max(2, baseSpeed * .8f);
            if (currentSlow > baseSpeed * .45f) added /= 20;
            return currentSlow + Mathf.Min(added, baseSpeed * .9f);
        }

        public static float Invulnerability(float damage, float maxHealth)
            => Mathf.Clamp(damage / Mathf.Max(1, maxHealth) * .4f / .15f, .2f, .4f);

        public static bool ProjectileOutside(Vector2 pos)
            => Mathf.Abs(pos.x) > ArenaGeometry.HalfSize.x + 1 || Mathf.Abs(pos.y) > ArenaGeometry.HalfSize.y + 1;

        // First entry into a circle, including start overlaps. Infinity means no hit.
        public static float SweepFraction(Vector2 center, float radius, Vector2 from, Vector2 to)
        {
            Vector2 offset = from - center, delta = to - from;
            float c = offset.sqrMagnitude - radius * radius;
            if (c <= 0) return 0;
            float a = delta.sqrMagnitude;
            if (a < .0000001f) return float.PositiveInfinity;
            float b = Vector2.Dot(offset, delta), discriminant = b * b - a * c;
            if (discriminant < 0) return float.PositiveInfinity;
            float t = (-b - Mathf.Sqrt(discriminant)) / a;
            return t >= 0 && t <= 1 ? t : float.PositiveInfinity;
        }
    }
}

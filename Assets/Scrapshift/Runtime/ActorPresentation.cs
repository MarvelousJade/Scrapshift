using UnityEngine;

namespace Scrapshift
{
    public static class ActorPresentation
    {
        public const float PlayerScale = .85f;
        // Presentation is separate from contact/separation radii and combat balance.
        public static float EnemyScale(EnemyEntry enemy) => enemy.role == "Boss" ? 1.65f :
            enemy.id.Contains("bruiser") || enemy.role == "Spawner" ? 1.2f : enemy.id == "baby_alien" ? 1.4f : 1;
    }
}

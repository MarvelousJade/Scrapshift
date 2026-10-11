using System;
using UnityEngine;

namespace Scrapshift
{
    public static class SpawnPlacement
    {
        public static readonly Vector2 Bounds = new Vector2(10.2f, 4.9f);
        static float Range(System.Random random, float min, float max) => min + (float)random.NextDouble() * (max - min);
        static Vector2 InRect(System.Random random, Vector2 center, Vector2 half) => center +
            new Vector2(Range(random, -half.x, half.x), Range(random, -half.y, half.y));

        // Retain formation/edge constraints even when the requested clearance is impossible.
        static Vector2 Safe(Func<Vector2> sample, Vector2 player, float distance)
        {
            Vector2 best = sample(); float bestDistance = (best - player).sqrMagnitude;
            for (int i = 0; i < 128 && bestDistance < distance * distance; i++)
            {
                Vector2 candidate = sample(); float d = (candidate - player).sqrMagnitude;
                if (d > bestDistance) { best = candidate; bestDistance = d; }
            }
            return best;
        }

        public static Vector2 Anchor(SpawnGroup group, Vector2 player, System.Random random)
        {
            float inset = Mathf.Max(0, group.inset) + Mathf.Max(0, group.area) * .5f;
            Vector2 half = new Vector2(Mathf.Max(.1f, Bounds.x - inset), Mathf.Max(.1f, Bounds.y - inset));
            return Safe(() => InRect(random, Vector2.zero, half), player, Mathf.Min(Bounds.x, 3 + Mathf.Max(0, group.area) * .5f));
        }

        public static Vector2 Position(SpawnGroup group, UnitEntry unit, Vector2 anchor, Vector2 player, System.Random random)
        {
            float inset = Mathf.Clamp(group.inset, 0, Bounds.y - .1f);
            Vector2 half = Bounds - Vector2.one * inset;
            Func<Vector2> sample = () =>
            {
                if (group.edge)
                {
                    Vector2 p = InRect(random, Vector2.zero, half);
                    switch (random.Next(4))
                    {
                        case 0: p.x = -half.x; break;
                        case 1: p.x = half.x; break;
                        case 2: p.y = -half.y; break;
                        default: p.y = half.y; break;
                    }
                    return p;
                }
                if (group.area < 0) return InRect(random, Vector2.zero, half);
                Vector2 point = InRect(random, anchor, Vector2.one * group.area * .5f);
                return new Vector2(Mathf.Clamp(point.x, -half.x, half.x), Mathf.Clamp(point.y, -half.y, half.y));
            };
            return Safe(sample, player, 3 + Mathf.Max(0, unit.distance));
        }
    }
}

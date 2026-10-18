using System;
using UnityEngine;

namespace Scrapshift
{
    public static class SpawnPlacement
    {
        public static Vector2 Bounds => ArenaGeometry.HalfSize;
        static float SourceEdge=>Mathf.Min(.64f,Mathf.Floor(Mathf.Min(Bounds.x,Bounds.y)*40)/100);
        // ZoneService raw points, already centered and Y-flipped into our world.
        // The rolls keep their source ordering: zero Y samples the top edge.
        public static Vector2 SourceRandomPosition(float rollX,float rollY)
        {
            Vector2 half=Bounds-Vector2.one*SourceEdge;
            return new Vector2(Mathf.LerpUnclamped(-half.x,half.x,rollX),Mathf.LerpUnclamped(half.y,-half.y,rollY));
        }
        public static Vector2 SourceAreaPosition(Vector2 center,float area,float rollX,float rollY)
        {
            Vector2 margin=Bounds-Vector2.one*(SourceEdge+area*.5f);
            center=new Vector2(Mathf.Clamp(center.x,-margin.x,margin.x),Mathf.Clamp(center.y,-margin.y,margin.y));
            return center+new Vector2((rollX-.5f)*area,(.5f-rollY)*area);
        }
        static float Range(System.Random random, float min, float max) => min + (float)random.NextDouble() * (max - min);
        static Vector2 InRect(System.Random random, Vector2 center, Vector2 half) => center +
            new Vector2(Range(random, -half.x, half.x), Range(random, -half.y, half.y));

        // entity_spawner.gd: retry and relax by five source pixels down to 25.
        static Vector2 Safe(Func<Vector2> sample, Vector2 player, float distance)
        {
            Vector2 candidate = sample();
            while ((candidate-player).sqrMagnitude < distance*distance)
            {
                candidate = sample(); distance = Mathf.Max(.25f, distance-.05f);
                if(distance <= .25f) break;
            }
            return candidate;
        }

        public static Vector2 Anchor(SpawnGroup group, Vector2 player, System.Random random)
        {
            float inset = Mathf.Max(0, group.inset) + Mathf.Max(0, group.area) * .5f;
            Vector2 half = new Vector2(Mathf.Max(.1f, Bounds.x - inset), Mathf.Max(.1f, Bounds.y - inset));
            return Safe(() => InRect(random, Vector2.zero, half), player, Mathf.Min(Bounds.x, 3 + Mathf.Max(0, group.area) * .5f));
        }

        public static Vector2 Position(SpawnGroup group, UnitEntry unit, Vector2 anchor, Vector2 player, System.Random random)
        {
            float inset = group.edge ? .64f : Mathf.Clamp(group.inset, 0, Bounds.y - .1f);
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
                Vector2 margin = Bounds - Vector2.one * (.64f + group.area * .5f);
                Vector2 center = new Vector2(Mathf.Clamp(anchor.x,-margin.x,margin.x),Mathf.Clamp(anchor.y,-margin.y,margin.y));
                return InRect(random, center, Vector2.one * group.area * .5f);
            };
            return Safe(sample, player, 3 + Mathf.Max(0, unit.distance));
        }
    }
}

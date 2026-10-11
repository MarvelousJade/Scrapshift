using System;
using UnityEngine;

namespace Scrapshift
{
    public sealed class ScrapMotion
    {
        public Vector2 position, velocity;
        public float height, verticalSpeed, angle, spin, age;
        public bool magnetized;
        float magnetSpeed;

        public void Step(float dt, Vector2 target, float radius, bool reducedMotion)
        {
            age += dt;
            if (age > .16f && Vector2.Distance(position, target) <= radius) magnetized = true;
            if (magnetized)
            {
                magnetSpeed = Mathf.Min(16, magnetSpeed + dt * 36);
                position = Vector2.MoveTowards(position, target, dt * magnetSpeed);
                height = Mathf.MoveTowards(height, 0, dt * 3);
                return;
            }
            position = RunRules.ClampPosition(position + velocity * dt);
            velocity *= Mathf.Exp(-dt * 6);
            if (reducedMotion) { height = 0; spin = 0; return; }
            verticalSpeed -= dt * 16;
            height += verticalSpeed * dt;
            if (height < 0)
            {
                height = 0;
                verticalSpeed = Mathf.Abs(verticalSpeed) > 1 ? -verticalSpeed * .28f : 0;
            }
            angle += spin * dt;
            spin *= Mathf.Exp(-dt * (height > 0 ? 2 : 14));
        }

        // Last piece carries the remainder, even when only one slot remains.
        public static int[] SplitValue(int value, int availableSlots)
        {
            int count = Math.Min(Math.Min(6, Math.Max(0, value)), Math.Max(0, availableSlots));
            var pieces = new int[count];
            for (int i = 0; i < count; i++) pieces[i] = i == count - 1 ? value - count + 1 : 1;
            return pieces;
        }
    }
}

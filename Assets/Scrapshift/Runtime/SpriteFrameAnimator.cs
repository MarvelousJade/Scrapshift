using UnityEngine;

namespace Scrapshift
{
    // Driven by the simulation so pause and hit-stop also pause the animation.
    public sealed class SpriteFrameAnimator : MonoBehaviour
    {
        public Sprite[] idleFrames, moveFrames;
        [Range(3, 12)] public float walkFramesPerSecond = 7;
        SpriteRenderer view;
        float clock;
        bool moving;
        public int Frame { get; private set; }
        public bool Moving => moving;
        public bool FacingLeft { get; private set; }

        public void Tick(Vector2 movement, float dt, bool reducedMotion)
        {
            if (view == null) view = GetComponent<SpriteRenderer>();
            bool next = movement.sqrMagnitude > .01f;
            if (next != moving) { clock = 0; moving = next; }
            clock += Mathf.Max(0, dt);
            if (Mathf.Abs(movement.x) > .05f) FacingLeft = movement.x < 0;
            Frame = FrameAt(clock, moving, moveFrames.Length, walkFramesPerSecond);
            var frames = moving ? moveFrames : idleFrames;
            if (frames != null && frames.Length > 0) view.sprite = frames[Frame % frames.Length];
            view.flipX = FacingLeft;
        }

        public static int FrameAt(float time, bool moving, int walkCount = 6, float walkFps = 7)
        {
            if (moving) return Mathf.FloorToInt(time * walkFps) % Mathf.Max(1, walkCount);
            float cycle = Mathf.Repeat(time, 3);
            if (cycle < 2.64f) return 0;
            return Mathf.Min(3, 1 + Mathf.FloorToInt((cycle - 2.64f) / .12f));
        }

        // Short kick followed by a slower return. Zero before/after the firing interval.
        public static float Recoil(float age)
        {
            if (age < 0 || age >= .16f) return 0;
            return age < .025f ? age / .025f : 1 - (age - .025f) / .135f;
        }
    }
}

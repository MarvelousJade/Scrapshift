using UnityEngine;

namespace Scrapshift
{
    public static class PresentationLayout
    {
        public const float Aspect = 1440f / 900f;
        public static Rect Viewport(int width, int height)
        {
            if (width <= 0 || height <= 0) return new Rect(0, 0, 1, 1);
            float actual = (float)width / height;
            if (actual > Aspect) { float w = Aspect / actual; return new Rect((1 - w) / 2, 0, w, 1); }
            float h = actual / Aspect; return new Rect(0, (1 - h) / 2, 1, h);
        }
    }
}

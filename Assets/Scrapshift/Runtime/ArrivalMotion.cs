using UnityEngine;

namespace Scrapshift
{
    // Reference landing keys, converted from screen pixels into our world coordinates.
    public static class ArrivalMotion
    {
        public const float Duration = 1.2f;
        public static Vector2 Offset(float time)
        {
            if(time < .7f) return Vector2.Lerp(new Vector2(-2.65f,7.25f),new Vector2(-.01f,1.4f),1-Mathf.Pow(1-Mathf.Clamp01(time/.7f),3));
            if(time < 1) return new Vector2(-.01f,1.4f);
            return Vector2.Lerp(new Vector2(-.01f,1.4f),Vector2.zero,Mathf.Clamp01((time-1)/.1f));
        }
        public static Vector2 BodyScale(float time)
        {
            if(time<1) return Vector2.one;
            if(time<1.05f)return Vector2.Lerp(Vector2.one,new Vector2(.776f,2.02f),(time-1)/.05f);
            if(time<1.1f)return Vector2.Lerp(new Vector2(.776f,2.02f),new Vector2(1.481f,.762f),(time-1.05f)/.05f);
            return Vector2.Lerp(new Vector2(1.481f,.762f),Vector2.one,Mathf.Clamp01((time-1.1f)/.1f));
        }
        public static Vector2 CanopyScale(float time)
        {
            if(time<.7f)return Vector2.one;
            if(time<.8f)return Vector2.Lerp(Vector2.one,new Vector2(.716f,1.468f),(time-.7f)/.1f);
            if(time<.9f)return Vector2.Lerp(new Vector2(.716f,1.468f),new Vector2(1.234f,.374f),(time-.8f)/.1f);
            return Vector2.Lerp(new Vector2(1.234f,.374f),Vector2.zero,Mathf.Clamp01((time-.9f)/.1f));
        }
    }
}

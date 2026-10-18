using UnityEngine;

namespace Scrapshift
{
    // Reference landing keys, converted from screen pixels into our world coordinates.
    public static class ArrivalMotion
    {
        public const float Duration = 1.2f;
        public static float CanopyRotation(float time)=>time<.5f?Mathf.Lerp(0,8.2f,Mathf.SmoothStep(0,1,time/.5f)):
            time<.7f?Mathf.Lerp(8.2f,-4.2f,Mathf.SmoothStep(0,1,(time-.5f)/.2f)):
            Mathf.Lerp(-4.2f,0,Mathf.SmoothStep(0,1,(time-.7f)/.1f));
        // Sprite-local source keys plus its -24px Animation registration equal
        // the Weapons-container keys. Share interpolation to keep them attached.
        public static Vector2 Offset(float time)=>ArrivalWeaponMotion.Offset(time);
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

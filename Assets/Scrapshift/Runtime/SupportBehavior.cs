using UnityEngine;
namespace Scrapshift
{
    public static class SupportBehavior
    {
        // Godot Tween TRANS_EXPO, EASE_IN_OUT used by Bonk Dog's half-second leap.
        public static float JumpProgress(float t)=>t<=0?0:t>=1?1:t<.5f?Mathf.Pow(2,20*t-10)/2:(2-Mathf.Pow(2,-20*t+10))/2;
        public static float CatInterval(float interval,bool boosted)=>interval*(boosted?.5f:1);
        public static bool MayReserve(string kind,bool boss,bool elite,bool claimed)=>kind=="ratzilla"&&boss?false:!claimed||(kind=="blazemander"?elite:boss);
    }
}

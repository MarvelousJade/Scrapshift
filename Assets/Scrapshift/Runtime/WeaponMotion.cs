using UnityEngine;
namespace Scrapshift
{
    public static class WeaponMotion
    {
        public static float HalfDuration(string id)=>id=="smg"?.05f:id=="laser_gun"?.2f:.1f;
        public static float Distance(string id)=>id=="smg"?.1f:id=="laser_gun"?.4f:.25f;
        public static float Duration(Equipment w,float fireRate=1)=>(w.scaling!=null?w.recoilHalf*2:w.melee?.18f:HalfDuration(w.id)*2)/Mathf.Max(1,fireRate);
        public static float Recoil(Equipment w,float age,float fireRate=1)
        {
            if(w.scaling==null)return Recoil(w.id,age,fireRate);
            float rate=Mathf.Max(1,fireRate),half=w.recoilHalf/rate;
            if(age<0||age>=half*2||half<=0)return 0;
            return w.recoil/rate*(age<half?Ease(age/half):1-Ease((age-half)/half));
        }
        static float Ease(float t)=>t>=1?1:1-Mathf.Pow(2,-10*Mathf.Clamp01(t));
        public static float Recoil(string id,float age,float fireRate=1)
        {
            float rate=Mathf.Max(1,fireRate);float half=HalfDuration(id)/rate;
            if(age<0||age>=half*2)return 0;
            return Distance(id)/rate*(age<half?Ease(age/half):1-Ease((age-half)/half));
        }
        // Only time left after the shooting tween is allowed to reduce cooldown.
        public static void Step(Equipment w,float dt,ref float shooting,ref float cooldown)
        {
            cooldown=Mathf.Max(0,cooldown-Mathf.Max(0,dt-shooting));
            shooting=Mathf.Max(0,shooting-dt);
        }
    }
}

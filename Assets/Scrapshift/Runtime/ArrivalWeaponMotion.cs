using UnityEngine;
namespace Scrapshift
{
    // player.tscn landing tracks 10-12. These gameplay transforms are separate
    // from the approved body illustration's ArrivalMotion presentation.
    public static class ArrivalWeaponMotion
    {
        static readonly float[] times={0,.45f,.7f,.95f,1,1.1f,1.2f};
        static readonly Vector2[] positions={new Vector2(-265,-749),new Vector2(-27.1691f,-221.988f),new Vector2(-1,-164),new Vector2(-1,-164),new Vector2(-1,-164),new Vector2(0,-24),new Vector2(0,-24)};
        static readonly float[] rotations={13.8133f,1.36924f,0,0,0,0,0};
        static int Segment(float time)
        {
            int i=0;while(i+1<times.Length-1&&times[i+1]<=time)i++;return i;
        }
        static float Cubic(float before,float from,float to,float after,float t)
        {
            // Godot 3.6 Animation::_cubic_interpolate(Variant): uniform cubic
            // neighbors; non-looping boundaries duplicate their endpoint keys.
            // https://github.com/godotengine/godot/blob/3.6-stable/scene/resources/animation.cpp
            return .5f*(2*from+(-before+to)*t+(2*before-5*from+4*to-after)*t*t+(-before+3*from-3*to+after)*t*t*t);
        }
        static Vector2 PositionPixels(float time)
        {
            if(time<=0)return positions[0];if(time>=times[times.Length-1])return positions[positions.Length-1];
            int i=Segment(time),before=Mathf.Max(0,i-1),after=Mathf.Min(positions.Length-1,i+2);
            float t=(time-times[i])/(times[i+1]-times[i]);
            return new Vector2(Cubic(positions[before].x,positions[i].x,positions[i+1].x,positions[after].x,t),Cubic(positions[before].y,positions[i].y,positions[i+1].y,positions[after].y,t));
        }
        // Offset from our neutral body-centered Weapons origin. Source end
        // position is (0,-24) pixels relative to the player's raw origin.
        public static Vector2 Offset(float time)
        {var p=PositionPixels(time);return new Vector2(p.x,-p.y-24)/100;}
        public static float Rotation(float time)
        {
            if(time<=0)return -rotations[0];if(time>=times[times.Length-1])return -rotations[rotations.Length-1];
            int i=Segment(time);float t=(time-times[i])/(times[i+1]-times[i]);
            return -Cubic(rotations[Mathf.Max(0,i-1)],rotations[i],rotations[i+1],rotations[Mathf.Min(rotations.Length-1,i+2)],t);
        }
        // Every supplied landing scale key is (1,1); before .45 it retains the
        // scene's same initial scale. Body squash must not enter this transform.
        public static Vector2 Scale(float time)=>Vector2.one;
        static Vector2 Rotate(Vector2 value,float degrees)=>Quaternion.Euler(0,0,degrees)*value;
        public static Vector2 TransformMount(Vector2 playerBody,Vector2 neutralLocalMount,float time)
            =>playerBody+Offset(time)+Rotate(Vector2.Scale(neutralLocalMount,Scale(time)),Rotation(time));
        public static Vector2 VisualDirection(Vector2 localWeaponDirection,float time)=>Rotate(localWeaponDirection,Rotation(time));
        public static Vector2 TransformPoint(Vector2 playerBody,Vector2 neutralLocalMount,Vector2 weaponLocalPoint,Vector2 localWeaponDirection,float time)
        {
            float aim=Mathf.Atan2(localWeaponDirection.y,localWeaponDirection.x)*Mathf.Rad2Deg;
            return TransformMount(playerBody,neutralLocalMount,time)+Rotate(Vector2.Scale(Rotate(weaponLocalPoint,aim),Scale(time)),Rotation(time));
        }
        // Source weapon.gd writes a global target angle into LOCAL rotation.
        // Its parent rotation affects visuals/melee/muzzle global position.
        // RangedWeaponShootingBehavior passes that LOCAL angle directly to the
        // world projectile, so its velocity must NOT use VisualDirection.
        public static Vector2 ProjectileDirection(Vector2 localWeaponDirection)=>localWeaponDirection;
    }
}

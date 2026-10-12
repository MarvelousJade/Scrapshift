using UnityEngine;

namespace Scrapshift
{
    public static class SpringMotion
    {
        public const float IdlePeriod=1, WalkPeriod=.6f;
        public const float LegSpread=.31f, LegY=-.48f, LegHeight=.16f, LegWidth=1, LegRestAngle=110;
        // Recreates the source's pose landmarks; smooth interpolation is a Unity adaptation.
        public static Vector3 Pose(float time,bool moving,bool reducedMotion=false)
        {
            if(reducedMotion)return new Vector3(1,1,0);
            if(!moving)
            {
                float t=(1-Mathf.Cos(Mathf.Repeat(time,IdlePeriod)*Mathf.PI*2))*.5f;
                return new Vector3(1+.15f*t,1-.15f*t,-.05f*t);
            }
            float phase=Mathf.Repeat(time,WalkPeriod*.5f)/.1f;
            Vector3 a,b;
            if(phase<1){a=new Vector3(1,1,0);b=new Vector3(1.2f,.8f,-.1f);}
            else if(phase<2){a=new Vector3(1.2f,.8f,-.1f);b=new Vector3(.9f,1.1f,.05f);}
            else {a=new Vector3(.9f,1.1f,.05f);b=new Vector3(1,1,0);}
            return Vector3.Lerp(a,b,Mathf.SmoothStep(0,1,phase-Mathf.Floor(phase)));
        }
        public static Vector2 Foot(float time,int side,bool moving,bool reducedMotion=false)
        {
            if(!moving || reducedMotion)return new Vector2(side*LegSpread,0);
            float stride=Mathf.Sin(time*Mathf.PI*2/WalkPeriod)*side;
            return new Vector2(side*LegSpread+stride*.035f,Mathf.Max(0,stride)*.035f);
        }
        public static float LegAngle(float time,int side,bool moving,bool reducedMotion=false)
            => side*(LegRestAngle+(moving && !reducedMotion ? Mathf.Sin(time*Mathf.PI*2/WalkPeriod)*12 : 0));
        public static Matrix4x4 PortraitLegTransform(Vector2 center,int side)
        {
            // Mirror in sprite space before rotation, just like SpriteRenderer.flipX.
            // GUIUtility pivot helpers premultiply and would mirror after rotation.
            return Matrix4x4.TRS(center,Quaternion.Euler(0,0,-LegAngle(0,side,false)),
                new Vector3(side<0?-LegWidth:LegWidth,1,1))*Matrix4x4.Translate(-center);
        }
    }
}

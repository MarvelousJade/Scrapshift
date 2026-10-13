using UnityEngine;
namespace Scrapshift
{
    // Reference Animation tracks adapted to our single-sprite support actors.
    public static class SupportMotion
    {
        public static Vector2 TurretScale(float age)
        {
            if(age>=.3f)return Vector2.one;
            var a=new Vector2(1.1f,.9f);var b=new Vector2(.8f,1.2f);var c=new Vector2(1.2f,.8f);
            return age<.1f?Vector2.Lerp(a,b,age/.1f):age<.2f?Vector2.Lerp(b,c,Mathf.Pow((age-.1f)/.1f,2.82843f)):Vector2.Lerp(c,Vector2.one,(age-.2f)/.1f);
        }
        public static float TurretLift(float age)=>age>=.3f?0:age<.1f?Mathf.Lerp(-.04f,.08f,age/.1f):age<.2f?Mathf.Lerp(.08f,-.06f,Mathf.Pow((age-.1f)/.1f,2.73208f)):Mathf.Lerp(-.06f,0,(age-.2f)/.1f);
        public static float PetSway(string kind,float phase)=>-Mathf.Sin(phase*Mathf.PI)*(kind=="catling_gun"?1.8f:kind=="bot_o_mine"?1.2f:kind=="doc_moth"?0:3f);
        public static Vector2 PetScale(string kind,float phase,bool moving,float attack)
        {
            float pulse=(1-Mathf.Cos(phase*Mathf.PI*2))*.5f;
            float amount=moving?(kind=="ratzilla"?.2f:kind=="scapegoat"?.15f:kind=="bot_o_mine"?.03f:.05f):.025f;
            if(kind=="doc_moth")return new Vector2(1-.15f*pulse,1+.025f*pulse);
            if(kind=="jellyshield")amount=.07f;
            var scale=new Vector2(1+amount*pulse,1-amount*pulse);
            if(attack>0){float hit=Mathf.Sin(Mathf.Clamp01(attack/.3f)*Mathf.PI);scale+=new Vector2(.13f,-.1f)*hit;}
            return scale;
        }
    }
}

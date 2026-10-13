using UnityEngine;
namespace Scrapshift
{
    // Source: pillar_projectile.tscn and shooting_attack_behavior.gd (100 px/world unit).
    public static class BossPatterns
    {
        public const float PillarEnd=1.04f, PillarRadius=.23f;
        public static bool PillarActive(float age)=>age>=.54f&&age<.68f;
        public static Vector2 SpawnOffset(float diameter,bool border,bool evenlySpaced,int index,int count,float randomX,float randomY)
        {
            float radius=diameter*.5f;
            if(!border)return new Vector2(randomX,randomY)*radius;
            float angle=evenlySpaced?index*Mathf.PI*2/count:(randomX+1)*Mathf.PI;
            return new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*radius;
        }
    }
    public sealed partial class ArenaGame
    {
        GameObject PillarWarning()
        {
            var obj=new GameObject("Cybernetic pillar warning");
            var ring=MakeLine("Warning footprint",obj.transform,coral,.045f,true);ring.sortingOrder=13;ring.positionCount=20;
            for(int i=0;i<20;i++){float a=i*Mathf.PI*2/20;ring.SetPosition(i,new Vector3(Mathf.Cos(a)*.23f,Mathf.Sin(a)*.12f,0));}
            var pillar=MakeLine("Rising energy",obj.transform,coral,.3f);pillar.sortingOrder=14;pillar.positionCount=2;
            pillar.SetPosition(0,Vector3.zero);pillar.SetPosition(1,Vector3.up*.8f);pillar.enabled=false;
            return obj;
        }
        void TickPillar(Shot shot)
        {
            var lines=shot.obj.GetComponentsInChildren<LineRenderer>();
            float fade=shot.age<.68f?1:Mathf.Clamp01((BossPatterns.PillarEnd-shot.age)/.36f);
            lines[0].startColor=lines[0].endColor=new Color(coral.r,coral.g,coral.b,(.45f+.55f*Mathf.Clamp01(shot.age/.5f))*fade);
            lines[1].enabled=shot.age>=.5f;
            lines[1].startColor=lines[1].endColor=BossPatterns.PillarActive(shot.age)?amber:coral;
            lines[1].transform.localScale=new Vector3(fade,Mathf.Clamp01((shot.age-.5f)/.04f)*fade,1);
        }
    }
}

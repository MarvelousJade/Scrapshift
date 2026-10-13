using System;
using System.Collections.Generic;
using UnityEngine;
namespace Scrapshift
{
    [Serializable] public sealed class ProjectilePatternCatalog {public ProjectilePattern[] patterns;}
    [Serializable] public sealed class ProjectilePattern {public string id;public float rate,speed,growth;public PatternGenerator[] generators;}
    [Serializable] public sealed class PatternGenerator
    {public int anchor,along,copies;public Vector2 offset,step,direction,random,sin,sinSpeed;public float delay,delayStep,speed,rate,damage;public bool active;}
    public sealed partial class ArenaGame
    {
        sealed class PatternEmitter {public PatternGenerator generator;public Vector2 position;public float clock,interval;}
        readonly List<PatternEmitter> patternEmitters=new List<PatternEmitter>();
        ProjectilePatternCatalog projectilePatterns;ProjectilePattern currentPattern;
        void PrepareProjectileEvent()
        {
            patternEmitters.Clear();if(!bulletWaves.Contains(Rules.Wave)||fogWaves.Contains(Rules.Wave))return;
            if(projectilePatterns==null)projectilePatterns=JsonUtility.FromJson<ProjectilePatternCatalog>(Resources.Load<TextAsset>("projectile-patterns").text);
            currentPattern=projectilePatterns.patterns[placementRandom.Next(projectilePatterns.patterns.Length)];
            float wave=Rules.Wave;if(eliteWaves.ContainsKey(Rules.Wave)||hordeWaves.Contains(Rules.Wave))wave/=2;
            Vector2 size=ArenaGeometry.HalfSize*2;
            foreach(var g in currentPattern.generators)
            {
                if(!g.active)continue;
                int anchor=Mathf.Max(0,g.anchor-1);Vector2 origin=new Vector2(anchor%3*size.x/2,anchor/3*size.y/2)+g.offset;
                int copies=g.copies;
                if(g.along==1)copies=g.step.x==0?1:Mathf.Max(1,(int)(Mathf.Abs(size.x-origin.x)/g.step.x)+1);
                if(g.along==2)copies=g.step.y==0?1:Mathf.Max(1,(int)(Mathf.Abs(size.y-origin.y)/g.step.y)+1);
                float interval=Mathf.Max(.05f,(currentPattern.rate-wave/20)*g.rate);
                for(int i=0;i<copies;i++){Vector2 pos=origin+g.step*i;pos=new Vector2(pos.x-size.x/2,size.y/2-pos.y);patternEmitters.Add(new PatternEmitter{generator=g,position=pos,interval=interval,clock=interval*(g.delay+i*g.delayStep)});}
            }
        }
        void TickProjectileEvent(float dt)
        {
            if(currentPattern==null)return;
            float eventWave=Rules.Wave;if(eliteWaves.ContainsKey(Rules.Wave)||hordeWaves.Contains(Rules.Wave))eventWave/=2;
            foreach(var emitter in patternEmitters)
            {
                emitter.clock+=dt;
                while(emitter.clock>emitter.interval)
                {
                    emitter.clock-=emitter.interval;if(shots.Count>=1200)break;
                    var g=emitter.generator;Vector2 pos=emitter.position+new Vector2(UnityEngine.Random.Range(-g.random.x,g.random.x),UnityEngine.Random.Range(-g.random.y,g.random.y))*.5f;
                    Vector2 direction=new Vector2(g.direction.x,-g.direction.y).normalized;
                    float initialDamage=Mathf.Round((1+currentPattern.growth*(Rules.Wave-1))*Rules.EnemyDamageFactor*.5f);
                    var obj=PlasmaOrb("Security grid pulse");obj.transform.position=pos;
                    shots.Add(new Shot{obj=obj,pos=pos,velocity=direction*(currentPattern.speed+eventWave*.02f)*g.speed,damage=Mathf.Max(1,(initialDamage+currentPattern.growth*(Rules.Wave-1))*g.damage),life=45,hostile=true,
                        sinusoidal=new Vector2(g.sin.x,-g.sin.y),sinSpeed=g.sinSpeed});
                }
            }
        }
    }
}

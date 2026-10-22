using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Scrapshift
{
    [Serializable] public sealed class WeaponReferenceProfile
    {
        public string id,scene;public int tier,attackType;public bool alternate,returnDamage,speedWithRange,canBounce=true;
        public float accuracy=1,soundDb=-5,effectScale=1;public float[] half,hitOffset,muzzle,muzzleRecoil;public SecondaryReference[] secondary;public ExplosionReference[] explosions;
    }
    [Serializable] public sealed class ExplosionReference
    {public int index,smokeAmount;public string texture;public string[] clips;public float soundDb,radius;[NonSerialized] public Sprite sprite;[NonSerialized] public AudioClip[] audio;}
    [Serializable] public sealed class SecondaryReference {public int index;public string scene;}
    [Serializable] public sealed class ProjectileReference
    {
        public string id,start,end,trail;public string[] frames;public float[] times,offset,scale,half,hitOffset,sinusoidal,sinSpeed;
        public float duration,stop,spin,radius,stopDelay;public bool loop,slow,delayed,beam,rail;
        [NonSerialized] public Sprite[] sprites;
        public int FrameIndex(float age)
        {if(loop&&duration>0)age%=duration;int i=0;while(i+1<times.Length&&times[i+1]<=age)i++;return i;}
        public Sprite Frame(float age)
        {
            if(sprites==null)sprites=frames.Select(path=>MakeSprite(path,beam)).ToArray();
            return sprites[FrameIndex(age)];
        }
        public static Sprite MakeSprite(string path,bool left=false)
        {var t=Resources.Load<Texture2D>(path);var s=Sprite.Create(t,new Rect(0,0,t.width,t.height),new Vector2(left?0:.5f,.5f),100,0,SpriteMeshType.FullRect);s.name=path;return s;}
    }
    [Serializable] public sealed class ReferenceImpactAudio
    {public string id;public string[] clips;[NonSerialized] public AudioClip[] loaded;}
    public static class WeaponReference
    {
        [Serializable] sealed class Data {public WeaponReferenceProfile[] weapons;public ProjectileReference[] projectiles;public ReferenceImpactAudio[] audio;}
        static Data data;static readonly Dictionary<string,WeaponReferenceProfile> weapons=new Dictionary<string,WeaponReferenceProfile>();
        static readonly Dictionary<string,ProjectileReference> projectiles=new Dictionary<string,ProjectileReference>();
        static void Load()
        {
            if(data!=null)return;data=JsonUtility.FromJson<Data>(Resources.Load<TextAsset>("weapon-reference").text);
            foreach(var w in data.weapons)weapons[w.id+":"+w.tier]=w;
            foreach(var p in data.projectiles)projectiles[p.id]=p;
        }
        public static WeaponReferenceProfile Profile(Equipment w)
        {Load();if(w==null)return null;weapons.TryGetValue(w.id+":"+w.tier,out var p);return p;}
        public static ProjectileReference Projectile(Equipment w)
        {Load();string path=w?.projectileScene??Profile(w)?.scene;if(string.IsNullOrEmpty(path)&&w?.specialSpawn==true)path="projectiles/bullet/bullet_projectile.tscn";if(string.IsNullOrEmpty(path))return null;projectiles.TryGetValue(path,out var p);return p;}
        public static ProjectileReference[] Projectiles {get{Load();return data.projectiles;}}
        public static WeaponReferenceProfile[] Profiles {get{Load();return data.weapons;}}
        public static void Apply(ReferenceCatalog catalog)
        {
            foreach(var w in catalog.data.equipment)
            {
                var p=Profile(w);if(p==null)continue;
                foreach(var secondary in p.secondary??Array.Empty<SecondaryReference>())
                    if(secondary.index<w.traits.Length&&w.traits[secondary.index].projectile!=null)w.traits[secondary.index].projectile.projectileScene=secondary.scene;
            }
        }
        public static AudioClip[] ImpactClips(bool critical,bool burning)
        {Load();var a=data.audio.Single(x=>x.id==(burning?"burn":critical?"crit":"hurt"));return a.loaded??(a.loaded=a.clips.Select(Resources.Load<AudioClip>).ToArray());}
        public static void Impact(GameAudio audio,Vector2 position,bool critical,bool burning)
        {audio.Play(critical?SoundCue.Critical:SoundCue.Impact,position,ImpactClips(critical,burning),1,.2f,true,burning?ContextSoundProfile.Burn:(int?)null);}
        public static void Fire(GameAudio audio,Equipment w,Vector2 position)
        {audio.Play(w.melee?SoundCue.Swing:SoundCue.Fire,position,w.sounds,Mathf.Pow(10,(Profile(w)?.soundDb??-5)/20),.2f,true,ContextSoundProfile.Weapon(w.id,w.melee));}
        public static float Speed(Equipment w,float rangeStat)
        {return Profile(w)?.speedWithRange==true?(int)Mathf.Clamp(w.projectileSpeed*100*(1+rangeStat/300),50,6000)/100f:w.projectileSpeed;}
        public static float Spread(Equipment w,float addedProjectiles,float roll)
        {return Mathf.Lerp(-1,1,roll)*Mathf.Max(0,w.spread+addedProjectiles*.1f);}
        public static int ProjectileCount(int baseCount,int addedProjectiles)=>baseCount>0?Mathf.Max(0,baseCount+addedProjectiles):baseCount;
        public static float Lifetime(float range,float speed,bool manual)=> (range+(manual?.5f:1))/Mathf.Max(.001f,speed);
        public static Vector2 AuthoredMuzzleOffset(Equipment weapon,float recoil=0)
        {
            var p=Profile(weapon);if(p?.muzzle==null||p.muzzle.Length!=2)return Vector2.zero;
            var point=new Vector2(p.muzzle[0],p.muzzle[1]);
            if(p.muzzleRecoil?.Length==2)point+=new Vector2(p.muzzleRecoil[0],p.muzzleRecoil[1])*recoil;
            return point;
        }
        public static Vector2 MuzzlePosition(Equipment weapon,Vector2 mount,Vector2 localDirection,float arrivalTime,float recoil=0)
        {
            float angle=Mathf.Atan2(localDirection.y,localDirection.x)*Mathf.Rad2Deg+ArrivalWeaponMotion.Rotation(arrivalTime);
            return mount+(Vector2)(Quaternion.Euler(0,0,angle)*AuthoredMuzzleOffset(weapon,recoil));
        }
        public static float DamageAfterHit(float damage,float reduction,float bonus)=>damage<=0?0:Mathf.Max(1,(int)(damage-damage*Math.Max(0,Math.Min(1,(double)reduction-bonus/100d))));
        public static int AttackType(Equipment w,int shot)
        {var p=Profile(w);return p==null?0:p.alternate?(p.attackType+shot-1)%2:p.attackType;}
    }

    public static class ReferenceBeam
    {
        public const float Duration=.2f;
        // Contents:scale keys at source 0/.2/.4 seconds, playback speed 2.
        // Godot 3 cubic interpolation duplicates the first/last neighbors.
        static Vector4 Curve(bool first)
        {
            float a=first?.1f:1,b=first?1:.00001f;
            float tangentA=(b-.1f)*.5f,tangentB=(.00001f-a)*.5f;
            return new Vector4(2*a-2*b+tangentA+tangentB,-3*a+3*b-2*tangentA-tangentB,tangentA,a);
        }
        static float Evaluate(Vector4 curve,float t)=>((curve.x*t+curve.y)*t+curve.z)*t+curve.w;
        public static float Thickness(float age)
        {
            if(age<=0)return .1f;if(age>=Duration)return .00001f;
            bool first=age<.1f;return Evaluate(Curve(first),(age-(first?0:.1f))/.1f);
        }
        public static float MaximumThickness(float from,float to)
        {
            if(to<0||from>=Duration||to<from)return 0;
            from=Mathf.Max(0,from);to=Mathf.Min(Duration,to);float result=0;
            for(int segment=0;segment<2;segment++)
            {
                float begin=segment*.1f,end=begin+.1f;if(to<begin||from>end)continue;
                float a=Mathf.Clamp01((from-begin)/.1f),b=Mathf.Clamp01((to-begin)/.1f);var curve=Curve(segment==0);
                result=Mathf.Max(result,Mathf.Max(Evaluate(curve,a),Evaluate(curve,b)));
                float discriminant=4*curve.y*curve.y-12*curve.x*curve.z;
                if(discriminant<0)continue;
                float root=Mathf.Sqrt(discriminant);
                float t=(-2*curve.y-root)/(6*curve.x);
                if(t>=a&&t<=b)result=Mathf.Max(result,Evaluate(curve,t));
                t=(-2*curve.y+root)/(6*curve.x);
                if(t>=a&&t<=b)result=Mathf.Max(result,Evaluate(curve,t));
            }
            return result;
        }
        public static Vector2 Half(ProjectileReference profile,float range,float width,float age)
            =>new Vector2(profile.half[0]*Mathf.Max(1,(range+2*width)/width),profile.half[1]*Thickness(age));
        public static Vector2 Center(ProjectileReference profile,float range,float width,float age)
            =>new Vector2(profile.hitOffset[0]*Mathf.Max(1,(range+2*width)/width),profile.hitOffset[1]*Thickness(age));
        static float HitAtThickness(ProjectileReference profile,float range,float width,float thickness,Vector2 origin,float angle,Vector2 target,float radius)
        {
            float scaleX=Mathf.Max(1,(range+2*width)/width);
            Vector2 center=new Vector2(profile.hitOffset[0]*scaleX,profile.hitOffset[1]*thickness);
            Vector2 half=new Vector2(profile.half[0]*scaleX,profile.half[1]*thickness);
            Vector2 local=Quaternion.Euler(0,0,-angle)*(target-origin);
            if(!ReferenceMeleeMotion.Overlaps(local,radius,center,0,half))return float.PositiveInfinity;
            // The beam appears all at once. Retain deterministic along-beam hit
            // ordering for our piercing resolution after testing its rectangle.
            return Mathf.Clamp01((local.x-radius)/Mathf.Max(.00001f,half.x*2));
        }
        public static float HitFraction(ProjectileReference profile,float range,float width,float age,Vector2 origin,float angle,Vector2 target,float radius)
            =>age<0||age>=Duration?float.PositiveInfinity:HitAtThickness(profile,range,width,Thickness(age),origin,angle,target,radius);
        public static float IntervalHitFraction(ProjectileReference profile,float range,float width,float from,float to,Vector2 origin,float angle,Vector2 target,float radius)
        {
            float thickness=MaximumThickness(from,to);
            return thickness<=0?float.PositiveInfinity:HitAtThickness(profile,range,width,thickness,origin,angle,target,radius);
        }

    }

    // Sprite animation is driven by the same paused simulation clock as collision.
    public sealed class WeaponProjectileView : MonoBehaviour
    {
        ProjectileReference data;SpriteRenderer sprite;Transform start,end;float range,lastAge,beamWidth;ParticleSystem trail;Material tint,trailMaterial;
        public float CapturedRange=>range;
        public float BeamWidth=>beamWidth;
        public void Initialize(ProjectileReference profile,float weaponRange)
        {
            data=profile;range=weaponRange;var root=GetComponent<SpriteRenderer>();root.enabled=false;
            sprite=new GameObject("Reference projectile").AddComponent<SpriteRenderer>();sprite.transform.SetParent(transform,false);sprite.sortingOrder=13;
            if(data.rail){tint=new Material(Resources.Load<Shader>("WeaponReferenceTone"));sprite.sharedMaterial=tint;}
            if(!string.IsNullOrEmpty(data.trail))CreateTrail();
            if(data.beam)
            {
                beamWidth=data.Frame(0).rect.width/100;
                start=Cap(data.start,"Beam start");end=Cap(data.end,"Beam end");
            }
            Pose(0);
        }
        Transform Cap(string path,string name)
        {var v=new GameObject(name).AddComponent<SpriteRenderer>();v.transform.SetParent(transform,false);v.sprite=ProjectileReference.MakeSprite(path,true);v.sortingOrder=13;return v.transform;}
        public void Pose(float age)
        {
            if(trail!=null)trail.Simulate(Mathf.Max(0,age-lastAge),false,false,true);lastAge=age;
            sprite.sprite=data.Frame(age);
            if(data.beam)
            {
                float thickness=ReferenceBeam.Thickness(age);
                float width=sprite.sprite.rect.width/100;
                float y=(-data.offset[1]/100-sprite.sprite.rect.height/200)*thickness;
                sprite.transform.localPosition=new Vector3(width,y,0);sprite.transform.localScale=new Vector3(Mathf.Max(1,range/width),thickness,1);
                start.localScale=end.localScale=new Vector3(1,thickness,1);start.localPosition=new Vector3(0,y,0);end.localPosition=new Vector3(range+width,y,0);
            }
            else
            {
                sprite.transform.localPosition=new Vector3(data.offset[0]/100,-data.offset[1]/100,0);
                sprite.transform.localScale=new Vector3(data.scale[0],data.scale[1],1);
            }
        }
        public void StopEmission(){if(trail!=null){var e=trail.emission;e.enabled=false;}}
        void CreateTrail()
        {
            trail=new GameObject("Reference trail").AddComponent<ParticleSystem>();trail.transform.SetParent(transform,false);trail.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var m=trail.main;m.playOnAwake=false;m.loop=true;m.simulationSpace=ParticleSystemSimulationSpace.World;m.maxParticles=data.rail?16:8;m.startLifetime=data.rail?.2f:1;m.startSpeed=data.rail?0:.05f;
            var texture=Resources.Load<Texture2D>(data.trail);float size=(data.rail?texture.width:texture.width/9f)/100*(data.rail?.2f:.55f);
            m.startSize=new ParticleSystem.MinMaxCurve(size*(data.rail?.55f:.75f),size);m.startRotation=new ParticleSystem.MinMaxCurve(0,Mathf.PI*2);
            var shape=trail.shape;shape.enabled=true;shape.shapeType=ParticleSystemShapeType.Circle;shape.radius=data.rail?.0809f:.1f;
            var emission=trail.emission;emission.rateOverTime=data.rail?80:8;
            var colors=trail.colorOverLifetime;colors.enabled=true;var gradient=new Gradient();
            if(data.rail)gradient.SetKeys(new[]{new GradientColorKey(Color.white*.298039f,0),new GradientColorKey(Color.black,1)},new[]{new GradientAlphaKey(1,0),new GradientAlphaKey(0,1)});
            else
            {
                gradient.SetKeys(new[]{new GradientColorKey(Color.white*.352941f,0),new GradientColorKey(new Color(1,.82f,0)*.352941f,.075f),new GradientColorKey(new Color(1,.63f,0)*.352941f,.225f),new GradientColorKey(new Color(1,.4f,0)*.352941f,.321f),new GradientColorKey(Color.red*.352941f,.406f),new GradientColorKey(new Color(.278f,.05f,.05f)*.352941f,.72f),new GradientColorKey(new Color(.21f,.149f,.149f)*.352941f,1)},new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(1,.075f),new GradientAlphaKey(1,.406f),new GradientAlphaKey(.686f,.72f),new GradientAlphaKey(0,1)});
                var force=trail.forceOverLifetime;force.enabled=true;force.space=ParticleSystemSimulationSpace.World;force.y=.5f;
                var sheet=trail.textureSheetAnimation;sheet.enabled=true;sheet.numTilesX=9;sheet.numTilesY=1;sheet.frameOverTime=new ParticleSystem.MinMaxCurve(1);sheet.cycleCount=3;
            }
            colors.color=gradient;trailMaterial=new Material(Resources.Load<Shader>("WeaponReferenceAdditive"));trailMaterial.mainTexture=texture;
            var renderer=trail.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=trailMaterial;renderer.sortingOrder=12;
        }
        void OnDestroy(){if(tint!=null)Destroy(tint);if(trailMaterial!=null)Destroy(trailMaterial);}
    }
}

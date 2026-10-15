using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Scrapshift
{
    [Serializable] public sealed class WeaponReferenceProfile
    {
        public string id,scene;public int tier,attackType;public bool alternate,returnDamage,speedWithRange,canBounce=true;
        public float accuracy=1,soundDb=-5;public float[] half,hitOffset;public SecondaryReference[] secondary;public ExplosionReference[] explosions;
    }
    [Serializable] public sealed class ExplosionReference
    {public int index;public string texture;public string[] clips;public float soundDb,radius;[NonSerialized] public Sprite sprite;[NonSerialized] public AudioClip[] audio;}
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
        {Load();string path=w?.projectileScene??Profile(w)?.scene;if(string.IsNullOrEmpty(path))return null;projectiles.TryGetValue(path,out var p);return p;}
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
        {return Profile(w)?.speedWithRange==true?Mathf.Clamp(w.projectileSpeed*(1+rangeStat/300),.5f,60):w.projectileSpeed;}
        public static float Spread(Equipment w,float addedProjectiles,float roll)
        {return Mathf.Lerp(-1,1,roll)*Mathf.Max(0,w.spread+addedProjectiles*.1f);}
        public static float Lifetime(float range,float speed,bool manual)=> (range+(manual?.5f:1))/Mathf.Max(.001f,speed);
        public static float DamageAfterHit(float damage,float reduction,float bonus)=>damage<=0?0:Mathf.Max(1,damage*(1-Mathf.Clamp01(reduction-bonus/100)));
        public static int AttackType(Equipment w,int shot)
        {var p=Profile(w);return p==null?0:p.alternate?(p.attackType+shot-1)%2:p.attackType;}
    }

    // Sprite animation is driven by the same paused simulation clock as collision.
    public sealed class WeaponProjectileView : MonoBehaviour
    {
        ProjectileReference data;SpriteRenderer sprite;Transform start,end;float range,lastAge;ParticleSystem trail;Material tint,trailMaterial;
        public void Initialize(ProjectileReference profile,float weaponRange)
        {
            data=profile;range=weaponRange;var root=GetComponent<SpriteRenderer>();root.enabled=false;
            sprite=new GameObject("Reference projectile").AddComponent<SpriteRenderer>();sprite.transform.SetParent(transform,false);sprite.sortingOrder=13;
            if(data.rail){tint=new Material(Resources.Load<Shader>("WeaponReferenceTone"));sprite.sharedMaterial=tint;}
            if(!string.IsNullOrEmpty(data.trail))CreateTrail();
            if(data.beam)
            {
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
                float thickness=age<.1f?Mathf.Lerp(.1f,1,age/.1f):Mathf.Lerp(1,.00001f,(age-.1f)/.1f);
                float width=sprite.sprite.rect.width/100;
                sprite.transform.localPosition=Vector3.right*width;sprite.transform.localScale=new Vector3(Mathf.Max(1,range/width),thickness,1);
                start.localScale=end.localScale=new Vector3(1,thickness,1);end.localPosition=Vector3.right*(range+width);
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

using System.Collections.Generic;
using UnityEngine;
namespace Scrapshift
{
    public struct MeleePose
    {public Vector2 offset;public float angle;public bool active;}
    public static class ReferenceMeleeMotion
    {
        static float Ease(float x)=>x>=1?1:1-Mathf.Pow(2,-10*Mathf.Clamp01(x));
        public static float StrikeDuration(float speed,float range)=>
            (Mathf.Max(.01f,.2f-speed/10)+Mathf.Max(0,range*100/Mathf.Clamp(70*(1+speed/3),70,120))*.15f)/2;
        public static float ReturnDuration(float speed)=>speed>0?.2f/(1+speed*3):.2f;
        public static MeleePose Pose(float age,float recoilTime,float recoil,float strike,float back,float range,bool sweep,bool flip,bool returnDamage)
        {
            if(age>=recoilTime+strike+back-.000001f)return new MeleePose();
            float side=(flip?1:-1)*range/2,angle=(flip?1:-1)*162;
            var windup=new Vector2(-recoil,sweep?side:0);var result=new MeleePose();
            if(age<recoilTime){float t=Ease(age/recoilTime);result.offset=windup*t;result.angle=sweep?angle*t:0;return result;}
            age-=recoilTime;result.active=age<strike||returnDamage&&age<strike+back;
            var peak=new Vector2(range*(sweep?.75f:1),0);var finish=sweep?new Vector2(-recoil,-side):peak;
            if(age<strike)
            {
                float t=age/strike;
                if(sweep){result.offset=t<.5f?Vector2.Lerp(windup,peak,t*2):Vector2.Lerp(peak,finish,(t-.5f)*2);result.angle=Mathf.Lerp(angle,-angle,t);}
                else result.offset=Vector2.Lerp(windup,peak,Ease(t));
            }
            else {float t=Ease((age-strike)/back);result.offset=Vector2.Lerp(finish,Vector2.zero,t);result.angle=sweep?Mathf.Lerp(-angle,0,t):0;}
            return result;
        }
        public static bool Overlaps(Vector2 point,float radius,Vector2 center,float angle,Vector2 half)
        {
            Vector2 local=Quaternion.Euler(0,0,-angle)*(point-center);
            var nearest=new Vector2(Mathf.Clamp(local.x,-half.x,half.x),Mathf.Clamp(local.y,-half.y,half.y));
            return (local-nearest).sqrMagnitude<=radius*radius;
        }
    }
    public sealed partial class ArenaGame
    {
        sealed class MeleeAttack
        {
            public float age,recoilTime,recoil,strike,back,range,angle;public bool sweep,flip,returnDamage,sounded;
            public Vector2 direction;public readonly HashSet<Enemy> hit=new HashSet<Enemy>();
        }
        readonly MeleeAttack[] meleeAttacks=new MeleeAttack[24];
        void BeginMeleeAttack(int index,Equipment w,Vector2 direction,float distance)
        {
            var p=WeaponReference.Profile(w);bool sweep=WeaponReference.AttackType(w,weaponShots[index])==1;
            float range=Rules.WeaponRange(w);if(sweep)range=Mathf.Min(range,Mathf.Max(2.5f,distance));
            float speed=Rules.WeaponAttackSpeed(w);var attack=new MeleeAttack{direction=direction,angle=Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg,
                range=range,sweep=sweep,flip=direction.x<0,returnDamage=p?.returnDamage??false,
                recoilTime=w.recoilHalf/Mathf.Max(1,1+speed),recoil=w.recoil/Mathf.Max(1,1+speed),strike=ReferenceMeleeMotion.StrikeDuration(speed,range),back=ReferenceMeleeMotion.ReturnDuration(speed)};
            meleeAttacks[index]=attack;swings[index]=attack.recoilTime+attack.strike+attack.back;
        }
        void TickMeleeAttack(int index,Equipment w,Vector2 mount,float dt)
        {
            var a=meleeAttacks[index];if(a==null)return;
            var profile=WeaponReference.Profile(w);float until=Mathf.Min(a.age+dt,a.recoilTime+a.strike+a.back);
            // Sample the moving source-shaped hitbox, so fast sweeps cannot skip targets at low frame rates.
            for(float t=a.age;t<=until+.00001f;t=Mathf.Min(until,t+1f/240))
            {
                var pose=ReferenceMeleeMotion.Pose(t,a.recoilTime,a.recoil,a.strike,a.back,a.range,a.sweep,a.flip,a.returnDamage);
                if(pose.active)
                {
                    if(!a.sounded){WeaponReference.Fire(sound,w,playerPos);a.sounded=true;}
                    float angle=a.angle+pose.angle;var rotation=Quaternion.Euler(0,0,a.angle);
                    Vector2 origin=mount+(Vector2)(rotation*(pose.offset+Vector2.right*ActorPresentation.WeaponPivot(w.id)));
                    Vector2 offset=profile==null?Vector2.zero:new Vector2(profile.hitOffset[0],profile.hitOffset[1]);
                    Vector2 center=origin+(Vector2)(Quaternion.Euler(0,0,angle)*offset);
                    Vector2 half=profile==null?new Vector2(.32f,.1f):new Vector2(profile.half[0],profile.half[1]);
                    foreach(var enemy in enemies.ToArray())if(enemy.marker==null&&!a.hit.Contains(enemy)&&ReferenceMeleeMotion.Overlaps(HurtCenter(enemy),HurtRadius(enemy),center,angle,half))
                    {a.hit.Add(enemy);Damage(enemies.IndexOf(enemy),Rules.WeaponDamage(w),w.crit,w.knockback,a.direction,w);orbitHits++;}
                }
                if(t>=until)break;
            }
            a.age=until;var current=ReferenceMeleeMotion.Pose(a.age,a.recoilTime,a.recoil,a.strike,a.back,a.range,a.sweep,a.flip,a.returnDamage);
            var view=equipmentViews[index];view.transform.position=mount+(Vector2)(Quaternion.Euler(0,0,a.angle)*(current.offset+Vector2.right*ActorPresentation.WeaponPivot(w.id)));
            view.transform.rotation=Quaternion.Euler(0,0,a.angle+current.angle);view.GetComponent<SpriteRenderer>().flipY=a.flip;
            if(a.age>=a.recoilTime+a.strike+a.back)meleeAttacks[index]=null;
        }
        void ConfigureReferenceShot(Shot shot,bool manual=false)
        {
            var p=WeaponReference.Projectile(shot.weapon);if(p==null||shot.structureSpec!=null)return;
            shot.projectileRef=p;shot.obj.transform.localScale=Vector3.one;
            shot.projectileView=shot.obj.AddComponent<WeaponProjectileView>();shot.projectileView.Initialize(p,Rules.WeaponRange(shot.weapon));
            shot.life=WeaponReference.Lifetime(Rules.WeaponRange(shot.weapon),shot.velocity.magnitude,manual);
            if(p.stop>0)shot.life=Mathf.Min(shot.life,p.stop);
            if(p.beam)shot.life=p.stop;
            if(p.slow)shot.slow=2;if(p.delayed)shot.hitDelay=UnityEngine.Random.Range(0,.05f);
            shot.sinusoidal=new Vector2(p.sinusoidal[0],p.sinusoidal[1]);shot.sinSpeed=new Vector2(p.sinSpeed[0],p.sinSpeed[1]);
            shot.critBounces=(int)(shot.weapon.Trait("bounce_on_crit")?.value??0);shot.critPierces=(int)(shot.weapon.Trait("pierce_on_crit")?.value??0);
            if(WeaponReference.Profile(shot.weapon)?.canBounce==false)shot.bounces=0;
        }
        sealed class ReferenceExplosion
        {public Vector2 position;public float age,radius,damage;public Equipment weapon;public GameObject obj;public readonly HashSet<Enemy> hit=new HashSet<Enemy>();}
        readonly List<ReferenceExplosion> weaponExplosions=new List<ReferenceExplosion>();
        void SpawnReferenceExplosion(Enemy victim,Equipment weapon,WeaponTrait trait,float damage)
        {
            var profile=WeaponReference.Profile(weapon);int index=System.Array.IndexOf(weapon.traits,trait);
            var reference=System.Array.Find(profile?.explosions??System.Array.Empty<ExplosionReference>(),x=>x.index==index);
            float scale=Mathf.Max(.1f,trait.radius*(1+Rules.Stat("explosion_size")/100));
            var explosion=new ReferenceExplosion{position=victim.pos,radius=(reference?.radius??1.4734f)*scale,damage=damage*Mathf.Max(0,1+Rules.Stat("explosion_damage")/100),weapon=weapon};explosion.hit.Add(victim);
            if(reference!=null)
            {
                if(reference.sprite==null)reference.sprite=ProjectileReference.MakeSprite(reference.texture);
                explosion.obj=SpriteObject("Weapon explosion",reference.sprite,victim.pos,14);explosion.obj.transform.localScale=Vector3.one*scale*1.36719f;
                explosion.obj.GetComponent<SpriteRenderer>().color=new Color(.788235f,.788235f,.788235f,.392157f);
                if(reference.soundDb>-50)
                {
                    if(reference.audio==null)reference.audio=System.Array.ConvertAll(reference.clips,path=>Resources.Load<AudioClip>(path));
                    sound.Play(SoundCue.Impact,victim.pos,reference.audio,Mathf.Pow(10,reference.soundDb/20),.2f,true);
                }
            }
            weaponExplosions.Add(explosion);
        }
        void TickReferenceExplosions(float dt)
        {
            foreach(var explosion in weaponExplosions.ToArray())
            {
                int before=Rules.Kills;
                foreach(var e in enemies.ToArray())if(e.marker==null&&!explosion.hit.Contains(e)&&Vector2.Distance(HurtCenter(e),explosion.position+Vector2.down*.03f)<explosion.radius+HurtRadius(e))
                {explosion.hit.Add(e);Damage(enemies.IndexOf(e),explosion.damage,explosion.weapon.crit,0,e.pos-explosion.position,explosion.weapon,true);}
                Rules.RecordExplosionKills(Rules.Kills-before);explosion.age+=dt;
                if(explosion.age>=.05f){if(explosion.obj!=null)Destroy(explosion.obj);weaponExplosions.Remove(explosion);}
            }
        }
        Vector2 ProjectileHitOffset(Shot shot)
        {
            var p=shot.projectileRef;if(p==null||p.beam)return Vector2.zero;
            float angle=shot.obj.transform.eulerAngles.z;return Quaternion.Euler(0,0,angle)*new Vector2(p.hitOffset[0],p.hitOffset[1]);
        }
    }
}

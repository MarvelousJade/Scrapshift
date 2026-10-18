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
            // weapon.gd:get_direction follows the existing target while shooting.
            // UpdateEquipment freezes weaponDirections for manual melee instead.
            if(weaponDirections[index].sqrMagnitude>0)
                a.angle=Mathf.Atan2(weaponDirections[index].y,weaponDirections[index].x)*Mathf.Rad2Deg;
            var profile=WeaponReference.Profile(w);float until=Mathf.Min(a.age+dt,a.recoilTime+a.strike+a.back);
            // Both source melee coroutines play their cue when recoil completes,
            // independently of whether a physics sample sees the active hitbox.
            if(!a.sounded&&until>=a.recoilTime){WeaponReference.Fire(sound,w,playerPos);a.sounded=true;}
            // Source Area2D contacts use the current physics pose. Additional
            // samples between ticks would introduce contacts absent in Godot.
            {
                var pose=ReferenceMeleeMotion.Pose(until,a.recoilTime,a.recoil,a.strike,a.back,a.range,a.sweep,a.flip,a.returnDamage);
                if(pose.active)
                {
                    float angle=a.angle+pose.angle+ArrivalWeaponMotion.Rotation(WeaponArrivalTime);var rotation=Quaternion.Euler(0,0,a.angle+ArrivalWeaponMotion.Rotation(WeaponArrivalTime));
                    Vector2 origin=mount+(Vector2)(rotation*(pose.offset+Vector2.right*ActorPresentation.WeaponPivot(w.id)));
                    Vector2 offset=profile==null?Vector2.zero:new Vector2(profile.hitOffset[0],profile.hitOffset[1]);
                    Vector2 center=origin+(Vector2)(Quaternion.Euler(0,0,angle)*offset);
                    Vector2 half=profile==null?new Vector2(.32f,.1f):new Vector2(profile.half[0],profile.half[1]);
                    foreach(var enemy in enemies.ToArray())if(enemy.marker==null&&!a.hit.Contains(enemy)&&ReferenceMeleeMotion.Overlaps(HurtCenter(enemy),HurtRadius(enemy),center,angle,half))
                    {a.hit.Add(enemy);DamageWithWeaponIndex(enemies.IndexOf(enemy),Rules.WeaponDamage(w),w.crit,w.knockback,a.direction,w,false,false,false,index);orbitHits++;}
                }
            }
            a.age=until;var current=ReferenceMeleeMotion.Pose(a.age,a.recoilTime,a.recoil,a.strike,a.back,a.range,a.sweep,a.flip,a.returnDamage);
            var view=equipmentViews[index];view.transform.position=mount+(Vector2)(Quaternion.Euler(0,0,a.angle+ArrivalWeaponMotion.Rotation(WeaponArrivalTime))*(current.offset+Vector2.right*ActorPresentation.WeaponPivot(w.id)));
            view.transform.rotation=Quaternion.Euler(0,0,a.angle+current.angle+ArrivalWeaponMotion.Rotation(WeaponArrivalTime));view.GetComponent<SpriteRenderer>().flipY=a.flip;
            if(a.age>=a.recoilTime+a.strike+a.back)meleeAttacks[index]=null;
        }
        void ConfigureReferenceShot(Shot shot,bool manual=false)
        {
            if(ConfigureSupportShot(shot))return;
            var p=WeaponReference.Projectile(shot.weapon);if(p==null||shot.structureSpec!=null)return;
            shot.projectileRef=p;shot.obj.transform.localScale=Vector3.one;
            float range=shot.structure?Rules.PetRange(shot.weapon,true):shot.weapon.pet?Rules.PetRange(shot.weapon):Rules.WeaponRange(shot.weapon);
            shot.projectileView=shot.obj.AddComponent<WeaponProjectileView>();shot.projectileView.Initialize(p,range);
            shot.life=WeaponReference.Lifetime(range,shot.velocity.magnitude,manual);
            if(p.stop>0)shot.life=Mathf.Min(shot.life,p.stop);
            if(p.beam)shot.life=p.stop;
            if(p.slow)shot.slow=2;if(p.delayed){shot.hitDelay=UnityEngine.Random.Range(0,.05f);shot.delayedHitboxPending=true;}
            shot.sinusoidal=new Vector2(p.sinusoidal[0],p.sinusoidal[1]);shot.sinSpeed=new Vector2(p.sinSpeed[0],p.sinSpeed[1]);
            shot.critBounces=(int)(shot.weapon.Trait("bounce_on_crit")?.value??0);shot.critPierces=(int)(shot.weapon.Trait("pierce_on_crit")?.value??0);
            if(WeaponReference.Profile(shot.weapon)?.canBounce==false)shot.bounces=0;
        }
        sealed class ReferenceExplosion
        {public Vector2 position;public float age,radius,damage;public Equipment weapon;public BurnPayload burnPayload;public int weaponIndex=-1,kills;public bool structure;public GameObject obj;public readonly HashSet<Enemy> hit=new HashSet<Enemy>();}
        readonly List<ReferenceExplosion> weaponExplosions=new List<ReferenceExplosion>();
        bool resolvingWeaponExplosion;
        void QueueSupportExplosion(Vector2 sourceOrigin,Equipment stats,float damage,float radius,bool structure,Enemy directHit=null,BurnPayload burnPayload=null)
        {
            var explosion=new ReferenceExplosion{position=sourceOrigin,weapon=stats,damage=damage,radius=radius,structure=structure,burnPayload=burnPayload??Rules.CaptureBurn(stats,structure)};
            if(directHit!=null)explosion.hit.Add(directHit);
            weaponExplosions.Add(explosion);
        }
        bool TryInterceptWeaponExplosion(Enemy victim,Equipment weapon,float damage,bool secondary,int weaponIndex,BurnPayload burnPayload=null)
        {
            if(secondary||weapon?.traits==null)return false;
            bool intercepted=false;
            foreach(var trait in weapon.traits)
                if((trait.key=="effect_explode"||trait.key=="effect_explode_custom"||trait.key=="effect_explode_melee")&&SpecialWeaponRules.Explodes(trait,UnityEngine.Random.value))
                {SpawnReferenceExplosion(victim,weapon,trait,damage,false,weaponIndex,burnPayload);intercepted=true;}
            if(!intercepted)return false;
            // hurt_area_entered_deferred still emits hit_something(self, 0).
            // For melee this independently rolls the weapon callback explosion.
            ApplyStarterHitEffects(victim,weapon,false,false,false,false,damage,weaponIndex,burnPayload);
            return true;
        }
        void SpawnReferenceExplosion(Enemy victim,Equipment weapon,WeaponTrait trait,float damage,bool excludeVictim=true,int weaponIndex=-1,BurnPayload burnPayload=null)
        {
            var profile=WeaponReference.Profile(weapon);int index=System.Array.IndexOf(weapon.traits,trait);
            var reference=System.Array.Find(profile?.explosions??System.Array.Empty<ExplosionReference>(),x=>x.index==index);
            float scale=Mathf.Max(.1f,trait.radius*(1+Rules.Stat("explosion_size")/100));
            // Explosive percentage is already included additively in the weapon
            // damage stat before contact, even when this explosion roll fails.
            Vector2 position=EnemyOrigin(victim);
            var explosion=new ReferenceExplosion{position=position,radius=(reference?.radius??1.4734f)*scale,damage=damage,weapon=weapon,weaponIndex=weaponIndex,burnPayload=burnPayload??Rules.CaptureBurn(weapon)};if(excludeVictim)explosion.hit.Add(victim);
            if(reference!=null)
            {
                if(reference.sprite==null)reference.sprite=ProjectileReference.MakeSprite(reference.texture);
                explosion.obj=SpriteObject("Weapon explosion",reference.sprite,position,14);explosion.obj.transform.localScale=Vector3.one*scale*1.36719f;
                explosion.obj.GetComponent<SpriteRenderer>().color=new Color(.788235f,.788235f,.788235f,.392157f);
                if(reference.soundDb>-50)
                {
                    if(reference.audio==null)reference.audio=System.Array.ConvertAll(reference.clips,path=>Resources.Load<AudioClip>(path));
                    sound.Play(SoundCue.Impact,position,reference.audio,Mathf.Pow(10,reference.soundDb/20),.2f,true,ContextSoundProfile.Explosion);
                }
            }
            weaponExplosions.Add(explosion);
        }
        void TickReferenceExplosions(float dt)
        {
            foreach(var explosion in weaponExplosions.ToArray())
            {
                int before=Rules.Kills;
                bool previous=resolvingWeaponExplosion;resolvingWeaponExplosion=true;
                try
                {
                    foreach(var e in enemies.ToArray())if(e.marker==null&&!explosion.hit.Contains(e)&&SupportBehavior.ExplosionOverlaps(explosion.position,explosion.radius,e.type.id,e.pos))
                    {explosion.hit.Add(e);DamageWithWeaponIndex(enemies.IndexOf(e),explosion.damage,explosion.weapon.crit,0,e.pos-explosion.position,explosion.weapon,true,false,explosion.structure,explosion.weaponIndex,explosion.burnPayload);}
                }
                finally{resolvingWeaponExplosion=previous;}
                explosion.kills+=Rules.Kills-before;Rules.RecordExplosionKills(explosion.kills);explosion.age+=dt;
                if(explosion.age>=.05f){if(explosion.obj!=null)Destroy(explosion.obj);weaponExplosions.Remove(explosion);}
            }
        }
        Vector2 ProjectileHitOffset(Shot shot)
        {
            var p=shot.projectileRef;if(p==null||p.beam)return Vector2.zero;
            float angle=shot.obj.transform.eulerAngles.z;return Quaternion.Euler(0,0,angle)*new Vector2(p.hitOffset[0],p.hitOffset[1]);
        }
        float ReferenceShotHitFraction(Shot shot,Enemy enemy,Vector2 previous,float previousAge)
        {
            if(shot.delayedHitboxPending||shot.hitDelay>0)return float.PositiveInfinity;
            float elapsed=shot.age-previousAge;
            float activeElapsed=Mathf.Min(elapsed,Mathf.Max(0,shot.life+elapsed));
            if(shot.projectileRef?.beam==true)
            {
                var view=shot.projectileView;
                float range=view!=null?view.CapturedRange:Rules.WeaponRange(shot.weapon);
                float width=view!=null?view.BeamWidth:1;
                return ReferenceBeam.HitFraction(shot.projectileRef,range,width,previousAge+activeElapsed,shot.pos,shot.obj.transform.eulerAngles.z,HurtCenter(enemy),HurtRadius(enemy));
            }
            Vector2 offset=ProjectileHitOffset(shot);
            // The supplied projectile's Area2D is sampled at physics positions;
            // it has no swept collision. Native flame-cap grazing evidence
            // demonstrates that sweeping introduces contacts the source misses.
            float radius=HurtRadius(enemy)+(shot.projectileRef?.radius??CombatGeometry.BulletRadius);
            // An ordinary source hitbox is already enabled when spawned. Keep
            // that first overlap snapshot without sweeping the intervening path.
            if(previousAge<=0&&shot.projectileRef?.delayed!=true&&(HurtCenter(enemy)-previous-offset).sqrMagnitude<=radius*radius)return 0;
            return (HurtCenter(enemy)-shot.pos-offset).sqrMagnitude<=radius*radius?0:float.PositiveInfinity;
        }
    }
}

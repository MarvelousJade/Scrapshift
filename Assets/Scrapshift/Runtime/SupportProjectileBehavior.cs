using System;
using System.Linq;
using UnityEngine;

namespace Scrapshift
{
    // Reuse source projectile geometry without replacing Scrapshift support art.
    public static class SupportProjectileBehavior
    {
        const string Bullet="projectiles/bullet/bullet_projectile.tscn";
        const string Flame="projectiles/flames/flame_projectile.tscn";
        static ProjectileReference blueFlame,healing,tyler;
        static ProjectileReference Existing(string scene)=>WeaponReference.Projectiles.Single(p=>p.id==scene);
        static ProjectileReference Copy(string scene,string id)
        {
            var result=JsonUtility.FromJson<ProjectileReference>(JsonUtility.ToJson(Existing(scene)));result.id=id;return result;
        }
        public static string PetScene(string kind,bool melee)=>melee?null:kind=="blazemander"?Flame:kind=="catling_gun"||kind=="bot_o_mine"?Bullet:null;
        public static string PetArtKind(Equipment weapon)=>weapon?.pet==true&&weapon.projectileScene==Flame?"pet_flame":null;
        public static ProjectileReference Structure(string kind)
        {
            switch(kind)
            {
                case "turret":case "builder":return Existing(Bullet);
                case "rocket":return Existing("projectiles/rocket/rocket_projectile.tscn");
                case "laser":return Existing("projectiles/bullet_laser/bullet_laser.tscn");
                case "flame":
                    return blueFlame??(blueFlame=Copy(Flame,"projectiles/blue_flames/blue_flame_projectile.tscn"));
                case "healing":
                    if(healing==null)
                    {
                        healing=Copy(Bullet,"projectiles/bullet_medical/turret_healing/bullet_turret_healing.tscn");
                        healing.radius=1.04f;healing.half=new[]{1.04f,1.04f};healing.hitOffset=new[]{0f,0f};
                    }
                    return healing;
                case "tyler":
                    if(tyler==null)
                    {
                        tyler=Copy("projectiles/bullet_lightning/lightning_projectile.tscn","projectiles/bullet_lightning/delayed_lightning_projectile.tscn");
                        tyler.delayed=true;
                    }
                    return tyler;
                default:return null;
            }
        }
        public static float Speed(float speed,string kind,float range)
            =>kind=="flame"?(int)Mathf.Clamp(speed*100*(1+range/300),50,6000)/100f:speed;
    }
    public sealed partial class ArenaGame
    {
        bool ConfigureSupportShot(Shot shot)
        {
            if(shot.structureSpec==null&&shot.weapon?.pet!=true)return false;
            var profile=shot.structureSpec!=null?SupportProjectileBehavior.Structure(shot.structureSpec.kind):WeaponReference.Projectile(shot.weapon);
            if(profile==null)return false;
            shot.projectileRef=profile;shot.delayedHitboxPending=profile.delayed;
            float range=Rules.PetRange(shot.weapon,shot.structure);
            float speed=SupportProjectileBehavior.Speed(shot.weapon.projectileSpeed,shot.structureSpec?.kind,Rules.Stat("range"));
            shot.velocity=shot.velocity.normalized*speed;
            shot.life=WeaponReference.Lifetime(range,speed,false);
            if(profile.stop>0)shot.life=Mathf.Min(shot.life,profile.stop);
            if(profile.delayed&&shot.structureSpec?.delayed!=true)shot.hitDelay=UnityEngine.Random.Range(0,.05f);
            shot.sinusoidal=new Vector2(profile.sinusoidal[0],profile.sinusoidal[1]);shot.sinSpeed=new Vector2(profile.sinSpeed[0],profile.sinSpeed[1]);
            return true;
        }
    }
}

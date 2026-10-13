using System.Linq;
using UnityEngine;
namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        void WanderPet(Pet p,float speed,float dt)
        {
            if(Vector2.Distance(p.pos,p.destination)<.1f)p.destination=RandomPosition(playerPos,0);
            p.pos=Vector2.MoveTowards(p.pos,p.destination,speed*dt);
        }
        Enemy PetRangedTarget(Vector2 pos,float range)
        {
            Enemy best=null;float distance=range*range;
            foreach(var e in enemies)if(e.marker==null&&e.hp>0&&e.type.role!="Tree")
            {float d=(e.pos-pos).sqrMagnitude;if(d<distance){distance=d;best=e;}}
            return best;
        }
        Enemy SelectPetTarget(Pet p)
        {
            bool fire=p.spec.kind=="blazemander";float range=fire?5:6;
            bool Valid(Enemy e)=>e!=null&&enemies.Contains(e)&&e.hp>0&&e.marker==null&&e.type.role!="Tree"&&SupportBehavior.MayReserve(p.spec.kind,e.type.role=="Boss"||e.type.role=="Elite",e.type.role=="Elite",pets.Any(o=>o!=p&&o.spec.kind==p.spec.kind&&o.target==e));
            bool nearby=Valid(p.target)&&(p.target.pos-p.pos).sqrMagnitude<range*range;
            if(Valid(p.target)&&(!fire||p.target.burnTicks==0)&& (nearby||!enemies.Any(e=>Valid(e)&&(e.pos-p.pos).sqrMagnitude<range*range)))return p.target;
            p.target=null;Enemy best=null;float score=float.PositiveInfinity;
            foreach(var e in enemies)
            {
                if(!Valid(e))continue;float d=(e.pos-p.pos).sqrMagnitude;
                if(d>range*range){if(!fire&&best==null)best=e;continue;}
                if(fire&&e.burnTicks>0)d+=10000;
                if(d<score){score=d;best=e;}
            }
            p.target=best;return best;
        }
        void PetMelee(Pet p)
        {
            float radius=p.spec.kind=="ratzilla"?.3339f:.498f;
            foreach(var enemy in enemies.ToArray())if(enemy.marker==null&&enemy.type.role!="Tree"&&Vector2.Distance(enemy.pos,p.pos)<radius+HurtRadius(enemy))
            {p.attackPose=.3f;Damage(enemies.IndexOf(enemy),Rules.WeaponDamage(p.weapon),p.weapon.crit,p.weapon.knockback,enemy.pos-p.pos,p.weapon,true);}
        }
        void FirePet(Pet p,Vector2 origin,Vector2 target,Equipment weapon)
        {
            float angle=Mathf.Atan2(target.y-origin.y,target.x-origin.x);
            for(int i=0;i<weapon.pellets;i++)
            {float a=angle+UnityEngine.Random.Range(-weapon.spread,weapon.spread);StarterProjectile(origin,new Vector2(Mathf.Cos(a),Mathf.Sin(a)),weapon,true,structure:p.spec.structure);}
            p.attackPose=.3f;OriginalSupportAudio.Play(sound,"shot:"+p.spec.kind,origin);
        }
        void TickCatGuns(Pet p,float dt)
        {
            Vector2 left=p.rig!=null?p.rig.GunOrigin(0):p.pos+Vector2.left*.2f;
            Vector2 right=p.rig!=null?p.rig.GunOrigin(1):p.pos+Vector2.right*.2f;
            var lt=PetRangedTarget(p.rig!=null?p.rig.GunMount(0):left,p.weapon.range);var rt=PetRangedTarget(p.rig!=null?p.rig.GunMount(1):right,p.weapon.range);
            float interval=SupportBehavior.CatInterval(p.spec.stats.interval,p.boosted);
            if(p.clock<=0&&lt!=null)
            {FirePet(p,left,lt.pos,p.weapon);p.clock=interval;p.rightClock=interval/2;p.rig?.GunKick(0);}
            else if(p.rightClock<=0&&rt!=null)
            {FirePet(p,right,rt.pos,p.weapon);p.rightClock=interval;p.clock=interval/2;p.rig?.GunKick(1);}
            p.rig?.TickGunRecoil(dt,reducedMotion);
        }
        void PoseCatGuns(Pet p)
        {
            for(int i=0;i<2;i++)
            {Vector2 origin=p.rig.GunMount(i);var target=PetRangedTarget(origin,p.weapon.range);if(target!=null)p.rig.AimGun(i,target.pos-origin);}
        }
    }
}

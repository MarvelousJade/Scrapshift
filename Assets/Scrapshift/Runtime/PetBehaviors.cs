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
            {float d=(e.pos+CombatGeometry.OriginOffset(e.type.id)-pos).sqrMagnitude;if(d<distance){distance=d;best=e;}}
            return best;
        }
        Enemy SelectPetTarget(Pet p)
        {
            bool fire=p.spec.kind=="blazemander";float range=fire?5:6;
            bool Alive(Enemy e)=>e!=null&&enemies.Contains(e)&&e.hp>0&&e.marker==null&&e.type.role!="Tree";
            bool Valid(Enemy e)=>Alive(e)&&SupportBehavior.MayReserve(p.spec.kind,e.type.role=="Boss"||e.type.role=="Elite",e.type.role=="Elite",pets.Any(o=>o!=p&&o.spec.kind==p.spec.kind&&o.target==e));
            p.targetRange.RemoveAll(e=>!Alive(e)||!SupportBehavior.PetBodyZone(p.pos,range,e.type.id,e.pos));
            foreach(var e in enemies)
            {
                if(!Alive(e)||p.targetRange.Contains(e)||!SupportBehavior.PetBodyZone(p.pos,range,e.type.id,e.pos))continue;
                // Source Range.body_entered clears an existing target only when
                // the range was empty. Exiting the range does not clear it.
                if(p.targetRange.Count==0)p.target=null;
                p.targetRange.Add(e);
            }
            if(Alive(p.target)&&(!fire||p.target.burnTicks==0||!p.targetRange.Any(e=>Alive(e)&&e.burnTicks==0)))return p.target;
            p.target=null;Enemy best=null;float score=float.PositiveInfinity;
            Vector2 origin=p.pos+SupportBehavior.PetOriginOffset;
            foreach(var e in p.targetRange)
            {
                if(!Valid(e))continue;float d=(e.pos+CombatGeometry.OriginOffset(e.type.id)-origin).sqrMagnitude;
                if(fire&&e.burnTicks>0)d+=10000;
                if(d<score){score=d;best=e;}
            }
            if(best==null&&!fire)best=enemies.FirstOrDefault(Valid);
            p.target=best;return best;
        }
        void PetMelee(Pet p,bool persistent=false)
        {
            foreach(var enemy in enemies.ToArray())if(enemy.marker==null&&enemy.hp>0&&SupportBehavior.PetHitOverlaps(p.spec.kind,p.pos,enemy.type.id,enemy.pos)&&(!persistent||p.meleeHits.Add(enemy)))
            {p.attackPose=.3f;Damage(enemies.IndexOf(enemy),Rules.WeaponDamage(p.weapon),p.weapon.crit,p.weapon.knockback,enemy.pos-p.pos,p.weapon,true);}
        }
        bool AdvancePetAttack(Pet p,float dt)
        {
            bool ended=p.meleePulse||p.rangedPulse;
            if(p.meleePulse){p.meleePulse=false;p.clock=Rules.PetCooldown(p.spec.stats.interval);p.meleeHitActive=false;p.meleeHits.Clear();}
            if(p.rangedPulse){p.rangedPulse=false;p.secondaryClock=Rules.PetCooldown(p.spec.secondary.interval);}
            // Blazemander's attack flag resets on the next physics frame. Its
            // independent .15s timer then enables the hitbox until the next reset.
            if(p.pendingMelee){p.meleeDelay-=dt;if(p.meleeDelay<=0){p.pendingMelee=false;p.meleeHitActive=true;}}
            return ended;
        }
        bool AdvanceScapegoatRevival(Pet p,float dt)
        {
            if(p.health>=p.maxHealth)return true;
            p.reviveHealth+=p.maxHealth/p.spec.revive*dt;
            p.health=Mathf.Min(p.maxHealth,Mathf.Floor(p.reviveHealth));
            return false;
        }
        void TickLootwormAttack(Pet p)
        {
            if(p.treeDamage<=0)p.treeDamage=Rules.WeaponDamage(p.weapon);
            bool Near(Enemy e)=>enemies.Contains(e)&&e.hp>0&&e.marker==null&&e.type.role=="Tree"&&
                (e.pos+CombatGeometry.BodyOffset(e.type.id)-p.pos-SupportBehavior.PetHitOffset("lootworm")).sqrMagnitude<=Mathf.Pow(.6f+CombatGeometry.Enemy(e.type.id).body,2);
            p.targetRange.RemoveAll(e=>!Near(e));
            foreach(var e in enemies)if(Near(e)&&!p.targetRange.Contains(e))
            {p.targetRange.Add(e);p.treeDamage=Mathf.Max(p.treeDamage,e.maxHp/5);}
            if(p.meleePulse){p.meleePulse=false;p.clock=Rules.PetCooldown(p.spec.stats.interval);return;}
            if(p.clock>0||p.targetRange.Count==0)return;
            p.meleePulse=true;p.attackPose=.3f;
            // hit_something disables this pulse's damage after its first hit.
            var hit=enemies.FirstOrDefault(e=>e.marker==null&&e.hp>0&&SupportBehavior.PetHitOverlaps("lootworm",p.pos,e.type.id,e.pos));
            if(hit!=null)Damage(enemies.IndexOf(hit),p.treeDamage,p.weapon.crit,p.weapon.knockback,hit.pos-p.pos,p.weapon,true);
        }
        void FirePet(Pet p,Vector2 origin,Vector2 target,Equipment weapon)
        {
            Vector2 aim=target-p.pos-SupportBehavior.PetOriginOffset;
            float jitter=Mathf.Abs(Rules.Stat("accuracy")/100);
            float angle=Mathf.Atan2(aim.y,aim.x)+UnityEngine.Random.Range(-jitter,jitter);
            float spread=weapon.spread+Rules.Stat("projectiles")*.1f;
            for(int i=0;i<weapon.pellets+(int)Rules.Stat("projectiles");i++)
            {float a=angle+UnityEngine.Random.Range(-spread,spread);StarterProjectile(origin,new Vector2(Mathf.Cos(a),Mathf.Sin(a)),weapon,true,structure:p.spec.structure);}
            p.attackPose=.3f;OriginalSupportAudio.Play(sound,"shot:"+p.spec.kind,origin);
        }
        void TickCatGuns(Pet p,float dt)
        {
            Vector2 left=p.rig!=null?p.rig.GunOrigin(0):p.pos+Vector2.left*.2f;
            Vector2 right=p.rig!=null?p.rig.GunOrigin(1):p.pos+Vector2.right*.2f;
            var lt=CatGunTarget(p,left);var rt=CatGunTarget(p,right);
            float interval=SupportBehavior.CatInterval(Rules.PetCooldown(p.spec.stats.interval),p.boosted);
            if(p.clock<=0&&lt!=null)
            {FirePet(p,left,lt.pos+CombatGeometry.OriginOffset(lt.type.id),p.weapon);p.clock=interval;p.rightClock=interval/2;p.rig?.GunKick(0);}
            else if(p.rightClock<=0&&rt!=null)
            {FirePet(p,right,rt.pos+CombatGeometry.OriginOffset(rt.type.id),p.weapon);p.rightClock=interval;p.clock=interval/2;p.rig?.GunKick(1);}
            p.rig?.TickGunRecoil(dt,reducedMotion);
        }
        void PoseCatGuns(Pet p)
        {
            for(int i=0;i<2;i++)
            {Vector2 origin=p.rig.GunOrigin(i);var target=CatGunTarget(p,origin);if(target!=null)p.rig.AimGun(i,target.pos+CombatGeometry.OriginOffset(target.type.id)-p.pos-SupportBehavior.PetOriginOffset);}
        }
        Enemy CatGunTarget(Pet p,Vector2 muzzle)
        {
            float range=Rules.PetRange(p.weapon),bestDistance=range*range;Enemy best=null;
            foreach(var e in enemies)if(e.hp>0&&e.marker==null&&SupportBehavior.PetBodyZone(p.pos,4,e.type.id,e.pos))
            {
                float distance=(e.pos+CombatGeometry.OriginOffset(e.type.id)-muzzle).sqrMagnitude;
                if(distance<bestDistance){best=e;bestDistance=distance;}
            }
            return best;
        }
    }
}

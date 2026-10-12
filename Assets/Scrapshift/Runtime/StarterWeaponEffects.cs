using System.Collections.Generic;
using UnityEngine;

namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        sealed class StarterTurret { public GameObject obj; public Vector2 pos; public Equipment stats; public float clock; }
        readonly List<StarterTurret> starterTurrets=new List<StarterTurret>();
        readonly Dictionary<Equipment,int> ghostKills=new Dictionary<Equipment,int>();
        Enemy NearestStarterTarget(Vector2 point,HashSet<Enemy> excluded=null)
        {
            Enemy target=null;float best=float.PositiveInfinity;
            foreach(var e in enemies)
            {
                if(e.marker!=null||e.hp<=0||excluded!=null&&excluded.Contains(e))continue;
                float d=(e.pos-point).sqrMagnitude;if(d<best){best=d;target=e;}
            }
            return target;
        }
        void StarterProjectile(Vector2 position,Vector2 direction,Equipment stats,bool secondary,Enemy exclude=null)
        {
            if(shots.Count>=400)return;
            var obj=SpriteObject("Weapon projectile",catalog.muzzleFrames[2],position,13);
            obj.transform.localScale=Vector3.one*.35f;obj.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg);
            var shot=new Shot{obj=obj,pos=position,velocity=direction.normalized*stats.projectileSpeed,
                life=Mathf.Min(6,(stats.range+1)/Mathf.Max(1,stats.projectileSpeed)),damage=Rules.WeaponDamage(stats),
                crit=stats.crit,pierce=stats.piercing,bounces=stats.bounce,knockback=stats.knockback,weapon=stats,secondary=secondary};
            if(exclude!=null)shot.hit.Add(exclude);shots.Add(shot);shotsFired++;
        }
        void ApplyStarterHit(Enemy victim,Equipment weapon,bool critical,bool killed,bool secondary)
        {
            if(weapon==null||secondary||weapon.traits==null)return;
            foreach(var trait in weapon.traits)
            {
                switch(trait.key)
                {
                    case "effect_burning":
                        if(!killed&&UnityEngine.Random.value<trait.burnChance)
                        {victim.burnDamage=Mathf.Max(1,(trait.burnDamage+Rules.Stat("elemental_damage")*trait.burnScale)*Rules.DamageScale);victim.burnTicks=(int)trait.burnTicks;victim.burnClock=1;}
                        break;
                    case "effect_slow_in_zone":
                        if(!killed)victim.contactSlow+=2;break;
                    case "effect_projectiles_on_hit":case "effect_lightning_on_hit":
                        for(int i=0;i<(int)trait.value;i++)
                        {
                            var target=trait.key=="effect_lightning_on_hit"?NearestStarterTarget(victim.pos,new HashSet<Enemy>{victim}):null;
                            Vector2 direction=target!=null?(target.pos-victim.pos).normalized:UnityEngine.Random.insideUnitCircle.normalized;
                            StarterProjectile(victim.pos,direction,trait.projectile.AsEquipment(),true,victim);
                        }
                        break;
                    case "effect_explode_custom":
                        if(UnityEngine.Random.value<trait.chance)
                        {
                            float radius=trait.radius*(1+Rules.Stat("explosion_size")/100);
                            Burst(victim.pos,12,amber);
                            foreach(var e in enemies.ToArray())if(e!=victim&&e.marker==null&&Vector2.Distance(e.pos,victim.pos)<radius)
                                Damage(enemies.IndexOf(e),Rules.WeaponDamage(weapon),weapon.crit,weapon.knockback,e.pos-victim.pos,weapon,true);
                        }
                        break;
                    case "gold_on_crit_kill":
                        if(killed&&critical&&UnityEngine.Random.value<trait.value/100)Drop(victim.pos,1,false);break;
                    case "effect_gain_stat_every_killed_enemies":
                        if(killed)
                        {
                            ghostKills.TryGetValue(weapon,out int count);count++;
                            if(count>=trait.value){count=0;Rules.GainWeaponStat(trait.stat,trait.amount);FloatText(playerPos,"+1 Attack Speed",mint,false);}
                            ghostKills[weapon]=count;
                        }
                        break;
                }
            }
        }
        void ResetStructures()
        {
            foreach(var t in starterTurrets)Destroy(t.obj);starterTurrets.Clear();
            foreach(var w in Rules.Inventory)
            {
                var trait=w.Trait("turret");if(trait?.projectile==null)continue;
                var pos=new Vector2(UnityEngine.Random.Range(-7f,7f),UnityEngine.Random.Range(-5f,5f));
                var obj=SpriteObject("Deployable turret",w.image,pos,7);
                obj.transform.localScale=Vector3.one*ActorPresentation.SpriteLengthScale(w.image,.6f);
                AddGroundShadow(obj,w.image.bounds.size.x,-.2f);
                starterTurrets.Add(new StarterTurret{obj=obj,pos=pos,stats=trait.projectile.AsEquipment()});
            }
        }
        void UpdateStarterEffects(float dt)
        {
            foreach(var e in enemies.ToArray())if(e.burnTicks>0&&e.marker==null)
            {
                e.burnClock-=dt;if(e.burnClock>0)continue;
                e.burnClock+=1;e.burnTicks--;Burst(e.pos,2,amber);
                Damage(enemies.IndexOf(e),e.burnDamage,-100,0,Vector2.up,null,true,true);
            }
            foreach(var turret in starterTurrets)
            {
                turret.clock-=dt;if(turret.clock>0)continue;
                var target=NearestStarterTarget(turret.pos);
                if(target==null||Vector2.Distance(target.pos,turret.pos)>turret.stats.range)continue;
                StarterProjectile(turret.pos,target.pos-turret.pos,turret.stats,true);
                turret.clock=turret.stats.interval+turret.stats.recoilHalf*2;
                sound.Play(SoundCue.Fire,turret.pos);
            }
        }
        void ClearStarterEffects()
        {
            foreach(var t in starterTurrets)Destroy(t.obj);starterTurrets.Clear();
        }
    }
}

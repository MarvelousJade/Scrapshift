using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        float alienClock;
        readonly Queue<(Vector2 position,ConditionalEffect effect,float damage)> implantExplosions=new Queue<(Vector2,ConditionalEffect,float)>();
        readonly Queue<(Enemy target,float damage)> implantRetaliations=new Queue<(Enemy,float)>();
        void TickImplantAttacks(float dt)
        {
            var eyes=Rules.Effects("alien_eyes").ToArray();
            if(eyes.Length==0){alienClock=0;return;}
            alienClock+=dt;float interval=eyes[0].interval;if(interval<=0)return;
            while(alienClock>=interval)
            {
                alienClock-=interval;int count=(int)eyes.Sum(e=>e.value);
                for(int i=0;i<count;i++){float angle=Mathf.PI*2*i/count;StarterProjectile(playerPos,new Vector2(Mathf.Cos(angle),Mathf.Sin(angle)),eyes[0].projectile.AsEquipment(),true);}
                sound.Play(SoundCue.Pulse,playerPos);
            }
        }
        void EnemyImplantDeath(Enemy victim)
        {
            foreach(var effect in Rules.Effects("projectiles_on_death"))for(int i=0;i<(int)effect.value;i++)
                StarterProjectile(victim.pos,UnityEngine.Random.insideUnitCircle.normalized,effect.projectile.AsEquipment(),true,victim);
            QueueImplantExplosions("explode_on_death",victim.pos);
        }
        void QueueImplantExplosions(string trigger,Vector2 position)
        {
            if(Rules.TryItemExplosion(trigger,UnityEngine.Random.value,out var effect,out float damage))
                implantExplosions.Enqueue((position,effect,damage));
        }
        void ResolveImplantAttacks()
        {
            while(implantRetaliations.Count>0)
            {var attack=implantRetaliations.Dequeue();if(enemies.Contains(attack.target))Damage(enemies.IndexOf(attack.target),attack.damage,-100,0,Vector2.zero,null,true);}
            // New deaths may enqueue further explosions; never recurse through Damage.
            while(implantExplosions.Count>0)
            {
                var explosion=implantExplosions.Dequeue();var effect=explosion.effect;var stats=effect.projectile.AsEquipment();
                float damage=explosion.damage,radius=effect.radius*Mathf.Max(0,1+Rules.Stat("explosion_size")/100);
                int kills=0;
                // Imported Rip & Tear / Spicy Sauce use plain ItemExplodingEffect:
                // handle_explode_effect forwards their empty burning data, and
                // PlayerExplosion has no weapon/projectile life-steal callback.
                var burnPayload=new BurnPayload();
                bool previous=resolvingWeaponExplosion;resolvingWeaponExplosion=true;
                try
                {
                    foreach(var target in enemies.ToArray())if(target.marker==null&&Vector2.Distance(target.pos,explosion.position)<=radius)
                    {
                        bool alive=target.hp>0;
                        DamageWithWeaponIndex(enemies.IndexOf(target),damage,stats.crit,stats.knockback,target.pos-explosion.position,stats,true,false,false,-1,burnPayload);
                        // Hitbox.killed_something is immediate, while the global
                        // enemy die callback and Rules.Kills are deferred.
                        if(alive&&target.hp<=0)kills++;
                    }
                }
                finally{resolvingWeaponExplosion=previous;}
                Rules.RecordExplosionKills(kills);Burst(explosion.position,18,amber);sound.Play(SoundCue.Impact,explosion.position,contextProfile:ContextSoundProfile.Explosion);
            }
        }
        void TryVulnerability(Enemy target,string scalingStat)
        {
            if(target.iceApplied)return;
            var effect=Rules.Effects("enemy_percent_damage_taken").FirstOrDefault(e=>e.key.Replace("stat_","")==scalingStat);
            if(effect==null)return;target.iceApplied=true;target.iceClock=effect.interval;target.icePercent=effect.value;Burst(target.pos,4,mint);
        }
    }
    public sealed partial class RunRules
    {
        public bool TryItemExplosion(string trigger,double roll,out ConditionalEffect template,out float damage)
        {
            var effects=Effects(trigger).ToArray();template=effects.FirstOrDefault();damage=0;
            // run_data.gd:handle_explode_effect rolls the summed chance once, then
            // sums every copy's independently calculated damage into one explosion.
            float chance=effects.Sum(e=>e.chance);
            if(template==null||chance<=0||roll>chance)return false;
            damage=effects.Sum(ItemExplosionDamage);
            return true;
        }
        float ItemExplosionDamage(ConditionalEffect effect)
        {
            // Unlike init_ranged_stats(..., true), get_explosion_damage explicitly
            // applies Nail/Frozen Heart scaling, even to an item-created explosion.
            float scaled=effect.projectile.damage+AddedWeaponScaling;
            foreach(var stat in effect.projectile.scaling??Array.Empty<StatEffect>())
                scaled+=(stat.key=="levels"?Level:Stat(stat.key))*stat.value;
            int basis=Mathf.Max(1,(int)scaled);
            return Mathf.Max(1,Mathf.Floor(basis*(1+(Stat("percent_damage")+Stat("explosion_damage"))/100)+.5f));
        }
        public float DodgeRetaliation {get;private set;}
        public bool LastHitDodged {get;private set;}
        void RetaliationTriggers()
        {foreach(var e in Effects("dmg_on_dodge"))if(random.NextDouble()<e.chance)DodgeRetaliation+=Mathf.Max(1,Mathf.Floor((int)Mathf.Max(1,Stat(e.key.Replace("stat_",""))*e.value/100)*DamageScale+.5f));}
        public bool WeaponScalesWith(Equipment weapon,string stat)=>weapon?.scaling!=null&&weapon.scaling.Any(s=>s.key==stat)||weapon!=null&&!weapon.specialSpawn&&Effects("weapon_scaling_stats").Any(e=>e.key=="stat_"+stat);
    }
}

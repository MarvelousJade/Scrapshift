using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        float alienClock;
        readonly Queue<(Vector2 position,ConditionalEffect effect)> implantExplosions=new Queue<(Vector2,ConditionalEffect)>();
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
        {foreach(var effect in Rules.Effects(trigger))if(UnityEngine.Random.value<effect.chance)implantExplosions.Enqueue((position,effect));}
        void ResolveImplantAttacks()
        {
            while(implantRetaliations.Count>0)
            {var attack=implantRetaliations.Dequeue();if(enemies.Contains(attack.target))Damage(enemies.IndexOf(attack.target),attack.damage,-100,0,Vector2.zero,null,true);}
            // New deaths may enqueue further explosions; never recurse through Damage.
            while(implantExplosions.Count>0)
            {
                var explosion=implantExplosions.Dequeue();var effect=explosion.effect;var stats=effect.projectile.AsEquipment();
                float damage=Rules.WeaponDamage(stats,Rules.Stat("explosion_damage")),radius=effect.radius*Mathf.Max(0,1+Rules.Stat("explosion_size")/100);
                int before=Rules.Kills;
                foreach(var target in enemies.ToArray())if(target.marker==null&&Vector2.Distance(target.pos,explosion.position)<=radius)
                    Damage(enemies.IndexOf(target),damage,stats.crit,stats.knockback,target.pos-explosion.position,stats,true);
                Rules.RecordExplosionKills(Rules.Kills-before);Burst(explosion.position,18,amber);sound.Play(SoundCue.Impact,explosion.position,contextProfile:ContextSoundProfile.Explosion);
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
        public float DodgeRetaliation {get;private set;}
        public bool LastHitDodged {get;private set;}
        void RetaliationTriggers()
        {foreach(var e in Effects("dmg_on_dodge"))if(random.NextDouble()<e.chance)DodgeRetaliation+=(int)(Mathf.Max(1,Stat(e.key.Replace("stat_",""))*e.value/100)*DamageScale);}
        public bool WeaponScalesWith(Equipment weapon,string stat)=>weapon?.scaling!=null&&weapon.scaling.Any(s=>s.key==stat)||weapon!=null&&!weapon.specialSpawn&&Effects("weapon_scaling_stats").Any(e=>e.key=="stat_"+stat);
    }
}

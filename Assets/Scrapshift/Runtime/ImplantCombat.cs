using System;
using System.Collections.Generic;
using UnityEngine;
namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        readonly Queue<(Enemy target,float damage,float vulnerability)> implantDamage=new Queue<(Enemy,float,float)>();
        Action<float> CaptureImplantDamageTarget()
        {
            // Unit.take_damage leaves its zero-HP victim in the source spawner
            // until deferred die. Selecting that victim aborts the proc; filtering
            // it out would incorrectly guarantee a different live enemy.
            var candidates=enemies.FindAll(e=>e.marker==null&&e.type.role!="Tree");
            if(candidates.Count==0)return null;
            var target=candidates[UnityEngine.Random.Range(0,candidates.Count)];
            if(target.hp<=0)return null;
            // Main.handle_stat_damages applies HP immediately. Defer only die's
            // list mutation, so later current-HP effects and hits see this hit.
            return damage=>
            {
                if(Rules.Phase!=RunPhase.Playing||target.hp<=0||!enemies.Contains(target))return;
                DamageWithCapturedVulnerability(enemies.IndexOf(target),damage,-100,0,Vector2.zero,null,true,false,false,-1,null,null);
            };
        }
        void ResolveImplantDamage()
        {
            // Retain the historical queue/wrapper for explicit staged requests;
            // production stat procs now apply their HP synchronously above.
            while(implantDamage.Count>0)
            {
                var attack=implantDamage.Dequeue();
                if(Rules.Phase!=RunPhase.Playing||attack.target.hp<=0||!enemies.Contains(attack.target))continue;
                DamageWithCapturedVulnerability(enemies.IndexOf(attack.target),attack.damage,-100,0,Vector2.zero,null,true,false,false,-1,null,attack.vulnerability);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        readonly Queue<(Enemy target,float damage)> implantDamage=new Queue<(Enemy,float)>();
        Action<float> CaptureImplantDamageTarget()
        {
            // Unit.take_damage leaves its zero-HP victim in the source spawner
            // until deferred die. Selecting that victim aborts the proc; filtering
            // it out would incorrectly guarantee a different live enemy.
            var candidates=enemies.FindAll(e=>e.marker==null&&e.type.role!="Tree");
            if(candidates.Count==0)return null;
            var target=candidates[UnityEngine.Random.Range(0,candidates.Count)];
            if(target.hp<=0)return null;
            return damage=>implantDamage.Enqueue((target,damage));
        }
        void ResolveImplantDamage()
        {
            // Resolve chained death effects iteratively, outside enemy/projectile list traversal.
            while(implantDamage.Count>0)
            {
                var attack=implantDamage.Dequeue();
                if(Rules.Phase!=RunPhase.Playing||attack.target.hp<=0||!enemies.Contains(attack.target))continue;
                Damage(enemies.IndexOf(attack.target),attack.damage,-100,0,Vector2.zero,null,true);
            }
        }
    }
}

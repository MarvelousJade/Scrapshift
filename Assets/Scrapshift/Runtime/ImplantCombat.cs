using System.Collections.Generic;
using UnityEngine;
namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        readonly Queue<float> implantDamage=new Queue<float>();
        void QueueImplantDamage(float damage)=>implantDamage.Enqueue(damage);
        void ResolveImplantDamage()
        {
            // Resolve chained death effects iteratively, outside enemy/projectile list traversal.
            while(implantDamage.Count>0)
            {
                float damage=implantDamage.Dequeue();
                var targets=enemies.FindAll(e=>e.marker==null&&e.hp>0&&e.type.role!="Tree");
                if(targets.Count==0||Rules.Phase!=RunPhase.Playing)continue;
                var target=targets[UnityEngine.Random.Range(0,targets.Count)];
                Damage(enemies.IndexOf(target),damage,-100,0,Vector2.zero,null,true);
            }
        }
    }
}

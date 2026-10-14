using System;
using System.Collections.Generic;
using UnityEngine;
namespace Scrapshift
{
    public sealed partial class RunRules
    {
        public void EnemyBoost(float hp,float speed)
        {AddTemporary("max_hp",hp);AddTemporary("speed",speed);AddTemporary("attack_speed",speed);Health=Mathf.Min(MaxHealth,Health+Mathf.Max(0,hp));}
    }
    public sealed partial class ArenaGame
    {
        float friendlyBoostClock,friendlyBoostHP;
        void TickFriendlyBoost(float dt)
        {
            if(friendlyBoostClock<=0)return;friendlyBoostClock-=dt;
            if(friendlyBoostClock<=0){Rules.EnemyBoost(-friendlyBoostHP,-20);friendlyBoostHP=0;}
        }
        void BoostNearbyEntity(Enemy buffer,float damage)
        {
            var candidates=new List<Action>();
            foreach(var ally in enemies)if(ally!=buffer&&ally.marker==null&&!ally.boosted&&ally.behavior.canBoost&&ally.type.role!="Tree"&&Vector2.Distance(ally.pos,buffer.pos)<3.0904f)
                candidates.Add(()=>{ally.boosted=true;ally.boostDamage=damage;ally.maxHp*=2.5f;ally.hp=ally.maxHp;ally.speed*=1.5f;ally.supportFlash=.45f;});
            if(friendlyBoostClock<=0&&Vector2.Distance(playerPos,buffer.pos)<3.0904f)
                candidates.Add(()=>{friendlyBoostHP=(int)(Rules.MaxHealth*.2f);Rules.EnemyBoost(friendlyBoostHP,20);friendlyBoostClock=5;FloatText(playerPos,"BOOST",mint,false);});
            foreach(var turret in starterTurrets)if(turret.spec.kind!="garden"&&turret.spec.kind!="wandering_bot"&&turret.boostClock<=0&&Vector2.Distance(turret.pos,buffer.pos)<3.0904f)
                candidates.Add(()=>{turret.boostClock=5;turret.clock=Mathf.Max(0,turret.clock-Rules.StructureCooldown(turret.stats.interval)*.2f);FloatText(turret.pos,"BOOST",mint,false);});
            if(candidates.Count==0)return;candidates[placementRandom.Next(candidates.Count)]();buffer.supportFlash=.45f;
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
namespace Scrapshift
{
    public sealed partial class RunRules
    {
        public void EnemyBoost(float hp,float speed)
        {
            AddTemporary("max_hp",hp);AddTemporary("speed",speed);AddTemporary("attack_speed",speed);
            // player.boost adds HP explicitly, in addition to the subsequent
            // dirty-Max-HP refresh. Neither positive contribution is a Heal.
            if(hp>=0)Health+=hp;
            // boost_ended alone explicitly clamps, against uncapped Utils stat.
            else Health=Mathf.Min(Health,(int)Stat("max_hp"));
        }
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
            foreach(var ally in enemies)if(ally!=buffer&&ally.marker==null&&ally.hp>0&&!ally.boosted&&ally.behavior.canBoost&&CombatGeometry.BufferZoneOverlaps(buffer.type.id,buffer.pos,ally.type.id,ally.pos))
                candidates.Add(()=>{ally.boosted=true;ally.boostDamage=damage;ally.maxHp=EnemyReferenceRules.ScaledHealth(ally.type.health+ally.type.growth*(Rules.Wave-1),Rules.EnemyHealthFactor*Rules.DangerMultiplier*GameSettings.Value("enemyHealth")*2.5f);ally.hp=ally.maxHp;ally.speed*=1.5f;ally.supportFlash=.45f;});
            // Both supplied buffer scenes mask enemy body layer 4 only. Player
            // and structure branches in their scripts cannot receive a body entry.
            if(candidates.Count==0)return;candidates[placementRandom.Next(candidates.Count)]();buffer.supportFlash=.45f;
        }
    }
}

using System;
using UnityEngine;
namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        void CheckImplantAttacks()
        {
            foreach(string id in new[]{"alien_eyes","baby_with_a_beard","rip_and_tear","spicy_sauce","ice_cube","riposte"})
            {
                Begin();ClearCombat();Rules.Inventory.Clear();Rules.Items.Clear();Rules.Items.Add(Array.Find(catalog.data.items,i=>i.id==id));
                var target=FeelEnemy(Vector2.right);bool passed=false;
                if(id=="alien_eyes"){TickImplantAttacks(2.99f);bool waited=shots.Count==0;TickImplantAttacks(.02f);passed=waited&&shots.Count==6;}
                else if(id=="baby_with_a_beard"){EnemyImplantDeath(target);passed=shots.Count==1&&shots[0].hit.Contains(target);}
                else if(id=="rip_and_tear"||id=="spicy_sauce")
                {
                    // Repeated independent rolls avoid turning a probability check into a flaky fixture.
                    for(int i=0;i<100;i++)QueueImplantExplosions(id=="rip_and_tear"?"explode_on_death":"explode_on_consumable",target.pos);
                    ResolveImplantAttacks();passed=target.hp<1000;
                }
                else if(id=="ice_cube")
                {
                    TryVulnerability(target,"ranged_damage");bool ignored=!target.iceApplied;
                    Damage(enemies.IndexOf(target),10,-100,0,Vector2.zero,catalog.Weapon("wand",1),true);
                    bool initial=Mathf.Abs(target.hp-989)<.01f;target.iceClock=0;TryVulnerability(target,"elemental_damage");
                    passed=ignored&&initial&&target.iceApplied&&target.iceClock==0;
                }
                else
                {
                    Rules.GainWeaponStat("dodge",100);Rules.GainWeaponStat("dodge_cap",100);Rules.GainWeaponStat("melee_damage",10);
                    HurtPlayer(1,target.type,target);ResolveImplantAttacks();passed=Rules.LastHitDodged&&Mathf.Abs(target.hp-970)<.01f;
                }
                if(passed)starterReport.attackImplantsPassed.Add(id);else Debug.LogError("Attack implant fixture failed: "+id);
            }
        }
    }
}

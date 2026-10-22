using UnityEngine;

namespace Scrapshift
{
    public enum ConsumableKind { None, Fruit, Crate, LegendaryCrate }

    // Base-difficulty single-player profiles from the supplied unit *_stats.tres files.
    public static class ReferenceLoot
    {
        public static bool CanDropConsumables(string enemy)
        {
            switch(enemy)
            {
                case "junkie":case "dire_junkie":case "evil_mob":case "slasher_egg":case "infected_slasher_egg":case "corrupted_tree":return false;
                default:return true;
            }
        }
        static bool BossClass(string enemy,string role)
        {
            if(role=="Boss"||role=="Elite")return true;
            switch(enemy)
            {
                case "butcher":case "colossus":case "croc":case "gargoyle":case "mantis":case "mom":case "monk":case "rhino":case "invoker":case "predator":return true;
                default:return false;
            }
        }
        // World units; source stats store pixels. The source uses the unrounded
        // material amount before spawn_gold chooses its integer object count.
        public static float BaseGoldSpread(string enemy)=>enemy=="tree"||enemy=="looter"?.75f:enemy=="evil_mob"?1.25f:0;
        public static float MaterialSpread(string enemy,float unroundedAmount)=>Mathf.Clamp((unroundedAmount-1)*.25f,BaseGoldSpread(enemy),2);
        public static float MaterialBatchChance(string enemy,int wave,bool horde,string role=null)
        {
            // Neutrals bypass the enemy roll. Always-drop consumable stats also
            // force gold despite can_drop_consumables=false on Evil Mob.
            if(enemy=="tree"||enemy=="evil_mob"||enemy=="looter"||BossClass(enemy,role))return 1;
            float chance=wave<5?1:Mathf.Max(.5f,1-wave*.015f);
            return chance*(horde?.65f:1);
        }
        // Utils.get_chance_success explicitly rejects zero, then uses <=.
        static bool Succeeds(double roll,float chance)=>chance>0&&roll<=chance;
        public static ConsumableKind Roll(string enemy, float luck, int spawnedCrates, double dropRoll, double crateRoll, float crateChancePercent=0, string role=null, int wave=1)
        {
            if(!CanDropConsumables(enemy))return ConsumableKind.None;
            bool boss=BossClass(enemy,role),always=enemy=="looter"||boss;
            float drop=.01f, crate=.01f;
            switch(enemy)
            {
                case "tree": drop=1;crate=.2f;break;
                case "chaser": case "fin_alien": drop=.02f;crate=.03f;break;
                case "bruiser": case "horned_bruiser": case "pursuer": case "slasher":case "healer":case "mad_slasher": drop=crate=.03f;break;
                case "spitter":case "horned_spitter": drop=.03f;crate=.1f;break;
                case "tentacle":drop=crate=.02f;break;
            }
            if(always)drop=crate=1;
            float factor=Mathf.Max(0,1+luck/100);
            if(!always&&!Succeeds(dropRoll,Mathf.Clamp01(drop*factor)))return ConsumableKind.None;
            // main.gd applies the crate-chance effect after luck and the number of
            // crates already spawned in this wave.
            float itemChance=crate*factor/(1+Mathf.Max(0,spawnedCrates));
            itemChance*=1+crateChancePercent/100;
            if(always&&crate>=1&&wave<=20)itemChance=1;
            return Succeeds(crateRoll,Mathf.Clamp01(itemChance))?boss&&wave<=20?ConsumableKind.LegendaryCrate:ConsumableKind.Crate:ConsumableKind.Fruit;
        }
    }
}

using UnityEngine;

namespace Scrapshift
{
    public enum ConsumableKind { None, Fruit, Crate, LegendaryCrate }

    // Base-difficulty single-player profiles from the supplied unit *_stats.tres files.
    public static class ReferenceLoot
    {
        public static ConsumableKind Roll(string enemy, float luck, int spawnedCrates, double dropRoll, double crateRoll)
        {
            if(enemy=="junkie" || enemy=="evil_mob") return ConsumableKind.None;
            if(enemy=="looter") return ConsumableKind.Crate;
            if(enemy=="predator") return ConsumableKind.LegendaryCrate;
            float drop=.01f, crate=.01f;
            switch(enemy)
            {
                case "tree": drop=1;crate=.2f;break;
                case "chaser": case "fin_alien": drop=.02f;crate=.03f;break;
                case "bruiser": case "horned_bruiser": case "pursuer": case "slasher": drop=crate=.03f;break;
                case "spitter": drop=.03f;crate=.1f;break;
            }
            float factor=Mathf.Max(0,1+luck/100);
            if(dropRoll>=Mathf.Clamp01(drop*factor))return ConsumableKind.None;
            return crateRoll<Mathf.Clamp01(crate*factor/(1+Mathf.Max(0,spawnedCrates)))?ConsumableKind.Crate:ConsumableKind.Fruit;
        }
    }
}

using UnityEngine;
namespace Scrapshift
{
    // item_service.gd reset_tiers_data/get_tier_from_wave. Public tiers are 1..4.
    public static class ReferenceRarity
    {
        public static int Roll(int waveOrLevel, float luck, double roll)
        {
            float factor = luck >= 0 ? 1 + luck / 100 : 1 / (1 + Mathf.Abs(luck) / 100);
            if (roll <= Mathf.Min(.08f, Mathf.Max(0, waveOrLevel - 7) * .0023f * factor)) return 4;
            if (roll <= Mathf.Min(.25f, Mathf.Max(0, waveOrLevel - 3) * .02f * factor)) return 3;
            if (roll <= Mathf.Min(.6f, Mathf.Max(0, waveOrLevel - 1) * .06f * factor)) return 2;
            return 1;
        }
        public static int Upgrade(int level, float luck, double roll) => level == 5 ? 2 :
            level == 10 || level == 15 || level == 20 ? 3 : level > 0 && level % 5 == 0 ? 4 : Roll(level, luck, roll);
    }
}

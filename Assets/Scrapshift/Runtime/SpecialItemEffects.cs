using System.Collections.Generic;
namespace Scrapshift
{
    public static class SpecialItemEffects
    {
        static readonly HashSet<string> percent=new HashSet<string>{"pickup_range","xp_gain","items_price","harvesting_growth","recycling_gains","enemy_health","enemy_speed","explosion_size","explosion_damage","piercing_damage"};
        public static bool IsSecondary(string key)=>percent.Contains(key)||key=="knockback"||key=="consumable_heal"||key=="item_box_gold"||key=="bounce"||key=="piercing"||key=="lose_hp_per_second"||key=="burning_spread";
        public static bool IsPenalty(string key,float value)=>(key=="items_price"||key=="enemy_health"||key=="enemy_speed"||key=="lose_hp_per_second")?value>0:value<0;
        public static string Description(string key,float value)
        {
            string amount=value.ToString("+0.##;-0.##;0",System.Globalization.CultureInfo.InvariantCulture);
            if(key=="consumable_heal")return amount+" HP recovered from consumables";
            if(key=="item_box_gold")return amount+" materials when collecting a crate";
            if(key=="lose_hp_per_second")return $"Lose {value:0.##} HP every second";
            if(key=="burning_spread")return $"Burning spreads to {value:0.##} additional enemies";
            return amount+(percent.Contains(key)?"%":"")+" "+ItemEntry.StatName(key);
        }
    }
}

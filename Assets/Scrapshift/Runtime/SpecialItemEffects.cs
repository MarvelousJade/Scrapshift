using System.Collections.Generic;
namespace Scrapshift
{
    public static class SpecialItemEffects
    {
        static readonly Dictionary<string,string> lifecycle=new Dictionary<string,string>
        {
            {"instant_gold_attracting","{0}% chance to attract a material as soon as it drops"},
            {"heal_when_pickup_gold","{0}% chance to heal 1 HP when picking up a material"},
            {"chance_double_gold","{0}% chance to double the value of picked-up materials"},
            {"gain_pct_gold_start_wave","Gain {0}% of your materials when a wave starts"},
            {"hp_start_wave","{0}% starting HP every wave"},
            {"hp_start_next_wave","{0}% starting HP next wave only"},
            {"torture","Restore {0} HP per second. Other healing is disabled"},
            {"free_rerolls","{0} free shop rerolls each wave"},
            {"hit_protection","Nullify {0} hits each wave"},
            {"damage_against_bosses","{0}% damage against bosses and elites"},
            {"heal_on_crit_kill","{0}% chance to heal 1 HP on a critical kill"},
            {"gold_drops","{0}% materials dropped"},
            {"dodge_cap","Dodge is capped at {0}%"},
            {"enemy_fruit_drops","{0}% extra chance for enemies to drop fruit"},
            {"one_shot_trees","Trees are destroyed in one hit"},
            {"burning_cooldown_reduction","Burning activates {0}% faster"},
            {"number_of_enemies","{0}% enemies"},
            {"trees","{0} trees in each tree spawn group"}
            ,{"structure_attack_speed","{0}% structure attack speed"}
            ,{"structures_can_crit","Structures can critically hit"}
        };
        static readonly HashSet<string> percent=new HashSet<string>{"pickup_range","xp_gain","items_price","harvesting_growth","recycling_gains","enemy_health","enemy_speed","explosion_size","explosion_damage","piercing_damage"};
        public static bool IsSecondary(string key)=>lifecycle.ContainsKey(key)||percent.Contains(key)||key=="knockback"||key=="consumable_heal"||key=="item_box_gold"||key=="bounce"||key=="piercing"||key=="lose_hp_per_second"||key=="burning_spread";
        public static bool IsPenalty(string key,float value)=>(key=="items_price"||key=="enemy_health"||key=="enemy_speed"||key=="lose_hp_per_second")?value>0:value<0;
        public static string Description(string key,float value)
        {
            string amount=value.ToString("+0.##;-0.##;0",System.Globalization.CultureInfo.InvariantCulture);
            if(lifecycle.TryGetValue(key,out var template))return string.Format(System.Globalization.CultureInfo.InvariantCulture,template,value.ToString("0.##",System.Globalization.CultureInfo.InvariantCulture));
            if(key=="consumable_heal")return amount+" HP recovered from consumables";
            if(key=="item_box_gold")return amount+" materials when collecting a crate";
            if(key=="lose_hp_per_second")return $"Lose {value:0.##} HP every second";
            if(key=="burning_spread")return $"Burning spreads to {value:0.##} additional enemies";
            return amount+(percent.Contains(key)?"%":"")+" "+ItemEntry.StatName(key);
        }
    }
}

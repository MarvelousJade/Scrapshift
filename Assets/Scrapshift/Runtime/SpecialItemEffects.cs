using System.Collections.Generic;
namespace Scrapshift
{
    public static class SpecialItemEffects
    {
        static readonly Dictionary<string,string> lifecycle=new Dictionary<string,string>
        {
            {"","Spent implant"},
            {"fog_visibility","{0}% visibility during fog waves"},
            {"item_hourglass","Replay the current wave, then replace this implant with a spent organ and start at 1 HP"},
            {"extra_elite_next_wave_chance","{0}% chance for an additional elite next wave"},
            {"jellyshield_count","{0} orbital companions block enemy projectiles"},
            {"weapon_slot","{0} weapon slots"},
            {"weapon_slot_upgrades","Unlock 1 weapon slot per level, up to {0}"},
            {"min_weapon_tier","Weapons start at tier II"},
            {"max_weapon_tier","Weapons cannot exceed tier II"},
            {"no_duplicate_weapons","Cannot equip duplicate weapons"},
            {"can_attack_while_moving","Cannot attack while moving"},
            {"die_in_one_hit","Any unblocked hit kills you"},
            {"map_size","{0}% map size"},
            {"pacifist","Gain materials and XP equal to {0}% of surviving enemies at wave end"},
            {"cryptid","Gain {0} materials and XP per living tree at wave end"},
            {"group_structures","Structures spawn close together"},
            {"trees_start_wave","Start each wave with an additional tree spawn"},
            {"max_melee_weapons","Can equip at most {0} melee weapons"},
            {"max_ranged_weapons","Can equip at most {0} ranged weapons"},
            {"minimum_weapons_in_shop","At least {0} weapon in each shop roll"},
            {"destroy_weapons","All weapons are destroyed when entering a shop"},
            {"no_heal","Cannot heal"},
            {"hp_shop","Shop purchases cost Max HP"},
            {"all_weapons_count_for_sets","Every weapon counts toward every weapon set"},
            {"beast_master_effect","Companions scale with Melee, Ranged, Elemental Damage and Engineering"},
            {"boosted_wanted_item_tag","10% chance to favor companion-tagged shop items"},
            {"upgraded_baits","Owned beacons add lamprey groups each wave"},
            {"extra_loot_aliens_next_wave","{0} additional loot aliens appear next wave"},
            {"tree_turrets","Spawn {0} turret near each destroyed tree"},
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
        static readonly HashSet<string> percent=new HashSet<string>{"pickup_range","xp_gain","items_price","harvesting_growth","recycling_gains","enemy_health","enemy_damage","enemy_speed","weapons_price","next_level_xp_needed","explosion_size","explosion_damage","piercing_damage"};
        public static bool IsSecondary(string key)=>lifecycle.ContainsKey(key)||percent.Contains(key)||key=="knockback"||key=="consumable_heal"||key=="item_box_gold"||key=="bounce"||key=="piercing"||key=="lose_hp_per_second"||key=="burning_spread";
        public static bool IsPenalty(string key,float value)=>(key=="items_price"||key=="enemy_health"||key=="enemy_speed"||key=="lose_hp_per_second")?value>0:value<0;
        public static string Description(string key,float value)
        {
            string amount=value.ToString("+0.##;-0.##;0",System.Globalization.CultureInfo.InvariantCulture);
            if(key=="hp_start_next_wave"&&value<=-100)return "Start the next wave with 1 HP";
            if(lifecycle.TryGetValue(key,out var template))return string.Format(System.Globalization.CultureInfo.InvariantCulture,template,value.ToString("0.##",System.Globalization.CultureInfo.InvariantCulture));
            if(key=="consumable_heal")return amount+" HP recovered from consumables";
            if(key=="item_box_gold")return amount+" materials when collecting a crate";
            if(key=="lose_hp_per_second")return $"Lose {value:0.##} HP every second";
            if(key=="burning_spread")return $"Burning spreads to {value:0.##} additional enemies";
            return amount+(percent.Contains(key)?"%":"")+" "+ItemEntry.StatName(key);
        }
    }
}

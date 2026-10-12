using System.Collections.Generic;
namespace Scrapshift
{
    // Display names only: reference identities, effects, prices and shop filters remain intact.
    public static class CyberneticTheme
    {
        static readonly Dictionary<string,string> names=new Dictionary<string,string>
        {
            {"alien_magic","Xeno Gland"},
            {"alloy","Alloy Tendons"},
            {"bat","Hemovore Jaw"},
            {"beanie","Cortical Insulator"},
            {"blindfold","Blind Optics"},
            {"blood_leech","Leech Vessel"},
            {"boiling_water","Thermal Circulator"},
            {"book","Memory Bank"},
            {"bowler_hat","Cranial Governor"},
            {"broken_mouth","Fractured Mandible"},
            {"butterfly","Reflex Filament"},
            {"cake","Nutrient Bladder"},
            {"cape","Phase Membrane"},
            {"charcoal","Carbon Node"},
            {"claw_tree","Grip Tendons"},
            {"clover","Probability Chip"},
            {"coffee","Synapse Accelerator"},
            {"compass","Vestibular Compass"},
            {"cyclops_worm","Cyclops Cortex"},
            {"defective_steroids","Muscle Stimulator"},
            {"duct_tape","Dermal Seal"},
            {"energy_bracelet","Energy Shunt"},
            {"exoskeleton","Exoskeletal Spine"},
            {"fertilizer","Nutrient Recycler"},
            {"fin","Balance Fin"},
            {"fresh_meat","Cultured Tissue"},
            {"fuel_tank","Auxiliary Fuel Sac"},
            {"gambling_token","Risk Processor"},
            {"glass_cannon","Brittle Power Core"},
            {"glasses","Optical Lenses"},
            {"goat_skull","Cranial Spurs"},
            {"gummy_berserker","Berserker Gland"},
            {"head_injury","Damaged Cortex"},
            {"heavy_bullets","Ballistic Amplifier"},
            {"hedgehog","Needle Dermis"},
            {"helmet","Cranial Plating"},
            {"injection","Adrenal Injector"},
            {"insanity","Overclocked Cortex"},
            {"jetpack","Dorsal Thrusters"},
            {"leather_vest","Dermal Weave"},
            {"lens","Focal Implant"},
            {"little_muscley_dude","Myofiber Bundle"},
            {"lost_duck","Errant Signal Chip"},
            {"lucky_charm","Fortune Coprocessor"},
            {"mastery","Motor Memory"},
            {"medal","Veteran Neural Cache"},
            {"metal_plate","Subdermal Plate"},
            {"missile","Missile Guidance Node"},
            {"mushroom","Fungal Symbiote"},
            {"night_goggles","Low-Light Optics"},
            {"octopus","Octal Nerve Cluster"},
            {"panda","Metabolic Buffer"},
            {"peaceful_bee","Peacekeeper Chip"},
            {"pencil","Precision Stylus"},
            {"plant","Photosynthetic Gland"},
            {"poisonous_tonic","Toxic Endocrine Pump"},
            {"potato","Total-Body Augment"},
            {"propeller_hat","Gyroscopic Crown"},
            {"reinforced_steel","Reinforced Tendons"},
            {"ritual","Ritual Firmware"},
            {"scope","Telescopic Optics"},
            {"shady_potion","Unstable Serum Pump"},
            {"shmoop","Soft-Tissue Graft"},
            {"small_magazine","Feed Accelerator"},
            {"sunglasses","Polarized Optics"},
            {"terrified_onion","Panic Gland"},
            {"toolbox","Maintenance Cortex"},
            {"toxic_sludge","Toxic Reservoir"},
            {"tractor","Industrial Digestor"},
            {"warrior_helmet","Combat Cranial Shell"},
            {"wheat","Nutrient Synthesizer"},
            {"wheelbarrow","Storage Organ"},
            {"wings","Glide Membrane"},
            {"wolf_helmet","Predator Cortex"},
        };
        public static string ItemName(string id,string fallback)=>names.TryGetValue(id,out var name)?name:fallback;
        public static string UpgradeName(string stat)
        {
            switch(stat)
            {
                case "max_hp":return "Synthetic Heart";
                case "hp_regeneration":return "Repair Gland";
                case "percent_damage":return "Combat Cortex";
                case "attack_speed":return "Neural Relay";
                case "crit_chance":return "Targeting Optics";
                case "armor":return "Dermal Plating";
                case "harvesting":return "Salvage Implant";
                case "speed":return "Motor Nerves";
                default:return ItemEntry.StatName(stat);
            }
        }
    }
}

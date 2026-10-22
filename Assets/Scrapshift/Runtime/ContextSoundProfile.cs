namespace Scrapshift
{
    // Shared by baking and playback: source identity alone cannot identify a weapon.
    public static class ContextSoundProfile
    {
        public const int Laser=100,Rail=101,Rocket=102,Electric=103,Flame=104,
            Blade=105,Heavy=106,Ballistic=107,Rapid=108,Bow=109,Organic=110,
            Ethereal=111,Motor=112,Heal=113,Plasma=114,Ice=115,Explosion=116,
            Voice=117,Shield=118,Status=119,Burn=120,HealLoop=121;
        public static int Weapon(string id,bool melee)
        {
            switch(id)
            {
                case "laser_gun":case "gatling_laser":return Laser;
                case "rail_gun":case "sniper_gun":return Rail;
                case "rocket_launcher":case "nuclear_launcher":return Rocket;
                case "lightning_shiv":case "thunder_sword":case "taser":return Electric;
                case "torch":case "flaming_knuckles":case "flamethrower":case "fireball":return Flame;
                case "ghost_flint":case "ghost_axe":case "ghost_scepter":return Ethereal;
                case "circular_saw":case "drill":return Motor;
                case "medical_gun":return Heal;
                case "plasma_sledgehammer":case "particle_accelerator":case "obliterator":case "excalibur":case "dextroyer":case "vorpal_sword":return Plasma;
                case "wand":case "icicle":return Ice;
                case "slingshot":case "crossbow":case "shuriken":return Bow;
                case "potato_thrower":case "cactus_mace":case "sharp_tooth":return Organic;
                case "smg":case "minigun":case "chain_gun":return Rapid;
                case "fist":case "hand":case "stick":case "fighting_stick":case "hammer":case "rock":case "power_fist":case "plank":case "wrench":case "spiky_shield":return Heavy;
                default:return melee?Blade:Ballistic;
            }
        }
        public static int Support(string key)
        {
            switch(key)
            {
                case "shot:laser":return Laser;
                case "shot:rocket":return Rocket;
                case "shot:flame":case "shot:blazemander":return Flame;
                case "shot:tyler":case "shot:wandering_bot":return Electric;
                case "shot:garden":return Organic;
                case "shot:healing":return Heal;
                case "shield_hit":return Shield;
                case "moth_enter":case "moth_exit":case "goat_revive":case "mine_press":return Status;
                default:return key.StartsWith("shot:")?Rapid:Voice;
            }
        }
    }
}

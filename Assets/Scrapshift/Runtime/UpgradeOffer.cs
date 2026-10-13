namespace Scrapshift
{
    public enum UpgradeKind
    {
        Orbit,Pulse,Damage,Speed,Repair,Critical,Magnet,Armor,Regeneration,Harvesting,Move,LifeSteal,Melee,Ranged,Elemental,Engineering,Range,Dodge,Luck
    }
    public sealed class UpgradeOffer
    {
        public UpgradeKind Kind
        {
            get;
        }
        public int Tier
        {
            get;
        }
        public UnityEngine.Sprite Image
        {
            get;
        }
        public int Amount
        {
            get;
        }
        public UpgradeOffer(UpgradeKind kind,int tier,UpgradeEntry[] entries=null)
        {
            Kind=kind;
            Tier=tier;
            var entry=entries==null?null:System.Array.Find(entries,e=>e.stat==Stat);
            Amount=entry==null?tier:entry.values[tier-1];
            Image=entry?.image;
        }
        public string Stat=>Kind==UpgradeKind.LifeSteal?"lifesteal":Kind==UpgradeKind.Melee?"melee_damage":Kind==UpgradeKind.Ranged?"ranged_damage":Kind==UpgradeKind.Elemental?"elemental_damage":Kind==UpgradeKind.Engineering?"engineering":Kind==UpgradeKind.Range?"range":Kind==UpgradeKind.Dodge?"dodge":Kind==UpgradeKind.Luck?"luck":Kind==UpgradeKind.Damage?"percent_damage":Kind==UpgradeKind.Speed?"attack_speed":Kind==UpgradeKind.Repair?"max_hp":Kind==UpgradeKind.Critical?"crit_chance":Kind==UpgradeKind.Armor?"armor":Kind==UpgradeKind.Regeneration?"hp_regeneration":Kind==UpgradeKind.Harvesting?"harvesting":"speed";
        public string Title=>CyberneticTheme.UpgradeName(Stat).ToUpperInvariant();
        public string Detail=>ItemEntry.FormatStat(Stat,Amount);
        public string Rarity=>Tier==4?"LEGENDARY":Tier==3?"RARE":Tier==2?"UNCOMMON":"COMMON";
        public string Icon=>Kind==UpgradeKind.Critical?"!":Kind==UpgradeKind.Speed?">>":"+";
    }
}

namespace Scrapshift
{
    public enum UpgradeKind { Orbit, Pulse, Damage, Speed, Repair, Critical, Magnet }
    public sealed class UpgradeOffer
    {
        public UpgradeKind Kind { get; }
        public int Tier { get; }
        public UpgradeOffer(UpgradeKind kind, int tier) { Kind = kind; Tier = tier; }
        public string Rarity => Kind == UpgradeKind.Orbit || Kind == UpgradeKind.Pulse ? "NEW WEAPON" : Tier == 3 ? "EPIC" : Tier == 2 ? "RARE" : "COMMON";
        public string Title => new[] { "Orbit blades", "Pulse core", "Heavy rounds", "Quick hands", "Field lunch", "Deadeye", "Scavenger" }[(int)Kind];
        public string Icon => new[] { "X", "O", "+", ">>", "+", "!", "$" }[(int)Kind];
        public string Detail => Kind == UpgradeKind.Orbit ? "Two blades circle you.\nCut through nearby creatures."
            : Kind == UpgradeKind.Pulse ? "Unleash a shockwave\nevery 3 seconds."
            : Kind == UpgradeKind.Damage ? $"+{15 * Tier}% damage\nAll weapons hit harder."
            : Kind == UpgradeKind.Speed ? $"+{18 * Tier}% attack speed\n+0.25 movement speed"
            : Kind == UpgradeKind.Repair ? $"+{15 * Tier} maximum health\nHeal 40 HP\n+0.3 pickup range"
            : Kind == UpgradeKind.Critical ? $"+{8 * Tier}% critical chance\nCritical hits deal double damage.\nChance caps at 75%."
            : $"+{.65f * Tier:0.00} pickup range\n+0.35 movement speed";
    }
}

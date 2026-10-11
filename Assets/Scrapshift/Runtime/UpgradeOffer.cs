namespace Scrapshift {
public enum UpgradeKind {Orbit,Pulse,Damage,Speed,Repair,Critical,Magnet,Armor,Regeneration,Harvesting,Move}
public sealed class UpgradeOffer {
 public UpgradeKind Kind {get;}public int Tier {get;}
 public UpgradeOffer(UpgradeKind kind,int tier){Kind=kind;Tier=tier;}
 public string Stat=>Kind==UpgradeKind.Damage?"percent_damage":Kind==UpgradeKind.Speed?"attack_speed":Kind==UpgradeKind.Repair?"max_hp":Kind==UpgradeKind.Critical?"crit_chance":Kind==UpgradeKind.Armor?"armor":Kind==UpgradeKind.Regeneration?"hp_regeneration":Kind==UpgradeKind.Harvesting?"harvesting":"speed";
 public int Amount=>Tier*(Kind==UpgradeKind.Damage?5:Kind==UpgradeKind.Speed?5:Kind==UpgradeKind.Repair?3:Kind==UpgradeKind.Critical?3:Kind==UpgradeKind.Armor?1:Kind==UpgradeKind.Regeneration?2:Kind==UpgradeKind.Harvesting?5:3);
 public string Title=>ItemEntry.StatName("stat_"+Stat).ToUpperInvariant();public string Detail=>$"+{Amount} {ItemEntry.StatName("stat_"+Stat)}";
 public string Rarity=>Tier==4?"LEGENDARY":Tier==3?"EPIC":Tier==2?"RARE":"COMMON";public string Icon=>Kind==UpgradeKind.Critical?"!":Kind==UpgradeKind.Speed?">>":"+";
}}

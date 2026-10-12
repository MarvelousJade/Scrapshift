using System;
using UnityEngine;
namespace Scrapshift
{
    [Serializable] public sealed class UnitEntry
    {
        public string id;
        public int min,max;
        public float chance,distance;
    }
    [Serializable] public sealed class SpawnGroup
    {
        public float start,interval,reduce,minimum,chance,area,inset;
        public int repeats,bucket;
        public bool edge;
        public UnitEntry[] units;
    }
    [Serializable] public sealed class WaveEntry
    {
        public float duration;
        public int cap;
        public SpawnGroup[] groups;
    }
    [Serializable] public sealed class EnemyEntry
    {
        public string id,role,sprite;
        public float health,growth,speed,speedRandom,damage,damageGrowth,armor,armorGrowth,resistance,cooldown,range,chargeSpeed,dropChance;
        public int value;
        public Sprite image;
    }
    [Serializable] public sealed class Equipment
    {
        public string id,title,icon;
        public int tier,value,pellets,piercing;
        public bool melee;
        public float damage,interval,range,spread,crit,knockback;
        public Sprite image;
        public AudioClip[] sounds;
        public string[] setIds,soundNames;
        public StatEffect[] scaling;
        public WeaponTrait[] traits;
        public bool available=true;
        public float critMultiplier=2,lifesteal,projectileSpeed=30,pierceLoss=.5f,bounceLoss=.5f,reloadMultiplier=1,recoil=.25f,recoilHalf=.1f;
        public int bounce,reloadEvery=-1;
        public bool HasSet(string id)=>setIds!=null&&Array.IndexOf(setIds,id)>=0;
        public WeaponTrait Trait(string key)=>traits==null?null:Array.Find(traits,t=>t.key==key);
        public string Detail=>$"{damage:0} damage | {interval:0.00}s\n{range:0.0} range"+(pellets>1?$" | {pellets} shots":"")+(melee?"\nMelee":"\nRanged");
    }
    [Serializable] public sealed class StatEffect
    {
        public string key;
        public float value;
    }
    [Serializable] public sealed class ItemEntry
    {
        public string id,title,icon;
        public int tier,value;
        public StatEffect[] effects;
        public Sprite image;
        public string wearSlot;
        public Vector2 wearOffset;
        public float wearScale = .2f;
        public int wearOrder = 11;
        public AppearanceLayer appearance;
        public string Detail
        {
            get
            {
                string s="";
                foreach(var e in effects)s+=FormatStat(e.key,e.value)+"\n";
                return s.TrimEnd();
            }
        }
        public static string StatName(string key)
        {
            key=key.Replace("stat_","");
            if(key=="max_hp")return "Max HP";
            if(key=="hp_regeneration")return "HP Regeneration";
            if(key=="percent_damage")return "Damage";
            if(key=="crit_chance")return "Critical Chance";
            return System.Globalization.CultureInfo.InvariantCulture.TextInfo.ToTitleCase(key.Replace('_',' '));
        }
        public static string FormatStat(string key,float value)
        {
            string unit=(key.Contains("percent_damage")||key.Contains("attack_speed")||key.Contains("crit_chance")||key.EndsWith("speed")||key.Contains("lifesteal")||key.Contains("dodge"))?"%":"";
            return $"{value:+0;-0;0}{unit} {StatName(key)}";
        }
    }
    [Serializable] public sealed class UpgradeEntry
    {
        public string stat,icon;
        public int[] values;
        public Sprite image;
    }
    [Serializable] public sealed class CatalogData
    {
        public WaveEntry[] waves;
        public EnemyEntry[] enemies;
        public Equipment[] equipment;
        public ItemEntry[] items;
        public UpgradeEntry[] upgrades;
        public CharacterProfile[] characters;
        public SetEntry[] sets;
        public int rosterSlots;
    }
    public sealed class ReferenceCatalog:ScriptableObject
    {
        public CatalogData data;
        public Font referenceFont;
        public TextAsset uiText;
        public string starterVersion;
        void OnEnable(){if(data!=null)WeaponSets.Configure(data.sets);}
        public Sprite warning;
        public Sprite[] extraStatIcons;
        public Sprite[] scrapShapes, muzzleFrames;
        public Sprite repairPickup, shadow;
        public Material spriteMaterial;
        public Mesh shadowMesh;
        public Material shadowMaterial;
        public Equipment Weapon(string id,int tier)=>Array.Find(data.equipment,w=>w.id==id&&w.tier==tier);
        public EnemyEntry Enemy(string id)=>Array.Find(data.enemies,e=>e.id==id);
    }
    public sealed class ShopOffer
    {
        public int rolledWave;
        public Equipment weapon;
        public ItemEntry item;
        public bool locked;
        public int Value=>weapon!=null?weapon.value:item.value;
        public int Tier=>weapon!=null?weapon.tier:item.tier;
        public string Title=>weapon!=null?weapon.title:item.title;
        public string Detail=>weapon!=null?weapon.Detail:item.Detail;
        public Sprite Image=>weapon!=null?weapon.image:item.image;
    }
}

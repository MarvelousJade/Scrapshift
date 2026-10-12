using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Scrapshift
{
    public enum RunPhase
    {
        Title,Playing,Upgrade,Shop,Paused,Won,Lost,Loot
    }
    // Scene-independent economy: catalog and RNG are injectable for reproducible tests.
    public sealed partial class RunRules
    {
        public const int WaveCount=20;
        static readonly HashSet<string> shopItems = new HashSet<string>
        { "coffee", "beanie", "glasses", "helmet", "leather_vest", "bat", "fertilizer", "cake" };
        public static bool IsShopItem(string id) => shopItems.Contains(id);
        public static void ConfigureShopItems(ItemEntry[] items)
        {if(items==null||!items.Any(i=>i.available))return;shopItems.Clear();foreach(var i in items)if(i.available)shopItems.Add(i.id);}
        public static readonly int[] Durations=
        {
            20,25,30,35,40,45,50,55,60,60,60,60,60,60,60,60,60,60,60,90
        };
        public RunPhase Phase
        {
            get;
            private set;
        }
        =RunPhase.Title;
        public int Wave
        {
            get;
            private set;
        }
        public float Remaining
        {
            get;
            private set;
        }
        public float Health
        {
            get;
            private set;
        }
        public float MaxHealth=>Math.Max(1,10+Stat("max_hp"));
        public int Kills
        {
            get;
            private set;
        }
        public int Scrap
        {
            get;
            private set;
        }
        public int XP
        {
            get;
            private set;
        }
        public int Level
        {
            get;
            private set;
        }
        =0;
        public int PendingLevels
        {
            get;
            private set;
        }
        public int UpgradeCount
        {
            get;
            private set;
        }
        public int NextLevel=>(Level+4)*(Level+4);
        public float DamageScale=>Math.Max(0,1+Stat("percent_damage")/100);
        public float FireRate=>Math.Max(.1f,1+Stat("attack_speed")/100);
        public float MoveSpeed=>4.5f*Math.Max(.2f,1+Stat("speed")/100);
        public float PickupRadius=>Mathf.Max(.3f,1.5f*(1+Stat("pickup_range")/100));
        public float CritChance=>Stat("crit_chance")/100;
        public bool OrbitUnlocked=>false;
        public bool PulseUnlocked=>false;
        public bool WaveUpgrade=>Phase==RunPhase.Upgrade;
        public UpgradeOffer[] Offers
        {
            get;
            private set;
        }
        =Array.Empty<UpgradeOffer>();
        public ShopOffer[] Shop
        {
            get;
            private set;
        }
        =new ShopOffer[4];
        public readonly List<Equipment> Inventory=new List<Equipment>();
        public readonly List<ItemEntry> Items=new List<ItemEntry>();
        readonly Dictionary<string,float> stats=new Dictionary<string,float>();
        readonly System.Random random;
        ReferenceCatalog catalog;
        int rerolls,banked;float xpFraction;
        public int Banked=>banked;
        public int RerollPrice=>(int)(Wave*.75f)+Math.Max(1,(int)(Wave*.4f))*(rerolls+1);
        public bool CanReroll=>(Phase==RunPhase.Upgrade||Phase==RunPhase.Shop)&&Scrap>=RerollPrice;
        public RunRules(int seed=-1)
        {
            random=seed<0?new System.Random():new System.Random(seed);
        }
        public void Configure(ReferenceCatalog source)
        {
            catalog=source;
            ConfigureShopItems(source?.data.items);
            WeaponSets.Configure(source?.data.sets);
        }
        float PermanentStat(string name)=>stats.TryGetValue(name,out var v)?v:0;
        public float Stat(string name)=>(PermanentStat(name)+WeaponSets.Bonus(Inventory,name)+EquipmentStat(name))*(Character?.Gain(name)??1);
        public int SetCount(WeaponSet set)=>WeaponSets.Count(Inventory,set);
        public float WeaponRange(Equipment weapon)=>Mathf.Max(.25f,weapon.range+Stat("range")*(weapon.melee?.005f:.01f)+(Character?.WeaponBonus(weapon,"max_range")??0)/100);
        void Add(string name,float value)
        {
            stats[name]=PermanentStat(name)+value;
            Health=Math.Min(Health,MaxHealth);
        }
        public void ReturnToTitle() { Phase=RunPhase.Title; }
        public void Start(CharacterProfile character=null,string weaponId=null)
        {
            stats.Clear();xpFraction=0;Character=character;
            Inventory.Clear();
            Items.Clear();
            crates.Clear();CrateItem=null;regenElapsed=lifeStealCooldown=0;
            if(catalog!=null)
            {
                var starting=catalog.Weapon(weaponId??"pistol",1);
                if(!CanEquip(starting)||character!=null&&Array.IndexOf(character.startingWeapons,starting.id)<0)throw new ArgumentException("Invalid starting weapon for character");
                Inventory.Add(starting);
                if(character!=null)Inventory.Add(catalog.Weapon(character.DefaultWeapon,1));
            }
            if(character!=null)foreach(var s in character.stats)stats[s.key]=s.value;
            Phase=RunPhase.Playing;
            Wave=1;
            Remaining=Durations[0];
            Health=MaxHealth;
            Kills=Scrap=XP=PendingLevels=UpgradeCount=rerolls=banked=0;
            Level=0;
            Offers=Array.Empty<UpgradeOffer>();
            Shop=new ShopOffer[4];
        }
        public void Tick(float seconds)
        {
            if(Phase!=RunPhase.Playing||seconds<=0)return;
            TickSurvival(Math.Min(seconds,Remaining));
            Remaining=Math.Max(0,Remaining-seconds);
            if(Remaining>0)return;
            if(Wave==WaveCount)
            {
                Phase=RunPhase.Won;
                return;
            }
            int harvest=(int)Stat("harvesting");
            Scrap=Math.Max(0,Scrap+harvest);
            if(harvest>0)GainXP(harvest);
            if(harvest>0)Add("harvesting",Mathf.Ceil(harvest*Mathf.Max(0,5+Stat("harvesting_growth"))/100));
            rerolls=0;
            OpenRewards();
        }
        public void Hurt(float amount)
        {
            if(Phase!=RunPhase.Playing||amount<=0)return;
            if(random.NextDouble()<Mathf.Clamp(Stat("dodge")/100,0,.6f))return;
            float armor=Stat("armor");
            amount*=armor>=0?1/(1+armor/15):2-1/(1-armor/15);
            Health=Math.Max(0,Health-Math.Max(1,amount));
            if(Health==0)Phase=RunPhase.Lost;
        }
        public void Heal(float amount)
        {
            if(Phase==RunPhase.Playing&&amount>0)Health=Math.Min(MaxHealth,Health+amount);
            if(Health>=MaxHealth)regenElapsed=0;
        }
        public void DefeatBoss()
        {
            if(Phase==RunPhase.Playing&&Wave==20)Phase=RunPhase.Won;
        }
        public void Kill()
        {
            if(Phase==RunPhase.Playing)Kills++;
        }
        void GainXP(int value)
        {
            float gained=value*Mathf.Max(0,1+Stat("xp_gain")/100)+xpFraction;
            XP+=(int)gained;xpFraction=gained-(int)gained;
            while(XP>=NextLevel)
            {
                XP-=NextLevel;
                Level++;
                Add("max_hp",1);
                PendingLevels++;
            }
        }
        public void Collect(int value)
        {
            if(Phase!=RunPhase.Playing||value<=0)return;
            int bonus=Math.Min(value,banked);
            banked-=bonus;
            Scrap+=value+bonus;
            GainXP(value+bonus);
        }
        public void BankMaterials(int value)
        {
            banked+=Math.Max(0,value);
        }
        public int Price(int value)=>Math.Max(1,(int)((value+Wave+value*Wave*.1f)*Mathf.Max(0,1+Stat("items_price")/100)));
        public int RecycleValue(int value)=>Math.Max(1,(int)(Price(value)*Mathf.Clamp(.25f+Stat("recycling_gains")/100,.01f,1)));
        static int PriceAt(int value,int wave)=>Math.Max(1,(int)(value+wave+value*wave*.1f));
        public int Price(ShopOffer offer){int wave=offer.rolledWave>0?offer.rolledWave:Wave;return Math.Max(1,(int)((offer.Value+wave+offer.Value*wave*.1f)*Mathf.Max(0,1+Stat("items_price")/100)));}
        void Draft(bool reroll=false)
        {
            var pool=new List<UpgradeKind>
            {
                UpgradeKind.Damage,UpgradeKind.Speed,UpgradeKind.Repair,UpgradeKind.Critical,UpgradeKind.Armor,UpgradeKind.Regeneration,UpgradeKind.Harvesting,UpgradeKind.Move
            };
            if(reroll) pool.RemoveAll(kind=>Offers.Any(o=>o.Kind==kind));
            int rewardLevel=Level-PendingLevels+1;
            Offers=new UpgradeOffer[4];
            for(int i=0;i<4;i++)
            {
                int p=random.Next(pool.Count);
                int tier=ReferenceRarity.Upgrade(rewardLevel,Stat("luck"),random.NextDouble());
                Offers[i]=new UpgradeOffer(pool[p],tier,catalog?.data.upgrades);
                pool.RemoveAt(p);
            }
        }
        public void Choose(int choice)
        {
            if(Phase!=RunPhase.Upgrade||choice<0||choice>=Offers.Length)return;
            var o=Offers[choice];
            Add(o.Stat,o.Amount);
            UpgradeCount++;
            PendingLevels--;
            rerolls=0;
            if(PendingLevels>0)Draft();
            else OpenShop();
        }
        void OpenShop()
        {
            Phase=RunPhase.Shop;
            rerolls=0;
            RollShop();
        }
        void RollShop()
        {
            if(catalog==null)return;
            int guaranteed=Wave<=2?2:Wave<=5?1:0;
            int existing=Shop.Count(s=>s!=null&&s.locked&&s.weapon!=null);
            var excluded=Shop.Where(s=>s!=null).ToList();
            for(int i=0;i<4;i++)
            {
                if(Shop[i]!=null&&Shop[i].locked)continue;
                bool weapon=existing<guaranteed||(Wave>2&&random.NextDouble()<.35);
                int tier=ReferenceRarity.Roll(Wave,Stat("luck"),random.NextDouble());
                if(weapon)
                {
                    var fallback=catalog.data.equipment.Where(w=>w.tier==tier&&CanEquip(w)).ToArray();
                    var pool=fallback.Where(w=>!excluded.Any(s=>s.weapon==w)).ToArray();
                    if(pool.Length==0)pool=fallback;
                    double wanted=random.NextDouble();
                    float early=Math.Max(0,6-Wave);
                    var preferred=wanted<.2+early*.01 ? pool.Where(w=>Inventory.Any(owned=>owned.id==w.id)).ToArray() :
                        wanted<.35+early*.04 ? pool.Where(w=>Inventory.Any(owned=>ShopSelection.SharesSet(owned,w))).ToArray() : pool;
                    if(preferred.Length>0)pool=preferred;
                    Shop[i]=new ShopOffer
                    {
                        weapon=pool[random.Next(pool.Length)],rolledWave=Wave
                    };
                    existing++;
                }
                else
                {
                    var pool=ShopSelection.ItemPool(catalog.data.items,Character,tier,random.NextDouble()<.05,
                        excluded.Where(s=>s.item!=null).Select(s=>s.item),Items.Concat(Shop.Where(s=>s?.item!=null&&s.locked).Select(s=>s.item)));
                    if(pool.Length==0){Shop[i]=null;continue;}
                    Shop[i]=new ShopOffer
                    {
                        item=pool[random.Next(pool.Length)],rolledWave=Wave
                    };
                }
                excluded.Add(Shop[i]);
            }
        }
        public void ToggleLock(int index)
        {
            if(Phase==RunPhase.Shop&&index>=0&&index<4&&Shop[index]!=null)Shop[index].locked=!Shop[index].locked;
        }
        int FindMatch(Equipment w)=>w.tier>=4?-1:Inventory.FindIndex(x=>x.id==w.id&&x.tier==w.tier);
        public bool CanBuy(int i)
        {
            if(Phase!=RunPhase.Shop||i<0||i>=4||Shop[i]==null)return false;
            var s=Shop[i];
            return Scrap>=Price(s)&&(s.weapon==null||CanEquip(s.weapon)&&(Inventory.Count<6||FindMatch(s.weapon)>=0));
        }
        public bool Buy(int i)
        {
            if(!CanBuy(i))return false;
            var s=Shop[i];
            Scrap-=Price(s);
            if(s.weapon!=null)
            {
                if(Inventory.Count==6)Inventory[FindMatch(s.weapon)]=catalog.Weapon(s.weapon.id,s.weapon.tier+1);
                else Inventory.Add(s.weapon);
            }
            else
            {
                ApplyItem(s.item);
            }
            Health=Math.Min(Health,MaxHealth);
            Shop[i]=null;
            return true;
        }
        public bool CanCombine(int i)=>Phase==RunPhase.Shop&&i>=0&&i<Inventory.Count&&Inventory[i].tier<4&&Inventory.Where((w,j)=>j!=i&&w.id==Inventory[i].id&&w.tier==Inventory[i].tier).Any();
        public void Combine(int index)
        {
            if(!CanCombine(index))return;
            var w=Inventory[index];
            for(int i=0;i<Inventory.Count;i++)if(i!=index&&Inventory[i].id==w.id&&Inventory[i].tier==w.tier)
            {
                Inventory[index]=catalog.Weapon(w.id,w.tier+1);
                Inventory.RemoveAt(i);
                Health=Math.Min(Health,MaxHealth);
                return;
            }
        }
        public void Recycle(int i)
        {
            if(Phase!=RunPhase.Shop||i<0||i>=Inventory.Count)return;
            Scrap+=RecycleValue(Inventory[i].value);
            Inventory.RemoveAt(i);
            Health=Math.Min(Health,MaxHealth);
        }
        public void Reroll()
        {
            if(!CanReroll)return;
            Scrap-=RerollPrice;
            rerolls++;
            if(Phase==RunPhase.Upgrade)Draft(true);
            else RollShop();
        }
        public void NextWave()
        {
            if(Phase!=RunPhase.Shop)return;
            Wave++;
            Remaining=Durations[Wave-1];
            Health=MaxHealth;
            regenElapsed=lifeStealCooldown=0;
            Phase=RunPhase.Playing;
            for(int i=0;i<4;i++)if(Shop[i]!=null&&!Shop[i].locked)Shop[i]=null;
        }
        public void Pause()
        {
            if(Phase==RunPhase.Playing)Phase=RunPhase.Paused;
            else if(Phase==RunPhase.Paused)Phase=RunPhase.Playing;
        }
        public static Vector2 ClampPosition(Vector2 p)=>ArenaGeometry.Clamp(p);
        public static bool Touches(Vector2 a,Vector2 b,float r)=>(a-b).sqrMagnitude<=r*r;
    }
}

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
        float healthCap=float.PositiveInfinity,speedCap=float.PositiveInfinity;
        public float MaxHealth=>Math.Max(1,Math.Min(healthCap,10+Stat("max_hp")));
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
        public int NextLevel=>Math.Max(1,(int)((Level+4)*(Level+4)*(1+Stat("next_level_xp_needed")/100)));
        public float DamageScale=>Math.Max(0,1+Stat("percent_damage")/100);
        public float FireRate=>Math.Max(.1f,1+Stat("attack_speed")/100);
        public float MoveSpeed=>4.5f*Math.Max(.2f,1+Math.Min(speedCap,Stat("speed"))/100);
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
        public int RerollPrice=>Phase==RunPhase.Shop&&freeShopRerolls>0?0:(int)(Wave*.75f)+Math.Max(1,(int)(Wave*.4f))*(rerolls+1);
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
        public float Stat(string name)=>(PermanentStat(name)+SetStat(name)+EquipmentStat(name)+ConditionalStat(name))*(Character?.Gain(name)??1)+LinkedStat(name);
        public int SetCount(WeaponSet set)=>StatWithoutSets("all_weapons_count_for_sets")>0?Inventory.Count:WeaponSets.Count(Inventory,set);
        public float WeaponRange(Equipment weapon)=>Mathf.Max(.25f,weapon.range+Stat("range")*(weapon.melee?.005f:.01f)+(Character?.WeaponBonus(weapon,"max_range")??0)/100);
        void Add(string name,float value)
        {
            stats[name]=PermanentStat(name)+value;
            Health=Math.Min(Health,MaxHealth);
        }
        public void ReturnToTitle() { Phase=RunPhase.Title; }
        public void Start(CharacterProfile character=null,string weaponId=null)
        {
            Level=0;burningKills=0;queuedBaits=0;ExtraElite=false;UsedLocks=false;runConsumables=runRecycles=projectileHits=0;runHealing=0;stats.Clear();nextWaveStats.Clear();burnKillCounts.Clear();xpFraction=0;Character=character;
            ResetConditionalEffects();
            RemovedEnemies=0;IsHordeWave=false;healthCap=speedCap=float.PositiveInfinity;LivingEnemies=LivingTrees=BurningEnemies=StructureCount=PetCount=0;
            Inventory.Clear();
            Items.Clear();
            crates.Clear();CrateItem=null;regenElapsed=lifeStealCooldown=0;
            if(character!=null)foreach(var s in character.stats)stats[s.key]=s.value;
            if(catalog!=null&&WeaponSlots>0)
            {
                var starting=catalog.Weapon(weaponId??"pistol",MinimumWeaponTier)??catalog.Weapon(weaponId??"pistol",1);
                if(!CanEquip(starting)||character!=null&&Array.IndexOf(character.startingWeapons,starting.id)<0)throw new ArgumentException("Invalid starting weapon for character");
                Inventory.Add(starting);
                if(character!=null&&WeaponSlots>1){var bonus=catalog.Weapon(character.DefaultWeapon,Math.Max(character.bonusTier,MinimumWeaponTier));if(!CanEquip(bonus))bonus=starting;Inventory.Add(bonus);}
            }
            if(character!=null)foreach(var s in character.stats)stats[s.key]=s.value;
            if(character?.startingItems!=null)foreach(var id in character.startingItems)
            {var item=Array.Find(catalog.data.items,i=>i.id==id);if(item==null)throw new InvalidOperationException("Missing starting implant: "+id);ApplyItem(item);}
            foreach(var acquired in Inventory)Progress?.Count("weapon:"+acquired.id);
            Phase=RunPhase.Playing;
            Wave=1;
            Remaining=Durations[0];
            Health=MaxHealth;
            Kills=Scrap=XP=PendingLevels=UpgradeCount=rerolls=banked=0;
            Level=0;freeShopRerolls=knownFreeRerolls=0;ApplyNextWaveEffects();BeginItemWave();
            Offers=Array.Empty<UpgradeOffer>();
            Shop=new ShopOffer[4];
        }
        public void Tick(float seconds)
        {
            if(Phase!=RunPhase.Playing||seconds<=0)return;
            TickCharacter(Math.Min(seconds,Remaining));TickConditionalEffects(Math.Min(seconds,Remaining));
            if(Phase!=RunPhase.Playing)return;
            Remaining=Math.Max(0,Remaining-seconds);
            if(Remaining>0)return;
            PermanentTrigger("stats_end_of_wave");ConvertStats("convert_stats_end_of_wave",true);
            if(Wave==WaveCount)
            {
                Phase=RunPhase.Won;
                ResetConditionalEffects();
                return;
            }
            int harvest=(int)Stat("harvesting");
            int income=harvest+(Mathf.FloorToInt((LivingEnemies+RemovedEnemies)*Stat("pacifist")/100+.5f)/(IsHordeWave?2:1))+LivingTrees*(int)Stat("cryptid");
            Scrap=Math.Max(0,Scrap+income);
            if(income>0)GainXP(income);
            if(harvest>0)Add("harvesting",Mathf.Ceil(harvest*Mathf.Max(0,5+Stat("harvesting_growth"))/100));
            rerolls=0;
            ResetConditionalEffects();
            OpenRewards();
        }
        public void Hurt(float amount)
        {
            if(Phase!=RunPhase.Playing||amount<=0)return;
            LastHitProtected=false;LastHitDodged=false;DodgeRetaliation=0;
            if(random.NextDouble()<Mathf.Clamp(Stat("dodge")/100,0,DodgeLimit)){LastHitDodged=true;DodgeTriggers();RetaliationTriggers();return;}
            if(HitProtection>0){HitProtection--;LastHitProtected=true;return;}
            float armor=Stat("armor");
            amount*=armor>=0?1/(1+armor/15):2-1/(1-armor/15);
            Health=Stat("die_in_one_hit")>0?0:Math.Max(0,Health-Math.Max(1,amount));
            DamageTriggers();
            if(Health==0)Phase=RunPhase.Lost;
        }
        public void Heal(float amount,bool fromTorture=false)
        {
            float before=Health;
            if(Phase==RunPhase.Playing&&amount>0&&Stat("no_heal")<=0&&(Stat("torture")<=0||fromTorture))Health=Math.Min(MaxHealth,Health+amount);
            if(Health>before){runHealing+=Health-before;Progress?.Check("chal_medicine",runHealing);RandomDamageTrigger("dmg_when_heal");}
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
                PermanentTrigger("stats_on_level_up");
                PendingLevels++;
            }
        }
        public void Collect(int value)
        {
            if(Phase!=RunPhase.Playing||value<=0)return;
            int bonus=Math.Min(value,banked);
            banked-=bonus;
            int boost=random.NextDouble()<Mathf.Clamp01(Stat("chance_double_gold")/100)?2:1;
            Scrap+=(value+bonus)*boost;Progress?.Count("materials",(value+bonus)*boost);
            GainXP((value+bonus)*boost);
            if(random.NextDouble()<Mathf.Clamp01(Stat("heal_when_pickup_gold")/100))Heal(1);
            RandomDamageTrigger("dmg_when_pickup_gold");
        }
        public void BankMaterials(int value)
        {
            banked+=Math.Max(0,value);
        }
        public int Price(int value)=>Math.Max(1,(int)((value+Wave+value*Wave*(Difficulty>=6?.11f:.1f))*Mathf.Max(0,1+Stat("items_price")/100)));
        public int RecycleValue(int value)=>Math.Max(1,(int)(Price(value)*Mathf.Clamp(.25f+Stat("recycling_gains")/100,.01f,1)));
        int PriceAt(int value,int wave)=>Math.Max(1,(int)(value+wave+value*wave*(Difficulty>=6?.11f:.1f)));
        public int Price(ShopOffer offer)
        {
            int wave=offer.rolledWave>0?offer.rolledWave:Wave;
            float value=offer.Value*(offer.weapon!=null?1+Stat("weapons_price")/100:1);
            float specific=Effects("specific_items_price").Where(e=>e.key=="item_"+offer.item?.id).Sum(e=>e.value);
            int cost=Math.Max(1,(int)((value+wave+value*wave*(Difficulty>=6?.11f:.1f))*Mathf.Max(0,1+(Stat("items_price")+specific)/100)));
            return UsesHealthShop?Mathf.CeilToInt(cost/20f):cost;
        }
        void Draft(bool reroll=false)
        {
            var pool=new List<UpgradeKind>
            {
                UpgradeKind.Damage,UpgradeKind.Speed,UpgradeKind.Repair,UpgradeKind.Critical,UpgradeKind.Armor,UpgradeKind.Regeneration,UpgradeKind.Harvesting,UpgradeKind.Move,
                UpgradeKind.LifeSteal,UpgradeKind.Melee,UpgradeKind.Ranged,UpgradeKind.Elemental,UpgradeKind.Engineering,UpgradeKind.Range,UpgradeKind.Dodge,UpgradeKind.Luck
            };
            pool.RemoveAll(kind=>(Character?.bannedUpgrades??Array.Empty<string>()).Contains("upgrade_"+new UpgradeOffer(kind,1).Stat));
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
            if(Stat("destroy_weapons")>0)Inventory.Clear();
            UpgradeWeaponImplants();BeginShopRerolls();rerolls=0;
            RollShop();
            if(!GameSettings.Flag("keepLock"))foreach(var offer in Shop)if(offer!=null)offer.locked=false;
        }
        void RollShop()
        {
            if(catalog==null)return;
            int guaranteed=Math.Max((int)Stat("minimum_weapons_in_shop"),Wave<=2?2:Wave<=5?1:0);
            var guaranteedItems=new Queue<string>(Effects("guaranteed_shop_items").Select(e=>e.key.Replace("item_","")));
            int existing=Shop.Count(s=>s!=null&&s.locked&&s.weapon!=null);
            var excluded=Shop.Where(s=>s!=null).ToList();
            for(int i=0;i<4;i++)
            {
                if(Shop[i]!=null&&Shop[i].locked)continue;
                bool weapon=WeaponSlots>0&&(Wave<=2?existing<guaranteed:guaranteedItems.Count==0&&(existing<guaranteed||random.NextDouble()<.35));
                int tier=ReferenceRarity.Roll(Wave,Stat("luck"),random.NextDouble());
                if(weapon)
                {
                    tier=Mathf.Clamp(tier,MinimumWeaponTier,MaximumWeaponTier);
                    var fallback=catalog.data.equipment.Where(w=>w.tier==tier&&CanEquip(w)).ToArray();
                    if(fallback.Length==0){Shop[i]=null;continue;}
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
                    var pool=ShopSelection.ItemPool(catalog.data.items,Character,tier,random.NextDouble()<WantedTagChance,
                        excluded.Where(s=>s.item!=null).Select(s=>s.item),Items.Concat(Shop.Where(s=>s?.item!=null&&s.locked).Select(s=>s.item)));
                    if(guaranteedItems.Count>0){string id=guaranteedItems.Dequeue();pool=catalog.data.items.Where(item=>item.id==id).ToArray();}
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
            if(Phase==RunPhase.Shop&&index>=0&&index<4&&Shop[index]!=null){Shop[index].locked=!Shop[index].locked;if(Shop[index].locked)UsedLocks=true;}
        }
        int FindMatch(Equipment w)=>w.tier>=MaximumWeaponTier?-1:Inventory.FindIndex(x=>x.id==w.id&&x.tier==w.tier);
        public bool CanBuy(int i)
        {
            if(Phase!=RunPhase.Shop||i<0||i>=4||Shop[i]==null)return false;
            var s=Shop[i];
            return (UsesHealthShop?MaxHealth>Price(s):Scrap>=Price(s))&&(s.weapon==null||CanEquip(s.weapon)&&(Inventory.Count<WeaponSlots&&CanAddWeapon(s.weapon)||FindMatch(s.weapon)>=0));
        }
        public bool Buy(int i)
        {
            if(!CanBuy(i))return false;
            var s=Shop[i];
            if(s.weapon!=null)Progress?.Count("weapon:"+s.weapon.id);
            if(UsesHealthShop)Add("max_hp",-Price(s)/(Character?.Gain("max_hp")??1));else Scrap-=Price(s);
            if(s.weapon!=null)
            {
                if(Inventory.Count>=WeaponSlots||!CanAddWeapon(s.weapon))Inventory[FindMatch(s.weapon)]=catalog.Weapon(s.weapon.id,s.weapon.tier+1);
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
        public bool CanCombine(int i)=>Phase==RunPhase.Shop&&i>=0&&i<Inventory.Count&&Inventory[i].tier<MaximumWeaponTier&&Inventory.Where((w,j)=>j!=i&&w.id==Inventory[i].id&&w.tier==Inventory[i].tier).Any();
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
            Scrap+=RecycleValue(Inventory[i].value);runRecycles++;Progress?.Check("chal_recycling",runRecycles);
            Inventory.RemoveAt(i);
            Health=Math.Min(Health,MaxHealth);
        }
        public void Reroll()
        {
            if(!CanReroll)return;
            int cost=RerollPrice;Scrap-=cost;
            if(Phase==RunPhase.Shop&&freeShopRerolls>0)freeShopRerolls--;else rerolls++;
            if(Phase==RunPhase.Upgrade)Draft(true);
            else RollShop();
        }
        public void NextWave()
        {
            if(Phase!=RunPhase.Shop)return;
            Wave=PrepareNextWave();
            Remaining=Durations[Wave-1];
            ResetConditionalEffects();
            Health=MaxHealth;
            regenElapsed=lifeStealCooldown=0;
            Phase=RunPhase.Playing;ApplyNextWaveEffects();BeginItemWave();
            for(int i=0;i<4;i++)if(Shop[i]!=null&&!Shop[i].locked)Shop[i]=null;
        }
        public void Abandon() { if(Phase==RunPhase.Paused){Health=0;Phase=RunPhase.Lost;} }
        public void Pause()
        {
            if(Phase==RunPhase.Playing)Phase=RunPhase.Paused;
            else if(Phase==RunPhase.Paused)Phase=RunPhase.Playing;
        }
        public static Vector2 ClampPosition(Vector2 p)=>ArenaGeometry.Clamp(p);
        public static bool Touches(Vector2 a,Vector2 b,float r)=>(a-b).sqrMagnitude<=r*r;
    }
}

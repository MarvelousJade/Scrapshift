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
        // Supplied DebugService.starting_gold is unconditionally used by RunData.
        public const int ReferenceStartingMaterials=30;
        public const int WaveCount=20;
        static readonly HashSet<string> shopItems = new HashSet<string>
        { "coffee", "beanie", "glasses", "helmet", "leather_vest", "bat", "fertilizer", "cake" };
        public static bool IsShopItem(string id) => shopItems.Contains(id);
        public static void ConfigureShopItems(ItemEntry[] items)
        {if(items==null)return;shopItems.Clear();foreach(var i in items)if(i.available&&i.maxCount!=0)shopItems.Add(i.id);}
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
            get { ReconcileMaximumHealth();return IntermissionHealth?MaxHealth:currentHealth; }
            private set { currentHealth=value; }
        }
        float currentHealth,observedMaxHealth;
        bool initializingHealth,reconcilingHealth;
        bool IntermissionHealth=>Phase==RunPhase.Shop||Phase==RunPhase.Upgrade||Phase==RunPhase.Loot||Phase==RunPhase.Paused&&pausedFrom!=RunPhase.Playing;
        // Source StatsManager reloads the final dirty stats together. Observe the
        // effective maximum at a health/physics boundary, not after each Add.
        void ReconcileMaximumHealth()
        {
            if(initializingHealth||reconcilingHealth)return;
            if(Phase!=RunPhase.Playing&&!(Phase==RunPhase.Paused&&pausedFrom==RunPhase.Playing))return;
            reconcilingHealth=true;
            try
            {
                float maximum=MaxHealth;
                if(currentHealth>0&&maximum>observedMaxHealth)currentHealth+=maximum-observedMaxHealth;
                observedMaxHealth=maximum;
            }
            finally { reconcilingHealth=false; }
        }
        void FinishHealthInitialization(){observedMaxHealth=MaxHealth;initializingHealth=false;}
        float healthCap=float.PositiveInfinity,speedCap=float.PositiveInfinity;
        public float MaxHealth=>Math.Max(1,(int)Math.Min(healthCap,Stat("max_hp")));
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
        public float XPProgress=>XP+xpFraction;
        public float NextLevelExact=>Math.Max(1,(Level+4)*(Level+4)*(1+Stat("next_level_xp_needed")/100));
        public int NextLevel=>Mathf.CeilToInt(NextLevelExact);
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
        int rerolls,banked,weaponSlotsEarned;float xpFraction;bool bonusFreeShopReroll;
        public int Banked=>banked;
        public bool Abandoned {get;private set;}
        public int RerollPrice=>Phase==RunPhase.Shop&&(bonusFreeShopReroll||freeShopRerolls>0)?0:Mathf.CeilToInt(((int)(Wave*.75f)+Math.Max(1,(int)(Wave*.4f))*(rerolls+1))*Mathf.Max(.1f,1+Stat("reroll_price")/100));
        public bool CanReroll=>(Phase==RunPhase.Upgrade&&Offers.Length>1||Phase==RunPhase.Shop&&Shop.Count(o=>o?.locked==true)<4)&&Scrap>=RerollPrice;
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
        public float Stat(string name)=>(PermanentStat(name)+(name=="max_hp"?10:0)+SetStat(name)+EquipmentStat(name)+ConditionalStat(name))*(Character?.Gain(name)??1)+LinkedStat(name);
        public int SetCount(WeaponSet set)=>StatWithoutSets("all_weapons_count_for_sets")>0?Inventory.Count:WeaponSets.Count(Inventory,set);
        public float WeaponRange(Equipment weapon)=>(int)Mathf.Max(25,weapon.range*100+Stat("range")*(weapon.melee?.5f:1)+(Character?.WeaponBonus(weapon,"max_range")??0))/100f;
        void Add(string name,float value)
        {
            stats[name]=PermanentStat(name)+value;
        }
        public void ReturnToTitle() { Phase=RunPhase.Title; }
        public void Start(CharacterProfile character=null,string weaponId=null,int startingMaterials=0)
        {
            initializingHealth=true;
            ResetWaveSettlement();
            Abandoned=false;
            ResetSpecialShopEffects();
            Level=0;burningKills=0;queuedBaits=0;ExtraElite=false;UsedLocks=false;runConsumables=runRecycles=projectileHits=0;runHealing=0;stats.Clear();nextWaveStats.Clear();burnKillCounts.Clear();xpFraction=0;bonusFreeShopReroll=false;weaponSlotsEarned=0;Character=character;
            ResetConditionalEffects();
            RemovedEnemies=0;IsHordeWave=false;healthCap=speedCap=float.PositiveInfinity;LivingEnemies=LivingTrees=BurningEnemies=StructureCount=PetCount=0;
            Inventory.Clear();
            Items.Clear();
            crates.Clear();CrateItem=null;ResetSurvival();
            if(character!=null)foreach(var s in character.stats)stats[s.key]=s.value;
            if(catalog!=null&&WeaponSlots>0)
            {
                var starting=catalog.Weapon(weaponId??"pistol",MinimumWeaponTier)??catalog.Weapon(weaponId??"pistol",1);
                if(!CanEquip(starting)||character!=null&&Array.IndexOf(character.startingWeapons,starting.id)<0)throw new ArgumentException("Invalid starting weapon for character");
                Inventory.Add(starting);
                if(character!=null&&!string.IsNullOrEmpty(character.bonusWeapon))
                {
                    var bonus=catalog.Weapon(character.bonusWeapon,character.bonusTier);
                    if(!CanEquip(bonus))throw new InvalidOperationException("Invalid character-granted starting weapon: "+character.bonusWeapon);
                    Inventory.Add(bonus);
                }
            }
            if(character!=null)foreach(var s in character.stats)stats[s.key]=s.value;
            if(character?.startingItems!=null)foreach(var id in character.startingItems)
            {var item=Array.Find(catalog.data.items,i=>i.id==id);if(item==null)throw new InvalidOperationException("Missing starting implant: "+id);ApplyItem(item);}
            foreach(var acquired in Inventory)Progress?.Count("weapon:"+acquired.id);
            Phase=RunPhase.Playing;
            Wave=1;
            Remaining=Durations[0];
            Health=MaxHealth;
            Kills=XP=PendingLevels=UpgradeCount=rerolls=banked=0;Scrap=Math.Max(0,startingMaterials);
            Level=0;freeShopRerolls=knownFreeRerolls=0;ApplyNextWaveEffects();BeginItemWave();
            FinishHealthInitialization();
            Offers=Array.Empty<UpgradeOffer>();
            Shop=new ShopOffer[4];
        }
        public void SetShowcaseWave(int wave)
        {
            if(Phase!=RunPhase.Playing)throw new InvalidOperationException("A showcase wave requires an active run.");
            if(wave<1||wave>WaveCount)throw new ArgumentOutOfRangeException(nameof(wave));
            Wave=wave;
            Remaining=Durations[wave-1];
        }
        public void Tick(float seconds)
        {
            if(Phase!=RunPhase.Playing||waveSettlementPending||seconds<=0)return;
            ReconcileMaximumHealth();
            TickCharacter(Math.Min(seconds,Remaining));TickConditionalEffects(Math.Min(seconds,Remaining));
            if(Phase!=RunPhase.Playing)return;
            Remaining=Math.Max(0,Remaining-seconds);
            if(Remaining>0)return;
            // Scene-free rule consumers fast-forward the cleanup lifecycle.
            // ArenaGame intercepts expiry and advances these stages in real time.
            BeginWaveSettlement();
            AdvanceWaveSettlement(WaveSettlementDuration);
            CompleteWaveSettlement();
        }
        public const float HarvestingGrowthDelay=.66f;
        bool waveSettlementPending,harvestingGrowthScheduled,harvestingGrowthApplied;
        float waveSettlementAge;
        public bool WaveSettlementPending=>waveSettlementPending;
        public float WaveSettlementAge=>waveSettlementAge;
        public float WaveSettlementDuration=>Wave==WaveCount?4f:2f;
        public int WaveSettlementReceiptWave {get;private set;}
        public int MaterialsBeforeSettlement {get;private set;}
        public int MaterialsAfterSettlement {get;private set;}
        public float HarvestingAtSettlement {get;private set;}
        void ResetWaveSettlement()
        {
            waveSettlementPending=harvestingGrowthScheduled=harvestingGrowthApplied=false;waveSettlementAge=0;
            WaveSettlementReceiptWave=MaterialsBeforeSettlement=MaterialsAfterSettlement=0;HarvestingAtSettlement=0;
        }
        public bool BeginWaveSettlement()
        {
            if(Phase!=RunPhase.Playing||waveSettlementPending)return false;
            waveSettlementPending=true;waveSettlementAge=0;Remaining=0;
            PermanentTrigger("stats_end_of_wave");ConvertStats("convert_stats_end_of_wave",true);
            // Source Main credits harvesting before cleanup consumes pickups
            // and resets TempStats. Its separate .66-second timer grows the
            // newly effective stat after that reset, not the income snapshot.
            float harvest=Stat("harvesting");
            int income=(int)(harvest+(Mathf.FloorToInt((LivingEnemies+RemovedEnemies)*Stat("pacifist")/100+.5f)/(IsHordeWave?2:1))+LivingTrees*Stat("cryptid"));
            WaveSettlementReceiptWave=Wave;MaterialsBeforeSettlement=Scrap;HarvestingAtSettlement=harvest;
            Scrap=Math.Max(0,Scrap+income);MaterialsAfterSettlement=Scrap;
            if(income>0)GainXP(income);
            harvestingGrowthScheduled=harvest>0;harvestingGrowthApplied=false;
            rerolls=0;ResetConditionalEffects();
            return true;
        }
        public void AdvanceWaveSettlement(float seconds)
        {
            if(!waveSettlementPending||Phase!=RunPhase.Playing||seconds<=0)return;
            waveSettlementAge+=seconds;
            if(!harvestingGrowthScheduled||harvestingGrowthApplied||waveSettlementAge<HarvestingGrowthDelay)return;
            harvestingGrowthApplied=true;
            float harvest=Stat("harvesting");
            if(harvest>0)Add("harvesting",Mathf.Ceil(harvest*Mathf.Max(0,5+Stat("harvesting_growth"))/100));
        }
        public bool CompleteWaveSettlement()
        {
            if(!waveSettlementPending||Phase!=RunPhase.Playing||waveSettlementAge<WaveSettlementDuration)return false;
            waveSettlementPending=false;
            if(Wave==WaveCount)Phase=RunPhase.Won;
            else OpenRewards();
            return true;
        }
        public void Hurt(float amount)
        {
            if(Phase!=RunPhase.Playing||amount<=0)return;
            LastHitProtected=false;LastHitDodged=false;DodgeRetaliation=0;
            if(random.NextDouble()<Mathf.Clamp(Stat("dodge")/100,0,DodgeLimit)){LastHitDodged=true;DodgeTriggers();RetaliationTriggers();return;}
            if(HitProtection>0){HitProtection--;LastHitProtected=true;return;}
            int armor=(int)Stat("armor");
            float armorFactor=armor>=0?1/(1+armor/15f):2-1/(1-armor/15f);
            int taken=Math.Max(1,EnemyReferenceRules.SourceRound(amount*armorFactor));
            Health=Stat("die_in_one_hit")>0?0:Math.Max(0,Health-taken);
            BeginRegenerationAfterDamage();
            DamageTriggers();
            if(Health==0)Phase=RunPhase.Lost;
        }
        public void Heal(float amount,bool fromTorture=false)
        {
            float before=Health;
            if(Phase==RunPhase.Playing&&amount>0&&Stat("no_heal")<=0&&(Stat("torture")<=0||fromTorture))Health=Math.Min(MaxHealth,Health+amount);
            if(Health>before){runHealing+=Health-before;Progress?.Check("chal_medicine",runHealing);RandomDamageTrigger("dmg_when_heal");}
        }
        public void DefeatBoss()
        {
            // Source main.on_boss_died starts the wave timer at 0.1 seconds;
            // expiry still runs end-wave effects, harvesting and cleanup.
            if(Phase==RunPhase.Playing&&!waveSettlementPending&&Wave==WaveCount)Remaining=.1f;
        }
        public void Kill()
        {
            if(Phase==RunPhase.Playing)Kills++;
        }
        void GainXP(int value)
        {
            float gained=value*Mathf.Max(0,1+Stat("xp_gain")/100)+xpFraction;
            XP+=(int)gained;xpFraction=gained-(int)gained;
            while(XP+xpFraction>=NextLevelExact)
            {
                float remaining=XP+xpFraction-NextLevelExact;
                XP=(int)remaining;xpFraction=remaining-XP;
                Level++;
                Add("max_hp",1);
                PermanentTrigger("stats_on_level_up");
                PendingLevels++;
            }
        }
        public void Collect(int value)
        {
            if(Phase!=RunPhase.Playing||value<=0)return;
            int boost=random.NextDouble()<Mathf.Clamp01(Stat("chance_double_gold")/100)?2:1;
            int gained=Math.Max(0,Mathf.CeilToInt(value*(1+Stat("increase_material_value")/100)*boost));
            Scrap+=gained;Progress?.Count("materials");
            GainXP(gained);
            if(random.NextDouble()<Mathf.Clamp01(Stat("heal_when_pickup_gold")/100))Heal(1);
            RandomDamageTrigger("dmg_when_pickup_gold");
            Progress?.CheckStats(this);
        }
        public void BankMaterials(int value)
        {
            banked+=Math.Max(0,value);
        }
        // main.gd:spawn_gold reserves the previous wave's bonus when a pickup is
        // created. Merging overflow into an existing pile does not consume it.
        public int PrepareMaterialDrop(int value)
        {
            value=Math.Max(0,value);
            int bonus=Math.Min(value,banked);banked-=bonus;return value+bonus;
        }
        public int Price(int value)=>ReferencePrice(Wave,value,false,null);
        public int RecycleValue(int value,bool isWeapon=false)
        {
            if(value==1)return 1;
            int shopPrice=ReferencePrice(Wave,value,isWeapon,null);
            int recycled=Math.Max(1,(int)(shopPrice*Mathf.Clamp(.25f+Stat("recycling_gains")/100,.01f,1)));
            return Math.Min(shopPrice,recycled);
        }
        int PriceAt(int value,int wave)=>Math.Max(1,(int)(value+wave+value*wave*(Difficulty>=6?.11f:.1f)));
        int ReferencePrice(int wave,int baseValue,bool isWeapon,string id)
        {
            // ItemService keeps this untyped Variant fractional until the final
            // return cast, including Arms Dealer's heavily discounted base.
            float adjusted=isWeapon?baseValue*(1+Stat("weapons_price")/100):baseValue;
            float specific=0;
            if(!string.IsNullOrEmpty(id))
            {
                var effect=Effects("specific_items_price").FirstOrDefault(e=>((isWeapon?"weapon_":"item_")+id).Contains(e.key));
                if(effect!=null)specific=effect.value;
            }
            float factor=1+(Stat("items_price")+specific)/100;
            return Math.Max(1,(int)((adjusted+wave+adjusted*wave*(Difficulty>=6?.11f:.1f))*factor));
        }
        public int Price(ShopOffer offer)
        {
            int wave=offer.rolledWave>0?offer.rolledWave:Wave;
            int cost=ReferencePrice(wave,offer.Value,offer.weapon!=null,offer.weapon!=null?offer.weapon.id+"_"+offer.weapon.tier:offer.item?.id);
            return UsesHealthShop?Mathf.CeilToInt(cost/20f):cost;
        }
        void Draft(bool reroll=false)
        {
            // ItemService.get_upgrades returns only upgrade_weapon_slot while
            // Baby's current slot count is below its weapon_slot_upgrades cap.
            if(Stat("weapon_slot_upgrades")>0&&WeaponSlots<Stat("weapon_slot_upgrades"))
            {Offers=new[]{new UpgradeOffer(UpgradeKind.WeaponSlot,1,catalog?.data.upgrades)};return;}
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
                int tier=ReferenceRarity.Upgrade(rewardLevel,Stat("luck"),random.NextDouble());
                int p=random.Next(pool.Count);
                Offers[i]=new UpgradeOffer(pool[p],tier,catalog?.data.upgrades);
                pool.RemoveAt(p);
            }
        }
        public void Choose(int choice)
        {
            if(Phase!=RunPhase.Upgrade||choice<0||choice>=Offers.Length)return;
            var o=Offers[choice];
            if(o.Kind==UpgradeKind.WeaponSlot)weaponSlotsEarned++;
            else Add(o.Stat,o.Amount);
            UpgradeCount++;
            PendingLevels--;
            Progress?.CheckStats(this);
            rerolls=0;
            if(PendingLevels>0)Draft();
            else OpenShop();
        }
        void OpenShop()
        {
            Phase=RunPhase.Shop;
            if(Stat("destroy_weapons")>0)Inventory.Clear();
            UpgradeWeaponImplants();BeginShopRerolls();EnterSpecialShop();rerolls=0;
            RollShop();
            bonusFreeShopReroll=Shop.All(o=>o==null);
            if(!GameSettings.Flag("keepLock"))foreach(var offer in Shop)if(offer!=null)offer.locked=false;
        }
        void RollShop(bool reroll=false,int tierIncrease=0)
        {
            if(catalog==null)return;
            int guaranteed=Math.Max((int)Stat("minimum_weapons_in_shop"),Wave<=2?2:Wave<=5?1:0);
            var guaranteedItems=new Queue<string>(Effects("guaranteed_shop_items").Select(e=>e.key.Replace("item_","")));
            int existing=Shop.Count(s=>s!=null&&s.locked&&s.weapon!=null);
            // The reference starts a new shop with only locked offers in the exclusion list.
            // A reroll excludes the offers it replaces as well as the new offers in this roll.
            var excluded=Shop.Where(s=>s!=null&&(reroll||s.locked)).ToList();
            var ownedItems=Items.Concat(Shop.Where(s=>s?.item!=null&&s.locked).Select(s=>s.item)).ToList();
            for(int i=0;i<4;i++)
            {
                if(Shop[i]!=null&&Shop[i].locked)continue;
                bool weapon=WeaponSlots>0&&(Wave<=2?existing<guaranteed:guaranteedItems.Count==0&&(random.NextDouble()<.35||existing<guaranteed));
                bool guaranteedItem=!weapon&&guaranteedItems.Count>0;
                // _get_rand_item_for_wave draws this value before rarity, even for items.
                double wanted=guaranteedItem?0:random.NextDouble();
                int tier=guaranteedItem?0:Mathf.Clamp(ReferenceRarity.Roll(Wave,Stat("luck"),random.NextDouble())+tierIncrease,1,4);
                if(weapon)
                {
                    tier=Mathf.Clamp(tier,MinimumWeaponTier,MaximumWeaponTier);
                    var fallback=catalog.data.equipment.Where(w=>w.tier==tier&&CanOfferWeapon(w)).ToArray();
                    if(fallback.Length==0){Shop[i]=null;continue;}
                    var pool=fallback.Where(w=>!excluded.Any(s=>s.weapon==w)).ToArray();
                    if(pool.Length==0)pool=fallback;
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
                    ItemEntry[] pool;
                    if(guaranteedItem){string id=guaranteedItems.Dequeue();pool=catalog.data.items.Where(item=>item.id==id).ToArray();}
                    else pool=ShopSelection.ItemPool(catalog.data.items,Character,tier,random.NextDouble()<WantedTagChance,
                        excluded.Where(s=>s.item!=null).Select(s=>s.item),ownedItems);
                    if(pool.Length==0){Shop[i]=null;continue;}
                    Shop[i]=new ShopOffer
                    {
                        item=pool[random.Next(pool.Length)],rolledWave=Wave
                    };
                }
                excluded.Add(Shop[i]);
            }
        }
        public bool CanLock(int index)
        {
            if(Phase!=RunPhase.Shop||index<0||index>=Shop.Length||Shop[index]==null||Stat("disable_item_locking")>0)return false;
            // Supplied hourglass_data.tres and broken_hourglass_data.tres are the
            // only item resources with is_lockable=false; catalog has no such field.
            string id=Shop[index].item?.id;
            return id!="hourglass"&&id!="broken_hourglass";
        }
        public void ToggleLock(int index)
        {
            if(CanLock(index))Shop[index].locked=!Shop[index].locked;
        }
        int FindMatch(Equipment w)=>w.tier>=MaximumWeaponTier?-1:Inventory.FindIndex(x=>x.id==w.id&&x.tier==w.tier);
        public bool CanBuy(int i)
        {
            if(Phase!=RunPhase.Shop||i<0||i>=4||Shop[i]==null)return false;
            var s=Shop[i];
            return (UsesHealthShop?MaxHealth>=Price(s):Scrap>=Price(s))&&CanAcquire(s);
        }
        public bool Buy(int i)
        {
            if(!CanBuy(i))return false;
            return PurchaseOffer(i,false);
        }
        bool PurchaseOffer(int i,bool stolen)
        {
            var s=Shop[i];
            if(s.weapon!=null)Progress?.Count("weapon:"+s.weapon.id);
            // RunData.remove_currency removes the displayed cost from base max-HP
            // stats; the character's stat-gain multiplier is applied afterward.
            if(!stolen){if(UsesHealthShop)Add("max_hp",-Price(s));else Scrap-=Price(s);}
            if(s.weapon!=null)
            {
                if(Inventory.Count>=WeaponSlots||!CanAddWeapon(s.weapon))
                {Inventory.Add(s.weapon);CombineCopies(s.weapon);}
                else Inventory.Add(s.weapon);
            }
            else
            {
                BuyItemWithDuplicates(s.item);
            }
            Shop[i]=null;
            bonusFreeShopReroll=Shop.All(o=>o==null);
            Progress?.CheckStats(this);
            return true;
        }
        public bool CanCombine(int i)=>Phase==RunPhase.Shop&&i>=0&&i<Inventory.Count&&Inventory[i].tier<MaximumWeaponTier&&Inventory.Where((w,j)=>j!=i&&w.id==Inventory[i].id&&w.tier==Inventory[i].tier).Any();
        public void Combine(int index)
        {
            if(!CanCombine(index))return;
            var w=Inventory[index];
            CombineCopies(w);
            Progress?.CheckStats(this);
        }
        void CombineCopies(Equipment weapon)
        {
            // base_shop._combine_weapon calls RunData.remove_weapon twice;
            // each removes the first equal copy. add_weapon appends the upgrade.
            for(int removed=0;removed<2;removed++)
                Inventory.RemoveAt(Inventory.FindIndex(w=>w.id==weapon.id&&w.tier==weapon.tier));
            Inventory.Add(catalog.Weapon(weapon.id,weapon.tier+1));
        }
        public void Recycle(int i)
        {
            if(Phase!=RunPhase.Shop||i<0||i>=Inventory.Count)return;
            Scrap+=RecycleValue(Inventory[i].value,true);runRecycles++;Progress?.Check("chal_recycling",runRecycles);
            Inventory.RemoveAt(i);
            Progress?.CheckStats(this);
        }
        public void Reroll()
        {
            if(!CanReroll)return;
            int cost=RerollPrice;Scrap-=cost;
            if(Phase==RunPhase.Shop&&bonusFreeShopReroll)bonusFreeShopReroll=false;
            else if(Phase==RunPhase.Shop&&freeShopRerolls>0)freeShopRerolls--;
            else rerolls++;
            if(Phase==RunPhase.Upgrade)Draft(true);
            else {int increase=ApplyShopRerollEffects();RollShop(true,increase);ShopRerollCount++;Progress?.Check("chal_unlucky",ShopRerollCount);Progress?.CheckStats(this);}
        }
        public void NextWave()
        {
            if(Phase!=RunPhase.Shop)return;
            initializingHealth=true;
            UsedLocks|=Shop.Any(o=>o?.locked==true);
            Wave=PrepareNextWave();
            ResetWaveSettlement();
            Remaining=Durations[Wave-1];
            ResetConditionalEffects();
            Health=MaxHealth;
            ResetSurvival();
            Phase=RunPhase.Playing;ApplyNextWaveEffects();BeginItemWave();
            FinishHealthInitialization();
            for(int i=0;i<4;i++)if(Shop[i]!=null&&!Shop[i].locked)Shop[i]=null;
        }
        public void Abandon() { if(Phase==RunPhase.Paused){Abandoned=true;Health=0;Phase=RunPhase.Lost;} }
        RunPhase pausedFrom=RunPhase.Playing;
        public void Pause()
        {
            if(Phase==RunPhase.Paused)Phase=pausedFrom;
            else if(Phase==RunPhase.Playing||Phase==RunPhase.Shop||Phase==RunPhase.Upgrade||Phase==RunPhase.Loot)
            {pausedFrom=Phase;Phase=RunPhase.Paused;}
        }
        public static Vector2 ClampPosition(Vector2 p)=>ArenaGeometry.Clamp(p);
        public static bool Touches(Vector2 a,Vector2 b,float r)=>(a-b).sqrMagnitude<=r*r;
    }
}

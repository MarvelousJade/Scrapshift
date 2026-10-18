using System;
using System.Collections.Generic;
using System.Linq;

namespace Scrapshift
{
    public sealed partial class RunRules
    {
        // Source RunData snapshots persistent run/player data before actors are
        // created. Catalog resources, RNG and global progression stay live.
        sealed class RetryCheckpoint
        {
            public Dictionary<string,float> stats,temporary,next;
            public Equipment[] weapons;public ItemEntry[] items;public ShopOffer[] offers;
            public string[] shopGroups;
            public int wave,difficulty,materials,xp,level,kills,upgrades,banked,slots,retries,queuedBaits,extraBaits,eliteSpawns,
                steals,shopRerolls,freeRerolls,knownFreeRerolls,extraLoot,protection,consumables,recycles,projectileHits,burningKills;
            public float health,observedHealth,healthCap,speedCap,xpFraction,healing;
            public bool usedLocks,extraElite,freeReroll;
            public CharacterProfile character;
        }
        RetryCheckpoint retryCheckpoint;
        public int Retries {get;private set;}
        public bool CanRetryWave=>retryCheckpoint!=null&&!Abandoned&&Phase==RunPhase.Lost&&!DeathCleanupPending;
        static ShopOffer CopyRetryOffer(ShopOffer offer)=>offer==null?null:new ShopOffer{rolledWave=offer.rolledWave,weapon=offer.weapon,item=offer.item,locked=offer.locked};
        public void CaptureWaveRetryState()
        {
            if(Phase!=RunPhase.Playing||WaveSettlementPending)return;
            retryCheckpoint=new RetryCheckpoint{
                stats=new Dictionary<string,float>(stats),temporary=new Dictionary<string,float>(temporaryStats),next=new Dictionary<string,float>(nextWaveStats),
                weapons=Inventory.ToArray(),items=Items.ToArray(),offers=Array.ConvertAll(Shop,CopyRetryOffer),shopGroups=shopSpawnGroups.ToArray(),
                wave=Wave,difficulty=Difficulty,materials=Scrap,xp=XP,level=Level,kills=Kills,upgrades=UpgradeCount,banked=banked,slots=weaponSlotsEarned,retries=Retries,
                queuedBaits=queuedBaits,extraBaits=ExtraBaitGroups,eliteSpawns=shopEliteSpawns,steals=StealsRemaining,shopRerolls=ShopRerollCount,
                freeRerolls=freeShopRerolls,knownFreeRerolls=knownFreeRerolls,extraLoot=ExtraLootAliens,protection=HitProtection,
                consumables=runConsumables,recycles=runRecycles,projectileHits=projectileHits,burningKills=burningKills,
                health=currentHealth,observedHealth=observedMaxHealth,healthCap=healthCap,speedCap=speedCap,xpFraction=xpFraction,healing=runHealing,
                usedLocks=UsedLocks,extraElite=ExtraElite,freeReroll=bonusFreeShopReroll,character=Character};
        }
        static void RestoreRetryDictionary(Dictionary<string,float> target,Dictionary<string,float> source)
        {target.Clear();foreach(var entry in source)target.Add(entry.Key,entry.Value);}
        public bool RetryWave()
        {
            if(!CanRetryWave)return false;
            var saved=retryCheckpoint;
            initializingHealth=true;
            RestoreRetryDictionary(stats,saved.stats);RestoreRetryDictionary(temporaryStats,saved.temporary);RestoreRetryDictionary(nextWaveStats,saved.next);
            Inventory.Clear();Inventory.AddRange(saved.weapons);Items.Clear();Items.AddRange(saved.items);
            Shop=Array.ConvertAll(saved.offers,CopyRetryOffer);Offers=Array.Empty<UpgradeOffer>();
            shopSpawnGroups.Clear();shopSpawnGroups.AddRange(saved.shopGroups);shopEliteSpawns=saved.eliteSpawns;
            Wave=saved.wave;Difficulty=saved.difficulty;Scrap=saved.materials;XP=saved.xp;Level=saved.level;Kills=saved.kills;UpgradeCount=saved.upgrades;
            banked=saved.banked;weaponSlotsEarned=saved.slots;xpFraction=saved.xpFraction;Character=saved.character;
            queuedBaits=saved.queuedBaits;ExtraBaitGroups=saved.extraBaits;ExtraLootAliens=saved.extraLoot;ExtraElite=saved.extraElite;
            StealsRemaining=saved.steals;ShopRerollCount=saved.shopRerolls;freeShopRerolls=saved.freeRerolls;knownFreeRerolls=saved.knownFreeRerolls;bonusFreeShopReroll=saved.freeReroll;
            UsedLocks=saved.usedLocks;runConsumables=saved.consumables;runRecycles=saved.recycles;projectileHits=saved.projectileHits;burningKills=saved.burningKills;runHealing=saved.healing;
            Health=saved.health;observedMaxHealth=saved.observedHealth;healthCap=saved.healthCap;speedCap=saved.speedCap;HitProtection=saved.protection;
            Remaining=Durations[Wave-1];PendingLevels=0;crates.Clear();CrateItem=null;consumableGains.Clear();burnKillCounts.Clear();resetOnHitStats.Clear();
            LastHitProtected=LastHitDodged=false;DodgeRetaliation=0;HealingDrones=0;
            effectElapsed=0;effectSeconds=noHitSeconds=0;moving=false;convertedHalfWave=false;stationaryIncome=0;
            ResetSurvival();ResetWaveSettlement();ResetDeathCleanup();
            Abandoned=false;Retries=saved.retries+1;pausedFrom=RunPhase.Playing;Phase=RunPhase.Playing;
            initializingHealth=false;reconcilingHealth=false;
            return true;
        }
        // New runs must not inherit a previous run's retry counter/checkpoint.
        void ResetWaveRetry(){Retries=0;retryCheckpoint=null;}
    }
}

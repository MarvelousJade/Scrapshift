using System;
using System.Linq;
using UnityEngine;

namespace Scrapshift
{
    // Source-derived finite action scenarios shared by EditMode and opt-in
    // headless player checks. These inspect model receipts, never UI/input.
    public static class HeadlessShopAcceptance
    {
        public static readonly string[] Cases={"character-prices","demon-budget","capacity","bunny-paid-order","sold-out-order","locked-rejection","lock-transition","coupon-removal","recycling-removal","anvil-targets","hourglass-once","candy-seeds"};
        static void Check(bool value,string message){if(!value)throw new InvalidOperationException(message);}
        static ItemEntry Item(ReferenceCatalog c,string id)=>c.data.items.Single(i=>i.id==id);
        static RunRules Run(ReferenceCatalog c,int seed=877,string character=null)
        {
            var r=new RunRules(seed);r.Configure(c);
            var profile=character==null?null:c.data.characters.Single(p=>p.id==character);
            r.Start(profile,profile==null?null:StarterChoices.Available(c,profile).FirstOrDefault(),10000);
            // Entrepreneur discards the starting purse at wave start. Fund the
            // staged shop only afterward, without pickup XP or extra level stats.
            if(r.Scrap<10000)r.GainWeaponCriticalMaterials(10000-r.Scrap);
            return r;
        }
        static void Shop(RunRules r){r.Tick(r.Remaining);while(r.Phase==RunPhase.Upgrade)r.Choose(0);Check(r.Phase==RunPhase.Shop,"Expected shop action boundary.");}
        static ShopOffer Offer(ReferenceCatalog c,string id,int wave)=>new ShopOffer{item=Item(c,id),rolledWave=wave};
        public static string Execute(ReferenceCatalog c,string name)
        {
            var r=Run(c);
            switch(name)
            {
                case "character-prices":
                    // Source ItemService.get_value: coffee base20, pistol base10,
                    // wave5 yields35 and20 before character modifiers; floor last.
                    string[] ids={"mutant","saver","entrepreneur","glutton","baby"};
                    int[] coffee={52,52,26,43,28},pistol={30,30,15,25,16};
                    string receipt="";
                    for(int i=0;i<ids.Length;i++)
                    {
                        r=Run(c,877,ids[i]);r.SetShowcaseWave(5);
                        // Showcase setup now runs the ordinary wave-start effects,
                        // including Entrepreneur's material reset. Fund only after it.
                        if(r.Scrap<10000)r.GainWeaponCriticalMaterials(10000-r.Scrap);
                        Shop(r);
                        var item=Offer(c,"coffee",5);var weapon=new ShopOffer{weapon=c.Weapon("pistol",1),rolledWave=5};
                        Check(r.Price(item)==coffee[i]&&r.Price(weapon)==pistol[i],ids[i]+" source integer price");
                        r.Shop[0]=item;int purchaseBalance=r.Scrap;Check(r.Buy(0)&&r.Scrap==purchaseBalance-coffee[i],ids[i]+" exact item debit");
                        r.Inventory.Clear();r.Shop[0]=weapon;purchaseBalance=r.Scrap;Check(r.Buy(0)&&r.Scrap==purchaseBalance-pistol[i]&&r.Inventory.Count==1,ids[i]+" exact weapon debit");
                        receipt+=ids[i]+":"+coffee[i]+"/"+pistol[i]+";";
                    }
                    r=Run(c,877,"arms_dealer");Shop(r);r.Shop[0]=new ShopOffer{weapon=c.Weapon("knife",4),rolledWave=2};int armsPurse=r.Scrap;
                    Check(r.Price(r.Shop[0])==7&&r.Buy(0)&&r.Scrap==armsPurse-7,"Arms Dealer fractional weapon base and debit");
                    r=Run(c,877,"fisherman");Shop(r);Check(r.Price(Offer(c,"bait",5))==1&&r.Price(Offer(c,"coffee",5))==35,"Fisherman specific bait factor cannot discount coffee");
                    r.Shop[0]=Offer(c,"bait",5);int fisherPurse=r.Scrap;Check(r.Buy(0)&&r.Scrap==fisherPurse-1,"Fisherman Bait exact debit");
                    return receipt+"ArmsDealerKnifeIV:7;FishermanBait:1";
                case "demon-budget":
                    r=Run(c,877,"demon");Shop(r);r.GainWeaponStat("max_hp",1-r.Stat("max_hp"));
                    var one=new ShopOffer{item=new ItemEntry{id="headless-cost-one",value=1,effects=Array.Empty<StatEffect>(),conditionalEffects=Array.Empty<ConditionalEffect>()},rolledWave=1};
                    r.Shop[0]=one;int materials=r.Scrap;Check(r.ShopBalance==1&&r.Buy(0)&&r.ShopBalance==0,"Spend last raw HP exactly once");
                    one.item.effects=new[]{new StatEffect{key="stat_max_hp",value=10}};r.Shop[0]=one;
                    Check(!r.Buy(0)&&r.Stat("max_hp")==0&&r.Scrap==materials&&r.MaxHealth>=1,"No HP-granting purchase on zero raw HP");
                    int rerollCost=r.RerollPrice;Check(rerollCost>0&&r.CanReroll,"Demon rawHP0 may pay material reroll");r.Reroll();
                    Check(r.Scrap==materials-rerollCost&&r.Stat("max_hp")==0,"Reroll debits material only at rawHP0");
                    return "rawHP 1 -> 0; next HP-grant rejected; paid reroll debits material only";
                case "capacity":
                    Shop(r);r.Inventory.Clear();for(int i=0;i<r.WeaponSlots;i++)r.Inventory.Add(c.Weapon("pistol",1));
                    r.Shop[0]=new ShopOffer{weapon=c.Weapon("pistol",1),rolledWave=1};int cost=r.Price(r.Shop[0]),balance=r.Scrap;
                    Check(r.Buy(0)&&r.Scrap==balance-cost&&r.Inventory.Count==r.WeaponSlots&&r.Inventory.Count(w=>w.tier==2)==1,"Full capacity duplicate buys once and combines");
                    r.Shop[0]=new ShopOffer{weapon=c.Weapon("knife",1),rolledWave=1};balance=r.Scrap;var rejected=r.Shop[0];
                    Check(!r.Buy(0)&&r.Scrap==balance&&ReferenceEquals(rejected,r.Shop[0]),"Unmatched full inventory purchase preserves offer and balance");
                    r.Inventory.Clear();for(int i=0;i<r.WeaponSlots;i++)r.Inventory.Add(c.Weapon("pistol",4));
                    r.Shop[0]=new ShopOffer{weapon=c.Weapon("pistol",4),rolledWave=1};Check(!r.Buy(0)&&!r.CanCombine(0),"No unsupported tier5 from maxed copies");
                    r=Run(c,877,"ranger");Shop(r);r.Inventory.Clear();r.Shop[0]=new ShopOffer{weapon=c.Weapon("knife",1),rolledWave=1};balance=r.Scrap;
                    Check(!r.CanBuy(0)&&!r.Buy(0)&&r.Scrap==balance&&r.Inventory.Count==0,"Unusable melee offer cannot bypass Ranger eligibility");
                    return "capacity duplicate -> one tierII; unmatched/max tier/unusable rejected without debit";
                case "bunny-paid-order":
                    Shop(r);int paid=r.RerollPrice;r.Reroll();int secondPaid=r.RerollPrice;
                    var bunny=Item(c,"dangerous_bunny");r.Shop[0]=new ShopOffer{item=bunny,rolledWave=1};Check(r.Buy(0),"Buy first Bunny");
                    int money=r.Scrap;Check(r.RerollPrice==0,"Bunny usable immediately");r.Reroll();Check(r.Scrap==money&&r.RerollPrice==secondPaid,"Free roll preserves paid index");
                    r.Shop[0]=new ShopOffer{item=bunny,rolledWave=1};Check(r.Buy(0),"Buy additional Bunny");money=r.Scrap;r.Reroll();
                    Check(r.Scrap==money&&r.RerollPrice==secondPaid,"Additional Bunny usable exactly once");r.Reroll();Check(r.Scrap==money-secondPaid&&r.RerollPrice>secondPaid&&paid>0,"Paid progression resumes");
                    return "two separately purchased Bunnies each grant one immediate roll; paid count retained";
                case "sold-out-order":
                    r.GrantItem(Item(c,"dangerous_bunny"));Shop(r);
                    for(int i=0;i<4;i++){r.Shop[i]=Offer(c,"coffee",1);Check(r.Buy(i),"Buy complete offer set");}
                    int initial=r.Scrap;Check(r.RerollPrice==0,"Sold-out bonus free");r.Reroll();
                    Check(r.Scrap==initial&&r.RerollPrice==0,"Sold-out bonus before Bunny");r.Reroll();
                    Check(r.Scrap==initial&&r.RerollPrice==1,"Bunny consumed second; paid index still zero");r.Reroll();Check(r.Scrap==initial-1&&r.RerollPrice==2,"First paid after both freebies");
                    return "sold-out bonus -> Bunny -> cost1 -> next cost2";
                case "locked-rejection":
                    r.GrantItem(Item(c,"dangerous_bunny"));Shop(r);for(int i=0;i<4;i++)r.ToggleLock(i);
                    var same=r.Shop.ToArray();int gold=r.Scrap;Check(!r.CanReroll,"Four locked blocks free reroll");r.Reroll();
                    Check(r.Scrap==gold&&r.ShopRerollCount==0&&r.Shop.SequenceEqual(same),"Rejected free action has no effects");r.ToggleLock(0);r.Reroll();Check(r.Scrap==gold&&r.ShopRerollCount==1,"Rejected attempt did not consume Bunny");
                    r=new RunRules(877);r.Configure(c);r.Start();Shop(r);same=r.Shop.ToArray();gold=r.Scrap;
                    Check(gold==0&&!r.CanReroll,"No materials rejects paid roll");r.Reroll();Check(r.Scrap==0&&r.ShopRerollCount==0&&r.Shop.SequenceEqual(same),"Unaffordable action preserves complete receipt");
                    return "4 locks reject without mutation/token use; unlocking permits free roll; unaffordable paid roll preserves offers/count/balance";
                case "lock-transition":
                    Shop(r);r.ToggleLock(0);r.ToggleLock(0);r.NextWave();Check(!r.UsedLocks,"Temporary lock does not mark run");Shop(r);
                    var locked=r.Shop[0];int retained=r.Price(locked);r.ToggleLock(0);int wave=r.Wave;r.NextWave();Check(r.UsedLocks,"Carried lock marks run");Shop(r);
                    Check(r.Wave==wave+1&&ReferenceEquals(locked,r.Shop[0])&&r.Price(locked)==retained,"Same identity and rolled price across actual wave/shop transition");
                    Check(locked.locked==GameSettings.Flag("keepLock"),"New shop lock preference applies after retention");
                    return "temporary lock unused; carried offer same identity, original price, UsedLocks true";
                case "coupon-removal":
                    Shop(r);var target=Offer(c,"coffee",5);int original=r.Price(target);var coupon=Item(c,"coupon");r.Shop[0]=new ShopOffer{item=coupon,rolledWave=r.Wave};Check(r.Buy(0),"Acquire actual Coupon through purchase");
                    Check(r.Price(target)==33&&original==35,"Coupon reduces next operation by 5%, final truncation");r.RemoveItem(coupon);Check(r.Price(target)==original,"Coupon removal restores next operation");
                    return "wave5 coffee 35 -> 33 -> 35";
                case "recycling-removal":
                    r.SetShowcaseWave(5);Shop(r);var machine=Item(c,"recycling_machine");int before=r.RecycleValue(c.Weapon("pistol",1).value,true);r.Shop[0]=new ShopOffer{item=machine,rolledWave=r.Wave};Check(r.Buy(0),"Acquire actual Recycling Machine through purchase");
                    int boosted=r.RecycleValue(c.Weapon("pistol",1).value,true);Check(before==5&&boosted==12,"Recycling Machine 25% -> 60% source truncation");
                    r.Inventory.Clear();r.Inventory.Add(c.Weapon("pistol",1));int purse=r.Scrap;r.Recycle(0);Check(r.Scrap==purse+boosted&&r.Inventory.Count==0,"Actual recycle credits once");
                    r.RemoveItem(machine);Check(r.RecycleValue(c.Weapon("pistol",1).value,true)==before,"Removal restores next recycle");return "wave5 pistol recycle 5 -> 12 -> 5; one actual credit";
                case "anvil-targets":
                    int[] chosen=new int[3];
                    for(int seed=0;seed<96;seed++)
                    {
                        r=Run(c,seed);r.Inventory.Clear();r.Inventory.Add(c.Weapon("pistol",1));r.Inventory.Add(c.Weapon("knife",2));r.Inventory.Add(c.Weapon("wrench",3));r.Inventory.Add(c.Weapon("smg",4));
                        r.GrantItem(Item(c,"anvil"));Shop(r);Check(r.Inventory.Count==4&&r.Inventory.Contains(c.Weapon("smg",4)),"Anvil excludes max-tier and preserves count");
                        string[] targetIds={"pistol","knife","wrench"};int[] tiers={2,3,4};int n=Array.FindIndex(targetIds,id=>r.Inventory.Any(w=>w.id==id&&w.tier==tiers[Array.IndexOf(targetIds,id)]));Check(n>=0,"Upgrade one source-eligible target");chosen[n]++;
                        r.RemoveItem(Item(c,"anvil"));var snapshot=r.Inventory.ToArray();r.NextWave();Shop(r);Check(r.Inventory.SequenceEqual(snapshot),"Removed Anvil does not act next shop");
                    }
                    Check(chosen.All(n=>n>0),"Bounded seeds exercise every eligible target, no probability claim");
                    r=Run(c);r.Inventory.Clear();r.Inventory.Add(c.Weapon("pistol",4));r.GrantItem(Item(c,"anvil"));Shop(r);Check(r.Stat("armor")==2,"No eligible target yields +2 Armor");
                    r=Run(c);r.Inventory.Clear();r.GrantItem(Item(c,"anvil"));Shop(r);Check(r.Stat("armor")==2,"Empty weapon pool also yields +2 Armor");
                    r=Run(c);r.Inventory.Clear();r.Inventory.Add(c.Weapon("pistol",1));Shop(r);r.Shop[0]=Offer(c,"anvil",r.Wave);Check(r.Buy(0)&&r.Inventory.Single().tier==1,"Purchasing Anvil does not immediately act in current shop");
                    r.NextWave();Shop(r);Check(r.Inventory.Single().tier==2,"Purchased Anvil acts exactly once on next shop entry");
                    return "96 seeds eligible targets counts="+string.Join(",",chosen)+"; removal inactive; max-only/empty fallback +2 armor; purchase first acts next shop";
                case "hourglass-once":
                    r.SetShowcaseWave(7);Shop(r);r.Shop[0]=Offer(c,"hourglass",7);Check(!r.CanLock(0),"Hourglass not lockable");Check(r.Buy(0),"Buy actual Hourglass");r.NextWave();
                    Check(r.Wave==7&&r.Items.Count(i=>i.id=="hourglass")==0&&r.Items.Count(i=>i.id=="broken_hourglass")==1,"One repeated wave and one broken replacement");
                    Shop(r);r.Shop[0]=Offer(c,"broken_hourglass",7);Check(!r.CanLock(0),"Broken Hourglass also not lockable");r.NextWave();Check(r.Wave==8&&r.Items.Count(i=>i.id=="broken_hourglass")==1,"Consumed Hourglass never repeats again");return "7 -> 7 -> 8; one broken replacement";
                case "candy-seeds":
                    int successes=0;for(int seed=0;seed<256;seed++)
                    {
                        r=Run(c,seed);r.GrantItem(Item(c,"candy_bag"));Check(r.Stat("extra_elite_next_wave_chance")==10,"Actual imported Candy chance10");Shop(r);r.NextWave();bool result=r.ExtraElite;successes+=result?1:0;
                        var replay=Run(c,seed);replay.GrantItem(Item(c,"candy_bag"));Shop(replay);replay.NextWave();Check(replay.ExtraElite==result,"Bounded natural outcome replay");
                        r.RemoveItem(Item(c,"candy_bag"));Shop(r);r.NextWave();Check(!r.ExtraElite,"Removed Candy has no subsequent elite branch");
                    }
                    Check(successes>0&&successes<256,"Natural chance exercises success and failure, not a frequency/equal RNG assertion");return "256 seeds natural10% outcomes: "+successes+" successes, "+(256-successes)+" failures; deterministic replay; removal clears future chance";
                default:throw new ArgumentException(name);
            }
        }
    }
}

using System;
using System.Linq;
namespace Scrapshift
{
    [Serializable] public sealed class CharacterEffectRow { public string kind,key,extra;public float value; }
    [Serializable] public sealed class CharacterDesign { public string id;public string[] wantedTags,bannedItems;public CharacterEffectRow[] rows; }
    [Serializable] public sealed class ItemDesignData { public ItemEntry[] items;public CharacterDesign[] characters; }
    public static class ShopSelection
    {
        public static bool SharesSet(Equipment a,Equipment b)=>a.setIds!=null&&b.setIds!=null&&a.setIds.Intersect(b.setIds).Any();
        public static ItemEntry[] ItemPool(ItemEntry[] items,CharacterProfile character,int tier,bool preferTags,
            System.Collections.Generic.IEnumerable<ItemEntry> excluded,System.Collections.Generic.IEnumerable<ItemEntry> owned)
        {
            var pool=items.Where(i=>i.tier==tier&&RunRules.IsShopItem(i.id)).ToArray();
            while(pool.Length==0&&tier>1){tier--;pool=items.Where(i=>i.tier==tier&&RunRules.IsShopItem(i.id)).ToArray();}
            // Empty tiers are a deliberate small-catalog fallback. Hard restrictions survive fallback.
            pool=pool.Where(i=>!(character?.bannedItems??Array.Empty<string>()).Any(id=>id==i.id||id=="item_"+i.id)&&
                !(character?.bannedItemGroups??Array.Empty<string>()).Intersect(i.groups??Array.Empty<string>()).Any()&&
                (i.maxCount<=0||owned.Count(a=>a.id==i.id)<i.maxCount)).ToArray();
            var fresh=pool.Where(i=>!excluded.Any(e=>e.id==i.id)).ToArray();
            if(fresh.Length>0)pool=fresh;
            if(preferTags&&character?.wantedTags!=null)
            {var preferred=pool.Where(i=>(i.tags??Array.Empty<string>()).Intersect(character.wantedTags).Any()).ToArray();if(preferred.Length>0)pool=preferred;}
            return pool;
        }
    }
}

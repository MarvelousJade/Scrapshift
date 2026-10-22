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
            var tierPool=items.Where(i=>i.tier==tier&&RunRules.IsShopItem(i.id)).ToArray();
            // item_service.gd keeps a backup before wanted-tag and banned-group filters.
            // Its final fallback is the original pool at the same rarity tier.
            var backup=tierPool.Where(i=>!excluded.Any(e=>e.id==i.id)&&
                (i.maxCount<0||owned.Count(a=>a.id==i.id)<i.maxCount)).ToArray();
            // Character bans are preference filters in the source, unlike the
            // run's explicit banned-item list. They do not change its backup.
            var pool=backup.Where(i=>!(character?.bannedItems??Array.Empty<string>()).Any(id=>id==i.id||id=="item_"+i.id)&&
                !(character?.bannedItemGroups??Array.Empty<string>()).Intersect(i.groups??Array.Empty<string>()).Any()).ToArray();
            if(preferTags&&character?.wantedTags?.Length>0)
                pool=pool.Where(i=>(i.tags??Array.Empty<string>()).Intersect(character.wantedTags).Any()).ToArray();
            return pool.Length>0?pool:backup.Length>0?backup:tierPool;
        }
    }
}

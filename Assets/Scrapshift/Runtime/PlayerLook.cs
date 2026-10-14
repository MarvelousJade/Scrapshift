using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scrapshift
{
    [Serializable] public sealed class AppearanceLayer
    {
        public string id, slot;
        public Sprite image;
        public Vector2 offset;
        public float scale = 1;
        public int priority, order = 11;
        public bool sourceDefined, characterAppearance;
        public float depth;
        public Vector2 stretch = Vector2.one;
        public Vector2 Stretch => stretch.x == 0 || stretch.y == 0 ? Vector2.one : stretch;
    }

    [Serializable] public sealed class PlayerLook
    {
        public string name, id;
        public AppearanceLayer[] layers;
    }

    public static class AppearanceResolver
    {
        // Character defaults first, then unique items in acquisition order.
        // Higher priority wins; later equal-priority layers replace earlier ones.
        public static IEnumerable<AppearanceLayer> ItemLayers(ItemEntry item)
            => item.appearances != null && item.appearances.Length > 0 ? item.appearances : item.appearance != null ? new[]{item.appearance} : Array.Empty<AppearanceLayer>();
        public static List<AppearanceLayer> Resolve(PlayerLook look, IReadOnlyList<ItemEntry> items, bool hideItems=false)
        {
            var result = new List<AppearanceLayer>();
            if (look?.layers != null) foreach (var layer in look.layers) if(!hideItems||!layer.sourceDefined||layer.characterAppearance)Add(result, layer);
            var seen = new HashSet<string>();
            foreach (var item in items)
                if (item != null && seen.Add(item.id)) foreach(var layer in ItemLayers(item))if(!hideItems||layer.characterAppearance)Add(result, layer);
            return result.OrderBy(a=>a.order).ToList();
        }

        public static void Add(List<AppearanceLayer> result, AppearanceLayer layer)
        {
            if (layer == null || layer.image == null && !layer.sourceDefined) return;
            if (!string.IsNullOrEmpty(layer.slot))
            {
                int index = result.FindIndex(a => a.slot == layer.slot);
                if (index >= 0)
                {
                    if (result[index].priority > layer.priority) return;
                    result.RemoveAt(index);
                }
            }
            result.Add(layer);
        }
    }

    // The source removes an acquired layer without reviving a layer it previously replaced.
    public sealed class AppearanceState
    {
        readonly List<AppearanceLayer> active=new List<AppearanceLayer>();
        readonly Dictionary<string,int> counts=new Dictionary<string,int>();
        readonly Dictionary<string,ItemEntry> owned=new Dictionary<string,ItemEntry>();
        PlayerLook look;bool initialized;
        public void Reset(){initialized=false;active.Clear();counts.Clear();owned.Clear();}
        public List<AppearanceLayer> Sync(PlayerLook next,IReadOnlyList<ItemEntry> items,bool hideItems=false)
        {
            if(!initialized||look!=next)
            {
                Reset();look=next;initialized=true;
                if(next?.layers!=null)foreach(var layer in next.layers)if(!hideItems||!layer.sourceDefined||layer.characterAppearance)AppearanceResolver.Add(active,layer);
            }
            var current=items.Where(i=>i!=null).GroupBy(i=>i.id).ToDictionary(g=>g.Key,g=>g.Count());
            foreach(var id in counts.Keys)if(!current.TryGetValue(id,out int count)||count<counts[id])
                foreach(var layer in AppearanceResolver.ItemLayers(owned[id]))active.Remove(layer);
            var seen=new HashSet<string>();
            foreach(var item in items)if(item!=null&&seen.Add(item.id))
            {
                counts.TryGetValue(item.id,out int before);
                if(current[item.id]>before&&before==0)foreach(var layer in AppearanceResolver.ItemLayers(item))if(!hideItems||layer.characterAppearance)AppearanceResolver.Add(active,layer);
                owned[item.id]=item;
            }
            counts.Clear();foreach(var pair in current)counts.Add(pair.Key,pair.Value);
            // The source setting suppresses new additions; it does not erase already acquired appearances.
            return active.OrderBy(a=>a.order).ToList();
        }
    }
}

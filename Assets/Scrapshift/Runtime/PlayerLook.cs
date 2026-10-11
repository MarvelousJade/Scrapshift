using System;
using System.Collections.Generic;
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
    }

    [Serializable] public sealed class PlayerLook
    {
        public string name;
        public AppearanceLayer[] layers;
    }

    public static class AppearanceResolver
    {
        // Character defaults first, then unique items in acquisition order.
        // Higher priority wins; later equal-priority layers replace earlier ones.
        public static List<AppearanceLayer> Resolve(PlayerLook look, IReadOnlyList<ItemEntry> items)
        {
            var result = new List<AppearanceLayer>();
            if (look?.layers != null) foreach (var layer in look.layers) Add(result, layer);
            var seen = new HashSet<string>();
            foreach (var item in items)
                if (item != null && seen.Add(item.id)) Add(result, item.appearance);
            result.Sort((a, b) => { int order = a.order.CompareTo(b.order); return order != 0 ? order : string.CompareOrdinal(a.id, b.id); });
            return result;
        }

        static void Add(List<AppearanceLayer> result, AppearanceLayer layer)
        {
            if (layer == null || layer.image == null) return;
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
}

using System.Collections.Generic;
using UnityEngine;

namespace Scrapshift
{
    public sealed class CharacterWardrobe : MonoBehaviour
    {
        readonly List<SpriteRenderer> views = new List<SpriteRenderer>();
        readonly List<ItemEntry> cachedItems = new List<ItemEntry>();
        List<AppearanceLayer> selected = new List<AppearanceLayer>();
        PlayerLook cachedLook;
        Material cachedMaterial;
        bool initialized;
        public int VisibleCount { get; private set; }
        public IReadOnlyList<AppearanceLayer> Layers => selected;

        public static List<ItemEntry> VisibleItems(IReadOnlyList<ItemEntry> items)
        {
            var layers = AppearanceResolver.Resolve(null, items);
            var result = new List<ItemEntry>();
            var seen = new HashSet<string>();
            foreach (var item in items)
                if (item != null && seen.Add(item.id) && layers.Contains(item.appearance)) result.Add(item);
            return result;
        }

        public void Sync(IReadOnlyList<ItemEntry> items, Material material, bool facingLeft)
        {
            var animation = GetComponent<SpriteFrameAnimator>();
            var look = animation != null ? animation.Look : null;
            bool changed = !initialized || cachedLook != look || cachedMaterial != material || cachedItems.Count != items.Count;
            if (!changed) for (int i = 0; i < items.Count; i++) if (cachedItems[i] != items[i]) { changed = true; break; }
            if (changed)
            {
                selected = AppearanceResolver.Resolve(look, items);
                // Reuse renderer objects so switching presets does not leave a frame of old features.
                while (views.Count < selected.Count)
                {
                    var obj = new GameObject("Appearance layer");
                    obj.transform.SetParent(animation != null ? animation.BodyRoot : transform, false);
                    views.Add(obj.AddComponent<SpriteRenderer>());
                }
                VisibleCount = 0;
                for (int i = 0; i < views.Count; i++)
                {
                    var view = views[i]; view.enabled = i < selected.Count;
                    if (!view.enabled) continue;
                    var layer = selected[i];
                    view.name = "Appearance " + layer.id;
                    view.sprite = layer.image; view.sharedMaterial = material; view.sortingOrder = layer.order;
                    view.transform.localScale = Vector3.one * layer.scale;
                    foreach (var item in items) if (item.appearance == layer) { VisibleCount++; break; }
                }
                cachedItems.Clear(); foreach (var item in items) cachedItems.Add(item);
                cachedLook = look; cachedMaterial = material; initialized = true;
            }
            for (int i = 0; i < selected.Count; i++)
            {
                var offset = selected[i].offset;
                views[i].transform.localPosition = new Vector3(offset.x * (facingLeft ? -1 : 1), offset.y, -.02f);
                views[i].flipX = facingLeft;
            }
        }
    }
}

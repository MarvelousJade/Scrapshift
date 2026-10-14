using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scrapshift
{
    public sealed class CharacterWardrobe : MonoBehaviour
    {
        static readonly Dictionary<Sprite,Sprite> worldArt=new Dictionary<Sprite,Sprite>();
        static Sprite WorldAppearance(Sprite image){if(!Application.isPlaying)return image;if(!worldArt.TryGetValue(image,out var result)){result=AtlasIconMask.CreateWorldSprite(image);worldArt.Add(image,result);}return result;}
        readonly List<SpriteRenderer> views = new List<SpriteRenderer>();
        readonly List<ItemEntry> cachedItems = new List<ItemEntry>();
        List<AppearanceLayer> selected = new List<AppearanceLayer>();
        PlayerLook cachedLook;
        Material cachedMaterial;
        bool initialized;
        bool cachedHide;
        readonly AppearanceState state = new AppearanceState();
        public void Reset(){initialized=false;state.Reset();}
        public int VisibleCount { get; private set; }
        public IReadOnlyList<AppearanceLayer> Layers => selected;

        public static List<ItemEntry> VisibleItems(IReadOnlyList<ItemEntry> items)
        {
            var layers = AppearanceResolver.Resolve(null, items);
            var result = new List<ItemEntry>();
            var seen = new HashSet<string>();
            foreach (var item in items)
                if (item != null && seen.Add(item.id) && AppearanceResolver.ItemLayers(item).Any(layers.Contains)) result.Add(item);
            return result;
        }

        public void Sync(IReadOnlyList<ItemEntry> items, Material material, bool facingLeft, bool hideItems=false)
        {
            var animation = GetComponent<SpriteFrameAnimator>();
            var look = animation != null ? animation.Look : null;
            bool changed = !initialized || cachedLook != look || cachedMaterial != material || cachedItems.Count != items.Count || cachedHide!=hideItems;
            if (!changed) for (int i = 0; i < items.Count; i++) if (cachedItems[i] != items[i]) { changed = true; break; }
            if (changed)
            {
                selected = state.Sync(look, items, hideItems);
                if(animation?.bodyView!=null)animation.bodyView.enabled=!selected.Any(l=>l.slot=="skin"&&l.image!=null);
                if(animation?.bodyView!=null)
                {
                    var group=animation.BodyRoot.GetComponent<UnityEngine.Rendering.SortingGroup>();
                    if(group==null)group=animation.BodyRoot.gameObject.AddComponent<UnityEngine.Rendering.SortingGroup>();
                    group.sortingOrder=10;
                }
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
                    var view = views[i]; view.enabled = i < selected.Count && selected[i].image!=null;
                    if (!view.enabled) continue;
                    var layer = selected[i];
                    view.name = "Appearance " + layer.id;
                    view.sprite = WorldAppearance(layer.image); view.sharedMaterial = material; view.sortingOrder = layer.order;
                    view.transform.localScale = new Vector3(layer.Stretch.x,layer.Stretch.y,1) * layer.scale;
                    foreach (var item in items) if (AppearanceResolver.ItemLayers(item).Contains(layer)) { VisibleCount++; break; }
                }
                cachedItems.Clear(); foreach (var item in items) cachedItems.Add(item);
                cachedLook = look; cachedMaterial = material; cachedHide=hideItems; initialized = true;
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



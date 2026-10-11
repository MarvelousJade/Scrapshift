using System.Collections.Generic;
using UnityEngine;

namespace Scrapshift
{
    public sealed class CharacterWardrobe : MonoBehaviour
    {
        readonly List<SpriteRenderer> views = new List<SpriteRenderer>();
        readonly List<ItemEntry> selected = new List<ItemEntry>();
        int lastCount = -1;
        public int VisibleCount => selected.Count;

        public static List<ItemEntry> VisibleItems(IReadOnlyList<ItemEntry> items)
        {
            var result = new List<ItemEntry>();
            var slots = new HashSet<string>();
            for (int i = items.Count - 1; i >= 0; i--)
                if (!string.IsNullOrEmpty(items[i].wearSlot) && slots.Add(items[i].wearSlot))
                    result.Add(items[i]);
            result.Sort((a, b) => a.wearOrder.CompareTo(b.wearOrder));
            return result;
        }

        public void Sync(IReadOnlyList<ItemEntry> items, Material material, bool facingLeft)
        {
            if (lastCount != items.Count)
            {
                foreach (var view in views) if (view != null) Destroy(view.gameObject);
                views.Clear(); selected.Clear(); selected.AddRange(VisibleItems(items));
                foreach (var item in selected)
                {
                    var obj = new GameObject("Worn " + item.title);
                    var animation=GetComponent<SpriteFrameAnimator>();
                    obj.transform.SetParent(animation!=null?animation.BodyRoot:transform, false);
                    var view = obj.AddComponent<SpriteRenderer>();
                    view.sprite = item.image; view.sharedMaterial = material; view.sortingOrder = item.wearOrder;
                    obj.transform.localScale = Vector3.one * item.wearScale;
                    views.Add(view);
                }
                lastCount = items.Count;
            }
            for (int i = 0; i < views.Count; i++)
            {
                Vector2 offset = selected[i].wearOffset;
                views[i].transform.localPosition = new Vector3(offset.x * (facingLeft ? -1 : 1), offset.y, -.02f);
                views[i].flipX = facingLeft;
            }
        }
    }
}

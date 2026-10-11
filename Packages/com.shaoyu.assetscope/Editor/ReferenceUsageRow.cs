using System;
using UnityEngine.UIElements;

namespace AssetScope
{
    public sealed class ReferenceUsageRow : Button
    {
        ReferenceUsage usage;
        public ReferenceUsageRow(Action<ReferenceUsage> activate)
        {
            name = "reference-location";
            // Register once: ListView recycles rows and rebinds their current usage.
            clicked += () => { if (usage != null) activate(usage); };
            style.unityTextAlign = UnityEngine.TextAnchor.MiddleLeft;
        }
        public void Bind(ReferenceUsage value)
        {
            usage = value;
            text = $"Open source · {value.hierarchyPath} · {value.componentType}\n{value.propertyPath} → {value.targetAssetPath}\n{value.targetName} · local ID {value.targetLocalId}";
            tooltip = text + "\nSelects the source component in Prefab Mode or a saved scene, or the exact source subasset.";
        }
        public void Unbind() { usage = null; text = ""; tooltip = ""; }
    }
}

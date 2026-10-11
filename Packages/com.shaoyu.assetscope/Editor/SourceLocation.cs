using System;
using System.Collections.Generic;
using UnityEngine;

namespace AssetScope
{
    [Serializable]
    public sealed class SourceLocation
    {
        public string assetGuid;
        public string dependencyHash;
        public long localId;
        public string typeName;
        public int componentIndex = -1;
        public int[] hierarchyIndices = Array.Empty<int>();
        public string[] hierarchyNames = Array.Empty<string>();

        public static SourceLocation Capture(UnityEngine.Object owner, long localId, int componentIndex)
        {
            var source = new SourceLocation { localId = localId, componentIndex = componentIndex, typeName = owner.GetType().AssemblyQualifiedName };
            if (!(owner is Component component)) return source;
            var indices = new List<int>(); var names = new List<string>();
            for (var node = component.transform; node != null; node = node.parent)
            {
                indices.Add(node.GetSiblingIndex()); names.Add(node.name);
            }
            indices.Reverse(); names.Reverse();
            source.hierarchyIndices = indices.ToArray(); source.hierarchyNames = names.ToArray();
            return source;
        }
    }
}

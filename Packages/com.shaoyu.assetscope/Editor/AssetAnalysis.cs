using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AssetScope
{
    [Serializable]
    public sealed class ScanOptions
    {
        public bool inspectSavedScenes;
        public bool inspectProperties = true;
        public bool useCache = true;
        public string excludedFolders = "";
        public string Signature => $"analysis-v4|scenes={inspectSavedScenes}|properties={inspectProperties}";
        public ScanOptions Copy() => JsonUtility.FromJson<ScanOptions>(JsonUtility.ToJson(this));
        public bool Includes(string path) => !(excludedFolders ?? "").Split(';')
            .Select(p => p.Trim().Replace('\\', '/').TrimEnd('/')).Where(p => p.Length > 0)
            .Any(p => AssetCatalog.IsInScope(path, p));
    }

    [Serializable]
    public sealed class ReferenceUsage
    {
        public string sourceAssetPath;
        public SourceLocation source;
        public long sourceLocalId;
        public string hierarchyPath;
        public string componentType;
        public string propertyPath;
        public string targetAssetPath;
        public string targetGuid;
        public long targetLocalId;
        public string targetName;
    }

    [Serializable]
    public sealed class AssetAnalysis
    {
        public string assetPath;
        public string guid;
        public string dependencyHash;
        public string signature;
        public string[] dependencies = Array.Empty<string>();
        public Finding[] findings = Array.Empty<Finding>();
        public ReferenceUsage[] usages = Array.Empty<ReferenceUsage>();
        public string inspection = "Dependencies only";
    }

    public static class AssetInspector
    {
        public static AssetAnalysis Inspect(string path, ScanOptions options)
        {
            var result = new AssetAnalysis
            {
                assetPath = path, guid = AssetDatabase.AssetPathToGUID(path),
                dependencyHash = AssetDatabase.GetAssetDependencyHash(path).ToString(), signature = options.Signature,
                dependencies = AssetDatabase.GetDependencies(path, false).Where(p => p != path).Distinct().OrderBy(p => p, StringComparer.Ordinal).ToArray()
            };
            var findings = new List<Finding>();
            var usages = new List<ReferenceUsage>();
            if (path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
            {
                var root = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (root == null) throw new InvalidOperationException("Prefab could not be loaded.");
                InspectHierarchy(root, path, options, findings, usages);
                result.inspection = options.inspectProperties ? "Prefab hierarchy and serialized properties" : "Prefab missing scripts";
            }
            else if (path.EndsWith(".unity", StringComparison.OrdinalIgnoreCase))
            {
                if (options.inspectSavedScenes)
                {
                    var scene = EditorSceneManager.OpenPreviewScene(path);
                    try
                    {
                        foreach (var root in scene.GetRootGameObjects()) InspectHierarchy(root, path, options, findings, usages);
                        result.inspection = "Saved scene preview; unsaved changes excluded";
                    }
                    finally { EditorSceneManager.ClosePreviewScene(scene); }
                }
                else result.inspection = "Scene skipped by option; dependencies only";
            }
            else if (options.inspectProperties)
            {
                // Enumerate subassets too, preserving local file IDs in reference identities.
                foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(path))
                {
                    if (asset is ScriptableObject || asset is Material)
                        InspectObject(asset, path, asset.name, findings, usages);
                }
                result.inspection = "Serialized properties of ScriptableObject / Material assets; other types dependencies only";
            }
            if (options.inspectProperties && (!path.EndsWith(".unity", StringComparison.OrdinalIgnoreCase) || options.inspectSavedScenes))
            {
                // Prefer precise live-property diagnostics when they already identify
                // this field; retain saved-file evidence when Unity resolves it to None.
                var liveMissingFields = new HashSet<string>(findings.Where(f => f.code == "MISSING_REFERENCE").Select(f => f.sourceLocalId + ":" + f.propertyPath));
                foreach (var saved in SavedReferenceValidator.Inspect(path))
                    if (!liveMissingFields.Contains(saved.sourceLocalId + ":" + saved.propertyPath) && !(saved.propertyPath == "m_Script" && findings.Any(f => f.code == "MISSING_SCRIPT")))
                        findings.Add(saved);
            }
            result.findings = findings.OrderBy(f => f.hierarchyPath, StringComparer.Ordinal).ThenBy(f => f.propertyPath, StringComparer.Ordinal).ToArray();
            foreach (var usage in usages) { usage.source.assetGuid = result.guid; usage.source.dependencyHash = result.dependencyHash; }
            result.usages = usages.OrderBy(u => u.hierarchyPath, StringComparer.Ordinal).ThenBy(u => u.componentType, StringComparer.Ordinal).ThenBy(u => u.propertyPath, StringComparer.Ordinal).ToArray();
            return result;
        }

        static void InspectHierarchy(GameObject root, string path, ScanOptions options, List<Finding> findings, List<ReferenceUsage> usages)
        {
            findings.AddRange(MissingScriptValidator.Inspect(root, path));
            if (!options.inspectProperties) return;
            foreach (var transform in root.GetComponentsInChildren<Transform>(true))
            {
                var hierarchy = MissingScriptValidator.HierarchyPath(transform, root.transform);
                var components = transform.GetComponents<Component>();
                for (var index = 0; index < components.Length; index++)
                    if (components[index] != null) InspectObject(components[index], path, hierarchy, findings, usages, index);
            }
        }

        static void InspectObject(Object owner, string path, string hierarchy, List<Finding> findings, List<ReferenceUsage> usages, int componentIndex = -1)
        {
            var component = owner.GetType().Name + (componentIndex >= 0 ? $"[{componentIndex}]" : "");
            using (var serialized = new SerializedObject(owner))
            {
                var property = serialized.GetIterator();
                var managedIds = new HashSet<long>();
                bool enterChildren = true;
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(owner, out string _, out long sourceId);
                var source = SourceLocation.Capture(owner, sourceId, componentIndex);
                while (property.Next(enterChildren))
                {
                    enterChildren = true;
                    // SerializeReference graphs can contain cycles and shared nodes.
                    // Inspect each managed object once per owner to guarantee termination.
                    if (property.propertyType == SerializedPropertyType.ManagedReference && property.managedReferenceId >= 0)
                        enterChildren = managedIds.Add(property.managedReferenceId);
                    if (property.propertyType != SerializedPropertyType.ObjectReference) continue;
                    var target = property.objectReferenceValue;
                    if (target == null)
                    {
#if UNITY_6000_6_OR_NEWER
                        if (property.objectReferenceEntityIdValue.IsValid())
#else
                        if (property.objectReferenceInstanceIDValue != 0)
#endif
                            findings.Add(new Finding
                            {
                                code = "MISSING_REFERENCE", assetPath = path, hierarchyPath = hierarchy,
                                componentType = component, propertyPath = property.propertyPath, sourceLocalId = sourceId,
                                message = "Serialized object reference cannot be resolved. Restore its target or clear the field deliberately."
                            });
                        continue; // Ordinary unassigned fields are not errors.
                    }
                    var targetPath = AssetDatabase.GetAssetPath(target);
                    if (string.IsNullOrEmpty(targetPath) || !AssetDatabase.TryGetGUIDAndLocalFileIdentifier(target, out string guid, out long localId)) continue;
                    usages.Add(new ReferenceUsage
                    {
                        sourceAssetPath = path, sourceLocalId = sourceId, source = source, hierarchyPath = hierarchy, componentType = component,
                        propertyPath = property.propertyPath, targetAssetPath = targetPath, targetGuid = guid,
                        targetLocalId = localId, targetName = target.name
                    });
                }
            }
        }
    }
}

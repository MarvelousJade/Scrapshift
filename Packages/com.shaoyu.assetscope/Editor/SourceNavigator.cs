using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace AssetScope
{
    public sealed class NavigationResult
    {
        public bool Success { get; }
        public string Message { get; }
        public Object Source { get; }
        public NavigationResult(bool success, string message, Object source = null) { Success = success; Message = message; Source = source; }
    }

    public static class SourceNavigator
    {
        public static NavigationResult Navigate(ReferenceUsage usage)
        {
            Scene opened = default;
            var previous = SceneManager.GetActiveScene();
            bool success = false;
            try
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating)
                    return Fail("Wait for Edit Mode and asset imports to finish, then retry.");
                var source = usage?.source;
                if (source == null || string.IsNullOrEmpty(source.assetGuid) || string.IsNullOrEmpty(source.dependencyHash))
                    return Fail("This reference has no source location. Run a fresh scan.");
                var path = AssetDatabase.GUIDToAssetPath(source.assetGuid);
                // Unity may retain a GUID-to-path mapping after deletion in this session.
                if (string.IsNullOrEmpty(path) || AssetDatabase.AssetPathToGUID(path, AssetPathToGUIDOptions.OnlyExistingAssets) != source.assetGuid)
                    return Fail("The source asset was deleted or replaced. Run a fresh scan.");
                if (AssetDatabase.GetAssetDependencyHash(path).ToString() != source.dependencyHash)
                    return Fail("The source or its dependencies changed. Run a fresh scan before navigating.");

                Object owner;
                if (source.componentIndex < 0)
                {
                    owner = AssetDatabase.LoadAllAssetsAtPath(path).FirstOrDefault(asset =>
                        asset != null && AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out string guid, out long id) && guid == source.assetGuid && id == source.localId);
                    if (owner != null && EditorUtility.IsDirty(owner)) return Fail("The source asset has unsaved changes. Save it and rescan before navigating.");
                }
                else
                {
                    var currentStage = PrefabStageUtility.GetCurrentPrefabStage();
                    if (currentStage != null && currentStage.scene.isDirty)
                        return Fail("The open prefab has unsaved changes. Save or discard them yourself, then rescan.");
                    if (path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
                    {
                        var stage = currentStage != null && currentStage.assetPath == path ? currentStage : PrefabStageUtility.OpenPrefab(path);
                        if (stage == null) return Fail("Prefab Mode could not open the source.");
                        owner = ResolveComponent(new[] { stage.prefabContentsRoot }, source);
                    }
                    else if (path.EndsWith(".unity", StringComparison.OrdinalIgnoreCase))
                    {
                        var scene = SceneManager.GetSceneByPath(path);
                        if (scene.IsValid() && scene.isLoaded && scene.isDirty)
                            return Fail("The source scene has unsaved changes. Save it and rescan before navigating.");
                        if (!scene.IsValid() || !scene.isLoaded)
                        {
                            // Never replace or silently save an untitled user scene.
                            for (int i = 0; i < SceneManager.sceneCount; i++)
                                if (string.IsNullOrEmpty(SceneManager.GetSceneAt(i).path))
                                    return Fail("Save the untitled scene before opening this source additively, then retry.");
                            if (currentStage != null) StageUtility.GoToMainStage();
                            opened = scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                        }
                        else if (currentStage != null) StageUtility.GoToMainStage();
                        owner = ResolveComponent(scene.GetRootGameObjects(), source);
                    }
                    else return Fail("This source type does not support component navigation.");
                }

                if (owner == null || owner.GetType().AssemblyQualifiedName != source.typeName)
                    return Fail("The recorded object or component no longer matches. Run a fresh scan.");
                using (var serialized = new SerializedObject(owner))
                {
                    var property = serialized.FindProperty(usage.propertyPath);
                    if (property == null || property.propertyType != SerializedPropertyType.ObjectReference)
                        return Fail("The recorded reference field no longer exists. Run a fresh scan.");
                }
                Selection.activeObject = owner;
                EditorGUIUtility.PingObject(owner is Component component ? component.gameObject : owner);
                success = true;
                return new NavigationResult(true, $"Selected {usage.componentType} on {owner.name} • {usage.propertyPath}", owner);
            }
            catch (Exception e) { return Fail("Source navigation failed: " + e.Message); }
            finally
            {
                if (!success && opened.IsValid() && opened.isLoaded) EditorSceneManager.CloseScene(opened, true);
                if (previous.IsValid() && previous.isLoaded && !EditorSceneManager.IsPreviewScene(previous)) SceneManager.SetActiveScene(previous);
            }
        }

        static NavigationResult Fail(string message) => new NavigationResult(false, message);

        static Component ResolveComponent(GameObject[] roots, SourceLocation source)
        {
            var indices = source.hierarchyIndices; var names = source.hierarchyNames;
            if (indices == null || names == null || indices.Length == 0 || indices.Length != names.Length) return null;
            if (indices[0] < 0 || indices[0] >= roots.Length || roots[indices[0]] == null) return null;
            var node = roots[indices[0]].transform;
            for (int i = 0; i < indices.Length; i++)
            {
                if (i > 0)
                {
                    if (indices[i] < 0 || indices[i] >= node.childCount) return null;
                    node = node.GetChild(indices[i]);
                }
                if (node.name != names[i]) return null;
            }
            var components = node.GetComponents<Component>();
            return source.componentIndex >= 0 && source.componentIndex < components.Length ? components[source.componentIndex] : null;
        }
    }
}

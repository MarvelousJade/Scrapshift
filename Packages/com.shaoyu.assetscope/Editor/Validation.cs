using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AssetScope
{
    [Serializable]
    public sealed class Finding
    {
        public string code;
        public string assetPath;
        public string hierarchyPath;
        public int missingCount;
        public string message;
    }

    public static class MissingScriptValidator
    {
        public static List<Finding> Inspect(GameObject root, string assetPath)
        {
            var result = new List<Finding>();
            if (root == null) return result;
            // Structural validation must include inactive objects without activating them.
            foreach (var transform in root.GetComponentsInChildren<Transform>(true))
            {
                var count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject);
                if (count == 0) continue;
                result.Add(new Finding
                {
                    code = "MISSING_SCRIPT", assetPath = assetPath,
                    hierarchyPath = HierarchyPath(transform, root.transform), missingCount = count,
                    message = $"{count} missing script component(s). Inspect the component and restore its script or remove it deliberately."
                });
            }
            return result;
        }

        static string HierarchyPath(Transform node, Transform root)
        {
            var names = new List<string>();
            while (node != null)
            {
                // Sibling indices disambiguate repeated object names.
                names.Add($"{node.name}[{node.GetSiblingIndex()}]");
                if (node == root) break;
                node = node.parent;
            }
            names.Reverse();
            return string.Join("/", names);
        }
    }

    public enum ScanState { Running, Complete, Cancelled, Failed, Stale }

    public sealed class ScanSession
    {
        readonly string[] paths;
        readonly Func<string, string[]> dependencies;
        readonly Func<string, List<Finding>> validate;
        public DependencyIndex Index { get; } = new DependencyIndex();
        public List<Finding> Findings { get; } = new List<Finding>();
        public int Processed { get; private set; }
        public int Total => paths.Length;
        public string Folder { get; }
        public ScanState State { get; private set; } = ScanState.Running;
        public string Error { get; private set; } = "";

        public ScanSession(string[] paths, Func<string, string[]> dependencies, Func<string, List<Finding>> validate, string folder = "Assets")
        {
            Folder = folder;
            this.paths = (string[])paths.Clone();
            this.dependencies = dependencies;
            this.validate = validate;
        }

        public void Step()
        {
            if (State != ScanState.Running) return;
            if (Processed == paths.Length) { State = ScanState.Complete; return; }
            var path = paths[Processed];
            try
            {
                var edges = dependencies(path);
                var findings = validate(path);
                Index.Add(path, edges);
                Findings.AddRange(findings);
                Processed++;
                if (Processed == paths.Length) State = ScanState.Complete;
            }
            catch (Exception e)
            {
                State = ScanState.Failed;
                Error = $"{path}: {e.GetType().Name}: {e.Message}";
            }
        }

        public void Cancel() { if (State == ScanState.Running) State = ScanState.Cancelled; }
        public void Invalidate() { State = ScanState.Stale; }

        public static ScanSession Create(string folder)
        {
            return new ScanSession(AssetCatalog.Find("", folder),
                p => AssetDatabase.GetDependencies(p, false),
                p => p.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase)
                    ? MissingScriptValidator.Inspect(AssetDatabase.LoadAssetAtPath<GameObject>(p), p)
                    : new List<Finding>(), folder);
        }
    }

    [Serializable]
    public sealed class ScanReport
    {
        public int schemaVersion = 1;
        public string unityVersion;
        public string generatedUtc;
        public string folder;
        public string state;
        public int processed;
        public int total;
        public string error;
        public string scope = "AssetDatabase direct dependencies of assets in the selected folder; missing scripts in prefabs, including inactive children. Scenes are indexed for dependencies but not opened or validated. No unused-asset inference.";
        public Finding[] findings;

        public static ScanReport From(ScanSession session) => new ScanReport
        {
            unityVersion = Application.unityVersion, generatedUtc = DateTime.UtcNow.ToString("O"),
            folder = session.Folder, state = session.State.ToString(), processed = session.Processed,
            total = session.Total, error = session.Error, findings = session.Findings.ToArray()
        };
    }
}

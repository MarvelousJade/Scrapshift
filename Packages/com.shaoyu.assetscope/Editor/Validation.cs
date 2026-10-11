using System;
using System.Collections.Generic;
using System.Linq;
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
        public string severity = "Warning";
        public string componentType;
        public string propertyPath;
        public long sourceLocalId;
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

        public static string HierarchyPath(Transform node, Transform root)
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

    public enum ScanState { Running, Complete, Cancelled, Failed, Stale, CompleteWithErrors }

    public sealed class ScanSession
    {
        readonly string[] paths;
        readonly Func<string, string[]> dependencies;
        readonly Func<string, List<Finding>> validate;
        Func<string, AssetAnalysis> analyze;
        AnalysisCache cache;
        int cacheGeneration;
        bool continueOnError;
        readonly System.Diagnostics.Stopwatch clock = System.Diagnostics.Stopwatch.StartNew();
        public DependencyIndex Index { get; } = new DependencyIndex();
        public List<Finding> Findings { get; } = new List<Finding>();
        public List<AssetAnalysis> Assets { get; } = new List<AssetAnalysis>();
        public List<ReferenceUsage> Usages { get; } = new List<ReferenceUsage>();
        public ScanOptions Options { get; private set; } = new ScanOptions();
        public int CacheHits { get; private set; }
        public int ErrorCount { get; private set; }
        public long ElapsedMilliseconds => clock.ElapsedMilliseconds;
        public string CacheWarning => cache?.Warning ?? "";
        public bool HasResults => State == ScanState.Complete || State == ScanState.CompleteWithErrors;
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
            if (cache != null && cache.Generation != cacheGeneration) { Invalidate(); return; }
            if (Processed == paths.Length) { Finish(); return; }
            var path = paths[Processed];
            try
            {
                var asset = analyze != null ? analyze(path) : new AssetAnalysis { assetPath = path, dependencies = dependencies(path), findings = validate(path).ToArray() };
                Index.Add(path, asset.dependencies);
                Findings.AddRange(asset.findings);
                Usages.AddRange(asset.usages);
                Assets.Add(asset);
                Processed++;
                if (Processed == paths.Length) Finish();
            }
            catch (Exception e)
            {
                ErrorCount++;
                Error = $"{path}: {e.GetType().Name}: {e.Message}";
                if (!continueOnError) { State = ScanState.Failed; clock.Stop(); return; }
                Findings.Add(new Finding { code = "ASSET_SCAN_ERROR", severity = "Error", assetPath = path, message = Error });
                Processed++;
                if (Processed == paths.Length) Finish();
            }
        }

        void Finish()
        {
            State = ErrorCount == 0 ? ScanState.Complete : ScanState.CompleteWithErrors;
            clock.Stop(); cache?.Save();
        }

        public void Cancel() { if (State == ScanState.Running) { State = ScanState.Cancelled; clock.Stop(); } }
        public void Invalidate() { State = ScanState.Stale; clock.Stop(); }

        public static ScanSession Create(string folder, ScanOptions options = null, AnalysisCache cache = null)
        {
            if (!AssetDatabase.IsValidFolder(folder)) throw new ArgumentException("Select an existing project folder.", nameof(folder));
            var snapshot = (options ?? new ScanOptions()).Copy();
            var session = new ScanSession(AssetCatalog.Find("", folder).Where(snapshot.Includes).ToArray(), null, null, folder)
                { Options = snapshot, continueOnError = true };
            if (snapshot.useCache) { session.cache = cache ?? AnalysisCache.Shared; session.cacheGeneration = session.cache.Generation; }
            session.analyze = path =>
            {
                if (session.cache != null && session.cache.TryGet(AssetDatabase.AssetPathToGUID(path), path,
                    AssetDatabase.GetAssetDependencyHash(path).ToString(), snapshot.Signature, out var cached))
                    { session.CacheHits++; return cached; }
                var analysis = AssetInspector.Inspect(path, snapshot);
                session.cache?.Put(analysis);
                return analysis;
            };
            return session;
        }
    }

    [Serializable]
    public sealed class ScanReport
    {
        public int schemaVersion = 2;
        public string unityVersion;
        public string generatedUtc;
        public string folder;
        public string state;
        public int processed;
        public int total;
        public string error;
        public int errors;
        public int cacheHits;
        public long elapsedMilliseconds;
        public string cacheWarning;
        public ScanOptions options;
        public string scope;
        public AssetAnalysis[] assets;
        public ReferenceUsage[] usages;
        public Finding[] findings;

        public static ScanReport From(ScanSession session) => new ScanReport
        {
            unityVersion = Application.unityVersion, generatedUtc = DateTime.UtcNow.ToString("O"),
            folder = session.Folder, state = session.State.ToString(), processed = session.Processed,
            total = session.Total, error = session.Error, findings = session.Findings.ToArray()
            , errors = session.ErrorCount, cacheHits = session.CacheHits, elapsedMilliseconds = session.ElapsedMilliseconds,
            cacheWarning = session.CacheWarning, options = session.Options.Copy(), assets = session.Assets.ToArray(), usages = session.Usages.ToArray(),
            scope = "Direct asset dependencies in selected folder, excluding configured folders. Prefab missing scripts; " +
                (session.Options.inspectProperties ? "serialized references in prefab components, ScriptableObjects and Materials. " : "serialized properties disabled. ") +
                (session.Options.inspectSavedScenes ? "Scenes: saved files inspected in preview scenes; unsaved edits excluded. " : "Scenes: dependencies only; content inspection disabled. ") +
                "No runtime reference completeness or safe-to-delete inference."
        };
    }
}

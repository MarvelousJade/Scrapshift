using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AssetScope
{
    public sealed class AnalysisCache
    {
        [Serializable] sealed class Store { public int version = 2; public AssetAnalysis[] entries; }
        readonly Dictionary<string, AssetAnalysis> entries = new Dictionary<string, AssetAnalysis>();
        readonly string file;
        public int Generation { get; private set; }
        public string Warning { get; private set; } = "";
        public int Count => entries.Count;
        static AnalysisCache shared;
        public static AnalysisCache Shared => shared ?? (shared = new AnalysisCache("Library/AssetScope/analysis-v2.json"));

        public AnalysisCache(string file = null)
        {
            this.file = file;
            if (file == null || !File.Exists(file)) return;
            try
            {
                var store = JsonUtility.FromJson<Store>(File.ReadAllText(file));
                if (store == null || store.version != 2 || store.entries == null) return;
                foreach (var entry in store.entries.Take(4096))
                    if (entry != null && !string.IsNullOrEmpty(entry.guid)) entries[Key(entry.guid, entry.signature)] = entry;
            }
            catch (Exception e) { Warning = "Cache ignored: " + e.Message; entries.Clear(); }
        }

        static string Key(string guid, string signature) => guid + "|" + signature;
        static AssetAnalysis Copy(AssetAnalysis value) => JsonUtility.FromJson<AssetAnalysis>(JsonUtility.ToJson(value));

        public bool TryGet(string guid, string path, string hash, string signature, out AssetAnalysis analysis)
        {
            analysis = null;
            if (!entries.TryGetValue(Key(guid, signature), out var cached) || cached.assetPath != path || cached.dependencyHash != hash) return false;
            analysis = Copy(cached); return true;
        }

        public void Put(AssetAnalysis value)
        {
            if (string.IsNullOrEmpty(value.guid)) return;
            if (entries.Count >= 4096) entries.Clear();
            entries[Key(value.guid, value.signature)] = Copy(value);
        }

        public void Clear()
        {
            entries.Clear(); Generation++;
            if (file != null && File.Exists(file))
                try { File.Delete(file); } catch (Exception e) { Warning = "Cache invalidation could not remove disk cache: " + e.Message; }
        }

        public void Save()
        {
            if (file == null) return;
            var temporary = file + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(file)));
                File.WriteAllText(temporary, JsonUtility.ToJson(new Store { entries = entries.Values.OrderBy(e => e.assetPath, StringComparer.Ordinal).ToArray() }));
                if (File.Exists(file)) File.Replace(temporary, file, null); else File.Move(temporary, file);
            }
            catch (Exception e) { Warning = "Results are valid but cache could not be saved: " + e.Message; }
            finally
            {
                try { if (File.Exists(temporary)) File.Delete(temporary); }
                catch (Exception e) { Warning = "Cache temporary file could not be removed: " + e.Message; }
            }
        }

        internal static void ProjectImported()
        {
            // A target can move without changing the referencing asset's content hash.
            // Conservatively invalidate every cached record on imports/moves/deletions.
            Shared.Clear();
        }
    }

    sealed class AssetScopeImportWatcher : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
        {
            if (imported.Length + deleted.Length + moved.Length + movedFrom.Length > 0) AnalysisCache.ProjectImported();
        }
    }
}

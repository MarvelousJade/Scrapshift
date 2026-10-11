using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace AssetScope
{
    [Serializable]
    public sealed class SearchPreset
    {
        public string name;
        public string folder;
        public string query;
        public bool prefabsOnly;
        public ScanOptions options = new ScanOptions();
    }

    public sealed class SearchPresets
    {
        [Serializable] sealed class Store { public int version = 1; public List<SearchPreset> presets = new List<SearchPreset>(); }
        readonly string path;
        Store store = new Store();
        public string Warning { get; private set; } = "";
        public SearchPresets(string path = "UserSettings/AssetScopePresets.json")
        {
            this.path = path;
            if (!File.Exists(path)) return;
            try
            {
                var loaded = JsonUtility.FromJson<Store>(File.ReadAllText(path));
                if (loaded != null && loaded.version == 1 && loaded.presets != null) store = loaded;
            }
            catch (Exception e) { Warning = "Saved searches could not be read: " + e.Message; }
        }
        public string[] Names => store.presets.Select(p => p.name).OrderBy(p => p, StringComparer.Ordinal).ToArray();
        public SearchPreset Get(string name)
        {
            var preset = store.presets.FirstOrDefault(p => p.name == name);
            return preset == null ? null : JsonUtility.FromJson<SearchPreset>(JsonUtility.ToJson(preset));
        }
        public void Save(SearchPreset preset)
        {
            if (string.IsNullOrWhiteSpace(preset.name)) throw new ArgumentException("Enter a name for this search.");
            var copy = JsonUtility.FromJson<SearchPreset>(JsonUtility.ToJson(preset));
            copy.name = copy.name.Trim();
            var updated = store.presets.Where(p => p.name != copy.name).ToList(); updated.Add(copy);
            var next = new Store { presets = updated };
            ReportWriter.WriteAtomic(path, JsonUtility.ToJson(next, true));
            store = next; Warning = "";
        }
    }
}

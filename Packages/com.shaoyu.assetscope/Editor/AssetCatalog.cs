using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace AssetScope
{
    public static class AssetCatalog
    {
        public static bool IsInScope(string path, string folder)
        {
            folder = (folder ?? "Assets").Replace('\\', '/').TrimEnd('/');
            return path != null && (path == folder || path.StartsWith(folder + "/", StringComparison.Ordinal));
        }

        public static string[] Find(string query, string folder = "Assets")
        {
            if (!AssetDatabase.IsValidFolder(folder)) return Array.Empty<string>();
            return AssetDatabase.FindAssets(query ?? "", new[] { folder })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => IsInScope(p, folder) && !AssetDatabase.IsValidFolder(p))
                .Distinct().OrderBy(p => p, StringComparer.Ordinal).ToArray();
        }
    }

    public sealed class DependencyIndex
    {
        readonly Dictionary<string, string[]> outgoing = new Dictionary<string, string[]>();
        readonly Dictionary<string, HashSet<string>> incoming = new Dictionary<string, HashSet<string>>();
        public int Count => outgoing.Count;
        public string[] Paths => outgoing.Keys.OrderBy(p => p, StringComparer.Ordinal).ToArray();

        public void Add(string path, IEnumerable<string> dependencies)
        {
            // A path may be updated; first remove its old reverse edges.
            if (outgoing.TryGetValue(path, out var old))
                foreach (var dependency in old)
                    if (incoming.TryGetValue(dependency, out var users)) users.Remove(path);
            var edges = dependencies.Where(p => p != path).Distinct()
                .OrderBy(p => p, StringComparer.Ordinal).ToArray();
            outgoing[path] = edges;
            foreach (var dependency in edges)
            {
                if (!incoming.TryGetValue(dependency, out var users))
                    incoming[dependency] = users = new HashSet<string>();
                users.Add(path);
            }
        }

        public string[] Dependencies(string path) => outgoing.TryGetValue(path, out var edges) ? edges.ToArray() : Array.Empty<string>();
        public string[] Referencers(string path) => incoming.TryGetValue(path, out var users)
            ? users.OrderBy(p => p, StringComparer.Ordinal).ToArray() : Array.Empty<string>();

        public string[] Transitive(string path, bool reverse = false)
        {
            var visited = new HashSet<string> { path };
            var queue = new Queue<string>(); queue.Enqueue(path);
            while (queue.Count > 0)
                foreach (var next in reverse ? Referencers(queue.Dequeue()) : Dependencies(queue.Dequeue()))
                    if (visited.Add(next)) queue.Enqueue(next);
            visited.Remove(path);
            return visited.OrderBy(p => p, StringComparer.Ordinal).ToArray();
        }

        public string[] ShortestChain(string from, string to)
        {
            if (from == to) return new[] { from };
            var parent = new Dictionary<string, string> { [from] = null };
            var queue = new Queue<string>(); queue.Enqueue(from);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                foreach (var next in Dependencies(current))
                {
                    if (parent.ContainsKey(next)) continue;
                    parent[next] = current;
                    if (next == to)
                    {
                        var chain = new List<string>();
                        for (var node = to; node != null; node = parent[node]) chain.Add(node);
                        chain.Reverse(); return chain.ToArray();
                    }
                    queue.Enqueue(next);
                }
            }
            return Array.Empty<string>();
        }
    }
}

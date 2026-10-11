using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;

namespace AssetScope
{
    // A narrow fallback for Unity's inline YAML object references. After a cold
    // import Unity may expose an unresolved GUID as null with EntityId.None.
    // This is a saved-file diagnostic, not a general YAML/property-path parser.
    public static class SavedReferenceValidator
    {
        static readonly Regex Reference = new Regex(@"^\s*(?:-\s*)?(?:(?<field>[A-Za-z_][A-Za-z0-9_]*):\s*)?\{fileID:\s*(?<id>-?\d+),\s*guid:\s*(?<guid>[a-fA-F0-9]{32}),\s*type:\s*\d+\}\s*$");
        static readonly Regex Builtin = new Regex(@"^(?:0{32}|0{16}[def]0{15})$", RegexOptions.IgnoreCase);
        static readonly Regex Block = new Regex(@"^\s*[A-Za-z_][A-Za-z0-9_]*:\s*[|>][-+]?\s*$");
        static readonly Regex Document = new Regex(@"^--- !u!\d+ &(?<id>-?\d+)");

        public static IEnumerable<Finding> Inspect(string path)
        {
            if (!File.Exists(path)) return Array.Empty<Finding>();
            var extension = Path.GetExtension(path).ToLowerInvariant();
            if (extension != ".asset" && extension != ".prefab" && extension != ".mat" && extension != ".unity") return Array.Empty<Finding>();
            // Avoid reading arbitrary binary payloads into a string.
            using (var reader = new StreamReader(path))
            {
                var header = new char[9];
                if (reader.ReadBlock(header, 0, header.Length) != header.Length || new string(header) != "%YAML 1.1") return Array.Empty<Finding>();
                return InspectText(path, new string(header) + reader.ReadToEnd(), guid => !string.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(guid)));
            }
        }

        public static Finding[] InspectText(string path, string text, Func<string, bool> guidExists)
        {
            if (!text.StartsWith("%YAML 1.1", StringComparison.Ordinal)) return Array.Empty<Finding>();
            var result = new List<Finding>();
            var lines = text.Replace("\r\n", "\n").Split('\n');
            int blockIndent = -1;
            long ownerId = 0;
            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i]; int indent = line.TakeWhile(char.IsWhiteSpace).Count();
                if (blockIndent >= 0 && (string.IsNullOrWhiteSpace(line) || indent > blockIndent)) continue;
                blockIndent = -1;
                var document = Document.Match(line);
                if (document.Success) { long.TryParse(document.Groups["id"].Value, out ownerId); continue; }
                if (Block.IsMatch(line)) { blockIndent = indent; continue; }
                var match = Reference.Match(line);
                if (!match.Success || match.Groups["id"].Value == "0") continue;
                var guid = match.Groups["guid"].Value;
                if (Builtin.IsMatch(guid) || guidExists(guid)) continue;
                var field = match.Groups["field"].Success ? match.Groups["field"].Value : "[array entry]";
                result.Add(new Finding {
                    code = "MISSING_REFERENCE", assetPath = path, sourceLocalId = ownerId, hierarchyPath = $"Saved YAML line {i + 1}",
                    componentType = "Saved object reference", propertyPath = field,
                    message = $"Saved reference points to unresolved asset GUID {guid}. Location is a YAML field/line, not a reconstructed component property path."
                });
            }
            return result.ToArray();
        }
    }
}

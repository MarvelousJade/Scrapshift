using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Search;
using UnityEngine;

namespace AssetScope
{
    public static class AssetScopeSearchProvider
    {
        [SearchItemProvider]
        public static SearchProvider CreateProvider()
        {
            return new SearchProvider("assetscope", "AssetScope")
            {
                filterId = "scope:", priority = 90,
                fetchItems = (context, items, provider) => Fetch(context, provider),
                toObject = (item, type) => AssetDatabase.LoadMainAssetAtPath(item.id),
                fetchThumbnail = (item, context) => AssetDatabase.GetCachedIcon(item.id) as Texture2D,
                trackSelection = (item, context) => EditorGUIUtility.PingObject(AssetDatabase.LoadMainAssetAtPath(item.id))
            };
        }

        static IEnumerable<SearchItem> Fetch(SearchContext context, SearchProvider provider)
        {
            foreach (var path in AssetCatalog.Find(context.searchQuery))
                yield return provider.CreateItem(context, path, 0, System.IO.Path.GetFileName(path), path, null, path);
        }
    }
}

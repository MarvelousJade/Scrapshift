using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Scrapshift.Editor
{
    // Run in the isolated Unity validation clone. Only these three character
    // fields come from the revised roster import; root reviews YAML diff
    // before carrying the intended fields to the main checkout.
    public static class SyncStartingItemChoiceCatalog
    {
        [Serializable] sealed class ProfileFields
        {
            public string id;
            public string[] startingItems,startingItemChoices;
            public CharacterEffectRow[] effectRows;
        }
        [Serializable] sealed class RosterFields { public ProfileFields[] characters; }

        public static void Apply()
        {
            try
            {
                string json=File.ReadAllText("Assets/Scrapshift/Content/Reference/starter-profiles.json");
                var roster=JsonUtility.FromJson<RosterFields>(json);
                var catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
                if(roster?.characters==null||roster.characters.Length!=50||catalog?.data?.characters==null||catalog.data.characters.Length!=50)
                    throw new InvalidOperationException("Expected 50 imported and catalog characters");
                var byId=roster.characters.ToDictionary(c=>c.id);
                foreach(var profile in catalog.data.characters)
                {
                    if(!byId.TryGetValue(profile.id,out var source))throw new InvalidOperationException("Missing roster profile "+profile.id);
                    profile.startingItems=source.startingItems??Array.Empty<string>();
                    profile.startingItemChoices=source.startingItemChoices??Array.Empty<string>();
                    if(profile.id=="beast_master")profile.effectRows=source.effectRows??Array.Empty<CharacterEffectRow>();
                }
                EditorUtility.SetDirty(catalog);
                AssetDatabase.SaveAssets();
                Debug.Log("STARTING_CHOICE_CATALOG_SYNC_DONE 50");
                EditorApplication.Exit(0);
            }
            catch(Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }
    }
}

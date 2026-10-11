using System.IO;
using UnityEditor;
using UnityEngine;

namespace Scrapshift.Editor
{
    public static partial class ContentBuilder
    {
        static void EnsureWardrobe()
        {
            if (File.Exists(Root + "/WardrobeV1.txt")) return;
            var catalog = AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(Root + "/Definitions/ReferenceCatalog.asset");
            foreach (var item in catalog.data.items)
            {
                string slot = "belt-left"; Vector2 offset = new Vector2(-.44f, -.3f); float scale = .22f; int order = 11;
                switch (item.id)
                {
                    case "beanie": slot = "head"; offset = new Vector2(-.03f, .5f); scale = .59f; break;
                    case "helmet": slot = "head"; offset = new Vector2(.02f, .48f); scale = .60f; break;
                    case "head_injury": slot = "head"; offset = new Vector2(.02f, .30f); scale = .64f; break;
                    case "goat_skull": slot = "head"; offset = new Vector2(.02f, .49f); scale = .47f; break;
                    case "glasses": slot = "eyes"; offset = new Vector2(.07f, .12f); scale = .55f; order = 13; break;
                    case "blindfold": slot = "eyes"; offset = new Vector2(.06f, .17f); scale = .54f; order = 13; break;
                    case "leather_vest": slot = "body"; offset = new Vector2(.02f, -.27f); scale = .47f; break;
                    case "bat": slot = "back"; offset = new Vector2(-.04f, -.03f); scale = 1.1f; order = 8; break;
                    case "hedgehog": slot = "back"; offset = new Vector2(-.37f, .04f); scale = .43f; order = 8; break;
                    case "fertilizer": slot = "back"; offset = new Vector2(-.38f, -.05f); scale = .49f; order = 8; break;
                    case "claw_tree": slot = "crest"; offset = new Vector2(-.05f, .66f); scale = .27f; break;
                    case "broken_mouth": slot = "mouth"; offset = new Vector2(.08f, -.12f); scale = .25f; order = 14; break;
                    case "coffee": slot = "belt-right"; offset = new Vector2(.45f, -.29f); scale = .25f; break;
                    case "cake": slot = "belt-right"; offset = new Vector2(.45f, -.27f); scale = .24f; break;
                    case "gummy_berserker": slot = "belt-right"; offset = new Vector2(.45f, -.28f); scale = .25f; break;
                    case "cyclops_worm": slot = "shoulder-left"; offset = new Vector2(-.44f, .14f); scale = .27f; break;
                    case "decomposing_flesh": slot = "shoulder-left"; offset = new Vector2(-.44f, .14f); scale = .25f; break;
                    case "jelly": slot = "shoulder-right"; offset = new Vector2(.43f, .12f); scale = .24f; break;
                    case "defective_steroids": slot = "left-injector"; offset = new Vector2(-.28f, -.37f); scale = .23f; break;
                    case "injection": slot = "right-injector"; offset = new Vector2(.29f, -.38f); scale = .23f; break;
                    case "gambling_token": slot = "badge"; offset = new Vector2(.03f, -.20f); scale = .16f; order = 12; break;
                    case "insanity": slot = "badge"; offset = new Vector2(.03f, -.20f); scale = .20f; order = 12; break;
                    case "blood_leech": case "fresh_meat": break;
                    default: throw new System.InvalidOperationException("Missing wearable mapping: " + item.id);
                }
                item.wearSlot = slot; item.wearOffset = offset; item.wearScale = scale; item.wearOrder = order;
            }
            EditorUtility.SetDirty(catalog); AssetDatabase.SaveAssets();
            File.WriteAllText(Root + "/WardrobeV1.txt", "All 24 items mapped to wearable slots; newest per slot visible, all effects retained.\n");
            AssetDatabase.Refresh();
        }
    }
}

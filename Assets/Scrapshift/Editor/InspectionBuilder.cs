using UnityEditor;
namespace Scrapshift.Editor
{
    public static partial class ContentBuilder
    {
        static void EnsureInspectionUI()
        {
            var catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(Root+"/Definitions/ReferenceCatalog.asset");
            catalog.referenceFont=AssetDatabase.LoadAssetAtPath<UnityEngine.Font>(Root+"/Reference/Anybody-Medium.ttf");
            catalog.uiText=AssetDatabase.LoadAssetAtPath<UnityEngine.TextAsset>(Root+"/Reference/ui-text.json");
            if(catalog.referenceFont==null||catalog.uiText==null)throw new System.Exception("Missing reference UI font/text.");
            var text=new InspectionText(catalog.uiText);
            var design=UnityEngine.JsonUtility.FromJson<ItemDesignData>(System.IO.File.ReadAllText(Root+"/Reference/item-design.json"));
            var expanded=new System.Collections.Generic.List<ItemEntry>();
            foreach(var item in design.items)
            {
                var target=System.Array.Find(catalog.data.items,i=>i.id==item.id);
                bool original=target?.image!=null&&!AssetDatabase.GetAssetPath(target.image).Contains("/Reference/");
                if(target==null)target=new ItemEntry{id=item.id};
                if(!original){target.image=StatPresentation.Icon(catalog,System.Array.Find(item.effects,e=>e.key.StartsWith("stat_"))?.key.Substring(5)??"engineering");target.appearance=null;target.wearSlot=null;}
                target.title=CyberneticTheme.ItemName(item.id,text.T("ITEM_"+item.id.ToUpperInvariant(),item.title));target.available=item.available;
                target.value=item.value;target.tier=item.tier;target.effects=item.effects;
                target.tags=item.tags;target.groups=item.groups;target.maxCount=item.maxCount;
                expanded.Add(target);
            }
            catalog.data.items=expanded.ToArray();RunRules.ConfigureShopItems(catalog.data.items);
            foreach(var profile in design.characters)
            {
                var target=System.Array.Find(catalog.data.characters,c=>c.id==profile.id);
                target.wantedTags=profile.wantedTags;target.bannedItems=profile.bannedItems;target.effectRows=profile.rows;
            }
            foreach(var weapon in catalog.data.equipment)weapon.title=text.WeaponName(weapon);
            EditorUtility.SetDirty(catalog);AssetDatabase.SaveAssets();
        }
    }
}

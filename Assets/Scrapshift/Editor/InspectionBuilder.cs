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
            var upgrades=UnityEngine.JsonUtility.FromJson<CatalogData>(System.IO.File.ReadAllText("Assets/Scrapshift/Resources/base-upgrades.json")).upgrades;
            foreach(var upgrade in upgrades)upgrade.image=StatPresentation.Icon(catalog,upgrade.stat);
            catalog.data.upgrades=upgrades;
            var design=UnityEngine.JsonUtility.FromJson<ItemDesignData>(System.IO.File.ReadAllText(Root+"/Reference/item-design.json"));
            var expanded=new System.Collections.Generic.List<ItemEntry>();
            foreach(var item in design.items)
            {
                var target=System.Array.Find(catalog.data.items,i=>i.id==item.id);
                bool original=target?.image!=null&&!AssetDatabase.GetAssetPath(target.image).Contains("/Reference/");
                if(target==null)target=new ItemEntry{id=item.id};
                if(!original){target.image=StatPresentation.Icon(catalog,System.Array.Find(item.effects,e=>e.key.StartsWith("stat_"))?.key.Substring(5)??"engineering");target.appearance=null;target.wearSlot=null;}
                target.title=CyberneticTheme.ItemName(item.id,text.T("ITEM_"+item.id.ToUpperInvariant(),item.title));target.available=item.available;
                target.value=item.value;target.tier=item.tier;target.effects=item.effects;target.conditionalEffects=item.conditionalEffects;
                target.tags=item.tags;target.groups=item.groups;target.maxCount=item.maxCount;
                expanded.Add(target);
            }
            catalog.data.items=expanded.ToArray();RunRules.ConfigureShopItems(catalog.data.items);
            ApplyDedicatedItemIcons(catalog);
            ApplyDedicatedWeaponArt(catalog);
            foreach(var profile in design.characters)
            {
                var target=System.Array.Find(catalog.data.characters,c=>c.id==profile.id);
                target.wantedTags=profile.wantedTags;target.bannedItems=profile.bannedItems;target.effectRows=profile.rows;
            }
            foreach(var weapon in catalog.data.equipment)weapon.title=text.WeaponName(weapon);
            EnsureReferenceAppearances(catalog);
            EditorUtility.SetDirty(catalog);AssetDatabase.SaveAssets();
        }

        // Preserve the item-ID mappings whenever the expanded catalog is rebuilt.
        static void ApplyDedicatedItemIcons(ReferenceCatalog catalog)
        {
            const string path=Root+"/OriginalArt/blast-gland-v1.png";
            var sprite=ImportDedicatedSprite(path,512);
            var item=System.Array.Find(catalog.data.items,i=>i.id=="dynamite");
            if(item==null)throw new System.Exception("Blast Gland catalog entry is missing.");
            item.image=sprite;
            ApplyAllItemIcons(catalog);
        }

        static void ApplyDedicatedWeaponArt(ReferenceCatalog catalog)
        {
            var sprite=ImportDedicatedSprite(Root+"/OriginalArt/jousting-lance-v2.png",2048);
            int count=0;
            foreach(var weapon in catalog.data.equipment)if(weapon.id=="jousting_lance")
            {weapon.image=sprite;count++;}
            if(count!=4)throw new System.Exception("Expected four Jousting Lance tiers; found "+count);
        }

        static UnityEngine.Sprite ImportDedicatedSprite(string path,int maxSize)
        {
            AssetDatabase.ImportAsset(path);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            if(importer==null)throw new System.Exception("Missing dedicated art: "+path);
            if(importer.textureType!=TextureImporterType.Sprite||importer.spriteImportMode!=SpriteImportMode.Single||
               importer.maxTextureSize!=maxSize||!importer.alphaIsTransparency||importer.textureCompression!=TextureImporterCompression.Uncompressed)
            {
                importer.textureType=TextureImporterType.Sprite;
                importer.spriteImportMode=SpriteImportMode.Single;
                importer.alphaIsTransparency=true;
                importer.maxTextureSize=maxSize;
                importer.textureCompression=TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
            var sprite=AssetDatabase.LoadAssetAtPath<UnityEngine.Sprite>(path);
            if(sprite==null)throw new System.Exception("Could not import sprite: "+path);
            return sprite;
        }

        public static void RefreshDedicatedArt()
        {
            var catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(Root+"/Definitions/ReferenceCatalog.asset");
            ApplyDedicatedItemIcons(catalog);
            ApplyDedicatedWeaponArt(catalog);
            EditorUtility.SetDirty(catalog);AssetDatabase.SaveAssets();
        }
    }
}

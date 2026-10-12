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
            foreach(var weapon in catalog.data.equipment)weapon.title=text.WeaponName(weapon);
            EditorUtility.SetDirty(catalog);AssetDatabase.SaveAssets();
        }
    }
}

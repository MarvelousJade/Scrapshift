using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace Scrapshift.Editor
{
    public static partial class ContentBuilder
    {
        static void EnsureRoundCanopy()
        {
            var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(Root+"/Meshes/SalvageCanopy.asset");
            if(mesh!=null&&mesh.bounds.size.y>1.7f)return;
            var points=new List<Vector2>();
            for(int i=0;i<96;i++)
            {
                float a=i*Mathf.PI*2/96;
                points.Add(new Vector2(Mathf.Cos(a)*.74f,1.3f+Mathf.Sin(a)*.91f));
            }
            FilledMesh("SalvageCanopy",points);
            string path=Root+"/Prefabs/SalvageParachute.prefab";
            var root=PrefabUtility.LoadPrefabContents(path);
            for(int i=root.transform.childCount-1;i>=0;i--)
            {
                var child=root.transform.GetChild(i);
                if(child.name=="Suspension rope"||child.name=="Canopy seam")Object.DestroyImmediate(child.gameObject);
            }
            quad=AssetDatabase.LoadAssetAtPath<Mesh>(Root+"/Meshes/Quad.asset");
            materials["Ink"]=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Materials/Ink.mat");
            foreach(float side in new[]{-1f,1f})
            {
                Vector2 previous=new Vector2(0,.28f);
                for(int i=1;i<=12;i++)
                {
                    float t=i/12f;var next=new Vector2(side*Mathf.Sin(t*Mathf.PI*.5f)*.65f,.28f+t*1.15f);
                    PresentationLine(root,"Canopy seam",previous,next,.025f,-.06f);previous=next;
                }
                PresentationLine(root,"Suspension rope",new Vector2(side*.17f,2.12f),new Vector2(side*.32f,1.45f),.018f,-.06f);
            }
            PrefabUtility.SaveAsPrefabAsset(root,path);PrefabUtility.UnloadPrefabContents(root);AssetDatabase.SaveAssets();
        }
    }
}

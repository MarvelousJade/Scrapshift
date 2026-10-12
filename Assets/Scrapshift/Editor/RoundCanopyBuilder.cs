using UnityEditor;
using UnityEngine;
namespace Scrapshift.Editor
{
    public static partial class ContentBuilder
    {
        static void EnsureRoundCanopy()
        {
            var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(Root+"/Meshes/SalvageCanopy.asset");
            // Validate the open canopy geometry, not the obsolete oval-height marker.
            if(mesh!=null&&mesh.vertexCount==258&&mesh.bounds.min.y>.99f&&mesh.bounds.max.y<1.91f)return;
            // A strip preserves concave scallops; a center fan can bridge their recesses.
            var vertices=new Vector3[258];var triangles=new int[128*6];
            for(int i=0;i<=128;i++)
            {
                float t=i/128f,x=-.8f+1.6f*t;
                vertices[i*2]=new Vector3(x,1+Mathf.Sqrt(Mathf.Max(0,1-x*x/.64f))*.9f,0);
                vertices[i*2+1]=new Vector3(x,1+Mathf.Abs(Mathf.Sin(t*Mathf.PI*4))*.21f,0);
                if(i==128)continue;int k=i*6,v=i*2;
                triangles[k]=v;triangles[k+1]=v+2;triangles[k+2]=v+1;
                triangles[k+3]=v+1;triangles[k+4]=v+2;triangles[k+5]=v+3;
            }
            var replacement=new Mesh{name="SalvageCanopy",vertices=vertices,triangles=triangles};replacement.RecalculateBounds();replacement.RecalculateNormals();
            EditorUtility.CopySerialized(replacement,mesh);Object.DestroyImmediate(replacement);EditorUtility.SetDirty(mesh);
            string path=Root+"/Prefabs/SalvageParachute.prefab";
            var root=PrefabUtility.LoadPrefabContents(path);
            for(int i=root.transform.childCount-1;i>=0;i--)
            {
                var child=root.transform.GetChild(i);
                if(child.name=="Suspension rope"||child.name=="Canopy seam")Object.DestroyImmediate(child.gameObject);
            }
            quad=AssetDatabase.LoadAssetAtPath<Mesh>(Root+"/Meshes/Quad.asset");
            materials["Ink"]=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Materials/Ink.mat");
            foreach(float x in new[]{-.8f,-.4f,0,.4f,.8f})
                PresentationLine(root,"Suspension rope",new Vector2(x,1.01f),new Vector2(x*.08f,.18f),.018f,-.06f);
            foreach(float side in new[]{-1f,1f})
            {
                Vector2 previous=new Vector2(side*.4f,1.02f);
                for(int i=1;i<=16;i++)
                {float t=i/16f;var next=new Vector2(side*(.4f*(1-t)+.14f*Mathf.Sin(t*Mathf.PI)),1.02f+.86f*t);PresentationLine(root,"Canopy seam",previous,next,.018f,-.06f);previous=next;}
            }
            PrefabUtility.SaveAsPrefabAsset(root,path);PrefabUtility.UnloadPrefabContents(root);AssetDatabase.SaveAssets();
        }
    }
}

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Scrapshift.Editor
{
    public static partial class ContentBuilder
    {
        static void EnsureYardPresentation()
        {
            var scene=EditorSceneManager.OpenScene(ScenePath);
            bool hasFloor=false,hasParachute=false;
            foreach(var root in scene.GetRootGameObjects())
            {
                if(root.name=="Deck art")hasFloor=root.transform.Find("Soft yard floor")!=null;
                var game=root.GetComponent<ArenaGame>();if(game!=null)hasParachute=game.parachutePrefab!=null;
            }
            // A marker can survive a restored/older scene; validate the actual serialized result too.
            if(File.Exists(Root+"/YardPresentationV1.txt")&&hasFloor&&hasParachute)return;
            foreach(var guid in AssetDatabase.FindAssets("t:Material",new[]{Root}))
            {var m=AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));materials[m.name]=m;}
            circle=AssetDatabase.LoadAssetAtPath<Mesh>(Root+"/Meshes/Circle.asset");
            quad=AssetDatabase.LoadAssetAtPath<Mesh>(Root+"/Meshes/Quad.asset");
            YardMaterial("YardOutside","#494338");YardMaterial("YardRim","#37332B");
            YardMaterial("YardStone","#68614F");YardMaterial("YardHighlight","#807760");
            foreach(var root in scene.GetRootGameObjects())
            {
                if(root.name!="Deck art")continue;
                for(int i=root.transform.childCount-1;i>=0;i--)
                {var child=root.transform.GetChild(i);if(child.name=="Floor"||child.name.EndsWith("rail")||child.name=="Dust patch"||child.name=="Outside earth"||child.name.StartsWith("Soft ")||child.name=="Buried scrap"||child.name=="Scrap glint"||child.name=="Dry grass"||child.name=="Small stone"||child.name=="Stone highlight")Object.DestroyImmediate(child.gameObject);}
                Shape(root,"Outside earth",quad,"YardOutside",0,0,5.2f,60,50);
                var boundary=new List<Vector2>();
                // Soft corners and small irregularities; every edge stays outside the collision rectangle.
                for(int corner=0;corner<4;corner++)for(int i=0;i<48;i++)
                {
                    float angle=(corner*90+i*90f/48)*Mathf.Deg2Rad;
                    Vector2 c=new Vector2(corner==0||corner==3?9.94f:-9.94f,corner<2?7.38f:-7.38f);
                    float ripple=.004f*Mathf.Sin((corner*48+i)*.73f)+.003f*Mathf.Sin((corner*48+i)*1.41f);
                    boundary.Add(c+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*(.53f+ripple));
                }
                // Add points along the long sides so their silhouette is gently wavy too.
                var edge=new List<Vector2>();
                for(int i=0;i<boundary.Count;i++)
                {
                    var a=boundary[i];var b=boundary[(i+1)%boundary.Count];edge.Add(a);
                    if(Vector2.Distance(a,b)<1)continue;
                    int n=Mathf.CeilToInt(Vector2.Distance(a,b)/.18f);
                    for(int k=1;k<n;k++){var p=Vector2.Lerp(a,b,(float)k/n);var outward=(a+b).normalized;edge.Add(p+outward*(.025f*Mathf.Sin(k*.7f)+.02f*Mathf.Sin(k*1.67f)));}
                }
                var rim=FilledMesh("YardBoundary",edge);
                Shape(root,"Soft earth rim",rim,"YardRim",0,0,5.1f,1.012f,1.016f);
                Shape(root,"Soft yard floor",rim,"Deck",0,0,5,1,1);
                var random=new System.Random(731);
                for(int i=0;i<125;i++)
                {
                    float x=(float)random.NextDouble()*19.5f-9.75f,y=(float)random.NextDouble()*14.5f-7.25f;
                    if(i%5==0)
                    {
                        float a=(float)random.NextDouble()*180;
                        Shape(root,"Buried scrap",quad,"YardStone",x,y,4.9f,.14f,.09f,a);
                        Shape(root,"Scrap glint",quad,"YardHighlight",x-.015f,y+.02f,4.88f,.09f,.015f,a);
                    }
                    else if(i%5==1)
                    {
                        for(int k=-1;k<=1;k++)Shape(root,"Dry grass",quad,"Grid",x+k*.045f,y+.045f,4.9f,.025f,.12f-Mathf.Abs(k)*.02f,-k*26);
                    }
                    else
                    {
                        float w=.08f+(float)random.NextDouble()*.18f;
                        Shape(root,"Small stone",circle,"YardStone",x,y,4.9f,w,w*.48f);
                        Shape(root,"Stone highlight",circle,"YardHighlight",x-.008f,y+.015f,4.88f,w*.7f,w*.18f);
                    }
                }
            }
            // Original native geometry: cream scalloped canopy, dark seams and suspension ropes.
            var parachute=new GameObject("Salvage parachute");
            var silhouette=new List<Vector2>();
            for(int i=0;i<=48;i++){float a=i*Mathf.PI/48;silhouette.Add(new Vector2(Mathf.Cos(a)*.9f,1+Mathf.Sin(a)*.57f));}
            for(int i=0;i<=48;i++){float x=-.9f+i*1.8f/48;silhouette.Add(new Vector2(x,1+Mathf.Abs(Mathf.Sin(i*Mathf.PI/16))*.10f));}
            var canopy=FilledMesh("SalvageCanopy",silhouette);
            Shape(parachute,"Canopy outline",canopy,"Ink",0,0,-.03f,1.04f,1.02f);
            Shape(parachute,"Canvas",canopy,"White",0,0,-.04f,1,1);
            foreach(float x in new[]{-.87f,-.3f,.3f,.87f})
                PresentationLine(parachute,"Suspension rope",new Vector2(x,1.04f),new Vector2(x*.22f,.2f),.022f,-.01f);
            PresentationLine(parachute,"Canopy seam",new Vector2(-.3f,1.08f),new Vector2(-.18f,1.48f),.018f,-.05f);
            PresentationLine(parachute,"Canopy seam",new Vector2(.3f,1.08f),new Vector2(.18f,1.48f),.018f,-.05f);
            var prefab=PrefabUtility.SaveAsPrefabAsset(parachute,Root+"/Prefabs/SalvageParachute.prefab");Object.DestroyImmediate(parachute);
            foreach(var root in scene.GetRootGameObjects()){var game=root.GetComponent<ArenaGame>();if(game!=null){game.parachutePrefab=prefab;EditorUtility.SetDirty(game);}}
            EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            File.WriteAllText(Root+"/YardPresentationV1.txt","Original soft yard boundary, quiet debris and native geometry parachute. No reference textures copied.\n");AssetDatabase.Refresh();
        }
        static void PresentationLine(GameObject parent,string name,Vector2 a,Vector2 b,float width,float z)
        {
            Vector2 middle=(a+b)*.5f;Shape(parent,name,quad,"Ink",middle.x,middle.y,z,Vector2.Distance(a,b),width,Mathf.Atan2(b.y-a.y,b.x-a.x)*Mathf.Rad2Deg);
        }
        static void YardMaterial(string name,string color)
        {
            if(materials.ContainsKey(name))SetColor(name,color);else AddMaterial(name,color);
        }
        static Mesh FilledMesh(string name,List<Vector2> points)
        {
            var vertices=new Vector3[points.Count+1];var triangles=new int[points.Count*3];
            Vector2 center=Vector2.zero;foreach(var p in points)center+=p;vertices[0]=center/points.Count;
            for(int i=0;i<points.Count;i++){vertices[i+1]=points[i];triangles[i*3]=0;triangles[i*3+1]=i+1;triangles[i*3+2]=(i+1)%points.Count+1;}
            var mesh=new Mesh{name=name,vertices=vertices,triangles=triangles};mesh.RecalculateBounds();mesh.RecalculateNormals();
            string path=Root+"/Meshes/"+name+".asset";var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(existing!=null){EditorUtility.CopySerialized(mesh,existing);Object.DestroyImmediate(mesh);EditorUtility.SetDirty(existing);return existing;}
            AssetDatabase.CreateAsset(mesh,path);return mesh;
        }
    }
}

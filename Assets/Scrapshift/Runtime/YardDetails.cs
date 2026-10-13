using System.Collections.Generic;
using UnityEngine;
namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        void AddYardDetails(Transform parent)
        {
            // Original vector silhouettes, following the reference's quiet, scattered tile motifs.
            // One mesh for the entire ground layer, no colliders or individual update callbacks.
            var vertices=new List<Vector3>();var colors=new List<Color>();var triangles=new List<int>();
            void Polygon(Vector2 center,Vector2[] points,Color color,float scale,float z)
            {
                int start=vertices.Count;vertices.Add(new Vector3(center.x,center.y,z));colors.Add(color);
                foreach(var p in points){vertices.Add(new Vector3(center.x+p.x*scale,center.y+p.y*scale,z));colors.Add(color);}
                for(int i=0;i<points.Length;i++){triangles.Add(start);triangles.Add(start+i+1);triangles.Add(start+(i+1)%points.Length+1);}
            }
            var floor=MapThemes.Floor(selectedMap);var outline=Color.Lerp(floor,Color.black,.40f);var stone=Color.Lerp(floor,MapThemes.Accent(selectedMap),.35f);var grass=MapThemes.Accent(selectedMap);
            var rock=new[]{new Vector2(-.12f,-.035f),new Vector2(-.10f,.025f),new Vector2(-.055f,.045f),new Vector2(-.03f,.10f),new Vector2(.035f,.105f),new Vector2(.075f,.045f),new Vector2(.12f,.01f),new Vector2(.105f,-.035f),new Vector2(.02f,-.055f)};
            var random=new System.Random(8172+selectedMap*977);
            float Rand(float a,float b)=>Mathf.Lerp(a,b,(float)random.NextDouble());
            for(int i=0;i<245;i++)
            {
                var p=new Vector2(Rand(-9.6f,9.6f),Rand(-7.1f,7.1f));float size=Rand(.45f,.85f);
                if(selectedMap==3&&i%3!=0)
                {
                    var crystal=new[]{new Vector2(-.065f,-.03f),new Vector2(.06f,-.03f),new Vector2(.035f,.09f),new Vector2(0,.13f),new Vector2(-.04f,.07f)};
                    Polygon(p,crystal,outline,size,4.83f);Polygon(p,crystal,grass,size*.65f,4.82f);
                }
                else if((selectedMap==1||selectedMap==4)&&i%3!=0)
                {
                    var scrap=new[]{new Vector2(-.10f,-.015f),new Vector2(.08f,-.04f),new Vector2(.12f,.035f),new Vector2(-.04f,.05f)};
                    Polygon(p,scrap,outline,size,4.83f);Polygon(p,scrap,grass,size*.6f,4.82f);
                }
                else if(selectedMap==5&&i%4==0)
                {
                    var pool=new Vector2[18];for(int k=0;k<pool.Length;k++){float a=k*Mathf.PI*2/pool.Length;pool[k]=new Vector2(Mathf.Cos(a)*.22f,Mathf.Sin(a)*.08f);}
                    Polygon(p,pool,outline,size,4.83f);Polygon(p,pool,grass,size*.82f,4.82f);
                }
                else if(i%3==0)
                {
                    int count=i%6==0?3:1;
                    for(int j=0;j<count;j++)
                    {var at=p+new Vector2(j*.13f,0);float s=size*(j==0?1:.55f);Polygon(at,rock,outline,s,4.83f);Polygon(at+Vector2.up*.006f,rock,stone,s*.70f,4.82f);}
                }
                else
                {
                    int blades=i%3==1?3:2;
                    for(int j=0;j<blades;j++)
                    {
                        float bend=(j-(blades-1)*.5f)*.075f;
                        var blade=new[]{new Vector2(-.03f,-.025f),new Vector2(.025f,-.025f),new Vector2(bend*.5f+.035f,.045f),new Vector2(bend+.028f,.11f),new Vector2(bend+.013f,.15f),new Vector2(bend-.01f,.16f),new Vector2(bend-.028f,.135f),new Vector2(bend-.02f,.08f),new Vector2(-.035f,.025f)};
                        var at=p+new Vector2((j-1)*.035f,0);Polygon(at,blade,outline,size,4.83f);Polygon(at+Vector2.up*.009f,blade,grass,size*.64f,4.82f);
                    }
                }
            }
            var mesh=new Mesh{name="Small ground motifs",vertices=vertices.ToArray(),colors=colors.ToArray(),triangles=triangles.ToArray()};mesh.RecalculateBounds();
            var obj=new GameObject("Quiet grass and pebble clusters");obj.transform.SetParent(parent,false);obj.AddComponent<MeshFilter>().sharedMesh=mesh;obj.AddComponent<MeshRenderer>().sharedMaterial=catalog.spriteMaterial;
        }
    }
}

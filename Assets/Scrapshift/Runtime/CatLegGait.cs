using UnityEngine;
namespace Scrapshift
{
    // Deform only the lower paw regions; fixed UVs keep the drawn legs attached.
    public sealed class CatLegGait : MonoBehaviour
    {
        Mesh mesh;Material material;MeshRenderer view;SpriteRenderer source;
        Vector3[] rest,posed;float width,height,left,bottom;
        public void Initialize(SpriteRenderer sprite)
        {
            source=sprite;var s=sprite.sprite;var b=s.bounds;
            width=b.size.x;height=b.size.y;left=b.min.x;bottom=b.min.y;
            const int columns=48,rows=36;
            rest=new Vector3[(columns+1)*(rows+1)];posed=new Vector3[rest.Length];
            var uv=new Vector2[rest.Length];var colors=new Color[rest.Length];
            var triangles=new int[columns*rows*6];int t=0;
            for(int y=0;y<=rows;y++)for(int x=0;x<=columns;x++)
            {
                int i=y*(columns+1)+x;float u=x/(float)columns,v=y/(float)rows;
                rest[i]=new Vector3(left+u*width,bottom+v*height,0);
                uv[i]=new Vector2((s.rect.x+u*s.rect.width)/s.texture.width,(s.rect.y+v*s.rect.height)/s.texture.height);colors[i]=Color.white;
                if(x<columns&&y<rows){triangles[t++]=i;triangles[t++]=i+columns+1;triangles[t++]=i+1;triangles[t++]=i+1;triangles[t++]=i+columns+1;triangles[t++]=i+columns+2;}
            }
            mesh=new Mesh{name="Connected cat leg mesh"};mesh.MarkDynamic();mesh.vertices=rest;mesh.uv=uv;mesh.colors=colors;mesh.triangles=triangles;
            var surface=new GameObject("Animated leg surface");surface.transform.SetParent(transform,false);
            surface.AddComponent<MeshFilter>().sharedMesh=mesh;view=surface.AddComponent<MeshRenderer>();
            material=new Material(Shader.Find("Sprites/Default"));material.mainTexture=s.texture;view.sharedMaterial=material;
            view.sortingOrder=sprite.sortingOrder;source.enabled=false;
        }
        public static Vector2 StepOffset(float x,float y,float phase,bool moving)
        {
            if(!moving)return Vector2.zero;
            float weight=Mathf.SmoothStep(0,1,Mathf.Clamp01((.27f-y)/.22f));
            float side=x<.35f?-1:1;float step=Mathf.Sin(phase*Mathf.PI*2)*side;
            return new Vector2(step*.055f,Mathf.Max(0,step)*.02f)*weight;
        }
        public void Pose(float phase,bool moving,bool dead)
        {
            for(int i=0;i<rest.Length;i++)
            {
                var offset=StepOffset((rest[i].x-left)/width,(rest[i].y-bottom)/height,phase,moving);
                posed[i]=rest[i]+new Vector3(offset.x*width,offset.y*width,0);
            }
            mesh.vertices=posed;mesh.RecalculateBounds();material.color=dead?Color.gray:Color.white;
        }
        void OnDestroy(){if(Application.isPlaying){if(mesh!=null)Destroy(mesh);if(material!=null)Destroy(material);}else{if(mesh!=null)DestroyImmediate(mesh);if(material!=null)DestroyImmediate(material);}}
    }
}

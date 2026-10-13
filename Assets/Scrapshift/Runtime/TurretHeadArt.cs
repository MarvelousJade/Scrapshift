using UnityEngine;
namespace Scrapshift
{
    // Side-view upper assemblies; the circular rear joint is the rotation origin.
    public static class TurretHeadArt
    {
        static readonly Sprite[] heads=new Sprite[5];
        public static Sprite Get(int index)
        {
            if(heads[index]!=null)return heads[index];
            var atlas=Resources.Load<Texture2D>("TurretHeadsV2");var pixels=atlas.GetPixels32();
            float[] edges={0,.2f,.419f,.601f,.806f,1};
            int x0=(int)(edges[index]*atlas.width),x1=(int)(edges[index+1]*atlas.width);
            int l=x1,r=x0,b=atlas.height,t=0;
            for(int y=0;y<atlas.height;y++)for(int x=x0;x<x1;x++)if(pixels[y*atlas.width+x].a>32)
            {l=Mathf.Min(l,x);r=Mathf.Max(r,x+1);b=Mathf.Min(b,y);t=Mathf.Max(t,y+1);}
            heads[index]=Sprite.Create(atlas,Rect.MinMaxRect(l,b,r,t),new Vector2((new[]{.052f,.244f,.457f,.644f,.854f}[index]*atlas.width-l)/(r-l),(.47f*atlas.height-b)/(t-b)),100,0,SpriteMeshType.FullRect);
            heads[index].name="TurretHead_"+index;return heads[index];
        }
    }
}

using UnityEngine;
namespace Scrapshift
{
    // Original transparent six-by-three atlas; each complete ally gets its own silhouette.
    public static class SupportArt
    {
        static Sprite[] sprites;
        static readonly string[] structures={"turret","flame","rocket","laser","healing","landmine","garden","tyler","wandering_bot"};
        static readonly string[] pets={"bonk_dog","catling_gun","bot_o_mine","blazemander","ratzilla","doc_moth","lootworm","scapegoat","jellyshield"};
        public static Sprite Structure(string kind)=>Get(Mathf.Max(0,System.Array.IndexOf(structures,kind)));
        public static Sprite Pet(string kind)=>Get(9+Mathf.Max(0,System.Array.IndexOf(pets,kind)));
        public static void Apply(ReferenceCatalog catalog)
        {
            foreach(var item in catalog.data.items)
            {
                if(item.id=="jellyshield"){item.image=Pet(item.id);continue;}
                foreach(var effect in item.conditionalEffects??System.Array.Empty<ConditionalEffect>())
                {
                    if(effect.structure!=null)item.image=Structure(effect.structure.kind);
                    else if(effect.pet!=null)item.image=Pet(effect.pet.kind);
                }
            }
        }
        static Sprite Get(int index)
        {
            if(sprites==null)
            {
                var atlas=Resources.Load<Texture2D>("SupportAtlas");
                if(atlas==null)throw new System.InvalidOperationException("Missing support atlas");
                var pixels=atlas.GetPixels32();sprites=new Sprite[18];
                for(int i=0;i<18;i++)
                {
                    int x0=i%6*atlas.width/6,x1=(i%6+1)*atlas.width/6;
                    int y0=(2-i/6)*atlas.height/3,y1=(3-i/6)*atlas.height/3;
                    int left=x1,right=x0,bottom=y1,top=y0;
                    for(int y=y0;y<y1;y++)for(int x=x0;x<x1;x++)if(pixels[y*atlas.width+x].a>32)
                    {left=Mathf.Min(left,x);right=Mathf.Max(right,x);bottom=Mathf.Min(bottom,y);top=Mathf.Max(top,y);}
                    var rect=new Rect(left,bottom,right-left+1,top-bottom+1);
                    sprites[i]=Sprite.Create(atlas,rect,new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);
                    sprites[i].name="Support_"+(i<9?structures[i]:pets[i-9]);
                }
            }
            return sprites[index];
        }
    }
}

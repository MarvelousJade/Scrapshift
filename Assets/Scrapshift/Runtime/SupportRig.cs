using System;
using System.Collections.Generic;
using UnityEngine;
namespace Scrapshift
{
    // Presentation-only articulated parts. Simulation positions and attack clocks stay in ArenaGame.
    public sealed class SupportRig : MonoBehaviour
    {
        static readonly Dictionary<int,Sprite> parts=new Dictionary<int,Sprite>();
        static Texture2D atlas;
        Transform facing,head,body;
        readonly List<Transform> limbs=new List<Transform>();
        SpriteRenderer source,headView;
        SpriteRenderer[] views;
        string kind;float unit;
        readonly Transform[] guns=new Transform[2];
        readonly SpriteRenderer[] gunViews=new SpriteRenderer[2];
        readonly float[] gunRecoil=new float[2];
        public Transform AimingHead=>head;
        public Transform AnimatedAssembly=>facing;
        public Transform FirstLimb=>limbs.Count>0?limbs[0]:null;
        public bool IsTurret=>head!=null;
        public static SupportRig Create(GameObject root,string kind,bool turret)
        {
            int animal=Array.IndexOf(new[]{"bonk_dog","catling_gun","blazemander","ratzilla","scapegoat"},kind);
            int gun=Array.IndexOf(new[]{"turret","flame","rocket","laser","healing"},kind);
            if(turret?gun<0:animal<0&&kind!="doc_moth")return null;
            var rig=root.AddComponent<SupportRig>();rig.kind=kind;
            rig.source=root.GetComponent<SpriteRenderer>();rig.source.enabled=false;
            rig.unit=1/Mathf.Max(.0001f,root.transform.localScale.x);
            rig.facing=new GameObject("Articulation").transform;rig.facing.SetParent(root.transform,false);rig.facing.localScale=Vector3.one*rig.unit;
            if(turret)
            {
                rig.Part(6,"Fixed tripod",rig.facing,new Vector2(0,-.14f),.49f,7);
                rig.head=new GameObject("Aiming head pivot").transform;rig.head.SetParent(rig.facing,false);rig.head.localPosition=new Vector3(0,.055f,0);
                rig.headView=rig.Part(7+gun,"Gun head",rig.head,Vector2.zero,.47f,8);
                rig.headView.flipX=gun==0;
            }
            else if(kind=="doc_moth")
            {
                for(int i=0;i<4;i++)
                {
                    var pivot=new GameObject("Wing hinge "+i).transform;pivot.SetParent(rig.facing,false);
                    pivot.localPosition=new Vector3(i%2==0?-.055f:.055f,i<2?.01f:-.07f,0);
                    var wing=rig.Part(17,"Wing",pivot,Vector2.zero,i<2?.33f:.25f,6);
                    wing.transform.localScale=Vector3.Scale(wing.transform.localScale,new Vector3(i%2==0?1:-1,1,1));
                    rig.limbs.Add(pivot);
                }
                rig.body=rig.Part(16,"Moth body",rig.facing,Vector2.zero,.34f,8).transform;
            }
            else
            {
                int leg=animal==0?13:animal==2?14:animal==3?15:12;
                for(int i=0;i<(kind=="catling_gun"?0:4);i++)
                {
                    var pivot=new GameObject("Leg hip "+i).transform;pivot.SetParent(rig.facing,false);
                    pivot.localPosition=new Vector3(i%2==0?-.13f:.12f,-.09f+(i<2?.015f:0),0);
                    rig.Part(leg,"Paw",pivot,Vector2.zero,.19f,i<2?6:8);
                    rig.limbs.Add(pivot);
                }
                rig.body=rig.Part(animal,"Pet body",rig.facing,new Vector2(0,.055f),.60f,7).transform;
                if(kind=="catling_gun")
                {
                    foreach(Transform child in rig.body)DestroyImmediate(child.gameObject);
                    var view=rig.body.GetComponent<SpriteRenderer>();view.sprite=CatBody();view.enabled=true;
                    rig.body.localScale=Vector3.one*(.60f/Mathf.Max(view.sprite.bounds.size.x,view.sprite.bounds.size.y));
                }
            }
            rig.views=rig.facing.GetComponentsInChildren<SpriteRenderer>();return rig;
        }
        static Sprite catBody;
        static Sprite CatBody()
        {
            if(catBody!=null)return catBody;
            var texture=Resources.Load<Texture2D>("CatBodyV2");var pixels=texture.GetPixels32();
            int l=texture.width,b=texture.height,r=0,t=0;
            for(int y=0;y<texture.height;y++)for(int x=0;x<texture.width;x++)if(pixels[y*texture.width+x].a>32){l=Mathf.Min(l,x);b=Mathf.Min(b,y);r=Mathf.Max(r,x+1);t=Mathf.Max(t,y+1);}
            catBody=Sprite.Create(texture,Rect.MinMaxRect(l,b,r,t),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);return catBody;
        }
        public void ConfigureCatGuns(Sprite sprite)
        {
            for(int i=0;i<2;i++)
            {
                var pivot=new GameObject("Independent gun "+i).transform;pivot.SetParent(facing,false);
                pivot.localPosition=new Vector3(i==0?-.075f:.10f,i==0?-.035f:.12f,0);guns[i]=pivot;
                var view=new GameObject("Gun").AddComponent<SpriteRenderer>();view.transform.SetParent(pivot,false);
                view.sprite=sprite;view.sortingOrder=i==0?9:6;
                view.transform.localScale=Vector3.one*(.27f/Mathf.Max(sprite.bounds.size.x,sprite.bounds.size.y));
                view.transform.localPosition=Vector3.right*.075f;gunViews[i]=view;
            }
            views=facing.GetComponentsInChildren<SpriteRenderer>();
        }
        public Vector2 GunOrigin(int index)=>guns[index]!=null?guns[index].TransformPoint(.20f,0,0):transform.position;
        public Vector2 GunMount(int index)=>guns[index]!=null?guns[index].position:transform.position;
        public void AimGun(int index,Vector2 direction)
        {
            if(guns[index]==null)return;
            var local=facing.InverseTransformVector(direction);guns[index].localRotation=Quaternion.Euler(0,0,Mathf.Atan2(local.y,local.x)*Mathf.Rad2Deg);
            gunViews[index].flipY=local.x<0;
        }
        public void GunKick(int index){gunRecoil[index]=.10f;}
        public void TickGunRecoil(float dt,bool reduced)
        {
            for(int i=0;i<2;i++)if(guns[i]!=null)
            {gunRecoil[i]=Mathf.Max(0,gunRecoil[i]-dt);gunViews[i].transform.localPosition=Vector3.right*(.075f-(reduced?0:gunRecoil[i]*.3f));}
        }
        SpriteRenderer Part(int index,string name,Transform parent,Vector2 pos,float length,int order)
        {
            var go=new GameObject(name);go.transform.SetParent(parent,false);go.transform.localPosition=pos;
            var view=go.AddComponent<SpriteRenderer>();view.sprite=GetPart(index);view.sortingOrder=order;
            if(index<2)
            {
                // Separate rectangular bands avoid both cross-cell pixels and runtime mesh mutation.
                view.enabled=false;var sprite=view.sprite;var rect=sprite.rect;
                var split=new[]{Rect.MinMaxRect(index==0?0:335,atlas.height-175,index==0?335:598,atlas.height),Rect.MinMaxRect(index==0?0:297,atlas.height-360,index==0?297:598,atlas.height-175)};
                foreach(var band in split)
                {
                    var r=Rect.MinMaxRect(Mathf.Max(rect.xMin,band.xMin),Mathf.Max(rect.yMin,band.yMin),Mathf.Min(rect.xMax,band.xMax),Mathf.Min(rect.yMax,band.yMax));
                    var section=new GameObject("Body section");section.transform.SetParent(go.transform,false);
                    section.transform.localPosition=(r.center-rect.position-sprite.pivot)/100;
                    var renderer=section.AddComponent<SpriteRenderer>();renderer.sortingOrder=order;
                    renderer.sprite=Sprite.Create(atlas,r,new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);
                }
            }
            go.transform.localScale=Vector3.one*(length/Mathf.Max(view.sprite.bounds.size.x,view.sprite.bounds.size.y));return view;
        }
        public void Aim(Vector2 direction,float dt,float age,bool reduced)
        {
            if(head==null)return;
            // Reference turrets keep their sprite upright; projectile direction tracks targets.
            // Recoil moves the connected assembly, never the head away from its tripod.
            head.localRotation=Quaternion.identity;headView.flipY=false;
            head.localScale=Vector3.one;head.localPosition=new Vector3(0,.055f,0);
            var pose=reduced?Vector2.one:SupportMotion.TurretScale(age);
            facing.localScale=new Vector3(pose.x,pose.y,1)*unit;
            facing.localPosition=Vector3.up*(reduced?0:SupportMotion.TurretLift(age))*unit;
        }
        public void PetPose(float phase,bool moving,bool flip,bool reduced,bool dead)
        {
            facing.localScale=new Vector3(flip?-unit:unit,unit,unit);
            float stride=!reduced&&moving&&!dead?Mathf.Sin(phase*Mathf.PI*2):0;
            for(int i=0;i<limbs.Count;i++)
            {
                float sign=i%2==0?1:-1;
                float degrees=kind=="doc_moth"?(reduced?0:Mathf.Sin(phase*Mathf.PI*2)*30)*sign+(i<2?0:40*sign):stride*24*(i==0||i==3?1:-1);
                limbs[i].localRotation=Quaternion.Euler(0,0,degrees);
            }
            if(kind=="catling_gun")
            {
                // Reference move cycle: 2 Hz bounce and +/-1.8 degree body sway, no leg tracks.
                body.localRotation=Quaternion.Euler(0,0,reduced||!moving||dead?0:-1.8f*Mathf.Sin(phase*Mathf.PI));
            }
            if(kind=="doc_moth")body.localPosition=new Vector3(0,reduced?0:Mathf.Sin(phase*Mathf.PI/3)*.035f,0);
            foreach(var view in views){var c=dead?Color.gray:Color.white;c.a=view.color.a;view.color=c;}
        }
        public static Sprite GetPart(int index)
        {
            if(parts.TryGetValue(index,out var cached))return cached;
            if(atlas==null)atlas=Resources.Load<Texture2D>("SupportParts");
            int row=index/6,col=index%6;
            int[][] xs={new[]{0,295,598,887,1182,1478,1774},new[]{0,310,591,887,1182,1478,1774},new[]{0,295,591,887,1182,1478,1774}};
            int[] ys={0,360,590,887};var bands=new List<Rect>();
            // The dog's raised tail shares a column with the cat's lower gun; isolate with two bands.
            if(index<2){bands.Add(Rect.MinMaxRect(index==0?0:335,0,index==0?335:598,175));bands.Add(Rect.MinMaxRect(index==0?0:297,175,index==0?297:598,360));}
            else bands.Add(Rect.MinMaxRect(xs[row][col],ys[row],xs[row][col+1],ys[row+1]));
            var pixels=atlas.GetPixels32();int l=atlas.width,b=atlas.height,r=0,t=0;
            for(int i=0;i<bands.Count;i++)
            {
                var v=bands[i];v.y=atlas.height-v.yMax;bands[i]=v;
                for(int y=(int)v.yMin;y<v.yMax;y++)for(int x=(int)v.xMin;x<v.xMax;x++)if(pixels[y*atlas.width+x].a>32){l=Mathf.Min(l,x);b=Mathf.Min(b,y);r=Mathf.Max(r,x+1);t=Mathf.Max(t,y+1);}
            }
            var rect=Rect.MinMaxRect(l,b,r,t);var pivot=index>=12&&index<=15?new Vector2(.69f,.79f):index==17?new Vector2(.90f,.12f):new Vector2(.5f,.5f);
            var sprite=Sprite.Create(atlas,rect,pivot,100,0,SpriteMeshType.FullRect);sprite.name="RigPart_"+index;
            parts[index]=sprite;return sprite;
        }
    }
}

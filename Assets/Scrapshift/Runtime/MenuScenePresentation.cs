using UnityEngine;

namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        RenderTexture menuGround;
        Texture2D titleBackdrop,titleHero;
        float menuPoseTime=-1;
        float MenuTime=>reducedMotion?0:menuPoseTime>=0?menuPoseTime:Time.unscaledTime;

        // Capture only the existing yard, never actors, pickups or interface icons.
        void PrepareMenuGround()
        {
            if(menuGround!=null||!yardPolished||Event.current.type!=EventType.Repaint)return;
            var yard=GameObject.Find("Deck art");if(yard==null)return;
            var nodes=yard.GetComponentsInChildren<Transform>(true);
            var layers=new int[nodes.Length];
            var go=new GameObject("Menu ground capture");var camera=go.AddComponent<Camera>();
            camera.enabled=false;camera.orthographic=true;camera.orthographicSize=3.8f;
            camera.aspect=16f/10;camera.transform.position=new Vector3(0,0,-20);
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.43f,.40f,.34f);
            camera.cullingMask=1<<31;
            menuGround=new RenderTexture(1600,1000,16){name="Zoomed salvage yard",filterMode=FilterMode.Bilinear};
            try
            {
                for(int i=0;i<nodes.Length;i++){layers[i]=nodes[i].gameObject.layer;nodes[i].gameObject.layer=31;}
                camera.targetTexture=menuGround;camera.Render();
            }
            finally
            {
                for(int i=0;i<nodes.Length;i++)nodes[i].gameObject.layer=layers[i];
                camera.targetTexture=null;Destroy(go);
            }
        }
        void DrawMenuGround(Rect rect,bool close=false)
        {
            PrepareMenuGround();
            Box(rect,new Color(.34f,.32f,.27f));
            if(menuGround!=null)
            {
                // Keep terrain proportions, with a closer crop inside the portrait card.
                float h=close?.48f:1f;float w=h*rect.width/rect.height/menuGround.width*menuGround.height;
                if(w>1){h/=w;w=1;}
                GUI.DrawTextureWithTexCoords(rect,menuGround,new Rect((1-w)*.5f,(1-h)*.5f,w,h));
            }
            Box(rect,new Color(0,0,0,close?.16f:.28f));
        }
        void AnimatedMenuPortrait(Vector2 origin,float scale)
        {
            var old=GUI.color;GUI.color=new Color(0,0,0,.30f);
            DrawIcon(catalog.shadow,new Rect(origin.x-scale*.44f,origin.y+scale*.42f,scale*.88f,scale*.20f));GUI.color=old;
            // The same idle pose as SpriteFrameAnimator: body spring with small matching leg flex.
            Portrait(origin,scale,playerAnimation.Look,false,MenuTime);
        }
        void TitleScene()
        {
            if(titleBackdrop==null)titleBackdrop=Resources.Load<Texture2D>("Menu/TitlePlate");
            if(titleHero==null)titleHero=Resources.Load<Texture2D>("Menu/TitleHero");
            if(titleBackdrop==null){DrawMenuGround(FullCanvas);return;}
            // All layers share the same aspect-preserving crop, including ultrawide screens.
            float scale=Mathf.Max(FullCanvas.width/1672,FullCanvas.height/941);
            var scene=new Rect(FullCanvas.center.x-836*scale,FullCanvas.center.y-470.5f*scale,1672*scale,941*scale);
            GUI.DrawTexture(scene,titleBackdrop,ScaleMode.StretchToFill);
            MenuEnemy(scene,"horned_bruiser",new Rect(65,20,285,310),0);
            MenuEnemy(scene,"spitter",new Rect(1470,35,225,270),1);
            MenuEnemy(scene,"helmet_alien",new Rect(100,330,365,300),2);
            MenuEnemy(scene,"charger",new Rect(550,335,280,275),3);
            MenuEnemy(scene,"horned_bruiser",new Rect(1260,325,280,270),4);
            MenuEnemy(scene,"helmet_alien",new Rect(1400,410,365,355),5);
            MenuEnemy(scene,"horned_bruiser",new Rect(1380,710,360,350),6);
            RoundedBox(MenuRect(scene,new Rect(810,756,400,40)),new Color(0,0,0,.30f),20*scale);
            if(titleHero!=null)
            {
                var pose=SpringMotion.Pose(MenuTime*.5f,false,reducedMotion);
                // Gameplay spring curve at half speed: a relaxed two-second title bounce.
                float sx=1+(pose.x-1)*.3f,sy=1+(pose.y-1)*.3f;
                var rect=new Rect(1000-410*sx,823-460*sy+pose.z*65,820*sx,460*sy);
                GUI.DrawTexture(MenuRect(scene,rect),titleHero,ScaleMode.ScaleToFit,true);
            }
        }
        static Rect MenuRect(Rect scene,Rect local)=>new Rect(scene.x+local.x*scene.width/1672,scene.y+local.y*scene.height/941,local.width*scene.width/1672,local.height*scene.height/941);
        void MenuEnemy(Rect scene,string id,Rect local,int index)
        {
            var rect=MenuRect(scene,local);var matrix=GUI.matrix;var color=GUI.color;
            GUIUtility.RotateAroundPivot(reducedMotion?0:Mathf.Sin(MenuTime*.85f+index*.72f)*4,rect.center);
            GUI.color=new Color(.45f,.34f,.51f,.52f);DrawIcon(catalog.Enemy(id).image,rect);
            GUI.matrix=matrix;GUI.color=color;
        }
        void OnDestroy()
        {
            if(menuGround!=null){menuGround.Release();Destroy(menuGround);}
            if(mittenMesh!=null)Destroy(mittenMesh);
        }
    }
}

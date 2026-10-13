using UnityEngine;

namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        RenderTexture menuGround;
        Texture2D titleBackdrop;
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
            // The same idle pose as SpriteFrameAnimator: fixed feet, body-only spring.
            Portrait(origin,scale,playerAnimation.Look,false,MenuTime);
        }
        void TitleScene()
        {
            if(titleBackdrop==null)titleBackdrop=Resources.Load<Texture2D>("Menu/SalvageTitleEquipped");
            if(titleBackdrop!=null)GUI.DrawTexture(FullCanvas,titleBackdrop,ScaleMode.ScaleAndCrop);else DrawMenuGround(FullCanvas);
        }
        void OnDestroy()
        {
            if(menuGround!=null){menuGround.Release();Destroy(menuGround);}
            if(mittenMesh!=null)Destroy(mittenMesh);
        }
    }
}

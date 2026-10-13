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
        void AnimatedMenuPortrait(Vector2 origin,float scale,bool armed=false)
        {
            float time=MenuTime;
            float stunt=Mathf.Clamp01((Mathf.Repeat(time,7)-5.8f)/1.2f);
            float jump=Mathf.Sin(stunt*Mathf.PI);
            float bounce=reducedMotion?0:Mathf.Abs(Mathf.Sin(time*Mathf.PI/ .95f))*scale*.035f;
            RoundedBox(new Rect(origin.x-scale*.44f,origin.y+scale*.47f,scale*.88f,scale*.16f),new Color(0,0,0,.28f),scale*.08f);
            var center=origin+Vector2.up*(-bounce-jump*scale*.24f);
            var before=GUI.matrix;
            float angle=stunt>0?Mathf.SmoothStep(0,360,stunt):0;

            float squash=reducedMotion?0:Mathf.Cos(time*Mathf.PI*2/.95f)*.025f;
            GUI.matrix=before*Matrix4x4.TRS(center,Quaternion.Euler(0,0,angle),new Vector3(1+squash,1-squash,1))*Matrix4x4.Translate(-center);
            Portrait(center,scale,playerAnimation.Look,false,time);
            if(armed)
            {
                foreach(int side in new[]{-1,1})
                {
                    var weapon=catalog.Weapon(side<0?"smg":"pistol",1);
                    var at=center+new Vector2(side*scale*.60f,-scale*.09f);
                    var pose=GUI.matrix;
                    GUI.matrix=pose*Matrix4x4.TRS(at,Quaternion.Euler(0,0,(side<0?-145:-35)+Mathf.Sin(time*3)*4),Vector3.one)*Matrix4x4.Translate(-at);
                    DrawIcon(weapon?.image,new Rect(at.x-scale*.25f,at.y-scale*.15f,scale*.5f,scale*.3f));GUI.matrix=pose;
                }
            }
            GUI.matrix=before;
        }
        void TitleScene()
        {
            if(titleBackdrop==null)titleBackdrop=Resources.Load<Texture2D>("Menu/SalvageTitle");
            if(titleBackdrop!=null)GUI.DrawTexture(FullCanvas,titleBackdrop,ScaleMode.ScaleAndCrop);else DrawMenuGround(FullCanvas);
            float time=MenuTime;
            var old=GUI.color;
            for(int i=0;i<8;i++)
            {
                float a=i*Mathf.PI*2/8+time*.025f;
                var center=new Vector2(790+Mathf.Cos(a)*660,525+Mathf.Sin(a)*365);
                GUI.color=new Color(.60f,.57f,.68f,.18f);
                var matrix=GUI.matrix;GUI.matrix=matrix*Matrix4x4.TRS(center,Quaternion.Euler(0,0,Mathf.Sin(time*.5f+i)*9),Vector3.one)*Matrix4x4.Translate(-center);
                DrawIcon(catalog.data.enemies[i%catalog.data.enemies.Length].image,new Rect(center.x-95,center.y-95,190,190));GUI.matrix=matrix;
            }
            GUI.color=old;
            AnimatedMenuPortrait(new Vector2(790,570),265,true);
        }
        void OnDestroy()
        {
            if(menuGround!=null){menuGround.Release();Destroy(menuGround);}
        }
    }
}

using UnityEngine;
using System.Collections.Generic;
namespace Scrapshift
{
    public static class PresentationPolish { public static Color BodyTint=>Color.white; }
    public sealed partial class ArenaGame
    {
        Rect lastUiHover;int optionsTab;float stepClock;bool yardPolished;float smokeClock;
        sealed class Dust { public LineRenderer line;public SpriteRenderer smoke;public GameObject root;public Vector2 pos;public float age,rotation,scale=1;public Vector2 velocity;public bool pickup; }
        Sprite smokeSprite;
        readonly List<Dust> footDust=new List<Dust>();
        void TickFootsteps(Vector2 movement,float dt)
        {
            if(movement.sqrMagnitude<.01f){stepClock=smokeClock=0;return;}
            smokeClock-=dt;if(smokeClock<=0){smokeClock=.125f*UnityEngine.Random.Range(.65f,1.35f);if(!reducedMotion)AddFootDust(playerPos+new Vector2(UnityEngine.Random.Range(-.1f,.1f),-.24f),false);}
            stepClock-=dt;if(stepClock>0)return;stepClock=.28f;
            sound.Play(SoundCue.Step,playerPos);

        }
        void AddFootDust(Vector2 pos,bool pickup)
        {
            if(footDust.Count>=32)return;
            var root=new GameObject(pickup?"Material contact":"Foot dust");root.transform.position=new Vector3(pos.x,pos.y,-2);
            if(!pickup)
            {
                if(smokeSprite==null)
                {
                    var texture=new Texture2D(64,64,TextureFormat.RGBA32,false){filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};var colors=new Color[4096];
                    for(int y=0;y<64;y++)for(int x=0;x<64;x++)
                    {float r=Vector2.Distance(new Vector2(x,y),new Vector2(31.5f,31.5f))/31.5f;float angle=Mathf.Atan2(y-31.5f,x-31.5f);float edge=.73f+.14f*Mathf.Sin(angle*7)+.05f*Mathf.Sin(angle*11);float a=Mathf.SmoothStep(1,0,Mathf.InverseLerp(edge-.06f,edge,r));colors[y*64+x]=new Color(.44f,.45f,.43f,a*.8f);}
                    texture.SetPixels(colors);texture.Apply();smokeSprite=Sprite.Create(texture,new Rect(0,0,64,64),Vector2.one*.5f,140);
                }
                var sprite=root.AddComponent<SpriteRenderer>();sprite.sprite=smokeSprite;sprite.sharedMaterial=catalog.spriteMaterial;sprite.sortingOrder=2;
                root.transform.localScale=new Vector3(1,.7f,1);footDust.Add(new Dust{smoke=sprite,root=root,pos=pos,velocity=UnityEngine.Random.insideUnitCircle*.3f,rotation=UnityEngine.Random.Range(194f,1246f),scale=UnityEngine.Random.Range(.75f,1.25f)});return;
            }
            var line=MakeLine("Puff",root.transform,pickup?mint:new Color(.6f,.65f,.7f,.25f),pickup?.025f:.13f,true);
            line.sortingOrder=pickup?15:1;line.positionCount=12;
            for(int i=0;i<12;i++){float a=i*Mathf.PI*2/12;line.SetPosition(i,new Vector3(Mathf.Cos(a)*.11f,Mathf.Sin(a)*.055f,0));}
            footDust.Add(new Dust{line=line,root=root,pos=pos,pickup=pickup});
        }
        void ClearFootDust(){foreach(var d in footDust)if(d.root!=null)Destroy(d.root);footDust.Clear();stepClock=0;}
        void PickupContactEffect(){AddFootDust(playerPos,true);}
        void RemoveWaveEnemies()
        {
            births.Clear();foreach(var e in enemies.ToArray())if(e.type.role!="Tree"||e.marker!=null){Destroy(e.obj);if(e.marker!=null)Destroy(e.marker);enemies.Remove(e);}
            foreach(var shot in shots)Destroy(shot.obj);shots.Clear();
        }
        void TickAmbientPresentation()
        {
            if(!yardPolished){PolishYard();yardPolished=true;}
            float dt=Mathf.Min(Time.deltaTime,.05f);
            bool intermission=Rules.Phase==RunPhase.Shop||Rules.Phase==RunPhase.Loot||Rules.Phase==RunPhase.Upgrade||waveSweeping;
            if(intermission&&!inspectionReview)
            {
                playerAnimation.Tick(Vector2.zero,dt,reducedMotion);player.SetActive(true);
                foreach(var e in enemies)if(e.type.role=="Tree"&&e.marker==null){e.activeAge+=dt;UpdateEnemyPresentation(e,Vector2.zero,dt);}
            }
            if(Rules.Phase==RunPhase.Paused||inspectionReview)return;
            for(int i=footDust.Count-1;i>=0;i--)
            {
                var d=footDust[i];d.age+=dt;float t=d.age/(d.pickup?.45f:.75f);
                var c=d.pickup?mint:Color.white;c.a*=1-t;if(d.line!=null)d.line.startColor=d.line.endColor=c;else {d.smoke.color=Color.white;d.pos+=d.velocity*dt;d.root.transform.position=(Vector3)d.pos+Vector3.back*2;d.root.transform.Rotate(0,0,d.rotation*dt);}
                d.root.transform.localScale=Vector3.one*(d.pickup?1+t*1.7f:Mathf.Max(0,1-t)*d.scale);
                if(t>=1){Destroy(d.root);footDust.RemoveAt(i);}
            }
        }
        void PolishYard()
        {
            var yard=GameObject.Find("Deck art");if(yard==null)return;
            var mesh=new Mesh{name="Vertical exterior gradient"};
            var vertices=new List<Vector3>();var colors=new List<Color>();var triangles=new List<int>();
            float[] levels={-25,-ArenaGeometry.HalfSize.y,ArenaGeometry.HalfSize.y,25};
            for(int i=0;i<4;i++)
            {
                vertices.Add(new Vector3(-30,levels[i],5.15f));vertices.Add(new Vector3(30,levels[i],5.15f));
                var color=i<2?new Color(.24f,.13f,.33f):new Color(.015f,.025f,.055f);colors.Add(color);colors.Add(color);
                if(i<3){int v=i*2;triangles.AddRange(new[]{v,v+2,v+1,v+1,v+2,v+3});}
            }
            mesh.vertices=vertices.ToArray();mesh.triangles=triangles.ToArray();mesh.uv=new Vector2[8];mesh.colors=colors.ToArray();
            var obj=new GameObject("Dark blue to violet exterior");obj.transform.SetParent(yard.transform,false);obj.AddComponent<MeshFilter>().sharedMesh=mesh;obj.AddComponent<MeshRenderer>().sharedMaterial=catalog.spriteMaterial;
            foreach(Transform child in yard.transform)
            {
                if(child.name=="Stone highlight")child.gameObject.SetActive(false);
                if(child.name!="Small stone")continue;
                child.gameObject.SetActive(false);
                var plate=new GameObject("Broken machine plate");plate.transform.position=child.position+new Vector3(0,0,-.05f);plate.transform.SetParent(yard.transform,true);
                var line=MakeLine("Plate edge",plate.transform,new Color(.43f,.46f,.44f),.025f,true);line.positionCount=5;line.sortingOrder=-20;
                line.SetPositions(new[]{new Vector3(-.09f,-.04f,0),new Vector3(.05f,-.06f,0),new Vector3(.11f,.01f,0),new Vector3(.04f,.065f,0),new Vector3(-.08f,.04f,0)});
                plate.transform.rotation=Quaternion.Euler(0,0,child.position.x*27);
            }
        }
        void DrawTabbedOptions(bool paused)
        {
            Text(420,100,600,75,"Options",new GUIStyle(title){fontSize=58,alignment=TextAnchor.MiddleCenter});
            string[] tabs={"Gameplay","Audio","Video","Controls"};
            for(int i=0;i<tabs.Length;i++)if(MenuButton(new Rect(235+i*245,218,235,54),tabs[i],optionsTab==i))optionsTab=i;
            RoundedBox(new Rect(235,285,970,430),panelColor,8);
            if(optionsTab==0)
            {
                if(MenuButton(new Rect(390,345,660,60),reducedMotion?"Reduced motion: On":"Reduced motion: Off")){reducedMotion=!reducedMotion;PlayerPrefs.SetInt("reducedMotion",reducedMotion?1:0);}
                Text(390,435,660,100,"Automatic attacks\n20 waves",center);
            }
            else if(optionsTab==1)
            {
                if(MenuButton(new Rect(390,340,660,60),muted?"Sound effects: Off":"Sound effects: On"))ToggleSound();
                Text(390,445,660,40,$"Effects volume  {sound.Volume*100:0}%",center);
                float v=GUI.HorizontalSlider(new Rect(420,510,600,30),sound.Volume,0,1);if(Mathf.Abs(v-sound.Volume)>.001f)sound.SetVolume(v);
            }
            else if(optionsTab==2)
            {
                if(MenuButton(new Rect(390,345,660,60),DisplayLabel))ToggleFullscreen();
                Text(390,455,660,40,Screen.width+" × "+Screen.height,center);
            }
            else Text(390,335,660,290,"Move: WASD / Arrow keys\nPause / Back: Escape\nConfirm: Enter\nInspect: Tab / Shift+Tab\nWeapon actions: Left / Right click\nFullscreen: F11",center);
            if(MenuButton(new Rect(470,760,500,60),"Back")){if(paused)pauseOptions=false;else menuPage=0;}
        }
    }
}

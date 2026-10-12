using UnityEngine;
using System.Collections.Generic;
namespace Scrapshift
{
    public static class PresentationPolish { public static Color BodyTint=>Color.white; }
    public sealed partial class ArenaGame
    {
        Rect lastUiHover;int optionsTab;float stepClock;bool yardPolished;
        sealed class Dust { public LineRenderer line;public Vector2 pos;public float age;public bool pickup; }
        readonly List<Dust> footDust=new List<Dust>();
        void TickFootsteps(Vector2 movement,float dt)
        {
            if(movement.sqrMagnitude<.01f){stepClock=0;return;}
            stepClock-=dt;if(stepClock>0)return;stepClock=.28f;
            sound.Play(SoundCue.Step,playerPos);
            if(!reducedMotion)AddFootDust(playerPos-new Vector2(movement.x,movement.y).normalized*.15f,false);
        }
        void AddFootDust(Vector2 pos,bool pickup)
        {
            if(footDust.Count>=32)return;
            var root=new GameObject(pickup?"Material contact":"Foot dust");root.transform.position=new Vector3(pos.x,pos.y,-2);
            var line=MakeLine("Puff",root.transform,pickup?mint:new Color(.6f,.65f,.7f,.25f),pickup?.025f:.13f,true);
            line.sortingOrder=pickup?15:1;line.positionCount=12;
            for(int i=0;i<12;i++){float a=i*Mathf.PI*2/12;line.SetPosition(i,new Vector3(Mathf.Cos(a)*.11f,Mathf.Sin(a)*.055f,0));}
            footDust.Add(new Dust{line=line,pos=pos,pickup=pickup});
        }
        void ClearFootDust(){foreach(var d in footDust)if(d.line!=null)Destroy(d.line.transform.parent.gameObject);footDust.Clear();stepClock=0;}
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
                var d=footDust[i];d.age+=dt;float t=d.age/.45f;
                var c=d.pickup?mint:new Color(.6f,.65f,.7f,.25f);c.a*=1-t;d.line.startColor=d.line.endColor=c;
                d.line.transform.parent.localScale=Vector3.one*(1+t*(d.pickup?1.7f:1.1f));
                if(t>=1){Destroy(d.line.transform.parent.gameObject);footDust.RemoveAt(i);}
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
                Text(390,435,660,100,"Automatic attacks\n20 waves Â· Danger 0",center);
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
                Text(390,455,660,40,Screen.width+" Ã— "+Screen.height,center);
            }
            else Text(390,335,660,290,"Move: WASD / Arrow keys\nPause / Back: Escape\nConfirm: Enter\nInspect: Tab / Shift+Tab\nWeapon actions: Left / Right click\nFullscreen: F11",center);
            if(MenuButton(new Rect(470,760,500,60),"Back")){if(paused)pauseOptions=false;else menuPage=0;}
        }
    }
}

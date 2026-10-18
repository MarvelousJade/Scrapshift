using UnityEngine;
using System.Collections.Generic;
namespace Scrapshift
{
    public static class PresentationPolish { public static Color BodyTint=>Color.white; }
    // Reference player_move.tres: a .6-second loop with contacts at .1 and .4.
    // Advance animation time so speed changes retain phase instead of restarting a timer.
    public sealed class FootstepTiming
    {
        double elapsed,next=.1;
        public void Reset(){elapsed=0;next=.1;}
        public int Tick(bool moving,float dt,float moveSpeed)
        {
            if(!moving){Reset();return 0;}
            elapsed+=System.Math.Max(0,dt)*System.Math.Max(0,moveSpeed)/4.5;
            int contacts=0;
            while(elapsed>=next){contacts++;next+=.3;}
            return contacts;
        }
    }
    public sealed partial class ArenaGame
    {
        static readonly Color scrapPickupGreen=new Color32(157,249,42,255);
        Rect lastUiHover;int optionsTab;bool yardPolished;float smokeClock;
        readonly FootstepTiming footstepTiming=new FootstepTiming();
        sealed class Dust { public LineRenderer line;public SpriteRenderer smoke;public GameObject root;public Vector2 pos;public float age,rotation,scale=1;public Vector2 velocity;public bool pickup; }
        Sprite smokeSprite;
        readonly List<Dust> footDust=new List<Dust>();
        void TickFootsteps(Vector2 movement,float dt)
        {
            bool moving=movement.sqrMagnitude>=.01f;
            int contacts=footstepTiming.Tick(moving,dt,Rules.MoveSpeed);
            if(!moving){smokeClock=0;return;}
            smokeClock-=dt;if(smokeClock<=0){smokeClock=.125f*UnityEngine.Random.Range(.65f,1.35f);if(!reducedMotion)AddFootDust(playerPos+new Vector2(UnityEngine.Random.Range(-.1f,.1f),-.24f),false);}
            for(int i=0;i<contacts;i++)sound.Play(SoundCue.Step,playerPos);

        }
        void AddFootDust(Vector2 pos,bool pickup)
        {
            if(!GameSettings.Flag("effects")||footDust.Count>=32)return;
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
            var line=MakeLine("Puff",root.transform,pickup?scrapPickupGreen:new Color(.6f,.65f,.7f,.25f),pickup?.025f:.13f,true);
            line.sortingOrder=pickup?15:1;line.positionCount=12;
            for(int i=0;i<12;i++){float a=i*Mathf.PI*2/12;line.SetPosition(i,new Vector3(Mathf.Cos(a)*.11f,Mathf.Sin(a)*.055f,0));}
            footDust.Add(new Dust{line=line,root=root,pos=pos,pickup=pickup});
        }
        void ClearFootDust(){foreach(var d in footDust)if(d.root!=null)Destroy(d.root);footDust.Clear();footstepTiming.Reset();}
        void PickupContactEffect()
        {
            if(!GameSettings.Flag("effects")||reducedMotion||footDust.Count>56)return;
            for(int i=0;i<6;i++)
            {
                var root=new GameObject("Material pickup particle");
                var p=playerPos+UnityEngine.Random.insideUnitCircle*.15f;root.transform.position=(Vector3)p+Vector3.back*2;
                var line=MakeLine("Spark",root.transform,scrapPickupGreen,.08f);line.sortingOrder=15;line.numCapVertices=4;line.positionCount=2;
                line.SetPosition(0,new Vector3(-.025f,0,0));line.SetPosition(1,new Vector3(.025f,0,0));
                footDust.Add(new Dust{root=root,line=line,pos=p,pickup=true,velocity=UnityEngine.Random.insideUnitCircle.normalized*UnityEngine.Random.Range(3f,6f)});
            }
        }
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
                var d=footDust[i];d.age+=dt;float t=d.age/(d.pickup?.3f:.75f);
                var c=d.pickup?scrapPickupGreen:Color.white;c.a*=1-t;if(d.line!=null)d.line.startColor=d.line.endColor=c;else {d.smoke.color=Color.white;d.pos+=d.velocity*dt;d.root.transform.position=(Vector3)d.pos+Vector3.back*2;d.root.transform.Rotate(0,0,d.rotation*dt);}
                if(d.pickup){d.pos+=d.velocity*dt;d.root.transform.position=(Vector3)d.pos+Vector3.back*2;}
                d.root.transform.localScale=Vector3.one*Mathf.Max(0,1-t)*d.scale;
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
                // Keep older saved scenes free of the removed decorative stones too.
                if(child.name=="Small stone"||child.name=="Stone highlight"||child.name=="Small faceted stone"||child.name=="Buried scrap"||child.name=="Scrap glint"||child.name=="Dry grass")child.gameObject.SetActive(false);
            }
            mapChoice=Mathf.Clamp((int)GameSettings.Value("mapTheme",0),-1,MapThemes.Names.Length-1);
            ApplyMapTheme(loadoutShowcase>=0?ShowcaseLoadouts.All[loadoutShowcase].map:
                mapChoice<0?new System.Random().Next(MapThemes.Names.Length):mapChoice);
        }
    }
}

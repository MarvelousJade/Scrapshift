using System;
using UnityEngine;

namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        public GameObject parachutePrefab;
        GameObject parachute;
        float arrivalAge=ArrivalMotion.Duration;
        bool arrivalImpact, endSummary;
        int menuPage, startingWeapon;
        EnemyEntry lastAttacker;
        Vector2 summaryScroll;
        static readonly string[] startingWeapons={"pistol","smg","double_barrel_shotgun","laser_gun","knife","spear"};
        bool Arriving => arrivalAge<ArrivalMotion.Duration;

        void StartSelectedRun()
        {
            Begin();Rules.Inventory.Clear();Rules.Inventory.Add(catalog.Weapon(startingWeapons[startingWeapon],1));SyncEquipment();
            arrivalAge=0;arrivalImpact=false;player.SetActive(true);
            if(parachutePrefab!=null)parachute=Instantiate(parachutePrefab);
            gameCamera.transform.position=Vector3.back*20;PoseArrival(0);
        }
        void PoseArrival(float time)
        {
            Vector2 offset=ArrivalMotion.Offset(time);if(reducedMotion)offset=Vector2.zero;
            player.SetActive(true);playerAnimation.Tick(Vector2.zero,0,true);
            player.transform.position=(Vector3)(playerPos+offset)+Vector3.back;
            var scale=reducedMotion?Vector2.one:ArrivalMotion.BodyScale(time);
            player.transform.localScale=new Vector3(scale.x,scale.y,1)*ActorPresentation.PlayerScale;
            playerShadow.Place(playerPos,ActorPresentation.PlayerScale,Mathf.InverseLerp(.95f,1.1f,time)*.392157f);
            UpdateEquipment(0);
            foreach(var view in equipmentViews)if(view!=null)view.transform.position+=(Vector3)offset;
            if(parachute!=null)
            {
                parachute.transform.position=(Vector3)(playerPos+offset)+Vector3.back*3;
                Vector2 canopy=ArrivalMotion.CanopyScale(time);parachute.transform.localScale=new Vector3(canopy.x,canopy.y,1);
                parachute.transform.rotation=Quaternion.Euler(0,0,reducedMotion?0:Mathf.Sin(time*Mathf.PI*2)*8);
            }
        }
        void TickArrival(float dt)
        {
            arrivalAge=Mathf.Min(ArrivalMotion.Duration,arrivalAge+dt);PoseArrival(arrivalAge);
            if(!arrivalImpact&&arrivalAge>=1.1f){arrivalImpact=true;sound.Play(SoundCue.Install);if(!reducedMotion)Burst(playerPos+Vector2.down*.3f,8,new Color(.68f,.63f,.48f));}
            if(!Arriving){if(parachute!=null)Destroy(parachute);player.transform.localScale=Vector3.one*ActorPresentation.PlayerScale;}
        }
        void ReturnToSelection(int page)
        {
            ClearCombat();if(parachute!=null)Destroy(parachute);arrivalAge=ArrivalMotion.Duration;
            Rules.ReturnToTitle();menuPage=page;endSummary=false;player.SetActive(false);sound.StopAll();
        }
        void ConfirmMenu()
        {
            if(Rules.Phase==RunPhase.Title){if(menuPage<3)menuPage++;else if(menuPage==3)StartSelectedRun();}
            else if(!endSummary)endSummary=true;else StartSelectedRun();
        }
        static void RoundedBox(Rect r,Color color,float radius=12) => GUI.DrawTexture(r,Texture2D.whiteTexture,ScaleMode.StretchToFill,true,0,color,0,radius);
        bool MenuButton(Rect r,string label,bool selected=false)
        {
            bool hover=r.Contains(Event.current.mousePosition);
            RoundedBox(new Rect(r.x+3,r.y+5,r.width,r.height),new Color(0,0,0,.45f));
            RoundedBox(r,selected?paper:ink);
            RoundedBox(new Rect(r.x+3,r.y+3,r.width-6,r.height-6),hover?new Color(.27f,.25f,.21f):panelColor,9);
            return GUI.Button(r,label,center);
        }
        void Portrait(Vector2 origin,float scale,PlayerLook look,bool gear=false)
        {
            var layers=AppearanceResolver.Resolve(look,gear?Rules.Items:Array.Empty<ItemEntry>());
            foreach(var layer in layers)if(layer.order<10)DrawAppearance(layer,origin,scale);
            if(playerAnimation.footSprite!=null)foreach(int side in new[]{-1,1})
            {
                var sprite=playerAnimation.footSprite;Vector2 size=sprite.bounds.size*(SpringMotion.LegHeight/sprite.bounds.size.y)*scale;
                Vector2 p=origin+new Vector2(side*SpringMotion.LegSpread,-SpringMotion.LegY)*scale;
                var before=GUI.matrix;GUIUtility.ScaleAroundPivot(new Vector2(side<0?-SpringMotion.LegWidth:SpringMotion.LegWidth,1),p);
                DrawIcon(sprite,new Rect(p-size*.5f,size));GUI.matrix=before;
            }
            var hero=playerAnimation.idleFrames[0];Vector2 bodySize=(Vector2)hero.bounds.size*scale;
            DrawIcon(hero,new Rect(origin-bodySize*.5f,bodySize));
            foreach(var layer in layers)if(layer.order>=10)DrawAppearance(layer,origin,scale);
        }
        void SelectionUI()
        {
            Overlay();
            if(menuPage==4)
            {
                Text(420,100,600,75,"Options",new GUIStyle(title){fontSize=58,alignment=TextAnchor.MiddleCenter});
                if(MenuButton(new Rect(470,285,500,65),muted?"Sound: Off":"Sound: On"))ToggleSound();
                if(MenuButton(new Rect(470,375,500,65),reducedMotion?"Reduced motion: On":"Reduced motion: Off")){reducedMotion=!reducedMotion;PlayerPrefs.SetInt("reducedMotion",reducedMotion?1:0);}
                Text(470,477,500,40,$"Effects volume  {sound.Volume*100:0}%",center);
                float volume=GUI.HorizontalSlider(new Rect(490,543,460,30),sound.Volume,0,1);if(Mathf.Abs(volume-sound.Volume)>.001f)sound.SetVolume(volume);
                if(MenuButton(new Rect(50,805,235,55),"Back"))menuPage=0;return;
            }
            Text(200,40,1040,85,menuPage==1?"Character Selection":menuPage==2?"Weapon Selection":"Run Setup",new GUIStyle(title){fontSize=56,alignment=TextAnchor.MiddleCenter});
            float cardX=menuPage==1?500:310;
            RoundedBox(new Rect(cardX,160,440,410),panelColor);
            Text(cardX+20,182,400,45,playerAnimation.Look?.name??"Scavenger",new GUIStyle(heading){alignment=TextAnchor.MiddleCenter});
            Portrait(new Vector2(cardX+220,345),140,playerAnimation.Look);
            Text(cardX+30,467,380,80,"10 Max HP\nCosmetic look · same starting stats",center);
            if(menuPage>=2)
            {
                var w=catalog.Weapon(startingWeapons[startingWeapon],1);
                RoundedBox(new Rect(790,160,380,410),panelColor);Text(815,182,330,50,w.id=="smg"?"SMG":w.title,new GUIStyle(heading){alignment=TextAnchor.MiddleCenter});
                DrawIcon(w.image,new Rect(890,259,180,125));Text(825,419,310,120,w.Detail,center);
            }
            if(menuPage==1)
            {
                var looks=playerAnimation.looks;float first=720-looks.Length*53;
                for(int i=0;i<looks.Length;i++){Rect r=new Rect(first+i*106,620,94,100);if(MenuButton(r,"",playerAnimation.lookIndex==i)){ChangeLook(i-playerAnimation.lookIndex);}Portrait(r.center+Vector2.up*3,61,looks[i]);}
            }
            else if(menuPage==2)
            {
                for(int i=0;i<startingWeapons.Length;i++){Rect r=new Rect(390+i*110,620,98,100);if(MenuButton(r,"",i==startingWeapon))startingWeapon=i;DrawIcon(catalog.Weapon(startingWeapons[i],1).image,new Rect(r.x+10,r.y+15,78,70));}
            }
            else
            {
                RoundedBox(new Rect(390,610,660,120),panelColor);Text(415,625,610,40,"Standard · 20 Waves",heading);Text(415,677,610,35,"Auto aim and fire · WASD / arrows to move",body);
            }
            if(MenuButton(new Rect(50,805,235,55),"Back"))menuPage--;
            if(MenuButton(new Rect(1080,805,310,55),menuPage==3?"Start Run  [Enter]":"Continue  [Enter]"))ConfirmMenu();
        }
        void ReferenceTitleUI()
        {
            if(menuPage!=0){SelectionUI();return;}
            // Background uses our own enemies as a subdued illustrated crowd.
            var old=GUI.color;GUI.color=new Color(.4f,.36f,.29f,.62f);
            for(int i=0;i<7;i++)DrawIcon(catalog.data.enemies[i].image,new Rect(550+i%4*220,430+i/4*170,235,240));GUI.color=old;
            var brand=new GUIStyle(title){fontSize=142,alignment=TextAnchor.MiddleCenter};
            OutlinedText(125,72,1190,175,"SCRAPSHIFT",brand);
            Text(300,245,840,42,"THE SALVAGE YARD",new GUIStyle(heading){alignment=TextAnchor.MiddleCenter});
            Portrait(new Vector2(820,526+Mathf.Sin(Time.unscaledTime*3)*3),275,playerAnimation.Look);
            if(MenuButton(new Rect(145,453,345,72),"Start  [Enter]"))menuPage=1;
            if(MenuButton(new Rect(145,545,345,72),"Options"))menuPage=4;
            if(MenuButton(new Rect(145,637,345,72),"Quit"))Application.Quit();
            Text(50,841,1000,32,"WASD / ARROWS  Move      ESC  Pause      M  Sound",tiny);
        }
        void ReferenceEndUI()
        {
            Overlay();bool won=Rules.Phase==RunPhase.Won;
            Text(120,100,1200,115,won?"RUN WON":"RUN LOST",new GUIStyle(title){alignment=TextAnchor.MiddleCenter});
            if(!endSummary)
            {
                if(won){Portrait(new Vector2(720,417),160,playerAnimation.Look,true);Text(460,574,520,45,"20 waves survived",center);}
                else
                {
                    Text(400,295,640,48,lastAttacker==null?"The shift is over":"Killed by: "+lastAttacker.id.Replace('_',' '),center);
                    if(lastAttacker!=null)DrawIcon(lastAttacker.image,new Rect(630,377,180,180));
                }
                if(MenuButton(new Rect(470,668,500,70),"Ok  [Enter]"))endSummary=true;return;
            }
            RoundedBox(new Rect(55,260,345,450),panelColor);Text(80,280,300,48,"Stats",heading);
            string[] labels={"Wave","Level","Defeated","Upgrades","Max HP","Damage","Attack speed","Armor"};
            string[] values={Rules.Wave+" / 20",Rules.Level.ToString(),Rules.Kills.ToString(),Rules.UpgradeCount.ToString(),Rules.MaxHealth.ToString("0"),Rules.Stat("percent_damage")+"%",Rules.Stat("attack_speed")+"%",Rules.Stat("armor").ToString("0")};
            for(int i=0;i<labels.Length;i++){Text(80,340+i*42,210,37,labels[i],body);Text(281,340+i*42,95,37,values[i],new GUIStyle(body){alignment=TextAnchor.MiddleRight});}
            RoundedBox(new Rect(427,260,958,450),panelColor);Text(453,280,900,45,$"Weapons ({Rules.Inventory.Count}/6)",heading);
            for(int i=0;i<Rules.Inventory.Count;i++){var w=Rules.Inventory[i];Rect r=new Rect(457+i*111,345,96,86);Frame(r,TierColor(w.tier),2);Box(new Rect(r.x+3,r.y+3,90,80),panelColor);DrawIcon(w.image,new Rect(r.x+10,r.y+10,76,66));}
            Text(453,461,900,45,$"Items ({Rules.Items.Count})",heading);
            summaryScroll=GUI.BeginScrollView(new Rect(453,520,904,163),summaryScroll,new Rect(0,0,875,Mathf.Max(135,Mathf.Ceil(Rules.Items.Count/12f)*66)));
            for(int i=0;i<Rules.Items.Count;i++){Rect r=new Rect(i%12*72,i/12*66,62,58);RoundedBox(r,new Color(.22f,.21f,.18f),5);DrawIcon(Rules.Items[i].image,new Rect(r.x+7,r.y+4,48,48));}GUI.EndScrollView();
            for(int i=1;i<=20;i++){Rect r=new Rect(85+(i-1)*65,735,54,37);RoundedBox(r,i<=Rules.Wave?new Color(.45f,.48f,.32f):panelColor,5);Text(r.x,r.y,r.width,r.height,i.ToString(),center);}
            if(MenuButton(new Rect(130,812,340,60),"Restart  [Enter]"))StartSelectedRun();
            if(MenuButton(new Rect(550,812,340,60),"New Run"))ReturnToSelection(1);
            if(MenuButton(new Rect(970,812,340,60),"Main Menu"))ReturnToSelection(0);
        }
    }
}

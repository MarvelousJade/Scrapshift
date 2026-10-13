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

        void ScaleMapArt()
        {var deck=GameObject.Find("Deck art");if(deck!=null)deck.transform.localScale=new Vector3(ArenaGeometry.HalfSize.x/ArenaGeometry.BaseHalfSize.x,ArenaGeometry.HalfSize.y/ArenaGeometry.BaseHalfSize.y,1);}
        void StartSelectedRun()
        {
            pauseOptions=false;Begin();Rules.Progress=progress;Rules.Difficulty=selectedDifficulty;recordedFinish=false;Rules.Start(SelectedCharacter,SelectedWeapon?.id);ArenaGeometry.SetMapSize(Rules.Stat("map_size"));ScaleMapArt();PlanEncounters();StartWave();
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
            player.transform.localScale=Vector3.one*ActorPresentation.PlayerScale;
            playerAnimation.BodyRoot.localScale=new Vector3(scale.x,scale.y,1);
            playerShadow.Place(playerPos,ActorPresentation.PlayerScale,Mathf.InverseLerp(.95f,1.1f,time)*.392157f);
            UpdateEquipment(0);
            foreach(var view in equipmentViews)if(view!=null)view.transform.position+=(Vector3)offset;
            if(parachute!=null)
            {
                parachute.transform.position=(Vector3)(playerPos+offset)+Vector3.back*3;
                Vector2 canopy=ArrivalMotion.CanopyScale(time);parachute.transform.localScale=new Vector3(canopy.x,canopy.y,1);
                parachute.transform.rotation=Quaternion.Euler(0,0,reducedMotion?0:ArrivalMotion.CanopyRotation(time));
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
            if((Rules.Phase==RunPhase.Playing||Rules.Phase==RunPhase.Paused)&&Rules.Wave<5)progress?.Complete("chal_hourglass");
            ClearCombat();if(parachute!=null)Destroy(parachute);arrivalAge=ArrivalMotion.Duration;
            Rules.ReturnToTitle();menuPage=page;endSummary=false;player.SetActive(false);sound.StopAll();
        }
        void ConfirmMenu()
        {
            sound.Play(SoundCue.UiPress);
            if(Rules.Phase==RunPhase.Title)
            {
                if(menuPage==1&&randomCharacter){var unlocked=Array.FindAll(catalog.data.characters,c=>c.available);var chosen=unlocked[UnityEngine.Random.Range(0,unlocked.Length)];ChangeLook(Array.IndexOf(catalog.data.characters,chosen)-playerAnimation.lookIndex);randomCharacter=false;}
                if(menuPage==1&&!SelectedCharacter.available)return;
                if(menuPage==1&&SelectedCharacter.weaponSlots==0)menuPage=3;else if(menuPage<3)menuPage++;else if(menuPage==3)StartSelectedRun();
            }
            else if(!endSummary)endSummary=true;else StartSelectedRun();
        }
        static void RoundedBox(Rect r,Color color,float radius=12) => GUI.DrawTexture(r,Texture2D.whiteTexture,ScaleMode.StretchToFill,true,0,color,0,radius);
        bool MenuButton(Rect r,string label,bool selected=false)
        {
            bool lit=selected||r.Contains(Event.current.mousePosition);
            RoundedBox(r,lit?paper:panelColor,9);
            var labelStyle=new GUIStyle(center){fontSize=Mathf.Min(27,(int)(r.height*.58f)),wordWrap=false,font=catalog.referenceFont};
            while(labelStyle.fontSize>12&&labelStyle.CalcSize(new GUIContent(label)).x>r.width-18)labelStyle.fontSize--;labelStyle.normal.textColor=lit?ink:paper;
            if(GUI.enabled&&r.Contains(Event.current.mousePosition)&&Event.current.type==EventType.Repaint&&lastUiHover!=r){lastUiHover=r;sound.Play(SoundCue.UiFocus);}
            bool clicked=GUI.Button(r,label,labelStyle);if(clicked)sound.Play(SoundCue.UiPress);return clicked;
        }
        void Portrait(Vector2 origin,float scale,PlayerLook look,bool gear=false)
        {
            var layers=AppearanceResolver.Resolve(look,gear?Rules.Items:Array.Empty<ItemEntry>());
            foreach(var layer in layers)if(layer.order<10)DrawAppearance(layer,origin,scale);
            if(playerAnimation.footSprite!=null)foreach(int side in new[]{-1,1})
            {
                var sprite=playerAnimation.footSprite;Vector2 size=sprite.bounds.size*(SpringMotion.LegHeight/sprite.bounds.size.y)*scale;
                Vector2 p=origin+new Vector2(side*SpringMotion.LegSpread,-SpringMotion.LegY)*scale;
                var before=GUI.matrix;
                GUI.matrix=before*SpringMotion.PortraitLegTransform(p,side);
                DrawIcon(sprite,new Rect(p-size*.5f,size));GUI.matrix=before;
            }
            var hero=playerAnimation.idleFrames[0];Vector2 bodySize=(Vector2)hero.bounds.size*scale;
            var bodyColor=GUI.color;GUI.color=PresentationPolish.BodyTint;DrawIcon(hero,new Rect(origin-bodySize*.5f,bodySize));GUI.color=bodyColor;
            foreach(var layer in layers)if(layer.order>=10)DrawAppearance(layer,origin,scale);
        }
        void SelectionUI()
        {
            MenuBackdrop();
            if(menuPage==4)
            {
                DrawTabbedOptions(false);return;
            }
            OutlinedText(250,48,940,78,menuPage==1?"Character Selection":menuPage==2?"Weapon Selection":"Difficulty Selection",new GUIStyle(title){fontSize=49,alignment=TextAnchor.MiddleCenter});
            if(MenuButton(new Rect(24,24,178,44),"Back"))menuPage--;
            float left=menuPage==1?335:menuPage==3?40:210;
            float cardWidth=570;
            RoundedBox(new Rect(left,155,cardWidth,420),new Color(.16f,.15f,.13f,.82f));
            CharacterDetails(left);
            if(menuPage==1)
            {
                RoundedBox(new Rect(935,155,230,420),panelColor);
                Text(955,184,190,40,"Run options",heading);
                Text(957,265,185,150,"Single player\n20 waves\nThe salvage yard",body);
                CharacterGrid();
            }
            else
            {
                var w=SelectedWeapon;
                float wx=menuPage==3?630:822;
                RoundedBox(new Rect(wx,155,330,420),panelColor);
                Text(wx+20,174,290,47,w==null?"No weapons":w.id=="smg"?"SMG":w.title,heading);
                DrawIcon(w?.image,new Rect(wx+20,246,86,83));
                Text(wx+123,252,185,75,w==null?"Unarmed":WeaponSets.For(w)?.Name??"Weapon",body);
                if(w!=null)DrawOfferDetail(new ShopOffer{weapon=w},wx+20,352,290,PreviewRules());
                if(menuPage==2)
                {
                    WeaponGrid();
                }
                else
                {
                    RoundedBox(new Rect(980,155,370,420),panelColor);
                    Text(1000,183,330,45,selectedDifficulty==6?"Nightmare":"Danger "+selectedDifficulty,heading);
                    var difficultyPreview=new RunRules{Difficulty=selectedDifficulty};Text(1000,252,330,290,difficultyPreview.DifficultyDescription,new GUIStyle(body){fontSize=21});
                    for(int d=0;d<=6;d++){GUI.enabled=d<=progress.Difficulty;if(MenuButton(new Rect(383+d*98,633,84,84),d==6?"N":d.ToString(),d==selectedDifficulty))selectedDifficulty=d;GUI.enabled=true;}
                    Text(460,724,520,36,"20 waves",center);
                    if(MenuButton(new Rect(565,794,310,62),"Start"))StartSelectedRun();
                }
            }
        }
        void ReferenceTitleUI()
        {
            if(menuPage!=0){SelectionUI();return;}
            Box(FullCanvas,new Color(.25f,.24f,.22f));
            var old=GUI.color;GUI.color=new Color(.65f,.6f,.53f,.28f);
            for(int i=0;i<20;i++)DrawIcon(catalog.data.enemies[i%catalog.data.enemies.Length].image,new Rect((i%5)*320-90+(i/5%2)*65,(i/5)*260-55,240,235));
            GUI.color=old;
            var brand=new GUIStyle(title){fontSize=142,alignment=TextAnchor.MiddleCenter};
            OutlinedText(125,72,1190,175,"SCRAPSHIFT",brand);
            Text(300,245,840,42,"THE SALVAGE YARD",new GUIStyle(heading){alignment=TextAnchor.MiddleCenter});
            Portrait(new Vector2(780,575+Mathf.Sin(Time.unscaledTime*3)*3),275,playerAnimation.Look);
            if(MenuButton(new Rect(30,545,200,65),"Start"))menuPage=1;
            if(MenuButton(new Rect(30,627,200,58),"Options"))menuPage=4;
            if(MenuButton(new Rect(30,705,200,58),"Quit"))Application.Quit();
        }
        void ReferenceEndUI()
        {
            if(endSummary)MenuBackdrop();else Overlay();bool won=Rules.Phase==RunPhase.Won;
            if(!endSummary)OutlinedText(120,100,1200,115,won?"RUN WON":"RUN LOST",new GUIStyle(title){alignment=TextAnchor.MiddleCenter});
            else OutlinedText(300,28,1100,58,$"{(won?"Run Won":"Run Lost")} - Salvage Yard     Wave {Rules.Wave} - Danger {Rules.Difficulty}",new GUIStyle(heading){alignment=TextAnchor.MiddleCenter});
            if(!endSummary)
            {
                if(won){Portrait(new Vector2(720,417),160,playerAnimation.Look,true);Text(460,625,520,45,"20 waves survived",center);}
                else
                {
                    Text(400,282,640,82,lastAttacker==null?"The shift is over":"Killed by:\n"+System.Globalization.CultureInfo.InvariantCulture.TextInfo.ToTitleCase(lastAttacker.id.Replace('_',' ')),new GUIStyle(center){fontSize=27});
                    if(lastAttacker!=null){Frame(new Rect(655,385,130,135),new Color(.28f,.28f,.28f));DrawIcon(lastAttacker.image,new Rect(667,397,106,111));}
                }
                if(MenuButton(new Rect(495,550,450,57),"Ok"))endSummary=true;return;
            }
            DrawStatsPanel(true);
            DrawRunInventory(new Rect(390,104,990,680));
            Text(1090,123,260,45,"Progress",heading);
            Text(1090,188,260,80,$"Level {Rules.Level}\n{Rules.Kills} enemies defeated",body);
            DrawInventoryTooltip();
            if(MenuButton(new Rect(130,812,340,60),"Restart"))StartSelectedRun();
            if(MenuButton(new Rect(550,812,340,60),"New Run"))ReturnToSelection(1);
            if(MenuButton(new Rect(970,812,340,60),"Main Menu"))ReturnToSelection(0);
        }
    }
}

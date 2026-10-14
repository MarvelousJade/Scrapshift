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
            loadoutShowcase=-1;showcaseCleanFrame=false;
            if(mapChoice<0)ApplyMapTheme(new System.Random().Next(MapThemes.Names.Length));
            pauseOptions=pauseCodex=false;Begin();Rules.Progress=progress;Rules.Difficulty=selectedDifficulty;recordedFinish=false;Rules.Start(SelectedCharacter,SelectedWeapon?.id);ArenaGeometry.SetMapSize(Rules.Stat("map_size"));ScaleMapArt();PlanEncounters();StartWave();
            music.StartOnce();
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
            if(loadoutShowcase<0&&(Rules.Phase==RunPhase.Playing||Rules.Phase==RunPhase.Paused)&&Rules.Wave<5)progress?.Complete("chal_hourglass");
            loadoutShowcase=-1;showcaseCleanFrame=false;
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
        bool MenuButton(Rect r,string label,bool selected=false,bool cardAction=false,int fontSize=0)
        {
            bool lit=selected||r.Contains(Event.current.mousePosition);
            RoundedBox(r,lit?paper:cardAction?new Color(.16f,.16f,.16f):panelColor,9);
            var labelStyle=new GUIStyle(center){fontSize=fontSize>0?fontSize:Mathf.Min(27,(int)(r.height*.58f)),wordWrap=false,font=catalog.referenceFont};
            while(labelStyle.fontSize>12&&labelStyle.CalcSize(new GUIContent(label)).x>r.width-18)labelStyle.fontSize--;labelStyle.normal.textColor=lit?ink:paper;
            if(GUI.enabled&&r.Contains(Event.current.mousePosition)&&Event.current.type==EventType.Repaint&&lastUiHover!=r){lastUiHover=r;sound.Play(SoundCue.UiFocus);}
            bool clicked=GUI.Button(r,label,labelStyle);if(clicked)sound.Play(SoundCue.UiPress);return clicked;
        }
        void Portrait(Vector2 origin,float scale,PlayerLook look,bool gear=false,float motionTime=-1)
        {
            var layers=AppearanceResolver.Resolve(look,gear?Rules.Items:Array.Empty<ItemEntry>());
            var spring=motionTime>=0?SpringMotion.Pose(motionTime,false,reducedMotion):new Vector3(1,1,0);
            if(playerAnimation.footSprite!=null)foreach(int side in new[]{-1,1})
            {
                var sprite=playerAnimation.footSprite;Vector2 size=sprite.bounds.size*(SpringMotion.LegHeight/sprite.bounds.size.y)*scale;
                Vector2 p=origin+new Vector2(side*SpringMotion.LegSpread*spring.x,-SpringMotion.LegY+spring.z*.3f)*scale;
                var before=GUI.matrix;
                GUI.matrix=before*Matrix4x4.TRS(p,Quaternion.Euler(0,0,side*(spring.x-1)*35),Vector3.one)*Matrix4x4.Translate(-p)*SpringMotion.PortraitLegTransform(p,side);
                DrawIcon(sprite,new Rect(p-size*.5f,size));GUI.matrix=before;
            }
            var portraitMatrix=GUI.matrix;
            if(motionTime>=0)
            {
                GUI.matrix=portraitMatrix*Matrix4x4.TRS(origin+Vector2.down*(-spring.z*scale),Quaternion.identity,new Vector3(spring.x,spring.y,1))*Matrix4x4.Translate(-origin);
            }
            foreach(var layer in layers)if(layer.order<10)DrawAppearance(layer,origin,scale);
            var hero=playerAnimation.idleFrames[0];Vector2 bodySize=(Vector2)hero.bounds.size*scale;
            var bodyColor=GUI.color;GUI.color=PresentationPolish.BodyTint;DrawIcon(hero,new Rect(origin-bodySize*.5f,bodySize));GUI.color=bodyColor;
            foreach(var layer in layers)if(layer.order>=10)DrawAppearance(layer,origin,scale);
            GUI.matrix=portraitMatrix;
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
            float left=menuPage==1?97:menuPage==3?40:210;
            float cardWidth=menuPage==1?637:570;
            RoundedBox(new Rect(left,155,cardWidth,420),new Color(.16f,.15f,.13f,.82f));
            CharacterDetails(left);
            if(menuPage==1)
            {
                RoundedBox(new Rect(737,155,300,417),panelColor);
                RoundedBox(new Rect(864,250,48,57),paper,5);
                RoundedBox(new Rect(858,246,60,12),paper,5);
                for(int line=0;line<3;line++)RoundedBox(new Rect(874,269+line*10,28,3),Color.black,1);
                Text(757,330,260,45,"Records",new GUIStyle(center){fontSize=26});
                string record=randomCharacter?"Select a character":progress.Completed("chal_"+SelectedCharacter.id)?"Run completed":"None yet";
                Text(757,399,260,75,record,new GUIStyle(center){fontSize=22});
                RoundedBox(new Rect(1077,155,267,417),panelColor);
                Text(1092,184,237,40,"Run Options",new GUIStyle(center){fontSize=24});
                RoundedBox(new Rect(1102,240,217,65),Color.black,8);
                Text(1112,250,197,45,"Single player",new GUIStyle(center){fontSize=23});
                RoundedBox(new Rect(1102,315,217,65),Color.black,8);
                Text(1112,325,197,45,"20 waves",new GUIStyle(center){fontSize=23});
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
            if(menuPage==5){MenuBackdrop();DrawPauseCodex();return;}
            if(menuPage!=0){SelectionUI();return;}
            TitleScene();
            if(MenuButton(new Rect(30,545,200,65),"Start"))menuPage=1;
            if(MenuButton(new Rect(30,705,200,58),"Codex"))menuPage=5;
            if(MenuButton(new Rect(30,627,200,58),"Options"))menuPage=4;
            Text(256,496,360,35,"PHOTO / VIDEO SHOWCASES",new GUIStyle(heading){fontSize=23});
            if(MenuButton(new Rect(254,545,280,65),"Martial Monk"))BeginLoadoutShowcase(0);
            if(MenuButton(new Rect(254,627,280,58),"Cyberpunk Ranger"))BeginLoadoutShowcase(1);
            if(MenuButton(new Rect(254,705,280,58),"Auto Turret"))BeginLoadoutShowcase(2);
            if(MenuButton(new Rect(254,783,280,58),"Druid Pets"))BeginLoadoutShowcase(3);
#if !UNITY_WEBGL || UNITY_EDITOR
            if(MenuButton(new Rect(30,783,200,58),"Quit"))Application.Quit();
#endif
        }
        void ReferenceEndUI()
        {
            if(DrawUnlockReward())return;
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
            DrawRunInventory(new Rect(440,104,610,680));
            Text(1090,123,260,45,"Progress",heading);
            Text(1090,188,260,80,$"Level {Rules.Level}\n{Rules.Kills} enemies defeated",body);
            DrawInventoryTooltip();
            if(MenuButton(new Rect(130,812,340,60),"Restart"))StartSelectedRun();
            if(MenuButton(new Rect(550,812,340,60),"New Run"))ReturnToSelection(1);
            if(MenuButton(new Rect(970,812,340,60),"Main Menu"))ReturnToSelection(0);
        }
    }
}

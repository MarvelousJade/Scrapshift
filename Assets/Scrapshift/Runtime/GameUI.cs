using UnityEngine;

namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        GUIStyle damageSmall, damageBig, cardTitle, center, icon;
        readonly Color ink = new Color(.025f,.024f,.022f), paper = new Color(.96f,.96f,.94f), panelColor = new Color(0,0,0,.82f);
        void EnsureStyles()
        {
            if (body != null) return;
            inspectionText=new InspectionText(catalog.uiText);
            body = Style(21, paper); tiny = Style(16, new Color(.78f,.79f,.69f));
            heading = Style(30, paper, FontStyle.Bold); title = Style(78, paper, FontStyle.Bold);
            number = Style(36, paper, FontStyle.Bold); cardTitle = Style(29, paper, FontStyle.Bold);
            center = Style(21, paper, FontStyle.Bold); center.alignment = TextAnchor.MiddleCenter;
            icon = Style(56, paper, FontStyle.Bold); icon.alignment = TextAnchor.MiddleCenter;
            damageSmall = Style(22, Color.white, FontStyle.Bold); damageSmall.alignment = TextAnchor.MiddleCenter;
            damageBig = Style(30, amber, FontStyle.Bold); damageBig.alignment = TextAnchor.MiddleCenter;
            body.font=tiny.font=heading.font=title.font=number.font=cardTitle.font=center.font=catalog.referenceFont;
            inspectionBody=new GUIStyle(body){fontSize=18,richText=true,fontStyle=FontStyle.Normal};
            inspectionTitle=new GUIStyle(inspectionBody){fontSize=22};
            button = new GUIStyle { fontSize = 21, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter,
                normal = { textColor = ink }, hover = { textColor = ink }, active = { textColor = ink } };
        }
        static GUIStyle Style(int size, Color color, FontStyle weight = FontStyle.Normal) => new GUIStyle(GUI.skin.label)
        { fontSize = size, fontStyle = weight, normal = { textColor = color }, wordWrap = true };
        static void Box(Rect rect, Color color) { var old = GUI.color; GUI.color = color; GUI.DrawTexture(rect, Texture2D.whiteTexture); GUI.color = old; }
        void Text(float x, float y, float w, float h, string text, GUIStyle style) => GUI.Label(new Rect(x,y,w,h), text, style);
        void Frame(Rect r, Color color, float border = 4)
        {
            // Focus is one solid surface, not a white inset over a dark frame.
            if(color==paper){RoundedBox(r,paper,8);return;}
            RoundedBox(r, ink,8); RoundedBox(new Rect(r.x+border,r.y+border,r.width-border*2,r.height-border*2), color,Mathf.Max(2,8-border));
        }
        bool Button(Rect r, string label)
        {
            return MenuButton(r,label);
        }
        void OnGUI()
        {
            EnsureStyles(); float scale = Mathf.Min(Screen.width / 1440f, Screen.height / 900f);
            GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 1440*scale)/2,(Screen.height-900*scale)/2), Quaternion.identity, Vector3.one*scale);
            DrawGameScreen();
            DrawAchievementToast();
        }
        void DrawGameScreen()
        {
            BeginInspection();
            if(appearanceReview&&appearanceStage>=2&&appearanceStage<6){AppearanceReviewUI();return;}
            if(scaleReview)
            {
                Text(25,20,1390,50,"PROPORTIONS / BOLD LEGS / ROUND SHADOWS",heading);
                Text(25,800,1390,70,"Player (left) + first-wave enemy (right) / staged rendering, no AI or combat",center);
                return;
            }
            if (Rules.Phase == RunPhase.Title) { TitleUI(); return; }
            if (Rules.Phase == RunPhase.Loot) { LootUI(); EndInspection(); return; }
            if (Rules.Phase == RunPhase.Shop) { GUI.enabled=selectedWeapon<0;ShopUI();GUI.enabled=true; if(selectedWeapon>=0)DrawWeaponActions();else EndInspection(); return; }
            if (Rules.Phase == RunPhase.Upgrade) { UpgradeUI(); EndInspection(); return; }
            DrawNightmareFog();DrawHud();
            var boss=enemies.Find(e=>(e.type.role=="Boss"||e.type.role=="Elite")&&e.marker==null); if(boss!=null&&GameSettings.Flag("bossHp")){Frame(new Rect(400,111,640,25),panelColor,2);Box(new Rect(404,115,632*Mathf.Clamp01(boss.hp/boss.maxHp),17),coral);Text(400,140,640,30,boss.type.id.ToUpperInvariant(),center);}
            DrawFeedback();
            DrawSupportShowcase();
            if (Rules.Phase == RunPhase.Upgrade) UpgradeUI();
            else if (Rules.Phase == RunPhase.Shop) ShopUI();
            else if (Rules.Phase == RunPhase.Paused) PauseUI();
            else if (Rules.Phase == RunPhase.Won || Rules.Phase == RunPhase.Lost) EndUI();
            EndInspection();
        }
        void Overlay() => Box(FullCanvas, new Color(0,0,0,GameSettings.Flag("darkening")?.74f:0));
        void TitleUI() => ReferenceTitleUI();
        void ChangeLook(int direction)
        {
            var looks=playerAnimation.looks;if(looks==null||looks.Length==0)return;
            if(direction!=0)startingWeapon=0;
            playerAnimation.lookIndex=(playerAnimation.lookIndex+direction+looks.Length)%looks.Length;
            if(titleModel!=null)titleModel.GetComponent<SpriteFrameAnimator>().lookIndex=playerAnimation.lookIndex;
        }
        Color TierColor(int tier) => tier==4 ? new Color(1,59/255f,59/255f) : tier==3 ? new Color(173/255f,90/255f,1) : tier==2 ? new Color(90/255f,190/255f,1) : Color.white;
        Color TierBackground(int tier) => tier==4 ? new Color(36/255f,9/255f,9/255f) : tier==3 ? new Color(16/255f,10/255f,27/255f) : tier==2 ? new Color(15/255f,32/255f,40/255f) : ink;
        void TierCard(Rect r,int tier)
        {
            // A single colored perimeter: do not stack it inside Frame's black perimeter.
            RoundedBox(r,tier>1?TierColor(tier):Color.black,8);
            RoundedBox(new Rect(r.x+2,r.y+2,r.width-4,r.height-4),TierBackground(tier),6);
        }
        Color InventoryTileColor(int tier)=>Color.Lerp(new Color(.075f,.072f,.065f),TierColor(tier),tier>1?.16f:0);
        void DrawHud()
        {
            var hudMatrix=GUI.matrix;GUI.matrix=hudMatrix*Matrix4x4.Translate(new Vector3(FullCanvas.xMin,FullCanvas.yMin,0));
            Frame(new Rect(24,22,262,38), new Color(.22f,.18f,.17f), 6);
            Box(new Rect(30,28,250*Mathf.Clamp01(Rules.Health/Rules.MaxHealth),26), new Color(.83f,.07f,.05f));
            Text(24,22,262,38,$"{Mathf.CeilToInt(Rules.Health)} / {Rules.MaxHealth:0}",center);
            Frame(new Rect(24,64,262,38),new Color(.25f,.25f,.24f),6);
            Box(new Rect(30,70,250*Mathf.Clamp01((float)Rules.XP/Rules.NextLevel),26),new Color(.2f,.8f,.18f));
            Text(24,64,262,38,$"LV.{Rules.Level}",center);
            DrawMoneyCounter();
            DrawStoredMaterials();
            GUI.matrix=hudMatrix*Matrix4x4.Translate(new Vector3(0,FullCanvas.yMin,0));
            OutlinedText(550,20,340,40,$"WAVE {Rules.Wave}",new GUIStyle(center){fontSize=32});
            var timer=new GUIStyle(number) { fontSize=44, alignment=TextAnchor.MiddleCenter };
            if(Rules.Phase==RunPhase.Playing||Rules.Phase==RunPhase.Paused)
                OutlinedText(630,61,180,56,supportShowcase?"DEMO":(waveSweeping?0:Mathf.CeilToInt(Rules.Remaining)).ToString(),timer);
            DrawPendingRewards();
            GUI.matrix=hudMatrix;
        }

        void OutlinedText(float x,float y,float w,float h,string label,GUIStyle style)
        {
            var outline=new GUIStyle(style); outline.normal.textColor=Color.black;
            Text(x-2,y,w,h,label,outline);Text(x+2,y,w,h,label,outline);
            Text(x,y-2,w,h,label,outline);Text(x,y+2,w,h,label,outline);
            Text(x,y,w,h,label,style);
        }

        void UpgradeUI()
        {
            Overlay(); DrawNightmareFog();DrawHud(); DrawStatsPanel();
            OutlinedText(ContentLeft,220,ContentWidth,64,"Level Up!",new GUIStyle(number) { fontSize=44, alignment=TextAnchor.MiddleCenter });
            
            for(int i=0;i<Rules.Offers.Length;i++)
            {
                var o=Rules.Offers[i];float x=OfferX(i),width=OfferWidth;
                TierCard(new Rect(x,320,width,270),o.Tier);
                RoundedBox(new Rect(x+14,340,66,70),new Color(.075f,.075f,.075f),6);
                DrawIcon(o.Image,new Rect(x+20,346,54,58));
                Text(x+92,342,width-108,58,CyberneticTheme.UpgradeName(o.Stat)+(o.Tier>1?" "+new[]{"","","II","III","IV"}[o.Tier]:""),new GUIStyle(body){fontSize=20,normal={textColor=TierColor(o.Tier)}});
                Text(x+92,400,width-108,28,"Upgrade",new GUIStyle(body){fontSize=18,normal={textColor=new Color(.89f,.85f,.62f)}});
                var gain=new GUIStyle(body);gain.normal.textColor=new Color(.38f,1,.25f);
                RichText(StatLine(o.Stat,o.Amount),new Rect(x+18,450,width-36,65),18);
                if(MenuButton(new Rect(x+16,530,width-32,44),"Choose",cardAction:true))SelectUpgrade(i);
            }
            GUI.enabled=Rules.CanReroll;
            if(RerollButton(new Rect(ContentLeft+(ContentWidth-406)/2,630,406,56)))RerollUpgrade();
            GUI.enabled=true;
            DrawSetTooltip();
        }
        bool pauseOptions,pauseCodex;
        void PauseUI()
        {
            Overlay();
            if(pauseOptions)
            {
                DrawTabbedOptions(true);
                return;
            }
            if(pauseCodex){DrawPauseCodex();return;}
            DrawStatsPanel();
            float left=FullCanvas.xMin+40,width=Mathf.Min(515,FullCanvas.width*.33f);
            if(Button(new Rect(left,203,width,55),"Resume"))Rules.Pause();
            if(Button(new Rect(left,278,width,55),"Restart"))StartSelectedRun();
            if(Button(new Rect(left,353,width,55),"End the run"))Rules.Abandon();
            if(Button(new Rect(left,428,width,55),"Codex"))pauseCodex=true;
            if(Button(new Rect(left,503,width,55),"Options"))pauseOptions=true;
            if(Button(new Rect(left,578,width,55),"Return to Main Menu"))ReturnToSelection(0);
            float inventoryLeft=left+width+20;
            DrawRunInventory(new Rect(inventoryLeft,64,StatsLeft-inventoryLeft-18,640));
            DrawWaveTimeline(787);
            DrawInventoryTooltip();DrawSetTooltip();DrawItemSortMenu();
        }
        void EndUI() => ReferenceEndUI();
    }
}

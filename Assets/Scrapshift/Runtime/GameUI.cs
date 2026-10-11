using UnityEngine;

namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        GUIStyle damageSmall, damageBig, cardTitle, center, icon;
        readonly Color ink = new Color(.025f,.024f,.022f), paper = new Color(.96f,.96f,.94f), panelColor = new Color(.09f,.085f,.075f,.96f);
        void EnsureStyles()
        {
            if (body != null) return;
            body = Style(21, paper); tiny = Style(16, new Color(.78f,.79f,.69f));
            heading = Style(30, paper, FontStyle.Bold); title = Style(78, paper, FontStyle.Bold);
            number = Style(36, paper, FontStyle.Bold); cardTitle = Style(29, paper, FontStyle.Bold);
            center = Style(21, paper, FontStyle.Bold); center.alignment = TextAnchor.MiddleCenter;
            icon = Style(56, paper, FontStyle.Bold); icon.alignment = TextAnchor.MiddleCenter;
            damageSmall = Style(22, Color.white, FontStyle.Bold); damageSmall.alignment = TextAnchor.MiddleCenter;
            damageBig = Style(30, amber, FontStyle.Bold); damageBig.alignment = TextAnchor.MiddleCenter;
            button = new GUIStyle { fontSize = 21, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter,
                normal = { textColor = ink }, hover = { textColor = ink }, active = { textColor = ink } };
        }
        static GUIStyle Style(int size, Color color, FontStyle weight = FontStyle.Normal) => new GUIStyle(GUI.skin.label)
        { fontSize = size, fontStyle = weight, normal = { textColor = color }, wordWrap = true };
        static void Box(Rect rect, Color color) { var old = GUI.color; GUI.color = color; GUI.DrawTexture(rect, Texture2D.whiteTexture); GUI.color = old; }
        void Text(float x, float y, float w, float h, string text, GUIStyle style) => GUI.Label(new Rect(x,y,w,h), text, style);
        void Frame(Rect r, Color color, float border = 4)
        {
            Box(new Rect(r.x + 5, r.y + 6, r.width, r.height), new Color(.10f,.09f,.11f,.65f));
            Box(r, ink); Box(new Rect(r.x+border,r.y+border,r.width-border*2,r.height-border*2), color);
        }
        bool Button(Rect r, string label)
        {
            bool hover = r.Contains(Event.current.mousePosition);
            Frame(r, hover ? paper : new Color(.78f,.78f,.76f));
            return GUI.Button(r, label, button);
        }
        void OnGUI()
        {
            EnsureStyles(); float scale = Mathf.Min(Screen.width / 1440f, Screen.height / 900f);
            GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 1440*scale)/2,(Screen.height-900*scale)/2), Quaternion.identity, Vector3.one*scale);
            if(appearanceReview&&appearanceStage>=2&&appearanceStage<6){AppearanceReviewUI();return;}
            if(scaleReview)
            {
                Text(25,20,1390,50,"PROPORTIONS / BOLD LEGS / ROUND SHADOWS",heading);
                Text(25,800,1390,70,"Player (left) + first-wave enemy (right) / staged rendering, no AI or combat",center);
                return;
            }
            if (Rules.Phase == RunPhase.Title) { TitleUI(); return; }
            if (Rules.Phase == RunPhase.Loot) { LootUI(); return; }
            if (Rules.Phase == RunPhase.Shop) { ShopUI(); return; }
            if (Rules.Phase == RunPhase.Upgrade) { UpgradeUI(); return; }
            DrawHud();
            var boss=enemies.Find(e=>e.type.role=="Boss"&&e.marker==null); if(boss!=null){Frame(new Rect(400,111,640,25),panelColor,2);Box(new Rect(404,115,632*Mathf.Clamp01(boss.hp/boss.maxHp),17),coral);Text(400,140,640,30,"PREDATOR",center);}
            DrawFeedback();
            if (Rules.Phase == RunPhase.Upgrade) UpgradeUI();
            else if (Rules.Phase == RunPhase.Shop) ShopUI();
            else if (Rules.Phase == RunPhase.Paused) PauseUI();
            else if (Rules.Phase == RunPhase.Won || Rules.Phase == RunPhase.Lost) EndUI();
        }
        void Overlay() => Box(new Rect(0,0,1440,900), new Color(.055f,.05f,.045f,.78f));
        void TitleUI() => ReferenceTitleUI();
        void ChangeLook(int direction)
        {
            var looks=playerAnimation.looks;if(looks==null||looks.Length==0)return;
            playerAnimation.lookIndex=(playerAnimation.lookIndex+direction+looks.Length)%looks.Length;
            if(titleModel!=null)titleModel.GetComponent<SpriteFrameAnimator>().lookIndex=playerAnimation.lookIndex;
        }
        Color TierColor(int tier) => tier==4 ? new Color(1,.32f,.32f) : tier==3 ? new Color(.76f,.43f,1) : tier==2 ? new Color(.40f,.66f,1) : paper;
        void DrawHud()
        {
            Frame(new Rect(24,22,246,34), new Color(.22f,.18f,.17f), 4);
            Box(new Rect(28,26,238*Mathf.Clamp01(Rules.Health/Rules.MaxHealth),26), new Color(.83f,.07f,.05f));
            Text(24,22,246,34,$"{Mathf.CeilToInt(Rules.Health)} / {Rules.MaxHealth:0}",center);
            Frame(new Rect(24,64,246,27),new Color(.25f,.25f,.24f),3);
            Box(new Rect(27,67,240*Mathf.Clamp01((float)Rules.XP/Rules.NextLevel),21),new Color(.2f,.8f,.18f));
            Text(185,65,81,25,$"LV.{Rules.Level}",tiny);
            DrawIcon(catalog.scrapShapes[0],new Rect(26,105,30,35));
            OutlinedText(67,96,160,51,Rules.Scrap.ToString(),number);
            if(Rules.Banked>0)OutlinedText(26,146,235,30,$"Stored  {Rules.Banked}",body);
            if(Rules.PendingCrates>0)OutlinedText(26,180,260,30,$"Item crates  {Rules.PendingCrates}",body);
            OutlinedText(550,20,340,40,$"WAVE {Rules.Wave}",center);
            var timer=new GUIStyle(number) { alignment=TextAnchor.MiddleCenter };
            if(Rules.Phase==RunPhase.Playing||Rules.Phase==RunPhase.Paused)
                OutlinedText(630,61,180,56,Mathf.CeilToInt(Rules.Remaining).ToString(),timer);
            Text(1120,23,296,32,$"{Rules.Wave} / {RunRules.WaveCount}",tiny);
            Text(28,864,900,26,"WASD / ARROWS  Move     ESC  Pause     M  Sound",tiny);
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
            Overlay(); DrawHud(); DrawStatsPanel();
            OutlinedText(24,188,1062,64,"Level Up!",new GUIStyle(number) { alignment=TextAnchor.MiddleCenter });
            Text(24,254,1062,32,$"Choose an upgrade  /  {Rules.PendingLevels} remaining",center);
            for(int i=0;i<Rules.Offers.Length;i++)
            {
                var o=Rules.Offers[i];float x=24+i*268;
                Frame(new Rect(x,320,252,300),ink,2);
                Box(new Rect(x+3,323,246,4),TierColor(o.Tier));
                Box(new Rect(x+14,340,66,70),new Color(.17f,.16f,.15f));
                DrawIcon(o.Image,new Rect(x+20,346,54,58));
                Text(x+92,342,144,58,ItemEntry.StatName(o.Stat),body);
                Text(x+92,400,144,28,o.Rarity,tiny);
                var gain=new GUIStyle(body);gain.normal.textColor=new Color(.38f,1,.25f);
                Text(x+18,466,216,75,o.Detail,gain);
                if(Button(new Rect(x+16,555,220,48),$"Choose  [{i+1}]"))SelectUpgrade(i);
            }
            GUI.enabled=Rules.CanReroll;
            if(Button(new Rect(363,664,384,56),$"REROLL  - {Rules.RerollPrice}   [R]"))RerollUpgrade();
            GUI.enabled=true;
            DrawSetTooltip();
        }
        void PauseUI()
        {
            Overlay(); Frame(new Rect(445,200,550,505),panelColor);
            Text(480,242,480,50,"TAKE A BREATHER",heading);
            if (Button(new Rect(480,327,480,62),"RESUME   [ESC]")) Rules.Pause();
            if (Button(new Rect(480,412,480,62),muted ? "SOUND: OFF" : "SOUND: ON")) ToggleSound();
            if (Button(new Rect(480,497,480,62),reducedMotion ? "REDUCED MOTION: ON" : "REDUCED MOTION: OFF"))
            { reducedMotion=!reducedMotion; PlayerPrefs.SetInt("reducedMotion", reducedMotion ? 1 : 0); }
            Text(480,582,480,30,$"EFFECTS VOLUME  {sound.Volume*100:0}%",tiny);
            float volume=GUI.HorizontalSlider(new Rect(484,624,470,24),sound.Volume,0,1);
            if (Mathf.Abs(volume-sound.Volume) > .001f) sound.SetVolume(volume);
            Text(480,657,480,28,"Your wave timer is stopped.",tiny);
        }
        void EndUI() => ReferenceEndUI();
    }
}

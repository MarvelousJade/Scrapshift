using UnityEngine;

namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        GUIStyle damageSmall, damageBig, cardTitle, center, icon;
        readonly Color ink = new Color(.08f,.08f,.09f), paper = new Color(.96f,.96f,.94f), panelColor = new Color(.20f,.20f,.21f);
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
            Frame(r, hover ? paper : mint);
            return GUI.Button(r, label, button);
        }
        void OnGUI()
        {
            EnsureStyles(); float scale = Mathf.Min(Screen.width / 1440f, Screen.height / 900f);
            GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 1440*scale)/2,(Screen.height-900*scale)/2), Quaternion.identity, Vector3.one*scale);
            Box(new Rect(0,0,1440,102), ink); Box(new Rect(0,800,1440,100), ink);
            if (Rules.Phase == RunPhase.Title) { TitleUI(); return; }
            Text(32,15,400,32,"HEALTH",tiny);
            Frame(new Rect(32,48,320,34), new Color(.35f,.21f,.23f), 3);
            Box(new Rect(36,52,312*Mathf.Clamp01(Rules.Health/Rules.MaxHealth),26), coral);
            Text(32,46,320,35,$"{Mathf.CeilToInt(Rules.Health)} / {Rules.MaxHealth:0}",center);
            Text(510,13,420,34,$"WAVE {Rules.Wave} / 20",center);
            Text(645,44,160,53,$"{Mathf.CeilToInt(Rules.Remaining):00}s",number);
            Text(1100,16,300,28,"MATERIALS",tiny);
            Text(1100,43,300,50,$"{Rules.Scrap}    [+{Rules.Banked}]",number);
            Text(32,815,650,30,$"WEAPONS {Rules.Inventory.Count} / 6    |    ITEMS {Rules.Items.Count}",body);
            Text(32,854,800,25,"WASD / ARROWS  Move     ESC  Pause     M  Sound",tiny);
            Text(960,813,440,27,$"LEVEL {Rules.Level}       XP {Rules.XP} / {Rules.NextLevel}",tiny);
            Frame(new Rect(960,851,440,20),panelColor,3);
            Box(new Rect(964,855,432*Mathf.Clamp01((float)Rules.XP/Rules.NextLevel),12),mint);
            DrawFeedback();
            if (Rules.Phase == RunPhase.Upgrade) UpgradeUI();
            else if (Rules.Phase == RunPhase.Shop) ShopUI();
            else if (Rules.Phase == RunPhase.Paused) PauseUI();
            else if (Rules.Phase == RunPhase.Won || Rules.Phase == RunPhase.Lost) EndUI();
        }
        void Overlay() => Box(new Rect(0,102,1440,698), new Color(.09f,.08f,.10f,.80f));
        void TitleUI()
        {
            Text(40,28,1000,45,"SCRAPSHIFT    /    THE SALVAGE YARD",heading);
            Frame(new Rect(100,185,770,515),panelColor);
            Text(140,224,680,35,"SMALL SCAVENGER. BIG TROUBLE.",tiny);
            Text(133,270,720,105,"SCRAPSHIFT",title);
            Text(140,388,650,95,"The yard is crawling. Grab your rivet gun, collect the scrap, and turn a bad shift into a great build.",body);
            Text(140,501,650,58,"20 waves. 6 weapon slots. Build your own loadout.\nYour weapons aim and fire automatically.",tiny);
            if (Button(new Rect(140,590,355,66),"LET'S GO!   [ENTER]")) Begin();
            Text(532,598,300,55,"WASD / ARROWS\nto move",body);
            Text(957,639,350,50,"THE SCAVENGER",center);
            Text(40,834,1300,40,"MOVE  /  COLLECT  /  UPGRADE  /  SURVIVE",center);
        }
        Color TierColor(int tier) => tier==4 ? new Color(1,.32f,.32f) : tier==3 ? new Color(.76f,.43f,1) : tier==2 ? new Color(.40f,.66f,1) : paper;
        void UpgradeUI()
        {
            Overlay(); Text(72,135,1100,50,$"LEVEL UP!   {Rules.PendingLevels} reward(s) remaining",heading);
            Text(72,192,1150,35,"Choose a stat upgrade, then visit the shop.",body);
            for(int i=0;i<Rules.Offers.Length;i++)
            {
                var o=Rules.Offers[i];float x=72+i*328;Frame(new Rect(x,266,308,338),TierColor(o.Tier),3);
                Box(new Rect(x+5,271,298,328),panelColor);
                Text(x+20,290,270,28,o.Rarity,tiny);Text(x+20,346,270,62,o.Title,heading);
                Text(x+20,440,268,75,o.Detail,body);
                if(Button(new Rect(x+20,532,268,52),$"SELECT   [{i+1}]"))SelectUpgrade(i);
            }
            GUI.enabled=Rules.CanReroll;if(Button(new Rect(1036,660,308,54),$"REROLL   {Rules.RerollPrice}   [R]"))RerollUpgrade();GUI.enabled=true;
            Text(72,669,900,45,$"MATERIALS {Rules.Scrap}    |    Upgrades don't spend materials. Rerolls do.",body);
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
        void EndUI()
        {
            Overlay(); Frame(new Rect(355,220,730,435),panelColor); bool won=Rules.Phase==RunPhase.Won;
            Text(400,260,650,60,won ? "SHIFT COMPLETE!" : "END OF THE SHIFT",heading);
            Text(400,346,640,85,won ? "Yard secured. One tiny scavenger, a whole lot of firepower." : "Keep moving, grab the scrap, and watch for the red spawn warnings.",body);
            Text(400,461,640,40,$"{Rules.Kills} DEFEATED     /     {Rules.UpgradeCount} UPGRADES",tiny);
            if (Button(new Rect(400,546,640,65),"ONE MORE RUN   [ENTER]")) Begin();
        }
    }
}

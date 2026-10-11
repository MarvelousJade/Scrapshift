using UnityEngine;
namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        int selectedWeapon=-1;
        Vector2 itemScroll;
        void DrawIcon(Sprite sprite,Rect r)
        {
            if(sprite==null)return;
            GUI.DrawTexture(r,sprite.texture,ScaleMode.ScaleToFit,true);
        }
        void ShopUI()
        {
            Box(new Rect(0,0,1440,900),new Color(.12f,.12f,.13f));
            Text(40,25,650,54,$"SHOP  /  WAVE {Rules.Wave} CLEARED",heading);
            Text(1060,30,340,45,$"MATERIALS   {Rules.Scrap}",heading);
            Frame(new Rect(32,104,278,718),panelColor,3);
            Text(54,127,244,40,"STATS",heading);
            string[] names=
            {
                "max_hp","hp_regeneration","lifesteal","percent_damage","melee_damage","ranged_damage","attack_speed","crit_chance","armor","dodge","speed","range","harvesting"
            };
            for(int i=0;i<names.Length;i++)
            {
                float value=names[i]=="max_hp"?Rules.MaxHealth:Rules.Stat(names[i]);
                Text(54,185+i*39,234,34,$"{ItemEntry.StatName("stat_"+names[i])}   {value:0}",tiny);
            }
            for(int i=0;i<4;i++)
            {
                float x=338+i*267;
                var offer=Rules.Shop[i];
                Frame(new Rect(x,106,249,344),offer==null?panelColor:TierColor(offer.Tier),3);
                Box(new Rect(x+5,111,239,334),panelColor);
                if(offer==null)
                {
                    Text(x+20,244,209,45,"SOLD",center);
                    continue;
                }
                DrawIcon(offer.Image,new Rect(x+75,130,100,95));
                Text(x+16,235,217,62,offer.Title,center);
                Text(x+18,305,213,111,offer.Detail,tiny);
                GUI.enabled=Rules.CanBuy(i);
                if(Button(new Rect(x,468,249,48),offer.weapon!=null&&Rules.Inventory.Count==6&&!Rules.CanBuy(i)&&Rules.Scrap>=Rules.Price(offer.Value)?"SLOTS FULL":$"BUY   {Rules.Price(offer.Value)}"))
                {
                    Rules.Buy(i);
                    SyncEquipment();
                    sound.Play(SoundCue.Install);
                }
                GUI.enabled=true;
                if(Button(new Rect(x,525,249,37),offer.locked?"LOCKED":"LOCK"))
                {
                    Rules.ToggleLock(i);
                    sound.Play(SoundCue.Reroll);
                }
            }
            Text(338,587,830,40,$"WEAPONS ({Rules.Inventory.Count}/6)     Click a slot to combine or recycle",body);
            for(int i=0;i<6;i++)
            {
                float x=338+i*123;
                var w=i<Rules.Inventory.Count?Rules.Inventory[i]:null;
                Frame(new Rect(x,633,107,103),selectedWeapon==i?mint:w==null?panelColor:TierColor(w.tier),3);
                Box(new Rect(x+5,638,97,93),panelColor);
                if(w!=null)
                {
                    DrawIcon(w.image,new Rect(x+15,645,77,64));
                    Text(x+8,707,90,23,$"TIER {w.tier}",tiny);
                }
                if(GUI.Button(new Rect(x,633,107,103),GUIContent.none,GUIStyle.none))selectedWeapon=i;
            }
            if(selectedWeapon>=0&&selectedWeapon<Rules.Inventory.Count)
            {
                var w=Rules.Inventory[selectedWeapon];
                Text(338,749,650,38,w.title+"  |  "+w.Detail.Replace('\n',' '),tiny);
                GUI.enabled=Rules.CanCombine(selectedWeapon);
                if(Button(new Rect(1092,637,300,43),"COMBINE DUPLICATE"))
                {
                    Rules.Combine(selectedWeapon);
                    SyncEquipment();
                    sound.Play(SoundCue.Install);
                }
                GUI.enabled=true;
                if(selectedWeapon<Rules.Inventory.Count&&Button(new Rect(1092,693,300,43),$"RECYCLE  +{Mathf.Max(1,(int)(Rules.Price(Rules.Inventory[selectedWeapon].value)*.25f))}"))
                {
                    Rules.Recycle(selectedWeapon);
                    selectedWeapon=-1;
                    SyncEquipment();
                    sound.Play(SoundCue.Install);
                }
            }
            Text(338,789,150,28,$"ITEMS ({Rules.Items.Count})",tiny);
            itemScroll=GUI.BeginScrollView(new Rect(468,776,924,57),itemScroll,new Rect(0,0,Mathf.Max(900,Rules.Items.Count*52),40));
            for(int i=0;i<Rules.Items.Count;i++)
            {
                DrawIcon(Rules.Items[i].image,new Rect(i*52,0,42,40));
                GUI.Label(new Rect(i*52,0,42,40),new GUIContent("",Rules.Items[i].title+"\n"+Rules.Items[i].Detail));
            }
            GUI.EndScrollView();
            GUI.enabled=Rules.CanReroll;
            if(Button(new Rect(338,847,344,43),$"REROLL   {Rules.RerollPrice}   [R]"))
            {
                Rules.Reroll();
                sound.Play(SoundCue.Reroll);
            }
            GUI.enabled=true;
            if(Button(new Rect(1048,847,344,43),$"GO  /  WAVE {Rules.Wave+1}   [ENTER]"))AdvanceWave();
            if(!string.IsNullOrEmpty(GUI.tooltip))
            {
                Frame(new Rect(716,616,360,145),ink);
                Text(733,630,327,120,GUI.tooltip,body);
            }
        }
    }
}

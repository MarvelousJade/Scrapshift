using UnityEngine;

namespace Scrapshift
{
    public static class StatPresentation
    {
        public static readonly string[] Keys={"max_hp","hp_regeneration","lifesteal","percent_damage","melee_damage","ranged_damage","elemental_damage","attack_speed","crit_chance","engineering","range","armor","dodge","speed","luck","harvesting"};
        public static readonly string[] ExtraKeys={"lifesteal","melee_damage","ranged_damage","elemental_damage","engineering","range","dodge","luck"};
        public static string Unit(string key)=>key=="lifesteal"||key=="percent_damage"||key=="attack_speed"||key=="crit_chance"||key=="dodge"||key=="speed"?"%":"";
        public static Sprite Icon(ReferenceCatalog catalog,string key)
        {
            var upgrade=System.Array.Find(catalog.data.upgrades,u=>u.stat==key);
            if(upgrade!=null&&upgrade.image!=null)return upgrade.image;
            int i=System.Array.IndexOf(ExtraKeys,key);
            return i>=0&&catalog.extraStatIcons!=null&&i<catalog.extraStatIcons.Length?catalog.extraStatIcons[i]:null;
        }
    }
    public sealed partial class ArenaGame
    {
        void MenuBackdrop()
        {
            Box(FullCanvas,new Color(.31f,.30f,.27f));
            var before=GUI.color;GUI.color=new Color(.65f,.63f,.55f,.14f);
            for(int i=0;i<22;i++)
            {
                float x=i<11?i*145-50:i%11*145-50;
                float y=i<11?-25:780;
                DrawIcon(catalog.data.items[i%catalog.data.items.Length].image,new Rect(x,y,115,120));
            }
            GUI.color=before;
        }
        void DrawStatIcon(string key,Rect rect)=>DrawIcon(StatPresentation.Icon(catalog,key),rect);
        void DrawRunInventory(Rect area)
        {
            RoundedBox(area,panelColor);
            Text(area.x+20,area.y+14,area.width-40,45,$"Weapons ({Rules.Inventory.Count}/6)",heading);
            for(int i=0;i<Rules.Inventory.Count;i++)
            {
                var w=Rules.Inventory[i];var r=new Rect(area.x+20+i*109,area.y+76,96,90);
                Frame(r,TierColor(w.tier),2);Box(new Rect(r.x+3,r.y+3,r.width-6,r.height-6),panelColor);
                DrawIcon(w.image,new Rect(r.x+9,r.y+9,78,72));
                GUI.Label(r,new GUIContent("",w.title+"\n"+w.Detail+"\n"+WeaponSets.For(w)?.Name));
            }
            Text(area.x+20,area.y+200,area.width-40,45,$"Items ({Rules.Items.Count})",heading);
            summaryScroll=GUI.BeginScrollView(new Rect(area.x+20,area.y+257,area.width-40,area.height-275),summaryScroll,new Rect(0,0,area.width-65,Mathf.Max(area.height-295,Mathf.Ceil(Rules.Items.Count/9f)*68)));
            for(int i=0;i<Rules.Items.Count;i++)
            {
                var item=Rules.Items[i];var r=new Rect(i%9*73,i/9*68,62,60);
                RoundedBox(r,new Color(.23f,.22f,.2f),5);DrawIcon(item.image,new Rect(r.x+6,r.y+5,50,50));
                GUI.Label(r,new GUIContent("",item.title+"\n"+item.Detail));
            }
            GUI.EndScrollView();
        }
        void DrawInventoryTooltip()
        {
            if(string.IsNullOrEmpty(GUI.tooltip))return;
            RoundedBox(new Rect(710,360,360,210),ink);Text(730,376,320,180,GUI.tooltip,body);
        }
        void DrawWaveTimeline(float y)
        {
            for(int i=1;i<=20;i++)
            {
                var r=new Rect(335+(i-1)*36,y,31,36);RoundedBox(r,i==Rules.Wave?paper:panelColor,4);
                var style=new GUIStyle(center){fontSize=17};style.normal.textColor=i==Rules.Wave?ink:i<Rules.Wave?new Color(.55f,.78f,.4f):paper;
                Text(r.x,r.y,r.width,r.height,i.ToString(),style);
            }
        }
    }
}

using UnityEngine;
using System.Linq;

namespace Scrapshift
{
    public static class StatPresentation
    {
        public static readonly string[] Keys={"max_hp","hp_regeneration","lifesteal","percent_damage","melee_damage","ranged_damage","elemental_damage","attack_speed","crit_chance","engineering","range","armor","dodge","speed","luck","harvesting"};
        public static readonly string[] ExtraKeys={"lifesteal","melee_damage","ranged_damage","elemental_damage","engineering","range","dodge","luck"};
        public static string Unit(string key)=>key=="lifesteal"||key=="percent_damage"||key=="attack_speed"||key=="crit_chance"||key=="dodge"||key=="speed"?"%":"";
        public static Sprite Icon(ReferenceCatalog catalog,string key)
        {
            key=key.StartsWith("stat_")?key.Substring(5):key;
            if(key=="attack_speed_mod")key="attack_speed";
            if(key=="levels")return catalog.levelIcon;
            if(key.StartsWith("item:"))return System.Array.Find(catalog.data.items,i=>i.id==key.Substring(5))?.image;
            if(key=="piercing_damage")return System.Array.Find(catalog.data.items,i=>i.id=="pumpkin")?.image;
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
            DrawMenuGround(FullCanvas);
        }
        void DrawStatIcon(string key,Rect rect)=>DrawIcon(StatPresentation.Icon(catalog,key),rect);
        void DrawRunInventory(Rect area)
        {
            if(Event.current.type==EventType.Repaint)inventoryHoveredThisFrame=false;
            Text(area.x,area.y,area.width,42,$"Weapons ({Rules.Inventory.Count}/{Rules.WeaponSlots})",heading);
            Box(new Rect(area.x+245,area.y+22,Mathf.Max(0,area.width-245),2),new Color(.4f,.49f,.41f,.35f));
            int columns=Mathf.Clamp(Mathf.FloorToInt((area.width-18)/88),1,6);
            for(int i=0;i<Rules.Inventory.Count;i++)
            {
                var w=Rules.Inventory[i];var r=new Rect(area.x+i%columns*88,area.y+58+i/columns*88,80,80);
                Frame(r,InventoryHover("weapon:"+i,new Rect(r.position-area.position,r.size),area,Vector2.zero)?paper:InventoryTileColor(w.tier),2);
                DrawIcon(w.image,new Rect(r.x+7,r.y+7,66,66));InspectWeapon("weapon:"+i,w,r);
            }
            float itemY=area.y+58+Mathf.Max(1,Mathf.CeilToInt(Rules.Inventory.Count/(float)columns))*88+10;
            Text(area.x,itemY,100,42,"Items",heading);
            Box(new Rect(area.x+105,itemY+22,Mathf.Max(0,area.width-400),2),new Color(.4f,.49f,.41f,.35f));
            DrawItemSorting(new Rect(area.xMax-290,itemY,290,38));
            var viewport=new Rect(area.x,itemY+58,area.width,Mathf.Max(88,area.yMax-itemY-58));var items=SortedItems();
            summaryScroll=BeginNavigationScrollView(viewport,summaryScroll,new Rect(0,0,area.width-18,Mathf.Max(viewport.height-1,Mathf.Ceil((items.Length+1)/(float)columns)*88)));
            for(int i=0;i<items.Length;i++)
            {
                var item=items[i];var r=new Rect(i%columns*88,i/columns*88,80,80);string id="item:"+Rules.Items.IndexOf(item);
                Frame(r,InventoryHover(id,r,viewport,summaryScroll)?paper:InventoryTileColor(item.tier),2);
                DrawIcon(item.image,new Rect(r.x+7,r.y+7,66,66));InspectItem(id,item,r,viewport,summaryScroll);
                int count=Rules.Items.Count(e=>e.id==item.id);if(count>1)OutlinedText(r.x+50,r.y+54,28,24,count.ToString(),tiny);
            }
            var character=Rules.Character??SelectedCharacter;var cr=new Rect(items.Length%columns*88,items.Length/columns*88,80,80);
            Frame(cr,InventoryHover("character",cr,viewport,summaryScroll)?paper:InventoryTileColor(1),2);DrawHeroFace(new Rect(cr.x+13,cr.y+8,54,64));
            string description=CharacterEffectDescription(character);
            Inspect("character",cr,character.title,description,playerAnimation.idleFrames[0],"Character",1,viewport,new Rect(viewport.position+cr.position-summaryScroll,cr.size));
            EndNavigationScrollView();
            if(Event.current.type==EventType.Repaint&&!inventoryHoveredThisFrame)inventoryHover=null;
        }
        void DrawInventoryTooltip()
        {
            // Inspection is drawn once after the entire screen, above all panels.
        }
        void DrawWaveTimeline(float y)
        {
            float left=FullCanvas.xMin+40,width=FullCanvas.width-80;
            OutlinedText(left,y-42,width,38,$"Danger {Rules.Difficulty} - Wave {Rules.Wave}",new GUIStyle(body){fontSize=29});
            RoundedBox(new Rect(left,y,width,90),new Color(0,0,0,.7f),7);
            for(int i=1;i<=20;i++)
            {
                float x=left+38+(i-1)*(width-76)/19f;Color color=i<=Rules.Wave?Color.yellow:paper;
                if(i==Rules.Wave){RoundedBox(new Rect(x-24,y+7,48,54),Color.yellow,24);RoundedBox(new Rect(x-21,y+10,42,48),ink,21);DrawHeroFace(new Rect(x-19,y+12,38,44));}
                else if(i==20){DrawIcon(catalog.Enemy("predator").image,new Rect(x-24,y+9,48,48));}
                else RoundedBox(new Rect(x-9,y+23,18,18),color,9);
                if(i%5==0)OutlinedText(x-25,y+60,50,27,i.ToString(),new GUIStyle(center){fontSize=22,normal={textColor=color}});
            }
        }
    }
}

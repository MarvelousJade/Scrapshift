using System;
using System.Linq;
using UnityEngine;

namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        bool randomCharacter;
        CharacterProfile SelectedCharacter=>catalog.data.characters[playerAnimation.lookIndex];
        string[] CharacterWeapons=>StarterChoices.Available(catalog,SelectedCharacter);
        Equipment SelectedWeapon=>catalog.Weapon(CharacterWeapons[Mathf.Clamp(startingWeapon,0,CharacterWeapons.Length-1)],1);
        RunRules previewRules;
        RunRules PreviewRules()
        {
            if(previewRules==null){previewRules=new RunRules(42);previewRules.Configure(catalog);}
            previewRules.Start(SelectedCharacter,SelectedWeapon.id);return previewRules;
        }
        void DrawLock(Rect r)
        {
            Vector2 p=r.center;
            RoundedBox(new Rect(p.x-12,p.y-18,24,26),new Color(.48f,.47f,.43f),9);
            RoundedBox(new Rect(p.x-7,p.y-13,14,21),panelColor,5);
            RoundedBox(new Rect(p.x-17,p.y-3,34,27),new Color(.48f,.47f,.43f),4);
            RoundedBox(new Rect(p.x-2,p.y+5,4,11),panelColor,2);
        }
        void CharacterGrid()
        {
            const int columns=13;const float step=70;
            for(int slot=0;slot<=catalog.data.rosterSlots;slot++)
            {
                var r=new Rect(265+slot%columns*step,598+slot/columns*66,60,58);
                if(slot==0)
                {
                    if(r.Contains(Event.current.mousePosition)&&Event.current.type==EventType.Repaint)randomCharacter=true;
                    if(MenuButton(r,"?",randomCharacter)){randomCharacter=true;ConfirmMenu();}
                }
                else if(slot<=catalog.data.characters.Length)
                {
                    int i=slot-1;
                    if(r.Contains(Event.current.mousePosition)&&Event.current.type==EventType.Repaint){randomCharacter=false;ChangeLook(i-playerAnimation.lookIndex);}
                    if(MenuButton(r,"",!randomCharacter&&playerAnimation.lookIndex==i)){randomCharacter=false;ChangeLook(i-playerAnimation.lookIndex);ConfirmMenu();}
                    Portrait(r.center+Vector2.up*2,35,playerAnimation.looks[i]);
                }
                else
                {
                    RoundedBox(r,panelColor,7);DrawLock(r);
                    GUI.Label(r,new GUIContent("","Locked"));
                }
            }
        }
        void WeaponGrid()
        {
            var choices=CharacterWeapons;
            float step=Mathf.Min(76,1180f/(choices.Length+1));float left=720-((choices.Length+1)*step-10)/2f;
            for(int slot=0;slot<=choices.Length;slot++)
            {
                Rect r=new Rect(left+slot*step,626,step-10,68);
                if(slot==0)
                {
                    if(MenuButton(r,"?"))
                    {
                        startingWeapon=UnityEngine.Random.Range(0,choices.Length);ConfirmMenu();
                    }
                    continue;
                }
                int i=slot-1;var weapon=catalog.Weapon(choices[i],1);
                if(r.Contains(Event.current.mousePosition)&&Event.current.type==EventType.Repaint)startingWeapon=i;
                if(MenuButton(r,"",i==startingWeapon)){startingWeapon=i;ConfirmMenu();}
                DrawIcon(weapon.image,new Rect(r.x+5,r.y+7,r.width-10,54));
            }
        }
        void CharacterDetails(float left)
        {
            string name=randomCharacter&&menuPage==1?"Random":SelectedCharacter.title;
            Text(left+20,164,530,58,name,new GUIStyle(heading){fontSize=40,alignment=TextAnchor.MiddleCenter});
            Frame(new Rect(left+20,244,180,245),new Color(.32f,.29f,.25f));
            if(randomCharacter&&menuPage==1)Text(left+20,300,180,120,"?",new GUIStyle(title){alignment=TextAnchor.MiddleCenter});
            else Portrait(new Vector2(left+110,365),100,playerAnimation.Look);
            string details=randomCharacter&&menuPage==1?"Choose an unlocked\ncharacter at random.":SelectedCharacter.description;
            if(randomCharacter&&menuPage==1)Text(left+222,246,326,263,details,new GUIStyle(body){fontSize=20});
            else DrawCharacterEffects(SelectedCharacter,left+220,246,328);
            if(!(randomCharacter&&menuPage==1)&&!string.IsNullOrEmpty(SelectedCharacter.DefaultWeapon))
            {
                RoundedBox(new Rect(left+20,499,180,61),new Color(.35f,.32f,.29f),8);
                var r=new Rect(left+146,505,48,49);RoundedBox(r,panelColor,5);
                DrawIcon(catalog.Weapon(SelectedCharacter.DefaultWeapon,1).image,new Rect(r.x+4,r.y+4,40,41));
            }
        }
        void DrawCharacterEffects(CharacterProfile profile,float x,float y,float width)
        {
            var style=new GUIStyle(inspectionBody){fontSize=18,richText=true};
            var lines=new System.Collections.Generic.List<(Sprite image,string label,float height)>();
            foreach(var row in profile.effectRows.Where(r=>r.kind!="weapon"))
            {
                string value=(row.value>0?"+":"")+row.value.ToString("0.##");
                string color=row.value<0?"#FF3B3B":"#76FF40";
                string stat=inspectionText.StatName(row.key).TrimStart('%',' ');
                string unit=StatPresentation.Unit(row.key);
                string colored="<color="+color+">"+value+unit+"</color>";
                string label=colored+" "+stat;
                Sprite image=StatPresentation.Icon(catalog,row.key);
                if(row.kind=="class")label=colored+" "+stat+" with "+System.Globalization.CultureInfo.InvariantCulture.TextInfo.ToTitleCase(row.extra)+" weapons";
                else if(row.kind=="weapon")
                {var weapon=catalog.Weapon(row.key,1);image=weapon.image;label="You start with <color=#76FF40>"+row.value.ToString("0")+" "+inspectionText.WeaponName(weapon)+"</color>";}
                else if(row.kind=="item")
                {var item=Array.Find(catalog.data.items,i=>i.id==row.key);image=item.image;label="Start with <color=#76FF40>"+row.value.ToString("0")+" "+inspectionText.ItemName(item)+"</color>";}
                else if(row.kind=="gain")label=stat+" modifications are "+(row.value<0?"reduced":"increased")+" by <color="+color+">"+Mathf.Abs(row.value).ToString("0")+"%</color>";
                else if(row.kind=="noMelee")label="<color=#FF3B3B>You can't equip melee weapons</color>";
                float h=style.CalcHeight(new GUIContent(label),width-30);
                lines.Add((image,label,h));
            }
            y+=Mathf.Max(0,(245-lines.Sum(l=>l.height+8))/2);
            foreach(var line in lines)
            {
                DrawIcon(line.image,new Rect(x,y+3,21,21));Text(x+30,y,width-30,line.height+4,line.label,style);y+=line.height+8;
            }
        }
    }
}

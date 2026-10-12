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
            float left=720-(Mathf.Min(10,choices.Length+1)*76-10)/2f;
            for(int slot=0;slot<=choices.Length;slot++)
            {
                Rect r=new Rect(left+slot%10*76,626+slot/10*79,66,68);
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
                DrawIcon(weapon.image,new Rect(r.x+7,r.y+7,52,54));
            }
        }
        void CharacterDetails(float left)
        {
            string name=randomCharacter&&menuPage==1?"Random":SelectedCharacter.title;
            Text(left+20,164,530,58,name,new GUIStyle(heading){fontSize=40,alignment=TextAnchor.MiddleCenter});
            Frame(new Rect(left+20,244,180,245),new Color(.32f,.29f,.25f));
            if(randomCharacter&&menuPage==1)Text(left+20,300,180,120,"?",new GUIStyle(title){alignment=TextAnchor.MiddleCenter});
            else Portrait(new Vector2(left+110,365),100,playerAnimation.Look);
            string details=randomCharacter&&menuPage==1?"Choose one of the four\nplayable characters at random.":SelectedCharacter.description;
            Text(left+222,246,326,263,details,new GUIStyle(body){fontSize=20});
            if(!(randomCharacter&&menuPage==1))Text(left+20,511,530,38,$"{PreviewRules().MaxHealth:0.##} starting Max HP",tiny);
        }
    }
}

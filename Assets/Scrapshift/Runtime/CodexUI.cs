using System;
using System.Linq;
using UnityEngine;

namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        int codexTab, codexSelected, codexTier=1, codexSort;
        bool codexReverse;
        Vector2 codexScroll, codexDetailScroll;
        void DrawCodexFace(CharacterProfile character,Rect r)
        {
            var hero=playerAnimation.idleFrames[0];float scale=Mathf.Min(r.width/hero.bounds.size.x,r.height/hero.bounds.size.y);
            Vector2 size=(Vector2)hero.bounds.size*scale;DrawIcon(hero,new Rect(r.center-size*.5f,size));
            int index=Array.IndexOf(catalog.data.characters,character);
            foreach(var layer in AppearanceResolver.Resolve(playerAnimation.looks[index],Array.Empty<ItemEntry>()))if(layer.order>=10)DrawAppearance(layer,r.center,scale);
        }
        sealed class CodexEntry
        {
            public string id,title,description,category;
            public Sprite image;
            public bool locked,known;
            public int tier=1,price;
            public long count;
            public Equipment weapon;
            public UnlockDefinition challenge;
            public CharacterProfile character;
        }
        CodexEntry RewardEntry(string reward)
        {
            var item=catalog.data.items.FirstOrDefault(i=>reward=="item_"+i.id);
            if(item!=null)return new CodexEntry{id=item.id,title=inspectionText.ItemName(item),description=inspectionText.ItemDescription(item),category="Item",image=item.image,tier=item.tier,price=item.value,locked=!item.available,count=progress.Total("item:"+item.id),known=progress.Total("item:"+item.id)>0};
            var weapon=catalog.data.equipment.Where(w=>reward=="weapon_"+w.id).OrderBy(w=>w.tier).FirstOrDefault();
            if(weapon!=null)return new CodexEntry{id=weapon.id,title=inspectionText.WeaponName(weapon),description=inspectionText.WeaponDescription(CodexBaseRules(),weapon),category=inspectionText.Sets(weapon),image=weapon.image,tier=weapon.tier,price=weapon.value,locked=!weapon.available,count=progress.Total("weapon:"+weapon.id),known=progress.Total("weapon:"+weapon.id)>0,weapon=weapon};
            var character=catalog.data.characters.FirstOrDefault(c=>reward=="character_"+c.id);
            if(character!=null)return new CodexEntry{id=character.id,character=character,title=character.title,description=CharacterEffectDescription(character),category="Character",image=playerAnimation.idleFrames[0],known=character.available,locked=!character.available};
            return new CodexEntry{id=reward,title=reward.Replace('_',' '),category="Difficulty",description="Available for your next run",known=progress.Unlocked(reward)};
        }
        RunRules codexBaseRules;
        RunRules CodexBaseRules(){if(Rules.Phase!=RunPhase.Title)return Rules;if(codexBaseRules==null){codexBaseRules=new RunRules();codexBaseRules.Configure(catalog);}return codexBaseRules;}
        CodexEntry[] SortCodex(CodexEntry[] entries)
        {
            var sorted=codexSort==1?entries.OrderBy(e=>e.title):codexSort==2?entries.OrderByDescending(e=>e.count):entries.OrderBy(e=>e.tier);
            return (codexReverse?sorted.Reverse():sorted).ToArray();
        }
        CodexEntry[] CodexEntries()
        {
            if(codexTab==0)return progress.Definitions.Where(d=>!d.id.StartsWith("unlock_difficulty_")).Select(d=>{
                var r=RewardEntry(d.reward);r.challenge=d;r.known=progress.Completed(d.id);r.locked=!r.known;return r;
            }).ToArray();
            if(codexTab==1)return SortCodex(catalog.data.items.Select(i=>RewardEntry("item_"+i.id)).ToArray());
            if(codexTab==2)return SortCodex(catalog.data.equipment.GroupBy(w=>w.id).Select(g=>g.OrderBy(w=>w.tier).First()).Select(w=>RewardEntry("weapon_"+w.id)).ToArray());
            return catalog.data.enemies.GroupBy(e=>e.id).Select(g=>g.First()).Select(e=>new CodexEntry{id=e.id,title=System.Globalization.CultureInfo.InvariantCulture.TextInfo.ToTitleCase(e.id.Replace('_',' ')),category=e.role,image=e.image,known=progress.Total("enemy:"+e.id)>0,count=progress.Total("enemy:"+e.id),description=$"HP: {e.health:0.##} (+{e.growth:0.##} per wave)\nDamage: {e.damage:0.##} (+{e.damageGrowth:0.##} per wave)\nSpeed: {e.speed:0.##}\nArmor: {e.armor:0.##}"}).ToArray();
        }
        void DrawPauseCodex()
        {
            float left=FullCanvas.xMin+65,width=FullCanvas.width-130;
            RoundedBox(new Rect(left-10,25,width+20,840),Color.black,32);
            RoundedBox(new Rect(left,35,width,820),new Color(.48f,.53f,.51f),25);
            RoundedBox(new Rect(left+13,125,width-26,610),new Color(.10f,.16f,.14f),15);
            string[] tabs={"Challenges","Items","Weapons","Enemies"};float tabW=(width-40)/4;
            for(int i=0;i<4;i++)if(MenuButton(new Rect(left+20+i*tabW,55,tabW-8,62),tabs[i],codexTab==i)){codexTab=i;codexSelected=0;codexTier=1;codexScroll=codexDetailScroll=Vector2.zero;}
            var entries=CodexEntries();if(entries.Length==0)return;
            codexSelected=Mathf.Clamp(codexSelected,0,entries.Length-1);
            int cols=codexTab==0?8:codexTab==1?7:codexTab==2?5:4;
            float gridW=width*.43f,cell=(gridW-38)/cols;
            bool sortable=codexTab==1||codexTab==2;
            if(sortable){if(Button(new Rect(left+20,138,gridW-85,38),"Sorted by "+new[]{"rarity","name","times acquired"}[codexSort]+" ▾")){codexSort=(codexSort+1)%3;codexSelected=0;codexScroll=Vector2.zero;}if(Button(new Rect(left+gridW-58,138,58,38),codexReverse?"↑":"↓")){codexReverse=!codexReverse;codexSelected=0;codexScroll=Vector2.zero;}}
            var viewport=new Rect(left+20,sortable?190:140,gridW,sortable?535:585);
            if(Event.current.type==EventType.KeyDown)
            {
                int step=Event.current.keyCode==KeyCode.RightArrow?1:Event.current.keyCode==KeyCode.LeftArrow?-1:Event.current.keyCode==KeyCode.DownArrow?cols:Event.current.keyCode==KeyCode.UpArrow?-cols:0;
                if(step!=0){codexSelected=Mathf.Clamp(codexSelected+step,0,entries.Length-1);codexDetailScroll=Vector2.zero;codexScroll.y=Mathf.Clamp(codexSelected/cols*cell-viewport.height+cell,0,float.MaxValue);sound.Play(SoundCue.InventoryFocus);Event.current.Use();}
            }
            codexScroll=GUI.BeginScrollView(viewport,codexScroll,new Rect(0,0,gridW-20,Mathf.Max(584,Mathf.Ceil(entries.Length/(float)cols)*cell)));
            for(int i=0;i<entries.Length;i++)
            {
                var e=entries[i];var r=new Rect(i%cols*cell,i/cols*cell,cell-8,cell-8);
                Frame(r,i==codexSelected?paper:InventoryTileColor(e.tier),2);
                if(e.locked)DrawLock(new Rect(r.x+12,r.y+12,r.width-24,r.height-24));
                else {var old=GUI.color;if(!e.known)GUI.color=Color.black;if(e.character!=null)DrawCodexFace(e.character,new Rect(r.x+7,r.y+7,r.width-14,r.height-14));else DrawIcon(e.image,new Rect(r.x+7,r.y+7,r.width-14,r.height-14));GUI.color=old;}
                if(GUI.Button(r,GUIContent.none,GUIStyle.none)){codexSelected=i;codexTier=e.tier;codexDetailScroll=Vector2.zero;sound.Play(SoundCue.InventoryFocus);}
            }
            GUI.EndScrollView();
            var selected=entries[codexSelected];float x=left+gridW+40,w=width-gridW-65;
            RoundedBox(new Rect(x,140,w,585),new Color(0,0,0,.72f),10);
            DrawCodexDetails(selected,new Rect(x+20,158,w-40,550));
            float completion=entries.Count(e=>e.known)/(float)entries.Length;
            RoundedBox(new Rect(left+25,749,width-50,30),Color.black,8);RoundedBox(new Rect(left+28,752,(width-56)*completion,24),new Color(.4f,.65f,.3f),6);
            Text(left+25,749,width-50,30,$"{completion*100:0}%  ({entries.Count(e=>e.known)}/{entries.Length})",center);
            if(Button(new Rect(left+width/2-200,793,400,48),"Back")){pauseCodex=false;if(Rules.Phase==RunPhase.Title)menuPage=0;}
        }
        void DrawCodexDetails(CodexEntry entry,Rect r,bool reward=false)
        {
            string label=entry.title,description=entry.description;
            bool reveal=entry.known||entry.challenge!=null;
            if(entry.weapon!=null&&reveal&&entry.challenge==null){var selected=catalog.Weapon(entry.id,codexTier)??entry.weapon;label=inspectionText.WeaponName(selected);entry.tier=selected.tier;}
            if(entry.challenge!=null)
            {
                label=inspectionText.T(entry.challenge.name,entry.challenge.id.Substring(5).Replace('_',' '));
                // Some reference names are format templates; never display raw translation placeholders.
                if(entry.challenge.name=="DIFFICULTY_NB")label="Danger "+entry.challenge.value;
                else if(label.Contains("{")||label.StartsWith("CHAL_"))label=System.Globalization.CultureInfo.InvariantCulture.TextInfo.ToTitleCase(entry.challenge.id.Substring(5).Replace('_',' '));
                description=(entry.known?"<color=#76FF40>Completed</color>":"Not completed")+"\n"+progress.Requirement(entry.challenge.reward)+"\n\nReward: "+entry.title+"\n"+entry.description;
            }
            else if(entry.locked){label="???";description=progress.Requirement((codexTab==1?"item_":"weapon_")+entry.id);}
            else if(!entry.known){label="???";description=codexTab==3?"Defeat this enemy to unlock more information.":"Acquire this "+(codexTab==1?"item":"weapon")+" to unlock more information.";}
            Text(r.x,r.y,r.width,75,label,new GUIStyle(heading){fontSize=27,normal={textColor=TierColor(entry.tier)}});
            if(entry.locked&&entry.challenge==null)DrawLock(new Rect(r.x,r.y+82,92,92));else {var old=GUI.color;if(!reveal)GUI.color=Color.black;if(entry.character!=null)DrawCodexFace(entry.character,new Rect(r.x,r.y+82,92,92));else DrawIcon(entry.image,new Rect(r.x,r.y+82,92,92));GUI.color=old;}
            Text(r.x+112,r.y+84,r.width-112,85,entry.category+"\n"+(entry.challenge!=null||reward?"":codexTab==3?"Defeated: "+entry.count:"Acquired: "+entry.count),body);
            float y=r.y+190;
            if(entry.weapon!=null&&reveal&&entry.challenge==null)
            {
                var tiers=catalog.data.equipment.Where(a=>a.id==entry.id).OrderBy(a=>a.tier).ToArray();
                if(!tiers.Any(a=>a.tier==codexTier))codexTier=tiers[0].tier;
                for(int i=0;i<tiers.Length;i++)if(MenuButton(new Rect(r.x+i*75,y,68,40),new[]{"","I","II","III","IV"}[tiers[i].tier],codexTier==tiers[i].tier))codexTier=tiers[i].tier;
                var weapon=tiers.First(a=>a.tier==codexTier);description=inspectionText.WeaponDescription(CodexBaseRules(),weapon);entry.price=weapon.value;y+=53;
            }
            if(!reward&&entry.challenge==null&&codexTab!=3&&!entry.locked){Text(r.x,y,r.width,28,"Base price: "+entry.price,body);y+=36;}
            var viewport=new Rect(r.x,y,r.width,r.yMax-y);float h=RichHeight(description,r.width-24)+20;
            codexDetailScroll=GUI.BeginScrollView(viewport,codexDetailScroll,new Rect(0,0,r.width-20,Mathf.Max(h,viewport.height-1)));
            RichText(description,new Rect(0,0,r.width-24,h));GUI.EndScrollView();
        }
        bool DrawUnlockReward()
        {
            if(progress.NewlyUnlocked.Count==0)return false;
            Overlay();var reward=RewardEntry(progress.NewlyUnlocked[0]);reward.known=true;reward.locked=false;
            OutlinedText(350,65,740,85,"Unlocked!",new GUIStyle(title){fontSize=56,alignment=TextAnchor.MiddleCenter});
            RoundedBox(new Rect(380,175,680,520),Color.black,14);
            DrawCodexDetails(reward,new Rect(410,195,620,480),true);
            if(Button(new Rect(470,745,500,60),"Continue"))progress.NewlyUnlocked.RemoveAt(0);
            return true;
        }
    }
}

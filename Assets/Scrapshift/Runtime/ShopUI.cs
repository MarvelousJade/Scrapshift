using UnityEngine;
using System.Linq;
namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        int selectedWeapon=-1;
        float ContentLeft=>FullCanvas.xMin+24;
        float StatsLeft=>FullCanvas.xMax-360;
        float ContentWidth=>StatsLeft-24-ContentLeft;
        float OfferWidth=>(ContentWidth-24)/4;
        float OfferX(int index)=>ContentLeft+index*(OfferWidth+8);
        readonly System.Collections.Generic.Dictionary<string,Vector2> detailScroll=new System.Collections.Generic.Dictionary<string,Vector2>();
        Vector2 itemScroll,weaponScroll;
        string inventoryHover;bool inventoryHoveredThisFrame;
        bool InventoryHover(string id,Rect local,Rect viewport,Vector2 scroll)
        {
            var root=new Rect(viewport.position+local.position-scroll,local.size);
            bool over=inspectionReview?forcedInspection==id:keyboardInspection?focusedInspection==id:viewport.Contains(lastInspectionMouse)&&root.Contains(lastInspectionMouse);
            if(over&&Event.current.type==EventType.Repaint)
            {
                inventoryHoveredThisFrame=true;
                if(inventoryHover!=id){inventoryHover=id;sound.Play(SoundCue.InventoryFocus);}
            }
            return over;
        }
        bool RerollButton(Rect r)
        {
            bool hover=r.Contains(Event.current.mousePosition);
            RoundedBox(r,hover?paper:Color.black,9);
            Rect key=new Rect(r.x+18,r.center.y-14,28,28);
            RoundedBox(key,new Color(.18f,.18f,.18f),3);RoundedBox(new Rect(key.x+3,key.y+3,22,22),new Color(.35f,.35f,.35f),2);
            Text(key.x,key.y,28,28,"F",new GUIStyle(center){fontSize=18});
            var text=new GUIStyle(center){fontSize=27,wordWrap=false};text.normal.textColor=hover?ink:paper;
            string label="REROLL - "+Rules.RerollPrice;
            while(text.fontSize>16&&text.CalcSize(new GUIContent(label)).x>r.width-120)text.fontSize--;
            Text(r.x+55,r.y,r.width-108,r.height,label,text);
            DrawIcon(catalog.scrapShapes[0],new Rect(r.xMax-49,r.center.y-17,34,34));
            if(GUI.enabled&&hover&&Event.current.type==EventType.Repaint&&lastUiHover!=r){lastUiHover=r;sound.Play(SoundCue.UiFocus);}
            bool clicked=GUI.Button(r,GUIContent.none,GUIStyle.none);if(clicked)sound.Play(SoundCue.UiPress);return clicked;
        }
        bool MoneyButton(Rect r,string label,bool large=false)
        {
            bool pressed=Button(r,"");
            var style=new GUIStyle(center){fontSize=large?36:24,wordWrap=false};
            style.normal.textColor=GUI.enabled?(r.Contains(Event.current.mousePosition)?ink:paper):coral;
            float iconSize=large?34:30;
            float textWidth=Mathf.Min(r.width-iconSize-20,style.CalcSize(new GUIContent(label)).x+8);
            float start=r.center.x-(textWidth+iconSize+8)/2;
            Text(start,r.y,textWidth,r.height,label,style);
            DrawIcon(catalog.scrapShapes[0],new Rect(start+textWidth+8,r.center.y-iconSize/2,iconSize,iconSize));
            return pressed;
        }
        void DrawIcon(Sprite sprite,Rect r)
        {
            if(sprite==null)return;
            var preview=AtlasIconMask.Preview(sprite);if(preview!=null){GUI.DrawTexture(r,preview,ScaleMode.ScaleToFit,true);return;}
            Rect source = sprite.rect;
            // Half-texel inset prevents bilinear sampling of neighboring atlas cells.
            source=Rect.MinMaxRect(source.xMin+.5f,source.yMin+.5f,source.xMax-.5f,source.yMax-.5f);
            float scale = Mathf.Min(r.width / source.width, r.height / source.height);
            Rect target = new Rect(r.center.x - source.width * scale / 2, r.center.y - source.height * scale / 2,
                source.width * scale, source.height * scale);
            foreach(var region in AtlasIconMask.Regions(sprite,source))
            {
                var section=new Rect(target.x+(region.x-source.x)*scale,target.y+(source.yMax-region.yMax)*scale,region.width*scale,region.height*scale);
                GUI.DrawTextureWithTexCoords(section,sprite.texture,new Rect(region.x/sprite.texture.width,region.y/sprite.texture.height,region.width/sprite.texture.width,region.height/sprite.texture.height),true);
            }
        }
        bool secondaryStats;Vector2 secondaryStatsScroll;
        static readonly string[] secondaryStatKeys={"pickup_range","consumable_heal","xp_gain","items_price","weapons_price","recycling_gains","harvesting_growth","explosion_damage","explosion_size","piercing","piercing_damage","bounce","knockback","enemy_health","enemy_damage","enemy_speed","number_of_enemies","gold_drops","trees","structure_attack_speed","burning_spread","fog_visibility"};
        void DrawStatsPanel(bool summary=false)
        {
            var previousMatrix=GUI.matrix;
            GUI.matrix=previousMatrix*Matrix4x4.Translate(new Vector3(summary?-1050:StatsLeft-1110,summary?80:Rules.Phase==RunPhase.Upgrade?143:Rules.Phase==RunPhase.Paused?40:0,0));
            RoundedBox(new Rect(1110,24,336,summary?680:Rules.Phase==RunPhase.Paused?708:650),new Color(.065f,.06f,.05f,.93f),8);
            Text(1130,43,296,43,"Stats",new GUIStyle(heading) { alignment=TextAnchor.MiddleCenter });
            if(Button(new Rect(1125,94,133,35),"Primary"))secondaryStats=false;
            if(Button(new Rect(1266,94,135,35),"Secondary"))secondaryStats=true;
            if(secondaryStats)
            {
                var viewport=new Rect(1124,143,310,505);secondaryStatsScroll=GUI.BeginScrollView(viewport,secondaryStatsScroll,new Rect(0,0,290,secondaryStatKeys.Length*57));
                for(int i=0;i<secondaryStatKeys.Length;i++){string key=secondaryStatKeys[i];RichText(SpecialItemEffects.Description(key,Rules.Stat(key)),new Rect(7,i*57,272,55),17);}
                GUI.EndScrollView();GUI.matrix=previousMatrix;return;
            }
            Text(1133,138,250,30,$"Current Level                         {Rules.Level}",new GUIStyle(body){fontSize=17});
            var names=StatPresentation.Keys;
            var right=new GUIStyle(inspectionBody) { alignment=TextAnchor.MiddleRight };
            for(int i=0;i<names.Length;i++)
            {
                float value=names[i]=="max_hp"?Rules.MaxHealth:Rules.Stat(names[i]);
                float y=174+i*26;
                DrawStatIcon(names[i],new Rect(1125,y+2,24,24));
                var label=new GUIStyle(inspectionBody);
                label.normal.textColor=value<0?coral:value>0?new Color(.39f,1,.29f):paper;
                Text(1157,y,188,32,inspectionText.StatName(names[i]),label);
                right.normal.textColor=label.normal.textColor;
                Text(1381,y,49,32,value.ToString("0"),right);
                Inspect("stat:"+names[i],new Rect(1120,y,312,25),inspectionText.StatName(names[i]),inspectionText.StatDescription(Rules,names[i]),StatPresentation.Icon(catalog,names[i]));
            }
            
            GUI.matrix=previousMatrix;
        }

        void DrawOfferDetail(ShopOffer offer,float x,float y,float width,RunRules preview=null)
        {
            if(offer.weapon!=null)
            {
                var w=offer.weapon;
                var style=new GUIStyle(inspectionBody){fontSize=18};
                var content=new GUIContent(inspectionText.WeaponDescription(preview??Rules,w));
                float height=RichHeight(content.text,width-18,style.fontSize);
                var key=w.id+":"+x;
                detailScroll.TryGetValue(key,out var scroll);
                scroll=GUI.BeginScrollView(new Rect(x,y,width,Mathf.Min(218,height+8)),scroll,new Rect(0,0,width-18,height+4));
                RichText(content.text,new Rect(0,0,width-18,height+4),style.fontSize);GUI.EndScrollView();detailScroll[key]=scroll;
            }
            else
            {
                var style=new GUIStyle(inspectionBody){fontSize=19,richText=true};
                var content=new GUIContent(inspectionText.ItemDescription(offer.item));
                float height=RichHeight(content.text,width-18,style.fontSize);
                var key="item:"+offer.item.id+":"+x;detailScroll.TryGetValue(key,out var scroll);
                scroll=GUI.BeginScrollView(new Rect(x,y,width,Mathf.Min(218,height+8)),scroll,new Rect(0,0,width-18,height+4));
                RichText(content.text,new Rect(0,0,width-18,height+4),style.fontSize);GUI.EndScrollView();detailScroll[key]=scroll;
            }
        }

        void ShopUI()
        {
            if(Event.current.type==EventType.Repaint)inventoryHoveredThisFrame=false;
            Box(FullCanvas,new Color(.19f,.175f,.15f,.76f));
            OutlinedText(ContentLeft,32,420,52,$"Shop (Wave {Rules.Wave})",heading);
            DrawIcon(catalog.scrapShapes[0],new Rect(550,30,42,48));
            Text(602,30,180,48,Rules.ShopBalance.ToString("0")+(Rules.UsesHealthShop?" HP":""),new GUIStyle(number){fontSize=40,alignment=TextAnchor.MiddleLeft});
            GUI.enabled=Rules.CanReroll;
            if(RerollButton(new Rect(StatsLeft-318,27,294,56)))RerollUpgrade();
            GUI.enabled=true;
            DrawStatsPanel();
            for(int i=0;i<4;i++)
            {
                float x=OfferX(i),width=OfferWidth;var offer=Rules.Shop[i];
                TierCard(new Rect(x,118,width,402),offer==null?1:offer.Tier);
                if(offer==null){Text(x+14,292,width-28,45,"Sold",center);continue;}
                RoundedBox(new Rect(x+14,132,68,74),new Color(.075f,.072f,.065f),7);
                DrawIcon(offer.Image,new Rect(x+18,137,60,64));
                var offerTitle=offer.Title=="Smg"?"SMG":offer.Title;var offerStyle=new GUIStyle(body);offerStyle.normal.textColor=TierColor(offer.Tier);
                foreach(var word in offerTitle.Split(' '))while(offerStyle.fontSize>16&&offerStyle.CalcSize(new GUIContent(word)).x>width-107)offerStyle.fontSize--;
                float titleHeight=Mathf.Clamp(offerStyle.CalcHeight(new GUIContent(offerTitle),width-107),26,54);
                Text(x+93,132,width-107,titleHeight,offerTitle,offerStyle);
                Text(x+93,132+titleHeight,width-107,54,offer.weapon!=null?inspectionText.Sets(offer.weapon):"Item",new GUIStyle(body){fontSize=18,normal={textColor=new Color(.89f,.85f,.62f)}});
                if(offer.weapon!=null)InspectWeapon("offer:"+i,offer.weapon,new Rect(x+12,132,width-24,86));
                DrawOfferDetail(offer,x+16,225,width-32);
                GUI.enabled=Rules.CanBuy(i);
                string price=offer.weapon!=null&&Rules.Inventory.Count==Rules.WeaponSlots&&!Rules.CanBuy(i)&&Rules.Scrap>=Rules.Price(offer)?"Slots full":Rules.Price(offer).ToString();
                if(MoneyButton(new Rect(x+(width-154)/2,451,154,55),price,true))
                {Rules.Buy(i);selectedWeapon=-1;SyncEquipment();sound.Play(SoundCue.Install);}
                GUI.enabled=true;
                if(Button(new Rect(x+12,528,width-24,43),offer.locked?"Locked":"Lock"))
                {Rules.ToggleLock(i);sound.Play(SoundCue.Reroll);}
            }
            float weaponLeft=StatsLeft-292,itemWidth=weaponLeft-ContentLeft-24;
            Text(ContentLeft,618,100,40,"Items",heading);
            Box(new Rect(ContentLeft+105,637,Mathf.Max(0,weaponLeft-318-ContentLeft-113),2),new Color(.4f,.49f,.41f,.45f));DrawItemSorting(new Rect(weaponLeft-318,618,290,38));
            Text(weaponLeft,618,280,40,$"Weapons ({Rules.Inventory.Count}/{Rules.WeaponSlots})",heading);
            int columns=Mathf.Max(1,Mathf.FloorToInt((itemWidth-16)/88));
            var itemViewport=new Rect(ContentLeft,670,itemWidth,180);var sortedItems=SortedItems();
            itemScroll=GUI.BeginScrollView(itemViewport,itemScroll,new Rect(0,0,itemWidth-18,Mathf.Max(160,Mathf.Ceil((sortedItems.Length+1)/(float)columns)*88)));
            for(int i=0;i<sortedItems.Length;i++)
            {
                float x=i%columns*88,y=i/columns*88;
                Frame(new Rect(x,y,80,80),InventoryHover("item:"+Rules.Items.IndexOf(sortedItems[i]),new Rect(x,y,80,80),itemViewport,itemScroll)?paper:InventoryTileColor(sortedItems[i].tier),2);
                DrawIcon(sortedItems[i].image,new Rect(x+7,y+6,66,66));
                int owned=Rules.Items.Count(item=>item.id==sortedItems[i].id);if(owned>1)OutlinedText(x+50,y+54,28,24,owned.ToString(),tiny);
                InspectItem("item:"+Rules.Items.IndexOf(sortedItems[i]),sortedItems[i],new Rect(x,y,80,80),itemViewport,itemScroll);
            }
            var character=Rules.Character??SelectedCharacter;
            float cx=sortedItems.Length%columns*88,cy=sortedItems.Length/columns*88;
            var characterRect=new Rect(cx,cy,80,80);
            Frame(characterRect,InventoryHover("character",characterRect,itemViewport,itemScroll)?paper:InventoryTileColor(1),2);
            DrawHeroFace(new Rect(cx+13,cy+8,54,64));
            string description=string.Join("\n",character.effectRows.Where(row=>row.kind!="weapon").Select(row=>(StatPresentation.Keys.Contains(row.key)?"[["+row.key+"]] ":"• ")+CharacterEffectText(character,row).label));
            Inspect("character",characterRect,character.title,description,playerAnimation.idleFrames[0],"Character",1,itemViewport,new Rect(itemViewport.position+characterRect.position-itemScroll,characterRect.size));
            GUI.EndScrollView();
            var weaponViewport=new Rect(weaponLeft,670,270,180);
            weaponScroll=GUI.BeginScrollView(weaponViewport,weaponScroll,new Rect(0,0,254,Mathf.Max(174,Mathf.Ceil(Rules.Inventory.Count/3f)*86)));
            for(int i=0;i<Rules.Inventory.Count;i++)
            {
                float x=2+i%3*84,y=2+i/3*86;var w=i<Rules.Inventory.Count?Rules.Inventory[i]:null;
                Frame(new Rect(x,y,82,80),selectedWeapon==i||InventoryHover("weapon:"+i,new Rect(x,y,82,80),weaponViewport,weaponScroll)?paper:InventoryTileColor(w==null?1:w.tier),2);
                if(w!=null)
                {
                    DrawIcon(w.image,new Rect(x+8,y+8,66,64));
                    
                    InspectWeapon("weapon:"+i,w,new Rect(x,y,82,80),weaponViewport,weaponScroll);
                }
                if(w!=null&&GUI.enabled&&Event.current.type==EventType.MouseDown&&Event.current.button==1&&new Rect(x,y,82,80).Contains(Event.current.mousePosition)){selectedWeapon=i;Event.current.Use();}
                if(w!=null&&GUI.Button(new Rect(x,y,82,80),GUIContent.none,GUIStyle.none))selectedWeapon=i;
            }
            GUI.EndScrollView();
            if(Button(new Rect(StatsLeft,686,336,56),$"Go (Wave {Mathf.Max(1,Rules.Wave+1-(int)Rules.Stat("item_hourglass"))})")) {selectedWeapon=-1;AdvanceWave();}
            if(!string.IsNullOrEmpty(GUI.tooltip))
            {
                Frame(new Rect(30,469,335,172),ink);
                Text(47,484,303,142,GUI.tooltip,body);
            }
            DrawSetTooltip();DrawItemSortMenu();
            if(Event.current.type==EventType.Repaint&&!inventoryHoveredThisFrame)inventoryHover=null;
        }

        WeaponSet hoveredSet;
        Vector2 setScroll;
        void DrawWeaponSets()
        {
            hoveredSet=null;
            Text(1130,531,266,28,"Weapon sets",new GUIStyle(body) {fontSize=20,fontStyle=FontStyle.Bold});
            var active=System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Where(WeaponSets.All,set=>Rules.SetCount(set)>0));
            setScroll=GUI.BeginScrollView(new Rect(1124,565,283,100),setScroll,new Rect(0,0,265,active.Length*46));
            for(int i=0;i<active.Length;i++)
            {
                var set=active[i];int count=Rules.SetCount(set);float y=i*46;
                var label=new GUIStyle(tiny){fontSize=15};label.normal.textColor=count>=2?new Color(.39f,1,.29f):tiny.normal.textColor;
                Text(6,y,110,44,$"{set.Name} {count}/6",label);
                Text(119,y,140,44,count>=2?set.Description(count):"Needs 2 weapons",label);
                if(new Rect(0,y,265,44).Contains(Event.current.mousePosition))hoveredSet=set;
            }
            GUI.EndScrollView();
        }
        void DrawSetTooltip()
        {
            if(!setReview)return; // Normal inspection owns the attached set panels.
            if(setReview && (setReviewStage==1 || setReviewStage==2))hoveredSet=WeaponSets.Primitive;
            if(hoveredSet==null)return;
            int count=Rules.SetCount(hoveredSet);
            Frame(new Rect(754,238,336,282),ink,3);
            Text(773,253,298,32,$"{hoveredSet.Name} ({count}/6 equipped)",body);
            Text(773,289,298,26,count>=2?"Current total highlighted":"Inactive until 2 are equipped",tiny);
            for(int n=2;n<=6;n++)
            {
                var row=new GUIStyle(tiny);row.normal.textColor=n==Mathf.Min(6,count)?new Color(.39f,1,.29f):tiny.normal.textColor;
                Text(773,322+(n-2)*29,298,27,$"({n}) {hoveredSet.Description(n)}",row);
            }
            Text(773,475,298,26,"Duplicates count; tiers count equally.",tiny);
        }

        void DrawWardrobePreview()
        {
            Text(1130,675,260,25,"YOUR GEAR",tiny);
            Portrait(new Vector2(1260,735),60,playerAnimation.Look,true);
        }

        void DrawAppearance(AppearanceLayer layer, Vector2 origin, float scale)
        {
            Vector2 size = layer.image.rect.size / layer.image.pixelsPerUnit * layer.scale * scale;
            Vector2 center = origin + new Vector2(layer.offset.x, -layer.offset.y) * scale;
            DrawIcon(layer.image, new Rect(center - size * .5f, size));
        }
    }
}

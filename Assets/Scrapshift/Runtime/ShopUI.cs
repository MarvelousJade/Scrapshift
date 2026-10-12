using UnityEngine;
namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        int selectedWeapon=-1;
        readonly System.Collections.Generic.Dictionary<string,Vector2> detailScroll=new System.Collections.Generic.Dictionary<string,Vector2>();
        Vector2 itemScroll;
        void DrawIcon(Sprite sprite,Rect r)
        {
            if(sprite==null)return;
            Rect source = sprite.rect;
            float scale = Mathf.Min(r.width / source.width, r.height / source.height);
            Rect target = new Rect(r.center.x - source.width * scale / 2, r.center.y - source.height * scale / 2,
                source.width * scale, source.height * scale);
            GUI.DrawTextureWithTexCoords(target, sprite.texture, new Rect(source.x / sprite.texture.width,
                source.y / sprite.texture.height, source.width / sprite.texture.width, source.height / sprite.texture.height), true);
        }
        void DrawStatsPanel(bool summary=false)
        {
            var previousMatrix=GUI.matrix;
            if(summary)GUI.matrix=previousMatrix*Matrix4x4.Translate(new Vector3(-1050,80,0));
            Box(new Rect(1110,24,306,summary?680:770),new Color(.065f,.06f,.05f,.93f));
            Text(1130,43,266,43,"Stats",new GUIStyle(heading) { alignment=TextAnchor.MiddleCenter });
            Text(1133,90,250,30,$"Level {Rules.Level}",body);
            var names=StatPresentation.Keys;
            var right=new GUIStyle(inspectionBody) { alignment=TextAnchor.MiddleRight };
            for(int i=0;i<names.Length;i++)
            {
                float value=names[i]=="max_hp"?Rules.MaxHealth:Rules.Stat(names[i]);
                float y=122+i*25;
                DrawStatIcon(names[i],new Rect(1125,y+2,24,24));
                var label=new GUIStyle(inspectionBody);
                label.normal.textColor=value<0?coral:value>0?new Color(.39f,1,.29f):paper;
                Text(1157,y,188,32,inspectionText.StatName(names[i]),label);
                right.normal.textColor=label.normal.textColor;
                Text(1351,y,49,32,value.ToString("0"),right);
                Inspect("stat:"+names[i],new Rect(1120,y,282,25),inspectionText.StatName(names[i]),inspectionText.StatDescription(Rules,names[i]),StatPresentation.Icon(catalog,names[i]));
            }
            if(!summary)DrawWardrobePreview();
            GUI.matrix=previousMatrix;
        }

        void DrawOfferDetail(ShopOffer offer,float x,float y,float width,RunRules preview=null)
        {
            if(offer.weapon!=null)
            {
                var w=offer.weapon;
                var style=new GUIStyle(inspectionBody){fontSize=18};
                var content=new GUIContent(inspectionText.WeaponDescription(preview??Rules,w));
                float height=style.CalcHeight(content,width-18);
                var key=w.id+":"+x;
                detailScroll.TryGetValue(key,out var scroll);
                scroll=GUI.BeginScrollView(new Rect(x,y,width,Mathf.Min(218,height+8)),scroll,new Rect(0,0,width-18,height+4));
                GUI.Label(new Rect(0,0,width-18,height+4),content,style);GUI.EndScrollView();detailScroll[key]=scroll;
            }
            else
            {
                var style=new GUIStyle(inspectionBody){fontSize=19,richText=true};
                var content=new GUIContent(inspectionText.ItemDescription(offer.item));
                float height=style.CalcHeight(content,width-18);
                var key="item:"+offer.item.id+":"+x;detailScroll.TryGetValue(key,out var scroll);
                scroll=GUI.BeginScrollView(new Rect(x,y,width,Mathf.Min(218,height+8)),scroll,new Rect(0,0,width-18,height+4));
                GUI.Label(new Rect(0,0,width-18,height+4),content,style);GUI.EndScrollView();detailScroll[key]=scroll;
            }
        }

        void ShopUI()
        {
            Box(FullCanvas,new Color(.19f,.175f,.15f,.97f));
            OutlinedText(24,32,420,52,$"Shop (Wave {Rules.Wave})",heading);
            DrawIcon(catalog.scrapShapes[0],new Rect(558,34,34,40));
            Text(604,26,170,58,Rules.Scrap.ToString(),number);
            GUI.enabled=Rules.CanReroll;
            if(Button(new Rect(804,27,282,56),$"Reroll - {Rules.RerollPrice}"))RerollUpgrade();
            GUI.enabled=true;
            DrawStatsPanel();
            for(int i=0;i<4;i++)
            {
                float x=24+i*268;var offer=Rules.Shop[i];
                Frame(new Rect(x,124,252,432),ink,2);
                if(offer==null){Text(x+14,292,224,45,"Sold",center);continue;}
                Box(new Rect(x+3,127,246,4),TierColor(offer.Tier));
                Box(new Rect(x+14,143,68,74),new Color(.17f,.16f,.15f));
                DrawIcon(offer.Image,new Rect(x+18,148,60,64));
                var offerTitle=offer.Title=="Smg"?"SMG":offer.Title;var offerStyle=new GUIStyle(body);
                foreach(var word in offerTitle.Split(' '))while(offerStyle.fontSize>16&&offerStyle.CalcSize(new GUIContent(word)).x>145)offerStyle.fontSize--;
                Text(x+93,141,145,76,offerTitle,offerStyle);
                Text(x+16,224,220,27,offer.weapon!=null?(WeaponSets.For(offer.weapon)?.Name??"Weapon")+"  /  Tier "+offer.Tier:"Item",tiny);
                if(offer.weapon!=null)InspectWeapon("offer:"+i,offer.weapon,new Rect(x+12,140,228,110));
                DrawOfferDetail(offer,x+16,270,220);
                GUI.enabled=Rules.CanBuy(i);
                string price=offer.weapon!=null&&Rules.Inventory.Count==6&&!Rules.CanBuy(i)&&Rules.Scrap>=Rules.Price(offer)?"Slots full":Rules.Price(offer).ToString();
                if(Button(new Rect(x+49,491,154,49),price))
                {Rules.Buy(i);selectedWeapon=-1;SyncEquipment();sound.Play(SoundCue.Install);}
                GUI.enabled=true;
                if(Button(new Rect(x+12,568,228,40),offer.locked?"Locked":"Lock"))
                {Rules.ToggleLock(i);sound.Play(SoundCue.Reroll);}
            }
            Text(24,635,470,40,$"Items ({Rules.Items.Count})",heading);
            Text(556,635,530,40,$"Weapons ({Rules.Inventory.Count}/6)",heading);
            Box(new Rect(24,681,1062,2),new Color(.38f,.36f,.31f));
            itemScroll=GUI.BeginScrollView(new Rect(24,698,510,155),itemScroll,new Rect(0,0,490,Mathf.Max(132,Mathf.Ceil(Rules.Items.Count/7f)*66)));
            for(int i=0;i<Rules.Items.Count;i++)
            {
                float x=i%7*69,y=i/7*66;
                Frame(new Rect(x,y,60,58),new Color(.24f,.23f,.20f),2);
                DrawIcon(Rules.Items[i].image,new Rect(x+7,y+6,46,46));
                InspectItem("item:"+i,Rules.Items[i],new Rect(x,y,60,58),new Rect(24,698,510,155),itemScroll);
            }
            GUI.EndScrollView();
            for(int i=0;i<6;i++)
            {
                float x=556+i*89;var w=i<Rules.Inventory.Count?Rules.Inventory[i]:null;
                Frame(new Rect(x,700,82,80),selectedWeapon==i?paper:w==null?panelColor:TierColor(w.tier),2);
                Box(new Rect(x+4,704,74,72),new Color(.19f,.18f,.16f));
                if(w!=null)
                {
                    DrawIcon(w.image,new Rect(x+8,712,66,47));
                    Text(x+9,753,64,25,$"Tier {w.tier}",tiny);
                    InspectWeapon("weapon:"+i,w,new Rect(x,700,82,80));
                }
                if(w!=null&&GUI.enabled&&Event.current.type==EventType.MouseDown&&Event.current.button==1&&new Rect(x,700,82,80).Contains(Event.current.mousePosition)){selectedWeapon=i;Event.current.Use();}
                if(w!=null&&GUI.Button(new Rect(x,700,82,80),GUIContent.none,GUIStyle.none))selectedWeapon=i;
            }
            if(Button(new Rect(1110,806,306,61),$"Go (Wave {Rules.Wave+1})")) {selectedWeapon=-1;AdvanceWave();}
            if(!string.IsNullOrEmpty(GUI.tooltip))
            {
                Frame(new Rect(30,469,335,172),ink);
                Text(47,484,303,142,GUI.tooltip,body);
            }
            DrawSetTooltip();
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

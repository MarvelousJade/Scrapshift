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
            Rect source = sprite.rect;
            float scale = Mathf.Min(r.width / source.width, r.height / source.height);
            Rect target = new Rect(r.center.x - source.width * scale / 2, r.center.y - source.height * scale / 2,
                source.width * scale, source.height * scale);
            GUI.DrawTextureWithTexCoords(target, sprite.texture, new Rect(source.x / sprite.texture.width,
                source.y / sprite.texture.height, source.width / sprite.texture.width, source.height / sprite.texture.height), true);
        }
        void DrawStatsPanel()
        {
            Box(new Rect(1110,24,306,750),new Color(.065f,.06f,.05f,.93f));
            Text(1130,43,266,43,"Stats",new GUIStyle(heading) { alignment=TextAnchor.MiddleCenter });
            Text(1133,90,250,30,$"Level {Rules.Level}",body);
            string[] names={"max_hp","hp_regeneration","lifesteal","percent_damage","melee_damage","ranged_damage","attack_speed","crit_chance","armor","dodge","speed","range","harvesting"};
            var right=new GUIStyle(body) { alignment=TextAnchor.MiddleRight };
            for(int i=0;i<names.Length;i++)
            {
                float value=names[i]=="max_hp"?Rules.MaxHealth:Rules.Stat(names[i]);
                float y=130+i*28;
                var entry=System.Array.Find(catalog.data.upgrades,u=>u.stat==names[i]);
                if(entry!=null)DrawIcon(entry.image,new Rect(1125,y+2,24,25));
                var label=new GUIStyle(body) { fontSize=18 };
                label.normal.textColor=value<0?coral:value>0?new Color(.39f,1,.29f):paper;
                Text(1157,y,198,32,ItemEntry.StatName(names[i]),label);
                right.normal.textColor=label.normal.textColor;
                Text(1351,y,45,32,value.ToString("0"),right);
            }
            DrawWeaponSets();
            DrawWardrobePreview();
        }

        void DrawOfferDetail(ShopOffer offer,float x,float y,float width)
        {
            if(offer.weapon!=null)
            {
                var w=offer.weapon;
                float damage=Mathf.Max(1,w.damage+Rules.Stat(w.melee?"melee_damage":"ranged_damage"))*Rules.DamageScale;
                string detail=$"Damage: <color=#79FF62>{damage:0.#}</color>"+(w.pellets>1?$" x {w.pellets}":"")+
                    $"\nCritical: x2 ({Mathf.Clamp01(Rules.CritChance+w.crit)*100:0}% chance)"+
                    $"\nCooldown: {Mathf.Max(.035f,w.interval/Rules.FireRate):0.00}s"+
                    $"\nKnockback: {w.knockback:0.#}"+
                    $"\nRange: {Rules.WeaponRange(w):0.0}"+
                    (w.piercing>0?$"\nPiercing: {w.piercing}":"");
                var style=new GUIStyle(body) { fontSize=18,richText=true };
                Text(x,y,width,218,detail,style);
            }
            else
            {
                for(int i=0;i<offer.item.effects.Length;i++)
                {
                    var effect=offer.item.effects[i]; var style=new GUIStyle(body) { fontSize=19 };
                    style.normal.textColor=effect.value<0?coral:new Color(.39f,1,.29f);
                    Text(x,y+i*57,width,56,ItemEntry.FormatStat(effect.key,effect.value),style);
                }
            }
        }

        void ShopUI()
        {
            Box(new Rect(0,0,1440,900),new Color(.19f,.175f,.15f,.97f));
            OutlinedText(24,32,420,52,$"Shop (Wave {Rules.Wave})",heading);
            DrawIcon(catalog.scrapShapes[0],new Rect(558,34,34,40));
            Text(604,26,170,58,Rules.Scrap.ToString(),number);
            GUI.enabled=Rules.CanReroll;
            if(Button(new Rect(804,27,282,56),$"REROLL - {Rules.RerollPrice}  [R]"))RerollUpgrade();
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
                Text(x+93,141,145,76,offer.Title=="Smg"?"SMG":offer.Title,body);
                Text(x+16,224,220,27,offer.weapon!=null?(WeaponSets.For(offer.weapon)?.Name??"Weapon")+"  /  Tier "+offer.Tier:"Item",tiny);
                if(offer.weapon!=null && new Rect(x+16,224,220,27).Contains(Event.current.mousePosition)) hoveredSet=WeaponSets.For(offer.weapon);
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
                GUI.Label(new Rect(x,y,60,58),new GUIContent("",Rules.Items[i].title+"\n"+Rules.Items[i].Detail));
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
                }
                if(GUI.Button(new Rect(x,700,82,80),GUIContent.none,GUIStyle.none))selectedWeapon=selectedWeapon==i?-1:i;
            }
            if(selectedWeapon>=0&&selectedWeapon<Rules.Inventory.Count)
            {
                var w=Rules.Inventory[selectedWeapon];
                Text(556,786,530,30,w.title+" / "+WeaponSets.For(w)?.Name+" / Tier "+w.tier,tiny);
                GUI.enabled=Rules.CanCombine(selectedWeapon);
                if(Button(new Rect(556,824,257,43),"Combine duplicate"))
                {Rules.Combine(selectedWeapon);selectedWeapon=-1;SyncEquipment();sound.Play(SoundCue.Install);}
                GUI.enabled=true;
                if(selectedWeapon>=0&&selectedWeapon<Rules.Inventory.Count&&Button(new Rect(828,824,257,43),$"Recycle (+{Mathf.Max(1,(int)(Rules.Price(Rules.Inventory[selectedWeapon].value)*.25f))})"))
                {Rules.Recycle(selectedWeapon);selectedWeapon=-1;SyncEquipment();sound.Play(SoundCue.Install);}
            }
            else Text(556,801,530,52,"Select a weapon to combine or recycle.",tiny);
            if(Button(new Rect(1110,806,306,61),$"Go (Wave {Rules.Wave+1})")) {selectedWeapon=-1;AdvanceWave();}
            Text(1130,867,266,24,"[ENTER]",center);
            if(!string.IsNullOrEmpty(GUI.tooltip))
            {
                Frame(new Rect(30,469,335,172),ink);
                Text(47,484,303,142,GUI.tooltip,body);
            }
            DrawSetTooltip();
        }

        WeaponSet hoveredSet;
        void DrawWeaponSets()
        {
            hoveredSet=null;
            Text(1130,508,266,28,"Weapon sets",new GUIStyle(body) {fontSize=20,fontStyle=FontStyle.Bold});
            for(int i=0;i<WeaponSets.All.Count;i++)
            {
                var set=WeaponSets.All[i];int count=Rules.SetCount(set);float y=540+i*35;
                var label=new GUIStyle(tiny);label.normal.textColor=count>=2?new Color(.39f,1,.29f):tiny.normal.textColor;
                Text(1130,y,127,32,$"{set.Name} {count}/6",label);
                var effect=new GUIStyle(label) {fontSize=15};
                Text(1260,y,140,34,count>=2?set.Description(count):"Needs 2 weapons",effect);
                if(new Rect(1124,y,280,34).Contains(Event.current.mousePosition))hoveredSet=set;
            }
        }
        void DrawSetTooltip()
        {
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
            Text(1130,654,260,25,"YOUR GEAR",tiny);
            Vector2 origin = new Vector2(1260, 712);
            const float scale = 60;
            var worn = AppearanceResolver.Resolve(playerAnimation.Look,Rules.Items);
            foreach (var layer in worn) if (layer.order < 10) DrawAppearance(layer, origin, scale);
            Sprite hero = playerAnimation.idleFrames[0];
            if(playerAnimation.footSprite!=null)foreach(int side in new[]{-1,1})
            {
                var leg=playerAnimation.footSprite;
                Vector2 legSize=leg.bounds.size*(SpringMotion.LegHeight/leg.bounds.size.y)*scale;
                Vector2 center=origin+new Vector2(side*SpringMotion.LegSpread,-SpringMotion.LegY)*scale;
                Matrix4x4 before=GUI.matrix;
                GUIUtility.ScaleAroundPivot(new Vector2(side<0?-SpringMotion.LegWidth:SpringMotion.LegWidth,1),center);
                DrawIcon(leg,new Rect(center-legSize*.5f,legSize));
                GUI.matrix=before;
            }
            Vector2 size = hero.rect.size / hero.pixelsPerUnit * scale;
            DrawIcon(hero, new Rect(origin - size * .5f, size));
            foreach (var layer in worn) if (layer.order >= 10) DrawAppearance(layer, origin, scale);
        }

        void DrawAppearance(AppearanceLayer layer, Vector2 origin, float scale)
        {
            Vector2 size = layer.image.rect.size / layer.image.pixelsPerUnit * layer.scale * scale;
            Vector2 center = origin + new Vector2(layer.offset.x, -layer.offset.y) * scale;
            DrawIcon(layer.image, new Rect(center - size * .5f, size));
        }
    }
}

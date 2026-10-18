using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        sealed class InspectEntry { public string id,title,description,category; public Sprite image; public Rect anchor; public Color color; public Equipment weapon; }
        readonly List<InspectEntry> inspectionEntries=new List<InspectEntry>();
        InspectionText inspectionText;
        InspectEntry hoveredInspection;
        string focusedInspection;
        bool keyboardInspection;
        Vector2 inspectionOrigin,lastInspectionMouse,intermissionItemScroll;
        RunPhase inspectionPhase;
        GUIStyle inspectionBody,inspectionTitle;
        void BeginInspection()
        {
            inspectionEntries.Clear();hoveredInspection=null;
            if(menuNavigationActive)keyboardInspection=false;
            inspectionOrigin=GUIUtility.GUIToScreenPoint(Vector2.zero);
            if(inspectionPhase!=Rules.Phase){focusedInspection=null;keyboardInspection=false;inspectionPhase=Rules.Phase;}
            if((Event.current.mousePosition-lastInspectionMouse).sqrMagnitude>1)keyboardInspection=false;
            lastInspectionMouse=Event.current.mousePosition;
        }
        void Inspect(string id,Rect local,string name,string description,Sprite image,string category="",int tier=1,Rect? clip=null,Rect? rootOverride=null)
        {
            Vector2 p=(GUIUtility.GUIToScreenPoint(local.position)-inspectionOrigin)/PresentationLayout.CanvasScale(Screen.width,Screen.height);
            var root=rootOverride??new Rect(p,local.size);
            if(clip.HasValue)
            {
                var c=clip.Value;root=Rect.MinMaxRect(Mathf.Max(c.xMin,root.xMin),Mathf.Max(c.yMin,root.yMin),Mathf.Min(c.xMax,root.xMax),Mathf.Min(c.yMax,root.yMax));
                if(root.width<=0||root.height<=0)return;
            }
            var entry=new InspectEntry{id=id,title=name,description=description,image=image,category=category,anchor=root,color=TierColor(tier)};
            inspectionEntries.Add(entry);
            bool navigationFocus=NavigationButton(local);
            if(navigationFocus){keyboardInspection=true;focusedInspection=id;}
            bool over=inspectionReview?forcedInspection==id:!menuNavigationActive&&root.Contains(lastInspectionMouse);
            if(over&&!keyboardInspection){hoveredInspection=entry;focusedInspection=id;}
            bool inventoryTile=(id=="character"||id.StartsWith("weapon:")||id.StartsWith("item:"))&&(Rules.Phase==RunPhase.Shop||Rules.Phase==RunPhase.Paused||Rules.Phase==RunPhase.Won||Rules.Phase==RunPhase.Lost);
            if(!inventoryTile&&(over&&!keyboardInspection||keyboardInspection&&focusedInspection==id))
            {
                // Match the tile corners; four rectangular strips left square white corners.
                GUI.DrawTexture(local,Texture2D.whiteTexture,ScaleMode.StretchToFill,true,0,paper,2,8);
            }
        }
        void InspectWeapon(string id,Equipment w,Rect r,Rect? clip=null,Vector2 scroll=default)
        {Inspect(id,r,inspectionText.WeaponName(w),inspectionText.WeaponDescription(Rules,w),w.image,inspectionText.Sets(w),w.tier,clip,clip.HasValue?(Rect?)new Rect(clip.Value.position+r.position-scroll,r.size):null);var entry=inspectionEntries.LastOrDefault(e=>e.id==id);if(entry!=null)entry.weapon=w;}
        void InspectItem(string id,ItemEntry item,Rect r,Rect? clip=null,Vector2 scroll=default)=>Inspect(id,r,inspectionText.ItemName(item),inspectionText.ItemDescription(item),item.image,"Item",item.tier,clip,clip.HasValue?(Rect?)new Rect(clip.Value.position+r.position-scroll,r.size):null);
        void EndInspection()
        {
            if(inspectionEntries.Count==0)return;
            var ev=Event.current;
            if(ev.type==EventType.KeyDown&&(ev.keyCode==KeyCode.Tab||!menuNavigationActive&&(ev.keyCode==KeyCode.LeftArrow||ev.keyCode==KeyCode.RightArrow||ev.keyCode==KeyCode.UpArrow||ev.keyCode==KeyCode.DownArrow)))
            {
                sound.Play(SoundCue.UiFocus);
                int index=inspectionEntries.FindIndex(e=>e.id==focusedInspection);
                if(index<0)index=0;
                else if(ev.keyCode==KeyCode.Tab)index=(index+(ev.shift?-1:1)+inspectionEntries.Count)%inspectionEntries.Count;
                else
                {
                    Vector2 dir=ev.keyCode==KeyCode.LeftArrow?Vector2.left:ev.keyCode==KeyCode.RightArrow?Vector2.right:ev.keyCode==KeyCode.UpArrow?Vector2.down:Vector2.up;
                    Vector2 from=inspectionEntries[index].anchor.center;float best=float.PositiveInfinity;int next=index;
                    for(int i=0;i<inspectionEntries.Count;i++)
                    {
                        Vector2 delta=inspectionEntries[i].anchor.center-from;float forward=Vector2.Dot(delta,dir);
                        if(forward<1)continue;float score=forward+Mathf.Abs(delta.x*dir.y-delta.y*dir.x)*3;
                        if(score<best){best=score;next=i;}
                    }
                    index=next;
                }
                keyboardInspection=true;focusedInspection=inspectionEntries[index].id;ev.Use();
            }
            var entry=keyboardInspection?inspectionEntries.Find(e=>e.id==focusedInspection):hoveredInspection;
            if(inspectionReview&&forcedInspection!=null)entry=inspectionEntries.Find(e=>e.id==forcedInspection);
            if(entry==null)return;
            lastDrawnInspection=entry.id;
            float width=510,bodyWidth=width-130;
            float textHeight=RichHeight(entry.description,bodyWidth);
            float titleHeight=inspectionTitle.CalcHeight(new GUIContent(entry.title),bodyWidth);
            float header=titleHeight+(string.IsNullOrEmpty(entry.category)?0:34);
            float height=Mathf.Max(122,header+textHeight+40);
            var sets=entry.weapon==null?new WeaponSet[0]:WeaponSets.All.Where(s=>entry.weapon.HasSet(s.Id)).ToArray();
            float groupWidth=width+(sets.Length>0?342:0),groupHeight=Mathf.Max(height,InspectionSetsHeight(sets));
            float x=entry.anchor.center.x>FullCanvas.center.x?entry.anchor.xMax-groupWidth:entry.anchor.xMin;
            x=Mathf.Clamp(x,FullCanvas.xMin+12,FullCanvas.xMax-groupWidth-12);
            float y=entry.anchor.center.y>FullCanvas.center.y?entry.anchor.yMin-groupHeight-10:entry.anchor.yMax+10;
            y=Mathf.Clamp(y,FullCanvas.yMin+12,FullCanvas.yMax-groupHeight-12);
            lastInspectionPanel=new Rect(x,y,groupWidth,groupHeight);
            RoundedBox(new Rect(x,y,width,height),new Color(0,0,0,1),10);
            if(entry.id=="character")DrawHeroFace(new Rect(x+18,y+20,88,88));else DrawIcon(entry.image,new Rect(x+18,y+20,88,88));
            var nameStyle=new GUIStyle(inspectionTitle);nameStyle.normal.textColor=entry.color;
            OutlinedText(x+118,y+16,bodyWidth,titleHeight,entry.title,nameStyle);
            if(!string.IsNullOrEmpty(entry.category))Text(x+118,y+20+titleHeight,bodyWidth,32,entry.category,new GUIStyle(inspectionBody){normal={textColor=new Color(.89f,.85f,.62f)}});
            RichText(entry.description,new Rect(x+118,y+22+header,bodyWidth,textHeight+8));
            DrawInspectionSets(sets,x+width+8,y);
        }
        float InspectionSetsHeight(WeaponSet[] sets)
        {
            float height=0;var style=new GUIStyle(inspectionBody){fontSize=16};
            foreach(var set in sets){height+=42;for(int n=2;n<=6;n++)height+=Mathf.Max(23,style.CalcHeight(new GUIContent("("+n+") "+set.Description(n)),306));height+=14;}
            return height;
        }
        void DrawInspectionSets(WeaponSet[] sets,float x,float y)
        {
            var style=new GUIStyle(inspectionBody){fontSize=16};
            foreach(var set in sets)
            {
                float h=InspectionSetsHeight(new[]{set})-6;
                RoundedBox(new Rect(x,y,334,h),new Color(0,0,0,1),10);
                Text(x+14,y+10,306,30,set.Name,new GUIStyle(inspectionBody){normal={textColor=new Color(.85f,.82f,.58f)}});
                float rowY=y+42;int count=Mathf.Min(6,Rules.SetCount(set));
                for(int n=2;n<=6;n++)
                {
                    string description="("+n+") "+set.Description(n);
                    float rowHeight=Mathf.Max(23,style.CalcHeight(new GUIContent(description),306));
                    style.normal.textColor=n==count?paper:new Color(.4f,.4f,.4f);
                    if(n==count)description=System.Text.RegularExpressions.Regex.Replace(description,@"[+-]\d+%?",m=>"<color=#76FF40>"+m.Value+"</color>");
                    Text(x+14,rowY,306,rowHeight,description,style);rowY+=rowHeight;
                }
                y+=h+6;
            }
        }
        void DrawIntermissionInventory()
        {
            Text(24,747,340,30,"Weapons ("+Rules.Inventory.Count+"/"+Rules.WeaponSlots+")",tiny);
            Text(385,747,690,30,"Items ("+Rules.Items.Count+")",tiny);
            for(int i=0;i<Rules.Inventory.Count;i++)
            {
                var r=new Rect(24+i%(Rules.Inventory.Count>6?12:6)*(Rules.Inventory.Count>6?29:56),783+i/(Rules.Inventory.Count>6?12:6)*43,Rules.Inventory.Count>6?27:50,40);RoundedBox(r,panelColor,5);DrawIcon(Rules.Inventory[i].image,new Rect(r.x+2,r.y+2,r.width-4,r.height-4));
                InspectWeapon("weapon:"+i,Rules.Inventory[i],r);
            }
            var viewport=new Rect(385,782,700,91);
            intermissionItemScroll=GUI.BeginScrollView(viewport,intermissionItemScroll,new Rect(0,0,Mathf.Max(680,Rules.Items.Count*58),60));
            for(int i=0;i<Rules.Items.Count;i++)
            {var r=new Rect(i*58,0,52,56);RoundedBox(r,panelColor,5);DrawIcon(Rules.Items[i].image,new Rect(r.x+4,4,44,48));InspectItem("item:"+i,Rules.Items[i],r,viewport,intermissionItemScroll);}
            GUI.EndScrollView();
        }
    }
}

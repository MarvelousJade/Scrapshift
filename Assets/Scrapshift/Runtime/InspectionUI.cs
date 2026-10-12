using System.Collections.Generic;
using UnityEngine;

namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        sealed class InspectEntry { public string id,title,description,category; public Sprite image; public Rect anchor; public Color color; }
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
            bool over=root.Contains(lastInspectionMouse);
            if(over&&!keyboardInspection){hoveredInspection=entry;focusedInspection=id;}
            if(over&&!keyboardInspection||keyboardInspection&&focusedInspection==id)
            {
                var c=paper;Box(new Rect(local.x,local.y,local.width,2),c);Box(new Rect(local.x,local.yMax-2,local.width,2),c);
                Box(new Rect(local.x,local.y,2,local.height),c);Box(new Rect(local.xMax-2,local.y,2,local.height),c);
            }
        }
        void InspectWeapon(string id,Equipment w,Rect r,Rect? clip=null)=>Inspect(id,r,inspectionText.WeaponName(w),inspectionText.WeaponDescription(Rules,w),w.image,inspectionText.Sets(w),w.tier,clip);
        void InspectItem(string id,ItemEntry item,Rect r,Rect? clip=null,Vector2 scroll=default)=>Inspect(id,r,inspectionText.ItemName(item),inspectionText.ItemDescription(item),item.image,"Item",item.tier,clip,clip.HasValue?(Rect?)new Rect(clip.Value.position+r.position-scroll,r.size):null);
        void EndInspection()
        {
            if(inspectionEntries.Count==0)return;
            var ev=Event.current;
            if(ev.type==EventType.KeyDown&&(ev.keyCode==KeyCode.Tab||ev.keyCode==KeyCode.LeftArrow||ev.keyCode==KeyCode.RightArrow||ev.keyCode==KeyCode.UpArrow||ev.keyCode==KeyCode.DownArrow))
            {
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
            float textHeight=inspectionBody.CalcHeight(new GUIContent(entry.description),bodyWidth);
            float titleHeight=inspectionTitle.CalcHeight(new GUIContent(entry.title),bodyWidth);
            float header=titleHeight+(string.IsNullOrEmpty(entry.category)?0:34);
            float height=Mathf.Max(122,header+textHeight+40);
            float x=entry.anchor.xMax+12;if(x+width>FullCanvas.xMax-12)x=entry.anchor.xMin-width-12;
            x=Mathf.Clamp(x,FullCanvas.xMin+12,FullCanvas.xMax-width-12);
            float y=Mathf.Clamp(entry.anchor.y,FullCanvas.yMin+12,FullCanvas.yMax-height-12);
            lastInspectionPanel=new Rect(x,y,width,height);
            RoundedBox(new Rect(x,y,width,height),new Color(0,0,0,.95f),10);
            DrawIcon(entry.image,new Rect(x+18,y+20,88,88));
            var nameStyle=new GUIStyle(inspectionTitle);nameStyle.normal.textColor=entry.color;
            OutlinedText(x+118,y+16,bodyWidth,titleHeight,entry.title,nameStyle);
            if(!string.IsNullOrEmpty(entry.category))Text(x+118,y+20+titleHeight,bodyWidth,32,entry.category,inspectionBody);
            Text(x+118,y+22+header,bodyWidth,textHeight+8,entry.description,inspectionBody);
        }
        void DrawIntermissionInventory()
        {
            Text(24,747,340,30,"Weapons ("+Rules.Inventory.Count+"/6)",tiny);
            Text(385,747,690,30,"Items ("+Rules.Items.Count+")",tiny);
            for(int i=0;i<Rules.Inventory.Count;i++)
            {
                var r=new Rect(24+i*56,783,50,56);RoundedBox(r,panelColor,5);DrawIcon(Rules.Inventory[i].image,new Rect(r.x+4,r.y+4,42,48));
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

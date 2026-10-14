using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        bool itemIconReview,itemIconCaptured;string itemIconReviewPath;int itemIconStage;float itemIconNext;
        readonly List<string> itemIconIssues=new List<string>();
        [Serializable] sealed class ItemIconRuntimeReport {public string status;public int items,unique;public string[] issues;}
        void BeginItemIconReview(string folder)
        {
            itemIconReview=true;itemIconReviewPath=Path.GetFullPath(folder);Directory.CreateDirectory(itemIconReviewPath);sound.SetMuted(true);
            var seen=new HashSet<string>();
            foreach(var item in catalog.data.items)
            {
                if(item.image==null){itemIconIssues.Add(item.id+": missing icon");continue;}
                string key=item.image.texture.name+":"+item.image.rect;
                if(!seen.Add(key))itemIconIssues.Add(item.id+": shared icon region");
                if(item.image.name.StartsWith("stats_"))itemIconIssues.Add(item.id+": stat placeholder");
            }
            File.WriteAllText(Path.Combine(itemIconReviewPath,"runtime.json"),JsonUtility.ToJson(new ItemIconRuntimeReport{status=itemIconIssues.Count==0?"Passed":"Failed",items=catalog.data.items.Length,unique=seen.Count,issues=itemIconIssues.ToArray()},true));
            Begin();ClearCombat();arrivalAge=ArrivalMotion.Duration;Rules.Tick(Rules.Remaining);
            while(Rules.Phase==RunPhase.Loot)Rules.ResolveCrate(false);
            while(Rules.Phase==RunPhase.Upgrade)Rules.Choose(0);
            string[] ids={"pumpkin","propeller_hat","alien_magic","injection"};
            for(int i=0;i<4;i++)Rules.Shop[i]=new ShopOffer{item=catalog.data.items.Single(x=>x.id==ids[i]),rolledWave=Rules.Wave};
            itemIconNext=Time.realtimeSinceStartup+1;
        }
        void ItemIconReviewTick()
        {
            if(Time.realtimeSinceStartup<itemIconNext)return;
            if(!itemIconCaptured)
            {
                string name=itemIconStage==0?"shop":itemIconStage==1?"codex":"catalog-"+(itemIconStage-2);
                ScreenCapture.CaptureScreenshot(Path.Combine(itemIconReviewPath,name+".png"));itemIconCaptured=true;itemIconNext=Time.realtimeSinceStartup+.3f;return;
            }
            itemIconStage++;itemIconCaptured=false;
            if(itemIconStage==8){Application.Quit(itemIconIssues.Count==0?0:1);enabled=false;return;}
            if(itemIconStage==1)
            {
                Rules.Pause();pauseCodex=true;codexTab=1;
                foreach(var item in catalog.data.items)progress.Count("item:"+item.id);
                codexSelected=Array.FindIndex(CodexEntries(),x=>x.id=="pumpkin");codexScroll=codexDetailScroll=Vector2.zero;
            }
            itemIconNext=Time.realtimeSinceStartup+.7f;
        }
        void DrawItemIconGallery()
        {
            Box(FullCanvas,new Color(.10f,.12f,.13f));
            Text(30,20,1380,45,"Individual item icons / page "+(itemIconStage-1)+" of 6",heading);
            var items=catalog.data.items.OrderBy(x=>x.id).ToArray();int start=(itemIconStage-2)*35;
            for(int k=start;k<Mathf.Min(start+35,items.Length);k++)
            {
                int local=k-start;float x=30+local%7*197,y=85+local/7*154;
                RoundedBox(new Rect(x,y,182,144),new Color(.17f,.2f,.21f),8);
                DrawIcon(items[k].image,new Rect(x+52,y+8,78,78));
                Text(x+8,y+93,166,47,items[k].title,new GUIStyle(body){fontSize=15,alignment=TextAnchor.MiddleCenter});
            }
        }
    }
}

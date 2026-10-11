using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        bool appearanceReview;
        string appearancePath;
        float appearanceStart;
        int appearanceStage;
        readonly List<GameObject> appearanceModels=new List<GameObject>();
        readonly List<ItemEntry[]> appearanceItems=new List<ItemEntry[]>();
        bool appearanceAligned=true,appearanceMirror=true;

        void BeginAppearanceReview(string folder)
        {
            appearanceReview=true;appearancePath=folder;Directory.CreateDirectory(folder);appearanceStart=Time.realtimeSinceStartup;reducedMotion=false;ChangeLook(1);
        }

        void AppearanceReviewTick()
        {
            if(!appearanceReview)return;
            float t=Time.realtimeSinceStartup-appearanceStart;
            if(appearanceStage==0&&t>2)
            {ScreenCapture.CaptureScreenshot(Path.Combine(appearancePath,"title-veteran.png"));appearanceStage=1;}
            if(appearanceStage==1&&t>3)
            {
                Begin();gameCamera.orthographicSize=7.5f;Rules.Inventory.Clear();SyncEquipment();player.SetActive(false);
                for(int i=0;i<6;i++)
                {
                    var model=Instantiate(playerPrefab);model.name="Appearance example "+i;
                    model.transform.position=new Vector3(-6+i%3*6, i<3?2.5f:-2.6f,-1);model.transform.localScale=Vector3.one*2.5f;
                    model.GetComponent<SpriteFrameAnimator>().lookIndex=i%3;model.AddComponent<CharacterWardrobe>();
                    string[][] sets={new[]{"beanie","coffee","cake"},new[]{"helmet","glasses","leather_vest","bat"},new[]{"head_injury","cyclops_worm","broken_mouth"}};
                    appearanceItems.Add(i<3?Array.Empty<ItemEntry>():sets[i%3].Select(id=>Array.Find(catalog.data.items,a=>a.id==id)).ToArray());
                    appearanceModels.Add(model);
                }
                appearanceStage=2;
            }
            if(appearanceStage>=2&&appearanceStage<6)
            {
                player.SetActive(false);
                for(int i=0;i<appearanceModels.Count;i++)
                {
                    var animator=appearanceModels[i].GetComponent<SpriteFrameAnimator>();
                    animator.Tick(t<6?Vector2.zero:t<9?Vector2.right:Vector2.left,Time.deltaTime,false);
                    var worn=appearanceModels[i].GetComponent<CharacterWardrobe>();worn.Sync(appearanceItems[i],catalog.spriteMaterial,animator.FacingLeft);
                    appearanceAligned&=worn.Layers.SequenceEqual(AppearanceResolver.Resolve(animator.Look,appearanceItems[i]));
                    if(t>9)appearanceMirror&=appearanceModels[i].GetComponentsInChildren<SpriteRenderer>().Where(v=>v.enabled&&v.transform.parent==animator.BodyRoot).All(v=>v.flipX);
                }
            }
            if(appearanceStage==2&&t>4.5f){ScreenCapture.CaptureScreenshot(Path.Combine(appearancePath,"looks-and-items.png"));appearanceStage=3;}
            if(appearanceStage==3&&t>7.5f){ScreenCapture.CaptureScreenshot(Path.Combine(appearancePath,"walking-right.png"));appearanceStage=4;}
            if(appearanceStage==4&&t>9.5f){ScreenCapture.CaptureScreenshot(Path.Combine(appearancePath,"walking-left.png"));appearanceStage=5;}
            if(appearanceStage==5&&t>11)
            {
                foreach(var model in appearanceModels)model.SetActive(false);
                Rules.Items.AddRange(appearanceItems[4]);Rules.Tick(Rules.Remaining);ClearCombat();
                while(Rules.Phase==RunPhase.Loot)Rules.ResolveCrate(false);
                while(Rules.Phase==RunPhase.Upgrade)Rules.Choose(0);
                appearanceStage=6;
            }
            if(appearanceStage==6&&t>12){ScreenCapture.CaptureScreenshot(Path.Combine(appearancePath,"shop-layers.png"));appearanceStage=7;}
            if(t>14)
            {
                bool passed=appearanceStage==7&&appearanceAligned&&appearanceMirror&&Rules.Phase==RunPhase.Shop;
                File.WriteAllText(Path.Combine(appearancePath,"report.txt"),
                    "Status: "+(passed?"Passed":"Failed")+"\nStaged rendering only; three cosmetic presets, three outfits, idle/right/left spring, title selector and shared shop composition. No AI/combat or balance claim.\n"+
                    "World/resolver agreement: "+appearanceAligned+"\nMirrored face layers: "+appearanceMirror+"\nFinal phase: "+Rules.Phase+"\n");
                Application.Quit(passed?0:1);enabled=false;
            }
        }

        void AppearanceReviewUI()
        {
            Text(25,20,1390,42,"MODULAR SCAVENGERS  /  ONE BODY, THREE LOOKS",heading);
            string[] names={"ROOKIE","VETERAN","PUNK"};
            for(int i=0;i<3;i++)
            {
                Text(195+i*360,419,330,38,names[i]+" / base",center);
                Text(195+i*360,723,330,48,new[]{"Beanie + coffee + cake","Helmet + glasses + vest + wings","Patch + cyclops eye + broken mouth"}[i],new GUIStyle(center){fontSize=16});
            }
            Text(25,836,1390,38,"Tiny pointed legs / slow idle spring / faster walking spring / no hands",center);
        }

    }
}

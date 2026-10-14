using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        bool appearanceParity,appearanceParityCaptured;string appearanceParityPath;int appearanceParityStage;float appearanceParityNext;
        readonly List<GameObject> parityModels=new List<GameObject>();
        [Serializable] sealed class AppearanceParityReport {public string status;public int items,eligible,characters,visibleWorldLayers;public bool mirrored;public string[] issues;}
        AppearanceParityReport appearanceParityReport;
        readonly List<string> parityIssues=new List<string>();
        readonly string[][] parityOutfits={new[]{"beanie"},new[]{"pumpkin"},new[]{"glasses","helmet"},new[]{"goat_skull"},new[]{"ghost_outfit"},new[]{"anvil"},new[]{"propeller_hat","alien_tongue"},new[]{"coffee","leather_vest"},new[]{"exoskeleton"},new[]{"cute_monkey"},new[]{"potato","blindfold"},new[]{"bait","torture"}};
        void BeginAppearanceParityReview(string folder)
        {
            if(Environment.GetCommandLineArgs().Contains("--remaining-wearable-review"))
            {
                string[][] outfits={new[]{"bandana"},new[]{"cyclops_worm"},new[]{"strange_book"},new[]{"warrior_helmet"},
                    new[]{"propeller_hat","leather_vest","glasses"},new[]{"coffee","strange_book","lumberjack_shirt"},new[]{"goat_skull","black_belt"},new[]{"wolf_helmet","sunglasses"},
                    new[]{"torture"},new[]{"robot_arm"},new[]{"scope","cute_monkey"},new[]{"tree","tentacle"}};
                for(int i=0;i<outfits.Length;i++)parityOutfits[i]=outfits[i];
            }
            if(Environment.GetCommandLineArgs().Contains("--template-wearable-review"))
            {
                string[][] outfits={new[]{"beanie"},new[]{"leather_vest"},new[]{"glasses"},new[]{"coffee"},
                    new[]{"beanie","glasses","leather_vest"},new[]{"coffee","glasses","leather_vest"},new[]{"beanie","leather_vest"},new[]{"coffee","leather_vest"},
                    new[]{"beanie"},new[]{"leather_vest"},new[]{"glasses"},new[]{"coffee"}};
                for(int i=0;i<outfits.Length;i++)parityOutfits[i]=outfits[i];
            }
            appearanceParity=true;appearanceParityPath=Path.GetFullPath(folder);Directory.CreateDirectory(appearanceParityPath);sound.SetMuted(true);
            Begin();ClearCombat();player.SetActive(false);if(titleModel!=null)titleModel.SetActive(false);
            if(gun!=null)gun.SetActive(false);foreach(var view in equipmentViews)if(view!=null)view.SetActive(false);
            appearanceParityReport=new AppearanceParityReport{items=catalog.data.items.Length,eligible=catalog.data.items.Count(i=>i.appearances?.Length>0),characters=playerAnimation.looks.Length,mirrored=true};
            for(int i=0;i<catalog.data.characters.Length;i++)if(playerAnimation.looks[i].id!=catalog.data.characters[i].id)parityIssues.Add("Character mapping mismatch: "+i);
            gameCamera.orthographicSize=6;gameCamera.transform.position=new Vector3(0,0,-20);
            for(int i=0;i<parityOutfits.Length;i++)
            {
                var obj=Instantiate(playerPrefab);obj.transform.position=new Vector3(-6+i%4*4,3.5f-i/4*3.5f,0);obj.transform.localScale=Vector3.one*1.3f;
                bool template=Environment.GetCommandLineArgs().Contains("--template-wearable-review");
                if(template&&i>=8)obj.transform.localScale=Vector3.one*.65f;
                var animation=obj.GetComponent<SpriteFrameAnimator>();animation.lookIndex=!template&&!Environment.GetCommandLineArgs().Contains("--remaining-wearable-review")&&i==4?18:0;animation.Tick(Vector2.right,0,false);
                var worn=obj.AddComponent<CharacterWardrobe>();var items=parityOutfits[i].Select(id=>catalog.data.items.Single(x=>x.id==id)).ToArray();worn.Sync(items,catalog.spriteMaterial,false);
                var expected=AppearanceResolver.Resolve(animation.Look,items);
                if(!worn.Layers.SequenceEqual(expected))parityIssues.Add("World layer mismatch: "+i);
                appearanceParityReport.visibleWorldLayers+=worn.VisibleCount;parityModels.Add(obj);
            }
            appearanceParityNext=Time.realtimeSinceStartup+1;
        }
        void AppearanceParityTick()
        {
            if(Time.realtimeSinceStartup<appearanceParityNext)return;
            if(!appearanceParityCaptured)
            {
                string name=appearanceParityStage==0?"world-right":appearanceParityStage==1?"world-left":appearanceParityStage<4?"characters-"+(appearanceParityStage-2):"items-"+(appearanceParityStage-4);
                ScreenCapture.CaptureScreenshot(Path.Combine(appearanceParityPath,name+".png"));appearanceParityCaptured=true;appearanceParityNext=Time.realtimeSinceStartup+.3f;return;
            }
            appearanceParityStage++;appearanceParityCaptured=false;
            if(appearanceParityStage==10)
            {
                if(!appearanceParityReport.mirrored)parityIssues.Add("An appearance did not mirror when facing left.");
                appearanceParityReport.status=parityIssues.Count==0?"Passed":"Failed";appearanceParityReport.issues=parityIssues.ToArray();
                File.WriteAllText(Path.Combine(appearanceParityPath,"runtime.json"),JsonUtility.ToJson(appearanceParityReport,true));Application.Quit(parityIssues.Count==0?0:1);enabled=false;return;
            }
            if(appearanceParityStage==1)for(int i=0;i<parityModels.Count;i++)
            {
                var obj=parityModels[i];var animation=obj.GetComponent<SpriteFrameAnimator>();animation.Tick(Vector2.left,.2f,false);var worn=obj.GetComponent<CharacterWardrobe>();
                worn.Sync(parityOutfits[i].Select(id=>catalog.data.items.Single(x=>x.id==id)).ToArray(),catalog.spriteMaterial,true);
                foreach(var renderer in obj.GetComponentsInChildren<SpriteRenderer>().Where(r=>r.enabled&&r.name.StartsWith("Appearance ")))
                    if(!renderer.flipX)appearanceParityReport.mirrored=false;
            }
            if(appearanceParityStage==2)foreach(var obj in parityModels)obj.SetActive(false);
            appearanceParityNext=Time.realtimeSinceStartup+.7f;
        }
        void DrawAppearanceParityReview()
        {
            if(appearanceParityStage<2)
            {
                Text(25,20,1390,42,"Fitted Scrapshift wearables / "+(appearanceParityStage==0?"facing right":"facing left"),heading);
                for(int i=0;i<parityModels.Count;i++)Text(30+i%4*355,265+i/4*255,340,50,string.Join(" + ",parityOutfits[i].Select(id=>catalog.data.items.Single(x=>x.id==id).title)),new GUIStyle(body){alignment=TextAnchor.MiddleCenter,fontSize=17});
                return;
            }
            Box(FullCanvas,new Color(.10f,.12f,.13f));bool characters=appearanceParityStage<4;int page=characters?appearanceParityStage-2:appearanceParityStage-4;
            var items=catalog.data.items.Where(i=>i.appearances?.Length>0).OrderBy(i=>i.id).ToArray();int count=characters?catalog.data.characters.Length:items.Length;
            Text(25,18,1390,44,(characters?"Character appearance layers":"Items that change appearance")+" / page "+(page+1),heading);
            for(int index=page*32;index<Mathf.Min(count,page*32+32);index++)
            {
                int k=index%32;float x=24+k%8*176,y=80+k/8*197;RoundedBox(new Rect(x,y,166,187),new Color(.17f,.2f,.21f),9);
                var look=characters?playerAnimation.looks[index]:playerAnimation.looks[0];var equipment=characters?Array.Empty<ItemEntry>():new[]{items[index]};
                var layers=AppearanceResolver.Resolve(look,equipment);Vector2 origin=new Vector2(x+83,y+91);float scale=54;
                foreach(var layer in layers.Where(l=>l.order<10))DrawAppearance(layer,origin,scale);
                var hero=playerAnimation.idleFrames[0];Vector2 size=(Vector2)hero.bounds.size*scale;if(!layers.Any(l=>l.slot=="skin"&&l.image!=null))DrawIcon(hero,new Rect(origin-size*.5f,size));
                foreach(var layer in layers.Where(l=>l.order>=10))DrawAppearance(layer,origin,scale);
                Text(x+6,y+142,154,39,characters?catalog.data.characters[index].title:items[index].title,new GUIStyle(body){fontSize=14,alignment=TextAnchor.MiddleCenter});
            }
        }
    }
}


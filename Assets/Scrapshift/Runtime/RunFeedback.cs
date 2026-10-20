using UnityEngine;
using System.Linq;
namespace Scrapshift
{
    public static class RunFeedback
    {
        public static float DetectionRange(float range)=>range+2f;
        public static bool CanAttack(float range,float squaredDistance)=>squaredDistance<=(range+.5f)*(range+.5f);
        public static Color SpawnColor(bool tree,float alpha)=>tree?new Color(.3f,1,.2f,alpha):new Color(1,.25f,.2f,alpha);
    }
    public sealed partial class ArenaGame
    {
        bool waveSweeping;float waveSweepAge,moneyArrivalTime=-10;int moneyArrivalAmount;
        void BeginWaveSweep()
        {
            Rules.RecordWaveEnd(Rules.LivingTrees,pickups.Where(p=>!p.fruit).Sum(p=>p.value));Rules.BeginWaveSettlement();RemoveWaveEnemies();
            waveSweeping=true;waveSweepAge=0;moneyArrivalTime=-10;moneyArrivalAmount=0;
            BankOptimizedCleanupMaterials();
            sound.StopCombat();sound.Play(SoundCue.WaveClear);
        }
        void BankOptimizedCleanupMaterials()
        {
            if(!GameSettings.Flag("optimizeEndWaves",false))return;
            for(int i=pickups.Count-1;i>=0;i--)
            {
                var pickup=pickups[i];if(pickup.fruit)continue;
                Rules.BankCleanupMaterials(pickup.value,true);Destroy(pickup.obj);pickups.RemoveAt(i);
            }
        }
        void TickWaveSweep(float dt)
        {
            waveSweepAge+=dt;Rules.AdvanceWaveSettlement(dt);
            float scale=PresentationLayout.CanvasScale(Screen.width,Screen.height);
            Vector3 target=gameCamera.ScreenToWorldPoint(new Vector3(42*scale,Screen.height-140.5f*scale,20));
            for(int i=pickups.Count-1;i>=0;i--)
            {
                var pickup=pickups[i];Vector2 to=pickup.fruit?playerPos:(Vector2)target;
                pickup.motion.position=Vector2.MoveTowards(pickup.motion.position,to,(8+waveSweepAge*45)*dt);
                pickup.obj.transform.position=new Vector3(pickup.motion.position.x,pickup.motion.position.y,-2);
                if(Vector2.Distance(pickup.motion.position,to)>.15f)continue;
                if(pickup.fruit)CollectPickup(pickup);else {Rules.BankMaterials(pickup.value);if(waveSweepAge-moneyArrivalTime>.12f)moneyArrivalAmount=0;moneyArrivalAmount+=pickup.value;moneyArrivalTime=waveSweepAge;}
                Destroy(pickup.obj);pickups.RemoveAt(i);
            }
            // Source EndWaveTimer is 2 seconds ordinarily and 4 on victory.
            if(waveSweepAge<Rules.WaveSettlementDuration||pickups.Count>0)return;
            if(Rules.CompleteWaveSettlement())waveSweeping=false;
        }
        void DrawMoneyCounter()
        {
            float age=waveSweeping?waveSweepAge-moneyArrivalTime:10;
            float pulse=age<.32f?Mathf.Sin(age/.32f*Mathf.PI)*.16f:0;
            var matrix=GUI.matrix;
            if(!reducedMotion)GUIUtility.ScaleAroundPivot(Vector2.one*(1+pulse),new Vector2(48,140));
            DrawIcon(catalog.scrapShapes[0],new Rect(26,123,30,35));
            OutlinedText(67,114,160,51,Rules.Scrap.ToString(),number);
            GUI.matrix=matrix;
            if(age<.7f&&moneyArrivalAmount>0)
            {
                var gain=new GUIStyle(body){fontSize=25,normal={textColor=new Color(.4f,1,.2f,1-age/.7f)}};
                OutlinedText(180,140-(reducedMotion?0:age*38),180,36,"+"+moneyArrivalAmount,gain);
            }
        }
        void DrawStoredMaterials()
        {
            DrawStatIcon("harvesting",new Rect(23,165,40,40));
            OutlinedText(67,159,170,51,Rules.Stat("harvesting").ToString("0"),number);
            if(waveSweeping&&waveSweepAge>1.1f&&Rules.Stat("harvesting")!=0)
                OutlinedText(28,245,280,38,"Harvesting "+Rules.Stat("harvesting").ToString("+0;-0"),body);
        }
        void DrawHeroFace(Rect r)
        {
            var hero=playerAnimation.idleFrames[0];float scale=Mathf.Min(r.width/hero.bounds.size.x,r.height/hero.bounds.size.y);
            var layers=AppearanceResolver.Resolve(playerAnimation.Look,System.Array.Empty<ItemEntry>(),GameSettings.Flag("hideItems",false));
            foreach(var layer in layers)if(layer.order<10)DrawAppearance(layer,r.center,scale);
            Vector2 size=(Vector2)hero.bounds.size*scale;if(!layers.Any(l=>l.slot=="skin"&&l.image!=null))DrawIcon(hero,new Rect(r.center-size*.5f,size));
            foreach(var layer in layers)if(layer.order>=10)DrawAppearance(layer,r.center,scale);
        }
        void DrawPendingRewards()
        {
            float right=FullCanvas.xMax-30;
            for(int i=0;i<Rules.PendingLevels;i++)
            {
                float x=right-64*(i%12+1),y=25+(i/12)*59;
                OutlinedText(x+29,y-4,31,52,"\u2191",new GUIStyle(center){fontSize=46,normal={textColor=new Color(.3f,1,.08f)}});
                DrawHeroFace(new Rect(x+2,y+2,40,52));
            }
            for(int i=0;i<Mathf.Min(8,Rules.PendingCrates);i++)
            {
                float y=28+Mathf.Max(1,Mathf.CeilToInt(Rules.PendingLevels/12f))*55;
                float x=right-58*(i+1);DrawCrateIcon(new Rect(x,y,46,42));
            }
        }
        void DrawWeaponActions()
        {
            if(selectedWeapon<0||selectedWeapon>=Rules.Inventory.Count){selectedWeapon=-1;return;}
            var w=Rules.Inventory[selectedWeapon];Overlay();
            const float width=350;bool combine=Rules.CanCombine(selectedWeapon);
            float textHeight=RichHeight(inspectionText.WeaponDescription(Rules,w),310),visibleHeight=Mathf.Min(350,textHeight);
            float height=128+visibleHeight+(combine?126:84),x=FullCanvas.xMax-704,y=Mathf.Max(24,670-height);
            float buttonsY=y+122+visibleHeight;
            RoundedBox(new Rect(x,y,width,height),Color.black,10);
            DrawIcon(w.image,new Rect(x+18,y+18,82,82));
            Text(x+115,y+18,215,54,inspectionText.WeaponName(w),new GUIStyle(inspectionTitle){normal={textColor=TierColor(w.tier)}});
            Text(x+115,y+76,215,32,inspectionText.Sets(w),new GUIStyle(inspectionBody){normal={textColor=new Color(.89f,.85f,.62f)}});
            detailScroll.TryGetValue("weapon-action",out var scroll);
            scroll=GUI.BeginScrollView(new Rect(x+20,y+116,326,visibleHeight),scroll,new Rect(0,0,310,textHeight));
            RichText(inspectionText.WeaponDescription(Rules,w),new Rect(0,0,310,textHeight));GUI.EndScrollView();detailScroll["weapon-action"]=scroll;
            if(combine){if(Button(new Rect(x+15,buttonsY,width-30,34),"Combine")){Rules.Combine(selectedWeapon);selectedWeapon=-1;SyncEquipment();sound.Play(SoundCue.Install);}buttonsY+=42;}
            if(Button(new Rect(x+15,buttonsY,width-30,34),"Recycle (+"+Rules.RecycleValue(w.value,true)+")"))
            {Rules.Recycle(selectedWeapon);selectedWeapon=-1;SyncEquipment();sound.Play(SoundCue.Install);}
            if(Button(new Rect(x+15,buttonsY+42,width-30,34),"Cancel"))selectedWeapon=-1;
            DrawInspectionSets(WeaponSets.All.Where(s=>w.HasSet(s.Id)).ToArray(),x+width+8,y);
        }
    }
}

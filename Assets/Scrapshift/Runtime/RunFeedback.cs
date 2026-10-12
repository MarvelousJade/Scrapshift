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
        void BeginWaveSweep(){RemoveWaveEnemies();waveSweeping=true;waveSweepAge=0;moneyArrivalTime=-10;moneyArrivalAmount=0;sound.StopCombat();sound.Play(SoundCue.WaveClear);}
        void TickWaveSweep(float dt)
        {
            waveSweepAge+=dt;
            float scale=PresentationLayout.CanvasScale(Screen.width,Screen.height);
            Vector3 target=gameCamera.ScreenToWorldPoint(new Vector3(42*scale,Screen.height-122.5f*scale,20));
            for(int i=pickups.Count-1;i>=0;i--)
            {
                var pickup=pickups[i];Vector2 to=pickup.fruit?playerPos:(Vector2)target;
                pickup.motion.position=Vector2.MoveTowards(pickup.motion.position,to,(8+waveSweepAge*45)*dt);
                pickup.obj.transform.position=new Vector3(pickup.motion.position.x,pickup.motion.position.y,-2);
                if(Vector2.Distance(pickup.motion.position,to)>.15f)continue;
                if(pickup.fruit)CollectPickup(pickup);else {Rules.BankMaterials(pickup.value);if(waveSweepAge-moneyArrivalTime>.12f)moneyArrivalAmount=0;moneyArrivalAmount+=pickup.value;moneyArrivalTime=waveSweepAge;}
                Destroy(pickup.obj);pickups.RemoveAt(i);
            }
            if(waveSweepAge<1.8f||pickups.Count>0)return;
            waveSweeping=false;Rules.Tick(Rules.Remaining);
        }
        void DrawMoneyCounter()
        {
            float age=waveSweeping?waveSweepAge-moneyArrivalTime:10;
            float pulse=age<.32f?Mathf.Sin(age/.32f*Mathf.PI)*.16f:0;
            var matrix=GUI.matrix;
            if(!reducedMotion)GUIUtility.ScaleAroundPivot(Vector2.one*(1+pulse),new Vector2(48,122));
            DrawIcon(catalog.scrapShapes[0],new Rect(26,105,30,35));
            OutlinedText(67,96,160,51,Rules.Scrap.ToString(),number);
            GUI.matrix=matrix;
            if(age<.7f&&moneyArrivalAmount>0)
            {
                var gain=new GUIStyle(body){fontSize=25,normal={textColor=new Color(.4f,1,.2f,1-age/.7f)}};
                OutlinedText(180,122-(reducedMotion?0:age*38),180,36,"+"+moneyArrivalAmount,gain);
            }
        }
        void DrawStoredMaterials()
        {
            if(Rules.Banked>0||waveSweeping)
            {
                RoundedBox(new Rect(28,201,29,28),new Color(.58f,.48f,.29f),10);
                Box(new Rect(35,196,16,5),paper);
                OutlinedText(67,192,170,42,Rules.Banked.ToString(),body);
            }
            DrawStatIcon("harvesting",new Rect(23,147,40,40));
            OutlinedText(67,141,170,51,Rules.Stat("harvesting").ToString("0"),number);
            if(waveSweeping&&waveSweepAge>1.1f&&Rules.Stat("harvesting")!=0)
                OutlinedText(28,245,280,38,"Harvesting "+Rules.Stat("harvesting").ToString("+0;-0"),body);
        }
        void DrawPendingRewards()
        {
            float right=FullCanvas.xMax-30;
            for(int i=0;i<Rules.PendingLevels;i++)
            {
                float x=right-58*(i%12+1);RoundedBox(new Rect(x,28+(i/12)*55,49,49),new Color(.12f,.16f,.09f,.9f),7);
                Text(x+3,25+(i/12)*55,44,49,"\u2191",new GUIStyle(center){fontSize=40,normal={textColor=new Color(.4f,1,.2f)}});
            }
            for(int i=0;i<Mathf.Min(8,Rules.PendingCrates);i++)
            {
                float y=28+Mathf.Max(1,Mathf.CeilToInt(Rules.PendingLevels/12f))*55;
                float x=right-58*(i+1);RoundedBox(new Rect(x,y,44,40),new Color(.48f,.29f,.12f),3);
                Box(new Rect(x+18,y+2,7,36),amber);Box(new Rect(x+2,y+16,40,6),amber);
            }
        }
        void DrawWeaponActions()
        {
            if(selectedWeapon<0||selectedWeapon>=Rules.Inventory.Count){selectedWeapon=-1;return;}
            var w=Rules.Inventory[selectedWeapon];Overlay();
            const float x=345,y=125,width=460;
            float textHeight=inspectionBody.CalcHeight(new GUIContent(inspectionText.WeaponDescription(Rules,w)),420);
            float buttonsY=y+138+textHeight;
            RoundedBox(new Rect(x,y,width,buttonsY-y+210),Color.black,10);
            DrawIcon(w.image,new Rect(x+18,y+18,82,82));
            Text(x+115,y+18,325,48,inspectionText.WeaponName(w),new GUIStyle(inspectionTitle){normal={textColor=TierColor(w.tier)}});
            Text(x+115,y+69,325,32,inspectionText.Sets(w),inspectionBody);
            Text(x+20,y+123,420,textHeight+8,inspectionText.WeaponDescription(Rules,w),inspectionBody);
            GUI.enabled=Rules.CanCombine(selectedWeapon);
            if(Button(new Rect(x+15,buttonsY+10,width-30,49),"Combine")){Rules.Combine(selectedWeapon);selectedWeapon=-1;SyncEquipment();sound.Play(SoundCue.Install);}
            GUI.enabled=true;
            if(Button(new Rect(x+15,buttonsY+72,width-30,49),"Recycle (+"+Rules.RecycleValue(w.value)+")"))
            {Rules.Recycle(selectedWeapon);selectedWeapon=-1;SyncEquipment();sound.Play(SoundCue.Install);}
            if(Button(new Rect(x+15,buttonsY+134,width-30,49),"Cancel"))selectedWeapon=-1;
            DrawInspectionSets(WeaponSets.All.Where(s=>w.HasSet(s.Id)).ToArray(),x+width+8,y);
        }
    }
}

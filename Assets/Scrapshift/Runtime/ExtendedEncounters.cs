using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Scrapshift
{
    [Serializable] public sealed class EncounterAttack
    {
        public bool enabled,charge,spawn,onTarget,borders,constant,randomDirection,away,inDirection,onTargetOnce,deleteOnDeath;
        public string projectile,summon;
        public float cooldown,initial,jitter,windup,active,chargeSpeed,minRange,maxRange,projectileSpeed,damage,growth,spread,spawnSpread,longCooldown;
        public int count,longEvery;
    }
    [Serializable] public sealed class EncounterState
    {public float hp,time,spacing,offset;public string movement;public EncounterAttack attack;}
    [Serializable] public sealed class EncounterEntry
    {
        public string id,art,role;public float health,growth,speed,speedRandom,damage,damageGrowth,armor,armorGrowth,resistance;public int value;
        public EncounterState[] states;public EncounterAttack[] extra;
    }
    [Serializable] public sealed class DifficultyEntry {public int level;public StatEffect[] effects;}
    [Serializable] public sealed class ExtendedEncounterData {public EncounterEntry[] enemies;public DifficultyEntry[] difficulties;}
    public sealed partial class ArenaGame
    {
        ExtendedEncounterData encounterData;
        readonly Dictionary<string,EncounterEntry> encounterDefinitions=new Dictionary<string,EncounterEntry>();
        readonly Dictionary<Enemy,int> encounterStates=new Dictionary<Enemy,int>();
        readonly Dictionary<Enemy,float> extraAttackClocks=new Dictionary<Enemy,float>();
        readonly Dictionary<int,string> eliteWaves=new Dictionary<int,string>();
        readonly HashSet<int> hordeWaves=new HashSet<int>(),fogWaves=new HashSet<int>(),bulletWaves=new HashSet<int>();

        void InitializeEncounters()
        {
            encounterData=JsonUtility.FromJson<ExtendedEncounterData>(Resources.Load<TextAsset>("extended-encounters").text);
            foreach(var d in encounterData.enemies)
            {
                encounterDefinitions[d.id]=d;
                if(catalog.Enemy(d.id)!=null)continue;
                var entry=new EnemyEntry{id=d.id,role=d.role,health=d.health,growth=d.growth,speed=d.speed,speedRandom=d.speedRandom,
                    damage=d.damage,damageGrowth=d.damageGrowth,armor=d.armor,armorGrowth=d.armorGrowth,resistance=d.resistance,value=d.value,image=catalog.Enemy(d.art).image};
                catalog.data.enemies=catalog.data.enemies.Concat(new[]{entry}).ToArray();
            }
        }
        void PlanEncounters()
        {
            eliteWaves.Clear();hordeWaves.Clear();fogWaves.Clear();bulletWaves.Clear();
            var pool=encounterData.enemies.Where(e=>e.role=="Elite").Select(e=>e.id).ToList();
            int count=Rules.Difficulty<2?0:Rules.Difficulty<4?1:3;
            for(int i=0;i<count;i++)
            {
                int wave=11+i*3+placementRandom.Next(2);
                if(i<2&&Rules.Character?.id!="jack"&&placementRandom.NextDouble()<.4){hordeWaves.Add(wave);continue;}
                int pick=placementRandom.Next(pool.Count);eliteWaves[wave]=pool[pick];pool.RemoveAt(pick);
            }
            if(Rules.Difficulty<6)return;
            int fog=placementRandom.Next(5,8);fogWaves.Add(fog);
            bulletWaves.UnionWith(new[]{placementRandom.Next(1,3),placementRandom.Next(3,5),Enumerable.Range(5,3).Where(w=>w!=fog).OrderBy(_=>placementRandom.Next()).First(),placementRandom.Next(8,11)});
            var remaining=Enumerable.Range(11,9).Where(w=>!eliteWaves.ContainsKey(w)&&!hordeWaves.Contains(w)).OrderBy(_=>placementRandom.Next()).ToArray();
            fogWaves.UnionWith(remaining.Take(2));bulletWaves.UnionWith(remaining.Skip(2).Take(3));
        }
        EnemyBehavior EncounterBehavior(EncounterEntry d,int state)
        {
            var s=d.states[state];var a=s.attack;
            return new EnemyBehavior{canBoost=d.role!="Elite"&&d.role!="Boss"&&d.role!="Buffer",movement=(EnemyMovement)Enum.Parse(typeof(EnemyMovement),s.movement),spacing=s.spacing,offset=s.offset,
                charge=a.charge,ranged=a.enabled&&!a.charge,cooldown=a.cooldown,jitter=a.jitter,initial=a.initial,windup=a.windup,active=a.charge?a.active:0,
                minRange=a.minRange,maxRange=a.spawn?9999:a.maxRange,chargeSpeed=a.chargeSpeed,projectileSpeed=a.projectileSpeed,projectileGrowth=a.growth};
        }
        void TickEncounterState(Enemy e,float dt)
        {
            if(!encounterDefinitions.TryGetValue(e.type.id,out var d))return;
            encounterStates.TryGetValue(e,out int current);int next=current;
            bool checkState=e.type.role!="Boss"||Mathf.FloorToInt(e.activeAge)>e.phaseCheck;
            if(checkState)e.phaseCheck=Mathf.FloorToInt(e.activeAge);
            for(int i=current+1;checkState&&i<d.states.Length;i++)if(e.hp/e.maxHp<=d.states[i].hp||e.activeAge>=d.states[i].time)next=i;
            if(next!=current)
            {if(e.type.role=="Boss")bossMutations+=next-current;encounterStates[e]=next;e.behavior=EncounterBehavior(d,next);e.attack=MakeAttack(e.behavior);e.hasDestination=false;FloatText(e.pos,"MUTATION",coral,true);}
            if(e.type.id=="invoker"&&next==2)e.speed=e.type.speed*2.5f;
            if(e.type.id=="healer")HealNearbyEnemies(e);
            if(e.type.id=="monk")e.speed=e.type.speed*(next==1||next==3?.5f:next==2?1.25f:1);
            if(d.extra.Length==0||e.attack==null)return;
            extraAttackClocks.TryGetValue(e,out float clock);clock-=dt;
            bool trigger=e.attack.Phase==AttackPhase.Windup||e.attack.Phase==AttackPhase.Active;
            if(trigger&&e.behavior.charge&&clock<=0)
            {var extra=d.extra[Math.Min(next,d.extra.Length-1)];FireEncounter(e,extra);clock=extra.cooldown;}
            extraAttackClocks[e]=clock;
        }
        readonly Dictionary<Enemy,HashSet<Enemy>> healerOccupants=new Dictionary<Enemy,HashSet<Enemy>>();
        void HealNearbyEnemies(Enemy healer)
        {
            if(!healerOccupants.TryGetValue(healer,out var prior))prior=new HashSet<Enemy>();
            var current=new HashSet<Enemy>();
            foreach(var ally in enemies)if(ally!=healer&&ally.marker==null&&ally.hp>0&&Vector2.Distance(ally.pos,healer.pos)<=2.00639f)
            {current.Add(ally);if(!prior.Contains(ally)){ally.hp=Mathf.Min(ally.maxHp,ally.hp+100+10*(Rules.Wave-1));ally.supportFlash=.2f;}}
            healerOccupants[healer]=current;
        }
        void FireEncounter(Enemy e,EncounterAttack a=null)
        {
            var d=encounterDefinitions[e.type.id];encounterStates.TryGetValue(e,out int state);a=a??d.states[state].attack;
            if(a.spawn)
            {
                var type=catalog.Enemy(a.summon);if(type!=null)for(int i=0;i<a.count;i++)births.Push(new Birth{type=type,pos=RunRules.ClampPosition(e.pos+UnityEngine.Random.insideUnitCircle*a.spawnSpread)},BirthLane.Summon);
                if(e.type.id=="slasher_egg"||e.type.id=="infected_slasher_egg"){e.hp=0;Destroy(e.obj);enemies.Remove(e);}
                if(e.type.id=="mom"&&d.extra.Length>0)FireEncounter(e,d.extra[0]);
                return;
            }
            Vector2 aim=a.inDirection?e.attack.Direction:(playerPos-e.pos).normalized;
            float baseAngle=a.randomDirection?UnityEngine.Random.Range(-Mathf.PI,Mathf.PI):Mathf.Atan2(aim.y,aim.x);
            for(int i=0;i<a.count;i++)
            {
                float angle=(a.randomDirection?UnityEngine.Random.Range(-Mathf.PI,Mathf.PI):baseAngle)+(a.constant&&a.count>1?-a.spread+i*2*a.spread/(a.count-1):UnityEngine.Random.Range(-a.spread,a.spread));
                Vector2 dir=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle));
                Vector2 offset=BossPatterns.SpawnOffset(a.spawnSpread,a.borders,a.constant,i,a.count,UnityEngine.Random.Range(-1f,1f),UnityEngine.Random.Range(-1f,1f));
                Vector2 pos=(a.onTarget?playerPos:e.pos)+offset;
                if(a.onTargetOnce&&i==0)pos=playerPos;
                if(a.away)dir=(pos-e.pos).normalized;
                float damage=Mathf.Max(1,(int)((a.damage+a.growth*(Rules.Wave-1))*Rules.EnemyDamageFactor));
                if(a.projectile.Contains("slash")&&a.projectileSpeed==0)
                {
                    EnemySlash(e);var slash=slashes[slashes.Count-1];slash.pos=pos;slash.obj.transform.position=pos;slash.damage=damage;continue;
                }
                if(shots.Count>=1200)break;
                bool pillar=a.projectile.Contains("pillar");
                var obj=pillar?PillarWarning():PlasmaOrb("Elite plasma");obj.transform.position=pos;
                shots.Add(new Shot{obj=obj,pos=pos,velocity=dir*a.projectileSpeed,life=pillar?BossPatterns.PillarEnd:a.projectileSpeed==0?3:20,pillar=pillar,damage=damage,hostile=true,attacker=e.type,sourceEnemy=e});
            }
        }
        void AddWaveEncounters()
        {
            Rules.RemovedEnemies=0;Rules.IsHordeWave=hordeWaves.Contains(Rules.Wave);healerOccupants.Clear();encounterStates.Clear();extraAttackClocks.Clear();PrepareProjectileEvent();
            for(int i=0;i<Rules.ExtraBaitGroups;i++)director.AddExtra(new SpawnGroup{start=2,interval=3,minimum=1,repeats=-1,chance=1,area=-1,inset=2,units=new[]{new UnitEntry{id="lamprey",min=7,max=8,chance=1}}});
            int baits=Rules.Items.Count(i=>i.id=="bait");
            if(Rules.Stat("upgraded_baits")>0&&baits>0)
            {
                int groups=Math.Max(1,baits/10),bonus=Math.Max(0,baits/15);
                for(int i=0;i<groups;i++)director.AddExtra(new SpawnGroup{start=Math.Max(15,(int)(i*50f/groups+1)),interval=placementRandom.Next(6,9),minimum=1,repeats=100,chance=1,area=placementRandom.NextDouble()>.5?-1:placementRandom.Next(200,301)/100f,edge=placementRandom.NextDouble()>.5,units=new[]{new UnitEntry{id="lamprey",min=1+bonus,max=placementRandom.Next(1+bonus,3+bonus)+bonus,chance=1}}});
            }
            if(Rules.Stat("trees_start_wave")>0)director.AddExtra(new SpawnGroup{start=0,repeats=-1,chance=1,area=-1,inset=2,units=new[]{new UnitEntry{id="tree",min=1,max=1,chance=1}}});
            if(eliteWaves.TryGetValue(Rules.Wave,out var elite))QueueElite(elite);
            if(Rules.ExtraElite){var pool=encounterData.enemies.Where(e=>e.role=="Elite").ToArray();QueueElite(pool[placementRandom.Next(pool.Length)].id);}
        }
        void QueueElite(string id)=>director.AddExtra(new SpawnGroup{start=1,repeats=-1,chance=1,area=-1,edge=true,units=new[]{new UnitEntry{id=id,min=1,max=1,chance=1}}});
        void TickNightmare(float dt)=>TickProjectileEvent(dt);
        Texture2D fogTexture;
        void DrawNightmareFog()
        {
            if(!fogWaves.Contains(Rules.Wave)||Rules.Phase!=RunPhase.Playing)return;
            if(fogTexture==null)
            {
                fogTexture=new Texture2D(256,256,TextureFormat.RGBA32,false);var pixels=new Color[256*256];
                for(int y=0;y<256;y++)for(int x=0;x<256;x++){float normalizedRadius=Vector2.Distance(new Vector2(x,y),Vector2.one*127.5f)/127.5f;pixels[y*256+x]=new Color(.015f,.02f,.04f,Mathf.SmoothStep(0,.98f,Mathf.InverseLerp(.55f,1,normalizedRadius)));}
                fogTexture.SetPixels(pixels);fogTexture.Apply();
            }
            Vector3 viewport=gameCamera.WorldToViewportPoint(playerPos);float radius=210*(2.75f-1.75f*Mathf.Min(20,Rules.Wave)/20)*Mathf.Max(.1f,1+Rules.Stat("fog_visibility")/100);
            Vector2 center=new Vector2(FullCanvas.x+viewport.x*FullCanvas.width,FullCanvas.y+(1-viewport.y)*FullCanvas.height);Rect r=new Rect(center-Vector2.one*radius,Vector2.one*radius*2);
            GUI.DrawTexture(r,fogTexture);Color fog=new Color(.015f,.02f,.04f,.98f);
            Box(Rect.MinMaxRect(FullCanvas.xMin,FullCanvas.yMin,FullCanvas.xMax,r.yMin),fog);Box(Rect.MinMaxRect(FullCanvas.xMin,r.yMax,FullCanvas.xMax,FullCanvas.yMax),fog);
            Box(Rect.MinMaxRect(FullCanvas.xMin,r.yMin,r.xMin,r.yMax),fog);Box(Rect.MinMaxRect(r.xMax,r.yMin,FullCanvas.xMax,r.yMax),fog);
        }
    }
    public sealed partial class RunRules
    {
        public float DangerMultiplier=>Difficulty==3?1.12f:Difficulty==4?1.26f:Difficulty==5?1.4f:Difficulty>=6?1.6f:1;
        public float EnemyDamageFactor=>DangerMultiplier*Mathf.Max(0,1+Stat("enemy_damage")/100);
        public string DifficultyDescription=>Difficulty==0?"No modifiers":Difficulty==1?"Additional enemies":Difficulty==2?"Additional enemies\nOne elite or horde wave":$"Additional enemies\n+{(DangerMultiplier-1)*100:0}% enemy HP and damage\n{(Difficulty>=4?3:1)} elite/horde waves"+(Difficulty>=5?"\nTwo final bosses":"")+(Difficulty>=6?"\n+10% enemy speed\nFog and projectile events":"");
    }
}


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
        public string projectileScene;public bool rotate=true, towards,fromPosition,baseConstant,baseAlternate,targetForward,scaleDuration,spawnRandom;
        public float speedRandom,speedStep,baseSpread,ringRotation,rotationRandom,targetSize,tail;public float[] degrees;
        public int chargeTarget,chargeTiming,spawnLimit=-1;
    }
    [Serializable] public sealed class EncounterState
    {public float hp,time,spacing,offset,rangeRandom,spacingRandom;public int updateFrames=-1;public bool allowWithin=true;public string movement;public EncounterAttack attack;}
    [Serializable] public sealed class EncounterEntry
    {
        public int boostCount=1;public float boostDamage=25;public bool canBoost=true;public string id,art,role;public float health,growth,speed,speedRandom,damage,damageGrowth,armor,armorGrowth,resistance;public int value;
        public EncounterState[] states;public EncounterAttack[] extra;
    }
    [Serializable] public sealed class DifficultyEntry {public int level;public StatEffect[] effects;}
    [Serializable] public sealed class ExtendedEncounterData {public EncounterEntry[] enemies;public DifficultyEntry[] difficulties;}
    public static class NightmareEventRules
    {
        // run_data.gd generates event waves only for mode 1. main.gd runs a
        // projectile event on every wave for mode 2, except active fog waves.
        public static void Plan(int difficulty,int projectileMode,System.Random random,IEnumerable<int> occupied,ISet<int> fog,ISet<int> bullets)
        {
            if(difficulty<=5)return;
            int firstFog=random.Next(5,8);fog.Add(firstFog);
            if(projectileMode==1)
            {
                var third=new[]{5,6,7}.Where(w=>w!=firstFog).ToArray();
                bullets.Add(random.Next(1,3));bullets.Add(random.Next(3,5));
                bullets.Add(third[random.Next(third.Length)]);bullets.Add(random.Next(8,11));
            }
            var blocked=new HashSet<int>(occupied);
            var remaining=Enumerable.Range(11,9).Where(w=>!blocked.Contains(w)).ToList();
            for(int i=0;i<2;i++){int pick=random.Next(remaining.Count);fog.Add(remaining[pick]);remaining.RemoveAt(pick);}
            if(projectileMode==1)
                for(int i=0;i<3;i++){int pick=random.Next(remaining.Count);bullets.Add(remaining[pick]);remaining.RemoveAt(pick);}
            else if(projectileMode==2)
            {
                for(int wave=1;wave<=20;wave++)if(!fog.Contains(wave))bullets.Add(wave);
            }
        }
    }
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
        void PlanEncounters(int projectileMode=1)
        {
            eliteWaves.Clear();hordeWaves.Clear();fogWaves.Clear();bulletWaves.Clear();
            var pool=encounterData.enemies.Where(e=>e.role=="Elite").Select(e=>e.id).ToList();
            int count=Rules.Difficulty<2?0:Rules.Difficulty<4?1:3;
            for(int i=0;i<count;i++)
            {
                int wave=11+i*3+placementRandom.Next(2);
                if(i<2&&Rules.Character?.id!="jack"&&Rules.Character?.id!="gangster"&&(Rules.Character?.id=="ogre"||placementRandom.NextDouble()<.4)){hordeWaves.Add(wave);continue;}
                int pick=placementRandom.Next(pool.Count);eliteWaves[wave]=pool[pick];pool.RemoveAt(pick);
            }
            NightmareEventRules.Plan(Rules.Difficulty,projectileMode,placementRandom,eliteWaves.Keys.Concat(hordeWaves),fogWaves,bulletWaves);
        }
        EnemyBehavior EncounterBehavior(EncounterEntry d,int state)
        {
            var s=d.states[state];var a=s.attack;
            return new EnemyBehavior{canBoost=d.canBoost,movement=(EnemyMovement)Enum.Parse(typeof(EnemyMovement),s.movement),spacing=s.spacing,spacingRandom=s.spacingRandom,offset=s.offset,
                charge=a.charge,ranged=a.enabled&&!a.charge,cooldown=a.cooldown,jitter=a.jitter,initial=a.initial,windup=a.windup,active=a.charge?a.active:a.tail,
                minRange=a.minRange,maxRange=a.spawn?9999:a.maxRange,chargeSpeed=a.chargeSpeed,projectileSpeed=a.projectileSpeed,projectileGrowth=a.growth,source=a,rangeRandom=s.rangeRandom,allowWithin=s.allowWithin,updateInterval=s.updateFrames<0?-1:s.updateFrames/60f};
        }
        void TickEncounterState(Enemy e,float dt)
        {
            if(!encounterDefinitions.TryGetValue(e.type.id,out var d))return;
            encounterStates.TryGetValue(e,out int current);int next=current;
            bool checkState=Mathf.FloorToInt(e.activeAge)>e.phaseCheck;
            if(checkState)e.phaseCheck=Mathf.FloorToInt(e.activeAge);
            for(int i=current+1;checkState&&i<d.states.Length;i++)if(e.hp/e.maxHp<=d.states[i].hp||e.activeAge>=d.states[i].time)next=i;
            if(next!=current)
            {if(e.type.role=="Boss")bossMutations+=next-current;encounterStates[e]=next;e.behavior=EncounterBehavior(d,next);e.attack=MakeAttack(e.behavior);e.hasDestination=false;ResetEnemyMovement(e);summonCounts.Remove(e);volleySigns.Remove(e);e.secondPhase=next>0;FloatText(e.pos,"MUTATION",coral,true);}
            if(e.type.id=="invoker"&&next==2)e.speed=e.type.speed*2.5f;
            if(e.type.id=="healer")HealNearbyEnemies(e);
            if(e.type.id=="monk")e.speed=e.type.speed*(next==1||next==3?.5f:next==2?1.25f:1);
            if(e.type.id=="gargoyle")e.speed=e.type.speed*(next==1?.75f:next>=2?.6f:1);
            if(d.extra.Length==0||e.attack==null)return;
            extraAttackClocks.TryGetValue(e,out float clock);clock-=dt;
            bool trigger=e.attack.Phase==AttackPhase.Active;
            if(trigger&&e.behavior.charge&&clock<=0)
            {var extra=d.extra[Math.Min(next,d.extra.Length-1)];FireEncounter(e,extra);clock=e.type.id=="croc"?.75f:e.type.id=="mantis"?.25f:extra.cooldown;}
            extraAttackClocks[e]=clock;
        }
        readonly Dictionary<Enemy,HashSet<Enemy>> healerOccupants=new Dictionary<Enemy,HashSet<Enemy>>();
        void HealNearbyEnemies(Enemy healer)
        {
            if(!healerOccupants.TryGetValue(healer,out var prior))prior=new HashSet<Enemy>();
            var current=new HashSet<Enemy>();
            foreach(var ally in enemies)if(ally!=healer&&ally.marker==null&&ally.hp>0&&CombatGeometry.HealerZoneOverlaps(healer.pos,ally.type.id,ally.pos))
            {current.Add(ally);if(!prior.Contains(ally)&&ally.hp<ally.maxHp){ally.hp=Mathf.Min(ally.maxHp,ally.hp+100+10*(Rules.Wave-1));ally.supportFlash=.2f;}}
            healerOccupants[healer]=current;
            // The supplied healer BoostZone masks enemy bodies (4); player
            // bodies use layer 2. Its Player callback branch is unreachable
            // in the supplied scene, so proximity must not heal the player.
        }
        readonly Dictionary<Enemy,int> summonCounts=new Dictionary<Enemy,int>();
        readonly Dictionary<Enemy,float> volleySigns=new Dictionary<Enemy,float>();
        Vector2 EnemyOrigin(Enemy e)=>e.pos+CombatGeometry.OriginOffset(e.type.id);
        Vector2 PlayerOrigin=>playerPos+CombatGeometry.PlayerOriginOffset;
        void SelectEnemyTarget(Enemy e)
        {
            Vector2 origin=EnemyOrigin(e);int nearest=int.MaxValue;e.targetPet=null;
            // enemy.gd visits pets first, then players with a strict comparison.
            foreach(var pet in pets)if(!pet.dead&&pet.spec.kind=="scapegoat")
            {
                float distance=(pet.pos+SupportBehavior.ScapegoatOriginOffset-origin).sqrMagnitude*10000;
                if(distance<nearest){nearest=(int)distance;e.targetPet=pet;}
            }
            if((PlayerOrigin-origin).sqrMagnitude*10000<nearest)e.targetPet=null;
            e.targetInitialized=true;e.targetUpdate=0;
        }
        void TickEnemyTarget(Enemy e,float dt)
        {
            if(!e.targetInitialized||e.targetPet!=null&&e.targetPet.dead){SelectEnemyTarget(e);return;}
            if(!pets.Any(p=>!p.dead&&p.spec.kind=="scapegoat"))return;
            e.targetUpdate+=dt;if(e.targetUpdate>=.25f)SelectEnemyTarget(e);
        }
        Vector2 EnemyTarget(Enemy e)
        {
            if(!e.targetInitialized||e.targetPet!=null&&e.targetPet.dead)SelectEnemyTarget(e);
            return e.targetPet==null?PlayerOrigin:e.targetPet.pos+SupportBehavior.ScapegoatOriginOffset;
        }
        void FireEncounter(Enemy e,EncounterAttack a=null)
        {
            Vector2 origin=EnemyOrigin(e);
            var d=encounterDefinitions[e.type.id];encounterStates.TryGetValue(e,out int state);a=a??d.states[state].attack;
            if(a.spawn)
            {
                summonCounts.TryGetValue(e,out int spawned);var type=catalog.Enemy(a.summon);
                int count=a.count;if(e.type.role=="Boss"||e.type.role=="Elite")count=EnemyReferenceRules.SummonCount(count,Rules.Stat("number_of_enemies"));
                if(a.spawnLimit>=0&&spawned>=a.spawnLimit)return;
                if(type!=null)for(int i=0;i<count;i++,spawned++)
                {
                    Vector2 pos=a.spawnRandom?SpawnPlacement.SourceRandomPosition(UnityEngine.Random.value,UnityEngine.Random.value):a.spawnSpread<0?origin:SpawnPlacement.SourceAreaPosition(origin,a.spawnSpread,UnityEngine.Random.value,UnityEngine.Random.value);
                    births.Push(new Birth{type=type,pos=pos-CombatGeometry.OriginOffset(type.id)},BirthLane.Summon);
                }
                summonCounts[e]=spawned;
                if(e.type.id=="slasher_egg"||e.type.id=="infected_slasher_egg")
                {
                    // The supplied egg calls die() with normal loot/global
                    // death callbacks when hatching, without a weapon hit.
                    e.hp=0;Rules.LivingEnemies=Mathf.Max(0,Rules.LivingEnemies-1);
                    Rules.TriggerDeathDamage();EnemyImplantDeath(e);Rules.ApplyBurningDeathStats(e.burnTicks>0);DropEnemyLoot(e);
                    Rules.RecordEnemy(e.type.id,false,e.activeAge,false);Rules.Kill();enemies.Remove(e);
                    if(e.obj!=null)Destroy(e.obj);
                }
                if(e.type.id=="mom"&&state==1&&d.extra.Length>0)FireEncounter(e,d.extra[0]);
                return;
            }
            Vector2 target=a.inDirection?origin+(e.attack?.Direction??Vector2.right):EnemyTarget(e);
            float baseSpread=UnityEngine.Random.Range(-a.baseSpread,a.baseSpread);
            if(a.baseConstant)
            {
                volleySigns.TryGetValue(e,out float prior);
                baseSpread=a.baseAlternate?(prior<0?a.baseSpread:-a.baseSpread):(UnityEngine.Random.value<.5f?-a.baseSpread:a.baseSpread);volleySigns[e]=baseSpread;
            }
            float ring=UnityEngine.Random.Range(0,a.ringRotation),rotation=UnityEngine.Random.Range(-a.rotationRandom,a.rotationRandom);
            for(int i=0;i<a.count;i++)
            {
                float baseAngle=Mathf.Atan2(target.y-origin.y,target.x-origin.x)+baseSpread;
                Vector2 pos=a.onTarget?target:origin;
                if(a.borders)
                {
                    float angle=a.constant?ring+i*Mathf.PI*2/a.count:UnityEngine.Random.Range(0,Mathf.PI*2);
                    if(a.degrees!=null&&i<a.degrees.Length)angle=-a.degrees[i]*Mathf.Deg2Rad+Mathf.Atan2(target.y-origin.y,target.x-origin.x);
                    pos+=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*a.spawnSpread*.5f;
                }
                else if(!a.onTargetOnce||i!=0)pos+=new Vector2(UnityEngine.Random.Range(-.5f,.5f),UnityEngine.Random.Range(-.5f,.5f))*a.spawnSpread;
                float shotAngle=a.randomDirection?UnityEngine.Random.Range(-Mathf.PI,Mathf.PI):baseAngle+UnityEngine.Random.Range(-a.spread,a.spread);
                if(a.constant&&a.count>1)shotAngle=baseAngle-a.spread+i*2*a.spread/(a.count-1);
                if(a.away||a.towards||a.fromPosition)
                {
                    target=a.fromPosition?EnemyTarget(e):a.towards?origin:pos;
                    Vector2 rotated=origin+(Vector2)(Quaternion.Euler(0,0,rotation*Mathf.Rad2Deg)*(target-origin));
                    Vector2 delta=rotated-(a.away?origin:pos);shotAngle=Mathf.Atan2(delta.y,delta.x);
                }
                Vector2 dir=new Vector2(Mathf.Cos(shotAngle),Mathf.Sin(shotAngle));
                float damage=EnemyReferenceRules.ScaledDamage(a.damage+a.growth*(Rules.Wave-1),Rules.EnemyDamageFactor*(e.boosted?1+e.boostDamage/100:e.type.role=="Elite"||e.type.role=="Boss"?EnemyReferenceRules.BossDamageFactor(Rules.Wave):1));
                if(a.projectile.Contains("slash"))
                {
                    var obj=HostileProjectile(a.projectileScene);obj.transform.position=pos;obj.transform.rotation=Quaternion.Euler(0,0,a.rotate?shotAngle*Mathf.Rad2Deg:0);
                    slashes.Add(new Slash{owner=e,obj=obj,pos=pos,direction=a.rotate?dir:Vector2.right,damage=damage,forward=a.projectile.Contains("_2"),deleteOnDeath=a.deleteOnDeath});continue;
                }
                if(shots.Count>=1200)break;
                bool pillar=a.projectile.Contains("pillar");var bullet=HostileProjectile(a.projectileScene);bullet.transform.position=pos;
                if(a.rotate)bullet.transform.rotation=Quaternion.Euler(0,0,shotAngle*Mathf.Rad2Deg);
                float speed=(int)((a.projectileSpeed+i*a.speedStep+UnityEngine.Random.Range(-a.speedRandom,a.speedRandom))*100)/100f;
                shots.Add(new Shot{obj=bullet,pos=pos,velocity=dir*speed*GameSettings.Value("enemySpeed"),life=pillar?BossPatterns.PillarEnd:20,pillar=pillar,damage=damage,hostile=true,attacker=e.type,sourceEnemy=e,deleteOnOwnerDeath=a.deleteOnDeath});
            }
        }
        void AddWaveEncounters()
        {
            friendlyBoostClock=0;friendlyBoostHP=0;Rules.RemovedEnemies=0;Rules.IsHordeWave=hordeWaves.Contains(Rules.Wave);healerOccupants.Clear();encounterStates.Clear();extraAttackClocks.Clear();summonCounts.Clear();volleySigns.Clear();PrepareProjectileEvent();
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
            AddShopEffectEncounters();
        }
        void QueueElite(string id)=>director.AddExtra(ShopSpawnCatalog.EliteGroup(id));
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
        public float EnemyHealthFactor=>Mathf.Max(.01f,1+Stat("enemy_health")/100);
        public float EnemySpeedFactor=>1+Stat("enemy_speed")/100;
        public float EnemyDamageFactor=>GameSettings.Value("enemyDamage")*DangerMultiplier*Mathf.Max(.01f,1+Stat("enemy_damage")/100);
        public string DifficultyDescription=>Difficulty==0?"No modifiers":Difficulty==1?"Additional enemies":Difficulty==2?"Additional enemies\nOne elite or horde wave":$"Additional enemies\n+{(DangerMultiplier-1)*100:0}% enemy HP and damage\n{(Difficulty>=4?3:1)} elite/horde waves"+(Difficulty>=5?"\nTwo final bosses":"")+(Difficulty>=6?"\n+10% enemy speed\nFog and projectile events":"");
    }
}


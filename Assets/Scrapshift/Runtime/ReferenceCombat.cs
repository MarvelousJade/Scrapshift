using System;
using System.Collections.Generic;
using UnityEngine;
namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        sealed class Birth
        {
            public EnemyEntry type;
            public Vector2 pos;
        }
        readonly ReferenceBirthQueue<Birth> births=new ReferenceBirthQueue<Birth>();
        readonly List<Enemy> corpses=new List<Enemy>();
        readonly GameObject[] equipmentViews=new GameObject[24];
        readonly SpriteRenderer[] muzzleViews = new SpriteRenderer[24];
        readonly int[] weaponShots=new int[24];
        readonly float[] weaponClocks=new float[24],swings=new float[24];
        readonly Enemy[] weaponTargets=new Enemy[24];
        readonly Vector2[] weaponDirections=new Vector2[24];
        readonly float[] weaponSpreads=new float[24];
        SpawnDirector director;
        System.Random placementRandom = new System.Random();
        Vector2 groupAnchor;
        int chargeReleases, rangedReleases, spawnRelocations, bossMutations;
        void RemoveEnemyForPerformance(Enemy enemy)
        {
            // EntitySpawner calls die() with can_drop_loot=false, retaining
            // global death callbacks and counters but granting no loot batch.
            enemy.hp=0;Rules.RemovedEnemies++;
            Rules.LivingEnemies=Mathf.Max(0,Rules.LivingEnemies-1);
            Rules.TriggerDeathDamage();EnemyImplantDeath(enemy);Rules.ApplyBurningDeathStats(enemy.burnTicks>0);
            Rules.RecordEnemy(enemy.type.id,false,enemy.activeAge,false);Rules.Kill();
            enemies.Remove(enemy);
            SpawnEnemyChildren(enemy);enemy.deathAge=0;enemy.knockback=Vector2.zero;
            if(enemy.telegraph!=null)enemy.telegraph.enabled=false;corpses.Add(enemy);
        }
        void SpawnEnemyChildren(Enemy enemy)
        {
            if(enemy.type.role!="Spawner")return;
            var type=catalog.Enemy(enemy.type.id=="bloated_spawner"?"dire_junkie":"junkie");
            for(int i=0;i<(enemy.type.id=="bloated_spawner"?5:3);i++)
            {
                Vector2 position=SpawnPlacement.SourceAreaPosition(EnemyOrigin(enemy),2,UnityEngine.Random.value,UnityEngine.Random.value);
                births.Push(new Birth{type=type,pos=position-CombatGeometry.OriginOffset(type.id)},BirthLane.Summon);
            }
        }
        void BeginSpawnGroup(SpawnGroup group)
        {
            var active = enemies.FindAll(e=>e.marker==null && e.type.role!="Boss" && e.type.role!="Elite" && e.type.role!="Tree");
            int cap = catalog.data.waves[Rules.Wave-1].cap;
            while(active.Count>cap)
            {
                var priority = active.FindAll(e=>e.type.id=="baby_alien" || e.type.id=="chaser" || e.type.id=="helmet_alien" || e.type.id=="fin_alien");
                var pool = priority.Count>0 ? priority : active;
                var victim = pool[placementRandom.Next(pool.Count)];
                RemoveEnemyForPerformance(victim);active.Remove(victim);
            }
            groupAnchor = SpawnPlacement.Anchor(group, PlayerOrigin, placementRandom);
        }
        float Radius(Enemy e) => CombatGeometry.Enemy(e.type.id).body;
        float HurtRadius(Enemy e) => CombatGeometry.Enemy(e.type.id).hurt;
        float ContactRadius(Enemy e) => CombatGeometry.Enemy(e.type.id).contact;
        Vector2 HurtCenter(Enemy e) => e.pos + CombatGeometry.HurtOffset(e.type.id);
        Vector2 ContactCenter(Enemy e) => e.pos + CombatGeometry.ContactOffset(e.type.id);
        Vector2 PlayerHurtCenter => playerPos + CombatGeometry.PlayerHurtOffset;
        GameObject SpriteObject(string name,Sprite sprite,Vector2 pos,int order=5)
        {
            var obj=new GameObject(name);
            obj.transform.position=new Vector3(pos.x,pos.y,-1);
            var r=obj.AddComponent<SpriteRenderer>();
            r.sprite=sprite;
            r.sharedMaterial=catalog.spriteMaterial;
            r.sortingOrder=order;
            return obj;
        }
        GroundShadow AddGroundShadow(GameObject actor, float width, float y)
        {
            var shadow = actor.AddComponent<GroundShadow>();
            shadow.Initialize(catalog.shadowMesh, catalog.shadowMaterial, width, y);
            return shadow;
        }
        void StartWave()
        {
            Rules.CaptureWaveRetryState();
            director=new SpawnDirector(CurrentSourceWave(),UnityEngine.Random.Range(1,int.MaxValue),Rules.Stat("number_of_enemies"),Rules.Stat("trees"),Rules.ExtraLootAliens);
            births.Clear();cratesSpawned=0;
            Array.Clear(weaponClocks,0,weaponClocks.Length);
            SyncEquipment();ResetStructures();AddWaveEncounters();
            if(Rules.Wave==20)
            {
                string first=placementRandom.Next(2)==0?"predator":"invoker";QueueElite(first);
                if(Rules.Difficulty>=5||Rules.Stat("double_boss")>0)QueueElite(first=="predator"?"invoker":"predator");
            }
        }
        void AdvanceWave()
        {
            if(Rules.Phase!=RunPhase.Shop)return;ClearCombat();Rules.NextWave();gameCamera.orthographicSize=ArenaGeometry.CameraHalfHeight;playerPos=CombatGeometry.PlayerBodyStart;StartWave();sound.Play(SoundCue.Install);
        }
        void SyncEquipment()
        {
            for(int i=0;i<equipmentViews.Length;i++)
            {
                if (equipmentViews[i] != null) Destroy(equipmentViews[i]);
                muzzleViews[i] = null;weaponHands[i]=null;
                equipmentViews[i] = i < Rules.Inventory.Count ? SpriteObject(Rules.Inventory[i].title, Rules.Inventory[i].image, playerPos, 12) : null;
                swings[i] = 0;meleeAttacks[i]=null;weaponShots[i]=0;weaponTargets[i]=null;weaponDirections[i]=Vector2.right;weaponSpreads[i]=0;
                if (equipmentViews[i] == null) continue;
                weaponClocks[i]=SpecialWeaponRules.InitialCooldown(Rules.WeaponCooldown(Rules.Inventory[i]),Rules.Inventory.Count,UnityEngine.Random.value);
                equipmentViews[i].transform.localScale = Vector3.one * ActorPresentation.WeaponScale(Rules.Inventory[i]);
                AddWeaponHand(i,Rules.Inventory[i]);
                if (Rules.Inventory[i].melee) continue;

            }
        }
        void SetEquipmentVisible(bool value)
        {
            foreach(var v in equipmentViews)if(v!=null)v.SetActive(value);
        }
        Vector2 RandomPosition(Vector2 away,float distance)
        {
            Vector2 p=Vector2.zero;for(int i=0;i<150;i++)
            {
                p=new Vector2(UnityEngine.Random.Range(-ArenaGeometry.HalfSize.x,ArenaGeometry.HalfSize.x),UnityEngine.Random.Range(-ArenaGeometry.HalfSize.y,ArenaGeometry.HalfSize.y));if(Vector2.Distance(p,away)>=Mathf.Max(.25f,distance-i*.03f))break;
            }
            return p;
        }
        void QueueGroup(SpawnGroup group,UnitEntry unit,int count)
        {
            var def = catalog.Enemy(unit.id);
            if (def == null) return;
            for (int i = 0; i < count; i++)
                births.Push(new Birth { type = def, pos = SpawnPlacement.Position(group, unit, groupAnchor, PlayerOrigin, placementRandom)-CombatGeometry.OriginOffset(def.id) }, def.role=="Tree"?BirthLane.Tree:def.role=="Boss"||def.role=="Elite"?BirthLane.Boss:BirthLane.Regular);
        }
        void UpdateBirthQueue(float dt) => births.Tick(dt, b=>Spawn(b.type,b.pos));
        EnemyAttack MakeAttack(EnemyBehavior behavior) => behavior.charge || behavior.ranged ?
            new EnemyAttack(behavior,()=>UnityEngine.Random.Range(Mathf.Max(1f/60,behavior.cooldown-behavior.jitter),behavior.cooldown+behavior.jitter)) : null;
        void Spawn(EnemyEntry def,Vector2 pos)
        {
            if(campaignCheck)campaignEnemyIds.Add(def.id);
            var behavior = encounterDefinitions.TryGetValue(def.id,out var encounter)?EncounterBehavior(encounter,0):EnemyBehavior.For(def.id);
            var attack = MakeAttack(behavior);
            var obj=SpriteObject(def.id,def.image,pos);var shadow=AddGroundShadow(obj, def.image.bounds.size.x * .9f, -def.image.bounds.extents.y + .05f);obj.SetActive(false);var marker=SpriteObject("Spawn warning",catalog.warning,pos+CombatGeometry.OriginOffset(def.id),2);if(def.role=="Tree")
            {
                marker.GetComponent<SpriteRenderer>().enabled=false;
                foreach(float sign in new[]{-1f,1f})
                {var stroke=MakeLine("Green tree warning",marker.transform,RunFeedback.SpawnColor(true,1),.12f);stroke.positionCount=2;stroke.SetPosition(0,new Vector3(-.4f,-.4f*sign,0));stroke.SetPosition(1,new Vector3(.4f,.4f*sign,0));}
            }
            sound.Play(SoundCue.Spawn,pos);
            marker.transform.localScale=Vector3.one*.6f;marker.transform.rotation=Quaternion.Euler(0,0,UnityEngine.Random.Range(-25,25));float hp=EnemyReferenceRules.ScaledHealth(def.health+def.growth*(Rules.Wave-1),Rules.EnemyHealthFactor*Rules.DangerMultiplier*GameSettings.Value("enemyHealth"));enemies.Add(new Enemy
            {
                obj=obj,shadow=shadow,marker=marker,type=def,hp=hp,maxHp=hp,pos=pos,speed=Mathf.Max(0,def.speed+(int)UnityEngine.Random.Range(-def.speedRandom*GameSettings.Value("enemySpeed")*100,def.speedRandom*GameSettings.Value("enemySpeed")*100)/100f),cooldown=def.role=="Buffer"?.25f:1,renderers=obj.GetComponents<Renderer>(),wander=new Vector2(UnityEngine.Random.Range(-behavior.offset,behavior.offset),UnityEngine.Random.Range(-behavior.offset,behavior.offset)),attack=attack,behavior=behavior,spacing=behavior.spacing+UnityEngine.Random.Range(-1f,1f),orbitSign=UnityEngine.Random.value<.5f?-1:1
            })
            ;var spawned=enemies[enemies.Count-1];ResetEnemyMovement(spawned);
            if(def.role=="Elite"||def.role=="Boss"){float factor=EnemyReferenceRules.BossHealthFactor(Rules.Wave,Rules.Difficulty>=5||Rules.Stat("double_boss")>0);spawned.hp=EnemyReferenceRules.SourceRound(spawned.hp*factor);spawned.maxHp=spawned.hp;}
            peakEnemies=Mathf.Max(peakEnemies,enemies.Count);
        }
        void UpdateEnemies(float dt)
        {
            TickFriendlyBoost(dt);
            // A lethal player contact flushes older queued enemy deaths before
            // terminal cleanup; iterate identities so that flush may remove
            // earlier list entries without invalidating this traversal.
            var enemyBatch=enemies.ToArray();Array.Reverse(enemyBatch);
            foreach(var e in enemyBatch)
            {
                if(Rules.Phase!=RunPhase.Playing)return;
                if(e.hp<=0||!enemies.Contains(e))continue;
                e.age += dt;e.iceClock=Mathf.Max(0,e.iceClock-dt); e.flash = Mathf.Max(0, e.flash - dt);
                if (e.marker != null)
                {
                    var mr = e.marker.GetComponent<SpriteRenderer>();
                    mr.color = RunFeedback.SpawnColor(e.type.role=="Tree",Mathf.Repeat(e.age * (2 + e.age * 8), 1) < .3f ? .35f : 1);
                    e.marker.transform.localScale = Vector3.one * (.52f + .12f * e.age);
                    if (e.age < 1) continue;
                    if (e.type.role!="Boss" && e.type.role!="Elite" && e.type.role!="Tree" && RunRules.Touches(ContactCenter(e), PlayerHurtCenter, ContactRadius(e) + CombatGeometry.PlayerHurtRadius))
                    {
                e.pos=SpawnPlacement.SourceRandomPosition(UnityEngine.Random.value,UnityEngine.Random.value)-CombatGeometry.OriginOffset(e.type.id);e.marker.transform.position=EnemyOrigin(e);e.age=0;
                        spawnRelocations++; continue;
                    }
                    Destroy(e.marker); e.marker = null; e.obj.SetActive(true); e.activeAge = 0;
                    if(e.type.role=="Tree")InvalidateLootwormTargets();
                }
                e.activeAge += dt;TickEncounterState(e,dt);
                TickEnemyTarget(e,dt);Vector2 targetPosition=EnemyTarget(e),origin=EnemyOrigin(e);
                Vector2 toward = (targetPosition-origin).normalized;
                float distance = Vector2.Distance(origin,targetPosition),speed=EnemyStatSpeed(e);
                e.cooldown -= dt; e.action += dt;
                string role = e.type.role;
                if (e.type.id=="predator" && !encounterDefinitions.ContainsKey(e.type.id) && !e.secondPhase && Mathf.Floor(e.activeAge)>e.phaseCheck)
                {
                    e.phaseCheck=Mathf.FloorToInt(e.activeAge);
                    if(e.hp<=e.maxHp*.5f || e.phaseCheck>=45)
                    { e.secondPhase=true; bossMutations++; e.behavior=EnemyBehavior.For(e.type.id,true); e.attack=MakeAttack(e.behavior); e.hasDestination=false; FloatText(e.pos,"MUTATION",coral,true); }
                }
                bool charge = e.behavior.charge;
                switch(e.behavior.movement)
                {
                    case EnemyMovement.KeepRange:
                        toward *= Mathf.Sign(distance-e.spacing);
                        speed = Mathf.Min(speed, Mathf.Abs(distance-e.spacing)/dt); break;
                    case EnemyMovement.Offset:
                        toward=(targetPosition+e.wander-origin).normalized;speed=Mathf.Min(speed,Vector2.Distance(targetPosition+e.wander,origin)/dt);break;
                    case EnemyMovement.Destination: case EnemyMovement.AroundPlayer: case EnemyMovement.Materials:
                        e.moveUpdate+=dt;
                        if(!e.hasDestination || Vector2.Distance(origin,e.destination)<.1f || e.behavior.updateInterval>=0&&e.moveUpdate>=e.behavior.updateInterval)
                        {
                            e.hasDestination=true;e.moveUpdate=0;
                            e.destination=e.behavior.movement==EnemyMovement.AroundPlayer?SpawnPlacement.SourceAreaPosition(targetPosition,e.moveRange,UnityEngine.Random.value,UnityEngine.Random.value):SpawnPlacement.SourceRandomPosition(UnityEngine.Random.value,UnityEngine.Random.value);
                            if(e.behavior.movement==EnemyMovement.Materials)
                            {
                                var materials=pickups.FindAll(p=>!p.fruit);
                                if(materials.Count>0) e.destination=materials[placementRandom.Next(materials.Count)].motion.position;
                            }
                        }
                        toward=(e.destination-origin).normalized;
                        speed=Mathf.Min(speed,Vector2.Distance(e.destination,origin)/dt); break;
                    case EnemyMovement.Pursuer: speed+=Mathf.Min(10,Mathf.FloorToInt(e.action))*EnemyReferenceRules.AdditiveSpeedBonus(.45f,GameSettings.Value("enemySpeed")); break;
                }
                if(e.type.id=="evil_mob") { speed=Mathf.Max(0,speed-e.evolution*1.25f); EatMaterials(e,dt); }
                if (role == "Tree") speed = 0;
                if (e.attack != null)
                {
                    var previous = e.attack.Phase;
                    bool released = e.attack.Step(dt,targetPosition-origin,distance>=e.behavior.minRange&&distance<=e.behavior.maxRange,()=>ChargeTarget(e,targetPosition));
                    if (previous != AttackPhase.Windup && e.attack.Phase == AttackPhase.Windup && charge)
                        sound.Play(SoundCue.Charge, e.pos);
                    if (charge && e.attack.Phase == AttackPhase.Windup) speed = 0;
                    else if (charge && e.attack.Phase == AttackPhase.Active)
                    { toward = e.attack.Direction; speed = EnemyStatSpeed(e) + EnemyReferenceRules.AdditiveSpeedBonus(e.behavior.chargeSpeed,GameSettings.Value("enemySpeed")); }
                    if (released)
                    {
                        if (charge) chargeReleases++;
                        else
                        {
                            rangedReleases++;
                            if(encounterDefinitions.ContainsKey(e.type.id))FireEncounter(e);
                            else if(e.type.id=="slasher") EnemySlash(e);
                            else for(int p=0;p<(role=="Boss"?16:1);p++)
                            {
                                Vector2 direction = role=="Boss" ? UnityEngine.Random.insideUnitCircle.normalized :
                                    (Vector2)(Quaternion.Euler(0,0,UnityEngine.Random.Range(-e.behavior.spread,e.behavior.spread)*Mathf.Rad2Deg)*e.attack.Direction);
                                EnemyBullet(e,direction);
                            }
                        }
                    }
                }
                speed = Mathf.Max(0,speed-e.contactSlow-e.itemSlow);
                e.contactSlow = Mathf.Max(0, e.contactSlow - 3 * dt);
                if (role == "Buffer" && e.cooldown <= 0)
                {
                    e.cooldown = 4;
                    int count=encounterDefinitions.TryGetValue(e.type.id,out var spec)?spec.boostCount:1;
                    for(int boost=0;boost<count;boost++)BoostNearbyEntity(e,spec?.boostDamage??25);

                }
                Vector2 separation = Vector2.zero;
                foreach (var other in enemies)
                {
                    if (other == e || other.marker != null) continue;
                    var delta=e.pos+CombatGeometry.BodyOffset(e.type.id)-other.pos-CombatGeometry.BodyOffset(other.type.id);float d=delta.magnitude,gap=Radius(e)+Radius(other);
                    if (d > .001f && d < gap) separation += delta / d * (gap - d) * 5;
                }
                bool winding = charge && e.attack != null && e.attack.Phase == AttackPhase.Windup;
                if (winding) separation = Vector2.zero;
                if (charge && e.attack.Phase == AttackPhase.Active) separation *= .15f;
                Vector2 push = CombatGeometry.StepKnockback(ref e.knockback, dt);
                e.pos = RunRules.ClampPosition(EnemyOrigin(e)+(toward*speed+separation)*dt+push)-CombatGeometry.OriginOffset(e.type.id);
                UpdateEnemyPresentation(e, toward * speed, dt);
                if(e.type.id=="predator") UpdateBossArm(e,dt);
                foreach(var pet in pets)if(!pet.dead&&pet.spec.kind=="scapegoat"&&Vector2.Distance(e.pos,pet.pos)<ContactRadius(e)+.3f)DamageScapegoat(pet);
                if (role != "Tree" && PlayerHurtboxActive && CombatGeometry.ContactEnabled(e.knockback, speed) && RunRules.Touches(ContactCenter(e), PlayerHurtCenter, ContactRadius(e) + CombatGeometry.PlayerHurtRadius))
                {
                    HurtPlayer(EnemyReferenceRules.ScaledDamage(e.type.damage+e.type.damageGrowth*(Rules.Wave-1),Rules.EnemyDamageFactor*(e.boosted?1+e.boostDamage/100:1)*(role=="Boss"||role=="Elite"?EnemyReferenceRules.BossDamageFactor(Rules.Wave):1)),e.type,e);
                    e.contactSlow = CombatGeometry.ContactSlow(e.contactSlow, EnemyStatSpeed(e)-e.itemSlow);
                    if (role == "Pursuer") e.action = 0;
                }
            }
        }

        LineRenderer MakeLine(string name, Transform parent, Color color, float width, bool loop = false)
        {
            var obj = new GameObject(name); obj.transform.SetParent(parent, false);
            var line = obj.AddComponent<LineRenderer>(); line.sharedMaterial = catalog.spriteMaterial;
            line.useWorldSpace = false; line.loop = loop; line.startWidth = line.endWidth = width;
            line.startColor = line.endColor = color; line.sortingOrder = 4;
            line.numCapVertices = 4; return line;
        }

        void UpdateEnemyPresentation(Enemy e, Vector2 velocity, float dt)
        {
            bool winding = e.attack != null && e.attack.Phase == AttackPhase.Windup;
            bool dashing = e.attack != null && e.attack.Phase == AttackPhase.Active && e.behavior.charge;
            var sprite = e.obj.GetComponent<SpriteRenderer>();
            if (Mathf.Abs(velocity.x) > .05f) sprite.flipX = velocity.x < 0;
            float scale = ActorPresentation.EnemyScale(e.type) * (1+e.evolution*.2f);
            var spring=SpringMotion.Pose(e.activeAge*(e.type.id=="baby_alien"?1:.8f),velocity.sqrMagnitude>.01f,reducedMotion);
            float anticipation = reducedMotion || !winding ? 0 : e.attack.Progress * .16f;
            float emerge = reducedMotion ? 1 : Mathf.Lerp(.45f, 1, Mathf.Clamp01(e.activeAge / .16f));
            e.obj.transform.position = new Vector3(e.pos.x, e.pos.y+spring.z*scale, -1);
            e.obj.transform.localScale = new Vector3(scale * (spring.x + anticipation) * emerge,
                scale * (spring.y - anticipation) * emerge, 1);
            e.obj.transform.rotation = Quaternion.Euler(0, 0, reducedMotion ? 0 : dashing ? -Mathf.Sign(velocity.x) * 12 : 0);
            e.shadow.Place(e.pos, scale * emerge);
            e.supportFlash = Mathf.Max(0, e.supportFlash - dt);
            sprite.color = winding ? Color.Lerp(Color.white, new Color(1, .35f, .2f), e.attack.Progress) :
                e.boosted || e.supportFlash > 0 ? new Color(.85f, .55f, 1) : Color.white;
            Flash(e, e.flash > 0 ? 1 : 0);

        }
        void HurtPlayer(float amount,EnemyEntry attacker=null,Enemy sourceEnemy=null)
        {
            if(invulnerable>0)return;float before=Rules.Health;Rules.Hurt(amount);float actual=before-Rules.Health;if(Rules.LastHitDodged&&Rules.DodgeRetaliation>0&&sourceEnemy!=null)implantRetaliations.Enqueue((sourceEnemy,Rules.DodgeRetaliation));if(actual<=0)
            {
                FloatText(playerPos,Rules.LastHitProtected?"BLOCK":"DODGE",mint,false);invulnerable=CombatGeometry.Invulnerability(0,Rules.MaxHealth);return;
            }
            AttractNearbyConsumables();QueueImplantExplosions("explode_on_hit",playerPos);lastAttacker=attacker;FloatText(playerPos,$"-{Mathf.CeilToInt(actual)}",coral,true);playerFlash=CombatGeometry.FlashDuration;invulnerable=CombatGeometry.Invulnerability(actual,Rules.MaxHealth);Shake(.05f);ImpactBurst(playerPos,UnityEngine.Random.insideUnitCircle,false,true);sound.Play(SoundCue.Hurt);PlayerHitParticles();
        }
        void EnemyBullet(Enemy e,Vector2 direction)
        {
            if (shots.Count >= 400) return;
            var obj = new GameObject("Hostile plasma"); obj.transform.position = e.pos;
            var rim = MakeLine("Coral projectile rim", obj.transform, new Color(1, .22f, .15f), .085f, true);
            rim.sortingOrder = 13; rim.positionCount = 16;
            for (int i = 0; i < 16; i++)
            { float a = i * Mathf.PI * 2 / 16; rim.SetPosition(i, new Vector3(Mathf.Cos(a) * .105f, Mathf.Sin(a) * .105f, 0)); }
            var core = MakeLine("Bright core", obj.transform, new Color(1, .9f, .55f), .09f);
            core.sortingOrder = 14; core.positionCount = 2;
            core.SetPosition(0, new Vector3(-.025f, 0, -.01f)); core.SetPosition(1, new Vector3(.025f, 0, -.01f));
            Vector2 origin=e.pos+(e.type.id=="predator"?Vector2.down*.88f:Vector2.zero);
            obj.transform.position=origin;
            shots.Add(new Shot { obj = obj, pos = origin, velocity = direction.normalized * e.behavior.projectileSpeed, life = 30,
                damage = ProjectileDamage(e), hostile = true, attacker=e.type,sourceEnemy=e });
        }
        Vector2 AimEquipment(int index,Equipment weapon,bool shooting,bool manual,Vector2 towardTarget,bool hasTarget,Vector2 manualDirection,Vector2 rest)
        {
            Vector2 direction=hasTarget?towardTarget.normalized:shooting?weaponDirections[index]:rest;
            if(manual)direction=CombatInput.ShouldRotateManual(weapon.melee,shooting)?manualDirection.normalized:weaponDirections[index];
            else if(shooting&&hasTarget)direction=Quaternion.Euler(0,0,weaponSpreads[index]*Mathf.Rad2Deg)*direction;
            weaponDirections[index]=direction;return direction;
        }
        void UpdateEquipment(float dt)
        {
            for(int i=0;i<Rules.Inventory.Count;i++)
            {
                var w=Rules.Inventory[i];WeaponMotion.Step(w,dt,ref swings[i],ref weaponClocks[i]);float range=Rules.WeaponRange(w);Vector2 mount=ArrivalWeaponMotion.TransformMount(playerPos,ActorPresentation.WeaponMount(Rules.Inventory.Count,i),WeaponArrivalTime);Enemy nearest=null;float distance=RunFeedback.DetectionRange(range)*RunFeedback.DetectionRange(range);foreach(var e in enemies)
                {
                    if(e.marker!=null||e.hp<=0)continue;float d=(e.pos-mount).sqrMagnitude;if(d<distance)
                    {
                        distance=d;nearest=e;
                    }
                }
                bool shooting=swings[i]>0;
                var aim=shooting?weaponTargets[i]:nearest;
                bool hasTarget=aim!=null&&enemies.Contains(aim);
                var stick=GameSettings.Flag("gamepadMove")&&CombatInput.UsingGamepad?CombatInput.AimStick:Vector2.zero;
                bool manual=CombatInput.ManualAim(GameSettings.Flag("manualAim",false),GameSettings.Flag("aimPressed",false),Input.GetMouseButton(0),stick);
                Vector2 manualDirection=weaponDirections[i];
                if(manual&&CombatInput.ShouldRotateManual(w.melee,shooting))manualDirection=GameSettings.Flag("gamepadMove")&&CombatInput.UsingGamepad?CombatInput.AimDirection:((Vector2)gameCamera.ScreenToWorldPoint(Input.mousePosition)-mount).normalized;
                Vector2 direction=AimEquipment(i,w,shooting,manual,hasTarget?aim.pos-mount:Vector2.zero,hasTarget,manualDirection,ActorPresentation.RestDirection(playerAnimation.FacingLeft));
                bool fire=Rules.CanAttack&&!shooting&&(manual||nearest!=null&&RunFeedback.CanAttack(range,distance))&&weaponClocks[i]<=0&&Rules.Phase==RunPhase.Playing;
                if(fire)
                {
                    weaponShots[i]++;float basis=Rules.WeaponCooldown(w)*60;int count=Mathf.Min(6,Rules.Inventory.Count);
                    float variation=Mathf.Min(count*basis/5,count*5);
                    weaponClocks[i]=w.reloadEvery>0&&weaponShots[i]%w.reloadEvery==0?basis*w.reloadMultiplier/60:UnityEngine.Random.Range(Mathf.Max(1,basis-variation),basis+variation)/60;
                    float accuracy=Mathf.Clamp01((WeaponReference.Profile(w)?.accuracy??1)+Rules.Stat("accuracy")/100);
                    weaponSpreads[i]=UnityEngine.Random.Range(-1+accuracy,1-accuracy);
                    direction=Quaternion.Euler(0,0,weaponSpreads[i]*Mathf.Rad2Deg)*direction;
                    weaponDirections[i]=direction;swings[i]=Rules.WeaponAnimationDuration(w);weaponTargets[i]=manual?null:nearest;
                    if(w.melee)BeginMeleeAttack(i,w,direction,manual?range:Mathf.Sqrt(distance));
                }
                var view=equipmentViews[i];if(view!=null)
                {
                    float age = Rules.WeaponAnimationDuration(w) - swings[i];
                    float kick = WeaponMotion.Recoil(w,age,Rules.WeaponFireRate(w));
                    float offset = ActorPresentation.WeaponPivot(w.id) + (w.melee ? swings[i] * 2 : -kick * (reducedMotion ? .2f : 1));
                    Vector2 visualDirection=ArrivalWeaponMotion.VisualDirection(direction,WeaponArrivalTime);
                    view.transform.position = new Vector3(mount.x + visualDirection.x * offset, mount.y + visualDirection.y * offset, -2);
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    view.transform.rotation = Quaternion.Euler(0, 0, angle+ArrivalWeaponMotion.Rotation(WeaponArrivalTime));
                    view.GetComponent<SpriteRenderer>().flipY = direction.x < 0;
                    view.GetComponent<SpriteRenderer>().sortingOrder=ActorPresentation.WeaponOrder(Rules.Inventory.Count,i);
                    PoseWeaponHand(i,direction.x<0,ActorPresentation.WeaponOrder(Rules.Inventory.Count,i));
                    if (muzzleViews[i] != null)
                    {
                        muzzleViews[i].enabled = age < .066f;
                        muzzleViews[i].sprite = catalog.muzzleFrames[Mathf.Clamp((int)(age / .022f), 0, 2)];
                    }
                }
                if(w.melee){TickMeleeAttack(i,w,mount,dt);continue;}
                if(!fire||!Rules.CanAttack)continue;float damage=Rules.WeaponDamage(w);
                {
                    // Source Sprite/Muzzle is authored per weapon. Firing starts
                    // before the new recoil tween; sprite artwork bounds and
                    // reduced-motion visual recoil do not move this spawn point.
                    Vector2 muzzle = WeaponReference.MuzzlePosition(w,mount,direction,WeaponArrivalTime);
                    if (muzzleViews[i] != null) { muzzleViews[i].enabled = true; muzzleViews[i].sprite = catalog.muzzleFrames[0]; }
                    int pellets=Mathf.Max(1,w.pellets+(int)Rules.Stat("projectiles"));
                    for(int p=0;p<pellets;p++)
                    {
                        Vector2 dir=Quaternion.Euler(0,0,WeaponReference.Spread(w,Rules.Stat("projectiles"),UnityEngine.Random.value)*Mathf.Rad2Deg) * direction;var obj=SpriteObject("Rivet tracer",catalog.muzzleFrames[2],muzzle,13);obj.transform.localScale=Vector3.one*.45f;obj.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(dir.y,dir.x)*Mathf.Rad2Deg);shots.Add(new Shot
                        {
                            obj=obj,pos=muzzle,velocity=dir*WeaponReference.Speed(w,Rules.Stat("range")),life=WeaponReference.Lifetime(range,WeaponReference.Speed(w,Rules.Stat("range")),manual),damage=damage,crit=w.crit,pierce=w.piercing+(int)Rules.Stat("piercing"),knockback=w.knockback,weapon=w,weaponIndex=i,burnPayload=Rules.CaptureBurn(w),bounces=w.bounce+(int)Rules.Stat("bounce")
                        })
                        ;ConfigureReferenceShot(shots[shots.Count-1],manual);shotsFired++;
                    }
                    WeaponReference.Fire(sound,w,playerPos);
                }
            }
        }
        bool ResolvePlayerProjectileContact(Shot shot,Vector2 previous,bool hurtboxActive)
        {
            if(!hurtboxActive||Rules.Phase!=RunPhase.Playing)return false;
            bool healing=shot.structureSpec?.healing==true;
            if(!healing&&!shot.hostile)return false;
            Vector2 offset=shot.pillar?Vector2.up*.04f:Vector2.zero;
            float radius=healing?1.04f:shot.pillar?BossPatterns.PillarRadius:CombatGeometry.HostileRadius;
            float reach=CombatGeometry.PlayerHurtRadius+radius;
            bool spawnOverlap=shot.age<=ReferencePhysicsClock.Step+.000001f&&Vector2.Distance(PlayerHurtCenter,previous+offset)<=reach;
            if(!spawnOverlap&&Vector2.Distance(PlayerHurtCenter,shot.pos+offset)>reach)return false;
            // player.take_damage counts a Projectile contact before its healing,
            // dodge/protection or invincibility branches, regardless of HP lost.
            Rules.RecordProjectileHit();
            if(healing)
            {
                float health=Rules.Health;Rules.Heal(shot.damage);
                if(Rules.Health>health){FloatText(playerPos,$"+{Rules.Health-health:0} HP",mint,false);sound.Play(SoundCue.Pickup);}
            }
            else HurtPlayer(shot.damage,shot.attacker,shot.sourceEnemy);
            shot.life=0;return true;
        }
        void UpdateWeapons(float dt)
        {
            TickReferenceExplosions(dt);
            // Hurtbox.disable uses a deferred collision-shape change. Contacts
            // queued in this batch remain eligible; later batches do not enter.
            bool playerHurtboxActive=PlayerHurtboxActive;
            for (int i = shots.Count - 1; i >= 0; i--)
            {
                var s = shots[i]; Vector2 old = s.pos;float previousAge=s.age;
                if(s.deleteOnOwnerDeath&&(s.sourceEnemy==null||s.sourceEnemy.hp<=0||!enemies.Contains(s.sourceEnemy))){Destroy(s.obj);shots.RemoveAt(i);continue;}
                if(s.projectileStopped)
                {
                    s.age+=dt;s.life-=dt;s.pos+=s.velocity*dt;s.obj.transform.position=s.pos;s.projectileView?.Pose(s.age);
                    if(s.life<=0){Destroy(s.obj);shots.RemoveAt(i);}continue;
                }
                s.age+=dt;
                if(s.structureSpec!=null)SupportProjectileArt.Pose(s.obj.GetComponent<SpriteRenderer>(),s.structureSpec.kind,s.age);
                else if(SupportProjectileBehavior.PetArtKind(s.weapon) is string petArt)SupportProjectileArt.Pose(s.obj.GetComponent<SpriteRenderer>(),petArt,s.age);
                s.projectileView?.Pose(s.age);if(s.projectileRef!=null&&s.projectileRef.spin!=0)s.obj.transform.Rotate(0,0,s.projectileRef.spin*dt);Vector2 waveVelocity=new Vector2(Mathf.Sin(s.age*s.sinSpeed.x)*s.sinusoidal.x,Mathf.Sin(s.age*s.sinSpeed.y)*s.sinusoidal.y)*.5f;
                if(s.projectileRef?.beam!=true)s.pos += (s.velocity+waveVelocity) * Mathf.Min(dt, Mathf.Max(0, s.life)); s.life -= dt;
                s.obj.transform.position = new Vector3(s.pos.x, s.pos.y, -2);
                if(s.structureSpec?.healing==true)
                {if(PetBlocksProjectile(old,s.pos,1.04f,true))s.life=0;else ResolvePlayerProjectileContact(s,old,playerHurtboxActive);}
                else if (s.hostile)
                {
                    s.obj.GetComponent<EnemyProjectileView>()?.Pose(s.age);
                    if(s.pillar&&!BossPatterns.PillarActive(s.age)){}
                    else if(PetBlocksProjectile(old,s.pos,s.pillar?BossPatterns.PillarRadius:CombatGeometry.HostileRadius))s.life=0;
                    else ResolvePlayerProjectileContact(s,old,playerHurtboxActive);
                }
                else
                {
                    Vector2 slowOffset=Quaternion.Euler(0,0,s.obj.transform.eulerAngles.z)*new Vector2(.01f,-.04f);
                    if(s.slow>0)foreach(var e in enemies)if(e.marker==null&&!s.slowed.Contains(e)&&Vector2.Distance(HurtCenter(e),s.pos+slowOffset)<=HurtRadius(e)+1.335f)
                    {e.contactSlow=SpecialWeaponRules.Slow(e.contactSlow,EnemyStatSpeed(e)-e.itemSlow,s.slow);s.slowed.Add(e);}
                    // Source enables the shape deferred after the movement
                    // callback. Resolve that flush here, after this tick moved.
                    s.hitDelay-=dt;s.delayedHitboxPending=false;
                    bool damageContactActive=s.hitDelay<=0;
                    // Area contacts are sampled at the physics tick. Simultaneous
                    // overlaps retain pool order; no swept path invents a hit.
                    while (true)
                    {
                        int nearest = -1; float first = float.PositiveInfinity;
                        for (int j = 0; j < enemies.Count; j++)
                        {
                            var e = enemies[j]; if (!damageContactActive || e.marker != null || e.hp<=0 || s.hit.Contains(e)) continue;
                            float t=ReferenceShotHitFraction(s,e,old,previousAge);
                            if (t < first) { first = t; nearest = j; }
                        }
                        if (nearest < 0) break;
                        var victim=enemies[nearest];s.hit.Add(victim);
                        bool explosiveStructure=s.structureSpec?.explosionRadius>0;
                        bool critical=false;
                        if(explosiveStructure)StructureExplosionWithDamage(EnemyOrigin(victim),s.structureSpec,s.damage,s.burnPayload);
                        else critical=DamageWithWeaponIndex(nearest,s.damage,s.crit,s.knockback,s.velocity,s.weapon,s.secondary,false,s.structure,s.weaponIndex,s.burnPayload);
                        if(critical&&s.critBounces>0){s.bounces++;s.critBounces--;}
                        if(critical&&s.critPierces>0){s.pierce++;s.critPierces--;}
                        if(s.bounces>0)
                        {
                            var target=RandomStarterTarget(victim);
                            Vector2 bounceDirection=target!=null?(EnemyOrigin(target)-s.pos).normalized:UnityEngine.Random.insideUnitCircle.normalized;
                            s.bounces--;s.velocity=bounceDirection*s.velocity.magnitude;s.life=101/Mathf.Max(.001f,s.velocity.magnitude);s.knockback=0;
                            s.obj.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(bounceDirection.y,bounceDirection.x)*Mathf.Rad2Deg);
                            s.damage=WeaponReference.DamageAfterHit(s.damage,s.weapon?.bounceLoss??.5f,Rules.Stat("bounce_damage"));break;
                        }
                        if(s.pierce--<=0){s.life=0;break;}
                        s.damage=WeaponReference.DamageAfterHit(s.damage,s.weapon?.pierceLoss??.5f,Rules.Stat("piercing_damage"));
                    }
                }
                if (s.life <= 0 || s.projectileRef?.beam!=true&&CombatGeometry.ProjectileOutside(s.pos))
                {
                    if(s.projectileRef!=null&&s.projectileRef.stopDelay>0){s.projectileStopped=true;s.life=s.projectileRef.stopDelay;s.projectileView?.StopEmission();}
                    else {Destroy(s.obj); shots.RemoveAt(i);}
                }
            }
        }
        bool Damage(int index,float damage,float crit=0,float knockback=1,Vector2 attackDirection=default,Equipment weapon=null,bool secondary=false,bool bypassArmor=false,bool structure=false)
            =>DamageWithWeaponIndex(index,damage,crit,knockback,attackDirection,weapon,secondary,bypassArmor,structure,-1);
        bool DamageWithWeaponIndex(int index,float damage,float crit,float knockback,Vector2 attackDirection,Equipment weapon,bool secondary,bool bypassArmor,bool structure,int weaponIndex,BurnPayload burnPayload=null)
            =>DamageWithCapturedVulnerability(index,damage,crit,knockback,attackDirection,weapon,secondary,bypassArmor,structure,weaponIndex,burnPayload,null);
        bool DamageWithCapturedVulnerability(int index,float damage,float crit,float knockback,Vector2 attackDirection,Equipment weapon,bool secondary,bool bypassArmor,bool structure,int weaponIndex,BurnPayload burnPayload,float? capturedVulnerability)
        {
            if(Rules.Phase!=RunPhase.Playing||index<0||index>=enemies.Count)return false;
            float sourceDamage=damage;
            if(weapon!=null)burnPayload??=Rules.CaptureBurn(weapon,structure);
            // BurningTimer passes no hitbox in the reference: zero crit chance,
            // zero knockback and no weapon/projectile life-steal callback.
            knockback=bypassArmor||resolvingWeaponExplosion?0:knockback+Rules.Stat("knockback")/10;
            var e=enemies[index];if(e.marker!=null||e.hp<=0)return false;
            if(weapon!=null&&Rules.WeaponScalesWith(weapon,"elemental_damage"))TryVulnerability(e,"elemental_damage");
            if(weapon!=null&&e.type.role!="Tree")e.itemSlow=Rules.ItemSpeedReduction(e.itemSlow,EnemyStatSpeed(e));
            if(!resolvingWeaponExplosion&&TryInterceptWeaponExplosion(e,weapon,damage,secondary,weaponIndex,burnPayload))return false;
            // Unit's hurt callback applies burning before take_damage. This
            // event flag also covers a successful burn-applying weapon hit.
            bool appliesBurn=!bypassArmor&&TryApplyWeaponBurn(e,weapon,structure,burnPayload);
            // boss.gd modifies the incoming integer before unit.gd applies crit,
            // vulnerability and armor, including hits without a weapon hitbox.
            if(e.type.role=="Boss"||e.type.role=="Elite")damage=Rules.DamageToBoss(damage);
            float criticalChance=bypassArmor||weapon==null?0:Mathf.Clamp01(Rules.CritChance+crit);
            bool critical=!bypassArmor&&(!structure||Rules.Stat("structures_can_crit")>0)&&criticalChance>0&&UnityEngine.Random.value<=criticalChance;if(critical)
            {
                damage=EnemyReferenceRules.CriticalDamage((int)damage,weapon?.critMultiplier??2);
            }
            float vulnerability=capturedVulnerability??(e.iceClock>0?e.icePercent:0);
            if(vulnerability!=0)damage=EnemyReferenceRules.SourceRound(damage*(1+vulnerability/100));
            damage=bypassArmor?(int)damage:EnemyReferenceRules.ArmoredDamage((int)damage,e.type.armor+e.type.armorGrowth*(Rules.Wave-1));
            if(critical)damage+=Rules.CriticalCurrentHealthDamage(e.hp,e.type.role=="Boss"||e.type.role=="Elite");
            if(bypassArmor||appliesBurn)damage+=Rules.BurnCurrentHealthDamage(e.hp,e.type.role=="Boss"||e.type.role=="Elite");
            if(e.type.role=="Tree"&&Rules.Stat("one_shot_trees")>0&&weapon!=null)damage=e.hp;
            if(!secondary&&SpecialWeaponRules.Execute(weapon,UnityEngine.Random.value)){damage=Mathf.Max(damage,e.hp);Burst(e.pos,18,mint);}
            if(!secondary){float currentSpeed=EnemyStatSpeed(e)-e.itemSlow;e.contactSlow=SpecialWeaponRules.Slow(e.contactSlow,currentSpeed,SpecialWeaponRules.EngineeringSlow(weapon,Rules.Stat("engineering"),currentSpeed));}
            e.hp=Mathf.Max(0,e.hp-damage);e.flash=CombatGeometry.FlashDuration;
            Vector2 away=e.pos-playerPos; if(attackDirection==Vector2.zero)attackDirection=away;
            e.knockback=CombatGeometry.Knockback(attackDirection,away,knockback,e.evolution>0?(int)(.5f+.25f*e.evolution):e.type.resistance);
            if((e.type.id=="fly"||e.type.id=="horned_fly")&&!bypassArmor&&weapon!=null&&!weapon.melee&&UnityEngine.Random.value<.25f)FireEncounter(e);
            Shake(Mathf.Min(damage/3,3)/100);ImpactBurst(e.pos,attackDirection,critical);Flash(e,1);
            FloatText(e.pos,Mathf.CeilToInt(damage)+(critical?"!":""),critical?amber:Color.white,critical);
            Burst(e.pos,critical?7:3,critical?amber:coral);WeaponReference.Impact(sound,e.pos,critical,bypassArmor);
            if(e.hp>0)
            {
                if(!bypassArmor)ApplyStarterHitEffects(e,weapon,critical,false,secondary,structure,sourceDamage,weaponIndex,burnPayload);
                return critical;
            }
            // Unit.take_damage emits the weapon kill callback before critical
            // item payouts, then resolves ordinary on-hit callbacks before die.
            if(!bypassArmor)ApplyStarterKillStatistics(weapon,weaponIndex);
            if(bypassArmor)Rules.RecordBurnKill();
            // Unit queues die before its healing/critical callbacks. Keep the
            // victim in the active list until the combat boundary, so nested
            // procs cannot invalidate the caller's integer traversal indices.
            pendingEnemyDeaths.Enqueue((e,bypassArmor,attackDirection,away,knockback));
            Rules.CriticalKillMaterials(critical);
            if(Rules.TryCriticalKillHeal(critical,UnityEngine.Random.value))FloatText(playerPos,"+1 HP",mint,false);
            if(!resolvingWeaponExplosion)ApplyStarterCriticalGold(weapon,critical);
            if(!bypassArmor)ApplyStarterHitEffects(e,weapon,critical,true,secondary,structure,sourceDamage,weaponIndex,burnPayload);
            return critical;
        }
        readonly Queue<(Enemy enemy,bool burnDamage,Vector2 direction,Vector2 away,float knockback)> pendingEnemyDeaths=
            new Queue<(Enemy,bool,Vector2,Vector2,float)>();
        void ResolveEnemyDeaths()
        {
            // A die callback can synchronously kill another enemy through
            // Cyberball. Process that newly queued death without recursion.
            while(pendingEnemyDeaths.Count>0)
            {
                var death=pendingEnemyDeaths.Dequeue();var e=death.enemy;
                if(!enemies.Contains(e))continue;
                FinalizeEnemyDeath(e,death.burnDamage,death.direction,death.away,death.knockback);
            }
        }
        void ResolveCombatCallbacks()
        {
            // Every lethal hit contributes one pending victim; its pending-HP
            // guard prevents duplicate deaths. Newly spawned children are birth
            // requests, so chained explosions/procs cannot grow this drain's
            // target set without bound.
            do
            {
                ResolveImplantDamage();ResolveEnemyDeaths();ResolveImplantAttacks();
            }
            while(pendingEnemyDeaths.Count>0||implantDamage.Count>0||implantExplosions.Count>0||implantRetaliations.Count>0);
        }
        void FinalizeEnemyDeath(Enemy e,bool bypassArmor,Vector2 attackDirection,Vector2 away,float knockback)
        {
            // Main updates its census before item callbacks, even though this
            // dying identity remains in the spawner until its later callback.
            if(e.type.role=="Tree")Rules.LivingTrees=Mathf.Max(0,Rules.LivingTrees-1);
            else Rules.LivingEnemies=Mathf.Max(0,Rules.LivingEnemies-1);
            if(e.type.role=="Boss")
            {
                if(enemies.FindAll(x=>x.type.role=="Boss"&&x.hp>0).Count==0)Rules.DefeatBoss();
                if(e.orbitProjectiles!=null)foreach(var orb in e.orbitProjectiles)Destroy(orb);
            }
            Flash(e,0);if(e.telegraph!=null)e.telegraph.enabled=false;e.deathAge=0;
            e.knockback=bypassArmor?Vector2.zero:CombatGeometry.Knockback(attackDirection,away,Mathf.Max(knockback,1.5f),e.type.resistance);
            corpses.Add(e);
            // Source Main callbacks can observe the zero-HP dying member before
            // the spawner removal callback. Keep membership through those
            // callbacks; the headless source probe validates this boundary.
            if(Rules.Phase==RunPhase.Playing)
            {
                if(e.type.role!="Tree")
                {
                    Rules.TriggerDeathDamage();EnemyImplantDeath(e);
                    Rules.ApplyBurningDeathStats(e.burnTicks>0||bypassArmor);
                }
                else DeployTreeTurrets(e.pos);
                DropEnemyLoot(e);SpawnEnemyChildren(e);
                Rules.RecordEnemy(e.type.id,e.type.role=="Tree",e.activeAge,e.type.role=="Boss"||e.type.role=="Elite");Rules.Kill();
            }
            enemies.Remove(e);
        }
        void DropEnemyLoot(Enemy enemy)
        {
            var type=enemy.type;
            var position=enemy.pos+CombatGeometry.OriginOffset(type.id);
            // main.spawn_loot resolves consumables independently, before the
            // wave/horde gate for the material batch.
            var consumable=ReferenceLoot.Roll(type.id,Rules.Stat("luck"),cratesSpawned,UnityEngine.Random.value,UnityEngine.Random.value,Rules.Stat("crate_chance"),type.role,Rules.Wave);
            if(consumable==ConsumableKind.None&&ReferenceLoot.CanDropConsumables(type.id)&&UnityEngine.Random.value<Rules.Stat("enemy_fruit_drops")/100)consumable=ConsumableKind.Fruit;
            DropConsumable(position,consumable,ReferenceLoot.BaseGoldSpread(type.id));
            float chance=ReferenceLoot.MaterialBatchChance(type.id,Rules.Wave,Rules.IsHordeWave,type.role);
            if(UnityEngine.Random.value>chance)return;
            int baseValue=type.id=="evil_mob"?enemy.gold*(enemy.evolution+1):type.value;
            float unrounded=Rules.MaterialDropAmount(baseValue,type.role=="Tree");
            int count=Rules.MaterialDropValue(baseValue,type.role=="Tree",UnityEngine.Random.value);
            DropWithSpread(position,count,false,ReferenceLoot.MaterialSpread(type.id,unrounded));
        }
        void Drop(Vector2 p,int value,bool fruit)=>DropWithSpread(p,value,fruit,0);
        void DropWithSpread(Vector2 p,int value,bool fruit,float spread)
        {
            if(fruit){DropConsumable(p,ConsumableKind.Fruit);return;}
            var materials=pickups.FindAll(pickup=>!pickup.fruit);
            // Source main.gd:spawn_gold caps material objects at 50 and adds each
            // excess unit to a random existing pile, leaving it on the ground.
            for(int i=0;i<value;i++)
            {
                if(materials.Count>=50)
                {
                    var pile=materials[UnityEngine.Random.Range(0,materials.Count)];pile.value++;
                    pile.motion.collisionScale=Mathf.Min(2,pile.motion.collisionScale+.05f);
                    float baseScale=ActorPresentation.SpriteLengthScale(pile.obj.GetComponent<SpriteRenderer>().sprite,.32f);
                    pile.obj.transform.localScale=Vector3.one*Mathf.Min(baseScale*2,pile.obj.transform.localScale.x+baseScale*.05f);
                    continue;
                }
                int amount=Rules.PrepareMaterialDrop(1);
                int shape = UnityEngine.Random.Range(0, catalog.scrapShapes.Length);
                var obj = SpriteObject("Scrap shape " + shape,catalog.scrapShapes[shape], p, 3);
                obj.transform.localScale = Vector3.one * ActorPresentation.SpriteLengthScale(obj.GetComponent<SpriteRenderer>().sprite, UnityEngine.Random.Range(.28f, .36f))*(amount>1?1.25f:1);
                var motion=ScrapMotion.CreateDrop(p,PickupPhysicsRate,spread);
                motion.magnetized=UnityEngine.Random.value<Rules.Stat("instant_gold_attracting")/100;motion.verticalSpeed=UnityEngine.Random.Range(2.5f,4f);motion.height=.06f;motion.angle=UnityEngine.Random.Range(0,360);motion.spin=UnityEngine.Random.Range(-260,260);
                motion.collisionScale=amount>1?1.25f:1;motion.collisionAngle=motion.angle;
                var pickup=new Pickup { obj = obj, value = amount,motion=motion };
                RouteInstantMaterialCollector(pickup);
                pickups.Add(pickup);materials.Add(pickup);
            }
            // main.spawn_gold emits even when all value merges into existing
            // piles (or the batch is empty); fruit creation returns above.
            InvalidateLootwormTargets();
        }
        void UpdateCorpses(float dt)
        {
            for(int i=corpses.Count-1;i>=0;i--)
            {
                var e=corpses[i];e.deathAge+=dt;float t=Mathf.Clamp01(e.deathAge/.5f);e.pos+=CombatGeometry.StepKnockback(ref e.knockback,dt);e.obj.transform.position=e.pos;e.shadow.Place(e.pos,ActorPresentation.EnemyScale(e.type)*(1+e.evolution*.2f)*(1-t),.392157f*(1-t));e.obj.transform.localScale=Vector3.one*ActorPresentation.EnemyScale(e.type)*(1+e.evolution*.2f)*Mathf.Max(.00001f,1-t);e.obj.transform.rotation=Quaternion.Euler(0,0,reducedMotion?0:t*360);e.obj.GetComponent<SpriteRenderer>().color=Color.Lerp(Color.white,new Color(.349f,.349f,.349f),t);if(e.deathAge>=1)
                {
                    Destroy(e.obj);corpses.RemoveAt(i);
                }
            }
        }
        void ClearReferenceCombat()
        {
            implantDamage.Clear();implantExplosions.Clear();implantRetaliations.Clear();pendingEnemyDeaths.Clear();alienClock=0;
            ClearStarterEffects();encounterStates.Clear();extraAttackClocks.Clear();births.Clear();foreach(var slash in slashes)Destroy(slash.obj);slashes.Clear();foreach(var e in corpses)Destroy(e.obj);corpses.Clear();
        }
    }
}

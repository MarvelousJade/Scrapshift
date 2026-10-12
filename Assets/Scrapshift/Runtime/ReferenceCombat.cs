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
        readonly GameObject[] equipmentViews=new GameObject[6];
        readonly SpriteRenderer[] muzzleViews = new SpriteRenderer[6];
        readonly int[] weaponShots=new int[6];
        readonly float[] weaponClocks=new float[6],swings=new float[6];
        readonly Enemy[] weaponTargets=new Enemy[6];
        readonly Vector2[] weaponDirections=new Vector2[6];
        SpawnDirector director;
        System.Random placementRandom = new System.Random();
        Vector2 groupAnchor;
        int chargeReleases, rangedReleases, spawnRelocations, bossMutations;
        void BeginSpawnGroup(SpawnGroup group)
        {
            var active = enemies.FindAll(e=>e.marker==null && e.type.role!="Boss" && e.type.role!="Tree");
            int cap = catalog.data.waves[Rules.Wave-1].cap;
            while(active.Count>cap)
            {
                var priority = active.FindAll(e=>e.type.id=="baby_alien" || e.type.id=="chaser" || e.type.id=="helmet_alien" || e.type.id=="fin_alien");
                var pool = priority.Count>0 ? priority : active;
                var victim = pool[placementRandom.Next(pool.Count)];
                Destroy(victim.obj); active.Remove(victim); enemies.Remove(victim);
            }
            groupAnchor = SpawnPlacement.Anchor(group, playerPos, placementRandom);
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
            director=new SpawnDirector(catalog.data.waves[Rules.Wave-1],UnityEngine.Random.Range(1,int.MaxValue),Rules.Stat("number_of_enemies"),Rules.Stat("trees"));
            births.Clear();cratesSpawned=0;
            Array.Clear(weaponClocks,0,6);
            SyncEquipment();ResetStructures();
            if(Rules.Wave==20)births.Push(new Birth
            {
                type=catalog.Enemy("predator"),pos=RandomPosition(playerPos,4)
            }, BirthLane.Boss)
            ;
        }
        void AdvanceWave()
        {
            if(Rules.Phase!=RunPhase.Shop)return;ClearCombat();Rules.NextWave();gameCamera.orthographicSize=ArenaGeometry.CameraHalfHeight;playerPos=Vector2.zero;StartWave();sound.Play(SoundCue.Install);
        }
        void SyncEquipment()
        {
            for(int i=0;i<6;i++)
            {
                if (equipmentViews[i] != null) Destroy(equipmentViews[i]);
                muzzleViews[i] = null;
                equipmentViews[i] = i < Rules.Inventory.Count ? SpriteObject(Rules.Inventory[i].title, Rules.Inventory[i].image, playerPos, 12) : null;
                swings[i] = 0;weaponShots[i]=0;weaponTargets[i]=null;weaponDirections[i]=Vector2.right;
                if (equipmentViews[i] == null) continue;
                equipmentViews[i].transform.localScale = Vector3.one * ActorPresentation.WeaponScale(Rules.Inventory[i]);
                if (Rules.Inventory[i].melee) continue;
                var flash = SpriteObject("Muzzle flash", catalog.muzzleFrames[0], Vector2.zero, 14);
                flash.transform.SetParent(equipmentViews[i].transform, false);
                flash.transform.localPosition = new Vector3(Rules.Inventory[i].image.bounds.max.x + .14f, 0, -.1f);
                flash.transform.localScale = Vector3.one * .48f;
                muzzleViews[i] = flash.GetComponent<SpriteRenderer>();
                muzzleViews[i].enabled = false;
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
                births.Push(new Birth { type = def, pos = SpawnPlacement.Position(group, unit, groupAnchor, playerPos, placementRandom) }, def.role=="Tree"?BirthLane.Tree:BirthLane.Regular);
        }
        void UpdateBirthQueue(float dt) => births.Tick(dt, b=>Spawn(b.type,b.pos));
        EnemyAttack MakeAttack(EnemyBehavior behavior) => behavior.charge || behavior.ranged ?
            new EnemyAttack(behavior,()=>UnityEngine.Random.Range(Mathf.Max(1f/60,behavior.cooldown-behavior.jitter),behavior.cooldown+behavior.jitter)) : null;
        void Spawn(EnemyEntry def,Vector2 pos)
        {
            if(campaignCheck)campaignEnemyIds.Add(def.id);
            var behavior = EnemyBehavior.For(def.id);
            var attack = MakeAttack(behavior);
            var obj=SpriteObject(def.id,def.image,pos);var shadow=AddGroundShadow(obj, def.image.bounds.size.x * .9f, -def.image.bounds.extents.y + .05f);obj.SetActive(false);var marker=SpriteObject("Spawn warning",catalog.warning,pos,2);if(def.role=="Tree")
            {
                marker.GetComponent<SpriteRenderer>().enabled=false;
                foreach(float sign in new[]{-1f,1f})
                {var stroke=MakeLine("Green tree warning",marker.transform,RunFeedback.SpawnColor(true,1),.12f);stroke.positionCount=2;stroke.SetPosition(0,new Vector3(-.4f,-.4f*sign,0));stroke.SetPosition(1,new Vector3(.4f,.4f*sign,0));}
            }
            marker.transform.localScale=Vector3.one*.6f;marker.transform.rotation=Quaternion.Euler(0,0,UnityEngine.Random.Range(-25,25));float hp=(def.health+def.growth*(Rules.Wave-1))*Mathf.Max(.01f,1+Rules.Stat("enemy_health")/100);enemies.Add(new Enemy
            {
                obj=obj,shadow=shadow,marker=marker,type=def,hp=hp,maxHp=hp,pos=pos,speed=Mathf.Max(0,def.speed+UnityEngine.Random.Range(-def.speedRandom,def.speedRandom)),cooldown=def.role=="Buffer"?.25f:1,renderers=obj.GetComponents<Renderer>(),wander=new Vector2(UnityEngine.Random.Range(-behavior.offset,behavior.offset),UnityEngine.Random.Range(-behavior.offset,behavior.offset)),attack=attack,behavior=behavior,spacing=behavior.spacing+UnityEngine.Random.Range(-1f,1f),orbitSign=UnityEngine.Random.value<.5f?-1:1
            })
            ;peakEnemies=Mathf.Max(peakEnemies,enemies.Count);
        }
        void UpdateEnemies(float dt)
        {
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                var e = enemies[i]; e.age += dt; e.flash = Mathf.Max(0, e.flash - dt);
                if (e.marker != null)
                {
                    var mr = e.marker.GetComponent<SpriteRenderer>();
                    mr.color = RunFeedback.SpawnColor(e.type.role=="Tree",Mathf.Repeat(e.age * (2 + e.age * 8), 1) < .3f ? .35f : 1);
                    e.marker.transform.localScale = Vector3.one * (.52f + .12f * e.age);
                    if (e.age < 1) continue;
                    if (e.type.role!="Boss" && e.type.role!="Tree" && RunRules.Touches(ContactCenter(e), PlayerHurtCenter, ContactRadius(e) + CombatGeometry.PlayerHurtRadius))
                    {
                        e.pos = RandomPosition(playerPos, 0); e.marker.transform.position = e.pos; e.age = 0;
                        spawnRelocations++; continue;
                    }
                    Destroy(e.marker); e.marker = null; e.obj.SetActive(true); e.activeAge = 0;
                }
                e.activeAge += dt;
                Vector2 toward = (playerPos - e.pos).normalized;
                float distance = Vector2.Distance(e.pos, playerPos), speed = e.speed;
                e.cooldown -= dt; e.action += dt;
                string role = e.type.role;
                if (role=="Boss" && !e.secondPhase && Mathf.Floor(e.activeAge)>e.phaseCheck)
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
                        toward=(playerPos+e.wander-e.pos).normalized; break;
                    case EnemyMovement.Destination: case EnemyMovement.AroundPlayer: case EnemyMovement.Materials:
                        if(!e.hasDestination || Vector2.Distance(e.pos,e.destination)<.1f)
                        {
                            e.hasDestination=true;
                            e.destination=e.behavior.movement==EnemyMovement.AroundPlayer ? RunRules.ClampPosition(playerPos+new Vector2(UnityEngine.Random.Range(-3f,3f),UnityEngine.Random.Range(-3f,3f))) : RandomPosition(playerPos,0);
                            if(e.behavior.movement==EnemyMovement.Materials)
                            {
                                var materials=pickups.FindAll(p=>!p.fruit);
                                if(materials.Count>0) e.destination=materials[placementRandom.Next(materials.Count)].motion.position;
                            }
                        }
                        toward=(e.destination-e.pos).normalized;
                        speed=Mathf.Min(speed,Vector2.Distance(e.destination,e.pos)/dt); break;
                    case EnemyMovement.Pursuer: speed+=EnemyBehavior.PursuerBonus(e.action); break;
                }
                if(e.type.id=="evil_mob") { speed=Mathf.Max(0,speed-e.evolution*1.25f); EatMaterials(e,dt); }
                if (role == "Tree") speed = 0;
                if (e.attack != null)
                {
                    var previous = e.attack.Phase;
                    bool released = e.attack.Step(dt, playerPos - e.pos, distance >= e.behavior.minRange && distance <= e.behavior.maxRange);
                    if (previous != AttackPhase.Windup && e.attack.Phase == AttackPhase.Windup && charge)
                        sound.Play(SoundCue.Charge, e.pos);
                    if (charge && e.attack.Phase == AttackPhase.Windup) speed = 0;
                    else if (charge && e.attack.Phase == AttackPhase.Active)
                    { toward = e.attack.Direction; speed = e.speed + e.behavior.chargeSpeed; }
                    if (released)
                    {
                        if (charge) chargeReleases++;
                        else
                        {
                            rangedReleases++;
                            if(e.type.id=="slasher") EnemySlash(e);
                            else for(int p=0;p<(role=="Boss"?16:1);p++)
                            {
                                Vector2 direction = role=="Boss" ? UnityEngine.Random.insideUnitCircle.normalized :
                                    (Vector2)(Quaternion.Euler(0,0,UnityEngine.Random.Range(-e.behavior.spread,e.behavior.spread)*Mathf.Rad2Deg)*e.attack.Direction);
                                EnemyBullet(e,direction);
                            }
                        }
                    }
                }
                speed = Mathf.Max(0, speed*Mathf.Max(.01f,1+Rules.Stat("enemy_speed")/100) - e.contactSlow-e.itemSlow);
                e.contactSlow = Mathf.Max(0, e.contactSlow - 3 * dt);
                if (role == "Buffer" && e.cooldown <= 0)
                {
                    e.cooldown = 4;
                    var eligible = enemies.FindAll(x => x != e && x.marker == null && !x.boosted && x.behavior.canBoost && x.type.role != "Tree" && Vector2.Distance(x.pos, e.pos) < 3.0904f);
                    var target = eligible.Count>0 ? eligible[placementRandom.Next(eligible.Count)] : null;
                    if (target != null)
                    {
                        target.boosted = true; target.hp *= 2.5f; target.maxHp *= 2.5f; target.speed *= 1.5f;
                        e.supportFlash = .45f; target.supportFlash = .45f;
                        FloatText(target.pos, "BOOST", new Color(.8f, .4f, 1), false);
                    }
                }
                Vector2 separation = Vector2.zero;
                foreach (var other in enemies)
                {
                    if (other == e || other.marker != null) continue;
                    var delta = e.pos - other.pos; float d = delta.magnitude, gap = Radius(e) + Radius(other);
                    if (d > .001f && d < gap) separation += delta / d * (gap - d) * 5;
                }
                bool winding = charge && e.attack != null && e.attack.Phase == AttackPhase.Windup;
                if (winding) separation = Vector2.zero;
                if (charge && e.attack.Phase == AttackPhase.Active) separation *= .15f;
                Vector2 push = CombatGeometry.StepKnockback(ref e.knockback, dt);
                e.pos = RunRules.ClampPosition(e.pos + (toward * speed + separation) * dt + push);
                UpdateEnemyPresentation(e, toward * speed, dt);
                if(role=="Boss") UpdateBossArm(e,dt);
                if (role != "Tree" && invulnerable <= 0 && CombatGeometry.ContactEnabled(e.knockback, speed) && RunRules.Touches(ContactCenter(e), PlayerHurtCenter, ContactRadius(e) + CombatGeometry.PlayerHurtRadius))
                {
                    HurtPlayer((e.type.damage + e.type.damageGrowth * (Rules.Wave - 1)) * (e.boosted ? 1.25f : 1),e.type);
                    e.contactSlow = CombatGeometry.ContactSlow(e.contactSlow, e.speed);
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
            if (e.attack != null && e.behavior.charge)
            {
                if (e.telegraph == null) e.telegraph = MakeLine("Attack direction", e.obj.transform, new Color(1, .55f, .22f, .65f), .055f);
                e.telegraph.enabled = winding;
                if (winding)
                {
                    // World coordinates keep the cue stable while the body squashes or tilts.
                    e.telegraph.useWorldSpace = true; e.telegraph.positionCount = 2;
                    Vector2 end = RunRules.ClampPosition(e.pos + e.attack.Direction * (e.behavior.charge ? 3 : .95f));
                    e.telegraph.SetPosition(0, new Vector3(e.pos.x, e.pos.y, -.8f));
                    e.telegraph.SetPosition(1, new Vector3(end.x, end.y, -.8f));
                }
            }
        }
        void HurtPlayer(float amount,EnemyEntry attacker=null)
        {
            if(invulnerable>0)return;float before=Rules.Health;Rules.Hurt(amount);float actual=before-Rules.Health;if(actual<=0)
            {
                FloatText(playerPos,Rules.LastHitProtected?"BLOCK":"DODGE",mint,false);invulnerable=CombatGeometry.Invulnerability(0,Rules.MaxHealth);return;
            }
            lastAttacker=attacker;FloatText(playerPos,$"-{Mathf.CeilToInt(actual)}",coral,true);playerFlash=CombatGeometry.FlashDuration;invulnerable=CombatGeometry.Invulnerability(actual,Rules.MaxHealth);Shake(.05f);ImpactBurst(playerPos,UnityEngine.Random.insideUnitCircle,false,true);sound.Play(SoundCue.Hurt);PlayerHitParticles();
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
            shots.Add(new Shot { obj = obj, pos = e.pos, velocity = direction.normalized * e.behavior.projectileSpeed, life = 30,
                damage = ProjectileDamage(e), hostile = true, attacker=e.type });
        }
        void UpdateEquipment(float dt)
        {
            for(int i=0;i<Rules.Inventory.Count;i++)
            {
                var w=Rules.Inventory[i];WeaponMotion.Step(w,dt,ref swings[i],ref weaponClocks[i]);float range=Rules.WeaponRange(w);Vector2 mount=playerPos+ActorPresentation.WeaponMount(Rules.Inventory.Count,i);Enemy nearest=null;float distance=RunFeedback.DetectionRange(range)*RunFeedback.DetectionRange(range);foreach(var e in enemies)
                {
                    if(e.marker!=null)continue;float d=(e.pos-mount).sqrMagnitude;if(d<distance)
                    {
                        distance=d;nearest=e;
                    }
                }
                bool shooting=swings[i]>0;
                var aim=shooting?weaponTargets[i]:nearest;
                Vector2 direction=aim!=null&&enemies.Contains(aim)?(aim.pos-mount).normalized:shooting?weaponDirections[i]:ActorPresentation.RestDirection(playerAnimation.FacingLeft);
                weaponDirections[i]=direction;
                bool fire=!shooting&&nearest!=null&&RunFeedback.CanAttack(range,distance)&&weaponClocks[i]<=0&&Rules.Phase==RunPhase.Playing&&!Arriving;
                if(fire){weaponShots[i]++;weaponClocks[i]=Rules.WeaponCooldown(w)*(w.reloadEvery>0&&weaponShots[i]%w.reloadEvery==0?w.reloadMultiplier:1);swings[i]=Rules.WeaponAnimationDuration(w);weaponTargets[i]=nearest;}
                var view=equipmentViews[i];if(view!=null)
                {
                    float age = Rules.WeaponAnimationDuration(w) - swings[i];
                    float kick = WeaponMotion.Recoil(w,age,Rules.WeaponFireRate(w));
                    float offset = ActorPresentation.WeaponPivot(w.id) + (w.melee ? swings[i] * 2 : -kick * (reducedMotion ? .2f : 1));
                    view.transform.position = new Vector3(mount.x + direction.x * offset, mount.y + direction.y * offset, -2);
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    view.transform.rotation = Quaternion.Euler(0, 0, angle);
                    view.GetComponent<SpriteRenderer>().flipY = direction.x < 0;
                    view.GetComponent<SpriteRenderer>().sortingOrder=ActorPresentation.WeaponOrder(Rules.Inventory.Count,i);
                    if (muzzleViews[i] != null)
                    {
                        muzzleViews[i].enabled = age < .066f;
                        muzzleViews[i].sprite = catalog.muzzleFrames[Mathf.Clamp((int)(age / .022f), 0, 2)];
                    }
                }
                if(!fire)continue;float damage=Rules.WeaponDamage(w);
                if(w.melee)
                {
                    Vector2 end=playerPos+direction*range;foreach(var target in enemies.ToArray())if(enemies.Contains(target)&&target.marker==null&&SegmentDistance(HurtCenter(target),playerPos,end)<HurtRadius(target)+.45f)
                    {
                        Damage(enemies.IndexOf(target),damage,w.crit,w.knockback,direction,w);orbitHits++;
                    }
                    Burst(end,4,paper);sound.Play(SoundCue.Swing,playerPos,w.sounds);
                }
                else
                {
                    Vector2 muzzle = view != null ? (Vector2)view.transform.position + direction * (w.image.bounds.max.x * ActorPresentation.WeaponScale(w)) : mount;
                    if (muzzleViews[i] != null) { muzzleViews[i].enabled = true; muzzleViews[i].sprite = catalog.muzzleFrames[0]; }
                    for(int p=0;p<w.pellets;p++)
                    {
                        Vector2 dir=Quaternion.Euler(0,0,(p-(w.pellets-1)/2f)*Mathf.Max(5,w.spread*Mathf.Rad2Deg)) * direction;var obj=SpriteObject("Rivet tracer",catalog.muzzleFrames[2],muzzle,13);obj.transform.localScale=Vector3.one*.45f;obj.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(dir.y,dir.x)*Mathf.Rad2Deg);shots.Add(new Shot
                        {
                            obj=obj,pos=muzzle,velocity=dir*w.projectileSpeed,life=(range+1)/w.projectileSpeed,damage=damage,crit=w.crit,pierce=w.piercing+(int)Rules.Stat("piercing"),knockback=w.knockback,weapon=w,bounces=w.bounce+(int)Rules.Stat("bounce")
                        })
                        ;shotsFired++;
                    }
                    sound.Play(SoundCue.Fire,playerPos,w.sounds);
                }
            }
        }
        void UpdateWeapons(float dt)
        {
            for (int i = shots.Count - 1; i >= 0; i--)
            {
                var s = shots[i]; Vector2 old = s.pos;
                s.pos += s.velocity * Mathf.Min(dt, Mathf.Max(0, s.life)); s.life -= dt;
                s.obj.transform.position = new Vector3(s.pos.x, s.pos.y, -2);
                if (s.hostile)
                {
                    if (SegmentDistance(PlayerHurtCenter, old, s.pos) <= CombatGeometry.PlayerHurtRadius + CombatGeometry.HostileRadius)
                    { HurtPlayer(s.damage,s.attacker); s.life = 0; }
                }
                else
                {
                    if(s.slow>0)foreach(var e in enemies)if(e.marker==null&&!s.slowed.Contains(e)&&SegmentDistance(HurtCenter(e),old,s.pos)<=HurtRadius(e)+1.335f)
                    {e.contactSlow=SpecialWeaponRules.Slow(e.contactSlow,e.speed,s.slow);s.slowed.Add(e);}
                    s.hitDelay-=dt;
                    // Resolve in travel order, not enemy-list/spawn order. Piercing loses damage after the nearest hit.
                    while (true)
                    {
                        int nearest = -1; float first = float.PositiveInfinity;
                        for (int j = 0; j < enemies.Count; j++)
                        {
                            var e = enemies[j]; if (s.hitDelay>0 || e.marker != null || s.hit.Contains(e)) continue;
                            float t = CombatGeometry.SweepFraction(HurtCenter(e), HurtRadius(e) + CombatGeometry.BulletRadius, old, s.pos);
                            if (t < first) { first = t; nearest = j; }
                        }
                        if (nearest < 0) break;
                        var victim=enemies[nearest];s.hit.Add(victim);
                        bool critical=Damage(nearest,s.damage,s.crit,s.knockback,s.velocity,s.weapon,s.secondary,false,s.structure);
                        int bounceBonus=critical?(int)(s.weapon?.Trait("bounce_on_crit")?.value??0):0;
                        if(s.bounces+bounceBonus>0)
                        {
                            var target=NearestStarterTarget(victim.pos,s.hit);
                            if(target!=null){s.bounces=Mathf.Max(0,s.bounces-1);s.pos=victim.pos;s.velocity=(target.pos-s.pos).normalized*s.velocity.magnitude;s.life=2;s.damage*=1-(s.weapon?.bounceLoss??.5f);break;}
                        }
                        int pierceBonus=critical?(int)(s.weapon?.Trait("pierce_on_crit")?.value??0):0;
                        if(s.pierce--+pierceBonus<=0){s.life=0;break;}
                        s.damage*=Mathf.Clamp01(1-(s.weapon?.pierceLoss??.5f)+Rules.Stat("piercing_damage")/100);
                    }
                }
                if (s.life <= 0 || CombatGeometry.ProjectileOutside(s.pos))
                { Destroy(s.obj); shots.RemoveAt(i); }
            }
        }
        bool Damage(int index,float damage,float crit=0,float knockback=1,Vector2 attackDirection=default,Equipment weapon=null,bool secondary=false,bool bypassArmor=false,bool structure=false)
        {
            if(index<0||index>=enemies.Count)return false;
            knockback+=Rules.Stat("knockback")/10;
            var e=enemies[index];if(e.marker!=null)return false;bool critical=(!structure||Rules.Stat("structures_can_crit")>0)&&UnityEngine.Random.value<Mathf.Clamp01(Rules.CritChance+crit);if(critical)
            {
                damage*=weapon?.critMultiplier??2;
            }
            damage=Mathf.Max(1,bypassArmor?damage:damage/(1+Mathf.Max(0,e.type.armor+e.type.armorGrowth*(Rules.Wave-1))/15));
            if(e.type.role=="Boss")damage=Rules.DamageToBoss(damage);
            if(critical)damage+=Rules.CriticalCurrentHealthDamage(e.hp,e.type.role=="Boss");
            if(weapon!=null&&e.type.role!="Tree")e.itemSlow=Rules.ItemSpeedReduction(e.itemSlow,e.speed);
            if(e.type.role=="Tree"&&Rules.Stat("one_shot_trees")>0&&weapon!=null)damage=e.hp;
            if(!secondary&&SpecialWeaponRules.Execute(weapon,UnityEngine.Random.value)){damage=Mathf.Max(damage,e.hp);Burst(e.pos,18,mint);}
            if(!secondary)e.contactSlow=SpecialWeaponRules.Slow(e.contactSlow,e.speed,SpecialWeaponRules.EngineeringSlow(weapon,Rules.Stat("engineering"),e.speed));
            e.hp-=damage;e.flash=CombatGeometry.FlashDuration;
            Vector2 away=e.pos-playerPos; if(attackDirection==Vector2.zero)attackDirection=away;
            e.knockback=CombatGeometry.Knockback(attackDirection,away,knockback,e.evolution>0?(int)(.5f+.25f*e.evolution):e.type.resistance);
            Shake(Mathf.Min(damage/3,3)/100);ImpactBurst(e.pos,attackDirection,critical);Flash(e,1);FloatText(e.pos,Mathf.CeilToInt(damage)+(critical?"!":""),critical?amber:Color.white,critical);Burst(e.pos,critical?7:3,critical?amber:coral);sound.Play(critical?SoundCue.Critical:e.hp<=0?SoundCue.Kill:SoundCue.Impact,e.pos);if(Rules.TryLifeSteal(UnityEngine.Random.value,weapon?.lifesteal??0))FloatText(playerPos,"+1 HP",mint,false);if(e.hp>0){ApplyStarterHit(e,weapon,critical,false,secondary);return critical;}Rules.Kill();Rules.CriticalKillMaterials(critical);if(Rules.TryCriticalKillHeal(critical,UnityEngine.Random.value))FloatText(playerPos,"+1 HP",mint,false);if(e.type.role=="Boss"){ Rules.DefeatBoss(); if(e.orbitProjectiles!=null)foreach(var orb in e.orbitProjectiles)Destroy(orb); }Flash(e,0);if(e.telegraph!=null)e.telegraph.enabled=false;e.deathAge=0;e.knockback=CombatGeometry.Knockback(attackDirection,away,Mathf.Max(knockback,1.5f),e.type.resistance);corpses.Add(e);enemies.Remove(e);Drop(e.pos,Rules.MaterialDropValue(e.type.id=="evil_mob"?e.gold*(e.evolution+1):e.type.value,e.type.role=="Tree",UnityEngine.Random.value),false);var consumable=ReferenceLoot.Roll(e.type.id,Rules.Stat("luck"),cratesSpawned,UnityEngine.Random.value,UnityEngine.Random.value);if(consumable==ConsumableKind.None&&UnityEngine.Random.value<Rules.Stat("enemy_fruit_drops")/100)consumable=ConsumableKind.Fruit;DropConsumable(e.pos,consumable);if(e.type.role=="Spawner")for(int i=0;i<3;i++)births.Push(new Birth
            {
                type=catalog.Enemy("junkie"),pos=RunRules.ClampPosition(e.pos+new Vector2(UnityEngine.Random.Range(-1f,1f),UnityEngine.Random.Range(-1f,1f)))
            }, BirthLane.Summon)
            ;ApplyStarterHit(e,weapon,critical,true,secondary);return critical;
        }
        void Drop(Vector2 p,int value,bool fruit)
        {
            if(fruit){DropConsumable(p,ConsumableKind.Fruit);return;}
            int[] values = ScrapMotion.SplitValue(value, 240 - pickups.FindAll(pickup=>!pickup.fruit).Count);
            if (values.Length == 0)
            {
                if (!fruit && value > 0) Rules.BankMaterials(value);
                return;
            }
            foreach (int amount in values)
            {
                int shape = UnityEngine.Random.Range(0, catalog.scrapShapes.Length);
                var obj = SpriteObject(fruit ? "Repair capsule" : "Scrap shape " + shape,
                    fruit ? catalog.repairPickup : catalog.scrapShapes[shape], p, 3);
                obj.transform.localScale = Vector3.one * ActorPresentation.SpriteLengthScale(obj.GetComponent<SpriteRenderer>().sprite, UnityEngine.Random.Range(.28f, .36f));
                pickups.Add(new Pickup { obj = obj, value = amount, fruit = fruit,
                    motion = new ScrapMotion { magnetized=UnityEngine.Random.value<Rules.Stat("instant_gold_attracting")/100,position = p, velocity = UnityEngine.Random.insideUnitCircle * 3.2f,
                        verticalSpeed = UnityEngine.Random.Range(2.5f, 4f), height = .06f,
                        angle = UnityEngine.Random.Range(0, 360), spin = UnityEngine.Random.Range(-260, 260) } });
            }
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
            ClearStarterEffects();births.Clear();foreach(var slash in slashes)Destroy(slash.obj);slashes.Clear();foreach(var e in corpses)Destroy(e.obj);corpses.Clear();
        }
    }
}

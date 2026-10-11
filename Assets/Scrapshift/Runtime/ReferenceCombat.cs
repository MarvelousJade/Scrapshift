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
        readonly Queue<Birth> births=new Queue<Birth>();
        readonly List<Enemy> corpses=new List<Enemy>();
        readonly GameObject[] equipmentViews=new GameObject[6];
        readonly SpriteRenderer[] muzzleViews = new SpriteRenderer[6];
        readonly float[] weaponClocks=new float[6],swings=new float[6];
        SpawnDirector director;
        float birthClock;
        System.Random placementRandom = new System.Random();
        Vector2 groupAnchor;
        int chargeReleases, rangedReleases, spawnRelocations;
        float RadiusScale(EnemyEntry e) => e.role == "Boss" ? 1.65f : e.id.Contains("bruiser") || e.role == "Spawner" ? 1.2f : e.id == "baby_alien" ? .72f : 1;
        void BeginSpawnGroup(SpawnGroup group) => groupAnchor = SpawnPlacement.Anchor(group, playerPos, placementRandom);
        float Radius(Enemy e) => e.type.role == "Tree" ? .45f : .3f * RadiusScale(e.type);
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
        void AddGroundShadow(GameObject actor, float width, float y)
        {
            var shadow = SpriteObject("Ground shadow", catalog.shadow, Vector2.zero, 1);
            shadow.transform.SetParent(actor.transform, false);
            shadow.transform.localPosition = new Vector3(0, y, .1f);
            shadow.transform.localScale = new Vector3(width, .18f, 1);
            shadow.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, .2f);
        }
        void StartWave()
        {
            director=new SpawnDirector(catalog.data.waves[Rules.Wave-1],UnityEngine.Random.Range(1,int.MaxValue));
            birthClock=0;
            Array.Clear(weaponClocks,0,6);
            SyncEquipment();
            if(Rules.Wave==20)births.Enqueue(new Birth
            {
                type=catalog.Enemy("predator"),pos=RandomPosition(playerPos,4)
            })
            ;
        }
        void AdvanceWave()
        {
            if(Rules.Phase!=RunPhase.Shop)return;Rules.NextWave();playerPos=Vector2.zero;StartWave();sound.Play(SoundCue.Install);
        }
        void SyncEquipment()
        {
            for(int i=0;i<6;i++)
            {
                if (equipmentViews[i] != null) Destroy(equipmentViews[i]);
                muzzleViews[i] = null;
                equipmentViews[i] = i < Rules.Inventory.Count ? SpriteObject(Rules.Inventory[i].title, Rules.Inventory[i].image, playerPos, 12) : null;
                swings[i] = 0;
                if (equipmentViews[i] == null) continue;
                equipmentViews[i].transform.localScale = Vector3.one * .8f;
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
                p=new Vector2(UnityEngine.Random.Range(-10.2f,10.2f),UnityEngine.Random.Range(-4.9f,4.9f));if(Vector2.Distance(p,away)>=Mathf.Max(.25f,distance-i*.03f))break;
            }
            return p;
        }
        void QueueGroup(SpawnGroup group,UnitEntry unit,int count)
        {
            var def = catalog.Enemy(unit.id);
            if (def == null) return;
            for (int i = 0; i < count; i++)
                births.Enqueue(new Birth { type = def, pos = SpawnPlacement.Position(group, unit, groupAnchor, playerPos, placementRandom) });
        }
        void UpdateBirthQueue(float dt)
        {
            birthClock-=dt;while(birthClock<=0&&births.Count>0)
            {
                birthClock += births.Count > 110 ? .025f : .05f;var b=births.Dequeue();if(enemies.Count>=catalog.data.waves[Rules.Wave-1].cap)
                {
                    int remove=enemies.FindIndex(e=>e.type.role!="Boss"&&e.type.role!="Looter"&&e.type.role!="Tree");if(remove<0)continue;Destroy(enemies[remove].obj);Destroy(enemies[remove].marker);enemies.RemoveAt(remove);
                }
                Spawn(b.type,b.pos);
            }
            if(births.Count==0)birthClock=0;
        }
        void Spawn(EnemyEntry def,Vector2 pos)
        {
            if(campaignCheck)campaignEnemyIds.Add(def.id);
            bool charge = def.role == "Charge" || def.role == "Boss";
            bool ranged = def.role == "Shooter" || def.role == "WanderShooter" || def.id == "slasher";
            var attack = charge || ranged ? new EnemyAttack(charge, def.cooldown, def.cooldown + UnityEngine.Random.Range(-.12f, .12f)) : null;
            var obj=SpriteObject(def.id,def.image,pos);AddGroundShadow(obj, .7f, -def.image.bounds.extents.y + .08f);obj.SetActive(false);var marker=SpriteObject("Spawn warning",catalog.warning,pos,2);marker.transform.localScale=Vector3.one*.6f;marker.transform.rotation=Quaternion.Euler(0,0,UnityEngine.Random.Range(-25,25));float hp=def.health+def.growth*(Rules.Wave-1);enemies.Add(new Enemy
            {
                obj=obj,marker=marker,type=def,hp=hp,maxHp=hp,pos=pos,speed=Mathf.Max(0,def.speed+UnityEngine.Random.Range(-def.speedRandom,def.speedRandom)),cooldown=def.cooldown,renderers=obj.GetComponents<Renderer>(),wander=UnityEngine.Random.insideUnitCircle,attack=attack,spacing=(def.role=="Buffer"?5:3)+UnityEngine.Random.Range(-.7f,.7f)
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
                    mr.color = new Color(1, .25f, .2f, Mathf.Repeat(e.age * (2 + e.age * 8), 1) < .3f ? .35f : 1);
                    e.marker.transform.localScale = Vector3.one * (.52f + .12f * e.age);
                    if (e.age < 1) continue;
                    if (RunRules.Touches(e.pos, playerPos, Radius(e) + .35f))
                    {
                        e.pos = RandomPosition(playerPos, 3); e.marker.transform.position = e.pos; e.age = 0;
                        spawnRelocations++; continue;
                    }
                    Destroy(e.marker); e.marker = null; e.obj.SetActive(true); e.activeAge = 0;
                }
                e.activeAge += dt;
                Vector2 toward = (playerPos - e.pos).normalized;
                float distance = Vector2.Distance(e.pos, playerPos), speed = e.speed;
                e.cooldown -= dt; e.action += dt;
                string role = e.type.role;
                bool charge = role == "Charge" || role == "Boss";
                if (role == "Shooter" || role == "Buffer")
                    speed *= Mathf.Clamp((distance - e.spacing) / .35f, -1, 1);
                else if (role == "WanderShooter" || role == "Looter" || role == "Spawner")
                {
                    if (e.action > 1.5f || Mathf.Abs(e.pos.x) > 9.9f || Mathf.Abs(e.pos.y) > 4.7f)
                    { e.action = 0; e.wander = (RandomPosition(e.pos, 2) - e.pos).normalized; }
                    toward = e.wander;
                    if (role == "Looter" && distance < 4) toward = ((e.pos - playerPos).normalized + e.wander * .2f).normalized;
                }
                else if (role == "Pursuer") speed *= 1 + Mathf.Min(10, e.activeAge) * .1f;
                if (role == "Tree") speed = 0;

                if (e.attack != null)
                {
                    var previous = e.attack.Phase;
                    bool released = e.attack.Step(dt, playerPos - e.pos, distance <= e.type.range);
                    if (previous != AttackPhase.Windup && e.attack.Phase == AttackPhase.Windup)
                    { if (charge) sound.Play(SoundCue.Charge, e.pos); }
                    if (e.attack.Phase == AttackPhase.Windup) speed = 0;
                    else if (e.attack.Phase == AttackPhase.Recovery) speed *= .3f;
                    else if (charge && e.attack.Phase == AttackPhase.Active)
                    { toward = e.attack.Direction; speed += e.type.chargeSpeed; }
                    if (released)
                    {
                        if (charge) chargeReleases++;
                        else
                        {
                            rangedReleases++;
                            int spread = e.type.id == "slasher" ? 1 : 0;
                            for (int p = -spread; p <= spread; p++) EnemyBullet(e, Quaternion.Euler(0, 0, p * 18) * e.attack.Direction);
                        }
                    }
                }
                if (role == "Boss" && e.cooldown <= 0)
                {
                    e.cooldown = 2; int count = e.hp < e.maxHp * .5f ? 16 : 10;
                    for (int p = 0; p < count; p++)
                    { float a = p * Mathf.PI * 2 / count + elapsed * .6f; EnemyBullet(e, new Vector2(Mathf.Cos(a), Mathf.Sin(a))); }
                }
                if (role == "Buffer" && e.cooldown <= 0)
                {
                    e.cooldown = 4;
                    var target = enemies.Find(x => x != e && x.marker == null && !x.boosted && x.type.role != "Boss" && x.type.role != "Tree" && Vector2.Distance(x.pos, e.pos) < 5);
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
                bool winding = e.attack != null && e.attack.Phase == AttackPhase.Windup;
                if (winding) separation = Vector2.zero;
                if (charge && e.attack.Phase == AttackPhase.Active) separation *= .15f;
                e.pos = RunRules.ClampPosition(e.pos + (toward * speed + e.knockback + separation) * dt);
                e.knockback = Vector2.Lerp(e.knockback, Vector2.zero, 10 * dt);
                UpdateEnemyPresentation(e, toward * speed, dt);
                if (role != "Tree" && invulnerable <= 0 && e.knockback.sqrMagnitude < 1 && RunRules.Touches(e.pos, playerPos, Radius(e) + .3f))
                {
                    HurtPlayer((e.type.damage + e.type.damageGrowth * (Rules.Wave - 1)) * (e.boosted ? 1.25f : 1));
                    if (role == "Pursuer") e.activeAge = 0;
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
            bool dashing = e.attack != null && e.attack.Phase == AttackPhase.Active && (e.type.role == "Charge" || e.type.role == "Boss");
            var sprite = e.obj.GetComponent<SpriteRenderer>();
            if (Mathf.Abs(velocity.x) > .05f) sprite.flipX = velocity.x < 0;
            float scale = RadiusScale(e.type);
            float bob = reducedMotion || velocity.sqrMagnitude < .01f ? 0 : Mathf.Sin(e.activeAge * (e.type.id == "baby_alien" ? 18 : 11)) * .045f;
            float anticipation = reducedMotion || !winding ? 0 : e.attack.Progress * .16f;
            float emerge = reducedMotion ? 1 : Mathf.Lerp(.45f, 1, Mathf.Clamp01(e.activeAge / .16f));
            e.obj.transform.position = new Vector3(e.pos.x, e.pos.y, -1);
            e.obj.transform.localScale = new Vector3(scale * (1 + bob + anticipation) * emerge,
                scale * (1 - bob - anticipation) * emerge, 1);
            e.obj.transform.rotation = Quaternion.Euler(0, 0, reducedMotion ? 0 : dashing ? -Mathf.Sign(velocity.x) * 12 : bob * 45);
            e.supportFlash = Mathf.Max(0, e.supportFlash - dt);
            sprite.color = winding ? Color.Lerp(Color.white, new Color(1, .35f, .2f), e.attack.Progress) :
                e.boosted || e.supportFlash > 0 ? new Color(.85f, .55f, 1) : Color.white;
            Flash(e, e.flash > 0 ? 1 : 0);
            if (e.attack != null)
            {
                if (e.telegraph == null) e.telegraph = MakeLine("Attack direction", e.obj.transform, new Color(1, .55f, .22f, .65f), .055f);
                e.telegraph.enabled = winding;
                if (winding)
                {
                    // World coordinates keep the cue stable while the body squashes or tilts.
                    e.telegraph.useWorldSpace = true; e.telegraph.positionCount = 2;
                    Vector2 end = RunRules.ClampPosition(e.pos + e.attack.Direction * (e.type.role == "Charge" || e.type.role == "Boss" ? 3 : .95f));
                    e.telegraph.SetPosition(0, new Vector3(e.pos.x, e.pos.y, -.8f));
                    e.telegraph.SetPosition(1, new Vector3(end.x, end.y, -.8f));
                }
            }
        }
        void HurtPlayer(float amount)
        {
            if(invulnerable>0)return;float before=Rules.Health;Rules.Hurt(amount);float actual=before-Rules.Health;if(actual<=0)
            {
                FloatText(playerPos,"DODGE",mint,false);invulnerable=.15f;return;
            }
            hurtOverlay=.35f;FloatText(playerPos,$"-{Mathf.CeilToInt(actual)}",coral,true);hitStop=reducedMotion?0:.025f;invulnerable=.6f;shake=.08f;sound.Play(SoundCue.Hurt);Burst(playerPos,8,coral);
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
            shots.Add(new Shot { obj = obj, pos = e.pos, velocity = direction.normalized * 5, life = 5,
                damage = e.type.damage + e.type.damageGrowth * (Rules.Wave - 1), hostile = true });
        }
        void UpdateEquipment(float dt)
        {
            for(int i=0;i<Rules.Inventory.Count;i++)
            {
                var w=Rules.Inventory[i];weaponClocks[i]-=dt;swings[i]=Mathf.Max(0,swings[i]-dt);float range=Mathf.Max(.5f,w.range+Rules.Stat("range")/100);Enemy nearest=null;float distance=range*range;foreach(var e in enemies)
                {
                    if(e.marker!=null)continue;float d=(e.pos-playerPos).sqrMagnitude;if(d<distance)
                    {
                        distance=d;nearest=e;
                    }
                }
                float a=i*Mathf.PI*2/Mathf.Max(1,Rules.Inventory.Count);Vector2 mount=playerPos+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*.72f;Vector2 direction=nearest!=null?(nearest.pos-mount).normalized:new Vector2(Mathf.Cos(a),Mathf.Sin(a));var view=equipmentViews[i];if(view!=null)
                {
                    float age = .18f - swings[i];
                    float kick = SpriteFrameAnimator.Recoil(age);
                    float offset = w.melee ? swings[i] * 2 : -kick * (reducedMotion ? .05f : .17f);
                    view.transform.position = new Vector3(mount.x + direction.x * offset, mount.y + direction.y * offset, -2);
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    view.transform.rotation = Quaternion.Euler(0, 0, angle + (w.melee || reducedMotion ? 0 : kick * 7));
                    view.GetComponent<SpriteRenderer>().flipY = direction.x < 0;
                    if (muzzleViews[i] != null)
                    {
                        muzzleViews[i].enabled = age < .066f;
                        muzzleViews[i].sprite = catalog.muzzleFrames[Mathf.Clamp((int)(age / .022f), 0, 2)];
                    }
                }
                if(nearest==null||weaponClocks[i]>0)continue;weaponClocks[i]=Mathf.Max(.035f,w.interval/Rules.FireRate);swings[i]=.18f;float damage=Mathf.Max(1,w.damage+Rules.Stat(w.melee?"melee_damage":"ranged_damage"))*Rules.DamageScale;
                if(w.melee)
                {
                    Vector2 end=playerPos+direction*range;for(int j=enemies.Count-1;j>=0;j--)if(enemies[j].marker==null&&SegmentDistance(enemies[j].pos,playerPos,end)<Radius(enemies[j])+.45f)
                    {
                        Damage(j,damage,w.crit,w.knockback);orbitHits++;
                    }
                    Burst(end,4,paper);sound.Play(SoundCue.Swing,playerPos,w.sounds);
                }
                else
                {
                    Vector2 muzzle = mount + direction * (w.image.bounds.max.x * .8f);
                    if (muzzleViews[i] != null) { muzzleViews[i].enabled = true; muzzleViews[i].sprite = catalog.muzzleFrames[0]; }
                    for(int p=0;p<w.pellets;p++)
                    {
                        Vector2 dir=Quaternion.Euler(0,0,(p-(w.pellets-1)/2f)*Mathf.Max(5,w.spread*Mathf.Rad2Deg)) * direction;var obj=SpriteObject("Rivet tracer",catalog.muzzleFrames[2],muzzle,13);obj.transform.localScale=Vector3.one*.45f;obj.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(dir.y,dir.x)*Mathf.Rad2Deg);shots.Add(new Shot
                        {
                            obj=obj,pos=muzzle,velocity=dir*30,life=range/30,damage=damage,crit=w.crit,pierce=w.piercing,knockback=w.knockback
                        })
                        ;shotsFired++;
                    }
                    sound.Play(SoundCue.Fire,playerPos,w.sounds);
                }
            }
        }
        void UpdateWeapons(float dt)
        {
            for(int i=shots.Count-1;i>=0;i--)
            {
                var s=shots[i];Vector2 old=s.pos;s.pos+=s.velocity*dt;s.life-=dt;s.obj.transform.position=new Vector3(s.pos.x,s.pos.y,-2);if(s.hostile)
                {
                    if(SegmentDistance(playerPos,old,s.pos)<.36f)
                    {
                        HurtPlayer(s.damage);s.life=0;
                    }
                }
                else for(int j=enemies.Count-1;j>=0;j--)
                {
                    var e=enemies[j];if(e.marker!=null||s.hit.Contains(e))continue;if(SegmentDistance(e.pos,old,s.pos)<Radius(e)+.1f)
                    {
                        s.hit.Add(e);Damage(j,s.damage,s.crit,s.knockback);if(s.pierce--<=0)
                        {
                            s.life=0;break;
                        }
                        s.damage*=.5f;
                    }
                }
                if(s.life<=0||Mathf.Abs(s.pos.x)>10.6f||Mathf.Abs(s.pos.y)>5.25f)
                {
                    Destroy(s.obj);shots.RemoveAt(i);
                }
            }
        }
        void Damage(int index,float damage,float crit=0,float knockback=1)
        {
            var e=enemies[index];if(e.marker!=null)return;bool critical=UnityEngine.Random.value<Mathf.Clamp01(Rules.CritChance+crit);if(critical)
            {
                damage*=2;hitStop=reducedMotion?0:.025f;shake=.035f;
            }
            damage=Mathf.Max(1,damage/(1+Mathf.Max(0,e.type.armor+e.type.armorGrowth*(Rules.Wave-1))/15));e.hp-=damage;e.flash=.08f;e.knockback=(e.pos-playerPos).normalized*3*knockback*(1-Mathf.Clamp01(e.type.resistance));Flash(e,1);FloatText(e.pos,Mathf.CeilToInt(damage)+(critical?"!":""),critical?amber:Color.white,critical);Burst(e.pos,critical?7:3,critical?amber:coral);sound.Play(critical?SoundCue.Critical:e.hp<=0?SoundCue.Kill:SoundCue.Impact,e.pos);if(Rules.Stat("lifesteal")>0&&UnityEngine.Random.value<Rules.Stat("lifesteal")/100)Rules.Heal(1);if(e.hp>0)return;Rules.Kill();if(e.type.role=="Boss")Rules.DefeatBoss();Flash(e,0);if(e.telegraph!=null)e.telegraph.enabled=false;e.deathAge=0;corpses.Add(e);enemies.RemoveAt(index);Drop(e.pos,e.type.value,false);if(e.type.role=="Tree"||UnityEngine.Random.value<e.type.dropChance)Drop(e.pos,0,true);if(e.type.role=="Spawner")for(int i=0;i<3;i++)births.Enqueue(new Birth
            {
                type=catalog.Enemy("junkie"),pos=RunRules.ClampPosition(e.pos+UnityEngine.Random.insideUnitCircle)
            })
            ;
        }
        void Drop(Vector2 p,int value,bool fruit)
        {
            int[] values = fruit ? (pickups.Count < 240 ? new[] { 0 } : Array.Empty<int>()) : ScrapMotion.SplitValue(value, 240 - pickups.Count);
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
                obj.transform.localScale = Vector3.one * (fruit ? .34f : UnityEngine.Random.Range(.24f, .34f));
                pickups.Add(new Pickup { obj = obj, value = amount, fruit = fruit,
                    motion = new ScrapMotion { position = p, velocity = UnityEngine.Random.insideUnitCircle * 3.2f,
                        verticalSpeed = UnityEngine.Random.Range(2.5f, 4f), height = .06f,
                        angle = UnityEngine.Random.Range(0, 360), spin = UnityEngine.Random.Range(-260, 260) } });
            }
        }
        void UpdateCorpses(float dt)
        {
            for(int i=corpses.Count-1;i>=0;i--)
            {
                var e=corpses[i];e.deathAge+=dt;float t=Mathf.Clamp01(e.deathAge/.5f);e.pos+=e.knockback*dt;e.knockback=Vector2.Lerp(e.knockback,Vector2.zero,dt*8);e.obj.transform.position=e.pos;e.obj.transform.localScale=Vector3.one*RadiusScale(e.type)*Mathf.Max(.00001f,1-t);e.obj.transform.rotation=Quaternion.Euler(0,0,reducedMotion?0:t*360);e.obj.GetComponent<SpriteRenderer>().color=Color.Lerp(Color.white,new Color(.349f,.349f,.349f),t);if(e.deathAge>=1)
                {
                    Destroy(e.obj);corpses.RemoveAt(i);
                }
            }
        }
        void ClearReferenceCombat()
        {
            births.Clear();foreach(var e in corpses)Destroy(e.obj);corpses.Clear();
        }
    }
}

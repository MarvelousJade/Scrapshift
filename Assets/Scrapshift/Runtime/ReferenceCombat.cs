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
        float Radius(Enemy e)=>e.type.role=="Boss"?.9f:e.type.role=="Tree"?.45f:.3f;
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
            var def=catalog.Enemy(unit.id);if(def==null)return;Vector2 anchor=RandomPosition(playerPos,3+Mathf.Max(0,group.area)*.5f);for(int i=0;i<count&&births.Count<100;i++)
            {
                Vector2 p=group.area<0?RandomPosition(playerPos,3+unit.distance):RunRules.ClampPosition(anchor+UnityEngine.Random.insideUnitCircle*group.area*.5f);if(group.edge)
                {
                    if(UnityEngine.Random.value<.5f)p.x=(UnityEngine.Random.value<.5f?-1:1)*(10.2f-group.inset);else p.y=(UnityEngine.Random.value<.5f?-1:1)*(4.9f-group.inset);
                }
                if(Vector2.Distance(p,playerPos)<3)p=RandomPosition(playerPos,3);births.Enqueue(new Birth
                {
                    type=def,pos=p
                })
                ;
            }
        }
        void UpdateBirthQueue(float dt)
        {
            birthClock-=dt;while(birthClock<=0&&births.Count>0)
            {
                birthClock+=.05f;var b=births.Dequeue();if(enemies.Count>=catalog.data.waves[Rules.Wave-1].cap)
                {
                    int remove=enemies.FindIndex(e=>e.type.role!="Boss"&&e.type.role!="Looter"&&e.type.role!="Tree");if(remove<0)continue;Destroy(enemies[remove].obj);Destroy(enemies[remove].marker);enemies.RemoveAt(remove);
                }
                Spawn(b.type,b.pos);
            }
            if(births.Count==0)birthClock=0;
        }
        void Spawn(EnemyEntry def,Vector2 pos)
        {
            var obj=SpriteObject(def.id,def.image,pos);AddGroundShadow(obj, .7f, -def.image.bounds.extents.y + .08f);obj.SetActive(false);var marker=SpriteObject("Spawn warning",catalog.warning,pos,2);marker.transform.localScale=Vector3.one*.6f;marker.transform.rotation=Quaternion.Euler(0,0,UnityEngine.Random.Range(-25,25));float hp=def.health+def.growth*(Rules.Wave-1);enemies.Add(new Enemy
            {
                obj=obj,marker=marker,type=def,hp=hp,maxHp=hp,pos=pos,speed=Mathf.Max(0,def.speed+UnityEngine.Random.Range(-def.speedRandom,def.speedRandom)),cooldown=def.cooldown,renderers=obj.GetComponents<Renderer>(),wander=UnityEngine.Random.insideUnitCircle
            })
            ;peakEnemies=Mathf.Max(peakEnemies,enemies.Count);
        }
        void UpdateEnemies(float dt)
        {
            for(int i=enemies.Count-1;i>=0;i--)
            {
                var e=enemies[i];e.age+=dt;e.flash=Mathf.Max(0,e.flash-dt);
                if(e.marker!=null)
                {
                    var mr=e.marker.GetComponent<SpriteRenderer>();mr.color=new Color(1,.12f,.12f,Mathf.Repeat(e.age*(2+e.age*8),1)<.3f?.25f:1);if(e.age<1)continue;if(RunRules.Touches(e.pos,playerPos,Radius(e)+.35f))
                    {
                        e.pos=RandomPosition(playerPos,3);e.marker.transform.position=e.pos;e.age=0;continue;
                    }
                    Destroy(e.marker);e.marker=null;e.obj.SetActive(true);
                }
                Vector2 toward=(playerPos-e.pos).normalized;float distance=Vector2.Distance(e.pos,playerPos),speed=e.speed;e.cooldown-=dt;e.action+=dt;string role=e.type.role;
                if(role=="Charge"||role=="Boss")
                {
                    float idle=Mathf.Max(.4f,e.type.cooldown);if(e.action<idle)
                    {
                        e.charge=toward;if(distance>e.type.range)e.action=Mathf.Min(e.action,idle-dt);
                    }
                    else if(e.action<idle+.4f)
                    {
                        speed=0;Flash(e,.5f);if(e.action-dt<idle)sound.Play(SoundCue.Charge,e.pos);
                    }
                    else if(e.action<idle+1.15f)
                    {
                        toward=e.charge;speed+=e.type.chargeSpeed;
                    }
                    else e.action=0;
                    if(role=="Boss"&&e.cooldown<=0)
                    {
                        e.cooldown=2;int count=e.hp<e.maxHp*.5f?16:10;for(int p=0;p<count;p++)
                        {
                            float a=p*Mathf.PI*2/count+elapsed*.6f;EnemyBullet(e,new Vector2(Mathf.Cos(a),Mathf.Sin(a)));
                        }
                    }
                }
                else if(role=="Shooter")
                {
                    speed=distance<2.8f?-speed:distance>3.5f?speed:0;if(e.cooldown<=0&&distance<e.type.range)
                    {
                        e.cooldown=Mathf.Max(.6f,e.type.cooldown);EnemyBullet(e,toward);
                    }
                }
                else if(role=="WanderShooter"||role=="Looter"||role=="Spawner"||role=="Buffer")
                {
                    if(e.action>1.5f)
                    {
                        e.action=0;e.wander=UnityEngine.Random.insideUnitCircle.normalized;
                    }
                    toward=e.wander;if(role=="Looter"&&distance<4)toward=-toward*.2f+(e.pos-playerPos).normalized;if(role=="WanderShooter"&&e.cooldown<=0)
                    {
                        e.cooldown=Mathf.Max(.7f,e.type.cooldown);EnemyBullet(e,(playerPos-e.pos).normalized);
                    }
                    if(role=="Buffer"&&e.cooldown<=0)
                    {
                        e.cooldown=4;var target=enemies.Find(x=>x!=e&&x.marker==null&&!x.boosted&&x.type.role!="Boss"&&x.type.role!="Tree"&&Vector2.Distance(x.pos,e.pos)<5);if(target!=null)
                        {
                            target.boosted=true;target.hp*=2.5f;target.maxHp*=2.5f;target.speed*=1.5f;FloatText(target.pos,"BOOST",new Color(.8f,.4f,1),false);
                        }
                    }
                }
                else if(role=="Pursuer")
                {
                    speed*=1+Mathf.Min(10,e.age-1)*.1f;
                }
                if(e.type.id=="slasher"&&e.cooldown<=0&&distance<5)
                {
                    e.cooldown=1.25f;for(int p=-1;p<=1;p++)EnemyBullet(e,Quaternion.Euler(0,0,p*18)*toward);
                }
                if(role=="Tree")speed=0;
                Vector2 separation=Vector2.zero; foreach(var other in enemies)
                {
                    if(other==e||other.marker!=null)continue;var delta=e.pos-other.pos;float d=delta.magnitude;float gap=Radius(e)+Radius(other);if(d>.001f&&d<gap)separation+=delta/d*(gap-d)*5;
                }
                e.pos=RunRules.ClampPosition(e.pos+(toward*speed+e.knockback+separation)*dt);e.knockback=Vector2.Lerp(e.knockback,Vector2.zero,10*dt);
                if(!(role=="Charge"&&e.action>=e.type.cooldown&&e.action<e.type.cooldown+.4f))Flash(e,e.flash>0?1:0);
                e.obj.transform.position=new Vector3(e.pos.x,e.pos.y,-1);float bob=reducedMotion?0:Mathf.Sin(e.age*12)*.025f;float scale=e.type.role=="Boss"?1.3f:1;e.obj.transform.localScale=new Vector3(scale+bob+e.flash,scale-bob+e.flash,1);
                if(role!="Tree"&&invulnerable<=0&&e.knockback.sqrMagnitude<1&&RunRules.Touches(e.pos,playerPos,Radius(e)+.3f))
                {
                    HurtPlayer((e.type.damage+e.type.damageGrowth*(Rules.Wave-1))*(e.boosted?1.25f:1));if(role=="Pursuer")e.age=1;
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
            if(shots.Count>=400)return;var obj=Instantiate(weapons[0].effectPrefab);var block=new MaterialPropertyBlock();block.SetColor("_Color",coral);obj.GetComponentInChildren<Renderer>().SetPropertyBlock(block);obj.transform.localScale=Vector3.one*1.8f;shots.Add(new Shot
            {
                obj=obj,pos=e.pos,velocity=direction.normalized*5,life=5,damage=e.type.damage+e.type.damageGrowth*(Rules.Wave-1),hostile=true
            })
            ;
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
                        Vector2 dir=Quaternion.Euler(0,0,(p-(w.pellets-1)/2f)*Mathf.Max(5,w.spread*Mathf.Rad2Deg)) * direction;var obj=Instantiate(weapons[0].effectPrefab);obj.transform.position=muzzle;obj.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(dir.y,dir.x)*Mathf.Rad2Deg);shots.Add(new Shot
                        {
                            obj=obj,pos=muzzle,velocity=dir*30,life=range/30,damage=damage,crit=w.crit,pierce=w.piercing
                        })
                        ;shotsFired++;
                    }
                    Burst(muzzle,2,amber);sound.Play(SoundCue.Fire,playerPos,w.sounds);
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
                        s.hit.Add(e);Damage(j,s.damage,s.crit);if(s.pierce--<=0)
                        {
                            s.life=0;break;
                        }
                        s.damage*=.5f;
                    }
                }
                if(s.life<=0)
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
            damage=Mathf.Max(1,damage/(1+Mathf.Max(0,e.type.armor+e.type.armorGrowth*(Rules.Wave-1))/15));e.hp-=damage;e.flash=.08f;e.knockback=(e.pos-playerPos).normalized*3*knockback*(1-Mathf.Clamp01(e.type.resistance));Flash(e,1);FloatText(e.pos,Mathf.CeilToInt(damage)+(critical?"!":""),critical?amber:Color.white,critical);Burst(e.pos,critical?7:3,critical?amber:coral);sound.Play(critical?SoundCue.Critical:e.hp<=0?SoundCue.Kill:SoundCue.Impact,e.pos);if(Rules.Stat("lifesteal")>0&&UnityEngine.Random.value<Rules.Stat("lifesteal")/100)Rules.Heal(1);if(e.hp>0)return;Rules.Kill();if(e.type.role=="Boss")Rules.DefeatBoss();Flash(e,0);e.deathAge=0;corpses.Add(e);enemies.RemoveAt(index);Drop(e.pos,e.type.value,false);if(e.type.role=="Tree"||UnityEngine.Random.value<e.type.dropChance)Drop(e.pos,0,true);if(e.type.role=="Spawner")for(int i=0;i<3;i++)births.Enqueue(new Birth
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
                var e=corpses[i];e.deathAge+=dt;float t=Mathf.Clamp01(e.deathAge/.5f);e.pos+=e.knockback*dt;e.knockback=Vector2.Lerp(e.knockback,Vector2.zero,dt*8);e.obj.transform.position=e.pos;e.obj.transform.localScale=Vector3.one*Mathf.Max(.00001f,1-t);e.obj.transform.rotation=Quaternion.Euler(0,0,reducedMotion?0:t*360);e.obj.GetComponent<SpriteRenderer>().color=Color.Lerp(Color.white,new Color(.349f,.349f,.349f),t);if(e.deathAge>=1)
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

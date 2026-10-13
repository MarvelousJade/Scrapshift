using System.Collections.Generic;
using UnityEngine;

namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        sealed class StarterTurret { public GameObject obj; public Vector2 pos; public Equipment stats; public float clock; public int shotsTaken;public WeaponTrait trait; public StructureSpec spec;public SupportRig rig;public float angle,orbitRadius,orbitSpeed,animationAge=.3f,baseScale;public bool firing;public readonly HashSet<Enemy> touched=new HashSet<Enemy>(); }
        sealed class Mine { public GameObject obj; public Vector2 pos; public Equipment stats; public float radius; public readonly HashSet<Enemy> occupants=new HashSet<Enemy>(); public bool playerOn; }
        readonly List<Mine> mines=new List<Mine>();
        readonly List<StarterTurret> mineSpawners=new List<StarterTurret>();
        readonly List<StarterTurret> starterTurrets=new List<StarterTurret>();
        readonly Dictionary<Equipment,int> ghostKills=new Dictionary<Equipment,int>();
        Enemy NearestStarterTarget(Vector2 point,HashSet<Enemy> excluded=null)
        {
            Enemy target=null;float best=float.PositiveInfinity;
            foreach(var e in enemies)
            {
                if(e.marker!=null||e.hp<=0||excluded!=null&&excluded.Contains(e))continue;
                float d=(e.pos-point).sqrMagnitude;if(d<best){best=d;target=e;}
            }
            return target;
        }
        void StarterProjectile(Vector2 position,Vector2 direction,Equipment stats,bool secondary,Enemy exclude=null,bool delayedSlow=false,bool structure=false,StructureSpec structureSpec=null)
        {
            if(shots.Count>=400)return;
            var obj=SpriteObject("Weapon projectile",catalog.muzzleFrames[2],position,13);
            obj.transform.localScale=Vector3.one*.35f;obj.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg);
            var shot=new Shot{obj=obj,pos=position,velocity=direction.normalized*stats.projectileSpeed,
                life=((structure?stats.range:Rules.WeaponRange(stats))+1)/Mathf.Max(1,stats.projectileSpeed),damage=structure?SpecialWeaponRules.StructureDamage(Rules,stats):Rules.WeaponDamage(stats),
                crit=stats.crit,pierce=stats.piercing+(int)Rules.Stat("piercing"),bounces=stats.bounce+(int)Rules.Stat("bounce"),knockback=stats.knockback,weapon=stats,secondary=secondary,structure=structure,structureSpec=structureSpec};
            if(structureSpec?.delayed==true)shot.hitDelay=UnityEngine.Random.Range(0,.05f);
            if(structureSpec?.kind=="flame")shot.damage=1;
            if(structureSpec!=null)SupportProjectileArt.Pose(obj.GetComponent<SpriteRenderer>(),structureSpec.kind,0);
            if(delayedSlow){shot.hitDelay=UnityEngine.Random.Range(0,.05f);shot.slow=2;}
            if(exclude!=null)shot.hit.Add(exclude);shots.Add(shot);shotsFired++;
        }
        void ApplyStarterHit(Enemy victim,Equipment weapon,bool critical,bool killed,bool secondary,bool structure=false)
        {
            if(weapon!=null&&!killed&&Rules.RollBurn(weapon,UnityEngine.Random.value,out float burnDamage,out int burnTicks,structure))
                ApplyBurn(victim,burnDamage,burnTicks,(int)Rules.Stat("burning_spread"),weapon.Trait("effect_burning")?.burnStat??"elemental_damage");
            if(weapon==null||secondary||weapon.traits==null)return;
            foreach(var trait in weapon.traits)
            {
                switch(trait.key)
                {
                    case "effect_burning":
                        break;
                    case "effect_slow_in_zone":
                        if(!killed)victim.contactSlow+=2;break;
                    case "effect_projectiles_on_hit":case "effect_lightning_on_hit":case "effect_slow_projectiles_on_hit":
                        for(int i=0;i<(int)trait.value;i++)
                        {
                            var target=trait.key=="effect_lightning_on_hit"?NearestStarterTarget(victim.pos,new HashSet<Enemy>{victim}):null;
                            Vector2 direction=target!=null?(target.pos-victim.pos).normalized:UnityEngine.Random.insideUnitCircle.normalized;
                            StarterProjectile(victim.pos,direction,trait.projectile.AsEquipment(),true,victim,trait.key=="effect_slow_projectiles_on_hit");
                        }
                        break;
                    case "effect_explode":case "effect_explode_melee":case "effect_explode_custom":
                        if(UnityEngine.Random.value<trait.chance)
                        {
                            float radius=trait.radius*(1+Rules.Stat("explosion_size")/100);int before=Rules.Kills;
                            Burst(victim.pos,12,amber);
                            foreach(var e in enemies.ToArray())if(e!=victim&&e.marker==null&&Vector2.Distance(e.pos,victim.pos)<radius)
                                Damage(enemies.IndexOf(e),Rules.WeaponDamage(weapon)*Mathf.Max(0,1+Rules.Stat("explosion_damage")/100),weapon.crit,weapon.knockback,e.pos-victim.pos,weapon,true);
                            Rules.RecordExplosionKills(Rules.Kills-before);
                        }
                        break;
                    case "gold_on_crit_kill":
                        if(killed&&critical&&UnityEngine.Random.value<trait.value/100)Drop(victim.pos,1,false);break;
                    case "effect_gain_stat_every_killed_enemies":
                        if(killed)
                        {
                            ghostKills.TryGetValue(weapon,out int count);count++;
                            if(count>=trait.value){count=0;Rules.GainWeaponStat(trait.stat,trait.amount);FloatText(playerPos,ItemEntry.FormatStat(trait.stat,trait.amount),mint,false);}
                            ghostKills[weapon]=count;
                        }
                        break;
                }
            }
        }
        void ResetStructures()
        {
            ClearStarterEffects();float groupWidth=ArenaGeometry.HalfSize.x*2/3;structureGroupCenter=new Vector2(UnityEngine.Random.Range(-groupWidth/2,groupWidth/2),UnityEngine.Random.Range(-groupWidth/2,groupWidth/2));ResetPets();
            foreach(var w in Rules.Inventory)
            {
                var trait=w.Trait("turret")??w.Trait("garden")??w.Trait("landmine");if(trait?.projectile==null)continue;
                DeployStructure(trait.structure??new StructureSpec{kind=trait.key,stats=trait.projectile,spawnInterval=trait.spawnInterval,animationSpeed=trait.animationSpeed,explosionRadius=trait.radius},w.image);
            }
            foreach(var item in Rules.Items)if(item.conditionalEffects!=null)foreach(var effect in item.conditionalEffects)
                if(effect.customKey=="structure"&&effect.structure!=null)for(int i=0;i<(int)effect.value;i++)DeployStructure(effect.structure,StructureImage(effect.structure.kind));
        }
        void UpdateStarterEffects(float dt)
        {
            foreach(var spawner in mineSpawners)
            {spawner.clock-=dt;while(spawner.clock<=0){spawner.clock+=spawner.trait.spawnInterval;SpawnMine(spawner,catalog.Weapon("screwdriver",1).image);}}
            for(int i=mines.Count-1;i>=0;i--)
            {
                var mine=mines[i];bool left=false;var center=mine.pos+Vector2.up*.16f;
                foreach(var e in mine.occupants)if(!enemies.Contains(e)||Vector2.Distance(e.pos,center)>.24f+HurtRadius(e)){left=true;break;}
                bool playerOn=Vector2.Distance(PlayerHurtCenter,center)<.24f+CombatGeometry.PlayerHurtRadius;
                if(mine.playerOn&&!playerOn)left=true;mine.playerOn=playerOn;
                foreach(var e in enemies)if(e.marker==null&&Vector2.Distance(e.pos,center)<.24f+HurtRadius(e))mine.occupants.Add(e);
                if(mine.occupants.Count>0||mine.playerOn)
                {
                    var renderer=mine.obj.GetComponent<SpriteRenderer>();
                    if(renderer.color!=coral){OriginalSupportAudio.Play(sound,"mine_press",mine.pos);mine.obj.transform.localScale*=.9f;renderer.color=coral;}
                }
                if(!left)continue;
                int explosionKillsBefore=Rules.Kills;
            foreach(var e in enemies.ToArray())if(e.marker==null&&Vector2.Distance(e.pos,mine.pos)<mine.radius)
                    Damage(enemies.IndexOf(e),SpecialWeaponRules.StructureDamage(Rules,mine.stats)*Mathf.Max(0,1+Rules.Stat("explosion_damage")/100),mine.stats.crit,mine.stats.knockback,e.pos-mine.pos,mine.stats,true,false,true);
                Rules.RecordExplosionKills(Rules.Kills-explosionKillsBefore);Burst(mine.pos,18,amber);sound.Play(SoundCue.Impact,mine.pos);Destroy(mine.obj);mines.RemoveAt(i);
            }
            foreach(var e in enemies.ToArray())if(e.burnTicks>0&&e.marker==null)
            {
                if(e.burnSpread>0)foreach(var target in enemies)if(target!=e&&target.marker==null&&target.burnTicks==0&&Vector2.Distance(target.pos,e.pos)<=1.28f)
                {ApplyBurn(target,e.burnDamage,e.burnTicks,e.burnSpread-1,e.burnStat);e.burnSpread=0;break;}
                e.burnClock-=dt;if(e.burnClock>0)continue;
                e.burnClock+=Rules.BurningInterval;e.burnTicks--;Burst(e.pos,2,amber);
                Damage(enemies.IndexOf(e),e.burnDamage,-100,0,Vector2.up,null,true,true);
            }
            foreach(var turret in starterTurrets)TickStructure(turret,dt);
        }
        void ClearStarterEffects()
        {
            foreach(var t in starterTurrets)Destroy(t.obj);starterTurrets.Clear();
            foreach(var mine in mines)Destroy(mine.obj);mines.Clear();mineSpawners.Clear();
        }
        void ApplyBurn(Enemy target,float damage,int ticks,int spread,string stat="elemental_damage")
        {stat=string.IsNullOrEmpty(stat)?"elemental_damage":stat;target.burnStat=stat;TryVulnerability(target,stat);if(target.burnTicks<=0)target.burnClock=Rules.BurningInterval;target.burnDamage=Mathf.Max(target.burnTicks>0?target.burnDamage:0,damage);target.burnTicks=Mathf.Max(target.burnTicks,ticks);target.burnSpread=Mathf.Max(target.burnSpread,spread);}
        void SpawnMine(StarterTurret spawner,Sprite image,Vector2? near=null)
        {
            image=SupportArt.Structure("landmine");
            var pos=near.HasValue?RunRules.ClampPosition(near.Value+new Vector2(UnityEngine.Random.Range(-.5f,.5f),UnityEngine.Random.Range(-.5f,.5f))):RandomPosition(playerPos,0);
            var obj=SpriteObject("Cybernetic proximity charge",image,pos,6);obj.transform.localScale=Vector3.one*ActorPresentation.SpriteLengthScale(image,.4f);
            mines.Add(new Mine{obj=obj,pos=pos,stats=spawner.stats,radius=spawner.trait.radius*Mathf.Max(0,1+Rules.Stat("explosion_size")/100)});
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        sealed class StarterTurret { public GameObject obj; public Vector2 pos; public Equipment stats; public float clock; public WeaponTrait trait; }
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
        void StarterProjectile(Vector2 position,Vector2 direction,Equipment stats,bool secondary,Enemy exclude=null,bool delayedSlow=false,bool structure=false)
        {
            if(shots.Count>=400)return;
            var obj=SpriteObject("Weapon projectile",catalog.muzzleFrames[2],position,13);
            obj.transform.localScale=Vector3.one*.35f;obj.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg);
            var shot=new Shot{obj=obj,pos=position,velocity=direction.normalized*stats.projectileSpeed,
                life=(stats.range+1)/Mathf.Max(1,stats.projectileSpeed),damage=structure?SpecialWeaponRules.StructureDamage(Rules,stats):Rules.WeaponDamage(stats),
                crit=stats.crit,pierce=stats.piercing,bounces=stats.bounce,knockback=stats.knockback,weapon=stats,secondary=secondary,structure=structure};
            if(delayedSlow){shot.hitDelay=UnityEngine.Random.Range(0,.05f);shot.slow=2;}
            if(exclude!=null)shot.hit.Add(exclude);shots.Add(shot);shotsFired++;
        }
        void ApplyStarterHit(Enemy victim,Equipment weapon,bool critical,bool killed,bool secondary)
        {
            if(weapon!=null&&!killed&&Rules.RollBurn(weapon,UnityEngine.Random.value,out float burnDamage,out int burnTicks))
                ApplyBurn(victim,burnDamage,burnTicks,(int)Rules.Stat("burning_spread"));
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
                            float radius=trait.radius*(1+Rules.Stat("explosion_size")/100);
                            Burst(victim.pos,12,amber);
                            foreach(var e in enemies.ToArray())if(e!=victim&&e.marker==null&&Vector2.Distance(e.pos,victim.pos)<radius)
                                Damage(enemies.IndexOf(e),Rules.WeaponDamage(weapon)*Mathf.Max(0,1+Rules.Stat("explosion_damage")/100),weapon.crit,weapon.knockback,e.pos-victim.pos,weapon,true);
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
            ClearStarterEffects();
            foreach(var w in Rules.Inventory)
            {
                var trait=w.Trait("turret")??w.Trait("garden")??w.Trait("landmine");if(trait?.projectile==null)continue;
                var pos=new Vector2(UnityEngine.Random.Range(-7f,7f),UnityEngine.Random.Range(-5f,5f));
                var obj=SpriteObject("Deployable turret",w.image,pos,7);
                obj.transform.localScale=Vector3.one*ActorPresentation.SpriteLengthScale(w.image,.6f);
                AddGroundShadow(obj,w.image.bounds.size.x,-.2f);
                var structure=new StarterTurret{obj=obj,pos=pos,stats=trait.projectile.AsEquipment(),trait=trait,clock=trait.key=="garden"?.2f/trait.animationSpeed:0};
                if(trait.key=="landmine"){Destroy(obj);structure.obj=null;structure.clock=trait.spawnInterval;mineSpawners.Add(structure);SpawnMine(structure,w.image);}
                else starterTurrets.Add(structure);
            }
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
                if(mine.occupants.Count>0||mine.playerOn)mine.obj.GetComponent<SpriteRenderer>().color=coral;
                if(!left)continue;
                foreach(var e in enemies.ToArray())if(e.marker==null&&Vector2.Distance(e.pos,mine.pos)<mine.radius)
                    Damage(enemies.IndexOf(e),SpecialWeaponRules.StructureDamage(Rules,mine.stats)*Mathf.Max(0,1+Rules.Stat("explosion_damage")/100),0,0,e.pos-mine.pos,mine.stats,true,false,true);
                Burst(mine.pos,18,amber);sound.Play(SoundCue.Impact,mine.pos);Destroy(mine.obj);mines.RemoveAt(i);
            }
            foreach(var e in enemies.ToArray())if(e.burnTicks>0&&e.marker==null)
            {
                if(e.burnSpread>0)foreach(var target in enemies)if(target!=e&&target.marker==null&&target.burnTicks==0&&Vector2.Distance(target.pos,e.pos)<=1.28f)
                {ApplyBurn(target,e.burnDamage,e.burnTicks,e.burnSpread-1);e.burnSpread=0;break;}
                e.burnClock-=dt;if(e.burnClock>0)continue;
                e.burnClock+=Rules.BurningInterval;e.burnTicks--;Burst(e.pos,2,amber);
                Damage(enemies.IndexOf(e),e.burnDamage,-100,0,Vector2.up,null,true,true);
            }
            foreach(var turret in starterTurrets)
            {
                turret.clock-=dt;if(turret.clock>0)continue;
                if(turret.trait?.key=="garden")
                {DropConsumable(turret.pos,ConsumableKind.Fruit);turret.clock=UnityEngine.Random.Range(Rules.StructureCooldown(turret.stats.interval)*.7f,Rules.StructureCooldown(turret.stats.interval)*1.3f)+.3f/turret.trait.animationSpeed;sound.Play(SoundCue.Pickup,turret.pos);continue;}
                var target=NearestStarterTarget(turret.pos);
                if(target==null||Vector2.Distance(target.pos,turret.pos)>turret.stats.range)continue;
                StarterProjectile(turret.pos,target.pos-turret.pos,turret.stats,true,structure:true);
                turret.clock=Rules.StructureCooldown(turret.stats.interval)+turret.stats.recoilHalf*2;
                sound.Play(SoundCue.Fire,turret.pos);
            }
        }
        void ClearStarterEffects()
        {
            foreach(var t in starterTurrets)Destroy(t.obj);starterTurrets.Clear();
            foreach(var mine in mines)Destroy(mine.obj);mines.Clear();mineSpawners.Clear();
        }
        void ApplyBurn(Enemy target,float damage,int ticks,int spread)
        {if(target.burnTicks<=0)target.burnClock=Rules.BurningInterval;target.burnDamage=Mathf.Max(target.burnTicks>0?target.burnDamage:0,damage);target.burnTicks=Mathf.Max(target.burnTicks,ticks);target.burnSpread=Mathf.Max(target.burnSpread,spread);}
        void SpawnMine(StarterTurret spawner,Sprite image)
        {
            var pos=new Vector2(UnityEngine.Random.Range(-7f,7f),UnityEngine.Random.Range(-5f,5f));
            var obj=SpriteObject("Cybernetic proximity charge",image,pos,6);obj.transform.localScale=Vector3.one*ActorPresentation.SpriteLengthScale(image,.4f);
            mines.Add(new Mine{obj=obj,pos=pos,stats=spawner.stats,radius=spawner.trait.radius*Mathf.Max(0,1+Rules.Stat("explosion_size")/100)});
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        sealed class StarterTurret { public GameObject obj; public Vector2 pos; public Equipment stats; public float clock,boostClock; public int shotsTaken;public WeaponTrait trait; public StructureSpec spec;public SupportRig rig;public float angle,orbitRadius,orbitSpeed,animationAge=.3f,baseScale;public bool firing;public readonly HashSet<Enemy> touched=new HashSet<Enemy>(); }
        sealed class Mine { public GameObject obj; public Vector2 pos; public Equipment stats; public float radius; public readonly HashSet<Enemy> occupants=new HashSet<Enemy>(); public bool playerOn; }
        readonly List<Mine> mines=new List<Mine>();
        readonly List<StarterTurret> mineSpawners=new List<StarterTurret>();
        readonly List<StarterTurret> starterTurrets=new List<StarterTurret>();
        readonly Dictionary<int,int> ghostKills=new Dictionary<int,int>();
        Enemy RandomStarterTarget(Enemy excluded)
        {
            // EntitySpawner.get_rand_enemy samples ordinary enemies, excluding
            // the struck unit. It does not choose the closest unit or bosses.
            var available=enemies.FindAll(e=>e!=excluded&&e.marker==null&&e.hp>0&&e.type.role!="Boss"&&e.type.role!="Elite"&&e.type.role!="Tree");
            return available.Count==0?null:available[UnityEngine.Random.Range(0,available.Count)];
        }
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
                life=((structure?Rules.PetRange(stats,true):stats.pet?Rules.PetRange(stats):Rules.WeaponRange(stats))+1)/Mathf.Max(1,stats.projectileSpeed),damage=structure?SpecialWeaponRules.StructureDamage(Rules,stats,structureSpec?.explosionRadius>0?Rules.Stat("explosion_damage"):0):Rules.WeaponDamage(stats),
                crit=stats.crit,pierce=stats.piercing+(int)Rules.Stat("piercing"),bounces=stats.bounce+(int)Rules.Stat("bounce"),knockback=stats.knockback,weapon=stats,burnPayload=Rules.CaptureBurn(stats,structure),secondary=secondary,structure=structure,structureSpec=structureSpec};
            if(structureSpec?.delayed==true)shot.hitDelay=UnityEngine.Random.Range(0,.05f);
            if(structureSpec?.kind=="flame")shot.damage=1;
            if(structureSpec!=null)SupportProjectileArt.Pose(obj.GetComponent<SpriteRenderer>(),structureSpec.kind,0);
            else if(SupportProjectileBehavior.PetArtKind(stats) is string petArt)SupportProjectileArt.Pose(obj.GetComponent<SpriteRenderer>(),petArt,0);
            if(delayedSlow){shot.hitDelay=UnityEngine.Random.Range(0,.05f);shot.slow=2;}
            ConfigureReferenceShot(shot);if(exclude!=null)shot.hit.Add(exclude);shots.Add(shot);shotsFired++;
        }
        void ApplyStarterHit(Enemy victim,Equipment weapon,bool critical,bool killed,bool secondary,bool structure=false,float hitDamage=-1)
        {
            if(!killed)TryApplyWeaponBurn(victim,weapon,structure);
            if(killed){ApplyStarterKillStatistics(weapon,-1);if(!secondary)ApplyStarterCriticalGold(weapon,critical);}
            ApplyStarterHitEffects(victim,weapon,critical,killed,secondary,structure,hitDamage);
        }
        bool TryApplyWeaponBurn(Enemy victim,Equipment weapon,bool structure,BurnPayload burnPayload=null)
        {
            if(weapon==null)return false;
            var payload=burnPayload??Rules.CaptureBurn(weapon,structure);
            if(!payload.Succeeds(UnityEngine.Random.value))return false;
            ApplyBurn(victim,payload.damage,payload.ticks,payload.spread,payload.stat);
            return true;
        }
        void ApplyStarterKillStatistics(Equipment weapon,int weaponIndex)
        {
            if(weapon==null||weapon.traits==null)return;
            // Inventory entries share catalog objects; the physical weapon
            // slot, not Equipment identity, owns its per-wave kill count.
            if(weaponIndex<0)weaponIndex=Rules.Inventory.IndexOf(weapon);
            ghostKills.TryGetValue(weaponIndex,out int count);ghostKills[weaponIndex]=++count;
            foreach(var effect in weapon.traits)if(effect.key=="effect_gain_stat_every_killed_enemies"&&effect.value>0&&count%(int)effect.value==0)
            {Rules.GainWeaponStat(effect.stat,effect.amount);FloatText(playerPos,ItemEntry.FormatStat(effect.stat,effect.amount),mint,false);}
        }
        void ApplyStarterCriticalGold(Equipment weapon,bool critical)
        {
            if(!critical||weapon?.traits==null)return;
            foreach(var trait in weapon.traits)if(trait.key=="gold_on_crit_kill"&&SpecialWeaponRules.CriticalMaterials(trait,true,true,UnityEngine.Random.value))Rules.GainWeaponCriticalMaterials(1);
        }
        void ApplyStarterHitEffects(Enemy victim,Equipment weapon,bool critical,bool killed,bool secondary,bool structure,float hitDamage,int weaponIndex=-1,BurnPayload burnPayload=null)
        {
            if(weapon==null)return;
            if(!secondary&&weapon.traits!=null)
            foreach(var trait in weapon.traits)
            {
                switch(trait.key)
                {
                    case "effect_burning":
                        break;
                    case "effect_slow_in_zone":
                        // This trait supplies text. The projectile's separate
                        // SlowHitbox applies the decaying slow in UpdateWeapons.
                        break;
                    case "effect_projectiles_on_hit":case "effect_lightning_on_hit":case "effect_slow_projectiles_on_hit":
                        for(int i=0;i<(int)trait.value;i++)
                        {
                            var target=trait.key=="effect_lightning_on_hit"?RandomStarterTarget(victim):null;
                            Vector2 direction=target!=null?(EnemyOrigin(target)-EnemyOrigin(victim)).normalized:UnityEngine.Random.insideUnitCircle.normalized;
                            StarterProjectile(EnemyOrigin(victim),direction,trait.projectile.AsEquipment(),true,victim,trait.key=="effect_slow_projectiles_on_hit");
                        }
                        break;
                    case "effect_explode":case "effect_explode_melee":case "effect_explode_custom":
                    case "gold_on_crit_kill":
                    case "effect_gain_stat_every_killed_enemies":
                        break;
                }
            }
            // Unit spawns on-hit projectiles before the weapon/projectile's
            // hit callback performs life steal and the melee-only explosion.
            if(!structure&&!resolvingWeaponExplosion&&Rules.TryLifeSteal(UnityEngine.Random.value,weapon.lifesteal+(Rules.Character?.WeaponBonus(weapon,"lifesteal")??0)/100))FloatText(playerPos,"+1 HP",mint,false);
            if(!secondary&&weapon.melee&&weapon.traits!=null)foreach(var trait in weapon.traits)
                if((trait.key=="effect_explode"||trait.key=="effect_explode_melee"||trait.key=="effect_explode_custom")&&SpecialWeaponRules.Explodes(trait,UnityEngine.Random.value))
                    SpawnReferenceExplosion(victim,weapon,trait,hitDamage>=0?hitDamage:Rules.WeaponDamage(weapon),true,weaponIndex,burnPayload);
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
                QueueSupportExplosion(mine.pos,mine.stats,SpecialWeaponRules.StructureDamage(Rules,mine.stats,Rules.Stat("explosion_damage")),mine.radius,true);
                Burst(mine.pos,18,amber);sound.Play(SoundCue.Impact,mine.pos,contextProfile:ContextSoundProfile.Explosion);Destroy(mine.obj);mines.RemoveAt(i);
            }
            foreach(var e in enemies.ToArray())if(e.burnTicks>0&&e.marker==null)
            {
                if(e.burnSpread>0)foreach(var target in enemies)if(target!=e&&target.marker==null&&target.burnTicks==0&&SpecialWeaponRules.BurnSpreadOverlaps(e.type.id,e.pos,target.type.id,target.pos))
                {ApplyBurn(target,e.burnDamage,e.burnTicks,e.burnSpread-1,e.burnStat);e.burnSpread=0;break;}
                e.burnClock-=dt;if(e.burnClock>0)continue;
                e.burnClock+=Rules.BurningInterval;e.burnTicks--;Burst(e.pos,2,amber);
                TryVulnerability(e,e.burnStat??"elemental_damage");
                Damage(enemies.IndexOf(e),e.burnDamage,-100,0,Vector2.up,null,true,true);
                if(e.burnTicks<=0)e.itemSlow=0;
            }
            foreach(var turret in starterTurrets)TickStructure(turret,dt);
        }
        void ClearStarterEffects()
        {
            ghostKills.Clear();
            foreach(var explosion in weaponExplosions)if(explosion.obj!=null)Destroy(explosion.obj);weaponExplosions.Clear();
            System.Array.Clear(meleeAttacks,0,meleeAttacks.Length);
            foreach(var t in starterTurrets)Destroy(t.obj);starterTurrets.Clear();
            foreach(var mine in mines)Destroy(mine.obj);mines.Clear();mineSpawners.Clear();
        }
        void ApplyBurn(Enemy target,float damage,int ticks,int spread,string stat="elemental_damage")
        {
            stat=string.IsNullOrEmpty(stat)?"elemental_damage":stat;
            bool fresh=target.burnTicks<=0;
            if(fresh||damage>=target.burnDamage)target.burnStat=stat;
            // Unit.set_burning resets the integer speed stat on a fresh burn
            // and at expiry; refreshing an existing burn only merges its data.
            if(fresh){target.burnClock=Rules.BurningInterval;target.itemSlow=0;}
            target.burnDamage=Mathf.Max(fresh?0:target.burnDamage,damage);
            target.burnTicks=Mathf.Max(target.burnTicks,ticks);target.burnSpread=Mathf.Max(target.burnSpread,spread);
        }
        void SpawnMine(StarterTurret spawner,Sprite image,Vector2? near=null)
        {
            image=SupportArt.Structure("landmine");
            var pos=near.HasValue?RunRules.ClampPosition(near.Value+new Vector2(UnityEngine.Random.Range(-.5f,.5f),UnityEngine.Random.Range(-.5f,.5f))):RandomPosition(playerPos,0);
            var obj=SpriteObject("Cybernetic proximity charge",image,pos,6);obj.transform.localScale=Vector3.one*ActorPresentation.SpriteLengthScale(image,.4f);
            mines.Add(new Mine{obj=obj,pos=pos,stats=spawner.stats,radius=SupportBehavior.ExplosionRadius(spawner.trait.radius,Rules.Stat("explosion_size"))});
        }
    }
}

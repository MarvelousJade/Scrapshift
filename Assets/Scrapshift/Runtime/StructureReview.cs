using System;
using UnityEngine;
namespace Scrapshift
{
    public sealed partial class ArenaGame
    {
        void CheckDeployableImplants()
        {
            var state=UnityEngine.Random.state;UnityEngine.Random.InitState(621);
            foreach(string id in new[]{"turret","turret_flame","turret_healing","turret_laser","turret_rocket","tyler","garden","landmines","wandering_bot"})
            {
                Begin();ClearCombat();Rules.Inventory.Clear();Rules.Items.Clear();Rules.Items.Add(Array.Find(catalog.data.items,i=>i.id==id));ResetStructures();
                bool passed=false;
                if(id=="landmines")
                {
                    var mine=mines[0];var target=FeelEnemy(mine.pos);playerPos=Vector2.one*9;UpdateStarterEffects(.02f);target.pos=mine.pos+Vector2.right;UpdateStarterEffects(.02f);UpdateWeapons(1f/60);passed=mines.Count==0&&target.hp<1000;
                }
                else
                {
                    var turret=starterTurrets[0];var target=FeelEnemy(turret.pos+Vector2.right*.6f);var neighbor=FeelEnemy(turret.pos+Vector2.right*1.2f);
                    if(id=="turret_healing"){Rules.Hurt(4);playerPos=turret.pos+Vector2.right;}
                    if(id=="wandering_bot"){playerPos=Vector2.zero;turret.angle=0;turret.orbitRadius=1.5f;target.pos=Vector2.right*1.5f;target.speed=3;}
                    float hp=Rules.Health;
                    for(int tick=0;tick<360;tick++){UpdateStarterEffects(1f/60);UpdateWeapons(1f/60);}
                    passed=id=="garden"?pickups.Exists(p=>p.consumable==ConsumableKind.Fruit):id=="wandering_bot"?target.contactSlow>0:id=="turret_healing"?Rules.Health>hp:id=="turret_flame"?target.burnTicks>0:id=="turret_laser"||id=="turret_rocket"?target.hp<1000&&neighbor.hp<1000:target.hp<1000;
                    if(id=="turret"||id=="turret_laser"||id=="turret_healing"||id=="tyler")passed&=CheckLiveSupportProjectilePresentation(turret.spec);
                }
                if(passed)starterReport.deployableImplantsPassed.Add(id);
                else Debug.LogError("Deployable implant fixture failed: "+id);
            }
            Begin();ClearCombat();Rules.Inventory.Clear();Rules.Items.Clear();Rules.GainWeaponStat("tree_turrets",1);
            Spawn(catalog.Enemy("tree"),Vector2.right*2);var tree=enemies[enemies.Count-1];Destroy(tree.marker);tree.marker=null;tree.obj.SetActive(true);
            Damage(enemies.IndexOf(tree),100000,-100,0,Vector2.zero);
            bool treeConverted=starterTurrets.Count==1&&starterTurrets[0].spec.kind=="turret";
            if(treeConverted){var target=FeelEnemy(starterTurrets[0].pos+Vector2.right*.6f);for(int i=0;i<120;i++){UpdateStarterEffects(1f/60);UpdateWeapons(1f/60);}treeConverted=target.hp<1000;}
            if(treeConverted)starterReport.deployableImplantsPassed.Add("pocket_factory");else Debug.LogError("Tree turret conversion fixture failed");
            UnityEngine.Random.state=state;
        }
        bool CheckLiveSupportProjectilePresentation(StructureSpec spec)
        {
            // Isolate a real projectile away from the staged targets, then use
            // the production update to check age advancement and renderer pose.
            int before=shots.Count;
            StarterProjectile(new Vector2(-4,-3),Vector2.right,spec.stats.AsEquipment(),true,structure:true,structureSpec:spec);
            if(shots.Count<=before)return false;
            var shot=shots[shots.Count-1];
            // The preceding behavior check retains randomly placed enemies.
            // Their collisions must not determine this separate sprite probe.
            var stagedEnemies=enemies.ToArray();var stagedPlayer=playerPos;
            try{enemies.Clear();playerPos=Vector2.one*8;UpdateWeapons(.04f);}
            finally{enemies.AddRange(stagedEnemies);playerPos=stagedPlayer;}
            string path=spec.kind=="laser"?"SupportProjectiles/laser1":spec.kind=="healing"?"SupportProjectiles/medical1":spec.kind=="tyler"?"WeaponReference/bullet_lightning_0002_87ab5590":"WeaponReference/frame0001_a728a067";
            Vector2 expected=spec.kind=="tyler"?new Vector2(.04f,.01f):new Vector2(.01f,-.01f);
            var renderer=shot.obj.GetComponent<SpriteRenderer>();
            Vector2 actual=(Vector2)renderer.bounds.center-shot.pos;
            bool alive=shots.Contains(shot),ageOkay=Mathf.Abs(shot.age-.04f)<.000001f,textureOkay=renderer.sprite.texture==Resources.Load<Texture2D>(path),offsetOkay=(actual-expected).sqrMagnitude<.00000001f;
            bool okay=alive&&ageOkay&&textureOkay&&offsetOkay;
            string diagnostic="Production support presentation "+spec.kind+": alive="+alive+", ageOK="+ageOkay+", textureOK="+textureOkay+", offsetOK="+offsetOkay+", hit count="+shot.hit.Count+", life="+shot.life+", position="+shot.pos+", isolated enemies="+stagedEnemies.Length+", age="+shot.age+", texture="+renderer.sprite.texture.name+", expected="+path+", rendered center offset=("+actual.x.ToString("R")+","+actual.y.ToString("R")+"), expected="+expected+", passed="+okay;
            Debug.Log(diagnostic);petReport?.diagnostics.Add(diagnostic);return okay;
        }
    }
}

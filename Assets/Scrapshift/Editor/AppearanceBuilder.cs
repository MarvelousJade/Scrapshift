using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Scrapshift.Editor
{
    public static partial class ContentBuilder
    {
        static void EnsureModularAppearance()
        {
            if (File.Exists(Root + "/ModularAppearanceV2.txt")) return;
            AssetDatabase.Refresh();
            var parts = ImportSheet("player-parts", CleanRegions("player-parts-v2"), 333,version:2);
            var worn = ImportSheet("wearables", CleanRegions("wearables-v2"), 220,version:2);
            var catalog = AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(Root + "/Definitions/ReferenceCatalog.asset");
            CleanCatalogArt(catalog);
            string[] ids = {"beanie","helmet","head_injury","goat_skull","glasses","blindfold","leather_vest","bat","hedgehog","fertilizer","claw_tree","broken_mouth","coffee","cake","gummy_berserker","cyclops_worm","decomposing_flesh","jelly","defective_steroids","injection","gambling_token","insanity","blood_leech","fresh_meat"};
            string[] slots = {"head","head","forehead","forehead","eyes","eyes","body","back","back","head","head","mouth","eyes","mouth","eyes","eyes","cheek-left","head","cheek-right","neck","cheek-left","eyes","neck","cheek-right"};
            float[] widths = {1.03f,1.06f,.25f,.45f,.66f,.7f,.62f,1.5f,1.28f,.46f,.8f,.32f,.57f,.38f,.61f,.4f,.24f,.62f,.27f,.19f,.16f,.6f,.16f,.24f};
            float[] xs = {0,0,-.18f,0,0,0,0,0,0,0,0,0,0,0,0,0,-.29f,0,.29f,.25f,-.29f,0,-.25f,.29f};
            float[] ys = {.47f,.46f,.35f,.4f,.1f,.1f,-.34f,.02f,.04f,.56f,.61f,-.19f,.1f,-.21f,.12f,.1f,-.12f,.49f,-.1f,-.34f,-.09f,.1f,-.34f,-.1f};
            for(int i=0;i<ids.Length;i++)
            {
                var item=catalog.data.items.Single(a=>a.id==ids[i]);
                int order=slots[i]=="back"?8:slots[i]=="eyes"?13:slots[i]=="mouth"?14:12;
                // Preserve reference precedence where it maps to our adapted feature slots.
                int priority=ids[i]=="helmet"||ids[i]=="cyclops_worm"?1:
                    ids[i]=="glasses"||ids[i]=="blindfold"||ids[i]=="leather_vest"||ids[i]=="bat"||ids[i]=="gambling_token"||ids[i]=="blood_leech"?2:0;
                item.appearance=Layer(ids[i],slots[i],worn[i],widths[i],xs[i],ys[i],order,priority);
            }
            EditorUtility.SetDirty(catalog);
            string path=Root+"/Prefabs/ScavengerAnimated.prefab";
            var root=PrefabUtility.LoadPrefabContents(path);
            var animator=root.GetComponent<SpriteFrameAnimator>();
            animator.bodyView.sprite=parts[0];animator.footSprite=parts[1];
            animator.idleFrames=new[]{parts[0]};animator.moveFrames=new[]{parts[0]};
            animator.looks=new[]{
                new PlayerLook {name="Rookie",layers=new[]{Layer("friendly","eyes",parts[2],.53f,0,.1f,13),Layer("smile","mouth",parts[5],.23f,0,-.2f,14)}},
                new PlayerLook {name="Veteran",layers=new[]{Layer("sleepy","eyes",parts[4],.54f,0,.11f,13),Layer("beard","mouth",parts[6],.62f,0,-.24f,14),Layer("swept-hair","head",parts[7],.65f,-.02f,.48f,12)}},
                new PlayerLook {name="Punk",layers=new[]{Layer("stern","eyes",parts[3],.56f,0,.12f,13),Layer("smile","mouth",parts[5],.22f,0,-.2f,14),Layer("mohawk","head",parts[8],.42f,0,.67f,12)}}
            };
            animator.lookIndex=0;
            foreach(var leg in new[]{animator.leftFoot,animator.rightFoot})
            {leg.sprite=parts[1];leg.transform.localScale=Vector3.one*(.18f/parts[1].bounds.size.y);leg.name=leg==animator.leftFoot?"Tiny left leg":"Tiny right leg";}
            animator.Tick(Vector2.zero,0,false);
            PrefabUtility.SaveAsPrefabAsset(root,path);PrefabUtility.UnloadPrefabContents(root);
            var scene=EditorSceneManager.OpenScene(ScenePath);
            var game=scene.GetRootGameObjects().Select(o=>o.GetComponent<ArenaGame>()).First(g=>g!=null);
            game.playerPrefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if(game.titleModel!=null)Object.DestroyImmediate(game.titleModel);
            game.titleModel=(GameObject)PrefabUtility.InstantiatePrefab(game.playerPrefab);
            game.titleModel.name="Animated title scavenger";game.titleModel.transform.position=new Vector3(6,.2f,-1);game.titleModel.transform.localScale=Vector3.one*3.8f;
            EditorUtility.SetDirty(game);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            File.WriteAllText(Root+"/ModularAppearanceV2.txt","Smooth unmarked shell and tapered legs; clean faces, wearables, enemies, items, equipment and upgrade icons. Three cosmetic looks; audited alpha separators.\n");
            AssetDatabase.Refresh();
        }

        static void CleanCatalogArt(ReferenceCatalog catalog)
        {
            var enemies=ImportSheet("enemies",CleanRegions("enemies-v5"),260,version:5);
            string[] enemyIds={"baby_alien","bruiser","buffer","charger","chaser","evil_mob","fin_alien","helmet_alien","horned_bruiser","horned_charger","junkie","looter","predator","pursuer","slasher","spawner","spitter","tree"};
            for(int i=0;i<enemyIds.Length;i++)catalog.Enemy(enemyIds[i]).image=enemies[i];
            var items=ImportSheet("items",CleanRegions("items-v2"),220,version:2);
            string[] itemIds={"bat","beanie","blindfold","blood_leech","broken_mouth","cake","claw_tree","coffee","cyclops_worm","decomposing_flesh","defective_steroids","fertilizer","fresh_meat","gambling_token","glasses","goat_skull","gummy_berserker","head_injury","hedgehog","helmet","injection","insanity","jelly","leather_vest"};
            foreach(var item in catalog.data.items)item.image=items[System.Array.IndexOf(itemIds,item.id)];
            var gear=ImportSheet("equipment",CleanRegions("equipment-v2"),220,true,true,version:2);
            string[] weaponIds={"pistol","smg","double_barrel_shotgun","laser_gun","knife","spear"};
            foreach(var weapon in catalog.data.equipment)weapon.image=gear[System.Array.IndexOf(weaponIds,weapon.id)];
            catalog.scrapShapes=new[]{gear[6],gear[7],gear[8],gear[10],gear[11]};catalog.repairPickup=gear[12];
            catalog.muzzleFrames=gear.Skip(13).Take(3).ToArray();catalog.warning=gear[16];catalog.shadow=gear[17];
            var stats=ImportSheet("stats",CleanRegions("stats-v2"),320,version:2);
            string[] statIds={"percent_damage","attack_speed","max_hp","crit_chance","armor","hp_regeneration","harvesting","speed"};
            foreach(var upgrade in catalog.data.upgrades)upgrade.image=stats[System.Array.IndexOf(statIds,upgrade.stat)];
        }

        static Rect[] Regions(int[] xs,int[] ys)
        {
            var result=new Rect[(xs.Length-1)*(ys.Length-1)];
            for(int y=0;y<ys.Length-1;y++)for(int x=0;x<xs.Length-1;x++)result[y*(xs.Length-1)+x]=Rect.MinMaxRect(xs[x],ys[y],xs[x+1],ys[y+1]);
            return result;
        }
        static AppearanceLayer Layer(string id,string slot,Sprite sprite,float width,float x,float y,int order,int priority=0)
            => new AppearanceLayer {id=id,slot=slot,image=sprite,scale=width/sprite.bounds.size.x,offset=new Vector2(x,y),order=order,priority=priority};
    }
}

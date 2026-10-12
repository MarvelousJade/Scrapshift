using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Scrapshift.Editor
{
    public static partial class ContentBuilder
    {
        static void EnsureStarterProfiles()
        {
            var catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(Root+"/Definitions/ReferenceCatalog.asset");
            if(catalog.starterVersion=="7"&&catalog.data.characters?.Length==4&&catalog.data.equipment.Length==205)return;
            AssetDatabase.Refresh();
            var data=JsonUtility.FromJson<StarterData>(File.ReadAllText(Root+"/Reference/starter-profiles.json"));
            // Generated art is not a mechanically aligned grid: slice in the actual transparent gutters.
            int[] rows={0,260,500,740,1024};
            int[][] columns={new[]{0,258,503,780,1016,1282,1536},new[]{0,258,507,770,1027,1282,1536},
                new[]{0,259,520,749,1023,1279,1536},new[]{0,249,523,759,1006,1289,1536}};
            var regions=new Rect[24];
            for(int y=0;y<4;y++)for(int x=0;x<6;x++)regions[y*6+x]=Rect.MinMaxRect(columns[y][x],rows[y],columns[y][x+1],rows[y+1]);
            var art=ImportSheet("starter-weapons",regions,220);
            string[] ids={"stick","fist","cactus_mace","ghost_flint","wrench","slingshot","taser","wand","icicle","hand","dagger","scissors","lightning_shiv","sharp_tooth","shuriken","revolver","shredder","crossbow","chopper","fighting_stick","hatchet","spiky_shield","rail_gun","claw"};
            int[][] baseColumns={new[]{0,214,400,588,770,966,1150,1355,1536},new[]{0,218,400,574,756,973,1152,1344,1536},new[]{0,217,403,582,768,957,1151,1365,1536},new[]{0,220,395,589,780,970,1156,1330,1536}};int[] baseRows={0,280,520,745,1024};
            var baseRegions=new Rect[32];for(int row=0;row<4;row++)for(int col=0;col<8;col++)baseRegions[row*8+col]=Rect.MinMaxRect(baseColumns[row][col],baseRows[row],baseColumns[row][col+1],baseRows[row+1]);
            // Exclude the neighboring gun stock/outline without changing the source artwork.
            baseRegions[26]=Rect.MinMaxRect(395,745,574,1024);
            baseRegions[12]=Rect.MinMaxRect(756,280,968,520);
            var baseArt=ImportSheet("base-weapons",baseRegions,220);
            string[] baseIds={"chain_gun","circular_saw","dextroyer","drill","excalibur","fireball","flamethrower","flaming_knuckles","gatling_laser","ghost_axe","ghost_scepter","hammer","jousting_lance","medical_gun","minigun","nuclear_launcher","obliterator","particle_accelerator","plank","plasma_sledgehammer","power_fist","potato_thrower","pruner","rock","rocket_launcher","screwdriver","scythe","sniper_gun","sword","thunder_sword","torch","vorpal_sword"};
            foreach(var w in data.equipment)
            {
                int index=Array.IndexOf(ids,w.id);
                int baseIndex=Array.IndexOf(baseIds,w.id);
                w.image=index>=0?art[index]:baseIndex>=0?baseArt[baseIndex]:catalog.Weapon(w.id,w.tier).image;
                w.sounds=w.soundNames.Select(n=>(AssetDatabase.LoadAssetAtPath<AudioClip>(Root+"/Reference/"+n+".wav")??AssetDatabase.LoadAssetAtPath<AudioClip>(Root+"/Reference/"+n+".mp3"))).ToArray();
                if(w.sounds.Any(c=>c==null))throw new Exception("Missing audio: "+w.id);
            }
            foreach(string path in Directory.GetFiles(Root+"/Reference","starter_*").Where(p=>p.EndsWith(".wav")||p.EndsWith(".mp3")))
            {
                var importer=(AudioImporter)AssetImporter.GetAtPath(path.Replace('\\','/'));
                importer.forceToMono=true;var settings=importer.defaultSampleSettings;
                settings.preloadAudioData=true;settings.loadType=AudioClipLoadType.DecompressOnLoad;settings.compressionFormat=AudioCompressionFormat.PCM;
                importer.defaultSampleSettings=settings;importer.SaveAndReimport();
            }
            catalog.data.equipment=data.equipment;catalog.data.characters=data.characters;catalog.data.sets=data.sets;catalog.data.rosterSlots=data.rosterSlots;
            catalog.starterVersion="7";WeaponSets.Configure(data.sets);EditorUtility.SetDirty(catalog);
            string prefabPath=Root+"/Prefabs/ScavengerAnimated.prefab";
            var root=PrefabUtility.LoadPrefabContents(prefabPath);var animator=root.GetComponent<SpriteFrameAnimator>();
            var old=animator.looks;
            if(old.Length==3)animator.looks=new[]{new PlayerLook{name="Rookie",layers=old[0].layers},
                new PlayerLook{name="Pugilist",layers=old[2].layers.Where(l=>l.slot!="head").ToArray()},
                new PlayerLook{name="Hothead",layers=old[2].layers},new PlayerLook{name="Sharpshot",layers=old[1].layers}};
            PrefabUtility.SaveAsPrefabAsset(root,prefabPath);PrefabUtility.UnloadPrefabContents(root);
            PlayerSettings.fullScreenMode=FullScreenMode.FullScreenWindow;
            AssetDatabase.SaveAssets();
        }
    }
}

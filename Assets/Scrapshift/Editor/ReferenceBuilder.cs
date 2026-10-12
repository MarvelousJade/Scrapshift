using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace Scrapshift.Editor
{
    public static partial class ContentBuilder
    {
        static void EnsureReference()
        {
            string folder=Root+"/Reference",path=Root+"/Definitions/ReferenceCatalog.asset";
            if(File.Exists(Root+"/ReferenceV2.txt"))return;
            AssetDatabase.Refresh();
            foreach(string file in Directory.GetFiles(folder,"*.png"))
            {
                var importer=(TextureImporter)AssetImporter.GetAtPath(file.Replace('\\','/'));
                importer.textureType=TextureImporterType.Sprite;
                importer.spriteImportMode=SpriteImportMode.Single;
                importer.spritePixelsPerUnit=100;
                importer.alphaIsTransparency=true;
                importer.mipmapEnabled=false;
                importer.textureCompression=TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
            foreach(string file in Directory.GetFiles(folder,"*.wav"))
            {
                var importer=(AudioImporter)AssetImporter.GetAtPath(file.Replace('\\','/'));
                importer.forceToMono=true;
                var settings=importer.defaultSampleSettings;
                settings.preloadAudioData=true;
                settings.loadType=AudioClipLoadType.DecompressOnLoad;
                settings.compressionFormat=AudioCompressionFormat.PCM;
                importer.defaultSampleSettings=settings;
                importer.SaveAndReimport();
            }
            var catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(path);
            if(catalog==null)
            {
                catalog=ScriptableObject.CreateInstance<ReferenceCatalog>();
                AssetDatabase.CreateAsset(catalog,path);
            }
            catalog.data=JsonUtility.FromJson<CatalogData>(File.ReadAllText(folder+"/catalog.json"));
            Func<string,Sprite> sprite=name=>AssetDatabase.LoadAssetAtPath<Sprite>(folder+"/"+name+".png");
            foreach(var e in catalog.data.enemies)e.image=sprite(e.sprite);
            foreach(var w in catalog.data.equipment)
            {
                w.image=sprite(w.icon);
                string prefix=w.melee?"Swing":w.id=="pistol"?"Fire":w.id;
                w.sounds=Directory.GetFiles(folder,prefix+"_*.wav").Select(p=>AssetDatabase.LoadAssetAtPath<AudioClip>(p.Replace('\\','/'))).ToArray();
            }
            foreach(var i in catalog.data.items)i.image=sprite(i.icon);
            foreach(var u in catalog.data.upgrades)u.image=sprite(u.icon);
            catalog.warning=sprite("spawn_warning");
            string matPath=Root+"/Materials/ReferenceSprite.mat";
            var mat=AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if(mat==null)
            {
                mat=new Material(Shader.Find("Scrapshift/ReferenceSprite"));
                AssetDatabase.CreateAsset(mat,matPath);
            }
            catalog.spriteMaterial=mat;
            EditorUtility.SetDirty(catalog);
            var bank=AssetDatabase.LoadAssetAtPath<SoundBank>(Root+"/Definitions/GameSounds.asset");
            foreach(var entry in bank.entries)
            {
                var clips=Directory.GetFiles(folder,(entry.cue==SoundCue.Hurt?SoundCue.Impact:entry.cue)+"_*.wav").OrderBy(p=>p).Select(p=>AssetDatabase.LoadAssetAtPath<AudioClip>(p.Replace('\\','/'))).ToArray();
                if(clips.Length>0)entry.variants=clips;
                entry.gain=entry.cue==SoundCue.Fire?.2f:entry.cue==SoundCue.Pickup?.18f:.35f;
            }
            EditorUtility.SetDirty(bank);
            var scene=EditorSceneManager.OpenScene(ScenePath);
            foreach(var root in scene.GetRootGameObjects())
            {
                var game=root.GetComponent<ArenaGame>();
                if(game!=null)
                {
                    game.catalog=catalog;
                    EditorUtility.SetDirty(game);
                }
            }
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            File.WriteAllText(Root+"/ReferenceV2.txt","Crash Zone reference catalog integration v1\n");
            AssetDatabase.Refresh();
        }
    }
}

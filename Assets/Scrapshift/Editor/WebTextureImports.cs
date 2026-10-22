using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

namespace Scrapshift.Editor
{
    // Temporary desktop-Web compression only. Authored pixels, sprite metadata,
    // readability and non-Web platform settings are left intact.
    public sealed class WebTextureImports : IDisposable
    {
        const string Art="Assets/Scrapshift/Content/OriginalArt/";
        [Serializable] sealed class ImportState
        {
            public string platformSettings,importer,format;
            public int width,height;
            public bool readable;
        }
        [Serializable] sealed class Entry
        {
            public string path,sourceSha256Before,sourceSha256During,sourceSha256After,error,skipReason;
            public ImportState before,during,after;
            public bool attempted,skipped,restored,sourceUnchanged,dimensionsUnchanged,readabilityUnchanged;
            [NonSerialized] public TextureImporterPlatformSettings original;
        }
        [Serializable] sealed class Receipt
        {
            public string scope="Build-scoped WebGL DXT5, quality100, unchanged dimensions/readability. Desktop compression candidate; browser format/visual verification is separate.",status,error;
            public bool restorationPassed;
            public Entry[] entries;
        }
        readonly string receiptPath;
        readonly List<Entry> entries=new List<Entry>();
        bool restored;
        string failure;
        WebTextureImports(string path){receiptPath=Path.GetFullPath(path);Directory.CreateDirectory(Path.GetDirectoryName(receiptPath));}
        static string Hash(string path)
        {using(var sha=SHA256.Create())using(var stream=File.OpenRead(path))return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-","").ToLowerInvariant();}
        static IEnumerable<string> Paths()
        {
            for(int i=1;i<=7;i++)yield return Art+"ItemIcons/items-"+i.ToString("00")+".png";
            for(int i=1;i<=9;i++)yield return Art+"FittedWearables/wearables-"+i.ToString("00")+".png";
            foreach(string id in new[]{"beanie","leather_vest","glasses","coffee","blindfold"})yield return Art+"TemplateWearables/"+id+".png";
            yield return Art+"base-weapons-v1.png";yield return Art+"starter-weapons-v1.png";
        }
        static ImportState State(string path,TextureImporter importer)
        {
            var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if(texture==null)throw new InvalidOperationException("Missing imported texture: "+path);
            return new ImportState{platformSettings=JsonUtility.ToJson(importer.GetPlatformTextureSettings("WebGL")),
                importer=EditorJsonUtility.ToJson(importer),width=texture.width,height=texture.height,format=texture.format.ToString(),readable=importer.isReadable};
        }
        public static WebTextureImports Begin(string receiptPath)
        {
            var scope=new WebTextureImports(receiptPath);
            try{scope.Prepare();return scope;}
            catch(Exception preparation)
            {
                scope.failure=preparation.ToString();
                try{scope.Restore();}catch(Exception restoration){throw new AggregateException("Web texture preparation and restoration failed",preparation,restoration);}
                throw;
            }
        }
        void Prepare()
        {
            foreach(string path in Paths())
            {
                var importer=AssetImporter.GetAtPath(path) as TextureImporter;
                if(importer==null)throw new InvalidOperationException("Missing texture importer: "+path);
                var entry=new Entry{path=path,sourceSha256Before=Hash(path),before=State(path,importer),original=importer.GetPlatformTextureSettings("WebGL")};
                // Register before mutation so an import exception cannot orphan settings.
                entries.Add(entry);Write("Preparing");
                var requested=new TextureImporterPlatformSettings();entry.original.CopyTo(requested);
                requested.name="WebGL";requested.overridden=true;requested.format=TextureImporterFormat.DXT5;
                requested.textureCompression=TextureImporterCompression.CompressedHQ;requested.compressionQuality=100;requested.crunchedCompression=false;
                // Block compression must never rescale approved art or sprite coordinates.
                // Reject before mutation, including oversized and non-block-aligned art.
                bool exceedsMaximum=entry.before.width>requested.maxTextureSize||entry.before.height>requested.maxTextureSize;
                bool unaligned=entry.before.width%4!=0||entry.before.height%4!=0;
                if(exceedsMaximum||unaligned)
                {
                    entry.skipped=true;
                    entry.skipReason=exceedsMaximum?"Imported dimensions exceed preserved WebGL maxTextureSize":"Imported dimensions are not multiples of the DXT5 4x4 block";
                    entry.during=entry.before;entry.sourceSha256During=entry.sourceSha256Before;
                    entry.dimensionsUnchanged=entry.readabilityUnchanged=true;
                    Write("Preparing");continue;
                }
                entry.attempted=true;importer.SetPlatformTextureSettings(requested);importer.SaveAndReimport();
                entry.during=State(path,importer);entry.sourceSha256During=Hash(path);
                entry.dimensionsUnchanged=entry.before.width==entry.during.width&&entry.before.height==entry.during.height;
                entry.readabilityUnchanged=entry.before.readable==entry.during.readable;
                if(!entry.dimensionsUnchanged||!entry.readabilityUnchanged||entry.sourceSha256Before!=entry.sourceSha256During)
                    throw new InvalidOperationException("Web texture preparation changed dimensions, readability or source pixels: "+path);
            }
            Write("Prepared");
        }
        public void CaptureDuring()
        {
            if(restored)throw new InvalidOperationException("Web texture scope was already restored");
            foreach(var entry in entries)
            {
                var importer=AssetImporter.GetAtPath(entry.path) as TextureImporter;
                if(importer==null)throw new InvalidOperationException("Texture importer disappeared: "+entry.path);
                entry.during=State(entry.path,importer);entry.sourceSha256During=Hash(entry.path);
            }
            Write("BuildCaptured");
        }
        public void Restore()
        {
            if(restored)return;
            var errors=new List<Exception>();
            foreach(var entry in entries)
            {
                try
                {
                    var importer=AssetImporter.GetAtPath(entry.path) as TextureImporter;
                    if(importer==null)throw new InvalidOperationException("Texture importer disappeared: "+entry.path);
                    if(entry.attempted)
                    {
                        importer.SetPlatformTextureSettings(entry.original);
                        // Restore the complete captured state too, including whether a
                        // platform record existed, while leaving the source PNG alone.
                        EditorJsonUtility.FromJsonOverwrite(entry.before.importer,importer);importer.SaveAndReimport();
                    }
                    entry.after=State(entry.path,importer);entry.sourceSha256After=Hash(entry.path);
                    entry.restored=entry.before.importer==entry.after.importer&&entry.before.platformSettings==entry.after.platformSettings;
                    entry.sourceUnchanged=entry.sourceSha256Before==entry.sourceSha256After;
                    if(!entry.restored||!entry.sourceUnchanged)throw new InvalidOperationException("Web texture restoration verification failed: "+entry.path);
                }
                catch(Exception ex){entry.error=ex.ToString();errors.Add(ex);}
            }
            restored=true;
            if(errors.Count>0)failure=(failure??"")+"\n"+new AggregateException(errors);
            Write(errors.Count==0?"Restored":"RestorationFailed");
            if(errors.Count>0)throw new AggregateException("Web texture restoration failed; see "+receiptPath,errors);
        }
        void Write(string status)
        {
            File.WriteAllText(receiptPath,JsonUtility.ToJson(new Receipt{status=status,error=failure,
                restorationPassed=restored&&entries.All(e=>e.restored&&e.sourceUnchanged),entries=entries.ToArray()},true));
        }
        public void Dispose()=>Restore();
    }
}

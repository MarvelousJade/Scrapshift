using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
namespace Scrapshift.Editor
{
    public sealed class CyberSoundBuildGuard : IPreprocessBuildWithReport
    {
        public int callbackOrder=>0;
        public void OnPreprocessBuild(BuildReport report)=>CyberSoundBuilder.Validate();
    }
    public static class CyberSoundBuilder
    {
        const string Folder="Assets/Scrapshift/Content/ContextAudioV1";
        const string Library="Assets/Scrapshift/Resources/CyberSoundLibrary.asset";
        [Serializable] sealed class SupportCatalog {public OriginalSupportAudio.Entry[] entries;}
        public static List<CyberSoundLibrary.Entry> Sources()
        {
            var result=new List<CyberSoundLibrary.Entry>();
            void Add(AudioClip clip,int profile)
            {
                if(clip==null)throw new Exception("Missing source audio for cyber profile "+profile);
                if(!result.Any(e=>e.source==clip&&e.profile==profile))result.Add(new CyberSoundLibrary.Entry{source=clip,profile=profile});
            }
            var bank=AssetDatabase.LoadAssetAtPath<SoundBank>(ContentBuilder.Root+"/Definitions/GameSounds.asset");
            foreach(var entry in bank.entries)
            {
                int profile=CyberSoundLibrary.Profile(entry.cue);if(profile<0)continue;
                foreach(var clip in entry.variants)Add(clip,profile);
            }
            var catalog=AssetDatabase.LoadAssetAtPath<ReferenceCatalog>(ContentBuilder.Root+"/Definitions/ReferenceCatalog.asset");
            foreach(var weapon in catalog.data.equipment)
                foreach(var clip in weapon.sounds??Array.Empty<AudioClip>())Add(clip,ContextSoundProfile.Weapon(weapon.id,weapon.melee));
            foreach(var clip in WeaponReference.ImpactClips(false,false))Add(clip,1);
            foreach(var clip in WeaponReference.ImpactClips(true,false))Add(clip,CyberSoundLibrary.Profile(SoundCue.Critical));
            foreach(var clip in WeaponReference.ImpactClips(false,true))Add(clip,ContextSoundProfile.Burn);
            foreach(var clip in bank.Find(SoundCue.Impact).variants)Add(clip,ContextSoundProfile.Explosion);
            foreach(var profile in WeaponReference.Profiles)
                foreach(var explosion in profile.explosions??Array.Empty<ExplosionReference>())
                    foreach(var path in explosion.clips??Array.Empty<string>())Add(Resources.Load<AudioClip>(path),ContextSoundProfile.Explosion);
            var support=JsonUtility.FromJson<SupportCatalog>(Resources.Load<TextAsset>("original-support-audio").text);
            // Each support role retains its source identity with a role-specific treatment.
            foreach(var entry in support.entries)
                foreach(var path in entry.clips)Add(Resources.Load<AudioClip>(path),ContextSoundProfile.Support(entry.key));
            Add(Resources.Load<AudioClip>("OriginalSupportAudio/scapegoat_heal_loop"),ContextSoundProfile.HealLoop);
            return result;
        }
        // Same tone equations, 1.45 layer strength, reflections and peak ceiling as approved tune_v3.py.
        public static float[] Process(float[] source,int rate,int channels,int profile)
        {
            int frames=source.Length/channels+Mathf.RoundToInt(.14f*rate);
            var output=new float[frames*channels];
            double peak=0,before=0,after=0;
            foreach(float value in source){peak=Math.Max(peak,Math.Abs(value));before+=value*value;}
            int delay1=Mathf.RoundToInt(.026f*rate)*channels,delay2=Mathf.RoundToInt(.052f*rate)*channels;
            int fade=Mathf.RoundToInt(.012f*rate);
            for(int i=0;i<frames;i++)
            {
                double t=(double)i/rate,layer;
                switch(profile)
                {
                    case 0:layer=.29*Tone(t,155,24,650)+.065*Tone(t,1100,34,300)+.045*Tone(t,78,19,70);break;
                    case 1:layer=.29*Tone(t,95,29,140)+.075*Tone(t,720,34,200)+.035*Tone(t,1017,43,70);break;
                    case 2:
                        double a=Math.Max(0,t-.045);
                        layer=.21*Tone(t,880,25,100)+.13*Math.Sin(2*Math.PI*1320*a)*Math.Exp(-28*a)*(1-Math.Exp(-a/.004));break;
                    case 3:layer=.24*(Tone(t,240,13,350)+.30*Tone(t,480,17,700))*(1-Math.Exp(-t/.018));break;
                    default:layer=0;break;
                }
                for(int c=0;c<channels;c++)
                {
                    int j=i*channels+c;
                    double value=(j<source.Length?source[j]:0)+1.45*layer*peak;
                    // End-run cues retain their own pitches/phrasing: derive the electronic layer
                    // from the supplied recording rather than adding an unrelated melody.
                    if(profile>=4)
                    {
                        double delay=(profile==4?.012:.019)+.003*Math.Sin(2*Math.PI*(profile==4?1.8:1.1)*t);
                        int frame=i-(int)(delay*rate),sample=frame*channels+c;
                        if(frame>=0&&sample<source.Length)
                            value+=.29*source[sample]*(1+.35*Math.Sin(2*Math.PI*(profile==4?38:23)*t))*Math.Min(1,t/.012);
                    }
                    if(j>=delay1&&j-delay1<source.Length)value+=.085*source[j-delay1];
                    if(j>=delay2&&j-delay2<source.Length)value+=.032*source[j-delay2];
                    if(i>=frames-fade)value*=Math.Max(0,(double)(frames-1-i)/(fade-1));
                    output[j]=(float)value;after+=value*value;
                }
            }
            double gain=after>0?Math.Sqrt(before/after):1,max=peak;
            foreach(float value in output)max=Math.Max(max,Math.Abs(value*gain));
            gain*=max>.94?.94/max:1;
            for(int i=0;i<output.Length;i++)output[i]*=(float)gain;
            return output;
        }
        static double Tone(double t,double frequency,double decay,double sweep)=>Math.Sin(2*Math.PI*(frequency*t+sweep*(1-Math.Exp(-35*t))/35))*Math.Exp(-decay*t)*(1-Math.Exp(-t/.003));
        [MenuItem("Scrapshift/Generate approved cyberpunk audio")]
        public static void Generate()
        {
            Directory.CreateDirectory(Folder);
            var entries=Sources();var manifest=new List<string>();
            foreach(var e in entries)
            {
                var source=e.source;source.LoadAudioData();
                var samples=new float[source.samples*source.channels];
                if(!source.GetData(samples,0))throw new Exception("Cannot decode source: "+source.name);
                var processed=e.profile<100?Process(samples,source.frequency,source.channels,e.profile):ContextSoundDesign.Process(samples,source.frequency,source.channels,e.profile);
                processed=ContextSoundDesign.Approved(source,e.profile,processed);
                string guid=AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(source));
                string path=Folder+"/"+guid+"-"+e.profile+".wav";
                using(var writer=new BinaryWriter(File.Create(path)))
                {
                    int bytes=processed.Length*2;
                    writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));writer.Write(36+bytes);writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));writer.Write(16);writer.Write((short)1);writer.Write((short)source.channels);writer.Write(source.frequency);writer.Write(source.frequency*source.channels*2);writer.Write((short)(source.channels*2));writer.Write((short)16);writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));writer.Write(bytes);
                    foreach(float v in processed){if(float.IsNaN(v)||Mathf.Abs(v)>.9401f)throw new Exception("Invalid processed sample");writer.Write((short)Math.Round(v*32767));}
                }
                AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
                var importer=(AudioImporter)AssetImporter.GetAtPath(path);
                importer.forceToMono=false; // Source channel layout is already baked; avoid mono normalization.
                var settings=importer.defaultSampleSettings;settings.loadType=AudioClipLoadType.DecompressOnLoad;settings.compressionFormat=AudioCompressionFormat.PCM;settings.preloadAudioData=true;importer.defaultSampleSettings=settings;importer.SaveAndReimport();
                e.edited=AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                manifest.Add(e.profile+"\t"+AssetDatabase.GetAssetPath(source)+"\t"+path);
            }
            var library=AssetDatabase.LoadAssetAtPath<CyberSoundLibrary>(Library);
            if(library==null){library=ScriptableObject.CreateInstance<CyberSoundLibrary>();AssetDatabase.CreateAsset(library,Library);}
            library.SetEntries(entries.ToArray());EditorUtility.SetDirty(library);AssetDatabase.SaveAssets();
            File.WriteAllLines(Folder+"/provenance.tsv",new[]{"profile\tsupplied_source_asset\tderived_context_asset"}.Concat(manifest));
            Validate();Debug.Log("Context audio baked and validated: "+entries.Count+" source/profile pairs.");
        }
        public static void Validate()
        {
            var library=AssetDatabase.LoadAssetAtPath<CyberSoundLibrary>(Library);
            if(library==null)throw new BuildFailedException("Generate approved cyberpunk audio before building.");
            foreach(var expected in Sources())
            {
                var e=library.entries.SingleOrDefault(x=>x.source==expected.source&&x.profile==expected.profile);
                if(e?.edited==null||e.edited.length<=0)throw new BuildFailedException("Missing context audio: "+expected.source.name);
            }
            var bank=AssetDatabase.LoadAssetAtPath<SoundBank>(ContentBuilder.Root+"/Definitions/GameSounds.asset");
            foreach(var entry in bank.entries.Where(e=>CyberSoundLibrary.Profile(e.cue)<0))
                foreach(var clip in entry.variants)if(library.Resolve(clip,entry.cue)!=clip)throw new BuildFailedException("Protected cue altered: "+entry.cue);
        }
    }
}

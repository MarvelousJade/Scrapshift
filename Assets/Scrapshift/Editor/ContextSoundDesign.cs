using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace Scrapshift.Editor
{
    // Offline, deterministic rendering. Runtime only selects baked recordings.
    public static class ContextSoundDesign
    {
        public static float[] Approved(AudioClip source,int profile,float[] fallback)
        {
            string guid=AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(source)),name=null;
            if(profile==100&&guid=="f225e25b85602e342afc76bf8751ff40")name="laser_gun";
            if(profile==101&&guid=="0dd2eb80798020f478f63cbc96402876")name="rail_gun";
            if(profile==102&&guid=="0fd38ae28e09f124d8830e524cd36e00")name="rocket_launcher";
            if(profile==103&&guid=="4b5c8b0af9891d84bae2afc5ceff403d")name="lightning_shiv";
            if(name==null)return fallback;
            // Canonical previews are generated PCM16 WAVs. Read the PCM directly so
            // importer compression cannot alter an already-approved waveform.
            byte[] wav=File.ReadAllBytes("Assets/Scrapshift/Content/ApprovedContextAudio/"+name+".wav");
            if(BitConverter.ToInt16(wav,20)!=1||BitConverter.ToInt16(wav,22)!=source.channels||BitConverter.ToInt32(wav,24)!=source.frequency||BitConverter.ToInt16(wav,34)!=16||System.Text.Encoding.ASCII.GetString(wav,36,4)!="data")throw new Exception("Invalid canonical preview: "+name);
            int bytes=BitConverter.ToInt32(wav,40);if(bytes!=wav.Length-44)throw new Exception("Invalid preview length");
            var data=new float[bytes/2];for(int i=0;i<data.Length;i++)data[i]=BitConverter.ToInt16(wav,44+i*2)/32768f;
            // Preserve the approved waveform; restore source energy within safe peak headroom.
            var original=new float[source.samples*source.channels];source.GetData(original,0);Normalize(data,original);
            return data;
        }
        static double Tone(double t,double f,double decay,double sweep=0)=>Math.Sin(2*Math.PI*(f*t+sweep*(1-Math.Exp(-35*t))/35))*Math.Exp(-decay*t);
        public static void Normalize(float[] output,float[] source)
        {
            double before=0,after=0,peak=0;
            foreach(var v in source)before+=v*v;
            foreach(var v in output){after+=v*v;peak=Math.Max(peak,Math.Abs(v));}
            double gain=after>0?Math.Sqrt(before/after):1;
            if(peak*gain>.92)gain=.92/peak;
            for(int i=0;i<output.Length;i++)output[i]*=(float)gain;
        }
        public static float[] Process(float[] source,int rate,int channels,int profile)
        {
            if(profile==ContextSoundProfile.HealLoop)
            {
                var loop=(float[])source.Clone();
                // Integer cycles and unchanged length preserve the supplied loop boundary.
                int count=source.Length/channels,cycles=Math.Max(1,(int)(31.0*count/rate));
                for(int i=0;i<count;i++)for(int c=0;c<channels;c++)loop[i*channels+c]*=(float)(1+.045*Math.Sin(2*Math.PI*cycles*i/count));
                Normalize(loop,source);return loop;
            }
            int frames=source.Length/channels+(int)(.14*rate);
            var output=new float[frames*channels];var random=new System.Random(151026+profile);
            double peak=0;foreach(var v in source)peak=Math.Max(peak,Math.Abs(v));
            double filtered=0;
            for(int i=0;i<frames;i++)
            {
                double t=(double)i/rate,noise=random.NextDouble()*2-1;
                filtered+= (1-Math.Exp(-2*Math.PI*1800/rate))*(noise-filtered);
                double high=noise-filtered,layer=0,dry=1;
                switch(profile)
                {
                    case ContextSoundProfile.Laser: dry=.55*Math.Exp(-t/.3);layer=.30*Tone(t,520,11,1450)+.09*Tone(t,1050,7,160)+.08*high*Math.Exp(-t/0.007);break;
                    case ContextSoundProfile.Rail: dry=.7;layer=.36*Tone(t,57,16,185)+.19*high*Math.Exp(-t/.016)+.10*Tone(t,240,22,1700)+.04*(Tone(t,640,10)+Tone(t,947,10));break;
                    case ContextSoundProfile.Rocket: dry=.55*Math.Exp(-t/.2);layer=.70*filtered*(1-Math.Exp(-t/.025))*Math.Exp(-t/.14)+.34*Tone(t,48,14,120);break;
                    case ContextSoundProfile.Electric:
                        dry=.7;double arcs=0;foreach(double at in new[]{.018,.047,.081,.123})if(t>=at)arcs+=Math.Exp(-(t-at)/.009);
                        layer=.22*high*arcs+.08*Math.Sin(2*Math.PI*(1300*t+2*Math.Sin(2*Math.PI*73*t)))*arcs;break;
                    case ContextSoundProfile.Flame:layer=.4*filtered*(1-Math.Exp(-t/.012))*Math.Exp(-t/.14)+.07*Tone(t,85,20);break;
                    case ContextSoundProfile.Blade:layer=.12*high*Math.Exp(-Math.Pow((t-.045)/.03,2))+.055*Tone(t,1600,45,300);break;
                    case ContextSoundProfile.Heavy:layer=.26*Tone(t,75,25,75)+.08*filtered*Math.Exp(-t/.035);break;
                    case ContextSoundProfile.Ballistic:layer=.16*Tone(t,110,35,180)+.07*high*Math.Exp(-t/.01);break;
                    case ContextSoundProfile.Rapid:layer=.12*Tone(t,135,65,220)+.05*high*Math.Exp(-t/.008);break;
                    case ContextSoundProfile.Bow:layer=.12*Tone(t,330,38,400)+.06*high*Math.Exp(-t/.035);break;
                    case ContextSoundProfile.Organic:layer=.17*Tone(t,180,24,260)+.09*filtered*Math.Exp(-t/.07);break;
                    case ContextSoundProfile.Ethereal:layer=.09*(Tone(t,480,9,80)+Tone(t,721,12,120))*(1-Math.Exp(-t/.012));break;
                    case ContextSoundProfile.Motor:layer=.12*(Tone(t,160,9)+.35*Tone(t,480,10))*(.7+.3*Math.Sin(2*Math.PI*83*t));break;
                    case ContextSoundProfile.Heal:layer=.09*Tone(t,660,16)+.06*Tone(t,990,13)*(1-Math.Exp(-t/.025));break;
                    case ContextSoundProfile.Plasma:layer=.20*Tone(t,100,19,450)+.10*Tone(t,600,14,300);break;
                    case ContextSoundProfile.Ice:layer=.055*(Tone(t,1300,35)+Tone(t,1911,40))+.08*high*Math.Exp(-t/.025);break;
                    case ContextSoundProfile.Explosion:layer=.30*Tone(t,48,13,55)+.30*filtered*Math.Exp(-t/.11);break;
                    case ContextSoundProfile.Shield:layer=.14*Tone(t,310,18,210)+.07*Tone(t,740,24);break;
                    case ContextSoundProfile.Burn:layer=.19*high*Math.Exp(-t/.07);break;
                    // Voices get source-derived modulation below, never replacement animal tones.
                    case ContextSoundProfile.Voice:break;
                    case ContextSoundProfile.Status:layer=.055*Tone(t,740,30,120);break;
                    default:
                        var cue=(SoundCue)(profile-200);
                        switch(cue)
                        {
                            case SoundCue.Step:layer=.035*filtered*Math.Exp(-t/.018);break;
                            case SoundCue.Consume:layer=.045*Tone(t,320,40,120);break;
                            case SoundCue.UiFocus:layer=.04*Tone(t,800,85);break;
                            case SoundCue.InventoryFocus:layer=.05*Tone(t,600,70,80);break;
                            case SoundCue.UiPress:layer=.07*Tone(t,430,55,200);break;
                            case SoundCue.Reroll:layer=.08*Tone(t,520,30,450);break;
                            case SoundCue.Install:case SoundCue.LevelUp:case SoundCue.WaveClear:layer=.07*Tone(t,660,12)+.04*Tone(t,990,10);break;
                            case SoundCue.Charge:case SoundCue.Spawn:layer=.10*Tone(t,180,12,-70);break;
                            case SoundCue.Hurt:layer=.17*Tone(t,85,23,65);break;
                            case SoundCue.Critical:layer=.2*Tone(t,90,30,140)+.07*high*Math.Exp(-t/.018);break;
                            case SoundCue.Kill:layer=.13*Tone(t,110,30,70);break;
                        }
                        break;
                }
                double attack=Math.Min(1,t/.001),fade=Math.Min(1,(frames-1-i)/(.012*rate));
                for(int c=0;c<channels;c++)
                {
                    int j=i*channels+c;double value=j<source.Length?source[j]*dry:0;
                    // Light metallic modulation keeps vocal/status phrasing and UI identity intact.
                    if(profile==ContextSoundProfile.Voice||profile>=200)value*=1+.045*Math.Sin(2*Math.PI*31*t);
                    output[j]=(float)((value+peak*layer*attack)*Math.Max(0,fade));
                }
            }
            Normalize(output,source);return output;
        }
    }
}

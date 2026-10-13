"""Render the original Neon Salvage score. No sampled/reference melodies or audio.
64 bars, 146 BPM, D minor; original EDM/industrial-metal arrangement with synthesized chugs.
"""
from pathlib import Path
import json, wave
import numpy as np

RATE=44100
BEAT=60/146
N=round(64*4*BEAT*RATE)
mix=np.zeros((N,2),np.float64)
rng=np.random.default_rng(7126)
def add(signal, beat, gain=1, pan=0):
    indices=(round(beat*BEAT*RATE)+np.arange(len(signal)))%N
    mix[indices,0]+=signal*gain*np.sqrt((1-pan)/2)
    mix[indices,1]+=signal*gain*np.sqrt((1+pan)/2)
def time(seconds): return np.arange(round(seconds*RATE))/RATE
def note(midi): return 440*2**((midi-69)/12)
def synth(midi,length,kind):
    t=time(length);f=note(midi)
    if kind=='guitar':
        # Band-limited detuned power chord, hard distortion then cabinet-like roll-off.
        s=np.zeros_like(t)
        for ratio in (1,1.003,1.5,2):
            for h in range(1,19):
                s+=np.sin(2*np.pi*f*ratio*h*t)/(h*4)
        s=np.tanh(s*7)
        spectrum=np.fft.rfft(s);freq=np.fft.rfftfreq(len(s),1/RATE)
        s=np.fft.irfft(spectrum/(1+(freq/4200)**4),n=len(s))
        env=np.minimum(t/.003,1)*np.exp(-t*(14 if length<BEAT*.6 else 3))*np.minimum((length-t)/.018,1)
    elif kind=='bass':
        s=np.sin(2*np.pi*f*t)+.4*np.sin(4*np.pi*f*t)+.2*np.sin(6*np.pi*f*t)
        env=np.minimum(t/.003,1)*np.exp(-t*3)*np.minimum((length-t)/.02,1)
    else:
        s=np.sin(2*np.pi*f*t+1.5*np.sin(2*np.pi*f*2*t)*np.exp(-t*18))+.18*np.sin(2*np.pi*f*1.002*t)
        env=np.minimum(t/.008,1)*np.exp(-t*7)*np.minimum((length-t)/.035,1)
    return s*env
def kick():
    t=time(.38);return np.sin(2*np.pi*(48*t+3*(1-np.exp(-t*35))))*np.exp(-t*14)
def snare():
    t=time(.23);noise=rng.standard_normal(len(t));noise-=np.roll(noise,1)
    return (.28*noise+.5*np.sin(2*np.pi*185*t))*np.exp(-t*24)*np.minimum(t/.002,1)
def hat():
    t=time(.075);noise=rng.standard_normal(len(t));return (noise-np.roll(noise,1))*np.exp(-t*65)*np.minimum(t/.001,1)
roots=[38,38,41,36,38,38,34,36]
for bar in range(64):
    root=roots[bar%8];b=bar*4;breakdown=24<=bar<28 or 48<=bar<52
    drop=bar>=8 and not breakdown
    # Four-on-floor kicks and backbeat snare, occasional double-kick fills.
    for beat in ([0,2] if breakdown else [0,1,2,3]):add(kick(),b+beat,.72)
    if bar%8==7:
        for beat in [2.5,3.25,3.5,3.75]:add(kick(),b+beat,.42)
    for beat in [1,3]:add(snare(),b+beat,.32 if drop else .2)
    for step in range(16 if drop else 8):
        add(hat(),b+step*(.25 if drop else .5),.028 if step%2 else .065,(-1 if step%2 else 1)*.55)
    pattern=[0,0,12,0,7,0,1,0] if bar%4<2 else [0,0,3,0,5,3,1,0]
    for step,degree in enumerate(pattern):
        if breakdown and step%2:continue
        at=b+step*.5
        add(synth(root-12,BEAT*.43,'bass'),at,.27)
        if not breakdown:
            length=BEAT*(.42 if step<6 else .7)
            add(synth(root+degree,length,'guitar'),at,.16 if drop else .10,-.65)
            add(synth(root+degree+.07,length,'guitar'),at+.018,.16 if drop else .10,.65)
    if bar%16>=8 or breakdown:
        degrees=[0,7,12,10,7,3,5,1]
        for j,degree in enumerate(degrees):
            signal=synth(root+24+degree,BEAT*.65,'pluck')
            add(signal,b+j*.5,.08,.2);add(signal,b+j*.5+.75,.025,-.5)
    if bar%8==7:
        for step in range(4):add(snare(),b+3+step*.25,.10+step*.035,step*.15-.2)
    if bar in (0,8,28,32,52,56):
        t=time(1.2);noise=rng.standard_normal(len(t));add((noise-np.roll(noise,1))*np.exp(-t*5),b,.065)
# Soft saturation, DC removal, and deterministic peak headroom. Notes wrap at the loop seam.
mix-=mix.mean(axis=0)
mix=np.tanh(mix*1.15)
mix*=.82/max(np.max(np.abs(mix)),.001)
root=Path(__file__).resolve().parents[1]
out=root/'Assets/Scrapshift/Resources/Music/NeonSalvage.wav'
out.parent.mkdir(parents=True,exist_ok=True)
pcm=(mix*32767).astype('<i2')
with wave.open(str(out),'wb') as f:
    f.setnchannels(2);f.setsampwidth(2);f.setframerate(RATE);f.writeframes(pcm.tobytes())
print(json.dumps({'path':str(out),'seconds':N/RATE,'peak':float(np.abs(mix).max()),'rms':float(np.sqrt((mix*mix).mean()))}))

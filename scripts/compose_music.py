"""Render the original Neon Salvage score. No sampled/reference melodies or audio.
32 bars, 126 BPM, E minor; deterministic subtractive/FM voices and synthesized drums.
"""
from pathlib import Path
import json, wave
import numpy as np

RATE=44100
BEAT=60/126
N=round(32*4*BEAT*RATE)
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
    if kind=='bass':
        s=np.sin(2*np.pi*f*t)+.32*np.sin(4*np.pi*f*t)+.12*np.sin(6*np.pi*f*t)
        env=np.minimum(t/.006,1)*np.exp(-t*5)*np.minimum((length-t)/.025,1)
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
roots=[40,36,43,38]
for bar in range(32):
    root=roots[(bar//2)%4];b=bar*4;breakdown=16<=bar<20
    # Sustained minor/major pads, gently pulsed; progression Em / C / G / D.
    t=time(4*BEAT);major=root in (36,43,38);chord=[root+24,root+24+(4 if major else 3),root+31]
    pad=sum(np.sin(2*np.pi*note(m)*t)+.3*np.sin(2*np.pi*note(m)*1.003*t) for m in chord)
    env=np.minimum(t/.15,1)*np.minimum((4*BEAT-t)/.18,1)*(.65+.35*np.sin(np.pi*t/BEAT)**2)
    add(pad*env,b,.045)
    for step in range(8):
        if not breakdown or step%2==0:
            add(synth(root+(12 if step in (3,7) else 0),BEAT*.46,'bass'),b+step*.5,.23)
        if not breakdown:
            add(hat(),b+step*.5,.065 if step%2 else .04,(-1 if step%2 else 1)*.45)
    for beat in ([0,2] if breakdown else [0,1.5,2,3.5]):add(kick(),b+beat,.5)
    if not breakdown:
        for beat in [1,3]:add(snare(),b+beat,.24)
    if bar>=4:
        degrees=[0,7,12,10,7,3,5,7] if root==40 else [0,7,12,7,4,7,14,12]
        for j,degree in enumerate(degrees):
            if breakdown and j%2:continue
            signal=synth(root+24+degree,BEAT*.8,'pluck')
            gain=.075 if bar<24 else .10
            add(signal,b+j*.5,gain,.25)
            add(signal,b+j*.5+.75,gain*.26,-.5)
    if bar%8==7:
        for step in range(4):add(snare(),b+3+step*.25,.09+step*.02,step*.15-.2)
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

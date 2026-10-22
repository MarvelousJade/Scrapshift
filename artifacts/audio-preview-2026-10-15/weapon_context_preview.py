"""Standalone weapon proposals. Never writes to game assets."""
import base64
import hashlib
import html
import json
import wave
from pathlib import Path
import numpy as np

ROOT = Path(__file__).resolve().parent / 'weapon-context-v1'
SR = 48000
items = json.loads((ROOT / 'sources.json').read_text())
rng = np.random.default_rng(151026)

def read(path):
    with wave.open(str(path)) as w:
        assert w.getsampwidth() == 2 and w.getframerate() == SR
        return np.frombuffer(w.readframes(w.getnframes()), '<i2').astype(float).reshape(-1,w.getnchannels()).mean(axis=1)/32768

def low(x, hz):
    f = np.fft.rfftfreq(len(x), 1/SR)
    return np.fft.irfft(np.fft.rfft(x)/(1+(f/hz)**4), n=len(x))

def tone(freq):
    return np.sin(2*np.pi*np.cumsum(freq)/SR)

def write(path, x):
    assert np.isfinite(x).all() and np.max(np.abs(x)) < .96
    with wave.open(str(path), 'wb') as w:
        w.setparams((1,2,SR,0,'NONE','not compressed'))
        w.writeframes(np.round(x*32767).astype('<i2').tobytes())

def uri(path, mime):
    return 'data:'+mime+';base64,'+base64.b64encode(Path(path).read_bytes()).decode()

notes = {
 'laser_gun': ('Clean energy pulse', 'The green barrel cell gets a focused, descending energy pulse with a tiny capacitor click and a soft electronic tail.'),
 'rail_gun': ('Heavy magnetic discharge', 'The bulky powered barrel gets a low pressure punch, a sharp magnetic snap and a short metallic resonance. No wind-up delay.'),
 'rocket_launcher': ('Ignition + exhaust', 'The visible rocket and launcher tube get a mechanical launch thunk followed by rushing exhaust. This is the launch sound, not the explosion.'),
 'lightning_shiv': ('Electric slash', 'The yellow lightning-shaped blade gets a fast cutting swish with several brief electrical arcs, kept short for repeated attacks.')
}
cards=[]
report=[]
for item in items:
    key=item['id']
    before=hashlib.sha256(Path(item['current']).read_bytes()).hexdigest()
    current=read(item['current'])
    n=max(len(current), int(SR*({'laser_gun':.6,'rail_gun':.5,'rocket_launcher':.7,'lightning_shiv':.4}[key])))
    t=np.arange(n)/SR
    base=np.pad(current,(0,n-len(current)))
    noise=rng.normal(0,1,n)
    if key=='laser_gun':
        pulse=tone(520+1450*np.exp(-t/0.038)+45*np.sin(2*np.pi*38*t))
        y=.47*base*np.exp(-t/.22)+.27*pulse*np.exp(-t/.09)
        y+=.09*tone(1050+160*np.exp(-t/.07))*np.exp(-t/.14)
        y+=.08*(noise-low(noise,2200))*np.exp(-t/.007)
    elif key=='rail_gun':
        y=.64*base+.35*tone(57+185*np.exp(-t/.016))*np.exp(-t/.065)
        y+=.19*(noise-low(noise,1500))*np.exp(-t/.016)
        y+=.11*tone(240+1700*np.exp(-t/.009))*np.exp(-t/.045)
        y+=.045*(tone(np.full(n,640))+tone(np.full(n,947)))*np.exp(-t/.105)
    elif key=='rocket_launcher':
        exhaust=low(noise,1900)
        exhaust/=max(np.std(exhaust),.001)
        env=(1-np.exp(-t/.025))*np.exp(-t/.14)
        y=.55*base*np.exp(-t/.14)+.30*exhaust*env
        y+=.34*tone(48+120*np.exp(-t/.02))*np.exp(-t/.07)
        y+=.06*low(noise,500)*np.exp(-t/.24)
    else:
        y=.65*base*np.exp(-t/.22)
        arcs=np.zeros(n)
        for at in [.018,.047,.081,.123]:
            dt=np.maximum(0,t-at)
            arcs+=(t>=at)*np.exp(-dt/.009)
        y+=.14*(noise-low(noise,1600))*arcs
        y+=.08*tone(1300+650*np.sin(2*np.pi*73*t))*arcs
        y+=.07*(noise-low(noise,700))*np.exp(-((t-.07)/.045)**2)
    y-=np.mean(y)
    fade=min(480,len(y)//10)
    y[:48]*=np.linspace(0,1,48)
    y[-fade:]*=np.linspace(1,0,fade)
    # Equal energy across the same window, then common headroom scaling.
    y*=np.sqrt(np.sum(base*base)/max(np.sum(y*y),1e-12))
    gain=min(1,.92/max(np.max(np.abs(base)),np.max(np.abs(y))))
    a=base*gain
    b=y*gain
    cp=ROOT/(key+'-current.wav'); npth=ROOT/(key+'-proposed.wav')
    write(cp,a); write(npth,b)
    assert hashlib.sha256(Path(item['current']).read_bytes()).hexdigest()==before
    assert np.sqrt(np.mean((a-b)**2))>.001
    report.append(dict(weapon=key,current_asset=item['current'],current_sha256=before,preview_gain=gain,peak_current=float(max(abs(a))),peak_new=float(max(abs(b))),rms_current=float(np.sqrt(np.mean(a*a))),rms_new=float(np.sqrt(np.mean(b*b))),seconds=n/SR))
    title,desc=notes[key]
    cards.append(f'''<article><div class="identity"><img src="{uri(item['icon'],'image/png')}" alt="{html.escape(item['title'])} game artwork"><div><h2>{html.escape(item['title'])}</h2><strong>{title}</strong><p>{desc}</p></div></div><div class="players"><section><label>Current game sound</label><audio controls preload="none" src="{uri(cp,'audio/wav')}"></audio></section><section><label>New weapon-specific edit</label><audio controls preload="none" src="{uri(npth,'audio/wav')}"></audio></section></div></article>''')

(ROOT/'validation.json').write_text(json.dumps(report,indent=2))
(ROOT/'index.html').write_text('''<!doctype html><html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Weapon sound proposals · Scrapshift</title><style>
*{box-sizing:border-box}body{margin:0;background:#0d141b;color:#eaf3f5;font:16px system-ui,sans-serif}main{max-width:1020px;margin:auto;padding:36px 24px}h1{font-size:32px;margin:10px 0}h2{margin:0 0 8px;font-size:23px}.eyebrow{color:#81e7c1;letter-spacing:2px;font-size:12px;font-weight:700}p{color:#b7c7ce;line-height:1.55;max-width:780px}article{background:#17232d;border:1px solid #314450;border-radius:16px;padding:24px;margin:22px 0}.identity{display:flex;align-items:center;gap:30px}.identity img{width:225px;height:145px;object-fit:contain;flex-shrink:0}.identity strong{color:#81e7c1}.players{display:grid;grid-template-columns:1fr 1fr;gap:22px;border-top:1px solid #314450;padding-top:20px;margin-top:12px}label{display:block;font-weight:600;margin-bottom:12px}audio{width:100%;height:42px}button{background:#81e7c1;border:0;border-radius:8px;padding:12px 20px;color:#10251d;font-weight:700;cursor:pointer}.small{font-size:13px;color:#9fb2bd}@media(max-width:650px){.identity{display:block}.identity img{width:100%;height:125px;margin-bottom:18px}.players{grid-template-columns:1fr}main{padding:24px 14px}}
</style><main><div class="eyebrow">SCRAPSHIFT / AUDIO PREVIEW / 15 OCT 2026</div><h1>Sound that fits the weapon</h1><p>Four proposals based on the actual weapon names and in-game artwork. Compare the current approved cyberpunk sound with a new, more specific treatment.</p><button onclick="document.querySelectorAll('audio').forEach(a=>{a.pause();a.currentTime=0})">Stop all sounds</button><p class="small">Preview only · No game changes or build. One representative attack variant per weapon. Pairs are matched by RMS energy with shared peak headroom; perceived loudness may still differ. In-game random pitch and mixer settings are not simulated. Nothing plays automatically.</p>'''+''.join(cards)+'''<p class="small">Tell me the weapon name and which side you prefer. The original material pickup remains unchanged.</p></main><script>document.querySelectorAll('audio').forEach(a=>a.addEventListener('play',()=>document.querySelectorAll('audio').forEach(b=>{if(a!==b)b.pause()})))</script></html>''',encoding='utf-8')
print(json.dumps(report,indent=2))
print('Preview:',ROOT/'index.html')

"""Original deterministic sound design. No reference samples are read or copied.
Run with Python 3; outputs ordinary 44.1kHz PCM WAV assets for Unity/AssetScope.
"""
from pathlib import Path
import array, json, math, random, wave

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / 'Assets/Scrapshift/Content/Audio'
RATE = 44100
CUES = {
    'Fire': (.19, 4), 'Impact': (.14, 4), 'Kill': (.25, 3),
    'Hurt': (.40, 3), 'Critical': (.24, 3), 'Pickup': (.15, 4),
    'LevelUp': (.85, 1), 'Install': (.32, 2), 'Pulse': (.65, 2),
    'Swing': (.20, 3), 'Reroll': (.32, 2), 'Win': (1.50, 1),
    'Lose': (1.0, 1), 'WaveClear': (.8, 1), 'Charge': (.42, 2),
}

def write_wav(path, samples):
    pcm = array.array('h', (round(max(-1, min(1, x)) * 32767) for x in samples))
    with wave.open(str(path), 'wb') as f:
        f.setparams((1, 2, RATE, len(pcm), 'NONE', 'not compressed'))
        f.writeframes(pcm.tobytes())

def synth(cue, length, variant):
    rng = random.Random(7103 + variant * 173 + list(CUES).index(cue) * 997)
    detune = 1 + rng.uniform(-.05, .05)
    lp = 0.0
    result = []
    for i in range(round(length * RATE)):
        t = i / RATE
        n = rng.uniform(-1, 1)
        lp += .14 * (n - lp)
        high = n - lp
        def tone(freq, decay, sweep=0):
            return math.sin(2 * math.pi * detune * (freq*t + sweep * (1-math.exp(-t*35))/35)) * math.exp(-t*decay)
        if cue == 'Fire':
            # Air crack, low mechanical punch, and a short metallic bolt tail.
            v = .7*high*math.exp(-t*100) + .9*lp*math.exp(-t*27) + .50*tone(95,35,180) + .10*tone(1680,40)
        elif cue == 'Impact':
            v = .7*lp*math.exp(-t*38) + .32*tone(115,43,90) + .3*high*math.exp(-t*130)
        elif cue == 'Kill':
            v = .8*lp*math.exp(-t*23) + .5*tone(62,23,170) + .18*high*math.exp(-t*45)*(1+math.sin(t*130))
        elif cue == 'Hurt':
            v = .58*tone(64,12,170) + .5*lp*math.exp(-t*20) + .16*tone(150,14)*math.sin(t*40)
        elif cue == 'Critical':
            v = .50*high*math.exp(-t*60) + .5*tone(85,27,240) + .25*tone(1900,25) + .12*tone(2717,35)
        elif cue == 'Pickup':
            v = .30*tone(1320,32) + .18*tone(1980,40) + .1*tone(3100,60) + .20*high*math.exp(-t*220)
        elif cue == 'Pulse':
            v = .7*tone(45,8,240) + .6*lp*math.exp(-t*7) + .08*tone(650,11,500)
        elif cue == 'Swing':
            env = math.sin(math.pi*min(1,t/length))**2
            v = (.65*lp + .07*high) * env * math.exp(-t*4)
        elif cue == 'Charge':
            v = (.22*math.sin(2*math.pi*(170*t+190*t*t)) + .1*lp) * (t/length)**.6
        elif cue == 'Reroll':
            v = 0
            for start in (0, .065, .13, .21):
                a = t-start
                if a >= 0: v += (.2*high + .22*math.sin(2*math.pi*720*a))*math.exp(-a*70)
        else:
            notes = {'LevelUp': [523,659,784,1047], 'Install': [784,1047],
                     'Win': [523,659,784,1047,1319], 'Lose': [330,277,220],
                     'WaveClear': [392,523,659]}[cue]
            step = {'LevelUp': .115, 'Install': .065, 'Win': .17, 'Lose': .19, 'WaveClear': .12}[cue]
            v = 0
            for j, freq in enumerate(notes):
                a = t-j*step
                if a >= 0:
                    attack = min(1, a/.003)
                    v += attack*math.exp(-a*10)*(.24*math.sin(2*math.pi*freq*a)+.07*math.sin(2*math.pi*freq*2*a)+.035*math.sin(2*math.pi*freq*3*a))
        # Fade at both ends prevents cut clicks; remove DC before peak normalization.
        v *= min(1, t/.0015) * min(1, (length-t)/.025)
        result.append(v)
    mean = sum(result)/len(result)
    result = [(v-mean)*min(1,i/66,(len(result)-1-i)/110) for i,v in enumerate(result)]
    peak = max(abs(x) for x in result)
    return [v / max(peak, .001) * .78 for v in result]

def main():
    OUT.mkdir(parents=True, exist_ok=True)
    assets, report = {}, []
    for cue,(length,count) in CUES.items():
        for variant in range(count):
            samples = synth(cue,length,variant)
            path = OUT / f'{cue}_{variant+1:02}.wav'
            write_wav(path,samples)
            assets[(cue,variant)] = samples
            peak = max(abs(v) for v in samples)
            rms = math.sqrt(sum(v*v for v in samples)/len(samples))
            assert .01 < rms < .5 and peak < .8 and abs(samples[0]) < .001 and abs(samples[-1]) < .001
            report.append({'file':path.name,'seconds':length,'peak':round(peak,4),'rms':round(rms,4),'dc':round(sum(samples)/len(samples),6)})
    # Listening sampler: shooting/impacts, pickups, swing/pulse, hurt, upgrade, clear.
    demo = [0.0] * (RATE*12)
    events = [(0,'Fire'),(.35,'Fire'),(.7,'Fire'),(1.1,'Impact'),(1.4,'Impact'),(1.8,'Critical'),(2.15,'Kill'),
              (2.8,'Pickup'),(3,'Pickup'),(3.2,'Pickup'),(3.8,'Swing'),(4.3,'Pulse'),(5.4,'Hurt'),
              (6.2,'Reroll'),(6.9,'Install'),(7.6,'LevelUp'),(9,'WaveClear'),(10,'Win')]
    for j,(time,cue) in enumerate(events):
        for i,v in enumerate(assets[(cue,j%CUES[cue][1])]):
            at = round(time*RATE)+i
            if at<len(demo): demo[at] += v*.65
    folder=ROOT/'artifacts/audio';folder.mkdir(parents=True,exist_ok=True)
    write_wav(folder/'sound-sampler.wav',demo)
    (folder/'sample-validation.json').write_text(json.dumps({'sampleRate':RATE,'clips':report,'sampler':'Offline cue sequence, not captured gameplay audio.'},indent=2)+'\n')
    print(f'Generated and numerically checked {len(report)} original WAV clips.')

if __name__ == '__main__': main()

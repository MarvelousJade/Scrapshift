"""Create a review from the actual Windows fixture captures and imported audio."""
import json, html
from pathlib import Path
root=Path(__file__).resolve().parents[1]
out=root/'artifacts/weapon-fidelity'
data=json.loads((root/'Assets/Scrapshift/Resources/weapon-reference.json').read_text())
cards=[]
for i,p in enumerate(data['projectiles']):
    name=Path(p['id']).stem.replace('_',' ').title()
    cards.append(f'<figure><img loading="lazy" src="projectile-{i}.png"><figcaption>{html.escape(name)}</figcaption></figure>')
audio=[]
for bank in data['audio']:
    path=bank['clips'][0]
    file=next((root/'Assets/Scrapshift/Resources').glob(path+'.*'))
    audio.append(f'<section><b>{bank["id"].title()} impact · {len(bank["clips"])} variants</b><audio controls src="../../Assets/Scrapshift/Resources/{path}{file.suffix}"></audio></section>')
melee=[]
for name in ['knife','sword','circular_saw']:
    for i,label in enumerate(['Windup','Strike','Return']):
        melee.append(f'<figure><img loading="lazy" src="{name}-{i}.png"><figcaption>{name.replace("_"," ").title()} — {label}</figcaption></figure>')
page='''<!doctype html><meta charset="utf-8"><title>Weapon reference review</title>
<style>body{background:#17191d;color:#eee;font:18px system-ui;margin:32px auto;max-width:1400px;padding:0 24px}h1{font-size:38px}p{max-width:1050px;line-height:1.5}a{color:#8ddebd}.grid{display:grid;grid-template-columns:repeat(3,1fr);gap:16px}figure{margin:0;background:#24272d;border-radius:12px;overflow:hidden}img{width:100%;display:block}figcaption{padding:14px}section{background:#24272d;padding:18px;border-radius:12px}audio{display:block;width:100%;margin-top:12px}.note{color:#b8bdc6}@media(max-width:800px){.grid{grid-template-columns:1fr}}</style>
<h1>Weapon projectiles, attacks & impact sounds</h1>
<p>Reference textures and frame timing, weapon-specific projectile selection, spread and accuracy, bounce/pierce rules, source-shaped melee hitboxes, thrust/sweep motion, and original impact audio.</p>
<p><a href="../../Builds/WeaponFidelity/LaunchGame.cmd">Launch updated Windows game</a> · <a href="weapons.json">205 weapon/tier attack results</a> · <a href="tests.xml">326 automated tests</a></p>
<p class="note">Captures below are deliberately zoomed in for inspection. Attack checks prove each weapon can hit stationary targets; they do not prove complete balance equivalence. Unity particle simulation, audio mixing/voice limits, and enemy hurt shapes can still differ from Godot. Explosions use the source flash and hit area; the source smoke particles are not yet fully reproduced.</p>
<h2>Original impact sound samples</h2><div class="grid">'''+''.join(audio)+'''</div><h2>23 projectile scenes</h2><div class="grid">'''+''.join(cards)+'''</div><h2>Melee motion snapshots</h2><div class="grid">'''+''.join(melee)+'''</div>'''
(out/'review.html').write_text(page,encoding='utf-8')
print(out/'review.html')

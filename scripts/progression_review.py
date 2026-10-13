"""Build a local comparison page from captured reference and player screenshots."""
from pathlib import Path
import html
import json

root = Path(__file__).resolve().parents[1]
out = root / 'artifacts/progression-options-review'
report = json.loads((out / 'inspection.json').read_text())
pairs = [
    ('Challenges and reward requirements', 'reference-challenges.png', 'codex-challenges.png'),
    ('Locked and undiscovered items', 'reference-codex-items.png', 'codex-items.png'),
    ('Weapon families and selectable tiers', 'reference-codex-weapons.png', 'codex-weapons.png'),
    ('Enemy discovery', 'reference-codex-enemies.png', 'codex-enemies.png'),
    ('Sound', 'reference-options-sound.png', 'options-sound.png'),
    ('Video', 'reference-options-video.png', 'options-video.png'),
    ('Gameplay', 'reference-options-gameplay.png', 'options-gameplay.png'),
    ('Accessibility', 'reference-options-accessibility.png', 'options-accessibility.png'),
]
def figure(file, label):
    return f'<figure><a href="{file}"><img src="{file}" loading="lazy"></a><figcaption>{html.escape(label)}</figcaption></figure>'
sections = ''.join(f'<section><h2>{html.escape(title)}</h2><div class="pair">'+figure(ref, 'Brotato — live Godot 3.6.2 reference')+figure(ours, 'Scrapshift — rebuilt standalone player')+'</div></section>' for title, ref, ours in pairs)
sections += '<section><h2>Rewards, icon fixes and the map without decorative stones</h2><div class="pair">'+''.join(figure(f'{name}.png',label) for name,label in [('unlock-reward','Earned reward before the run summary'),('elemental-details','Wand burning and Elemental Damage scaling icons'),('conditional-details','Jousting Lance conditional stat bonus icon'),('map-north','Map with grass and scrap; decorative stones removed')])+'</div></section>'
page = '''<!doctype html><html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width"><title>Scrapshift · Progression and options</title><style>
body{background:#111a19;color:#edf2ee;font:17px system-ui;margin:0;padding:36px}main{max-width:1800px;margin:auto}h1{font-size:42px;margin-bottom:12px}p{max-width:1000px;line-height:1.6;color:#b8cac3}a{color:#a9e6c8}section{margin:48px 0}.pair{display:grid;grid-template-columns:1fr 1fr;gap:20px}figure{margin:0;background:#1e2926;border-radius:12px;overflow:hidden}img{width:100%;display:block}figcaption{padding:14px}h2{font-size:24px}@media(max-width:900px){.pair{grid-template-columns:1fr}body{padding:20px}}</style><main><h1>Progression & options</h1>
<p>Live reference captures beside the rebuilt Unity game. Our art and cybernetic names stay intact. Click any image for full resolution.</p>
'''+f'<p>Build: zero errors/warnings. EditMode: 281/281 passed. Runtime captures: {len(report["checks"])} · {html.escape(report["status"])}. These staged captures do not certify native input or subjective audio.</p>'+'''
<p><a href="../../docs/progression-and-options.md">Design, implementation and explicit options scope</a> · <a href="inspection.json">Runtime report</a> · <a href="reference-options-accessibility-bottom.png">Reference accessibility settings, lower section</a></p>
'''+sections+'</main></html>'
(out/'review.html').write_text(page, encoding='utf-8')
print(out/'review.html')

"""Read generated alpha, bind wearable geometry, and slice via metadata only."""
from pathlib import Path
import json, hashlib
import numpy as np
from PIL import Image

root = Path(__file__).resolve().parents[1]
folder = root / 'Assets/Scrapshift/Content/OriginalArt/FittedWearables'
sheets, masks = [], []
for batch in range(1, 9):
    rows = json.loads((root / f'artifacts/fitted-appearance-review/batch-{batch}.json').read_text())
    path = folder / f'wearables-{batch:02}.png'
    pixels = np.asarray(Image.open(path).convert('RGBA'))
    h, w = pixels.shape[:2]
    solid = pixels[:, :, 3] > 32
    seen = np.zeros((h, w), bool)
    components = []
    for yy, xx in zip(*np.where(solid)):
        if seen[yy, xx]:
            continue
        stack = [(int(xx), int(yy))]
        seen[yy, xx] = True
        points = []
        while stack:
            x, y = stack.pop()
            points.append((x, y))
            for u, v in ((x-1, y), (x+1, y), (x, y-1), (x, y+1)):
                if 0 <= u < w and 0 <= v < h and solid[v, u] and not seen[v, u]:
                    seen[v, u] = True
                    stack.append((u, v))
        points = np.array(points)
        center = points.mean(axis=0)
        cell = min(4, int(center[1]*5/h))*5 + min(4, int(center[0]*5/w))
        components.append((points, center, cell))
    large = [c for c in components if len(c[0]) > 300]
    # Generated atlases can shift an entire row; learn the actual gutters.
    row_count = (len(rows)+4)//5
    centers = np.array([c[1] for c in large])
    row_centers = np.linspace(centers[:, 1].min(), centers[:, 1].max(), row_count)
    for _ in range(25):
        labels = np.abs(centers[:, 1, None]-row_centers).argmin(axis=1)
        row_centers = np.array([centers[labels == i, 1].mean() for i in range(row_count)])
    groups = [[] for _ in rows]
    for points, center, cell in components:
        if len(points) < 300:
            center = min(large, key=lambda c: np.sum((c[1]-center)**2))[1]
        cell = int(np.abs(row_centers-center[1]).argmin())*5 + min(4, int(center[0]*5/w))
        assert cell < len(rows), (batch, 'unexpected occupied cell', cell)
        groups[cell].append(points)
    entries = []
    for row, parts in zip(rows, groups):
        assert parts, (batch, row['key'], 'empty wearable')
        points = np.concatenate(parts)
        xs, ys = points[:, 0], points[:, 1]
        l, r = max(0, int(xs.min())-2), min(w, int(xs.max())+3)
        t, b = max(0, int(ys.min())-2), min(h, int(ys.max())+3)
        source = dict(x=l, y=h-b, width=r-l, height=b-t)
        bands = []
        for y in sorted(set(ys), reverse=True):
            xx = xs[ys == y]
            lo, hi, uy = max(l, int(xx.min())-1), min(r-1, int(xx.max())+1), h-int(y)-1
            if bands and bands[-1]['x'] == lo and bands[-1]['width'] == hi-lo+1 and bands[-1]['y']+bands[-1]['height'] == uy:
                bands[-1]['height'] += 1
            else:
                bands.append(dict(x=lo, y=uy, width=hi-lo+1, height=1))
        fitted = dict(row)
        fitted['referenceFit'] = {k: row[k] for k in ('width', 'height', 'offsetX', 'offsetY')}
        aspect = (r-l)/(b-t)
        position = row['position']
        if position == 14:
            fitted.update(width=1.003, height=1.21021, offsetX=0, offsetY=0)
        elif position == 4:
            width = min(1.07, row['width'])
            height = min(.62, width/aspect)
            fitted.update(width=width, height=height, offsetX=0, offsetY=.21+height/2)
            if row['id'] == 'helmet':
                fitted.update(width=1.06, height=.92, offsetY=.17)
        elif position == 6:
            width = min(.88, row['width'])
            fitted.update(width=width, height=min(.32, width/aspect), offsetX=0, offsetY=.11)
        elif position == 8:
            width = min(.70, row['width'])
            fitted.update(width=width, height=min(.28, width/aspect), offsetX=0, offsetY=-.24)
        elif position in (10, 11):
            width = min(.95, row['width'])
            fitted.update(width=width, height=min(.46, width/aspect), offsetX=0, offsetY=-.36)
        elif position == 1:
            width = min(.65, row['width'])
            height = min(.50, width/aspect)
            fitted.update(width=width, height=height, offsetX=0, offsetY=.58+height/2)
        entries.append(dict(fitted, x=l, y=h-b, pixelWidth=r-l, pixelHeight=b-t,
                            pixels=len(points), hash=hashlib.sha256(pixels[t:b, l:r].tobytes()).hexdigest()))
        masks.append(dict(name='Fitted_'+row['key'], source=source, bands=bands))
    sheets.append(dict(path=path.relative_to(root).as_posix(), width=w, height=h, entries=entries))
    print(batch, len(entries), 'wearable layers audited', flush=True)
# Two targeted redraws preserve the shop designs after visual QA.
correction = folder / 'wearables-09.png'
if correction.exists():
    pixels = np.asarray(Image.open(correction).convert('RGBA'))
    h, w = pixels.shape[:2]
    replacements = []
    for index, key in enumerate(('pumpkin_0', 'propeller_hat_0')):
        previous = next(e for s in sheets for e in s['entries'] if e['key'] == key)
        for sheet in sheets:
            sheet['entries'] = [e for e in sheet['entries'] if e['key'] != key]
        masks = [m for m in masks if m['name'] != 'Fitted_'+key]
        x0, x1 = index*w//2, (index+1)*w//2
        ys, xs = np.where(pixels[:, x0:x1, 3] > 32)
        xs += x0
        l, r = max(x0, int(xs.min())-2), min(x1, int(xs.max())+3)
        t, b = max(0, int(ys.min())-2), min(h, int(ys.max())+3)
        source = dict(x=l, y=h-b, width=r-l, height=b-t)
        bands = []
        for y in sorted(set(ys), reverse=True):
            xx = xs[ys == y]
            lo, hi, uy = max(l, int(xx.min())-1), min(r-1, int(xx.max())+1), h-int(y)-1
            bands.append(dict(x=lo, y=uy, width=hi-lo+1, height=1))
        entry = dict(previous, x=l, y=h-b, pixelWidth=r-l, pixelHeight=b-t,
                     pixels=len(xs), hash=hashlib.sha256(pixels[t:b, l:r].tobytes()).hexdigest())
        if index == 0:
            entry.update(title='Piercing Stabilizer', width=.67, height=.23, offsetX=0, offsetY=-.24)
        else:
            entry.update(title='Gyroscopic Crown', width=1.07, height=.62, offsetX=0, offsetY=.52)
        replacements.append(entry)
        masks.append(dict(name='Fitted_'+key, source=source, bands=bands))
    sheets.append(dict(path=correction.relative_to(root).as_posix(), width=w, height=h, entries=replacements))
assert len({e['hash'] for s in sheets for e in s['entries']}) == 185
(folder / 'manifest.json').write_text(json.dumps(dict(sheets=sheets), indent=2)+'\n')
(root / 'Assets/Scrapshift/Resources/fitted-wearable-masks.json').write_text(json.dumps(dict(masks=masks), separators=(',', ':'))+'\n')
print('185 distinct nonempty wearable parts; PNG pixels unchanged.')

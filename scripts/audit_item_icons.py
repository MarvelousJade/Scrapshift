"""Audit generated atlas alpha and write sprite/mask coordinates without rewriting pixels."""
from pathlib import Path
from collections import deque
import json,hashlib
import numpy as np
from PIL import Image
root=Path(__file__).resolve().parents[1];folder=root/'Assets/Scrapshift/Content/OriginalArt/ItemIcons'
manifest=[];masks=[]
for batch in range(1,8):
 rows=json.loads((root/f'artifacts/item-icon-audit/batch-{batch}.json').read_text());cols=4 if batch==7 else 5
 path=folder/f'items-{batch:02}.png';im=np.asarray(Image.open(path).convert('RGBA'));h,w=im.shape[:2];solid=im[:,:,3]>32;seen=np.zeros((h,w),bool);components=[]
 for yy,xx in zip(*np.where(solid)):
  if seen[yy,xx]:continue
  q=[(int(xx),int(yy))];seen[yy,xx]=True;points=[]
  while q:
   x,y=q.pop();points.append((x,y))
   for u,v in ((x-1,y),(x+1,y),(x,y-1),(x,y+1)):
    if 0<=u<w and 0<=v<h and solid[v,u] and not seen[v,u]:seen[v,u]=True;q.append((u,v))
  a=np.array(points);center=a.mean(axis=0);cell=min(cols-1,int(center[1]*cols/h))*cols+min(cols-1,int(center[0]*cols/w));components.append((a,center,cell))
 groups=[[] for _ in rows]
 large=[x for x in components if len(x[0])>400]
 for points,center,cell in components:
  if len(points)<400:
   nearest=min(large,key=lambda c:np.sum((c[1]-center)**2));cell=nearest[2]
  groups[cell].append(points)
 entries=[]
 for i,parts in enumerate(groups):
  assert parts,(batch,i);points=np.concatenate(parts);xs=points[:,0];ys=points[:,1];l=max(0,int(xs.min())-2);r=min(w,int(xs.max())+3);t=max(0,int(ys.min())-2);b=min(h,int(ys.max())+3)
  row=rows[i];source=dict(x=l,y=h-b,width=r-l,height=b-t)
  bands=[]
  for y in sorted(set(ys),reverse=True):
   xx=xs[ys==y];lo=max(l,int(xx.min())-1);hi=min(r-1,int(xx.max())+1);uy=h-int(y)-1
   if bands and bands[-1]['x']==lo and bands[-1]['width']==hi-lo+1 and bands[-1]['y']+bands[-1]['height']==uy:bands[-1]['height']+=1
   else:bands.append(dict(x=lo,y=uy,width=hi-lo+1,height=1))
  entries.append(dict(id=row['id'],title=row['title'],**source,pixels=len(points),hash=hashlib.sha256(im[t:b,l:r].tobytes()).hexdigest()))
  masks.append(dict(name='Item_'+row['id'],source=source,bands=bands))
 manifest.append(dict(path=str(path.relative_to(root)).replace('\\','/'),width=w,height=h,entries=entries))
 print(batch,len(entries),'icons;',sum(e['pixels'] for e in entries),'visible pixels',flush=True)
assert len({e['hash'] for s in manifest for e in s['entries']})==166
(folder/'manifest.json').write_text(json.dumps(dict(sheets=manifest),indent=2)+'\n')
(root/'Assets/Scrapshift/Resources/item-icon-masks.json').write_text(json.dumps(dict(masks=masks),separators=(',',':'))+'\n')
print('166 nonempty distinct image regions verified')

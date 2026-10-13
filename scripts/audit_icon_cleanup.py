"""Read PNG alpha; update sprite rectangles/clipping geometry without editing bitmap pixels."""
from pathlib import Path
from collections import deque
import json,re,hashlib
import numpy as np
from PIL import Image
root=Path(__file__).resolve().parents[1]
art=root/'Assets/Scrapshift/Content/OriginalArt'
pattern=r'(name: ([^\n]+)\s+rect:\s+serializedVersion: 2\s+x: )([\d.]+)(\s+y: )([\d.]+)(\s+width: )([\d.]+)(\s+height: )([\d.]+)'
records=[];report=[]
for sheet in ['items-v2','stats-v2','equipment-v2','starter-weapons-v1','base-weapons-v1','wearables-v2']:
 path=art/(sheet+'.png');image=Image.open(path);alpha=np.asarray(image)[:,:,3];solid=alpha>30;h,w=solid.shape
 meta_path=path.with_suffix('.png.meta');meta=meta_path.read_text();matches=list(re.finditer(pattern,meta))
 boxes=[]
 for m in matches:
  x,y,rw,rh=[int(float(m.group(i))) for i in (3,5,7,9)];boxes.append((x,h-y-rh,x+rw,h-y))
 seen=np.zeros_like(solid);groups=[[] for _ in boxes];large=[0 for _ in boxes]
 for y,x in zip(*np.where(solid)):
  if seen[y,x]:continue
  queue=[(int(x),int(y))];seen[y,x]=True;points=[]
  while queue:
   xx,yy=queue.pop();points.append((xx,yy))
   for u,v in ((xx-1,yy),(xx+1,yy),(xx,yy-1),(xx,yy+1)):
    if 0<=u<w and 0<=v<h and solid[v,u] and not seen[v,u]:seen[v,u]=True;queue.append((u,v))
  px=np.array([p[0] for p in points]);py=np.array([p[1] for p in points])
  overlap=[int(((px>=l)&(px<r)&(py>=t)&(py<b)).sum()) for l,t,r,b in boxes]
  if max(overlap):idx=int(np.argmax(overlap))
  else:idx=min(range(len(boxes)),key=lambda i:(px.mean()-(boxes[i][0]+boxes[i][2])/2)**2+(py.mean()-(boxes[i][1]+boxes[i][3])/2)**2)
  groups[idx].extend(points)
  if len(points)>500:large[idx]+=1
 updated={}
 for i,points in enumerate(groups):
  if not points: # The equipment shadow is intentionally a transparent placeholder.
   continue
  xs=[p[0] for p in points];ys=[p[1] for p in points]
  l=max(0,min(xs)-2);r=min(w,max(xs)+3);t=max(0,min(ys)-2);b=min(h,max(ys)+3)
  source=dict(x=l,y=h-b,width=r-l,height=b-t);name=matches[i].group(2);updated[name]=source
  rows={}
  for x,y in points:
   if y not in rows:rows[y]=[x,x]
   else:rows[y]=[min(x,rows[y][0]),max(x,rows[y][1])]
  bands=[]
  for y,(lo,hi) in sorted(rows.items(),reverse=True):
   lo=max(l,lo-1);hi=min(r-1,hi+1);uy=h-y-1
   if bands and bands[-1]['x']==lo and bands[-1]['width']==hi-lo+1 and bands[-1]['y']+bands[-1]['height']==uy:bands[-1]['height']+=1
   else:bands.append(dict(x=lo,y=uy,width=hi-lo+1,height=1))
  records.append(dict(name=name,source=source,bands=bands))
 def replace(m):
  if m.group(2) not in updated:return m.group(0)
  s=updated[m.group(2)];return m.group(1)+str(s['x'])+m.group(4)+str(s['y'])+m.group(6)+str(s['width'])+m.group(8)+str(s['height'])
 meta_path.write_text(re.sub(pattern,replace,meta))
 report.append(dict(sheet=sheet,sprites=len(matches),masked=len(updated),alphaPixels=int(solid.sum()),assignedPixels=sum(map(len,groups)),largeComponentsPerSprite=large,sha256=hashlib.sha256(path.read_bytes()).hexdigest()))
 print(sheet,len(matches),'sprites; all',int(solid.sum()),'solid pixels assigned')
(root/'Assets/Scrapshift/Resources/clean-icon-masks.json').write_text(json.dumps(dict(masks=records),separators=(',',':'))+'\n')
(root/'artifacts/icon-cleanup-review/atlas-audit.json').write_text(json.dumps(report,indent=2)+'\n')

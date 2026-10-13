"""Read-only bitmap analysis: preserve full connected icons across uneven atlas gutters."""
from pathlib import Path
import json,re
import numpy as np
from PIL import Image
root=Path(__file__).resolve().parents[1]
source=(root/'Assets/Scrapshift/Runtime/AchievementArt.cs').read_text()
names=re.search(r'Symbols=\{(.*?)\}',source).group(1).replace('"','').split(',')
records=[]
for sheet,cols,rows,keys in [('A',6,4,names[:24]),('B',5,3,names[24:]+['unused_trophy'])]:
 im=Image.open(root/f'Assets/Scrapshift/Resources/AchievementIcons{sheet}.png');solid=np.asarray(im)[:,:,3]>30;h,w=solid.shape
 seen=np.zeros_like(solid);groups=[[] for _ in keys]
 for y,x in zip(*np.where(solid)):
  if seen[y,x]:continue
  pending=[(int(x),int(y))];seen[y,x]=True;points=[]
  while pending:
   xx,yy=pending.pop();points.append((xx,yy))
   for u,v in [(xx-1,yy),(xx+1,yy),(xx,yy-1),(xx,yy+1)]:
    if 0<=u<w and 0<=v<h and solid[v,u] and not seen[v,u]:seen[v,u]=True;pending.append((u,v))
  votes=[0]*len(keys)
  for xx,yy in points:votes[min(rows-1,yy*rows//h)*cols+min(cols-1,xx*cols//w)]+=1
  groups[int(np.argmax(votes))].extend(points)
 for key,points in zip(keys,groups):
  assert points,key
  xs,ys=zip(*points);l=max(0,min(xs)-2);r=min(w,max(xs)+3);t=max(0,min(ys)-2);b=min(h,max(ys)+3)
  scan={}
  for x,y in points:
   if y not in scan:scan[y]=[x,x]
   else:scan[y]=[min(x,scan[y][0]),max(x,scan[y][1])]
  bands=[dict(x=max(l,a-1),y=h-y-1,width=min(r-1,z+1)-max(l,a-1)+1,height=1) for y,(a,z) in scan.items()]
  records.append(dict(name='Achievement_'+key,source=dict(x=l,y=h-b,width=r-l,height=b-t),bands=bands))
 print(sheet,len(keys),'icons',sum(map(len,groups)),'pixels assigned')
(root/'Assets/Scrapshift/Resources/achievement-icon-masks.json').write_text(json.dumps(dict(masks=records),separators=(',',':'))+'\n')

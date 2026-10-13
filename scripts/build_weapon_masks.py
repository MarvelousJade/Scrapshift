"""Derive sprite clipping geometry from existing artwork; never modifies pixels."""
from pathlib import Path
from PIL import Image
import json
root=Path(__file__).resolve().parents[1]
im=Image.open(root/'Assets/Scrapshift/Content/OriginalArt/base-weapons-v1.png');alpha=im.getchannel('A');seen=set();parts=[]
for y in range(im.height):
 for x in range(im.width):
  if (x,y) in seen or alpha.getpixel((x,y))<=30:continue
  pending=[(x,y)];seen.add((x,y));points=[]
  while pending:
   xx,yy=pending.pop();points.append((xx,yy))
   for u,v in ((xx-1,yy),(xx+1,yy),(xx,yy-1),(xx,yy+1)):
    if 0<=u<im.width and 0<=v<im.height and (u,v) not in seen and alpha.getpixel((u,v))>30:seen.add((u,v));pending.append((u,v))
  if len(points)>500:parts.append(points)
assert len(parts)==32
parts.sort(key=lambda p:(round(sum(y for x,y in p)/len(p)/240),sum(x for x,y in p)/len(p)))
# The four rows have centers near 180,400,630,850; use actual median row bands.
parts.sort(key=lambda p:(int((sum(y for x,y in p)/len(p)-60)//240),sum(x for x,y in p)/len(p)))
records=[]
for index,points in enumerate(parts):
 xs=[x for x,y in points];ys=[y for x,y in points];left,right=min(xs)-2,max(xs)+3;top,bottom=min(ys)-2,max(ys)+3
 rows={}
 for x,y in points:
  if y not in rows:rows[y]=[x,x]
  else:rows[y]=[min(x,rows[y][0]),max(x,rows[y][1])]
 strips=[]
 for y,(lo,hi) in sorted(rows.items(),reverse=True):
  # Keep a one-pixel antialias fringe unless it belongs to an adjacent solid component.
  if alpha.getpixel((lo-1,y))<=30:lo-=1
  if alpha.getpixel((hi+1,y))<=30:hi+=1
  uy=im.height-y-1
  if strips and strips[-1]['x']==lo and strips[-1]['width']==hi-lo+1 and strips[-1]['y']+strips[-1]['height']==uy:strips[-1]['height']+=1
  else:strips.append(dict(x=lo,y=uy,width=hi-lo+1,height=1))
 records.append(dict(name=f'base-weapons_{index:02}',source=dict(x=left,y=im.height-bottom,width=right-left,height=bottom-top),bands=strips))
(root/'Assets/Scrapshift/Resources/weapon-icon-masks.json').write_text(json.dumps(dict(masks=records),separators=(',',':'))+'\n')
print('Generated',len(records),'geometry masks without modifying artwork')

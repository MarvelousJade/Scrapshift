"""Read-only PNG inspection: find transparent atlas separators; never edit pixels."""
from PIL import Image
from pathlib import Path
import hashlib
import json

root = Path(__file__).resolve().parents[1]
specs = [('player-parts-v2', 3, [0,500,820,1254]), ('enemies-v5',6,[0,370,663,1024]),
         ('wearables-v2',6,[0,285,550,760,1024]), ('items-v2',6,[0,272,522,756,1024]),
         ('equipment-v2',6,[0,341,683,1024]), ('stats-v2',4,[0,512,1024])]
result = {}
for name, cols, ys in specs:
    path = root/'Assets/Scrapshift/Content/OriginalArt'/(name+'.png')
    im = Image.open(path)
    alpha = im.getchannel('A')
    mask = alpha.point(lambda v: 255 if v > 30 else 0)
    clear = alpha.histogram()[0]/(im.width*im.height)
    if clear < .15:
        raise ValueError((name, 'Insufficient transparent background', clear))
    edges_y = [0]
    for mid in ys[1:-1]:
        gaps = [y for y in range(mid-45,mid+46) if not mask.crop((0,y,im.width,y+1)).getbbox()]
        edges_y.append(min(gaps, key=lambda y:abs(y-mid)) if gaps else mid)
    edges_y.append(im.height-1)
    regions = []
    for r in range(len(edges_y)-1):
        band = mask.crop((0,edges_y[r],im.width,edges_y[r+1]+1))
        edges_x = [0]
        for c in range(1,cols):
            mid = im.width*c//cols
            gaps = [x for x in range(mid-100,mid+101) if not band.crop((x,0,x+1,band.height)).getbbox()]
            if not gaps: raise ValueError((name, r, c, 'No vertical separator'))
            edges_x.append(min(gaps, key=lambda x:abs(x-mid)))
        edges_x.append(im.width-1)
        for c in range(cols):
            local_y = []
            for mid in (edges_y[r],edges_y[r+1]):
                if mid in (0,im.height-1): local_y.append(mid); continue
                gaps = [y for y in range(mid-45,mid+46) if not mask.crop((edges_x[c],y,edges_x[c+1]+1,y+1)).getbbox()]
                if not gaps: raise ValueError((name,r,c,'No local horizontal separator'))
                local_y.append(min(gaps,key=lambda y:abs(y-mid)))
            # A single transparent separator column can border both neighboring rectangles.
            regions.append([edges_x[c],local_y[0],edges_x[c+1]+1,local_y[1]+1])
    result[name] = {'regions':regions,'size':im.size,'transparentFraction':clear,'sha256':hashlib.sha256(path.read_bytes()).hexdigest()}
    print(name, 'transparent', round(clear,3), 'sprites',len(regions))
(root/'docs/clean-atlas-regions.json').write_text(json.dumps(result,indent=2)+'\n')

"""Compare completed deterministic combat benchmark receipts without claiming game FPS."""
import argparse,hashlib,json,statistics
from pathlib import Path
p=argparse.ArgumentParser()
p.add_argument('--baseline',nargs='+',type=Path,required=True)
p.add_argument('--optimized',nargs='+',type=Path,required=True)
p.add_argument('--output',type=Path,required=True)
a=p.parse_args()
def read(paths):
 rows=[]
 for path in paths:
  raw=path.read_bytes();data=json.loads(raw)
  assert len(data['samples'])==2 and data['graphicsDeviceAvailable'] and data['renderedRequested'],path
  assert [s['enemies'] for s in data['samples']]==[100,200],path
  assert all(s['frames']==360 and s['shotsFired']>0 for s in data['samples']),path
  rows.append((path,hashlib.sha256(raw).hexdigest(),data))
 return rows
b=read(a.baseline);o=read(a.optimized);rows=[]
for i,count in enumerate([100,200]):
 baseline=[r[2]['samples'][i] for r in b];optimized=[r[2]['samples'][i] for r in o]
 keys=('positionChecksum','healthChecksum','shotsFired','remainingShots')
 matches=all(s[k]==baseline[0][k] for s in baseline+optimized for k in keys)
 assert matches,f'Gameplay checksum differs for {count} enemies'
 mean=lambda values,key:statistics.mean(s[key] for s in values)
 before=mean(baseline,'simulationMeanMs');after=mean(optimized,'simulationMeanMs')
 rows.append(dict(enemies=count,checksumsMatch=matches,baselineCpuMeanMs=before,optimizedCpuMeanMs=after,
  cpuReductionPercent=100*(1-after/before),baselineFrameMeanMs=mean(baseline,'frameIntervalMeanMs'),
  optimizedFrameMeanMs=mean(optimized,'frameIntervalMeanMs'),baselineP95FrameMs=[s['frameIntervalP95Ms'] for s in baseline],
  optimizedP95FrameMs=[s['frameIntervalP95Ms'] for s in optimized],baselineGen0=[s['gen0Collections'] for s in baseline],
  optimizedGen0=[s['gen0Collections'] for s in optimized],baselineManagedAfter=[s['monoUsedAfterBytes'] for s in baseline],
  optimizedManagedAfter=[s['monoUsedAfterBytes'] for s in optimized],allocationCounterAvailable=all(s['allocationCounterValid'] for s in baseline+optimized)))
report=dict(status='Passed',scope='Synthetic 100/200-enemy rendered fixture, fixed simulation step, UI disabled. CPU and frame intervals differ; no whole-game FPS or byte-allocation claim. Memory snapshots are not retained-heap/leak measurements.',
 inputs=[dict(path=str(path),sha256=digest) for path,digest,_ in b+o],rows=rows)
a.output.write_text(json.dumps(report,indent=2)+'\n')
print(json.dumps(report['rows'],indent=2))

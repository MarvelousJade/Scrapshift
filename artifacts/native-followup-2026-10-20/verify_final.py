from pathlib import Path
import hashlib,json,xml.etree.ElementTree as ET
r=Path.cwd();a=r/'artifacts/native-followup-2026-10-20';c=Path('E:/Codex Projects/Job Search Assistant/tmp/ScrapshiftParity');o=Path('E:/UnityProjects/brotato-(3.x)');rc=Path('E:/Codex Projects/Job Search Assistant/tmp/BrotatoParity20261020')
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
inputs=read(a/'build-r40-source-manifest.json');files=read(a/'build-r40-manifest.json')
for row in inputs:assert sha(r/row['path'])==row['sha256']==sha(c/row['path']),row['path']
for row in files:assert sha(r/'Builds/GameplayParity20261020R40'/row['path'])==row['sha256'],row['path']
original=read(a/'reference-safe-policy-preparation/original-after.json');clone=read(a/'reference-safe-policy-preparation/clone-after.json')
original_drift=[k for k,v in original.items() if sha(o/k)!=v];clone_drift=[k for k,v in clone.items() if sha(rc/k)!=v];assert not original_drift,original_drift;assert not clone_drift,clone_drift
suite=ET.parse(a/'suite-r40.xml').getroot().attrib;assert suite['passed']=='1286' and suite['failed']=='0' and suite['skipped']=='0'
comparison=read(a/'full-campaign-r40/economy-comparison.json');assert comparison['allChecksPassed'] and len(comparison['checks'])==18
for folder in ['full-campaign-r40','original-safe-full']:assert read(a/folder/'process.json')['exitCode']==0
u=read(a/'full-campaign-r40/campaign-check.json');s=read(a/'original-safe-full/campaign-check.json');assert u['completedWaves']==20 and s['completed_waves']==20 and u['safeLoadoutPolicy'] and s['safe_loadout_policy']
report=dict(codeCommit='ede4094',sourceInputs=len(inputs),playerFiles=len(files),originalSourceInputs=len(original),referenceCloneInputs=len(clone),allPostRunHashesMatch=True,originalModified=False,suite=suite,comparisonChecks=comparison['checks'],physicalController='Deferred; no new hardware verification',scope='Finite normal-timing matched-policy headless campaigns, scoped native/software evidence; no universal gameplay, human balance, browser or licensing claim')
(a/'final-postrun-verification.json').write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8');print({k:v for k,v in report.items() if k not in ('suite','comparisonChecks')})

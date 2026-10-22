from pathlib import Path
import json,hashlib,re,xml.etree.ElementTree as ET
r=Path.cwd();a=r/'artifacts/death-item-followup-2026-10-20';clone=Path('E:/Codex Projects/Job Search Assistant/tmp/ScrapshiftParity')
def read(p):return json.loads(p.read_text(encoding='utf-8'))
inputs=read(a/'build-r35-source-manifest.json');files=read(a/'build-r35-manifest.json');build=r/'Builds/GameplayParity20261020R35'
for x in inputs:
 assert hashlib.sha256((r/x['path']).read_bytes()).hexdigest()==x['sha256']
 assert hashlib.sha256((clone/x['path']).read_bytes()).hexdigest()==x['sha256']
for x in files:assert hashlib.sha256((build/x['path']).read_bytes()).hexdigest()==x['sha256']
reports={}
for folder,name in [('death-player','headless-death-timing.json'),('chain-player','headless-item-chain.json'),('proc-player','headless-proc-target.json'),('unlocks-player','headless-unlocks.json'),('survival-preflight','campaign-check.json'),('survival-player','campaign-check.json')]:
 d=read(a/folder/name);p=read(a/folder/'process.json');assert d['status']=='Passed' and p['exitCode']==0
 logs=[]
 for f in [a/folder/'player.log',a/folder/'runner-stderr.log']:
  logs.extend([line for line in f.read_text(encoding='utf-8',errors='replace').splitlines() if re.search(r'exception|error|assert',line,re.I)])
 reports[folder]=dict(status=d['status'],exitCode=p['exitCode'],elapsedSeconds=p['elapsedSeconds'],diagnosticMatches=logs)
d=read(a/'death-player/headless-death-timing.json');assert len(d['rows'])==8 and all(x['passed'] for x in d['rows'])
c=read(a/'chain-player/headless-item-chain.json');assert len(c['explosions'])==4 and all(x['burnTicks']==0 and x['playerHealthBefore']==x['playerHealthAfter'] and x['lifeStealTimerUntouched'] and x['expectedHealth']==x['observedHealth'] for x in c['explosions'])
assert [x['completed'] for x in c['fireworks']]==[False,True] and c['beardBurns'] and c['beardStealsLife']
u=read(a/'unlocks-player/headless-unlocks.json');assert len(u['rows'])==109 and all(all(x[k] for k in ['below','completed','once','reloaded','frozen','nextRun']) for x in u['rows'])
s=read(a/'survival-player/campaign-check.json');assert s['phase']=='Lost' and s['observedNetDamage']>0 and not s['invulnerabilityInjected'] and s['deathCleanupAge']>=.5
suite=ET.parse(a/'suite-attempt-2.xml').getroot();assert suite.attrib['failed']=='0' and suite.attrib['passed']=='1271'
summary=dict(codeCommit='debe1cf',sourceInputs=len(inputs),playerFiles=len(files),allHashesMatch=True,suite=suite.attrib,packaged=reports,survival={k:s[k] for k in ['phase','completedWaves','observedNetDamage','observedNetHealing','lowestObservedHealth','deathCleanupAge','realtime','pickupPhysicsRate','invulnerabilityInjected']},physicalController='Skipped per user; no hardware acceptance',desktopInputUsed=False)
(a/'r35-input-verification.json').write_text(json.dumps(summary,indent=2)+'\n',encoding='utf-8');print(json.dumps(summary,indent=2))

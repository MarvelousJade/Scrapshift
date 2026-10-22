from pathlib import Path
import json,subprocess,os,time
root=Path(__file__).resolve().parents[2];base=root/'artifacts/headless-completion-2026-10-20';exe=root/'Builds/GameplayParity20261020R31/Scrapshift.exe'
for name,flag in [('shops','--headless-shops-review'),('unlocks','--headless-unlocks-review')]:
 out=base/(name+'-r31-attempt-1');out.mkdir();args=[str(exe),'-batchmode','-nographics',flag,str(out),'-logFile',str(out/'player.log')]
 env=os.environ.copy()
 for key,sub in [('APPDATA','Roaming'),('LOCALAPPDATA','Local')]:
  folder=out/'host-data'/sub;folder.mkdir(parents=True);env[key]=str(folder)
 spec=dict(executable=str(exe),workingDirectory=str(root),output=str(out),arguments=args[1:],environment={k:env[k] for k in ('APPDATA','LOCALAPPDATA')});(out/'runner-spec.json').write_text(json.dumps(spec,indent=2)+'\n')
 start=time.time()
 with (out/'runner-stdout.log').open('w') as stdout,(out/'runner-stderr.log').open('w') as stderr:
  result=subprocess.run(args,cwd=root,env=env,stdout=stdout,stderr=stderr,timeout=120)
 (out/'process.json').write_text(json.dumps(dict(exitCode=result.returncode,elapsedSeconds=time.time()-start),indent=2)+'\n');print(name,result.returncode,flush=True)
 assert result.returncode==0
 report=json.loads((out/('headless-shops-review.json' if name=='shops' else 'headless-unlocks.json')).read_text(encoding='utf-8-sig'));print(report.get('status'),flush=True);assert report.get('status')=='Passed'

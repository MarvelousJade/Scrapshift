from pathlib import Path
import json,subprocess,os,sys,time
spec=json.loads(Path(sys.argv[1]).read_text());out=Path(spec['output']);out.mkdir(parents=True,exist_ok=True);env=os.environ.copy();env.update(spec.get('environment',{}));start=time.time()
with (out/'runner-stdout.log').open('w') as stdout,(out/'runner-stderr.log').open('w') as stderr:
 result=subprocess.run([spec['executable']]+spec['arguments'],cwd=spec['workingDirectory'],env=env,stdout=stdout,stderr=stderr,timeout=spec.get("timeoutSeconds",120))
(out/'process.json').write_text(json.dumps(dict(exitCode=result.returncode,elapsedSeconds=time.time()-start),indent=2)+'\n');print(result.returncode,flush=True);sys.exit(result.returncode)

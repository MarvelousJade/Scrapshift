"""Verify immutable evidence Git blobs preserve the working evidence bytes."""
from pathlib import Path
import hashlib,subprocess
root=Path(__file__).resolve().parents[2]
prefix='artifacts/native-followup-2026-10-20'
algorithm=subprocess.check_output(['git','rev-parse','--show-object-format'],cwd=root).decode().strip()
raw=subprocess.check_output(['git','ls-files','--stage','-z','--',prefix],cwd=root)
count=0
for entry in raw.split(b'\0'):
 if not entry:continue
 meta,name=entry.split(b'\t',1);data=(root/name.decode()).read_bytes()
 oid=hashlib.new(algorithm,b'blob '+str(len(data)).encode()+b'\0'+data).hexdigest()
 assert oid==meta.split()[1].decode(),name.decode()
 count+=1
print(f'{count} tracked evidence blobs match original working bytes exactly.')

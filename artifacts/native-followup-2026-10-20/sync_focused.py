from pathlib import Path
import json, hashlib, shutil
root=Path(__file__).resolve().parents[2]
clone=Path("E:/Codex Projects/Job Search Assistant/tmp/ScrapshiftParity")
prior=json.loads((root/"artifacts/parity-remaining-2026-10-20/final-source-manifest.json").read_text())
paths={x["path"] for x in prior}
for directory in ("Runtime","Editor","Tests"):
    for p in (root/"Assets/Scrapshift"/directory).rglob("*"):
        if p.is_file() and (p.suffix in (".cs",".asmdef") or p.name.endswith((".cs.meta",".asmdef.meta"))): paths.add(p.relative_to(root).as_posix())
records=[]
for rel in sorted(paths):
    src=root/rel;dst=clone/rel
    if not src.is_file(): raise RuntimeError("Missing input "+rel)
    dst.parent.mkdir(parents=True,exist_ok=True)
    if not dst.exists() or src.read_bytes()!=dst.read_bytes(): shutil.copy2(src,dst)
    digest=hashlib.sha256(src.read_bytes()).hexdigest()
    records.append(dict(path=rel,sha256=digest,matches=hashlib.sha256(dst.read_bytes()).hexdigest()==digest))
Path(__file__).with_name("source-manifest.json").write_text(json.dumps(records,indent=2)+"\n")
assert all(x["matches"] for x in records)
print(f"Verified {len(records)} source/settings/import inputs in validation clone; generated catalog not overwritten")

rel="Assets/Scrapshift/Content/Definitions/ReferenceCatalog.asset"
a=(root/rel).read_bytes();b=(clone/rel).read_bytes()
assert a.replace(b"\r\n",b"\n")==b.replace(b"\r\n",b"\n"),"Generated catalog contents differ"
Path(__file__).with_name("catalog-comparison.json").write_text(json.dumps(dict(path=rel,rootSha256=hashlib.sha256(a).hexdigest(),cloneSha256=hashlib.sha256(b).hexdigest(),equalAfterLineEndingNormalization=True,overwritten=False),indent=2)+"\n")

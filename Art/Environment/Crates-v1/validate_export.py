from pathlib import Path
import json,math,re,uuid,zipfile
root=Path('Assets/Environment/CratesLowPoly');art=Path('Art/Environment/Crates-v1')
p=root/'Editor/CratePrefabSetup.cs.meta'
if not p.exists():p.write_text('fileFormatVersion: 2\nguid: '+uuid.uuid4().hex+'\nMonoImporter:\n  externalObjects: {}\n  serializedVersion: 2\n  defaultReferences: []\n  executionOrder: 0\n  icon: {instanceID: 0}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n')
results=[]
for obj in sorted(root.glob('*.obj')):
    vs=[];uv=[];ns=[];fs=[]
    for l in obj.read_text().splitlines():
        s=l.split()
        if not s:continue
        if s[0]=='v':vs.append(tuple(map(float,s[1:])))
        elif s[0]=='vt':uv.append(tuple(map(float,s[1:])))
        elif s[0]=='vn':ns.append(tuple(map(float,s[1:])))
        elif s[0]=='f':fs.append([tuple(int(j)-1 for j in token.split('/')) for token in s[1:]])
    assert min(v[1] for v in vs)>=-1e-6
    volume=0
    for f in fs:
        assert len(f)==3
        for a,b,c in f:assert 0<=a<len(vs) and 0<=b<len(uv) and 0<=c<len(ns)
        a,b,c=[vs[i[0]] for i in f]
        ab=[b[i]-a[i] for i in range(3)];ac=[c[i]-a[i] for i in range(3)]
        n=(ab[1]*ac[2]-ab[2]*ac[1],ab[2]*ac[0]-ab[0]*ac[2],ab[0]*ac[1]-ab[1]*ac[0])
        length=math.sqrt(sum(x*x for x in n));assert length>1e-8
        assert sum(n[i]/length*ns[f[0][2]][i] for i in range(3))>.999
        volume+=sum(a[i]*(b[(i+1)%3]*c[(i+2)%3]-b[(i+2)%3]*c[(i+1)%3]) for i in range(3))/6
    assert volume>0
    assert all(0<=u<=1 and 0<=v<=1 for u,v in uv)
    assert 'globalScale: 1\n' in Path(str(obj)+'.meta').read_text()
    results.append({'file':obj.name,'triangles':len(fs),'valid_indices_normals_uv':True,'base_y':min(v[1] for v in vs),'signed_volume':round(volume,5)})
for p in root.rglob('*'):
    if p.is_file() and p.suffix!='.meta':assert Path(str(p)+'.meta').exists(),p
(art/'validation.json').write_text(json.dumps(results,indent=2))
print(json.dumps(results,indent=2))
with zipfile.ZipFile(art/'CasseLowPoly-Unity-v3.zip','w',zipfile.ZIP_DEFLATED) as z:
    z.write(Path(str(root)+'.meta'),str(root)+'.meta')
    for p in root.rglob('*'):
        if p.is_file():z.write(p,p.as_posix())
    for name in ['README.txt','Casse_preview.png','model_report.json','validation.json']:
        z.write(art/name,name)
print('ZIP created.')

from pathlib import Path
import math,json,re,uuid,shutil,zipfile
from PIL import Image
OUT=Path('Assets/Environment/HayLowPoly'); ART=Path('Art/Environment/Hay-v1')
faces=[]
def face(p,uv=None):
    a,b,c=p[:3];u=[b[i]-a[i] for i in range(3)];v=[c[i]-a[i] for i in range(3)]
    n=(u[1]*v[2]-u[2]*v[1],u[2]*v[0]-u[0]*v[2],u[0]*v[1]-u[1]*v[0])
    l=math.sqrt(sum(x*x for x in n));assert l>1e-9;n=tuple(x/l for x in n)
    if uv is None:
        axis=max(range(3),key=lambda i:abs(n[i]))
        axes=[i for i in range(3) if i!=axis]
        uv=[(.06+.82*(v[axes[0]]+1.8)/3.6,.06+.82*(v[axes[1]]+1.8)/3.6) for v in p]
    for i in range(1,len(p)-1):
        ids=[0,i,i+1];faces.append(([p[j] for j in ids],[uv[j] for j in ids],n))
def save(name):
    lines=['# Y-up metres; ground pivot; triangulated','mtllib Hay.mtl','o '+name,'usemtl Hay','s off']
    for p,uv,n in faces:lines+=['v %.6f %.6f %.6f'%v for v in p]
    for p,uv,n in faces:lines+=['vt %.6f %.6f'%v for v in uv]
    for p,uv,n in faces:lines+=['vn %.6f %.6f %.6f'%n]*3
    for i in range(len(faces)):lines+=['f '+' '.join(f'{j}/{j}/{j}' for j in range(3*i+1,3*i+4))]
    (OUT/(name+'.obj')).write_text('\n'.join(lines)+'\n')
    vs=[v for p,_,_ in faces for v in p]
    return dict(name=name,triangles=len(faces),dimensions=[round(max(v[i] for v in vs)-min(v[i] for v in vs),3) for i in range(3)])
# Chamfered rectangular bale, longitudinal axis X. Cross section around YZ.
section=[(.08,-.48),(.70,-.48),(.80,-.38),(.80,.38),(.70,.48),(.08,.48),(0,.38),(0,-.38)]
rings=[[(x,.4+(y-.4)*s,z*s) for y,z in section] for x,s in [(-.8,.90),(-.70,1),(.70,1),(.8,.90)]]
for a,b in zip(rings,rings[1:]):
    for i in range(8):
        j=(i+1)%8;face([a[j],b[j],b[i],a[i]])
face(rings[0][::-1]);face(rings[-1])
# Two narrow binding straps, integrated in the same material and mesh.
for x in [-.43,.43]:
    for i in range(8):
        j=(i+1)%8
        p=[(xx,.4+(section[k][0]-.4)*1.012,section[k][1]*1.012) for xx,k in [(x-.018,i),(x+.018,i),(x+.018,j),(x-.018,j)]]
        face(p[::-1],[(.96,.8),(.985,.8),(.985,.2),(.96,.2)])
# Raise the binding underside onto the ground plane.
faces[:]=[([(x,y+.0048,z) for x,y,z in p],uv,n) for p,uv,n in faces]
report=[save('Fieno_01_Balla')]
faces.clear()
# Tall bell-shaped haystack: flared foot, steep sides, softly rounded crown.
N=12
profile=[(0,1,0,0),(.28,.96,.015,0),(1.30,.81,-.025,.015),
         (2.30,.62,-.065,.025),(2.85,.43,-.095,.015),(3.08,.20,-.105,.01)]
rings=[]
for y,r,cx,cz in profile:
    rings.append([(cx+1.55*r*(1+.035*math.sin(i*2.3))*math.cos(i*2*math.pi/N),
                   y,cz+1.40*r*(1+.035*math.cos(i*1.7))*math.sin(i*2*math.pi/N)) for i in range(N)])
# Continuous cylindrical UVs keep straw vertical over all side rings.
for k,(a,b) in enumerate(zip(rings,rings[1:])):
    for i in range(N):
        j=(i+1)%N
        u0=.04+.90*i/N;u1=.04+.90*(i+1)/N
        v0=.04+.90*profile[k][0]/3.14;v1=.04+.90*profile[k+1][0]/3.14
        face([a[i],b[i],b[j],a[j]],[(u0,v0),(u0,v1),(u1,v1),(u1,v0)])
face(rings[0])
tip=(-.105,3.14,.01)
for i in range(N):
    j=(i+1)%N
    face([rings[-1][i],tip,rings[-1][j]],
         [(.04+.90*i/N,.923),(.04+.90*(i+.5)/N,.94),(.04+.90*(i+1)/N,.923)])
report.append(save('Fieno_02_Mucchio'))
def meta(p,body):
    target=Path(str(p)+'.meta')
    if not target.exists():target.write_text('fileFormatVersion: 2\nguid: '+uuid.uuid4().hex+'\n'+body)
    return re.search(r'^guid: (\w+)',target.read_text(),re.M)[1]
meta(OUT,'folderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n')
src=Path('Assets/Environment/CratesLowPoly')
texmeta=(src/'CrateAtlas.png.meta').read_text()
texmeta=re.sub(r'^guid: \w+','guid: '+uuid.uuid4().hex,texmeta,flags=re.M)
tp=OUT/'HayPixel.png.meta'
if not tp.exists():tp.write_text(texmeta)
tg=re.search(r'^guid: (\w+)',tp.read_text(),re.M)[1]
mat=(src/'CrateAtlas.mat').read_text().replace('CrateAtlas','Hay').replace('6f5a4ed0d6fb4cc0ba2ba4168bf133c0',tg)
(OUT/'Hay.mat').write_text(mat)
mg=meta(OUT/'Hay.mat','NativeFormatImporter:\n  externalObjects: {}\n  mainObjectFileID: 2100000\n')
(OUT/'Hay.mtl').write_text('newmtl Hay\nKd 1 1 1\nKa 0 0 0\nKs 0 0 0\nd 1\nillum 1\nmap_Kd HayPixel.png\n')
meta(OUT/'Hay.mtl','DefaultImporter:\n  externalObjects: {}\n')
for item in report:
    p=OUT/(item['name']+'.obj.meta')
    if not p.exists():
        text=(src/'Cassa_01_Rinforzata.obj.meta').read_text()
        text=re.sub(r'^guid: \w+','guid: '+uuid.uuid4().hex,text,flags=re.M)
        text=text.replace('CrateAtlas','Hay').replace('ea04ba26069d465fb905cc95f60ec2e1',mg).replace('addColliders: 0','addColliders: 1')
        p.write_text(text)
(ART/'model_report.json').write_text(json.dumps(report,indent=2))
print(json.dumps(report,indent=2))

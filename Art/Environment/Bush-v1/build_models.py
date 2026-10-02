from pathlib import Path
import math,json,re,uuid
OUT=Path('Assets/Environment/BushLowPoly');ART=Path('Art/Environment/Bush-v1')
def meta(p,body):
    target=Path(str(p)+'.meta')
    if not target.exists():target.write_text('fileFormatVersion: 2\nguid: '+uuid.uuid4().hex+'\n'+body)
    return re.search(r'^guid: (\w+)',target.read_text(),re.M)[1]
def guid(p):return re.search(r'^guid: (\w+)',Path(str(p)+'.meta').read_text(),re.M)[1]
meta(OUT,'folderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n')
oak=Path('Assets/Environment/OakLowPolyTest');crate=Path('Assets/Environment/CratesLowPoly')
p=OUT/'BushPixel.png.meta'
if not p.exists():p.write_text(re.sub(r'^guid: \w+','guid: '+uuid.uuid4().hex,(oak/'FoliagePixel.png.meta').read_text(),flags=re.M))
tg=guid(OUT/'BushPixel.png')
mat=(oak/'Foliage.mat').read_text().replace('m_Name: Foliage','m_Name: BushFoliage').replace(guid(oak/'FoliagePixel.png'),tg)
(OUT/'BushFoliage.mat').write_text(mat)
mg=meta(OUT/'BushFoliage.mat','NativeFormatImporter:\n  externalObjects: {}\n  mainObjectFileID: 2100000\n')
(OUT/'Bush.mtl').write_text('newmtl BushFoliage\nKd 1 1 1\nKs 0 0 0\nd 1\nillum 1\nmap_Kd BushPixel.png\nmap_d BushPixel.png\n')
meta(OUT/'Bush.mtl','DefaultImporter:\n  externalObjects: {}\n')
faces=[]
def face(p,uv):
    a,b,c=p[:3];u=[b[i]-a[i] for i in range(3)];v=[c[i]-a[i] for i in range(3)]
    n=(u[1]*v[2]-u[2]*v[1],u[2]*v[0]-u[0]*v[2],u[0]*v[1]-u[1]*v[0])
    length=math.sqrt(sum(x*x for x in n));assert length>1e-8
    n=tuple(x/length for x in n)
    for i in range(1,len(p)-1):
        ids=[0,i,i+1];faces.append(([p[j] for j in ids],[uv[j] for j in ids],n))
def tuft(x,y,z,r,h,angle):
    # Bent radial card: visible from overhead, plus two side cards for perspective.
    ring=[(x+r*math.cos(angle+i*math.pi/4),y,z+r*math.sin(angle+i*math.pi/4)) for i in range(8)]
    uv=[(.5+.5*math.cos(i*math.pi/4),.5+.5*math.sin(i*math.pi/4)) for i in range(8)]
    for i in range(8):
        j=(i+1)%8;face([ring[i],(x,y+h,z),ring[j]],[uv[i],(.5,.5),uv[j]])
    for theta in [angle,angle+math.pi/2]:
        dx=r*.86*math.cos(theta);dz=r*.86*math.sin(theta)
        face([(x-dx,max(0,y-h*.8),z-dz),(x+dx,max(0,y-h*.8),z+dz),
              (x+dx,y+h*.75,z+dz),(x-dx,y+h*.75,z-dz)],[(0,0),(1,0),(1,1),(0,1)])
models=[
('Cespuglio_01_Compatto',[(0,.58,0,.75,.40,0),(-.42,.30,0,.58,.30,.7),(.38,.29,.08,.57,.28,1.5),(0,.31,-.40,.60,.28,2.4),(0,.27,.40,.59,.28,3.1)]),
('Cespuglio_02_Largo',[(-.65,.34,0,.78,.38,.3),(.10,.46,.05,.82,.39,1.2),(.73,.28,-.07,.63,.30,2),(-.80,.20,.44,.55,.25,2.8),(.54,.19,.45,.70,.26,3.4),(-.28,.24,-.49,.73,.29,4.2),(.64,.22,-.44,.51,.24,5)]),
('Cespuglio_03_Ciuffi',[(-.53,.40,.22,.67,.38,0),(.49,.52,.17,.74,.42,.8),(0,.92,-.30,.72,.46,1.8),(-.46,.62,-.43,.56,.35,2.5),(.42,.64,-.42,.58,.33,3.4),(-.75,.19,.10,.47,.25,4.1),(.79,.23,.10,.49,.25,4.8),(0,.25,.56,.63,.30,5.4)])]
report=[]
for name,tufts in models:
    faces.clear()
    for t in tufts:tuft(*t)
    lines=['# metres Y up ground pivot','mtllib Bush.mtl','o '+name,'usemtl BushFoliage','s off']
    for p,uv,n in faces:lines+=['v %.6f %.6f %.6f'%v for v in p]
    for p,uv,n in faces:lines+=['vt %.6f %.6f'%v for v in uv]
    for p,uv,n in faces:lines+=['vn %.6f %.6f %.6f'%n]*3
    for i in range(len(faces)):lines+=['f '+' '.join(f'{j}/{j}/{j}' for j in range(i*3+1,i*3+4))]
    obj=OUT/(name+'.obj');obj.write_text('\n'.join(lines)+'\n')
    mp=Path(str(obj)+'.meta')
    if not mp.exists():
        s=(crate/'Cassa_01_Rinforzata.obj.meta').read_text()
        s=re.sub(r'^guid: \w+','guid: '+uuid.uuid4().hex,s,flags=re.M)
        s=s.replace('CrateAtlas','BushFoliage').replace(guid(crate/'CrateAtlas.mat'),mg)
        mp.write_text(s)
    vs=[v for p,_,_ in faces for v in p]
    lo=[min(v[i] for v in vs) for i in range(3)];hi=[max(v[i] for v in vs) for i in range(3)]
    size=[hi[i]-lo[i] for i in range(3)]
    report.append(dict(name=name,triangles=len(faces),dimensions=[round(x,3) for x in size]))
    # Conservative static box; foliage cards remain visual, with no per-leaf collider.
    s=(crate/'Cassa_01_Rinforzata.prefab').read_text().replace('Cassa_01_Rinforzata',name).replace(guid(crate/'Cassa_01_Rinforzata.obj'),guid(obj)).replace(guid(crate/'CrateAtlas.mat'),mg)
    s=re.sub(r'  m_Size:.*','  m_Size: {x: %.4f, y: %.4f, z: %.4f}'%(size[0]*.65,hi[1]*.7,size[2]*.65),s)
    s=re.sub(r'  m_Center:.*','  m_Center: {x: %.4f, y: %.4f, z: %.4f}'%((lo[0]+hi[0])/2,hi[1]*.35,(lo[2]+hi[2])/2),s)
    prefab=OUT/(name+'.prefab');prefab.write_text(s)
    meta(prefab,'PrefabImporter:\n  externalObjects: {}\n')
(ART/'model_report.json').write_text(json.dumps(report,indent=2))
print(json.dumps(report,indent=2))

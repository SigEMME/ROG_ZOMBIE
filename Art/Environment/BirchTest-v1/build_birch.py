"""Birch reference study: solid slender wood and upward-facing foliage cards."""
from pathlib import Path
import sys,math,json
sys.path.insert(0,str(Path(__file__).resolve().parent.parent/'OakTest-v1'))
from build_oak import faces,tube,card,triangle
ROOT=Path(__file__).resolve().parents[3]
OUT=ROOT/'Assets/Environment/BirchLowPolyTest'
OUT.mkdir(parents=True,exist_ok=True)
def build(name='Betulla_Test_01',variant=0):
    faces.clear()
    tube([(0,0,0),(.06,1.9,0),(-.20,3.4,.04),(-.55,5.2,.08),(-.25,6.9,.05)],[.24,.19,.14,.08,.025],6)
    tube([(.02,2.2,0),(.65,3.6,.06),(1.20,5.2,.12),(1.0,6.1,.20)],[.15,.11,.06,.02],4)
    for start,tip in [((-.2,3.3,0),(-1.35,4.3,.3)),((-.5,4.9,0),(-1.55,5.5,-.3)),((-.4,5.5,.1),(-.7,6.6,-.65)),((.65,3.6,.05),(1.7,4.7,.65)),((1.1,5.0,.12),(1.8,5.6,-.5)),((-.15,3.6,0),(-.2,4.7,1.15))]:
        tube([start,tip],[.07,.012],4)
    clusters=[(-.25,6.9,.05,1.6),(-.75,6.25,-.55,1.65),(1.0,6.0,.20,1.85),(-1.45,5.45,-.25,1.85),(1.7,5.55,-.5,1.9),(-.4,5.3,.25,1.8),(-1.25,4.35,.3,1.8),(1.55,4.7,.7,1.75),(-.2,4.65,1.15,1.65),(.25,3.55,.35,1.35),(.35,5.45,-1.0,1.7)]
    if variant==1:
        clusters=[(x,y,z,w*.93) for i,(x,y,z,w) in enumerate(clusters) if i!=9]
    elif variant==2:
        clusters=[(x*1.08,y+(.22 if i%3==0 else -.08),z,w*(1.1 if i%2==0 else .88)) for i,(x,y,z,w) in enumerate(clusters)]
    for k,(x,y,z,w) in enumerate(clusters):
        for i,tilt in enumerate([0,30,-35]):
            angle=k*.71+i*1.6;t=math.radians(tilt)
            u=(math.cos(angle),0,math.sin(angle))
            v=(math.sin(angle)*math.cos(t),math.sin(t),-math.cos(angle)*math.cos(t))
            offset=(i-1)*.13
            card((x+u[0]*offset,y+i*.09,z+u[2]*offset),u,v,w,w*.85)
    if variant:
        original=list(faces);faces.clear()
        sx,sy,sz=(.76,.67,.76) if variant==1 else (1.20,1.04,1.08)
        for points,uv,_,material in original:
            points=[(x*sx+(.35*(y/7)**2 if variant==2 else 0),y*sy,z*sz) for x,y,z in points]
            triangle(points,uv,material)
    lines=['# Birch: Y up, metres, base pivot; foliage consists of flat cards','mtllib Tree.mtl','o '+name,'s off']
    for p,uv,n,m in faces:
        lines.extend('v '+' '.join(f'{x:.6f}' for x in v) for v in p)
        lines.extend('vt '+' '.join(f'{x:.6f}' for x in v) for v in uv)
        lines.extend(['vn '+' '.join(f'{x:.6f}' for x in n)]*3)
    last=None
    for i,(_,_,n,m) in enumerate(faces):
        if m=='Foliage':assert n[1]>0
        if m!=last:lines.append('usemtl '+m);last=m
        lines.append('f '+' '.join(f'{j}/{j}/{j}' for j in range(i*3+1,i*3+4)))
    (OUT/(name+'.obj')).write_text('\n'.join(lines)+'\n')
    foliage=sum(f[3]=='Foliage' for f in faces)
    report={'wood_triangles':sum(f[3]=='Bark' for f in faces),'foliage_cards':foliage//2,'foliage_triangles':foliage,'triangles':len(faces),'bounds_min':[min(p[j] for f in faces for p in f[0]) for j in range(3)],'bounds_max':[max(p[j] for f in faces for p in f[0]) for j in range(3)]}
    report_file='model-report.json' if variant==0 else name+'-report.json'
    (Path(__file__).parent/report_file).write_text(json.dumps(report,indent=2));print(report)
if __name__=='__main__':build()

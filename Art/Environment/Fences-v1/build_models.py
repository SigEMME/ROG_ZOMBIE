"""Rebuild original low-poly fence geometry. Python standard library only."""
from pathlib import Path
import math
import json

ROOT = Path(__file__).resolve().parents[3]
OUT = ROOT / 'Assets/Environment/FencesLowPoly'
OUT.mkdir(parents=True, exist_ok=True)

def sub(a,b): return tuple(x-y for x,y in zip(a,b))
def cross(a,b): return (a[1]*b[2]-a[2]*b[1],a[2]*b[0]-a[0]*b[2],a[0]*b[1]-a[1]*b[0])

class Model:
    def __init__(self): self.faces=[]
    def face(self, points, uv=None):
        n=cross(sub(points[1],points[0]),sub(points[2],points[0]))
        length=math.sqrt(sum(x*x for x in n)); assert length>1e-8
        n=tuple(x/length for x in n)
        uv=uv or [(p[0]*.37+.23,p[1]*.51) for p in points]
        for i in range(1,len(points)-1):
            self.faces.append(([points[j] for j in (0,i,i+1)],[uv[j] for j in (0,i,i+1)],n))
    def prism(self, outline, depth, z=0, angle=0, center=(0,0)):
        # Outline is counterclockwise in XY; extrude on Z with outward normals.
        c,s=math.cos(angle),math.sin(angle)
        def pos(p,d): return (center[0]+c*p[0]-s*p[1],center[1]+s*p[0]+c*p[1],z+d)
        back=[pos(p,-depth/2) for p in outline]; front=[pos(p,depth/2) for p in outline]
        uv=[(p[0]*.5+.31,p[1]*.6+.17) for p in outline]
        self.face(front,uv); self.face(back[::-1],uv[::-1])
        for i in range(len(outline)):
            j=(i+1)%len(outline)
            self.face([back[i],back[j],front[j],front[i]],[(0,0),(0,1),(.15,1),(.15,0)])
    def box(self,x,y,w,h,d=.16,z=0,angle=0):
        self.prism([(-w/2,-h/2),(w/2,-h/2),(w/2,h/2),(-w/2,h/2)],d,z,angle,(x,y))
    def beam(self,a,b,width=.15,z=.17):
        dx,dy=b[0]-a[0],b[1]-a[1]
        self.box((a[0]+b[0])/2,(a[1]+b[1])/2,width,math.hypot(dx,dy),.13,z,math.atan2(-dx,dy))
    def save(self,name):
        rows=['# metres; Y up; base-centred pivot; one mesh/material','mtllib Wood.mtl','o '+name,'usemtl Wood','s off']
        for points,uv,n in self.faces:
            rows.extend('v '+' '.join(f'{a:.6f}' for a in p) for p in points)
            rows.extend('vt '+' '.join(f'{a:.6f}' for a in p) for p in uv)
            rows.extend(['vn '+' '.join(f'{a:.6f}' for a in n)]*3)
        for i in range(len(self.faces)):
            ids=range(i*3+1,i*3+4); rows.append('f '+' '.join(f'{j}/{j}/{j}' for j in ids))
        (OUT/(name+'.obj')).write_text('\n'.join(rows)+'\n',encoding='utf-8')
        return {'triangles':len(self.faces),'vertices_before_import_welding':len(self.faces)*3,'materials':1}

models={}
for kind in ('Piena','Aperta','Rinforzata','Danneggiata'):
    m=Model()
    for x,h in [(-1.9,1.85),(0,1.78),(1.9,1.82)]: m.box(x,h/2,.2,h,.24)
    if kind in ('Piena','Danneggiata'):
        for i in range(12):
            if kind=='Danneggiata' and i==7: continue
            x=-1.69+i*.307; h=[1.52,1.6,1.47,1.57][i%4]
            if kind=='Danneggiata' and i in (3,4,8):
                h=[1.03,.78,1.16][(3,4,8).index(i)]
                m.prism([(-.14,0),(.14,0),(.14,h-.16),(.02,h),(-.14,h-.07)],.12,center=(x,.12))
            else:
                m.prism([(-.14,0),(.14,0),(.14,h-.025),(-.14,h)],.12,center=(x,.12))
        for y in (.48,1.23):
            if kind=='Danneggiata':
                m.beam((-1.98,y),(.13,y-.04)); m.beam((.53,y-.12),(1.98,y-.07))
            else: m.beam((-1.98,y),(1.98,y-.05))
    else:
        for y in ((.35,.77,1.2,1.57) if kind=='Aperta' else (.35,1.55)):
            m.beam((-1.98,y),(0,y-.045)); m.beam((0,y-.045),(1.98,y+.015))
        if kind=='Rinforzata':
            for a,b in [(-1.82,-.1),(.1,1.82)]:
                m.beam((a,.4),(b,1.5),.13,-.08)
                m.beam((a,1.5),(b,.4),.13,-.23)
    models['Staccionata_'+kind]=m

(OUT/'Wood.mtl').write_text('newmtl Wood\nKa 0.2 0.2 0.2\nKd 1 1 1\nKs 0 0 0\nd 1\nillum 1\nmap_Kd WoodPixel.png\n')
report={name:m.save(name) for name,m in models.items()}
(Path(__file__).parent/'models-report.json').write_text(json.dumps(report,indent=2)+'\n')
print(json.dumps(report,indent=2))

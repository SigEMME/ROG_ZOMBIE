"""Top-down oak: solid trunk/branches and upward-facing alpha foliage cards."""
from pathlib import Path
import math,json,sys
sys.path.insert(0,str(Path(__file__).resolve().parent.parent/'TreeTest-v1'))
from build_tree import faces,triangle,cross,sub
ROOT=Path(__file__).resolve().parents[3]
OUT=ROOT/'Assets/Environment/OakLowPolyTest'
OUT.mkdir(parents=True,exist_ok=True)
def norm(v):
    length=math.sqrt(sum(x*x for x in v));return tuple(x/length for x in v)
def tube(points,radii,sides):
    rings=[]
    for k,p in enumerate(points):
        axis=norm(sub(points[min(k+1,len(points)-1)],points[max(0,k-1)]))
        a=norm(cross(axis,(0,0,1)));b=cross(axis,a)
        rings.append([tuple(p[j]+radii[k]*(a[j]*math.cos(i*math.tau/sides)+b[j]*math.sin(i*math.tau/sides)) for j in range(3)) for i in range(sides)])
        if k==0 and p[1]==0:
            rings[-1]=[(x,0,z) for x,y,z in rings[-1]]
    for k in range(len(rings)-1):
        for i in range(sides):
            j=(i+1)%sides
            p=[rings[k][i],rings[k][j],rings[k+1][j],rings[k+1][i]]
            uv=[(i/sides,k*.5),((i+1)/sides,k*.5),((i+1)/sides,(k+1)*.5),(i/sides,(k+1)*.5)]
            for ids in [(0,1,2),(0,2,3)]:triangle([p[t] for t in ids],[uv[t] for t in ids],'Bark')
    for k,flip in [(0,True),(len(rings)-1,False)]:
        for i in range(sides):
            p=[points[k],rings[k][i],rings[k][(i+1)%sides]]
            if flip:p.reverse()
            triangle(p,[(v[0]*.5,v[2]*.5) for v in p],'Bark')
def card(center,u,v,width,height):
    p=[tuple(center[j]+su*u[j]*width*.5+sv*v[j]*height*.5 for j in range(3)) for su,sv in [(-1,-1),(1,-1),(1,1),(-1,1)]]
    uv=[(0,0),(1,0),(1,1),(0,1)]
    for ids in [(0,1,2),(0,2,3)]:triangle([p[t] for t in ids],[uv[t] for t in ids],'Foliage')
def build(name='Quercia_Test_01',variant=0):
    faces.clear()
    tube([(0,0,0),(.04,.4,0),(-.10,1.6,.03),(.20,2.7,.02),(.07,3.9,0)],[.50,.38,.32,.22,.08],6)
    branches=[((-1.8,4.0,.2),(-.85,3.05,.05)),((1.8,4.15,.35),(.92,3.1,.25)),((.3,4.3,-1.45),(.15,3.15,-.8)),((-.4,4.2,1.5),(-.2,3.05,.8)),((.75,5.0,.05),(.45,3.9,.02))]
    for tip,mid in branches:tube([(.05,2.25,0),mid,tip],[.21,.13,.04],4)
    clusters=[(-1.8,4.15,.2,2.65,2.1),(1.8,4.25,.35,2.7,2.2),(.3,4.4,-1.45,2.7,2.0),(-.4,4.3,1.5,2.7,2.05),(.45,5.1,.05,2.9,2.1),(-.85,5.0,-.6,2.5,1.9),(.15,4.25,0,2.7,2.1),(1.1,4.8,1.0,2.2,1.8)]
    if variant==1:
        clusters=[(x*1.14,y+(.16 if i%2==0 else -.10),z*1.12,w*1.08,h) for i,(x,y,z,w,h) in enumerate(clusters)]
    elif variant==2:
        clusters=[(x+(.28 if i%2 else -.12),y+(.25 if i in (4,5) else 0),z,w*(.88 if i in (0,3) else 1),h) for i,(x,y,z,w,h) in enumerate(clusters) if i!=6]
    for k,(x,y,z,w,h) in enumerate(clusters):
        # Three upward-facing cards replace five crossed cards per cluster.
        # Offset tilted layers add depth without edge-on planes in a top-down camera.
        for i in range(2):
            angle=i*math.pi/2+k*.63
            tilt=math.radians(28 if i==0 else -32)
            u=(math.cos(angle),0,math.sin(angle))
            v=(math.sin(angle)*math.cos(tilt),math.sin(tilt),-math.cos(angle)*math.cos(tilt))
            offset=.20 if i==0 else -.18
            card((x+u[0]*offset,y+.20+i*.18,z+u[2]*offset),u,v,w*.94,w*.86)
        a=k*.37
        card((x,y+.58,z),(math.cos(a),0,math.sin(a)),(math.sin(a),0,-math.cos(a)),w,w*.88)
    if variant:
        original=list(faces);faces.clear()
        sx,sy,sz=(1.14,.88,1.14) if variant==1 else (.82,1.19,.86)
        for points,uv,_,material in original:
            points=[(x*sx+(.27*(y/6)**2 if variant==2 else 0),y*sy,z*sz) for x,y,z in points]
            triangle(points,uv,material)
    lines=['# Oak, Y up, metres, base pivot; foliage uses only flat cards','mtllib Tree.mtl','o '+name,'s off']
    for p,uv,n,m in faces:
        lines.extend('v '+' '.join(f'{x:.6f}' for x in v) for v in p)
        lines.extend('vt '+' '.join(f'{x:.6f}' for x in v) for v in uv)
        lines.extend(['vn '+' '.join(f'{x:.6f}' for x in n)]*3)
    last=None
    for i,(_,_,_,m) in enumerate(faces):
        if m!=last:lines.append('usemtl '+m);last=m
        lines.append('f '+' '.join(f'{j}/{j}/{j}' for j in range(i*3+1,i*3+4)))
    (OUT/(name+'.obj')).write_text('\n'.join(lines)+'\n')
    foliage_triangles=sum(f[3]=='Foliage' for f in faces)
    assert foliage_triangles==(42 if variant==2 else 48)
    assert all(n[1]>0 for _,_,n,m in faces if m=='Foliage')
    report={'wood_triangles':sum(f[3]=='Bark' for f in faces),'foliage_cards':foliage_triangles//2,'foliage_triangles':foliage_triangles,'triangles':len(faces),'bounds_min':[min(p[j] for f in faces for p in f[0]) for j in range(3)],'bounds_max':[max(p[j] for f in faces for p in f[0]) for j in range(3)]}
    report_path='model-report.json' if variant==0 else name+'-report.json'
    (Path(__file__).parent/report_path).write_text(json.dumps(report,indent=2))
    print(report)
if __name__=='__main__':build()

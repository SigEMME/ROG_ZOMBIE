from pathlib import Path
import math,json,uuid,re
OUT=Path('Assets/Environment/FarmBuildingsLowPoly')
ART=Path('Art/Environment/FarmBuildings-v1')
faces=[]
def sub(a,b):return tuple(x-y for x,y in zip(a,b))
def add(a,b):return tuple(x+y for x,y in zip(a,b))
def mul(a,t):return tuple(x*t for x in a)
def dot(a,b):return sum(x*y for x,y in zip(a,b))
def cross(a,b):return (a[1]*b[2]-a[2]*b[1],a[2]*b[0]-a[0]*b[2],a[0]*b[1]-a[1]*b[0])
def unit(a):
    length=math.sqrt(dot(a,a));assert length>1e-8
    return mul(a,1/length)
def face(p,mat=0,grain=None):
    n=unit(cross(sub(p[1],p[0]),sub(p[2],p[0])))
    if grain is None:
        # On walls preserve vertical board grain, roof uses its downhill direction.
        grain=(0,1,0) if abs(n[1])<.7 else (0,0,1)
    along=sub(grain,mul(n,dot(grain,n)))
    if dot(along,along)<1e-8:along=sub(p[1],p[0])
    along=unit(along);across=cross(along,n)
    coords=[(dot(v,across),dot(v,along)) for v in p]
    u0=min(v[0] for v in coords);v0=min(v[1] for v in coords)
    span=max(max(v[0] for v in coords)-u0,max(v[1] for v in coords)-v0)
    # Fixed maximum texel density; no squashing on narrow frame beams.
    scale=min(.42/span,.17)
    ox=.5*(mat%2);oy=.5*(1-mat//2)
    uv=[(ox+.04+(u-u0)*scale,oy+.04+(v-v0)*scale) for u,v in coords] if mat<4 else [(0,0)]*len(p)
    for i in range(1,len(p)-1):
        ids=(0,i,i+1);q=[p[j] for j in ids]
        nn=unit(cross(sub(q[1],q[0]),sub(q[2],q[0])))
        faces.append((q,[uv[j] for j in ids],nn,'FarmAtlas' if mat<4 else 'Dark'))
def box(c,size,mat=0):
    x,y,z=c;w,h,d=[v/2 for v in size]
    p=[(x+xx,y+yy,z+zz) for xx,yy,zz in [(-w,-h,-d),(w,-h,-d),(w,h,-d),(-w,h,-d),(-w,-h,d),(w,-h,d),(w,h,d),(-w,h,d)]]
    for ids in [(0,3,2,1),(4,5,6,7),(0,4,7,3),(1,2,6,5),(0,1,5,4),(3,7,6,2)]:
        face([p[i] for i in ids],mat)
def beam(a,b,width,mat=1):
    axis=unit(sub(b,a))
    ref=(0,0,1) if abs(axis[2])<.9 else (0,1,0)
    u=mul(unit(cross(axis,ref)),width/2);v=mul(unit(cross(axis,u)),width/2)
    p=[add(c,add(mul(u,su),mul(v,sv))) for c in [a,b] for su,sv in [(-1,-1),(1,-1),(1,1),(-1,1)]]
    for ids in [(0,3,2,1),(4,5,6,7),(0,1,5,4),(1,2,6,5),(2,3,7,6),(3,0,4,7)]:
        face([p[i] for i in ids],mat,axis)
def panel(center,right,w,h,mat):
    x,y,z=center
    u=mul(right,w/2);v=(0,h/2,0)
    face([sub(sub(center,u),v),sub(add(center,u),v),add(add(center,u),v),add(sub(center,u),v)],mat)
def window(c,right,w=.8,h=1.15,door=False):
    n=cross(right,(0,1,0));c=add(c,mul(n,.022))
    panel(c,right,w,h,0 if door else 4)
    c=add(c,mul(n,.012));t=.10
    for sign in [-1,1]:
        panel(add(c,mul(right,sign*(w/2+t/2))),right,t,h+2*t,1)
        panel(add(c,(0,sign*(h/2+t/2),0)),right,w,t,1)
    if not door:
        panel(c,right,.055,h,1)
        panel(c,right,w,.055,1)
    else:
        # A low-poly geometric diagonal brace on each leaf.
        for sign in [-1,1]:
            a=add(c,add(mul(right,sign*w*.43),(0,-h*.40,0)))
            b=add(c,add(mul(right,sign*.05),(0,h*.40,0)))
            beam(a,b,.065,1)
        panel(c,right,.07,h,1)
def building(w,d,h,rise,mat=0,gambrel=False,center=(0,0),barn=False,bays=2,door_centers=None,door_width=None):
    cx,cz=center
    box((cx,.18,cz),(w,.36,d),3)
    box((cx,h/2+.18,cz),(w,h-.36,d),mat)
    roof=[(-w/2-.20,h)]
    if gambrel:roof += [(-w*.31,h+rise*.72),(0,h+rise)]
    else:roof += [(0,h+rise)]
    if gambrel:roof += [(w*.31,h+rise*.72)]
    roof += [(w/2+.20,h)]
    # Gable triangles/convex polygons, front +Z and rear -Z.
    gable=[(cx+x,y,cz+d/2) for x,y in roof]
    face(gable[::-1],mat)
    face([(x,y,cz-d/2) for x,y,_ in gable],mat)
    # One continuous, equally scaled atlas region over the whole main roof.
    # Segments retain their geometry but no longer restart the texture.
    roof_distance=0
    roof_scale=.42/max(d+.48,sum(math.dist(a,b) for a,b in zip(roof,roof[1:])))
    for (x0,y0),(x1,y1) in zip(roof,roof[1:]):
        count=max(3,round(d/1.3))
        for i in range(count):
            z0=cz-d/2-.24+(d+.48)*i/count;z1=cz-d/2-.24+(d+.48)*(i+1)/count
            p=[(cx+x0,y0+.055,z0),(cx+x0,y0+.055,z1),(cx+x1,y1+.055,z1),(cx+x1,y1+.055,z0)]
            face(p,2,(x1-x0,y1-y0,0))
            run=math.hypot(x1-x0,y1-y0)
            uv=[(.04+(z-(cz-d/2-.24))*roof_scale,
                 .04+(roof_distance+(run if k>=2 else 0))*roof_scale) for k,(_,_,z) in enumerate(p)]
            for offset,ids in enumerate([(0,1,2),(0,2,3)]):
                q,_,n,m=faces[-2+offset]
                faces[-2+offset]=(q,[uv[j] for j in ids],n,m)
        roof_distance+=math.hypot(x1-x0,y1-y0)
        # underside and fascia, no paper-thin silhouette.
        z0=cz-d/2-.24;z1=cz+d/2+.24
        top=[(cx+x0,y0+.055,z0),(cx+x0,y0+.055,z1),(cx+x1,y1+.055,z1),(cx+x1,y1+.055,z0)]
        bottom=[(x,y-.09,z) for x,y,z in top]
        face(bottom[::-1],2,(x1-x0,y1-y0,0))
        for j in range(4):
            k=(j+1)%4;face([top[j],bottom[j],bottom[k],top[k]],2)
    # Pale weathered trims, replacing the pristine white paint of reference 1.
    for sx in [-1,1]:
        for sz in [-1,1]:
            beam((cx+sx*(w/2+.025),.25,cz+sz*(d/2+.025)),(cx+sx*(w/2+.025),h,cz+sz*(d/2+.025)),.13,1)
    for sz in [-1,1]:
        for a,b in zip(roof,roof[1:]):
            beam((cx+a[0],a[1],cz+sz*(d/2+.25)),(cx+b[0],b[1],cz+sz*(d/2+.25)),.12,1)
    for sx in [-1,1]:
        beam((cx+sx*w/2,h-.04,cz-d/2),(cx+sx*w/2,h-.04,cz+d/2),.13,1)
    for offset in (door_centers if door_centers is not None else [0]):
        window((cx+offset,1.55 if barn else 1.3,cz+d/2),(1,0,0),
               door_width if door_width is not None else (w*.48 if barn else 1.1),
               2.6 if barn else 2.2,True)
    window((cx,h+rise*.32,cz+d/2),(1,0,0),.8,.65)
    for sx in [-1,1]:
        for frac in ([-.28,.25] if bays==2 else [-.36+.72*i/(bays-1) for i in range(bays)]):
            window((cx+sx*w/2,h*.56,cz+d*frac),(0,0,-sx),.85,1.15)
    if barn and h>4:
        for sx in [-1,1]:window((cx+sx*w*.29,h*.73,cz+d/2),(1,0,0),.75,1.05)
    return roof
def save(name):
    lines=['# ROG ZOMBIE - metres, Y up, base pivot','mtllib Farm.mtl','o '+name,'s off']
    for p,uv,n,m in faces:lines += ['v %.6f %.6f %.6f'%v for v in p]
    for p,uv,n,m in faces:lines += ['vt %.6f %.6f'%v for v in uv]
    for p,uv,n,m in faces:lines += ['vn %.6f %.6f %.6f'%n]*3
    material=None
    for i,(_,_,_,m) in enumerate(faces):
        if m!=material:lines.append('usemtl '+m);material=m
        lines.append('f '+' '.join(f'{j}/{j}/{j}' for j in range(3*i+1,3*i+4)))
    (OUT/(name+'.obj')).write_text('\n'.join(lines)+'\n')
    vs=[v for p,_,_,_ in faces for v in p]
    return dict(name=name,triangles=len(faces),dimensions=[round(max(v[i] for v in vs)-min(v[i] for v in vs),3) for i in range(3)])
def meta(p,body):
    q=Path(str(p)+'.meta')
    if not q.exists():q.write_text('fileFormatVersion: 2\nguid: '+uuid.uuid4().hex+'\n'+body)
    return re.search(r'^guid: (\w+)',q.read_text(),re.M)[1]
def build_base():
    report=[]
    building(6,8,4.7,2.8,0,True,barn=True)
    # Attached low lean-to: wooden shell and sloping metal roof.
    box((-4.3,1.25,-.6),(2.6,2.5,4.8),0)
    p=[(-5.75,2.6,-3.2),(-5.75,2.6,2),(-2.9,3.6,2),(-2.9,3.6,-3.2)]
    face(p,2,(2.85,1,0));face([(x,y-.09,z) for x,y,z in p][::-1],2)
    for i in range(4):
        j=(i+1)%4;face([p[i],(p[i][0],p[i][1]-.09,p[i][2]),(p[j][0],p[j][1]-.09,p[j][2]),p[j]],2)
    for z in [-3,1.8]:
        q=[(-5.6,2.5,z),(-3,2.5,z),(-3,3.52,z),(-5.6,2.6,z)]
        face(q if z>0 else q[::-1],0)
    for z in [-3.2,2]:beam((-5.75,2.6,z),(-2.9,3.6,z),.12,1)
    window((-4.3,1.2,1.8),(1,0,0),1,1.2)
    report.append(save('Fienile_01_Annesso'));faces.clear()
    building(4.6,6,3.15,1.7,0,False,barn=True)
    report.append(save('Rimessa_02_Rurale'));faces.clear()
    building(5.4,6.6,2.85,1.9,1)
    window((-1.65,1.5,3.3),(1,0,0),.78,1.10)
    window((1.65,1.5,3.3),(1,0,0),.78,1.10)
    box((0,.12,3.66),(1.5,.24,.7),3)
    box((-1.4,4.35,-1.9),(.48,2,.48),3)
    box((-1.4,5.36,-1.9),(.62,.14,.62),3)
    panel((-1.4,5.42,-1.9),(1,0,0),.30,.04,4)
    report.append(save('Casa_03_Abbandonata'))
    (ART/'model_report.json').write_text(json.dumps(report,indent=2))
    for p in [OUT,OUT/'Editor']:meta(p,'folderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n')
    source=Path('Assets/Environment/CratesLowPoly')
    texturemeta=(source/'CrateAtlas.png.meta').read_text()
    texturemeta=re.sub(r'^guid: \w+','guid: '+uuid.uuid4().hex,texturemeta,count=1,flags=re.M)
    if not (OUT/'FarmAtlas.png.meta').exists():(OUT/'FarmAtlas.png.meta').write_text(texturemeta)
    tg=re.search(r'^guid: (\w+)',(OUT/'FarmAtlas.png.meta').read_text(),re.M)[1]
    oldtg=re.search(r'^guid: (\w+)',(source/'CrateAtlas.png.meta').read_text(),re.M)[1]
    mat=(source/'CrateAtlas.mat').read_text().replace('CrateAtlas','FarmAtlas').replace(oldtg,tg)
    if not (OUT/'FarmAtlas.mat').exists():(OUT/'FarmAtlas.mat').write_text(mat)
    mg=meta(OUT/'FarmAtlas.mat','NativeFormatImporter:\n  externalObjects: {}\n  mainObjectFileID: 2100000\n')
    dark=mat.replace('m_Name: FarmAtlas','m_Name: Dark')
    dark=dark.replace('{fileID: 2800000, guid: '+tg+', type: 3}','{fileID: 0}')
    dark=re.sub(r'(_BaseColor|_Color): \{[^\n]+\}',r'\1: {r: 0.035, g: 0.028, b: 0.021, a: 1}',dark)
    if not (OUT/'Dark.mat').exists():(OUT/'Dark.mat').write_text(dark)
    dg=meta(OUT/'Dark.mat','NativeFormatImporter:\n  externalObjects: {}\n  mainObjectFileID: 2100000\n')
    (OUT/'Farm.mtl').write_text('newmtl FarmAtlas\nKd 1 1 1\nmap_Kd FarmAtlas.png\nnewmtl Dark\nKd 0.035 0.028 0.021\n')
    meta(OUT/'Farm.mtl','DefaultImporter:\n  externalObjects: {}\n')
    template=(source/'Cassa_01_Rinforzata.obj.meta').read_text()
    oldmg=re.search(r'^guid: (\w+)',(source/'CrateAtlas.mat.meta').read_text(),re.M)[1]
    for r in report:
        path=OUT/(r['name']+'.obj.meta')
        if path.exists():continue
        s=re.sub(r'^guid: \w+','guid: '+uuid.uuid4().hex,template,count=1,flags=re.M)
        s=s.replace('name: CrateAtlas','name: FarmAtlas').replace(oldmg,mg)
        s=s.replace('  materials:\n','  - first:\n      type: UnityEngine:Material\n      assembly: UnityEngine.CoreModule\n      name: Dark\n    second: {fileID: 2100000, guid: '+dg+', type: 2}\n  materials:\n')
        path.write_text(s)
    print(json.dumps(report,indent=2))

if __name__ == "__main__":build_base()

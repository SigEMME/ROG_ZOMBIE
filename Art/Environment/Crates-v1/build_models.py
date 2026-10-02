from pathlib import Path
import math, json, uuid, re
OUT=Path('Assets/Environment/CratesLowPoly')
ART=Path('Art/Environment/Crates-v1')
faces=[]
def sub(a,b):return tuple(x-y for x,y in zip(a,b))
def cross(a,b):return (a[1]*b[2]-a[2]*b[1],a[2]*b[0]-a[0]*b[2],a[0]*b[1]-a[1]*b[0])
def face(p,mat=0):
    n=cross(sub(p[1],p[0]),sub(p[2],p[0]));l=math.sqrt(sum(x*x for x in n))
    assert l>1e-8
    n=tuple(x/l for x in n)
    # Planar projection preserves trapezoid shape in UV space. Mapping these
    # frames to rectangles bends the grain at the triangulation diagonal.
    ox=.5*(mat%2);oy=.5*(1-mat//2)
    edges=[sub(p[(i+1)%len(p)],p[i]) for i in range(len(p))]
    along=max(edges,key=lambda e:sum(v*v for v in e))
    length=math.sqrt(sum(v*v for v in along))
    along=tuple(v/length for v in along)
    across=cross(along,n)
    projected=[(sum(v[i]*across[i] for i in range(3)),sum(v[i]*along[i] for i in range(3))) for v in p]
    umin=min(v[0] for v in projected);vmin=min(v[1] for v in projected)
    extent=max(max(v[0] for v in projected)-umin,max(v[1] for v in projected)-vmin)
    # Grain runs in atlas V; place it along the plank's longest edge.
    scale=.40/extent
    uv=[(ox+.05+(u-umin)*scale,oy+.05+(v-vmin)*scale) for u,v in projected]
    for i in range(1,len(p)-1):
        ids=(0,i,i+1)
        tn=cross(sub(p[i],p[0]),sub(p[i+1],p[0]))
        tl=math.sqrt(sum(v*v for v in tn))
        assert tl>1e-8
        faces.append(([p[j] for j in ids],[uv[j] for j in ids],tuple(v/tl for v in tn)))
def box(c,size,mat=0,angle=0):
    x,y,z=c;w,h,d=[v/2 for v in size]
    p=[]
    for px,py,pz in [(-w,-h,-d),(w,-h,-d),(w,h,-d),(-w,h,-d),(-w,-h,d),(w,-h,d),(w,h,d),(-w,h,d)]:
        p.append((x+px*math.cos(angle)-py*math.sin(angle),y+px*math.sin(angle)+py*math.cos(angle),z+pz))
    for ids in [(0,3,2,1),(4,5,6,7),(0,4,7,3),(1,2,6,5),(0,1,5,4),(3,7,6,2)]:
        face([p[i] for i in ids],mat)
def beam(a,b,width,depth,mat=0):
    dx=b[0]-a[0];dy=b[1]-a[1]
    box(tuple((x+y)/2 for x,y in zip(a,b)),(width,math.hypot(dx,dy),depth),mat,-math.atan2(dx,dy))
def lid_board(x,w,z0,z1,y,broken=False,mat=0):
    # Extruded irregular pentagon: visible broken edge, closed solid.
    outline=[(x-w/2,z0),(x+w/2,z0),(x+w/2,z1-.075 if broken else z1)]
    if broken:outline += [(x+w*.05,z1)]
    outline += [(x-w/2,z1-.03 if broken else z1)]
    lo=[(a,y-.04,b) for a,b in outline];hi=[(a,y+.04,b) for a,b in outline]
    # polygon in XZ is clockwise viewed from above.
    def poly(points):
        n=cross(sub(points[1],points[0]),sub(points[2],points[0]));l=math.sqrt(sum(v*v for v in n));n=tuple(v/l for v in n)
        uv=[(.05+.4*(a-(x-w/2))/max(w,z1-z0)+.5*(mat%2),.55+.4*(b-z0)/max(w,z1-z0)) for a,_,b in points]
        # Fan rooted on the opposite edge keeps this small notch polygon valid.
        for i in range(1,len(points)-1):faces.append(([points[j] for j in (0,i,i+1)],[uv[j] for j in (0,i,i+1)],n))
    poly(lo);poly(hi[::-1])
    for i in range(len(lo)):
        j=(i+1)%len(lo);face([lo[i],hi[i],hi[j],lo[j]],mat)
def top_rect(x0,x1,z0,z1,y,mat):
    face([(x0,y,z0),(x0,y,z1),(x1,y,z1),(x1,y,z0)],mat)

def crate(w,d,h,mat=0,metal=False,broken=0):
    # A connected framed shell: inset sides, flush top and no separate corner posts.
    t=.09
    for side in range(4):
        width=w if side%2==0 else d
        def pos(u,y,inset=0):
            if side==0:return (u,y,d/2-inset)
            if side==1:return (w/2-inset,y,-u)
            if side==2:return (-u,y,-d/2+inset)
            return (-w/2+inset,y,u)
        outer=[pos(-width/2,0),pos(width/2,0),pos(width/2,h),pos(-width/2,h)]
        rim=[pos(-width/2+t,t),pos(width/2-t,t),pos(width/2-t,h-t),pos(-width/2+t,h-t)]
        inner=[pos(-width/2+t+.018,t+.018,.02),pos(width/2-t-.018,t+.018,.02),
               pos(width/2-t-.018,h-t-.018,.02),pos(-width/2+t+.018,h-t-.018,.02)]
        for i in range(4):
            j=(i+1)%4
            face([outer[i],outer[j],rim[j],rim[i]],mat)
            face([rim[i],rim[j],inner[j],inner[i]],mat)
        face(inner,mat)
    face([(-w/2,0,-d/2),(w/2,0,-d/2),(w/2,0,d/2),(-w/2,0,d/2)],mat)
    # Top borders meet the centre exactly at h: no recessed trough or corner gaps.
    top_rect(-w/2,w/2,-d/2,-d/2+t,h,mat)
    top_rect(-w/2,w/2,d/2-t,d/2,h,mat)
    top_rect(-w/2,-w/2+t,-d/2+t,d/2-t,h,mat)
    top_rect(w/2-t,w/2,-d/2+t,d/2-t,h,mat)
    count=5 if w>1.2 else 4
    bw=(w-2*t)/count
    for i in range(count):
        x0=-w/2+t+i*bw;x1=x0+bw
        if not broken:
            top_rect(x0,x1,-d/2+t,d/2-t,h,mat)
        elif i in (1,2):
            lid_board((x0+x1)/2,bw,-d/2+t,-d*.06+.06*(i%2),h-.04,True,mat)
            if broken==1:lid_board((x0+x1)/2,bw,d*.24,d/2-t,h-.04,False,mat)
        else:lid_board((x0+x1)/2,bw,-d/2+t,d/2-t,h-.04,False,mat)
    if broken:
        # Only open crates need an interior.
        top_rect(-w/2+t,w/2-t,-d/2+t,d/2-t,.07,mat)
        corners=[(-w/2+t,-d/2+t),(w/2-t,-d/2+t),(w/2-t,d/2-t),(-w/2+t,d/2-t)]
        for i,(x,z) in enumerate(corners):
            xx,zz=corners[(i+1)%4]
            face([(x,.07,z),(xx,.07,zz),(xx,h,zz),(x,h,z)],mat)
    for z in [-d/2-.009,d/2+.009]:
        beam((-w*.32,.19,z),(w*.32,h-.19,z),.075,.02,mat)
    if metal:
        # Thin three-face metal corner caps, rather than 8 solid intersecting cubes.
        for sx in [-1,1]:
            for sz in [-1,1]:
                x=sx*(w/2+.002);z=sz*(d/2+.002)
                for y0,y1 in [(0,.12),(h-.12,h)]:
                    q=[(x-sx*.12,y0,z),(x,y0,z),(x,y1,z),(x-sx*.12,y1,z)]
                    face(q if sx*sz>0 else q[::-1],2)
                    q=[(x,y0,z),(x,y0,z-sz*.12),(x,y1,z-sz*.12),(x,y1,z)]
                    face(q if sx*sz>0 else q[::-1],2)
                top_rect(min(x,x-sx*.12),max(x,x-sx*.12),min(z,z-sz*.12),max(z,z-sz*.12),h+.002,2)

def sack(center,length,rx,rz,horizontal=False,phase=0):
    # Fuller cloth body, gathered neck, knot and flared cloth above the tie.
    n=14
    profile=[(0,.35),(.05,.70),(.14,.91),(.30,1),(.48,.98),(.65,.85),(.78,.60),(.86,.18),(.89,.15),(.94,.29),(1,.23)]
    rings=[]
    for j,(yy,r) in enumerate(profile):
        ring=[]
        for i in range(n):
            ang=2*math.pi*i/n
            fold=1+.055*math.cos(5*ang+phase)*(1 if j<6 else 2)
            a=rx*r*math.cos(ang)*fold
            b=length*yy
            c=rz*r*math.sin(ang)*fold
            if horizontal:
                # Long sacks lying on their side, ends slightly slumped.
                v=(center[0]+a,center[1]+c,center[2]+b-length/2)
            else:v=(center[0]+a+.045*yy*yy,center[1]+b,center[2]+c)
            ring.append(v)
        rings.append(ring)
    # Rotation (a,b,c)->(a,c,b) reverses winding for horizontal sacks.
    def cloth(points):
        face(points[::-1] if horizontal else points,3)
    for r,reverse in [(rings[0],True),(rings[-1],False)]:
        for i in range(1,n-1):
            tri=[r[0],r[i],r[i+1]]
            cloth(tri if reverse else tri[::-1])
    for j,(a,b) in enumerate(zip(rings,rings[1:])):
        for i in range(n):
            k=(i+1)%n
            q=[a[i],b[i],b[k],a[k]]
            # One continuous cylindrical island per sack, not a full texture per face.
            uv=[(.55+.4*i/n,.05+.4*profile[j][0]),
                (.55+.4*i/n,.05+.4*profile[j+1][0]),
                (.55+.4*(i+1)/n,.05+.4*profile[j+1][0]),
                (.55+.4*(i+1)/n,.05+.4*profile[j][0])]
            if horizontal:q=q[::-1];uv=uv[::-1]
            face(q,0 if j==7 else 3)
            if j!=7:
                for idx,ids in enumerate([(0,1,2),(0,2,3)]):
                    pp,_,nn=faces[-2+idx]
                    faces[-2+idx]=(pp,[uv[k] for k in ids],nn)

def save(name):
    lines=['# ROG ZOMBIE | Y-up, metres, base pivot','mtllib Crates.mtl','o '+name,'usemtl CrateAtlas','s off']
    for p,uv,n in faces:
        lines += ['v %.6f %.6f %.6f'%v for v in p]
    for p,uv,n in faces:lines += ['vt %.6f %.6f'%v for v in uv]
    for p,uv,n in faces:lines += ['vn %.6f %.6f %.6f'%n]*3
    for i in range(len(faces)):
        ids=range(i*3+1,i*3+4);lines.append('f '+' '.join(f'{j}/{j}/{j}' for j in ids))
    (OUT/(name+'.obj')).write_text('\n'.join(lines)+'\n')
    vs=[v for p,_,_ in faces for v in p]
    return dict(name=name,triangles=len(faces),dimensions=[round(max(v[i] for v in vs)-min(v[i] for v in vs),3) for i in range(3)])
def meta(path,content):
    target=Path(str(path)+'.meta')
    if not target.exists():target.write_text('fileFormatVersion: 2\nguid: '+uuid.uuid4().hex+'\n'+content)
    return re.search(r'^guid: (\w+)',target.read_text(),re.M)[1]
report=[]
for name,w,d,h,mat,metal,broken in [
 ('Cassa_01_Rinforzata',1,1,1,0,True,0),
 ('Cassa_02_Invecchiata',1,1,.95,1,False,0),
 ('Cassa_03_Lunga',1.65,.82,.65,0,False,0),
 ('Cassa_04_Danneggiata',1,.95,.85,0,False,1),
 ('Cassa_05_Lunga_Rotta',1.65,.90,.68,0,False,2)]:
    faces.clear();crate(w,d,h,mat,metal,broken);report.append(save(name))
faces.clear()
for x in [-.52,.52]:box((x,.065,0),(.15,.13,1.05))
for z in [-.43,-.215,0,.215,.43]:box((0,.155,z),(1.4,.07,.195))
sack((-.30,.42,-.015),.86,.35,.23,True,0)
sack((-.29,.82,-.04),.83,.34,.22,True,1)
sack((.37,.19,.05),.83,.27,.28,False,2)
report.append(save('Sacchi_06_Pallet'))
(ART/'model_report.json').write_text(json.dumps(report,indent=2))
for p in [OUT,OUT/'Editor']:meta(p,'folderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n')
textureguid=meta(OUT/'CrateAtlas.png','TextureImporter:\n  serializedVersion: 13\n  externalObjects: {}\n  mipmaps:\n    enableMipMap: 1\n    sRGBTexture: 1\n  isReadable: 0\n  textureSettings:\n    serializedVersion: 2\n    filterMode: 0\n    aniso: 1\n    mipBias: 0\n    wrapU: 1\n    wrapV: 1\n    wrapW: 1\n  maxTextureSize: 512\n  nPOTScale: 1\n  textureCompression: 0\n  alphaSource: 0\n  textureType: 0\n  textureShape: 1\n  platformSettings:\n  - serializedVersion: 3\n    buildTarget: DefaultTexturePlatform\n    maxTextureSize: 512\n    resizeAlgorithm: 1\n    textureFormat: -1\n    textureCompression: 0\n    overridden: 0\n')
source=Path('Assets/Environment/RocksLowPoly')
m=(source/'Stone.mat').read_text().replace('m_Name: Stone','m_Name: CrateAtlas').replace('d5d4320f2d4d418c87b3298347cc3010',textureguid)
if not (OUT/'CrateAtlas.mat').exists():(OUT/'CrateAtlas.mat').write_text(m)
mg=meta(OUT/'CrateAtlas.mat','NativeFormatImporter:\n  externalObjects: {}\n  mainObjectFileID: 2100000\n')
(OUT/'Crates.mtl').write_text('newmtl CrateAtlas\nKd 1 1 1\nKa 0 0 0\nKs 0 0 0\nd 1\nillum 1\nmap_Kd CrateAtlas.png\n')
meta(OUT/'Crates.mtl','DefaultImporter:\n  externalObjects: {}\n')
template=(source/'Masso_01_Monolite.obj.meta').read_text()
for r in report:
    p=OUT/(r['name']+'.obj.meta')
    if not p.exists():
        s=re.sub(r'^guid: \w+',lambda m:'guid: '+uuid.uuid4().hex,template,count=1,flags=re.M)
        s=s.replace('name: Stone','name: CrateAtlas').replace('323fdb3a90ae4b6090d27a5ca9809a10',mg)
        s=re.sub(r'globalScale: [^\n]+','globalScale: 1',s).replace('addColliders: 1','addColliders: 0')
        p.write_text(s)
print(json.dumps(report,indent=2))

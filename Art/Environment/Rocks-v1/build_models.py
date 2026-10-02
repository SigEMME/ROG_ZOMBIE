"""Ten original low-poly rocks, metres, Y up, base-centred pivot. Standard library."""
from pathlib import Path
from collections import Counter
import math, random, json

ROOT = Path(__file__).resolve().parents[3]
OUT = ROOT / 'Assets/Environment/RocksLowPoly'
OUT.mkdir(parents=True, exist_ok=True)
# name, width, height, depth, sides, middle height, top radius, lean
SPECS = [
 ('01_Monolite',1.4,2.8,1.15,10,.66,.72,.10),
 ('02_LastraInclinata',2.4,3.1,.95,12,.55,.55,.36),
 ('03_MassoPiatto',3.4,.85,2.5,16,.42,.68,-.08),
 ('04_Cuneo',1.9,1.4,1.35,10,.40,.15,.30),
 ('05_Blocco',1.5,1.3,1.5,9,.68,.79,-.04),
 ('06_Scheggia',.7,1.8,.65,10,.52,.18,.16),
 ('07_Arrotondato',1.8,1.6,1.7,14,.46,.42,-.08),
 ('08_Allungato',3.2,1.2,1.15,12,.48,.52,-.12),
 ('09_Piramidale',1.7,2.2,1.4,10,.35,.09,.06),
 ('10_Ciottolo',.8,.55,.7,12,.43,.60,.10),
]

def sub(a,b): return tuple(x-y for x,y in zip(a,b))
def cross(a,b): return (a[1]*b[2]-a[2]*b[1],a[2]*b[0]-a[0]*b[2],a[0]*b[1]-a[1]*b[0])
def dot(a,b): return sum(x*y for x,y in zip(a,b))

def build(spec,seed):
    name,w,h,d,n,middle,top,lean=spec
    rng=random.Random(seed)
    angles=[2*math.pi*i/n+rng.uniform(-.12,.12) for i in range(n)]
    radii=[rng.uniform(.84,1.12) for i in range(n)]
    vertices=[]
    for level,(height,scale) in enumerate(((0,.76),(middle,1),(1,top))):
        for i,a in enumerate(angles):
            y=height if level==0 else height+rng.uniform(-.07,.07)
            vertices.append((math.cos(a)*radii[i]*scale+lean*height,y,math.sin(a)*radii[i]*scale))
    # Normalize the artistic bounds; keep the bottom ring flat on the ground.
    lo=[min(v[i] for v in vertices) for i in range(3)]
    hi=[max(v[i] for v in vertices) for i in range(3)]
    vertices=[((v[0]-lo[0])/(hi[0]-lo[0])*w-w/2,(v[1]-lo[1])/(hi[1]-lo[1])*h,(v[2]-lo[2])/(hi[2]-lo[2])*d-d/2) for v in vertices]
    faces=[]
    for level in range(2):
        for i in range(n):
            a=level*n+i;b=level*n+(i+1)%n;c=b+n;e=a+n
            faces.extend(((a,e,c),(a,c,b)))
    for ring,up in ((0,False),(2,True)):
        center=tuple(sum(vertices[ring*n+i][axis] for i in range(n))/n for axis in range(3))
        ci=len(vertices);vertices.append(center)
        for i in range(n):
            a=ring*n+i;b=ring*n+(i+1)%n
            faces.append((ci,b,a) if up else (ci,a,b))
    edges=Counter(tuple(sorted((f[i],f[(i+1)%3]))) for f in faces for i in range(3))
    assert 50 <= len(faces) <= 100, 'Each rock must have 50–100 triangular faces'
    assert all(count==2 for count in edges.values()),'Mesh must be watertight'
    volume=sum(dot(vertices[a],cross(vertices[b],vertices[c]))/6 for a,b,c in faces)
    assert volume>0
    lines=['# metres, Y up, base-centred pivot','mtllib Stone.mtl','o Masso_'+name,'usemtl Stone','s off']
    for f in faces:
        points=[vertices[i] for i in f]
        normal=cross(sub(points[1],points[0]),sub(points[2],points[0]))
        length=math.sqrt(dot(normal,normal));assert length>1e-8
        normal=tuple(x/length for x in normal)
        # Dominant-axis planar UVs retain comparable texel density across facets.
        axis=max(range(3),key=lambda i:abs(normal[i]));uvaxes=[i for i in range(3) if i!=axis]
        for p in points:lines.append('v '+' '.join(f'{x:.6f}' for x in p))
        for p in points:lines.append('vt '+ ' '.join(f'{p[i]*.5:.6f}' for i in uvaxes))
        for _ in range(3):lines.append('vn '+' '.join(f'{x:.6f}' for x in normal))
    for fi in range(len(faces)):
        lines.append('f '+' '.join(f'{i}/{i}/{i}' for i in range(fi*3+1,fi*3+4)))
    (OUT/('Masso_'+name+'.obj')).write_text('\n'.join(lines)+'\n')
    return {'name':'Masso_'+name,'triangles':len(faces),'dimensions_m':[w,h,d],'closed_manifold':True,'signed_volume':round(volume,5)}

if __name__=='__main__':
    report=[build(spec,730+i) for i,spec in enumerate(SPECS)]
    (OUT/'Stone.mtl').write_text('newmtl Stone\nKa 0.2 0.2 0.2\nKd 1 1 1\nKs 0 0 0\nd 1\nillum 1\nmap_Kd StonePixel.png\n')
    (Path(__file__).parent/'models-report.json').write_text(json.dumps(report,indent=2)+'\n')
    print(json.dumps(report,indent=2))

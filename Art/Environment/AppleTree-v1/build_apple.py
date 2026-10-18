"""Generate the apple tree OBJ; reuse the established oak card/tube topology."""
from pathlib import Path
import ast, math, json, uuid, re

ROOT = Path(__file__).resolve().parents[3]
OUT = ROOT / 'Assets/C1/Environment/AppleTreeLowPoly'
OAK = ROOT / 'Assets/C1/Environment/OakLowPolyTest'
OUT.mkdir(parents=True, exist_ok=True)
faces = []

# Load geometry functions only: the old authoring scripts have legacy output paths.
for source, names in [('TreeTest-v1/build_tree.py', ('sub', 'cross', 'triangle')),
                      ('OakTest-v1/build_oak.py', ('norm', 'tube', 'card'))]:
    tree = ast.parse((ROOT / 'Art/Environment' / source).read_text())
    exec(compile(ast.Module(body=[n for n in tree.body if isinstance(n, ast.FunctionDef)
                                 and n.name in names], type_ignores=[]), source, 'exec'))

tube([(0,0,0),(.025,.25,0),(-.06,1.0,.04),(.03,1.65,0),(.08,2.5,.03)],
     [.23,.18,.14,.10,.035], 6)
clusters = [(-.92,2.35,.10,1.85),(.94,2.48,.20,1.85),(.05,2.50,-.95,1.9),
            (-.16,2.42,.98,1.8),(-.28,3.02,-.12,1.9),(.62,2.93,.56,1.65)]
for k,(x,y,z,w) in enumerate(clusters):
    tube([(.01,1.13,0),(x*.55,y-.65,z*.55),(x,y-.07,z)], [.11,.07,.015], 5)
    for i in range(3):
        a = k*.87+i*math.pi/2
        t = math.radians([25,-28,0][i])
        card((x,y+i*.13,z), (math.cos(a),0,math.sin(a)),
             (math.sin(a)*math.cos(t),math.sin(t),-math.cos(a)*math.cos(t)),w,w*.85)

apple_count = 0
for k,(x,y,z,w) in enumerate(clusters):
    for i in range(4):
        a=k*.93+i*math.tau/4
        center=(x+math.cos(a)*w*.25,y+.35+(i%2)*.035,z+math.sin(a)*w*.23)
        r=.115+(k%3)*.008
        rings=[]
        # Low-poly apple with a broad shoulder and recessed top.
        for h,rad in [(-.88,.2),(-.45,.78),(.2,1),(.65,.8),(.82,.25)]:
            rings.append([(center[0]+r*rad*math.cos(a+j*math.tau/7),
                           center[1]+r*h,center[2]+r*rad*math.sin(a+j*math.tau/7)) for j in range(7)])
        start=len(faces)
        for l in range(4):
            for j in range(7):
                p=[rings[l][j],rings[l+1][j],rings[l+1][(j+1)%7],rings[l][(j+1)%7]]
                for ids in [(0,1,2),(0,2,3)]:
                    triangle([p[v] for v in ids],[(.5,.5)]*3,'Apple')
        for l,up in [(0,False),(4,True)]:
            c=(center[0],center[1]+r*(-.9 if not up else .7),center[2])
            for j in range(7):
                p=[c,rings[l][(j+1)%7],rings[l][j]] if up else [c,rings[l][j],rings[l][(j+1)%7]]
                triangle(p,[(.5,.5)]*3,'Apple')
        # Verify outward side normals before export.
        for p,uv,n,m in faces[start:start+56]:
            radial=(sum(v[0] for v in p)/3-center[0],0,sum(v[2] for v in p)/3-center[2])
            assert sum(n[j]*radial[j] for j in range(3))>0
        tube([(center[0],center[1]+r*.7,center[2]),
              (center[0]+.016,center[1]+r*1.35,center[2])],[.013,.008],4)
        apple_count+=1

lines=['# Melo 01, metres, Y up, ground pivot','mtllib AppleTree.mtl','o Melo_01','s off']
for p,uv,n,m in faces:
    lines += ['v '+' '.join(f'{x:.6f}' for x in v) for v in p]
    lines += ['vt '+' '.join(f'{x:.6f}' for x in v) for v in uv]
    lines += ['vn '+' '.join(f'{x:.6f}' for x in n)]*3
last=None
for i,(_,_,_,m) in enumerate(faces):
    if m!=last: lines.append('usemtl '+m);last=m
    lines.append('f '+' '.join(f'{j}/{j}/{j}' for j in range(i*3+1,i*3+4)))
(OUT/'Melo_01.obj').write_text('\n'.join(lines)+'\n')
(OUT/'AppleTree.mtl').write_text('newmtl Bark\nKd 1 1 1\nmap_Kd ../OakLowPolyTest/BarkPixel.png\n\nnewmtl Foliage\nKd 1 1 1\nmap_Kd ../OakLowPolyTest/FoliagePixel.png\nmap_d ../OakLowPolyTest/FoliagePixel.png\n\nnewmtl Apple\nKd 0.72 0.055 0.035\n')

def guid(path):
    if path.exists(): return re.search(r'^guid: (\w+)',path.read_text(),re.M).group(1)
    value=uuid.uuid4().hex
    path.write_text('fileFormatVersion: 2\nguid: '+value+'\n')
    return value
apple_guid=guid(OUT/'Apple.mat.meta')
mat=(OAK/'Bark.mat').read_text().replace('m_Name: Bark','m_Name: Apple')
mat=re.sub(r'\{fileID: 2800000, guid: \w+, type: 3\}', '{fileID: 0}',mat)
mat=mat.replace('{r: 1, g: 1, b: 1, a: 1}','{r: 0.72, g: 0.055, b: 0.035, a: 1}')
(OUT/'Apple.mat').write_text(mat)
meta=(OAK/'Quercia_Test_01.obj.meta').read_text()
meta=re.sub(r'^guid: \w+', 'guid: '+guid(OUT/'Melo_01.obj.meta'),meta,count=1,flags=re.M)
meta=meta.replace('  materials:', '  - first:\n      type: UnityEngine:Material\n      assembly: UnityEngine.CoreModule\n      name: Apple\n    second: {fileID: 2100000, guid: '+apple_guid+', type: 2}\n  materials:')
(OUT/'Melo_01.obj.meta').write_text(meta)
assert all(n[1]>0 for _,_,n,m in faces if m=='Foliage')
report={'triangles':len(faces),'apples':apple_count,'foliage_cards':18,
        'bounds_min':[min(p[j] for f in faces for p in f[0]) for j in range(3)],
        'bounds_max':[max(p[j] for f in faces for p in f[0]) for j in range(3)]}
Path(__file__).with_name('model-report.json').write_text(json.dumps(report,indent=2))
print(report)

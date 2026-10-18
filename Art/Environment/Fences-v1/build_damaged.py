"""Damaged open/reinforced fences; originals and shared material remain untouched."""
from pathlib import Path
import ast, math, json, re, uuid
ROOT=Path(__file__).resolve().parents[3]
OUT=ROOT/'Assets/C1/Environment/FencesLowPoly'
tree=ast.parse(Path(__file__).with_name('build_models.py').read_text())
exec(compile(ast.Module(body=[n for n in tree.body if isinstance(n,(ast.FunctionDef,ast.ClassDef))],type_ignores=[]),'fence_geometry','exec'))

def broken(m,a,b,width=.15,z=.17,tip=True):
    # Convex splintered end, closed on every side; local X follows the rail.
    dx,dy=b[0]-a[0],b[1]-a[1]; length=math.hypot(dx,dy)
    outline=[(0,-width/2),(length-.10,-width/2),(length,width*.12),
             (length-.07,width/2),(0,width/2)]
    if not tip: outline=[(length-x,-y) for x,y in outline]
    m.prism(outline,.13,z,math.atan2(dy,dx),a)

report={}
for kind in ('Aperta','Rinforzata'):
    m=Model()
    for x,h in [(-1.9,1.85),(1.9,1.82)]: m.box(x,h/2,.2,h,.24)
    # Retain the central post position for modular alignment, but break its top.
    h=1.50 if kind=='Aperta' else 1.43
    m.prism([(-.1,0),(.1,0),(.1,h-.13),(.035,h),(-.1,h-.05)],.24)
    if kind=='Aperta':
        m.beam((-1.98,.35),(0,.305));m.beam((0,.305),(1.98,.365))
        for y,left,right in [(.77,-.72,-.30),(1.2,-1.17,-.45),(1.57,-1.42,-.62)]:
            broken(m,(-1.98,y),(left,y-.035))
            broken(m,(right,y-.20),(0,y-.16),tip=False)
        broken(m,(0,.76),(.66,.60));broken(m,(1.27,.73),(1.98,.785),tip=False)
        broken(m,(0,1.13),(.42,1.06));broken(m,(1.56,1.12),(1.98,1.215),tip=False)
        # A fractured upper board remains hanging from the outer post.
        broken(m,(.80,.94),(1.98,1.585),tip=False)
    else:
        m.beam((-1.98,.35),(0,.305));broken(m,(0,.305),(.72,.22))
        broken(m,(1.36,.30),(1.98,.365),tip=False)
        broken(m,(-1.98,1.55),(-.95,1.46));broken(m,(-.40,1.35),(0,1.36),tip=False)
        broken(m,(0,1.38),(.57,1.25));broken(m,(1.45,1.49),(1.98,1.565),tip=False)
        m.beam((-1.82,.4),(-.1,1.38),.13,-.08)
        broken(m,(-1.82,1.5),(-1.21,1.08),.13,-.23)
        broken(m,(-.65,.76),(-.1,.4),.13,-.23,False)
        broken(m,(.1,.4),(.68,.75),.13,-.08)
        broken(m,(1.4,1.24),(1.82,1.5),.13,-.08,False)
        broken(m,(.1,1.38),(.56,.89),.13,-.23)
        broken(m,(1.23,.76),(1.82,.4),.13,-.23,False)
    name='Staccionata_'+kind+'_Danneggiata'
    report[name]=m.save(name)
    meta=OUT/(name+'.obj.meta')
    if not meta.exists():
        source=(OUT/('Staccionata_'+kind+'.obj.meta')).read_text()
        meta.write_text(re.sub(r'^guid: \w+','guid: '+uuid.uuid4().hex,source,count=1,flags=re.M))
    report[name]['bounds_min']=[min(p[j] for f in m.faces for p in f[0]) for j in range(3)]
    report[name]['bounds_max']=[max(p[j] for f in m.faces for p in f[0]) for j in range(3)]
Path(__file__).with_name('damaged-report.json').write_text(json.dumps(report,indent=2))
print(json.dumps(report,indent=2))

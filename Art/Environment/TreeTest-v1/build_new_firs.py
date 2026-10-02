"""Two firs with different canopy structure and dedicated foliage textures."""
from pathlib import Path
import json
from build_variants import build,OUT

SPECS=[
    ('Abete_06_Salvia',6.8,.25,[1.65,1.22,1.40,.91,.92,.40],1.45,.85,.92,6,.20),
    ('Abete_07_Oliva',4.9,.30,[1.60,1.95,1.54,1.28,.82,.39],.60,.65,1.13,8,0),
]

if __name__=='__main__':
    report=[]
    for i,spec in enumerate(SPECS):
        report.append(build(spec,1301+i))
        suffix='Salvia' if i==0 else 'Oliva'
        p=OUT/(spec[0]+'.obj')
        p.write_text(p.read_text().replace('mtllib Tree.mtl','mtllib Tree'+suffix+'.mtl').replace('usemtl Foliage','usemtl Foliage'+suffix))
    (Path(__file__).parent/'new-firs-report.json').write_text(json.dumps(report,indent=2))
    print(json.dumps(report,indent=2))

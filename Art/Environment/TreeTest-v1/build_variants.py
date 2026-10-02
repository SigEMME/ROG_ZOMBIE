"""Additional card-based trees, reusing the original trunk topology and materials."""
from pathlib import Path
import math, json, random, re, uuid
from build_tree import OUT, faces, triangle, build_trunk

SPECS = [
    ('Abete_02_Slanciato',7.2,.24,[1.2,1.1,.98,.84,.69,.51,.28],1.25,.80,1.10,6,0),
    ('Abete_04_Giovane',2.8,.13,[.85,.65,.40,.20],.42,.46,.72,6,0),
    ('Abete_05_Irregolare',6.2,.28,[1.7,1.48,1.40,1.06,.72,.35],1.10,.73,1.22,7,.55),
]

def trunk_faces():
    result=build_trunk()
    faces.clear()
    return result

def build(spec,seed):
    name,height,radius,radii,bottom,spacing,drop,sectors,bend=spec
    rng=random.Random(seed);faces.clear()
    total=bottom+(len(radii)-1)*spacing+drop
    sy=height/total
    def center(y):return (bend*(y/height)**1.7, .12*bend*math.sin(y))
    for points,uv in trunk_faces():
        transformed=[]
        for x,y,z in points:
            y=y/5.65*height*.96;cx,cz=center(y)
            transformed.append((x*radius/.32+cx,y,z*radius/.32+cz))
        triangle(transformed,uv,'Bark')
    cards=0
    for tier,r in enumerate(radii):
        for i in range(sectors):
            if bend and tier in (0,2) and i==2:continue
            a=i*math.tau/sectors+tier*.41
            rr=r*(rng.uniform(.78,1.13) if bend else 1)
            y=(bottom+tier*spacing)*sy
            dy=drop*sy
            rx,rz=math.cos(a),math.sin(a);tx,tz=-rz,rx
            width=rr*(2*math.sin(math.pi/sectors))*1.42
            cx,cz=center(y+dy*.5)
            def point(rad,yy,side):return(cx+rx*rad+tx*width*.5*side,yy,cz+rz*rad+tz*width*.5*side)
            p=[point(rr,y,-1),point(rr,y,1),point(.015,y+dy,1),point(.015,y+dy,-1)]
            uv=[(0,0),(1,0),(1,1),(0,1)]
            for ids in [(0,1,2),(0,2,3)]:triangle([p[j] for j in ids],[uv[j] for j in ids],'Foliage')
            cards+=1
    lines=['# Metres, Y up, base pivot, trunk solid + flat foliage cards','mtllib Tree.mtl','o '+name,'s off']
    for p,uv,n,m in faces:
        lines.extend('v '+' '.join(f'{x:.6f}' for x in v) for v in p)
        lines.extend('vt '+' '.join(f'{x:.6f}' for x in v) for v in uv)
        lines.extend(['vn '+' '.join(f'{x:.6f}' for x in n)]*3)
    material=None
    for i,(_,_,_,m) in enumerate(faces):
        if m!=material:lines.append('usemtl '+m);material=m
        lines.append('f '+' '.join(f'{j}/{j}/{j}' for j in range(i*3+1,i*3+4)))
    (OUT/(name+'.obj')).write_text('\n'.join(lines)+'\n')
    mp=OUT/(name+'.obj.meta')
    if not mp.exists():
        source=(OUT/'Abete_02_Slanciato.obj.meta').read_text()
        mp.write_text(re.sub(r'guid: [a-f0-9]{32}','guid: '+uuid.uuid4().hex,source,count=1))
    return {'name':name,'height_m':height,'trunk_triangles':60,'foliage_cards':cards,'total_triangles':len(faces)}

if __name__=='__main__':
    report=[build(s,901+i) for i,s in enumerate(SPECS)]
    (Path(__file__).parent/'variants-report.json').write_text(json.dumps(report,indent=2))
    print(json.dumps(report,indent=2))

from pathlib import Path
import json,hashlib
from build_models import faces,building,box,beam,face,window,panel,save
root=Path('Assets/Environment/FarmBuildingsLowPoly')
art=Path('Art/Environment/FarmBuildings-v1')

def annex(w,d,cx,cz,high=3.6):
    low=2.6
    box((cx,low/2,cz),(w,low,d),0)
    p=[(cx-w/2-.15,low,cz-d/2-.2),(cx-w/2-.15,low,cz+d/2+.2),
       (cx+w/2+.12,high,cz+d/2+.2),(cx+w/2+.12,high,cz-d/2-.2)]
    face(p,2,(w,high-low,0))
    for i in range(4):
        j=(i+1)%4
        face([p[i],(p[i][0],p[i][1]-.09,p[i][2]),(p[j][0],p[j][1]-.09,p[j][2]),p[j]],2)
    for sign in [-1,1]:
        z=cz+sign*d/2
        q=[(cx-w/2,low,z),(cx+w/2,low,z),(cx+w/2,high-.09,z),(cx-w/2,low+.01,z)]
        face(q if sign>0 else q[::-1],0)
        beam((cx-w/2-.15,low,z),(cx+w/2+.12,high,z),.12,1)
    window((cx,1.4,cz+d/2),(1,0,0),1,1.2)
    for f in [-.25,.25]:window((cx-w/2,1.4,cz+f*d),(0,0,1),.85,1.15)

def porch(w,front,depth,posts):
    box((0,.1,front+depth/2),(w,.2,depth),3)
    p=[(-w/2,2.45,front+depth),(w/2,2.45,front+depth),
       (w/2,2.8,front-.06),(-w/2,2.8,front-.06)]
    face(p,2,(0,-.35,depth))
    face([(x,y-.08,z) for x,y,z in p][::-1],2)
    for x in [-w/2+.1+(w-.2)*i/(posts-1) for i in range(posts)]:
        beam((x,.2,front+depth-.12),(x,2.45,front+depth-.12),.14,1)
    beam((-w/2,2.4,front+depth),(w/2,2.4,front+depth),.14,1)
    box((0,.07,front+depth+.25),(1.45,.14,.5),3)

def chimney(x,z):
    box((x,4.35,z),(.48,2,.48),3)
    box((x,5.36,z),(.62,.14,.62),3)

def generate(kind,color):
    large=color=='Azzurro'
    if kind.startswith('Fienile'):
        w,d=(10,16) if large else (8,12)
        building(w,d,4.7,2.8,0,True,barn=True,bays=5 if large else 4,
                 door_centers=[-2.7,2.7] if large else [0],door_width=2.8)
        annex(3.2,10 if large else 7,-w/2-1.55,-1)
        # Structural posts articulate additional bays, same cross section as original trims.
        for z in [-d/2+d*i/(5 if large else 4) for i in range(1,5 if large else 4)]:
            beam((w/2+.03,.36,z),(w/2+.03,4.7,z),.13,1)
        features='5 campate, due portoni, annesso lungo' if large else '4 campate, corpo e annesso ampliati'
    elif kind.startswith('Rimessa'):
        w,d=(9,13) if large else (6.8,10)
        building(w,d,3.15,1.7,0,barn=True,bays=4 if large else 3,
                 door_centers=[-2.2,2.2] if large else [0],door_width=2.6)
        if large:
            # Small covered loading porch in front of the twin door facade.
            porch(8,d/2,1.8,3)
        features='4 campate, due portoni, tettoia di carico' if large else '3 campate e maggiore superficie di deposito'
    else:
        w,d=(9,12) if large else (7.2,9)
        building(w,d,2.85,1.9,1,bays=4 if large else 3,door_width=1.1)
        for x in ([-3.2,-1.7,1.7,3.2] if large else [-2.4,2.4]):
            window((x,1.5,d/2),(1,0,0),.78,1.1)
        chimney(-1.4,-d*.27)
        if large:chimney(2.2,-d*.22)
        porch(w-.5,d/2,2 if large else 1.5,4 if large else 3)
        features='4 campate, veranda ampia, due camini' if large else '3 campate, veranda e nuove finestre'
    return features

if __name__=='__main__':
    old=json.loads((art/'model_report.json').read_text())
    report=[r for r in old if not r['name'].endswith(('_Salvia','_Azzurro'))]
    for color in ['Salvia','Azzurro']:
        for base in report[:3]:
            faces.clear()
            name=base['name']+'_'+color
            features=generate(base['name'],color)
            item=save(name)
            p=root/(name+'.obj')
            p.write_text(p.read_text().replace('mtllib Farm.mtl','mtllib Farm_'+color+'.mtl').replace('usemtl FarmAtlas','usemtl FarmAtlas_'+color))
            report.append(dict(item,texture='FarmAtlas_'+color+'.png',features=features))
    (art/'model_report.json').write_text(json.dumps(report,indent=2))
    print(json.dumps(report,indent=2))

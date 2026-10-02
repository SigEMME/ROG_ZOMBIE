"""Shared low-poly tree geometry helpers and reusable trunk topology."""
from pathlib import Path
import math, json

ROOT=Path(__file__).resolve().parents[3]
OUT=ROOT/'Assets/Environment/TreeLowPolyTest'
OUT.mkdir(parents=True,exist_ok=True)
faces=[]
def sub(a,b):return tuple(x-y for x,y in zip(a,b))
def cross(a,b):return (a[1]*b[2]-a[2]*b[1],a[2]*b[0]-a[0]*b[2],a[0]*b[1]-a[1]*b[0])
def triangle(points,uv,material):
    n=cross(sub(points[1],points[0]),sub(points[2],points[0]))
    length=math.sqrt(sum(x*x for x in n));assert length>1e-9
    faces.append((points,uv,tuple(x/length for x in n),material))

def build_trunk():
    faces.clear()
    rings=[]
    for y,r,cx,cz in [(0,.32,0,0),(.35,.24,.01,0),(1.8,.17,.07,-.03),(3.7,.10,-.02,.05),(5.65,.025,.04,.01)]:
        rings.append([(cx+r*math.cos(i*math.tau/6),y,cz+r*math.sin(i*math.tau/6)) for i in range(6)])
    for k in range(4):
        for i in range(6):
            j=(i+1)%6
            p=[rings[k][i],rings[k+1][i],rings[k+1][j],rings[k][j]]
            uv=[(i/6,p[0][1]*.5),(i/6,p[1][1]*.5),((i+1)/6,p[2][1]*.5),((i+1)/6,p[3][1]*.5)]
            for ids in [(0,1,2),(0,2,3)]:triangle([p[x] for x in ids],[uv[x] for x in ids],'Bark')
    for k,up in [(0,False),(4,True)]:
        center=tuple(sum(p[a] for p in rings[k])/6 for a in range(3))
        for i in range(6):
            p=[center,rings[k][(i+1)%6],rings[k][i]] if up else [center,rings[k][i],rings[k][(i+1)%6]]
            triangle(p,[(v[0]+.5,v[2]+.5) for v in p],'Bark')
    return [(p,uv) for p,uv,n,m in faces]

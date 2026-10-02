from pathlib import Path
from PIL import Image, ImageDraw
import math,json
root=Path('Assets/Environment/HayLowPoly')
texture=Image.open(root/'HayPixel.png').convert('RGB')
canvas=Image.new('RGB',(1000,500),(35,41,46))
report=json.loads(Path('Art/Environment/Hay-v1/model_report.json').read_text())
for view,item in enumerate(report):
    verts=[];uv=[];normals=[];faces=[]
    for line in (root/(item['name']+'.obj')).read_text().splitlines():
        s=line.split()
        if not s:continue
        if s[0]=='v':verts.append(tuple(map(float,s[1:])))
        elif s[0]=='vt':uv.append(tuple(map(float,s[1:])))
        elif s[0]=='vn':normals.append(tuple(map(float,s[1:])))
        elif s[0]=='f':faces.append([int(v.split('/')[0])-1 for v in s[1:]])
    elevation=math.radians(52);az=math.radians(32)
    def proj(v):
        x,y,z=v;a=x*math.cos(az)-z*math.sin(az);b=x*math.sin(az)+z*math.cos(az)
        return (a,y*math.cos(elevation)-b*math.sin(elevation),y*math.sin(elevation)+b*math.cos(elevation))
    p=[proj(v) for v in verts]
    xmin=min(v[0] for v in p);xmax=max(v[0] for v in p);ymin=min(v[1] for v in p);ymax=max(v[1] for v in p)
    scale=min(415/(xmax-xmin),360/(ymax-ymin))
    p=[(250+(v[0]-(xmin+xmax)/2)*scale,440-(v[1]-ymin)*scale,v[2]) for v in p]
    tile=Image.new('RGB',(500,500),(35,41,46));pix=tile.load();depth=[-1e20]*250000
    for f in faces:
        a,b,c=[p[i] for i in f]
        den=(b[1]-c[1])*(a[0]-c[0])+(c[0]-b[0])*(a[1]-c[1])
        if abs(den)<1e-8:continue
        n=normals[f[0]]
        light=.55+.45*max(0,n[0]*-.35+n[1]*.85+n[2]*.35)
        for y in range(max(0,math.floor(min(a[1],b[1],c[1]))),min(500,math.ceil(max(a[1],b[1],c[1])))):
            for x in range(max(0,math.floor(min(a[0],b[0],c[0]))),min(500,math.ceil(max(a[0],b[0],c[0])))):
                u=((b[1]-c[1])*(x+.5-c[0])+(c[0]-b[0])*(y+.5-c[1]))/den
                v=((c[1]-a[1])*(x+.5-c[0])+(a[0]-c[0])*(y+.5-c[1]))/den;w=1-u-v
                if min(u,v,w)<0:continue
                z=u*a[2]+v*b[2]+w*c[2]
                if z<=depth[y*500+x]:continue
                tu=u*uv[f[0]][0]+v*uv[f[1]][0]+w*uv[f[2]][0]
                tv=u*uv[f[0]][1]+v*uv[f[1]][1]+w*uv[f[2]][1]
                tx=int(max(0,min(511,int(tu*512)))/512*texture.width)
                ty=int(max(0,min(511,511-int(tv*512)))/512*texture.height)
                color=texture.getpixel((tx,ty))
                depth[y*500+x]=z;pix[x,y]=tuple(min(255,int(ch*light)) for ch in color)
    draw=ImageDraw.Draw(tile)
    draw.text((25,22),item['name'].replace('_',' '),fill='white')
    draw.text((25,46),str(item['triangles'])+' triangoli | vista ortografica 52 gradi',fill=(190,200,207))
    canvas.paste(tile,((view%3)*500,(view//3)*500))
canvas.save('Art/Environment/Hay-v1/Fieno_preview.png')
print('Rendered two actual meshes.')

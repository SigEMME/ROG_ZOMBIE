from pathlib import Path
from PIL import Image,ImageDraw
import math
root=Path('Assets/Environment/TreeLowPolyTest')
canvas=Image.new('RGB',(2000,850),(35,41,46))
for view in range(4):
    suffix='Salvia' if view<2 else 'Oliva'
    model=root/('Abete_06_Salvia.obj' if view<2 else 'Abete_07_Oliva.obj')
    foliage_file='Foliage'+suffix+'Pixel.png'
    verts=[];uv=[];normals=[];faces=[];mat=''
    for line in model.read_text().splitlines():
        s=line.split()
        if not s:continue
        if s[0]=='v':verts.append(tuple(map(float,s[1:])))
        elif s[0]=='vt':uv.append(tuple(map(float,s[1:])))
        elif s[0]=='vn':normals.append(tuple(map(float,s[1:])))
        elif s[0]=='usemtl':mat='Foliage' if s[1].startswith('Foliage') else s[1]
        elif s[0]=='f':faces.append(([int(v.split('/')[0])-1 for v in s[1:]],mat))
    textures={m:Image.open(root/f).convert('RGBA') for m,f in [('Bark','BarkPixel.png'),('Foliage',foliage_file)]}
    fol=textures['Foliage']
    assert fol.getextrema()[3][0]==0 and fol.getextrema()[3][1]==255
    elevation=math.radians(35 if view%2==0 else 75)
    az=math.radians(25)
    def proj(v):
        x,y,z=v;a=x*math.cos(az)-z*math.sin(az);b=x*math.sin(az)+z*math.cos(az)
        return (a,y*math.cos(elevation)-b*math.sin(elevation),y*math.sin(elevation)+b*math.cos(elevation))
    p=[proj(v) for v in verts]
    xmin=min(v[0] for v in p);xmax=max(v[0] for v in p);ymin=min(v[1] for v in p);ymax=max(v[1] for v in p)
    scale=min(430/(xmax-xmin),690/(ymax-ymin))
    p=[(250+(v[0]-(xmin+xmax)/2)*scale,760-(v[1]-ymin)*scale,v[2]) for v in p]
    tile=Image.new('RGB',(500,850),(35,41,46));pix=tile.load();depth=[-1e20]*(500*850)
    for f,material in faces:
        a,b,c=[p[i] for i in f]
        den=(b[1]-c[1])*(a[0]-c[0])+(c[0]-b[0])*(a[1]-c[1])
        if abs(den)<1e-8:continue
        n=normals[f[0]]
        light=.67+.33*abs(n[0]*-.3+n[1]*.85+n[2]*.4)
        texture=textures[material];res=256 if material=='Foliage' else 128
        for y in range(max(0,math.floor(min(a[1],b[1],c[1]))),min(850,math.ceil(max(a[1],b[1],c[1])))):
            for x in range(max(0,math.floor(min(a[0],b[0],c[0]))),min(500,math.ceil(max(a[0],b[0],c[0])))):
                u=((b[1]-c[1])*(x+.5-c[0])+(c[0]-b[0])*(y+.5-c[1]))/den
                v=((c[1]-a[1])*(x+.5-c[0])+(a[0]-c[0])*(y+.5-c[1]))/den;w=1-u-v
                if min(u,v,w)<0:continue
                z=u*a[2]+v*b[2]+w*c[2]
                if z<=depth[y*500+x]:continue
                tu=u*uv[f[0]][0]+v*uv[f[1]][0]+w*uv[f[2]][0]
                tv=u*uv[f[0]][1]+v*uv[f[1]][1]+w*uv[f[2]][1]
                if material=='Bark':tu%=1;tv%=1
                tu=max(0,min(.99999,tu));tv=max(0,min(.99999,tv))
                color=texture.getpixel((int((int(tu*res)+.5)/res*texture.width),int((res-1-int(tv*res)+.5)/res*texture.height)))
                if material=='Foliage' and color[3]<128:continue
                if False:
                    color=(114,143,123) if material=='Foliage' else (133,96,63)
                    if min(u,v,w)<.016:color=(32,43,44)
                depth[y*500+x]=z
                pix[x,y]=tuple(min(255,int(ch*light)) for ch in color[:3])
    draw=ImageDraw.Draw(tile)
    labels=['SALVIA - VISTA INCLINATA','SALVIA - DALL\'ALTO','OLIVA - VISTA INCLINATA','OLIVA - DALL\'ALTO']
    draw.text((25,25),labels[view],fill='white')
    canvas.paste(tile,(view*500,0))
canvas.save('Art/Environment/TreeTest-v1/Abeti_nuovi_preview.png')

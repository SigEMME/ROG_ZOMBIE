import bpy,json
from pathlib import Path
report=[]
for name in ['CAR01','BUS01','TRACK01','Tractor01','Pickup01']:
    bpy.ops.wm.read_factory_settings(use_empty=True)
    path=Path('C:/UnityProjects/ROG_ZOMBIE/Assets/Environment')/name/(name+'_Unity.fbx')
    bpy.ops.import_scene.fbx(filepath=str(path))
    meshes=[]
    for o in bpy.context.scene.objects:
        if o.type!='MESH':continue
        o.data.calc_loop_triangles()
        meshes.append(dict(name=o.name,vertices=len(o.data.vertices),triangles=len(o.data.loop_triangles),uv_layers=len(o.data.uv_layers),dimensions=list(o.dimensions),materials=[m.name if m else None for m in o.data.materials]))
    report.append(dict(model=name,meshes=meshes))
Path('C:/UnityProjects/ROG_ZOMBIE/Art/Environment/Vehicles-v1/FBX-inspection.json').write_text(json.dumps(report,indent=2))
print(json.dumps(report,indent=2))

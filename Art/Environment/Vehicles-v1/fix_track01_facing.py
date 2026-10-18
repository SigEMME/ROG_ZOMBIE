"""Repair TRACK01's mirrored log rows without re-exporting unrelated FBX data.
Run with Blender --background --python this_file.py. Writes a verified candidate to output/.
"""
from pathlib import Path
from array import array
import copy
import numpy as np
from io_scene_fbx import parse_fbx, encode_bin

ROOT = Path(__file__).resolve().parents[3]
SOURCE = ROOT / 'Assets/C1/Environment/TRACK01/TRACK01_Unity.fbx'
OUTPUT = ROOT / 'output/TRACK01_Unity_fixed.fbx'

def child(node, name):
    return next(n for n in node.elems if n.id == name)

root, version = parse_fbx.parse(str(SOURCE))
original = copy.deepcopy(root)
objects = child(root, b'Objects')
model = next(n for n in objects.elems if n.id == b'Model' and n.props[1].startswith(b'TRACK01_Rimorchio_Carico\0'))
connections = child(root, b'Connections')
geometry_ids = {n.props[1] for n in connections.elems if n.props[0] == b'OO' and n.props[2] == model.props[0]}
geo = next(n for n in objects.elems if n.id == b'Geometry' and n.props[0] in geometry_ids)
vertices = np.array(child(geo, b'Vertices').props[0]).reshape(-1, 3)
indices = child(geo, b'PolygonVertexIndex').props[0]
assert len(indices) % 3 == 0
faces = np.array(indices).reshape(-1, 3)
assert np.all(faces[:, :2] >= 0) and np.all(faces[:, 2] < 0)
faces[:, 2] = -faces[:, 2] - 1
adj = [set() for _ in vertices]
for face in faces:
    for v in face: adj[v].update(int(w) for w in face if w != v)
seen = set()
flipped = set()
log_count = reversed_logs = 0
for start in range(len(vertices)):
    if start in seen: continue
    group, stack = set(), [start]
    while stack:
        v = stack.pop()
        if v in group: continue
        group.add(v)
        stack.extend(adj[v] - group)
    seen.update(group)
    points = vertices[sorted(group)]
    size = np.ptp(points, axis=0)
    if len(group) != 16 or not (size[2] > 3 and size[0] < .3 and size[1] < .3): continue
    log_count += 1
    center = points.mean(axis=0)
    polygon_ids = [i for i, f in enumerate(faces) if int(f[0]) in group]
    signs = []
    for i in polygon_ids:
        a, b, c = vertices[faces[i]]
        signs.append(float(np.dot(np.cross(b-a, c-a), (a+b+c)/3-center)))
    assert all(s > 0 for s in signs) or all(s < 0 for s in signs), 'Mixed log winding'
    if signs[0] < 0:
        flipped.update(polygon_ids)
        reversed_logs += 1
assert log_count == 16
assert reversed_logs in (0, 8)

norm_layer = child(geo, b'LayerElementNormal')
uv_layer = child(geo, b'LayerElementUV')
for layer in (norm_layer, uv_layer):
    assert child(layer, b'MappingInformationType').props[0] == b'ByPolygonVertex'
    assert child(layer, b'ReferenceInformationType').props[0] == b'IndexToDirect'
normals = child(norm_layer, b'Normals').props[0]
normal_indices = child(norm_layer, b'NormalsIndex').props[0]
uv_indices = child(uv_layer, b'UVIndex').props[0]
# Duplicate inverted normals, because original entries may also be used by other parts.
normal_map = {}
for i in sorted(flipped):
    offset = 3*i
    a,b,c = map(int, faces[i])
    indices[offset:offset+3] = array(indices.typecode, (a,c,-b-1))
    ni = [normal_indices[offset+j] for j in (0,2,1)]
    for j, old in enumerate(ni):
        if old not in normal_map:
            normal_map[old] = len(normals)//3
            normals.extend(-x for x in normals[3*old:3*old+3])
        normal_indices[offset+j] = normal_map[old]
    uv_indices[offset+1],uv_indices[offset+2] = uv_indices[offset+2],uv_indices[offset+1]
# FBX edges reference polygon corners; keep their undirected vertex pairs identical.
edges = child(geo, b'Edges').props[0]
for i, corner in enumerate(edges):
    polygon, local = divmod(corner, 3)
    if polygon in flipped: edges[i] = polygon*3 + (2,1,0)[local]

methods = dict(Y='add_int16', C='add_char', I='add_int32', L='add_int64', F='add_float32', D='add_float64', S='add_string', R='add_bytes', f='add_float32_array', d='add_float64_array', i='add_int32_array', l='add_int64_array', b='add_bool_array', c='add_byte_array', B='add_bool', Z='add_int8')
def encode(node):
    result = encode_bin.FBXElem(node.id)
    for t, value in zip(node.props_type, node.props): getattr(result, methods[chr(t)])(bool(value) if chr(t) == "B" else value)
    result.elems.extend(encode(n) for n in node.elems)
    return result
encode_bin._write_timedate_hack = lambda _: None  # Preserve original metadata too.
encode_bin.write(str(OUTPUT), encode(root), version)
verified, new_version = parse_fbx.parse(str(OUTPUT))
assert version == new_version

def compare(a, b, path=''):
    assert a.id == b.id and a.props_type == b.props_type and len(a.elems) == len(b.elems), (path,a.id,a.props_type,b.props_type)
    changes = []
    for x,y in zip(a.props,b.props):
        if x != y: changes.append(path+'/'+a.id.decode(errors='replace'))
    for i,(x,y) in enumerate(zip(a.elems,b.elems)): changes += compare(x,y,path+'/'+a.id.decode(errors='replace')+':'+str(i))
    return changes
assert not compare(root,verified), 'FBX serialization changed data'
changes = compare(original, verified)
assert len(changes) == (5 if flipped else 0), changes
assert all(p.endswith(('/PolygonVertexIndex','/Edges','/Normals','/NormalsIndex','/UVIndex')) for p in changes), changes
report = f'PASS: {log_count} logs; {reversed_logs} reversed logs corrected; {len(flipped)} triangles flipped.\nOnly polygon winding, normal direction and associated corner indices changed.\nVertex positions, UV coordinates, material assignments, hierarchy, IDs and all other FBX data preserved.\n'
(ROOT/'output/TRACK01-facing-validation.txt').write_text(report)
print(report)
from pathlib import Path
import sys,uuid,re,json
sys.path.insert(0,'Art/Environment/TreeTest-v1')
import build_variants as fir
name='Abete_08_Salvia_Espanso'
report=fir.build((name,5.9,.28,[1.9,1.55,1.15,1.25,.85,.65,.30],.80,.60,1.0,7,.12),1408)
p=fir.OUT/(name+'.obj')
p.write_text(p.read_text().replace('mtllib Tree.mtl','mtllib TreeSalvia.mtl').replace('usemtl Foliage','usemtl FoliageSalvia'))
# The new model inherits the Salvia material mapping, with its own asset identity.
mp=fir.OUT/(name+'.obj.meta')
guid=re.search(r'^guid: (\w+)',mp.read_text(),re.M).group(1)
mp.write_text(re.sub(r'guid: [a-f0-9]{32}','guid: '+guid,(fir.OUT/'Abete_06_Salvia.obj.meta').read_text(),count=1))
Path('Art/Environment/TreeTest-v1/Abete_08-report.json').write_text(json.dumps(report,indent=2))
print(report)
sys.path.insert(0,'Art/Environment/BirchTest-v1')
import build_birch
for name,variant in [('Betulla_02_Giovane',1),('Betulla_03_Espansa',2)]:
    build_birch.build(name,variant)
    mp=build_birch.OUT/(name+'.obj.meta')
    if not mp.exists():
        mp.write_text(re.sub(r'guid: [a-f0-9]{32}','guid: '+uuid.uuid4().hex,(build_birch.OUT/'Betulla_Test_01.obj.meta').read_text(),count=1))
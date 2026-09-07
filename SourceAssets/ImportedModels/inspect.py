import bpy, json
from mathutils import Vector
for name in ['door','duct']:
 bpy.ops.wm.open_mainfile(filepath='C:/Users/Onur/Desktop/Exit7/SourceAssets/ImportedModels/'+name+'.blend')
 print('MODEL',name)
 for o in bpy.data.objects:
  if o.type=='MESH': print(o.name,'dim',tuple(round(x,3) for x in o.dimensions),'pos',tuple(round(x,3) for x in o.location),'mats',[m.name for m in o.data.materials])
 print('IMAGES',[(i.name, i.filepath, tuple(i.size)) for i in bpy.data.images])

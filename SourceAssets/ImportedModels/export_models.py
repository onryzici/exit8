import bpy, os
from mathutils import Vector
root='C:/Users/Onur/Desktop/Exit7/'
for kind in ['Door','Vent']:
 bpy.ops.wm.open_mainfile(filepath=root+'SourceAssets/ImportedModels/'+('door' if kind=='Door' else 'duct')+'.blend')
 objs=[o for o in bpy.data.objects if o.type=='MESH' and (kind=='Door' or o.name=='modular_airduct_rectangular_01_vent_01')]
 if kind=='Door':
  for im in bpy.data.images:
   if im.size[0]>0:
    path=root+'Assets/ImportedModels/Door/'+('normal' if 'norm' in im.name else 'specular' if 'spec' in im.name else 'diffuse')+'.png'
    im.filepath_raw=path;im.file_format='PNG';im.save()
 coords=[o.matrix_world@Vector(c) for o in objs for c in o.bound_box]
 lo=Vector(tuple(min(v[i] for v in coords) for i in range(3)));hi=Vector(tuple(max(v[i] for v in coords) for i in range(3)));center=(lo+hi)/2
 print(kind,'bounds',lo[:],hi[:])
 for o in list(bpy.data.objects):
  if o not in objs:bpy.data.objects.remove(o,do_unlink=True)
 for o in objs:
  o.select_set(True);bpy.context.view_layer.objects.active=o
  bpy.ops.object.transform_apply(location=False,rotation=True,scale=True)
  o.location-=center
  if kind=='Door':
   mod=o.modifiers.new('Small manufactured edge bevel','BEVEL');mod.width=.0015;mod.segments=3
   bpy.ops.object.modifier_apply(modifier=mod.name)
  for poly in o.data.polygons:poly.use_smooth=False
 bpy.ops.wm.save_as_mainfile(filepath=root+'SourceAssets/ImportedModels/'+kind+'-adapted.blend')
 bpy.ops.export_scene.fbx(filepath=root+'Assets/ImportedModels/'+kind+'/'+kind+'.fbx',use_selection=True,object_types={'MESH'},add_leaf_bones=False,bake_anim=False,axis_forward='-Z',axis_up='Y',use_mesh_modifiers=True)

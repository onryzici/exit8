import bpy
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
parts=[]
def box(name,p,dims,bevel):
 bpy.ops.mesh.primitive_cube_add(size=1,location=p);o=bpy.context.object;o.name=name;o.dimensions=dims;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 mod=o.modifiers.new('Moulded rounded edges','BEVEL');mod.width=bevel;mod.segments=4;bpy.ops.object.modifier_apply(modifier=mod.name)
 mod=o.modifiers.new('Weighted surface normals','WEIGHTED_NORMAL');mod.keep_sharp=True;bpy.ops.object.modifier_apply(modifier=mod.name);parts.append(o)
box('Rubber tile',(0,0,0),(.298,.298,.014),.002)
for x in [-.108,-.036,.036,.108]:box('Moulded rib',(x,0,.0105),(.025,.245,.007),.0033)
bpy.ops.object.select_all(action='SELECT');bpy.context.view_layer.objects.active=parts[0];bpy.ops.object.join();bpy.context.object.name='Clean directional tactile tile'
bpy.ops.wm.save_as_mainfile(filepath=r'C:\Users\Onur\Desktop\Exit7\SourceAssets\TactileTile.blend')
bpy.ops.export_scene.fbx(filepath=r'C:\Users\Onur\Desktop\Exit7\Assets\Loop\TactileTile.fbx',use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',add_leaf_bones=False)
import bpy
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
def box(name,p,d,b):
 bpy.ops.mesh.primitive_cube_add(size=1,location=p);o=bpy.context.object;o.name=name;o.dimensions=d;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 m=o.modifiers.new('Rounded leather edges','BEVEL');m.width=b;m.segments=4;bpy.ops.object.modifier_apply(modifier=m.name)
 m=o.modifiers.new('Weighted normals','WEIGHTED_NORMAL');bpy.ops.object.modifier_apply(modifier=m.name)
box('Leather body',(0,0,0),(.095,.40,.29),.018)
for s in [-1,1]:
 box('Stitched leather face',(s*.050,0,0),(.004,.36,.25),.012)
 box('Handle riser',(0,s*.055,.175),(.018,.022,.063),.008)
 box('Metal handle mount',(0,s*.055,.15),(.025,.035,.016),.003)
box('Leather grip',(0,0,.203),(.024,.12,.024),.011)
box('Zipper spine',(0,0,.145),(.009,.36,.006),.002)
bpy.ops.object.select_all(action='SELECT')
bpy.ops.wm.save_as_mainfile(filepath=r'C:\Users\Onur\Desktop\Exit7\SourceAssets\CommuterBriefcase.blend')
bpy.ops.export_scene.fbx(filepath=r'C:\Users\Onur\Desktop\Exit7\Assets\Commuter\Briefcase.fbx',use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',add_leaf_bones=False)
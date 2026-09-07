import bpy, math
from mathutils import Vector
bpy.ops.object.select_all(action='SELECT'); bpy.ops.object.delete(use_global=False)
parts=[]
def ell(name,p,s):
 bpy.ops.mesh.primitive_uv_sphere_add(segments=24,ring_count=16,location=p)
 o=bpy.context.object;o.name=name;o.scale=s;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);parts.append(o);return o
def bone(a,b,r1,r2=None):
 p=(Vector(a)+Vector(b))/2;o=ell('Anatomical form',p,(r1,r2 or r1,(Vector(b)-Vector(a)).length/2+r1));o.rotation_euler=(Vector(b)-Vector(a)).to_track_quat('Z','Y').to_euler();return o
ell('Thorax',(0,0,1.65),(.29,.14,.43));ell('Pelvis',(0,.01,1.15),(.20,.13,.23));ell('Neck',(0,0,2.03),(.075,.07,.16));ell('Head',(0,-.025,2.26),(.115,.095,.18))
for side in [-1,1]:
 bone((side*.13,0,1.18),(side*.16,.035,.66),.095,.085);bone((side*.16,.035,.66),(side*.17,0,.10),.06,.065)
 ell('Foot',(side*.17,-.06,.065),(.075,.16,.06))
 bone((side*.25,0,1.91),(side*.38,-.015,1.48),.073,.065);bone((side*.38,-.015,1.48),(side*.39,-.055,.97),.048,.045)
 ell('Palm',(side*.39,-.06,.87),(.047,.029,.10))
 for f in range(4):bone((side*.39+(f-1.5)*.022,-.065,.82),(side*.39+(f-1.5)*.026,-.08,.69-abs(f-1.5)*.013),.012)
bpy.ops.object.select_all(action='DESELECT')
for o in parts:o.select_set(True)
bpy.context.view_layer.objects.active=parts[0];bpy.ops.object.join();body=bpy.context.object;body.name='Watcher sculpt'
rem=body.modifiers.new('Unified organic surface','REMESH');rem.mode='VOXEL';rem.voxel_size=.012;bpy.ops.object.modifier_apply(modifier=rem.name)
smooth=body.modifiers.new('Skin smoothing','SMOOTH');smooth.factor=.7;smooth.iterations=4;bpy.ops.object.modifier_apply(modifier=smooth.name)
for p in body.data.polygons:p.use_smooth=True
for side in [-1,1]:ell('Eye glint',(side*.038,-.115,2.295),(.014,.008,.006))
bpy.ops.object.select_all(action='SELECT')
bpy.ops.wm.save_as_mainfile(filepath=r'C:\Users\Onur\Desktop\Exit7\SourceAssets\Watcher.blend')
bpy.ops.export_scene.fbx(filepath=r'C:\Users\Onur\Desktop\Exit7\Assets\Loop\Watcher.fbx',use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',add_leaf_bones=False)

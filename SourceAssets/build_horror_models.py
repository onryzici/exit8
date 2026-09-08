import bpy, math
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'Assets/Resources/Horror'
OUT.mkdir(parents=True,exist_ok=True)
def ell(name,p,s):
 bpy.ops.mesh.primitive_uv_sphere_add(segments=24,ring_count=16,location=p)
 o=bpy.context.object;o.name=name;o.scale=s;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 for f in o.data.polygons:f.use_smooth=True
 return o
def bone(name,a,b,width,depth=None):
 o=ell(name,(Vector(a)+Vector(b))/2,(width,depth or width,(Vector(b)-Vector(a)).length/2+width*.3))
 o.rotation_euler=(Vector(b)-Vector(a)).to_track_quat('Z','Y').to_euler()
 return o
def unify(parts,name,voxel=.012):
 bpy.ops.object.select_all(action='DESELECT')
 for o in parts:o.select_set(True)
 bpy.context.view_layer.objects.active=parts[0];bpy.ops.object.join();o=bpy.context.object;o.name=name
 r=o.modifiers.new('Organic joined anatomy','REMESH');r.mode='VOXEL';r.voxel_size=voxel;bpy.ops.object.modifier_apply(modifier=r.name)
 r=o.modifiers.new('Sculpt smoothing','SMOOTH');r.factor=.6;r.iterations=3;bpy.ops.object.modifier_apply(modifier=r.name)
 r=o.modifiers.new('Game mesh','DECIMATE');r.ratio=.55;bpy.ops.object.modifier_apply(modifier=r.name)
 for f in o.data.polygons:f.use_smooth=True
 return o
def hand(p,side=1,scale=1):
 x,y,z=p;parts=[ell('Palm',(x,y,z),(.058*scale,.035*scale,.11*scale))]
 for i in range(4):
  dx=(i-1.5)*.031*scale
  a=(x+dx,y,z-.07*scale);b=(x+dx*1.25,y-.02*scale,z-(.19+(1-abs(i-1.5)/2)*.05)*scale)
  c=(b[0]+side*.015*scale,b[1]-.055*scale,b[2]-.095*scale)
  parts.extend([bone('Finger',a,b,.012*scale),bone('Finger tip',b,c,.009*scale)])
 parts.append(bone('Thumb',(x-side*.05*scale,y,z+.01*scale),(x-side*.13*scale,y-.035*scale,z-.13*scale),.02*scale))
 return parts
def head(p,scale=1):
 x,y,z=p
 h=ell('Head skin',(x,y,z),(.135*scale,.113*scale,.235*scale))
 # Deep recessed eye sockets and a stretched open mouth, cut into the actual head mesh.
 for dx,dz,sizes in [(-.052,.075,(.037,.07,.036)),(.05,.067,(.036,.07,.039)),(.006,-.095,(.049,.09,.10))]:
  cutter=ell('Face cavity cutter',(x+dx*scale,y-.102*scale,z+dz*scale),tuple(v*scale for v in sizes))
  bpy.context.view_layer.objects.active=h
  m=h.modifiers.new('Carved facial cavity','BOOLEAN');m.operation='DIFFERENCE';m.object=cutter;bpy.ops.object.modifier_apply(modifier=m.name);bpy.data.objects.remove(cutter,do_unlink=True)
  ell('Mouth darkness' if dz<0 else 'Empty eye socket',(x+dx*scale,y-.069*scale,z+dz*scale),(sizes[0]*scale*.94,.025*scale,sizes[2]*scale*.94))
 bone('Nose bridge',(x,y-.106*scale,z+.065*scale),(x+.009*scale,y-.143*scale,z-.015*scale),.017*scale,.012*scale)
 for side in [-1,1]:
  bone('Brow ridge',(x+side*.024*scale,y-.10*scale,z+.117*scale),(x+side*.092*scale,y-.08*scale,z+.095*scale),.018*scale)
  ell('Ear',(x+side*.14*scale,y,z+.015*scale),(.02*scale,.023*scale,.06*scale))
 for i in range(5):
  ell('Old tooth',(x+(i-2)*.014*scale,y-.128*scale,z-.027*scale),(.005*scale,.012*scale,(.013+(.004 if i%2 else 0))*scale))
 return h
def export(name):
 bpy.ops.object.select_all(action='SELECT')
 bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'SourceAssets'/f'{name}.blend'))
 bpy.ops.export_scene.fbx(filepath=str(OUT/f'{name}.fbx'),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',add_leaf_bones=False)
 print('EXPORTED',name,sum(len(o.data.polygons) for o in bpy.context.scene.objects if o.type=='MESH'))
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
parts=[ell('Thorax',(0,.045,1.66),(.235,.13,.39)),ell('Waist',(0,.045,1.32),(.12,.085,.26)),ell('Pelvis',(0,.02,1.12),(.17,.115,.20))]
for side in [-1,1]:
 for i in range(5):parts.append(bone('Rib',(side*.03,-.077,1.83-i*.085),(side*(.19-i*.009),-.028,1.79-i*.085),.023,.019))
 parts.extend([bone('Thigh',(side*.115,.02,1.16),(side*.16,.08,.62),.074,.068),ell('Knee',(side*.16,.05,.62),(.064,.06,.07)),bone('Shin',(side*.16,.08,.62),(side*.21,0,.12),.043,.044),ell('Foot',(side*.21,-.07,.065),(.064,.18,.057))])
 shoulder=(side*.22,.045,1.93);elbow=(side*.39,.10,1.29);wrist=(side*.42,-.09,.72)
 parts.extend([bone('Upper arm',shoulder,elbow,.054,.052),ell('Elbow',elbow,(.047,.06,.055)),bone('Long forearm',elbow,wrist,.033,.037)])
 parts+=hand((side*.42,-.09,.65),side)
parts+=[bone('Twisted neck',(0,.03,1.97),(.068,-.035,2.21),.061,.052),bone('Collar bone',(-.22,-.025,1.94),(.20,-.025,1.95),.024)]
unify(parts,'Emaciated skin')
head((.075,-.05,2.39),1)
export('LongWatcher')
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
parts=[ell('Hunched back',(0,.14,.73),(.24,.37,.22)),ell('Pelvis',(0,.47,.58),(.17,.19,.16)),bone('Neck',(0,-.12,.68),(0,-.34,.52),.08)]
for side in [-1,1]:
 parts.extend([bone('Upper arm',(side*.2,-.03,.68),(side*.62,-.35,.35),.05),bone('Forearm',(side*.62,-.35,.35),(side*.69,-.77,.16),.032)])
 parts+=hand((side*.69,-.78,.13),side,.8)
 parts.extend([bone('Folded thigh',(side*.12,.45,.58),(side*.41,.76,.37),.065),bone('Bent shin',(side*.41,.76,.37),(side*.29,.46,.12),.04),ell('Foot',(side*.29,.38,.10),(.06,.18,.06))])
for i in range(6):parts.append(ell('Vertebra',(0,.06+i*.075,.94-i*.027),(.035,.04,.027)))
unify(parts,'Crawler skin')
head((0,-.42,.49),1.03)
export('CeilingCrawler')
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
parts=[bone('Forearm',(0,.03,.58),(0,0,.10),.043,.036)]+hand((0,0,0),1,1.25)
unify(parts,'Door hand skin',.008)
export('DoorHand')
# Extract an anatomically detailed forearm from the project's MIT-licensed Rocketbox mesh.
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
bpy.ops.import_scene.fbx(filepath=str(ROOT/'Assets/Commuter/Male_Adult_08.fbx'))
rig=next(o for o in bpy.context.scene.objects if o.type=='ARMATURE')
hand_bone=rig.data.bones.get('Bip01 R Hand')
fore_bone=rig.data.bones.get('Bip01 R Forearm')
wrist=rig.matrix_world@hand_bone.head_local
elbow=rig.matrix_world@fore_bone.head_local
rotation=(elbow-wrist).rotation_difference(Vector((0,0,1)))
meshes=[]
for obj in list(bpy.context.scene.objects):
 if obj.type!='MESH':continue
 allowed={g.index for g in obj.vertex_groups if g.name.startswith('Bip01 R Finger') or g.name in ('Bip01 R Hand','Bip01 R Forearm')}
 if not allowed:continue
 keep={v.index for v in obj.data.vertices if sum(g.weight for g in v.groups if g.group in allowed)>.65}
 faces=[poly for poly in obj.data.polygons if all(i in keep for i in poly.vertices)]
 if not faces:continue
 used=sorted({i for poly in faces for i in poly.vertices});mapping={old:i for i,old in enumerate(used)}
 vertices=[rotation@(obj.matrix_world@obj.data.vertices[i].co-wrist) for i in used]
 mesh=bpy.data.meshes.new('Detailed hand');mesh.from_pydata(vertices,[],[[mapping[i] for i in poly.vertices] for poly in faces]);mesh.update()
 if obj.data.uv_layers:
  uv=mesh.uv_layers.new(name='UVMap')
  for new_poly,old_poly in zip(mesh.polygons,faces):
   for ni,oi in zip(new_poly.loop_indices,old_poly.loop_indices):uv.data[ni].uv=obj.data.uv_layers.active.data[oi].uv
 result=bpy.data.objects.new('Detailed hand skin',mesh);bpy.context.collection.objects.link(result);meshes.append(result)
 for poly in mesh.polygons:poly.use_smooth=True
for obj in list(bpy.context.scene.objects):
 if obj not in meshes:bpy.data.objects.remove(obj,do_unlink=True)
export('DetailedDoorHand')

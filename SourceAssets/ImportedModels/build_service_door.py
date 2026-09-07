import bpy, math
from mathutils import Vector
root='C:/Users/Onur/Desktop/Exit7/'
bpy.ops.wm.read_factory_settings(use_empty=True)
materials={}
for name,c,metal,rough in [('Paint',(.47,.49,.48,1),0,.45),('Stainless',(.58,.6,.61,1),.9,.26),('Seal',(.022,.023,.024,1),0,.8)]:
 m=bpy.data.materials.new(name);m.diffuse_color=c;materials[name]=m

def box(name,loc,dim,mat='Paint',bevel=.002):
 bpy.ops.mesh.primitive_cube_add(size=1,location=loc);o=bpy.context.object;o.name=name;o.dimensions=dim;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);o.data.materials.append(materials[mat])
 mod=o.modifiers.new('Manufactured edge radius','BEVEL');mod.width=bevel;mod.segments=4;bpy.ops.object.modifier_apply(modifier=mod.name)
 mod=o.modifiers.new('Face weighted normals','WEIGHTED_NORMAL');mod.keep_sharp=True;bpy.ops.object.modifier_apply(modifier=mod.name)
 return o

def cyl(name,a,b,r,mat='Stainless'):
 a,b=Vector(a),Vector(b);bpy.ops.mesh.primitive_cylinder_add(vertices=48,radius=r,depth=(b-a).length,location=(a+b)/2);o=bpy.context.object;o.name=name;o.rotation_euler=(b-a).to_track_quat('Z','Y').to_euler();o.data.materials.append(materials[mat]);mod=o.modifiers.new('Machined radius','BEVEL');mod.width=.001;mod.segments=3;bpy.ops.object.modifier_apply(modifier=mod.name)
 for f in o.data.polygons:f.use_smooth=len(f.vertices)==4
 return o
box('Door recessed shadow',(0,.003,1.075),(.936,.05,2.15),'Seal')
box('Flush steel leaf',(0,-.026,1.073),(.866,.046,2.092),bevel=.0025)
for x in [-.454,.454]:
 box('Slim rebated steel frame',(x,-.032,1.09),(.032,.064,2.18))
 box('Frame folded outer lip',(x+math.copysign(.019,x),-.012,1.09),(.008,.017,2.18),bevel=.0008)
box('Mitred head frame',(0,-.032,2.165),(.94,.064,.03))
box('Satin threshold',(0,-.02,.012),(.89,.07,.009),'Stainless',.001)
for z in [.28,1.08,1.83]:
 box('Recessed hinge plate',(-.422,-.052,z),(.03,.005,.09),'Stainless',.001)
 cyl('Hinge barrel',(-.434,-.055,z-.046),(-.434,-.055,z+.046),.006)
 for d in [-.022,.022]:cyl('Hinge knuckle separation',(-.434,-.055,z+d),(-.434,-.055,z+d+.001),.0063,'Seal')
cyl('Handle rose',(.302,-.05,1.035),(.302,-.059,1.035),.028)
cyl('Handle spindle',(.302,-.059,1.035),(.302,-.087,1.035),.012)
# Turned knob with a rounded silhouette; profile is revolved around the spindle.
profile=[(.0,.011),(.003,.019),(.007,.024),(.016,.025),(.023,.023),(.026,.016),(.027,0)]
v=[];faces=[]
for dep,r in profile:
 for i in range(64):
  a=2*math.pi*i/64;v.append((.302+math.cos(a)*r,-.087-dep,1.035+math.sin(a)*r))
for j in range(len(profile)-1):
 for i in range(64):faces.append((j*64+i,j*64+(i+1)%64,(j+1)*64+(i+1)%64,(j+1)*64+i))
me=bpy.data.meshes.new('Turned knob');me.from_pydata(v,[],faces);me.update();o=bpy.data.objects.new('Turned stainless knob',me);bpy.context.collection.objects.link(o);me.materials.append(materials['Stainless'])
for f in me.polygons:f.use_smooth=True
box('Cylinder key slot',(.302,-.1145,1.035),(.003,.001,.014),'Seal',.0003)
for x in [-.452,.452]:
 for z in [.14,1.09,2.03]:
  cyl('Countersunk fixing',(x,-.064,z),(x,-.066,z),.0034)
  box('Fixing screw slot',(x,-.067,z),(.004,.001,.0008),'Seal',.0001)
# Uniform UV density for paint microstructure, and centered export coordinates.
bpy.ops.object.select_all(action='SELECT')
for o in bpy.context.selected_objects:
 if o.type=='MESH':o.location.z-=1.09
bpy.ops.wm.save_as_mainfile(filepath=root+'SourceAssets/ImportedModels/Station-service-door.blend')
bpy.ops.export_scene.fbx(filepath=root+'Assets/StationRefinement/ServiceDoor.fbx',use_selection=True,object_types={'MESH'},add_leaf_bones=False,bake_anim=False,axis_forward='-Z',axis_up='Y')

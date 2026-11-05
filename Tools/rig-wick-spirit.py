import bpy,bmesh,math,json,pathlib
from mathutils import Vector
OUT=pathlib.Path('C:/Users/10029/Desktop/食光兽/美术/灯芯精灵_骨骼绑定_v01');OUT.mkdir(exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath='C:/Users/10029/Desktop/食光兽/美术/灯芯精灵_99446面_格式转换/WickSpirit.fbx')
mesh=next(o for o in bpy.context.scene.objects if o.type=='MESH');mesh.name='WickSpirit_Body'
bpy.context.view_layer.objects.active=mesh
bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
for v in mesh.data.vertices:
 x,y,z=v.co;v.co=(y-.024,-(x-.06),z)
bm=bmesh.new();bm.from_mesh(mesh.data);bmesh.ops.remove_doubles(bm,verts=list(bm.verts),dist=1e-6)
boundary=[e for e in bm.edges if e.is_boundary]
if boundary:bmesh.ops.holes_fill(bm,edges=boundary,sides=6)
bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(mesh.data);bm.free()
for p in mesh.data.polygons:p.use_smooth=True
arm=bpy.data.armatures.new('WickSpirit_Skeleton');rig=bpy.data.objects.new('WickSpirit_Rig',arm);bpy.context.collection.objects.link(rig)
bpy.ops.object.select_all(action='DESELECT');rig.select_set(True);bpy.context.view_layer.objects.active=rig;rig.show_in_front=True
bpy.ops.object.mode_set(mode='EDIT')
def bone(name,head,tail,parent=None,deform=True):
 b=arm.edit_bones.new(name);b.head=head;b.tail=tail;b.use_deform=deform
 if parent:b.parent=arm.edit_bones[parent]
 return b
bone('Root',(0,0,0),(0,0,.075),deform=False)
bone('Hips',(0,0,.135),(0,0,.185),'Root')
bone('Spine',(0,0,.185),(0,0,.23),'Hips')
bone('Chest',(0,0,.23),(0,0,.275),'Spine')
bone('Neck',(0,0,.275),(0,0,.335),'Chest')
bone('Head',(0,0,.335),(0,0,.78),'Neck')
for side,s in [('L',1),('R',-1)]:
 bone('Shoulder.'+side,(0,0,.259),(s*.064,0,.259),'Chest')
 bone('UpperArm.'+side,(s*.064,0,.259),(s*.115,-.006,.211),'Shoulder.'+side)
 bone('Forearm.'+side,(s*.115,-.006,.211),(s*.151,-.01,.163),'UpperArm.'+side)
 bone('Hand.'+side,(s*.151,-.01,.163),(s*.164,-.015,.133),'Forearm.'+side)
 bone('UpperLeg.'+side,(s*.061,0,.144),(s*.067,-.012,.087),'Hips')
 bone('LowerLeg.'+side,(s*.067,-.012,.087),(s*.071,0,.038),'UpperLeg.'+side)
 bone('Foot.'+side,(s*.071,0,.038),(s*.071,-.052,.025),'LowerLeg.'+side)
 bone('Toes.'+side,(s*.071,-.052,.025),(s*.071,-.073,.021),'Foot.'+side)
bpy.ops.object.mode_set(mode='OBJECT');mesh.select_set(True);rig.select_set(True);bpy.context.view_layer.objects.active=rig
try:bpy.ops.object.parent_set(type='ARMATURE_AUTO');heat='Blender bone-heat automatic weights'
except RuntimeError as e:heat='Bone heat failed: '+str(e)
weighted=sum(any(g.weight>1e-6 for g in v.groups) for v in mesh.data.vertices)
print('HEAT_RESULT',heat,weighted,len(mesh.data.vertices),flush=True)
if weighted<len(mesh.data.vertices)*.95:
 # A closed voxel proxy avoids tiny nonmanifold seams in the generated surface.
 proxy=mesh.copy();proxy.data=mesh.data.copy();bpy.context.collection.objects.link(proxy);proxy.name='Temporary_WeightProxy'
 proxy.modifiers.clear();proxy.vertex_groups.clear();proxy.parent=None
 bpy.ops.object.select_all(action='DESELECT');proxy.select_set(True);bpy.context.view_layer.objects.active=proxy
 remesh=proxy.modifiers.new('Closed weight-solving surface','REMESH');remesh.mode='VOXEL';remesh.voxel_size=.004;remesh.use_smooth_shade=True
 bpy.ops.object.modifier_apply(modifier=remesh.name)
 rig.select_set(True);bpy.context.view_layer.objects.active=rig;bpy.ops.object.parent_set(type='ARMATURE_AUTO')
 count=sum(any(g.weight>1e-6 for g in v.groups) for v in proxy.data.vertices)
 print('PROXY_HEAT',count,len(proxy.data.vertices),flush=True)
 if count<len(proxy.data.vertices)*.95:raise RuntimeError('Closed proxy heat weights incomplete')
 from mathutils.kdtree import KDTree
 tree=KDTree(len(proxy.data.vertices))
 for v in proxy.data.vertices:tree.insert(v.co,v.index)
 tree.balance();mesh.vertex_groups.clear()
 for group in proxy.vertex_groups:mesh.vertex_groups.new(name=group.name)
 for v in mesh.data.vertices:
  weights={};total=0
  for co,index,d in tree.find_n(v.co,4):
   factor=1/max(d,1e-5)**2;total+=factor
   for g in proxy.data.vertices[index].groups:weights[g.group]=weights.get(g.group,0)+g.weight*factor
  for group,w in weights.items():mesh.vertex_groups[group].add([v.index],w/total,'REPLACE')
 bpy.data.objects.remove(proxy,do_unlink=True);heat='Blender bone heat on closed voxel proxy, interpolated to original textured mesh'
# Keep the large sculpted flame head rigid; blend the narrow neck to avoid a hard seam.
head=mesh.vertex_groups.get('Head')
for v in mesh.data.vertices:
 if v.co.z>=.335:
  for g in list(v.groups):mesh.vertex_groups[g.group].remove([v.index])
  head.add([v.index],1,'REPLACE')
# Remove tiny far-reaching weights and normalize to four influences for game export.
for v in mesh.data.vertices:
 weights=[]
 for g in v.groups:
  w=g.weight;name=mesh.vertex_groups[g.group].name
  if .12<v.co.z<.23 and abs(v.co.x)<.07 and any(k in name for k in ['Arm','Hand','Shoulder']):w=0
  if w>1e-5:weights.append((g.group,w))
 weights=sorted(weights,key=lambda t:t[1],reverse=True)[:4];total=sum(w for _,w in weights)
 for g in list(v.groups):mesh.vertex_groups[g.group].remove([v.index])
 for idx,w in weights:mesh.vertex_groups[idx].add([v.index],w/total,'REPLACE')
for im in bpy.data.images:
 if im.has_data and im.source=='FILE':
  try:im.pack()
  except RuntimeError:pass
mesh.data.calc_loop_triangles()
report={'method':heat,'boneCount':len(arm.bones),'triangles':len(mesh.data.loop_triangles),'vertices':len(mesh.data.vertices),'unweighted':sum(not any(g.weight>1e-6 for g in v.groups) for v in mesh.data.vertices),'maxInfluences':max(len(v.groups) for v in mesh.data.vertices),'notes':['Skeleton fitted by script to the character; skin weights generated by Blender bone heat.','Rigid flame head above neck; maximum four normalized influences.','QA motion is a deformation check, not finished gameplay animation.']}
assert report['unweighted']==0 and report['triangles']<100000
# Export a clean rest-pose skeleton without helper cameras or extra terminal bones.
bpy.ops.object.select_all(action='DESELECT');mesh.select_set(True);rig.select_set(True);bpy.context.view_layer.objects.active=rig
bpy.ops.export_scene.fbx(filepath=str(OUT/'WickSpirit_Rigged.fbx'),use_selection=True,object_types={'MESH','ARMATURE'},add_leaf_bones=False,bake_anim=False,path_mode='COPY',embed_textures=True,axis_forward='-Z',axis_up='Y')
# Keyed FK pose checks remain editable in the blend.
for pb in rig.pose.bones:pb.rotation_mode='XYZ'
poses={1:{},20:{'UpperArm.L':(0,0,-45),'Forearm.L':(0,0,-25),'Head':(0,12,0)},40:{'UpperLeg.L':(25,0,0),'LowerLeg.L':(-25,0,0),'UpperLeg.R':(-15,0,0),'UpperArm.R':(-20,0,0)},60:{'UpperLeg.L':(15,0,0),'UpperLeg.R':(15,0,0),'LowerLeg.L':(-30,0,0),'LowerLeg.R':(-30,0,0),'Spine':(10,0,0)},80:{'Head':(0,25,0),'Forearm.R':(0,0,25)},100:{}}
for frame,pose in poses.items():
 for pb in rig.pose.bones:
  pb.rotation_euler=[math.radians(v) for v in pose.get(pb.name,(0,0,0))];pb.keyframe_insert(data_path='rotation_euler',frame=frame)
rig.animation_data.action.name='QA_PoseCheck';scene=bpy.context.scene;scene.frame_start=1;scene.frame_end=100;scene.render.fps=24;scene.frame_set(1)
# Portable material-aware orthographic inspection scene.
scene.render.engine='CYCLES';scene.cycles.samples=16
scene.render.resolution_x=720;scene.render.resolution_y=720;scene.render.resolution_percentage=100
scene.world=bpy.data.worlds.new('Preview World');scene.world.color=(.16,.16,.16)
def point(obj,target):obj.rotation_euler=(Vector(target)-obj.location).to_track_quat('-Z','Y').to_euler()
bpy.ops.object.camera_add(location=(1.4,-3,.95));cam=bpy.context.object;cam.name='QA_Camera';cam.data.type='ORTHO';cam.data.ortho_scale=1.35;point(cam,(0,0,.48));scene.camera=cam
for loc,power,size in [((1,-2,3),180,3),((-2,-1,1),95,2),((0,2,2),130,2)]:
 bpy.ops.object.light_add(type='AREA',location=loc);light=bpy.context.object;light.data.energy=power;light.data.shape='DISK';light.data.size=size;point(light,(0,0,.45))
scene.render.image_settings.file_format='PNG';scene.render.film_transparent=False
for frame in [1,20,40,80]:
 scene.frame_set(frame);scene.render.filepath=str(OUT/('pose-%03d.png'%frame));bpy.ops.render.render(write_still=True)
scene.frame_set(1);bpy.ops.object.select_all(action='DESELECT');rig.select_set(True);bpy.context.view_layer.objects.active=rig
for screen in bpy.data.screens:
 for area in screen.areas:
  if area.type=='VIEW_3D':
   area.spaces.active.region_3d.view_distance=1.6;area.spaces.active.region_3d.view_location=(0,0,.48);area.spaces.active.shading.type='MATERIAL'
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'WickSpirit_Rigged.blend'))
(OUT/'rig-check.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
print('RIG_COMPLETE',json.dumps(report),flush=True)

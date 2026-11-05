import bpy,math,json,pathlib
from mathutils import Vector,Quaternion,Matrix
P=pathlib.Path(__file__).resolve().parents[1]
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath=str(P/'Source/WickSpirit_Rigged.fbx'))
t=next(o for o in bpy.context.scene.objects if o.type=='ARMATURE');mesh=next(o for o in bpy.context.scene.objects if o.type=='MESH')
t.name='WickSpirit_Rig';scene=bpy.context.scene
mapping={'Hips':'Hips','Spine':'LowerBack','Chest':'Spine','Neck':'Neck1','Head':'Head'}
for side,src in [('L','Left'),('R','Right')]:
 for a,b in [('Shoulder','Shoulder'),('UpperArm','Arm'),('Forearm','ForeArm'),('Hand','Hand'),('UpperLeg','UpLeg'),('LowerLeg','Leg'),('Foot','Foot'),('Toes','ToeBase')]:mapping[a+'.'+side]=src+b
clips=[('Jump_01','143_05','Jump distances'),('Jump_02','143_06','Jump heights'),('Jump_03','143_07','Jump sideways'),('Push_01','81_05','Push heavy object'),('Push_02','81_06','Push heavy box'),('Push_03','82_06','Pushing heavy object'),('Idle_01','140_06','Idle'),('Idle_02','140_07','Idle'),('Idle_03','139_02','Shifting weight')]
reports=[];actions=[]
rest={b.name:b.matrix_local.to_quaternion() for b in t.data.bones}
neutral={}
for name in mapping:
 b=t.data.bones[name];direction=(b.tail_local-b.head_local).normalized()
 if any(name.startswith(x) for x in ['Shoulder','UpperArm','Forearm','Hand']):goal=Vector((1 if name.endswith('L') else -1,0,0))
 elif name.startswith(('UpperLeg','LowerLeg')):goal=Vector((0,0,-1))
 elif name.startswith(('Foot','Toes')):goal=Vector((0,-1,0))
 else:goal=Vector((0,0,1))
 neutral[name]=direction.rotation_difference(goal)@rest[name]
for label,id,description in clips:
 bpy.ops.import_anim.bvh(filepath=str(P/f'Source/{id}/{id}.bvh'),global_scale=.01,axis_forward='-Z',axis_up='Y',update_scene_fps=True)
 s=bpy.context.object;source_action=s.animation_data.action;sfps=scene.render.fps/scene.render.fps_base;end=int(source_action.frame_range[1]);scene.frame_set(1)
 ref={n:s.pose.bones[src].matrix.to_quaternion().copy() for n,src in mapping.items()}
 sourceheight=s.pose.bones['Hips'].head.z;scale=.135/sourceheight
 scene.frame_set(2);initial=s.pose.bones['Hips'].head.copy();delta=s.pose.bones['Hips'].matrix.to_quaternion()@ref['Hips'].inverted();forward=delta@Vector((0,-1,0));yaw=math.atan2(forward.x,-forward.y);normal=Quaternion((0,0,1),-yaw)
 samples=[]
 for f in range(2,end+1,4):
  scene.frame_set(f)
  world={n:(normal@s.pose.bones[src].matrix.to_quaternion()@ref[n].inverted()@neutral[n]).normalized() for n,src in mapping.items()}
  # Large flame head needs proportion-aware pitch reduction; keep source yaw.
  factors={'Head':.25,'Neck':.35,'Chest':.6,'Spine':.7,'Hips':.85} if label.startswith('Push') else {'Head':.6,'Neck':.75}
  for n,factor in factors.items():
   e=(world[n]@neutral[n].inverted()).to_euler('XYZ');e.x*=factor;e.y*=factor;world[n]=e.to_quaternion()@neutral[n]
  if label.startswith('Push'):
   for n in world:
    if n.startswith(('UpperArm','Forearm','Hand')):world[n]=Quaternion((1,0,0),math.radians(-30))@world[n]
  samples.append((f,world,s.pose.bones['Hips'].head.z,min(s.pose.bones[x].head.z for x in ['LeftToeBase','RightToeBase','LeftFoot','RightFoot'])))
 floor=sorted(x[3] for x in samples)[int(len(samples)*.1)]
 bpy.data.objects.remove(s,do_unlink=True);bpy.data.actions.remove(source_action)
 t.animation_data_create();action=bpy.data.actions.new(label);action.use_fake_user=True;t.animation_data.action=action;actions.append(action);scene.render.fps=30;scene.render.fps_base=1
 evaluated=[]
 for frame,(_,world,hipz,footz) in enumerate(samples,1):
  scene.frame_set(frame)
  for pb in t.pose.bones:
   pb.rotation_mode='QUATERNION';pb.location=(0,0,0);pb.scale=(1,1,1)
   if pb.name=='Root':pb.rotation_quaternion=Quaternion();continue
   parent=pb.parent;pr=world.get(parent.name,rest[parent.name]);localrest=rest[parent.name].inverted()@rest[pb.name]
   pb.rotation_quaternion=(localrest.inverted()@pr.inverted()@world[pb.name]).normalized()
   pb.keyframe_insert(data_path='rotation_quaternion',frame=frame)
  root=t.pose.bones['Root'];bpy.context.view_layer.update()
  # Ground clearance is evaluated on the deformed mesh, not merely bone endpoints.
  dg=bpy.context.evaluated_depsgraph_get();ev=mesh.evaluated_get(dg);em=ev.to_mesh();minimum=min((mesh.matrix_world@v.co).z for v in em.vertices);ev.to_mesh_clear()
  clearance=max(0,footz-floor)*scale if label.startswith('Jump') else 0
  root.location=rest['Root'].inverted()@Vector((0,0,clearance-minimum));root.keyframe_insert(data_path='location',frame=frame);root.keyframe_insert(data_path='rotation_quaternion',frame=frame)
  evaluated.append({'frame':frame,'rootZ':clearance-minimum,'groundClearance':clearance})
 scene.frame_start=1;scene.frame_end=len(samples);scene.frame_set(1)
 report={'name':label,'sourceId':id,'description':description,'sourceFrames':[2,end],'fps':30,'frames':len(samples),'duration':(len(samples)-1)/30,'scaleRatio':scale,'maxClearance':max(x['groundClearance'] for x in evaluated),'rootMode':'horizontal in-place; evaluated mesh ground correction; source-derived vertical clearance for jumps','samples':evaluated}
 reports.append(report)
 print('RETARGETED',label,len(samples),flush=True)
 # Save independent bound FBX with one take, keeping all other actions for the master file.
 bpy.ops.object.select_all(action='DESELECT');t.select_set(True);mesh.select_set(True);bpy.context.view_layer.objects.active=t
 bpy.ops.export_scene.fbx(filepath=str(P/f'Exports/{label}.fbx'),use_selection=True,object_types={'MESH','ARMATURE'},add_leaf_bones=False,bake_anim=True,bake_anim_use_all_actions=False,bake_anim_use_nla_strips=False,bake_anim_simplify_factor=0,path_mode='COPY',embed_textures=True,axis_forward='-Z',axis_up='Y')
for im in bpy.data.images:
 if im.has_data:
  try:im.pack()
  except:pass
t.animation_data.action=actions[0];scene.frame_start=1;scene.frame_end=int(actions[0].frame_range[1]);scene.frame_set(1)
bpy.ops.wm.save_as_mainfile(filepath=str(P/'Exports/WickSpirit_9Actions.blend'))
(P/'QA/retarget-report.json').write_text(json.dumps({'mapping':mapping,'bones':len(t.data.bones),'clips':reports},indent=2))
bpy.ops.export_scene.gltf(filepath=str(P/'Exports/WickSpirit_9Actions.glb'),export_format='GLB',use_selection=True,export_animations=True,export_animation_mode='ACTIONS',export_force_sampling=True)
print('ALL_DONE',flush=True)

import bpy,math,pathlib
root=pathlib.Path('C:/Users/10029/Desktop/食光兽/美术/灯芯精灵_骨骼绑定_v01')
bpy.ops.wm.open_mainfile(filepath=str(root/'WickSpirit_Rigged.blend'))
rig=bpy.data.objects['WickSpirit_Rig'];mesh=bpy.data.objects['WickSpirit_Body'];scene=bpy.context.scene
rig.animation_data_clear()
for a in list(bpy.data.actions):bpy.data.actions.remove(a)
for name,frames in [('Idle',48),('Walk',24),('Airborne',24)]:
 rig.animation_data_create();rig.animation_data.action=bpy.data.actions.new(name)
 for frame in range(frames+1):
  t=frame/frames*math.tau;pose={}
  if name=='Idle':pose={'Chest':(math.sin(t)*1.3,0,0),'Head':(0,math.sin(t)*1.5,0)}
  elif name=='Walk':
   for side,s in [('L',1),('R',-1)]:
    phase=t+(0 if s==1 else math.pi)
    pose['UpperLeg.'+side]=(math.sin(phase)*26,0,0)
    pose['LowerLeg.'+side]=(-max(0,math.sin(phase))*27,0,0)
    pose['Foot.'+side]=(max(0,math.sin(phase))*12,0,0)
    pose['UpperArm.'+side]=(-math.sin(phase)*17,0,0)
    pose['Forearm.'+side]=(0,0,-s*8)
   pose['Chest']=(0,math.sin(t)*2,math.sin(t)*1.5)
  else:
   pose={'UpperLeg.L':(22,0,0),'UpperLeg.R':(-12,0,0),'LowerLeg.L':(-35,0,0),'LowerLeg.R':(-22,0,0),'UpperArm.L':(0,0,-14),'UpperArm.R':(0,0,14),'Forearm.L':(0,0,-12),'Forearm.R':(0,0,12)}
  for pb in rig.pose.bones:
   pb.rotation_mode='XYZ';pb.rotation_euler=[math.radians(v) for v in pose.get(pb.name,(0,0,0))];pb.keyframe_insert('rotation_euler',frame=frame+1)
 rig.animation_data.action.use_fake_user=True
rig.animation_data.action=None
for pb in rig.pose.bones:pb.rotation_euler=(0,0,0)
scene.frame_set(1)
bpy.ops.object.select_all(action='DESELECT');mesh.select_set(True);rig.select_set(True);bpy.context.view_layer.objects.active=rig
out=pathlib.Path('UnityProject/Assets/Models/Rigged');out.mkdir(parents=True,exist_ok=True)
bpy.ops.export_scene.fbx(filepath=str(out.resolve()/'WickSpirit_Game.fbx'),use_selection=True,object_types={'MESH','ARMATURE'},add_leaf_bones=False,bake_anim=True,bake_anim_use_all_actions=True,bake_anim_use_nla_strips=False,bake_anim_simplify_factor=0,path_mode='COPY',embed_textures=True,axis_forward='-Z',axis_up='Y')
print('GAME_ANIMATIONS_EXPORTED')

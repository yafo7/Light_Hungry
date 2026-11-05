import bpy,json,pathlib
P=pathlib.Path(__file__).resolve().parents[1]
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath=str(P/'Source/WickSpirit_Rigged.fbx'))
t=next(o for o in bpy.context.scene.objects if o.type=='ARMATURE')
data={'target':{'world':[list(r) for r in t.matrix_world],'bones':{b.name:{'head':list(b.head_local),'tail':list(b.tail_local)} for b in t.data.bones}}}
bpy.ops.import_anim.bvh(filepath=str(P/'Source/81_06/81_06.bvh'),global_scale=.01,axis_forward='-Z',axis_up='Y',use_fps_scale=False,update_scene_fps=True)
s=bpy.context.object;bpy.context.scene.frame_set(1)
data['source']={'world':[list(r) for r in s.matrix_world],'bones':{b.name:{'head':list(b.head_local),'tail':list(b.tail_local),'posehead':list(s.pose.bones[b.name].head),'posetail':list(s.pose.bones[b.name].tail)} for b in s.data.bones},'range':list(s.animation_data.action.frame_range),'fps':bpy.context.scene.render.fps}
(P/'QA/inspection.json').write_text(json.dumps(data,indent=2))
print('INSPECTED',len(t.data.bones),len(s.data.bones),flush=True)

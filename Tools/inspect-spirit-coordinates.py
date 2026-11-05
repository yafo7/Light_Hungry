import bpy,json
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath='C:/Users/10029/Desktop/食光兽/美术/灯芯精灵_99446面_格式转换/WickSpirit.fbx')
o=next(o for o in bpy.context.scene.objects if o.type=='MESH')
vs=[o.matrix_world@v.co for v in o.data.vertices]
for lo,hi in [(0,.05),(.05,.10),(.1,.17),(.17,.23),(.23,.3),(.3,.35),(.35,.4),(.4,.5)]:
 p=[v for v in vs if lo<=v.z<hi]
 print(lo,hi,len(p),[[min(v[k] for v in p),max(v[k] for v in p)] for k in range(3)])

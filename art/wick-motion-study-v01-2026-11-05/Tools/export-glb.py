import bpy,pathlib,json
P=pathlib.Path(__file__).resolve().parents[1]
bpy.ops.wm.open_mainfile(filepath=str(P/'Exports/WickSpirit_9Actions.blend'))
bpy.ops.export_scene.gltf(filepath=str(P/'Exports/WickSpirit_9Actions.glb'),export_format='GLB',use_selection=True,export_animations=True,export_animation_mode='ACTIONS',export_force_sampling=True)
print('ACTIONS',[(a.name,list(a.frame_range)) for a in bpy.data.actions],flush=True)

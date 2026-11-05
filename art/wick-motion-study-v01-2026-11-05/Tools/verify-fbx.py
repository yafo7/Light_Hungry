import bpy,pathlib,json,math
P=pathlib.Path(__file__).resolve().parents[1];results=[]
for file in sorted((P/'Exports').glob('*.fbx')):
 bpy.ops.wm.read_factory_settings(use_empty=True);bpy.ops.import_scene.fbx(filepath=str(file));rig=next(o for o in bpy.context.scene.objects if o.type=='ARMATURE');mesh=next(o for o in bpy.context.scene.objects if o.type=='MESH');action=rig.animation_data.action;start,end=action.frame_range;mins=[];finite=True
 for f in [start+(end-start)*k/8 for k in range(9)]:
  bpy.context.scene.frame_set(int(f),subframe=f-int(f));ev=mesh.evaluated_get(bpy.context.evaluated_depsgraph_get());m=ev.to_mesh();z=[(mesh.matrix_world@v.co).z for v in m.vertices];finite=finite and all(math.isfinite(x) for x in z);mins.append(min(z));ev.to_mesh_clear()
 row={'clip':file.stem,'bones':len(rig.data.bones),'vertices':len(mesh.data.vertices),'actions':[a.name for a in bpy.data.actions],'frameRange':[start,end],'finiteMesh':finite,'minimumSampledZ':min(mins),'maximumSampledClearance':max(mins),'unweighted':sum(not v.groups for v in mesh.data.vertices),'textures':sum(i.has_data for i in bpy.data.images)};results.append(row);print('VERIFIED',row,flush=True)
(P/'QA/fbx-roundtrip.json').write_text(json.dumps(results,indent=2))

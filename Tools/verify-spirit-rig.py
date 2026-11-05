import bpy,json,pathlib,math
out=pathlib.Path('C:/Users/10029/Desktop/食光兽/美术/灯芯精灵_骨骼绑定_v01')
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath=str(out/'WickSpirit_Rigged.fbx'))
rig=next(o for o in bpy.context.scene.objects if o.type=='ARMATURE')
mesh=next(o for o in bpy.context.scene.objects if o.type=='MESH')
mesh.data.calc_loop_triangles()
report={'reimportedFBX':True,'bones':len(rig.data.bones),'triangles':len(mesh.data.loop_triangles),'unweighted':sum(not any(g.weight>1e-6 for g in v.groups) for v in mesh.data.vertices),'maxInfluences':max(sum(g.weight>1e-6 for g in v.groups) for v in mesh.data.vertices),'armatureModifier':any(m.type=='ARMATURE' and m.object==rig for m in mesh.modifiers),'uvLayers':len(mesh.data.uv_layers)}
images=[n.image for m in mesh.data.materials if m.node_tree for n in m.node_tree.nodes if n.type=='TEX_IMAGE' and n.image]
report['loadedTextures']=sum(len(i.pixels)>0 for i in images)
bpy.context.view_layer.update();dg=bpy.context.evaluated_depsgraph_get();obj=mesh.evaluated_get(dg);ev=obj.to_mesh();rest=[v.co.copy() for v in ev.vertices];obj.to_mesh_clear()
pb=rig.pose.bones['UpperArm.L'];pb.rotation_mode='XYZ';pb.rotation_euler.z=-.6;bpy.context.view_layer.update()
obj=mesh.evaluated_get(dg);ev=obj.to_mesh();dist=[(v.co-rest[i]).length for i,v in enumerate(ev.vertices)];obj.to_mesh_clear()
report['movedVerticesOnArmPose']=sum(d>.001 for d in dist);report['maxDisplacement']=max(dist)
assert report['unweighted']==0 and report['maxInfluences']<=4 and report['armatureModifier'] and report['uvLayers']>0 and report['loadedTextures']>0 and report['movedVerticesOnArmPose']>50
(out/'fbx-roundtrip-check.json').write_text(json.dumps(report,indent=2),encoding='utf-8');print(json.dumps(report))

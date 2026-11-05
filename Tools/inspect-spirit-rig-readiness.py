import bpy,bmesh,json,pathlib
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath='C:/Users/10029/Desktop/食光兽/美术/灯芯精灵_99446面_格式转换/WickSpirit.fbx')
report={'armatures':sum(o.type=='ARMATURE' for o in bpy.context.scene.objects),'meshes':[]}
for o in bpy.context.scene.objects:
 if o.type!='MESH':continue
 bm=bmesh.new();bm.from_mesh(o.data)
 bmesh.ops.remove_doubles(bm,verts=list(bm.verts),dist=0.000001)
 bm.verts.ensure_lookup_table();seen=set();components=[]
 for v in bm.verts:
  if v.index in seen:continue
  stack=[v];seen.add(v.index);count=0
  while stack:
   n=stack.pop();count+=1
   for e in n.link_edges:
    w=e.other_vert(n)
    if w.index not in seen:seen.add(w.index);stack.append(w)
  components.append(count)
 report['meshes'].append({'name':o.name,'verticesAfterSeamWeld':len(bm.verts),'connectedComponents':len(components),'largestComponents':sorted(components,reverse=True)[:15],'boundaryEdges':sum(e.is_boundary for e in bm.edges),'nonManifoldEdges':sum(not e.is_manifold for e in bm.edges),'looseVertices':sum(not v.link_faces for v in bm.verts)})
 bm.free()
pathlib.Path('Validation/mixamo-model-inspection.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print(json.dumps(report))

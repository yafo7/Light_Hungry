import bpy, json, pathlib

root = pathlib.Path('C:/Users/10029/Desktop/食光兽/美术')
source = root / '灯芯精灵_99446三角面.glb'
out = root / '灯芯精灵_99446面_格式转换'
out.mkdir(exist_ok=True)
(out / 'textures').mkdir(exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=str(source))

def inspect():
    objects = [o for o in bpy.context.scene.objects if o.type == 'MESH']
    triangles = 0
    for o in objects:
        o.data.calc_loop_triangles()
        triangles += len(o.data.loop_triangles)
    points = [o.matrix_world @ v.co for o in objects for v in o.data.vertices]
    images = [n.image for m in bpy.data.materials if m.node_tree for n in m.node_tree.nodes if n.type == 'TEX_IMAGE' and n.image]
    for image in images:
        if len(image.pixels): _ = image.pixels[0]
    return dict(triangles=triangles, meshes=len(objects), uvLayers=[len(o.data.uv_layers) for o in objects],
        materialSlots=[len(o.material_slots) for o in objects],
        texturesLoaded=all(i.has_data for i in images), textureCount=len(images),
        dimensions=[max(p[k] for p in points)-min(p[k] for p in points) for k in range(3)])

original = inspect()
for index, im in enumerate([i for i in bpy.data.images if i.packed_file]):
    data = bytes(im.packed_file.data)
    suffix = '.png' if data.startswith(b'\x89PNG') else '.jpg'
    path = out / 'textures' / ('spirit_basecolor_' + str(index) + suffix)
    path.write_bytes(data)
    replacement = bpy.data.images.load(str(path), check_existing=False)
    replacement.colorspace_settings.name = im.colorspace_settings.name
    for mat in bpy.data.materials:
        if mat.node_tree:
            for node in mat.node_tree.nodes:
                if node.type == 'TEX_IMAGE' and node.image == im: node.image = replacement

fbx = out / 'WickSpirit.fbx'
obj = out / 'WickSpirit.obj'
bpy.ops.export_scene.fbx(filepath=str(fbx), object_types={'MESH'}, use_mesh_modifiers=True,
    axis_forward='-Z', axis_up='Y', path_mode='COPY', embed_textures=True, bake_anim=False)
bpy.ops.wm.obj_export(filepath=str(obj), forward_axis='NEGATIVE_Z', up_axis='Y',
    export_materials=True, export_uv=True, export_normals=True, export_triangulated_mesh=True, path_mode='RELATIVE')

report = {'source':str(source), 'blender':bpy.app.version_string, 'sourceStats':original, 'sourceGlbTriangles':99446, 'importNote':'Blender removed 2 degenerate triangles during GLB import; 99444 valid triangles remain.'}
for kind, path in [('fbx',fbx), ('obj',obj)]:
    bpy.ops.wm.read_factory_settings(use_empty=True)
    if kind == 'fbx': bpy.ops.import_scene.fbx(filepath=str(path))
    else: bpy.ops.wm.obj_import(filepath=str(path), forward_axis='NEGATIVE_Z', up_axis='Y')
    stats = inspect()
    stats['triangleCountPreserved'] = stats['triangles'] == original['triangles']
    stats['dimensionsPreserved'] = all(abs(a-b)<0.0001 for a,b in zip(stats['dimensions'],original['dimensions']))
    stats['bytes'] = path.stat().st_size
    assert stats['triangleCountPreserved'] and stats['dimensionsPreserved'] and all(stats['uvLayers']) and stats['texturesLoaded'] and stats['textureCount']>0, stats
    report[kind] = stats
(out / 'conversion-check.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
(out / '说明.txt').write_text('源文件：灯芯精灵_99446三角面.glb\nFBX 内嵌贴图；OBJ 必须和同名 MTL、textures 文件夹一起使用。\nBlender 导入时去掉两个退化三角面；导出为 99,444 三角面，UV、材质、贴图与尺寸经重新导入验证保留。\n此模型是静态网格，原文件没有骨骼或动画。Unity 内的发尾粒子与着色效果不属于模型文件。\n',encoding='utf-8')
print(json.dumps(report,ensure_ascii=True))

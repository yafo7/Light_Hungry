import pathlib,json,hashlib,zipfile
P=pathlib.Path(__file__).resolve().parents[1]
sha=lambda f:hashlib.sha256(f.read_bytes()).hexdigest()
tree=json.loads((P/'Source/tree.json').read_text());ids=['143_05','143_06','143_07','81_05','81_06','82_06','140_06','140_07','139_02'];rows=[]
for id in ids:
 entry=next(x for x in tree['tree'] if x['path'].endswith('/'+id+'.zip'))
 rows.append({'id':id,'downloadURL':'https://raw.githubusercontent.com/Shriinivas/cmubvh/main/'+entry['path'],'repositoryBlobSha':entry['sha'],'zipSHA256':sha(P/f'Source/{id}.zip'),'bvhSHA256':sha(P/f'Source/{id}/{id}.bvh')})
orig=pathlib.Path('C:/Users/10029/Desktop/食光兽/美术/灯芯精灵_骨骼绑定_v01/WickSpirit_Rigged.fbx')
data={'date':'2026-11-05','repository':'https://github.com/Shriinivas/cmubvh','repositoryTreeSnapshot':tree['sha'],'sourceFBX':str(orig),'sourceFBXSHA256':sha(orig),'copiedSourceUnchanged':sha(orig)==sha(P/'Source/WickSpirit_Rigged.fbx'),'motions':rows}
(P/'sources.json').write_text(json.dumps(data,ensure_ascii=False,indent=2),encoding='utf-8')
fbx=json.loads((P/'QA/fbx-roundtrip.json').read_text());browser=json.loads((P/'QA/browser-checks.json').read_text());checks={'nineBoundFBX':len(fbx)==9,'originalFBXUnchanged':data['copiedSourceUnchanged'],'all22Bones':all(x['bones']==22 for x in fbx),'allWeighted':all(x['unweighted']==0 for x in fbx),'allTextured':all(x['textures']>=1 for x in fbx),'finiteGeometry':all(x['finiteMesh'] for x in fbx),'sampledFloorPenetrationBelow1mm':all(x['minimumSampledZ']>=-.001 for x in fbx),**browser['checks']}
(P/'QA/final-checks.json').write_text(json.dumps({'checks':checks,'diagnostics':{'strict0_1mmFloorTolerance':all(x['minimumSampledZ']>=-.0001 for x in fbx),'maxSampledPenetrationModelUnits':max(0,-min(x['minimumSampledZ'] for x in fbx)),'floorToleranceNote':'Subframe interpolation after FBX roundtrip leaves approximately 0.207 mm penetration in Push_01 assuming 1 unit = 1 metre; within the 1 mm preview tolerance, not exact contact.'},'validationBoundary':'Geometry checked at 9 timestamps per clip. No exhaustive self-collision, hand-object IK or Unity scene validation.'},indent=2))
assert all(checks.values()),checks
files=[f for f in sorted(P.rglob('*')) if f.is_file() and f.name!='SHA256SUMS.txt' and f.suffix!='.blend1']
(P/'SHA256SUMS.txt').write_text('\n'.join(sha(f)+'  '+f.relative_to(P).as_posix() for f in files),encoding='utf-8')
files.append(P/'SHA256SUMS.txt');z=P.parent/'灯芯精灵_九组动作绑定验证_v01.zip'
with zipfile.ZipFile(z,'w',zipfile.ZIP_DEFLATED,compresslevel=5) as out:
 for f in files:out.write(f,P.name+'/'+f.relative_to(P).as_posix())
with zipfile.ZipFile(z) as inp:
 assert inp.testzip() is None
 for f in files:assert hashlib.sha256(inp.read(P.name+'/'+f.relative_to(P).as_posix())).hexdigest()==sha(f)
z.with_suffix('.zip.sha256.txt').write_text(sha(z)+'  '+z.name,encoding='utf-8')
print(json.dumps({'checks':checks,'zip':str(z),'files':len(files),'bytes':z.stat().st_size},ensure_ascii=False))

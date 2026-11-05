from pathlib import Path
import re,shutil,json,hashlib
root=Path(__file__).resolve().parent.parent
src=root/'.tools/NovaShader-sparse'
dst=root/'UnityProject-NOVA-URP'
shutil.copytree(src/'Assets/Nova',dst/'Packages/jp.co.cyberagent.nova',dirs_exist_ok=True)
shutil.copy2(src/'LICENSE.md',dst/'Packages/jp.co.cyberagent.nova/LICENSE.md')
lookup={}
for meta in (src/'Assets').rglob('*.meta'):
 m=re.search(r'^guid: (\w+)',meta.read_text(errors='replace'),re.M)
 if m:lookup[m[1]]=Path(str(meta)[:-5])
queue=[src/'Assets/Samples/Prefabs/pfb_eff_fire.prefab'];seen=set();records=[]
while queue:
 p=queue.pop()
 if p in seen or not p.is_file():continue
 seen.add(p)
 if 'Nova' in p.relative_to(src).parts:continue
 target=dst/'Assets/NovaSamples'/p.relative_to(src/'Assets/Samples')
 target.parent.mkdir(parents=True,exist_ok=True)
 shutil.copy2(p,target)
 meta=Path(str(p)+'.meta')
 if meta.exists():shutil.copy2(meta,Path(str(target)+'.meta'))
 records.append({'source':str(p.relative_to(src)).replace('\\','/'),'sha256':hashlib.sha256(p.read_bytes()).hexdigest()})
 if p.suffix in ('.prefab','.mat','.asset'):
  for guid in re.findall(r'guid: ([0-9a-f]{32})',p.read_text(errors='replace')):
   if guid in lookup:queue.append(lookup[guid])
(dst/'Assets/NovaSamples/LICENSE.md').write_bytes((src/'LICENSE.md').read_bytes())
(root/'Validation/nova-urp-v01/source-manifest.json').write_text(json.dumps({'repository':'https://github.com/CyberAgentGameEntertainment/NovaShader','tag':'2.0.0','commit':'36f6d890b06f7c2ae01c7f15f0ed2a2b42c65572','license':'MIT','samples':records},indent=2),encoding='utf-8')
print('Official sample dependencies:',len(records))

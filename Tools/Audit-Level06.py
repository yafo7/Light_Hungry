"""Read-only asset/package inspection; writes a delivery record after Unity validation."""
from pathlib import Path
import hashlib
import json
import re
import tarfile
from PIL import Image

root = Path(__file__).resolve().parents[1]
art = root / 'art/level06-unity-assets-v01-2026-11-05'
project = root / 'UnityProject-NOVA-URP'
validation = root / 'Validation/level06'

def summary(path):
    data = json.loads(path.read_text(encoding='utf-8-sig'))
    return dict(completed=data['completed'], assertions=len(data['checks']),
                failed=[c['name'] for c in data['checks'] if not c['passed']],
                runtime_errors=data['errors'], jumps=data['jumps'], deaths=data['deaths'],
                transformations=data['transformations'])

images = []
for name in ['background.png', 'terrain-atlas.png', 'props-atlas.png']:
    p = art / name
    with Image.open(p) as im:
        images.append(dict(file=name, size=list(im.size), mode=im.mode,
                           alpha_range=list(im.getchannel('A').getextrema()) if 'A' in im.getbands() else None,
                           sha256=hashlib.sha256(p.read_bytes()).hexdigest(),
                           imported_copy_identical=p.read_bytes() == (project / 'Assets/Art/Level06' / name).read_bytes()))

with tarfile.open(art / 'GardenReusable.unitypackage', 'r:gz') as package:
    members = package.getmembers()
    paths = sorted(package.extractfile(m).read().decode('utf-8-sig').strip()
                   for m in members if m.name.endswith('/pathname'))

scene = (project / 'Assets/Scenes/SixthGreenhouse.unity').read_text(encoding='utf-8-sig')
prefabs = []
for p in sorted((project / 'Assets/Prefabs/GardenReusable').glob('*.prefab')):
    guid = re.search(r'^guid: (\w+)', Path(str(p)+'.meta').read_text(), re.M)[1]
    pattern = r'm_SourcePrefab: \{fileID: \d+, guid: ' + guid + r','
    prefabs.append(dict(name=p.stem, packaged=p.relative_to(project).as_posix() in paths,
                        source_linked_scene_instances=len(re.findall(pattern, scene))))

log = (validation / 'Player.log').read_text(encoding='utf-8-sig', errors='replace')
report = dict(images=images, prefabs=prefabs, package_asset_paths=paths,
              scene='UnityProject-NOVA-URP/Assets/Scenes/SixthGreenhouse.unity',
              layers=[name for name in ['01_Background_NoCollision', '02_Horizontal_Foreground_Collisions',
                       '03_Reusable_Interactive_Props'] if name in scene],
              nova_runtime_log=[line for line in log.splitlines() if 'NOVA_HEAD_FLAME shader=' in line],
              level06=summary(validation / 'native-run/playthrough.json'),
              level05_before_visual_only_revision=summary(validation / 'fifth-regression/playthrough.json'),
              build=json.loads((validation / 'build-result.json').read_text()),
              limits=['Level07 is a design only.', 'Cross-project package import not tested.',
                      'Level05-to-Level06 transition is configured but not covered by these playthrough scripts.',
                      'Art prefabs need scene-specific collision and puzzle wiring.'])
report['asset_integrity_passed'] = (all(i['imported_copy_identical'] for i in images)
    and len(prefabs) == 9 and all(p['packaged'] and p['source_linked_scene_instances'] > 0 for p in prefabs)
    and len(report['layers']) == 3)
(validation / 'delivery-audit.json').write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding='utf-8')
print(json.dumps(dict(asset_integrity_passed=report['asset_integrity_passed'], prefabs=len(prefabs),
                     package_assets=len(paths), level06=report['level06'], build=report['build']),
                 ensure_ascii=False, indent=2))
if not report['asset_integrity_passed']:
    raise SystemExit(1)

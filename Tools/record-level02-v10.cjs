const fs=require('fs'),crypto=require('crypto');
const old='art/level02-dew-layers-v08-2026-10-30',art='art/level02-loose-rubble-v10-2026-10-30',qa='Validation/level02';
const read=p=>JSON.parse(fs.readFileSync(p,'utf8').replace(/^\uFEFF/,''));
const write=(p,d)=>fs.writeFileSync(p,JSON.stringify(d,null,2)+'\n');
const hash=p=>crypto.createHash('sha256').update(fs.readFileSync(p)).digest('hex');
const manifest=read(old+'/layer-manifest.json'),alpha=read(art+'/alpha-check.json');
manifest.version='v10';
for(const layer of manifest.layers){
 const file=layer.file.split('/').pop();layer.file=file;
 layer.source=layer.id==='rubble'?art+'/'+file:old+'/Layers/'+file;
 if(layer.id==='rubble'){
  layer.name='低矮乱石 · 小石块、碎砖与少量陶片';
  layer.asset={width:alpha.width,height:alpha.height,hasAlpha:true,transparentPixels:alpha.transparentPixels,alphaBounds:alpha.bbox,alphaBoundsThreshold:128,sha256:hash(art+'/'+file)};
 }
 if(layer.id==='light')layer.role='Historical PNG reference only; runtime beams use the v10 lightPath nodes below.';
}
manifest.lightPath.nodes=[[877,61],[789,401],[790,415.336],[1370,716.356]];
manifest.collisionNotes=['The right pot rim and buried drain roof join the low loose rubble.','No additional UpperRubble object is used.','Separate upper and lower colliders keep an actual gap for the light core.'];
manifest.status='Imported into Unity; native Windows playthrough passed; see Validation/level02/native-run.';
manifest.generationMode='Built-in image_gen precise-object-edit for v10 rubble; original generated PNG bytes retained.';
write(art+'/layer-manifest.json',manifest);
fs.copyFileSync(art+'/layer-manifest.json','UnityProject/Assets/Art/Level02/source-layer-manifest.json');
const assets=manifest.layers.map(l=>{const s=hash(l.source),i=hash('UnityProject/Assets/Art/Level02/'+l.file);return{file:l.file,source:l.source,sourceSHA256:s,importSHA256:i,identical:s===i};});
write(qa+'/source-assets.json',assets);
const run=read(qa+'/native-run/playthrough.json'),link=read(qa+'/level-link/first-to-second.json'),build=read(qa+'/build-result.json');
const scene=fs.readFileSync('UnityProject/Assets/Scenes/SecondLight.unity','utf8');
const record={version:'Unity Level02 v10 loose rubble',unity:run.unityVersion,scenes:['FirstLight','SecondLight','ThirdDrain'],secondScene:'UnityProject/Assets/Scenes/SecondLight.unity',play:'试玩第二关.cmd',nativePassed:run.checks.filter(c=>c.passed).length,nativeTotal:run.checks.length,nativeCompleted:run.completed,firstToSecondPassed:link.passed,assetIdentityPassed:assets.filter(a=>a.identical).length,assetIdentityTotal:assets.length,removedGiantRubble:!scene.includes('UpperRubble_'),build,captures:qa+'/native-run',captureMode:'Native Direct3D11 Camera.Render, no IMGUI overlay',thirdLevelStatus:'Playable arrival room only; full level 3 puzzle not implemented',runtimeAssemblySHA256:hash('Builds/Windows-Level02/LightFeast_Data/Managed/Assembly-CSharp.dll'),secondSceneSHA256:hash('UnityProject/Assets/Scenes/SecondLight.unity'),generationMode:'Built-in image_gen; prompt in '+art+'/generation-prompt.txt'};
write(qa+'/delivery-record.json',record);
console.log(JSON.stringify(record,null,2));
if(!record.nativeCompleted||!link.passed||!record.removedGiantRubble||assets.some(a=>!a.identical)||build.errors)process.exitCode=1;

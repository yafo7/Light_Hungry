// Registers unmodified generated PNG assets and creates an editable vector light path.
// Does not edit/cut/repaint generated image pixels.
const fs=require('fs'),path=require('path'),crypto=require('crypto');
const sharp=require('C:/Users/10029/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp');
const root=path.resolve(__dirname,'../art/level02-dew-layers-v08-2026-10-30');
const W=1774,H=887;
const layers=[
 {id:'background',name:'背景 · 陶盆内壁与暗渠后壁',file:'Layers/00-background.png',matrix:[1,0,0,1,0,0],order:0,role:'静态背景，不加行走碰撞'},
 {id:'ground',name:'地面 · 苔地／落脚台／下行石阶',file:'Layers/10-ground.png',matrix:[1,0,0,1.1,0,-23],order:10,role:'行走与平台碰撞；按顶缘另建简化碰撞轮廓'},
 {id:'leaf',name:'叶片与露珠 · 折射载体',file:'Layers/20-leaf-dew.png',matrix:[.41,0,0,.41,341,265],order:20,role:'独立物件；第二关初始光路已对准，后续可做微摆动画'},
 {id:'rubble',name:'碎石堆 · 实体障碍与透明窄缝',file:'Layers/30-rubble.png',matrix:[.61,0,0,.61,546,252.1],order:30,role:'上下岩块分别配置碰撞，保留真实缝口；本关不推动或清除'},
 {id:'light',name:'光线 · 入射／露珠内／出射',file:'Layers/40-light.png',matrix:[1,0,0,1,0,0],order:40,role:'独立视觉层；运行时从光路数据生成，发光范围不等同有效判定'}
];
const nodes=[[877,61],[789,401],[790,429],[1370,713.2]];
const poly=nodes.map(p=>p.join(',')).join(' ');
const light=`<svg xmlns="http://www.w3.org/2000/svg" width="1774" height="887" viewBox="0 0 1774 887"><defs>
<linearGradient id="shaft" x1="0" y1="0" x2="0" y2="1"><stop stop-color="#ffd37c" stop-opacity=".19"/><stop offset="1" stop-color="#ffd37c" stop-opacity="0"/></linearGradient>
<filter id="wide" x="-100%" y="-100%" width="300%" height="300%"><feGaussianBlur stdDeviation="12"/></filter>
<filter id="soft" x="-100%" y="-100%" width="300%" height="300%"><feGaussianBlur stdDeviation="3"/></filter></defs>
<path d="M849 61L903 61L967 517L700 517Z" fill="url(#shaft)" filter="url(#wide)"/>
<polyline points="${poly}" fill="none" stroke="#ffb74a" stroke-opacity=".38" stroke-width="18" filter="url(#wide)"/>
<polyline points="${poly}" fill="none" stroke="#ffd47b" stroke-opacity=".75" stroke-width="6" filter="url(#soft)"/>
<polyline points="${poly}" fill="none" stroke="#fff2bb" stroke-opacity=".95" stroke-width="1.9" stroke-linecap="round" stroke-linejoin="round"/>
<g fill="#ffe9ac" opacity=".5">${Array.from({length:32},(_,i)=>{const y=92+(i*61)%410,x=805+(i*29)%111;return `<circle cx="${x}" cy="${y}" r="${.6+(i%4)*.25}"/>`}).join('')}</g></svg>`;
fs.writeFileSync(path.join(root,'Layers/40-light.svg'),light);
async function stats(file){const p=path.join(root,file),meta=await sharp(p).metadata();const {data,info}=await sharp(p).ensureAlpha().raw().toBuffer({resolveWithObject:true});let z=0,partial=0,solid=0,x0=W,y0=H,x1=-1,y1=-1;for(let y=0;y<info.height;y++)for(let x=0;x<info.width;x++){let a=data[(y*info.width+x)*4+3];a===0?z++:a===255?solid++:partial++;if(a>8){x0=Math.min(x0,x);y0=Math.min(y0,y);x1=Math.max(x1,x);y1=Math.max(y1,y)}}return {width:meta.width,height:meta.height,hasAlpha:!!meta.hasAlpha,transparentPixels:z,partialAlphaPixels:partial,opaquePixels:solid,alphaBounds:[x0,y0,x1,y1],sha256:crypto.createHash('sha256').update(fs.readFileSync(p)).digest('hex')}}
(async()=>{
 await sharp(Buffer.from(light)).png().toFile(path.join(root,'Layers/40-light.png'));
 for(const l of layers)l.asset=await stats(l.file);
 const spec={version:'v08',canvas:{width:W,height:H,origin:'top-left',pixelsPerCharacterHeight:63},registration:'Use each matrix as SVG pixel-space affine transform; PNG bytes are unmodified. Files are full canvas but extracted objects require these placement transforms.',layers,
  lightPath:{nodes,unit:'scene pixels',type:'authored refracted path; art guide, not a Snell-law simulation',chargeZoneHint:{x:1000,feetY:571},arrivalHint:{x:1370,feetY:728}},
  confirmed:{dewInsteadOfMirror:true,level02ContainsCreature:false,chargeSeconds:3,globalAbility:true,lightMovement:'player-controlled bidirectional along the same light path'},
  proposed:{dewPreset:true,bodyHeightPixels:63,bodyWidthPixels:26.46,moteCoreDiameterPixels:5.04,autoReformAtSafeLanding:true},
  collisionNotes:['Build simplified separate ground and rubble colliders; do not trace all plant edges.','Keep slit clearance measured normal to beam; glow can be wider than mote collider.','Prevent jumping over rubble or landing inside rock; validate in Unity graybox.','Painted back walls never define the traversable plane.'],
  generationMode:'built-in image_gen for painted assets; native SVG for runtime-oriented light overlay; no fallback API',
  status:'Art layer reconstruction and registered browser preview; not yet imported to Unity or gameplay-tested'};
 fs.writeFileSync(path.join(root,'layer-manifest.json'),JSON.stringify(spec,null,2));
 fs.writeFileSync(path.join(root,'layer-data.js'),'window.LAYER_MANIFEST='+JSON.stringify(spec)+';');
 const img=l=>`<image href="${l.file}" width="1774" height="887" transform="matrix(${l.matrix.join(' ')})"/>`;
 fs.writeFileSync(path.join(root,'registered-scene.svg'),`<svg xmlns="http://www.w3.org/2000/svg" width="1774" height="887" viewBox="0 0 1774 887">${layers.map(img).join('')}</svg>`);
 const {data,info}=await sharp(path.join(root,'Layers/30-rubble.png')).ensureAlpha().raw().toBuffer({resolveWithObject:true});
 const alpha=(x,y)=>x<0||y<0||x>=info.width||y>=info.height?0:data[(Math.round(y)*info.width+Math.round(x))*4+3];
 // Sample a finite-radius light core through the actual RGBA slit, in source coordinates.
 const radius=2.52/.61,n=Math.hypot(1,.49),fail=[];let count=0,maxAlpha=0;
 for(let x=880;x<=1320;x+=1){const y=.49*x+94;for(let offset=-radius;offset<=radius;offset+=.5){const a=alpha(x-offset*.49/n,y+offset/n);maxAlpha=Math.max(maxAlpha,a);count++;if(a>=128)fail.push({x,y,offset,a});}}
 const qa={generatedAt:new Date().toISOString(),layerCount:layers.length,allCanvasMatch:layers.every(l=>l.asset.width===W&&l.asset.height===H),foregroundAlpha:layers.slice(1).every(l=>l.asset.hasAlpha&&l.asset.transparentPixels>0),slitCheck:{samples:count,coreDiameterScenePixels:5.04,maxAlpha,blockedSamples:fail.length,firstFailures:fail.slice(0,6),scope:'Image alpha only, not Unity collision or physical optics'},artReconstructionNotLossless:true};
 fs.writeFileSync(path.join(root,'QA/alpha-and-slit.json'),JSON.stringify(qa,null,2));console.log(JSON.stringify(qa));
})();

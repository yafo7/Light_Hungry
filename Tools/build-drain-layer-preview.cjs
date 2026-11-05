const fs=require('fs'),path=require('path'),crypto=require('crypto');
const sharp=require('C:/Users/10029/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp');
const root=path.resolve(__dirname,'../art/level03-continuous-layers-v04-2026-10-31');
const W=2170,H=725;
const atlas='Props/props-atlas.png';
const sprites=[
 ['bulb','荧光球茎',[18,0,397,388],[.5,.04]],
 ['root-knot','唤醒根结',[450,8,365,400],[.5,.9]],
 ['mirror','升降反光片',[910,75,282,274],[.5,.5]],
 ['counterweight','配重踏板',[16,514,386,240],[.5,.36]],
 ['winch','绞盘',[444,505,368,235],[.5,.94]],
 ['guide','固定滑轮导架',[938,413,225,420],[.5,.05]],
 ['ferry','移动踏板',[16,947,402,214],[.5,.32]],
 ['wheel','水轮转子',[448,844,368,359],[.5,.5]],
 ['scale','食光兽光鳞',[897,833,340,421],[.52,.5]]
].map(([id,name,r,pivot])=>({id,name,file:atlas,sourceRect:{x:r[0],y:r[1],width:r[2],height:r[3]},unityRect:{x:r[0],y:1254-r[1]-r[3],width:r[2],height:r[3]},pivot,preview:`Props/${id}.svg`}));
const instances=[
 ['guide-A','guide',[516,83,155,485]],
 ['bulb-A','bulb',[673,-3,126,124]],
 ['root-switch','root-knot',[88,304,51,56]],
 ['winch-left','winch',[372,369,80,51]],
 ['winch-right','winch',[870,298,80,51]],
 ['counterweight-A','counterweight',[510,472,164,102]],
 ['mirror-A','mirror',[690,170,92,90]],
 ['water-wheel','wheel',[1810,438,134,131]],
 ['moving-deck','ferry',[1818,375,120,64]],
 ['feeding-scale','scale',[2022,125,160,198]]
].map(([id,sprite,r])=>({id,sprite,rect:{x:r[0],y:r[1],width:r[2],height:r[3]},...(id==='guide-A'?{sourceRectOverride:{x:938,y:413,width:225,height:310},note:'Only upper pulleys and straight guide rods; omit bottom crossbar so open-water gap has no false standing surface.'}:{})}));
const img=(file,x=0,y=0,w=W,h=H)=>`<image href="${file}" x="${x}" y="${y}" width="${w}" height="${h}" preserveAspectRatio="none"/>`;
const sprite=(s,r,id='')=>`<svg ${id?`id="${id}"`:''} x="${r.x}" y="${r.y}" width="${r.width}" height="${r.height}" viewBox="${s.sourceRect.x} ${s.sourceRect.y} ${s.sourceRect.width} ${s.sourceRect.height}" preserveAspectRatio="none" overflow="hidden">${img(atlas,0,0,1254,1254)}</svg>`;
const defs=`<defs><filter id="beam-soft" x="-20%" y="-100%" width="140%" height="300%"><feGaussianBlur stdDeviation="6"/></filter><clipPath id="surface-window"><rect y="542" width="2170" height="183"/></clipPath><marker id="arrow" viewBox="0 0 10 10" refX="8" refY="5" markerWidth="5" markerHeight="5" orient="auto-start-reverse"><path d="M 0 0 L 10 5 L 0 10 z" fill="#cfcbff"/></marker></defs>`;
function body(){return `${defs}
 <g data-layer="background"><svg width="2170" height="725" viewBox="0 10 2170 702" preserveAspectRatio="none" overflow="hidden">${img('Layers/00-background.png',0,0,2170,725)}</svg></g>
 <g data-layer="water">${img('Layers/20-water.png')}</g>
 <g data-layer="platforms">${img('Layers/10-platforms.png')}</g>
 <g data-layer="props">
  <g id="mechanism-lines" fill="none" stroke-linecap="round">
   <path d="M690 76 V353 M700 76 V353 M1719 407 H2010" stroke="#1c1811" stroke-width="11"/>
   <path d="M690 76 V353 M700 76 V353 M1719 407 H2010" stroke="#a88647" stroke-width="4"/>
   <path d="M407 369 V123 Q407 105 434 105 H546 M658 105 H693" stroke="#302319" stroke-width="5"/>
   <path d="M407 369 V123 Q407 105 434 105 H546 M658 105 H693" stroke="#c2a05b" stroke-width="2"/>
   <path id="rope-A" d="M535 112 V482 M649 112 V482" stroke="#241b12" stroke-width="6"/>
   <path id="rope-A-highlight" d="M535 112 V482 M649 112 V482" stroke="#b89a59" stroke-width="2"/>
   <path d="M909 308 H948 V125 H707" stroke="#b89a59" stroke-width="2"/>
  </g>
  ${instances.map(i=>sprite({...sprites.find(s=>s.id===i.sprite),...(i.sourceRectOverride?{sourceRect:i.sourceRectOverride}:{})},i.rect,i.id)).join('\n')}
 </g>
 <g data-layer="light" fill="none" stroke-linecap="round">
  <path class="beam-path" d="M736 112 V215 H2075" stroke="#ffad31" opacity=".75" stroke-width="13" filter="url(#beam-soft)"/>
  <path class="beam-path" d="M736 112 V215 H2075" stroke="#ffd568" stroke-width="4"/>
  <path class="beam-path" d="M736 112 V215 H2075" stroke="#fff3bd" stroke-width="1.6"/>
 </g>
 <g data-layer="water" clip-path="url(#surface-window)">${img('Layers/20-water.png')}</g>
 <g data-layer="route" style="display:none" fill="none" stroke="#cfcbff" stroke-width="4" stroke-linecap="round" stroke-linejoin="round" marker-end="url(#arrow)">
  <path d="M785 400 Q835 442 881 478 L997 497 H1225"/>
  <path d="M1225 497 Q1253 404 1290 404"/>
  <path d="M1290 404 Q1257 368 1223 417"/>
  <path d="M1223 417 Q1168 320 1132 344"/>
  <path d="M1132 344 Q1173 258 1229 281"/>
  <path d="M1229 281 Q1274 223 1329 255 H1588"/>
 </g>`;}
(async()=>{
 const files=['References/continuous-master.png','Layers/00-background.png','Layers/10-platforms.png','Layers/20-water.png','Layers/90-camera-roots.png',atlas];
 const assets=[];for(const file of files){const b=fs.readFileSync(path.join(root,file)),m=await sharp(b).metadata();assets.push({file,width:m.width,height:m.height,hasAlpha:m.hasAlpha,bytes:b.length,sha256:crypto.createHash('sha256').update(b).digest('hex')});}
 const manifest={version:'level03-continuous-layers-v04',date:'2026-10-31',status:'Art reconstruction and layer preview; no Unity scene or collision validation',coordinates:{origin:'top-left',units:'art pixels; not final gameplay H',world:{width:W,height:H},nativeTexturePixelsPreserved:true},layers:[
  {id:'background',name:'后景',file:'Layers/00-background.png',space:'world',sortingOrder:0,sourceRect:{x:0,y:10,width:2170,height:702},destinationRect:{x:0,y:0,width:W,height:H},note:'UV window omits source export border; original PNG unchanged.'},
  {id:'water-rear',name:'中景：瀑布和后水面',file:'Layers/20-water.png',space:'world',sortingOrder:10,rect:{x:0,y:0,width:W,height:H}},
  {id:'platforms',name:'中景：静态跳台',file:'Layers/10-platforms.png',space:'world',sortingOrder:20,rect:{x:0,y:0,width:W,height:H}},
  {id:'props',name:'交互道具',file:atlas,space:'world',sortingOrder:30,sprites:'sprites',instances:'instances'},
  {id:'light',name:'光路',file:'light-path.svg',space:'world',sortingOrder:40,nativeGeometry:true,note:'Editable polylines; not baked into art textures.'},
  {id:'water-front',name:'中景：前水面',file:'Layers/20-water.png',space:'world',sortingOrder:50,clip:{x:0,y:542,width:2170,height:183},note:'Second instance of same texture, clipped below platforms to cover submerged edges.'},
  {id:'camera-roots',name:'前景：固定镜头根须',file:'Layers/90-camera-roots.png',space:'screen',sortingOrder:100,fixedToCamera:true,screenRect:{x:0,y:0,width:1,height:1},excludeFromWorldPanorama:true}
 ],sprites,instances,mechanism:{type:'single shared mirror and counterweight',high:{mirrorTop:170,counterweightTop:472,beamY:215},middle:{mirrorTop:250,counterweightTop:392,beamY:295},constantSumOfArtY:642,aperture:{centerX:1179,centerY:215},note:'Preview states illustrate opposite travel; no solver or colliders.'},assets};
 fs.writeFileSync(path.join(root,'layer-manifest.json'),JSON.stringify(manifest,null,2));
 fs.writeFileSync(path.join(root,'layer-manifest.js'),'window.DRAIN_MANIFEST='+JSON.stringify(manifest)+';');
 fs.writeFileSync(path.join(root,'Props/unity-sprite-rects.csv'),'id,name,x,y,width,height,pivotX,pivotY\n'+sprites.map(s=>[s.id,s.name,s.unityRect.x,s.unityRect.y,s.unityRect.width,s.unityRect.height,s.pivot[0],1-s.pivot[1]].join(',')).join('\n'));
 for(const s of sprites){const r=s.sourceRect;fs.writeFileSync(path.join(root,s.preview),`<svg xmlns="http://www.w3.org/2000/svg" width="${r.width}" height="${r.height}" viewBox="${r.x} ${r.y} ${r.width} ${r.height}">${img('props-atlas.png',0,0,1254,1254)}</svg>`);}
 fs.writeFileSync(path.join(root,'registered-world.svg'),`<svg xmlns="http://www.w3.org/2000/svg" width="${W}" height="${H}" viewBox="0 0 ${W} ${H}">${body()}</svg>`);
 fs.writeFileSync(path.join(root,'light-path.svg'),`<svg xmlns="http://www.w3.org/2000/svg" width="${W}" height="${H}" viewBox="0 0 ${W} ${H}">${defs}<path d="M736 112 V215 H2075" fill="none" stroke="#ffeaa2" stroke-width="4"/></svg>`);
 const tpl=fs.readFileSync(path.join(__dirname,'drain-preview-template.html'),'utf8');
 fs.writeFileSync(path.join(root,'index.html'),tpl.replace('<!--WORLD_OVERVIEW-->',`<svg id="overview" viewBox="0 0 ${W} ${H}" xmlns="http://www.w3.org/2000/svg">${body()}</svg>`).replace('<!--CAMERA_WORLD-->',`<svg id="world" width="${W}" height="${H}" viewBox="0 0 ${W} ${H}" xmlns="http://www.w3.org/2000/svg">${body().replaceAll('id="','id="camera-').replaceAll('url(#','url(#camera-')}</svg>`));
 console.log(JSON.stringify({folder:root,assets:assets.map(a=>({file:a.file,size:[a.width,a.height],alpha:a.hasAlpha})),sprites:sprites.length,world:[W,H]},null,2));
})();

'use strict';
(() => {
  const NS='http://www.w3.org/2000/svg';
  const manifestFiles=[
    {file:'l1-background.png',name:'园墙背景',purpose:'完整园墙、树冠与远景；无人物与地面条。'},
    {file:'l1-ground.png',name:'地面条',purpose:'横向透明地面，铺在第一关近景。'},
    {file:'spirit.png',name:'光精灵',purpose:'两关共用同一角色，锁定可见高度 1H。'},
    {file:'reflector.png',name:'园灯与反光板',purpose:'花圃墙式园灯、平面反光板和低位手柄。'},
    {file:'tail-rubble.png',name:'阶石与尾尖',purpose:'阶石掩埋一小片尾尖光鳞。'},
    {file:'pot.png',name:'倒扣陶盆',purpose:'宽口陶盆与盆沿缺口，通向第二关。'},
    {file:'l2-background.png',name:'盆内合成参照',purpose:'前次含地面的合成参照；新预览改用独立背景与地面。'},
    {file:'l2-background-clean.png',name:'盆内独立背景',purpose:'盆壁、植物、云母机关与光束；不含地面和中央低土台。'},
    {file:'l2-ground.png',name:'盆内独立地面',purpose:'透明地面与中央低土台，保留原幅坐标覆盖背景。'}
  ];
  const config={
    l1:{width:96,height:48,groundY:6},l2:{width:32,height:16,groundY:4.35},
    placements:{
      'l1-background':{file:'l1-background.png',x:-9.6,y:-9.4,width:115.2,height:57.6,fit:'contain',anchor:'center-box',layer:'background'},
      'l1-ground':{file:'l1-ground.png',x:0,y:6.4,width:96,height:6,fit:'width',anchor:'top-left',layer:'ground'},
      'l1-player':{file:'spirit.png',x:34,y:6,height:1,fit:'height',anchor:'bottom-center',layer:'character'},
      reflector:{file:'reflector.png',x:36,y:5.75,width:4,height:4.5,fit:'contain',anchor:'bottom-left',layer:'interactive'},
      'tail-rubble':{file:'tail-rubble.png',x:49,y:6,width:22,height:7,fit:'contain',anchor:'bottom-left',layer:'interactive'},
      pot:{file:'pot.png',x:70,y:6,width:25.2,height:12,fit:'contain',anchor:'bottom-left',layer:'interactive'},
      'l2-background':{file:'l2-background-clean.png',x:0,y:0,width:32,height:16,fit:'contain',anchor:'center-box',layer:'background'},
      'l2-ground':{file:'l2-ground.png',x:0,y:0,width:32,height:16,fit:'contain',anchor:'center-box',bounds:'canvas',layer:'ground'},
      'l2-player':{file:'spirit.png',x:5,y:4.35,height:1,fit:'height',anchor:'bottom-center',layer:'character'}
    },
    beamPoints:[{x:57.6,y:29},{x:39,y:8.1},{x:52,y:7.5}]
  };
  const state={level:'l1',zoom:1,focus:46,ruler:false,layers:{background:true,ground:true,character:true,interactive:true,beams:true}};
  const assets=new Map(manifestFiles.map(d=>[d.file,{...d,loaded:false,loading:false,width:0,height:0,metadata:null}]));
  const $=id=>document.getElementById(id),worldRoot=$('world-root'),svg=$('scene-svg');
  let manifestBusy=false,manifestSignature='',animation=null,frame=0;
  const svgEl=(name,attrs={},text)=>{const e=document.createElementNS(NS,name);for(const [k,v] of Object.entries(attrs))e.setAttribute(k,String(v));if(text!==undefined)e.textContent=text;return e;};
  const clamp=(n,a,b)=>Math.max(a,Math.min(b,n));
  const clone=o=>JSON.parse(JSON.stringify(o));
  function assetBounds(asset){
    const m=asset.metadata,W=asset.width||Number(m?.width)||1,H=asset.height||Number(m?.height)||1,b=m?.bbox;
    if(b&&[b.x,b.y,b.width,b.height].every(Number.isFinite)&&b.width>0&&b.height>0){const x=clamp(b.x,0,W-1),y=clamp(b.y,0,H-1);return {x,y,width:Math.min(b.width,W-x),height:Math.min(b.height,H-y),imageWidth:W,imageHeight:H,fromMetadata:true};}
    return {x:0,y:0,width:W,height:H,imageWidth:W,imageHeight:H,fromMetadata:false};
  }
  function placementRect(id){
    const p=config.placements[id],a=assets.get(p.file);let b=assetBounds(a);if(p.bounds==='canvas')b={...b,x:0,y:0,width:b.imageWidth,height:b.imageHeight,mode:'full-canvas'};const ratio=b.width/b.height;let width,height;
    if(p.fit==='height'){height=p.height;width=height*ratio;}
    else if(p.fit==='width'){width=p.width;height=width/ratio;}
    else {const factor=Math.min(p.width/b.width,p.height/b.height);width=b.width*factor;height=b.height*factor;}
    let x=p.x,y=p.y;if(p.anchor==='bottom-center')x-=width/2;else if(p.anchor==='top-left')y-=height;else if(p.anchor==='center-box'){x+=(p.width-width)/2;y+=(p.height-height)/2;}
    return {x,y,width,height,bbox:b,loaded:a.loaded};
  }
  function addAsset(id){
    const p=config.placements[id],a=assets.get(p.file);if(!a.loaded||!state.layers[p.layer])return;
    const r=placementRect(id),world=config[state.level],b=r.bbox;
    const group=svgEl('svg',{x:r.x,y:world.height-r.y-r.height,width:r.width,height:r.height,viewBox:[b.x,b.y,b.width,b.height].join(' '),preserveAspectRatio:'xMidYMid meet',overflow:'hidden','data-asset':id,'data-visible-height':r.height,'data-visible-width':r.width});
    group.append(svgEl('image',{href:a.currentUrl||a.file,x:0,y:0,width:b.imageWidth,height:b.imageHeight,preserveAspectRatio:'none'}));worldRoot.append(group);
  }
  function addBeams(){
    if(state.level!=='l1'||!state.layers.beams)return;
    const points=config.beamPoints.map(p=>p.x+','+(48-p.y)).join(' '),g=svgEl('g',{'data-layer':'beams','aria-label':'墙洞经园灯反射至尾鳞的示意光路'});
    g.append(svgEl('polyline',{points,fill:'none',stroke:'#ffcd71','stroke-width':.32,opacity:.18,'stroke-linejoin':'round'}));
    g.append(svgEl('polyline',{points,fill:'none',stroke:'#ffe6a0','stroke-width':.07,opacity:.88,'stroke-linejoin':'round'}));
    for(const p of config.beamPoints.slice(1))g.append(svgEl('circle',{cx:p.x,cy:48-p.y,r:.12,fill:'#fff3bf',opacity:.8}));worldRoot.append(g);
  }
  function addRuler(){
    if(!state.ruler)return;const p=config.placements[state.level+'-player'],H=config[state.level].height,x=p.x-1,y=H-p.y;
    const g=svgEl('g',{'data-layer':'ruler',stroke:'#435d80','stroke-width':.035,fill:'none'});
    g.append(svgEl('path',{d:`M ${x} ${y} v -1 M ${x-.12} ${y} h .24 M ${x-.12} ${y-1} h .24`}));
    const label=svgEl('text',{x:x-.22,y:y-.42,fill:'#334c6b',stroke:'#faf8ed','stroke-width':.08,'paint-order':'stroke','text-anchor':'end','font-size':.4,'font-family':'sans-serif'},'1H');g.append(label);
    const start=clamp(p.x-3,0,config[state.level].width-8),base=y+.38;g.append(svgEl('path',{d:`M ${start} ${base} h 8`}));for(let i=0;i<=8;i++)g.append(svgEl('path',{d:`M ${start+i} ${base-.08} v .16`}));
    g.append(svgEl('text',{x:start+4,y:base+.5,fill:'#334c6b',stroke:'#faf8ed','stroke-width':.07,'paint-order':'stroke','text-anchor':'middle','font-size':.32,'font-family':'sans-serif'},'每格 1H'));worldRoot.append(g);
  }
  function renderWorld(){
    worldRoot.replaceChildren();if(state.level==='l1'){['l1-background','reflector','tail-rubble','pot','l1-ground'].forEach(addAsset);addBeams();addAsset('l1-player');}else{addAsset('l2-background');addAsset('l2-ground');addAsset('l2-player');}addRuler();
    const relevant=state.level==='l1'?manifestFiles.slice(0,6).map(d=>d.file):['spirit.png','l2-background-clean.png','l2-ground.png'];$('waiting').hidden=relevant.some(file=>assets.get(file).loaded);applyCamera();updateStatus();
  }
  function cameraBox(){
    if(state.level==='l2')return {x:0,y:0,width:32,height:16};
    const width=96/state.zoom,height=48/state.zoom,cx=clamp(state.focus,width/2,96-width/2),cy=clamp(6+.28*height,height/2,48-height/2);
    return {x:cx-width/2,y:48-cy-height/2,width,height};
  }
  function applyCamera(){
    const box=cameraBox();svg.setAttribute('viewBox',[box.x,box.y,box.width,box.height].join(' '));
    $('zoom').value=state.zoom;$('zoom-output').value=state.zoom.toFixed(2)+'×';$('focus').value=state.focus;$('focus-output').value=state.focus.toFixed(1)+'H';
    const overview=state.zoom<1.015;$('view-metric').textContent=state.level==='l2'?'盆内视窗 32H × 16H':(overview?'全景 ':'当前视窗 ')+box.width.toFixed(overview?0:1)+'H × '+box.height.toFixed(overview?0:1)+'H';
    $('stage-label').textContent=state.level==='l2'?'第二关 · 与推近镜头同一尺度':overview?'第一关 · 全景':'第一关 · 地面镜头 '+state.zoom.toFixed(2)+'×';
    $('scale-status').textContent='角色 1H · 约占当前屏高 '+(100/box.height).toFixed(2)+'%';
    $('focus').disabled=state.level==='l2'||overview;
  }
  function stopAnimation(){if(frame)cancelAnimationFrame(frame);frame=0;animation=null;}
  function animateCamera(zoom,focus=state.focus,duration=2600){
    if(state.level!=='l1')return;stopAnimation();zoom=clamp(Number(zoom),1,3);focus=clamp(Number(focus),16,80);
    if(matchMedia('(prefers-reduced-motion: reduce)').matches||duration<=0){state.zoom=zoom;state.focus=focus;applyCamera();return;}
    const startZoom=state.zoom,startFocus=state.focus,start=performance.now();animation={zoom,focus,duration};
    const tick=now=>{const t=Math.min(1,(now-start)/duration),e=t<.5?2*t*t:1-Math.pow(-2*t+2,2)/2;state.zoom=startZoom+(zoom-startZoom)*e;state.focus=startFocus+(focus-startFocus)*e;applyCamera();if(t<1)frame=requestAnimationFrame(tick);else{frame=0;animation=null;}};frame=requestAnimationFrame(tick);
  }
  function selectLevel(level){
    if(!['l1','l2'].includes(level))return;stopAnimation();state.level=level;document.querySelectorAll('[data-level]').forEach(b=>b.setAttribute('aria-pressed',String(b.dataset.level===level)));const second=level==='l2';
    for(const id of ['overview','push-in','zoom'])$(id).disabled=second;document.querySelectorAll('[data-focus]').forEach(b=>b.disabled=second);
    $('camera-note').textContent=second?'第二关固定为 32H × 16H，与第一关三倍镜头保持同一角色尺度。':'推近后可沿地面巡视；世界与角色尺寸保持不变。';
    for(const key of ['ground','interactive','beams']){const checkbox=document.querySelector('.layer-toggle[data-layer='+key+']'),embedded=second&&key!=='ground';checkbox.disabled=embedded;checkbox.closest('label').classList.toggle('is-disabled',embedded);checkbox.closest('label').title=embedded?'盆内此内容已绘入背景，无法独立隐藏。':'';}
    const p=config.placements[level+'-player'];$('player-position').min=second?'2':'14';$('player-position').max=second?'30':'92';$('player-position').value=p.x;$('player-output').value=p.x.toFixed(1)+'H';
    $('scene-title').textContent=second?'第二关陶盆内部的同尺度预览':'第一关园门苔地的图层组合预览';
    $('scene-note').textContent=second?'第二关背景、地面和人物可独立开关。云母机关与光束仍绘在背景中；露珠为装饰。地面按完整画布原坐标叠加，人物滑条用于尺度摆位，未运行碰撞或物理反射。':'这是美术图层、镜头与尺度预览。人物滑条只改变摆位；当前没有运行游戏碰撞、行走或物理反射。';renderWorld();
  }
  function updateStatus(){
    const list=[...assets.values()],ready=list.filter(a=>a.loaded).length,boxes=list.filter(a=>a.metadata?.bbox).length;
    $('load-status').textContent=ready+'/'+list.length+' 张本地图片已载入'+(ready<list.length?' · 未就绪图片自动重试':'');
    $('metadata-status').textContent=boxes?'资产尺度数据：'+boxes+'/'+list.length+' · assets-data.js':'包围盒数据未就绪 · 暂按整图适配';
    for(const a of list){const card=document.querySelector('[data-resource="'+a.file+'"]');if(!card)continue;const thumb=card.querySelector('.asset-thumb');if(a.loaded&&!thumb.querySelector('img')){thumb.replaceChildren();const img=new Image();img.src=a.currentUrl||a.file;img.alt=a.name;thumb.append(img);}card.querySelector('small').textContent=a.loaded?a.width+' × '+a.height+' · 原图 ↗':a.file+' · 等待图片';}
  }
  function makeResourceCards(){const grid=$('resource-grid');for(const a of assets.values()){const link=document.createElement('a');link.className='asset-card';link.href=a.file;link.target='_blank';link.rel='noopener';link.dataset.resource=a.file;const thumb=document.createElement('div');thumb.className='asset-thumb';const pending=document.createElement('span');pending.textContent='等待图片';thumb.append(pending);const copy=document.createElement('div');copy.className='asset-copy';const title=document.createElement('strong');title.textContent=a.name;const p=document.createElement('p');p.textContent=a.purpose;const small=document.createElement('small');small.textContent=a.file;copy.append(title,p,small);link.append(thumb,copy);grid.append(link);}}
  function loadAsset(a,force=false){
    if(a.loading||(!force&&a.loaded))return;a.loading=true;const img=new Image(),url=a.file+(force||a.attempted?'?v='+Date.now():'');a.attempted=true;
    img.onload=()=>{a.loaded=true;a.loading=false;a.width=img.naturalWidth;a.height=img.naturalHeight;a.currentUrl=url;const thumb=document.querySelector('[data-resource="'+a.file+'"] .asset-thumb img');if(thumb)thumb.src=url;renderWorld();};img.onerror=()=>{a.loading=false;if(!a.loaded)updateStatus();};img.src=url;
  }
  function applyManifest(data){
    if(!data||!Array.isArray(data.files))return false;const signature=JSON.stringify(data.files);if(signature===manifestSignature)return true;manifestSignature=signature;
    for(const entry of data.files){const a=assets.get(entry.file);if(a)a.metadata=clone(entry);}renderWorld();return true;
  }
  function loadManifest(){
    if(manifestBusy)return;manifestBusy=true;const script=document.createElement('script');script.src='assets-data.js?v='+Date.now();script.onload=()=>{applyManifest(window.ART_ASSETS);manifestBusy=false;script.remove();};script.onerror=()=>{manifestBusy=false;script.remove();};document.head.append(script);
  }
  function reloadAssets(force=false){loadManifest();assets.forEach(a=>loadAsset(a,force));}
  document.querySelectorAll('[data-level]').forEach(b=>b.addEventListener('click',()=>selectLevel(b.dataset.level)));
  $('overview').addEventListener('click',()=>animateCamera(1,state.focus,2200));$('push-in').addEventListener('click',()=>animateCamera(3,state.focus,2600));
  $('zoom').addEventListener('input',e=>{stopAnimation();state.zoom=Number(e.target.value);applyCamera();});$('focus').addEventListener('input',e=>{stopAnimation();state.focus=Number(e.target.value);applyCamera();});
  document.querySelectorAll('[data-focus]').forEach(b=>b.addEventListener('click',()=>{const target={player:config.placements['l1-player'].x,reflector:config.beamPoints[1].x,tail:config.beamPoints[2].x,pot:80}[b.dataset.focus];animateCamera(3,target,1300);}));
  $('player-position').addEventListener('input',e=>{const x=Number(e.target.value);config.placements[state.level+'-player'].x=x;$('player-output').value=x.toFixed(1)+'H';renderWorld();});
  document.querySelectorAll('.layer-toggle').forEach(c=>c.addEventListener('change',()=>{state.layers[c.dataset.layer]=c.checked;renderWorld();}));$('ruler-toggle').addEventListener('change',e=>{state.ruler=e.target.checked;renderWorld();});$('reload-assets').addEventListener('click',()=>reloadAssets(true));
  window.microPreview={
    getState:()=>clone({...state,camera:cameraBox(),animating:!!animation,config}),
    getMeasurements:()=>Object.fromEntries(Object.keys(config.placements).map(id=>[id,placementRect(id)])),
    setPlacement:(id,patch)=>{if(!config.placements[id])throw new Error('Unknown placement: '+id);for(const k of ['x','y','width','height'])if(k in patch&&!Number.isFinite(patch[k]))throw new Error('Placement '+k+' must be finite');Object.assign(config.placements[id],patch);selectLevel(state.level);return placementRect(id);},
    setBeamPoints:points=>{if(!Array.isArray(points)||points.length!==3||!points.every(p=>Number.isFinite(p.x)&&Number.isFinite(p.y)))throw new Error('Provide three {x,y} world points');config.beamPoints=clone(points);renderWorld();},
    setLevel:selectLevel,
    setCamera:({zoom=state.zoom,focus=state.focus,duration=0}={})=>animateCamera(zoom,focus,duration),
    setManifest:applyManifest,
    reloadAssets:()=>reloadAssets(true)
  };
  makeResourceCards();if(window.ART_ASSETS)applyManifest(window.ART_ASSETS);selectLevel('l1');reloadAssets();
  setInterval(()=>{if(!document.hidden)reloadAssets();},5000);document.addEventListener('visibilitychange',()=>{if(!document.hidden)reloadAssets();});
})();

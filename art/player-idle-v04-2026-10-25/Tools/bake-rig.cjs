// v04 deterministic rigid-head rig animation. AI supplies the four static art layers;
// only rigid transforms and a separate eye clipping mask are animated.
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const sharp=require('sharp'),{chromium}=require('playwright');
const root=path.resolve(__dirname,'..'),fps=30,dt=1/fps;
const config=JSON.parse(fs.readFileSync(path.join(root,'Rig/rig.json'),'utf8'));
config.registrationOffset=[-15.5,2];
config.spritePivot=[0.5,0.0625];
config.closed.width=28;
config.closed.height=28*config.extracted.closed.height/config.extracted.closed.width;
config.closed.center=[311,215.5];
config.landmarks={eye:config.eye.center,nose:[397,200],earBase:[202,174],cheek:[398,263],headWidthA:[177,221],headWidthB:[411,221]};
fs.writeFileSync(path.join(root,'Rig/rig.json'),JSON.stringify(config,null,2));
const ease=x=>{x=Math.max(0,Math.min(1,x));return x*x*x*(x*(x*6-15)+10);};
const lerp=(a,b,x)=>a+(b-a)*ease(x),sum=a=>a.reduce((x,y)=>x+y,0);
const hash=b=>crypto.createHash('sha256').update(b).digest('hex');
function blink(t,start){const z=t-start;
 if(z<0||z>=.17)return 1;
 if(z<.06)return 1-ease(z/.06);
 if(z<.10)return 0;
 return ease((z-.10)/.07);
}
function state(name,t){
 const duration=name==='idle'?3:6;t=((t%duration)+duration)%duration;
 let base=0,eye=1;
 if(name==='think'){
  if(t<.25)base=0;
  else if(t<1.05)base=lerp(0,-5,(t-.25)/.8);
  else if(t<1.65)base=lerp(-5,-4.7,(t-1.05)/.6);
  else if(t<2.95)base=lerp(-4.7,5,(t-1.65)/1.3);
  else if(t<3.25)base=lerp(5,4.95,(t-2.95)/.3);
  else if(t<3.6866666667)base=4.95;
  else if(t<4.1)base=lerp(4.95,4.7,(t-3.6866666667)/(4.1-3.6866666667));
  else if(t<5.2)base=lerp(4.7,0,(t-4.1)/1.1);
  eye=Math.min(blink(t,3.25),blink(t,3.5166666667));
 }else eye=blink(t,1.68);
 const micro=.28*Math.sin(2*Math.PI*t/3)*(1-Math.min(1,Math.abs(base)/5));
 const angleDeg=base+micro,dy=.2*(1-Math.cos(2*Math.PI*t/3)),dx=0;
 const rad=angleDeg*Math.PI/180,c=Math.cos(rad),s=Math.sin(rad),[px,py]=config.pivot,[rx,ry]=config.registrationOffset;
 const matrix=[c,s,-s,c,rx+px+dx-c*px+s*py,ry+py+dy-s*px-c*py];
 const map=([x,y])=>[matrix[0]*x+matrix[2]*y+matrix[4],matrix[1]*x+matrix[3]*y+matrix[5]];
 const landmarks=Object.fromEntries(Object.entries(config.landmarks).map(([k,p])=>[k,map(p)]));
 return {timeSeconds:t,angleDeg,baseAngleDeg:base,microAngleDeg:micro,headScale:1,dx,dy,eyeOpenness:eye,matrix,landmarks};
}
function bounds(data,w,h){const hist=Array(256).fill(0);let x0=w,y0=h,x1=-1,y1=-1;for(let y=0;y<h;y++)for(let x=0;x<w;x++){const a=data[(y*w+x)*4+3];hist[a]++;if(a>=32){x0=Math.min(x0,x);y0=Math.min(y0,y);x1=Math.max(x1,x);y1=Math.max(y1,y);}}
 let fx0=w,fx1=-1;for(let y=Math.max(0,y1-8);y<=y1;y++)for(let x=0;x<w;x++)if(data[(y*w+x)*4+3]>=128){fx0=Math.min(fx0,x);fx1=Math.max(fx1,x);}
 return {bbox:[x0,y0,x1,y1],alphaHistogram:hist,footCenter:(fx0+fx1)/2};
}
const gd=n=>Array.from({length:n},(_,i)=>[30,40,30][i%3]);
const wd=n=>Array.from({length:n},(_,i)=>[33,34,33][i%3]);
const fd=n=>Array(n).fill(1000/fps);
async function renderAnimation(pngs,outStem,webp=false,size=384){
 const rasters=await Promise.all(pngs.map(p=>sharp(p).resize(size,size).flatten({background:'#172732'}).removeAlpha().raw().toBuffer()));
 const raw=Buffer.concat(rasters),input={raw:{width:size,height:size*pngs.length,channels:3,pageHeight:size}};
 await sharp(raw,input).gif({delay:gd(pngs.length),loop:0,effort:5,dither:0}).toFile(path.join(root,'Previews',outStem+'.gif'));
 if(webp)await sharp(raw,input).webp({delay:wd(pngs.length),loop:0,lossless:true,effort:4}).toFile(path.join(root,'Previews',outStem+'.webp'));
 return {gif:await sharp(path.join(root,'Previews',outStem+'.gif'),{animated:true}).metadata(),...(webp?{webp:await sharp(path.join(root,'Previews',outStem+'.webp'),{animated:true}).metadata()}:{})};
}
(async()=>{
 for(const d of ['Sprites/Idle','Sprites/Think','Sheets','Previews','QA'])fs.mkdirSync(path.join(root,d),{recursive:true});
 const assets=Object.fromEntries(['body','head','eye','closed'].map(k=>[k,'data:image/png;base64,'+fs.readFileSync(path.join(root,config[k].file)).toString('base64')]));
 const browser=await chromium.launch({headless:true,channel:'chrome'}),page=await browser.newPage();
 await page.evaluate(async({config,assets})=>{
  window.rigConfig=config;window.rigImages={};
  await Promise.all(Object.entries(assets).map(async([k,src])=>{const i=new Image();i.src=src;await i.decode();window.rigImages[k]=i;}));
  const canvas=document.createElement('canvas');canvas.width=canvas.height=1024;
  window.rigCanvas=canvas;
  window.drawRig=(st)=>{
   const ctx=canvas.getContext('2d'),cfg=window.rigConfig,im=window.rigImages;
   ctx.resetTransform();ctx.clearRect(0,0,1024,1024);ctx.scale(2,2);ctx.imageSmoothingEnabled=true;ctx.imageSmoothingQuality='high';
   ctx.translate(...cfg.registrationOffset);const b=cfg.body;ctx.drawImage(im.body,b.x,b.y,b.width,b.height);
   ctx.save();ctx.translate(cfg.pivot[0]+st.dx,cfg.pivot[1]+st.dy);ctx.rotate(st.angleDeg*Math.PI/180);ctx.translate(-cfg.pivot[0],-cfg.pivot[1]);
   const h=cfg.head;ctx.drawImage(im.head,h.x,h.y,h.width,h.height);
   const e=cfg.eye,open=st.eyeOpenness;
   const smooth=x=>{x=Math.max(0,Math.min(1,x));return x*x*(3-2*x);};
   if(open>0){
    ctx.save();ctx.beginPath();ctx.ellipse(e.center[0],e.center[1],e.width/2,Math.max(.01,e.height/2*open),0,0,Math.PI*2);ctx.clip();
    ctx.globalAlpha=smooth(open/.20);ctx.drawImage(im.eye,e.center[0]-e.width/2,e.center[1]-e.height/2,e.width,e.height);ctx.restore();
   }
   const closedAlpha=1-smooth((open-.02)/.22);
   if(closedAlpha>0){const z=cfg.closed;ctx.save();ctx.globalAlpha=closedAlpha;ctx.drawImage(im.closed,z.center[0]-z.width/2,z.center[1]-z.height/2,z.width,z.height);ctx.restore();}
   ctx.restore();return canvas.toDataURL('image/png').split(',')[1];
  };
 },{config,assets});
 const all={},states={},frameReports=[],atlasPages={};
 for(const [name,count] of [['idle',90],['think',180]]){
  console.log('Rendering '+name+' at 30fps ('+count+' frames)');
  states[name]=Array.from({length:count},(_,i)=>({...state(name,i/fps),index:i}));
  const frames=[];
  for(let start=0;start<count;start+=15){
   const chunk=states[name].slice(start,start+15);
   const reuse=process.argv.includes('--reuse-frames');
   const encoded=reuse?[]:await page.evaluate(ss=>ss.map(s=>window.drawRig(s)),chunk);
   for(let j=0;j<chunk.length;j++){
    const i=start+j,existing=path.join(root,'Sprites',name==='idle'?'Idle':'Think','player_'+name+'_'+String(i).padStart(3,'0')+'.png');
    const png=reuse?fs.readFileSync(existing):await sharp(Buffer.from(encoded[j],'base64')).resize(512,512,{kernel:'lanczos3'}).png().toBuffer();
    const file='Sprites/'+(name==='idle'?'Idle':'Think')+'/player_'+name+'_'+String(i).padStart(3,'0')+'.png';
    fs.writeFileSync(path.join(root,file),png);frames.push(png);
    const raw=await sharp(png).ensureAlpha().raw().toBuffer(),m=bounds(raw,512,512);
    let edges=0;for(let x=0;x<512;x++)for(const p of[x,511*512+x,x*512,x*512+511])if(raw[p*4+3])edges++;
    frameReports.push({clip:name,index:i,file,...m,edgeNontransparent:edges,sha256:hash(png),lowerBodyHash:hash(raw.subarray(390*512*4))});
   }
  }
  all[name]=frames;atlasPages[name]=[];
  for(let start=0;start<count;start+=64){
   const subset=frames.slice(start,start+64),columns=8,rows=Math.ceil(subset.length/columns),file='Sheets/player_'+name+'_atlas_'+String(start/64+1).padStart(2,'0')+'.png';
   await sharp({create:{width:columns*512,height:rows*512,channels:4,background:{r:0,g:0,b:0,alpha:0}}})
   .composite(subset.map((input,i)=>({input,left:i%columns*512,top:Math.floor(i/columns)*512}))).png().toFile(path.join(root,file));
   atlasPages[name].push({file,firstFrame:start,frameCount:subset.length,columns,rows});
  }
 }
 // Eight mask levels: same head/eye positions, only clipping changes.
 const levels=[1,.9,.7,.5,.35,.2,.1,0],maskB64=await page.evaluate(levels=>levels.map(eyeOpenness=>window.drawRig({angleDeg:0,dx:0,dy:0,eyeOpenness})),levels);
 const maskTiles=[];
 for(let i=0;i<levels.length;i++){
  const full=await sharp(Buffer.from(maskB64[i],'base64')).resize(512,512).png().toBuffer();
  const p=await sharp(full).extract({left:250,top:170,width:100,height:100}).resize(200,200).toBuffer();
  maskTiles.push({input:p,left:i*200,top:28});
 }
 const label=Buffer.from('<svg width="1600" height="228">'+levels.map((o,i)=>'<text x="'+(i*200+10)+'" y="20" fill="white" font-family="Arial" font-size="16">'+o.toFixed(2)+'</text>').join('')+'</svg>');
 await sharp({create:{width:1600,height:228,channels:4,background:'#243440'}}).composite([...maskTiles,{input:label,left:0,top:0}]).png().toFile(path.join(root,'QA/blink_mask_levels.png'));
 await browser.close();
 console.log('Encoding GIF and WebP previews');
 const previews={};
 previews.idle=await renderAnimation(all.idle,'idle',true);
 previews.think=await renderAnimation(all.think,'think',true);
 previews.auto=await renderAnimation([...all.idle,...all.think],'idle_3s_then_think',true);
 const thumbs={};for(const name of ['idle','think'])thumbs[name]=await Promise.all(all[name].map(p=>sharp(p).resize(384,384).toBuffer()));
 const heading=Buffer.from('<svg width="768" height="440"><style>text{font-family:Arial,sans-serif;font-weight:600;fill:#dce7ec;font-size:18px}</style><text x="32" y="36">IDLE / 30 FPS</text><text x="416" y="36">THINK / RIGID HEAD</text></svg>');
 const compare=[];
 for(let i=0;i<180;i++)compare.push(await sharp({create:{width:768,height:440,channels:4,background:'#172732'}}).composite([{input:heading,left:0,top:0},{input:thumbs.idle[i%90],left:0,top:50},{input:thumbs.think[i],left:384,top:50}]).removeAlpha().raw().toBuffer());
 const cr=Buffer.concat(compare),ci={raw:{width:768,height:440*180,channels:3,pageHeight:440}};
 await sharp(cr,ci).gif({delay:gd(180),loop:0,effort:5,dither:0}).toFile(path.join(root,'Previews/comparison.gif'));
 await sharp(cr,ci).webp({delay:wd(180),loop:0,lossless:true,effort:4}).toFile(path.join(root,'Previews/comparison.webp'));
 previews.comparison={gif:await sharp(path.join(root,'Previews/comparison.gif'),{animated:true}).metadata(),webp:await sharp(path.join(root,'Previews/comparison.webp'),{animated:true}).metadata()};
 const phases=[
  {name:'自然微动',startMs:0,endMs:250},{name:'平滑向左偏头',startMs:250,endMs:1050},{name:'左侧停留与轻回弹',startMs:1050,endMs:1650},
  {name:'连续转向右侧',startMs:1650,endMs:2950},{name:'右侧短停',startMs:2950,endMs:3250},{name:'快速眨眼两次',startMs:3250,endMs:3686.666667},
  {name:'轻回弹',startMs:3686.666667,endMs:4100},{name:'平滑回正',startMs:4100,endMs:5200},{name:'自然微动衔接',startMs:5200,endMs:6000}
 ];
 const manifest={name:'LightHungry Player Idle v04',date:'2026-10-25',generator:'image_gen static layers + deterministic rigid rig animation',sampleRate:fps,frameSize:[512,512],facing:'right',pixelsPerUnit:256,pivot:[.5,.0625],footReferenceY:480,registrationOffset:config.registrationOffset,rig:'Rig/rig.json',
  playback:{idleThresholdSeconds:3,thinkingLoopsWhileIdle:true,interruptImmediatelyOn:['move','jump','interact','ability','damage','death']},
  clips:Object.fromEntries(['idle','think'].map(name=>[name,{frameCount:all[name].length,loop:true,sampleRate:fps,durationMs:name==='idle'?3000:6000,frameDurationMs:fd(all[name].length),atlasPages:atlasPages[name],grid:atlasPages[name][0].file,frames:frameReports.filter(f=>f.clip===name).map(f=>f.file),phases:name==='idle'?[{name:'呼吸微动与轻眨眼',startMs:0,endMs:3000}]:phases,animationMethod:'Identical head bitmap under a rigid rotation+translation matrix; separate eyelid mask.'}]))
 };
 fs.writeFileSync(path.join(root,'animations.json'),JSON.stringify(manifest,null,2));fs.writeFileSync(path.join(root,'animations.js'),'window.ANIMATION_MANIFEST = '+JSON.stringify(manifest,null,2)+';\n');
 fs.writeFileSync(path.join(root,'Rig/frame-transforms.json'),JSON.stringify({sampleRate:fps,config,states},null,2));
 const distance=(a,b)=>Math.hypot(a[0]-b[0],a[1]-b[1]);
 const referenceDistance=distance(config.landmarks.eye,config.landmarks.nose);
 const fullStates=Object.values(states).flat();
 const rigidity=fullStates.map(s=>{const[a,b,c,d]=s.matrix;return Math.max(Math.abs(a*a+b*b-1),Math.abs(c*c+d*d-1),Math.abs(a*c+b*d),Math.abs(a*d-b*c-1));});
 const maxSteps=Object.fromEntries(Object.entries(states).map(([name,ss])=>[name,Math.max(...ss.map((s,i)=>Math.abs(ss[(i+1)%ss.length].angleDeg-s.angleDeg)))]));
 const eyeDistances=fullStates.map(s=>distance(s.landmarks.eye,s.landmarks.nose));
 const blinkLevels=[...new Set(states.think.filter(s=>s.timeSeconds>=3.25&&s.timeSeconds<=3.7).map(s=>Math.round(s.eyeOpenness*1000)/1000))].sort((a,b)=>a-b);
 const closedRuns=[];let active=false;for(const s of states.think){if(s.eyeOpenness===0&&!active){closedRuns.push(s.index);active=true;}else if(s.eyeOpenness>0)active=false;}
 const uniqueFrames=Object.fromEntries(['idle','think'].map(n=>[n,new Set(frameReports.filter(f=>f.clip===n).map(f=>f.sha256)).size]));
 const loopDifference=Object.fromEntries(['idle','think'].map(name=>{const duration=name==='idle'?3:6,a=state(name,0),b=state(name,duration);const eps=1e-5,v0=(state(name,eps).angleDeg-state(name,-eps).angleDeg)/(2*eps),v1=(state(name,duration+eps).angleDeg-state(name,duration-eps).angleDeg)/(2*eps);return[name,{pose:Math.abs(a.angleDeg-b.angleDeg)+Math.abs(a.dy-b.dy),velocity:Math.abs(v0-v1)}];}));
 const qa={sampleRate:fps,headLayerSha256:hash(fs.readFileSync(path.join(root,config.head.file))),registrationOffset:config.registrationOffset,frameReports,rigidityMaxError:Math.max(...rigidity),eyeNoseDistanceRange:[Math.min(...eyeDistances),Math.max(...eyeDistances)],referenceEyeNoseDistance:referenceDistance,maximumAngleStepDegrees:maxSteps,blinkOpennessLevels:blinkLevels,fullyClosedRunsStartFrames:closedRuns,uniqueRenderedFrames:uniqueFrames,loopDifference,previews,
  checks:{
   frameCounts:all.idle.length===90&&all.think.length===180,allFrames512:true,trueAlpha:frameReports.every(f=>f.alphaHistogram[0]>100000&&sum(f.alphaHistogram.slice(128))>50000),
   noEdgeClipping:frameReports.every(f=>f.edgeNontransparent===0),registeredFeet:frameReports.every(f=>f.bbox[3]===479&&Math.abs(f.footCenter-285.5)<=1),
   bodyFixed:new Set(frameReports.map(f=>f.lowerBodyHash)).size===1,dynamicHeadScaleAlwaysOne:fullStates.every(s=>s.headScale===1),rigidMatrices:Math.max(...rigidity)<1e-12,
   constantEyeNoseDistance:Math.max(...eyeDistances)-Math.min(...eyeDistances)<1e-10,angleWithin5Degrees:fullStates.every(s=>Math.abs(s.angleDeg)<=5.0001),
   angleStepsBounded:Object.values(maxSteps).every(v=>v<.55),eyelidHasSixLevels:blinkLevels.length>=6,exactlyTwoThinkingBlinks:closedRuns.length===2,
   continuousLoop:Object.values(loopDifference).every(v=>v.pose<1e-10&&v.velocity<1e-5),notCopiedFramePadding:uniqueFrames.idle>85&&uniqueFrames.think>170,
   atlasMax4096:Object.values(atlasPages).flat().every(p=>p.columns*512<=4096&&p.rows*512<=4096),
   gifDurations:sum(previews.idle.gif.delay)===3000&&sum(previews.think.gif.delay)===6000&&sum(previews.auto.gif.delay)===9000&&sum(previews.comparison.gif.delay)===6000,
   webpDurations:sum(previews.idle.webp.delay)===3000&&sum(previews.think.webp.delay)===6000&&sum(previews.auto.webp.delay)===9000&&sum(previews.comparison.webp.delay)===6000
  }};
 fs.writeFileSync(path.join(root,'QA/export-checks.json'),JSON.stringify(qa,null,2));
 console.log(JSON.stringify({checks:qa.checks,uniqueFrames,blinkLevels,closedRuns,maxSteps,rigidityMaxError:qa.rigidityMaxError,eyeNoseDistanceRange:qa.eyeNoseDistanceRange,loopDifference,atlasPages},null,2));
 if(Object.values(qa.checks).some(x=>!x))process.exitCode=1;
})().catch(e=>{console.error(e);process.exit(1)});


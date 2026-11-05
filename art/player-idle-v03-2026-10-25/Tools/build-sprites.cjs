// Packaging only: generated drawings are cropped, uniformly scaled per source sheet,
// registered to the same foot anchor and packed. No local deformation or repainting.
const fs=require('node:fs'),path=require('node:path'),sharp=require('sharp');
const root=path.resolve(__dirname,'..');
const metrics=JSON.parse(fs.readFileSync(path.join(root,'QA/source-metrics.json'),'utf8'));
// A common 1.08 visual-size correction keeps the new neutral body near the old 380px height.
const SIZE=512,FOOT_Y=479,FOOT_X=285.5,TARGET_FOOT_WIDTH=214;
const transparent={r:0,g:0,b:0,alpha:0};
const clips={
 idle:{cols:4,rows:4,delay:[400,250,200,200,200,200,150,120,60,60,60,120,200,200,250,330]},
 think:{cols:8,rows:4,delay:[200,180,180,180,180,180,180,180,400,400,180,180,180,180,180,180,180,180,180,300,60,60,60,80,60,60,60,120,180,180,180,720]}
};
const sum=a=>a.reduce((x,y)=>x+y,0);
function measure(data,w,h) {
 const histogram=Array(256).fill(0);let x0=w,y0=h,x1=-1,y1=-1;
 for(let y=0;y<h;y++)for(let x=0;x<w;x++){const a=data[(y*w+x)*4+3];histogram[a]++;if(a>=32){x0=Math.min(x0,x);y0=Math.min(y0,y);x1=Math.max(x1,x);y1=Math.max(y1,y);}}
 let fx0=w,fx1=-1;for(let y=Math.max(0,y1-8);y<=y1;y++)for(let x=0;x<w;x++)if(data[(y*w+x)*4+3]>=128){fx0=Math.min(fx0,x);fx1=Math.max(fx1,x);}
 return {bbox:[x0,y0,x1,y1],footCenter:(fx0+fx1)/2,footWidth:fx1-fx0+1,alphaHistogram:histogram};
}
function median(a){const s=a.slice().sort((x,y)=>x-y);return(s[(s.length-1)>>1]+s[s.length>>1])/2;}
async function exportGif(pngs,delay,dest,size=384){
 const rasters=await Promise.all(pngs.map(p=>sharp(p).resize(size,size).flatten({background:'#172732'}).raw().toBuffer()));
 await sharp(Buffer.concat(rasters),{raw:{width:size,height:size*rasters.length,channels:3,pageHeight:size}})
 .gif({delay,loop:0,effort:5,dither:0}).toFile(dest);
 return sharp(dest,{animated:true}).metadata();
}
const frameFile=(name,i)=>'Sprites/'+(name==='idle'?'Idle':'Think')+'/player_'+name+'_'+String(i).padStart(2,'0')+'.png';
(async()=>{
 for(const d of ['Sprites/Idle','Sprites/Think','Sheets','Previews','QA'])fs.mkdirSync(path.join(root,d),{recursive:true});
 const sourceFrames={},sourceScale={},qa={method:'Complete connected sprites are extracted beyond nominal cell boundaries. One isotropic scale per generated sheet based on median planted-foot width with common visual-size correction; fixed foot registration; no per-frame shape scaling.',frames:[],gifs:{},sourceScale:{}};
 for(const name of ['idle','thinkA','thinkB']){
  const src=metrics[name],scale=TARGET_FOOT_WIDTH/median(src.frames.map(f=>f.footWidth));sourceScale[name]=scale;
  const pngs=[];
  for(const m of src.frames){
   const x=Math.max(0,m.bbox[0]-4),y=Math.max(0,m.bbox[1]-4),r=Math.min(m.w,m.bbox[2]+5),b=Math.min(m.h,m.bbox[3]+5);
   const raster=await sharp(path.join(root,src.source)).extract({left:m.x+x,top:m.y+y,width:r-x,height:b-y})
    .resize(Math.round((r-x)*scale),Math.round((b-y)*scale),{kernel:'lanczos3'}).ensureAlpha().raw().toBuffer({resolveWithObject:true});
   const mm=measure(raster.data,raster.info.width,raster.info.height);
   const left=Math.round(FOOT_X-mm.footCenter),top=FOOT_Y-mm.bbox[3];
   if(left<0||top<0||left+raster.info.width>SIZE||top+raster.info.height>SIZE)throw new Error('Sprite outside 512 canvas: '+name+' '+m.i);
   pngs.push(await sharp({create:{width:SIZE,height:SIZE,channels:4,background:transparent}})
    .composite([{input:raster.data,raw:{width:raster.info.width,height:raster.info.height,channels:4},left,top}]).png().toBuffer());
  }
  sourceFrames[name]=pngs;
 }
 qa.sourceScale=sourceScale;
 const all={idle:sourceFrames.idle.slice(),think:[...sourceFrames.thinkA,...sourceFrames.thinkB]};
 const origins={idle:Array.from({length:16},(_,i)=>({sheet:'idle',cell:i})),think:Array.from({length:32},(_,i)=>({sheet:i<16?'thinkA':'thinkB',cell:i%16}))};
 all.idle[15]=all.idle[0];origins.idle[15]={reuse:'idle:00'};
 for(const i of [0,31]){all.think[i]=all.idle[0];origins.think[i]={reuse:'idle:00'};}
 for(const i of [8,9]){all.think[i]=all.think[7];origins.think[i]={reuse:'think:07',reason:'Stable left-tilt hold.'};}
 if(process.argv.includes('--reuse-blink')){
  for(const i of [22,24,26]){all.think[i]=all.think[20];origins.think[i]={reuse:'think:20',reason:'Stable half-closed blink pose.'};}
  all.think[25]=all.think[21];origins.think[25]={reuse:'think:21',reason:'Second fully closed blink.'};
  for(const i of [23,27]){all.think[i]=all.think[19];origins.think[i]={reuse:'think:19',reason:'Open eye between and after blinks.'};}
 }
 const horizontalPages={};
 for(const [name,c] of Object.entries(clips)){
  const frames=all[name];
  for(let i=0;i<frames.length;i++){
   fs.writeFileSync(path.join(root,frameFile(name,i)),frames[i]);
   const raw=await sharp(frames[i]).ensureAlpha().raw().toBuffer(),m=measure(raw,SIZE,SIZE);let edge=0;
   for(let z=0;z<SIZE;z++)for(const p of[z,(SIZE-1)*SIZE+z,z*SIZE,z*SIZE+SIZE-1])if(raw[p*4+3]>0)edge++;
   qa.frames.push({clip:name,index:i,size:[SIZE,SIZE],...m,edgeNontransparent:edge,origin:origins[name][i]});
  }
  await sharp({create:{width:c.cols*SIZE,height:c.rows*SIZE,channels:4,background:transparent}})
   .composite(frames.map((input,i)=>({input,left:(i%c.cols)*SIZE,top:Math.floor(i/c.cols)*SIZE})))
   .png().toFile(path.join(root,'Sheets/player_'+name+'_grid.png'));
  horizontalPages[name]=[];
  for(let start=0;start<frames.length;start+=8){
   const page=frames.slice(start,start+8),file='Sheets/player_'+name+'_strip_'+String(start/8+1).padStart(2,'0')+'.png';
   await sharp({create:{width:page.length*SIZE,height:SIZE,channels:4,background:transparent}})
    .composite(page.map((input,i)=>({input,left:i*SIZE,top:0}))).png().toFile(path.join(root,file));
   horizontalPages[name].push({file,firstFrame:start,frameCount:page.length,dimensions:[page.length*SIZE,SIZE]});
  }
  console.log('Exporting '+name+' GIF');
  qa.gifs[name]=await exportGif(frames,c.delay,path.join(root,'Previews',name+'.gif'));
 }
 console.log('Exporting automatic preview GIF');
 qa.gifs.auto=await exportGif([...all.idle,...all.think],[...clips.idle.delay,...clips.think.delay],path.join(root,'Previews/idle_3s_then_think.gif'));
 function frameAt(name,t){const d=clips[name].delay;let p=t%sum(d);for(let i=0;i<d.length;i++){if(p<d[i])return i;p-=d[i];}return 0;}
 console.log('Exporting comparison GIF');
 const thumb={};for(const name of ['idle','think'])thumb[name]=await Promise.all(all[name].map(p=>sharp(p).resize(384,384).toBuffer()));
 const heading=Buffer.from('<svg width="768" height="440"><style>text{font-family:Arial,sans-serif;font-weight:600;fill:#dce7ec;font-size:18px}</style><text x="32" y="36">IDLE / 3s</text><text x="416" y="36">THINK / 6s</text><path d="M384 64V410" stroke="#304450"/></svg>');
 const compare=[],cache=new Map();
 for(let t=0;t<6000;t+=40){
  const a=frameAt('idle',t),b=frameAt('think',t),key=a+':'+b;
  if(!cache.has(key))cache.set(key,await sharp({create:{width:768,height:440,channels:4,background:'#172732'}})
   .composite([{input:heading,left:0,top:0},{input:thumb.idle[a],left:0,top:50},{input:thumb.think[b],left:384,top:50}]).removeAlpha().raw().toBuffer());
  compare.push(cache.get(key));
 }
 await sharp(Buffer.concat(compare),{raw:{width:768,height:440*compare.length,channels:3,pageHeight:440}})
  .gif({delay:Array(compare.length).fill(40),loop:0,effort:5,dither:0}).toFile(path.join(root,'Previews/comparison.gif'));
 qa.gifs.comparison=await sharp(path.join(root,'Previews/comparison.gif'),{animated:true}).metadata();
 // A fixed face crop for human review. This is only a contact sheet, never a game asset.
 const eyeTiles=[];
 for(let i=0;i<all.think.length;i++){
  const crop=await sharp(all.think[i]).extract({left:220,top:110,width:220,height:180}).resize(264,216).toBuffer();
  const label=Buffer.from('<svg width="280" height="250"><text x="10" y="20" fill="#edf1f4" font-family="Arial" font-size="15">'+String(i).padStart(2,'0')+' / '+clips.think.delay[i]+'ms</text></svg>');
  eyeTiles.push(await sharp({create:{width:280,height:250,channels:4,background:'#233746'}}).composite([{input:crop,left:8,top:28},{input:label,left:0,top:0}]).png().toBuffer());
 }
 await sharp({create:{width:2240,height:1000,channels:4,background:'#172732'}}).composite(eyeTiles.map((input,i)=>({input,left:i%8*280,top:Math.floor(i/8)*250}))).png().toFile(path.join(root,'QA/think-eye-contact-sheet.png'));
 const cumulative=d=>d.reduce((a,v)=>[...a,a[a.length-1]+v],[0]);
 const ts=cumulative(clips.think.delay);
 const phase=(name,first,last)=>({name,startMs:ts[first],endMs:ts[last],firstFrame:first,lastFrame:last-1});
 const manifest={
  name:'LightHungry Player Idle v03',date:'2026-10-25',generator:'built-in image_gen',frameSize:[SIZE,SIZE],facing:'right',pixelsPerUnit:256,pivot:[0.5,0.0625],footReferenceY:480,
  alpha:'RGBA PNG; generated alpha retained through cropping and uniform resizing.',
  playback:{idleThresholdSeconds:3,thinkingLoopsWhileIdle:true,interruptImmediatelyOn:['move','jump','interact','ability','damage','death'],timerReset:'Any gameplay action or leaving grounded neutral state.'},
  clips:Object.fromEntries(Object.entries(clips).map(([name,c])=>[name,{
   frameCount:all[name].length,loop:true,durationMs:sum(c.delay),frameDurationMs:c.delay,grid:'Sheets/player_'+name+'_grid.png',gridLayout:[c.cols,c.rows],horizontalPages:horizontalPages[name],
   frames:all[name].map((_,i)=>frameFile(name,i)),frameOrigins:origins[name],
   endpointReuse:name==='idle'?'Frame 15 reuses frame 00.':'Frames 00 and 31 reuse idle frame 00; frames 08 and 09 reuse frame 07 for a stable left hold.',
   phases:name==='idle'?[{name:'呼吸与眨眼',startMs:0,endMs:3000}]:[
    phase('进入思考',0,2),phase('头向左偏',2,8),phase('左偏停留',8,10),phase('缓慢转向右侧',10,19),phase('右偏停留',19,20),phase('快速眨眼两次',20,28),phase('放下手并回正',28,32)
   ],
   ...(name==='think'?{blinkFrames:[{closedFrame:21},{closedFrame:25}],eyeRule:'Solid black bead eye; closed line, no visible sclera or rolling.'}:{})
  }]))
 };
 fs.writeFileSync(path.join(root,'animations.json'),JSON.stringify(manifest,null,2));
 fs.writeFileSync(path.join(root,'animations.js'),'window.ANIMATION_MANIFEST = '+JSON.stringify(manifest,null,2)+';\n');
 qa.checks={
  frameCount:qa.frames.length===48,
  clipDurations:sum(clips.idle.delay)===3000&&sum(clips.think.delay)===6000,
  everyFrameTransparent:qa.frames.every(f=>f.alphaHistogram[0]>100000),
  everyFrameVisible:qa.frames.every(f=>sum(f.alphaHistogram.slice(128))>50000),
  feetAligned:qa.frames.every(f=>f.bbox[3]===FOOT_Y&&Math.abs(f.footCenter-FOOT_X)<=1),
  noFrameEdgeClipping:qa.frames.every(f=>f.edgeNontransparent===0),
  sharedEndpoints:all.idle[0].equals(all.idle[15])&&all.idle[0].equals(all.think[0])&&all.idle[0].equals(all.think[31]),
  stableLeftHold:all.think[7].equals(all.think[8])&&all.think[7].equals(all.think[9]),
  autoStartsThinkAt3Seconds:sum(clips.idle.delay)===3000,
  gifDurations:sum(qa.gifs.idle.delay)===3000&&sum(qa.gifs.think.delay)===6000&&sum(qa.gifs.auto.delay)===9000&&sum(qa.gifs.comparison.delay)===6000,
  gifComparisonMinimumDelay:Math.min(...qa.gifs.comparison.delay)>=40,
  sheetsWithin4096:clips.idle.cols*SIZE<=4096&&clips.think.cols*SIZE<=4096&&horizontalPages.think.every(p=>p.dimensions[0]<=4096)
 };
 qa.manualReviewRequired=['Meaning and direction of head tilts','Two black-eye blink gestures without eye rolling','Paint/outline consistency while played at game scale'];
 qa.visibleHeightRanges=Object.fromEntries(['idle','think'].map(name=>{const f=qa.frames.filter(x=>x.clip===name),h=f.map(x=>x.bbox[3]-x.bbox[1]+1);return[name,{minimumPx:Math.min(...h),maximumPx:Math.max(...h),neutralFrameHeightPx:h[0],alphaThreshold:32}];}));
 qa.commonVisualScaleCorrection=TARGET_FOOT_WIDTH/198;
 fs.writeFileSync(path.join(root,'QA/export-checks.json'),JSON.stringify(qa,null,2));
 console.log(JSON.stringify({checks:qa.checks,sourceScale,phases:manifest.clips.think.phases,gifDurations:Object.fromEntries(Object.entries(qa.gifs).map(([k,v])=>[k,sum(v.delay)]))},null,2));
 if(Object.values(qa.checks).some(v=>!v))process.exitCode=1;
})().catch(e=>{console.error(e);process.exit(1)});


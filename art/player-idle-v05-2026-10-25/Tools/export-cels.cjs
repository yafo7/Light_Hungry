// Export reviewed complete-cel PNGs. This never generates interpolated poses.
const fs=require('node:fs'),path=require('node:path'),sharp=require('sharp');
const {sum,quantize}=require('./cel-utils.cjs');
const root=path.resolve(__dirname,'..'),qa=JSON.parse(fs.readFileSync(path.join(root,'QA/prepared.json'),'utf8')),plan=qa.plan;
if((plan.status||'').toUpperCase().includes('INTAKE'))throw new Error('Prepared assets are marked intake-only; finish cel review and update the source plan before media export.');
const all={idle:[],think:[]};
function at(clip,t){const ds=plan.clips[clip].frameDurationMs;let p=t%sum(ds);for(let i=0;i<ds.length;i++){if(p<ds[i]-1e-7)return i;p-=ds[i];}return 0;}
async function encode(pngs,delays,stem){
 const raw=Buffer.concat(await Promise.all(pngs.map(p=>sharp(p).resize(384,384).flatten({background:'#172732'}).removeAlpha().raw().toBuffer())));
 const opts={raw:{width:384,height:384*pngs.length,channels:3,pageHeight:384}};
 const g=quantize(delays,10),w=quantize(delays,1);
 if(g.some(v=>v<20))throw new Error('Source clip GIF delay under20ms: '+stem);
 await sharp(raw,opts).gif({delay:g,loop:0,effort:5,dither:0}).toFile(path.join(root,'Previews',stem+'.gif'));
 await sharp(raw,opts).webp({delay:w,loop:0,lossless:true,effort:4}).toFile(path.join(root,'Previews',stem+'.webp'));
 return {gif:await sharp(path.join(root,'Previews',stem+'.gif'),{animated:true}).metadata(),webp:await sharp(path.join(root,'Previews',stem+'.webp'),{animated:true}).metadata()};
}
(async()=>{
 const atlasPages={},clips={};
 for(const clip of ['idle','think']){
  const reports=qa.frameReports.filter(f=>f.clip===clip).sort((a,b)=>a.index-b.index);
  all[clip]=reports.map(f=>fs.readFileSync(path.join(root,f.file)));atlasPages[clip]=[];
  for(let start=0;start<all[clip].length;start+=64){
   const part=all[clip].slice(start,start+64),columns=8,rows=Math.ceil(part.length/8),file='Sheets/player_'+clip+'_atlas_'+String(start/64+1).padStart(2,'0')+'.png';
   await sharp({create:{width:columns*512,height:rows*512,channels:4,background:{r:0,g:0,b:0,alpha:0}}})
    .composite(part.map((input,i)=>({input,left:i%8*512,top:Math.floor(i/8)*512}))).png().toFile(path.join(root,file));
   atlasPages[clip].push({file,firstFrame:start,frameCount:part.length,columns,rows});
  }
  clips[clip]={frameCount:all[clip].length,loop:true,durationMs:plan.clips[clip].durationMs,frameDurationMs:plan.clips[clip].frameDurationMs,
   frames:reports.map(f=>f.file),frameOrigins:reports.map(f=>f.origin),independentImageCount:qa.uniqueImages[clip],uniqueFrameCount:qa.uniqueImages[clip],
   atlasPages:atlasPages[clip],grid:atlasPages[clip][0].file,phases:plan.clips[clip].phases||[],animationMethod:'Complete character sprite cels switched directly according to frameDurationMs.'};
 }
 const manifest={name:plan.name||'LightHungry Player Idle v05',date:plan.date||'2026-10-25',status:plan.status,generator:'built-in image_gen complete character cels',productionMode:'complete-sprite-cels',
  frameSize:[512,512],facing:'right',pixelsPerUnit:256,pivot:[.5,.0625],footReferenceY:480,
  registration:{kind:qa.registrationMode,neutralHeightPx:qa.neutralHeightTarget,footBottomY:479,footCenterX:285.5,details:'QA/prepared.json'},
  playback:{idleThresholdSeconds:3,thinkingLoopsWhileIdle:true,interruptImmediatelyOn:['move','jump','interact','ability','damage','death']},clips};
 fs.writeFileSync(path.join(root,'animations.json'),JSON.stringify(manifest,null,2));fs.writeFileSync(path.join(root,'animations.js'),'window.ANIMATION_MANIFEST = '+JSON.stringify(manifest,null,2)+';\n');
 console.log('Atlases and manifest ready. Encoding review media.');
 const previews={};
 previews.idle=await encode(all.idle,clips.idle.frameDurationMs,'idle');
 previews.think=await encode(all.think,clips.think.frameDurationMs,'think');
 previews.auto=await encode([...all.idle,...all.think],[...clips.idle.frameDurationMs,...clips.think.frameDurationMs],'idle_3s_then_think');
 const duration=clips.think.durationMs;
 const thumbs={};
 for(const clip of ['idle','think'])thumbs[clip]=await Promise.all(all[clip].map(p=>sharp(p).resize(384,384).png().toBuffer()));
 const title=Buffer.from('<svg width="768" height="440"><style>text{font-family:Arial,sans-serif;fill:#edf0f2;font-weight:600;font-size:18px}</style><text x="28" y="35">IDLE / FULL SPRITE CELS</text><text x="408" y="35">THINK / FULL SPRITE CELS</text></svg>');
 const cache=new Map();
 async function pair(t){const a=at('idle',t),b=at('think',t),key=a+':'+b;if(!cache.has(key))cache.set(key,await sharp({create:{width:768,height:440,channels:4,background:'#172732'}})
  .composite([{input:title,left:0,top:0},{input:thumbs.idle[a],left:0,top:50},{input:thumbs.think[b],left:384,top:50}]).removeAlpha().raw().toBuffer());return cache.get(key);}
 // A 20ms comparison sampling grid avoids very short delays from the union of two timelines.
 // An exact union had 1-10ms intervals that the WebP encoder expanded; the duration check caught this.
 // Individual clip GIFs preserve their own authored timing to cumulative 10ms rounding.
 const sampled=[],sampleDelay=[];for(let t=0;t<duration;t+=20){sampled.push(await pair(t));sampleDelay.push(Math.min(20,duration-t));}
 await sharp(Buffer.concat(sampled),{raw:{width:768,height:440*sampled.length,channels:3,pageHeight:440}}).webp({delay:sampleDelay,loop:0,lossless:true,effort:4}).toFile(path.join(root,'Previews/comparison.webp'));
 await sharp(Buffer.concat(sampled),{raw:{width:768,height:440*sampled.length,channels:3,pageHeight:440}}).gif({delay:sampleDelay,loop:0,effort:5,dither:0}).toFile(path.join(root,'Previews/comparison.gif'));
 previews.comparison={gif:await sharp(path.join(root,'Previews/comparison.gif'),{animated:true}).metadata(),webp:await sharp(path.join(root,'Previews/comparison.webp'),{animated:true}).metadata(),timingNote:'Both comparison formats sample the authored complete cels every20ms without interpolation. Individual WebP clips preserve exact authored durations; individual GIF clips use cumulative10ms rounding.'};
 const total=clips.idle.durationMs+clips.think.durationMs;
 const mediaChecks={
  gifDurations:sum(previews.idle.gif.delay)===clips.idle.durationMs&&sum(previews.think.gif.delay)===clips.think.durationMs&&sum(previews.auto.gif.delay)===total&&sum(previews.comparison.gif.delay)===duration,
  webpDurations:sum(previews.idle.webp.delay)===clips.idle.durationMs&&sum(previews.think.webp.delay)===clips.think.durationMs&&sum(previews.auto.webp.delay)===total&&sum(previews.comparison.webp.delay)===duration,
  atlasMax4096:Object.values(atlasPages).flat().every(p=>p.columns*512<=4096&&p.rows*512<=4096),
  atlasCoverage:Object.entries(atlasPages).every(([c,ps])=>sum(ps.map(p=>p.frameCount))===clips[c].frameCount),
  timerMatchesIdleDuration:clips.idle.durationMs===3000,
  noRigMetadata:!('rig' in manifest)&&!('sampleRate' in manifest),
 };
 const report={...qa,previews,checks:{...qa.checks,...mediaChecks},exportedClipCounts:Object.fromEntries(Object.entries(clips).map(([c,v])=>[c,{frameCount:v.frameCount,durationMs:v.durationMs,independentImageCount:v.independentImageCount}]))};
 fs.writeFileSync(path.join(root,'QA/export-checks.json'),JSON.stringify(report,null,2));
 console.log(JSON.stringify({checks:report.checks,clips:report.exportedClipCounts,atlasPages,mediaDurations:Object.fromEntries(Object.entries(previews).map(([k,v])=>[k,{gif:sum(v.gif.delay),webp:sum(v.webp.delay)}]))},null,2));
 if(Object.values(report.checks).some(v=>!v))process.exitCode=1;
})().catch(e=>{console.error(e);process.exit(1)});


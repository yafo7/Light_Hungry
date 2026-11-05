// v05 complete character cels only. No parts, rig, optical flow, morphing or crossfades.
// Usage: node prepare-cels.cjs ../source-plan.json
const fs=require('node:fs'),path=require('node:path'),sharp=require('sharp');
const {hash,sum,components,bounds,frameFile,diff,montage}=require('./cel-utils.cjs');
const root=path.resolve(__dirname,'..'),planPath=path.resolve(process.argv[2]||path.join(root,'source-plan.json'));
const plan=JSON.parse(fs.readFileSync(planPath,'utf8'));
(async()=>{
 for(const dir of ['Source','Sprites/Idle','Sprites/Think','Sheets','Previews','QA'])fs.mkdirSync(path.join(root,dir),{recursive:true});
 const sourceInfo=[],rawCels={idle:[],think:[]};
 for(let sourceIndex=0;sourceIndex<plan.sources.length;sourceIndex++){
  const s=plan.sources[sourceIndex],cols=s.cols||4,rows=s.rows||3,count=cols*rows;
  if(!['idle','think'].includes(s.clip))throw new Error('Unknown clip '+s.clip);
  const src=path.isAbsolute(s.source)?s.source:path.resolve(path.dirname(planPath),s.source);
  const withinSource=path.resolve(src).startsWith(path.resolve(root,'Source')+path.sep);
  const saved=withinSource?path.relative(root,src).split(path.sep).join('/'):'Source/'+s.clip+'_sheet_'+String(s.frameStart).padStart(3,'0')+'.png';
  if(path.resolve(src)!==path.resolve(root,saved))fs.copyFileSync(src,path.join(root,saved));
  const whole=await sharp(src).ensureAlpha().raw().toBuffer({resolveWithObject:true});
  const silhouettes=components(whole.data,whole.info.width,whole.info.height,count,cols);
  const clippedSourceCells=silhouettes.flatMap((c,i)=>(c.x0<=0||c.y0<=0||c.x1>=whole.info.width-1||c.y1>=whole.info.height-1)?[i]:[]);
  const sourceBytes=fs.readFileSync(src);
  const record={...s,cols,rows,source:saved,originalSource:src,sha256:hash(sourceBytes),bytes:sourceBytes.length,width:whole.info.width,height:whole.info.height,components:silhouettes,clippedSourceCells};
  sourceInfo.push(record);
  for(let j=0;j<count;j++){
   const c=silhouettes[j],x=Math.max(0,c.x0-5),y=Math.max(0,c.y0-5),w=Math.min(whole.info.width,c.x1+6)-x,h=Math.min(whole.info.height,c.y1+6)-y;
   const crop=await sharp(src).extract({left:x,top:y,width:w,height:h}).ensureAlpha().raw().toBuffer({resolveWithObject:true}),m=bounds(crop.data,w,h);
   const index=s.frameStart+j;
   if(rawCels[s.clip][index])throw new Error('Overlapping source cels '+s.clip+' '+index);
   rawCels[s.clip][index]={sourceIndex,sourceCell:j,sourceFile:saved,sourceRect:[x,y,w,h],width:w,height:h,raw:crop.data,metrics:m};
  }
 }
 for(const clip of ['idle','think']){
  const durations=plan.clips[clip].frameDurationMs;
  if(!durations?.length||durations.some(d=>!(d>0)))throw new Error('Invalid durations '+clip);
  if(rawCels[clip].length!==durations.length||Array.from({length:durations.length},(_,i)=>!rawCels[clip][i]).some(Boolean))throw new Error('Incomplete source coverage '+clip);
  if(Math.abs(sum(durations)-plan.clips[clip].durationMs)>.001)throw new Error('Duration mismatch '+clip);
 }
 const neutral=rawCels.idle[0].metrics,desiredHeight=plan.registration?.neutralHeightPx||378;
 const targetFootWidth=plan.registration?.targetFootWidth||(neutral.footWidth*desiredHeight/(neutral.bbox[3]-neutral.bbox[1]+1));
 const byHeight=plan.registration?.mode==='whole-character-per-frame-uniform-height';
 const footY=479,footX=285.5,frameReports=[],all={idle:[],think:[]},rawOutput={idle:[],think:[]};
 for(const clip of ['idle','think'])for(let i=0;i<rawCels[clip].length;i++){
  const cel=rawCels[clip][i];let scale=byHeight?desiredHeight/(cel.metrics.bbox[3]-cel.metrics.bbox[1]+1):targetFootWidth/cel.metrics.footWidth;
  // One uniform transform is applied to the entire drawing; no local image adjustment.
  let raster,m;
  for(let pass=0;pass<(byHeight?3:1);pass++){
   const resize=byHeight?{height:Math.round(cel.height*scale),kernel:'lanczos3'}:{width:Math.round(cel.width*scale),kernel:'lanczos3'};
   raster=await sharp(cel.raw,{raw:{width:cel.width,height:cel.height,channels:4}}).resize(resize).ensureAlpha().raw().toBuffer({resolveWithObject:true});
   m=bounds(raster.data,raster.info.width,raster.info.height);const actualHeight=m.bbox[3]-m.bbox[1]+1;
   if(!byHeight||actualHeight===desiredHeight||pass===2)break;scale*=desiredHeight/actualHeight;
  }
  const left=Math.round(footX-m.footCenter),top=footY-m.bbox[3];
  if(left<0||top<0||left+raster.info.width>512||top+raster.info.height>512)throw new Error('Cel outside canvas '+clip+':'+i+'; inspect whole-character registration.');
  const png=await sharp({create:{width:512,height:512,channels:4,background:{r:0,g:0,b:0,alpha:0}}}).composite([{input:raster.data,raw:{width:raster.info.width,height:raster.info.height,channels:4},left,top}]).png().toBuffer();
  all[clip][i]=png;
  frameReports.push({clip,index:i,file:frameFile(clip,i),origin:{source:cel.sourceFile,sourceCell:cel.sourceCell},registration:{kind:'whole-character uniform scale and translation only',sourceRect:cel.sourceRect,requestedUniformScale:scale,outputRasterSize:[raster.info.width,raster.info.height],translate:[left,top]}});
 }
 for(const reuse of plan.frameReuse||[]){
  const origin=all[reuse.fromClip]?.[reuse.fromIndex];if(!origin||!all[reuse.clip]?.[reuse.index])throw new Error('Invalid frameReuse '+JSON.stringify(reuse));
  all[reuse.clip][reuse.index]=origin;
  const entry=frameReports.find(f=>f.clip===reuse.clip&&f.index===reuse.index);
  entry.origin={reuse:{clip:reuse.fromClip,index:reuse.fromIndex},reason:reuse.reason||'Explicitly reused hold pose'};
  entry.registration={reuse:{clip:reuse.fromClip,index:reuse.fromIndex},reason:'Same complete cel and registration reused.'};
 }
 for(const report of frameReports){
  const png=all[report.clip][report.index];fs.writeFileSync(path.join(root,report.file),png);
  const raw=await sharp(png).ensureAlpha().raw().toBuffer();rawOutput[report.clip][report.index]=raw;
  const m=bounds(raw,512,512);let edge=0;for(let z=0;z<512;z++)for(const p of[z,511*512+z,z*512,z*512+511])if(raw[p*4+3])edge++;
  Object.assign(report,m,{sha256:hash(png),edgeNontransparent:edge,dimensions:[512,512]});
 }
 const adjacent=[];
 for(const clip of ['idle','think'])for(let i=0;i<all[clip].length;i++){
  const next=(i+1)%all[clip].length;
  adjacent.push({clip,fromFrame:i,toFrame:next,isLoop:next===0,isSourceBoundary:sourceInfo.some(s=>s.clip===clip&&s.frameStart===next&&next!==0),...diff(rawOutput[clip][i],rawOutput[clip][next])});
 }
 adjacent.sort((a,b)=>b.foregroundMeanAbsoluteRgbDifference-a.foregroundMeanAbsoluteRgbDifference);
 const stateTransitions=[{fromClip:'idle',fromFrame:all.idle.length-1,toClip:'think',toFrame:0},{fromClip:'think',fromFrame:all.think.length-1,toClip:'idle',toFrame:0}].map(d=>({...d,...diff(rawOutput[d.fromClip][d.fromFrame],rawOutput[d.toClip][d.toFrame])}));
 const keys=new Map();
 const add=(clip,index,label)=>{index=Math.max(0,Math.min(all[clip].length-1,index));keys.set(clip+':'+index,{clip,index,label});};
 for(const s of sourceInfo){add(s.clip,s.frameStart,s.clip.toUpperCase()+' '+s.frameStart+' / sheet start');add(s.clip,s.frameStart+s.cols*s.rows-1,s.clip.toUpperCase()+' '+(s.frameStart+s.cols*s.rows-1)+' / sheet end');}
 for(const clip of ['idle','think'])for(const p of plan.clips[clip].phases||[]){
  let time=0,index=0;while(index<all[clip].length-1&&time+plan.clips[clip].frameDurationMs[index]<=p.startMs){time+=plan.clips[clip].frameDurationMs[index++];}
  add(clip,index,clip.toUpperCase()+' '+index+' / phase');
 }
 await montage([...keys.values()].map(k=>({png:all[k.clip][k.index],label:k.label})),path.join(root,'QA/key_poses_and_seams.png'),{columns:6,tile:256});
 const seamPairs=adjacent.filter(d=>d.isSourceBoundary||d.isLoop);
 const seamItems=seamPairs.flatMap(d=>[{png:all[d.clip][d.fromFrame],label:d.clip+' '+d.fromFrame+' BEFORE'},{png:all[d.clip][d.toFrame],label:d.clip+' '+d.toFrame+' AFTER'}]);
 for(const d of stateTransitions)seamItems.push({png:all[d.fromClip][d.fromFrame],label:d.fromClip+' '+d.fromFrame+' SWITCH'},{png:all[d.toClip][d.toFrame],label:d.toClip+' '+d.toFrame+' SWITCH'});
 await montage(seamItems,path.join(root,'QA/seam_pairs.png'),{columns:6,tile:256});
 await montage(adjacent.slice(0,12).flatMap(d=>[{png:all[d.clip][d.fromFrame],label:d.clip+' '+d.fromFrame+' / diff '+d.foregroundMeanAbsoluteRgbDifference.toFixed(1)},{png:all[d.clip][d.toFrame],label:d.clip+' '+d.toFrame}]),path.join(root,'QA/largest_adjacent_changes.png'),{columns:6,tile:256});
 const qa={method:'Complete sprite cels. Only complete-character uniform scaling and translation registration. No rig, part transform, optical flow, warping, morphing or crossfade.',registrationMode:byHeight?'whole-character-per-frame-uniform-height':'whole-character-per-frame-uniform-foot-width',sourceInfo,targetFootWidth:byHeight?null:targetFootWidth,neutralHeightTarget:desiredHeight,plan,frameReports,adjacent,stateTransitions,
  uniqueImages:Object.fromEntries(['idle','think'].map(c=>[c,new Set(frameReports.filter(f=>f.clip===c).map(f=>f.sha256)).size])),
  visibleHeightRanges:Object.fromEntries(['idle','think'].map(c=>{const h=frameReports.filter(f=>f.clip===c).map(f=>f.bbox[3]-f.bbox[1]+1);return[c,{minimumPx:Math.min(...h),maximumPx:Math.max(...h),neutralFrameHeightPx:h[0]}];})),
  checks:{allFrames512:frameReports.every(f=>f.dimensions[0]===512&&f.dimensions[1]===512),trueAlpha:frameReports.every(f=>f.alphaHistogram[0]>100000&&sum(f.alphaHistogram.slice(128))>50000),registeredFeet:frameReports.every(f=>f.bbox[3]===479&&Math.abs(f.footCenter-285.5)<=1),noEdgeClipping:frameReports.every(f=>f.edgeNontransparent===0),noSourceCanvasClipping:sourceInfo.every(s=>s.clippedSourceCells.length===0)},
  manualReviewRequired:'Character shape consistency and animation fluidity are visual-review judgments; no rigid-head proof is claimed.'};
 fs.writeFileSync(path.join(root,'QA/prepared.json'),JSON.stringify(qa,null,2));
 fs.writeFileSync(path.join(root,'QA/adjacent-frame-differences.json'),JSON.stringify(adjacent,null,2));
 console.log(JSON.stringify({checks:qa.checks,uniqueImages:qa.uniqueImages,targetFootWidth,visibleHeightRanges:qa.visibleHeightRanges,largestChanges:adjacent.slice(0,12)},null,2));
 if(Object.values(qa.checks).some(v=>!v))process.exitCode=1;
})().catch(e=>{console.error(e);process.exit(1)});


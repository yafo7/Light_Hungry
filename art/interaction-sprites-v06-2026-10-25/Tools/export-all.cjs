const fs=require('fs'),path=require('path'),sharp=require('sharp');
const {hash,sum,bounds,diff,montage,quantize}=require('./cel-utils.cjs');
const root=path.resolve(__dirname,'..'),old=path.join(root,'Source/V05');
const oldM=JSON.parse(fs.readFileSync(path.join(old,'animations.json'),'utf8'));
const clips={},reports=[],buffers=new Map();
function absolute(f){return path.isAbsolute(f)?f:path.join(root,f)}
function chooseAt(c,t){let p=c.loop?t%c.durationMs:Math.min(t,c.durationMs-0.001);for(let i=0;i<c.frameCount;i++){if(p<c.frameDurationMs[i])return i;p-=c.frameDurationMs[i]}return c.frameCount-1}
async function png(f){if(!buffers.has(f))buffers.set(f,await sharp(absolute(f)).ensureAlpha().png().toBuffer());return buffers.get(f)}
async function add(id,label,sources,delays,loop,pivot){
const dir='Sprites/'+id;fs.mkdirSync(path.join(root,dir),{recursive:true});
const frames=[],origins=[];
for(let i=0;i<sources.length;i++){const source=sources[i],file=dir+'/'+id+'_'+String(i).padStart(3,'0')+'.png';fs.copyFileSync(absolute(source),path.join(root,file));frames.push(file);origins.push(source.split(path.sep).join('/'));}
clips[id]={label,frameCount:frames.length,durationMs:sum(delays),frameDurationMs:delays,loop,frameSize:[512,512],pivot,pixelsPerUnit:256,frames,frameOrigins:origins};
}
function reverse(id,from,label){const c=clips[from],indices=Array.from({length:c.frameCount},(_,i)=>c.frameCount-1-i);clips[id]={...c,label,frames:indices.map(i=>c.frames[i]),frameDurationMs:indices.map(i=>c.frameDurationMs[i]),frameOrigins:indices.map(i=>({reusedCompleteCelFrom:from,index:i})),reverseOf:from};}
async function encode(frames,delays,stem,size=320){
 const images=await Promise.all(frames.map(async f=>sharp(await png(f)).resize(size,size).flatten({background:'#182a36'}).removeAlpha().raw().toBuffer()));
 const input=Buffer.concat(images),opts={raw:{width:size,height:size*frames.length,channels:3,pageHeight:size}};
 const dir=path.join(root,'Previews');
 await sharp(input,opts).webp({lossless:true,effort:3,loop:0,delay:quantize(delays,1)}).toFile(path.join(dir,stem+'.webp'));
 await sharp(input,opts).gif({effort:4,dither:0,loop:0,delay:quantize(delays,10)}).toFile(path.join(dir,stem+'.gif'));
}
(async()=>{
for(const d of ['Sprites','Sheets','Previews','QA'])fs.mkdirSync(path.join(root,d),{recursive:true});
await add('idle','普通待机',oldM.clips.idle.frames.map(f=>path.join(old,f)),oldM.clips.idle.frameDurationMs,true,[.5,.0625]);
const oldKeep=oldM.clips.think.frames.slice(0,66).map(f=>path.join(old,f));
const bridge=[6,7,9,10,11].map(i=>'Source/Bridge/bridge_'+String(i).padStart(2,'0')+'.png');
const settle=[0,1,2,4,5,6].map(i=>'Source/Cels/settle/'+String(i).padStart(2,'0')+'.png');
await add('think','思考待机 · 收势修正版',[...oldKeep,...bridge,...settle,path.join(old,oldM.clips.idle.frames[0])],[...oldM.clips.think.frameDurationMs.slice(0,66),...Array(5).fill(30),...Array(6).fill(25),200],true,[.5,.0625]);
clips.think.phases=[{name:'抬手',startMs:0,endMs:1000},{name:'微左偏与停留',startMs:1000,endMs:2000},{name:'缓慢向右偏',startMs:2000,endMs:3000},{name:'快速双眨眼',startMs:3000,endMs:4000},{name:'回正与收手',startMs:4000,endMs:5500},{name:'新收势过渡',startMs:5500,endMs:5800},{name:'稳定回到idle',startMs:5800,endMs:6000}];
const wheelCCW=[...Array.from({length:8},(_,i)=>'Source/Cels/wheel_top/'+String(i).padStart(2,'0')+'.png'),...[15,16,18,19,20,21,22].map(i=>'Source/Cels/wheel/'+String(i).padStart(2,'0')+'.png')];
// Eight top positions plus seven lower positions, ordered around the same fixed axle.
await add('wheel_ccw','Wheel 逆时针',wheelCCW,Array(wheelCCW.length).fill(60),true,[.5,.5]);
const w=clips.wheel_ccw,wi=[0,...Array.from({length:w.frameCount-1},(_,i)=>w.frameCount-1-i)];
clips.wheel_cw={...w,label:'Wheel 顺时针',frames:wi.map(i=>w.frames[i]),frameOrigins:wi.map(i=>({reusedCompleteCelFrom:'wheel_ccw',index:i})),reverseOf:'wheel_ccw'};
await add('lever_on','Lever 左 → 右',Array.from({length:16},(_,i)=>'Source/Cels/lever/'+String(i).padStart(2,'0')+'.png'),Array(16).fill(35),false,[.5,.0625]);reverse('lever_off','lever_on','Lever 右 → 左');
await add('mirror_left','Mirror 向左倾',Array.from({length:16},(_,i)=>'Source/Cels/mirror/'+String(i).padStart(2,'0')+'.png'),Array(16).fill(50),false,[.5,.0625]);reverse('mirror_right','mirror_left','Mirror 向右倾');
for(const[id,c]of Object.entries(clips)){
 c.atlasPages=[];
 for(let first=0;first<c.frameCount;first+=64){const frames=c.frames.slice(first,first+64),cols=8,rows=Math.ceil(frames.length/cols),file='Sheets/'+id+'_'+(first/64+1)+'.png';
 await sharp({create:{width:cols*512,height:rows*512,channels:4,background:'#00000000'}}).composite(await Promise.all(frames.map(async(f,i)=>({input:await png(f),left:i%cols*512,top:Math.floor(i/cols)*512})))).png().toFile(path.join(root,file));
 c.atlasPages.push({file,firstFrame:first,frameCount:frames.length,columns:cols,rows});
 }
 c.uniqueImageCount=new Set(await Promise.all(c.frames.map(async f=>hash(await png(f))))).size;
}
const seen=new Set();for(const c of Object.values(clips))for(const f of c.frames){if(seen.has(f))continue;seen.add(f);const raw=await sharp(await png(f)).ensureAlpha().raw().toBuffer();const m=bounds(raw,512,512);let edge=0;for(let z=0;z<512;z++)for(const p of[z,511*512+z,z*512,z*512+511])if(raw[p*4+3]>0)edge++;
reports.push({file:f,sha256:hash(await png(f)),dimensions:[512,512],alphaZero:m.alphaHistogram[0],alphaNonzero:512*512-m.alphaHistogram[0],edgeNontransparent:edge,bbox:m.bbox});}
const manifest={name:'小食光兽 · 收势修正与道具交互 v06',date:'2026-10-25',productionMode:'complete-sprite-cels',frameSize:[512,512],playback:{idleThresholdSeconds:3},clips,notes:['Sprite PNG cel playback only. No skeletal animation or live part transforms.','Whole-cel uniform scale and anchor registration only.','Reverse prop actions reuse complete forward cels in reverse order.','Source drawings may retain small paint variation; not a rigid-pixel guarantee.']};
fs.writeFileSync(path.join(root,'animations.json'),JSON.stringify(manifest,null,2));fs.writeFileSync(path.join(root,'animations.js'),'window.ANIMATION_MANIFEST='+JSON.stringify(manifest)+';');
const old70=await sharp(path.join(old,oldM.clips.think.frames[70])).raw().toBuffer(),old71=await sharp(path.join(old,oldM.clips.think.frames[71])).raw().toBuffer();
const last=await sharp(await png(clips.think.frames.at(-1))).raw().toBuffer(),prev=await sharp(await png(clips.think.frames.at(-2))).raw().toBuffer();
const tail=[];for(let i=65;i<clips.think.frameCount-1;i++){const a=await sharp(await png(clips.think.frames[i])).raw().toBuffer(),b=await sharp(await png(clips.think.frames[i+1])).raw().toBuffer();tail.push({from:i,to:i+1,...diff(a,b)});}
const at5830=chooseAt(clips.think,5830),at5910=chooseAt(clips.think,5910);
const fix={oldAt5830:70,oldAt5910:71,newAt5830:at5830,newAt5910:at5910,sameNewPoseAtReportedTimes:hash(await png(clips.think.frames[at5830]))===hash(await png(clips.think.frames[at5910])),oldFinalStep:diff(old70,old71),newFinalStep:diff(prev,last),tail,stableRestStartsMs:5800};
fs.writeFileSync(path.join(root,'QA/player-ending-fix.json'),JSON.stringify(fix,null,2));
await montage([{png:path.join(old,oldM.clips.think.frames[70]),label:'v05 / 5.83s'},{png:path.join(old,oldM.clips.think.frames[71]),label:'v05 / 5.91s'},{png:await png(clips.think.frames[at5830]),label:'v06 / 5.83s'},{png:await png(clips.think.frames[at5910]),label:'v06 / 5.91s'}],path.join(root,'QA/ending-before-after.png'),{columns:4,tile:256});
await montage(clips.think.frames.slice(65).map((f,i)=>({png:path.join(root,f),label:'v06 '+(65+i)})),path.join(root,'QA/ending-new-sequence.png'),{columns:7,tile:200});
console.log('Atlases ready; encoding previews');
for(const id of ['idle','think'])await encode(clips[id].frames,clips[id].frameDurationMs,id);
for(const [a,b,stem]of[['wheel_cw','wheel_ccw','wheel'],['lever_on','lever_off','lever'],['mirror_left','mirror_right','mirror']]){
 const ca=clips[a],cb=clips[b];
 if(stem==='wheel')await encode(ca.frames,ca.frameDurationMs,stem);
 else await encode([...ca.frames,ca.frames.at(-1),...cb.frames,cb.frames.at(-1)],[...ca.frameDurationMs,350,...cb.frameDurationMs,350],stem);
}
const rawCompare=[],delayCompare=[];for(let t=5300;t<6500;t+=40){const tt=Math.min(t,5999),oldF=oldM.clips.think.frames[chooseAt({...oldM.clips.think,loop:false},tt)],newF=clips.think.frames[chooseAt({...clips.think,loop:false},tt)];
const title=Buffer.from('<svg width="640" height="360"><g fill="#efcf91" font-size="18" font-family="Arial"><text x="20" y="28">v05 / BEFORE</text><text x="340" y="28">v06 / AFTER</text></g></svg>');
rawCompare.push(await sharp({create:{width:640,height:360,channels:4,background:'#182a36'}}).composite([{input:title,left:0,top:0},{input:await sharp(path.join(old,oldF)).resize(320).png().toBuffer(),left:0,top:40},{input:await sharp(await png(newF)).resize(320).png().toBuffer(),left:320,top:40}]).removeAlpha().raw().toBuffer());delayCompare.push(40);}
await sharp(Buffer.concat(rawCompare),{raw:{width:640,height:360*rawCompare.length,channels:3,pageHeight:360}}).webp({lossless:true,loop:0,delay:delayCompare,effort:3}).toFile(path.join(root,'Previews/ending_comparison.webp'));
const media={};for(const name of ['idle','think','wheel','lever','mirror'])for(const ext of ['webp','gif']){const m=await sharp(path.join(root,'Previews',name+'.'+ext),{animated:true}).metadata();media[name+'.'+ext]={pages:m.pages,durationMs:sum(m.delay),delays:m.delay};}
const checks={allFrames512:reports.every(r=>r.dimensions[0]===512&&r.dimensions[1]===512),allHaveTransparency:reports.every(r=>r.alphaZero>60000&&r.alphaNonzero>10000),noEdgeClipping:reports.every(r=>r.edgeNontransparent===0),reportedTimePairFixed:fix.sameNewPoseAtReportedTimes,finalStepReducedMoreThanHalf:fix.newFinalStep.foregroundMeanAbsoluteRgbDifference<fix.oldFinalStep.foregroundMeanAbsoluteRgbDifference*.5,thinkStill6Seconds:clips.think.durationMs===6000,leverEndpointsMatchReverse:clips.lever_on.frames[0]===clips.lever_off.frames.at(-1),mirrorEndpointsMatchReverse:clips.mirror_left.frames[0]===clips.mirror_right.frames.at(-1),atlasMax4096:Object.values(clips).every(c=>c.atlasPages.every(a=>a.columns*512<=4096&&a.rows*512<=4096)),idleMedia3Seconds:media['idle.webp'].durationMs===3000,thinkMedia6Seconds:media['think.webp'].durationMs===6000,wheelMediaMatches:media['wheel.webp'].durationMs===clips.wheel_cw.durationMs};
fs.writeFileSync(path.join(root,'QA/export-checks.json'),JSON.stringify({checks,clips:Object.fromEntries(Object.entries(clips).map(([k,c])=>[k,{frameCount:c.frameCount,durationMs:c.durationMs,uniqueImageCount:c.uniqueImageCount}])),uniqueSpriteFiles:reports.length,reports,media},null,2));
console.log(JSON.stringify({checks,frames:reports.length,fix:{old:fix.oldFinalStep.foregroundMeanAbsoluteRgbDifference,new:fix.newFinalStep.foregroundMeanAbsoluteRgbDifference,maxNewTail:Math.max(...tail.map(t=>t.foregroundMeanAbsoluteRgbDifference))}},null,2));
if(Object.values(checks).some(v=>!v))process.exitCode=1;
})().catch(e=>{console.error(e);process.exit(1)});

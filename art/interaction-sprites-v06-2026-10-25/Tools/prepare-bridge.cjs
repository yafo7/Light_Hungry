const fs=require('fs'),path=require('path'),sharp=require('sharp');
const {components,bounds,diff,montage}=require('./cel-utils.cjs');
const root=path.resolve(__dirname,'..'),old=path.join(root,'Source/V05');
async function read(file){return sharp(file).ensureAlpha().raw().toBuffer({resolveWithObject:true});}
async function saveCharacterCel(crop,out){
 const m=bounds(crop.data,crop.info.width,crop.info.height),scale=378/(m.bbox[3]-m.bbox[1]+1);
 const r=await sharp(crop.data,{raw:crop.info}).resize({width:Math.round(crop.info.width*scale),kernel:'lanczos3'}).ensureAlpha().raw().toBuffer({resolveWithObject:true});
 const b=bounds(r.data,r.info.width,r.info.height),left=Math.round(285.5-b.footCenter),top=479-b.bbox[3];
 const buf=await sharp({create:{width:512,height:512,channels:4,background:'#00000000'}}).composite([{input:r.data,raw:r.info,left,top}]).png().toBuffer();
 fs.writeFileSync(out,buf);return buf;
}
(async()=>{
 const file=path.join(root,'Source/bridge_generated.png'),r=await read(file),cs=components(r.data,r.info.width,r.info.height,12,4),frames=[];
 fs.mkdirSync(path.join(root,'Source/Bridge'),{recursive:true});
 for(let i=0;i<12;i++){const c=cs[i],left=Math.max(0,c.x0-4),top=Math.max(0,c.y0-4),width=Math.min(r.info.width,c.x1+5)-left,height=Math.min(r.info.height,c.y1+5)-top;
 const crop=await sharp(file).extract({left,top,width,height}).raw().toBuffer({resolveWithObject:true});frames.push(await saveCharacterCel(crop,path.join(root,'Source/Bridge/bridge_'+String(i).padStart(2,'0')+'.png')));}
 const inputs={};for(const i of [64,65,66,67,68,69,70,71])inputs['old'+i]=await read(path.join(old,'Sprites/Think/player_think_'+String(i).padStart(3,'0')+'.png'));
 for(let i=0;i<12;i++)inputs['b'+i]=await read(frames[i]);
 const metrics={original:diff(inputs.old70.data,inputs.old71.data),bridge:frames.map((_,i)=>({i,from70:diff(inputs.old70.data,inputs['b'+i].data),to71:diff(inputs['b'+i].data,inputs.old71.data),next:i<11?diff(inputs['b'+i].data,inputs['b'+(i+1)].data):null}))};
 fs.writeFileSync(path.join(root,'QA/bridge-metrics.json'),JSON.stringify(metrics,null,2));
 await montage([{png:path.join(old,'Sprites/Think/player_think_070.png'),label:'OLD 70 / 5.83s'},...frames.map((png,i)=>({png,label:'BRIDGE '+i})),{png:path.join(old,'Sprites/Think/player_think_071.png'),label:'IDLE TARGET'}],path.join(root,'QA/bridge-contact.png'),{columns:7,tile:200});
 console.log(JSON.stringify({original:metrics.original,bridge:metrics.bridge.map(m=>({i:m.i,from70:m.from70.foregroundMeanAbsoluteRgbDifference,to71:m.to71.foregroundMeanAbsoluteRgbDifference,next:m.next?.foregroundMeanAbsoluteRgbDifference}))},null,2));
})().catch(e=>{console.error(e);process.exit(1)});

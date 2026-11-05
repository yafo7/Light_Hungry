const fs=require('fs'),path=require('path'),sharp=require('sharp');
const {bounds,hash}=require('./cel-utils.cjs');
const root=path.resolve(__dirname,'..');
const abs=f=>path.join(root,f);
async function layer(rect,sourcePivot,file){
 const scale=.42, target=[256,372];
 const crop=await sharp(abs('Source/lever_parts_generated.png')).extract(rect).resize({width:Math.round(rect.width*scale)}).png().toBuffer();
 const left=Math.round(target[0]-(sourcePivot[0]-rect.left)*scale),top=Math.round(target[1]-(sourcePivot[1]-rect.top)*scale);
 await sharp({create:{width:512,height:512,channels:4,background:'#00000000'}}).composite([{input:crop,left,top}]).png().toFile(abs(file));
 return {sourceRect:rect,sourcePivot,uniformScale:scale,placement:[left,top]};
}
(async()=>{
 const base=await layer({left:202,top:413,width:742,height:420},[573,572],'Textures/lever_base.png');
 const arm=await layer({left:1262,top:66,width:288,height:752},[1406,742],'Textures/lever_arm.png');
 const source=await sharp(abs('Source/mirror_right_generated.png')).ensureAlpha().raw().toBuffer({resolveWithObject:true});
 const b=bounds(source.data,source.info.width,source.info.height).bbox;
 const trimmed=await sharp(abs('Source/mirror_right_generated.png')).extract({left:b[0]-3,top:b[1]-3,width:b[2]-b[0]+7,height:b[3]-b[1]+7}).resize({height:420}).png().toBuffer();
 const meta=await sharp(trimmed).metadata();
 await sharp({create:{width:512,height:512,channels:4,background:'#00000000'}}).composite([{input:trimmed,left:Math.round(256-meta.width/2),top:60}]).png().toFile(abs('Textures/mirror_right.png'));
 const rig={name:'LightHungry Lever v07',type:'rigid-skeletal-2d',textureSize:[512,512],coordinates:'pixels, x right, y down; clockwise positive degrees',pixelsPerUnit:256,
 bones:[{name:'root',parent:null,position:[0,0],scale:[1,1]},{name:'handle',parent:'root',position:[256,372],length:224.28,scale:[1,1]}],
 slots:[{name:'arm',bone:'handle',texture:'Textures/lever_arm.png',texturePivot:[256,372],weight:1,drawOrder:0},{name:'base',bone:'root',texture:'Textures/lever_base.png',texturePivot:[0,0],weight:1,drawOrder:1}],
 motion:{leftDegrees:-43,rightDegrees:43,durationMs:640,easing:'6t^5-15t^4+10t^3',animatedChannels:['handle.rotation'],scaling:false,translation:false},
 unity:{baseSpritePivot:[.5,.0625],armSpritePivot:[.5,.2734375],handleLocalPosition:[0,.421875,0],angleSign:-1},
 sourceRegistration:{base,arm}};
 fs.writeFileSync(abs('Rig/lever-rig.json'),JSON.stringify(rig,null,2));
 fs.writeFileSync(abs('Rig/lever-rig.js'),'window.LEVER_DEFINITION='+JSON.stringify(rig)+';');
 const reports=[];
 for(const file of ['Textures/lever_base.png','Textures/lever_arm.png','Textures/mirror_right.png']){
  const png=fs.readFileSync(abs(file)),{data,info}=await sharp(png).ensureAlpha().raw().toBuffer({resolveWithObject:true}),bb=bounds(data,info.width,info.height);
  let edge=0;for(let i=0;i<512;i++)for(const p of [i,511*512+i,i*512,i*512+511])if(data[p*4+3])edge++;
  reports.push({file,size:[info.width,info.height],bbox:bb.bbox,alphaHistogram:bb.alphaHistogram,edgeNontransparent:edge,sha256:hash(png)});
 }
 fs.writeFileSync(abs('QA/texture-checks.json'),JSON.stringify({checks:{all512:reports.every(r=>r.size.every(v=>v===512)),transparent:reports.every(r=>r.alphaHistogram[0]>100000),noEdgeClipping:reports.every(r=>!r.edgeNontransparent)},reports},null,2));
 console.log(JSON.stringify({rig:rig.bones,mirrorBounds:b,textures:reports.map(({file,bbox,edgeNontransparent})=>({file,bbox,edgeNontransparent}))},null,2));
})().catch(e=>{console.error(e);process.exit(1)});

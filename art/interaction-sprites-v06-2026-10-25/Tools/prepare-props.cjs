const fs=require('fs'),path=require('path'),sharp=require('sharp');
const {components,bounds,montage,diff}=require('./cel-utils.cjs');const root=path.resolve(__dirname,'..');
async function prepare(name,count,cols,kind){
const file=path.join(root,'Source/'+name+'_generated.png'),whole=await sharp(file).ensureAlpha().raw().toBuffer({resolveWithObject:true}),cs=components(whole.data,whole.info.width,whole.info.height,count,cols),result=[];
fs.mkdirSync(path.join(root,'Source/Cels',name),{recursive:true});
for(let i=0;i<count;i++){
const c=cs[i],left=Math.max(0,c.x0-3),top=Math.max(0,c.y0-3),width=Math.min(whole.info.width,c.x1+4)-left,height=Math.min(whole.info.height,c.y1+4)-top;
const raw=await sharp(file).extract({left,top,width,height}).ensureAlpha().raw().toBuffer({resolveWithObject:true});
let b=bounds(raw.data,width,height),s,anchor,gray;
if(kind==='char'){s=378/(b.bbox[3]-b.bbox[1]+1);anchor=[b.footCenter,b.bbox[3]];}
else if(kind==='wheel'){
let x0=width,y0=height,x1=0,y1=0;
for(let y=0;y<height;y++)for(let x=0;x<width;x++){let p=(y*width+x)*4;let[r,g,bl,a]=raw.data.slice(p,p+4);if(a>190&&r>35&&bl>g&&bl>r&&Math.max(r,g,bl)-Math.min(r,g,bl)<70){x0=Math.min(x0,x);x1=Math.max(x1,x);y0=Math.min(y0,y);y1=Math.max(y1,y);}}
gray=[x0,y0,x1,y1];s=290/(y1-y0+1);anchor=[x0+(x1-x0)*.66,(y0+y1)/2];
}else{s=(kind==='lever'?280:218)/b.footWidth;anchor=[b.footCenter,b.bbox[3]];}
if(kind==='wheel'){
 const pivot=name==='wheel_top'?[[280,255],[721,254],[1164,253],[1605,254],[282,653],[721,654],[1166,656],[1605,657]][i]:[[150,149],[406,149],[662,149],[918,149],[1174,149],[1430,149],[150,391],[406,391],[662,391],[918,391],[1174,391],[1430,391],[150,635],[406,635],[662,635],[918,635],[1174,635],[1430,635],[150,878],[406,878],[662,878],[918,878],[1174,878],[1430,878]][i];
 s=name==='wheel_top'?290/360:290/212;anchor=[pivot[0]-left,pivot[1]-top];
}
const r=await sharp(raw.data,{raw:{width,height,channels:4}}).resize({width:Math.round(width*s)}).ensureAlpha().raw().toBuffer({resolveWithObject:true}),scale=r.info.width/width;
const dest=kind==='wheel'?[256,256]:kind==='char'?[285.5,479]:[256,479];
const x=Math.round(dest[0]-anchor[0]*scale),y=Math.round(dest[1]-anchor[1]*scale);
if(x<0||y<0||x+r.info.width>512||y+r.info.height>512)throw Error('outside '+name+i+' '+[x,y,r.info.width,r.info.height]);
const png=await sharp({create:{width:512,height:512,channels:4,background:'#00000000'}}).composite([{input:r.data,raw:r.info,left:x,top:y}]).png().toBuffer();
const out=path.join(root,'Source/Cels',name,String(i).padStart(2,'0')+'.png');fs.writeFileSync(out,png);result.push({file:path.relative(root,out).split(path.sep).join('/'),sourceCell:i,gray,scale,translate:[x,y],png});
}
await montage(result.map((r,i)=>({png:r.png,label:name+' '+i})),path.join(root,'QA/'+name+'_contact.png'),{columns:cols,tile:200});
return result.map(({png,...r})=>r);
}
(async()=>{const data={};for(const[n,c,col,k]of[['settle',8,4,'char'],['wheel',24,6,'wheel'],['wheel_top',8,4,'wheel'],['lever',16,4,'lever'],['mirror',16,4,'mirror']]){data[n]=await prepare(n,c,col,k);console.log('prepared',n);}
fs.writeFileSync(path.join(root,'QA/cel-registration.json'),JSON.stringify(data,null,2));
const old=path.join(root,'Source/V05'),target=await sharp(path.join(old,'Sprites/Idle/player_idle_000.png')).raw().toBuffer();
for(const r of data.settle){const b=await sharp(path.join(root,r.file)).raw().toBuffer();console.log(r.sourceCell,diff(b,target).foregroundMeanAbsoluteRgbDifference);}
})().catch(e=>{console.error(e);process.exit(1)});

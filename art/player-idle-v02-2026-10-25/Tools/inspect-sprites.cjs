const sharp=require('sharp');
const fs=require('node:fs');
const path=require('node:path');
const root=path.resolve(__dirname,'..');
const sources=[
['idle',process.argv[2],4,2],
['think',process.argv[3],4,3],
['reference',process.argv[4],1,1]];
function metrics(data,w,h) {
 let transparent=0,opaque=0,partial=0,x0=w,y0=h,x1=-1,y1=-1,ax0=w,ay0=h,ax1=-1,ay1=-1;
 for(let y=0;y<h;y++)for(let x=0;x<w;x++){
  const a=data[(y*w+x)*4+3];
  if(a===0)transparent++;else if(a===255)opaque++;else partial++;
  if(a>0){ax0=Math.min(ax0,x);ay0=Math.min(ay0,y);ax1=Math.max(ax1,x);ay1=Math.max(ay1,y);}
  if(a>=32){x0=Math.min(x0,x);y0=Math.min(y0,y);x1=Math.max(x1,x);y1=Math.max(y1,y);}
 }
 let fx0=w,fx1=-1;
 for(let y=Math.max(0,y1-8);y<=y1;y++)for(let x=0;x<w;x++)if(data[(y*w+x)*4+3]>=128){fx0=Math.min(fx0,x);fx1=Math.max(fx1,x);}
 return {bbox:[x0,y0,x1,y1],alphaBounds:[ax0,ay0,ax1,ay1],footCenter:(fx0+fx1)/2,footWidth:fx1-fx0+1,transparent,opaque,partial};
}
(async()=>{
const all={};
for(const [name,p,cols,rows] of sources){
 const meta=await sharp(p).metadata();const frames=[];
 for(let i=0;i<cols*rows;i++){
  const x=Math.round(i%cols*meta.width/cols),r=Math.floor(i/cols),y=Math.round(r*meta.height/rows);
  const w=Math.round((i%cols+1)*meta.width/cols)-x,h=Math.round((r+1)*meta.height/rows)-y;
  const data=await sharp(p).extract({left:x,top:y,width:w,height:h}).ensureAlpha().raw().toBuffer();
  frames.push({i,w,h,...metrics(data,w,h)});
 }
 all[name]={source:p,width:meta.width,height:meta.height,frames};
}
fs.mkdirSync(path.join(root,'QA'),{recursive:true});
fs.writeFileSync(path.join(root,'QA','source-metrics.json'),JSON.stringify(all,null,2));
console.log(JSON.stringify(all,null,2));
})();


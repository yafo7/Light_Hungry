const fs=require('fs'),path=require('path');
const sharp=require('C:/Users/10029/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp');
const base=path.resolve(__dirname,'../art/level03-continuous-layers-v04-2026-10-31');
(async()=>{
 const out=[];
 for(const f of ['References/continuous-master.png','Layers/00-background.png','Layers/10-platforms.png','Layers/20-water.png','Layers/90-camera-roots.png','Props/props-atlas.png']){
  const p=path.join(base,f), m=await sharp(p).metadata();
  const {data,info}=await sharp(p).ensureAlpha().raw().toBuffer({resolveWithObject:true});
  let zero=0,partial=0,red=0,redOpaque=0,central=0;
  for(let y=0;y<info.height;y++)for(let x=0;x<info.width;x++){
   const i=(y*info.width+x)*4,a=data[i+3];
   if(a===0)zero++;else if(a<255)partial++;
   if(data[i]>220&&data[i+1]<70&&data[i+2]<70){red++;if(a>8)redOpaque++;}
   if(x>info.width*.25&&x<info.width*.75&&y>info.height*.25&&y<info.height*.75&&a>0)central++;
  }
  const o={file:f,width:m.width,height:m.height,hasAlpha:m.hasAlpha,zeroAlpha:zero,partialAlpha:partial,redPixels:red,visibleRedPixels:redOpaque,centralNontransparent:central};
  if(f.includes('atlas')){
   o.cells=[];
   for(let row=0;row<3;row++)for(let col=0;col<3;col++){
    const x0=Math.floor(col*info.width/3),x1=Math.floor((col+1)*info.width/3),y0=Math.floor(row*info.height/3),y1=Math.floor((row+1)*info.height/3);
    let minX=x1,minY=y1,maxX=-1,maxY=-1;
    for(let y=y0;y<y1;y++)for(let x=x0;x<x1;x++)if(data[(y*info.width+x)*4+3]>8){minX=Math.min(minX,x);maxX=Math.max(maxX,x);minY=Math.min(minY,y);maxY=Math.max(maxY,y);}
    o.cells.push({row,col,sourceRect:{x:minX,y:minY,width:maxX-minX+1,height:maxY-minY+1}});
   }
  }
  if(f.includes('background')){o.topRows=[];for(let y=0;y<22;y++){let white=0;for(let x=0;x<info.width;x++){let i=(y*info.width+x)*4;if(data[i]>240&&data[i+1]>240&&data[i+2]>240)white++;}o.topRows.push(white);}o.bottomRows=[];for(let y=info.height-22;y<info.height;y++){let white=0;for(let x=0;x<info.width;x++){let i=(y*info.width+x)*4;if(data[i]>240&&data[i+1]>240&&data[i+2]>240)white++;}o.bottomRows.push(white);}}
  out.push(o);
 }
 fs.writeFileSync(path.join(base,'QA/alpha-inspection.json'),JSON.stringify(out,null,2));console.log(JSON.stringify(out,null,2));
})();

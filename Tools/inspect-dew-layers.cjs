const fs=require('fs'), path=require('path');
const sharp=require('C:/Users/10029/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp');
const root=path.resolve(__dirname,'../art/level02-dew-layers-v08-2026-10-30');
(async()=>{for(const file of fs.readdirSync(path.join(root,'Layers')).filter(f=>f.endsWith('.png'))){
 const p=path.join(root,'Layers',file), meta=await sharp(p).metadata();
 const {data,info}=await sharp(p).ensureAlpha().raw().toBuffer({resolveWithObject:true});
 let zero=0,partial=0,solid=0,x0=info.width,y0=info.height,x1=-1,y1=-1;
 for(let y=0;y<info.height;y++)for(let x=0;x<info.width;x++){let a=data[(y*info.width+x)*4+3];if(a===0)zero++;else if(a===255)solid++;else partial++;if(a>8){x0=Math.min(x0,x);y0=Math.min(y0,y);x1=Math.max(x1,x);y1=Math.max(y1,y)}}
 console.log(JSON.stringify({file,width:meta.width,height:meta.height,hasAlpha:meta.hasAlpha,zero,partial,solid,bbox:[x0,y0,x1,y1]}));
 if(file==='30-rubble.png'){
 let samples=[];for(let x=925;x<=1250;x+=25){let ys=[];for(let y=Math.round(.443*x+105);y<Math.round(.443*x+175);y++)if(data[(y*info.width+x)*4+3]<16)ys.push(y);samples.push({x,ys})}console.log(JSON.stringify({slit:samples}));
 }
}})();

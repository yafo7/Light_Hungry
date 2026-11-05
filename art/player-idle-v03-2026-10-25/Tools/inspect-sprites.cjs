// Inspect complete connected sprite silhouettes without modifying pixels.
// Usage: node inspect-sprites.cjs idle.png thinkA.png thinkB.png reference.png
const sharp=require('sharp'),fs=require('node:fs'),path=require('node:path');
const root=path.resolve(__dirname,'..');
function findSprites(data,w,h,count){
 const mask=new Uint8Array(w*h),queue=new Uint32Array(w*h),components=[];
 for(let i=0;i<w*h;i++)mask[i]=data[i*4+3]>=96?1:0;
 for(let i=0;i<mask.length;i++)if(mask[i]){
  let head=0,tail=1,n=0,x0=w,y0=h,x1=-1,y1=-1;queue[0]=i;mask[i]=0;
  while(head<tail){const p=queue[head++],x=p%w,y=Math.floor(p/w);n++;x0=Math.min(x0,x);y0=Math.min(y0,y);x1=Math.max(x1,x);y1=Math.max(y1,y);
   for(let dy=-1;dy<=1;dy++)for(let dx=-1;dx<=1;dx++){if(!dx&&!dy)continue;const xx=x+dx,yy=y+dy;if(xx<0||xx>=w||yy<0||yy>=h)continue;const q=yy*w+xx;if(mask[q]){mask[q]=0;queue[tail++]=q;}}
  }
  if(n>1000)components.push({n,x0,y0,x1,y1});
 }
 const major=components.sort((a,b)=>b.n-a.n).slice(0,count);
 if(major.length!==count||major.some(c=>c.n<15000))throw new Error('Could not identify '+count+' complete sprite silhouettes.');
 // Rows are spatially separated; order by their vertical center, then x within each row.
 major.sort((a,b)=>(a.y0+a.y1)-(b.y0+b.y1));const ordered=[];
 for(let start=0;start<count;start+=4)ordered.push(...major.slice(start,start+4).sort((a,b)=>a.x0-b.x0));
 return ordered;
}
function measure(data,w,h) {
 const histogram=Array(256).fill(0);let x0=w,y0=h,x1=-1,y1=-1,ax0=w,ay0=h,ax1=-1,ay1=-1;
 for(let y=0;y<h;y++)for(let x=0;x<w;x++){const a=data[(y*w+x)*4+3];histogram[a]++;
  if(a>0){ax0=Math.min(ax0,x);ay0=Math.min(ay0,y);ax1=Math.max(ax1,x);ay1=Math.max(ay1,y);}
  if(a>=32){x0=Math.min(x0,x);y0=Math.min(y0,y);x1=Math.max(x1,x);y1=Math.max(y1,y);}
 }
 let fx0=w,fx1=-1;
 for(let y=Math.max(0,y1-8);y<=y1;y++)for(let x=0;x<w;x++)if(data[(y*w+x)*4+3]>=128){fx0=Math.min(fx0,x);fx1=Math.max(fx1,x);}
 return {bbox:[x0,y0,x1,y1],alphaBounds:[ax0,ay0,ax1,ay1],footCenter:(fx0+fx1)/2,footWidth:fx1-fx0+1,alphaHistogram:histogram};
}
(async()=>{
 const keys=['idle','thinkA','thinkB','reference'];const paths=process.argv.slice(2,6);
 if(paths.length!==4)throw new Error('Expected idle, thinkA, thinkB and reference file paths.');
 const all={};fs.mkdirSync(path.join(root,'Source'),{recursive:true});fs.mkdirSync(path.join(root,'QA'),{recursive:true});
 for(let k=0;k<keys.length;k++){
  const name=keys[k],src=paths[k],cols=name==='reference'?1:4,rows=cols;
  const fileName=name==='reference'?'original_player_idle_00.png':name+'_generated.png';
  const copied=path.join(root,'Source',fileName);
  if(path.resolve(src)!==path.resolve(copied))fs.copyFileSync(src,copied);
  const meta=await sharp(copied).metadata(),frames=[];
  const whole=await sharp(copied).ensureAlpha().raw().toBuffer();
  const components=name==='reference'?[{x0:0,y0:0,x1:meta.width-1,y1:meta.height-1,n:0}]:findSprites(whole,meta.width,meta.height,cols*rows);
  for(let i=0;i<components.length;i++){
   const component=components[i],x=Math.max(0,component.x0-5),y=Math.max(0,component.y0-5);
   const w=Math.min(meta.width,component.x1+6)-x,h=Math.min(meta.height,component.y1+6)-y;
   const data=await sharp(copied).extract({left:x,top:y,width:w,height:h}).ensureAlpha().raw().toBuffer();
   frames.push({i,x,y,w,h,component,...measure(data,w,h)});
  }
  all[name]={source:'Source/'+fileName,originalSource:src,width:meta.width,height:meta.height,cols,rows,extraction:'Complete connected silhouette, sorted by row then column; no nominal grid clipping.',frames};
 }
 fs.writeFileSync(path.join(root,'QA/source-metrics.json'),JSON.stringify(all,null,2));
 console.log(JSON.stringify(Object.fromEntries(Object.entries(all).map(([name,s])=>[name,{dimensions:[s.width,s.height],frames:s.frames.map(f=>({i:f.i,bbox:f.bbox,footCenter:f.footCenter,footWidth:f.footWidth,transparent:f.alphaHistogram[0]}))}])),null,2));
})().catch(e=>{console.error(e);process.exit(1)});


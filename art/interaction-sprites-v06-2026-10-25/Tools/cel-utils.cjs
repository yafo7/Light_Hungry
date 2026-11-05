const sharp=require('sharp'),crypto=require('node:crypto');
const hash=b=>crypto.createHash('sha256').update(b).digest('hex');
const sum=a=>a.reduce((x,y)=>x+y,0);
function components(data,w,h,count,cols){
 const mask=new Uint8Array(w*h),queue=new Uint32Array(w*h),out=[];
 for(let i=0;i<w*h;i++)mask[i]=data[i*4+3]>=96?1:0;
 for(let i=0;i<mask.length;i++)if(mask[i]){
  let head=0,tail=1,n=0,x0=w,y0=h,x1=-1,y1=-1;queue[0]=i;mask[i]=0;
  while(head<tail){const p=queue[head++],x=p%w,y=Math.floor(p/w);n++;x0=Math.min(x0,x);y0=Math.min(y0,y);x1=Math.max(x1,x);y1=Math.max(y1,y);
   for(let dy=-1;dy<=1;dy++)for(let dx=-1;dx<=1;dx++){const xx=x+dx,yy=y+dy;if(xx<0||xx>=w||yy<0||yy>=h)continue;const q=yy*w+xx;if(mask[q]){mask[q]=0;queue[tail++]=q;}}
  }if(n>1000)out.push({n,x0,y0,x1,y1});
 }
 const major=out.sort((a,b)=>b.n-a.n).slice(0,count);
 if(major.length!==count||major.some(c=>c.n<10000))throw new Error('Expected '+count+' complete character silhouettes.');
 major.sort((a,b)=>(a.y0+a.y1)-(b.y0+b.y1));
 const ordered=[];for(let i=0;i<count;i+=cols)ordered.push(...major.slice(i,i+cols).sort((a,b)=>a.x0-b.x0));return ordered;
}
function bounds(data,w,h){
 const hist=Array(256).fill(0);let x0=w,y0=h,x1=-1,y1=-1;
 for(let y=0;y<h;y++)for(let x=0;x<w;x++){const a=data[(y*w+x)*4+3];hist[a]++;if(a>=32){x0=Math.min(x0,x);y0=Math.min(y0,y);x1=Math.max(x1,x);y1=Math.max(y1,y);}}
 let fx0=w,fx1=-1;for(let y=Math.max(0,y1-8);y<=y1;y++)for(let x=0;x<w;x++)if(data[(y*w+x)*4+3]>=128){fx0=Math.min(fx0,x);fx1=Math.max(fx1,x);}
 return {bbox:[x0,y0,x1,y1],alphaHistogram:hist,footCenter:(fx0+fx1)/2,footWidth:fx1-fx0+1};
}
const frameFile=(clip,i)=>'Sprites/'+(clip==='idle'?'Idle':'Think')+'/player_'+clip+'_'+String(i).padStart(3,'0')+'.png';
function diff(a,b){
 let abs=0,foregroundAbs=0,union=0,intersection=0,changed=0;
 for(let i=0;i<a.length;i+=4){
  const aa=a[i+3]/255,ba=b[i+3]/255,fg=aa>.125||ba>.125;
  if(fg)union++;if(aa>.125&&ba>.125)intersection++;
  let delta=0;
  for(let c=0;c<3;c++){const bg=[35,55,70][c],d=Math.abs(a[i+c]*aa+bg*(1-aa)-b[i+c]*ba-bg*(1-ba));abs+=d;delta+=d;if(fg)foregroundAbs+=d;}
  if(delta/3>20)changed++;
 }
 return {meanAbsoluteRgbDifference:abs/(a.length/4*3),foregroundMeanAbsoluteRgbDifference:foregroundAbs/Math.max(1,union*3),alphaIntersectionOverUnion:intersection/Math.max(1,union),changedPixelFraction:changed/(a.length/4)};
}
function quantize(delays,step){let t=0,last=0;return delays.map(d=>{t+=d;const next=Math.round(t/step)*step,out=next-last;last=next;return out;});}
async function montage(items,file,{columns=6,tile=256,caption=34,background='#233746'}={}){
 const inputs=[];for(let i=0;i<items.length;i++){
  const x=i%columns*tile,y=Math.floor(i/columns)*(tile+caption);
  const sprite=await sharp(items[i].png).resize(tile,tile).png().toBuffer();
  const label=Buffer.from('<svg width="'+tile+'" height="'+caption+'"><text x="10" y="22" fill="#e6edf0" font-size="14" font-family="Arial">'+String(items[i].label).replace(/&/g,'&amp;').replace(/</g,'&lt;')+'</text></svg>');
  inputs.push({input:sprite,left:x,top:y+caption},{input:label,left:x,top:y});
 }
 await sharp({create:{width:columns*tile,height:Math.ceil(items.length/columns)*(tile+caption),channels:4,background}}).composite(inputs).png().toFile(file);
}
module.exports={hash,sum,components,bounds,frameFile,diff,quantize,montage};


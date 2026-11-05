const fs=require('node:fs'),path=require('node:path'),sharp=require('sharp'),{chromium}=require('playwright');
const root=path.resolve(__dirname,'..');
const source=process.argv[2]||'C:/Users/10029/.codex/generated_images/01a171d5-ad4b-7063-81ea-324c6e8c91ba/exec-bd6c6bd8-8be9-4f75-b1c0-cd6bb9730c8b.png';
function components(data,w,h){
 const mask=new Uint8Array(w*h),queue=new Uint32Array(w*h),out=[];
 for(let i=0;i<w*h;i++)mask[i]=data[i*4+3]>=64?1:0;
 for(let i=0;i<mask.length;i++)if(mask[i]){
  let head=0,tail=1,n=0,x0=w,y0=h,x1=0,y1=0;queue[0]=i;mask[i]=0;
  while(head<tail){const p=queue[head++],x=p%w,y=Math.floor(p/w);n++;x0=Math.min(x0,x);y0=Math.min(y0,y);x1=Math.max(x1,x);y1=Math.max(y1,y);
   for(let dy=-1;dy<=1;dy++)for(let dx=-1;dx<=1;dx++){const xx=x+dx,yy=y+dy;if(xx<0||xx>=w||yy<0||yy>=h)continue;const q=yy*w+xx;if(mask[q]){mask[q]=0;queue[tail++]=q;}}
  }
  if(n>100)out.push({n,x0,y0,x1,y1});
 }
 return out.sort((a,b)=>b.n-a.n);
}
(async()=>{
 for(const d of ['Source','Rig','QA','Tools'])fs.mkdirSync(path.join(root,d),{recursive:true});
 fs.copyFileSync(source,path.join(root,'Source/rig_components_generated.png'));
 const {data,info}=await sharp(source).ensureAlpha().raw().toBuffer({resolveWithObject:true});
 const comps=components(data,info.width,info.height);
 const main=comps.slice(0,2).sort((a,b)=>a.x0-b.x0),small=comps.filter(c=>c.y0>info.height*.55).sort((a,b)=>b.n-a.n).slice(0,2).sort((a,b)=>a.x0-b.x0);
 if(main.length!==2||small.length!==2)throw new Error('Expected body, head, eye and closed line');
 const extracted={};
 for(const [name,c] of [['body',main[0]],['head',main[1]],['eye',small[0]],['closed',small[1]]]){
  const x=Math.max(0,c.x0-4),y=Math.max(0,c.y0-4),w=Math.min(info.width,c.x1+5)-x,h=Math.min(info.height,c.y1+5)-y;
  const file=path.join(root,'Rig',name+'_raw.png');
  await sharp(source).extract({left:x,top:y,width:w,height:h}).png().toFile(file);
  extracted[name]={file:'Rig/'+name+'_raw.png',sourceRect:[x,y,w,h],width:w,height:h,component:c};
 }
 const body=extracted.body,head=extracted.head,eye=extracted.eye,closed=extracted.closed;
 const bodyW=360,headW=344;
 const bodyH=body.height/body.width*bodyW,headH=head.height/head.width*headW;
 const config={frameSize:[512,512],renderScale:2,footReferenceY:480,pivot:[285,285],
  body:{file:body.file,x:76,y:480-bodyH,width:bodyW,height:bodyH,scale:bodyW/body.width},
  head:{file:'Rig/head_feathered.png',rawFile:head.file,x:91,y:96,width:headW,height:headH,scale:headW/head.width,feather:'Fixed diagonal alpha ramp in source layer. No animated mask changes.'},
  eye:{file:eye.file,center:[311,215],width:34,height:34*eye.height/eye.width},
  closed:{file:closed.file,center:[311,217],width:34,height:34*closed.height/closed.width},extracted};
 const hr=await sharp(path.join(root,head.file)).ensureAlpha().raw().toBuffer({resolveWithObject:true});
 for(let y=0;y<hr.info.height;y++)for(let x=0;x<hr.info.width;x++){
  const wx=config.head.x+x*config.head.scale,wy=config.head.y+y*config.head.scale;
  const k=Math.max(0,Math.min(1,(wx-160)/250));const start=235+80*k,end=start+55;
  const t=Math.max(0,Math.min(1,(wy-start)/(end-start)));const opacity=1-t*t*(3-2*t);
  hr.data[(y*hr.info.width+x)*4+3]=Math.round(hr.data[(y*hr.info.width+x)*4+3]*opacity);
 }
 await sharp(hr.data,{raw:{width:hr.info.width,height:hr.info.height,channels:4}}).png().toFile(path.join(root,config.head.file));
 fs.writeFileSync(path.join(root,'Rig/rig.json'),JSON.stringify(config,null,2));
 const assets=Object.fromEntries(['body','head','eye','closed'].map(k=>[k,'data:image/png;base64,'+fs.readFileSync(path.join(root,config[k].file)).toString('base64')]));
 const browser=await chromium.launch({headless:true,channel:'chrome'}),page=await browser.newPage();
 const results=await page.evaluate(async({config,assets})=>{
  const imgs={};await Promise.all(Object.entries(assets).map(async([k,src])=>{const i=new Image();i.src=src;await i.decode();imgs[k]=i;}));
  const result={};
  for(const [name,deg] of [['neutral',0],['left',-5],['right',5]]){
   const c=document.createElement('canvas');c.width=c.height=1024;const ctx=c.getContext('2d');ctx.scale(2,2);ctx.imageSmoothingQuality='high';
   const b=config.body;ctx.drawImage(imgs.body,b.x,b.y,b.width,b.height);
   ctx.translate(...config.pivot);ctx.rotate(deg*Math.PI/180);ctx.translate(-config.pivot[0],-config.pivot[1]);
   const h=config.head;ctx.drawImage(imgs.head,h.x,h.y,h.width,h.height);
   const e=config.eye;ctx.drawImage(imgs.eye,e.center[0]-e.width/2,e.center[1]-e.height/2,e.width,e.height);
   result[name]=c.toDataURL('image/png').split(',')[1];
  }return result;
 },{config,assets});
 await browser.close();
 for(const [name,b64] of Object.entries(results))await sharp(Buffer.from(b64,'base64')).resize(512,512).png().toFile(path.join(root,'QA/rig_'+name+'.png'));
 const compsOut=[];for(const [i,name] of ['left','neutral','right'].entries())compsOut.push({input:path.join(root,'QA/rig_'+name+'.png'),left:i*512,top:0});
 await sharp({create:{width:1536,height:512,channels:4,background:'#243440'}}).composite(compsOut).png().toFile(path.join(root,'QA/rig_assembly_preview.png'));
 console.log(JSON.stringify(config,null,2));
})().catch(e=>{console.error(e);process.exit(1)});


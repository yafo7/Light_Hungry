const fs=require('node:fs'),path=require('node:path'),sharp=require('sharp');
(async()=>{
 const src=process.argv[2],tag=process.argv[3]||'a',out=path.resolve(__dirname,'..','Source','IdleRefs');
 fs.mkdirSync(out,{recursive:true});
 const m=await sharp(src).metadata(),frames=[];
 for(let i=0;i<12;i++){
  const l=Math.round(i%4*m.width/4),t=Math.round(Math.floor(i/4)*m.height/3),r=Math.round((i%4+1)*m.width/4),b=Math.round((Math.floor(i/4)+1)*m.height/3);
  const buf=await sharp(src).extract({left:l,top:t,width:r-l,height:b-t}).ensureAlpha().png().toBuffer();
  const raw=await sharp(buf).raw().toBuffer();let x0=r-l,y0=b-t,x1=-1,y1=-1,alpha0=0,edge=0;
  for(let y=0;y<b-t;y++)for(let x=0;x<r-l;x++){const a=raw[(y*(r-l)+x)*4+3];if(a===0)alpha0++;if(a>=32){x0=Math.min(x0,x);y0=Math.min(y0,y);x1=Math.max(x1,x);y1=Math.max(y1,y);if(x===0||y===0||x===r-l-1||y===b-t-1)edge++;}}
  if(i===0||i>=9)fs.writeFileSync(path.join(out,'idle_'+tag+'_'+String(i).padStart(2,'0')+'.png'),buf);
  frames.push({i,bounds:[x0,y0,x1,y1],visibleSize:[x1-x0+1,y1-y0+1],transparentPixels:alpha0,edgePixels:edge});
 }
 const report={source:src,width:m.width,height:m.height,hasAlpha:m.hasAlpha,columns:4,rows:3,frames};
 fs.writeFileSync(path.join(out,'idle_'+tag+'_metrics.json'),JSON.stringify(report,null,2));console.log(JSON.stringify(report,null,2));
})();

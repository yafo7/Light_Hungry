const sharp = require('C:/Users/10029/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp');
const fs = require('fs');
(async()=>{
 const path='art/level02-loose-rubble-v10-2026-10-30/30-rubble.png';
 const meta=await sharp(path).metadata();
 const {data,info}=await sharp(path).ensureAlpha().raw().toBuffer({resolveWithObject:true});
 const a=(x,y)=>data[(Math.round(y)*info.width+Math.round(x))*4+3];
 let box=[info.width,info.height,0,0],zero=0;
 for(let y=0;y<info.height;y++)for(let x=0;x<info.width;x++){if(a(x,y)===0)zero++;if(a(x,y)>128){box[0]=Math.min(box[0],x);box[1]=Math.min(box[1],y);box[2]=Math.max(box[2],x);box[3]=Math.max(box[3],y);}}
 const slit=[];
 for(let x=940;x<=1230;x+=20){let start=null;const runs=[];for(let y=Math.floor(.49*x+55);y<.49*x+140;y++){if(a(x,y)<20){if(start===null)start=y;}else if(start!==null){runs.push([start,y-1]);start=null;}}if(start!==null)runs.push([start,Math.floor(.49*x+139)]);slit.push({x,runs});}
 let best={clearance:-100};
 for(let slope=.48;slope<.57;slope+=.001)for(let intercept=15;intercept<115;intercept+=.5){let clearance=30;for(let x=965;x<=1230;x+=2){const y=slope*x+intercept;let radius=0;while(radius<20&&a(x,y-radius)<128&&a(x,y+radius)<128)radius++;clearance=Math.min(clearance,radius);if(clearance<=best.clearance)break;}if(clearance>best.clearance)best={slope,intercept,clearance};}
 const result={width:meta.width,height:meta.height,hasAlpha:meta.hasAlpha,transparentPixels:zero,bbox:box,slit,best};
 fs.writeFileSync('art/level02-loose-rubble-v10-2026-10-30/alpha-check.json',JSON.stringify(result,null,2));console.log(JSON.stringify(result));
})();

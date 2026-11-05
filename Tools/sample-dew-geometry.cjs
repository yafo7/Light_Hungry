const sharp=require('C:/Users/10029/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp');
const path=require('path');
(async()=>{for(const f of ['10-ground.png','20-leaf-dew.png','30-rubble.png']){
const {data,info}=await sharp(path.join(__dirname,'../art/level02-dew-layers-v08-2026-10-30/Layers',f)).ensureAlpha().raw().toBuffer({resolveWithObject:true});let out=[];
for(const x of [0,260,550,575,800,940,1000,1040,1080,1120,1200,1280,1368,1450,1500,1600,1700]){let y=0;while(y<info.height&&data[(y*info.width+x)*4+3]<128)y++;out.push([x,y])}console.log(f,JSON.stringify(out));
}})();

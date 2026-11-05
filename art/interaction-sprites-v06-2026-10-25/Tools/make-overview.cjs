const fs=require('fs'),path=require('path'),sharp=require('sharp');
const root=path.resolve(__dirname,'..'),M=JSON.parse(fs.readFileSync(path.join(root,'animations.json')));
function at(id,t){const c=M.clips[id];let p=Math.min(t,c.durationMs-.001);for(let i=0;i<c.frameCount;i++){if(p<c.frameDurationMs[i])return c.frames[i];p-=c.frameDurationMs[i]}return c.frames.at(-1);}
function transition(t,a,b,hold){const d=M.clips[a].durationMs,cycle=(d+hold)*2;t%=cycle;return t<d?at(a,t):t<d+hold?at(a,d):t<d*2+hold?at(b,t-d-hold):at(b,d);}
(async()=>{const cache=new Map(),raw=[],durations=[];async function small(f){if(!cache.has(f))cache.set(f,await sharp(path.join(root,f)).resize(300,300).png().toBuffer());return cache.get(f)}
const title=Buffer.from('<svg width="960" height="355"><g font-family="Arial" font-weight="600" font-size="18" fill="#eccb91"><text x="24" y="32">WHEEL</text><text x="344" y="32">LEVER</text><text x="664" y="32">MIRROR</text></g></svg>');
for(let t=0;t<1800;t+=20){const files=[at('wheel_cw',t%900),transition(t,'lever_on','lever_off',340),transition(t,'mirror_left','mirror_right',100)];
raw.push(await sharp({create:{width:960,height:355,channels:4,background:'#182a36'}}).composite([{input:title,left:0,top:0},...await Promise.all(files.map(async(f,i)=>({input:await small(f),left:i*320+10,top:48}))) ]).removeAlpha().raw().toBuffer());durations.push(20);}
await sharp(Buffer.concat(raw),{raw:{width:960,height:355*raw.length,channels:3,pageHeight:355}}).webp({lossless:true,loop:0,delay:durations,effort:3}).toFile(path.join(root,'Previews/props_overview.webp'));
const m=await sharp(path.join(root,'Previews/props_overview.webp'),{animated:true}).metadata();console.log({frames:m.pages,duration:m.delay.reduce((a,b)=>a+b,0)});
})().catch(e=>{console.error(e);process.exit(1)});

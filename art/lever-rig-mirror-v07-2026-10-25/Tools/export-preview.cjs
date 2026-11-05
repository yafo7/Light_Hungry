const fs=require('fs'),path=require('path'),sharp=require('sharp'),{chromium}=require('playwright');
const root=path.resolve(__dirname,'..'),abs=f=>path.join(root,f);
(async()=>{const browser=await chromium.launch({headless:true,channel:'chrome'});try{
 const page=await browser.newPage();await page.setContent('<canvas id="c" width="512" height="512"></canvas>');
 await page.addScriptTag({content:fs.readFileSync(abs('Rig/lever-runtime.js'),'utf8')});
 const definition=JSON.parse(fs.readFileSync(abs('Rig/lever-rig.json'),'utf8'));
 const images=Object.fromEntries(['arm','base'].map(k=>[k,'data:image/png;base64,'+fs.readFileSync(abs('Textures/lever_'+k+'.png')).toString('base64')]));
 await page.evaluate(async({definition,images})=>{window.rig=new LeverRig(definition);window.textures={};for(const[k,src]of Object.entries(images)){const im=new Image();im.src=src;await im.decode();textures[k]=im}}, {definition,images});
 async function render(angle){return Buffer.from(await page.evaluate(a=>{rig.seek(a);rig.draw(document.querySelector('canvas').getContext('2d'),textures);return document.querySelector('canvas').toDataURL('image/png').split(',')[1]},angle),'base64')}
 const poses=[];for(const[name,angle]of [['left',-43],['upright',0],['right',43]]){const png=await render(angle);fs.writeFileSync(abs('Previews/lever_'+name+'.png'),png);poses.push(png)}
 const composite=[];for(let i=0;i<poses.length;i++)composite.push({input:await sharp(poses[i]).resize(320).png().toBuffer(),left:i*320,top:0});
 await sharp({create:{width:960,height:320,channels:4,background:'#203640'}}).composite(composite).png().toFile(abs('QA/lever-three-poses.png'));
 const frames=[];for(let ms=0;ms<1920;ms+=20){let t=ms%960,forward=ms<960,p=Math.min(1,t/640);p=p*p*p*(p*(p*6-15)+10);const angle=forward?-43+86*p:43-86*p;
 frames.push(await sharp(await render(angle)).resize(320).flatten({background:'#203640'}).removeAlpha().raw().toBuffer());}
 await sharp(Buffer.concat(frames),{raw:{width:320,height:320*frames.length,channels:3,pageHeight:320}}).webp({lossless:true,loop:0,delay:Array(frames.length).fill(20),effort:3}).toFile(abs('Previews/lever_rig.webp'));
 const mirror=await sharp(abs('Textures/mirror_right.png')).resize(320).png().toBuffer(),overview=[];
 for(const frame of frames){overview.push(await sharp({create:{width:640,height:320,channels:3,background:'#203640'}}).composite([{input:frame,raw:{width:320,height:320,channels:3},left:0,top:0},{input:mirror,left:320,top:0}]).removeAlpha().raw().toBuffer());}
 await sharp(Buffer.concat(overview),{raw:{width:640,height:320*overview.length,channels:3,pageHeight:320}}).webp({lossless:true,loop:0,delay:Array(overview.length).fill(20),effort:3}).toFile(abs('Previews/overview.webp'));
 console.log(JSON.stringify({previewFrames:frames.length,durationMs:1920,textureCount:2}));
}finally{await browser.close()}})().catch(e=>{console.error(e);process.exit(1)});

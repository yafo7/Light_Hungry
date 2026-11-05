const fs=require('fs'),path=require('path'),{pathToFileURL}=require('url'),{chromium}=require('playwright'),sharp=require('sharp');
const P=path.resolve(__dirname,'..');
(async()=>{const browser=await chromium.launch({headless:true,channel:'chrome',args:['--enable-webgl','--use-angle=swiftshader']});try{const page=await browser.newPage({viewport:{width:1080,height:860}}),errors=[];page.on('pageerror',e=>errors.push(e.message));await page.goto(pathToFileURL(path.join(P,'Previews/index.html')).href);await page.waitForSelector('body[data-ready=true]',{timeout:60000});
const clips=await page.evaluate(()=>study.clips.map(c=>({name:c.name,duration:c.duration,tracks:c.tracks.length})));const strips=[];let changing=0;
for(let i=0;i<9;i++){const tiles=[],hashes=[];await page.evaluate(i=>{study.pick(i);study.pause()},i);
for(let k=0;k<6;k++){await page.evaluate(({i,k})=>{study.group.rotation.y=k>=3?Math.PI/2:0;study.seek(study.clips[i].duration*[.15,.45,.75][k%3])},{i,k});const data=await page.locator('canvas').screenshot();const png=await sharp(data).resize(230,220,{fit:'cover'}).png().toBuffer();tiles.push({input:png,left:k*230,top:0});hashes.push(require('crypto').createHash('sha256').update(data).digest('hex'));}
if(new Set(hashes.slice(0,3)).size===3)changing++;const strip=await sharp({create:{width:1380,height:220,channels:3,background:'#213846'}}).composite(tiles).png().toBuffer();fs.writeFileSync(path.join(P,'QA',clips[i].name+'-poses.png'),strip);strips.push({input:strip,left:0,top:i*220});
console.log('VISUAL',clips[i].name);}
await sharp({create:{width:1380,height:1980,channels:3,background:'#213846'}}).composite(strips).png().toFile(path.join(P,'QA/all-nine-poses.png'));
await page.evaluate(()=>{study.pick(6);study.pause();study.group.rotation.y=0;study.seek(2)});await page.screenshot({path:path.join(P,'QA/preview-desktop.png'),fullPage:true});
const report={clips,checks:{nineClips:clips.length===9,allAnimated:changing===9,allHaveTracks:clips.every(c=>c.tracks>20),noBrowserErrors:!errors.length},errors};fs.writeFileSync(path.join(P,'QA/browser-checks.json'),JSON.stringify(report,null,2));console.log(JSON.stringify(report));
}finally{await browser.close()}})().catch(e=>{console.error(e);process.exit(1)});

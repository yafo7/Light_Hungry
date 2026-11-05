const {chromium}=require('playwright');
const path=require('node:path'),fs=require('node:fs'),{pathToFileURL}=require('node:url');
const root=path.resolve(__dirname,'..');
(async()=>{
const manifest=JSON.parse(fs.readFileSync(path.join(root,'animations.json'),'utf8'));
const threshold=manifest.playback.idleThresholdSeconds*1000;
const qa=path.join(root,'QA');fs.mkdirSync(qa,{recursive:true});
const browser=await chromium.launch({headless:true,channel:'chrome'});
try{
 const page=await browser.newPage({viewport:{width:1220,height:1000},deviceScaleFactor:1});
 const errors=[];page.on('pageerror',e=>errors.push(e.message));page.on('console',m=>{if(m.type()==='error')errors.push(m.text());});
 await page.goto(pathToFileURL(path.join(root,'Previews/index.html')).href);
 await page.waitForSelector('body[data-ready="true"]');
 await page.click('#reset');
 await page.waitForTimeout(Math.max(0,threshold-250));
 const before=await page.locator('#view').getAttribute('data-clip');
 await page.waitForFunction(()=>document.querySelector('#view').dataset.clip==='think',null,{timeout:2000});
 const transitionClock=await page.locator('#clock').innerText();
 await page.click('#play');const paused=await page.locator('#view').getAttribute('data-frame');
 await page.waitForTimeout(200);
 const stable=(await page.locator('#view').getAttribute('data-frame'))===paused;
 const phases=manifest.clips.think.phases??[];
 const hold=phases.find(p=>/停|保持|hold/i.test(p.name))??phases.find(p=>/左偏/i.test(p.name))??phases[0];
 if(hold)await page.locator('#phasebar button').filter({hasText:hold.name}).first().click();
 else await page.locator('#frames button').nth(Math.min(8,manifest.clips.think.frames.length-1)).click();
 await page.screenshot({path:path.join(qa,'preview-desktop.png'),fullPage:true});
 await page.selectOption('#background','light');
 await page.screenshot({path:path.join(qa,'preview-light.png'),fullPage:true});
 const displayedCounts=await page.evaluate(()=>({idle:document.querySelector('#idleMeta').textContent,think:document.querySelector('#thinkMeta').textContent}));
 const phaseCount=await page.locator('#phasebar button').count();
 const thumbnailCount=await page.locator('#frames button').count();
 await page.click('#reset');const reset=await page.locator('#view').getAttribute('data-clip');
 await page.click('[data-mode="think"]');await page.click('#play');await page.click('#next');
 const manual=await page.locator('#view').getAttribute('data-clip');
 const manualFrame=await page.locator('#view').getAttribute('data-frame');
 await page.selectOption('#background','checker');await page.setViewportSize({width:390,height:844});
 await page.screenshot({path:path.join(qa,'preview-mobile.png'),fullPage:true});
 const noOverflow=await page.evaluate(()=>document.documentElement.scrollWidth<=innerWidth);
 const checks={
  imagesLoaded:await page.evaluate(()=>Object.values(images).flat().every(i=>i.complete&&i.naturalWidth===canvas.width&&i.naturalHeight===canvas.height)),
  idleBeforeThreshold:before==='idle',
  thinkAfterThreshold:Number.parseFloat(transitionClock)>=threshold/1000,
  pauseStable:stable,actionResetsIdle:reset==='idle',
  manualThinking:manual==='think'&&Number(manualFrame)>=1,
  allThinkFramesShown:thumbnailCount===manifest.clips.think.frames.length,
  allPhasesShown:phaseCount===phases.length,
  dynamicFrameCounts:displayedCounts.idle.startsWith(manifest.clips.idle.frames.length+' 帧')&&displayedCounts.think.startsWith(manifest.clips.think.frames.length+' 帧'),
  noMobilePageOverflow:noOverflow,noScriptErrors:errors.length===0
 };
 const result={checks,transitionClock,displayedCounts,errors};
 fs.writeFileSync(path.join(qa,'preview-checks.json'),JSON.stringify(result,null,2));console.log(JSON.stringify(result,null,2));
 if(Object.values(checks).some(v=>!v))process.exitCode=1;
}finally{await browser.close();}
})().catch(e=>{console.error(e);process.exit(1)});


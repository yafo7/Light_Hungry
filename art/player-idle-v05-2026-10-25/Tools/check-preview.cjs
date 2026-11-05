const {chromium}=require('playwright');
const path=require('node:path'),fs=require('node:fs'),{pathToFileURL}=require('node:url');
const root=path.resolve(__dirname,'..');
(async()=>{
 const manifest=JSON.parse(fs.readFileSync(path.join(root,'animations.json'),'utf8'));
 const threshold=manifest.playback.idleThresholdSeconds*1000,qa=path.join(root,'QA');
 fs.mkdirSync(qa,{recursive:true});
 const browser=await chromium.launch({headless:true,channel:'chrome'});
 try{
  const page=await browser.newPage({viewport:{width:1220,height:1000},deviceScaleFactor:1});
  const errors=[];page.on('pageerror',e=>errors.push(e.message));page.on('console',m=>{if(m.type()==='error')errors.push(m.text());});
  await page.goto(pathToFileURL(path.join(root,'Previews/index.html')).href);
  await page.waitForSelector('body[data-ready="true"]',{timeout:30000});
  const thumbnailCountCollapsed=await page.locator('#frames button').count();
  await page.click('#reset');await page.waitForTimeout(Math.max(0,threshold-250));
  const before=await page.locator('#view').getAttribute('data-clip');
  await page.waitForFunction(()=>document.querySelector('#view').dataset.clip==='think',null,{timeout:2000});
  const transitionClock=await page.evaluate(()=>elapsed/1000);
  await page.click('#play');const paused=await page.locator('#view').getAttribute('data-frame');
  await page.waitForTimeout(200);
  const pauseStable=(await page.locator('#view').getAttribute('data-frame'))===paused;
  const phases=manifest.clips.think.phases??[],hold=phases.find(p=>/停|保持|hold/i.test(p.name))??phases[0];
  if(hold)await page.locator('#phasebar button').filter({hasText:hold.name}).first().click();
  const phaseLocated=hold?await page.evaluate(({start,end})=>clipTime>=start&&clipTime<end,{start:hold.startMs,end:hold.endMs}):true;
  await page.screenshot({path:path.join(qa,'preview-desktop.png'),fullPage:true});
  await page.selectOption('#background','light');await page.screenshot({path:path.join(qa,'preview-light.png'),fullPage:true});
  await page.locator('#framePanel summary').click();
  await page.waitForFunction(()=>document.querySelector('#frames').children.length>0);
  const visibleThumbs=await page.locator('#frames button').count(),buildsBefore=await page.evaluate(()=>stats.thumbnailBuilds);
  await page.click('#pageNext');const visibleAfterPage=await page.locator('#frames button').count();
  const pagedFrame=Number(await page.locator('#frames button').first().getAttribute('data-index'));
  await page.locator('#frames button').nth(1).click();const clickedFrame=Number(await page.locator('#view').getAttribute('data-frame'));
  await page.click('#locate');
  const selectedCount=await page.locator('#frames button.selected').count();
  await page.screenshot({path:path.join(qa,'preview-frames.png'),fullPage:true});
  await page.locator('#framePanel summary').click();
  await page.click('[data-mode="think"]');
  const sample=await page.evaluate(()=>new Promise(resolve=>{
   const started=performance.now(),d0=stats.drawCalls,t0=stats.thumbnailBuilds,startTime=clipTime;
   const duration=Math.min(8000,clips.think.duration+200);
   const frameEvents=[],sampleFrame=()=>{const now=performance.now();frameEvents.push({at:now-started,frame:Number(canvas.dataset.frame),clipMs:clipTime});
    if(now-started>=duration){resolve({wallMs:now-started,startTime,endTime:clipTime,drawCalls:stats.drawCalls-d0,thumbnailBuilds:stats.thumbnailBuilds-t0,renderMsMax:stats.renderMsMax,frameEvents});}
    else requestAnimationFrame(sampleFrame);
   };requestAnimationFrame(sampleFrame);
  }));
  const visibleTransitions=sample.frameEvents.filter((e,i,a)=>i===0||e.frame!==a[i-1].frame);
  const think=manifest.clips.think,count=think.frames?.length??think.frameCount;
  const delay=think.frameDurationMs??Array(count).fill(1000/(think.sampleRate??manifest.sampleRate));
  const starts=[0];for(const ms of delay)starts.push(starts.at(-1)+ms);const duration=starts.at(-1);
  let expectedTransitions=0;
  for(let loop=Math.floor(sample.startTime/duration);loop<=Math.floor(sample.endTime/duration);loop++)for(const start of starts.slice(0,-1)){
   const boundary=loop*duration+start;
   if(boundary>sample.startTime+0.00001&&boundary<=sample.endTime+0.00001)expectedTransitions++;
  }
  const measuredPaintRate=sample.drawCalls/(sample.wallMs/1000),cadenceCountError=Math.abs(sample.drawCalls-expectedTransitions);
  const intervals=[];
  for(let i=2;i<visibleTransitions.length;i++){
   const previous=visibleTransitions[i-1],current=visibleTransitions[i];
   if(current.frame===(previous.frame+1)%count)intervals.push(Math.abs((current.at-previous.at)-delay[previous.frame]));
  }
  const meanIntervalErrorMs=intervals.length?intervals.reduce((a,b)=>a+b,0)/intervals.length:0;
  await page.click('#play');
  await page.selectOption('#speed','.5');await page.click('#play');
  const slowStart=await page.evaluate(()=>elapsed);await page.waitForTimeout(500);await page.click('#play');
  const slowDelta=await page.evaluate(()=>elapsed)-slowStart;
  await page.selectOption('#speed','1');
  await page.click('[data-mode="auto"]');await page.click('#reset');
  const reset=await page.locator('#view').getAttribute('data-clip');
  await page.click('[data-mode="think"]');await page.click('#play');await page.click('#next');
  const manual=await page.locator('#view').getAttribute('data-clip');
  const manualFrame=Number(await page.locator('#view').getAttribute('data-frame'));
  const sourceTypes=await page.locator('body').getAttribute('data-source');
  const displayedCounts=await page.evaluate(()=>({idle:$('idleMeta').textContent,think:$('thinkMeta').textContent}));
  const phaseCount=await page.locator('#phasebar button').count();
  await page.selectOption('#background','checker');await page.setViewportSize({width:390,height:844});
  await page.screenshot({path:path.join(qa,'preview-mobile.png'),fullPage:true});
  const noOverflow=await page.evaluate(()=>document.documentElement.scrollWidth<=innerWidth);
  const resourceCoverage=await page.evaluate(()=>Object.values(clips).every(c=>c.renderers.length===c.count&&c.renderers.every(r=>r?.image.complete&&r.image.naturalWidth>0)));
  const counts={idle:manifest.clips.idle.frames?.length??manifest.clips.idle.frameCount,think:manifest.clips.think.frames?.length??manifest.clips.think.frameCount};
  const expectedAtlas=['idle','think'].filter(name=>manifest.clips[name].atlasPages?.length).length;
  const checks={
   completeSpriteCelMode:manifest.productionMode==='complete-sprite-cels',
   allFramesCovered:resourceCoverage,
   atlasPreferred:expectedAtlas===0||sourceTypes.split('/').filter(t=>t==='atlas').length===expectedAtlas,
   idleBeforeThreshold:before==='idle',thinkAfterThreshold:transitionClock>=threshold/1000&&transitionClock<threshold/1000+.2,
   pauseStable,actionResetsIdle:reset==='idle',manualThinking:manual==='think'&&manualFrame>=1,
   thumbnailsLazy:thumbnailCountCollapsed===0,thumbnailsBounded:visibleThumbs<=12&&visibleAfterPage<=12,
   thumbnailPaging:clickedFrame===pagedFrame+1&&selectedCount===1,
   noThumbnailRebuildDuringPlayback:sample.thumbnailBuilds===0,
   cadenceMatchesManifest:cadenceCountError<=Math.max(1,expectedTransitions*.025)&&meanIntervalErrorMs<25,
   slowPlayback:slowDelta>180&&slowDelta<350,
   allPhasesShown:phaseCount===phases.length,phasePositioning:phaseLocated,
   dynamicFrameCounts:displayedCounts.idle.startsWith(counts.idle+' 帧')&&displayedCounts.think.startsWith(counts.think+' 帧'),
   noMobilePageOverflow:noOverflow,noScriptErrors:errors.length===0
  };
  const result={checks,productionMode:manifest.productionMode,transitionSeconds:transitionClock,displayedCounts,sourceTypes,performance:{wallMs:sample.wallMs,paintedFrames:sample.drawCalls,expectedTransitions,cadenceCountError,meanIntervalErrorMs,measuredPaintRate,renderMsMax:sample.renderMsMax,thumbnailBuilds:sample.thumbnailBuilds,visibleTransitions:visibleTransitions.length,slowDelta},note:'Draw cadence is compared with manifest frame durations. These counts do not assert unique drawings or a fixed 30-fps pose rate.',errors};
  fs.writeFileSync(path.join(qa,'preview-checks.json'),JSON.stringify(result,null,2));console.log(JSON.stringify(result,null,2));
  if(Object.values(checks).some(v=>!v))process.exitCode=1;
 }finally{await browser.close();}
})().catch(e=>{console.error(e);process.exit(1)});


const fs=require('fs'),path=require('path'),{pathToFileURL}=require('url');
const {chromium}=require('C:/Users/10029/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/playwright');
const root=path.resolve(__dirname,'../art/level03-continuous-layers-v04-2026-10-31');
(async()=>{
 const browser=await chromium.launch({headless:true,executablePath:'C:/Program Files/Google/Chrome/Application/chrome.exe',args:['--allow-file-access-from-files']});
 const page=await browser.newPage({viewport:{width:1600,height:1200},deviceScaleFactor:1}),errors=[];
 page.on('pageerror',e=>errors.push(e.message));page.on('requestfailed',r=>errors.push(r.url()+': '+r.failure().errorText));
 await page.goto(pathToFileURL(path.join(root,'index.html')).href);await page.waitForLoadState('networkidle');
 const assets=await page.evaluate(()=>Promise.all(window.DRAIN_MANIFEST.assets.map(a=>new Promise(resolve=>{const i=new Image();i.onload=()=>resolve({file:a.file,ok:true,width:i.naturalWidth,height:i.naturalHeight});i.onerror=()=>resolve({file:a.file,ok:false});i.src=a.file;}))));
 await page.locator('#overview').evaluate(e=>e.style.width='2170px');await page.locator('#overview-wrap').evaluate(e=>e.style.width='2170px');
 await page.locator('#overview').screenshot({path:path.join(root,'连续长图_叠层.png')});
 await page.locator('#overview').evaluate(e=>e.style.width='');await page.locator('#overview-wrap').evaluate(e=>e.style.width='');
 const captureRect=()=>page.locator('#roots').evaluate(e=>{const r=e.getBoundingClientRect();return {x:r.x,y:r.y,width:r.width,height:r.height};});
 await page.locator('#camera').scrollIntoViewIfNeeded();
 await page.click('[data-preset="A"]');const rootA=await captureRect();const worldA=await page.locator('#world').getAttribute('style');
 await page.locator('#camera').screenshot({path:path.join(root,'QA/镜头A_固定根须.png')});
 await page.click('[data-preset="B"]');const rootB=await captureRect();const worldB=await page.locator('#world').getAttribute('style');
 await page.locator('#camera').screenshot({path:path.join(root,'QA/镜头B_固定根须.png')});
 await page.click('[data-preset="C"]');const rootC=await captureRect();
 await page.locator('#camera').screenshot({path:path.join(root,'QA/镜头C_固定根须.png')});
 const toggles={};for(const key of ['background','platforms','water','props','light','route']){const cb=page.locator(`[data-toggle="${key}"]`);await cb.check();const on=await page.locator(`[data-layer="${key}"]`).evaluateAll(es=>es.every(e=>getComputedStyle(e).display!=='none'));await cb.uncheck();const off=await page.locator(`[data-layer="${key}"]`).evaluateAll(es=>es.every(e=>getComputedStyle(e).display==='none'));toggles[key]=on&&off;if(key!=='route')await cb.check();}
 await page.locator('#toggle-roots').uncheck();const rootHidden=await page.locator('#roots').evaluate(e=>getComputedStyle(e).display==='none');await page.locator('#toggle-roots').check();
 await page.click('[data-state="middle"]');const middle=await page.evaluate(()=>({mirror:+document.getElementById('mirror-A').getAttribute('y'),counterweight:+document.getElementById('counterweight-A').getAttribute('y'),beam:document.querySelector('.beam-path').getAttribute('d')}));
 await page.click('[data-state="high"]');const high=await page.evaluate(()=>({mirror:+document.getElementById('mirror-A').getAttribute('y'),counterweight:+document.getElementById('counterweight-A').getAttribute('y'),beam:document.querySelector('.beam-path').getAttribute('d')}));
 await page.click('[data-preset="B"]');await page.screenshot({path:path.join(root,'分层总览.png'),fullPage:true});
 const result={errors,assets,toggles,rootToggle:rootHidden,fixedRootPositions:[rootA,rootB,rootC],rootsFixed:JSON.stringify(rootA)===JSON.stringify(rootB)&&JSON.stringify(rootB)===JSON.stringify(rootC),worldMoves:worldA!==worldB,mechanism:{middle,high,oppositeTravel:middle.mirror>high.mirror&&middle.counterweight<high.counterweight,constantYSum:middle.mirror+middle.counterweight===high.mirror+high.counterweight},scope:'Browser composition only, no Unity or jump/collision tests'};
 fs.writeFileSync(path.join(root,'QA/browser-check.json'),JSON.stringify(result,null,2));console.log(JSON.stringify(result,null,2));await browser.close();
 if(errors.length||assets.some(a=>!a.ok)||!Object.values(toggles).every(Boolean)||!result.rootsFixed||!result.worldMoves||!result.mechanism.oppositeTravel)process.exitCode=1;
})();

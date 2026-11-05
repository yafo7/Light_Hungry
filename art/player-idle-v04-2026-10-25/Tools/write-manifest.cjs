const fs=require('node:fs'),path=require('node:path');
const root=path.resolve(__dirname,'..'),config=JSON.parse(fs.readFileSync(path.join(root,'Rig/rig.json'),'utf8')),fps=30;
const phases=[{name:'自然微动',startMs:0,endMs:250},{name:'平滑向左偏头',startMs:250,endMs:1050},{name:'左侧停留与轻回弹',startMs:1050,endMs:1650},{name:'连续转向右侧',startMs:1650,endMs:2950},{name:'右侧短停',startMs:2950,endMs:3250},{name:'快速眨眼两次',startMs:3250,endMs:3686.666667},{name:'轻回弹',startMs:3686.666667,endMs:4100},{name:'平滑回正',startMs:4100,endMs:5200},{name:'自然微动衔接',startMs:5200,endMs:6000}];
const manifest={name:'LightHungry Player Idle v04',date:'2026-10-25',generator:'image_gen static layers + deterministic rigid rig animation',sampleRate:fps,frameSize:[512,512],facing:'right',pixelsPerUnit:256,pivot:[.5,.0625],footReferenceY:480,registrationOffset:config.registrationOffset,rig:'Rig/rig.json',playback:{idleThresholdSeconds:3,thinkingLoopsWhileIdle:true,interruptImmediatelyOn:['move','jump','interact','ability','damage','death']},clips:{}};
for(const [name,count] of [['idle',90],['think',180]]){
 const atlasPages=[];for(let start=0;start<count;start+=64){const n=Math.min(64,count-start);atlasPages.push({file:'Sheets/player_'+name+'_atlas_'+String(start/64+1).padStart(2,'0')+'.png',firstFrame:start,frameCount:n,columns:8,rows:Math.ceil(n/8)});}
 const frames=Array.from({length:count},(_,i)=>'Sprites/'+(name==='idle'?'Idle':'Think')+'/player_'+name+'_'+String(i).padStart(3,'0')+'.png');
 for(const file of [...frames,...atlasPages.map(p=>p.file)])if(!fs.existsSync(path.join(root,file)))throw new Error('Missing '+file);
 manifest.clips[name]={frameCount:count,loop:true,sampleRate:fps,durationMs:name==='idle'?3000:6000,frameDurationMs:Array(count).fill(1000/fps),atlasPages,grid:atlasPages[0].file,frames,phases:name==='idle'?[{name:'呼吸微动与轻眨眼',startMs:0,endMs:3000}]:phases,animationMethod:'Identical head bitmap under a rigid rotation+translation matrix; separate eyelid mask.'};
}
fs.writeFileSync(path.join(root,'animations.json'),JSON.stringify(manifest,null,2));fs.writeFileSync(path.join(root,'animations.js'),'window.ANIMATION_MANIFEST = '+JSON.stringify(manifest,null,2)+';\n');
console.log('Manifest written for existing 270 PNG frames and five atlases.');


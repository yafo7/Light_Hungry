const fs=require('fs');
const sharp=require('C:/Users/10029/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp');
const dir='art/level03-extended-pulley-v02-2026-10-31';
const jumps=[
 {id:'warmup',name:'节奏短跳',gap:1.1,rise:.45,landing:1.8},
 {id:'counterweight_in',name:'跳上配重',gap:1.2,rise:.5,landing:1.25},
 {id:'counterweight_out',name:'离开配重',gap:1.15,rise:.4,landing:2.0},
 {id:'precision_flat',name:'窄台远跳',gap:1.5,rise:.1,landing:1.0},
 {id:'precision_up',name:'折返高跳',gap:1.05,rise:.85,landing:1.2},
 {id:'ferry',name:'踏板端点起跳',gap:1.15,rise:.3,landing:1.5}
];
for(const j of jumps){const t=(9+Math.sqrt(81-2*29.43*j.rise))/29.43;j.idealReach=+(3.2*t).toFixed(2);j.edgeDistanceMargin=+(j.idealReach-j.gap).toFixed(2);}
const spec={version:'ThirdDrain expanded design v02',date:'2026-10-31',status:'Design only; no new Unity scene or runtime playtest',lengthH:76,previousLengthH:34,relativeLength:76/34,characterHeightUnit:'H; same world scale as Level02',targetFirstPlayMinutes:[8,12],targetRequiredJumps:[24,28],cameraWidthH:[15,18],artPanels:[{id:'A',windowH:[0,30],state:'middle'},{id:'B',windowH:[24,54],state:'high'},{id:'C',windowH:[48,78],state:'high'}],zones:[{id:'A',x:[0,30],purpose:'shutter, rhythmic jumps, middle-height counterweight crossing',jumpBudget:8},{id:'B',x:[30,52],purpose:'upper winch, narrow shelves and local switchback',jumpBudget:9},{id:'C',x:[52,76],purpose:'precision jumps, slow moving platform, scale and outlet',jumpBudget:9}],checkpoints:[{x:19,label:'过闸前'},{x:31,label:'上层绞盘后'},{x:61,label:'摆渡前'}],pulley:{constantSumH:8.6,states:[{name:'低位',mirrorY:2.4,counterweightY:6.2,route:'step unreachable'},{name:'中位',mirrorY:5.4,counterweightY:3.2,route:'cross from left bank to upper bank'},{name:'高位',mirrorY:8.2,counterweightY:.4,route:'counterweight lowers behind player; upper route remains safe'}],source:[27.35,10.8],mirrorX:27.35,reflectorAngle:'fixed 45-degree backslash',reflectorPlayerCollision:false,depthNote:'Mirror and guides behind the foreground jump plane; the counterweight step is in the playable foreground.',leftBank:{x:19.2,y:2.7,width:2},counterweight:{x:22.4,width:1.25},farBank:{x:24.8,y:3.6,width:2},upperWinchPlatform:{x:27.9,y:4.1,width:3},operation:'two cranks on the same chain drive; both ratchet-lock; reversible',lowAndMidLight:'stopped by real opaque masonry before the creature',highLight:'horizontal y=8.2 through actual rib aperture to scale near x=74',target:[74,8.2]},movingPlatform:{widthH:1.3,totalTravelH:1.5,maxSpeedHPerSec:.6,endpointPauseSeconds:.4,approxCycleSeconds:6,drive:'old water paddle and crank linkage',rail:'background non-walkable guide, not a continuous bridge',note:'Transfer moving-platform velocity and verify landing physics during later implementation.'},jumpModules:jumps,recovery:{loops:'Separate per-section loops. Under-route grates/level drops prevent walking beneath the entire level to the exit.',retained:'lamp state, mirror setting, locally reached checkpoints',resetRule:'upper checkpoint keeps high mirror state; before crossing checkpoint restores its recorded middle/low state, never a stranded far-side configuration'},lightForm:'Global three-second standing charge remains. High beam stays above early standable platforms; no invisible per-level ability ban.',validation:'Analytical jump range estimates only; switchback clearance, overlaps, moving-platform timing and bypass routes require Unity graybox testing.'};
fs.writeFileSync(dir+'/design-spec.json',JSON.stringify(spec,null,2));
const e=[];const T=(x,y,t,size=18,c='#334943',extra='')=>`<text x="${x}" y="${y}" font-size="${size}" fill="${c}" ${extra}>${t}</text>`;
const R=(x,y,w,h,c,ex='')=>`<rect x="${x}" y="${y}" width="${w}" height="${h}" fill="${c}" ${ex}/>`;
const L=(x,y,x2,y2,c='#75897d',w=3,ex='')=>`<line x1="${x}" y1="${y}" x2="${x2}" y2="${y2}" stroke="${c}" stroke-width="${w}" ${ex}/>`;
e.push('<svg xmlns="http://www.w3.org/2000/svg" width="1600" height="1190" viewBox="0 0 1600 1190"><defs><marker id="arrow" markerWidth="8" markerHeight="8" refX="7" refY="4" orient="auto"><path d="M0,0 L8,4 L0,8" fill="none" stroke="#527e70" stroke-width="1.4"/></marker></defs><g font-family="Microsoft YaHei, Noto Sans CJK SC, sans-serif">');
e.push(R(0,0,1600,1190,'#f6f2e9'),T(50,55,'03 排水暗渠 · 加长与进阶设计',32),T(50,90,'76H 长度 / 约 5 个游玩屏宽 / 目标 24–28 次跳跃 / H 为主角高度 / 仍为待实测设计',18,'#7b877d'));
e.push(T(50,133,'一套机关，两次用途：先中位搭路，再高位送光',23));
const states=[['① 初始低位',2.4,6.2,'配重太高，无法接上两侧落脚台。'],['② 中位：搭路',5.4,3.2,'先过配重踏板，到达上层第二绞盘。'],['③ 高位：送光',8.2,.4,'上层继续升镜，照亮深处的光鳞。']];
for(let i=0;i<3;i++){
 const x=50+i*505,y=170,sy=h=>y+310-h*23;
 const [title,m,c,caption]=states[i];
 e.push(R(x,y,480,406,'#fffdf7','rx="16" stroke="#ded8ca"'),T(x+22,y+32,title,23));
 e.push(R(x+25,sy(2.7),125,76,'#ad9e87'),R(x+235,sy(3.6),90,95,'#ad9e87'),R(x+345,sy(4.1),110,106,'#ad9e87'));
 e.push(T(x+45,sy(2.7)+22,'左岸',15),T(x+245,sy(3.6)+22,'右岸',15),T(x+350,sy(4.1)+22,'上层绞盘',14));
 e.push(L(x+330,sy(10.8),x+330,sy(m),'#d39b2f',3),R(x+315,sy(10.8)-15,30,23,'#bb9557','rx="4"'));
 e.push(L(x+316,sy(m)-14,x+344,sy(m)+14,'#6f8885',6),L(x+330,sy(m),x+455,sy(m),'#d39b2f',3));
 if(i<2)e.push(R(x+440,sy(m)-17,11,35,'#948b7d'),T(x+361,sy(m)-10,'受阻',13,'#987629'));else e.push(`<ellipse cx="${x+455}" cy="${sy(m)}" rx="9" ry="14" fill="#e5bf67"/>`);
 const top=y+55,left=x+179,right=x+305,ty=sy(c);
 e.push(`<circle cx="${left}" cy="${top}" r="9" fill="#cfb481" stroke="#80694c" stroke-width="2"/><circle cx="${right}" cy="${top}" r="9" fill="#cfb481" stroke="#80694c" stroke-width="2"/>`);
 e.push(`<path d="M${left-9},${ty} V${top} Q${left-9},${top-9} ${left},${top-9} H${right} Q${right+9},${top-9} ${right+9},${top} V${sy(m)-14}" fill="none" stroke="#88714d" stroke-width="3"/>`);
 e.push(R(x+165,ty,45,12,'#b3a46e','stroke="#76653d" stroke-width="2"'),T(x+149,ty-10,'配重踏板',14));
 if(i===1)e.push(`<path d="M${x+139},${sy(2.7)-4} Q${x+157},${ty-40} ${x+179},${ty-4} M${x+200},${ty-4} Q${x+218},${sy(3.6)-35} ${x+249},${sy(3.6)-4}" fill="none" stroke="#527e70" stroke-width="2" stroke-dasharray="4 3" marker-end="url(#arrow)"/>`);
 e.push(T(x+22,y+344,`镜托 ${m.toFixed(1)}H ↑ / 配重 ${c.toFixed(1)}H ↓`,17,'#8c744f'),T(x+22,y+377,caption,17));
}
e.push(T(50,620,'路线节奏：一段学习、一段组合、一段执行',23));
const zones=[['A 0–30H｜约 8 跳','开灯 → 节奏短跳 → 中位搭路','宽台起步 · 两侧绞盘 · 过闸前恢复点','#dce7db'],['B 30–52H｜约 9 跳','高位送光 → 窄台 → 局部折返','高低交替 · 上层出口 · 失足回本段','#e5dfcb'],['C 52–76H｜约 9 跳','精确连跳 → 慢速摆渡 → 光鳞','摆渡前休息 · 端点停顿 · 最后宽台','#d9e5e8']];
zones.forEach((a,i)=>{const x=50+i*505;e.push(R(x,642,480,141,a[3],'rx="12"'),T(x+20,677,a[0],20),T(x+20,716,a[1],20),T(x+20,752,a[2],16,'#718079'));});
e.push(T(50,828,'跳跃模块：远跳配低高差，高跳配短间距',23));
const widths=[50,400,630,865,1100,1330];const headers=['模块','净距','上升','落脚宽度','理论距离','余量'];
headers.forEach((h,i)=>e.push(T(widths[i]+10,865,h,17,'#7f877d')));
jumps.slice(0,5).forEach((j,i)=>{const y=898+i*39;e.push(L(50,y+12,1540,y+12,'#ddd9ce',1));[j.name,j.gap+'H',j.rise+'H',j.landing+'H',j.idealReach+'H',j.edgeDistanceMargin+'H'].forEach((s,n)=>e.push(T(widths[n]+10,y,s,17)));});
e.push(T(50,1120,'移动踏板：宽 1.3H，往复行程 1.5H，端点停约 0.4 秒；导轨处于背景，不能直接踩着走过去。',17,'#6f7d73'),T(50,1154,'表中距离只作解析估算；落点、顶头空间、折返碰撞与移动平台惯性须在 Unity 灰盒验证。',16,'#8a8d80'));
e.push('</g></svg>');const svg=e.join('\n');fs.writeFileSync(dir+'/03-mechanism-and-pacing.svg',svg);
sharp(Buffer.from(svg)).png().toFile(dir+'/03-mechanism-and-pacing.png').then(()=>console.log(JSON.stringify({file:dir+'/03-mechanism-and-pacing.png',jumps},null,2)));

// Native explanatory vector drawing; no painted image is edited here.
const fs=require('fs'),path=require('path'),sharp=require(process.env.CODEX_SHARP_MODULE||'sharp');
const out=path.join(__dirname,'../art/level02-lightform-v07-2026-10-30');fs.mkdirSync(out,{recursive:true});
const S=[12.6,9.6],M=[12.6,1.2],E=[23.4,-1.1],gapX=19.2;
const beamY=x=>M[1]+(x-M[0])*(E[1]-M[1])/(E[0]-M[0]);
const normalize=v=>{const n=Math.hypot(...v);return v.map(x=>x/n)};
const incoming=[0,-1],outgoing=normalize([E[0]-M[0],E[1]-M[1]]),normal=normalize([incoming[0]-outgoing[0],incoming[1]-outgoing[1]]);
const spec={version:'v07',status:'design only, no Unity implementation',supersedes:{level02:'v06 feeding target and open walking passage',preserves:'downstream 3.5H connector and 7H main drain concept'},confirmed:{globalAbility:true,level02CreaturePart:false,level02Goal:'Use light form to cross a rubble-blocked drain inlet through a real small gap',chargeSeconds:3,chargingVisual:'body rim light grows, then body becomes one light mote',control:'player-controlled bidirectional movement along light path'},proposedDefaults:{keys:{backward:'A',forward:'D',stop:'release both',manualReform:'E, safe-ground only; optional'},chargeRequiresStanding:true,interactionPausesCharging:true,effectiveLight:['direct traced sunlight','reflected traced sunlight'],excludedLight:['own emission','creature glow','baked bright background'],breakChargeOnMovementOrLeavingLight:true,autoReformAtTutorialSafeLanding:true,rearmAfterReform:'leave and re-enter a valid beam',beamLost:'return to last safe corporeal anchor',particleCoreDiameterH:.08,slitClearanceNormalToRayH:.18,bodyHeightH:1,bodyWidthH:.42},layout:{unit:'H',potFloorY:0,potWidthH:25.2,potHeightH:11,source:S,fixedReflector:M,reflectorNormalXY:normal,beamPolyline:[S,M,E],chargeFeet:[14.3,0],chargeRayHeightH:beamY(14.3),rubbleAcrossInletX:[17.4,22.4],slitCenter:[gapX,beamY(gapX)],slitAxis:outgoing,safeLandingFeet:[22,-1.3],arrivalOnRay:[22,beamY(22)],oldOpenStairRouteBlocked:true},needsRuntimeValidation:['body light overlap','three-second uninterrupted timing','reflection and physical slit alignment','no corporeal bypass','bidirectional curve traversal','safe reform capsule clearance','compatibility with Level 01 mirror interaction']};
fs.writeFileSync(path.join(out,'lightform-spec.json'),JSON.stringify(spec,null,2));
let a=[];const add=s=>a.push(s),X=x=>88+(x-10)*80,Y=y=>720-y*80;
const p=(x,y)=>`${X(x).toFixed(2)},${Y(y).toFixed(2)}`,poly=pts=>pts.map(v=>p(...v)).join(' ');
const text=(x,y,t,size=17,c='#33433c',anchor='start')=>add(`<text x="${x}" y="${y}" font-size="${size}" fill="${c}" text-anchor="${anchor}">${t}</text>`);
function spirit(x,y,h,glow=0){if(glow)add(`<ellipse cx="${x}" cy="${y-h*.55}" rx="${h*(.48+glow*.15)}" ry="${h*(.6+glow*.16)}" fill="url(#halo)" opacity="${glow}"/>`);add(`<g transform="translate(${x},${y}) scale(${h})" stroke="#987543" stroke-width=".025" fill="#fff0c6"><path d="M-.13,-.29 L-.21,0 L-.06,0 L.01,-.14 L.11,0 L.23,0 L.13,-.29 Z"/><path d="M-.26,-.28 Q-.46,-.48 -.31,-.80 L-.13,-.7 L-.14,-1 L.08,-.81 Q.39,-.76 .35,-.47 Q.32,-.25 -.26,-.28Z"/><ellipse cx=".12" cy="-.55" rx=".028" ry=".052" fill="#584630"/><ellipse cx=".25" cy="-.54" rx=".022" ry=".046" fill="#584630"/></g>`);}
function orb(x,y,r){add(`<circle cx="${x}" cy="${y}" r="${Math.max(16,r*6)}" fill="url(#halo)"/><circle cx="${x}" cy="${y}" r="${r}" fill="#fff8d2"/>`);}
add(`<svg xmlns="http://www.w3.org/2000/svg" width="1480" height="1030" viewBox="0 0 1480 1030"><defs><radialGradient id="halo"><stop stop-color="#ffe4a1" stop-opacity=".95"/><stop offset=".5" stop-color="#efbe56" stop-opacity=".4"/><stop offset="1" stop-color="#efbe56" stop-opacity="0"/></radialGradient><pattern id="earth" width="20" height="20" patternUnits="userSpaceOnUse"><rect width="20" height="20" fill="#d5c7ae"/><path d="M0 20L20 0" stroke="#c4b698" stroke-width=".8"/></pattern><marker id="arrow" markerWidth="7" markerHeight="7" refX="6" refY="3.5" orient="auto-start-reverse"><path d="M0 0L7 3.5L0 7Z" fill="#578a8b"/></marker></defs><rect width="1480" height="1030" fill="#f6f1e5"/><g font-family="Microsoft YaHei,Microsoft YaHei UI,Arial,sans-serif">`);
text(42,49,'第二关 · 化光穿隙',29);text(42,80,'全局能力教学：站在光中 3 秒 → 一个光核 → A / D 沿光路前后移动',17,'#6b7665');
const captions=[['0 秒','站定受光'],['约 1 秒','身体边缘发亮'],['约 2 秒','光晕扩大、轮廓消融'],['满 3 秒','收拢成一个光核'],['光态','A 后退 / D 前进']];
for(let i=0;i<5;i++){let x=42+i*283;add(`<rect x="${x}" y="108" width="264" height="239" rx="12" fill="#293c3b"/>`);text(x+18,139,captions[i][0],18,'#f3e5ba');add(`<line x1="${x+24}" y1="279" x2="${x+240}" y2="279" stroke="#a8aa88" stroke-width="3"/>`);
 if(i<3){add(`<path d="M${x+106} 157L${x+154} 157L${x+171} 279L${x+94} 279Z" fill="#eec05a" opacity=".12"/>`);spirit(x+132,278,102,[.12,.5,.95][i]);}
 else if(i===3){orb(x+132,224,4.2);add(`<path d="M${x+85} 245Q${x+103} 217 ${x+125} 224" stroke="#e8c477" stroke-width="2" fill="none" stroke-dasharray="3 6"/>`);}
 else{add(`<line x1="${x+35}" y1="223" x2="${x+229}" y2="223" stroke="#e7bf63" stroke-width="3"/><line x1="${x+74}" y1="253" x2="${x+190}" y2="253" stroke="#8cbdba" stroke-width="2" marker-start="url(#arrow)" marker-end="url(#arrow)"/>`);orb(x+132,223,4.2);}
 text(x+132,318,captions[i][1],16,'#e1e6d4','middle');}
text(42,380,'前三秒完成前仍是实体；光晕大小与光核碰撞大小分开。具体恢复规则见设计文档中的建议项。',15,'#647363');
add('<rect x="42" y="407" width="1396" height="567" rx="12" fill="#eae4d4"/>');text(65,442,'同一条真实光路：顶部孔 → 固定反光片 → 平地充能 → 石缝 → 缝后平台',20);
// Floor and cavity under the blocked inlet.
add(`<path d="M${p(10.2,0)}L${p(17.4,0)}L${p(17.4,-2.7)}L${p(21.1,-2.7)}L${p(21.4,-1.3)}L${p(23.4,-1.3)}L${p(24.7,-2.8)}L${p(25.7,-2.8)}L${p(25.7,-3)}L${p(10.2,-3)}Z" fill="url(#earth)"/>`);
add(`<path d="M${p(10.2,0)}L${p(17.4,0)} M${p(21.4,-1.3)}L${p(23.4,-1.3)}L${p(24.7,-2.8)}L${p(25.7,-2.8)}" fill="none" stroke="#7b8665" stroke-width="5"/>`);
const half=.09,n=[-outgoing[1],outgoing[0]],off=(x,k)=>[x+n[0]*k,beamY(x)+n[1]*k];
const upper=[off(17.4,half),[17.4,1.8],[18.1,2.2],[19.15,1.75],[20.15,2.05],[20.9,1.5],off(20.9,half)];
const lower=[off(17.4,-half),off(20.9,-half),[21.1,-2.7],[17.4,-2.7]];
add(`<polygon points="${poly(upper)}" fill="#858c78" stroke="#59634f" stroke-width="2"/><polygon points="${poly(lower)}" fill="#747e6c" stroke="#59634f" stroke-width="2"/>`);
add(`<path d="M${p(18.1,2.2)}L${p(18.65,.6)}L${p(19.15,1.75)} M${p(19.75,1.95)}L${p(20.1,.35)}" fill="none" stroke="#5d674f" stroke-width="2"/>`);
// The fixed reflector lies on a low support; ideal two-dimensional reflection is explicit.
add(`<path d="M${p(11.7,.1)}Q${p(11.8,1.2)} ${p(12.6,1.2)}" fill="none" stroke="#728253" stroke-width="4"/><ellipse cx="${X(12.4)}" cy="${Y(1.05)}" rx="49" ry="12" fill="#94a365"/>`);
let tangent=[-normal[1],normal[0]];add(`<line x1="${X(M[0]-.25*tangent[0])}" y1="${Y(M[1]-.25*tangent[1])}" x2="${X(M[0]+.25*tangent[0])}" y2="${Y(M[1]+.25*tangent[1])}" stroke="#f3ecd6" stroke-width="7"/>`);
add(`<polyline points="${poly([[12.6,3],M,E])}" fill="none" stroke="#d2a34c" stroke-width="8" opacity=".25"/><polyline points="${poly([[12.6,3],M,E])}" fill="none" stroke="#fff2b8" stroke-width="3"/>`);
spirit(X(14.3),Y(0),80,.7);orb(X(gapX),Y(beamY(gapX)),.04*80);
// A ghost outline on the safe platform describes a future state, not a second character.
add(`<rect x="${X(22)-16.8}" y="${Y(-.3)}" width="33.6" height="80" rx="15" fill="none" stroke="#6b9d9b" stroke-width="2" stroke-dasharray="5 5"/>`);
text(X(10.6),493,'顶部孔投下的日光',15);text(X(11),Y(-.7),'固定反光片，已经对准',15);text(X(13.55),Y(-.62),'站定 3 秒',17,'#9d7935');
text(X(17.7),Y(2.55),'石砾始终堵住入口',16);text(X(19.6),Y(-1.9),'真缝隙约 0.18H',15,'#fff2cf','middle');
text(X(21.6),Y(.35),'缝后安全平台',16);text(X(21.6),Y(-.05),'自动恢复：建议',14,'#647c73');
text(X(23.6),Y(-2.35),'接第三关',16);
add(`<line x1="${X(15.2)}" y1="${Y(.2)}" x2="${X(16.6)}" y2="${Y(-.1)}" stroke="#578a8b" stroke-width="2" marker-start="url(#arrow)" marker-end="url(#arrow)"/>`);
text(65,951,'图中光核与落脚轮廓用于说明不同阶段；同一时刻只有一个玩家。示意坐标不是已验证的 Unity 碰撞布局。',14,'#677263');
text(42,1008,'第二关没有食光兽部位。此处教会的是通行能力；碎石不会因为过关而消失。',17);
add('</g></svg>');const svg=a.join('\n');fs.writeFileSync(path.join(out,'化光教学与光路.svg'),svg);sharp(Buffer.from(svg)).png().toFile(path.join(out,'化光教学与光路.png')).then(()=>console.log('Saved lightform-spec.json and teaching SVG/PNG'));

const fs=require('fs');
const sharp=require('C:/Users/10029/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp');
const dir='art/level03-lantern-pulley-v01-2026-10-31';
const platforms=[['入口',0,0,5],['操作台',5.9,-.5,6.1],['踏墩',13,-.5,1.6],['踏墩',15.6,-.5,1.8],['跳台 1',18.4,.2,2.3],['跳台 2',21.7,.9,1.8],['跳台 3',24.5,1.6,1.8],['跳台 4',27.3,2.3,2],['终点台',30.3,3,3.7]];
const spec={version:'ThirdDrain concept v01',date:'2026-10-31',status:'Design proposal only. Existing Unity ThirdDrain remains arrival preview.',unit:'H = standing character height, same world scale as Level02',camera:'Orthographic side view perpendicular to back wall; camera zoom does not change world scale',platforms:platforms.map(([name,x,y,w])=>({name,x,y,width:w})),optics:{source:[14,6.8],mirrorX:14,mirrorLowY:1,mirrorSolvedY:4.1,mirrorAngle:'fixed 45-degree backslash in side view; vertical incoming ray reflects horizontally right',target:[31.5,4.1],lowRayBlockedBy:'front of jump platform 3 at x=24.5; top y=1.6',travelRange:[1,4.6],toleranceH:.18,type:'reflection, not refraction'},controls:{lamp:'one-time shutter lever at entrance',winch:'E engages; W/S raises/lowers the reflector; release locks ratchet; E leaves',movement:'A/D, Space jump; previous 3-second light form retained'},recovery:{shallowWaterDepthH:.25,dryReturnWalkY:-2.3,returnSteps:'toward operation terrace; <=0.6H each',checkpoint:'after setting reflector, preserve lamp and lift state'},narrative:'A cropped tail-root scale drinks the light; its local skin fold relaxes upward and uncovers an existing outlet.',physicsReference:{moveSpeed:3.2,jumpSpeed:9,gravity:29.43,note:'Numbers from current Level02 controller, analytical check only; no ThirdDrain runtime playtest.'}};
spec.jumpEstimates=platforms.slice(1).map((p,i)=>{const a=platforms[i],gap=p[1]-a[1]-a[3],rise=p[2]-a[2],v=9,g=29.43,t=(v+Math.sqrt(v*v-2*g*rise))/g;return{from:a[0],to:p[0],gapH:+gap.toFixed(2),riseH:+rise.toFixed(2),idealHorizontalReachH:+(3.2*t).toFixed(2),note:'Analytical bound; platform widths, collision and timing require graybox testing.'};});
fs.writeFileSync(dir+'/scene-spec.json',JSON.stringify(spec,null,2));
const X=x=>80+40*x,Y=y=>500-40*y;
const pieces=[];
const line=(x1,y1,x2,y2,stroke='#5c8178',width=3,extra='')=>`<line x1="${x1}" y1="${y1}" x2="${x2}" y2="${y2}" stroke="${stroke}" stroke-width="${width}" ${extra}/>`;
const text=(x,y,t,size=18,fill='#384746',extra='')=>`<text x="${x}" y="${y}" font-size="${size}" fill="${fill}" ${extra}>${t}</text>`;
const rect=(x,y,w,h,fill,extra='')=>`<rect x="${x}" y="${y}" width="${w}" height="${h}" fill="${fill}" ${extra}/>`;
pieces.push(`<svg xmlns="http://www.w3.org/2000/svg" width="1600" height="1110" viewBox="0 0 1600 1110"><defs><marker id="a" markerWidth="9" markerHeight="9" refX="7" refY="4" orient="auto"><path d="M0,0 L8,4 L0,8" fill="none" stroke="#5c8178" stroke-width="1.5"/></marker><marker id="gold" markerWidth="8" markerHeight="8" refX="7" refY="4" orient="auto"><path d="M0,0 L8,4 L0,8" fill="none" stroke="#c58b22" stroke-width="1.4"/></marker></defs><g font-family="Microsoft YaHei, Noto Sans CJK SC, sans-serif">`);
pieces.push(rect(0,0,1600,1110,'#f6f2e8'),text(60,57,'03  提起一束光 · 排水暗渠',32),text(60,92,'关卡结构提案  /  正交侧视  /  H = 主角站立高度  /  美术示意与可玩尺寸分开校准',17,'#6b7770'));
pieces.push(rect(55,127,1490,505,'#e4e9e3','rx="18"'),rect(70,555,1460,46,'#bcd2d0','rx="6"'));
for(let y=160;y<555;y+=40)pieces.push(line(75,y,1525,y,'#d8ded7',1));
pieces.push(text(100,160,'砖拱、苔痕与根须：背景层',17,'#74837b'),text(105,620,'可见浅水与返回路：失足后从下方回到操作台，保留开灯与升镜进度。',17));
for(let i=0;i<platforms.length;i++){const [name,x,y,w]=platforms[i],top=Y(y);pieces.push(rect(X(x),top,w*40,555-top,'#b3a18a','stroke="#71695d" stroke-width="2"'),line(X(x),top,X(x+w),top,'#506d55',5));pieces.push(text(X(x)+w*20,top+24,name,14,'#4a4b43','text-anchor="middle"'));if(i){const prev=platforms[i-1],sx=X(prev[1]+prev[3]-.3),ex=X(x+.4),sy=Y(prev[2]) -7,ey=top-7;pieces.push(`<path d="M${sx},${sy} Q${(sx+ex)/2},${Math.min(sy,ey)-48} ${ex},${ey}" fill="none" stroke="#5c8178" stroke-width="2.2" stroke-dasharray="5 4" marker-end="url(#a)"/>`);}}
// Source, fixed guide, moving reflector, and a continuous rope routed clear of the optical shaft.
pieces.push(rect(X(14)-21,Y(6.8)-36,42,36,'#b88d48','rx="6" stroke="#705c3d" stroke-width="2"'),text(X(14)+37,Y(6.8)-22,'① 检修灯：向下出光',18));
pieces.push(line(X(13.2),Y(6.5),X(13.2),Y(.7),'#809187',4));
pieces.push(`<circle cx="${X(12.6)}" cy="${Y(7.1)}" r="13" fill="#ddc69d" stroke="#7c6745" stroke-width="3"/>`);
pieces.push(`<path d="M${X(10.6)},${Y(.05)} L${X(12.6)-13},${Y(7.1)} A13,13 0 0 1 ${X(12.6)+13},${Y(7.1)} L${X(12.6)+13},${Y(4.6)} L${X(13.4)},${Y(4.6)}" fill="none" stroke="#856d47" stroke-width="3"/>`);
pieces.push(`<circle cx="${X(10.6)}" cy="${Y(.05)}" r="15" fill="#ccb084" stroke="#755d39" stroke-width="3"/>`,text(X(7.7),Y(1.65),'② 绞盘 → 拉绳 → 升镜',18));
pieces.push(`<path d="M${X(2)},${Y(.7)} L${X(2)},${Y(6.7)} L${X(13.5)},${Y(7.5)} L${X(14)},${Y(7.1)}" fill="none" stroke="#84918a" stroke-width="1.5" stroke-dasharray="3 4"/>`,text(X(.3),Y(1.35),'开遮光片',16));
pieces.push(line(X(13.45),Y(4.65),X(14.55),Y(3.55),'#647b80',7),line(X(13.45),Y(1.55),X(14.55),Y(.45),'#9ba9a3',5,'stroke-dasharray="5 5"'));
pieces.push(line(X(14),Y(6.8),X(14),Y(4.1),'#d6a134',4,'marker-end="url(#gold)"'),line(X(14),Y(4.1),X(31.5),Y(4.1),'#d6a134',4,'marker-end="url(#gold)"'));
pieces.push(line(X(14),Y(4.1),X(14),Y(1),'#b49b63',2,'stroke-dasharray="6 5"'),line(X(14),Y(1),X(24.5),Y(1),'#b49b63',2,'stroke-dasharray="6 5"'));
pieces.push(text(X(16),Y(4.1)-17,'高位：光路越过所有砖台',18,'#9a6e1f'),text(X(15),Y(2.4),'低位：被较高砖台挡住',17,'#8e7751'));
pieces.push(line(X(12.5),Y(1.2),X(12.5),Y(4.2),'#5c8178',2,'marker-end="url(#a)"'),text(X(11.7),Y(3.1),'升',18));
pieces.push(`<ellipse cx="${X(31.5)}" cy="${Y(4.1)}" rx="15" ry="26" fill="#e6c270" stroke="#ae8446" stroke-width="2"/>`,text(X(29.5),Y(5.25),'④ 尾根光鳞',18),text(X(31),Y(1.4),'⑤ 出口',18));
pieces.push(text(X(23),Y(3.4),'③ 连续短跳',18));
// Standing reference figure: always the same H.
pieces.push(`<circle cx="${X(9.5)}" cy="${Y(.2)}" r="11" fill="#fff4c3" stroke="#9f8e5f"/><path d="M${X(9.5)},${Y(.2)+11} v16 m0,-7 l-8,8 m8,-8 l8,8" fill="none" stroke="#9f8e5f" stroke-width="3"/>`,text(X(8.8),Y(-1.55),'主角 = 1H',13));
pieces.push(text(65,674,'机关状态对照',24),text(1030,674,'跳跃与恢复',24));
function state(x,title,high){pieces.push(rect(x,695,460,320,'#fffdf6','rx="14" stroke="#ded7c9"'),text(x+22,733,title,20));const sx=x+105,beamY=high?795:882;pieces.push(rect(sx-14,760,28,23,'#b88d48','rx="4"'),line(sx,783,sx,beamY,'#c58b22',3),line(sx-17,beamY-17,sx+17,beamY+17,'#647b80',5),rect(x+276,845,46,92,'#a59580','stroke="#71695d" stroke-width="2"'),line(x+24,937,x+435,937,'#7f9485',3),line(sx,beamY,high?x+410:x+276,beamY,'#c58b22',3));if(high)pieces.push(`<ellipse cx="${x+413}" cy="${beamY}" rx="11" ry="18" fill="#e6c270"/>`);pieces.push(text(x+22,970,high?'松手棘爪锁止，平台跳跃时不必持续拉绳。':'方向已经正确，但高度不够。',17),text(x+22,997,high?'镜片固定倾角，只沿导轨升降。':'引导玩家发现：移动镜片会平移出射光。',16,'#798178'));}
state(55,'A  开灯 / 镜片低位',false);state(540,'B  升镜 / 光鳞受光',true);
pieces.push(rect(1025,695,520,320,'#fffdf6','rx="14" stroke="#ded7c9"'),text(1050,734,'短跳为主，间距由前两关控制器校准',20));
pieces.push(rect(1070,870,130,60,'#b3a18a'),rect(1250,835,130,95,'#b3a18a'),line(1200,898,1250,898,'#5c8178',2),text(1190,923,'约 1.0H',15));
pieces.push(`<path d="M1165,858 Q1234,740 1290,824" fill="none" stroke="#5c8178" stroke-width="3" stroke-dasharray="6 4" marker-end="url(#a)"/>`,text(1320,808,'上升 0.7H',16));
pieces.push(text(1050,969,'宽落脚台 ≥ 1.6H；浅水下方有连续返回路。',16),text(1050,997,'尺寸为灰盒起点，尚未做第三关运行时跳跃验证。',15,'#798178'));
pieces.push(text(60,1065,'实线金色 = 解谜后光路   ·   虚线金色 = 低位受阻光路   ·   绿虚线 = 跳跃方向   ·   不新增旋转镜片或视角切换教学',17,'#6b7770'),'</g></svg>');
fs.writeFileSync(dir+'/03-layout-and-mechanism.svg',pieces.join('\n'));
sharp(Buffer.from(pieces.join('\n'))).png().toFile(dir+'/03-layout-and-mechanism.png').then(()=>console.log(JSON.stringify({diagram:dir+'/03-layout-and-mechanism.png',jumps:spec.jumpEstimates},null,2)));

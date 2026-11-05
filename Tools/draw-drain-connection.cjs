// Engineering diagram only. This script never edits the painted source images.
const fs=require('fs'),path=require('path');
const sharp=require(process.env.CODEX_SHARP_MODULE||'sharp');
const out=path.join(__dirname,'../art/level02-03-drain-connection-v06-2026-10-30');
fs.mkdirSync(out,{recursive:true});
const S=26,X=x=>80+x*S,Y=y=>445-y*S;
const pt=(x,y)=>`${X(x).toFixed(2)},${Y(y).toFixed(2)}`;
const poly=pts=>pts.map(p=>pt(...p)).join(' ');
const route=[[2,0],[17.4,0]];
for(let i=1;i<=18;i++){route.push([17.4+i*.45,-(i-1)*.3]);route.push([17.4+i*.45,-i*.3]);}
route.push([29,-5.4],[35,-7.6],[48,-7.6]);
const roof=[[22.4,0],[25.5,-1.9],[29,-1.9],[35,-4.1],[35,-.6],[48,-.6]];
const cave=[[17.4,0],...roof,[48,-7.6],...route.slice(1,-1).reverse()];
const spec={version:'v06',status:'Art layout and dimensioned section; not a Unity playable level',unit:'H = same character mesh total visible height',camera:{projection:'orthographic',gameplayHeightH:16,transition:'follow right and downward without changing character scale',overviewIsNotGameplayCamera:true},pot:{widthH:25.2,heightH:11,rimY:-1,roofY:10,topWidthH:12,retainOriginalExternalSilhouette:true},catchment:{openingX:[17.4,22.4],openingWidthH:5,stairStart:[17.4,0],stairEnd:[25.5,-5.4],risers:18,riserH:.3,treadH:.45,entry:'in soil floor, not ceramic side wall'},connector:{start:[25.5,-5.4],landingEnd:[29,-5.4],end:[35,-7.6],clearHeightH:3.5,insideWidthZH:2.8,dryLedgeWidthZH:1.1},mainDrain:{entry:[35,-7.6],ceilingY:-.6,clearHeightH:7,insideWidthZH:4.6,dryLedgeWidthZH:1.2,waterSurfaceY:-8.25,waterDepthH:.25,turn:'continue into the previously designed perpendicular B passage'},walkSurfaceXY:route,roofProfileXY:roof,mechanics:'Keep Level 02 leaf reflector / exposed tail scale and Level 03 M1→M2 sequence. Optical coordinates are not defined by this section.',limits:['Geometric proposal, not a collision simulation.','Watercolor images are not dimension-authoritative.','Original Level 01 Unity scene remains unchanged.']};
fs.writeFileSync(path.join(out,'connection-spec.json'),JSON.stringify(spec,null,2));
let a=[];const add=s=>a.push(s);
const text=(x,y,t,size=17,color='#334441',anchor='start')=>add(`<text x="${x}" y="${y}" font-size="${size}" fill="${color}" text-anchor="${anchor}">${t}</text>`);
const line=(p,q,colour='#61736b',width=2,dash='')=>add(`<line x1="${p[0]}" y1="${p[1]}" x2="${q[0]}" y2="${q[1]}" stroke="${colour}" stroke-width="${width}" ${dash?`stroke-dasharray="${dash}"`:''}/>`);
const worldLine=(p,q,c,w,d)=>line([X(p[0]),Y(p[1])],[X(q[0]),Y(q[1])],c,w,d);
function dim(x1,y1,x2,y2,label,vertical=false){const c='#507e88';worldLine([x1,y1],[x2,y2],c,1.5);if(vertical){worldLine([x1-.15,y1],[x1+.15,y1],c,1.5);worldLine([x2-.15,y2],[x2+.15,y2],c,1.5);text(X(x1)+10,(Y(y1)+Y(y2))/2+5,label,15,c);}else{worldLine([x1,y1-.15],[x1,y1+.15],c,1.5);worldLine([x2,y2-.15],[x2,y2+.15],c,1.5);text((X(x1)+X(x2))/2,Y(y1)-10,label,15,c,'middle');}}
function hero(x,y){add(`<g transform="translate(${X(x)},${Y(y)}) scale(${S})" fill="#fff2b6" stroke="#74552e" stroke-width=".045"><path d="M-.18,-.31 L-.23,0 L-.08,0 L0,-.17 L.09,0 L.22,0 L.17,-.31 Z"/><path d="M-.29,-.32 Q-.49,-.58 -.28,-.92 L-.11,-.77 L-.05,-1 L.16,-.84 Q.4,-.69 .3,-.41 Q.07,-.22 -.29,-.32Z"/><ellipse cx=".18" cy="-.54" rx=".027" ry=".055" fill="#4f4032"/></g>`);}
add(`<svg xmlns="http://www.w3.org/2000/svg" width="1480" height="850" viewBox="0 0 1480 850"><defs><pattern id="earth" width="22" height="22" patternUnits="userSpaceOnUse"><rect width="22" height="22" fill="#e3d7bf"/><path d="M-2 8L8-2M2 24L24 2M18 26L26 18" stroke="#cabfa8" stroke-width=".7"/></pattern><marker id="arrow" markerWidth="8" markerHeight="8" refX="7" refY="4" orient="auto"><path d="M0 0L8 4L0 8Z" fill="#368390"/></marker></defs><rect width="1480" height="850" fill="#f7f3e8"/><g font-family="Microsoft YaHei,Microsoft YaHei UI,Arial,sans-serif">`);
text(52,54,'02 → 03  陶盆 · 集水口 · 排水暗渠',30);
text(53,86,'连续纵剖面 / 同一世界坐标     H = 主角高度     概念灰盒尺寸，尚未进行 Unity 碰撞验证',16,'#727b70');
add(`<rect x="52" y="112" width="1376" height="600" rx="12" fill="#eeeadc"/>`);
// Ground and excavation: no circular doorway is punched through the pot wall.
add(`<path d="M${pt(-1,0)}H${X(49)}V${Y(-9.4)}H${X(-1)}Z" fill="url(#earth)"/>`);
add(`<polygon points="${poly(cave)}" fill="#283d43" stroke="#71827d" stroke-width="5"/>`);
// Original inverted-pot shell, with its central drainage hole sectioned.
add(`<path d="M${pt(0,-1)} L${pt(6.6,10)} L${pt(12.15,10)} L${pt(12.15,9.6)} L${pt(6.9,9.6)} L${pt(.55,-.6)} Z M${pt(13.05,10)}L${pt(18.6,10)}L${pt(25.2,-1)}L${pt(24.65,-.6)}L${pt(18.3,9.6)}L${pt(13.05,9.6)}Z" fill="#bf7449" stroke="#825032" stroke-width="2"/>`);
add(`<path d="M${pt(.6,-.6)}L${pt(6.9,9.6)}L${pt(18.3,9.6)}L${pt(24.6,-.6)}L${pt(22.4,0)}L${pt(.6,0)}Z" fill="#bc9b70" opacity=".2"/>`);
worldLine([-.2,-1],[1.0,-1],'#825032',7);worldLine([24.2,-1],[25.5,-1],'#825032',7);
// Stylized leaf and tail, behind the traversal route.
add(`<path d="M${pt(9,0)}Q${pt(8.5,3.6)} ${pt(11,4.4)}" fill="none" stroke="#6d8059" stroke-width="5"/><path d="M${pt(9.5,4.1)}Q${pt(11,6)} ${pt(15,5)}Q${pt(12,3.4)} ${pt(9.5,4.1)}Z" fill="#86945b" stroke="#66744b"/>`);
add(`<path d="M${pt(15,1.1)}Q${pt(18,1)} ${pt(21,4.2)}L${pt(23.2,2.3)}Q${pt(19.4,.1)} ${pt(15,1.1)}Z" fill="#d6d0b5" stroke="#9a9c87" stroke-width="2"/>`);
worldLine([12.6,9.6],[12,4.5],'#d0a044',2,'6 5');worldLine([12,4.5],[18,2.3],'#d0a044',2,'6 5');
// Dry walking edge, same route at both scene boundaries.
add(`<polyline points="${poly(route)}" fill="none" stroke="#d7c29b" stroke-width="6" stroke-linejoin="round"/>`);
for(let x=35;x<48;x+=1.4){worldLine([x,-7.6],[x,-8.1],'#70898a',1);}
add(`<rect x="${X(35)}" y="${Y(-8.25)}" width="${13*S}" height="${.25*S}" fill="#77a6ae"/><path d="M${pt(35,-8.25)}Q${pt(38,-8.15)} ${pt(40,-8.25)}T${pt(48,-8.25)}" fill="none" stroke="#b6d5d6" stroke-width="2"/>`);
// Main drain's masonry, a mirror and the onward corner reference.
for(let y=-.6;y>-7;y-=1){worldLine([35,y],[48,y],'#506368',1,'26 4');}
add(`<rect x="${X(44.5)}" y="${Y(-.6)}" width="${S*.6}" height="${S*7}" fill="#5c6c6c" opacity=".6"/>`);
add(`<ellipse cx="${X(39.5)}" cy="${Y(-5.8)}" rx="10" ry="19" fill="#b0c1ba" stroke="#c6a668" stroke-width="3"/>`);worldLine([39.5,-6.5],[39.5,-7.6],'#c6a668',3);
hero(5.5,0);hero(27.3,-5.4);hero(37.1,-7.6);
// Walking arrows, separate in colour from optical hints.
add(`<path d="M${pt(5,.7)}H${X(16.6)}L${pt(24.6,-4.2)}L${pt(28.7,-4.2)}L${pt(35.2,-6.7)}H${X(47.7)}" fill="none" stroke="#368390" stroke-width="3" stroke-dasharray="9 7" marker-end="url(#arrow)"/>`);
text(X(9),128,'第二关：陶盆内部',21,'#825032','middle');text(X(39),394,'第三关：原砖砌主渠 A 段',20,'#334441','middle');
text(X(1),Y(6.3),'盆沿贴地，顶部为原盆底',15);text(X(1),Y(5.3),'外形沿用第一关',15,'#727b70');
dim(0,11.1,25.2,11.1,'陶盆宽 25.2H');dim(17.4,1.1,22.4,1.1,'地面集水口 5H');
dim(28,-5.4,28,-1.9,'3.5H',true);dim(46,-7.6,46,-.6,'7H',true);
text(X(22.8),Y(4),'右侧盆壁保持完整',16,'#825032');line([X(24),Y(3.6)],[X(24.8),Y(-.6)],'#825032',1.5);
text(X(16),Y(-3.4),'低石阶向下',16,'#334441');text(X(16),Y(-4.25),'单级 0.3H',14,'#727b70');
text(X(28),Y(-.45),'盆沿下面的砖砌通道',16,'#334441');
text(X(36),Y(-2),'从较低的支渠进入更高的主渠',14,'#cbd9d4');
text(X(40.8),Y(-6.7),'接原转角 → B',14,'#d7e5df');
text(68,748,'空间关系',17);text(165,748,'盆内地面 → 下行短阶 → 盆沿下方连接段 → 主渠干燥渠沿 → 原第三关转角',18);
text(68,782,'镜头关系',17);text(165,782,'游玩时保持 16H 画面高度，随角色向右、向下跟随；此展开图用于说明空间，不是单屏游戏构图。',16,'#5e6e65');
text(68,815,'同高的三个小精灵表示同一角色在不同位置；背景中的尾部不占用前景行走线。水面比主渠步道低 0.65H。',15,'#69766b');
add('</g></svg>');const svg=a.join('\n');fs.writeFileSync(path.join(out,'02-03-continuous-section.svg'),svg);
sharp(Buffer.from(svg)).png().toFile(path.join(out,'02-03-continuous-section.png')).then(()=>console.log(JSON.stringify({png:'02-03-continuous-section.png',svg:'02-03-continuous-section.svg',spec:'connection-spec.json'})));

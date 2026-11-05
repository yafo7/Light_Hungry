from pathlib import Path
import json, math, zipfile, xml.etree.ElementTree as ET
import draw_core as dc
from draw_core import C, FONT, BOLD, Image, ImageFont
from design_helpers import Sheet

OUT=Path(__file__).resolve().parent
dc.OUT=OUT;dc.W=2200;dc.H=1560;dc.S=2
PURPLE='#8A739D';GLASS='#75A7AE'
S=(250,350);O=(275,1095);A=(1240,920);B=(575,680);Q=(1020,700);D=(1390,425);T=(1530,361);WALL=(910,700);MISS=(1420,308)

def main():
    b=Sheet('05-single-light-route-v03','05  废弃泵房｜一束光，多次改道',
      '同一个光源、没有分光。先把光送去机关舱，再收回改道；镜背控制台需要实体抵达。')
    b.legend()
    b.rect(60,240,1580,970,C['white'],C['line'],1)
    b.rect(1670,240,470,970,C['white'],C['line'],1)
    b.rect(100,280,1500,880,'#EDF0EB')
    for x in (105,390,800,1100,1595):b.line([(x,315),(x,1150)],'#DCE3DD',12)
    b.line([(100,1150),(100,280),(1600,280),(1600,1150)],'#B7C2B9',4)
    b.rect(450,1105,1150,55,C['water'])
    for y in (1120,1140):b.line([(465,y),(1585,y)],'#BBD3D8',2)
    b.ground(105,1160,345,35);b.ground(1260,985,300,45);b.glass(835,985,425)
    for x,y,w in [(1400,900,125),(1300,815,125),(1400,730,125),(1220,665,160)]:
        b.ground(x,y,w,24)
        b.line([(x+w-12,y+24),(1550,y+65),(1550,y+24)],'#BCC7BE',2)
    # B has one aperture, with a valid full-body landing on its floor.
    b.rect(430,570,265,175,'#E3E7E1','#546B67',3)
    b.rect(680,701,25,32,C['white'])
    for yy in (701,733):b.line([(680,yy),(695,yy)],'#546B67',3)
    b.line([(430,745),(695,745)],C['ink'],5)
    # Q is suspended, with its physical control on C above and to the right.
    b.line([(1280,642),(1080,642),(1080,700),(1050,700)],PURPLE,3,True)
    b.wheel(1280,627,'K2 镜背手轮')
    b.line([(910,666),(910,737)],C['muted'],7)
    b.line([Q,WALL],C['ghost'],2,True)
    # D can initially be entered only through the glass floor.
    b.rect(1280,289,255,13,'#D2DBD2',C['muted'],2)
    b.rect(1280,302,9,188,'#DBEEEE',GLASS,2)
    b.glass(1280,490,210);b.ground(1540,490,60,40)
    b.line([(1535,315),(1535,490)],C['green'],5)
    for yy in (340,370,400,430,460):b.line([(1525,yy-12),(1544,yy+12)],C['green'],3)
    b.beast(1460,300,135,120,*T,'');b.items=b.items[:-2]
    b.tag(1450,306,'侧腹：浅白\n光鳞：灰暗',C['muted'],17)
    # Only the final single beam is gold. The abandoned branch is gray.
    b.source(*S,'唯一窗缝光源 S')
    b.ray([S,O,A,Q,D,T]);b.line([A,B],C['ghost'],3,True)
    b.mirror(*O,S,A,'M0 固定',fixed=True)
    b.mirror(*A,O,Q,'M1 改道镜')
    mirror_start=len(b.items)
    b.mirror(*Q,A,D,'M2 悬镜')
    b.items.pop(mirror_start)  # Suspended head: no floor-standing pedestal.
    b.mirror(*D,Q,T,'M3 末镜')
    b.line([D,MISS],C['ghost'],2,True)
    b.tag(1300,338,'未调：照空墙',C['muted'],17)
    b.wheel(490,700,'K1 检修卷轮')
    b.wheel(1460,454,'K3 手轮')
    b.arrow([(1445,438),(1416,431)],PURPLE,3,True)
    b.arrow([(490,670),(490,536),(1580,536),(1580,860),(1527,860)],PURPLE,2,True)
    for x,y in [(1210,985),(604,745),(1280,665),(1390,490)]:b.pad(x,y)
    b.spirit(150,1154);b.walk([(180,1147),(240,1147)])
    # Gray route is historic, so it cannot be mistaken for a simultaneous beam.
    b.arrow([(1198,884),(633,683)],C['ghost'],3,True)
    b.arrow([(640,730),(1180,922)],C['ghost'],3,True)
    b.lightmove([(330,1068),(1190,913)])
    b.lightmove([(1203,891),(1033,720)])
    b.lightmove([(1045,667),(1345,437)])
    b.walk([(1295,969),(1364,969)])
    b.jump((1365,970),(1445,889));b.jump((1430,890),(1355,804))
    b.jump((1360,804),(1445,719));b.jump((1408,719),(1280,654))
    b.arrow([(1340,686),(1490,765),(1385,849),(1508,926),(1350,971)],C['blue'],2,True)
    b.walk([(1430,475),(1585,475)])
    b.badge(1060,850,'1');b.badge(1210,835,'2');b.badge(1476,818,'3');b.badge(1125,579,'4');b.badge(1470,530,'5')
    b.tag(126,1073,'入口',C['blue'],23);b.tag(113,1180,'← 第 04 关：池岸管口',C['blue'],19)
    b.tag(315,402,'光源旁没有可恢复实体的落脚处',C['muted'],19)
    b.tag(465,596,'B｜窄缝检修舱',C['green'],22)
    b.tag(430,777,'化光进入；实体转卷轮；沿原光返回 A',C['green'],20)
    b.tag(706,660,'仅容光核的缝隙',C['muted'],18);b.line([(730,690),(700,714)],C['muted'],2)
    b.tag(732,820,'旧路 A → B\nM1 改向后，这里变暗',C['muted'],19)
    b.tag(780,737,'M2 初始照挡板\n虚线为旧方向',C['muted'],18)
    b.tag(1120,1078,'A｜玻璃高台：反复调整 M1',C['green'],21)
    b.tag(1120,1119,'B 检修舱 / M2 悬镜，二者只能选一个',C['muted'],19)
    b.tag(1080,738,'M2 处无落脚面\n不能在这里恢复实体',C['muted'],18)
    b.tag(1370,595,'C｜镜背控制台',C['green'],20)
    b.tag(1370,628,'低于光路；上行调镜后\n沿实体跳台返回 A',C['blue'],17)
    b.tag(1305,1041,'K1 展开跳台并锁止',PURPLE,20)
    b.tag(1034,389,'D｜顶部玻璃夹层',C['green'],22)
    b.tag(1060,440,'沿光穿玻璃到 D\n恢复实体后操作 K3',C['muted'],19)
    b.tag(1350,565,'光鳞受光 → 出口根帘舒展',C['green'],18)
    b.tag(1360,251,'第 06 关 → 温室苗床',C['blue'],20)
    b.rect(440,300,600,84,C['white'],C['line'],1)
    b.text(460,317,'金色只画最终通光状态',25,C['ink'],True)
    b.text(460,352,'灰色虚线为已放弃的光路；各阶段见状态图。',20,C['muted'])
    b.text(1698,268,'行动与改光顺序',30,C['ink'],True)
    steps=[
      ('01  先把光借给 B','抵达 A，将 M1 对准检修缝。\n化光进 B，展开实体检修跳台；\n再沿同一束光回退到 A。'),
      ('02  收回光，改送 M2','在 A 转 M1：B → 悬镜 M2。\nB 熄灭；M2 初态只照空墙。\n此时无法直接乘光抵达 D。'),
      ('03  用实体去镜子背面','沿新跳台到 C，转 K2 调 M2。\n从上方观察光斑，接通 D；\n再沿实体跳台返回 A。'),
      ('04  沿新路线去 D','A 充光三秒，再出发。\n沿 M1 → M2 → D 连续反射；\nM2 只是转折点，不在此落脚。'),
      ('05  把行路光变成喂养光','在 D 恢复实体，转 K3 调 M3。\n同一束光照到侧腹暗鳞。\n根帘舒展后，实体走向温室。')]
    for i,(title,txt) in enumerate(steps):
        y=335+i*158;b.text(1700,y,title,24,C['green'],True);b.text(1700,y+44,txt,20,C['muted'])
        if i<4:b.line([(1700,y+139),(2110,y+139)],C['line'],1)
    b.text(1700,1155,'开过的机关保留，旧的光路收回。',21,C['ink'],True)
    cards=[('M1：决定把光给谁','先照 B，取得跳台；再改照 M2。','进入下一段，必须放弃上一条光路。'),
           ('M2：光到得了，人站不下','操纵杆在上方 C；镜面悬在台下。','化光抵达镜面，不等于能操作机关。'),
           ('M3：抵达之后，再次改光','来到 D 后，调整同一束光的末段。','喂鳞成功，才从根帘出口进入温室。')]
    for i,(title,l1,l2) in enumerate(cards):
        x=60+i*705;b.rect(x,1240,670,160,C['white'],C['line'],1)
        b.text(x+22,1260,title,27,C['green'],True);b.text(x+22,1307,l1,21,C['muted']);b.text(x+22,1345,l2,21,C['ink'])
    b.text(62,1431,'全关只有一束有效光，不分裂、不复制；改镜只影响出射段，M0 → A 的入射段始终保留。',23,C['ink'])
    b.text(62,1480,'站光三秒化光，手动沿光前后移动。机关必须实体操作；A、B、D 首次抵达需化光，C 需走实体检修路线。',22,C['muted'])
    b.text(62,1521,'v03 设计草图 · 保留微观泵房与透光玻璃 · 未实装；镜架限位、净空、跳距与折射/反射碰撞需灰盒验证。',19,C['muted'])
    return b

def mini(b,x,y,mode):
    # All panels repeat identical geometry. Coordinates are panel-local.
    p={'S':(80,65),'O':(95,430),'A':(785,350),'B':(310,220),'Q':(590,255),'D':(875,105),'T':(958,60),'W':(512,255),'X':(903,18)}
    def pt(k):return(x+p[k][0],y+p[k][1])
    b.rect(x+30,y+22,950,447,'#EDF0EB',C['line'],1)
    b.ground(x+35,y+465,150,12);b.glass(x+625,y+389,215);b.ground(x+840,y+389,115,15)
    b.rect(x+227,y+170,135,93,'#E3E7E1',C['muted'],2)
    b.rect(x+353,y+225,13,23,C['white'])
    b.ground(x+760,y+225,65,15)
    for xx,yy in [(860,340),(800,298),(865,256)]:
        if mode>1:b.ground(x+xx,y+yy,70,12)
        else:b.line([(x+xx,y+yy),(x+xx+70,y+yy)],C['ghost'],3,True)
    b.line([(x+780,y+209),(x+653,y+209),(x+653,y+255),(x+610,y+255)],PURPLE,2,True)
    b.rect(x+780,y+15,170,9,C['ground'],C['muted'],1)
    b.rect(x+780,y+24,6,119,'#DBEEEE',GLASS,1);b.glass(x+780,y+144,170)
    b.rect(x+955,y+132,25,21,C['ground'],C['muted'],1)
    b.poly([(x+958,y+47),(x+970,y+60),(x+958,y+74),(x+946,y+60)],'#F2CF69' if mode==4 else C['scale'],C['ink'],1)
    route=['S','O','A']+(['B'] if mode==1 else ['Q','W'] if mode==2 else ['Q','D','X'] if mode==3 else ['Q','D','T'])
    b.ray([pt(k) for k in route])
    if mode>1:b.line([pt('A'),pt('B')],C['ghost'],2,True)
    for key in ['O','A','Q','D']:
        xx,yy=pt(key);b.ellipse(xx-12,yy-12,24,24,C['white'],C['green'] if key!='O' else C['muted'],2)
        if key in route:
            i=route.index(key);pa=pt(route[i-1]);pb=pt(route[i+1]);v=(xx-pa[0],yy-pa[1]);u=(pb[0]-xx,pb[1]-yy)
            lv=math.hypot(*v);lu=math.hypot(*u);ang=math.atan2(v[1]/lv+u[1]/lu,v[0]/lv+u[0]/lu)
            b.line([(xx-13*math.cos(ang),yy-13*math.sin(ang)),(xx+13*math.cos(ang),yy+13*math.sin(ang))],C['green'],4)
    xx,yy=pt('S');b.ellipse(xx-11,yy-11,22,22,'#F6D886',C['gold'],2)
    labels=[('S',-30,-35,'唯一光源'),('O',-25,21,'M0'),('A',-18,22,'A / M1'),('B',-64,-41,'B 检修舱'),('Q',-48,20,'M2 悬镜'),('D',10,-20,'D / M3'),('T',-16,-41,'光鳞')]
    for key,dx,dy,t in labels:
        xx,yy=pt(key);b.tag(xx+dx,yy+dy,t,C['muted'],18)
    b.tag(x+718,y+185,'C 镜背控制',C['green'],18)
    if mode==1:
        b.lightmove([(x+750,y+320),(x+365,y+214)])
        b.lightmove([(x+372,y+247),(x+748,y+353)])
        b.tag(x+412,y+300,'去 B 操作，再沿原光返回',C['orange'],20)
    elif mode==2:
        b.walk([(x+835,y+374),(x+890,y+331),(x+830,y+285),(x+895,y+243),(x+790,y+212)])
        b.tag(x+374,y+315,'B 已熄灭；人沿跳台去 C',C['blue'],20)
        b.line([(x+512,y+240),(x+512,y+271)],C['muted'],5)
    elif mode==3:
        b.arrow([(x+800,y+242),(x+913,y+280),(x+861,y+371)],C['blue'],2,True)
        b.lightmove([(x+760,y+323),(x+610,y+255),(x+849,y+91)])
        b.tag(x+383,y+327,'调好 M2 → 实体回 A → 化光到 D',C['blue'],20)
    else:
        b.walk([(x+889,y+131),(x+969,y+131)])
        b.tag(x+384,y+321,'在 D 调 M3，同一束光喂鳞',C['green'],20)

def states():
    b=Sheet('05-single-light-states-v03','05  同一束光的四种状态｜每格只画当时有效的光路',
      '同一空间重复四次：金色 = 当前光；灰虚线 = 已熄灭旧路。机关状态可以保留，光本身不能同时留在两条路上。')
    panels=[(60,160,1,'①  先照机关舱','S → M0 → M1 → B','操作 B 的卷轮，展开跳台；保持此角度返回 A。'),
            (1130,160,2,'②  回 A，重调 M1','S → M0 → M1 → M2 → 挡板','把光从 B 收回送向悬镜；实体爬到 C，准备调 M2。'),
            (60,835,3,'③  在 C 调 M2，再回 A 出发','S → M0 → M1 → M2 → M3 → 空墙','M2 没有落脚点；需从 C 实体回 A，再沿新光路去 D。'),
            (1130,835,4,'④  到 D 后，调整 M3','S → M0 → M1 → M2 → M3 → 光鳞','光路最后用于喂养，出口根帘舒展；实体进入温室。')]
    for x,y,mode,title,route,note in panels:
        b.rect(x,y,1010,640,C['white'],C['line'],1)
        b.text(x+20,y+15,title,27,C['green'],True);b.text(x+20,y+57,route,22,C['ink'])
        mini(b,x+6,y+106,mode)
        b.text(x+20,y+598,note,19,C['muted'])
    b.text(62,1515,'关键：必须先回到 A，才把通往 B 的光收走。光态不操作镜子；所有改镜动作都在安全实体平台完成。',22,C['ink'])
    return b

def editable(boards):
    result=[]
    for bi,b in enumerate(boards):
        for e in b.items:
            n=len(result)+1;typ=e['type'];x=e.get('x',0);y=e.get('y',0);w=e.get('w',0);h=e.get('h',0)
            if typ in ('line','poly'):
                pts=e['pts'];x=min(p[0] for p in pts);y=min(p[1] for p in pts);w=max(p[0] for p in pts)-x;h=max(p[1] for p in pts)-y
            if typ=='text':
                f=ImageFont.truetype(BOLD if e['bold'] else FONT,e['size']);w=max(f.getlength(l) for l in e['t'].split('\n'));h=e['size']*1.35*len(e['t'].split('\n'))
            a=dict(id=f'singlelight-{n:04d}',type={'rect':'rectangle','poly':'line'}.get(typ,typ),x=x,y=y+bi*1660,width=max(w,.01),height=max(h,.01),angle=0,strokeColor=e.get('color',e.get('stroke')) or 'transparent',backgroundColor=e.get('fill') or 'transparent',fillStyle='solid',strokeWidth=e.get('lw',1),strokeStyle='dashed' if e.get('dash') else 'solid',roughness=0,opacity=100,groupIds=[],frameId=None,roundness=None,seed=n*31,version=1,versionNonce=n*17,isDeleted=False,boundElements=None,updated=1793750400000,link=None,locked=False)
            if typ in ('line','poly'):
                pp=pts+[pts[0]] if typ=='poly' else pts;a.update(points=[[px-x,py-y] for px,py in pp],lastCommittedPoint=None,startBinding=None,endBinding=None,startArrowhead=None,endArrowhead=None)
            if typ=='text':a.update(fontSize=e['size'],fontFamily=2,text=e['t'],originalText=e['t'],textAlign='left',verticalAlign='top',containerId=None,autoResize=True,lineHeight=1.35,baseline=e['size'])
            result.append(a)
    (OUT/'level05-single-light-v03.excalidraw').write_text(json.dumps(dict(type='excalidraw',version=2,source='local',elements=result,appState=dict(viewBackgroundColor=C['paper'],gridSize=None),files={}),ensure_ascii=False),encoding='utf-8')
    return result

if __name__=='__main__':
    boards=[main(),states()]
    for b in boards:b.save()
    elements=editable(boards)
    for b in boards:
        ET.parse(OUT/(b.key+'.svg'))
        with Image.open(OUT/(b.key+'.png')) as im:assert im.size==(dc.W,dc.H)
    assert len({e['id'] for e in elements})==len(elements)
    (OUT/'validation.json').write_text(json.dumps(dict(dimensions=[dc.W,dc.H],boards=2,svg_xml='pass',editable_objects=len(elements),unique_ids=True,source_count=1,split_beams=False,unity_graybox='not performed',excalidraw_editor_import='not performed'),indent=2),encoding='utf-8')
    print(json.dumps(dict(output=str(OUT),objects=len(elements))))

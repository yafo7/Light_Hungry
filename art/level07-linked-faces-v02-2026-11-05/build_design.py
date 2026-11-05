"""Level 07 front/back interlocking puzzle proposal. No Unity changes."""
from pathlib import Path
import copy, json, zipfile, xml.etree.ElementTree as ET
import draw_core as dc
from draw_core import C,FONT,BOLD,Image,ImageFont
from design_helpers import Sheet
OUT=Path(__file__).resolve().parent
dc.OUT=OUT;dc.W=2400;dc.H=1740;dc.S=2
LINK='#AE7798';WOOD='#A18365'

class Layer(Sheet):
    def __init__(self):self.items=[]

def paste(b,s,x,y,scale=1):
    for src in s.items:
        e=copy.deepcopy(src)
        for k in ('x','y','w','h','lw','size'):
            if k in e:e[k]*=scale
        if 'x' in e:e['x']+=x
        if 'y' in e:e['y']+=y
        if 'pts' in e:e['pts']=[(px*scale+x,py*scale+y) for px,py in e['pts']]
        if 'size' in e:e['size']=max(11,round(e['size']))
        b.items.append(e)

def gate(b,x,y,opened):
    if opened:
        b.rect(x,y,48,16,'#B9C8A8',C['green'],2)
        b.line([(x+24,y+25),(x+24,340)],C['ghost'],2,True)
    else:
        b.rect(x,y,48,340-y,'#A5B99D',C['green'],3)
        for yy in range(y+24,330,40):b.line([(x+5,yy),(x+43,yy+24)],'#738D6C',2)

def face(front=True,phase=4,labels=True,flow=True):
    # Identical world x values on both sides. Pane offset is solely layout.
    b=Layer();steps=phase>=2;bridge=phase>=3;far=phase>=4
    b.rect(0,0,1600,420,'#EFF2E9' if front else '#EEF0F3',C['line'],1)
    for x in (250,430,900,1270):b.line([(x,18),(x,395)],'#D5DDD4',2,True)
    b.line([(25,23),(1575,23)],C['ghost'],4)
    if front:
        b.ground(35,340,465,60);b.ground(1210,340,355,60)
        b.ground(895,170,90,18)
        for x,y,w in [(540,305,80),(675,245,80),(800,205,75)]:
            if steps:
                b.ground(x,y,w,16)
                b.line([(x+8,y+16),(x+35,y+52),(x+w-8,y+16)],WOOD,2)
            else:
                b.line([(x,y),(x,y+70)],C['ghost'],6)
                b.line([(x,y),(x+w,y)],C['ghost'],2,True)
        b.rect(1030,23,145,377,'#B2C3AC',C['ink'],3)
        for yy in range(45,390,42):b.line([(1043,yy),(1160,yy+25)],'#91A98C',3)
        b.text(1053,157,'厚叶幕\n与花架墙',24,C['ink'],True)
        b.lever(945,147,'')
        b.source(95,218,'')
        b.ray([(95,218),(250,310)])
        b.mirror(250,310,tag=None,fixed=True)
        b.mirror(430,310,tag=None,fixed=True)
        b.mirror(1270,310,tag=None,fixed=True)
        b.rect(486,285,14,42,'#6B7771',C['ink'],2)
        if far:
            b.ray([(1270,310),(1470,235)])
            b.line([(430,310),(484,310)],C['ghost'],2,True)
        else:b.ray([(430,310),(484,310)])
        b.beast(1400,112,165,155,1470,235,'')
        b.items=b.items[:-2]
        if phase>=4:b.poly([(1451,235),(1470,211),(1489,235),(1470,259)],'#F3D77E',C['gold'],2)
        b.line([(1540,263),(1540,284 if far else 340)],C['green'],4)
        if not far:
            for yy in (281,304,327):b.line([(1530,yy-8),(1550,yy+8)],C['green'],3)
        if flow:
            if phase==1:
                b.spirit(125,335);b.walk([(145,326),(220,326)])
            elif phase in (2,3,4):
                b.jump((480,330),(575,295));b.jump((605,295),(710,235))
                b.jump((744,235),(840,195));b.jump((866,195),(940,160))
                b.arrow([(936,190),(820,227),(705,277),(560,333),(440,333)],C['blue'],3,True)
            if far:b.walk([(1300,329),(1550,329)])
        if labels:
            labs=[(60,33,'正面 A｜叶幕走廊',C['ink'],28),
                  (110,360,'06 → 入口',C['blue'],20),(207,255,'入口镜 P',C['green'],20),
                  (395,364,'R0 对应落点',C['green'],20),(530,360,'展开后的踏阶：必须实体跳跃',C['blue'],20),
                  (854,106,'K前：开背桥',LINK,22),(1220,364,'R1：墙后落点',C['green'],20),
                  (1390,64,'浅白背侧 · 暗鳞',C['muted'],20),(1350,288,'→ 08 雨槽屋脊',C['blue'],18)]
            for x,y,t,c,sz in labs:b.tag(x,y,t,c,sz)
            b.tag(515,77,'不发光的实体跳跃区',C['muted'],20)
    else:
        b.ground(35,340,745,60);b.ground(1100,340,465,60)
        b.line([(290,352),(770,352)],WOOD,4)
        b.line([(1110,352),(1480,352)],WOOD,4)
        for x,y,w in [(340,270,80),(480,205,80),(620,145,80),(740,95,90)]:
            b.ground(x,y,w,15)
            b.line([(x+7,y+15),(x+35,y+45),(x+w-8,y+15)],WOOD,2)
        b.lever(790,73,'')
        if bridge:
            b.ground(780,340,320,14)
            b.line([(780,351),(1100,351)],WOOD,5)
        else:
            b.line([(780,340),(1100,340)],C['ghost'],3,True)
            b.line([(780,340),(780,395)],WOOD,8)
            b.line([(1100,340),(1100,395)],WOOD,8)
            b.rect(766,307,14,33,WOOD,C['ink'],2)
        gate(b,900,23,bridge)
        b.mirror(250,310,tag=None,fixed=True)
        rx=1270 if far else 430
        b.ray([(250,310),(rx,310)])
        b.mirror(rx,310,tag=None)
        b.rect(rx-32,333,64,8,WOOD,C['ink'],2)
        for xx in (rx-23,rx+23):b.ellipse(xx-7,339,14,14,C['white'],WOOD,2)
        if far:b.mirror(430,310,tag=None,ghost=True)
        if flow:
            b.jump((295,330),(375,260));b.jump((400,260),(515,195));b.jump((546,195),(655,135));b.jump((690,135),(785,85))
            b.arrow([(785,122),(655,169),(510,236),(375,306),(280,326)],C['blue'],3,True)
            if far:b.walk([(470,375),(1235,375)])
        if labels:
            labs=[(60,33,'背面 B｜藤架检修侧',C['ink'],28),(185,365,'接收镜 Q',C['green'],20),
                  (325,365,'R0 停镜位',C['green'],20),(730,29,'K背',LINK,22),
                  (520,235,'第一段爬升\n开正面踏阶',C['blue'],20),(925,77,'联动挡叶',C['muted'],20),
                  (809,370,'轨桥（与挡叶一起开）',LINK,20),(1217,365,'R1 停镜位',C['green'],20)]
            for x,y,t,c,sz in labs:b.tag(x,y,t,c,sz)
            b.tag(1120,95,'镜车移到墙后\n来光始终接入',C['muted'],20)
    return b

def legend(b):
    b.rect(60,145,2280,64,C['white'],C['line'],1)
    b.walk([(85,178),(140,178)]);b.text(157,162,'实体行走 / 跳跃',22)
    b.ray([(438,178),(495,178)]);b.text(510,162,'唯一光路的平面段',22)
    b.arrow([(865,178),(925,178)],C['purple'],4,True);b.text(941,162,'光路穿到另一面',22)
    b.arrow([(1260,178),(1320,178)],LINK,3,True);b.text(1335,162,'机关跨面联动',22)
    b.line([(1670,178),(1730,178)],C['ghost'],3,True);b.text(1746,162,'旧位置 / 收起状态',22)
    b.rect(2130,168,19,19,C['scale']);b.text(2165,162,'待照光鳞',22)

def overview():
    b=Sheet('07-linked-faces-route-v02','07  叶幕走廊｜在这一面，修好另一面的路',
      '前后两面处于同一高度。先在背面开正面踏阶，再去正面开背面轨桥，最后移动回程镜越过厚叶墙。')
    legend(b)
    paste(b,face(True,4),60,250);paste(b,face(False,4),60,870)
    # Cross-plane rays: the diagram separates faces vertically only for reading.
    b.arrow([(310,560),(310,1180)],C['purple'],5,True)
    b.arrow([(1330,1180),(1330,560)],C['purple'],5,True)
    b.tag(330,688,'入口镜 P → 背面 Q',C['purple'],22)
    b.tag(1170,688,'镜车 R1 → 正面墙后',C['purple'],22)
    b.line([(490,590),(490,1150)],C['ghost'],2,True)
    b.tag(430,754,'R0：第一次返回的位置',C['muted'],21)
    b.tag(757,758,'同一根藤架立柱\n世界 x 坐标配准',C['muted'],21)
    # Distinct pink mechanical dependencies, separate from purple optical depth.
    b.arrow([(850,943),(850,826),(730,826),(730,550)],LINK,3,True)
    b.tag(620,714,'K背 → 正面踏阶锁定展开',LINK,21)
    b.arrow([(1005,397),(1705,397),(1705,1255),(1150,1255)],LINK,3,True)
    b.tag(1370,803,'K前 → 背面轨桥 + 挡叶',LINK,20)
    b.rect(1760,250,580,1040,C['white'],C['line'],1)
    b.text(1786,273,'四次跨面，三段组合任务',29,C['ink'],True)
    steps=[('① 正面入镜 → 背面','先认识前后位置对应。\n背面 R 镜车停在 R0，不急着推。'),
           ('② 背面爬升 → 开正面路','沿三级花架跳台到 K背，\n把正面叶片踏阶展开并锁住。'),
           ('③ 从 R0 返回 → 正面跳跃','回到背面光路，化光经 R0 返回。\n实体沿新踏阶到 K前，开背面轨桥。'),
           ('④ 原路跳回 → 再入背面','沿踏阶返回正面 R0 落点，\n借同一束光逆向回背面镜车旁。'),
           ('⑤ 推镜过桥 → 返回墙后','实体推 R 镜车：R0 → R1。\n停在对应镜座，再化光回正面墙后。'),
           ('⑥ 光鳞受光 → 进入雨槽','R1 对应的固定镜把光送向背鳞。\n出口舒展，实体向右进入第 08 关。')]
    for i,(title,txt) in enumerate(steps):
        y=338+i*145;b.text(1788,y,title,25,C['green'],True);b.text(1788,y+43,txt,21,C['muted'])
    b.rect(60,1320,2280,220,C['white'],C['line'],1)
    cards=[('一面改变另一面的可达性','K背 展开正面踏阶 → 才够到 K前。\nK前 开背面轨桥 → 镜车才能越过叶墙后方。'),
           ('跳跃承担具体目的','背面向上找开路机关；正面错层跳到远端。\n已展开的平台保留，回访有可以读懂的路线。'),
           ('回程落点由镜车决定','R0 回到叶墙之前；R1 回到叶墙之后。\n只移动一只镜车，不增加第二束光或自由传送。')]
    for i,(title,txt) in enumerate(cards):
        x=86+i*753;b.text(x,1344,title,28,C['green'],True);b.text(x,1395,txt,23,C['muted'])
    b.text(62,1575,'图中上栏 A、下栏 B 是同一藤架的前后展开，不是上下楼；紫线代表纵深通光，不是可跳跃的竖井。',24,C['ink'])
    b.text(62,1620,'正面 R0 的光在落脚点旁截停，照不到跳台区；必须恢复实体跳跃。背面桥未开时，有实际断轨和挡叶。',23,C['muted'])
    b.text(62,1661,'站光三秒化光，手动前后移动；开路机关锁止保留。只有 R0 / R1 镜窗可通向正面，其余位置有实心背衬。',23,C['muted'])
    b.text(62,1701,'v02 设计提案 · 主图为两处机关已开的完成态 · 尚未实装；反射、跨层镜映射、跳距、碰撞与镜车扫掠需灰盒验证。',20,C['muted'])
    return b

def paired(b,x,y,phase):
    scale=.62
    paste(b,face(True,phase,False,False),x,y,scale)
    paste(b,face(False,phase,False,False),x,y+287,scale)
    b.text(x+13,y+12,'正面 A',19,C['ink'],True);b.text(x+13,y+299,'背面 B',19,C['ink'],True)
    b.arrow([(x+155,y+192),(x+155,y+479)],C['purple'],3,True)
    rx=1270 if phase>=4 else 430
    b.arrow([(x+rx*scale,y+479),(x+rx*scale,y+192)],C['purple'],3,True)
    b.tag(x+235,y+216,'R0',C['green'],17);b.tag(x+760,y+216,'R1',C['green'],17)
    b.tag(x+453,y+321,'K背',LINK,18);b.tag(x+552,y+68,'K前',LINK,18)
    if phase==1:
        b.walk([(x+195,y+484),(x+230,y+442),(x+320,y+402),(x+405,y+365),(x+483,y+335)])
        b.tag(x+550,y+395,'先爬升到 K背',C['blue'],20)
    elif phase==2:
        b.walk([(x+295,y+199),(x+357,y+179),(x+438,y+141),(x+512,y+116),(x+577,y+94)])
        b.tag(x+642,y+404,'背桥还未打开',C['muted'],20)
    elif phase==3:
        b.arrow([(x+580,y+126),(x+510,y+148),(x+433,y+175),(x+300,y+205)],C['blue'],3,True)
        b.arrow([(x+295,y+517),(x+750,y+517)],C['blue'],4)
        b.tag(x+554,y+396,'回到背面，推镜过桥',C['blue'],20)
    else:
        b.walk([(x+807,y+203),(x+959,y+203)])
        b.tag(x+567,y+402,'同一镜车已在 R1',C['green'],20)

def states():
    b=Sheet('07-linked-faces-states-v02','07  四个状态｜前后交替，机关与路线互相依赖',
      '每个框内都把正面与背面同时展开，光路通过紫线连接；上下两段金线属于同一束光。')
    panels=[(60,155,1,'① 背面跳跃，准备正面','K背未开：正面踏阶收起，背面断轨与挡叶仍封闭。'),
            (1240,155,2,'② 返回正面，跳到 K前','K背已开：正面踏阶展开；沿其跳到 K前，准备背桥。'),
            (60,900,3,'③ 回背面，推同一镜车','两机关已开：正面原路返回，背面镜车才能 R0 → R1。'),
            (1240,900,4,'④ 返回正面墙后，喂养背鳞','R 位于 R1：纵深回程落点移到叶墙之后，出口开放。')]
    for x,y,phase,title,note in panels:
        b.rect(x,y,1100,710,C['white'],C['line'],1)
        b.text(x+20,y+15,title,29,C['green'],True)
        paired(b,x+24,y+73,phase)
        b.text(x+22,y+660,note,21,C['muted'])
    b.text(62,1650,'关键组合：开正面踏阶 → 到 K前；开背面轨桥 → 推 R 镜车；把 R 推过叶墙的世界 x 位置 → 返回墙后。',23,C['ink'])
    b.text(62,1694,'镜车两处停靠位有明确配准标记；中途停错仍在背面安全轨道，可继续推或回推，不会把玩家困在另一面。',22,C['muted'])
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
            a=dict(id=f'linked-faces-{n:05d}',type={'rect':'rectangle','poly':'line'}.get(typ,typ),x=x,y=y+bi*1840,width=max(w,.01),height=max(h,.01),angle=0,strokeColor=e.get('color',e.get('stroke')) or 'transparent',backgroundColor=e.get('fill') or 'transparent',fillStyle='solid',strokeWidth=e.get('lw',1),strokeStyle='dashed' if e.get('dash') else 'solid',roughness=0,opacity=100,groupIds=[],frameId=None,roundness=None,seed=n*31,version=1,versionNonce=n*17,isDeleted=False,boundElements=None,updated=1793836800000,link=None,locked=False)
            if typ in ('line','poly'):
                pp=pts+[pts[0]] if typ=='poly' else pts;a.update(points=[[px-x,py-y] for px,py in pp],lastCommittedPoint=None,startBinding=None,endBinding=None,startArrowhead=None,endArrowhead=None)
            if typ=='text':a.update(fontSize=e['size'],fontFamily=2,text=e['t'],originalText=e['t'],textAlign='left',verticalAlign='top',containerId=None,autoResize=True,lineHeight=1.35,baseline=e['size'])
            result.append(a)
    (OUT/'level07-linked-faces-v02.excalidraw').write_text(json.dumps(dict(type='excalidraw',version=2,source='local',elements=result,appState=dict(viewBackgroundColor=C['paper'],gridSize=None),files={}),ensure_ascii=False),encoding='utf-8')
    return result

if __name__=='__main__':
    boards=[overview(),states()]
    for b in boards:b.save()
    elements=editable(boards)
    for b in boards:
        ET.parse(OUT/(b.key+'.svg'))
        with Image.open(OUT/(b.key+'.png')) as im:assert im.size==(dc.W,dc.H)
    assert len({e['id'] for e in elements})==len(elements)
    issues=[]
    for bi,b in enumerate(boards):
        for e in b.items:
            if e['type']=='text':
                f=ImageFont.truetype(BOLD if e['bold'] else FONT,e['size'])
                if e['x']+max(f.getlength(l) for l in e['t'].split('\n'))>dc.W-20:issues.append([bi,e['t'],'right overflow'])
                if e['y']+e['size']*1.35*len(e['t'].split('\n'))>dc.H:issues.append([bi,e['t'],'bottom overflow'])
    assert not issues,issues
    (OUT/'validation.json').write_text(json.dumps(dict(dimensions=[dc.W,dc.H],boards=2,svg_xml='pass',editable_objects=len(elements),unique_ids=True,text_canvas_bounds='pass',unity_graybox='not performed',excalidraw_editor_import='not performed'),ensure_ascii=False,indent=2),encoding='utf-8')
    print(json.dumps({'output':str(OUT),'objects':len(elements)},ensure_ascii=False))

from draw_core import *
import xml.etree.ElementTree as ET
import zipfile

ROPE='#80715E'
def plate(b,x,y,w=135,seed=False):
    b.rect(x,y,w,9,'#C5D6C4',C['green'],3)
    b.line([(x+8,y+18),(x+15,y+10),(x+23,y+18),(x+31,y+10),(x+39,y+18)],C['green'],2)
    if seed:
        b.ellipse(x+35,y-25,30,25,'#BC9A68','#806A46',2)
        b.line([(x+45,y-23),(x+52,y-3)],'#806A46',2)
        b.line([(x+28,y-6),(x+28,y+2),(x+72,y+2),(x+72,y-6)],C['muted'],2)

def leaf(b,pts,ghost=False):
    if ghost:b.line(pts+[pts[0]],C['ghost'],2,True)
    else:b.poly(pts,'#D5E4CA',C['green'],2)

def make_main():
    b=Board('04-weight-route','04  蓄水浅池｜留下重量，带走光','复合谜题：压板需要持续重量；导向叶槽能运送种荚，却也会挡光。场景仍为左岸—中岛—右岸泵房。')
    # Replace irrelevant cross-depth legend with the mechanical connection legend.
    for item in b.items:
        if item['type']=='text' and item.get('t')=='纵深跨层':item['t']='绳索联动';item['color']=ROPE
        if item.get('stroke')==C['purple']:item['stroke']=ROPE
        if item.get('fill')==C['purple']:item['fill']=ROPE
    b.rect(80,600,1440,180,C['water'],None)
    for yy in (625,665,715,760):b.line([(90,yy),(1510,yy)],'#BED7DD',1)
    b.ground(80,530,270,250);b.ground(600,525,300,255);b.ground(1210,530,310,250)
    b.ground(620,440,65,30);b.ground(690,365,65,30)
    b.ground(775,300,35,30);b.ground(845,300,55,30)
    b.line([(775,266),(775,300)],ROPE,5);b.line([(900,266),(900,300)],ROPE,5)
    b.line([(810,298),(845,298)],C['green'],5)
    b.rect(947,205,66,254,'#CCD9C3',C['green'],2)
    b.rect(947,491,66,289,'#CCD9C3',C['green'],2)
    b.line([(956,217),(1004,320),(956,430)],'#95AC8E',2)
    b.line([(1004,503),(956,620),(1004,764)],'#95AC8E',2)
    b.label(90,230,'← 第 03 关：暗渠出口',20,C['blue'])
    b.label(1200,230,'第 05 关 → 泵房管口',20,C['blue'])
    b.beast(1320,295,195,155,1435,350,'下腹局部 / 灰暗光鳞')
    src=(175,285);m=(280,475);r=(1280,475);t=(1435,350)
    b.source(*src,'墙缝日光');b.ray([src,m,r,t])
    b.mirror(*m,src,r,'固定导光片',fixed=True);b.mirror(*r,m,t,'末端调镜')
    b.spirit(130,524);b.walk([(150,516),(232,516)])
    b.lightmove([(321,453),(641,453)]);b.lightmove([(867,454),(928,454),(944,475),(1017,475),(1040,454),(1235,454)])
    b.walk([(1320,515),(1490,515)])
    plate(b,716,517,145,True)
    b.ellipse(805,467,16,16,'#FFF2CD',C['orange'],2)
    b.label(710,556,'种荚在左侧浅窝\n右侧留给精灵充能',18,C['green'])
    b.label(384,670,'第一段水面',19,C['muted']);b.label(977,670,'第二段水面',19,C['muted'])
    b.label(365,740,'两段水面均不可直接跳过；取种荚的支台只从中岛可达。',19,C['muted'])

    # Same small source object at its original and final locations.
    b.ellipse(833,273,31,26,None,C['ghost'],2)
    b.label(760,225,'④ 高处种荚（原位虚线）',18,C['muted'])
    b.label(400,235,'储槽有挡边；放下叶槽\n才打开唯一出料口',18,C['green'])
    b.arrow([(879,281),(824,281)],C['green'],3)
    # Upper root gutter stays above the optical line.
    b.line([(820,305),(770,407)],ROPE,8)
    b.line([(831,309),(782,411)],ROPE,2)
    # Raised delivery leaf is clear of the light; its old delivery position crosses the beam.
    leaf(b,[(770,410),(780,365),(820,337),(806,384)])
    leaf(b,[(770,410),(782,415),(751,510),(729,510)],True)
    b.line([(778,410),(690,490)],C['green'],2)
    b.ellipse(677,480,23,23,None,C['green'],2)
    b.arrow([(803,409),(814,430),(798,452)],C['green'],2)
    b.label(430,330,'③ 放下叶槽送种荚\n⑤ 落稳后收起，让光通过',18,C['green'])
    b.label(425,565,'叶槽放下时会挡住主光\n浅窝独立，不随叶槽收起',18,C['muted'])
    b.line([(625,563),(745,490)],C['muted'],1)

    # Rope has a turning pulley at each bend and a remote lifted curtain.
    b.line([(855,517),(920,517),(920,263),(980,263),(980,337)],ROPE,3)
    for x,y in [(920,263),(980,263)]:b.ellipse(x-11,y-11,22,22,C['white'],ROPE,2)
    leaf(b,[(965,295),(987,319),(997,388),(979,411),(960,383)])
    leaf(b,[(965,407),(987,431),(997,500),(979,523),(960,495)],True)
    b.label(1035,300,'② 远端挡光叶帘',20,C['green'])
    b.label(1035,340,'有重量：抬起\n无重量：回落',19,C['muted'])
    b.line([(1030,387),(987,390)],C['muted'],2)
    b.label(1028,555,'根幕上接棚架，挡住跳跃绕行\n窄口只容光核，虚线为关帘',17,C['muted'])
    b.label(1035,398,'绳索沿后侧布置',17,C['muted'])
    # The platforming branch remains on the island side of the curtain and second water gap.
    b.jump((613,510),(650,429));b.jump((657,428),(719,354));b.jump((727,353),(798,289))
    b.arrow([(789,287),(871,287)],C['blue'],3)
    b.label(80,392,'先沿已有光路到中岛',18,C['blue'])
    b.badge(462,416,1);b.badge(875,559,2);b.badge(669,487,3);b.badge(897,288,4);b.badge(805,376,5);b.badge(1140,430,6);b.badge(1410,506,7)
    b.line([(1500,476),(1495,530)],C['green'],3)
    b.label(1230,620,'末镜喂光 → 根缝松开',18,C['green'])
    b.footer([('化光到中岛','沿固定第一段光'),('试踩重量压板','观察远端叶帘开合'),('放下导向叶槽','准备种荚滚落路线'),('连跳、推种荚','重物落入压板浅窝'),('收起导向叶槽','种荚留下，恢复光路'),('再次化光过水','稳定通光，不必抢跑'),('右岸调镜喂鳞','泵房入口根缝松开')],
             '本版新增提案：实体有重量，光态不触发重量压板。种荚压板＋叶槽收起，才构成稳定通路。')
    for item in b.items:
        if item['type']=='text' and item.get('t','').startswith('v01 非比例'):
            item['t']='v02 设计提案 · 图示最终解法 · 跳距、压板阈值、叶槽运动与安全恢复仍需灰盒验证。'
    return b

def make_states():
    b=Board('04-weight-states','04  机关状态｜让重量留下，让光线通过','观察两条因果：压板控制远处叶帘；导向叶槽放下时会阻挡主光。没有倒计时，也不要求先失败一次。')
    b.items=[e for e in b.items if not (e['type']=='rect' and e.get('y')==205)]
    # Simpler state legend instead of the generic route legend.
    b.items=[e for e in b.items if not (e.get('y',0)>=130 or (e.get('pts') and min(p[1] for p in e['pts'])>=130))]
    states=[
        (60,170,'A  实体踩板','人压着，叶帘抬起；暂时能通光。','人一旦离开，机关就无法保持。',True,False,False),
        (825,170,'B  化光后失重','压板弹起，远处叶帘回落。','只遮前方光路；当前岛面仍安全。',False,False,False),
        (60,570,'C  种荚已经压板','叶帘保持打开，但放下的叶槽挡光。','运输完成，还需要把叶槽收起。',False,True,True),
        (825,570,'D  收起叶槽后','种荚留在浅窝；整条光路持续接通。','精灵可在旁边化光，独自过水。',False,True,False),
    ]
    for x,y,title,cap,note,human,seed,down in states:
        b.rect(x,y,715,360,C['white'],C['line'],2)
        b.text(x+20,y+18,title,26,C['ink'],True)
        b.text(x+20,y+61,cap,19,C['muted'])
        beam_y=y+190;ground_y=y+235;px=x+260;curtain_x=x+550
        b.ground(x+25,ground_y,390,35);b.ground(x+600,ground_y,90,35)
        plate(b,px,ground_y-8,125,seed)
        opened=human or seed
        leaf_x=x+205
        if down:
            leaf(b,[(leaf_x,beam_y-35),(leaf_x+20,beam_y-25),(leaf_x+78,ground_y-12),(leaf_x+57,ground_y-12)])
            end=(leaf_x+40,beam_y)
        elif not opened:end=(curtain_x,beam_y)
        else:end=(x+675,beam_y)
        b.ray([(x+30,beam_y),end])
        if not down:leaf(b,[(leaf_x,beam_y-35),(leaf_x+22,beam_y-83),(leaf_x+58,beam_y-95),(leaf_x+43,beam_y-47)])
        ly=beam_y-93 if opened else beam_y-25
        leaf(b,[(curtain_x-10,ly),(curtain_x+9,ly+12),(curtain_x+16,ly+61),(curtain_x,ly+75),(curtain_x-15,ly+53)])
        b.line([(px+120,ground_y-8),(x+450,ground_y-8),(x+450,beam_y-106),(curtain_x,beam_y-106),(curtain_x,ly)],ROPE,2)
        if human or down:b.spirit(px+90,ground_y-12)
        else:
            b.ellipse(px+70,beam_y-8,16,16,'#F9D978',C['orange'],2)
            if opened and not down:b.lightmove([(px+101,beam_y-16),(x+664,beam_y-16)])
        b.label(x+20,y+283,note,19,C['green'] if seed else C['muted'])
        b.text(x+20,y+321,'叶槽：'+('放下' if down else '收起')+'    叶帘：'+('抬起' if opened else '关闭'),18,C['muted'])
    b.text(62,976,'解法：用种荚持续压住机关，再收起叶槽清空光路。',24,C['ink'],True)
    b.text(62,1022,'可通过走离压板提前观察失重反馈；化光试验不强制跌水，不设置机关限时。',21,C['muted'])
    b.text(62,1064,'“光态无重量”是本版新增设计提案；安全恢复与光路阻断细节尚需验证。',18,C['muted'])
    return b

boards=[make_main(),make_states()]
for b in boards:b.save()
elements=[];serial=0
for bi,b in enumerate(boards):
    ox=0;oy=bi*1180
    for e in b.items:
        serial+=1;typ=e['type'];x=e.get('x',0);y=e.get('y',0);w=e.get('w',0);h=e.get('h',0)
        if typ in ('line','poly'):
            pts=e['pts'];x=min(p[0] for p in pts);y=min(p[1] for p in pts);w=max(p[0] for p in pts)-x;h=max(p[1] for p in pts)-y
        if typ=='text':
            f=ImageFont.truetype(BOLD if e['bold'] else FONT,e['size']);w=max(f.getlength(l) for l in e['t'].split('\n'));h=e['size']*1.35*len(e['t'].split('\n'))
        item=dict(id=f'weight-{serial:04d}',type={'rect':'rectangle','poly':'line'}.get(typ,typ),x=x+ox,y=y+oy,width=max(w,.01),height=max(h,.01),angle=0,
                  strokeColor=e.get('color',e.get('stroke')) or 'transparent',backgroundColor=e.get('fill') or 'transparent',fillStyle='solid',strokeWidth=e.get('lw',1),strokeStyle='dashed' if e.get('dash') else 'solid',roughness=0,opacity=100,
                  groupIds=[],frameId=None,roundness=None,seed=serial*31,version=1,versionNonce=serial*17,isDeleted=False,boundElements=None,updated=1793750400000,link=None,locked=False)
        if typ in ('line','poly'):
            pp=pts+[pts[0]] if typ=='poly' else pts
            item.update(points=[[px-x,py-y] for px,py in pp],lastCommittedPoint=None,startBinding=None,endBinding=None,startArrowhead=None,endArrowhead=None)
        if typ=='text':item.update(fontSize=e['size'],fontFamily=2,text=e['t'],originalText=e['t'],textAlign='left',verticalAlign='top',containerId=None,autoResize=True,lineHeight=1.35,baseline=e['size'])
        elements.append(item)
(OUT/'level04-weight-v02.excalidraw').write_text(json.dumps(dict(type='excalidraw',version=2,source='local',elements=elements,appState=dict(viewBackgroundColor=C['paper'],gridSize=None),files={}),ensure_ascii=False),encoding='utf-8')
for b in boards:
    ET.parse(OUT/(b.key+'.svg'))
    with Image.open(OUT/(b.key+'.png')) as im:assert im.size==(W,H)
assert len(set(e['id'] for e in elements))==len(elements)
(OUT/'validation.json').write_text(json.dumps(dict(boards=2,size=[W,H],svg_xml='pass',unique_ids=True,elements=len(elements),unity_validation='not performed',new_rule='proposal: light form has no weight'),ensure_ascii=False,indent=2),encoding='utf-8')
with zipfile.ZipFile(OUT/'level04-weight-v02-pack.zip','w',zipfile.ZIP_DEFLATED) as z:
    for f in sorted(OUT.iterdir()):
        if f.is_file() and f.suffix in ['.png','.svg','.excalidraw','.md','.json','.py']:z.write(f,f.name)
print(json.dumps(dict(output=str(OUT),elements=len(elements)),ensure_ascii=False))

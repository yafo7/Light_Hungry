from pathlib import Path
import math, json, zipfile, xml.etree.ElementTree as ET
import draw_core as dc
from draw_core import Board, C, FONT, BOLD, ImageFont, Image

OUT=Path(__file__).resolve().parent
dc.OUT=OUT; dc.W=2200; dc.H=1560; dc.S=2
PURPLE='#8A739D'; GLASS='#75A7AE'; RUST='#94765D'

class Sheet(Board):
    def __init__(self,key,title,sub):
        self.key=key;self.items=[]
        self.rect(0,0,dc.W,dc.H,C['paper'])
        self.text(60,38,title,42,C['ink'],True)
        self.text(62,102,sub,23,C['muted'])
    def tag(self,x,y,t,col=C['green'],size=23):
        self.label(x,y,t,size,col)
    def wheel(self,x,y,tag):
        self.ellipse(x-23,y-23,46,46,C['white'],C['green'],3)
        for a in range(0,360,60):
            a=math.radians(a);self.line([(x,y),(x+20*math.cos(a),y+20*math.sin(a))],C['green'],2)
        self.ellipse(x-5,y-5,10,10,C['green'])
        self.tag(x-20,y+32,tag,size=18)
    def lever(self,x,y,tag):
        self.rect(x-22,y+13,44,9,C['ground'],C['green'],2)
        self.line([(x,y+12),(x+15,y-21)],C['green'],5)
        self.ellipse(x+9,y-27,13,13,C['white'],C['green'],2)
        self.tag(x-22,y+31,tag,size=18)
    def pad(self,x,y):
        self.rect(x-29,y-6,58,6,'#D5E8DB',C['green'],2)
    def legend(self):
        self.rect(60,151,2080,62,C['white'],C['line'],1)
        self.walk([(80,184),(139,184)]);self.text(153,169,'实体行走 / 跳跃',21)
        self.ray([(387,184),(449,184)]);self.text(464,169,'有效光束',21)
        self.lightmove([(657,184),(718,184)]);self.text(733,169,'化光行动（可回退）',21)
        self.arrow([(1018,184),(1075,184)],PURPLE,3,True);self.text(1090,169,'机关控制关系',21)
        self.glass(1349,180,50);self.text(1415,169,'透光玻璃',21)
        self.ellipse(1617,171,23,23,C['white'],C['green'],3);self.text(1655,169,'机关 / 安全落点',21)
        self.rect(1915,175,18,20,C['scale']);self.text(1945,169,'待照光鳞',21)

def main():
    b=Sheet('05-pumphouse-route-v02','05  废弃泵房｜化光进入机关位，恢复实体改变通路',
      '第四关池岸管口 → 泵房检修井 → 温室下层苗床　｜　新增 3 处化光到达的控制位置，穿插一次实体折返攀升')
    b.legend()
    b.rect(60,240,1580,970,C['white'],C['line'],1)
    b.rect(1670,240,470,970,C['white'],C['line'],1)
    b.rect(100,280,1500,880,'#EDF0EB')
    # Orthographic shaft, distant structure deliberately subdued.
    for x in (105,390,800,1100,1595):
        b.line([(x,315),(x,1150)],'#DCE3DD',12)
    b.line([(100,1150),(100,280),(1600,280),(1600,1150)],'#B7C2B9',4)
    b.rect(450,1105,1150,55,C['water'])
    for y in (1120,1140):b.line([(465,y),(1585,y)],'#BBD3D8',2)
    b.ground(105,1160,345,35)
    b.ground(1260,985,300,45)
    b.glass(835,985,425)
    # The service route exists only once K2 is latched. Frames sit behind optical plane.
    for x,y,w in [(1400,900,125),(1300,815,125),(1400,730,125),(1120,645,180)]:
        b.ground(x,y,w,24)
        b.line([(x+w-12,y+24),(1550,y+65),(1550,y+24)],'#BCC7BE',2)
    # K2 sealed inspection chamber; light slit at right, no bodily doorway.
    b.rect(430,570,265,175,'#E3E7E1','#546B67',3)
    b.line([(440,583),(680,583)],'#BDC8BE',2)
    b.rect(680,701,25,32,C['white'])
    b.line([(680,701),(695,701)],'#546B67',3);b.line([(680,733),(695,733)],'#546B67',3)
    b.line([(430,745),(695,745)],C['ink'],5)
    # Top glass mezzanine and genuinely blocked final exit.
    b.rect(1280,289,255,13,'#D2DBD2',C['muted'],2)
    b.rect(1280,302,9,188,'#DBEEEE',GLASS,2)
    for y in (325,365,405,445):b.line([(1282,y+10),(1287,y)],GLASS,1)
    b.glass(1280,490,210)
    b.ground(1540,490,60,40)
    b.line([(1535,315),(1535,490)],C['green'],5)
    for y in (340,370,400,430,460):b.line([(1525,y-12),(1544,y+12)],C['green'],3)
    b.beast(1460,300,135,120,1530,361,'侧腹浅白 / 目标鳞灰暗')
    b.items=b.items[:-2]
    b.tag(1455,306,'侧腹：浅白\n光鳞：灰暗',C['muted'],17)
    # Source → M0 → A → B. All positions in one continuous XY section.
    s=(250,350);m0=(275,1095);a=(1240,920);bb=(575,680);c=(1210,580);d=(1390,425);t=(1530,361)
    b.source(*s,'窄窗 S1');b.ray([s,m0,a,bb])
    b.source(1210,315,'窄窗 S2');b.ray([(1210,315),c,d,t])
    b.mirror(*m0,s,a,'M0 固定',fixed=True)
    b.mirror(*a,m0,bb,'M1 固定',fixed=True)
    b.ellipse(bb[0]-9,bb[1]-9,18,18,'#F7D88A',C['gold'],2)
    b.mirror(*c,(1210,315),d,'M2 可调')
    b.mirror(*d,c,t,'M3 远控',fixed=True)
    b.line([d,(1420,308)],C['ghost'],3,True)
    b.tag(1300,338,'初始：照空墙',C['muted'],17)
    # K1 lifted opaque shutter with old closed position as dashed contour.
    gx=890;gy=920+(gx-1240)*(680-920)/(575-1240)
    b.rect(gx-11,gy-105,22,70,'#D8C6AC',RUST,2)
    b.line([(gx-11,gy-33),(gx+11,gy-33),(gx+11,gy+33),(gx-11,gy+33),(gx-11,gy-33)],RUST,2,True)
    b.arrow([(gx+26,gy-7),(gx+26,gy-85)],PURPLE,2)
    b.lever(1335,947,'K1 拉杆')
    b.wheel(490,700,'K2 卷轮')
    b.wheel(1460,454,'K3 手轮')
    for x,y in [(1210,985),(604,745),(1210,645),(1390,490)]:b.pad(x,y)
    # Control dependencies, rendered separately from light and walking.
    b.arrow([(1348,930),(1530,930),(1530,1060),(945,1060),(945,gy),(gx+18,gy)],PURPLE,2,True)
    b.arrow([(490,671),(490,537),(1580,537),(1580,861),(1527,861)],PURPLE,2,True)
    b.arrow([(1444,440),(1415,432)],PURPLE,3,True)
    # Physical and light-form route, numbering matches right column.
    b.spirit(150,1154);b.walk([(180,1147),(240,1147)])
    b.lightmove([(325,1065),(1183,910)])
    b.lightmove([(1190,880),(630,680)])
    b.lightmove([(635,716),(1184,916)])
    b.walk([(1280,970),(1365,970)])
    b.jump((1365,970),(1445,889));b.jump((1430,890),(1355,804))
    b.jump((1360,804),(1445,719));b.jump((1408,719),(1240,634))
    b.lightmove([(1232,551),(1350,449)])
    b.walk([(1432,475),(1586,475)])
    b.badge(575,1020,'1');b.badge(1045,823,'2');b.badge(1044,875,'3')
    b.badge(1470,826,'4');b.badge(1278,545,'5');b.badge(1472,530,'6')
    # Direct labels, clear of arrow paths and interactive icons.
    b.tag(126,1072,'入口',C['blue'],23)
    b.tag(113,1180,'← 第 04 关：池岸管口',C['blue'],19)
    b.tag(315,365,'进光孔无落脚处，不能借此跳关',C['muted'],19)
    b.tag(895,1100,'A｜高处玻璃台：先化光穿玻璃，再落脚拉 K1',C['green'],21)
    b.tag(465,596,'B｜窄缝检修舱',C['green'],22)
    b.tag(427,777,'恢复实体转 K2；再沿来光原路返回 A',C['green'],20)
    b.tag(710,657,'仅光核可穿的检修缝',C['muted'],19)
    b.line([(735,687),(699,714)],C['muted'],2)
    b.tag(750,719,'K1 抬起遮光片 G1',C['green'],20)
    b.tag(959,868,'③ 回退',C['orange'],18)
    b.tag(1010,693,'C｜继光台\n实体调 M2',C['green'],21)
    b.tag(1320,1049,'K1 → 遮光片 G1',PURPLE,19)
    b.tag(1340,611,'K2 → 展开 3 块跳台',PURPLE,19)
    b.rect(430,393,490,128,C['white'],C['line'],1)
    b.text(450,409,'先用光开路，再用双脚攀高',25,C['ink'],True)
    b.text(450,454,'S1 只负责进入机关舱及返程；\n登上 C 调镜，才能接通 S2 的上层通路。',20,C['muted'])
    b.tag(1034,389,'D｜顶部玻璃夹层',C['green'],22)
    b.tag(1042,440,'从 C 化光穿玻璃到 D\n实体转 K3 → 调整 M3',C['muted'],19)
    b.tag(1360,565,'喂光成功：出口根帘舒展',C['green'],19)
    b.tag(1370,251,'第 06 关 → 温室苗床',C['blue'],20)
    b.tag(775,316,'图示接通后的光路；按步骤逐段解锁',C['muted'],19)
    # Compact, legible sequence lane.
    b.text(1698,268,'行动顺序',30,C['ink'],True)
    steps=[
      ('01  化光抵达 A','井底 → 高处玻璃台。\n落回实体，拉 K1 抬起 G1。'),
      ('02  沿新光路进入 B','穿过检修窄缝。\n在舱内转 K2，展开外侧跳台。'),
      ('03  原路化光回 A','K2 保持开启，无须压住。\n返程沿同一束光手动回退。'),
      ('04  恢复实体爬升到 C','沿新展开跳台折返向上。\n调 M2，把 S2 日光送到 D。'),
      ('05  化光到 D，再操作','穿过高处玻璃底板。\n在安全地面转 K3，远调 M3。'),
      ('06  侧腹光鳞受光','持续对准目标，根帘舒展。\n实体短跳 → 检修孔 → 第 06 关。')]
    for i,(title,txt) in enumerate(steps):
        y=330+i*137
        b.text(1700,y,title,24,C['green'] if i in(0,1,4) else C['ink'],True)
        b.text(1700,y+42,txt,20,C['muted'])
        if i<5:b.line([(1700,y+117),(2110,y+117)],C['line'],1)
    b.text(1700,1160,'光态负责抵达；实体负责操作。',21,C['ink'],True)
    # Three design promises, separate from the geometry drawing.
    cards=[('A  玻璃台拉杆','抵达限制：高差与横距超出跳跃范围。','机关作用：打开通往 B 的遮光片。'),
           ('B  狭缝舱卷轮','抵达限制：完整外壳只留一条细缝。','机关作用：展开实体前往 C 的跳台。'),
           ('D  夹层内手轮','抵达限制：仅入射光可穿玻璃底板。','机关作用：远调末镜，照亮侧腹光鳞。')]
    for i,(title,l1,l2) in enumerate(cards):
        x=60+i*705;b.rect(x,1240,670,160,C['white'],C['line'],1)
        b.text(x+22,1260,title,27,C['green'],True)
        b.text(x+22,1307,l1,21,C['muted']);b.text(x+22,1345,l2,21,C['ink'])
    b.text(62,1431,'规则不变：站在有效光中 3 秒化光；手动沿光路前后移动。所有机关须恢复实体后操作；K1 / K2 开启后保持。',23,C['ink'])
    b.text(62,1480,'原版的玻璃规则保留：实体能站，光与光核能穿。S1 保证往返，S2 负责末段；两束光互不切断。',22,C['muted'])
    b.text(62,1521,'v02 空间与依赖草图 · 箭头略偏离光束便于阅读 · 非比例碰撞图，实际跳距、缝隙净空和光学角度仍需灰盒验证。',19,C['muted'])
    return b

def export_editable(b):
    elements=[]
    for serial,e in enumerate(b.items,1):
        typ=e['type'];x=e.get('x',0);y=e.get('y',0);w=e.get('w',0);h=e.get('h',0)
        if typ in ('line','poly'):
            pts=e['pts'];x=min(p[0] for p in pts);y=min(p[1] for p in pts);w=max(p[0] for p in pts)-x;h=max(p[1] for p in pts)-y
        if typ=='text':
            f=ImageFont.truetype(BOLD if e['bold'] else FONT,e['size']);w=max(f.getlength(l) for l in e['t'].split('\n'));h=e['size']*1.35*len(e['t'].split('\n'))
        a=dict(id=f'pump-v2-{serial:04d}',type={'rect':'rectangle','poly':'line'}.get(typ,typ),x=x,y=y,width=max(w,.01),height=max(h,.01),angle=0,
          strokeColor=e.get('color',e.get('stroke')) or 'transparent',backgroundColor=e.get('fill') or 'transparent',fillStyle='solid',strokeWidth=e.get('lw',1),strokeStyle='dashed' if e.get('dash') else 'solid',roughness=0,opacity=100,
          groupIds=[],frameId=None,roundness=None,seed=serial*31,version=1,versionNonce=serial*17,isDeleted=False,boundElements=None,updated=1793750400000,link=None,locked=False)
        if typ in ('line','poly'):
            pp=pts+[pts[0]] if typ=='poly' else pts;a.update(points=[[px-x,py-y] for px,py in pp],lastCommittedPoint=None,startBinding=None,endBinding=None,startArrowhead=None,endArrowhead=None)
        if typ=='text':a.update(fontSize=e['size'],fontFamily=2,text=e['t'],originalText=e['t'],textAlign='left',verticalAlign='top',containerId=None,autoResize=True,lineHeight=1.35,baseline=e['size'])
        elements.append(a)
    (OUT/'level05-light-controls-v02.excalidraw').write_text(json.dumps(dict(type='excalidraw',version=2,source='local',elements=elements,appState=dict(viewBackgroundColor=C['paper'],gridSize=None),files={}),ensure_ascii=False),encoding='utf-8')
    return elements

if __name__=='__main__':
    board=main();board.save();elements=export_editable(board)
    ET.parse(OUT/(board.key+'.svg'))
    with Image.open(OUT/(board.key+'.png')) as im:assert im.size==(dc.W,dc.H)
    assert len(set(x['id'] for x in elements))==len(elements)
    (OUT/'validation.json').write_text(json.dumps(dict(dimensions=[dc.W,dc.H],svg_xml='pass',editable_objects=len(elements),unique_ids=True,unity_graybox='not performed',excalidraw_editor_import='not performed'),indent=2),encoding='utf-8')
    print(json.dumps(dict(output=str(OUT),objects=len(elements))))

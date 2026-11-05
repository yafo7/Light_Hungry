"""Level 06 design proposal; does not change the Unity project."""
from pathlib import Path
import copy, json, math, zipfile, xml.etree.ElementTree as ET
import draw_core as dc
from draw_core import C, FONT, BOLD, Image, ImageFont
from design_helpers import Sheet

OUT=Path(__file__).resolve().parent
dc.OUT=OUT; dc.W=2400; dc.H=1740; dc.S=2
S=(85,335); A=(210,650); B=(1430,650); CC=(380,170)
PH=(850,210); PL=(850,715); D=(1320,180); T=(1490,96)
F=(620,170+(620-380)*40/470)
STOP=(1095,170+(1095-380)*40/470)
G=(210+(650-440)*170/480,440)
RUST='#9A785D'; PALE='#EEF1E9'; RED='#AD5F50'

class Local(Sheet):
    def __init__(self): self.items=[]

def mount(b,x,y,high):
    """A real carrying tray; glass shard is below lower beam when tray is low."""
    b.ground(x-100,y,210,16)
    b.poly([(x-100,y+16),(x-76,y+36),(x+86,y+36),(x+110,y+16)],'#DDC6AF',RUST,2)
    for xx in [x-88,x+98]:
        b.line([(xx,55),(xx,y-3)],RUST,3)
        b.ellipse(xx-11,35,22,22,'#E9D6B8',RUST,2)
    # Short mirror stand, not a long rod crossing the lower light beam.
    b.line([(x,y),(x,y-20)],RUST,4)
    yy=y-35
    b.mirror(x,yy,CC,D,tag=None,fixed=True)
    # Remove the generic long mirror pedestal: the tray's support is above.
    b.items.pop(-3)

def scene(mode, labels=True):
    b=Local(); high=mode in ('initial','raised','ready','final')
    open_g=mode not in ('initial','lowered')
    upper=mode in ('wrong','ready','final')
    b.rect(0,0,1600,820,C['white'],C['line'],1)
    b.rect(20,18,1560,775,PALE)
    # Background greenhouse ribs are visual only, not collision platforms.
    for x in [40,355,670,985,1300,1570]:
        b.line([(x,35),(x,785)],'#DCE3D8',5)
    b.line([(20,105),(350,22),(1250,22),(1580,105)],'#C1CDC2',3)
    b.rect(520,740,780,60,C['water'])
    for y in [760,783]: b.line([(535,y),(1285,y)],'#BDCDD0',2)
    b.ground(55,710,465,90); b.ground(1300,710,255,90)
    b.glass(315,230,205); b.ground(1260,240,300,30)
    # Fixed wall steps let an entity on the LOW tray get back into the beam.
    b.ground(960,720,65,14); b.ground(1025,695,65,14)
    b.line([(970,734),(995,775),(1008,734)],C['ghost'],2)
    b.line([(1035,709),(1060,775),(1073,709)],C['ghost'],2)
    # Opaque greenhouse baffle: only the optical slit admits the upper path.
    b.rect(610,22,24,F[1]-27,'#BBC7B6',C['muted'],2)
    b.rect(610,F[1]+5,24,610-F[1]-5,'#BBC7B6',C['muted'],2)
    # A miss is absorbed before it can become an unintended route to shore B.
    # This second hanging blind reaches the roof; it has no exposed top to land on.
    right_slot=PH[1]+(D[1]-PH[1])*(STOP[0]-PH[0])/(D[0]-PH[0])
    b.rect(STOP[0],22,13,right_slot-27,'#99A58F',C['muted'],2)
    b.rect(STOP[0],right_slot+5,13,610-right_slot-5,'#99A58F',C['muted'],2)
    # G is a retractable opaque leaf blind, separate from the aiming baffle F.
    if open_g:
        b.rect(G[0]-20,G[1]-46,40,12,'#BDCEB5',C['green'],2)
        b.line([(G[0]-13,G[1]-29),(G[0]+13,G[1]+25)],C['ghost'],2,True)
    else:
        b.rect(G[0]-11,G[1]-40,22,80,'#8FA283',C['green'],3)
    # Fixed maintenance latch K remains low when the nursery tray is lifted.
    b.rect(765,714,53,29,'#D2DDD1',C['green'],2)
    b.lever(792,723,'')
    b.line([(778,719),(715,719),(715,468),(G[0]+25,468)],C['purple'],2,True)
    mount(b,850,245 if high else 750,high)
    # H on shore B is the only lift controller.
    b.wheel(1490,684,'')
    b.line([(1490,660),(1490,52),(948,52)],RUST,2)
    b.beast(1425,55,130,130,*T,'')
    b.items=b.items[:-2]
    if mode=='final':
        b.poly([(T[0]-19,T[1]),(T[0],T[1]-24),(T[0]+19,T[1]),(T[0],T[1]+24)],'#F4D67A',C['gold'],2)
    b.source(*S,'')
    if not upper: path=[S,A,B]
    elif mode=='wrong': path=[S,A,CC,STOP]
    else: path=[S,A,CC,PH,D,T if mode=='final' else (1420,38)]
    b.ray(path)
    b.mirror(*A,S,CC if upper else B,tag=None)
    b.mirror(*CC,A,PH if upper else (620,320),tag=None)
    b.mirror(*D,PH,T if mode=='final' else (1420,38),tag=None)
    # The exit is visibly held shut until the creature's scale has been fed.
    gate_x=1540
    b.line([(gate_x,185),(gate_x,204 if mode=='final' else 240)],C['green'],4)
    if mode!='final':
        for yy in (198,216,233):b.line([(gate_x-10,yy-8),(gate_x+9,yy+7)],C['green'],3)
    if upper: b.line([A,B],C['ghost'],2,True)
    # Entity / light flow is kept separate from the gold optics line.
    if mode=='initial':
        b.lightmove([(250,625),(1385,625)])
        b.arrow([(1160,300),(1160,670)],C['purple'],4,True)
    elif mode=='lowered':
        b.lightmove([(1370,625),(795,625)])
        b.arrow([(795,650),(795,738)],C['blue'],5)
        b.spirit(795,743)
    elif mode=='opened':
        b.spirit(795,743)
        b.walk([(820,738),(940,738),(985,708),(1055,684)])
        b.lightmove([(1075,628),(1380,628)])
    elif mode=='raised':
        b.arrow([(1160,670),(1160,300)],C['purple'],4,True)
        b.lightmove([(1380,677),(255,677)])
        b.spirit(1450,705)
    elif mode in ('ready','final'):
        b.lightmove([(235,602),(404,207)])
        b.lightmove([(431,148),(850,187),(1290,158)])
        if mode=='final':b.walk([(1360,225),(1540,225)])
    elif mode=='wrong':
        b.lightmove([(355,255),(228,602)])
        b.ellipse(PH[0]-31,PH[1]-31,62,62,None,RED,3)
    if labels:
        labs=[(130,744,'A｜改道镜',C['green']), (1340,744,'B｜升降卷轮',C['green']),
              (330,267,'C｜校准镜',C['green']), (1268,280,'D｜末镜 / 出口',C['green']),
              (45,285,'唯一棚缝来光',C['gold']), (284,500,'G 遮光帘',C['green']),
              (664,545,'F 不透光挡板\n只有一条导光窄缝',C['muted']),
              (930,620,'固定回光阶',C['muted']), (665,785,'沟槽：不能直接跳过',C['muted'])]
        for x,y,t,c in labs:b.tag(x,y,t,c,20)
        b.tag(704,308 if high else 773,'P｜高位接光' if high else 'P｜低位落脚',C['green'],22)
        b.tag(689,656,'K 固定在低处\n必须站 P 才够到',C['purple'],20)
        b.tag(1330,350,'肋侧浅白；暗鳞受光后发亮',C['muted'],18)
    return b

def paste(dst,src,x,y,scale=1):
    for original in src.items:
        e=copy.deepcopy(original)
        for k in ['x','w','size','lw']: 
            if k in e:e[k]=e[k]*scale
        for k in ['y','h']:
            if k in e:e[k]=e[k]*scale
        if 'x' in e:e['x']+=x
        if 'y' in e:e['y']+=y
        if 'pts' in e:e['pts']=[(px*scale+x,py*scale+y) for px,py in e['pts']]
        if 'size' in e:e['size']=max(11,round(e['size']))
        dst.items.append(e)

def legend(b):
    b.rect(60,143,2280,67,C['white'],C['line'],1)
    b.walk([(85,178),(142,178)]);b.text(156,161,'实体 / 恢复后下落',22)
    b.ray([(450,178),(513,178)]);b.text(528,161,'当前唯一光束',22)
    b.lightmove([(800,178),(865,178)]);b.text(880,161,'化光往返',22)
    b.line([(1090,178),(1150,178)],C['ghost'],3,True);b.text(1165,161,'旧路 / 另一档位置',22)
    b.arrow([(1500,178),(1560,178)],C['purple'],3,True);b.text(1575,161,'机械联动',22)
    b.glass(1810,177,70);b.text(1895,161,'透光玻璃台',22)
    b.rect(2140,166,22,22,C['scale']);b.text(2180,161,'目标暗鳞',22)

def overview():
    b=Sheet('06-combination-route-v02','06  温室苗床｜途中落脚，往返换位',
      '从“逐站转镜”升级为组合规划：一只托盘，先承担落脚，再承担接光。单束光 · 无计时机关 · 可回退修正')
    legend(b)
    paste(b,scene('final'),60,245)
    # The final solution is the only gold path. Previous lower configuration ghosted.
    b.tag(80,258,'主图：最终通光状态；低位操作见下方与分步图',C['ink'],23)
    b.line([(810,995),(1020,995)],C['ghost'],3,True)
    b.line([(910,960),(910,978)],C['ghost'],3,True)
    b.tag(740,1100,'P 的低位轮廓（高低两档，不能同时存在）',C['muted'],21)
    b.tag(84,1044,'← 第 05 关：泵房检修口',C['blue'],20)
    b.tag(1335,655,'→ 第 07 关：叶幕走廊',C['blue'],20)
    b.rect(1700,245,640,885,C['white'],C['line'],1)
    b.text(1728,268,'玩家要完成的六步',30,C['ink'],True)
    steps=[('① 先去 B：降下托盘','A 保持下层光路。化光到 B，\n将 P 从高位放到低位检修挡。'),
           ('② 回程途中：主动脱光','沿 B → A 回退，在 P 上方按 E。\n恢复实体，短落到托盘，操作低处 K。'),
           ('③ 开帘后：重新入光回 B','K 将 G 卷起并锁开。沿两级回光阶\n站入原光，三秒化光，返回 B。'),
           ('④ 换用途：升 P，再返回 A','在 B 把 P 升回高位，镜片归位。\n仍沿下层光回 A，再切换上层方向。'),
           ('⑤ 在 C：穿孔校准，接通 D','恢复实体转 C 镜，让光穿过 F 窄缝。\n高位 P 固定镜承接，折向 D。'),
           ('⑥ 在 D：行路光变为喂养光','沿 C → P → D 化光抵达，实体调末镜。\n照亮肋侧暗鳞，进入叶幕走廊。')]
    for i,(title,txt) in enumerate(steps):
        yy=330+i*127;b.text(1730,yy,title,26,C['green'],True);b.text(1730,yy+42,txt,22,C['muted'])
    # A larger, legible inset for the exact dismount / re-entry geometry.
    b.rect(60,1162,1050,418,C['white'],C['line'],1)
    b.text(84,1180,'途中恢复实体：落点与回光点分开',29,C['ink'],True)
    b.ray([(95,1280),(1070,1280)])
    b.lightmove([(975,1255),(378,1255)])
    b.ground(280,1455,330,20); b.ground(610,1412,100,15); b.ground(710,1367,130,15)
    b.line([(505,1455),(505,1430)],RUST,4);b.mirror(505,1407,tag=None,fixed=True)
    b.rect(312,1410,72,35,'#D7E3D1',C['green'],2);b.lever(348,1420,'')
    b.arrow([(387,1284),(387,1442)],C['blue'],5)
    char=Local();char.spirit(0,0)
    paste(b,char,390,1449,1.8);paste(b,char,780,1361,1.8)
    b.walk([(424,1442),(578,1442),(660,1400),(777,1354)])
    b.tag(93,1320,'按 E → 恢复重力\n短距离落在 P 上',C['blue'],21)
    b.tag(852,1340,'站回光里\n充能 3 秒\n再向右回 B',C['orange'],22)
    b.tag(430,1496,'P 镜片低于来光，不会截走返程光',C['muted'],20)
    b.tag(84,1541,'K 固定在沟壁；没有低位托盘，就无法站稳操作。',C['purple'],21)
    b.rect(1140,1162,1200,418,C['white'],C['line'],1)
    b.text(1168,1180,'真正需要推理的三个问题',29,C['ink'],True)
    cards=[('落脚还是接光？','P 低位才能碰到 K；P 高位才能把 C 的光交给 D。'),
           ('什么时候收走旧光？','必须先开帘、升起托盘并回到 A，再放弃下层路线。'),
           ('闸开了，为什么仍到不了 D？','如果忘记升 P，光穿孔后会落空；可原路回去修正。')]
    for i,(title,desc) in enumerate(cards):
        yy=1241+i*103;b.text(1170,yy,title,25,C['green'],True);b.text(1170,yy+40,desc,23,C['muted'])
    b.text(62,1612,'途中按 E：脚下短距内有安全平台才恢复并下落；无落点则保持光态。机关仍需实体站稳后操作。',25,C['ink'])
    b.text(62,1657,'安全规则：G 开启后锁止；错误镜向可反调；P 高位也是可站的实体平台；落水返回固定岸台，已开机关保留。',23,C['muted'])
    b.text(62,1700,'v02 设计提案，尚未实装。主图非比例；窄缝净空、最大跳距、三秒充能区与脱光下落需 Unity 灰盒验证。',21,C['muted'])
    return b

def state_sheet():
    b=Sheet('06-combination-states-v02','06  六个连续状态｜同一空间，只保留当前光路',
      '读图重点：② 与 ③ 在同一束下层光内完成；④ 托盘已经升回，玩家仍要回 A 才能换光。')
    panels=[
      ('initial','① A → B：先准备落脚点','初始 P 高、G 关闭。到 B 操作卷轮 H，降 P。'),
      ('lowered','② B → P：光路半途恢复实体','在托盘左侧上方脱光，落到 P；低处 K 在此才够得到。'),
      ('opened','③ P → B：打开 G，再借原光返回','K 锁开遮光帘。走回光阶，站光三秒，再化光向右。'),
      ('raised','④ B → A：升 P，返回后重新改道','P 改作高位接光镜；下层光保持到玩家回到 A。'),
      ('ready','⑤ A → C → P → D：校准上层链路','A 改上层。到 C 转镜穿过窄缝 F，沿高位 P 转折到 D。'),
      ('final','⑥ D → 光鳞：保留来光，只调整末段','在 D 恢复实体，调整末镜；目标受光后，出口开放。')]
    for i,(mode,title,note) in enumerate(panels):
        x=60+(i%2)*1160; y=155+(i//2)*504
        b.rect(x,y,1120,482,C['white'],C['line'],1)
        b.text(x+18,y+14,title,27,C['green'],True)
        src=scene(mode,False);paste(b,src,x+16,y+59,.665)
        # Crop is conceptual: show diagram compacted vertically to fit panel.
        # Adjust the diagram's y coordinates only, preserving horizontal positions.
        base=y+59
        for e in b.items[-len(src.items):]:
            if 'y' in e:e['y']=base+(e['y']-base)*.61
            if 'h' in e:e['h']*=.61
            if 'pts' in e:e['pts']=[(px,base+(py-base)*.61) for px,py in e['pts']]
        # Labels stay at readable print size, outside the dense optical chain.
        for px,py,t in [(170,314,'A'),(958,314,'B / H'),(242,99,'C'),(880,108,'D'),(517,301,'K')]:
            b.tag(x+px,y+py,t,C['green'],20)
        b.tag(x+609,y+(181 if mode in ('initial','raised','ready','final') else 358),
              'P 高' if mode in ('initial','raised','ready','final') else 'P 低',C['green'],20)
        b.text(x+19,y+438,note,22,C['muted'])
    b.text(62,1692,'每格的金线只有一条，不分光。G 的开启状态会保留；P 的高低档则必须在 B 主动调整。',23,C['ink'])
    return b

def logic_sheet():
    b=Sheet('06-combination-logic-v02','06  机关组合与可恢复的错误｜为什么不能一路直走',
      '把“走到哪里按什么”变成“先创造哪个条件、保留哪条回路、何时切换托盘用途”。')
    b.rect(60,160,2280,240,C['white'],C['line'],1)
    headers=['机关','可操作位置','作用','留存 / 可逆性']
    xs=[90,480,880,1780]
    for x,t in zip(xs,headers):b.text(x,178,t,26,C['green'],True)
    rows=[['A 改道镜','左岸固定台','下层去 B / 上层去 C，只能选一条','随时可反调'],
          ['H 升降卷轮','B 右岸固定台','P 低：供 K 落脚；P 高：供上层接光','高低两档可重复调整'],
          ['K 帘绳扣','沟壁低位；须站 P','卷起 G 遮光帘，开放 A → C','棘爪锁开，不随离开复位']]
    for i,row in enumerate(rows):
        for x,t in zip(xs,row):b.text(x,230+i*50,t,23,C['ink'])
    b.rect(60,430,1100,720,C['white'],C['line'],1)
    b.text(88,454,'可以试错：G 已开，但 P 忘了升',30,RED,True)
    paste(b,scene('wrong',False),81,525,.66)
    b.tag(630,730,'镜片还在低处\n高位接收点空着',RED,23)
    b.tag(803,747,'落空光止于挡片',RED,22)
    b.tag(150,1025,'C 仍有入射光 → 原路回 A → 改下层 → B 升 P',C['blue'],23)
    b.text(88,1090,'不重开关卡，也不要求无提示地自杀重置。',23,C['muted'])
    b.rect(1190,430,1150,720,C['white'],C['line'],1)
    b.text(1220,454,'三组条件同时成立，终点光路才成立',30,C['green'],True)
    blocks=[('进入 C','G 打开 + A 选上层','K 的锁止动作解决遮光。'),
            ('接通 D','P 高位 + C 镜对准 F 窄缝','同一个 P 不能既留在低位又在高位接光。'),
            ('完成救援','实体抵达 D + 末镜对准暗鳞','前面的输入光保持，最后一段从照墙改为喂养。')]
    for i,(a,c,d) in enumerate(blocks):
        yy=525+i*185;b.rect(1222,yy,1080,155,'#F0F3EA',C['line'],1)
        b.text(1245,yy+16,a,27,C['green'],True);b.text(1245,yy+57,c,27,C['ink'],True)
        b.text(1245,yy+104,d,23,C['muted'])
    b.text(1222,1100,'复杂度来自条件组合与状态切换，不靠倒计时。',24,C['ink'])
    b.rect(60,1180,2280,385,C['white'],C['line'],1)
    b.text(90,1204,'空间与规则必须在灰盒中落实',30,C['ink'],True)
    points=[
      '• P 的落脚区与回光阶分开：下落先到 K，随后走两级短阶重新站进光里。',
      '• K 固定在低处；回光阶离 K 超过实体交互范围，不能跳过“降 P”直接操作。',
      '• F 是接顶不透光挡板，缝只容光核；既阻止实体横跳，也阻止 C 绕过 P 直接照 D。',
      '• P 高位仍能落脚；右挡片同样接顶，只让正确反射光穿孔，不留下可站的挡片顶面。',
      '• 落水回固定岸台，保留 G 与 P 状态；不能把存档落点挂在会被移走的托盘上。']
    for i,t in enumerate(points):b.text(91,1260+i*51,t,25,C['muted'])
    b.text(62,1601,'实现边界：当前 E 找地面后直接定位；拟改为检查约 2 个身高内的安全落点与下落净空，再恢复重力短落。',23,C['ink'])
    b.text(62,1646,'本次只修改设计与图件。路径依赖已审阅，未宣称完成 Unity 通关测试；具体反射角、跳距、碰撞需实装验证。',23,C['muted'])
    b.text(62,1691,'空间衔接不变：05 废弃泵房 → 06 温室两座苗床与沟槽 → 07 叶幕走廊。',23,C['muted'])
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
            a=dict(id=f'greenhouse-{n:05d}',type={'rect':'rectangle','poly':'line'}.get(typ,typ),x=x,y=y+bi*1840,width=max(w,.01),height=max(h,.01),angle=0,strokeColor=e.get('color',e.get('stroke')) or 'transparent',backgroundColor=e.get('fill') or 'transparent',fillStyle='solid',strokeWidth=e.get('lw',1),strokeStyle='dashed' if e.get('dash') else 'solid',roughness=0,opacity=100,groupIds=[],frameId=None,roundness=None,seed=n*31,version=1,versionNonce=n*17,isDeleted=False,boundElements=None,updated=1793836800000,link=None,locked=False)
            if typ in ('line','poly'):
                pp=pts+[pts[0]] if typ=='poly' else pts;a.update(points=[[px-x,py-y] for px,py in pp],lastCommittedPoint=None,startBinding=None,endBinding=None,startArrowhead=None,endArrowhead=None)
            if typ=='text':a.update(fontSize=e['size'],fontFamily=2,text=e['t'],originalText=e['t'],textAlign='left',verticalAlign='top',containerId=None,autoResize=True,lineHeight=1.35,baseline=e['size'])
            result.append(a)
    (OUT/'level06-greenhouse-combination-v02.excalidraw').write_text(json.dumps(dict(type='excalidraw',version=2,source='local',elements=result,appState=dict(viewBackgroundColor=C['paper'],gridSize=None),files={}),ensure_ascii=False),encoding='utf-8')
    return result

if __name__=='__main__':
    boards=[overview(),state_sheet(),logic_sheet()]
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
    report=dict(dimensions=[dc.W,dc.H],boards=3,svg_xml='pass',editable_objects=len(elements),unique_ids=True,text_canvas_bounds='pass',source_count=1,split_beams=False,design_logic_review='completed; see README',unity_graybox='not performed',excalidraw_editor_import='not performed')
    (OUT/'validation.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
    print(json.dumps(dict(output=str(OUT),objects=len(elements)),ensure_ascii=False))

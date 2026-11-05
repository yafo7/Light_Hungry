"""Original vector level diagrams. No source artwork or Unity assets are edited."""
import sys, math, json, html, hashlib
from pathlib import Path
ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / '.tools/media-python'))
from PIL import Image, ImageDraw, ImageFont
OUT = Path(__file__).resolve().parent
W,H,S = 1600,1100,2
C = dict(paper='#F7F5EF', white='#FFFDF8', ink='#263B40', muted='#687A7D', line='#CDD6D1',
         ground='#DAE2DB', blue='#287FAF', gold='#DFA42D', orange='#D56D2F', purple='#8664B2',
         green='#478568', water='#E1EEF1', ghost='#AAB4B3', beast='#F1F0E4', scale='#6B7074')
FONT='C:/Windows/Fonts/msyh.ttc'
BOLD='C:/Windows/Fonts/msyhbd.ttc'
def esc(t): return html.escape(str(t))
class Board:
    def __init__(self, key, title, sub):
        self.key=key; self.items=[]
        self.rect(0,0,W,H,C['paper'],None)
        self.text(55,35,title,36,C['ink'],bold=True)
        self.text(57,85,sub,21,C['muted'])
        self.rect(48,130,1504,54,C['white'],C['line'],1)
        self.arrow([(70,157),(125,157)],C['blue'],4); self.text(138,143,'实体行走 / 跳跃',19)
        self.line([(355,157),(409,157)],C['gold'],4); self.text(422,143,'有效光束',19)
        self.arrow([(594,157),(650,157)],C['orange'],4,dash=True); self.text(663,143,'化光后的行动方向',19)
        self.arrow([(926,157),(982,157)],C['purple'],4,dash=True); self.text(995,143,'纵深跨层',19)
        self.ellipse(1188,145,23,23,None,C['green'],3); self.text(1224,143,'交互点',19)
        self.rect(1371,146,18,20,C['scale'],None); self.text(1402,143,'待照光鳞',19)
        self.rect(48,205,1504,620,C['white'],C['line'],1)
    def add(self,kind,**kw): self.items.append(dict(type=kind,**kw))
    def rect(self,x,y,w,h,fill=None,stroke=None,lw=2): self.add('rect',x=x,y=y,w=w,h=h,fill=fill,stroke=stroke,lw=lw)
    def ellipse(self,x,y,w,h,fill=None,stroke=None,lw=2): self.add('ellipse',x=x,y=y,w=w,h=h,fill=fill,stroke=stroke,lw=lw)
    def poly(self,pts,fill=None,stroke=None,lw=2): self.add('poly',pts=pts,fill=fill,stroke=stroke,lw=lw)
    def line(self,pts,color=None,lw=2,dash=False): self.add('line',pts=pts,stroke=color or C['ink'],lw=lw,dash=dash)
    def arrow(self,pts,color=None,lw=4,dash=False):
        col=color or C['blue']; self.line(pts,col,lw,dash)
        a,b=pts[-2:]; ang=math.atan2(b[1]-a[1],b[0]-a[0]); size=11+lw
        self.poly([b,(b[0]-size*math.cos(ang-.43),b[1]-size*math.sin(ang-.43)),(b[0]-size*math.cos(ang+.43),b[1]-size*math.sin(ang+.43))],col)
    def text(self,x,y,t,size=22,color=None,bold=False): self.add('text',x=x,y=y,t=t,size=size,color=color or C['ink'],bold=bold)
    def label(self,x,y,t,size=20,color=None):
        f=ImageFont.truetype(FONT,size); width=max(f.getlength(l) for l in t.split('\n'))
        self.rect(x-7,y-3,width+14,size*1.35*len(t.split('\n'))+5,C['white'],None)
        self.text(x,y,t,size,color)
    def badge(self,x,y,n,color=None):
        col=color or C['blue']; self.ellipse(x-19,y-19,38,38,C['white'],col,3)
        self.text(x-7,y-12,str(n),22,col,True)
    def ground(self,x,y,w,h=110):
        self.rect(x,y,w,h,C['ground'],None)
        self.line([(x,y+h),(x,y),(x+w,y),(x+w,y+h)],C['ink'],3)
        for xx in range(int(x+15),int(x+w-10),35): self.line([(xx,y+18),(xx+12,y+7)],C['line'],2)
    def glass(self,x,y,w):
        self.rect(x,y,w,11,'#DBEEEE','#73A8AB',2)
        for xx in range(x+14,x+w-10,32): self.line([(xx,y+9),(xx+12,y+1)],'#73A8AB',1)
    def mirror(self,x,y,inpoint=None,outpoint=None,tag=None,fixed=False,ghost=False):
        col=C['ghost'] if ghost else C['muted'] if fixed else C['green']
        if inpoint and outpoint:
            v=(x-inpoint[0],y-inpoint[1]); u=(outpoint[0]-x,outpoint[1]-y)
            lv=math.hypot(*v);lu=math.hypot(*u);t=(v[0]/lv+u[0]/lu,v[1]/lv+u[1]/lu)
            a=math.atan2(t[1],t[0])
        else:a=-.8
        dx,dy=24*math.cos(a),24*math.sin(a)
        self.line([(x,y+19),(x,y+47),(x-19,y+47),(x+19,y+47)],col,2,ghost)
        self.ellipse(x-30,y-30,60,60,None,col,2)
        self.line([(x-dx,y-dy),(x+dx,y+dy)],col,8,ghost)
        if tag:self.label(x-22,y+54,tag,19,col)
    def source(self,x,y,tag='光源'):
        self.ellipse(x-15,y-15,30,30,'#F6D886',C['gold'],2)
        for a in range(0,360,45):
            r=math.radians(a);self.line([(x+21*math.cos(r),y+21*math.sin(r)),(x+28*math.cos(r),y+28*math.sin(r))],C['gold'],2)
        self.label(x+35,y-15,tag,20,C['gold'])
    def ray(self,pts): self.line(pts,C['gold'],5)
    def lightmove(self,pts): self.arrow(pts,C['orange'],4,True)
    def walk(self,pts): self.arrow(pts,C['blue'],4)
    def jump(self,a,b):
        mx=(a[0]+b[0])/2;my=min(a[1],b[1])-70
        pts=[((1-t)**2*a[0]+2*(1-t)*t*mx+t*t*b[0],(1-t)**2*a[1]+2*(1-t)*t*my+t*t*b[1]) for t in [i/24 for i in range(25)]]
        self.walk(pts)
    def spirit(self,x,y):
        self.ellipse(x-12,y-39,24,25,'#FFF4CD',C['orange'],2)
        self.poly([(x-7,y-33),(x-8,y-48),(x+1,y-39),(x+9,y-46),(x+10,y-32)],'#FFF4CD',C['orange'],1)
        self.line([(x,y-13),(x,y-2),(x-7,y+4),(x,y-2),(x+7,y+4)],C['orange'],3)
    def beast(self,x,y,w,h,tx,ty,tag):
        pts=[(x+w,y),(x+w,y+h),(x+35,y+h),(x,y+h*.62),(x+12,y+h*.3),(x+w*.45,y)]
        self.poly(pts,C['beast'],'#BBC4BC',2)
        for row in range(3):
            for col in range(3):
                xx=x+45+col*45+(row%2)*12;yy=y+35+row*34
                if xx<x+w-20 and yy+12<y+h-8:self.line([(xx-15,yy),(xx,yy+12),(xx+15,yy)],'#D1D6CC',2)
        self.poly([(tx-19,ty),(tx,ty-24),(tx+19,ty),(tx,ty+24)],C['scale'],C['ink'],2)
        self.label(x+7,y+h+9,tag,18,C['muted'])
    def footer(self,steps,note):
        n=len(steps); gap=12; cw=(1504-gap*(n-1))/n
        for i,(a,b) in enumerate(steps):
            xx=48+i*(cw+gap);self.rect(xx,850,cw,140,C['white'],C['line'],1)
            self.text(xx+16,863,f'{i+1:02}',22,C['green'],True)
            self.text(xx+16,899,a,21,C['ink'],True); self.text(xx+16,935,b,18,C['muted'])
        self.text(52,1007,note,19,C['ink'])
        self.text(52,1043,'共同规则：站光 3 秒化光；沿光路手动前后移动。圆号表示先后顺序，箭头不限制回退。',18,C['muted'])
        self.text(52,1071,'v01 非比例草图 · 展示完成态光路，按步骤接通 · 恢复点为设计建议；跳距、碰撞和切换待灰盒验证。',16,C['muted'])
    def save(self):
        im=Image.new('RGB',(W*S,H*S),C['paper']);d=ImageDraw.Draw(im)
        svg=[f'<svg xmlns="http://www.w3.org/2000/svg" width="{W}" height="{H}" viewBox="0 0 {W} {H}">']
        for e in self.items:
            typ=e['type'];f=e.get('fill');st=e.get('stroke');lw=e.get('lw',1)
            style=f'fill="{f or "none"}" stroke="{st or "none"}" stroke-width="{lw}" stroke-linejoin="round" stroke-linecap="round"'
            if typ in ['rect','ellipse']:
                x,y,w,h=[e[k] for k in ['x','y','w','h']];box=tuple(round(q*S) for q in (x,y,x+w,y+h))
                getattr(d,'rectangle' if typ=='rect' else 'ellipse')(box,fill=f,outline=st,width=max(1,round(lw*S)))
                if typ=='rect':svg.append(f'<rect x="{x}" y="{y}" width="{w}" height="{h}" {style}/>')
                else:svg.append(f'<ellipse cx="{x+w/2}" cy="{y+h/2}" rx="{w/2}" ry="{h/2}" {style}/>')
            elif typ in ['line','poly']:
                pts=e['pts']; p=[(round(x*S),round(y*S)) for x,y in pts]
                if typ=='poly':
                    d.polygon(p,fill=f)
                    if st:d.line(p+[p[0]],fill=st,width=round(lw*S),joint='curve')
                elif e.get('dash'):
                    # Dashes drawn as arc-length intervals; same design in SVG and PNG.
                    distance=0
                    for a,b in zip(pts,pts[1:]):
                        ln=math.dist(a,b);j=0
                        while j<ln:
                            phase=(distance+j)%20;step=min(ln-j, (12-phase if phase<12 else 20-phase))
                            if step<.00001:step=.01
                            if phase<12:
                                u=j/ln;v=min(1,(j+step)/ln)
                                d.line([(int((a[0]+(b[0]-a[0])*u)*S),int((a[1]+(b[1]-a[1])*u)*S)),(int((a[0]+(b[0]-a[0])*v)*S),int((a[1]+(b[1]-a[1])*v)*S))],fill=st,width=round(lw*S))
                            j+=step
                        distance+=ln
                else:d.line(p,fill=st,width=round(lw*S),joint='curve')
                coords=' '.join(f'{x},{y}' for x,y in pts)
                dash='stroke-dasharray="12 8"' if e.get('dash') else ''
                svg.append(f'<{"polygon" if typ=="poly" else "polyline"} points="{coords}" {style} {dash}/>')
            elif typ=='text':
                x,y,t,sz=e['x'],e['y'],e['t'],e['size'];font=ImageFont.truetype(BOLD if e['bold'] else FONT,sz*S)
                for j,line in enumerate(t.split('\n')):
                    yy=y+j*sz*1.35; d.text((round(x*S),round(yy*S)),line,font=font,fill=e['color'],anchor='lt')
                    svg.append(f'<text x="{x}" y="{yy+sz*.88}" font-family="Microsoft YaHei, sans-serif" font-size="{sz}" font-weight="{700 if e["bold"] else 400}" fill="{e["color"]}">{esc(line)}</text>')
        svg.append('</svg>');(OUT/(self.key+'.svg')).write_text('\n'.join(svg),encoding='utf-8')
        im=im.resize((W,H),Image.Resampling.LANCZOS);im.save(OUT/(self.key+'.png'))
        return im

boards=[]
# 04: horizontal application, dry island between two mandatory crossings.
b=Board('04-pool-flow','04  蓄水浅池｜分段接光，穿过水面','场景：暗渠出水口 → 浅池 → 泵房墙脚　　身体局部：下腹　　核心：中途落脚，再调整下一段光路')
b.rect(80,600,1440,160,C['water'],None)
for yy in (620,661,704,742):b.line([(90,yy),(1510,yy)],'#B9D4DC',1)
b.ground(80,560,300,200);b.ground(600,510,180,250);b.ground(1020,560,500,200)
b.text(100,714,'左岸',22,C['muted']);b.text(646,714,'干燥中岛',20,C['muted']);b.text(1060,714,'右岸',22,C['muted'])
b.beast(1290,340,220,170,1390,425,'下腹局部（浅白）')
b.source(190,272,'墙缝日光')
A=(315,505);B=(690,455);D=(1100,505);T=(1390,425)
b.ray([(190,272),A,B,D,T])
b.mirror(*A,(190,272),B,'M1');b.mirror(*B,A,D,'M2');b.mirror(*D,B,T,'M3')
b.spirit(129,554);b.walk([(140,548),(245,548)])
b.lightmove([(345,481),(650,440)]);b.lightmove([(725,439),(1060,480)])
b.walk([(1150,548),(1480,548)])
b.badge(260,450,1);b.badge(490,413,2);b.badge(690,412,3);b.badge(970,445,4);b.badge(1450,533,5)
b.label(92,228,'← 接第 03 关：暗渠出水口',21,C['blue'])
b.label(1230,265,'第 05 关 → 泵房管口',21,C['blue'])
b.label(405,630,'水面跨度 > 普通跳距',19,C['muted']);b.label(800,680,'不布置可连续跳过的浮叶',18,C['muted'])
b.label(593,330,'中岛：恢复实体\n调整 M2，再出发',21,C['green'])
b.line([(682,396),(690,423)],C['green'],2)
b.line([(1500,505),(1495,560)],C['green'],4);b.line([(1498,527),(1480,513),(1498,527),(1516,512)],C['green'],3)
b.label(1230,590,'目标鳞亮起 → 出口根缝松开',18,C['green'])
b.footer([('左岸转 M1','接通中岛来光'),('第一次化光','沿光跨水到中岛'),('中岛转 M2','建立第二段光路'),('到右岸转 M3','把光送向目标鳞'),('下腹光鳞受光','步行进入泵房')], '落脚点：两岸与中岛。光路分两次接通，途中没有“强光自动吸走”的规则。')
boards.append(b)

# 05: inspection shaft. High inlet has no exit footing, glass can carry body but pass light.
b=Board('05-pumphouse-flow','05  废弃泵房｜沿光爬升，穿过玻璃架','场景：低处管口 → 检修井 → 高处检修孔　　身体局部：侧腹　　核心：实体与光对同一踏板的通过方式不同')
b.rect(170,230,1320,560,'#EEF0EB',None)
b.line([(185,650),(185,245),(450,245)],C['ink'],5)
b.line([(500,245),(1510,245),(1510,755)],C['ink'],5)
b.ground(80,710,430,70);b.ground(1030,572,220,208);b.ground(565,403,185,68);b.ground(1120,322,400,65)
b.glass(650,575,250);b.glass(820,445,250)
b.beast(1320,255,195,60,1400,286,'侧腹局部（浅白）')
src=(320,275);A=(340,655);B=(1130,517);D=(655,348);E=(1215,267);T=(1400,286)
b.source(*src,'高窗来光（孔后封闭）');b.ray([src,A,B,D,E,T])
b.mirror(*A,src,B,'底镜');b.mirror(*B,A,D,'中镜');b.mirror(*D,B,E,'上镜');b.mirror(*E,D,T,'固定末镜',fixed=True)
b.spirit(135,704);b.walk([(150,696),(285,696)])
b.lightmove([(385,628),(1087,505)]);b.lightmove([(1090,487),(698,345)]);b.lightmove([(704,317),(1170,249)])
b.walk([(1260,306),(1485,306)])
b.badge(275,624,1);b.badge(925,550,2);b.badge(1050,425,3);b.badge(785,325,4);b.badge(1450,301,5)
b.label(90,760,'← 第 04 关的池岸管口',20,C['blue'])
b.label(1125,215,'第 06 关 → 检修孔＋短梯',20,C['blue'])
b.label(475,480,'玻璃踏板\n实体能站，光能穿',20,'#54868B');b.line([(665,515),(755,580)],'#73A8AB',2)
b.label(430,680,'进光孔附近无落脚台，\n不能逆光直达出口。',18,C['muted'])
b.label(840,689,'每面可调镜旁保留固定踏台',20,C['green'])
b.footer([('底镜接光','从井底开始攀升'),('化光穿过玻璃','抵达右侧中平台'),('中镜转向左上','折返到上层踏台'),('上镜转向出口','沿光到右上平台'),('侧腹光鳞受光','经检修孔进入温室')], '与第 06 关区分：本关重点是竖向攀升和透光踏板；不增加升镜机关。')
boards.append(b)

# 06: three traversals to new heights; suspended C mirror has fixed landing beside it.
b=Board('06-seedbed-flow','06  温室苗床｜三次过沟，逐次配光','场景：同一座温室的两座苗床　　身体局部：肋侧　　核心：A 左下 → B 右下 → C 左上 → D 右上')
b.rect(540,620,495,160,'#ECE8DF',None)
b.ground(80,670,460,110);b.ground(1035,670,485,110)
b.ground(245,387,300,48);b.ground(1110,350,410,48)
b.line([(275,435),(275,610)],C['ghost'],4);b.line([(515,435),(515,610)],C['ghost'],4)
b.line([(1150,398),(1150,560)],C['ghost'],4)
b.beast(1320,267,195,65,1400,298,'肋侧局部（浅白）')
src=(130,490);A=(355,615);B=(1145,615);D=(470,332);E=(1200,295);T=(1400,298)
b.source(*src,'棚缝日光');b.ray([src,A,B,D,E,T])
b.mirror(*A,src,B,'A');b.mirror(*B,A,D,'B');b.mirror(*D,B,E,'C');b.mirror(*E,D,T,'D（固定）',fixed=True)
b.line([(470,255),(470,295)],C['muted'],3);b.ellipse(458,234,24,24,None,C['muted'],3)
b.line([(470,234),(1355,234),(1355,590)],C['muted'],3)
b.ellipse(1328,590,54,44,None,C['green'],3);b.line([(1355,612),(1386,612),(1386,596)],C['green'],3)
b.label(1260,665,'B 岸卷轴：升起 C 镜',19,C['green'])
b.line([(445,420),(495,420)],C['ghost'],5,True);b.arrow([(563,419),(563,328)],C['green'],3)
b.label(625,706,'C 镜低位被台沿遮光\n镜架从踏台后侧升起',19,C['green'])
b.spirit(130,664);b.walk([(145,653),(290,653)])
b.lightmove([(397,594),(1098,594)]);b.lightmove([(1095,578),(517,337)]);b.lightmove([(520,309),(1155,272)])
b.walk([(1250,335),(1485,335)])
b.badge(285,575,1);b.badge(1410,613,2);b.badge(926,488,3);b.badge(402,286,4);b.badge(1450,264,5);b.badge(1490,320,6)
b.label(92,725,'← 第 05 关检修孔',20,C['blue']);b.label(1150,742,'深沟：普通跳跃无法直跨',20,C['muted'])
b.label(755,667,'苗床之间的沟槽',20,C['muted'])
b.label(1000,445,'第 07 关 → 藤架 / 跨层镜',20,C['blue'])
b.line([(1260,440),(1480,365)],C['blue'],2)
b.label(125,420,'左上 C 平台\n高于普通跳跃可达范围',19,C['muted'])
b.footer([('A：接光去右岸','化光抵达 B'),('B：操作卷轴','升起左上 C 吊镜'),('B：转镜向左上','化光折返到 C'),('C：恢复并转镜','接通右上 D'),('D：肋侧受光','到达新的上层出口'),('进入叶幕走廊','看见下一关跨层镜')], '保留已接通的 A → B → C 入射光路；每次折返抵达新位置，不需要原路空跑。')
boards.append(b)

# 07: same-height planes, not a vertical pit. Horizontal registration is explicit.
b=Board('07-leafcurtain-flow','07  叶幕走廊｜去背面绕墙，从另一侧回来','场景：同一座藤架的前后走道　　身体局部：背侧　　核心：移动回程镜，改变返回正面的落点')
b.rect(75,235,1450,205,'#EDF1E9',None);b.rect(75,535,1450,240,'#EEF0F4',None)
b.label(90,242,'A 面｜前方走道',23,C['ink']);b.label(90,541,'B 面｜后方走道',23,C['ink'])
b.ground(80,395,1440,45);b.ground(80,715,1440,60)
b.rect(700,239,190,156,'#BACBBB',C['ink'],3)
b.text(732,268,'厚叶幕',23,C['ink'],True);b.text(723,305,'＋花架墙',22,C['ink'])
b.line([(700,253),(882,380)],'#90A38F',2);b.line([(705,380),(881,250)],'#90A38F',2)
b.beast(1320,268,200,116,1400,340,'背侧局部（浅白）')
# Corresponding coordinates, repeated structural landmark.
for xx in [350,570,1130]:b.line([(xx,412),(xx,628)],C['ghost'],1,True)
b.line([(965,248),(965,393)],C['ghost'],3);b.line([(965,549),(965,713)],C['ghost'],3)
b.label(975,469,'同一根藤架立柱（位置参照）',18,C['muted'])
P=(350,340);Q=(350,660);R=(1130,660);E=(1130,340);T=(1400,340)
b.source(215,292,'');b.ray([(215,292),P]);b.ray([Q,R]);b.ray([E,T])
b.arrow([P,Q],C['purple'],4,True);b.arrow([R,E],C['purple'],4,True)
b.mirror(*P,tag='入口镜',fixed=True);b.mirror(*Q,tag='接收镜',fixed=True)
b.mirror(*R,tag='回程镜',fixed=False);b.mirror(*E,tag='接收镜',fixed=True)
b.mirror(570,660,ghost=True);b.line([(570,627),(570,340)],C['ghost'],2,True)
b.ellipse(556,326,28,28,None,C['ghost'],2)
b.label(455,245,'旧光斑：仍在墙前',19,C['muted'])
b.label(460,564,'旧位置',19,C['muted']);b.label(1062,564,'推到墙后对应位置',19,C['green'])
b.arrow([(607,626),(1078,626)],C['blue'],5)
b.label(665,586,'实体推镜向右；水平来光持续接入',19,C['blue'])
b.spirit(135,389);b.walk([(150,379),(290,379)])
b.walk([(385,701),(530,701)]);b.walk([(1180,379),(1500,379)])
b.label(395,741,'连续安全地面：能落脚、推镜、回退',19,C['muted'])
b.badge(278,350,1);b.badge(303,548,2);b.badge(718,625,3);b.badge(1195,631,4);b.badge(1192,333,5);b.badge(1480,356,6)
b.label(87,470,'上下是展开示意，不是上下楼层：A、B 处于同一高度，前后有距离。',20,C['purple'])
b.label(90,445,'← 第 06 关藤架出口（接 A 面）',18,C['blue']);b.label(1242,229,'第 08 关 → 雨槽屋脊',19,C['blue'])
b.footer([('正面遇到叶幕','沿入口镜进入 B'),('B 面恢复实体','观察前后位置对应'),('把回程镜右移','移到叶幕另一侧'),('站光再次化光','沿回程镜进入 A'),('返回墙后平台','固定镜照亮背侧鳞'),('沿藤架上行','穿破玻璃到雨槽')], '首次跨层只开放一个移动镜；其余三面镜固定。紫线表示纵深光路，不能当成竖向跳跃。')
boards.append(b)

images=[b.save() for b in boards]
overview=Image.new('RGB',(2000,1505),C['paper']);od=ImageDraw.Draw(overview)
od.text((38,25),'04 浅池 → 05 泵房 → 06 苗床 → 07 叶幕',font=ImageFont.truetype(BOLD,35),fill=C['ink'])
od.text((40,78),'由横向接光，到竖向攀升、往返配光，再到前后层绕行。详图另附，可编辑源文件见同目录。',font=ImageFont.truetype(FONT,21),fill=C['muted'])
for i,im in enumerate(images):overview.paste(im.resize((980,674),Image.Resampling.LANCZOS),(10+(i%2)*1000,125+(i//2)*690))
overview.save(OUT/'00-overview.png')

# Native editable Excalidraw objects (not flattened images).
elements=[];serial=0
for bi,b in enumerate(boards):
    ox=(bi%2)*1680;oy=(bi//2)*1180
    for e in b.items:
        serial+=1;typ=e['type'];x=e.get('x',0);y=e.get('y',0);w=e.get('w',0);h=e.get('h',0)
        if typ in ('line','poly'):
            pts=e['pts'];x=min(p[0] for p in pts);y=min(p[1] for p in pts);w=max(p[0] for p in pts)-x;h=max(p[1] for p in pts)-y
        if typ=='text':
            f=ImageFont.truetype(BOLD if e['bold'] else FONT,e['size']);w=max(f.getlength(l) for l in e['t'].split('\n'));h=e['size']*1.35*len(e['t'].split('\n'))
        item=dict(id=f'flow-{serial:05d}',type={'rect':'rectangle','poly':'line'}.get(typ,typ),x=x+ox,y=y+oy,width=max(w,.01),height=max(h,.01),angle=0,
                  strokeColor=e.get('color',e.get('stroke')) or 'transparent',backgroundColor=e.get('fill') or 'transparent',fillStyle='solid',strokeWidth=e.get('lw',1),strokeStyle='dashed' if e.get('dash') else 'solid',roughness=0,opacity=100,
                  groupIds=[],frameId=None,roundness=None,seed=serial*31,version=1,versionNonce=serial*17,isDeleted=False,boundElements=None,updated=1793664000000,link=None,locked=False)
        if typ in ('line','poly'):
            pp=pts+[pts[0]] if typ=='poly' else pts
            item.update(points=[[px-x,py-y] for px,py in pp],lastCommittedPoint=None,startBinding=None,endBinding=None,startArrowhead=None,endArrowhead=None)
        if typ=='text':item.update(fontSize=e['size'],fontFamily=2,text=e['t'],originalText=e['t'],textAlign='left',verticalAlign='top',containerId=None,autoResize=True,lineHeight=1.35,baseline=e['size'])
        elements.append(item)
(OUT/'levels04-07-flow.excalidraw').write_text(json.dumps(dict(type='excalidraw',version=2,source='local',elements=elements,appState=dict(viewBackgroundColor=C['paper'],gridSize=None),files={}),ensure_ascii=False),encoding='utf-8')
print(json.dumps(dict(directory=str(OUT),boards=[b.key for b in boards],editable_elements=len(elements)),ensure_ascii=False))

import xml.etree.ElementTree as ET
import zipfile
assert len({e['id'] for e in elements}) == len(elements)
for board in boards:
    ET.parse(OUT/(board.key+'.svg'))
    with Image.open(OUT/(board.key+'.png')) as im: assert im.size == (W,H)
assert all(e['type'] in ['rectangle','ellipse','line','text'] for e in elements)
(OUT/'validation.json').write_text(json.dumps(dict(png_dimensions=[W,H],svg_xml_parse='pass',excalidraw_json_parse='pass',unique_element_ids=True,element_count=len(elements),unity_validation='not performed',excalidraw_editor_import='not performed'),ensure_ascii=False,indent=2),encoding='utf-8')
with zipfile.ZipFile(OUT/'levels04-07-flow-pack.zip','w',zipfile.ZIP_DEFLATED) as z:
    for f in sorted(OUT.iterdir()):
        if f.suffix in ['.png','.svg','.excalidraw','.md','.json','.py']:z.write(f,f.name)

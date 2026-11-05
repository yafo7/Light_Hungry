"""Original vector level diagrams. No source artwork or Unity assets are edited."""
import sys, math, json, html, hashlib
from pathlib import Path
ROOT = Path(__file__).resolve().parents[2]
# Use the bundled Python runtime's compatible Pillow rather than another ABI.
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

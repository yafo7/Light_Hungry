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


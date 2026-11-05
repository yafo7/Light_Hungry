from draw_core import *

DESIGNS = [
    dict(level=11,title='观测暗室',body='颈侧局部',idea='斜向跨层改变落点；错位孔必须在真实纵深里共线',
         route='塔楼破窗进入 A → 侧面观察两道错孔 → 调入口镜纵深角度 → 化光到 B → 转接收镜接固定回程 → 返回 A 墙后喂鳞 → 塔内楼梯上第12关',
         risks='A/B 主图同高展开，不是上下楼层；俯视图显示真实深度。镜子旁有实体站立位和邻近充能位；不能由投影重合替代实际通光。'),
    dict(level=14,title='暖房根室',body='胸侧局部',idea='四房间与两条后廊接力；复用跨层和穿隙，不增加新规则',
         route='A1 左上 → B上廊 → A2 右上稳定点Ⅰ → 经空井斜向跨层到 B下廊 → A3 左下稳定点Ⅱ → 光穿根隙到 A4 右下 → 喂胸侧鳞 → 同高塌墙到第15关',
         risks='A面隔间与B面上下廊均有实体分隔，不能直接步行绕过。γ光路经过前后走道之间的开放空井，不能穿楼板；稳定点保留上游光路。配对字母为跨层连接，不是自由传送门。')
]

def build():
    boards=[]
    b=Board('11-darkroom-flow','11  观测暗室｜斜向跨层，对准错位孔','场景：同一座观测塔内部　　身体局部：颈侧　　核心：镜面角度改变另一层落点；用侧面观察理解纵深')
    b.rect(75,239,1010,222,'#EEF0EB',None);b.rect(75,560,1010,223,'#ECEFF3',None)
    b.label(88,249,'A 前方走道',21);b.label(88,570,'B 后方走道',21)
    b.ground(80,425,995,36);b.ground(80,745,995,38)
    b.rect(467,249,185,176,'#C1C9C4',C['ink'],3);b.text(498,285,'前方隔墙',22)
    b.text(488,322,'不能直接穿越',19,C['muted'])
    b.beast(975,270,100,121,1028,324,'')
    b.label(965,470,'颈侧局部',18,C['muted'])
    p=(280,370);q=(580,690);r=(918,690);s=(918,370);t=(1028,324)
    b.source(186,313,'窄窗光');b.ray([(186,313),p]);b.ray([q,r]);b.ray([s,t])
    b.arrow([p,q],C['purple'],4,True);b.arrow([r,s],C['purple'],4,True)
    b.line([(280,402),(280,690)],C['ghost'],2,True);b.ellipse(269,679,22,22,None,C['ghost'],2)
    b.mirror(*p,tag='调纵深角');b.mirror(*q,tag='转向回程');b.mirror(*r,tag='固定回程',fixed=True);b.mirror(*s,tag='固定接收',fixed=True)
    b.spirit(120,419);b.walk([(143,409),(225,409)])
    b.walk([(610,730),(865,730)]);b.walk([(950,409),(1065,409)])
    b.badge(220,364,1);b.badge(420,476,2);b.badge(520,646,3);b.badge(715,658,4);b.badge(962,369,5)
    b.label(90,493,'A / B 同高展开；紫线表示向纵深移动。',19,C['purple'])
    b.label(111,640,'直射旧落点',18,C['muted'])
    b.label(640,600,'B 面落脚后，只调下游方向',19,C['green'])
    b.label(85,794,'第 10 关塔楼破窗 → A 面入口',18,C['blue']);b.label(750,235,'第 12 关 → 塔内楼梯',19,C['blue'])

    # Plan inset: the three-dimensional holes are explicitly aligned along one slanted ray.
    b.rect(1100,235,430,550,'#F3F0E9',C['line'],1)
    b.text(1120,250,'俯视：实际纵深关系',21,C['ink'],True)
    b.text(1120,283,'A → 两道隔墙 → B',18,C['muted'])
    a=(1155,333);end=(1460,699)
    b.line([(1120,333),(1505,333)],C['muted'],2);b.line([(1120,699),(1505,699)],C['muted'],2)
    b.text(1120,310,'A',18);b.text(1120,706,'B',18)
    for yy,xx,tag in [(455,1256.67,'孔 1'),(577,1358.33,'孔 2')]:
        b.rect(1120,yy-9,xx-1138,18,'#B6C2BB',C['muted'],1)
        b.rect(xx+18,yy-9,1505-xx-18,18,'#B6C2BB',C['muted'],1)
        b.label(xx+32,yy-43,tag,18,C['muted'])
    b.ray([a,end]);b.line([a,(1155,446)],C['ghost'],3,True)
    b.line([(1145,436),(1165,456)],C['muted'],3);b.line([(1165,436),(1145,456)],C['muted'],3)
    b.ellipse(a[0]-7,a[1]-7,14,14,C['green'],None);b.ellipse(end[0]-7,end[1]-7,14,14,C['green'],None)
    b.label(1135,483,'直射被挡',18,C['muted']);b.label(1180,738,'斜射依次穿过两孔',20,C['green'])
    b.footer([('观察前方障碍','切侧面看两道错孔'),('调入口镜角度','让斜光依次穿孔'),('化光进入 B 面','到接收镜旁落脚'),('接通固定回程','返回 A 面墙后'),('照亮颈侧光鳞','沿楼梯上观星台')],
             '只新增角度与落点的关系；本关不搬动回程镜。操作区留遮阴位，充能区紧邻镜架。')
    boards.append(b)

    b=Board('14-rootcellar-flow','14  暖房根室｜四室接光，完成最后的组合题','场景：根系分隔的前方四室＋后方上下廊　　身体局部：胸侧　　核心：跨层绕墙 → 稳定落点 → 再穿根隙')
    b.rect(75,240,875,545,'#EFF1EA',C['line'],2);b.rect(995,240,535,545,'#EEF0F4',C['line'],2)
    b.text(90,250,'A 面｜四个隔间',21,C['ink'],True);b.text(1010,250,'B 面｜两条后廊',21,C['ink'],True)
    # A real wall separates top rooms. The lower root partition has only a light-sized slit.
    b.rect(490,285,25,220,'#BAC5BB',C['ink'],2)
    b.rect(490,525,25,132,'#BAC5BB',C['ink'],2);b.rect(490,687,25,68,'#BAC5BB',C['ink'],2)
    b.ground(80,480,865,45);b.ground(80,755,865,30)
    b.ground(1000,480,525,45);b.ground(1000,755,525,30)
    b.text(105,290,'A1 入口室',20);b.text(545,290,'A2 右上室 / 稳定点Ⅰ',20,C['green'])
    b.text(105,545,'A3 左下室 / 稳定点Ⅱ',20,C['green']);b.text(550,545,'A4 终点室',20)
    b.text(1015,290,'上廊',20);b.text(1015,545,'下廊',20)
    b.beast(840,582,100,128,885,638,'胸侧局部')
    p=(265,425);a2=(710,425);a3=(265,700);a4=(760,655);t=(885,638)
    u1=(1100,425);u2=(1430,391);l1=(1100,700);l2=(1430,665)
    b.source(143,346,'光井');b.ray([(143,346),p]);b.ray([u1,u2]);b.ray([l1,l2]);b.ray([a3,a4,t])
    b.mirror(*p,tag='入口镜');b.mirror(*a2,tag='斜向镜');b.mirror(*a3,tag='送光穿隙');b.mirror(*a4,a3,t,tag='固定末镜',fixed=True)
    b.mirror(*u1,tag='上廊调镜');b.mirror(*u2,tag='固定回程',fixed=True);b.mirror(*l1,tag='下廊调镜');b.mirror(*l2,tag='固定回程',fixed=True)
    # Paired ports express connectivity, not a ray going through the sheet's walls.
    def port(x,y,letter,caption,right=True):
        b.ellipse(x-13,y-13,26,26,C['white'],C['purple'],2)
        b.text(x-7,y-12,letter,19,C['purple'],True)
        b.label(x+19 if right else x-148,y-13,caption,17,C['purple'])
    port(310,365,'α','→ B 上廊');port(1050,351,'α','来自 A1')
    port(1363,338,'β','→ A2',False);port(585,352,'β','来自 B 上廊')
    port(746,363,'γ','→ B 下廊');port(1050,607,'γ','来自 A2')
    port(1370,610,'δ','→ A3',False);port(144,620,'δ','来自 B 下廊')
    b.spirit(115,474);b.walk([(130,465),(205,465)])
    b.walk([(1137,465),(1370,465)]);b.walk([(1137,740),(1370,740)])
    b.lightmove([(306,694),(476,677),(531,672),(720,654)])
    b.walk([(815,740),(934,740)])
    b.badge(225,375,1);b.badge(1192,379,2);b.badge(647,404,3);b.badge(1195,650,4);b.badge(219,662,5);b.badge(826,710,6)
    b.label(342,589,'根隙只容光核',17,C['muted']);b.line([(426,613),(500,673)],C['muted'],2)
    b.label(106,792,'第 13 关 → A1；A4 同高塌墙 → 第 15 关',18,C['blue'])
    b.label(1005,792,'α β γ δ：同字母为跨层两端',17,C['purple'])
    b.footer([('A1 → B 上廊','沿入口光路跨层'),('上廊调镜 → A2','保留光路 / 稳态Ⅰ'),('A2 斜向跨层','经空井抵达 B 下廊'),('下廊调镜 → A3','保留光路 / 稳态Ⅱ'),('A3 接光穿根隙','化光到 A4 终点室'),('胸侧光鳞受光','同标高进入中央心庭')],
             'γ 穿过前后台之间的开放空井，不穿楼板。前层隔间与后层上下廊均不能直接步行贯通。')
    boards.append(b)
    return boards

if __name__=='__main__':
    for b in build(): b.save()

"""Late-game diagrams: observation deck, eyelid gallery and heart garden."""
from draw_core import *

DESIGNS = {
    '12': {
        'title': '露天观星台', 'body': '角根旁局部',
        'knowledge': '光源沿竖向轨道持续照到升降镜；改变镜高使出射光越过固定遮挡。配重可锁止，不限时。',
        'steps': ['从 11 关塔内楼梯登上观星台', '在左侧安全地面操作配重卷轴，将观测镜从低档升到高档', '锁止配重；高位光越过挡板，经固定末镜照亮角根旁光鳞', '穿过挡板下方通道，沿矮台到外侧检修梯，下到 13 关'],
        'risk': '高低两档都需持续截获入射光；低位光明确被瓦檐挡住。锁止状态不自行回落。出口开启以鳞片受光为条件，不能直接走过而跳过目标。',
        'story': '站上花园高处，只看见巨大的角根表面；不展示完整角或全兽。',
    },
    '13': {
        'title': '面颊花廊', 'body': '眼睑旁局部',
        'knowledge': '只重用遮光与转镜，降低认知负荷，让巨兽局部回应成为重点。',
        'steps': ['沿 12 关外梯到花廊', '转动叶柄拉杆，翻开遮住光路的花瓣', '在安全地面微调小镜，把光送到眼睑旁的鳞片', '观察眼睑轻动，沿旧灌溉槽下行进入 14 关'],
        'risk': '光照目标是眼旁鳞片，不是眼球。花瓣和小镜都可在实体地面操作；不设置限时收拢。',
        'story': '一小段眼睑轻轻起伏，回应玩家；避免完整眼睛、脸部或突然开眼惊吓。',
    },
    '15': {
        'title': '中央心庭', 'body': '胸前光鳞',
        'knowledge': '独立光路逐一接通并保留；三束光汇向同一胸鳞，综合旧规则，不添加新能力。',
        'steps': ['从 14 关塌墙进入同标高心庭', '对准左环镜 A，接通第一条独立光路', '登矮台对准中环镜 B，保留第一路', '对准右环镜 C，三路持续照向胸前光鳞', '步入中央暖区，观看胸前光脉与新芽回应'],
        'risk': 'A/B/C 各有独立入光孔，不让一面普通镜凭空分光；三路可交换完成顺序，操作地面始终可达。终局不引入未经确定的主角牺牲或消失。',
        'story': '胸前光脉汇合，新芽长出。镜头始终停留在局部庭院与身体表面。',
    },
}


def build12():
    b=Board('12-observatory-deck-flow','12  露天观星台｜升高接光，锁住配重',
            '场景：同一观测塔的屋顶 → 外侧检修梯　　身体局部：角根旁　　核心：改变镜高，越过固定遮挡')
    b.rect(80,230,1440,545,'#F1F4F1',None)
    b.ground(80,700,1035,80)
    b.ground(1115,656,100,124);b.ground(1215,612,100,168);b.ground(1315,568,205,212)
    # Roof-eave screen: light at low height is blocked, body passes below.
    b.rect(820,505,225,126,C['ground'],C['ink'],3)
    b.line([(832,515),(1034,620),(832,620),(1034,515)],C['line'],2)
    b.label(838,542,'旧瓦檐挡板',21,C['ink'])
    b.label(820,744,'下方可步行通过',18,C['muted'])
    # Light must remain above all traversable steps and the eave.
    src=(650,270);M=(650,427);R=(1210,427);T=(1430,404)
    b.beast(1360,247,160,255,*T,'仅角根旁局部')
    b.source(*src,'天光入射');b.ray([src,M,R,T])
    b.mirror(*M,src,R,'观测镜',fixed=False)
    b.mirror(*R,M,T,'固定末镜',fixed=True)
    b.mirror(650,595,ghost=True)
    b.line([(675,595),(819,595)],C['ghost'],3,True)
    b.line([(811,580),(827,610),(811,610),(827,580)],C['ghost'],2)
    b.label(690,618,'低档光被挡住',18,C['muted'])
    b.line([(606,355),(606,645)],C['muted'],3)
    b.line([(694,355),(694,645)],C['muted'],3)
    b.arrow([(725,577),(725,459)],C['green'],3)
    b.label(760,325,'升至高档后锁止\n光路与配重都保持',21,C['green'])
    # Rope and locking crank stay on a safe, reachable surface.
    b.ellipse(635,340,30,30,None,C['muted'],3)
    b.line([(650,340),(430,340),(430,637)],C['muted'],3)
    b.ellipse(405,638,50,42,None,C['green'],3)
    b.line([(430,657),(466,657),(466,642)],C['green'],3)
    b.label(317,727,'卷轴 / 锁止杆',20,C['green'])
    b.rect(353,355,40,68,'#B6BEB7',C['muted'],2)
    b.line([(373,355),(373,340),(430,340)],C['muted'],2)
    b.label(245,385,'配重',18,C['muted'])
    b.line([(310,399),(350,399)],C['muted'],1)
    b.spirit(130,694)
    b.walk([(149,682),(365,682)])
    b.walk([(485,680),(1065,680)])
    b.jump((1090,680),(1160,641));b.jump((1184,641),(1260,597));b.jump((1285,597),(1370,553))
    # Exit ladder is outside the platform silhouette.
    b.line([(1493,570),(1493,766)],C['muted'],3);b.line([(1520,570),(1520,766)],C['muted'],3)
    for yy in range(580,766,20):b.line([(1493,yy),(1520,yy)],C['muted'],2)
    b.walk([(1390,553),(1505,553),(1505,754)])
    b.badge(250,657,1);b.badge(488,642,2);b.badge(1060,420,3);b.badge(1440,553,4)
    b.label(91,235,'← 第 11 关塔内楼梯',20,C['blue'])
    b.label(1205,793,'第 13 关：沿外梯下行 →',20,C['blue'])
    b.footer([('登上观星台','找到升降观测镜'),('卷轴升镜','低档 → 高档'),('锁止配重','光越瓦檐，角根鳞亮起'),('沿外梯下行','到达面颊花廊')],
             '配重可以锁住，不要求抢时间；低位与高位都能接住同一束垂直来光。')
    return b


def build13():
    b=Board('13-cheek-gallery-flow','13  面颊花廊｜拨开花瓣，得到回应',
            '场景：塔外花廊 → 旧灌溉槽　　身体局部：眼睑旁　　核心：短小、平缓的配光与剧情停留')
    b.ground(80,620,790,155);b.ground(870,652,120,123);b.ground(990,684,120,91);b.ground(1110,716,410,59)
    # Only a cropped body patch and an open eyelid contour, never a complete eye.
    b.beast(1080,245,440,328,1290,442,'眼睑旁鳞片；不照眼球')
    b.line([(1195,282),(1245,297),(1310,311),(1380,320),(1450,323),(1520,320)],'#99A79E',5)
    b.line([(1230,270),(1285,283),(1350,293),(1425,297),(1520,295)],'#D0D7CC',2)
    b.label(1190,237,'眼睑片段（延伸出镜头）',19,C['muted'])
    src=(370,276);M=(700,565);T=(1290,442)
    b.source(*src,'花隙日光');b.ray([src,M,T])
    b.mirror(*M,src,T,'小镜')
    # A petal in the closed position would intersect the incoming ray near (535,420).
    b.line([(511,396),(575,449)],C['ghost'],6,True)
    b.poly([(510,445),(463,424),(440,374),(488,393)],'#E4DACA',C['green'],2)
    b.line([(510,445),(466,401)],C['green'],2)
    b.ellipse(501,436,18,18,None,C['green'],2)
    b.line([(510,445),(533,565)],C['green'],2)
    b.ellipse(520,552,26,26,None,C['green'],3)
    b.label(205,400,'花瓣翻开后固定\n不必赶在它合拢前通过',20,C['green'])
    b.label(554,450,'虚线：原遮光位置',18,C['muted'])
    b.line([(130,255),(130,565)],C['muted'],3);b.line([(156,255),(156,565)],C['muted'],3)
    for yy in range(265,565,20):b.line([(130,yy),(156,yy)],C['muted'],2)
    b.spirit(205,614);b.walk([(220,606),(480,606)]);b.walk([(565,606),(660,606)])
    b.walk([(755,605),(840,605),(920,637),(1040,669),(1170,701),(1475,701)])
    b.badge(250,565,1);b.badge(575,555,2);b.badge(760,545,3);b.badge(1400,690,4)
    b.label(91,229,'← 第 12 关外侧检修梯',20,C['blue'])
    b.label(850,350,'受光反馈：眼睑轻动\n没有开门口令或新能力',20,C['green'])
    b.line([(1047,405),(1190,330)],C['green'],1)
    b.label(1195,755,'第 14 关 → 灌溉槽下行',20,C['blue'])
    b.footer([('沿外梯进入','在花廊放慢脚步'),('翻开花瓣','给小镜让出日光'),('微调小镜','照眼旁鳞，眼睑轻动'),('沿灌溉槽下行','进入暖房根室')],
             '本关主动放缓：只有花瓣与小镜两处操作。回应来自巨兽局部呼吸，不是整只巨兽移动。')
    return b


def build15():
    b=Board('15-heart-garden-flow','15  中央心庭｜三路汇光，迎来新生',
            '场景：塌墙入口 → 下沉种子庭 → 胸前暖区　　身体局部：胸前光鳞　　核心：保留已接通的光路')
    b.rect(80,230,1440,545,'#F5F2E8',None)
    b.ground(80,650,1440,125);b.ground(640,595,160,55)
    # All incoming beams are independent: no fictitious splitting at a normal mirror.
    b.beast(1110,265,410,285,1300,320,'胸前局部 / 光脉逐条亮起')
    S1=(250,255);M1=(330,595);S2=(675,255);M2=(720,540);S3=(1030,255);M3=(1010,595);T=(1300,320)
    for src,m,tag in [(S1,M1,'A'),(S2,M2,'B'),(S3,M3,'C')]:
        b.source(*src,'入光孔 '+tag);b.ray([src,m,T]);b.mirror(*m,src,T,'环镜 '+tag)
    # Faint veins point into the one target; they are tissue response, not extra beams.
    b.line([(1300,344),(1320,394),(1370,454),(1480,517)],'#B8C3AB',2)
    b.line([(1318,320),(1390,305),(1460,288),(1520,280)],'#B8C3AB',2)
    b.line([(1315,338),(1420,398),(1520,451)],'#B8C3AB',2)
    b.spirit(125,644);b.walk([(145,635),(270,635)])
    b.walk([(385,635),(590,635)]);b.jump((605,635),(675,580))
    b.walk([(765,580),(820,635),(960,635)])
    b.walk([(1060,635),(1175,635)])
    b.ellipse(1138,621,150,25,'#F1DCA6',None)
    # Tiny new shoots, not a new mechanic or collectible objective.
    for xx in [1190,1245,1310]:
        b.line([(xx,648),(xx,616)],C['green'],2)
        b.poly([(xx,633),(xx-15,618),(xx-24,620),(xx-13,634)],'#C6DAB7',C['green'],1)
        b.poly([(xx,627),(xx+14,610),(xx+24,612),(xx+12,629)],'#C6DAB7',C['green'],1)
    b.badge(210,601,1);b.badge(392,578,2);b.badge(777,522,3);b.badge(1068,578,4);b.badge(1238,618,5)
    b.label(90,743,'← 第 14 关塌墙（同标高）',20,C['blue'])
    b.label(90,362,'三处独立入光孔\n分别接入三面环镜',19,C['muted'])
    b.label(500,712,'三面镜可交换完成次序；对准后保留状态',20,C['green'])
    b.label(1200,735,'停留观看：萌芽与呼吸',20,C['blue'])
    b.footer([('从塌墙进入','看见胸前汇光目标'),('对准左环镜 A','第一路持续照射'),('对准中环镜 B','保留 A，再接第二路'),('对准右环镜 C','三路汇向同一光鳞'),('走进中央暖区','光脉汇合，新芽长出')],
             '终局收束已有规则。没有新增变身、限时或牺牲要求；玩家可停留，灯芯精灵仍在庭中。')
    return b


def build(): return [build12(),build13(),build15()]

if __name__=='__main__':
    for board in build():
        board.save()
        print(board.key)

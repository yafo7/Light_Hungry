"""Export the later-level design pack. Does not touch the game project."""
from draw_core import *
import part08_10, part11_14, part12_13_15
import xml.etree.ElementTree as ET
import zipfile

boards=sorted(part08_10.build()+part11_14.build()+part12_13_15.build(),key=lambda b:b.key)
designs=part08_10.DESIGNS+part11_14.DESIGNS+[dict(level=int(k),**v) for k,v in part12_13_15.DESIGNS.items()]
designs.sort(key=lambda d:d['level'])

overview=Board('00-route-overview','后半程连续流线','')
overview.items=[]
overview.rect(0,0,W,H,C['paper'],None)
overview.text(50,35,'食光兽｜08—15 关连续流线',38,C['ink'],True)
overview.text(52,93,'07 叶幕出口 → 温室屋顶 → 阁楼 → 藤坡 → 观测塔 → 花廊下行 → 根室 → 中央心庭',22,C['muted'])
overview.text(52,146,'前半段增加空间理解；第 13 关停留，第 14 关综合，第 15 关汇光与新生。',22,C['green'])
cards=[
    (8,'雨槽屋脊','背脊表皮','滑镜对准窄口','短跳 → 化光跨断槽','横向强化'),
    (9,'园丁阁楼','肩部局部','换观察面找拉绳','攀袋堆 → 收起挡光布','观察与遮挡'),
    (10,'常春藤坡','肩颈局部','露珠穿隙＋实体连跳','沿藤根向右上攀爬','移动组合'),
    (11,'观测暗室','颈侧局部','斜射穿过纵深错孔','A → B → A 墙后','新空间认知'),
    (12,'露天观星台','角根旁局部','升镜越挡板、锁配重','塔内上来 → 外梯下行','旧规则组合'),
    (13,'面颊花廊','眼睑旁局部','翻花瓣、调小镜','局部回应 → 灌溉槽下行','情绪缓冲'),
    (14,'暖房根室','胸侧局部','前后层四室接光','两处稳定点 → 光穿根隙','综合谜题高点'),
    (15,'中央心庭','胸前光鳞','保留三路独立光束','光脉汇合 → 新芽生长','终局收束'),
]
for i,(num,title,body,idea,route,role) in enumerate(cards):
    col=i if i<4 else 7-i;x=50+col*380;y=240 if i<4 else 660
    overview.rect(x,y,340,260,C['white'],C['line'],2)
    overview.text(x+20,y+20,f'{num:02}',28,C['green'],True)
    overview.text(x+20,y+65,title,29,C['ink'],True)
    overview.text(x+20,y+116,body,20,C['muted'])
    overview.text(x+20,y+154,idea,20,C['ink'])
    overview.text(x+20,y+189,route,18,C['muted'])
    overview.text(x+20,y+225,role,19,C['green'])
for i in range(3):overview.arrow([(390+i*380,370),(425+i*380,370)],C['blue'],4)
overview.arrow([(1360,505),(1360,650)],C['blue'],4)
overview.label(1100,565,'同一座塔：室内 → 屋顶',21,C['blue'])
for i in range(3):overview.arrow([(1190-i*380,790),(1155-i*380,790)],C['blue'],4)
overview.text(52,974,'图件包含实体路线、光路、交互点、巨兽局部和相邻关卡接口；各关详图另附。',22,C['ink'])
overview.text(52,1021,'概念方案 v01｜普通跳距、角色净空、反射角及跨层切换尚需 Unity 灰盒验证。',20,C['muted'])

images=[b.save() for b in boards];overview.save()
for offset,name,title in [(0,'00-contact-08-11','08—11｜从屋顶走进观测塔'),(4,'00-contact-12-15','12—15｜从高处返回心庭')]:
    im=Image.new('RGB',(2000,1510),C['paper']);d=ImageDraw.Draw(im)
    d.text((35,25),title,font=ImageFont.truetype(BOLD,34),fill=C['ink'])
    d.text((38,76),'四关总览；请打开单张 PNG / SVG 查看文字与细节。',font=ImageFont.truetype(FONT,22),fill=C['muted'])
    for i,pic in enumerate(images[offset:offset+4]):im.paste(pic.resize((980,674),Image.Resampling.LANCZOS),(10+(i%2)*1000,125+(i//2)*690))
    im.save(OUT/(name+'.png'))

elements=[];serial=0
for bi,b in enumerate([overview]+boards):
    ox=0 if bi==0 else ((bi-1)%2)*1680
    oy=0 if bi==0 else (1+(bi-1)//2)*1180
    for e in b.items:
        serial+=1;typ=e['type'];x=e.get('x',0);y=e.get('y',0);w=e.get('w',0);h=e.get('h',0)
        if typ in ('line','poly'):
            pts=e['pts'];x=min(p[0] for p in pts);y=min(p[1] for p in pts);w=max(p[0] for p in pts)-x;h=max(p[1] for p in pts)-y
        if typ=='text':
            f=ImageFont.truetype(BOLD if e['bold'] else FONT,e['size']);w=max(f.getlength(l) for l in e['t'].split('\n'));h=e['size']*1.35*len(e['t'].split('\n'))
        item=dict(id=f'later-flow-{serial:05d}',type={'rect':'rectangle','poly':'line'}.get(typ,typ),x=x+ox,y=y+oy,width=max(w,.01),height=max(h,.01),angle=0,
                  strokeColor=e.get('color',e.get('stroke')) or 'transparent',backgroundColor=e.get('fill') or 'transparent',fillStyle='solid',strokeWidth=e.get('lw',1),strokeStyle='dashed' if e.get('dash') else 'solid',roughness=0,opacity=100,
                  groupIds=[],frameId=None,roundness=None,seed=serial*31,version=1,versionNonce=serial*17,isDeleted=False,boundElements=None,updated=1793664000000,link=None,locked=False)
        if typ in ('line','poly'):
            pp=pts+[pts[0]] if typ=='poly' else pts
            item.update(points=[[px-x,py-y] for px,py in pp],lastCommittedPoint=None,startBinding=None,endBinding=None,startArrowhead=None,endArrowhead=None)
        if typ=='text':item.update(fontSize=e['size'],fontFamily=2,text=e['t'],originalText=e['t'],textAlign='left',verticalAlign='top',containerId=None,autoResize=True,lineHeight=1.35,baseline=e['size'])
        elements.append(item)
editable=OUT/'levels08-15-flow.excalidraw'
editable.write_text(json.dumps(dict(type='excalidraw',version=2,source='local',elements=elements,appState=dict(viewBackgroundColor=C['paper'],gridSize=None),files={}),ensure_ascii=False),encoding='utf-8')
(OUT/'designs.json').write_text(json.dumps(designs,ensure_ascii=False,indent=2),encoding='utf-8')

intro='''# 第 08—15 关：空间与行动流线 v01

2026-11-03。延续上一版 04—07 关以及用户提供的两份 Excalidraw 谜题草案。本包是可修改的关卡构思，没有改动 Unity 工程或既定全局能力文档。

## 使用文件

- `00-route-overview.png/.svg`：后半程连续空间与难度节奏。
- `08-*.png/.svg` 至 `15-*.png/.svg`：八张独立流线图，每张 1600 × 1100。
- `00-contact-08-11.png`、`00-contact-12-15.png`：分组总览。
- `levels08-15-flow.excalidraw`：总览与八张原生可编辑矢量图，非截图贴入。
- `designs.json`：结构化设计记录。
- `draw_core.py`、`part*.py`、`build_all.py`：绘制与导出源代码。

## 规则与读图

蓝线为实体行走/跳跃；金线为有效光束；橙色虚箭头为玩家化光后的行动顺序；紫线或配对字母表示纵深跨层。光态仍由玩家手动前后控制，箭头不强制单向。部分橙线略偏离金线是为了读图，实际光态贴合光路。

图示是完成态，按步骤逐段建立。安全落点恢复实体、配重锁止、根缝响应、具体跨层镜表现均为本版设计建议，不是对已有全局规则的静默修改。调镜位旁应留遮阴操作区与相邻充能位，避免自动三秒化光干扰操作。

后半程不是逐关机械加码：08—10强化既有能力并交替平台与观察，11引入角度/纵深关系，12组合旧机关，13短暂舒缓，14综合达到难度高点，15用熟悉的操作收束剧情。

## 各关设计

'''
parts=[intro]
for row in designs:
    parts.append(f"### {row['level']:02} {row['title']}｜{row['body']}\n\n")
    parts.append(row.get('knowledge',row.get('idea',''))+'\n\n')
    if 'steps' in row:parts.append('\n'.join(f'{i+1}. {step}' for i,step in enumerate(row['steps']))+'\n\n')
    else:parts.append(row['route']+'。\n\n')
    risk=row.get('risks',row.get('risk',''))
    parts.append('检查要点：'+('；'.join(risk) if isinstance(risk,list) else risk)+'\n\n')
    if row.get('story'):parts.append('剧情反馈：'+row['story']+'\n\n')
parts.append('''## 空间连续性

07 叶幕破玻璃口 → 08 同一温室雨槽 → 09 阁楼窗 → 10 通风口的常春藤坡 → 11 观测塔低处破窗 → 12 同塔楼梯登屋顶 → 13 外梯下降到面颊花廊 → 14 旧灌溉槽下到暖房根室 → 15 同标高塌墙进入中央心庭。

11 的 A/B 主图是同高前后走道的展开，右侧俯视小图才展示两道孔的实际纵深。14 的 A 面四室与 B 面上下后廊是分开绘制的剖面，αβγδ是同一条光路在两端的索引；γ通过走道之间的空井斜下，不穿楼板，不引入随意传送的能力。

## 验证边界

已进行图片目视检查、SVG XML 解析、PNG 尺寸与 Excalidraw JSON/唯一 ID 检查。没有在 Excalidraw 编辑器内验收导入，也没有做 Unity 光学求解、最大跳距、角色碰撞与软锁实测。

所有场景只展示同一巨兽的浅白身体局部及灰暗待照光鳞；13不把光直照眼球，15不安排未经确定的主角牺牲、消失或展示全兽。
''')
(OUT/'README.md').write_text(''.join(parts),encoding='utf-8')

assert len(boards)==8 and [int(b.key[:2]) for b in boards]==list(range(8,16))
assert len({e['id'] for e in elements})==len(elements)
assert len(json.loads(editable.read_text(encoding='utf-8'))['elements'])==len(elements)
for b in [overview]+boards:
    ET.parse(OUT/(b.key+'.svg'))
    with Image.open(OUT/(b.key+'.png')) as im:assert im.size==(W,H)
(OUT/'validation.json').write_text(json.dumps(dict(levels=list(range(8,16)),png_size=[W,H],vector_boards=9,native_elements=len(elements),svg_xml='pass',excalidraw_structure='pass',unique_ids=True,visual_review='performed',unity_validation='not performed',editor_import='not performed'),indent=2),encoding='utf-8')
with zipfile.ZipFile(OUT/'levels08-15-flow-pack.zip','w',zipfile.ZIP_DEFLATED) as z:
    for f in sorted(OUT.iterdir()):
        if f.is_file() and f.suffix in ['.png','.svg','.excalidraw','.md','.json','.py']:z.write(f,f.name)
print(json.dumps(dict(directory=str(OUT),levels=8,editable_elements=len(elements),pack_bytes=(OUT/'levels08-15-flow-pack.zip').stat().st_size),ensure_ascii=False))

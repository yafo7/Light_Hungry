# light hungry

Unity **2022.3.62f3c1**，原生 2D 基础交互工程。用已生成的基础素材搭建一个可运行的测试房间；各系统均有独立预制体，后续可组合到新场景。

## 打开与运行

Unity Hub → Add project from disk → 选择本目录。打开 `Assets/LightHungry/Scenes/InteractionLab.unity`，点击 Play。

也可运行 `Builds/Windows/LightHungry.exe` 查看已构建的 Windows 原型。整个 `Builds/Windows` 目录需要一起保留，不要只拷贝 exe。

## 控制

| 按键 | 功能 |
| --- | --- |
| A / D | 左右移动 |
| W / S | 上下观察，同时朝上／下瞄准；松开后回到面向方向 |
| Space | 跳跃，短按跳得更低 |
| J | 切换身体反射 |
| K | 空腹时张嘴／闭嘴；储光后释放；释放中再次按下提前结束 |
| E | 操作最近的镜子、滑轮拉杆、手电筒 |
| 按住 E + A / D | 抓住箱子后推拉 |
| Q | 靠近镜子时切换镜面尺寸 |
| R | 重开当前房间，恢复全部初始状态 |
| Esc | 暂停／恢复 |
| F1 / F2 | 调试：切换反射／吞光解锁状态 |

## 房间中的内容

- 左侧窗光和台阶：验证身体反射、吞光、放光和高低差。
- 地面箱子与练习镜：验证抓取、遮光、镜面调角与尺寸变化。
- 上方光学台：固定手电筒、凸／凹透镜和接收器；手电筒可按 E 切换。
- 右侧镜子台座、双按钮和滑轮拉杆：站按钮向左／右移动；两侧同时压下停止；拉杆可将台座移动到另一端。
- 高处的野兽：光照到口部接收区，累计 3 秒即喂饱。
- 尖刺与跌落：重开当前房间。

房间默认解锁反射和吞光，适合联调。在 `Lab Session` 的 Inspector 取消 `Start With Reflection` / `Start With Swallow`，即可验证能力逐步获得；`ReflectUnlock` 和 `SwallowUnlock` 预制体可用于解锁。

## 实现规则

角色反射采用之前讨论的“沿观察方向定向反光”能力；场景平面镜使用入射角反射。两种规则有明确区分。反射时不自动吞光，张嘴吞到光后恢复普通状态。

储光容量 1 份。储光后按 K 释放 4 秒，释放时角色不能走跳，输出方向锁定；提前取消同样消耗该份储光。镜子、墙体、箱子、角色和接收区每帧重新计算光路，最多 12 次交互，避免无限反射。

透镜的首版**只改变光束宽度**，不模拟真实折射、焦点和能量增强。镜子缩放改变实际光学命中面长度，不提高能量。滑轮使用确定性的轨道运动，不模拟绳索张力。这些边界在脚本 Inspector 中也有提示。

## 继续开发

- 源码：`Assets/LightHungry/Runtime/`。
- 可复用预制体：`Assets/LightHungry/Prefabs/`。
- 美术：`Assets/LightHungry/Art/`，已按单帧设置透明、PPU 与 Pivot。
- 工程生成器：`Assets/LightHungry/Editor/BuildInteractionLab.cs`。
- 预制体与参数说明：`Documentation/预制体与工程结构.md`。
- 自动运行检查与截图：`Verification/`。

菜单 `Light Hungry → Build fresh InteractionLab` 会重建生成的场景和预制体。开始手工修改之后，不要随意执行重建菜单，以免覆盖自己的改动；普通修改直接编辑现有资源即可。

美术来源为本聊天生成的 `BasicArt_v01`，未使用外部图库。本工程没有更改其他已有 Unity 项目。

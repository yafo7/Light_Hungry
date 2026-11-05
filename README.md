# light hungry

## 当前版本：v0.7.0-alpha.1（七关水彩原型）

本次版本同步了七关水彩原型的源工程、关卡设计与美术资产。完整更新见 [CHANGELOG.md](CHANGELOG.md)，版本标识见 [VERSION](VERSION)。它是原型预发行版本，第八关尚未实装。

仓库包含独立工程，请在 Unity Hub 中选对目录：

| 目录 | Unity 版本 | 用途 |
| --- | --- | --- |
| [`UnityProject-NOVA-URP/`](UnityProject-NOVA-URP/) | **2022.3.62f3c1**，URP 14.0.12 | 当前七关水彩原型及主菜单、地图选关、暂停与声音设置 |
| [`UnityProject/`](UnityProject/) | 2021.3.42f1c1，内置管线 | 保留的原版工程 |
| 仓库根目录的 `Assets/`、`Packages/`、`ProjectSettings/` | **2022.3.62f3c1** | 原有程序实验工程，保留原目录和历史 |

两套运行逻辑保持独立。打开七关版本：Unity Hub → Add project from disk → 选择 **`UnityProject-NOVA-URP`**，使用已安装的 Unity **2022.3.62f3c1**。打开 `Assets/Scenes/GameMenu.unity` 后 Play 可进入完整菜单流程，也可直接 Play 任意独立关卡。构建设置以 GameMenu 为入口，随后为第一至第七关。

本地完整试玩程序为 `Builds/LightHungry-Complete/LightHungry.exe`，根目录 `开始试玩.cmd` 指向此版本。分享时须复制整个 `LightHungry-Complete` 文件夹。菜单 `Light Hungry/界面与流程/构建完整 Windows 游戏` 只更新流程配置并构建，不重建七关布局。

关卡目录位于 `Assets/Resources/GameShell/LevelCatalog.asset`，可配置名称、独立 Scene、缩略图、地图位置与可用状态。新增关卡后运行菜单中的“安装或更新游戏界面”将对应 Scene 纳入构建。新关卡的完成事件调用 `GameSession.Instance.CompleteLevel()`；现有七关由适配层和出口组件接入。菜单、音量和通关记录独立于各关布局。

### 下载源文件

模型、图片和素材包使用 Git LFS。克隆后执行：

```sh
git lfs install
git clone https://github.com/yafo7/Light_Hungry.git
cd Light_Hungry
git lfs pull
```

不要把 LFS 指针文件当成真实图片或模型导入 Unity。GitHub 的源代码 ZIP 是否包含实际 LFS 文件取决于仓库设置，建议使用上述克隆流程。

美术源文件位于 [`art/`](art/)，第七关的拆分与复用说明见 [分层素材说明](art/level07-unity-assets-v01-2026-11-05/README.md)。源码与全部对应 `.meta` 纳入管理；Unity 缓存、构建程序、自动验证产物和重复 ZIP 保留在本地。Git 克隆后需自行构建试玩程序。

在七关工程中用 `JourneyBuilder.Build` 构建完整流程，避免重新生成已经手工调整的布局。根目录 Windows 快捷脚本使用本机 Unity 2022 路径，换电脑时应调整路径或直接使用 Unity 编辑器菜单。

Unity 2022 完整流程构建成功，第七关输入驱动回归 **121/121**，第六关 **123/123**。界面验证入口为 `--journey-qa`，覆盖菜单、鼠标命中、选关、暂停、设置、记录与场景切换，使用独立 QA 存档键。自动检查不代替玩家对所有玩法组合和手感的验收。

## 原有程序实验工程说明（Unity 2022.3）

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

# 第六关温室苗床：分层素材与可复用构件

本目录保存第六关的背景、地形和机关图集、生成提示词与 Unity 素材包。关卡已接入 `UnityProject-NOVA-URP/Assets/Scenes/SixthGreenhouse.unity`，具体操作与搭建入口见 [第六关说明](C:/Users/10029/Documents/ChatGPT/食光兽/UnityProject-NOVA-URP/第六关说明.md)。

**素材内容与来源**

三张素材均使用内置 imagegen，沿用已确定的水彩温室风格。提示词随素材保存，便于后续调整同一套画风和拆分要求。

| 文件 | 内容 | 对应提示词 |
|---|---|---|
| `background.png` | 柔化、减淡的不透明温室后景；不承担平台或机关碰撞 | `background-prompt.txt` |
| `terrain-atlas.png` | 1536 × 1024 透明 RGBA 图集；苗床岸壁、玻璃台、陶片踏板、遮光布、浅白肋侧、根须出口拱架 | `terrain-prompt.txt` |
| `props-atlas.png` | 1536 × 1024 透明 RGBA 图集；独立镜面头、底座、卷轴、托盘、滑轮、手柄 | `props-prompt.txt` |
| `atlas-inspection.json` | 两张图集的尺寸、alpha 范围与各构件边界记录 | — |
| `GardenReusable.unitypackage` | 9 个可复用预制体及其引用依赖 | 由 Unity 编辑器导出 |

这是“完整后景 + 两张透明图集”的交付，不是每个构件各一张 PNG。各构件在 Unity 中通过网格 UV 选择图集区域，原始图集保留不变。透明区域使独立构件能够叠放到后景上；碰撞和机关状态不烘焙在图片里。

**Unity 中的文件对应**

| 位置 | 作用 |
|---|---|
| `Assets/Art/Level06` | 导入的三张素材 |
| `Assets/Art/GardenReusableGeometry` | 构件与场景绘图片使用的网格资产 |
| `Assets/Materials/GardenReusable` | 水彩层与交互描边等材质 |
| `Assets/Prefabs/GardenReusable` | 9 个可拖入场景的构件预制体 |
| `Assets/Scripts/GardenReusableProp.cs` | 构件类型、可动部件引用与镜面转向辅助 |
| `Assets/Editor/SixthLevelBuilder.cs` | 图集裁切、场景摆放、碰撞配置与素材包导出 |
| `Assets/Scripts/SixthGreenhouseFlow.cs` | 第六关光路、机关联动、交互与通关逻辑 |

场景实际分为 `01_Background_NoCollision`、`02_Horizontal_Foreground_Collisions`、`03_Reusable_Interactive_Props` 三组，另有主角、唯一光路、关卡控制器与正交摄像机。前景以正对摄像机的平面网格呈现水彩画面；主角为真实骨骼 3D 模型，使用 2D 平台移动与碰撞。

**9 个可复用预制体**

| 文件名 | 构件 |
|---|---|
| `GardenMirror.prefab` | 镜面头 + 固定底座、短支臂和轴钉 |
| `GardenWinch.prefab` | 升降卷轴 |
| `NurseryTray.prefab` | 育苗托盘 |
| `Pulley.prefab` | 滑轮 |
| `Lever.prefab` | 小手柄 / 绳扣 |
| `Bank.prefab` | 苗床岸壁 |
| `GlassDeck.prefab` | 玻璃站台 |
| `TerracottaStep.prefab` | 陶片踏板 |
| `RootArch.prefab` | 根须拱架 |

反光镜的 `RotatingMirrorHead` 是独立子对象，调镜时只转镜面头，不转底座和安装支臂。镜面朝向由当前入射与出射方向计算，A、C、D 的角度档和 P 的固定反射用途由本关控制器决定。

**复用时包含什么**

`GardenReusable.unitypackage` 使用 `Recurse | IncludeDependencies` 导出，包含上述预制体及其引用的纹理、材质、网格、脚本、着色器等依赖。它适合把同一套水彩构件带到后续关卡，或导入兼容渲染设置的 Unity 工程。

预制体的范围是**美术 + 元数据和镜面转向辅助**，不是通用的完整谜题系统。场景 builder 单独添加平台、隔板、托盘和出口的碰撞体；升降、绳线连接、交互范围、光路状态、恢复实体安全落点与通关条件由场景配置和玩法脚本组成。导入素材包后需要为新关卡配置这些关系，不能把“导入成功”当作机关可用或关卡可通关。

本关运行时的光束、绳线、遮光窄孔、暗鳞颜色和亮起反馈也不是一张固定概念图的内容：它们由场景对象与脚本控制。食光兽肋侧保持浅白，目标鳞单独变暗、受光后再变亮。

**搭建、试玩与验收**

- 双击工作区 [试玩第六关.cmd](C:/Users/10029/Documents/ChatGPT/食光兽/试玩第六关.cmd)，或打开 Unity 场景 `Assets/Scenes/SixthGreenhouse.unity`。
- A/D 移动，空格跳跃，站在有效光中静止 3 秒化光；光态 E 在安全落脚面上方恢复实体并短落，实体 E 操作机关，W/S 调镜，Tab 全景，R 重来。
- [Tools/Build-SixthGreenhouse.ps1](C:/Users/10029/Documents/ChatGPT/食光兽/Tools/Build-SixthGreenhouse.ps1) 默认重新生成场景、预制体与素材包并构建试玩；`-PlayerOnly` 只构建现有场景，`-Validate` 构建后运行原生验证，`-ValidateOnly` 验证已有试玩。
- 视觉修订后的原生复测：123 项断言全部通过，捕获运行错误为 0，已完成全关并目视检查关键截图；结果见 [playthrough.json](C:/Users/10029/Documents/ChatGPT/食光兽/Validation/level06/native-run/playthrough.json)。
- 素材核对：9 个预制体均有真实场景源引用；素材包包含 39 个资产；导入的三张图片与交付原图一致。详见 [delivery-audit.json](C:/Users/10029/Documents/ChatGPT/食光兽/Validation/level06/delivery-audit.json)。尚未在空白项目内测试导入，不把素材包导出等同于跨工程验证。

第七关当前仅有两面视角联动设计，未包含在这份可玩场景或素材包内。9 个构件可供其后续复用，不表示第七关已经实装。

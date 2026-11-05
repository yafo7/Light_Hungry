# 食光兽 · 第一缕光

Unity 2021.3.42f1c1，内置渲染管线。第一关「门槛苔地」的可运行美术与玩法原型。

## 打开

使用 Unity Hub 添加本目录，打开 `Assets/Scenes/FirstLight.unity`，点击 Play。
已有 Windows 版本位于工程上一级的 `Builds/Windows/LightFeast.exe`。

## 操作

- A / D 或左右方向键：水平移动。
- 空格：跳跃；开场时跳过镜头推近。
- E：靠近园灯下的小把手进入/退出调整；尾鳞苏醒后在陶盆缺口进入下一段。
- 调整时 A / D：镜片绕竖直轴左右转向。持续照亮尾尖约 2.2 秒。
- Tab：全景/游玩镜头。R：重新开始。Esc：松开镜子。

完整流程：进入花园 → 越过小碎砖 → 操作园灯反光片 → 尾尖受光并呼吸 → 前往陶盆缺口。
本原型在陶盆入口结束；第二关尚未制作。

## 场景与比例

摄影机正交、正对围墙。角色在 XY 平面移动；背景与远景不参与平台碰撞。
主角包含发冠的高度为 1 H，游玩画面高度为 16 H；开场展示约 96 H 宽的庭园。
陶盆宽 25.2 H、高约 11 H，入口交互区为 1.6 H。关卡地面表面在 Y=6。

Hierarchy 按背景、地面、交互物、光学构件、角色、流程组织：

- `01_Background_2D_NoCollision`：水彩背景板。
- `02_WalkableGround_XY`：地面图层、平台碰撞体、小碎砖。
- `03_InteractiveProps`：园灯固定部分、尾尖与石阶、陶盆。
- `04_OpticalDepth_3D`：真实 3D 反射镜面、光源、光鳞碰撞体、光束。
- `05_Character_3D`：用户 GLB 转换的网格预制体、运动和发梢粒子。
- `06_LevelFlow_AndValidation`：引导、过关、原生运行验证器。

反射使用 Physics.Raycast + Vector3.Reflect。镜片竖直轴 yaw，安装倾角固定。
光源、镜面和目标在 Z 方向错开，从而在正交画面中同时保持镜面可见与光路成立。
背景图只作画面层；光学的深度坐标不是自由行走方向。

## 美术源文件和主角

`Assets/Art/Level01` 中的 PNG 与已选分层美术字节一致。没有重绘源图。
材质在运行时裁掉透明留白，并隐藏园灯原图中的固定镜片；可旋转镜片独立绘制。
尾尖仅左侧网格产生微小呼吸，石阶一侧保持固定。

`Assets/Models/Source/WickSpirit.original.glb` 是用户原始模型的保留副本。
`Assets/Models/Runtime/WickSpirit.glb` 是 gltfpack v1.3 的轻量副本：
原始 1,988,934 三角面 → 99,446 三角面，59,639 顶点。
`Assets/Prefabs/WickSpirit.prefab` 是 Unity 可检查、可替换的实际 3D 模型。
模型没有骨骼。当前移动为程序摆动和脚部顶点位移；不是正式的骨骼走路动画。
发梢效果为世界空间余烬粒子和柔光。原 GLB 头发仍是实体网格，未实现发尾透明溶解。

## 重建与验证

编辑器菜单「食光兽 / 重建第一关原型」可重建场景；这会重写生成的场景与材质。
若手动修改场景，请先另存一份。原 PNG、原 GLB 不会被重建流程改写。
关闭此工程的编辑器后，在上一级目录运行：

```powershell
powershell -ExecutionPolicy Bypass -File .\Tools\Build-FirstLight.ps1 -Validate
powershell -ExecutionPolicy Bypass -File .\Tools\Verify-SourceAssets.ps1
```

`Validation/native-run` 包含原生 Direct3D 玩家截图、自动流程 JSON 和日志。
验证器驱动与人工游玩相同的移动、跳跃、操作、旋转输入；没有传送角色或直接设置过关。
自动通关不代替玩家对手感、美术或关卡趣味性的验收。

## 第三方工具

模型简化使用 [meshoptimizer / gltfpack v1.3](https://github.com/zeux/meshoptimizer/releases/tag/v1.3)，MIT 许可。
工具和许可位于工程上一级 `.tools/gltfpack-v1.3`；它不属于游戏运行依赖。
自定义 GLB 导入器只支持此模型的静态单节点、单材质、无压缩 float 顶点格式；导入其他模型请使用完整 glTF 导入方案。

# Lever 骨骼动画与 Mirror 侧视图 · v07

本次交付只修订 Lever 与 Mirror。v06 角色待机和 Wheel 保留在原目录中。

## Lever

- 两根骨骼：固定的 root，以及位于底部轴心的 handle。
- 活动杆、握把、底部连接端是一张固定贴图，全部跟随 handle；底座位于固定 root，遮住活动杆的连接端。
- 动画只改变 handle 角度：左 −43° → 右 +43°，每次 0.64 秒。使用五次缓动，起止速度与加速度为零。途中反向保持当前姿态，重新缓入。
- 骨骼长度 224.28 px，固定转轴 (256, 372)，动画缩放始终为 (1, 1)。这里的骨长是转轴到握把中心的距离。
- `Textures/lever_arm.png` 与 `lever_base.png`：512×512 透明层。
- `Rig/lever-rig.json`：可读骨骼与贴图绑定数据；`lever-runtime.js` 为可运行的刚性骨骼播放器。
- `Previews/index.html` 直接运行骨骼，并提供角度拖动、左右拨动、慢放、暂停和骨骼显示。
- `Previews/lever_rig.webp` 只是骨骼播放的演示导出，不是逐帧换图的动画源。

## Mirror

- `Textures/mirror_right.png`：512×512 透明 PNG，标准右侧视图，镜片竖直，上下对齐。
- `Source/mirror_right_generated.png`：高清透明原图。
- 金色镜框、黄铜竖直轴、灰色低底座；侧面只露出窄镜面与镜框厚度。
- 按已确认的结构，镜片从俯视角看绕竖直轴左右转向，支架固定。这次交付该动作的静态基准图；未把 v06 的左右倾斜动画作为新结构的动画交付。

## Unity 中使用 Lever

提供 `Unity/LightHungryLeverRig.cs` 作为简单接入参考。它控制一个 Transform 骨骼的旋转，不依赖第三方骨骼插件；尚未在用户 Unity 工程中运行验证。

1. 将两张 Lever PNG 设为 Sprite (2D and UI)，Single，Pixels Per Unit = 256，Mesh Type = Full Rect，关闭有损压缩。建议 Bilinear。
2. 底座 Sprite 的 Custom Pivot = (0.5, 0.0625)；活动杆的 Custom Pivot = (0.5, 0.2734375)。Unity Pivot 从左下角计。
3. 新建 LeverRoot，保持均匀缩放。在其子对象 Base 上添加 SpriteRenderer，使用底座，Local Position = (0,0,0)，Sorting Order = 1。
4. 新建子骨骼 Handle，Local Position = (0,0.421875,0)，Scale = (1,1,1)。为其添加 SpriteRenderer，使用活动杆，Sorting Order = 0。
5. 给 LeverRoot 添加 LightHungryLeverRig，拖入 Handle 引用。交互时调用 Toggle() 或 SetRight(bool)。默认左档。

如需要打包为 Spine / DragonBones / Unity 2D Animation 的原生工程，应在选定工具后另行转换；当前交付通用骨骼数据、透明贴图、实时预览及 Unity Transform 骨骼参考实现。

## 制作与检查

创意贴图使用内置 imagegen 生成。`prompts.json` 保存两次提示词与源文件记录。之后只做统一缩放、裁切、透明画布对齐和骨骼绑定，未对动画中的杆子做伸缩。

`QA/texture-checks.json` 记录完整 Alpha 直方图、边缘与尺寸检查；`QA/preview-checks.json` 记录动画、骨长、转轴、暂停、反向及浏览器检查。视觉检查图在 QA 目录。

Tools 脚本依赖 Node.js、sharp、playwright 与已安装的 Chrome。顺序：prepare-assets.cjs → export-preview.cjs → check-preview.cjs。修改后应重新生成哈希和 ZIP。

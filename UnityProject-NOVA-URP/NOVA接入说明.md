# NOVA 特效对比副本

本目录为独立 Unity 工程；原 `UnityProject` 和原 Windows-Level03 试玩未被替换。

- Unity 2021.3.42f1c1，URP 12.1.15。
- NOVA Shader 2.0.0，官方提交 `36f6d890b06f7c2ae01c7f15f0ed2a2b42c65572`。
- 来源：https://github.com/CyberAgentGameEntertainment/NovaShader/tree/2.0.0
- 源码在 `Packages/jp.co.cyberagent.nova`，保持官方代码，附 MIT 许可。
- 官方 `pfb_eff_fire.prefab` 与所需贴图、材质在 `Assets/NovaSamples`。来源和 SHA-256 清单位于工程外 `Validation/nova-urp-v01/source-manifest.json`。

实际使用的预制体为 `Assets/Resources/NovaHeadFlame.prefab`，基于官方火焰示例生成。保留两个火焰粒子层和火星，去掉示例灯光、光晕等环境效果。实际材质为 `Nova/Particles/UberUnlit`，使用官方 25 帧 3D 序列贴图与原有自定义顶点数据；调整为金白加色混合，缩短粒子寿命并缩小至人物后脑附近。并非之前自写的 SpiritFlame Shader；旧文件只是作为副本中未使用的历史资源保留。

`WickFlameTrail.cs` 控制后脑挂点、左右朝向、世界空间拖尾，以及化光时清空、恢复时重新发射。它不替代 NOVA 的着色代码。整体尺寸可在脚本中的 `.16f` 调整；颜色、透明度在 `Assets/NovaSettings/Spirit_ps_fire_*.mat` 调整。

人物水彩着色继续使用既有材质。URP 下将主体作为第一遍渲染，通过 `SpiritOutlineFeature` 单独执行三层轮廓光。水面、绳子和交互描边保留既有自定义着色器；这不是对所有旧 Shader 的全面重写。快照使用 URP 的 RenderSingleCamera，实录来自原生 D3D11 玩家。

## 试玩与核验

双击工程外 `试玩NOVA特效副本.cmd` 进入第三关。`打开NOVA副本工程.cmd` 打开可编辑工程。原来的 `试玩第三关.cmd` 仍然打开旧版，可对比查看。

证据：`Validation/nova-urp-v01/` 中提供头后火焰 GIF/MP4、水面 GIF、三个关卡的运行截图和日志。第三关 40 项、第二关 21 项自动检查通过；第一关到第二关真实交互切换通过。近景实录使用 QA 放大镜头，正式试玩比例保持不变。

构建成功，0 错误；有 1 条 Burst 构建警告：找不到 UnityEngine.TestRunner.dll 测试程序集。原生试玩检查未出现运行错误。这些结果不代替最终美术选型。

## 重建

副本中的 `NovaPrototypeBuilder.Build()` 会重新生成 URP 设置、NOVA 材质及头后火焰预制体，并构建到工程外 `Builds/Windows-NOVA-URP`。如已手动调过这些资源，请先保存为自己的版本，再运行该构建方法，避免参数被初始配置覆盖。通常修改现有工程后可直接通过 Unity 的 Build Settings 构建。

## 发梢衔接更新（2026-11-03）

发束后上方现在使用局部坐标遮罩，使末端逐渐透明；主体仍写入深度，透明发梢单独进行 alpha 混合。发梢不再绘制原先的强轮廓光，脸部和身体不受该遮罩影响。

为稳定控制方向，主火焰改用沿发束方向摆放的动画片，继续使用 NOVA UberUnlit 和官方火焰 3D 序列纹理（不是自写火焰 Shader）。原官方火焰预制体中的主火焰渲染器停用，保留火星粒子；这属于官方资源的角色化适配，不是原示例预制体原样播放。左右朝向对应正负 62 度，动画片与透明发梢重叠，化光时一同隐藏。运行实录与检查在 `Validation/nova-hair-v02`。

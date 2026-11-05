# 前三关淡水彩后景

以已确认的第四关背景为参照，降低后景的颜色浓度、细节对比与边缘锐度。第一关保留暖色庭园，第二关保留陶盆内的暖暗环境，第三关保留暗渠蓝灰纵深。第二关额外消除了生成时出现的宽亮带，功能光线仍由游戏单独绘制。

使用内置 **imagegen** 编辑原后景图；原始纹理保留。最终图已保存至当前目录，并导入现有 `UnityProject-NOVA-URP` 工程。

|关卡|最终背景图|游戏中的新图混合强度|
|---|---|---|
|01 门槛苔地|[level01-background.png](C:/Users/10029/Documents/ChatGPT/食光兽/art/backgrounds-pale-watercolor-v01-2026-11-04/level01-background.png)|85%|
|02 倒扣陶盆|[level02-background.png](C:/Users/10029/Documents/ChatGPT/食光兽/art/backgrounds-pale-watercolor-v01-2026-11-04/level02-background.png)|70%|
|03 排水暗渠|[level03-background.png](C:/Users/10029/Documents/ChatGPT/食光兽/art/backgrounds-pale-watercolor-v01-2026-11-04/level03-background.png)|72%|

原图参与混合，避免过度减淡；后景材质另有 0.65 源像素的轻微柔化。场景中的地面、跳台、陶盆接口碰撞、绳索、镜面、露珠、乱石、水动画及人物沿用原有资源与参数。

## 原生画面对比

以下截图来自同一原生游戏构建、同一相机位置与冻结的时间/姿态。Before 只将背景材质的 `Watercolor strength` 设为 0；After 使用上表强度。

|位置|调整前|调整后|
|---|---|---|
|第一关镜面与兽尾|[之前](C:/Users/10029/Documents/ChatGPT/食光兽/Validation/background-watercolor-v01/native-review/01-play-mirror-before.png)|[之后](C:/Users/10029/Documents/ChatGPT/食光兽/Validation/background-watercolor-v01/native-review/01-play-mirror-after.png)|
|第二关露珠与乱石|[之前](C:/Users/10029/Documents/ChatGPT/食光兽/Validation/background-watercolor-v01/native-review/02-play-dew-rubble-before.png)|[之后](C:/Users/10029/Documents/ChatGPT/食光兽/Validation/background-watercolor-v01/native-review/02-play-dew-rubble-after.png)|
|第三关滑轮区|[之前](C:/Users/10029/Documents/ChatGPT/食光兽/Validation/background-watercolor-v01/native-review/03-play-A-pulleys-before.png)|[之后](C:/Users/10029/Documents/ChatGPT/食光兽/Validation/background-watercolor-v01/native-review/03-play-A-pulleys-after.png)|
|第三关折返跳台|[之前](C:/Users/10029/Documents/ChatGPT/食光兽/Validation/background-watercolor-v01/native-review/03-play-B-platforms-before.png)|[之后](C:/Users/10029/Documents/ChatGPT/食光兽/Validation/background-watercolor-v01/native-review/03-play-B-platforms-after.png)|

## 工程内调整

工程目录：`C:/Users/10029/Documents/ChatGPT/食光兽/UnityProject-NOVA-URP`。

三张独立材质位于该工程的 `Assets/Materials/BackgroundWatercolor/`。选择相应材质后可在 Inspector 中调整 `Watercolor strength`（减淡风格强度）和 `Background softness`（轻微柔化）。强度 0 为原后景，1 为完整新图。专用 shader 只赋给 GardenWall、PotInterior_Background、Drain_Background 三个后景对象。

从 Unity 菜单“食光兽 → 前三关后景”可重新应用或构建；应用工具不重建整关。批处理入口为 [Build-WatercolorBackgrounds.ps1](C:/Users/10029/Documents/ChatGPT/食光兽/Tools/Build-WatercolorBackgrounds.ps1)，传入 `-Review` 会自动输出八组同机位对比。

## 编辑记录

- [完整初次编辑提示词](C:/Users/10029/Documents/ChatGPT/食光兽/art/backgrounds-pale-watercolor-v01-2026-11-04/generation-prompts.json)
- [第二关亮带修正提示词](C:/Users/10029/Documents/ChatGPT/食光兽/art/backgrounds-pale-watercolor-v01-2026-11-04/level02-lighting-correction-prompt.txt)
- [背景赋值与前景/碰撞保持审计](C:/Users/10029/Documents/ChatGPT/食光兽/Validation/background-watercolor-v01/assignment-audit.json)
- [原生画面检查报告](C:/Users/10029/Documents/ChatGPT/食光兽/Validation/background-watercolor-v01/native-review/review.json)

此次验证针对背景样式、前景保持和原生渲染；没有重新执行三关完整通关。第四关的样式和玩法未修改。

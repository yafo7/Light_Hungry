# 第五关 Unity 独立素材

使用内置 imagegen（无 CLI/API 回退）以已确认泵房效果图作为编辑和风格参考。生成提示词分别保留在 `background-prompt.txt`、`atlas-prompt.txt`。

`background.png`：1536×1024 泵房底图，已移除人物、镜面、光线、平台和手轮。只作为后景及水面采样，不提供碰撞。

`props-atlas.png`：1536×1024 RGBA 图集。水平侧视检修踏板、玻璃栈道、检修腔、镜头、手轮和根帘。已确认具有实际 alpha 通道。Unity 使用 UV 裁切，不额外改动生成图片像素。所有跳台和栈道上边缘平行画面横轴，没有可见的透视顶部。

素材复制至 `UnityProject-NOVA-URP/Assets/Art/Level05`；美术采样区域和配准见 `FifthLevelBuilder.cs`。入口/右岸岩石仍来自底图，在实际行走表面配独立水平碰撞；底图中的泵机底座和管箍没有碰撞。

原始输出保留：
- 后景：`C:/Users/10029/.codex/generated_images/01a178c4-f6ba-7bb0-9c25-d45225bbf2bd/exec-65fa99c1-1a57-473c-8bcf-3ac67268cc3c.png`
- 图集：`C:/Users/10029/.codex/generated_images/01a178c4-f6ba-7bb0-9c25-d45225bbf2bd/exec-11969c2a-991c-4b31-9463-6c9aa06e2480.png`

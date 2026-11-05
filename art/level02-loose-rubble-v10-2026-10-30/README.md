# 第二关 v10：低矮乱石堵口

替换 v09 被否定的巨石与大石板。当前素材由小石块、碎砖和少量陶片组成，保持右下洞口的低矮轮廓；不再额外叠加上方石塔。

- `30-rubble.png`：1774 × 887，真正透明的前景图，生成结果字节保持原样。
- `generation-prompt.txt`：内置 image_gen 编辑的最终提示词；没有使用 CLI 回退。
- `alpha-check.json`：实际 alpha 与窄缝采样；检测脚本 `Tools/inspect-loose-rubble.cjs`。
- Unity：`UnityProject/Assets/Scenes/SecondLight.unity`。
- PNG 在场景中的像素变换：缩放 0.61，偏移 `(546, 252.1)`；角色高 63 px。
- 窄缝中心原图像素线：`y = 0.519x + 60`；场景像素线：`y = 0.519x + 5.326`。
- 巨石 v09 保留历史源文件，但不再作为当前场景对象。

背景、地面和叶片仍来自 v08。当前效果以 `Validation/level02/native-run/` 的 Unity 原生截图为准，v08 分层网页展示的是旧素材组合。

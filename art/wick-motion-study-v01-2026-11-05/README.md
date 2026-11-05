# 灯芯精灵 · 九组动作资源绑定验证

2026-11-05。使用用户指定的 WickSpirit_Rigged.fbx，保留原模型、22 根骨骼、蒙皮与贴图。结果位于本独立目录，未修改桌面源模型或两个 Unity 工程。

打开 `Previews/index.html` 可离线切换九组动作，暂停、慢放、拖动时间轴、旋转视角及显示骨骼。用 Chrome/Edge 打开该 HTML 即可，无需联网。

## 资源清单

每行是一个独立 CMU 动捕 take，并非同一动作调速后算成三个。

| 文件 | 来源 ID | 原资源描述 | 约时长 | 适配观察 |
|---|---|---|---|---|
| Jump_01.fbx | 143_05 | Jumping Distances | 2.8 秒 | 跳远姿势；本交付为水平原地版，位移应由游戏驱动 |
| Jump_02.fbx | 143_06 | Jumping Heights | 3.3 秒 | 原地跳高，优先用于测试普通跳跃 |
| Jump_03.fbx | 143_07 | Jumping Sideways | 4.7 秒 | 侧跳，适合测试侧向躲避 |
| Push_01.fbx | 81_05 | push heavy object | 11.4 秒 | 推重物，优先候选；手位需结合箱体尺寸调整 |
| Push_02.fbx | 81_06 | push heavy box | 10.6 秒 | 推重箱，包含较明显的重心与蹬腿变化 |
| Push_03.fbx | 82_06 | Pushing on heavy object | 9.1 秒 | 用力较大、低姿态明显，适合比较，不优先作日常推箱循环 |
| Idle_01.fbx | 140_06 | Idle | 29.7 秒 | 长段自然站立变化，可按需要挑选短片段 |
| Idle_02.fbx | 140_07 | Idle | 10.3 秒 | 较短待机，优先候选 |
| Idle_03.fbx | 139_02 | Shifting Weight | 7.8 秒 | 站立换重心，适合增加待机变化 |

## 已交付

- `Exports/*.fbx`：九个独立文件，每个含原模型、贴图、蒙皮与一个已烘焙动作。
- `Exports/WickSpirit_9Actions.blend`：包含全部九个可编辑 Action。
- `Exports/WickSpirit_9Actions.glb`：包含九组动画的可移植预览模型。
- `Source`：用户源 FBX 副本、九个源 BVH / ZIP、仓库目录记录、转换作者的 READMEFIRST。
- `QA`：FBX 回读、浏览器检查、每组正面/侧面六姿态图及完整对照图。
- `Tools`：本轮重定向、导出与验证脚本。使用 Blender 5.2.3 LTS；浏览器使用 three.js。

## 适配方法与验证边界

源文件为人形动作，目标是大头、短肢的定制角色。按源文件第 1 帧 T Pose 校准并移除该帧，将 120 fps 动捕按每四帧取样烘焙为 30 fps；重定向到原 22 根骨骼，不改变模型网格、骨长或蒙皮权重。

目标整体朝向归一化，去除水平移动以便游戏驱动。跳跃保留按腿部比例缩放的源脚底离地高度。逐帧按实际变形网格修正最低点，避免短腿导致的地面穿透。

第一轮可视检查发现推重物时人类幅度的大前倾会让巨大火焰头过低。交付版减弱头颈和躯干俯仰，并对推动手臂作 30° 前抬修正；保留三个源 take 的节奏、肢体变化和差异。这些是重定向后的适配版本，而非原始人体动作的完全逐角复制。

已进行：九个 FBX 各自重新导入，检查骨骼数量、顶点数、贴图、动作、未加权顶点和九个采样时刻的变形网格；浏览器逐个播放、检查九组独立动画和正面/侧面姿态。具体数值见 JSON 检查记录。

地面检查保留实际残差：FBX 回读后的插值时刻，Push_01 最大采样穿地约 0.000207 模型单位（按 1 单位 = 1 米为 0.207 毫米）。未达到 0.1 毫米的严格阈值，在本预览采用的 1 毫米容差内；不声称全时刻零穿地。

尚未进行：用户 Unity 场景里的运行、游戏碰撞与跳跃曲线配合、箱体尺寸对应的手掌 IK 锁定、每个接触帧的自相交检测。原蒙皮在大幅抬臂时的腋下拉伸风险仍存在。原地处理不能替代脚底 IK，可能出现滑步。

这些是供选择的已绑定动作候选，保留源 take 的动作段落，尚未把每个 idle/push 修剪成首尾无缝循环。预览循环到开头时可能有接缝；正式使用时应选取动作段并在 Animator 中过渡。推荐先比较 Jump_02、Push_01、Idle_02。

## 来源和使用说明

原动捕：Carnegie Mellon University Graphics Lab Motion Capture Database，https://mocap.cs.cmu.edu/ ，NSF EIA-0196217。

BVH 转换：Bruce Hahne / cgspeed。下载镜像：https://github.com/Shriinivas/cmubvh 。九个动作的编号和描述对照：https://sites.google.com/a/cgspeed.com/cgspeed/motion-capture/the-motionbuilder-friendly-bvh-conversion-release-of-cmus-motion-capture-database/bvh-conversion-release-motions-list 。逐文件 URL、仓库 tree SHA 和校验值见 sources.json。

转换作者说明数据可用于研究和商业项目，转换版不增加使用限制：https://sites.google.com/a/cgspeed.com/cgspeed/motion-capture/the-daz-friendly-bvh-release-of-cmus-motion-capture-database/readmefirst-file-for-daz-friendly-primary-release 。不将其擅自标成 CC0；保留作者及原始使用说明。

另检索了 Quaternius Universal Animation Library（https://quaternius.com/packs/universalanimationlibrary.html ，官方标示 CC0），但本轮实际绑定的九个动作全部来自上述 CMU 数据，未把检索到的页面等同于已下载或已验证。

three.js 的 MIT 许可随 `Previews/THREE-LICENSE.txt` 提供。

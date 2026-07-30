# 失物招领（Lost And Found）v0.1

Unity 版本：`2022.3.62f3c1`  
渲染：Built-in Standard（无 URP/HDRP）  
输入：旧版 Input Manager  

## 一键重建场景

菜单：`Tools / Lost And Found / Build Scene`

会生成：房间、灯光、木桌、书、钱包、墙字、玩家、UI、材质贴图、Build Settings（仅 `LostAndFound`）。

过程截图：`Tools / Lost And Found / Capture Process Screenshots`  
输出目录：桌面 `失物招领_过程截图`

## 游玩步骤

1. 打开场景 `Assets/Scenes/LostAndFound.unity`，点 Play  
2. WASD 走动，鼠标环视；Esc 解锁鼠标，左键再锁定  
3. 走近并注视桌上蓝书，出现 `Is this your book?`，按 **E**（Yes 变绿，书消失）  
4. 可注视钱包，出现 `Is this your Wallet?`，按 **E**（No 变绿，钱包仍在）  
5. **拿书后**转身走后门洞出口触发器 → 控制锁定 + `GAME OVER!`  
6. 未拿书走出口不会结束

## 已知差异

- 若 `Assets/Resources/音乐3` 音频文件不存在，BGM 不播放（挂载点与 Bootstrap 已就绪，Console 有提示）
- 贴图为程序化占位噪声，可后续替换 AI 贴图

## 旧 Unity6 工程备份

原 EZPZ/URP 工程已移出，备份目录名形如：

`E:\UNITY\DDES9903_Unity6_Backup_YYYYMMDD_HHMMSS`

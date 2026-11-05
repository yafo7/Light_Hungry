@echo off
chcp 65001 >nul
start "食光兽 Unity 第七关" "D:\workshop\developer_learn\Unity_Hub\2022.3.62f3c1\Editor\Unity.exe" -projectPath "%~dp0UnityProject-NOVA-URP" -executeMethod SeventhLevelBuilder.Open

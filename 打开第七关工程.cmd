@echo off
chcp 65001 >nul
start "食光兽 Unity 第七关" "C:\Program Files\Unity\Hub\Editor\2021.3.42f1c1\Editor\Unity.exe" -projectPath "%~dp0UnityProject-NOVA-URP" -executeMethod SeventhLevelBuilder.Open

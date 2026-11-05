@echo off
chcp 65001 >nul
cd /d "%~dp0"
start "食光兽 Unity 第六关" "C:\Program Files\Unity\Hub\Editor\2021.3.42f1c1\Editor\Unity.exe" -projectPath "%~dp0UnityProject-NOVA-URP" -executeMethod SixthLevelBuilder.Open

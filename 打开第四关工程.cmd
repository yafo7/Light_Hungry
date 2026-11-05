@echo off
chcp 65001 >nul
start "" "C:\Program Files\Unity\Hub\Editor\2021.3.42f1c1\Editor\Unity.exe" -projectPath "%~dp0UnityProject-NOVA-URP" -executeMethod FourthLevelBuilder.Open

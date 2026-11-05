@echo off
chcp 65001 >nul
cd /d "%~dp0"
start "食光兽 - 第七关叶幕走廊" "Builds\Windows-Level07-NOVA\LightFeast.exe" --level07 -screen-fullscreen 0 -screen-width 1600 -screen-height 900

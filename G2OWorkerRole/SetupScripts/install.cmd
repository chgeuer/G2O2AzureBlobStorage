﻿@echo off

cd /d "%~dp0"
%SystemRoot%\System32\WindowsPowerShell\v1.0\powershell.exe -ExecutionPolicy Unrestricted -File "%~dp0install.ps1"
exit /b 0


cd /d "%~dp0"

%SystemRoot%\System32\WindowsPowerShell\v1.0\powershell.exe -ExecutionPolicy Unrestricted -File "%~dp0install2.ps1" > %logfilename% 2>>&1

exit /b 0

@echo off
REM This batch file runs the DirectoryCollection.ps1 PowerShell script.
REM The script is expected to be in the same directory as this .bat file.
REM The -ExecutionPolicy Bypass flag allows the script to run.

powershell.exe -ExecutionPolicy Bypass -File "%~dp0\DirectoryCollection.ps1"

REM The PAUSE command keeps the window open after the script finishes
REM so you can read the output.
echo.
pause
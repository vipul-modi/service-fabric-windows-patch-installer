@echo off

PowerShell -NonInteractive -noProfile -Command "& %~dp0verify.ps1"
exit /b %ERRORLEVEL%

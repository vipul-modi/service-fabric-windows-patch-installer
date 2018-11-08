@echo off
:: Install the patch
:: Typically a KB package is installed by calling the executable KBXXXXX.exe /q /norestart

:: The example patch simply writes a file to the root of the machine to indicate that the patch is installed
:: This is what the verify command checks
echo "This is an example patch." >  %SystemDrive%\_example_patch_installed_%Fabric_NodeName%.txt

:: once the patch is installed, it is recommended to reboot the machine
REM call shutdown /r /t 00

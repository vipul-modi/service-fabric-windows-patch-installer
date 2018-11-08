@echo off
:: verify that the patch is present, return 0 if it is present, otherwise -1

if EXIST "%SystemDrive%\_example_patch_installed_%Fabric_NodeName%.txt" (
    echo "The patch is installed".
    exit /b 0
) else (
    echo "The patch is not installed".
    exit /b 1
)
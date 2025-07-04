@echo off
echo Installing Playwright browsers...
echo.

REM Check if PowerShell is available
powershell -Command "Write-Host 'PowerShell is available'" >nul 2>&1
if %errorlevel% neq 0 (
    echo PowerShell is required but not found. Please install PowerShell.
    pause
    exit /b 1
)

REM Install Playwright browsers using the playwright.ps1 script
powershell -ExecutionPolicy Bypass -File "./playwright.ps1" install chromium

if %errorlevel% equ 0 (
    echo.
    echo Playwright browsers installed successfully!
    echo You can now run your tests.
) else (
    echo.
    echo Error installing Playwright browsers.
    echo Please check the error messages above.
)

echo.
pause
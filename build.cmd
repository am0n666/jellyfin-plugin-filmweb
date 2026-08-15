@echo off
setlocal enabledelayedexpansion

echo ====================================================
echo  Jellyfin Filmweb Plugin - Windows Build Script
echo ====================================================

where dotnet >nul 2>&1
if %errorlevel% neq 0 (
    echo [ERROR] .NET SDK (dotnet) nie zostal znaleziony w sciezce PATH!
    echo Pobierz i zainstaluj .NET 8.0 SDK: https://dotnet.microsoft.com/download/dotnet/8.0
    pause
    exit /b 1
)

set CONFIGURATION=Release
set OUTPUT_DIR=%~dp0dist\Filmweb

if exist "%OUTPUT_DIR%" (
    echo Czyszczenie katalogu wyjsciowego %OUTPUT_DIR%...
    rmdir /s /q "%OUTPUT_DIR%"
)

echo Kompilacja projektu w trybie %CONFIGURATION%...
dotnet publish "%~dp0src\Jellyfin.Plugin.Filmweb\Jellyfin.Plugin.Filmweb.csproj" -c %CONFIGURATION% -o "%OUTPUT_DIR%" --nologo

if %errorlevel% neq 0 (
    echo [ERROR] Kompilacja zakonczyla sie bledem!
    pause
    exit /b %errorlevel%
)

echo.
echo ====================================================
echo [OK] Kompilacja zakonczona pomyslnie!
echo Pliki wtyczki znajduja sie w:
echo   %OUTPUT_DIR%
echo.
echo Aby zainstalowac wtyczke:
echo 1. Skopiuj caly folder "%OUTPUT_DIR%" do katalogu plugins serwera Jellyfin:
echo    np. %%APPDATA%%\Jellyfin\Server\plugins\Filmweb
echo    lub C:\ProgramData\Jellyfin\Server\plugins\Filmweb
echo 2. Zrestartuj serwer Jellyfin.
echo ====================================================

pause

@echo off & setlocal EnableDelayedExpansion

:: ===========================================================================
::  build-updater.bat - builds aihelper-updater, the small program that applies
::  a downloaded AI Helper update.
::
::  It is wired into the PreBuildEvent in AI Helper.csproj, so it runs before
::  every build of the application and a failure here is a failure of that
::  build. postbuild.bat then copies the result into RES\tools of the output,
::  which is the folder the application reads it from at run time.
::
::  Arguments, in the order the csproj passes them:
::    arg1  configurationName  Debug or Release
::    arg2  msbuildToolsDir    folder MSBuild.exe is in; the PreBuildEvent
::                             passes $(MSBuildToolsPath), and vswhere is asked
::                             when that is empty or does not hold MSBuild.exe
::
::  The project builds straight into <repo>\tools\aihelper-updater, so there is
::  no copy step to forget: what a build produces is what is deployed.
::
::  The tool is .NET Framework, like the application it updates, so the build is
::  MSBuild's rather than a compiler invoked by hand. It has no package
::  references at all, so nothing has to be restored and the build works with
::  no network.
:: ===========================================================================

set configurationName=%~1
if "%configurationName%"=="" set configurationName=Debug

set projectPath=%~dp0AIHelperUpdater.csproj
if not exist "%projectPath%" goto :noProject

:: ---- 1. Find MSBuild -------------------------------------------------------
:: Labels are used rather than parenthesised blocks on purpose: the fallback
:: below has to spell out "%ProgramFiles(x86)%", and a closing bracket inside a
:: block closes the block instead of naming the variable.

set msbuildExe=
if "%~2"=="" goto :findMsbuild

set msbuildToolsDir=%~2
if "!msbuildToolsDir:~-1!"=="\" set msbuildToolsDir=!msbuildToolsDir:~0,-1!
if exist "!msbuildToolsDir!\MSBuild.exe" set msbuildExe=!msbuildToolsDir!\MSBuild.exe
if not "%msbuildExe%"=="" goto :msbuildReady

:findMsbuild
set vsWhere=%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe
if not exist "%vsWhere%" set vsWhere=C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe
if not exist "%vsWhere%" goto :noToolchain

for /f "usebackq tokens=*" %%I in (`"%vsWhere%" -latest -products * -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe`) do set msbuildExe=%%I

:msbuildReady
if "%msbuildExe%"=="" goto :noToolchain

:: ---- 2. Build --------------------------------------------------------------
:: The build runs in its own process. Node reuse is off because this is started
:: from a build: a child that joined the running build's nodes would wait for
:: nodes the build that started it is holding.
::
:: What this build is supposed to produce is removed first. The check after it
:: then tells this build's output from a file an earlier build left behind - a
:: check that only asked whether the file was there would pass on a stale one,
:: which is exactly what an output path pointing somewhere else leaves behind.

set updaterExePath=%~dp0..\tools\aihelper-updater\aihelper-updater.exe
if exist "%updaterExePath%" del "%updaterExePath%"

echo building aihelper-updater (%configurationName%)..
echo   msbuild: "%msbuildExe%"

"%msbuildExe%" "%projectPath%" /t:Build /p:Configuration=%configurationName% /p:Platform=AnyCPU /nodeReuse:false /nologo /v:m
if errorlevel 1 goto :buildFailed

:: ---- 3. Check that it landed where the application looks for it -------------
:: The project writes straight into the tools folder and nothing copies it there
:: afterwards, so a build that reported success but wrote somewhere else would
:: leave the application with no tool to hand over to - and silently, because
:: postbuild.bat treats a missing tool as a build that never made one. A platform
:: the project has no configuration for is the way that happens: the build then
:: falls back to the targets' default output path, bin\<platform>\<configuration>.
:: The folder is therefore checked here, where a wrong answer still fails a build.

if not exist "%updaterExePath%" goto :notDeployed

echo the updater is at %updaterExePath%
exit /b 0

:: ---- 4. What to do when it cannot be built ---------------------------------

:notDeployed
echo BUILD ERROR: aihelper-updater was built but is not where it is deployed from.
echo              expected: %updaterExePath%
echo              Check that AIHelperUpdater.csproj still writes to ..\tools\aihelper-updater\.
exit /b 1

:noProject
echo BUILD ERROR: aihelper-updater.csproj was not found beside this script.
exit /b 1

:noToolchain
echo BUILD ERROR: MSBuild was not found. It ships with Visual Studio; install the
echo              "Desktop development with .NET" workload to build aihelper-updater.
exit /b 1

:buildFailed
echo BUILD ERROR: aihelper-updater could not be compiled.
exit /b 1

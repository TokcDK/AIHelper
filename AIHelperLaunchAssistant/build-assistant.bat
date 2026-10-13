@echo off & setlocal EnableDelayedExpansion

:: ===========================================================================
::  build-assistant.bat - builds the native launch assistant.
::
::  It is wired into the PreBuildEvent in AI Helper.csproj, so it runs before
::  every build of the application and a failure here is a failure of that
::  build. postbuild.bat then copies the result into RES\tools of the output,
::  which is the folder the application reads it from at run time.
::
::  Arguments, in the order the csproj passes them:
::    arg1  configurationName  Debug or Release
::    arg2  outputDir          folder the tool is written to; the repository's
::                             tools folder by default
::
::  The tool is native C++ rather than .NET on purpose. It stays in memory for
::  the whole of a game session, and a native process with no window, no message
::  loop and no runtime is what makes that cost nothing - see light-assistant.md
::  beside the sources.
::
::  The compiler is found through vswhere rather than taken from the path: this
::  runs from a normal build, not from a Developer Command Prompt.
:: ===========================================================================

set configurationName=%~1
if "%configurationName%"=="" set configurationName=Release

set sourceDir=%~dp0

set outputDir=%~2
if "%outputDir%"=="" set outputDir=%sourceDir%..\tools\aihelper-launch-assistant

:: Intermediates stay beside the sources and not beside the tool: the tool's
:: folder is mirrored into the application's output, and object files have no
:: business being deployed with it.
set intermediateDir=%sourceDir%obj\%configurationName%

:: ---- 1. Find the C++ toolchain ---------------------------------------------

set vsWhere=%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe
if not exist "%vsWhere%" set vsWhere=C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe
if not exist "%vsWhere%" goto :noToolchain

set vsInstall=
for /f "usebackq tokens=*" %%I in (`"%vsWhere%" -latest -products * -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath`) do set vsInstall=%%I
if "%vsInstall%"=="" goto :noToolchain

set vcVarsAll=%vsInstall%\VC\Auxiliary\Build\vcvarsall.bat
if not exist "%vcVarsAll%" goto :noToolchain

call "%vcVarsAll%" x64 >nul 2>&1
if errorlevel 1 goto :noToolchain

:: ---- 2. The Windows SDK, for when vcvarsall could not find it ---------------
:: vcvarsall locates the Windows SDK through the registry. Where that cannot be
:: read it still reports success, but leaves the include and library paths with
:: no SDK in them at all - and the compile then stops on the first <windows.h>
:: with nothing to say about why. The SDK sits in a fixed place, so it is looked
:: for there as well, newest version first, which is what vcvarsall would have
:: chosen itself. This is a fallback, and it says so when it runs.

if not "%WindowsSdkDir%"=="" goto :sdkReady

set sdkRoot=
if exist "%ProgramFiles(x86)%\Windows Kits\10\Include\" set sdkRoot=%ProgramFiles(x86)%\Windows Kits\10
if not defined sdkRoot if exist "C:\Program Files (x86)\Windows Kits\10\Include\" set sdkRoot=C:\Program Files (x86)\Windows Kits\10
if not defined sdkRoot goto :noSdk

set sdkVersion=
for /f "delims=" %%V in ('dir /b /ad /o-n "!sdkRoot!\Include" 2^>nul') do (
	if not defined sdkVersion if exist "!sdkRoot!\Include\%%V\um\" if exist "!sdkRoot!\Include\%%V\ucrt\" set sdkVersion=%%V
)
if not defined sdkVersion goto :noSdk

echo   vcvarsall found no Windows SDK; using !sdkRoot!\!sdkVersion! directly
set INCLUDE=!sdkRoot!\Include\!sdkVersion!\ucrt;!sdkRoot!\Include\!sdkVersion!\shared;!sdkRoot!\Include\!sdkVersion!\um;!INCLUDE!
set LIB=!sdkRoot!\Lib\!sdkVersion!\ucrt\x64;!sdkRoot!\Lib\!sdkVersion!\um\x64;!LIB!

:sdkReady

:: ---- 3. Compile ------------------------------------------------------------

if not exist "%outputDir%" md "%outputDir%"
if not exist "%intermediateDir%" md "%intermediateDir%"

:: The tool is optimized in both configurations. It is the process that stays
:: resident, so what a developer runs has to cost what ships - an unoptimized
:: build of it is several times the size and measures differently, which would
:: make the one thing the tool exists for unverifiable outside a release. Debug
:: keeps its symbols; Release does not, and neither writes anything into the
:: tool's own folder: the program database goes among the intermediates, which
:: are never deployed.
set compilerFlags=/nologo /W4 /MT /O1 /Os /Gy /DUNICODE /D_UNICODE
set linkerFlags=/SUBSYSTEM:WINDOWS /ENTRY:wWinMainCRTStartup /INCREMENTAL:NO /OPT:REF /OPT:ICF
if /i "%configurationName%"=="Release" (
	set compilerFlags=!compilerFlags! /DNDEBUG
	set linkerFlags=!linkerFlags! /RELEASE
) else (
	set compilerFlags=!compilerFlags! /Zi
	set linkerFlags=!linkerFlags! /DEBUG /PDB:"%intermediateDir%\aihelper-launch-assistant.pdb"
)

echo building the launch assistant (%configurationName%)..

:: Compiled from the intermediates folder, so the object files and the program
:: database land there without a single path having to be spelled out.
pushd "%intermediateDir%"
cl !compilerFlags! /Fe:"%outputDir%\aihelper-launch-assistant.exe" "%sourceDir%assistant.cpp" "%sourceDir%ArgumentQuoting.cpp" "%sourceDir%CommandLine.cpp" "%sourceDir%ProcessLauncher.cpp" "%sourceDir%ToolLog.cpp" /link !linkerFlags! user32.lib shell32.lib
set buildResult=%errorlevel%
popd

if not "%buildResult%"=="0" goto :buildFailed

:: The object files are this machine's leftovers; what is deployed is the exe.
del /q "%intermediateDir%\*.obj" >nul 2>&1

echo the launch assistant is at %outputDir%\aihelper-launch-assistant.exe
exit /b 0

:: ---- 4. What to do when it cannot be built ---------------------------------

:noToolchain
echo BUILD ERROR: the C++ toolchain was not found. Install the "Desktop development with C++"
echo              workload - Microsoft.VisualStudio.Component.VC.Tools.x86.x64 - to build the
echo              launch assistant.
exit /b 1

:noSdk
echo BUILD ERROR: the Windows SDK was not found. vcvarsall did not provide it, and it is not in
echo              the usual place either. Install the Windows SDK to build the launch assistant.
exit /b 1

:buildFailed
echo BUILD ERROR: the launch assistant could not be compiled.
exit /b 1

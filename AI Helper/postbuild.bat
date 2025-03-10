@echo off & setlocal EnableDelayedExpansion

:: Constants
set "SEVEN_ZIP_PATH=C:\Program Files\7-Zip\7z.exe"
set "SUPPORTED_LANGS=zh-Hans zh-Hant cs de es fr it ja ko pl pt-BR ru tr"
set "MO_BASIC_GAMES_SUBPATH=MO\plugins\basic_games"
set "ROBOCOPY_RETRIES=5"
set "ROBOCOPY_WAIT=2"

:: Check admin privileges at start
call :checkAdmin || goto :exit

:: Input Parameters (with basic sanitization)
set "targetDir=%~1"
set "targetName=%~2"
set "projectDir=%~3"
set "versionNumber=%~4"
set "configurationName=%~5"

:: Validate parameters
for %%P in (targetDir targetName projectDir versionNumber configurationName) do (
    if "!%%P!"=="" (
        echo [ERROR] Parameter %%P is empty
        goto :exit
    )
)

:: Base paths
set "projectBuildDir=!projectDir!BUILD"
set "projectResDir=!projectBuildDir!\RES"
set "targetResDir=!targetDir!RES"
set "targetBasicGamesPluginsDir=!targetDir!\!MO_BASIC_GAMES_SUBPATH!\games"
set "targetResLocaleDir=!targetResDir!\locale"
set "targetReportDir=!targetResDir!\theme\default\report"
set "targetLibDir=!targetResDir!\lib"

:: Verify directories
if not exist "!projectDir!\" (
    echo [ERROR] Project directory "!projectDir!" not found
    goto :exit
)
if not exist "!targetDir!\" (
    echo [ERROR] Target directory "!targetDir!" not found
    goto :exit
)

:: Check if key directories are root directories
for %%D in ("!targetDir!" "!projectDir!" "!targetResDir!" "!targetLibDir!") do (
    if "%%~D:~3"=="" if "%%~D:~-1"=="\" (
        echo [ERROR] Directory "%%~D" is a root directory, which is not allowed
        goto :exit
    )
)

:: Create backup folder
for /f "tokens=2 delims==" %%I in ('wmic os get localdatetime /value') do set "DATETIME=%%I"
set "DATETIME=!DATETIME:~0,8!-!DATETIME:~8,2!!DATETIME:~10,2!!DATETIME:~12,2!"
set "backupFolder=!targetDir!\BAKS\!DATETIME!bak"
if not exist "!targetDir!\BAKS\" mkdir "!targetDir!\BAKS\"
mkdir "!backupFolder!" || (
    echo [ERROR] Failed to create backup folder "!backupFolder!"
    goto :exit
)

echo Moving libraries...
if not exist "!targetResDir!\" (
    mkdir "!targetResDir!" || (
        echo [ERROR] Failed to create target resource directory "!targetResDir!"
        goto :exit
    )
)

:: Clean and move library files
if exist "!targetLibDir!\" call :moveToBackup "!targetLibDir!" || goto :exit
robocopy "!targetDir!" "!targetLibDir!" *.dll *.pdb *.xml /XF "!targetName!.dll" "!targetName!.pdb" "!targetName!.xml" /LEV:1 /MOV /R:!ROBOCOPY_RETRIES! /W:!ROBOCOPY_WAIT!
if !ERRORLEVEL! GEQ 8 (
    echo [ERROR] Library move failed with code !ERRORLEVEL!
    goto :exit
)

:: Handle language directories
set "langMoveFailed=0"
for %%D in (!SUPPORTED_LANGS!) do (
    if exist "!targetLibDir!\%%D\" call :moveToBackup "!targetLibDir!\%%D" || goto :exit
    if exist "!targetDir!\%%D\" (
        robocopy "!targetDir!\%%D" "!targetLibDir!\%%D" /E /MOV /R:!ROBOCOPY_RETRIES! /W:!ROBOCOPY_WAIT!
        if !ERRORLEVEL! GEQ 8 (
            echo [WARNING] Failed to move language directory %%D
            set "langMoveFailed=1"
        ) else (
            call :moveToBackup "!targetDir!\%%D" || (
                echo [WARNING] Failed to move language directory %%D to backup
                set "langMoveFailed=1"
            )
        )
    )
)
if !langMoveFailed! NEQ 0 (
    echo [WARNING] Some language directories failed to move - continuing build
)

:: Copy resources
call :copyResources "!projectBuildDir!" "!targetDir!" "!MO_BASIC_GAMES_SUBPATH!\games" "basic_games" "*.py" || goto :exit
call :copyResources "!projectResDir!" "!targetResDir!" "theme\default\report" "report" "ReportTemplate.html *.jpg" || goto :exit
call :copyResources "!projectResDir!" "!targetResDir!" "links" "links" "*.txt" || goto :exit
call :copyResources "!projectResDir!" "!targetResDir!" "locale" "locale" "*.po *.mo" || goto :exit

:: Release handling
set "releasesDirPath=!targetDir!RELEASES"
set "targetReleaseDir=!targetDir!RELEASE"
if /i "!configurationName!"=="Release" (
    call :createRelease || goto :exit
)

:exit
echo Postbuild script finished!
endlocal & exit /b %ERRORLEVEL%

:copyResources
:: Parameters: sourceBase targetBase subPath folderName filePattern
set "srcDir=%~1\%3"
set "tgtDir=%~2\%3"
if not exist "!srcDir!\" (
    echo [WARNING] Source directory "!srcDir!" not found
    exit /b 0
)
echo Copying %4...
if not exist "!tgtDir!\" (
    mkdir "!tgtDir!" || (
        echo [ERROR] Failed to create target directory "!tgtDir!"
        exit /b 1
    )
)
robocopy "!srcDir!" "!tgtDir!" %5 /E /COPYALL /B /R:!ROBOCOPY_RETRIES! /W:!ROBOCOPY_WAIT!
if !ERRORLEVEL! GEQ 8 (
    echo [ERROR] Copy %4 failed with code !ERRORLEVEL!
    exit /b 1
)
exit /b 0

:createRelease
if not exist "!releasesDirPath!\" (
    mkdir "!releasesDirPath!" || (
        echo [ERROR] Failed to create releases directory "!releasesDirPath!"
        exit /b 1
    )
)
if exist "!targetReleaseDir!\" call :moveToBackup "!targetReleaseDir!" || exit /b 1
mkdir "!targetReleaseDir!\RES" || (
    echo [ERROR] Failed to create release directory structure
    exit /b 1
)

:: Copy Games directory if it exists
if exist "!projectBuildDir!\Games\" (
    echo Copying Games directory with base info...
    robocopy "!projectBuildDir!\Games\" "!targetReleaseDir!\Games\" /MIR /COPYALL /B /R:!ROBOCOPY_RETRIES! /W:!ROBOCOPY_WAIT!
    if !ERRORLEVEL! GEQ 8 (
        echo [ERROR] Failed to copy Games directory with code !ERRORLEVEL!
        exit /b 1
    )
)

:: Create symbolic links
call :createSymLink "!targetReleaseDir!" "!MO_BASIC_GAMES_SUBPATH!\games" "!targetBasicGamesPluginsDir!" "D" || exit /b 1
call :createSymLink "!targetReleaseDir!\RES" "locale" "!targetResLocaleDir!" "D" || exit /b 1
call :createSymLink "!targetReleaseDir!\RES" "theme\default\report" "!targetReportDir!" "D" || exit /b 1
call :createSymLink "!targetReleaseDir!\RES" "tools\kkmanager" "!targetResDir!\tools\kkmanager" "D" || exit /b 1
call :createSymLink "!targetReleaseDir!" "!targetName!.exe" "!targetDir!\!targetName!.exe" "" || exit /b 1

:: Create symbolic link for lib dir and delete PDB and XML files
set "targetReleaseLibDir=!targetReleaseDir!\RES\lib"
if not exist "!targetReleaseLibDir!" (
    echo Making link for the app exe libs dir...
    MKLINK "!targetReleaseLibDir!" "!targetLibDir!" /D
    if !ERRORLEVEL! NEQ 0 (
        echo [ERROR] Failed to create symlink for libs dir
        exit /b 1
    )
    del "!targetReleaseLibDir!\*.pdb" 2>nul
    del "!targetReleaseLibDir!\*.xml" 2>nul
)

:: Handle release archive
set "destName=!targetName! !versionNumber!"
set "destPath=!releasesDirPath!\!destName!.7z"
if not exist "!SEVEN_ZIP_PATH!" (
    echo [ERROR] 7-Zip not found at "!SEVEN_ZIP_PATH!"
    exit /b 1
)
"!SEVEN_ZIP_PATH!" a "!destPath!" -m0=LZMA2:d=96m:fb=128 -mx=9 -mmt4 "!targetReleaseDir!\*"
if !ERRORLEVEL! NEQ 0 (
    echo [ERROR] Archive creation failed with code !ERRORLEVEL!
    exit /b 1
)
exit /b 0

:createSymLink
:: Parameters: basePath linkName targetPath linkType(D for directory, empty for file)
set "parentDir=%~1"
set "linkPath=!parentDir!\%~2"
set "targetPath=%~3"
set "linkType=%~4"
if "!linkType!"=="D" (
    if not exist "!targetPath!\" (
        echo [WARNING] Directory symlink target "!targetPath!" does not exist
        exit /b 0
    )
) else (
    if not exist "!targetPath!" (
        echo [WARNING] File symlink target "!targetPath!" does not exist
        exit /b 0
    )
)
if exist "!linkPath!" (
    call :moveToBackup "!linkPath!" || (
        echo [ERROR] Failed to move existing "!linkPath!" to backup
        exit /b 1
    )
)
if not exist "!parentDir!" (
    mkdir "!parentDir!" || (
        echo [ERROR] Failed to create parent directory "!parentDir!"
        exit /b 1
    )
)
if "!linkType!"=="D" (
    mklink /D "!linkPath!" "!targetPath!"
) else (
    mklink "!linkPath!" "!targetPath!"
)
if !ERRORLEVEL! NEQ 0 (
    echo [ERROR] Failed to create symlink "!linkPath!" - admin privileges confirmed, check path or permissions
    exit /b 1
)
exit /b 0

:moveToBackup
set "itemToMove=%~1"
if exist "!itemToMove!" (
    for %%I in ("!itemToMove!") do set "itemName=%%~nxI"
    if "!itemName!"=="" (
        echo [ERROR] Cannot determine name for "!itemToMove!" to move to backup
        exit /b 1
    )
    move "!itemToMove!" "!backupFolder!\!itemName!" || (
        echo [ERROR] Failed to move "!itemToMove!" to backup
        exit /b 1
    )
)
exit /b 0

:checkAdmin
:: Check for admin privileges
net session >nul 2>&1
if !ERRORLEVEL! NEQ 0 (
    echo [ERROR] This script requires administrative privileges for symbolic link creation
    exit /b 1
)
exit /b 0
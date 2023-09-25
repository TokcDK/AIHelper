@echo off & setlocal EnableDelayedExpansion

set targetDir=%~1
echo targetDir:%targetDir%
set targetName=%~2
set projectDir=%~3
set versionNumber=%~4
set configurationName=%~5

set projectBuildDir=%projectDir%BUILD
set projectResDir=%projectBuildDir%\RES
set targetResDir=%targetDir%RES

echo move libs..
set targetLibDir=%targetResLib%\lib
::lib files
if exist "%targetLibDir%" rd "%targetLibDir%" /s /q
ROBOCOPY "%targetDir% " "%targetLibDir% " *.dll *.pdb *.xml /XF "%targetName%.dll" "%targetName%.pdb" "%targetName%.xml" /LEV:1 /MOV
::lib lang dirs
FOR %%D IN (zh-Hans zh-Hant cs de es fr it ja ko pl pt-BR ru tr) DO (
	if exist "%targetLibDir%\%%D" rd "%targetLibDir%\%%D" /s /q
	if exist "%targetDir%%%D" move "%targetDir%%%D" "%targetLibDir%\"
)

set moBasicGamesSubPath=MO\plugins\basic_games
set modOrganizerBasicGamesPluginsDirSubPath=%moBasicGamesSubPath%\games
set projectBasicGamesPluginsDir=%projectResDir%\%modOrganizerBasicGamesPluginsDirSubPath%
set targetBasicGamesPluginsDir=%targetDir%%modOrganizerBasicGamesPluginsDirSubPath%
if exist "%projectBasicGamesPluginsDir%" (
	echo copy Mod organizer basic game plugins..
	robocopy "%projectBasicGamesPluginsDir% " "%targetBasicGamesPluginsDir%\ " *.py /MIR /COPYALL /B /R:3 /W:1
)
set themeDefaultSubPath=theme\default
set reportDirSubPath=%themeDefaultSubPath%\report
set reportTemplateFileName=ReportTemplate.html
set projectReportDir=%projectResDir%\%reportDirSubPath%
set projectReportTemplatePath=%projectReportDir%\%reportTemplateFileName%
set targetReportDir=%targetResDir%\%reportDirSubPath%
set targetReportTemplatePath=%targetReportDir%\%reportTemplateFileName%
if exist "%projectReportTemplatePath%" (
	echo copy %reportTemplateFileName% and game backgrounds..
	robocopy "%projectReportDir% " "%targetReportDir%\ " %reportTemplateFileName% *.jpg /MIR /COPYALL /B /R:3 /W:1
)
set projectResLinksDir=%projectResDir%\links
set targetResLinksDir=%targetResLib%\links
if exist "%projectResLinksDir%" (
	echo copy weblinks info txt..
	robocopy "%projectResLinksDir% " "%targetResLib%\links\ " *.txt /MIR /COPYALL /B /R:3 /W:1
)
if exist "%projectResDir%\locale" (
	echo copy localization files
	robocopy "%projectResDir%\locale " "%targetResLib%\ " *.po *.mo /MIR /COPYALL /B /R:3 /W:1
)

:: release creation
set releasesDirPath=%targetDir%RELEASES
if not exist "%releasesDirPath%" md "%releasesDirPath%"
set targetReleaseDir=%targetDir%RELEASE
set targetReleaseResDir=%targetReleaseDir%\RES
if "%configurationName%" == "Release" (
	echo Release creation..

	:: recreate release dir
	rd "%targetReleaseDir%" /s /q
	md "%targetReleaseDir%"
	md "%targetReleaseResDir%"

	::make symlinks for dirs and files
	set targetReleaseBasicGamesPluginsDir=%targetReleaseDir%\%modOrganizerBasicGamesPluginsDirSubPath%
	if not exist "!targetReleaseBasicGamesPluginsDir!" (
		:: make parent dir
		md "%targetReleaseDir%\%moBasicGamesSubPath%"

		MKLINK "!targetReleaseBasicGamesPluginsDir!" "%targetBasicGamesPluginsDir%" /D
	)
	if not exist "%targetReleaseResDir%\lib" (
		MKLINK "%targetReleaseResDir%\lib" "%targetResDir%\lib" /D
	)
	if not exist "%targetReleaseResDir%\locale" (
		MKLINK "%targetReleaseResDir%\locale" "%targetResDir%\locale" /D
	)
	if not exist "%targetReleaseResDir%\%reportDirSubPath%" (
		:: make parent dir
		md "%targetReleaseResDir%\%themeDefaultSubPath%"

		MKLINK "%targetReleaseResDir%\%reportDirSubPath%" "%targetReportDir%" /D
	)
	if not exist "%targetReleaseDir%\tools\kkmanager" (
		MD "%targetReleaseResDir%\tools"
		MKLINK "%targetReleaseResDir%\tools\kkmanager" "%targetResDir%\tools\kkmanager" /D
	)
	if not exist "%targetReleaseDir%\%targetName%.exe" (
		MKLINK "%targetReleaseDir%\%targetName%.exe" "%targetDir%\%targetName%.exe"
	)
	set targetReleaseLibDir=%targetReleaseResDir%\lib
	if not exist "!targetReleaseLibDir!" (
		MKLINK "!targetReleaseLibDir!" "%targetLibDir%"
		Del "!targetReleaseLibDir!\*.pdb"
		Del "!targetReleaseLibDir!\*.xml"
	)

	:: create zip of release
	set destinationName=%targetName% %versionNumber%
	set destinationPath=%releasesDirPath%\!destinationName!
	set powershellApp=powershell Compress-Archive -Path '%targetReleaseDir%\*' -DestinationPath '!destinationPath!.zip' -Force
	set sevenZipApp7z="C:\Program Files\7-Zip\7z.exe" a "!destinationPath!.7z" -m0=LZMA2:d=96m:fb=128 -mx=9 -mmt4 "%targetReleaseDir%\*"
	set archiveAppRun=!sevenZipApp7z!	
	echo archive creating..
	echo run archive creation command: !archiveAppRun!
	!archiveAppRun!
)

:exit
echo postbuild script finished!
@echo off & setlocal EnableDelayedExpansion

:: ===========================================================================
::  postbuild.bat - runs after every build of "AI Helper".
::  It is wired into the PostBuildEvent in AI Helper.csproj, so it sits on the
::  normal build path and any failure here surfaces as a build error.
::
::  Arguments, in the order the csproj passes them:
::    arg1  targetDir          build output dir, ends with a slash
::    arg2  targetName         assembly name without extension
::    arg3  projectDir         project dir, ends with a slash
::    arg4  versionNumber      assembly version, read back from the built assembly by
::                             PostBuildMacros. Generated at build time by the
::                             SetAppVersion target as yy.M.d.N, from the version of
::                             the exe this build replaced, so only a Release build
::                             moves it on.
::    arg5  configurationName  Debug or Release
::
::  The csproj maps arg1-arg5 to TargetDir, TargetName, ProjectDir,
::  VersionNumber and ConfigurationName.
::
::  What this script does, in the order of the numbered steps below:
::    1. reads and validates the arguments
::    2. derives the source and destination roots
::    3. moves the referenced libraries out of the app root into RES\lib
::    4. mirrors the static BUILD\RES content next to the freshly built exe
::    5. copies the tools from the repository's tools folder into RES\tools of
::       the output, which is where the application reads them from
::    6. in Release only, copies the payload into RELEASE and archives it
::
::  Delayed expansion is enabled because several variables are both set and read
::  inside the same parenthesised block.
:: ===========================================================================

:: ---- 1. Read and validate the MSBuild arguments ---------------------------
set targetDir=%~1
set targetName=%~2
set projectDir=%~3
set versionNumber=%~4
set configurationName=%~5

::safe check if one of param is empty
if "%targetDir%" == "" goto :exit
if "%targetName%" == "" goto :exit
if "%projectDir%" == "" goto :exit
if "%versionNumber%" == "" goto :exit
if "%configurationName%" == "" goto :exit

:: ---- 2. Derive the source and destination roots ---------------------------
:: projectBuildDir is the checked-in BUILD folder, the source of the static
:: payload; targetResDir is the RES folder beside the freshly built exe and is
:: the destination for everything mirrored below.

set projectBuildDir=%projectDir%\BUILD
set projectResDir=%projectBuildDir%\RES
set targetResDir=%targetDir%\RES

:: ---- 3. Move the referenced libraries out of the app root into RES\lib -----
:: Keeps the app folder tidy: every dependency except the app's own assembly is
:: relocated into RES\lib, so the exe folder holds little more than the entry
:: point. The app's own dll, pdb and xml are excluded from the move.

echo move libs..
set targetLibDir=%targetResDir%\lib
:: Dependency files: moved rather than copied, and the app's own assembly is
:: excluded so the entry point stays in the app root.
::lib files
if exist "%targetLibDir%" rd "%targetLibDir%" /s /q
ROBOCOPY "%targetDir% " "%targetLibDir% " *.dll *.pdb *.xml /XF "%targetName%.dll" "%targetName%.pdb" "%targetName%.xml" /LEV:1 /MOV
:: Satellite assemblies live in one folder per culture; move each one that
:: exists.
::lib lang dirs
FOR %%D IN (zh-Hans zh-Hant cs de es fr it ja ko pl pt-BR ru tr) DO (
	if exist "%targetLibDir%\%%D" rd "%targetLibDir%\%%D" /s /q
	if exist "%targetDir%\%%D" move "%targetDir%\%%D" "%targetLibDir%\"
)

:: ---- 4. Mirror the static RES content from BUILD into the output -----------
:: Each block below copies one subtree of the checked-in BUILD\RES into the
:: output RES, and is skipped when its source is missing, so that a partial
:: checkout still builds.

:: 4a. Mod Organizer basic_games python plugins.
set moBasicGamesSubPath=MO\plugins\basic_games
set modOrganizerBasicGamesPluginsDirSubPath=%moBasicGamesSubPath%\games
set projectBasicGamesPluginsDir=%projectBuildDir%\%modOrganizerBasicGamesPluginsDirSubPath%
set targetBasicGamesPluginsDir=%targetResDir%\%modOrganizerBasicGamesPluginsDirSubPath%
if exist "%projectBasicGamesPluginsDir%\" (
	echo copy Mod organizer basic game plugins..
	robocopy "%projectBasicGamesPluginsDir% " "%targetBasicGamesPluginsDir%\ " *.py /MIR /COPYALL /B /R:3 /W:1
)
:: 4b. HTML report templates plus the per-game background images beside them.
set themeRootSubPath=theme
set themeDefaultSubPath=%themeRootSubPath%\default
set reportDirSubPath=%themeDefaultSubPath%\report
set reportTemplateFileName=ReportTemplate.html
set projectReportDir=%projectResDir%\%reportDirSubPath%
set projectReportTemplatePath=%projectReportDir%\%reportTemplateFileName%
set targetReportDir=%targetResDir%\%reportDirSubPath%
set targetReportTemplatePath=%targetReportDir%\%reportTemplateFileName%
if exist "%projectReportTemplatePath%" (
	echo copy report templates and game backgrounds..
	rem copy the report template and any game backgrounds beside it, mirroring the subtree so that any missing backgrounds are removed from the output.
	robocopy "%projectReportDir% " "%targetReportDir%\ " *.html *.jpg /MIR /COPYALL /B /R:3 /W:1
)
set themeCustomSubPath=%themeRootSubPath%\custom
set themeCustomProjectSubPath=%projectResDir%\%themeCustomSubPath%
:: 4b. Custom themes, if any exist.
if exist "%themeCustomProjectSubPath%\" (
	echo copy custom themes..
	rem copy the custom theme .ini files
	robocopy "%themeCustomProjectSubPath% " "%targetResDir%\%themeCustomSubPath%\ " *.ini /COPYALL /B /R:3 /W:1
)
:: 4c. Web link definitions consumed by the links UI.
set projectResLinksDir=%projectResDir%\links
set targetResLinksDir=%targetResDir%\links
if exist "%projectResLinksDir%" (
	echo copy weblinks info txt..
	robocopy "%projectResLinksDir% " "%targetResLinksDir%\ " *.txt /MIR /COPYALL /B /R:3 /W:1
)
:: 4d. Compiled gettext catalogues and their .po sources.
set projectResLocaleDir=%projectResDir%\locale
set targetResLocaleDir=%targetResDir%\locale
if exist "%projectResLocaleDir%" (
	echo copy localization files
	robocopy "%projectResLocaleDir% " "%targetResLocaleDir%\ " *.po *.mo /MIR /COPYALL /B /R:3 /W:1
)

:: ---- 5. Tools from the repository's tools folder ---------------------------
:: The tools are gathered in <repo>\tools, outside the application's output, so
:: that one build of a tool serves every platform. The launch assistant is built
:: there by the PreBuildEvent in the csproj, which has already run by the time
:: this script does; the rest are put there by hand. They still have to end up in
:: RES\tools of the output: that is the folder the application reads them from at
:: run time, and it is what the Release step below links into the skeleton. Each
:: block is skipped when its source is missing, so a checkout that has never
:: built a tool still builds.

set projectToolsDir=%projectDir%\..\tools
set targetToolsDir=%targetResDir%\tools
set kkManagerFolderName=kkmanager
if exist "%projectToolsDir%\!kkManagerFolderName!\" (
	echo copy kkmanager tool..
	robocopy "%projectToolsDir%\!kkManagerFolderName!\ " "%targetToolsDir%\!kkManagerFolderName!\ " *.* /MIR /COPYALL /B /R:3 /W:1
)
set launchAssistantFolderName=aihelper-launch-assistant
if exist "%projectToolsDir%\!launchAssistantFolderName!\" (
	echo copy the launch tool..
	robocopy "%projectToolsDir%\!launchAssistantFolderName!\ " "%targetToolsDir%\!launchAssistantFolderName!\ " *.* /MIR /COPYALL /B /R:3 /W:1

	rem A release ships programs and not the symbols they were built from, nor the log a tool writes
	rem while it runs. The log belongs where it runs and not in a build output - a copy mirrored in
	rem from the repository's tools folder is this machine's leftover from having run the tool there -
	rem and the program database is for whoever debugs the tool where it is built. Both exclusions
	rem are made for both tools, because the release block below ships the two the same way.
	if exist "%targetToolsDir%\!launchAssistantFolderName!\*.pdb" Del "%targetToolsDir%\!launchAssistantFolderName!\*.pdb"
	if exist "%targetToolsDir%\!launchAssistantFolderName!\*.log" Del "%targetToolsDir%\!launchAssistantFolderName!\*.log"
)
set updaterFolderName=aihelper-updater
if exist "%projectToolsDir%\!updaterFolderName!\" (
	echo copy the updater tool..
	robocopy "%projectToolsDir%\!updaterFolderName!\ " "%targetToolsDir%\!updaterFolderName!\ " *.* /MIR /COPYALL /B /R:3 /W:1

	rem The same two exclusions as the launch tool above.
	if exist "%targetToolsDir%\!updaterFolderName!\*.pdb" Del "%targetToolsDir%\!updaterFolderName!\*.pdb"
	if exist "%targetToolsDir%\!updaterFolderName!\*.log" Del "%targetToolsDir%\!updaterFolderName!\*.log"
)

:: ---- 6. Release packaging, Release configuration only ----------------------
:: release creation
set releasesDirPath=%targetDir%RELEASES
if not exist "%releasesDirPath%" md "%releasesDirPath%"
set targetReleaseDir=%targetDir%RELEASE
set targetReleaseResDir=%targetReleaseDir%\RES
:: Debug stops after the RES mirroring above; everything below is Release-only.
if "%configurationName%" == "Release" (
	echo Release creation..

	rem recreate release dir
	if exist "%targetReleaseDir%" rd "%targetReleaseDir%" /s /q
	md "%targetReleaseDir%"
	md "%targetReleaseResDir%"

	rem The release folder is a real copy of the payload and not a skeleton of symlinks. It used to
	rem be built out of MKLINK /D links pointing back into the build output, which kept one copy of
	rem every file on disk - but a link is not a file. An archiver that stores the links instead of
	rem following them packs a release of broken entries: Compress-Archive does, and 7-Zip only
	rem happens to follow them. Creating the links also needs a privilege a normal build does not
	rem have - Developer Mode or an elevated prompt - and copying needs none. It costs space in a
	rem folder the next build deletes, and it is what makes the release self-contained and the
	rem archive trustworthy.
	rem Note for anyone editing these comments: a closing parenthesis in one of them closes this
	rem block early and cmd aborts the whole batch with "was unexpected at this time". Keep the
	rem comments inside a block free of parentheses.

	rem The application, and the config file that carries its binding redirects, its RES\lib probing
	rem path, its NLog target and its settings defaults. It is not optional: a release that leaves it
	rem out logs nothing at all, because the log target is a section of this file.
	echo copy the app exe and its config..
	copy /Y "%targetDir%\%targetName%.exe" "%targetReleaseDir%\%targetName%.exe" >nul
	if exist "%targetDir%\%targetName%.exe.config" (
		copy /Y "%targetDir%\%targetName%.exe.config" "%targetReleaseDir%\%targetName%.exe.config" >nul
	)

	rem RES, one subtree at a time, exactly as step 4 mirrored it into the output - which is what the
	rem application reads at run time. The subtrees are listed here rather than derived, and that list
	rem is why the web link definitions and the shipped custom theme used to reach the output and never
	rem the release; anything added to BUILD\RES from now on has to be added here too.
	rem A release ships programs and not the symbols they were built from, so RES\lib loses its program
	rem databases and XML documentation files and RES\tools its program databases and the log a tool
	rem leaves on the machine that ran it. This is also where those files stop being deleted from the
	rem build output: the old code deleted them *through* the link, so a Release build stripped the
	rem symbols out of RES\lib of the very folder a debug session reads them from.
	echo copy libs to the release..
	robocopy "%targetLibDir% " "%targetReleaseResDir%\lib\ " *.* /MIR /COPYALL /B /R:3 /W:1 /XF *.pdb *.xml

	if exist "%targetResLocaleDir%\" (
		echo copy localization files to the release..
		robocopy "%targetResLocaleDir% " "%targetReleaseResDir%\locale\ " *.* /MIR /COPYALL /B /R:3 /W:1
	)

	if exist "%targetResLinksDir%\" (
		echo copy weblinks info txt to the release..
		robocopy "%targetResLinksDir% " "%targetReleaseResDir%\links\ " *.* /MIR /COPYALL /B /R:3 /W:1
	)

	if exist "%targetReportDir%\" (
		echo copy report templates to the release..
		robocopy "%targetReportDir% " "%targetReleaseResDir%\%reportDirSubPath%\ " *.* /MIR /COPYALL /B /R:3 /W:1
	)

	if exist "%targetResDir%\%themeCustomSubPath%\" (
		echo copy custom themes to the release..
		robocopy "%targetResDir%\%themeCustomSubPath% " "%targetReleaseResDir%\%themeCustomSubPath%\ " *.* /MIR /COPYALL /B /R:3 /W:1
	)

	if exist "%targetToolsDir%\" (
		echo copy the tools to the release..
		robocopy "%targetToolsDir% " "%targetReleaseResDir%\tools\ " *.* /MIR /COPYALL /B /R:3 /W:1 /XF *.pdb *.log
	)

	rem Mod Organizer's own copy of the game detection plugins. The application reads them from
	rem RES\MO and copies the plugin of the current game into the Mod Organizer folder, so a release
	rem without them cannot repair that folder.
	if exist "%targetResDir%\%modOrganizerBasicGamesPluginsDirSubPath%\" (
		echo copy basic games plugins into RES to the release..
		robocopy "%targetResDir%\%modOrganizerBasicGamesPluginsDirSubPath% " "%targetReleaseResDir%\%modOrganizerBasicGamesPluginsDirSubPath%\ " *.* /MIR /COPYALL /B /R:3 /W:1
	)

	rem The same plugins again where Mod Organizer loads them from, which is the path the updater's
	rem rules file names. Both copies ship: the one under RES is the application's source, this one
	rem is already in place for a fresh install.
	if exist "%targetBasicGamesPluginsDir%\" (
		echo copy basic games plugins to the release..
		robocopy "%targetBasicGamesPluginsDir% " "%targetReleaseDir%\%modOrganizerBasicGamesPluginsDirSubPath%\ " *.* /MIR /COPYALL /B /R:3 /W:1
	)

	rem Per-game base info. Copied from the checked-in BUILD folder, not from the output, which never
	rem gets it.
	if exist "%projectBuildDir%\Games" (
		echo games dir with base info
		robocopy "%projectBuildDir%\Games\ " "%targetReleaseDir%\Games\ " *.* /MIR /COPYALL /B /R:3 /W:1
	)

	rem Pack the release. Two packers are declared but only the one selected by
	rem "archiveAppRun" runs, so switching packers is a one-line change. Both now pack a folder of
	rem real files, which is what Compress-Archive needs to produce anything usable at all.
	rem create archive of the release
	set destinationName=%targetName% %versionNumber%
	set destinationPath=%releasesDirPath%\!destinationName!
	rem 7-Zip's "a" updates an archive that already exists instead of replacing it, so an archive
	rem left by an earlier build of the same version would keep every file the new release no longer
	rem ships - and the version can repeat, because Clean deletes the exe the counter is read from.
	rem Removing the destination first is what makes the archive the release folder and nothing else.
	if exist "!destinationPath!.zip" Del "!destinationPath!.zip"
	if exist "!destinationPath!.7z" Del "!destinationPath!.7z"
	set powershellApp=powershell Compress-Archive -Path '%targetReleaseDir%\*' -DestinationPath '!destinationPath!.zip' -Force
	set sevenZipApp7z="C:\Program Files\7-Zip\7z.exe" a -y "!destinationPath!.7z" -m0=LZMA2:d=96m:fb=128 -mx=9 -mmt4 "%targetReleaseDir%\*"
	set archiveAppRun=!sevenZipApp7z!	
	echo '!destinationPath!' archive creating..
	echo run archive creation command: !archiveAppRun!
	!archiveAppRun!
)

:: ---- 7. Done ---------------------------------------------------------------
:exit
echo postbuild script finished!
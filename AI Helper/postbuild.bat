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
::  What this script does:
::    1. reads and validates the arguments
::    2. moves the referenced libraries out of the app root into RES\lib
::    3. mirrors the static BUILD\RES content next to the freshly built exe
::    4. copies the tools from the repository's tools folder into RES\tools of
::       the output, which is where the application reads them from
::    5. in Release only, builds a symlinked RELEASE skeleton and archives it
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
	:: copy the report template and any game backgrounds beside it, mirroring the subtree so that any missing backgrounds are removed from the output.
	robocopy "%projectReportDir% " "%targetReportDir%\ " *.html *.jpg /MIR /COPYALL /B /R:3 /W:1
)
set themeCustomSubPath=%themeRootSubPath%\custom
set themeCustomProjectSubPath=%projectResDir%\%themeCustomSubPath%
:: 4b. Custom themes, if any exist.
if exist "%themeCustomProjectSubPath%\" (
	echo copy custom themes..
	:: copy the custom theme .ini files
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

	:: The log the tool writes while it runs belongs where it runs and not in a build output. A copy
	:: mirrored in from the repository's tools folder is this machine's leftover from having run the
	:: tool there, and the tool writes a fresh one beside itself wherever it is deployed.
	if exist "%targetToolsDir%\!launchAssistantFolderName!\*.log" Del "%targetToolsDir%\!launchAssistantFolderName!\*.log"
)
set updaterFolderName=aihelper-updater
if exist "%projectToolsDir%\!updaterFolderName!\" (
	echo copy the updater tool..
	robocopy "%projectToolsDir%\!updaterFolderName!\ " "%targetToolsDir%\!updaterFolderName!\ " *.* /MIR /COPYALL /B /R:3 /W:1

	:: A release ships programs and not the symbols they were built from. The program database is
	:: for whoever debugs the tool where it is built, and nothing that ships reads it.
	if exist "%targetToolsDir%\!updaterFolderName!\*.pdb" Del "%targetToolsDir%\!updaterFolderName!\*.pdb"
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

	:: recreate release dir
	if exist "%targetReleaseDir%" rd "%targetReleaseDir%" /s /q
	md "%targetReleaseDir%"
	md "%targetReleaseResDir%"

	:: Build the RELEASE skeleton out of symlinks pointing back into the normal
	:: build output, so the archive step picks up the real single copy of each
	:: payload instead of duplicating it.
	::make symlinks for dirs and files
	set targetReleaseBasicGamesPluginsDir=%targetReleaseDir%\%modOrganizerBasicGamesPluginsDirSubPath%
	:: Mod Organizer basic_games plugins. The parent dir must exist first.
	if not exist "!targetReleaseBasicGamesPluginsDir!" (
		echo make link for basic games plugins dir
		:: make parent dir
		md "%targetReleaseDir%\%moBasicGamesSubPath%"

		MKLINK "!targetReleaseBasicGamesPluginsDir!" "%targetBasicGamesPluginsDir%" /D
	)
	:: Localization catalogues.
	if not exist "%targetReleaseResDir%\locale" (
		echo make link for locale dir
		MKLINK "%targetReleaseResDir%\locale" "%targetResLocaleDir%" /D
	)
	:: Report template and game backgrounds.
	if not exist "%targetReleaseResDir%\%reportDirSubPath%" (
		echo make link for report dir
		:: make parent dir
		md "%targetReleaseResDir%\%themeDefaultSubPath%"

		MKLINK "%targetReleaseResDir%\%reportDirSubPath%" "%targetReportDir%" /D
	)
	:: Tools. The parent directory is made once, before either of the tools below is linked. It
	:: used to be made inside the KKManager branch, so the skeleton's RES\tools existed only when
	:: KKManager happened to be linked, and the tool added below had nowhere to be linked into.
	set toolsSubPath=tools
	if not exist "%targetReleaseResDir%\!toolsSubPath!" (
		echo make tools dir
		MD "%targetReleaseResDir%\!toolsSubPath!"
	)

	:: KKManager updater tool.
	set kkManSubpath=tools\kkmanager
	if not exist "%targetReleaseDir%\!kkManSubpath!" (
		echo make link for kkman
		MKLINK "%targetReleaseResDir%\!kkManSubpath!" "%targetResDir%\!kkManSubpath!" /D
	)

	:: The launch tool, which the application starts instead of staying in memory for the whole of
	:: a game session. Linked only when it was built: a build of the application alone leaves it out,
	:: and the application falls back to starting the game itself.
	set launchAssistantSubpath=tools\aihelper-launch-assistant
	if not exist "%targetReleaseDir%\!launchAssistantSubpath!" (
		if exist "%targetResDir%\!launchAssistantSubpath!\" (
			echo make link for the launch tool
			MKLINK "%targetReleaseResDir%\!launchAssistantSubpath!" "%targetResDir%\!launchAssistantSubpath!" /D

			:: The log it writes while it runs is the build machine's leftover and has no business in
			:: a release. Its build keeps the program database among the intermediates and never writes
			:: one beside the exe, so the symbols are only ever left over from an older build of it.
			if exist "%targetResDir%\!launchAssistantSubpath!\*.pdb" Del "%targetResDir%\!launchAssistantSubpath!\*.pdb"
			if exist "%targetResDir%\!launchAssistantSubpath!\*.log" Del "%targetResDir%\!launchAssistantSubpath!\*.log"
		)
	)

	:: The updater, which applies a downloaded update after the application has closed. Linked on the
	:: same terms as the launch tool: a build that did not produce it leaves it out, and an update
	:: then waits until one that did. Unlike the launch tool its build does write a program database
	:: beside the exe, so both are deleted here - and deleted through the link, which is what removes
	:: them from the shared RES\tools folder rather than from a private copy.
	set updaterSubpath=tools\aihelper-updater
	if not exist "%targetReleaseDir%\!updaterSubpath!" (
		if exist "%targetResDir%\!updaterSubpath!\" (
			echo make link for the updater tool
			MKLINK "%targetReleaseResDir%\!updaterSubpath!" "%targetResDir%\!updaterSubpath!" /D

			if exist "%targetResDir%\!updaterSubpath!\*.pdb" Del "%targetResDir%\!updaterSubpath!\*.pdb"
			if exist "%targetResDir%\!updaterSubpath!\*.log" Del "%targetResDir%\!updaterSubpath!\*.log"
		)
	)
	:: The application executable itself.
	if not exist "%targetReleaseDir%\%targetName%.exe" (
		echo make link for the app exe
		MKLINK "%targetReleaseDir%\%targetName%.exe" "%targetDir%\%targetName%.exe"
	)
	:: The relocated dependencies. A release must not ship debug symbols or XML
	:: docs - note these are deleted through the link, so they are removed from
	:: the shared RES\lib folder and not from a private copy.
	set targetReleaseLibDir=%targetReleaseResDir%\lib
	if not exist "!targetReleaseLibDir!" (
		echo make link for the app exe libs dir
		MKLINK "!targetReleaseLibDir!" "%targetLibDir%" /D
		Del "!targetReleaseLibDir!\*.pdb"
		Del "!targetReleaseLibDir!\*.xml"
	)
	:: Per-game base info. Copied for real, unlike the links above.
	if exist "%projectBuildDir%\Games" (
		echo games dir with base info
		robocopy "%projectBuildDir%\Games\ " "%targetReleaseDir%\Games\ " *.* /MIR /COPYALL /B /R:3 /W:1
	)

	:: Pack the release. Two packers are declared but only the one selected by
	:: "archiveAppRun" runs, so switching packers is a one-line change.
	:: create archive of the release
	set destinationName=%targetName% %versionNumber%
	set destinationPath=%releasesDirPath%\!destinationName!
	set powershellApp=powershell Compress-Archive -Path '%targetReleaseDir%\*' -DestinationPath '!destinationPath!.zip' -Force
	set sevenZipApp7z="C:\Program Files\7-Zip\7z.exe" a "!destinationPath!.7z" -m0=LZMA2:d=96m:fb=128 -mx=9 -mmt4 "%targetReleaseDir%\*"
	set archiveAppRun=!sevenZipApp7z!	
	echo '!destinationPath!' archive creating..
	echo run archive creation command: !archiveAppRun!
	!archiveAppRun!
)

:: ---- 7. Done ---------------------------------------------------------------
:exit
echo postbuild script finished!
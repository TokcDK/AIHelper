using AIHelper.Games;
using AIHelper.Manage.ui.themes;
using AIHelper.SharedData;
using CheckForEmptyDir;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace AIHelper.Manage
{
    class ManageSettings
    {
        /// <summary>
        /// reference to the main form
        /// </summary>
        public static MainForm MainForm { get; internal set; }

        public static GameData Games { get; set; } = new GameData();

        internal static bool IsFirstRun => ManageIni.GetIniValueIfExist(AiHelperIniPath, "FirstRun", "General") == "True";

        internal static string CurrentGameRegistryInstallDirKeyName => ManageSettings.Games.Game.RegistryInstallDirKey;


        internal static string UpdateInfosFilePath => Path.Combine(ManageSettings.CurrentGameModsUpdateDir, "updateinfo.txt");

        internal static string LanuageID => CultureInfo.CurrentCulture.Name;// T._("en-US");


        internal static string CurrentGameLinksInfoDirPath => Path.Combine(ManageSettings.AppResDirPath, "links");

        /// <summary>
        /// path to dir where links info files are located
        /// </summary>
        /// <returns></returns>

        internal static string LinksInfoFilePath
        {
            get
            {
                foreach (var path in new[]
                {
                    Path.Combine(CurrentGameLinksInfoDirPath, CurrentGame.GameAbbreviation + ".txt"),
                    Path.Combine(CurrentGameLinksInfoDirPath, "Default.txt")
                })
                {
                    if (File.Exists(path)) return path;
                }

                return "";
            }
        }

        /// <summary>
        /// game update installer update info ini file name
        /// </summary>
        /// <returns></returns>

        internal static string GameUpdateInstallerIniFileName => "gameupdate.ini";

        internal static string CurrentGameMoGamePyPluginPath =>
#pragma warning disable CA1308 // Normalize strings to uppercase
            Path.Combine(AppModOrganizerDirPath, "plugins", "basic_games", "games", ManageSettings.Games.Game.BasicGamePluginName
                //+ GetCurrentGameExeName()
                //.Replace("_64", string.Empty)
                //.Replace("_32", string.Empty)
                //.Replace("AI-Syoujyo", "aigirl")
                //.Replace("Honey Select", "honeyselect")
                //.ToLowerInvariant()
                + ".py"
                );
#pragma warning restore CA1308 // Normalize strings to uppercase

        /// <summary>
        /// name of Update section in app ini
        /// </summary>
        /// <returns></returns>
        internal static string AppIniUpdateSectionName => "Update";

        /// <summary>
        /// name of key for update check timeout in minutes
        /// </summary>
        /// <returns></returns>
        internal static string UpdatesCheckTimeoutMinutesKeyName => "UpdatesCheckTimeoutMinutes";

        internal static string AppLocaleDirPath => Path.Combine(AppResDirPath, "locale");

        internal static void SettingsInit()
        {
            //int index = ManageSettings.Games.CurrentGameListIndex;
            //ManageSettings.GetCurrentGamePath() = ListOfGames[index].GetGamePath();
            //ModsPath = SettingsManage.GetModsPath();
            //DownloadsPath = SettingsManage.GetDownloadsPath();
            //DataPath = SettingsManage.GetDataPath();
            //MODirPath = SettingsManage.GetMOdirPath();
            //MOexePath = SettingsManage.GetMOexePath();
            //ManageSettings.GetModOrganizerIniPath() = SettingsManage.GetModOrganizerINIpath();
            //Install2MODirPath = SettingsManage.GetInstall2MODirPath();
            //OverwriteFolder = SettingsManage.GetOverwriteFolder();
            //OverwriteFolderLink = SettingsManage.GetOverwriteFolderLink();
            //SetupXmlPath = MOManage.GetSetupXmlPathForCurrentProfile();

        }

        internal static string DateTimeBasedSuffix => "_" + DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);

        internal static bool MoIsNew { get => ManageSettings.MOIsNew; }

        internal static string DefaultBepInEx5OlderVersion => "5.0.1";

        /// <summary>
        /// Section name of AIHelper to store required values
        /// </summary>
        /// <returns></returns>

        internal static string AiMetaIniSectionName => "AISettings";

        /// <summary>
        /// key name for store mod info value like requirements or incompatibilities
        /// </summary>
        /// <returns></returns>

        internal static string AiMetaIniKeyModlistRulesInfoName => "ModlistRulesInfo";


        internal static string AiMetaIniKeyUpdateName => "ModUpdateInfo";


        internal static string ApplicationStartupPath = "";

        internal static string[] ScreenResolutions => new string[]
            {
                "1280 x 720 (16 : 9)",
                "1366 x 768 (16 : 9)",
                "1536 x 864 (16 : 9)",
                "1600 x 900 (16 : 9)",
                "1920 x 1080 (16 : 9)",
                "2048 x 1152 (16 : 9)",
                "2560 x 1440 (16 : 9)",
                "3200 x 1800 (16 : 9)",
                "3840 x 2160 (16 : 9)",
                //GetCustomRes()
            };

        //internal static string GetModOrganizerGithubLink()
        //{
        //    return "https://github.com/Modorganizer2/modorganizer/releases/latest";
        //}


        internal static string CurrentMoProfileModlistPath => Path.Combine(MoSelectedProfileDirPath, "modlist.txt");

        //private static string GetCustomRes()
        //{
        //    throw new NotImplementedException();
        //}

        /// <summary>
        /// Mod Organizer profiles directory name
        /// </summary>
        /// <returns></returns>

        public static string MoProfilesDirName => "Profiles";


        private static string GeneralMoPath => AppModOrganizerDirPath;


        internal static string MoIniFileName => "ModOrganizer.ini";


        private static string MoIniFilePath => Path.Combine(GeneralMoPath, MoIniFileName);

        /// <summary>
        /// file where store update check date times
        /// </summary>
        /// <returns></returns>

        internal static string UpdateCheckDateTimesFilePath => Path.Combine(ModsUpdateDirPath, "updatecheckdatetimes.txt");


        internal static string MoCategoriesFileName => "categories.dat";


        private static string MoCategoriesFilePath => Path.Combine(GeneralMoPath, MoCategoriesFileName);

        /// <summary>
        /// Dir where will be placed update files and backups
        /// </summary>
        /// <returns></returns>

        internal static string ModsUpdateDirName => "update";

        /// <summary>
        /// Dir where will be placed update files and backups
        /// </summary>
        /// <returns></returns>

        internal static string ModsUpdateDirPath => Path.Combine(AppResDirPath, ModsUpdateDirName);


        internal static string CurrentGameModsUpdateDir => Path.Combine(ModsUpdateDirPath, CurrentGameDirName);


        internal static string ModsUpdateDbInfoDir => Path.Combine(ModsUpdateDirPath, "infos");


        internal static string CurrentGameModListRulesPath => Path.Combine(AppResDirPath, "rules", CurrentGameDirName, "modlist.txt");

        /// <summary>
        /// Plugins update report file path
        /// </summary>
        /// <returns></returns>

        internal static string ThemesDirName => "theme";

        /// <summary>
        /// Plugins update report file path
        /// </summary>
        /// <returns></returns>

        internal static string ThemesDir => Path.Combine(AppResDirPath, ThemesDirName);


        internal static string DefaultThemeDirName => "default";


        internal static string DefaultThemeDirPath => Path.Combine(ThemesDir, DefaultThemeDirName);


        internal static string ReportDirName => "report";


        internal static string ReportDirPath => Path.Combine(DefaultThemeDirPath, ReportDirName);


        internal static string ReportBGDirPath => Path.Combine(ReportDirPath, "bg");

        internal class UpdateReport
        {
            /// <summary>
            /// Plugins update report file name
            /// </summary>
            /// <returns></returns>

            internal static string ReportFileName => "ReportTemplate.html";


            internal static string HtmlBeforeModReportSuccessLine => "<details style=\"color:white\">";


            internal static string HtmlBeforeModReportErrorLine => "<details style=\"color:red\">";


            internal static string HtmlBeforeModReportWarningLine => "<details style=\"color:orange\">";


            internal static string HtmlModReportInLineBeforeMainMessage => "<summary style=\"color:white\">";


            internal static string HtmlModReportPreModnameTags => "<p style=\"color:lightgreen;display:inline\">";


            internal static string HtmlModReportPostModnameTags => "</p>";


            internal static string HtmlModReportPreVersionTags => "<p style=\"color:yellow;display:inline\">";


            internal static string HtmlModReportPostVersionTags => "</p>";


            internal static string HtmlModReportInLineAfterMainMessage => "</summary>";


            internal static string HtmlAfterModReportLine => "</details>";

            /// <summary>
            /// Plugins update report file path
            /// </summary>
            /// <returns></returns>

            internal static string ReportFilePath => Path.Combine(ReportDirPath, ReportFileName);

            /// <summary>
            /// Patter to replace with BG imega path
            /// </summary>
            /// <returns></returns>

            internal static string BgImageLinkPathPattern => "%BGImageLinkPath%";

            /// <summary>
            /// Path to replace with Update report title
            /// </summary>
            /// <returns></returns>

            internal static string ModsUpdateReportHeaderTextPattern => "%ModsUpdateReportHeaderText%";

            /// <summary>
            /// Pattern to replace with list of mods update reports
            /// </summary>
            /// <returns></returns>

            internal static string SingleModUpdateReportsTextSectionPattern => "%SingleModUpdateReportsTextSection%";

            /// <summary>
            /// Pattern to replace with notice, placed under Update report title
            /// </summary>
            /// <returns></returns>

            internal static string ModsUpdateInfoNoticePattern => "%ModsUpdateInfoNotice%";

            /// <summary>
            /// Pattern to replace with web link to page where update info can be viewed
            /// </summary>
            /// <returns></returns>

            internal static string InfoLinkPattern => "{{visit}}";


            internal static string HtmlPreInfoLinkHtml => "<a target=\"_blank\" rel=\"noopener noreferrer\" href=\"";


            internal static string HtmlAfterInfoLinkHtml => "\">";


            internal static string HtmlAfterInfoLinkText => "</a>";


            internal static string HtmlReportStyleText => " style=\"background-color:gray;\"";


            internal static string HtmlReportCategoryTemplateFilePath => Path.Combine(CurrentGameLinksInfoDirPath, "htmlCategoryTemplate.txt");



            internal static string HtmlReportCategoryTemplate
            {
                get
                {
                    if (File.Exists(HtmlReportCategoryTemplateFilePath))
                    {
                        var content = File.ReadAllLines(HtmlReportCategoryTemplateFilePath).First(l => !l.StartsWith(";"));
                        if (content.Contains("%category%") && content.Contains("%items%")) return content;
                    }
                    return "<details style=\"color:white\"><summary style=\"color:white\"><p style=\"color:lightgreen;display:inline\">%category%</p></summary>%items%</details>";
                }
            }

            internal static string HtmlReportCategoryItemTemplateFilePath => Path.Combine(CurrentGameLinksInfoDirPath, "htmlCategoryItemTemplate.txt");


            internal static string HtmlReportCategoryItemTemplate
            {
                get
                {
                    if (File.Exists(HtmlReportCategoryItemTemplateFilePath))
                    {
                        var content = File.ReadAllLines(HtmlReportCategoryItemTemplateFilePath).First(l => !l.StartsWith(";"));
                        if (content.Contains("%link%")) return content;
                    }
                    return "<a target=\"_blank\" rel=\"noopener noreferrer\" href=\"%link%\">%text%</a> - <p style=\"color:yellow;display:inline\">%description%</p><p style=\"color:yellow;display:inline\"></p>";
                }
            }

            internal static string CurrentGameBgFileName => CurrentGame.GameAbbreviation + ".jpg";


            internal static string CurrentGameBgFilePath => Path.Combine(ReportBGDirPath, CurrentGameBgFileName).Replace(Path.DirectorySeparatorChar.ToString(), "/");


            internal static string PreInfoLinkTitleText => T._("Website") + ">";


            internal static string InfoLinkText => T._("click");


            internal static string PostInfoLinkTitleText => "<";


            internal static string TitleText => T._("Update report");


            internal static string ModsUpdateInfoNoticeText => T._("Click on line for more info");

            internal static string NoModsUpdatesFoundText => T._("No updates found");


            internal static string HtmlBeginText => "<html><body" + HtmlReportStyleText + "><h1>";


            internal static string HtmlAfterHeaderText => "</h2><hr><br>";


            internal static string HtmlBetweenModsText => "";//<p> already make new line //"<br>";;


            internal static string HtmLendText => "<hr></body></html>";
        }

        /// <summary>
        /// Path to setting.ini of current selected MO profile from current game.
        /// </summary>
        /// <returns></returns>

        internal static string MoSelectedProfileSettingsPath => Path.Combine(MoSelectedProfileDirPath, "settings.ini");

        //internal static Dictionary<string, Game> GetListOfGames()
        //{
        //    List<Game> ListOfGames = GamesList.GetGamesList();
        //    Dictionary<string, Game> ListOfGames1 = new Dictionary<string, Game>();

        //    if (Directory.Exists(ManageSettings.GetGamesFolderPath()))
        //    {
        //        foreach (var candidateFolder in Directory.EnumerateDirectories(ManageSettings.GetGamesFolderPath()))
        //        {
        //            if (FolderIsValid(candidateFolder))
        //            {
        //                foreach (var game in ListOfGames)
        //                {
        //                    if (File.Exists(Path.Combine(candidateFolder, "Data", game.GetGameEXEName() + ".exe")))
        //                    {
        //                        ListOfGames1.Add(Path.GetFileName(candidateFolder), game);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    else
        //    {
        //        ListOfGames.Clear();
        //    }


        //    if (ListOfGames.Count == 0)
        //    {
        //        try
        //        {
        //            if (Directory.Exists(Path.Combine(ManageSettings.ApplicationStartupPath, "Mods"))
        //                &&
        //                Directory.Exists(Path.Combine(ManageSettings.ApplicationStartupPath, "Data"))
        //                &&
        //                !ManageFilesFolders.CheckDirectoryNullOrEmpty_Fast(Path.Combine(ManageSettings.ApplicationStartupPath, "Data"))
        //                //&&
        //                //!ManageFilesFolders.CheckDirectoryNullOrEmpty_Fast(Path.Combine(ManageSettings.ApplicationStartupPath, GetAppModOrganizerDirName()))
        //                &&
        //                IsMOFolderValid(Path.Combine(ManageSettings.ApplicationStartupPath, GetAppModOrganizerDirName()))
        //                //&&
        //                //Directory.Exists(Path.Combine(Path.Combine(ManageSettings.ApplicationStartupPath, GetAppModOrganizerDirName(), GetMoProfilesDirName())))
        //                //&&
        //                //!ManageFilesFolders.CheckDirectoryNullOrEmpty_Fast(Path.Combine(ManageSettings.ApplicationStartupPath, GetAppModOrganizerDirName(), GetMoProfilesDirName()))
        //                &&
        //                !ManageSymLinks.IsSymLink(Path.Combine(ManageSettings.ApplicationStartupPath, GetAppModOrganizerDirName(), MoIniFileName()))
        //                &&
        //                !ManageSymLinks.IsSymLink(Path.Combine(ManageSettings.ApplicationStartupPath, GetAppModOrganizerDirName(), MoCategoriesFileName()))
        //                )
        //            {
        //                ListOfGames1.Add(ManageSettings.ApplicationStartupPath, new RootGame());
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            _log.Debug("RootGame check failed. Error:" + Environment.NewLine + ex);
        //        }
        //    }

        //    //ListOfGames = ListOfGames.Where
        //    //(game =>
        //    //    Directory.Exists(game.GetGamePath())
        //    //    &&
        //    //    !ManageFilesFolders.CheckDirectoryNullOrEmpty_Fast(Path.Combine(game.GetGamePath(), GetAppModOrganizerDirName(), GetMoProfilesDirName()))
        //    //).ToList();

        //    return ListOfGames1;
        //}

        //private static bool FolderIsValid(string folder)
        //{
        //    string MOFolder;
        //    if (Directory.Exists(folder)
        //        &&
        //        Directory.Exists(Path.Combine(folder, "Mods"))
        //        &&
        //        Directory.Exists(Path.Combine(folder, "Data"))
        //        &&
        //        !ManageFilesFolders.CheckDirectoryNullOrEmpty_Fast(Path.Combine(folder, "Data"))
        //        &&
        //        ((IsMOFolderValid(MOFolder = Path.Combine(folder, GetAppModOrganizerDirName()))) || MOFolderFound(folder, ref MOFolder))
        //        //&&
        //        //!ManageFilesFolders.CheckDirectoryNullOrEmpty_Fast(Path.Combine(folder, GetAppModOrganizerDirName()))
        //        //&&
        //        //Directory.Exists(Path.Combine(Path.Combine(folder, GetAppModOrganizerDirName(), GetMoProfilesDirName())))
        //        //&&
        //        //!ManageFilesFolders.CheckDirectoryNullOrEmpty_Fast(Path.Combine(folder, GetAppModOrganizerDirName(), GetMoProfilesDirName()))
        //        )
        //    {
        //        return true;
        //    }

        //    return false;
        //}

        //private static bool MOFolderFound(string folder, ref string MOFolder)
        //{
        //    foreach (var subfolder in Directory.EnumerateDirectories(folder))
        //    {
        //        if (IsMOFolderValid(folder))
        //        {
        //            MOFolder = subfolder;
        //            return true;
        //        }
        //    }
        //    return false;
        //}

        private static bool IsMoFolderValid(string folder)
        {
            if (Directory.Exists(Path.Combine(folder, MoProfilesDirName))
                    && File.Exists(Path.Combine(folder, MoIniFileName))
                    && File.Exists(Path.Combine(folder, MoCategoriesFileName))
                    && !Path.Combine(folder, MoProfilesDirName).IsNullOrEmptyDirectory(mask: "modlist.txt", searchForFiles: true, searchForDirs: false, recursive: false, preciseMask: true)
                    )
            {
                return true;
            }
            return false;
        }

        internal static string FolderNamesOfFoundGame
        {
            get
            {
                string listOfGamesString = string.Empty;
                foreach (var game in ManageSettings.Games.Games) listOfGamesString += game.GameDirName + Environment.NewLine;

                return listOfGamesString;
            }
        }


        /// <summary>
        /// List of found games
        /// </summary>
        /// <returns></returns>
        internal static List<GameBase> ListOfGames => ManageSettings.Games.Games;


        /// <summary>
        /// Currents selected game
        /// </summary>
        /// <returns></returns>
        internal static GameBase CurrentGame => ManageSettings.Games.Game;

        /// <summary>
        /// Base Games folder where can be placed mo game folders or mogame info files
        /// </summary>
        /// <returns></returns>

        internal static string GamesBaseFolderPath => Path.Combine(ManageSettings.ApplicationStartupPath, "Games");


        internal static string CurrentGameParentDirPath => ManageSettings.Games.Game.GameDirInfo.Parent.FullName;


        internal static string CurrentGameSetupXmlFilePath => Path.Combine(ManageSettings.CurrentGameOverwriteFolderPath, "UserData", "setup.xml");


        internal static string CurrentGameSetupXmlFilePathinData => Path.Combine(ManageSettings.CurrentGameDataDirPath, "UserData", "setup.xml");


        internal static int CurrentGameIndex => 0;


        internal static string SettingsExePath => Path.Combine(CurrentGameDataDirPath, IniSettingsExeName + ".exe");


        internal static string CurrentGameDirPath => Path.Combine(CurrentGameParentDirPath, ManageSettings.CurrentGameDirName);

        /// <summary>
        /// Current game Mod Organizer dir path
        /// </summary>
        /// <returns></returns>

        internal static string CurrentGameModOrganizerDirPath => Path.Combine(CurrentGameDirPath, AppModOrganizerDirName);


        internal static string CurrentGameMoOverwritePath => CurrentGameOverwriteFolderPath;

        internal static string MoCurrentGameName { get => MoIsNew ? ManageSettings.Games.Game.GameName : "Skyrim"; }

        internal static string DummyFileName => "TESV.exe";


        internal static string DummyFilePath => Path.Combine(CurrentGameDirPath, DummyFileName);

        /// <summary>
        /// return current game exe path
        /// </summary>
        /// <returns></returns>

        internal static string CurrentGameExePath => Path.Combine(CurrentGameDataDirPath, CurrentGameExeName + ".exe");

        /// <summary>
        /// return current game exe name
        /// </summary>
        /// <returns></returns>

        internal static string CurrentGameExeName => ManageSettings.Games.Game.GameExeName;

        /// <summary>
        /// return current game exe name with removed suffixes like _64 or _32
        /// </summary>
        /// <returns></returns>
        internal static string CurrentGameExeNameNoSuffixes
        {
            get
            {
                var currentGameExeName = CurrentGameExeName;
                if (currentGameExeName.EndsWith("_32", StringComparison.InvariantCulture) || currentGameExeName.EndsWith("_64", StringComparison.InvariantCulture))
                {
                    currentGameExeName = currentGameExeName.Remove(currentGameExeName.Length - 3, 3);
                }

                return currentGameExeName;
            }
        }

        internal static string CurrentGameDirName => ManageSettings.Games.Game.GameDirName;

        /// <summary>
        /// file name for file where contains info to create symlink
        /// </summary>
        /// <returns></returns>

        internal static string LinkInfoFileName => "linkinfo.txt";

        /// <summary>
        /// overall path file where contains info to create symlinks
        /// </summary>
        /// <returns></returns>

        internal static string OverallLinkInfoFilePath => Path.Combine(CurrentGameDirPath, LinkInfoFileName);


        internal static string UpdateReportHtmlFileName => "report.html";


        internal static string UpdateReportHtmlFilePath => Path.Combine(CurrentGameModsUpdateDir, UpdateReportHtmlFileName);


        internal static string CurrentGameDisplayingName => ManageSettings.Games.Game.GameDisplayingName;


        internal static string StudioExeName => ManageSettings.Games.Game.GameStudioExeName;


        internal static string IniSettingsExeName => ManageSettings.Games.Game.IniSettingsExeName;


        internal static string AppResDirPath => Path.Combine(ManageSettings.ApplicationStartupPath, "RES");


        internal static string CurrentGameModsDirPath => Path.Combine(CurrentGameDirPath, "Mods");


        internal static string DownloadsPath => Path.Combine(CurrentGameDirPath, "Downloads");


        internal static string CurrentGameDataDirPath => Path.Combine(CurrentGameDirPath, "Data");


        internal static string AppModOrganizerDirName => "MO";


        internal static string AppModOrganizerDirPath => AppOldModOrganizerDirPath;// Path.Combine(GetAppResDirPath(), GetAppModOrganizerDirName());


        internal static string AppOldModOrganizerDirPath => Path.Combine(ManageSettings.ApplicationStartupPath, AppModOrganizerDirName);


        internal static string AppMOexePath => Path.Combine(AppModOrganizerDirPath, "ModOrganizer.exe");

        internal static string MOSelectedProfileDirName { get; set; } = "";

        internal static string GetMoSelectedProfileDirName()
        {
            if (ManageSettings.MOSelectedProfileDirName.Length > 0)
            {
                return ManageSettings.MOSelectedProfileDirName;
            }
            else
            {
                ManageSettings.MOSelectedProfileDirName = ManageModOrganizer.MOremoveByteArray(ManageIni.GetIniValueIfExist(File.Exists(ModOrganizerIniPath) ? ModOrganizerIniPath : ModOrganizerInIpathForSelectedGame, "selected_profile", "General"));

                return ManageSettings.MOSelectedProfileDirName;
            }
        }


        internal static string MoSelectedProfileDirPath => Path.Combine(CurrentGameModOrganizerDirPath, MoProfilesDirName, GetMoSelectedProfileDirName());


        internal static string AppMOiniFilePath => Path.Combine(AppModOrganizerDirPath, MoIniFileName);


        internal static string MOiniPathForSelectedGame => ManageFilesFoldersExtensions.GreateFileFolderIfNotExists(Path.Combine(CurrentGameModOrganizerDirPath, MoIniFileName));


        internal static string AppMOcategoriesFilePath => Path.Combine(AppModOrganizerDirPath, MoCategoriesFileName);


        internal static string CurrentGameMOcategoriesFilePath => ManageFilesFoldersExtensions.GreateFileFolderIfNotExists(Path.Combine(CurrentGameModOrganizerDirPath, MoCategoriesFileName));


        internal static string Install2MoDirPath => Path.Combine(CurrentGameDirPath, ModsInstallDirName);


        internal static string CurrentGameOverwriteFolderPath => ManageFilesFoldersExtensions.GreateFileFolderIfNotExists(Path.Combine(CurrentGameModOrganizerDirPath, "overwrite"), true);


        internal static string OverwriteFolderLink => Path.Combine(CurrentGameDirPath, "MOUserData");


        internal static string AiHelperIniPath => Path.Combine(ManageSettings.ApplicationStartupPath, ManageSettings.ApplicationProductName + ".ini");

        internal static int GetCurrentGameIndexByFolderName(List<GameBase> listOfGames, string folderName)
        {
            // return first game index if was not found game folder name in ini or ini was empty
            if (string.IsNullOrWhiteSpace(folderName)) return 0;

            for (var i = 0; i < listOfGames.Count; i++)
            {
                if (listOfGames[i].GameDirName == folderName) return i;
            }
            return 0;
        }


        internal static string ModOrganizerInIpathForSelectedGame => Path.Combine(CurrentGameDirPath, MoIniFileName);


        internal static string ModOrganizerIniPath => Path.Combine(AppModOrganizerDirPath, MoIniFileName);

        internal static string MOmodeSwitchDataDirName { get => "MOModeRestoreInfo"; }


        internal static string MOmodeSwitchDataDirPath => Path.Combine(CurrentGameDirPath, MOmodeSwitchDataDirName);


        internal static string CurrentGameMOmodeBakDirPath =>
            //return Path.Combine(GetMOmodeSwitchDataDirPath(), GetCurrentGameFolderName());
            MOmodeSwitchDataDirPath;


        internal static string CurrentGameMOmodeDataFilesBakDirPath => Path.Combine(CurrentGameMOmodeBakDirPath, "MOmodeDataFilesBak");


        internal static string CurrentGameModdedDataFilesListFilePath => Path.Combine(CurrentGameMOmodeBakDirPath, "ModdedDataFilesList.txt");


        internal static string CurrentGameVanillaDataFilesListFilePath => Path.Combine(CurrentGameMOmodeBakDirPath, "VanillaDataFilesList.txt");


        internal static string CurrentGameVanillaDataEmptyFoldersListFilePath => Path.Combine(CurrentGameMOmodeBakDirPath, "VanillaDataEmptyFoldersList.txt");


        internal static string CurrentGameMoToStandartConvertationOperationsListFilePath => Path.Combine(CurrentGameMOmodeBakDirPath, "MOToStandartConvertationOperationsList.txt");


        internal static string CurrentGameZipmodsGuidListFilePath => Path.Combine(CurrentGameMOmodeBakDirPath, "ZipmodsGUIDList.txt");

        internal static string DefaultSetupXmlValue => "<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<Setting>\r\n  <Size>1280 x 720 (16 : 9)</Size>\r\n  <Width>1280</Width>\r\n  <Height>720</Height>\r\n  <Quality>2</Quality>\r\n  <FullScreen>false</FullScreen>\r\n  <Display>0</Display>\r\n  <Language>0</Language>\r\n</Setting>";


        internal static string BepInExPath => Path.Combine(IsMoMode ? CurrentGameModsDirPath : CurrentGameDataDirPath, "BepInEx");


        internal static string BepInExCfgDirPath => Path.Combine(BepInExPath, "BepInEx", "config");

        internal static string _bepInExCfgFilePath { get; set; }
        internal static string BepInExCfgFilePath
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_bepInExCfgFilePath) && File.Exists(_bepInExCfgFilePath)) return _bepInExCfgFilePath;
                return _bepInExCfgFilePath = ManageModOrganizer.GetLastPath(Path.Combine(BepInExCfgDirPath, "BepInEx.cfg"));
            }

            set { _bepInExCfgFilePath = value; }
        }

        internal static void SwitchBepInExDisplayedLogLevelValue(CheckBox bepInExConsoleCheckBox, Label bepInExDisplayedLogLevelLabel, bool onlyShow = false, string targetSectionName = "Logging.Console")
        {
            //string curValue = ManageINI.GetINIValueIfExist(ManageSettings.GetBepInExCfgFilePath(), "DisplayedLogLevel", "Logging.Console");
            string curValue = ManageCfg.GetCfgValueIfExist(BepInExCfgFilePath, "DisplayedLogLevel", "Logging.Console");

            //in BepinEx 5.4 looks like DisplayedLogLevel was deleted 
            if (curValue.Length == 0) bepInExDisplayedLogLevelLabel.Visible = false;
            if (onlyShow)
            {
                bepInExDisplayedLogLevelLabel.Text = curValue;
            }
            else //switch
            {
                string[] values = { "None", "Fatal", "Error", "Warning", "Message", "Info", "Debug", "All" };

                bool setNext = false;
                foreach (var value in values)
                {
                    if (setNext)
                    {
                        //ManageINI.WriteINIValue(ManageSettings.GetBepInExCfgFilePath(), "Logging.Console", "DisplayedLogLevel", /*" " +*/ value);
                        ManageCfg.WriteCfgValue(BepInExCfgFilePath, targetSectionName, "DisplayedLogLevel", /*" " +*/ value);
                        bepInExDisplayedLogLevelLabel.Text = value;
                        return;
                    }
                    if (value == curValue)
                    {
                        setNext = true;
                    }
                }
                //ManageINI.WriteINIValue(ManageSettings.GetBepInExCfgFilePath(), "Logging.Console", "DisplayedLogLevel", /*" " +*/ values[0]);
                ManageCfg.WriteCfgValue(BepInExCfgFilePath, "Logging.Console", "DisplayedLogLevel", /*" " +*/ values[0]);
                bepInExDisplayedLogLevelLabel.Text = values[0];
            }
        }

        /// <summary>
        /// Gets the supported languages.
        /// </summary>
        internal static IEnumerable<string> Languages
        {
            get
            {
                EnsureInitialized();
                return LanguageModeMap.Keys.OrderBy(p => p);
            }
        }

        /// <summary>
        /// List of known games
        /// </summary>
        public static List<string> KnownGames { get; internal set; }
        public static string KnownGamesIniKeyName { get => "known_games"; }
        public static string SettingsIniSectionName { get => "Settings"; }
        public static string SelectedGameIniKeyName { get => "selected_game"; }

        /// <summary>
        /// List of forms which need to be minimized
        /// </summary>
        /// <returns></returns>
        internal static Form[] ListOfFormsForMinimize =>
            //info: http://www.cyberforum.ru/windows-forms/thread31052.html
            new Form[2] { ManageSettings.MainForm._extraSettingsForm, ManageSettings.MainForm };

        /// <summary>
        /// Converts a language to its identifier.
        /// </summary>
        /// <param name="language">The language."</param>
        /// <returns>The identifier or <see cref="string.Empty"/> if none.</returns>
        internal static string LanguageEnumToIdentifier
                (string language)
        {
            EnsureInitialized();
            LanguageModeMap.TryGetValue(language, out string mode);
            return mode;
        }

        /// <summary>
        /// Converts a language to its identifier.
        /// </summary>
        /// <param name="identifier">The identifier of language."</param>
        /// <returns>The identifier or <see cref="string.Empty"/> if none.</returns>
        internal static string LanguageEnumFromIdentifier
                (string identifier)
        {
            EnsureInitialized(true);
            LanguageModeMapReversed.TryGetValue(identifier, out string language);
            return language;
        }

        /// <summary>
        /// Ensures the translator has been initialized.
        /// </summary>
        internal static void EnsureInitialized(bool reversed = false)
        {
            if (reversed)
            {
                if (LanguageModeMapReversed == null)
                {
                    LanguageModeMapReversed = new Dictionary<string, string>
                {
                    { "auto", T._("Auto") },
                    { "af", T._("Afrikaans") },
                    { "sq", T._("Albanian") },
                    { "ar", T._("Arabic") },
                    { "hy", T._("Armenian") },
                    { "az", T._("Azerbaijani") },
                    { "eu", T._("Basque") },
                    { "be", T._("Belarusian") },
                    { "bn", T._("Bengali") },
                    { "bg", T._("Bulgarian") },
                    { "ca", T._("Catalan") },
                    { "zh-CN", T._("Chinese") },
                    { "hr", T._("Croatian") },
                    { "cs", T._("Czech") },
                    { "da", T._("Danish") },
                    { "nl", T._("Dutch") },
                    { "en", T._("English") },
                    { "eo", T._("Esperanto") },
                    { "et", T._("Estonian") },
                    { "tl", T._("Filipino") },
                    { "fi", T._("Finnish") },
                    { "fr", T._("French") },
                    { "gl", T._("Galician") },
                    { "de", T._("German") },
                    { "ka", T._("Georgian") },
                    { "el", T._("Greek") },
                    { "ht", T._("Haitian Creole") },
                    { "iw", T._("Hebrew") },
                    { "hi", T._("Hindi") },
                    { "hu", T._("Hungarian") },
                    { "is", T._("Icelandic") },
                    { "id", T._("Indonesian") },
                    { "ga", T._("Irish") },
                    { "it", T._("Italian") },
                    { "ja", T._("Japanese") },
                    { "ko", T._("Korean") },
                    { "lo", T._("Lao") },
                    { "la", T._("Latin") },
                    { "lv", T._("Latvian") },
                    { "lt", T._("Lithuanian") },
                    { "mk", T._("Macedonian") },
                    { "ms", T._("Malay") },
                    { "mt", T._("Maltese") },
                    { "no", T._("Norwegian") },
                    { "fa", T._("Persian") },
                    { "pl", T._("Polish") },
                    { "pt", T._("Portuguese") },
                    { "ro", T._("Romanian") },
                    { "ru", T._("Russian") },
                    { "sr", T._("Serbian") },
                    { "sk", T._("Slovak") },
                    { "sl", T._("Slovenian") },
                    { "es", T._("Spanish") },
                    { "sw", T._("Swahili") },
                    { "sv", T._("Swedish") },
                    { "ta", T._("Tamil") },
                    { "te", T._("Telugu") },
                    { "th", T._("Thai") },
                    { "tr", T._("Turkish") },
                    { "uk", T._("Ukrainian") },
                    { "ur", T._("Urdu") },
                    { "vi", T._("Vietnamese") },
                    { "cy", T._("Welsh") },
                    { "yi", T._("Yiddish") }
                };
                }
            }
            else
            {
                if (LanguageModeMap == null)
                {
                    LanguageModeMap = new Dictionary<string, string>
                {
                    { T._("Auto"), "auto" },
                    { T._("Afrikaans"), "af" },
                    { T._("Albanian"), "sq" },
                    { T._("Arabic"), "ar" },
                    { T._("Armenian"), "hy" },
                    { T._("Azerbaijani"), "az" },
                    { T._("Basque"), "eu" },
                    { T._("Belarusian"), "be" },
                    { T._("Bengali"), "bn" },
                    { T._("Bulgarian"), "bg" },
                    { T._("Catalan"), "ca" },
                    { T._("Chinese"), "zh-CN" },
                    { T._("Croatian"), "hr" },
                    { T._("Czech"), "cs" },
                    { T._("Danish"), "da" },
                    { T._("Dutch"), "nl" },
                    { T._("English"), "en" },
                    { T._("Esperanto"), "eo" },
                    { T._("Estonian"), "et" },
                    { T._("Filipino"), "tl" },
                    { T._("Finnish"), "fi" },
                    { T._("French"), "fr" },
                    { T._("Galician"), "gl" },
                    { T._("German"), "de" },
                    { T._("Georgian"), "ka" },
                    { T._("Greek"), "el" },
                    { T._("Haitian Creole"), "ht" },
                    { T._("Hebrew"), "iw" },
                    { T._("Hindi"), "hi" },
                    { T._("Hungarian"), "hu" },
                    { T._("Icelandic"), "is" },
                    { T._("Indonesian"), "id" },
                    { T._("Irish"), "ga" },
                    { T._("Italian"), "it" },
                    { T._("Japanese"), "ja" },
                    { T._("Korean"), "ko" },
                    { T._("Lao"), "lo" },
                    { T._("Latin"), "la" },
                    { T._("Latvian"), "lv" },
                    { T._("Lithuanian"), "lt" },
                    { T._("Macedonian"), "mk" },
                    { T._("Malay"), "ms" },
                    { T._("Maltese"), "mt" },
                    { T._("Norwegian"), "no" },
                    { T._("Persian"), "fa" },
                    { T._("Polish"), "pl" },
                    { T._("Portuguese"), "pt" },
                    { T._("Romanian"), "ro" },
                    { T._("Russian"), "ru" },
                    { T._("Serbian"), "sr" },
                    { T._("Slovak"), "sk" },
                    { T._("Slovenian"), "sl" },
                    { T._("Spanish"), "es" },
                    { T._("Swahili"), "sw" },
                    { T._("Swedish"), "sv" },
                    { T._("Tamil"), "ta" },
                    { T._("Telugu"), "te" },
                    { T._("Thai"), "th" },
                    { T._("Turkish"), "tr" },
                    { T._("Ukrainian"), "uk" },
                    { T._("Urdu"), "ur" },
                    { T._("Vietnamese"), "vi" },
                    { T._("Welsh"), "cy" },
                    { T._("Yiddish"), "yi" }
                };
                }
            }

        }

        /// <summary>
        /// User files directory path
        /// </summary>
        /// <param name="subDirPath">subpath to check if exists</param>
        /// <returns></returns>
        internal static string GetUserfilesDirectoryPath(string subDirPath = "")
        {
            var modNames = new[] {
                "MyUserData",
                "MyUserFiles",
                GameUserDataModName
            };
            var subPath = (string.IsNullOrWhiteSpace(subDirPath) ? "" : "\\" + subDirPath);
            var currentGameModsDirPath = CurrentGameModsDirPath;
            foreach (var modName in modNames)
            {
                var dirPath = Path.Combine(currentGameModsDirPath, modName) + subPath;

                if (Directory.Exists(dirPath))
                {
                    return dirPath;
                }
            }

            return $"{CurrentGameOverwriteFolderPath}{subPath}";
        }

        /// <summary>
        /// True when mo mode activated
        /// </summary>
        /// <returns></returns>

        internal static bool IsMoMode = true;


        internal static string CurrentGameInstallDirPath => Path.Combine(AppResDirPath, "install", CurrentGameDirName);

        /// <summary>
        /// The language to translation mode map.
        /// </summary>
        internal static Dictionary<string, string> LanguageModeMap;


        internal static string CacheDirPath => Path.Combine(AppResDirPath, "cache");


        internal static string CurrentGameGetCacheDirPath => Path.Combine(CacheDirPath, CurrentGameDirName);

        /// <summary>
        /// file with saved path-guid pairs for guidlist
        /// </summary>
        /// <returns></returns>

        internal static string CachedGUIDFilePath => Path.Combine(CurrentGameGetCacheDirPath, "cachedzipmodsguid.txt");

        /// <summary>
        /// The language to translation mode map. (Reversed)
        /// </summary>
        internal static Dictionary<string, string> LanguageModeMapReversed;


        internal static bool CurrentGameIsHaveVr => File.Exists(Path.Combine(CurrentGameDataDirPath, CurrentGame.GameExeNameVr + ".exe"));


        internal static string DummyFileResPath => Path.Combine(AppResDirPath, "TESV.exe.dummy");


        internal static string CurrentGameExemoProfileName => ManageModOrganizer.GetMOcustomExecutableTitleByExeName(CurrentGameExeName);


        internal static string VarCurrentGameDataPath => "%Data%";


        internal static string VarCurrentGameModsPath => "%Mods%";


        internal static string VarCurrentGameMoOverwritePath => "%Overwrite%";


        internal static string UpdatedModsOlderVersionsBuckupDirName => "old";


        internal static string CurrentGameBakDirName => "Bak";


        internal static string CurrentGameBakDirPath => Path.Combine(CurrentGameDirPath, CurrentGameBakDirName);

        /// <summary>
        /// dir where will be stored old versions of updated content
        /// </summary>
        /// <returns></returns>

        internal static string UpdatedModsOlderVersionsBuckupDirPath => Path.Combine(CurrentGameBakDirPath, ModsUpdateDirName, UpdatedModsOlderVersionsBuckupDirName);


        internal static string ModsUpdateDirDownloadsPath => Path.Combine(ModsUpdateDirPath, ModsUpdateDirDownloadsName);


        internal static string ModsUpdateDirDownloadsName => "downloads";


        internal static string KkManagerDirPath => Path.Combine(AppResDirPath, "tools", "KKManager");


        internal static string KkManagerExeName => "KKManager.exe";


        internal static string KkManagerExePath => Path.Combine(KkManagerDirPath, KkManagerExeName);


        internal static string KkManagerStandaloneUpdaterExeName => "StandaloneUpdater.exe";


        internal static string KkManagerStandaloneUpdaterExePath => Path.Combine(KkManagerDirPath, KkManagerStandaloneUpdaterExeName);


        /// <summary>
        /// bleedingedge file indicator name
        /// </summary>
        /// <returns></returns>
        internal static string ZipmodsBleedingEdgeMarkFileName => "ilikebleeding.txt";


        /// <summary>
        /// path for bleedingedge file indicator for kkmanager to also check the  bleeding edge pack
        /// </summary>
        /// <returns></returns>
        internal static string ZipmodsBleedingEdgeMarkFilePath => Path.Combine(CurrentGameDataDirPath, "UserData", "LauncherEN", ZipmodsBleedingEdgeMarkFileName);


        internal static string ModsInstallDirName => "2MO";


        internal static string MoBaseGamesPluginGamesDirPath => Path.Combine(AppModOrganizerDirPath, "plugins", "basic_games", "games");


        internal static string DiscordGroupLink => "https://bit.ly/AIHelperDiscordRU"; //RU to EN for en


        internal static string ZipmodManifestGameNameByCurrentGame => ManageSettings.Games.Game.ZipmodManifestGameName;


        internal static string AppResToolsDirPath => Path.Combine(AppResDirPath, "tools");

        /// <summary>
        /// x64 locale emulator exe path
        /// </summary>
        /// <returns></returns>

        internal static string NtleaExePath => Path.Combine(AppResToolsDirPath, "ntlea", "x64", "ntleas.exe");

        /// <summary>
        /// Name of KKManager's target mod
        /// </summary>
        /// <returns></returns>

        internal static string KKManagerFilesModName => "KKManagerFiles";

        /// <summary>
        /// default meta ini value
        /// </summary>
        /// <returns></returns>

        internal static string DefaultMetaIni => "[General]\r\ncategory=\"\"\r\nversion=0.0.0.0\r\ngameName=" + MoCurrentGameName + "\r\nnotes=\"\"\r\nvalidated=true\r\nnewestVersion=\r\nignoredVersion=\r\ninstallationFile=\r\nrepository=\r\nmodid=-1\r\ncomments=\r\nnexusDescription=\r\nurl=\r\nhasCustomURL=false\r\nnexusFileStatus=1\r\nlastNexusQuery=\r\nlastNexusUpdate=\r\nnexusLastModified=2021-08-29T18:55:44Z\r\nconverted=false\r\ncolor=@Variant(\0\0\0\x43\0\xff\xff\0\0\0\0\0\0\0\0)\r\ntracked=0";

        /// <summary>
        /// Dirs where frm need to search kkmanager's created files
        /// </summary>
        /// <returns></returns>

        internal static IEnumerable<string> KKManagerUpdateSortDirs => new[] {
                ManageSettings.CurrentGameOverwriteFolderPath
                ,
                Path.Combine(ManageSettings.CurrentGameModsDirPath, ManageSettings.KKManagerFilesModName)
                };

        /// <summary>
        /// Notes for KKManagerFiles mod
        /// </summary>
        /// <returns></returns>

        internal static string KKManagerFilesNotes => "<!DOCTYPE HTML PUBLIC \\\"-//W3C//DTD HTML 4.0//EN\\\" \\\"http://www.w3.org/TR/REC-html40/strict.dtd\\\">\\n" +
                        "<html><head><meta name=\\\"qrichtext\\\" content=\\\"1\\\" /><style type=\\\"text/css\\\">" +
                        "\\np, li { white-space: pre-wrap; }\\n</style></head><body style=\\\" font-family:'MS Shell Dlg 2';" +
                        " font-size:8.25pt; font-weight:400; font-style:normal;\\\">" +
                        "\\n<p style=\\\" margin-top:0px; margin-bottom:0px; margin-left:0px; margin-right:0px;" +
                        " -qt-block-indent:0; text-indent:0px;\\\">\\x417\\x434\\x435\\x441\\x44c" +
                        " \\x445\\x440\\x430\\x43d\\x44f\\x442\\x441\\x44f \\x444\\x430\\x439\\x43b\\x44b" +
                        " \\x441\\x43e\\x437\\x434\\x430\\x43d\\x43d\\x44b\\x435 \\x43f\\x440\\x438" +
                        " \\x440\\x430\\x431\\x43e\\x442\\x435 KKManager \\x438 \\x432\\x445\\x43e\\x434\\x44f\\x449\\x435\\x439" +
                        " \\x432 \\x435\\x433\\x43e \\x441\\x43e\\x441\\x442\\x430\\x432" +
                        " \\x43f\\x440\\x43e\\x433\\x440\\x430\\x43c\\x43c\\x44b" +
                        " \\x43e\\x431\\x43d\\x43e\\x432\\x43b\\x435\\x43d\\x438\\x44f.</p></body></html>";

        internal static bool IsHaveSideloaderMods => ManageSettings.Games.Game.IsHaveSideloaderMods;

        /// <summary>
        /// temp dir for downloads of kkmanager standalone updater
        /// </summary>
        /// <returns></returns>

        internal static string KKManagerDownloadsTempDir => Path.Combine(CurrentGameDataDirPath, "temp", "KKManager_downloads");

        /// <summary>
        /// folder where place broken symlinks
        /// </summary>
        /// <returns></returns>

        internal static string CurrentGameBrokenSymlinksDirPath => Path.Combine(CurrentGameDirPath, "BrokenSymlinks");

        internal static string CurrentGameRegistryPath => ManageSettings.Games.Game.RegistryPath;

        /// <summary>
        /// Mod name of dir where game will place new created files
        /// </summary>
        /// <returns></returns>
        internal static string GameUserDataModName { get => "GameUserData"; }

        public static string CurrentGameModsDirName { get; set; } = "Mods";
        public static string CurrentGameDataDirName { get; set; } = "Data";
        public static string CurrentGameInstallDirName { get; set; } = "2MO";
        public static string DefaultInitSettingExeName { get; set; } = "InitSetting";
        public static string GameVRexeSuffixName { get; set; } = "VR";
        public static bool AutoShortcutRegistryCheckBoxChecked { get; internal set; } = true;
        public static bool INITDone { get; internal set; } = false;
        public static int CurrentGameListIndex { get; internal set; } = 0;
        public static string SetupXmlPath { get; internal set; } = "";
        public static string XUAiniPath { get; internal set; }
        public static bool CurrentGameIsChanging { get; internal set; }
        public static bool SetModOrganizerINISettingsForTheGame { get; internal set; }
        public static bool ExtraSettingsInitOnLoadIsInAction { get; internal set; }
        public static bool MOIsNew { get; internal set; } = true;
        public static string ApplicationProductName { get; internal set; }

        /// <summary>
        /// Dir of Mod Organizer Basic game plugins for managed games
        /// </summary>
        public static string AppResBasicGamesDir { get => Path.Combine(AppResDirPath, "basicgames", "games"); }
        /// <summary>
        /// Directory name the dir where functions data dirs are located
        /// </summary>
        public static string CurrentGameFunctionsDirName { get => "func"; }
        /// <summary>
        /// Directory path the dir where functions data dirs are located
        /// </summary>
        public static string CurrentGameFunctionsDirPath { get => Path.Combine(AppResDirPath, CurrentGameFunctionsDirName); }
        /// <summary>
        /// Data directory cleaning function dir name
        /// </summary>
        public static string CurrentGameCleanFunctionDirName { get => "cleandata"; }
        /// <summary>
        /// Data directory cleaning function dir path
        /// </summary>
        public static string CurrentGameCleanFunctionDirPath { get => Path.Combine(CurrentGameFunctionsDirPath, CurrentGameCleanFunctionDirName); }
        /// <summary>
        /// Data directory cleaning function Blacklist path for current game
        /// </summary>
        public static string CurrentGameCleanFunctionBlackListFilePath { get => Path.Combine(CurrentGameCleanFunctionDirPath, CurrentGame.GameAbbreviation+"-bl.txt"); }
        /// <summary>
        /// Data directory cleaning function Whitelist path for current game
        /// </summary>
        public static string CurrentGameCleanFunctionWhiteListFilePath { get => Path.Combine(CurrentGameCleanFunctionDirPath, CurrentGame.GameAbbreviation+"-wl.txt"); }
        public static string GameExeNameX32 { get => CurrentGame.GameExeNameX32; }
        public static string GameExeNameVr { get => CurrentGame.GameExeNameVr; }
        public static string GameStudioExeNameX32 { get => CurrentGame.GameStudioExeNameX32; }
        /// <summary>
        /// Current theme of the app
        /// </summary>
        public static IUITheme CurrentTheme { get; internal set; }
        public static string IniThemeKeyName { get => "Theme"; }
        public static string IniSettingsSectionName { get => "Settings"; }
        public static string ThemeLabelColorSetIgnoreNameMark { get => "OwnColor"; }
    }
}
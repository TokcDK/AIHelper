using AIHelper.Games;
using AIHelper.SharedData;
using CheckForEmptyDir;
using GetListOfSubClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace AIHelper.Manage
{
    class ManageSettings
    {
        internal static bool IsFirstRun()
        {
            return ManageIni.GetIniValueIfExist(GetAiHelperIniPath(), "FirstRun", "General") == "True";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetUpdateInfosFilePath()
        {
            return Path.Combine(ManageSettings.GetCurrentGameModsUpdateDir(), "updateinfo.txt");
        }

        internal static string GetCurrentGameMoGamePyPluginPath()
        {
#pragma warning disable CA1308 // Normalize strings to uppercase
            return Path.Combine(GetMOdirPath(), "plugins", "modorganizer-basic_games", "games", "game_"
                + GetCurrentGameExeName()
                .Replace("_64", string.Empty)
                .Replace("_32", string.Empty)
                .Replace("AI-Syoujyo", "aigirl")
                .Replace("Honey Select", "honeyselect")
                .ToLowerInvariant()
                + ".py"
                );
#pragma warning restore CA1308 // Normalize strings to uppercase
        }

        internal static string GetAppLocaleDirPath()
        {
            return Path.Combine(GetAppResDir(), "locale");
        }

        internal static void SettingsInit()
        {
            //int index = Properties.Settings.Default.CurrentGameListIndex;
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

        internal static bool MoIsNew { get => Properties.Settings.Default.MOIsNew; }

        internal static string GetDefaultBepInEx5OlderVersion()
        {
            return "5.0.1";
        }

        /// <summary>
        /// Section name of AIHelper to store required values
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string AiMetaIniSectionName()
        {
            return "AISettings";
        }

        /// <summary>
        /// key name for store mod info value like requirements or incompatibilities
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string AiMetaIniKeyModlistRulesInfoName()
        {
            return "ModlistRulesInfo";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string AiMetaIniKeyUpdateName()
        {
            return "ModUpdateInfo";
        }

        internal static string[] GetScreenResolutions()
        {
            return new string[]
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
        }

        internal static string GetModOrganizerGithubLink()
        {
            return "https://github.com/Modorganizer2/modorganizer/releases/latest";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetCurrentMoProfileModlistPath()
        {
            return Path.Combine(GetMoSelectedProfileDirPath(), "modlist.txt");
        }

        private static string GetCustomRes()
        {
            throw new NotImplementedException();
        }

        internal static List<Game> GetListOfExistsGames()
        {
            //List<Game> listOfGames = GamesList.GetGamesList();
            List<Type> listOfGames = Inherited.GetListOfInheritedTypes(typeof(Game));
            //var listOfGamesRead = new List<Game>(listOfGames);

            var listOfGameDirs = new List<Game>();
            foreach (var gameDir in Directory.EnumerateDirectories(GetGamesFolderPath()))
            {
                foreach (var gameType in listOfGames)
                {
                    if (gameType == typeof(RootGame))
                    {
                        continue;
                    }

                    var game = (Game)Activator.CreateInstance(gameType);
                    if (!File.Exists(Path.Combine(gameDir, "Data", game.GetGameExeName() + ".exe")))
                    {
                        continue;
                    }

                    game.SetGameFolderName(Path.GetFileName(gameDir));

                    var mods = Path.Combine(gameDir, "Mods");
                    if (!Directory.Exists(mods))
                    {
                        Directory.CreateDirectory(mods);
                    }

                    //  check and write mod organizer dir
                    var mo = Path.Combine(gameDir, "MO");
                    if (!Directory.Exists(mo))
                    {
                        Directory.CreateDirectory(mo);
                    }

                    //  check and write mod organizer ini
                    var moIni = Path.Combine(mo, MoIniFileName());
                    if (!File.Exists(moIni))
                    {
                        File.WriteAllText(moIni, Properties.Resources.defmoini);

                        var ini = ManageIni.GetINIFile(moIni);

                        // check mo ini game parameters exist
                        ini.SetKey("General", "gameName", game.GetGameFolderName());
                        ini.SetKey("General", "gamePath", "@ByteArray(" + Path.Combine(ManageSettings.GetGamesFolderPath(), game.GetGameFolderName()).Replace("\\", "\\\\") + ")");
                        ini.SetKey("General", "selected_profile", "Default");
                        ini.SetKey("Settings", "mod_directory", Path.Combine(gameDir, game.GetGameFolderName(), "Mods").Replace("\\", "\\\\"));
                        ini.SetKey("Settings", "download_directory", Path.Combine(gameDir, game.GetGameFolderName(), "Downloads").Replace("\\", "\\\\"));
                        ini.SetKey("Settings", "download_directory", Path.Combine(gameDir, game.GetGameFolderName(), "MO", "profiles").Replace("\\", "\\\\"));
                        ini.SetKey("Settings", "download_directory", Path.Combine(gameDir, game.GetGameFolderName(), "MO", "overwrite").Replace("\\", "\\\\"));
                    }

                    // check and write categories dat
                    var catDat = Path.Combine(mo, MoCategoriesFileName());
                    if (!File.Exists(catDat))
                    {
                        File.WriteAllText(catDat, "");
                    }

                    // check and write default profile
                    var profiles = Path.Combine(mo, GetMoProfilesDirName());
                    var defaultProfile = Path.Combine(profiles, "Default");
                    if (!Directory.Exists(defaultProfile))
                    {
                        Directory.CreateDirectory(defaultProfile);
                        File.WriteAllText(Path.Combine(defaultProfile, "modlist.txt"), "# This file was automatically generated by Mod Organizer.\r\n");
                    }

                    listOfGameDirs.Add(game);
                }

            }

            //if (Directory.Exists(GetGamesFolderPath()))
            //{
            //    foreach (var game in listOfGamesRead)
            //    {
            //        if (!game.IsValidGame())
            //        {
            //            listOfGames.Remove(game);
            //        }
            //    }
            //}
            //else
            //{
            //    listOfGames.Clear();
            //}


            //if (listOfGames.Count == 0)
            if (listOfGameDirs.Count == 0)
            {
                try
                {
                    if (Directory.Exists(Path.Combine(Properties.Settings.Default.ApplicationStartupPath, "Mods"))
                        &&
                        Directory.Exists(Path.Combine(Properties.Settings.Default.ApplicationStartupPath, "Data"))
                        &&
                        !Path.Combine(Properties.Settings.Default.ApplicationStartupPath, "Data").IsNullOrEmptyDirectory(mask: "*.exe", searchForFiles: true, searchForDirs: false, recursive: false)
                        //&&
                        //!ManageFilesFolders.CheckDirectoryNullOrEmpty_Fast(GetMOdirPath())
                        &&
                        IsMoFolderValid(GetMOdirPath())
                        //&&
                        //Directory.Exists(Path.Combine(GetMOdirPath(), GetMoProfilesDirName())))
                        //&&
                        //!ManageFilesFolders.CheckDirectoryNullOrEmpty_Fast(Path.Combine(GetMOdirPath(), GetMoProfilesDirName()))
                        &&
                        !ManageSymLinkExtensions.IsSymLink(MoIniFilePath())
                        &&
                        !ManageSymLinkExtensions.IsSymLink(MoCategoriesFilePath())
                        )
                    {
                        //listOfGames.Add(new RootGame());
                        listOfGameDirs.Add(new RootGame());
                    }
                }
                catch (Exception ex)
                {
                    ManageLogs.Log("RootGame check failed. Error:" + Environment.NewLine + ex);
                }
            }

            //ListOfGames = ListOfGames.Where
            //(game =>
            //    Directory.Exists(game.GetGamePath())
            //    &&
            //    !ManageFilesFolders.CheckDirectoryNullOrEmpty_Fast(Path.Combine(game.GetGamePath(), "MO", GetMoProfilesDirName()))
            //).ToList();

            return listOfGameDirs;
        }

        private static string GetMoProfilesDirName()
        {
            return "Profiles";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string GeneralMoPath()
        {
            return GetMOdirPath();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string MoIniFileName()
        {
            return "ModOrganizer.ini";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string MoIniFilePath()
        {
            return Path.Combine(GeneralMoPath(), MoIniFileName());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string MoCategoriesFileName()
        {
            return "categories.dat";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string MoCategoriesFilePath()
        {
            return Path.Combine(GeneralMoPath(), MoCategoriesFileName());
        }

        /// <summary>
        /// Dir where will be placed update files and backups
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetModsUpdateDir()
        {
            return Path.Combine(GetAppResDir(), "update");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetCurrentGameModsUpdateDir()
        {
            return Path.Combine(GetModsUpdateDir(), GetCurrentGameFolderName());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetModsUpdateDbInfoDir()
        {
            return Path.Combine(GetModsUpdateDir(), "infos");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetCurrentGameModListRulesPath()
        {
            return Path.Combine(GetAppResDir(), "rules", GetCurrentGameFolderName(), "modlist.txt");
        }

        /// <summary>
        /// Plugins update report file path
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetThemesDirName()
        {
            return "theme";
        }

        /// <summary>
        /// Plugins update report file path
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetThemesDir()
        {
            return Path.Combine(GetAppResDir(), GetThemesDirName());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetDefaultThemeDirName()
        {
            return "default";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetDefaultThemeDirPath()
        {
            return Path.Combine(GetThemesDir(), GetDefaultThemeDirName());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetReportDirName()
        {
            return "report";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetReportDirPath()
        {
            return Path.Combine(GetDefaultThemeDirPath(), GetReportDirName());
        }

        internal class UpdateReport
        {
            /// <summary>
            /// Plugins update report file name
            /// </summary>
            /// <returns></returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static string GetReportFileName()
            {
                return "ReportTemplate.html";
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static string HtmlBeforeModReportSuccessLine()
            {
                return "<details style=\"color:white\">";
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static string HtmlBeforeModReportErrorLine()
            {
                return "<details style=\"color:red\">";
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static string HtmlBeforeModReportWarningLine()
            {
                return "<details style=\"color:orange\">";
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static string HtmlModReportInLineBeforeMainMessage()
            {
                return "<summary style=\"color:white\">";
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static string HtmlModReportPreModnameTags()
            {
                return "<p style=\"color:lightgreen;display:inline\">";
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static string HtmlModReportPostModnameTags()
            {
                return "</p>";
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static string HtmlModReportPreVersionTags()
            {
                return "<p style=\"color:yellow;display:inline\">";
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static string HtmlModReportPostVersionTags()
            {
                return "</p>";
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static string HtmlModReportInLineAfterMainMessage()
            {
                return "</summary>";
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static string HtmlAfterModReportLine()
            {
                return "</details>";
            }

            /// <summary>
            /// Plugins update report file path
            /// </summary>
            /// <returns></returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static string GetReportFilePath()
            {
                return Path.Combine(GetReportDirPath(), GetReportFileName());
            }

            /// <summary>
            /// Patter to replace with BG imega path
            /// </summary>
            /// <returns></returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static string GetBgImageLinkPathPattern()
            {
                return "%BGImageLinkPath%";
            }

            /// <summary>
            /// Path to replace with Update report title
            /// </summary>
            /// <returns></returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static string GetModsUpdateReportHeaderTextPattern()
            {
                return "%ModsUpdateReportHeaderText%";
            }

            /// <summary>
            /// Pattern to replace with list of mods update reports
            /// </summary>
            /// <returns></returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static string GetSingleModUpdateReportsTextSectionPattern()
            {
                return "%SingleModUpdateReportsTextSection%";
            }

            /// <summary>
            /// Pattern to replace with notice, placed under Update report title
            /// </summary>
            /// <returns></returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static string GetModsUpdateInfoNoticePattern()
            {
                return "%ModsUpdateInfoNotice%";
            }

            /// <summary>
            /// Pattern to replace with web link to page where update info can be viewed
            /// </summary>
            /// <returns></returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static string InfoLinkPattern()
            {
                return "{{visit}}";
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static string HtmlPreInfoLinkHtml()
            {
                return "<a target=\"_blank\" rel=\"noopener noreferrer\" href=\"";
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static string HtmlAfterInfoLinkHtml()
            {
                return "\">";
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static string HtmlAfterInfoLinkText()
            {
                return "</a>";
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static string HtmlReportStyleText()
            {
                return " style=\"background-color:gray;\"";
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static string GetCurrentGameBgFileName()
            {
                return GetCurrentGameExeNameNoSuffixes() + "ReportBG.jpg";
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static string GetCurrentGameBgFilePath()
            {
                return Path.Combine(GetReportDirPath(), GetCurrentGameBgFileName()).Replace(Path.DirectorySeparatorChar.ToString(), "/");
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static string PreInfoLinkTitleText()
            {
                return T._("Website") + ">";
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static string InfoLinkText()
            {
                return T._("click");
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static string PostInfoLinkTitleText()
            {
                return "<";
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static string TitleText()
            {
                return T._("Update report");
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static string ModsUpdateInfoNoticeText()
            {
                return T._("Click on line for more info");
            }

            internal static string NoModsUpdatesFoundText()
            {
                return T._("No updates found");
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static string HtmlBeginText()
            {
                return "<html><body" + HtmlReportStyleText() + "><h1>";
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static string HtmlAfterHeaderText()
            {
                return "</h2><hr><br>";
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static string HtmlBetweenModsText()
            {
                return "";//<p> already make new line //"<br>";;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static string HtmLendText()
            {
                return "<hr></body></html>";
            }
        }

        /// <summary>
        /// Path to setting.ini of current selected MO profile from current game.
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetMoSelectedProfileSettingsPath()
        {
            return Path.Combine(GetMoSelectedProfileDirPath(), "settings.ini");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetCurrentGameSetupXmlFilePathinData()
        {
            return Path.Combine(ManageSettings.GetCurrentGameDataPath(), "UserData", "setup.xml");
        }

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
        //            if (Directory.Exists(Path.Combine(Properties.Settings.Default.ApplicationStartupPath, "Mods"))
        //                &&
        //                Directory.Exists(Path.Combine(Properties.Settings.Default.ApplicationStartupPath, "Data"))
        //                &&
        //                !ManageFilesFolders.CheckDirectoryNullOrEmpty_Fast(Path.Combine(Properties.Settings.Default.ApplicationStartupPath, "Data"))
        //                //&&
        //                //!ManageFilesFolders.CheckDirectoryNullOrEmpty_Fast(Path.Combine(Properties.Settings.Default.ApplicationStartupPath, "MO"))
        //                &&
        //                IsMOFolderValid(Path.Combine(Properties.Settings.Default.ApplicationStartupPath, "MO"))
        //                //&&
        //                //Directory.Exists(Path.Combine(Path.Combine(Properties.Settings.Default.ApplicationStartupPath, "MO", GetMoProfilesDirName())))
        //                //&&
        //                //!ManageFilesFolders.CheckDirectoryNullOrEmpty_Fast(Path.Combine(Properties.Settings.Default.ApplicationStartupPath, "MO", GetMoProfilesDirName()))
        //                &&
        //                !ManageSymLinks.IsSymLink(Path.Combine(Properties.Settings.Default.ApplicationStartupPath, "MO", MoIniFileName()))
        //                &&
        //                !ManageSymLinks.IsSymLink(Path.Combine(Properties.Settings.Default.ApplicationStartupPath, "MO", MoCategoriesFileName()))
        //                )
        //            {
        //                ListOfGames1.Add(Properties.Settings.Default.ApplicationStartupPath, new RootGame());
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            ManageLogs.Log("RootGame check failed. Error:" + Environment.NewLine + ex);
        //        }
        //    }

        //    //ListOfGames = ListOfGames.Where
        //    //(game =>
        //    //    Directory.Exists(game.GetGamePath())
        //    //    &&
        //    //    !ManageFilesFolders.CheckDirectoryNullOrEmpty_Fast(Path.Combine(game.GetGamePath(), "MO", GetMoProfilesDirName()))
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
        //        ((IsMOFolderValid(MOFolder = Path.Combine(folder, "MO"))) || MOFolderFound(folder, ref MOFolder))
        //        //&&
        //        //!ManageFilesFolders.CheckDirectoryNullOrEmpty_Fast(Path.Combine(folder, "MO"))
        //        //&&
        //        //Directory.Exists(Path.Combine(Path.Combine(folder, "MO", GetMoProfilesDirName())))
        //        //&&
        //        //!ManageFilesFolders.CheckDirectoryNullOrEmpty_Fast(Path.Combine(folder, "MO", GetMoProfilesDirName()))
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
            if (Directory.Exists(Path.Combine(folder, GetMoProfilesDirName()))
                    && File.Exists(Path.Combine(folder, MoIniFileName()))
                    && File.Exists(Path.Combine(folder, MoCategoriesFileName()))
                    && !Path.Combine(folder, GetMoProfilesDirName()).IsNullOrEmptyDirectory(mask: "modlist.txt", searchForFiles: true, searchForDirs: false, recursive: false, preciseMask: true)
                    )
            {
                return true;
            }
            return false;
        }

        internal static string GetFolderNamesOfFoundGame()
        {
            string listOfGamesString = string.Empty;
            foreach (var game in GameData.ListOfGames)
            {
                listOfGamesString += game.GetGameFolderName() + Environment.NewLine;
            }

            return listOfGamesString;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        /// <summary>
        /// List of found games
        /// </summary>
        /// <returns></returns>
        internal static List<Game> GetListOfGames()
        {
            return GameData.ListOfGames;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        /// <summary>
        /// Currents selected game
        /// </summary>
        /// <returns></returns>
        internal static Game GetCurrentGame()
        {
            return GameData.CurrentGame;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetGamesFolderPath()
        {
            //var GamesPath = Path.Combine(Application.StartupPath, "Games");
            return Path.Combine(Properties.Settings.Default.ApplicationStartupPath, "Games");
            //return Directory.Exists(GamesPath) ? GamesPath : Application.StartupPath;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int GetCurrentGameIndex()
        {
            return Properties.Settings.Default.CurrentGameListIndex;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetSettingsExePath()
        {
            return Path.Combine(GetCurrentGameDataPath(), GetIniSettingsExeName() + ".exe");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetCurrentGamePath()
        {
            return Path.Combine(GetGamesFolderPath(), ManageSettings.GetCurrentGameFolderName());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetCurrentGameMoOverwritePath()
        {
            return GetOverwriteFolder();
        }

        internal static string GetMoCurrentGameName()
        {
            if (MoIsNew)
            {
                return GameData.CurrentGame.GameName;
            }
            else
            {
                return "Skyrim";
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetDummyFileName()
        {
            return "TESV.exe";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetDummyFilePath()
        {
            return Path.Combine(GetCurrentGamePath(), GetDummyFileName());
        }

        /// <summary>
        /// return current game exe path
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetCurrentGameExePath()
        {
            return Path.Combine(GetCurrentGameDataPath(), GetCurrentGameExeName() + ".exe");
        }

        /// <summary>
        /// return current game exe name
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetCurrentGameExeName()
        {
            return GameData.CurrentGame.GetGameExeName();
        }

        /// <summary>
        /// return current game exe name with removed suffixes like _64 or _32
        /// </summary>
        /// <returns></returns>
        internal static string GetCurrentGameExeNameNoSuffixes()
        {
            var currentGameExeName = GetCurrentGameExeName();
            if (currentGameExeName.EndsWith("_32", StringComparison.InvariantCulture) || currentGameExeName.EndsWith("_64", StringComparison.InvariantCulture))
            {
                currentGameExeName = currentGameExeName.Remove(currentGameExeName.Length - 3, 3);
            }

            return currentGameExeName;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetCurrentGameFolderName()
        {
            return GameData.CurrentGame.GetGameFolderName();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string UpdateReportHtmlFileName()
        {
            return "report.html";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string UpdateReportHtmlFilePath()
        {
            return Path.Combine(GetCurrentGameModsUpdateDir(), UpdateReportHtmlFileName());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetCurrentGameDisplayingName()
        {
            return GameData.CurrentGame.GetGameDisplayingName();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetStudioExeName()
        {
            return GameData.CurrentGame.GetGameStudioExeName();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetIniSettingsExeName()
        {
            return GameData.CurrentGame.GetIniSettingsExeName();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetAppResDir()
        {
            return Path.Combine(Properties.Settings.Default.ApplicationStartupPath, "RES");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetCurrentGameModsPath()
        {
            return Path.Combine(GetCurrentGamePath(), "Mods");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetDownloadsPath()
        {
            return Path.Combine(GetCurrentGamePath(), "Downloads");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetCurrentGameDataPath()
        {
            return Path.Combine(GetCurrentGamePath(), "Data");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetMOdirPath()
        {
            return Path.Combine(Properties.Settings.Default.ApplicationStartupPath, "MO");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetMOexePath()
        {
            return Path.Combine(GetMOdirPath(), "ModOrganizer.exe");
        }

        internal static string GetMoSelectedProfileDirName()
        {
            if (Properties.Settings.Default.MOSelectedProfileDirName.Length > 0)
            {
                return Properties.Settings.Default.MOSelectedProfileDirName;
            }
            else
            {
                Properties.Settings.Default.MOSelectedProfileDirName = ManageModOrganizer.MOremoveByteArray(ManageIni.GetIniValueIfExist(File.Exists(GetModOrganizerIniPath()) ? GetModOrganizerIniPath() : GetModOrganizerInIpathForSelectedGame(), "selected_profile", "General"));

                return Properties.Settings.Default.MOSelectedProfileDirName;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetMoSelectedProfileDirPath()
        {
            return Path.Combine(GetCurrentGamePath(), "MO", GetMoProfilesDirName(), GetMoSelectedProfileDirName());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetMOiniPath()
        {
            return Path.Combine(GetMOdirPath(), MoIniFileName());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetMOiniPathForSelectedGame()
        {
            return ManageFilesFolders.GreateFileFolderIfNotExists(Path.Combine(GetCurrentGamePath(), "MO", MoIniFileName()));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetMOcategoriesPath()
        {
            return Path.Combine(GetMOdirPath(), MoCategoriesFileName());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetMOcategoriesPathForSelectedGame()
        {
            return ManageFilesFolders.GreateFileFolderIfNotExists(Path.Combine(GetCurrentGamePath(), "MO", MoCategoriesFileName()));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetInstall2MoDirPath()
        {
            return Path.Combine(GetCurrentGamePath(), ModsInstallDirName());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetOverwriteFolder()
        {
            return ManageFilesFolders.GreateFileFolderIfNotExists(Path.Combine(GetCurrentGamePath(), "MO", "overwrite"), true);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetOverwriteFolderLink()
        {
            return Path.Combine(GetCurrentGamePath(), "MOUserData");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetAiHelperIniPath()
        {
            return Path.Combine(Properties.Settings.Default.ApplicationStartupPath, Properties.Settings.Default.ApplicationProductName + ".ini");
        }

        internal static int GetCurrentGameIndexByFolderName(List<Game> listOfGames, string folderName)
        {
            // return first game index if was not found game folder name in ini or ini was empty
            if (string.IsNullOrWhiteSpace(folderName))
            {
                return 0;
            }

            for (var i = 0; i < listOfGames.Count; i++)
            {
                if (listOfGames[i].GetGameFolderName() == folderName)
                {
                    return i;
                }
            }
            return 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetModOrganizerInIpathForSelectedGame()
        {
            return Path.Combine(GetCurrentGamePath(), MoIniFileName());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetModOrganizerIniPath()
        {
            return Path.Combine(GetMOdirPath(), MoIniFileName());
        }

        internal static string MOmodeSwitchDataDirName { get => "momode"; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetMOmodeSwitchDataDirPath()
        {
            return Path.Combine(GetAppResDir(), MOmodeSwitchDataDirName);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetMOmodeDataFilesBakDirPath()
        {
            return Path.Combine(GetMOmodeSwitchDataDirPath(), GetCurrentGameFolderName(), "MOmodeDataFilesBak");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetModdedDataFilesListFilePath()
        {
            return Path.Combine(GetMOmodeSwitchDataDirPath(), GetCurrentGameFolderName(), "ModdedDataFilesList.txt");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetVanillaDataFilesListFilePath()
        {
            return Path.Combine(GetMOmodeSwitchDataDirPath(), GetCurrentGameFolderName(), "VanillaDataFilesList.txt");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetVanillaDataEmptyFoldersListFilePath()
        {
            return Path.Combine(GetMOmodeSwitchDataDirPath(), GetCurrentGameFolderName(), "VanillaDataEmptyFoldersList.txt");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetMoToStandartConvertationOperationsListFilePath()
        {
            return Path.Combine(GetMOmodeSwitchDataDirPath(), GetCurrentGameFolderName(), "MOToStandartConvertationOperationsList.txt");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetZipmodsGuidListFilePath()
        {
            return Path.Combine(GetMOmodeSwitchDataDirPath(), GetCurrentGameFolderName(), "ZipmodsGUIDList.txt");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetBepInExPath()
        {
            return Path.Combine(IsMoMode() ? GetCurrentGameModsPath() : GetCurrentGameDataPath(), "BepInEx");
        }

        internal static string GetDefaultSetupXmlValue()
        {
            return "<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<Setting>\r\n  <Size>1280 x 720 (16 : 9)</Size>\r\n  <Width>1280</Width>\r\n  <Height>720</Height>\r\n  <Quality>2</Quality>\r\n  <FullScreen>false</FullScreen>\r\n  <Display>0</Display>\r\n  <Language>0</Language>\r\n</Setting>";
        }

        internal static string GetBepInExCfgDirPath()
        {
            if (!IsMoMode())
            {
                return Path.Combine(GetBepInExPath(), "BepInEx", "config");
            }
            return ManageModOrganizer.GetLastMoFileDirPathFromEnabledModsOfActiveMoProfile(Path.Combine(GetBepInExPath(), "BepInEx", "config"), true);
        }

        internal static string GetBepInExCfgFilePath()
        {
            if (!IsMoMode())
            {
                return Path.Combine(GetBepInExPath(), "config", "BepInEx.cfg");
            }
            if (Properties.Settings.Default.BepinExCfgPath.Length > 0)
            {
                return Properties.Settings.Default.BepinExCfgPath;
            }
            return ManageModOrganizer.GetLastMoFileDirPathFromEnabledModsOfActiveMoProfile(Path.Combine(GetBepInExCfgDirPath(), "BepInEx.cfg"));
        }

        internal static void SwitchBepInExDisplayedLogLevelValue(CheckBox bepInExConsoleCheckBox, Label bepInExDisplayedLogLevelLabel, bool onlyShow = false, string targetSectionName = "Logging.Console")
        {
            //string curValue = ManageINI.GetINIValueIfExist(ManageSettings.GetBepInExCfgFilePath(), "DisplayedLogLevel", "Logging.Console");
            string curValue = ManageCfg.GetCfgValueIfExist(GetBepInExCfgFilePath(), "DisplayedLogLevel", "Logging.Console");
            if (curValue.Length == 0) //in BepinEx 5.4 looks like DisplayedLogLevel was deleted 
            {
                bepInExDisplayedLogLevelLabel.Visible = false;
            }
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
                        ManageCfg.WriteCfgValue(GetBepInExCfgFilePath(), targetSectionName, "DisplayedLogLevel", /*" " +*/ value);
                        bepInExDisplayedLogLevelLabel.Text = value;
                        return;
                    }
                    if (value == curValue)
                    {
                        setNext = true;
                    }
                }
                //ManageINI.WriteINIValue(ManageSettings.GetBepInExCfgFilePath(), "Logging.Console", "DisplayedLogLevel", /*" " +*/ values[0]);
                ManageCfg.WriteCfgValue(GetBepInExCfgFilePath(), "Logging.Console", "DisplayedLogLevel", /*" " +*/ values[0]);
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
        /// List of forms which need to be minimized
        /// </summary>
        /// <returns></returns>
        internal static Form[] ListOfFormsForMinimize()
        {
            //info: http://www.cyberforum.ru/windows-forms/thread31052.html
            return new Form[3] { GameData.MainForm._linksForm, GameData.MainForm._extraSettingsForm, GameData.MainForm };
        }

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
        /// True when mo mode activated
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsMoMode()
        {
            return Properties.Settings.Default.MOmode;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetCurrentGameInstallDirPath()
        {
            return Path.Combine(GetAppResDir(), "install", GetCurrentGameFolderName());
        }

        /// <summary>
        /// The language to translation mode map.
        /// </summary>
        internal static Dictionary<string, string> LanguageModeMap;

        /// <summary>
        /// The language to translation mode map. (Reversed)
        /// </summary>
        internal static Dictionary<string, string> LanguageModeMapReversed;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool GetCurrentGameIsHaveVr()
        {
            return File.Exists(Path.Combine(GetCurrentGameDataPath(), GetCurrentGameExeName() + "VR" + ".exe"));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetDummyFileResPath()
        {
            return Path.Combine(GetAppResDir(), "TESV.exe.dummy");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetCurrentGameExemoProfileName()
        {
            return ManageModOrganizer.GetMOcustomExecutableTitleByExeName(GetCurrentGameExeName());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string VarCurrentGameDataPath()
        {
            return "%Data%";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string VarCurrentGameModsPath()
        {
            return "%Mods%";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string VarCurrentGameMoOverwritePath()
        {
            return "%Overwrite%";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetUpdatedModsOlderVersionsBuckupDirName()
        {
            return "old";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetUpdatedModsOlderVersionsBuckupDirPath()
        {
            return Path.Combine(GetModsUpdateDir(), GetCurrentGameFolderName(), GetUpdatedModsOlderVersionsBuckupDirName());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetModsUpdateDirDownloadsPath()
        {
            return Path.Combine(GetModsUpdateDir(), GetModsUpdateDirDownloadsName());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetModsUpdateDirDownloadsName()
        {
            return "downloads";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetKkManagerDirPath()
        {
            return Path.Combine(GetAppResDir(), "tools", "KKManager");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetKkManagerExeName()
        {
            return "KKManager.exe";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetKkManagerExePath()
        {
            return Path.Combine(GetKkManagerDirPath(), GetKkManagerExeName());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string KkManagerStandaloneUpdaterExeName()
        {
            return "StandaloneUpdater.exe";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string KkManagerStandaloneUpdaterExePath()
        {
            return Path.Combine(GetKkManagerDirPath(), KkManagerStandaloneUpdaterExeName());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        /// <summary>
        /// bleedingedge file indicator name
        /// </summary>
        /// <returns></returns>
        internal static string ZipmodsBleedingEdgeMarkFileName()
        {
            return "ilikebleeding.txt";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        /// <summary>
        /// path for bleedingedge file indicator for kkmanager to also check the  bleeding edge pack
        /// </summary>
        /// <returns></returns>
        internal static string ZipmodsBleedingEdgeMarkFilePath()
        {
            return Path.Combine(GetCurrentGameDataPath(), "UserData", "LauncherEN", ZipmodsBleedingEdgeMarkFileName());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string ModsInstallDirName()
        {
            return "2MO";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetMoBaseGamesPluginGamesDirPath()
        {
            return Path.Combine(GetMOdirPath(), "plugins", "basic_games", "games");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetDiscordGroupLink()
        {
            return "https://bit.ly/AIHelperDiscordRU"; //RU to EN for en
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetZipmodManifestGameNameByCurrentGame()
        {
            return GameData.CurrentGame.ManifestGame;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetAppResToolsDirPath()
        {
            return Path.Combine(GetAppResDir(), "tools");
        }

        /// <summary>
        /// x64 locale emulator exe path
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string NtleaExePath()
        {
            return Path.Combine(GetAppResToolsDirPath(), "ntlea", "x64", "ntleas.exe");
        }

        /// <summary>
        /// Name of KKManager's target mod
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string KKManagerFilesModName()
        {
            return "KKManagerFiles";
        }

        /// <summary>
        /// default meta ini value
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetDefaultMetaIni()
        {
            return "[General]\r\ncategory=\"\"\r\nversion=0.0.0.0\r\ngameName="+GetMoCurrentGameName()+"\r\nnotes=\"\"\r\nvalidated=true\r\nnewestVersion=\r\nignoredVersion=\r\ninstallationFile=\r\nrepository=\r\nmodid=-1\r\ncomments=\r\nnexusDescription=\r\nurl=\r\nhasCustomURL=false\r\nnexusFileStatus=1\r\nlastNexusQuery=\r\nlastNexusUpdate=\r\nnexusLastModified=2021-08-29T18:55:44Z\r\nconverted=false\r\ncolor=@Variant(\0\0\0\x43\0\xff\xff\0\0\0\0\0\0\0\0)\r\ntracked=0";
        }
    }
}

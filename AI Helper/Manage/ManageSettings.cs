﻿using AIHelper.Games;
using AIHelper.SharedData;
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
            return ManageINI.GetINIValueIfExist(GetAIHelperINIPath(), "FirstRun", "General") == "True";
        }

        internal static string GetCurrentGameMOGamePyPluginPath()
        {
#pragma warning disable CA1308 // Normalize strings to uppercase
            return Path.Combine(GetMOdirPath(), "plugins", "modorganizer-basic_games", "games", "game_"
                + GetCurrentGameEXEName()
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

        internal static void SettingsINIT()
        {
            //int index = Properties.Settings.Default.CurrentGameListIndex;
            //Properties.Settings.Default.CurrentGamePath = ListOfGames[index].GetGamePath();
            //ModsPath = SettingsManage.GetModsPath();
            //DownloadsPath = SettingsManage.GetDownloadsPath();
            //DataPath = SettingsManage.GetDataPath();
            //MODirPath = SettingsManage.GetMOdirPath();
            //MOexePath = SettingsManage.GetMOexePath();
            //Properties.Settings.Default.ModOrganizerINIpath = SettingsManage.GetModOrganizerINIpath();
            //Install2MODirPath = SettingsManage.GetInstall2MODirPath();
            //OverwriteFolder = SettingsManage.GetOverwriteFolder();
            //OverwriteFolderLink = SettingsManage.GetOverwriteFolderLink();
            //SetupXmlPath = MOManage.GetSetupXmlPathForCurrentProfile();

        }

        internal static bool MOIsNew { get => Properties.Settings.Default.MOIsNew; }

        internal static string GetDefaultBepInEx5OlderVersion()
        {
            return "5.0.1";
        }

        /// <summary>
        /// Section name of AIHelper to store required values
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string AIMetaINISectionName()
        {
            return "AISettings";
        }

        /// <summary>
        /// key name for store mod info value like requirements or incompatibilities
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string AIMetaINIKeyModlistRulesInfoName()
        {
            return "ModlistRulesInfo";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string AIMetaINIKeyUpdateName()
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string CurrentMOProfileModlistPath()
        {
            return Path.Combine(GetMOSelectedProfileDirPath(), "modlist.txt");
        }

        private static string GetCustomRes()
        {
            throw new NotImplementedException();
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static List<Game> GetListOfExistsGames()
        {
            List<Game> ListOfGames = GamesList.GetGamesList();
            var ListOfGamesRead = new List<Game>(ListOfGames);
            if (Directory.Exists(GetGamesFolderPath()))
            {
                foreach (var game in ListOfGamesRead)
                {
                    if (
                            Directory.Exists(game.GetGamePath())
                            &&
                            !ManageFilesFolders.CheckDirectoryNullOrEmpty_Fast(Path.Combine(game.GetGamePath(), "MO", "Profiles"))
                        )
                    {

                    }
                    else
                    {
                        ListOfGames.Remove(game);
                    }
                }
            }
            else
            {
                ListOfGames.Clear();
            }


            if (ListOfGames.Count == 0)
            {
                try
                {
                    if (Directory.Exists(Path.Combine(Properties.Settings.Default.ApplicationStartupPath, "Mods"))
                        &&
                        Directory.Exists(Path.Combine(Properties.Settings.Default.ApplicationStartupPath, "Data"))
                        &&
                        !ManageFilesFolders.CheckDirectoryNullOrEmpty_Fast(Path.Combine(Properties.Settings.Default.ApplicationStartupPath, "Data"))
                        //&&
                        //!ManageFilesFolders.CheckDirectoryNullOrEmpty_Fast(GetMOdirPath())
                        &&
                        IsMOFolderValid(GetMOdirPath())
                        //&&
                        //Directory.Exists(Path.Combine(GetMOdirPath(), "Profiles")))
                        //&&
                        //!ManageFilesFolders.CheckDirectoryNullOrEmpty_Fast(Path.Combine(GetMOdirPath(), "Profiles"))
                        &&
                        !ManageSymLinkExtensions.IsSymLink(MOIniFilePath())
                        &&
                        !ManageSymLinkExtensions.IsSymLink(MOCategoriesFilePath())
                        )
                    {
                        ListOfGames.Add(new RootGame());
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
            //    !ManageFilesFolders.CheckDirectoryNullOrEmpty_Fast(Path.Combine(game.GetGamePath(), "MO", "Profiles"))
            //).ToList();

            return ListOfGames;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string GeneralMOPath()
        {
            return GetMOdirPath();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string MOIniFilePath()
        {
            return Path.Combine(GeneralMOPath(), "ModOrganizer.ini");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string MOCategoriesFilePath()
        {
            return Path.Combine(GeneralMOPath(), "categories.dat");
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
        internal static string GetModsUpdateDBInfoDir()
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
            internal static string HTMLBeforeModReportSuccessLine()
            {
                return "<details style=\"color:white\">";
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static string HTMLBeforeModReportErrorLine()
            {
                return "<details style=\"color:red\">";
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static string HTMLBeforeModReportWarningLine()
            {
                return "<details style=\"color:orange\">";
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static string HTMLModReportInLineBeforeMainMessage()
            {
                return "<summary style=\"color:white\">";
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static string HTMLModReportPreModnameTags()
            {
                return "<p style=\"color:lightgreen;display:inline\">";
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static string HTMLModReportPostModnameTags()
            {
                return "</p>";
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static string HTMLModReportPreVersionTags()
            {
                return "<p style=\"color:yellow;display:inline\">";
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static string HTMLModReportPostVersionTags()
            {
                return "</p>";
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static string HTMLModReportInLineAfterMainMessage()
            {
                return "</summary>";
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static string HTMLAfterModReportLine()
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
            internal static string GetBGImageLinkPathPattern()
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
            internal static string HTMLPreInfoLinkHTML()
            {
                return "<a target=\"_blank\" rel=\"noopener noreferrer\" href=\"";
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static string HTMLAfterInfoLinkHTML()
            {
                return "\">";
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static string HTMLAfterInfoLinkText()
            {
                return "</a>";
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static string HTMLReportStyleText()
            {
                return " style=\"background-color:gray;\"";
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static string GetCurrentGameBGFileName()
            {
                return GetCurrentGameEXENameNoSuffixes() + "ReportBG.jpg";
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static string GetCurrentGameBGFilePath()
            {
                return Path.Combine(GetReportDirPath(), GetCurrentGameBGFileName()).Replace(Path.DirectorySeparatorChar.ToString(), "/");
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
            internal static string HTMLBeginText()
            {
                return "<html><body" + HTMLReportStyleText() + "><h1>";
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static string HTMLAfterHeaderText()
            {
                return "</h2><hr><br>";
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static string HTMLBetweenModsText()
            {
                return "";//<p> already make new line //"<br>";;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static string HTMLendText()
            {
                return "<hr></body></html>";
            }
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
        //                //Directory.Exists(Path.Combine(Path.Combine(Properties.Settings.Default.ApplicationStartupPath, "MO", "Profiles")))
        //                //&&
        //                //!ManageFilesFolders.CheckDirectoryNullOrEmpty_Fast(Path.Combine(Properties.Settings.Default.ApplicationStartupPath, "MO", "Profiles"))
        //                &&
        //                !ManageSymLinks.IsSymLink(Path.Combine(Properties.Settings.Default.ApplicationStartupPath, "MO", "ModOrganizer.ini"))
        //                &&
        //                !ManageSymLinks.IsSymLink(Path.Combine(Properties.Settings.Default.ApplicationStartupPath, "MO", "categories.dat"))
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
        //    //    !ManageFilesFolders.CheckDirectoryNullOrEmpty_Fast(Path.Combine(game.GetGamePath(), "MO", "Profiles"))
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
        //        //Directory.Exists(Path.Combine(Path.Combine(folder, "MO", "Profiles")))
        //        //&&
        //        //!ManageFilesFolders.CheckDirectoryNullOrEmpty_Fast(Path.Combine(folder, "MO", "Profiles"))
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

        private static bool IsMOFolderValid(string folder)
        {
            if (Directory.Exists(Path.Combine(folder, "Profiles"))
                    && File.Exists(Path.Combine(folder, "ModOrganizer.ini"))
                    && File.Exists(Path.Combine(folder, "categories.dat"))
                    && !ManageFilesFolders.CheckDirectoryNullOrEmpty_Fast(Path.Combine(folder, "Profiles"))
                    )
            {
                return true;
            }
            return false;
        }

        internal static string GetStringListOfAllGames()
        {
            List<Game> ListOfGames = GamesList.GetGamesList();

            string listOfGamesString = string.Empty;
            foreach (var game in ListOfGames)
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
        internal static string GetSettingsEXEPath()
        {
            return Path.Combine(GetCurrentGameDataPath(), GetINISettingsEXEName() + ".exe");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetCurrentGamePath()
        {
            return Properties.Settings.Default.CurrentGamePath;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetCurrentGameMOOverwritePath()
        {
            return GetOverwriteFolder();
        }

        internal static string GETMOCurrentGameName()
        {
            if (MOIsNew)
            {
                if (GetCurrentGameEXEName() == "Koikatu")
                {
                    return "Koikatu";
                }
                else if (GetCurrentGameEXEName().StartsWith("HoneySelect2", StringComparison.InvariantCulture))
                {
                    return "HoneySelect2";
                }
                else if (GetCurrentGameEXEName().StartsWith("HoneySelect", StringComparison.InvariantCulture))
                {
                    return "HoneySelect";
                }
                else if (GetCurrentGameEXEName() == "AI-Syoujyo")
                {
                    return "AIGirl";
                }
                else if (GetCurrentGameEXEName() == "AI-SyoujyoTrial")
                {
                    return "AIGirlTrial";
                }

                return GetCurrentGameEXEName();
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
        /// return current game exe name
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetCurrentGameEXEName()
        {
            return Properties.Settings.Default.CurrentGameEXEName;
        }

        /// <summary>
        /// return current game exe name with removed suffixes like _64 or _32
        /// </summary>
        /// <returns></returns>
        internal static string GetCurrentGameEXENameNoSuffixes()
        {
            var CurrentGameEXEName = GetCurrentGameEXEName();
            if (CurrentGameEXEName.EndsWith("_32", StringComparison.InvariantCulture) || CurrentGameEXEName.EndsWith("_64", StringComparison.InvariantCulture))
            {
                CurrentGameEXEName = CurrentGameEXEName.Remove(CurrentGameEXEName.Length - 3, 3);
            }

            return CurrentGameEXEName;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetCurrentGameFolderName()
        {
            return Properties.Settings.Default.CurrentGameFolderName;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string UpdateReportHTMLFileName()
        {
            return "report.html";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string UpdateReportHTMLFilePath()
        {
            return Path.Combine(GetCurrentGameModsUpdateDir(), UpdateReportHTMLFileName());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetCurrentGameDisplayingName()
        {
            return Properties.Settings.Default.CurrentGameDisplayingName;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetStudioEXEName()
        {
            return Properties.Settings.Default.StudioEXEName;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetINISettingsEXEName()
        {
            return Properties.Settings.Default.INISettingsEXEName;
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

        internal static string GetMOSelectedProfileDirName()
        {
            if (Properties.Settings.Default.MOSelectedProfileDirName.Length > 0)
            {
                return Properties.Settings.Default.MOSelectedProfileDirName;
            }
            else
            {
                Properties.Settings.Default.MOSelectedProfileDirName = ManageMO.MOremoveByteArray(ManageINI.GetINIValueIfExist(File.Exists(GetModOrganizerINIpath()) ? GetModOrganizerINIpath() : GetModOrganizerINIpathForSelectedGame(), "selected_profile", "General"));

                return Properties.Settings.Default.MOSelectedProfileDirName;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetMOSelectedProfileDirPath()
        {
            return Path.Combine(GetCurrentGamePath(), "MO", "profiles", GetMOSelectedProfileDirName());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetMOiniPath()
        {
            return Path.Combine(GetMOdirPath(), "ModOrganizer.ini");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetMOiniPathForSelectedGame()
        {
            return ManageFilesFolders.GreateFileFolderIfNotExists(Path.Combine(GetCurrentGamePath(), "MO", "ModOrganizer.ini"));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetMOcategoriesPath()
        {
            return Path.Combine(GetMOdirPath(), "categories.dat");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetMOcategoriesPathForSelectedGame()
        {
            return ManageFilesFolders.GreateFileFolderIfNotExists(Path.Combine(GetCurrentGamePath(), "MO", "categories.dat"));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetInstall2MODirPath()
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
        internal static string GetAIHelperINIPath()
        {
            return Path.Combine(Properties.Settings.Default.ApplicationStartupPath, Properties.Settings.Default.ApplicationProductName + ".ini");
        }

        internal static int GetCurrentGameIndexByFolderName(List<Game> listOfGames, string FolderName)
        {
            for (var i = 0; i < listOfGames.Count; i++)
            {
                if (listOfGames[i].GetGameFolderName() == FolderName)
                {
                    return i;
                }
            }
            return 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetModOrganizerINIpathForSelectedGame()
        {
            return Path.Combine(GetCurrentGamePath(), "ModOrganizer.ini");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetModOrganizerINIpath()
        {
            return Path.Combine(GetMOdirPath(), "ModOrganizer.ini");
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
        internal static string GetMOToStandartConvertationOperationsListFilePath()
        {
            return Path.Combine(GetMOmodeSwitchDataDirPath(), GetCurrentGameFolderName(), "MOToStandartConvertationOperationsList.txt");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetZipmodsGUIDListFilePath()
        {
            return Path.Combine(GetMOmodeSwitchDataDirPath(), GetCurrentGameFolderName(), "ZipmodsGUIDList.txt");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetBepInExPath()
        {
            return Path.Combine(Properties.Settings.Default.MOmode ? GetCurrentGameModsPath() : GetCurrentGameDataPath(), "BepInEx");
        }

        internal static string GetBepInExCfgDirPath()
        {
            if (!Properties.Settings.Default.MOmode)
            {
                return Path.Combine(GetBepInExPath(), "BepInEx", "config");
            }
            return ManageMO.GetLastMOFileDirPathFromEnabledModsOfActiveMOProfile(Path.Combine(GetBepInExPath(), "BepInEx", "config"), true);
        }

        internal static string GetBepInExCfgFilePath()
        {
            if (!Properties.Settings.Default.MOmode)
            {
                return Path.Combine(GetBepInExPath(), "config", "BepInEx.cfg");
            }
            if (Properties.Settings.Default.BepinExCfgPath.Length > 0)
            {
                return Properties.Settings.Default.BepinExCfgPath;
            }
            return ManageMO.GetLastMOFileDirPathFromEnabledModsOfActiveMOProfile(Path.Combine(GetBepInExCfgDirPath(), "BepInEx.cfg"));
        }

        internal static void SwitchBepInExDisplayedLogLevelValue(CheckBox BepInExConsoleCheckBox, Label BepInExDisplayedLogLevelLabel, bool OnlyShow = false, string TargetSectionName = "Logging.Console")
        {
            //string curValue = ManageINI.GetINIValueIfExist(ManageSettings.GetBepInExCfgFilePath(), "DisplayedLogLevel", "Logging.Console");
            string curValue = ManageCFG.GetCFGValueIfExist(GetBepInExCfgFilePath(), "DisplayedLogLevel", "Logging.Console");
            if (curValue.Length == 0) //in BepinEx 5.4 looks like DisplayedLogLevel was deleted 
            {
                BepInExDisplayedLogLevelLabel.Visible = false;
            }
            if (OnlyShow)
            {
                BepInExDisplayedLogLevelLabel.Text = curValue;
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
                        ManageCFG.WriteCFGValue(GetBepInExCfgFilePath(), TargetSectionName, "DisplayedLogLevel", /*" " +*/ value);
                        BepInExDisplayedLogLevelLabel.Text = value;
                        return;
                    }
                    if (value == curValue)
                    {
                        setNext = true;
                    }
                }
                //ManageINI.WriteINIValue(ManageSettings.GetBepInExCfgFilePath(), "Logging.Console", "DisplayedLogLevel", /*" " +*/ values[0]);
                ManageCFG.WriteCFGValue(GetBepInExCfgFilePath(), "Logging.Console", "DisplayedLogLevel", /*" " +*/ values[0]);
                BepInExDisplayedLogLevelLabel.Text = values[0];
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
                return _languageModeMap.Keys.OrderBy(p => p);
            }
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
            _languageModeMap.TryGetValue(language, out string mode);
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
            _languageModeMapReversed.TryGetValue(identifier, out string language);
            return language;
        }

        /// <summary>
        /// Ensures the translator has been initialized.
        /// </summary>
        internal static void EnsureInitialized(bool reversed = false)
        {
            if (reversed)
            {
                if (_languageModeMapReversed == null)
                {
                    _languageModeMapReversed = new Dictionary<string, string>
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
                if (_languageModeMap == null)
                {
                    _languageModeMap = new Dictionary<string, string>
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
        internal static bool IsMOMode()
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
        internal static Dictionary<string, string> _languageModeMap;

        /// <summary>
        /// The language to translation mode map. (Reversed)
        /// </summary>
        internal static Dictionary<string, string> _languageModeMapReversed;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool GetCurrentGameIsHaveVR()
        {
            return File.Exists(Path.Combine(GetCurrentGameDataPath(), GetCurrentGameEXEName() + "VR" + ".exe"));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetDummyFileRESPath()
        {
            return Path.Combine(GetAppResDir(), "TESV.exe.dummy");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetCurrentGameEXEMOProfileName()
        {
            return ManageMO.GetMOcustomExecutableTitleByExeName(GetCurrentGameEXEName());
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
        internal static string VarCurrentGameMOOverwritePath()
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
        internal static string GetKKManagerPath()
        {
            return Path.Combine(GetAppResDir(), "tools", "KKManager");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string KKManagerStandaloneUpdaterEXEPath()
        {
            return Path.Combine(GetKKManagerPath(), "StandaloneUpdater.exe");
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
    }
}
using AI_Helper.Games;
using AI_Helper.Manage;
using AIHelper.Games;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace AIHelper.Manage
{
    class ManageSettings
    {
        internal static bool IsFirstRun()
        {
            return ManageINI.GetINIValueIfExist(GetAIHelperINIPath(), "FirstRun", "General") == "True";
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

        internal static string GetDefaultBepInEx5OlderVersion()
        {
            return "5.0.1";
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

        private static string GetCustomRes()
        {
            throw new NotImplementedException();
        }

        internal static List<Game> GetListOfExistsGames()
        {
            List<Game> ListOfGames = GamesList.GetGamesList();

            if (Directory.Exists(ManageSettings.GetGamesFolderPath()))
            {
                foreach (var game in ListOfGames)
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
                        //!ManageFilesFolders.CheckDirectoryNullOrEmpty_Fast(Path.Combine(Properties.Settings.Default.ApplicationStartupPath, "MO"))
                        &&
                        IsMOFolderValid(Path.Combine(Properties.Settings.Default.ApplicationStartupPath, "MO"))
                        //&&
                        //Directory.Exists(Path.Combine(Path.Combine(Properties.Settings.Default.ApplicationStartupPath, "MO", "Profiles")))
                        //&&
                        //!ManageFilesFolders.CheckDirectoryNullOrEmpty_Fast(Path.Combine(Properties.Settings.Default.ApplicationStartupPath, "MO", "Profiles"))
                        &&
                        !ManageSymLinks.IsSymLink(Path.Combine(Properties.Settings.Default.ApplicationStartupPath, "MO", "ModOrganizer.ini"))
                        &&
                        !ManageSymLinks.IsSymLink(Path.Combine(Properties.Settings.Default.ApplicationStartupPath, "MO", "categories.dat"))
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

        internal static string GetCurrentGameModListRulesPath()
        {
            return Path.Combine(ManageSettings.GetAppResDir(), "rules", ManageSettings.GetCurrentGameFolderName(), "modlist.txt");
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

        internal static string GetGamesFolderPath()
        {
            //var GamesPath = Path.Combine(Application.StartupPath, "Games");
            return Path.Combine(Properties.Settings.Default.ApplicationStartupPath, "Games");
            //return Directory.Exists(GamesPath) ? GamesPath : Application.StartupPath;
        }

        internal static int GetCurrentGameIndex()
        {
            return Properties.Settings.Default.CurrentGameListIndex;
        }

        internal static string GetSettingsEXEPath()
        {
            return Path.Combine(ManageSettings.GetCurrentGameDataPath(), GetINISettingsEXEName() + ".exe");
        }

        internal static string GetCurrentGamePath()
        {
            return Properties.Settings.Default.CurrentGamePath;
        }

        internal static string GetCurrentGameMOOverwritePath()
        {
            return GetOverwriteFolder();
        }

        internal static string GETMOCurrentGameName()
        {
            return "Skyrim";
        }

        internal static string GetDummyFilePath()
        {
            return Path.Combine(GetCurrentGamePath(), "TESV.exe");
        }

        internal static string GetCurrentGameEXEName()
        {
            return Properties.Settings.Default.CurrentGameEXEName;
        }

        internal static string GetCurrentGameFolderName()
        {
            return Properties.Settings.Default.CurrentGameFolderName;
        }

        internal static string GetCurrentGameDisplayingName()
        {
            return Properties.Settings.Default.CurrentGameDisplayingName;
        }

        internal static string GetStudioEXEName()
        {
            return Properties.Settings.Default.StudioEXEName;
        }

        internal static string GetINISettingsEXEName()
        {
            return Properties.Settings.Default.INISettingsEXEName;
        }

        internal static string GetAppResDir()
        {
            return Path.Combine(Properties.Settings.Default.ApplicationStartupPath, "RES");
        }

        internal static string GetCurrentGameModsPath()
        {
            return Path.Combine(GetCurrentGamePath(), "Mods");
        }

        internal static string GetDownloadsPath()
        {
            return Path.Combine(GetCurrentGamePath(), "Downloads");
        }

        internal static string GetCurrentGameDataPath()
        {
            return Path.Combine(GetCurrentGamePath(), "Data");
        }

        internal static string GetMOdirPath()
        {
            return Path.Combine(Properties.Settings.Default.ApplicationStartupPath, "MO");
        }

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
                Properties.Settings.Default.MOSelectedProfileDirName = ManageMO.MOremoveByteArray(ManageINI.GetINIValueIfExist(ManageSettings.GetModOrganizerINIpath(), "selected_profile", "General"));

                return Properties.Settings.Default.MOSelectedProfileDirName;
            }
        }

        internal static string GetMOSelectedProfileDirPath()
        {
            return Path.Combine(GetCurrentGamePath(), "MO", "profiles", GetMOSelectedProfileDirName());
        }

        internal static string GetMOiniPath()
        {
            return Path.Combine(GetMOdirPath(), "ModOrganizer.ini");
        }

        internal static string GetMOiniPathForSelectedGame()
        {
            return ManageFilesFolders.GreateFileFolderIfNotExists(Path.Combine(GetCurrentGamePath(), "MO", "ModOrganizer.ini"));
        }

        internal static string GetMOcategoriesPath()
        {
            return Path.Combine(GetMOdirPath(), "categories.dat");
        }

        internal static string GetMOcategoriesPathForSelectedGame()
        {
            return ManageFilesFolders.GreateFileFolderIfNotExists(Path.Combine(GetCurrentGamePath(), "MO", "categories.dat"));
        }

        internal static string GetInstall2MODirPath()
        {
            return Path.Combine(GetCurrentGamePath(), "2MO");
        }

        internal static string GetOverwriteFolder()
        {
            return ManageFilesFolders.GreateFileFolderIfNotExists(Path.Combine(GetCurrentGamePath(), "MO", "overwrite"), true);
        }

        internal static string GetOverwriteFolderLink()
        {
            return Path.Combine(GetCurrentGamePath(), "MOUserData");
        }

        internal static string GetAIHelperINIPath()
        {
            return Path.Combine(Properties.Settings.Default.ApplicationStartupPath, Application.ProductName + ".ini");
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

        internal static string GetModOrganizerINIpath()
        {
            return Path.Combine(GetMOdirPath(), "ModOrganizer.ini");
        }

        internal static string MOmodeSwitchDataDirName { get => "momode";}

        internal static string GetMOmodeSwitchDataDirPath()
        {
            return Path.Combine(GetAppResDir(), MOmodeSwitchDataDirName);
        }

        internal static string GetMOmodeDataFilesBakDirPath()
        {
            return Path.Combine(GetMOmodeSwitchDataDirPath(), GetCurrentGameFolderName(), "MOmodeDataFilesBak");
        }

        internal static string GetModdedDataFilesListFilePath()
        {
            return Path.Combine(GetMOmodeSwitchDataDirPath(), GetCurrentGameFolderName(), "ModdedDataFilesList.txt");
        }

        internal static string GetVanillaDataFilesListFilePath()
        {
            return Path.Combine(GetMOmodeSwitchDataDirPath(), GetCurrentGameFolderName(), "VanillaDataFilesList.txt");
        }

        internal static string GetVanillaDataEmptyFoldersListFilePath()
        {
            return Path.Combine(GetMOmodeSwitchDataDirPath(), GetCurrentGameFolderName(), "VanillaDataEmptyFoldersList.txt");
        }

        internal static string GetMOToStandartConvertationOperationsListFilePath()
        {
            return Path.Combine(GetMOmodeSwitchDataDirPath(), GetCurrentGameFolderName(), "MOToStandartConvertationOperationsList.txt");
        }

        internal static string GetZipmodsGUIDListFilePath()
        {
            return Path.Combine(GetMOmodeSwitchDataDirPath(), GetCurrentGameFolderName(), "ZipmodsGUIDList.txt");
        }

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
            string curValue = ManageCFG.GetCFGValueIfExist(ManageSettings.GetBepInExCfgFilePath(), "DisplayedLogLevel", "Logging.Console");
            if (curValue.Length == 0)
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
                        ManageCFG.WriteCFGValue(ManageSettings.GetBepInExCfgFilePath(), TargetSectionName, "DisplayedLogLevel", /*" " +*/ value);
                        BepInExDisplayedLogLevelLabel.Text = value;
                        return;
                    }
                    if (value == curValue)
                    {
                        setNext = true;
                    }
                }
                //ManageINI.WriteINIValue(ManageSettings.GetBepInExCfgFilePath(), "Logging.Console", "DisplayedLogLevel", /*" " +*/ values[0]);
                ManageCFG.WriteCFGValue(ManageSettings.GetBepInExCfgFilePath(), "Logging.Console", "DisplayedLogLevel", /*" " +*/ values[0]);
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

        internal static bool GetCurrentGameIsHaveVR()
        {
            return File.Exists(Path.Combine(GetCurrentGameDataPath(), GetCurrentGameEXEName() + "VR" + ".exe"));
        }
    }
}

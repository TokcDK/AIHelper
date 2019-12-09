using AI_Helper.Games;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace AI_Helper.Manage
{
    class ManageSettings
    {
        public static void SettingsINIT()
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

        public static int GetCurrentGameIndex()
        {
            return Properties.Settings.Default.CurrentGameListIndex;
        }

        public static string GetSettingsEXEPath()
        {
            return Path.Combine(Properties.Settings.Default.DataPath, GetINISettingsEXEName() + ".exe");
        }

        public static string GetCurrentGamePath()
        {
            return Properties.Settings.Default.CurrentGamePath;
        }

        public static string GETMOCurrentGameName()
        {
            return "Skyrim";
        }

        public static string GetDummyFilePath()
        {
            return Path.Combine(GetCurrentGamePath(), "TESV.exe");
        }

        public static string GetCurrentGameEXEName()
        {
            return Properties.Settings.Default.CurrentGameEXEName;
        }

        public static string GetCurrentGameFolderName()
        {
            return Properties.Settings.Default.CurrentGameFolderName;
        }

        public static string GetStudioEXEName()
        {
            return Properties.Settings.Default.StudioEXEName;
        }

        public static string GetINISettingsEXEName()
        {
            return Properties.Settings.Default.INISettingsEXEName;
        }

        public static string GetAppResDir()
        {
            return Path.Combine(Properties.Settings.Default.ApplicationStartupPath, "RES");
        }

        public static string GetModsPath()
        {
            return Path.Combine(GetCurrentGamePath(), "Mods");
        }

        public static string GetDownloadsPath()
        {
            return Path.Combine(GetCurrentGamePath(), "Downloads");
        }

        public static string GetDataPath()
        {
            return Path.Combine(GetCurrentGamePath(), "Data");
        }

        public static string GetMOdirPath()
        {
            return Path.Combine(Properties.Settings.Default.ApplicationStartupPath, "MO");
        }

        public static string GetMOexePath()
        {
            return Path.Combine(GetMOdirPath(), "ModOrganizer.exe");
        }

        public static string GetMOiniPath()
        {
            return Path.Combine(GetMOdirPath(), "ModOrganizer.ini");
        }

        public static string GetMOiniPathForSelectedGame()
        {
            return Path.Combine(GetCurrentGamePath(), "MO", "ModOrganizer.ini");
        }

        public static string GetMOcategoriesPath()
        {
            return Path.Combine(GetMOdirPath(), "categories.dat");
        }

        public static string GetMOcategoriesPathForSelectedGame()
        {
            return Path.Combine(GetCurrentGamePath(), "MO", "categories.dat");
        }

        public static string GetInstall2MODirPath()
        {
            return Path.Combine(GetCurrentGamePath(), "2MO");
        }

        public static string GetOverwriteFolder()
        {
            return Path.Combine(GetCurrentGamePath(), "MO", "overwrite");
        }

        public static string GetOverwriteFolderLink()
        {
            return Path.Combine(GetCurrentGamePath(), "MOUserData");
        }

        public static string GetAIHelperINIPath()
        {
            return Path.Combine(Properties.Settings.Default.ApplicationStartupPath, Application.ProductName + ".ini");
        }

        public static int GetCurrentGameIndexByFolderName(List<Game> listOfGames, string FolderName)
        {
            for (int i = 0; i < listOfGames.Count; i++)
            {
                if (listOfGames[i].GetGameFolderName() == FolderName)
                {
                    return i;
                }
            }
            return 0;
        }

        public static string GetModOrganizerINIpath()
        {
            return Path.Combine(GetMOdirPath(), "ModOrganizer.ini");
        }

        public static string GetMOmodeDataFilesBakDirPath()
        {
            return Path.Combine(GetAppResDir(), "momode", GetCurrentGameFolderName(), "MOmodeDataFilesBak");
        }

        public static string GetModdedDataFilesListFilePath()
        {
            return Path.Combine(GetAppResDir(), "momode", GetCurrentGameFolderName(), "ModdedDataFilesList.txt");
        }

        public static string GetVanillaDataFilesListFilePath()
        {
            return Path.Combine(GetAppResDir(), "momode", GetCurrentGameFolderName(), "VanillaDataFilesList.txt");
        }

        public static string GetVanillaDataEmptyFoldersListFilePath()
        {
            return Path.Combine(GetAppResDir(), "momode", GetCurrentGameFolderName(), "VanillaDataEmptyFoldersList.txt");
        }

        public static string GetMOToStandartConvertationOperationsListFilePath()
        {
            return Path.Combine(GetAppResDir(), "momode", GetCurrentGameFolderName(), "MOToStandartConvertationOperationsList.txt");
        }

        public static List<Game> GetListOfExistsGames()
        {
            List<Game> ListOfGames = new List<Game>()
            {
                new AISyoujyo(),
                new AISyoujyoTrial(),
                new HoneySelect()
            };

            ListOfGames = ListOfGames.Where
                (game =>
                    Directory.Exists(game.GetGamePath())
                    &&
                    !ManageFilesFolders.CheckDirectoryNullOrEmpty_Fast(Path.Combine(game.GetGamePath(), "MO", "Profiles"))
                ).ToList();
            return ListOfGames;
        }

        public static string GetBepInExPath()
        {
            return Path.Combine(GetModsPath(), "BepInEx");
        }

        public static string GetBepInExCfgDirPath()
        {
            return ManageMO.GetLastMOFileDirPathFromEnabledModsOfActiveMOProfile(Path.Combine(GetBepInExPath(), "BepInEx", "config"), true);
        }

        public static string GetBepInExCfgFilePath()
        {
            if (Properties.Settings.Default.BepinExCfgPath.Length > 0)
            {
                return Properties.Settings.Default.BepinExCfgPath;
            }
            return ManageMO.GetLastMOFileDirPathFromEnabledModsOfActiveMOProfile(Path.Combine(GetBepInExCfgDirPath(), "BepInEx.cfg"));
        }

        public static void SwitchBepInExDisplayedLogLevelValue(CheckBox BepInExConsoleCheckBox, Label BepInExDisplayedLogLevelLabel, bool OnlyShow = false)
        {
            string curValue = ManageINI.GetINIValueIfExist(ManageSettings.GetBepInExCfgFilePath(), "DisplayedLogLevel", "Logging.Console");
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
                        ManageINI.WriteINIValue(ManageSettings.GetBepInExCfgFilePath(), "Logging.Console", "DisplayedLogLevel", " " + value);
                        BepInExDisplayedLogLevelLabel.Text = value;
                        return;
                    }
                    if (value == curValue)
                    {
                        setNext = true;
                    }
                }
                ManageINI.WriteINIValue(ManageSettings.GetBepInExCfgFilePath(), "Logging.Console", "DisplayedLogLevel", " " + values[0]);
                BepInExDisplayedLogLevelLabel.Text = values[0];
            }
        }
    }
}

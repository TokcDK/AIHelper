using System.IO;

namespace AI_Girl_Helper.Manage
{
    class SettingsManage
    {
        public static int GetCurrentGameIndex()
        {
            return 0;
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
            return Path.Combine(Properties.Settings.Default.ApplicationStartupPath, "AI Girl Helper_RES");
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

        public static string GetInstall2MODirPath()
        {
            return Path.Combine(GetCurrentGamePath(), "2MO");
        }

        public static string GetOverwriteFolder()
        {
            return Path.Combine(GetMOdirPath(), "overwrite");
        }

        public static string GetOverwriteFolderLink()
        {
            return Path.Combine(GetCurrentGamePath(), "MOUserData");
        }

        public static string GetModOrganizerINIpath()
        {
            return Path.Combine(GetMOdirPath(), "ModOrganizer.ini");
        }

        public static string GetMOmodeDataFilesBakDirPath()
        {
            return Path.Combine(GetAppResDir(), "MOmodeDataFilesBak");
        }

        public static string GetModdedDataFilesListFilePath()
        {
            return Path.Combine(GetAppResDir(), "ModdedDataFilesList.txt");
        }

        public static string GetVanillaDataFilesListFilePath()
        {
            return Path.Combine(GetAppResDir(), "VanillaDataFilesList.txt");
        }

        public static string GetMOToStandartConvertationOperationsListFilePath()
        {
            return Path.Combine(GetAppResDir(), "MOToStandartConvertationOperationsList.txt");
        }
    }
}
